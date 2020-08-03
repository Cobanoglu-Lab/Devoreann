"""  
Copyright © 2020, University of Texas Southwestern Medical Center. All rights reserved.
Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
Department: Lyda Hill Department of Bioinformatics.
This software and any related documentation constitutes published and/or unpublished works and may contain valuable trade secrets and proprietary information belonging to The University of Texas Southwestern Medical Center (UT SOUTHWESTERN).  None of the foregoing material may be copied, duplicated or disclosed without the express written permission of UT SOUTHWESTERN.  IN NO EVENT SHALL UT SOUTHWESTERN BE LIABLE TO ANY PARTY FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES, INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS DOCUMENTATION, EVEN IF UT SOUTHWESTERN HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.  UT SOUTHWESTERN SPECIFICALLY DISCLAIMS ANY WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE AND ACCOMPANYING DOCUMENTATION, IF ANY, PROVIDED HEREUNDER IS PROVIDED "AS IS". UT SOUTHWESTERN HAS NO OBLIGATION TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
This software contains copyrighted materials from Oculus, Unity Technologies, Keras, TensorFlow, gRPC, NumPy, Matplotlib, OpenCV, Pyprind, Nvidia CUDA, wiki.unity3d.com, PyCharm, Visual Studio Community, and Google. Corresponding terms and conditions apply.
"""

""" Server responsible for creating models from Unity
"""

import os
os.environ["CUDA_DEVICE_ORDER"] = "PCI_BUS_ID"
os.environ['CUDA_VISIBLE_DEVICES'] = "0"#"-1"

# KERAS imports
from keras import backend as K
from keras.models import Sequential
from keras.layers import Dense, Dropout, Activation, Flatten
from keras.layers import Conv2D, MaxPooling2D
import time
from image_utils import clear_texture_cache

def main():
    """ Creates the server and handles communication
    @param conn_in multiprocessing Pipe connector for communication in.
    @param conn_out multiprocessing Pipe connector for communication out.
    """

class Server:
    def __init__(self, data_manager, evaluator):
        self.dm = data_manager
        self.eval = evaluator

        self.receive_request() #debug

    def receive_request(self):
        dm = self.dm  # referenced locally for convenience

        K.clear_session()
        #clear_texture_cache()

        model = Sequential()
        model.add(Conv2D(dm.num_filters, dm.kernel_size, input_shape=dm.input_shape))
        model.add(Activation('relu'))


        model.add(Flatten()) #debug

        # classification layer
        model.add(Dense(dm.num_classes))
        model.add(Activation('softmax'))

        model.compile(loss='binary_crossentropy', optimizer='adam', metrics=['accuracy'])

        model.summary()
        print("\nsuccess \t server: received accuracy: %f.\n" % self.eval.get_kfold_accuracy(model))
        print("\t Completed Server request.")

