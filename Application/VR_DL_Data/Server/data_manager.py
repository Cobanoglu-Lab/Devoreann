"""  
Copyright © 2020, University of Texas Southwestern Medical Center. All rights reserved.
Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
Department: Lyda Hill Department of Bioinformatics.
This software and any related documentation constitutes published and/or unpublished works and may contain valuable trade secrets and proprietary information belonging to The University of Texas Southwestern Medical Center (UT SOUTHWESTERN).  None of the foregoing material may be copied, duplicated or disclosed without the express written permission of UT SOUTHWESTERN.  IN NO EVENT SHALL UT SOUTHWESTERN BE LIABLE TO ANY PARTY FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES, INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS DOCUMENTATION, EVEN IF UT SOUTHWESTERN HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.  UT SOUTHWESTERN SPECIFICALLY DISCLAIMS ANY WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE AND ACCOMPANYING DOCUMENTATION, IF ANY, PROVIDED HEREUNDER IS PROVIDED "AS IS". UT SOUTHWESTERN HAS NO OBLIGATION TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
This software contains copyrighted materials from Oculus, Unity Technologies, Keras, TensorFlow, gRPC, NumPy, Matplotlib, OpenCV, Pyprind, Nvidia CUDA, wiki.unity3d.com, PyCharm, Visual Studio Community, and Google. Corresponding terms and conditions apply.
"""

import os
#os.environ["CUDA_DEVICE_ORDER"] = "PCI_BUS_ID"
#os.environ['CUDA_VISIBLE_DEVICES'] = "0"#"-1"

import configparser
import pickle
from sklearn.model_selection import StratifiedKFold
import numpy as np

from keras import backend as K
from keras.utils import to_categorical

class DataManager:
    def __init__(self, nFolds):
        self.batch_size = 32
        self.num_classes = 7
        self.epochs = 2
        self.num_filters = 32
        self.pool_size = (2, 2)
        self.kernel_size = (3, 3)
        self.dropout_dim = 0.5
        self.dense_neurons = 32
        self.IMG_SIZE = 128
        self.img_rows = 128
        self.img_cols = 128
        self.b_eval_advanced = True
        self.input_shape = (0,0,0)
        self.X = []
        self.Y = []
        self.X_final = []
        self.Y_final = []
        self.data = []
        self.nFolds = nFolds
        self._read_config()
        self._load_data()


    def _read_config(self):
        """ Set user-defined global properties.
        """
        config = configparser.ConfigParser()
        config.read('config.ini')

        self.batch_size = int(config['MODEL']['batch_size'])
        self.num_filters = int(config['MODEL']['num_filters'])
        self.dropout_dim = float(config['MODEL']['dropout_dim'])
        self.dense_neurons = int(config['MODEL']['dense_neurons'])
        _pool_size = config['MODEL']['pool_size']
        _kernel_size = config['MODEL']['kernel_size']
        self.IMG_SIZE = int(config['DATA']['image_size'])
        self.num_classes = int(config['CUSTOM']['num_classes'])
        self.epochs = int(config['MODEL']['epochs'])
        self.b_eval_advanced = (
                    config['MODEL']['complex_analysis'] == 'true' or config['MODEL']['complex_analysis'] == 'True')

        self.pool_size = tuple(map(int, _pool_size.split(',')))
        self.kernel_size = tuple(map(int, _kernel_size.split(',')))

        self.img_rows, self.img_cols = self.IMG_SIZE, self.IMG_SIZE

    def _load_data(self):
        """ Load data from pickle files.
        """
        pickle_in = open("X_train.pickle", "rb")
        self.X = pickle.load(pickle_in)
        pickle_in = open("y_train.pickle", "rb")
        self.Y = pickle.load(pickle_in)

        pickle_in = open("X_test.pickle", "rb")
        self.X_final = pickle.load(pickle_in)
        pickle_in = open("y_test.pickle", "rb")
        self.Y_final = pickle.load(pickle_in)

        # Set input shape:
        if K.image_data_format() == 'channels_first':
            self.input_shape = (3, self.img_rows, self.img_cols)
        else:
            self.input_shape = (self.img_rows, self.img_cols, 3)

        self.X = self.X.astype('float32')
        self.X /= 255
        self.X_final = self.X_final.astype('float32')
        self.X_final /= 255
        print('X shape:', self.X.shape)
        print(self.X.shape[0], 'Samples')

        num_datapoints = 3000
        self.X = self.X[0:num_datapoints]
        self.Y = self.Y[0:num_datapoints]

        num_datapoints = 2000
        self.X_final = self.X_final[0:num_datapoints]
        self.Y_final = self.Y_final[0:num_datapoints]

        self.Y_final = to_categorical(self.Y_final, self.num_classes)

        # Initialize Data
        kfold = StratifiedKFold(n_splits=self.nFolds, shuffle=True)

        if self.b_eval_advanced:
            # Loop through the indices the split() method returns
            for index, (train_indices, test_indices) in enumerate(kfold.split(self.X, self.Y)):
                if index == 0:
                    self.Y = to_categorical(self.Y, self.num_classes)

                # Generate batches from indices
                xtrain, xtest = self.X[train_indices], self.X[test_indices]
                ytrain, ytest = self.Y[train_indices], self.Y[test_indices]

                self.data.append(tuple([xtrain, xtest, ytrain, ytest]))

        if not self.b_eval_advanced:
            self.Y = to_categorical(self.Y, self.num_classes)

        #print(np.asarray(self.data).shape)
        #print(self.data)
        print("Y_final Shape", self.Y_final.shape)
