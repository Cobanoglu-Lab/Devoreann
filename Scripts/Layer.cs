/* Copyright © 2020, University of Texas Southwestern Medical Center. All rights reserved.
 *  Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
 *  Department: Lyda Hill Department of Bioinformatics 
 *  
 *  Copyright © 2020, University of Texas Southwestern Medical Center. All rights reserved.
Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
Department: Lyda Hill Department of Bioinformatics.
This software and any related documentation constitutes published and/or unpublished works and may contain valuable trade secrets and proprietary information belonging to The University of Texas Southwestern Medical Center (UT SOUTHWESTERN).  None of the foregoing material may be copied, duplicated or disclosed without the express written permission of UT SOUTHWESTERN.  IN NO EVENT SHALL UT SOUTHWESTERN BE LIABLE TO ANY PARTY FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES, INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS DOCUMENTATION, EVEN IF UT SOUTHWESTERN HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.  UT SOUTHWESTERN SPECIFICALLY DISCLAIMS ANY WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE AND ACCOMPANYING DOCUMENTATION, IF ANY, PROVIDED HEREUNDER IS PROVIDED "AS IS". UT SOUTHWESTERN HAS NO OBLIGATION TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
This software contains copyrighted materials from Oculus, Unity Technologies, Keras, TensorFlow, gRPC, NumPy, Matplotlib, OpenCV, Pyprind, Nvidia CUDA, wiki.unity3d.com, PyCharm, Visual Studio Community, and Google. Corresponding terms and conditions apply.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OVRTouchSample;

/* Enum to represent the DL layers. */
public enum LayerType{
    Conv,
    Pool,
    Dense,
    Full,
    Dropout
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(OVRGrabbable))]

/* A moveable game object that represents a layer in the DL network. */
public class Layer : MonoBehaviour {
    //public bool isBeingHeld = true;
    
    public new Rigidbody rigidbody;
    public OVRGrabbable grabbable;

    public LayerType layerType;
    private LayerManger manager;

    private Material material;

	/* Check if the VR Controller is being held. */
    public bool isBeingHeld()
    {
        return grabbable.isGrabbed;
    }

	/* Called by UnityEngine on initialization. */
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        grabbable = GetComponent<OVRGrabbable>(); // Oculus Grabbable Component for Unity
    }

	/* Configure physics settings for the layer on event pickup. */
    void OnPickup()
    {
        rigidbody.isKinematic = false;
        rigidbody.useGravity = true;
    }

	/* Called by Unity when this gameObject is intersected by a trigger volume. */
    private void OnTriggerEnter(Collider other)
    {
        // For intial behvaiour after taking from spwan volume:
        OVRGrabber grabber = other.GetComponent<OVRGrabber>();
        if (grabber != null)
        {
            //OnPickup();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Debug.LogError("hfi");
    }

    /*private void OnCollisionEnter(Collision collision)
    {
        if (!isBeingHeld())
        {
            SnapContainer container = collision.collider.GetComponent<SnapContainer>();
            if(container == null)
            {
                Destroy(gameObject);
            }
        }
    }*/
}
