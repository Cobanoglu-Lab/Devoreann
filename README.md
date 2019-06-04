# VR4DL <img src="https://raw.githubusercontent.com/Cobanoglu-Lab/VR4DL/master/VR_DL_Data/Repo/vr_icon_.png" width="36.5">
 > VR4DL is a Deep Learning Development Environment in Virtual Reality. With this tool **anyone** can create fully functional deep learning models that solve real-world classification problems.

 
## Built With
* Oculus Rift API
* Tensorflow/Keras
* Unity
* gRPC/Protocol Buffers

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

## Authors
* Kevin C. VanHorn
* Meyer Zinn
* Murat Can Çobanoğlu

## Acknowledgments
*Tumor images for classification were provided by Drs. Satwik Rajaram and Payal Kapur who is funded by the Kidney cancer SPORE grant (P50CA196516 ). The software is a derivative of work from the UT Southwestern hackathon, U-HACK Med 2018, and has continued development under the same Principal Investigator (Murat Can Çobanoğlu) and lead developer (Kevin VanHorn). The project was originally proposed by Murat Can Çobanoğlu, with the final code being submitted to the NCBI-Hackathons GitHub under the MIT License. Hackathon was sponsored by BICF from funding provided by Cancer Prevention and Research Institute of Texas (RP150596). We would like to thank hackathon contributors Xiaoxian Jing (Southern Methodist University), Siddharth Agarwal (University of Texas Arlington), and Michael Dannuzio (University of Texas at Dallas) for their initial work in design and development.*
