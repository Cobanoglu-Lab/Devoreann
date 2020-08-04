"""  
Copyright © 2020, University of Texas Southwestern Medical Center. All rights reserved.
Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
Department: Lyda Hill Department of Bioinformatics.
This software and any related documentation constitutes published and/or unpublished works and may contain valuable trade secrets and proprietary information belonging to The University of Texas Southwestern Medical Center (UT SOUTHWESTERN).  None of the foregoing material may be copied, duplicated or disclosed without the express written permission of UT SOUTHWESTERN.  IN NO EVENT SHALL UT SOUTHWESTERN BE LIABLE TO ANY PARTY FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES, INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS DOCUMENTATION, EVEN IF UT SOUTHWESTERN HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.  UT SOUTHWESTERN SPECIFICALLY DISCLAIMS ANY WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE AND ACCOMPANYING DOCUMENTATION, IF ANY, PROVIDED HEREUNDER IS PROVIDED "AS IS". UT SOUTHWESTERN HAS NO OBLIGATION TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
This software contains copyrighted materials from Oculus, Unity Technologies, Keras, TensorFlow, gRPC, NumPy, Matplotlib, OpenCV, Pyprind, Nvidia CUDA, wiki.unity3d.com, PyCharm, Visual Studio Community, and Google. Corresponding terms and conditions apply.
"""

""" Persistent process that runs on a dedicated GPU.
Handles new requests via a multiprocessing Queue.
"""

import os
import sys
from multiprocessing import Lock, Queue
from queue import Empty
from data_manager import DataManager
#rom gpu_utils import evaluate_advanced_threaded, train_advanced_threaded
from sklearn.metrics import roc_auc_score
import time

import tensorflow as tf
from keras.backend.tensorflow_backend import set_session

config = tf.ConfigProto(
    gpu_options = tf.GPUOptions(per_process_gpu_memory_fraction=0.5)
)
config.gpu_options.allow_growth = True
session = tf.Session(config=config)
set_session(session)

from pychannel import main as channel_main
from multiprocessing import Process, Queue

from keras.callbacks import Callback

MAX_EPOCHS = 3 # Reduced for demo purposes
PATIENCE = 1
GPU = 1

ucommandq = None #Unity command queue

#class CustomCallback(Callback):
#    def on_batch_end(self, batch, logs={}):
#        ucommandq.put("fit#"+logs.get('loss'))

def spawn_gpu_process(dm, gpu_index, lock, jobs, data, _auc_means, _acc_means, cq):
    """ Spawns a thread for a given GPU.
    :param gpu_index: the index of the GPU (starting at zero).
    :lock: multiprocess lock from evaluator.
    :jobs: multiprocess Queue of job indices mapping to the @data list.
    :return: N/A
    """
    global ucommandq, GPU
    GPU = gpu_index
    os.environ["CUDA_DEVICE_ORDER"] = "PCI_BUS_ID"
    os.environ["CUDA_VISIBLE_DEVICES"] = str(gpu_index) # Set the gpu to use. 

    from keras.models import load_model
    from keras import backend as K

    b_process_running = True
    ucommandq = cq

    print("\t Spawned GPU Process: %d" % gpu_index)
    print("\t\t CUDA Device: " + os.environ["CUDA_VISIBLE_DEVICES"])

    while b_process_running:
        job_idx = -1
        try:
            K.clear_session()
            job_idx = jobs.get(block=True, timeout=0.05)
            if job_idx != -1:
                if job_idx > 2 and gpu_index != 0:
                    jobs.put(job_idx)
                    time.sleep(5)
                    continue
                print("\t GPU[%d] Obtained job %d" % (gpu_index, job_idx))
                lock.acquire()
                _model = load_model('shared_model.h5')
                lock.release()
                print("success\t Loaded model.")

                _data = data[job_idx]
                try:
                    evaluate_advanced_threaded(dm, _model, _data, lock, job_idx, _auc_means, _acc_means)
                except Exception as e:
                    print(e)
                    jobs.put(job_idx)
                    time.sleep(1)
                    continue
        except Empty:
            continue
        except KeyboardInterrupt:
            b_process_running = True
            break
        except Exception as e:
            print(e)

    print("\t Process [%d] exited." % gpu_index)


# region Evaluation Functions:
# -----------------------------------------------------------------------------------------

def evaluate_advanced_threaded(dm, model, data, lock, idx, _auc_means, _acc_means):
    """ Train and evaluate a model.
    :return: A new AUC has been inserted into AUC_means & it's corresponding model has been saved.
    """
    #print("thread #" + str(idx) + " evaluate_advanced_threaded")
    xtrain = data[0]; xtest = data[1]; ytrain = data[2]; ytest= data[3]

    history = train_advanced_threaded(dm, model, xtrain, ytrain, xtest, ytest, lock, idx, _auc_means, _acc_means)

    accuracy_history = history.history['acc']
    val_accuracy_history = history.history['val_acc']
    lock.acquire()
    print(str(idx)+" - Last training accuracy: " + str(accuracy_history[-1]) + ", last validation accuracy: " + str(val_accuracy_history[-1]))
    lock.release()


class QueueStream:
    def __init__(self):
        global ucommandq, GPU
        self.q = ucommandq
        #self.data = ''
    def write(self, s):
        #self.data += s
        self.q.put(s)
    def flush(self):
        self.q.put(str(GPU) + "$")
        return

def train_advanced_threaded(dm, model, _xtrain, _ytrain, _xtest, _ytest, lock, _idx, _auc_means, _acc_means):
    from keras.callbacks import EarlyStopping
    from keras.callbacks import ModelCheckpoint
    from keras.models import load_model

    batch_size = dm.batch_size
    global MAX_EPOCHS, PATIENCE, ucommandq, GPU
    #print("thread #" + str(_idx) + " train_advanced_threaded")

    # simple early stopping
    es = EarlyStopping(monitor='val_loss', mode='min', verbose=0, patience=PATIENCE)
    mc = ModelCheckpoint('best_model'+str(_idx)+'.h5', monitor='val_acc', mode='max', verbose=1, save_best_only=True)

    ucommandq.put('start' + str(_idx))
    print("\n Put command start.")
    old_stdout = sys.stdout
    sys.stdout = x = QueueStream()
    _history = model.fit(_xtrain, _ytrain, batch_size=batch_size, validation_split=0.25, epochs=MAX_EPOCHS, verbose=1, callbacks=[es, mc])
    sys.stdout = sys.__stdout__

    saved_model = load_model('best_model'+str(_idx)+'.h5')

    # evaluate the model
    _, train_acc = saved_model.evaluate(_xtrain, _ytrain, verbose=0)
    _, test_acc = saved_model.evaluate(dm.X_final, dm.Y_final, verbose=0)
    y_pred = model.predict(dm.X_final)
    _roc = roc_auc_score(dm.Y_final, y_pred, average='macro')
    #ucommandq.put('finish#'+str(_idx))

   # _, final_acc = saved_model.evaluate(dm.X_final, dm.Y_final)
   # print('\n success \t' + str(final_acc))
    print('\n success \t [%d] - Train: %.3f, Test: %.3f, ROC: %.3f' % (_idx, train_acc, test_acc, _roc) + "\n")
    ucommandq.put("$FOLD:test%.3f:%d$" %(test_acc, GPU))
    _auc_means.put(_roc)
    _acc_means.put(train_acc)
    #lock.release()

    return _history

#endregion
