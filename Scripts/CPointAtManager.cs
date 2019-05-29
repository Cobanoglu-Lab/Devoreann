using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPointAtManager : MonoBehaviour {

    OVRCameraRig CameraRig;
    Transform LeftHand;
    Transform RightHand;

    LayerMask layerMask;

    LineRenderer lineRenderer;

    public GameObject Highlight;

    public bool bUsingRight = true;

    public float length = 2;

    bool bIsSelecting = false;

    int inputIndex = 0;

    CLayerManager layerManager;

    string path = "";

    void Awake()
    {
        layerMask = LayerMask.GetMask("Pointable");
        CameraRig = FindObjectOfType<OVRCameraRig>();
        Transform Parent = CameraRig.transform.Find("TrackingSpace");
        RightHand = Parent.Find("RightHandAnchor");
        LeftHand = Parent.Find("LeftHandAnchor");
        lineRenderer = GetComponent<LineRenderer>();

        layerManager = FindObjectOfType<CLayerManager>();
    }
	
	// Update is called once per frame
	void Update () {
        //Debug.DrawLine(LeftHand.position, LeftHand.position + LeftHand.up*10);
        //Debug.DrawLine(RightHand.position, RightHand.position + RightHand.up * 10);

        bIsSelecting = (OVRInput.Get(OVRInput.Button.One) || OVRInput.Get(OVRInput.Button.Three));

        //if (layerManager.isComputing) bIsSelecting = false; 

        if (bIsSelecting)
        {
            lineRenderer.enabled = true;
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(RightHand.position, RightHand.forward, out hit, Mathf.Infinity, layerMask))
            {
                bUsingRight = true;
                lineRenderer.SetPosition(0, RightHand.position);
                lineRenderer.SetPosition(1, hit.point);

                VRTrigger trigger = hit.collider.gameObject.GetComponent<VRTrigger>();
                if (trigger)
                {
                    trigger.StartCompute();
                    return;
                }

                Debug.DrawRay(RightHand.position, RightHand.forward * hit.distance, Color.yellow);
                MoveHighlight(hit.transform, hit);
            }
            else if (Physics.Raycast(LeftHand.position, LeftHand.forward, out hit, Mathf.Infinity, layerMask))
            {
                bUsingRight = false;
                lineRenderer.SetPosition(0, LeftHand.position);
                lineRenderer.SetPosition(1, hit.point);

                VRTrigger trigger = hit.collider.gameObject.GetComponent<VRTrigger>();
                if (trigger)
                {
                    trigger.StartCompute();
                    return;
                }

                Debug.DrawRay(LeftHand.position, LeftHand.forward * hit.distance, Color.yellow);
                MoveHighlight(hit.transform, hit);
            }
            else
            {
                if (bUsingRight)
                {
                    lineRenderer.SetPosition(0, RightHand.position);
                    lineRenderer.SetPosition(1, RightHand.position + RightHand.forward * length);
                }
                else
                {
                    lineRenderer.SetPosition(0, LeftHand.position);
                    lineRenderer.SetPosition(1, LeftHand.position + LeftHand.forward * length);
                }
            }
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }

    private void Start()
    {
        if (Application.isEditor)
        {
            path = "C:/Users/VR_Demo/Desktop/VR_DevelopmentIDE/VR_DevelopmentIDE/VR_DL/Package/input.ini";
        }
        else
        {
            path = Application.dataPath + "/input.ini";
        }
    }

    void MoveHighlight(Transform newLoc, RaycastHit hit)
    {
        if (Highlight)
        {
            Highlight.transform.position = newLoc.position;
        }

        CInputTexture input = hit.collider.gameObject.GetComponent<CInputTexture>();
        if (input)
        {
            inputIndex = input.index;
        }
    }

    public void WriteToConfig()
    {
        string[] lines = { "[INPUT]", "input_layer =" + inputIndex };
        System.IO.File.WriteAllLines(path, lines);
    }
}
