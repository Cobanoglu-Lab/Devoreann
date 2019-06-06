/* Copyright © 2019, University of Texas Southwestern Medical Center. All rights reserved.
 *  Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
 *  Department: Lyda Hill Department of Bioinformatics 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text.RegularExpressions;
using System.IO;

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

    public List<TextMesh> classTexts;

    string path = "";
    private string conf; // Location of configuration file.

    [HideInInspector]
    public string[] categories = new string[6];

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

    void Start()
    {
        if (Application.isEditor)
        {
            conf = "C:/Users/VR_Demo/Desktop/VR_DevelopmentIDE/VR_DevelopmentIDE/VR_DL/Package/config.ini";
            path = "C:/Users/VR_Demo/Desktop/VR_DevelopmentIDE/VR_DevelopmentIDE/VR_DL/Package/input.ini";

        }
        else
        {
            conf = Application.dataPath + "/config.ini";
            path = Application.dataPath + "/input.ini";

        }

        SetConfValues();
    }

    void SetConfValues()
    {
        string conf_str = File.ReadAllText(conf);

        Regex categories = new Regex(@"categories\s=\s\S*");
        Regex split = new Regex(@"\S*,\S*");
        string[] values = new string[0];

        Match match = categories.Match(conf_str);
        if (match.Success)
        {
            match = split.Match(match.Value);
            if (match.Success)
            {
                values = match.Value.Split(',');
            }
        }


        int cnt = 0;
        for(int i = 0; i < values.Length && i < 6; ++i)
        {
            classTexts[i].text = values[i];
            cnt++;
        }

        for(int i = cnt; i < 6;) // 0 < 4
        {
            for(int j = 0; j < cnt; ++j) // ex stride of 2
            {
                if (i+j < 6)
                {
                    classTexts[i + j].text = values[j];
                }
            }
            i+=cnt;

        }
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


/*
        Regex r_numClasses = new Regex(@"num_classes\s=\s\d*");
        Regex r_digit = new Regex(@"\d*");

        match = r_numClasses.Match(conf_str);
        if (match.Success)
        {
            match = r_digit.Match(match.Value);
            if (match.Success)
            {
                int.TryParse(match.Value, out num_classes);
            }
        }
*/