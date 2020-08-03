from __future__ import print_function

# set the GPU to use
import os
os.environ["CUDA_DEVICE_ORDER"]="PCI_BUS_ID"
os.environ["CUDA_VISIBLE_DEVICES"] = "0"

# Generate custom data from pathology images
import numpy as np
import matplotlib.pyplot as plt
import os
import cv2
import random
import pyprind  # progress bar
import pickle

DATADIR = "Z:/0_128"
CATEGORIES = ["blood", "inflammatory", "necrosis", "normal", "stroma", "tumor", "background"]
numDatapoints = 1000#2000 #800 #1920 #12800, 3200   #2000/500
numTesting = 1000#3000#200 #600
numValidation = 0#2000#480
IMG_SIZE = 128

progress = pyprind.ProgBar(numDatapoints * 8, monitor=True, title='Finding Files...')

training_data = []
testing_data = []
validation_data = []

# Load the images for training/testing output:
for category in CATEGORIES:
    path = os.path.join(DATADIR, category)                # Path to each directory
    class_num = CATEGORIES.index(category)
    idx = 0
    for img in os.listdir(path):
        progress.update()                                 # Update progress bar
        img_array = cv2.imread(os.path.join(path, img))
        if idx < numDatapoints:                           # Load train data (0 -> 999)
            training_data.append([img_array, class_num])
        elif idx >= numDatapoints and idx < numTesting+numDatapoints:      # Load test data  (1000 -> 1999)
            testing_data.append([img_array, class_num])
        elif idx < numTesting+numDatapoints+numValidation:
            validation_data.append([img_array, class_num])
        else:
            break
        idx += 1

print('Loaded all images.')

random.shuffle(training_data)
random.shuffle(testing_data)
random.shuffle(validation_data)

# Set Train/Test vars
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

X_val = []
y_val = []
for features, label in validation_data:
    X_val.append(features)
    y_val.append(label)

print(len(training_data))
print(len(testing_data))
print(len(validation_data))

# print(X[0].reshape(-1, IMG_SIZE, IMG_SIZE, 1))
X_train = np.array(X_train).reshape(-1, IMG_SIZE, IMG_SIZE, 3)
X_test = np.array(X_test).reshape(-1, IMG_SIZE, IMG_SIZE, 3)
X_val = np.array(X_test).reshape(-1, IMG_SIZE, IMG_SIZE, 3)

# region Save data to pickle

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

pickle_out = open("X_val.pickle","wb")
pickle.dump(X_val, pickle_out)
pickle_out.close()

pickle_out = open("y_val.pickle","wb")
pickle.dump(y_val, pickle_out)
pickle_out.close()
# endregion