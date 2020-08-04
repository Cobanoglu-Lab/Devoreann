"""  
Copyright © 2020, University of Texas Southwestern Medical Center. All rights reserved.
Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
Department: Lyda Hill Department of Bioinformatics.
This software and any related documentation constitutes published and/or unpublished works and may contain valuable trade secrets and proprietary information belonging to The University of Texas Southwestern Medical Center (UT SOUTHWESTERN).  None of the foregoing material may be copied, duplicated or disclosed without the express written permission of UT SOUTHWESTERN.  IN NO EVENT SHALL UT SOUTHWESTERN BE LIABLE TO ANY PARTY FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES, INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS DOCUMENTATION, EVEN IF UT SOUTHWESTERN HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.  UT SOUTHWESTERN SPECIFICALLY DISCLAIMS ANY WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE AND ACCOMPANYING DOCUMENTATION, IF ANY, PROVIDED HEREUNDER IS PROVIDED "AS IS". UT SOUTHWESTERN HAS NO OBLIGATION TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
This software contains copyrighted materials from Oculus, Unity Technologies, Keras, TensorFlow, gRPC, NumPy, Matplotlib, OpenCV, Pyprind, Nvidia CUDA, wiki.unity3d.com, PyCharm, Visual Studio Community, and Google. Corresponding terms and conditions apply.
"""

import os
import cv2
import configparser
import matplotlib.pyplot as plt
import numpy as np

images_per_row = 3
num_classes = 7

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


def plot_save_all(channelq, X, activations, max_index):
    set_input_layer(X)

    plt.clf()
    plt.cla()
    plt.close()
    plot_save_imgs(activations, max_index)
    for cnt in range(max_index):
        if(activations[cnt].ndim == 4):
            display_activation(activations, 4, 3, cnt)
        else:
            print("Not Displayed: " + str(cnt))


def set_input_layer(X):
    # Choose the input file:
    config = configparser.ConfigParser()
    config.read('input.ini')
    input_id = int(config['INPUT']['input_layer'])  # 0-6

    print('input_id : ' + str(input_id))

    input_image2 = X[input_id]

    input_image2 = np.expand_dims(input_image2, axis=0)

    return input_image2
