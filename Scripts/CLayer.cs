using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/* Enum to represent the DL layers. */
public enum CLayerType
{
    Conv,
    Pool,
    Dense,
    Full,
    Dropout
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(OVRGrabbable))]
public class CLayer : MonoBehaviour {
    public new Rigidbody rigidbody;
    public OVRGrabbable grabbable;

    public bool isNew = true;

    public Texture texture_01;
    public Texture texture_02;

    CLayerManager manager;

    public CLayerType layerType;

    [HideInInspector]
    public CSnapContainer ParentContainer = null;

    new Renderer renderer;
    List<Material> mList = new List<Material>(); // Layer contains multiple materials
    Material mActive;

    [HideInInspector]
    public int bakedIndex = -1; // Only set on successful computation
    public int actIndex = -1;   // Only set if can have an accompanying activation layer.

    CAllLayerManager activations;

    bool bActivationNormal = true;

    private void Awake()
    {
        // Grabbable Component:
        rigidbody = GetComponent<Rigidbody>();
        grabbable = GetComponent<OVRGrabbable>();

        // Handle Textures:
        renderer = GetComponent<Renderer>();

        manager = FindObjectOfType<CLayerManager>();

        activations = FindObjectOfType<CAllLayerManager>();
    }

    // TODO: move to containers
    public void SetTexture(Texture2D newTexture)
    {
        if (!renderer) return;
        renderer.GetMaterials(mList);
        if (mList.Count == 3)
        {
            // Here the format of the mesh is 3 material slots [0,12], with 1&2 being the front and back of the display
            mActive = mList[1];
            mActive.mainTexture = newTexture;
            mActive = mList[2];
            mActive.mainTexture = newTexture;
        }

        else if (mList.Count == 4)
        {
            mActive = mList[0];
            mActive.mainTexture = newTexture;
            mActive = mList[1];
            mActive.mainTexture = newTexture;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("GrabVolume"))
        {
            if (activations) activations.SetLayer(this);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        CLayerSpawner spawner = other.GetComponent<CLayerSpawner>();
        if (isNew && spawner)
        {
            if (spawner.layerType == this.layerType)
            {
                StartCoroutine(SetUsed());
            }
        }
    }

    IEnumerator SetUsed()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        isNew = false;
    }

    /* Check if the VR Controller is being held. */
    public bool isBeingHeld()
    {
        return grabbable.isGrabbed;
    }

    /* Returns the ID of the parent container, -1 if DNE. */
    /*public int GetContainerID()
    {
        if (ParentContainer) return ParentContainer.ContainerID;
        else return -1;
    }*/

    public int GetIndex()
    {
        if(activations) bActivationNormal = activations.bUseNormal;

        if (!bActivationNormal && actIndex>=0)
        {
            return actIndex + 2;
        }
        else {
            return bakedIndex + 2;
        }
    }
}
