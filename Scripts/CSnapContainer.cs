/* Copyright © 2019, University of Texas Southwestern Medical Center. All rights reserved.
 *  Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
 *  Department: Lyda Hill Department of Bioinformatics 
 *  
 *  Copyright © 2019, University of Texas Southwestern Medical Center. All rights reserved.
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
public class CSnapContainer : MonoBehaviour {

    BoxCollider Collider; // Bounds of the container.

    public CLayer ActiveLayer; // The object reference of the current layer being held.
    private CLayerManager manager;
    public int ContainerID = -1; // Set uninitialized ID for this container. ID should be set in Editor (start at 0).

    /* Initialization on game start. */
    void Awake()
    {
        // Register components:
        Collider = this.GetComponent<BoxCollider>();

        // Find layer manager:
        manager = FindObjectOfType<CLayerManager>();
    }

    /* Called when the containers bounds are entered. */
    private void OnTriggerEnter(Collider other)
    {
        CLayer layer = other.GetComponent<CLayer>();
        if (layer != null)
        {
            // Case: collision when falling / not held.
            if (!layer.isBeingHeld())
            {
                ActiveLayer = layer;
                ActiveLayer.ParentContainer = this;
                SnapLayerToContainer(ActiveLayer);
                //if (manager) { manager.OnAddLayer(); }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        CLayer layer = other.GetComponent<CLayer>();
        if (layer != null)
        {
            // Case: already within the collision bounds & released from being held:
            if (!layer.isBeingHeld())
            {
                SnapLayerToContainer(layer);

                if (ActiveLayer != layer)
                {
                    ActiveLayer = layer;
                    ActiveLayer.ParentContainer = this;
                    //if (manager) { manager.OnAddLayer(); }
                }
            }
        }
    }

    /* Snap a given layer to the container & add to the manager list. */
    private void SnapLayerToContainer(CLayer layer)
    {
        layer.rigidbody.isKinematic = true;
        layer.rigidbody.useGravity = false;
        layer.gameObject.transform.position = this.transform.position;
        layer.gameObject.transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 180));
    }

    private void OnTriggerExit(Collider other)
    {
        ActiveLayer = null; //ADDED

        CLayer layer = other.GetComponent<CLayer>();
        if (layer != null)
        {
            if(ActiveLayer)
                ActiveLayer.ParentContainer = null;

            ActiveLayer = null;

            //layer.rigidbody.isKinematic = false;
            //layer.rigidbody.useGravity = false;
            //if (manager) { manager.OnRemoveLayer(); }
        }
    }
}
