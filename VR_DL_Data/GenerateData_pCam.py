"""
Copyright © 2019, University of Texas Southwestern Medical Center. All rights reserved.
Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
Department: Lyda Hill Department of Bioinformatics
"""

from __future__ import print_function
import os
import pickle
from keras import backend as K
from keras.utils import to_categorical
from keras.models import Sequential
from keras.layers import Dense, Dropout, Activation, Flatten
from keras.layers import Conv2D, MaxPooling2D

from sklearn.model_selection import StratifiedKFold
from keras.callbacks import EarlyStopping
from keras.callbacks import ModelCheckpoint
from keras.models import load_model

import statistics

from keras.utils import HDF5Matrix
import numpy as np, scipy.stats as st

import tensorflow as tf
from sklearn.metrics import roc_auc_score
from sklearn.metrics import roc_curve

os.environ["CUDA_DEVICE_ORDER"]="PCI_BUS_ID"
os.environ["CUDA_VISIBLE_DEVICES"] = "0"

# Create and train model
batch_size = 128
num_classes = 2
epochs = 3
num_filters = 32
pool_size = (2, 2)
kernel_size = (3, 3)
dropout_dim = 0.5
dense_neurons = 128

IMG_SIZE = 96
img_rows, img_cols = IMG_SIZE, IMG_SIZE

num_datapoints = 14000

AUC = [] # For an individual fold
AUC_means = [] # For finding the median fivefold mean AUC over all datasets

def main():
    global X, Y, AUC_means, AUC_median

    load_data()

    print(X[0].shape)

    return

    kfold = StratifiedKFold(n_splits=5, shuffle=True)

    # Loop through the indices the split() method returns
    for index, (train_indices, test_indices) in enumerate(kfold.split(X, Y)):
        if index == 0:
            Y = to_categorical(Y, num_classes)

        # Generate batches from indices
        xtrain, xtest = X[train_indices], X[test_indices]
        ytrain, ytest = Y[train_indices], Y[test_indices]

        # Create new model.
        model = None
        model = create_model()

        history = train_model(model, xtrain, ytrain, xtest, ytest, index)
        accuracy_history = history.history['acc']
        val_accuracy_history = history.history['val_acc']
        print("Last training accuracy: " + str(accuracy_history[-1]) + ", last validation accuracy: " + str(
            val_accuracy_history[-1]))

    AUC_median = statistics.median(AUC_means)
    AUC_means = sorted(AUC_means)
    idx_mid = int(len(AUC_means) / 2)
    print("AUC MEDIAN: %f" % AUC_median)

    final_model = load_model('best_model' + str(idx_mid) + '.h5')
    final_model.save('final_model.h5')

    print(st.t.interval(0.95, len(AUC_means) - 1, AUC_median, scale=st.sem(AUC_means)))


def load_data():
    global X, Y, X_final, Y_final, input_shape
    x_train = HDF5Matrix('camelyonpatch_level_2_split_train_x.h5', 'x')
    y_train = HDF5Matrix('camelyonpatch_level_2_split_train_y.h5', 'y')
    #x_valid = HDF5Matrix('camelyonpatch_level_2_split_valid_x.h5', 'x')
    #y_valid = HDF5Matrix('camelyonpatch_level_2_split_valid_y.h5', 'y')
    x_test = HDF5Matrix('camelyonpatch_level_2_split_test_x.h5', 'x')
    y_test = HDF5Matrix('camelyonpatch_level_2_split_test_y.h5', 'y')

    X = x_train[0:num_datapoints]
    Y = [row[0][0][0] for row in y_train.data[0:num_datapoints]]
    X_final = x_test[0:num_datapoints]
    Y_final = [row[0][0][0] for row in y_test.data[0:num_datapoints]]


    pickle_out = open("X_train.pickle", "wb")
    pickle.dump(X, pickle_out)
    pickle_out.close()

    pickle_out = open("X_test.pickle", "wb")
    pickle.dump(X_final, pickle_out)
    pickle_out.close()

    pickle_out = open("y_train.pickle", "wb")
    pickle.dump(Y, pickle_out)
    pickle_out.close()

    pickle_out = open("y_test.pickle", "wb")
    pickle.dump(Y_final, pickle_out)
    pickle_out.close()


    #print(Y)

    if K.image_data_format() == 'channels_first':
        input_shape = (3, img_rows, img_cols)
    else:
        input_shape = (img_rows, img_cols, 3)

    Y_final = to_categorical(Y_final, num_classes)

    print('X shape:', x_train.shape)
    print(x_train.shape[0], 'Samples')

def create_model():
    _model = Sequential()

    _model.add(Conv2D(num_filters, kernel_size, input_shape=input_shape))
    _model.add(Activation('relu'))
    #---------------------------------------

    _model.add(Conv2D(num_filters, kernel_size))
    _model.add(Activation('relu'))

    _model.add(Conv2D(num_filters, kernel_size))
    _model.add(Activation('relu'))

    _model.add(Conv2D(num_filters, kernel_size))
    _model.add(Activation('relu'))

    _model.add(Conv2D(num_filters, kernel_size))
    _model.add(Activation('relu'))

    _model.add(MaxPooling2D(pool_size=pool_size))
    _model.add(MaxPooling2D(pool_size=pool_size))

    _model.add(MaxPooling2D(pool_size=pool_size))
    _model.add(MaxPooling2D(pool_size=pool_size))

    _model.add(Conv2D(num_filters, kernel_size))
    _model.add(Activation('relu'))

    #_model.add(MaxPooling2D(pool_size=pool_size))
    #_model.add(MaxPooling2D(pool_size=pool_size))

    #_model.add(Conv2D(num_filters, kernel_size))
    #_model.add(Activation('relu'))

    #_model.add(MaxPooling2D(pool_size=pool_size))

    #_model.add(Conv2D(nb_filters, kernel_size))
    #_model.add(Activation('relu'))

    #_model.add(MaxPooling2D(pool_size=pool_size))

    #----------------------------------------
    _model.add(Flatten())

    _model.add(Dense(num_classes))
    _model.add(Activation('softmax'))

    _model.compile(loss='binary_crossentropy', optimizer='adam', metrics=['accuracy'])

    return _model

def train_model(_model, _xtrain, _ytrain, _xtest, _ytest, _idx):

    # simple early stopping
    es = EarlyStopping(monitor='val_loss', mode='min', verbose=0, patience=3)
    mc = ModelCheckpoint('best_model'+str(_idx)+'.h5', monitor='val_acc', mode='max', verbose=1, save_best_only=True)
    _history = _model.fit(_xtrain, _ytrain, batch_size=batch_size, validation_split=0.2, epochs=20, verbose=0, callbacks=[es, mc])#, roc_callback(training_data=(xtrain, ytrain),validation_data=(xtest, ytest))])
    saved_model = load_model('best_model'+str(_idx)+'.h5')

    # evaluate the model
    _, train_acc = saved_model.evaluate(_xtrain, _ytrain, verbose=0)
    _, test_acc = saved_model.evaluate(_xtest, _ytest, verbose=0)
    print('Train: %.3f, Test: %.3f' % (train_acc, test_acc))

    import numpy as np

    #save_csv(saved_model, xtrain)

    y_pred = _model.predict(X_final)
    _roc = roc_auc_score(Y_final, y_pred, average='macro')
    print(_roc)
    AUC_means.append(_roc)
    #AUC_means.append(AUC[-1])
    AUC.clear()

    return _history

main()

