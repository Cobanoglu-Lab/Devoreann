"""  
Copyright © 2020, University of Texas Southwestern Medical Center. All rights reserved.
Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
Department: Lyda Hill Department of Bioinformatics.
This software and any related documentation constitutes published and/or unpublished works and may contain valuable trade secrets and proprietary information belonging to The University of Texas Southwestern Medical Center (UT SOUTHWESTERN).  None of the foregoing material may be copied, duplicated or disclosed without the express written permission of UT SOUTHWESTERN.  IN NO EVENT SHALL UT SOUTHWESTERN BE LIABLE TO ANY PARTY FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES, INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS DOCUMENTATION, EVEN IF UT SOUTHWESTERN HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.  UT SOUTHWESTERN SPECIFICALLY DISCLAIMS ANY WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE AND ACCOMPANYING DOCUMENTATION, IF ANY, PROVIDED HEREUNDER IS PROVIDED "AS IS". UT SOUTHWESTERN HAS NO OBLIGATION TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
This software contains copyrighted materials from Oculus, Unity Technologies, Keras, TensorFlow, gRPC, NumPy, Matplotlib, OpenCV, Pyprind, Nvidia CUDA, wiki.unity3d.com, PyCharm, Visual Studio Community, and Google. Corresponding terms and conditions apply.
"""

"""Spawns gpu_processes to evaluate a model
"""
import os
os.environ["CUDA_DEVICE_ORDER"] = "PCI_BUS_ID"
os.environ['CUDA_VISIBLE_DEVICES'] = ""#"-1"

from multiprocessing import Process, Lock, Queue, Pipe
from gpu_process import spawn_gpu_process
from sklearn.model_selection import StratifiedKFold
import statistics
import scipy.stats as st
import time, sys

# KERAS imports
from keras.utils import to_categorical
from keras.models import load_model


def main(nGPUS, nFolds, data_manager, q0, q1, q2):
    """ evaluator helper function for creating a GPUSpawnerPersistent
    """
    spawner = GPUSpawnerPersistent(nGPUS, nFolds, data_manager, q0, q1, q2)
    spawner.spawn_processes_p()
    return spawner

class GPUSpawnerPersistent:
    """ Spawns processes for each GPU and handles evaluation data.
    """
    def __init__(self, nGPUS, nFolds, data_manager, cq0, cq1, cq2):
        """
        :param nGPUS: number of gpus for which to spawn processes.
        :param nFolds: folds to run in total on a model.
        :processes: list of active processes (one per gpu)
        :jobs: Queue of job indices mapping to the @data list.
        """
        self.nGPUS = nGPUS
        self.nFolds = nFolds
        self.processes = []
        self.jobs = Queue()
        self.dm = data_manager
        self.data = []
        self.auc_means = Queue()
        self.acc_means = Queue()
        self.cqs = [cq0, cq1, cq2]
        self.AUC_median = 0
        self.ACC_median = 0  # Accuracy median

    def get_kfold_accuracy(self, _model):
        """ Get the accuracy of the model trained across each GPU for a k fold stratified method.
        :param _model:
        :return: the median model accuracy.
        """
        print("\t get_kfold_accuracy enter.\n")

        # Clear Queue:
        while self.jobs.qsize() != 0:
            self.jobs.get()

        _model.save('shared_model.h5')

        #sys.stdout = open('output', 'w')

        # Set job indices:
        for index in range(self.nFolds):
            print("Added Job")
            self.jobs.put(index)

        # Wait for queue to empty
        try:
            while not self.jobs.empty():
                time.sleep(.5)
        except KeyboardInterrupt:
            print("exiting get_kfold_accuracy.")

        #sys.stdout = sys.__stdout__

        try:
            while self.acc_means.qsize() != self.nFolds:
                time.sleep(.25)
        except KeyboardInterrupt:
            print("exiting get_kfold_accuracy.")

        return self.__eval_threads(_model)  # Get median model

    def on_exit(self):
        """ Should be called before exiting the program, waits for processes to finish.
        """
        for _p in self.processes:
            _p.join()

        print("\t Evaluator exited.")

    def spawn_processes_p(self):
        """ Spawn a persistent process on each GPU.
        :return: n/a
        """
        dm = self.dm
        print("\t GPUSpawnerPersistent::__spawn_processes_p")
        _lock = Lock()

        self.data = dm.data

        #for j in range(self.nFolds):
        #   self.jobs.put(j) # Add a job index to the shared queue.

        for _g in range(0,self.nGPUS): #debug 1, for removing TITAN GPU[0]
            #if _g == 1: continue
            p = Process(target=spawn_gpu_process, args=(self.dm,_g, _lock, self.jobs, self.data,self.auc_means, self.acc_means, self.cqs[_g]))
            self.processes.append(p)
            p.start()

    def __eval_threads(self, _model):
        """
        :return: the median AUC
        """
        print("\n\tGPU_Spawner::__eval_threads enter.\n")

        _auc_means = []
        _acc_means = []

        # Convert to list
        while self.auc_means.qsize() != 0:
            _auc_means.append(self.auc_means.get())
        while self.acc_means.qsize() != 0:
            _acc_means.append(self.acc_means.get())

        self.cqs[0].put("finished#")

        self.AUC_median = statistics.median(_auc_means)
        self.ACC_median = statistics.median(_acc_means)
        _auc_means = sorted(_auc_means)
        _acc_means = sorted(_acc_means)
        idx_mid = int(len(_auc_means) / 2)
        print("success\tAUC MEDIAN: %f" % self.AUC_median)
        print("success\tACC MEDIAN: %f" % self.ACC_median)

        #self.cqs[0].put("accs#")
        #accs_str = "$"
        #for m in _acc_means:
        #    accs_str += "accs" + str(m) + "$"
        #print(accs_str)
        #self.cqs[0].put(accs_str)

        #final_model = load_model('best_model' + str(idx_mid) + '.h5')
        #final_model.save('final_model.h5')

        print("success\t" + str(st.t.interval(0.95, len(_auc_means) - 1, self.AUC_median, scale=st.sem(_auc_means))))
        
        ci_acc = str(st.t.interval(0.95, len(_acc_means) - 1, self.ACC_median, scale=st.sem(_acc_means)))
        print("success\t" + ci_acc)
        
        self.cqs[0].put("accs"+ ci_acc + "$")

        return self.ACC_median
