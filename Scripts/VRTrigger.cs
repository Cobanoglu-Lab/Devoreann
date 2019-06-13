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
 * Triggers events to begin computation in the Layer Manager.
 */

[RequireComponent(typeof(Collider))]
public class VRTrigger : MonoBehaviour {

    Light PointLight;
    new Renderer renderer;

    // Bounds of this button, selectable in editor.
    Collider collider;

    private CLayerManager manager;

    private void Awake()
    {
        // Register Components:
        collider = GetComponent<Collider>();
        manager = (CLayerManager)FindObjectOfType(typeof(CLayerManager));
        renderer = GetComponent<Renderer>();


        PointLight = GetComponentInChildren<Light>();
        if (!PointLight) { Debug.LogError("ERROR: No point light found."); return; }

        PointLight.enabled = false;

    }

    /* Begin computation when VR hand intersects with button bounds. */
    private void OnTriggerEnter(Collider other)
    {
        manager.OnStartCompute();
        Debug.LogError("VRButton: OnTriggerEnter() Success.");

        if (PointLight && renderer)
        {
            renderer.material.EnableKeyword("_EMISSION");
            PointLight.enabled = true;
            
        }
    }

    public void StartCompute()
    {
        if (PointLight && renderer)
        {
            renderer.material.EnableKeyword("_EMISSION");
            PointLight.enabled = true;
        }

        manager.OnStartCompute();

    }

    public void DisableLight()
    {
        if (PointLight && renderer)
         {
             renderer.material.DisableKeyword("_EMISSION");
             PointLight.enabled = false;
         }
    }


    private void OnTriggerExit(Collider other)
    {
        /*if (PointLight && renderer)
        {
            renderer.material.DisableKeyword("_EMISSION");
            PointLight.enabled = false;
        }*/
    }
}
