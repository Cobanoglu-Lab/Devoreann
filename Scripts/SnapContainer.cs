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

/*
 * Container to hold layers - "snaps" the layer to within that position on drop by the user.
 */
public class SnapContainer : MonoBehaviour {

    BoxCollider Collider; // Bounds of the container.

    public Layer ActiveLayer; // The object reference of the current layer being held.

    private LayerManger manager;

    public int ContainerID = -1; // Set uninitialized ID for this container. ID should be set in Editor.

	/* Initialization on game start. */
	void Start () {
		// Register components:
        Collider = this.GetComponent<BoxCollider>();
       
		// Find layer manager:
	    manager = FindObjectOfType<LayerManger>();
    }

	/* Called when the containers bounds are entered. */
    private void OnTriggerEnter(Collider other)
    {
		
        Layer layer = other.GetComponent<Layer>();
        if(layer != null)
        {
			// Case: collision when falling / not held.
            if (!layer.isBeingHeld())
            {
                ActiveLayer = layer;

                layer.rigidbody.isKinematic = true;
                layer.rigidbody.useGravity = false;
                Vector3 otherT = other.gameObject.transform.position;
                other.gameObject.transform.position = this.transform.position;//.SetPositionAndRotation(otherT - transform.position);
                other.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));

                //CheckForCompleteNetwork(); //Add to trigger stay as well
                //manager.UpdateNodeStructure();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {

        Layer layer = other.GetComponent<Layer>();
        if (layer != null)
        {
			// Case: already within the collision bounds & released from being held:
            if (!layer.isBeingHeld())
            {
                ActiveLayer = layer;

                //UpdateText();


                layer.rigidbody.isKinematic = true;
                layer.rigidbody.useGravity = false;
                Vector3 otherT = other.gameObject.transform.position;
                other.gameObject.transform.position = this.transform.position;//.SetPositionAndRotation(otherT - transform.position);
                other.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));

                
                //CheckForCompleteNetwork();//ADDED - add to trigger stay as well
                //manager.UpdateNodeStructure();
            }
        }
    }


    private void OnTriggerExit(Collider other)
    {
        ActiveLayer = null; //ADDED

        Layer layer = other.GetComponent<Layer>();
        if (layer != null)
        {
            ActiveLayer = null;

            layer.rigidbody.isKinematic = false;
            layer.rigidbody.useGravity = true;
        }
    }

	/* (Unimplemented) Validate the network via the manager. */
    private void CheckForCompleteNetwork()
    {
        Debug.LogError("SNAP");

        if (manager)
        {
            // If all containers are full, execute the network:
            if (manager.AreContainersAllFull())
            {
                manager.ValidateLayers();
                if (manager.hasValidLayers)
                {
                    Debug.LogError("SNAP - Has Valid Layers");

                    // Run machine learning code.
                }
                else Debug.LogError("Invalid Network");
            }
            // If Containers are not full, check this current object is valid:
            else
            {
                Debug.LogError("SNAP - Containers Not full");

                manager.ValidateLayers();
                if (!manager.hasValidLayers)
                {
                    Destroy(ActiveLayer.gameObject); // Destroy the object if not valid
                    ActiveLayer = null;
                }
            }

        }
    }

}
