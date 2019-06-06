"""
Copyright © 2019, University of Texas Southwestern Medical Center. All rights reserved.
Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
Department: Lyda Hill Department of Bioinformatics
"""

from __future__ import print_function

import os
import cv2
import sys
import configparser
import pickle

import evaluator_pb2
import evaluator_pb2_grpc

import keras
from keras import backend as K
from keras.models import Sequential
from keras.layers import Dense, Dropout, Activation, Flatten
from keras.layers import Conv2D, MaxPooling2D
from keras.models import Model
from keras.utils import to_categorical
from keras.callbacks import Callback

from sklearn.model_selection import StratifiedKFold
from keras.callbacks import EarlyStopping
from keras.callbacks import ModelCheckpoint
from keras.models import load_model

import numpy as np
import statistics
import matplotlib.pyplot as plt
from sklearn.metrics import roc_auc_score
import scipy.stats as st
from sklearn.model_selection import train_test_split

# Set the GPU to use
os.environ["CUDA_DEVICE_ORDER"] = "PCI_BUS_ID"
os.environ["CUDA_VISIBLE_DEVICES"] = "1"

# Global Properties
batch_size = 32
num_classes = 7
epochs = 2
num_filters = 32
pool_size = (2, 2)
kernel_size = (3, 3)
dropout_dim = 0.5
dense_neurons = 32
b_eval_advanced = False

IMG_SIZE = 128
img_rows, img_cols = IMG_SIZE, IMG_SIZE

AUC = []  # For an individual fold
AUC_means = []  # For finding the median fivefold mean AUC over all datasets

def main():
    read_config()
    load_data()

def evaluate_simple(model):
    return train_simple(model)


def evaluate_advanced(model):
    global AUC_means, AUC_median, X, Y

    # region Run Local Testing
    kfold = StratifiedKFold(n_splits=5, shuffle=True)

    # Loop through the indices the split() method returns
    for index, (train_indices, test_indices) in enumerate(kfold.split(X, Y)):
        if index == 0:
            Y = to_categorical(Y, num_classes)

        # Generate batches from indices
        xtrain, xtest = X[train_indices], X[test_indices]
        ytrain, ytest = Y[train_indices], Y[test_indices]

        model = load_model('complex_model.h5')

        # Create new model.
        history = train_advanced(model, xtrain, ytrain, xtest, ytest, index)
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
    return AUC_median

def read_config():
    """Set user-defined global properties.
    """
    global batch_size, num_classes, num_filters, dropout_dim, dense_neurons
    global b_eval_advanced, pool_size, kernel_size, IMG_SIZE, epochs, img_cols, img_rows

    config = configparser.ConfigParser()
    config.read('config.ini')

    batch_size = int(config['MODEL']['batch_size'])
    num_filters = int(config['MODEL']['num_filters'])
    dropout_dim = float(config['MODEL']['dropout_dim'])
    dense_neurons = int(config['MODEL']['dense_neurons'])
    _pool_size = config['MODEL']['pool_size']
    _kernel_size = config['MODEL']['kernel_size']
    IMG_SIZE = int(config['DATA']['image_size'])
    num_classes = int(config['CUSTOM']['num_classes'])
    epochs = int(config['MODEL']['epochs'])
    b_eval_advanced = (config['MODEL']['complex_analysis'] == 'true' or config['MODEL']['complex_analysis'] == 'True')

    pool_size = tuple(map(int, _pool_size.split(',')))
    kernel_size = tuple(map(int, _kernel_size.split(',')))

    img_rows, img_cols = IMG_SIZE, IMG_SIZE

