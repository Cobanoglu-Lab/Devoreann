using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(OVRGrabbable))]

public class CListable : MonoBehaviour {
    public new Rigidbody rigidbody;
    public OVRGrabbable grabbable;

    public CLayer Layer;

    CListableManager listableManager;

    [HideInInspector]
    public CLayerType type;

    [HideInInspector]
    public int ListableIndex;

    [HideInInspector]
    public bool bActiveListable = false;

    //public float Speed = 10;
    float Speed;

    [HideInInspector]
    public Vector3 TargetLoc;

    public void DisableGravity()
    {
        rigidbody.isKinematic = false;
        rigidbody.useGravity = false;
    }

    private void Awake()
    {
        Layer = GetComponent<CLayer>();

        listableManager = FindObjectOfType<CListableManager>();
        grabbable = GetComponent<OVRGrabbable>();
        rigidbody = GetComponent<Rigidbody>();

        if(listableManager) Speed = listableManager.Speed;

        TargetLoc = transform.position;
        bActiveListable = false;
    }

    private void Start()
    {
        type = Layer.layerType;
    }

    private void Update()
    {
        Vector3 position = transform.position;

        if (bActiveListable && !grabbable.isGrabbed && (TargetLoc - position).sqrMagnitude > 0.000001f)
        {
            rigidbody.position = Vector3.Lerp(position, TargetLoc, Time.deltaTime * Speed);
            rigidbody.rotation = listableManager.transform.rotation*Quaternion.Euler(-90, 0, 180);
        }   
    }

    private void OnTriggerEnter(Collider other)
    {
        CListableManager manager = other.GetComponent<CListableManager>();

        if (manager)
        {
            rigidbody.isKinematic = true;
            bActiveListable = true;
            manager.AddNode(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CListableManager manager = other.GetComponent<CListableManager>();

        if (manager)
        {
            bActiveListable = false;
            manager.RemoveNode(this);
        }
    }

    public bool IsHeld()
    {
        return grabbable.isGrabbed;
    }

    public void Delete()
    {
        if (listableManager)
        {
            bActiveListable = false;
            listableManager.RemoveNode(this);
        }
        Destroy(this.gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponent<CListableManager>() != null)
        {
            if (grabbable && grabbable.isGrabbed)
            {
                listableManager.MoveNode(this);
            }
            else
            {
                rigidbody.isKinematic = true;
            }
        }
    }
}

