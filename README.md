# VR4DL <img src="https://raw.githubusercontent.com/Cobanoglu-Lab/VR4DL/master/VR_DL_Data/Repo/vr_icon.png" width="36">
 > VR4DL is a Deep Learning Development Environment in Virtual Reality. With this tool **anyone** can create fully functional deep learning models that solve real-world classification problems.

![Overview of VR Environment](https://raw.githubusercontent.com/Cobanoglu-Lab/VR4DL/master/VR_DL_Data/Repo/Figure_Overview.png)


 
## Built With
* Oculus Rift API
* Tensorflow/Keras
* Unity
* gRPC/Protocol Buffers

## Instructions
1. Configure Conda environment via `environment.yml:`

```sh
$ cd VR_DL_Data
$ conda env create -f environment.yml
$ conda activate VR_Env_GPU
```
2. Run gRPC server for model evaluation.
```sh
$ python server.py
```
2. Run as a standalone Oculus Rift application: `VR_DL.exe`

## PCam Usage
*This guide covers a use case for patches from the publicly available PatchCamelyon16 dataset.*

1. Download patch data from [`github.com/basveeling/pcam:`](github.com/basveeling/pcam) 
 ```sh
 camelyonpatch_level_2_split_train_x.h5.gz
 camelyonpatch_level_2_split_train_y.h5.gz
 camelyonpatch_level_2_split_test_x.h5.gz
 camelyonpatch_level_2_split_test_y.h5.gz
 ```
 2. Generate Training & Testing Data .pickle files:
 ```sh
 $ cd VR_DL_Data
 $ python GenerateData_pCam
 ```
 3. Update .config:
 ```sh
 image_size = 96
 categories = Tumor, Normal
 num_datapoints = 7000
 ```
 
 ## Custom Usage
*This guide covers a use case for custom patch data.*
1. Create a directory with folders corresponding to each class/category name.
*EX: Z:\MyDir\Class1, Z:\Data\Class2, etc. "

1. Specify properties in .config.
 ```sh
 image_size     = ...
 categories     = Class1, Class2, ...
 num_datapoints = ... # per class
 num_testing    = ... # per class
 num_classes    = ...
 directory      = Z:\MyDir
 ```

2. Generate Training & Testing Data .pickle files:
 ```sh
 $ cd VR_DL_Data
 $ python GenerateData
 ```

## Authors
* Kevin C. VanHorn
* Meyer Zinn
* Murat Can Cobanoglu

## Acknowledgments
*Tumor images for classification were provided by Drs. Satwik Rajaram and Payal Kapur who is funded by the Kidney cancer SPORE grant (P50CA196516 ). The software is a derivative of work from the UT Southwestern hackathon, U-HACK Med 2018, and has continued development under the same Principal Investigator (Murat Can Cobanoglu) and lead developer (Kevin VanHorn). The project was originally proposed by Murat Can Cobanoglu, with the final code being submitted to the NCBI-Hackathons GitHub under the MIT License. Hackathon was sponsored by BICF from funding provided by Cancer Prevention and Research Institute of Texas (RP150596). We would like to thank hackathon contributors Xiaoxian Jing (Southern Methodist University), Siddharth Agarwal (University of Texas Arlington), and Michael Dannuzio (University of Texas at Dallas) for their initial work in design and development.*

## Copyright
<<<<<<< HEAD
Copyright © 2019, University of Texas Southwestern Medical Center. All rights reserved.
Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
Department: Lyda Hill Department of Bioinformatics.
This software and any related documentation constitutes published and/or unpublished works and may contain valuable trade secrets and proprietary information belonging to The University of Texas Southwestern Medical Center (UT SOUTHWESTERN).  None of the foregoing material may be copied, duplicated or disclosed without the express written permission of UT SOUTHWESTERN.  IN NO EVENT SHALL UT SOUTHWESTERN BE LIABLE TO ANY PARTY FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES, INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS DOCUMENTATION, EVEN IF UT SOUTHWESTERN HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.  UT SOUTHWESTERN SPECIFICALLY DISCLAIMS ANY WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE AND ACCOMPANYING DOCUMENTATION, IF ANY, PROVIDED HEREUNDER IS PROVIDED "AS IS". UT SOUTHWESTERN HAS NO OBLIGATION TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
This software contains copyrighted materials from Oculus, Unity Technologies, Keras, TensorFlow, gRPC, NumPy, Matplotlib, OpenCV, Pyprind, Nvidia CUDA, wiki.unity3d.com, PyCharm, Visual Studio Community, and Google. Corresponding terms and conditions apply.
=======
*Copyright Â© 2019, University of Texas Southwestern Medical Center. All rights reserved. Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu. Department: Lyda Hill Department of Bioinformatics.*
>>>>>>> 6bca5c753cd90583a58b200bab1492c915494305