def load_data():
    """Load data from pickle files.
    """
    global X, Y, X_final, Y_final, input_shape

    pickle_in = open("X_train.pickle", "rb")
    X = pickle.load(pickle_in)
    pickle_in = open("y_train.pickle", "rb")
    Y = pickle.load(pickle_in)

    pickle_in = open("X_test.pickle", "rb")
    X_final = pickle.load(pickle_in)
    pickle_in = open("y_test.pickle", "rb")
    Y_final = pickle.load(pickle_in)

    if K.image_data_format() == 'channels_first':
        input_shape = (3, img_rows, img_cols)
    else:
        input_shape = (img_rows, img_cols, 3)

    X = X.astype('float32')
    X /= 255
    X_final = X_final.astype('float32')
    X_final /= 255
    print('X shape:', X.shape)
    print(X.shape[0], 'Samples')

    Y_final = to_categorical(Y_final, num_classes)

    if not b_eval_advanced:
        Y = to_categorical(Y, num_classes)

    print("Y_final Shape",Y_final.shape)


def train_simple(model):
    global X, Y

    _xtrain, _xtest, _ytrain, _ytest = train_test_split(X, Y, test_size=0.2)

    # simple early stopping
    es = EarlyStopping(monitor='val_loss', mode='min', verbose=0, patience=2)
    mc = ModelCheckpoint('best_model'+str('')+'.h5', monitor='val_acc', mode='max', verbose=1, save_best_only=True)
    _history = model.fit(_xtrain, _ytrain, batch_size=batch_size, validation_split=0.2, epochs=epochs, verbose=1, callbacks=[es, mc])#, roc_callback(training_data=(xtrain, ytrain),validation_data=(xtest, ytest))])
    saved_model = load_model('best_model'+str('')+'.h5')

    # evaluate the model
    _, train_acc = saved_model.evaluate(_xtrain, _ytrain, verbose=0)
    _, test_acc = saved_model.evaluate(_xtest, _ytest, verbose=0)
    print('Train: %.3f, Test: %.3f' % (train_acc, test_acc))

    score = model.evaluate(X_final, Y_final, verbose=0)

    return score[1]


def train_advanced(model, _xtrain, _ytrain, _xtest, _ytest, _idx):
    # simple early stopping
    es = EarlyStopping(monitor='val_loss', mode='min', verbose=0, patience=2)
    mc = ModelCheckpoint('best_model'+str(_idx)+'.h5', monitor='val_acc', mode='max', verbose=1, save_best_only=True)
    _history = model.fit(_xtrain, _ytrain, batch_size=batch_size, validation_split=0.2, epochs=20, verbose=0, callbacks=[es, mc])#, roc_callback(training_data=(xtrain, ytrain),validation_data=(xtest, ytest))])
    saved_model = load_model('best_model'+str(_idx)+'.h5')

    # evaluate the model
    _, train_acc = saved_model.evaluate(_xtrain, _ytrain, verbose=0)
    _, test_acc = saved_model.evaluate(_xtest, _ytest, verbose=0)
    print('Train: %.3f, Test: %.3f' % (train_acc, test_acc))

    y_pred = model.predict(X_final)
    _roc = roc_auc_score(Y_final, y_pred, average='macro')
    print(_roc)
    AUC_means.append(_roc)
    AUC.clear()

    return _history

class Evaluator(evaluator_pb2_grpc.EvaluatorServicer):
    def Evaluate(self, request, context):
        K.clear_session()
        clear_texture_cache()

        # Build Keras model
        print("Received evaluate request")
        model = Sequential()
        model.add(Conv2D(num_filters, kernel_size, input_shape=input_shape))
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
                model.add(Conv2D(num_filters, kernel_size))
                model.add(Activation('relu'))
            elif typ == "dropout":
                dropout = layer.dropout
                model.add(Dropout(dropout_dim))
                # do something here
            elif typ == "flatten":
                model.add(Flatten())
            elif typ == "dense":
                dense = layer.dense
                model.add(Dense(dense_neurons))
                model.add(Activation('relu'))
            elif typ == "maxpooling":
                model.add(MaxPooling2D(pool_size=pool_size))

        # classification layer
        model.add(Dense(num_classes))
        model.add(Activation('softmax'))

        model.compile(loss='binary_crossentropy', optimizer='adam', metrics=['accuracy'])

        if not b_eval_advanced:
            orig_stdout = sys.stdout
            f = open('out.txt', 'w')
            sys.stdout = f
            acc = evaluate_simple(model)
            sys.stdout = orig_stdout
            f.close()
        else:
            model.save('complex_model.h5')
            acc = evaluate_advanced(model)
            final_model = load_model('final_model.h5')
            model = final_model

        model.summary()

        layer_outputs = [layer.output for layer in model.layers]
        activation_model = Model(inputs=model.input, outputs=layer_outputs)
        activations = activation_model.predict(set_input_layer())
        plot_save_all(activations, len(activations) - 1)

        return evaluator_pb2.ProgressUpdate(accuracy=acc)


