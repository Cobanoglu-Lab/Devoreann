"""  
Copyright © 2020, University of Texas Southwestern Medical Center. All rights reserved.
Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
Department: Lyda Hill Department of Bioinformatics.
This software and any related documentation constitutes published and/or unpublished works and may contain valuable trade secrets and proprietary information belonging to The University of Texas Southwestern Medical Center (UT SOUTHWESTERN).  None of the foregoing material may be copied, duplicated or disclosed without the express written permission of UT SOUTHWESTERN.  IN NO EVENT SHALL UT SOUTHWESTERN BE LIABLE TO ANY PARTY FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES, INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS DOCUMENTATION, EVEN IF UT SOUTHWESTERN HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.  UT SOUTHWESTERN SPECIFICALLY DISCLAIMS ANY WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE AND ACCOMPANYING DOCUMENTATION, IF ANY, PROVIDED HEREUNDER IS PROVIDED "AS IS". UT SOUTHWESTERN HAS NO OBLIGATION TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
This software contains copyrighted materials from Oculus, Unity Technologies, Keras, TensorFlow, gRPC, NumPy, Matplotlib, OpenCV, Pyprind, Nvidia CUDA, wiki.unity3d.com, PyCharm, Visual Studio Community, and Google. Corresponding terms and conditions apply.
"""

""" Server responsible for creating models from Unity
"""

import sys
import evaluator_pb2
import evaluator_pb2_grpc
import os
import pickle
#os.environ["CUDA_DEVICE_ORDER"] = "PCI_BUS_ID"
#os.environ['CUDA_VISIBLE_DEVICES'] = "0"#"-1"

# KERAS imports
from keras import backend as K
from keras.models import Sequential
from keras.layers import Dense, Dropout, Activation, Flatten
from keras.layers import Conv2D, MaxPooling2D
from keras.models import Model
import time
from image_utils import clear_texture_cache, plot_save_all, set_input_layer

# Additional Classes:
import image_utils as Img

def main():
    """ Creates the server and handles communication
    @param conn_in multiprocessing Pipe connector for communication in.
    @param conn_out multiprocessing Pipe connector for communication out.
    """

class Evaluator(evaluator_pb2_grpc.EvaluatorServicer):
    def __init__(self, data_manager, evaluator, channelq, uchannelq):
        self.dm = data_manager
        self.eval = evaluator
        self.channelq = channelq
        self.uchannelq = uchannelq # Unity Queue()

        #self.receive_request() #debug

    def Evaluate(self, request, context):
        dm = self.dm  # referenced locally for convenience

        K.clear_session()
        clear_texture_cache()
        self.channelq.put("refresh_server#")
        self.channelq.put("clear_texture_cache#")

        # Build Keras model
        print("Received evaluate request")
        model = Sequential()
        model.add(Conv2D(dm.num_filters, dm.kernel_size, input_shape=dm.input_shape))
        model.add(Activation('relu'))
        print(request.layers)

        for layer in request.layers:
            typ = layer.WhichOneof("definition")
            print("> adding layer: " + typ)
            if typ == None:
                continue
            if typ == "convolution":
                # do something here
                conv = layer.convolution
                model.add(Conv2D(dm.num_filters, dm.kernel_size))
                model.add(Activation('relu'))
            elif typ == "dropout":
                dropout = layer.dropout
                model.add(Dropout(dm.dropout_dim))
                # do something here
            elif typ == "flatten":
                model.add(Flatten())
            elif typ == "dense":
                dense = layer.dense
                model.add(Dense(dm.dense_neurons))
                model.add(Activation('relu'))
            elif typ == "maxpooling":
                model.add(MaxPooling2D(pool_size=dm.pool_size))

        # classification layer
        model.add(Dense(dm.num_classes))
        model.add(Activation('softmax'))

        model.compile(loss='binary_crossentropy', optimizer='adam', metrics=['accuracy'])

        model.summary()
        
        #Summary to string:
        stringlist = []
        model.summary(print_fn=lambda x: stringlist.append(x))
        short_model_summary = "\n".join(stringlist)
        #self.uchannelq.put("summary#")
        self.uchannelq.put("summary#" + short_model_summary + "end#")

        #self.uchannelq.put("test#end")
        #self.uchannelq.put(str(model.summary()))

        #self.channelq.put('stream#output')

        acc = self.eval.get_kfold_accuracy(model)
        print("\nsuccess \t server: received accuracy: %f.\n" % acc)
        print("\t Completed Server request.")

        #self.channelq.put('end#')

        # Activation Layers
        layer_outputs = [layer.output for layer in model.layers]
        activation_model = Model(inputs=model.input, outputs=layer_outputs)
        activations = activation_model.predict(Img.set_input_layer(dm.X_final))
        pickled = pickle.dumps(activations)
        self.channelq.put('plot_save_all#pickle#' + str(len(pickled)))
        self.channelq.put(pickled)

        return evaluator_pb2.ProgressUpdate(accuracy=acc)

