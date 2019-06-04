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