# region Image Functions
def clear_texture_cache():
    for file in os.scandir(os.path.join(os.getcwd(),'Images')):
        print(file)
        if file.name.endswith('.png'):
            #print("removed")
            os.remove(file)
    print('Cleared texture cache.')
    # clear textures

def plot_save_input(input):
    plt.imshow(input)
    plt.axis('off')
    plt.setp(plt.gcf().get_axes(), xticks=[], yticks=[])
    plt.savefig('Images/input_image.png', frameon='false', bbox_inches='tight', transparent=True, pad_inches=0.0)

clear_texture_cache()

images_per_row = 3

# Saves an entire figure for a layer w/ index
def plot_save_fig(fig, index):
    plt.setp(plt.gcf().get_axes(), xticks=[], yticks=[])
    fig.savefig('Images/all_layer' + str(index) + '.png', frameon='false', bbox_inches='tight', transparent=True,
                pad_inches=0.0)

def plot_save_imgs(activations,max_index):
    for cnt in range(max_index):
        print(cnt)
        s = 'Images/activation' + str(cnt) + '.png'
        curLayer = activations[cnt]
        if(curLayer.ndim == 4):
            plt.clf()
            plt.cla()
            plt.close()
            plt.imshow(curLayer[0, :, :, 0], interpolation='none', cmap='viridis')
            plt.axis('off')
            plt.setp(plt.gcf().get_axes(), xticks=[], yticks=[])
            plt.savefig(s, frameon='false', bbox_inches='tight', transparent=True, pad_inches=0.0)
            # plt.show()
        elif (curLayer.ndim == 2):
            plt.axis([0, 1, 0, 1])
            plt.savefig(s, frameon='false', bbox_inches='tight', transparent=True, pad_inches=0.0)

def display_activation(activations, col_size, row_size, act_index):
    activation = activations[act_index]
    activation_index = 0
    fig, ax = plt.subplots(row_size, col_size, figsize=(row_size * 2.5, col_size * 1.5))
    for row in range(0, row_size):
        for col in range(0, col_size):
            ax[row][col].imshow(activation[0, :, :, activation_index])
            activation_index += 1
    plot_save_fig(fig, act_index)
    plt.clf()
    plt.cla()
    plt.close()

def plot_save_all(activations, max_index):
    set_input_layer()

    plt.clf()
    plt.cla()
    plt.close()
    plot_save_imgs(activations, max_index)
    for cnt in range(max_index):
        if(activations[cnt].ndim == 4):
            display_activation(activations, 4, 3, cnt)
        else:
            print("Not Displayed: " + str(cnt))

def set_input_layer():
    # Choose the input file:
    config = configparser.ConfigParser()
    config.read('input.ini')
    input_id = int(config['INPUT']['input_layer'])  # 0-6

    print('input_id : ' + str(input_id))

    input_image2 = cv2.imread("blood1.png")

    if input_id < num_classes - 1:
        input_image2 = X[input_id]
    else:
        input_image2 = X[0]

    input_image2 = np.expand_dims(input_image2, axis=0)

    return input_image2
# endregion

main()
