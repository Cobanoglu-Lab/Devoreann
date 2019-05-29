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
