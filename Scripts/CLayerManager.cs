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

public class CLayerManager : MonoBehaviour {
    //public CSnapContainer[] Containers; // Static array of containers.

    [HideInInspector]
    public List<CLayer> Layers;   // Dynamic list of layers.

    public string logpath;

    const int MAX_LAYERS = 8;

    CTextureLoader_Layers textureLoader;
    CUIManager UI;

    CPointAtManager pointAtManager;

    VRTrigger button;

    public bool bShouldThrow = false;

    // Evaluator
    EvaluateSimple evalSimple;
    [HideInInspector]
    public bool isComputing = false;
    public bool bDoComputation = false; // Debugging.

    private void Awake()
    {
        textureLoader = FindObjectOfType<CTextureLoader_Layers>();
        evalSimple = FindObjectOfType<EvaluateSimple>();
        UI = FindObjectOfType<CUIManager>();
        button = FindObjectOfType<VRTrigger>();
        pointAtManager = FindObjectOfType<CPointAtManager>();
    }

    /* Returns a layer at a given index, returning null if it does not exist*/
    public CLayer GetLayer(int LayerID)
    {
        if (LayerID < Layers.Count)
        {
            return Layers[LayerID];
        }
        return null;
    }

    #region Evaluator

    public void OnStartCompute()
    {
        // Only continue if the server is not already computing:
        if (isComputing) return;
        isComputing = true;

        if(pointAtManager) pointAtManager.WriteToConfig();

        if (UI) UI.UpdateProgressText("   Processing...");
        else { Debug.LogError("ERROR: No UI Object found. "); }

        if (evalSimple)
        {
            CLayerType[] layers = new CLayerType[Layers.Count];
            for (int i = 0; i < Layers.Count; i++)
            {
                layers[i] = Layers[i].layerType;
            }
            StartCoroutine(evalSimple.Evaluate(layers));
        }
    }

    public void CompleteComputation(float acc)
    {
        isComputing = false;
        if(UI) UI.UpdateProgressText("Accuracy: " + (acc * 100).ToString("0.00") + "%");
        Debug.LogError("Completed! Accuracy = " + acc);
        if(textureLoader) textureLoader.LoadAllLayerActivations();
        if (button) button.DisableLight();
    }

    public void Update()
    {
        //print(OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger));
        //print(OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger));

        if (bDoComputation)
        {
            bDoComputation = false;
            OnStartCompute();
        }
    }

    public void Start()
    {
        if (Application.isEditor)
        {
            logpath = "C:/Users/VR_Demo/Desktop/VR_DevelopmentIDE/VR_DevelopmentIDE/VR_DL/Package/out.txt";//"H:/VR_DevelopmentIDE/VR_DevelopmentIDE/VR_DL/Pathology/Images";
        }
        else
        {
            logpath = Application.dataPath + "/out.txt";
        }
    }

    public void UpdateProgress(string line)
    {
        if (UI) UI.UpdateProgressText(line);
    }
    #endregion

}
