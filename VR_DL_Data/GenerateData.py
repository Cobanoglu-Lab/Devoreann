from __future__ import print_function

import numpy as np
import os
import cv2
import random
import pyprind  # progress bar
import pickle
import configparser

os.environ["CUDA_DEVICE_ORDER"] = "PCI_BUS_ID"
os.environ["CUDA_VISIBLE_DEVICES"] = "0"

DATADIR = "Z:/0_128"
CATEGORIES = ["blood", "inflammatory", "necrosis", "normal", "stroma", "tumor", "background"]
num_datapoints = 2000
num_testing = 3000
IMG_SIZE = 128
training_data = []
testing_data = []


def read_config():
    """Specify user-defined properties & custom dataset.
    """
    global num_datapoints, num_testing, IMG_SIZE, CATEGORIES, DATADIR

    config = configparser.ConfigParser()
    config.read('config.ini')

    IMG_SIZE = int(config['DATA']['image_size'])
    num_datapoints = int(config['DATA']['num_datapoints'])
    num_testing = int(config['DATA']['num_testing'])
    CATEGORIES = config['CUSTOM']['categories'].split(',')
    DATADIR = config['CUSTOM']['directory']


def load_images():
    """Load all images for early stopping (train, test, validation) and advanced testing.
    """
    progress = pyprind.ProgBar(((num_testing + num_datapoints) * len(CATEGORIES)), monitor=True, title='Finding Files...')

    # Load the images for training/testing output:
    for category in CATEGORIES:
        path = os.path.join(DATADIR, category)                # Path to each directory
        class_num = CATEGORIES.index(category)
        idx = 0
        for img in os.listdir(path):
            progress.update()                                 # Update progress bar
            img_array = cv2.imread(os.path.join(path, img))
            if idx < num_datapoints:                           # Load train data (0 -> 999)
                training_data.append([img_array, class_num])
            elif idx >= num_datapoints and idx < num_testing+num_datapoints:      # Load test data  (1000 -> 1999)
                testing_data.append([img_array, class_num])
            else:
                break
            idx += 1

    random.shuffle(training_data)
    random.shuffle(testing_data)

    print('Loaded all images.')


def save_data():
    """Export pickle files.
    """
    X_train = []
    y_train = []
    for features, label in training_data:
        X_train.append(features)
        y_train.append(label)

    X_test = []
    y_test = []
    for features, label in testing_data:
        X_test.append(features)
        y_test.append(label)

    print(len(training_data))
    print(len(testing_data))

    X_train = np.array(X_train).reshape(-1, IMG_SIZE, IMG_SIZE, 3)
    X_test = np.array(X_test).reshape(-1, IMG_SIZE, IMG_SIZE, 3)

    # Save data to pickle file
    pickle_out = open("X_train.pickle","wb")
    pickle.dump(X_train, pickle_out)
    pickle_out.close()

    pickle_out = open("y_train.pickle","wb")
    pickle.dump(y_train, pickle_out)
    pickle_out.close()

    pickle_out = open("X_test.pickle","wb")
    pickle.dump(X_test, pickle_out)
    pickle_out.close()

    pickle_out = open("y_test.pickle","wb")
    pickle.dump(y_test, pickle_out)
    pickle_out.close()

read_config()
load_images()
save_data()
