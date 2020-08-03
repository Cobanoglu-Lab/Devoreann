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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.UI;

public class CModelHandler : MonoBehaviour
{
    private List<CGPU> GPUs;
    private CModel Model;
    public List<CFoldsProgress> foldDisplays;
    private CFoldsSource FoldSource;

    public List<GameObject> GPU_UIs;
    private CClient Client;

    public Text ConfidenceText;
    public GameObject ConfidenceContainer;

    public void Start()
    {
        FoldSource = FindObjectOfType<CFoldsSource>();
        GPUs = new List<CGPU>();
        GPUs.Add(new CGPU(0, foldDisplays[0]));
        GPUs.Add(new CGPU(1, foldDisplays[1]));
        GPUs.Add(new CGPU(2, foldDisplays[2]));
        if (FoldSource) 
            FoldSource.Init(foldDisplays[0], foldDisplays[1], foldDisplays[2]);
        Client = FindObjectOfType<CClient>();
    }

    public void StartFold(string fold, int gpu)
    {
        if(gpu >= 0 && gpu < GPUs.Count)
        {
            GPUs[gpu].Fold = fold[fold.Length-1];
            Debug.LogError("Started fold " + GPUs[gpu].Fold);
        }
    }

    // Deprecated
    public void IncrementEpoch(int g, string e)
    {
        if (g >= 0 && g < GPUs.Count)
        {
            string match;
            //Debug.LogWarning(e);
            int epoch;

            match = Regex.Match(e, @"\d\d:").Value;
            if (match.Length > 1)
            {
                int.TryParse(match.Substring(0, 2), out epoch);
                GPUs[g].Epoch = epoch;
                //Debug.LogError(g + " Epoch " + epoch + " -- " + match);
            }
            else
            {
                match = Regex.Match(e, @"\d/\d").Value;
                if (match.Length > 1)
                {
                    int.TryParse(match[0].ToString(), out epoch);
                    GPUs[g].Epoch = epoch;
                    //Debug.LogError(g + " Epoch " + epoch);
                }
            }
        }
    }

    public void SetFold(int g, string data)
    {
        if (FoldSource) FoldSource.MoveFoldToGPU(g);
        //GPUs[g].IncrementFold();
    }

    public void Reset()
    {
        if(FoldSource) FoldSource.Reset();
        foreach (CFoldsProgress display in foldDisplays)
        {
            display.Reset();
            ShowGPUs(false);
        }
    }

    public void ShowGPUs(bool bEnabled)
    {
        foreach(GameObject g in GPU_UIs)
        {
            g.SetActive(bEnabled);
        }
        Client.ResetGPUView();
        //if(ConfidenceText) ConfidenceText.text = "";
        if (ConfidenceContainer) ConfidenceContainer.SetActive(!bEnabled);
    }

    public void ProcessCI(string str)
    {
        try {
            List<float> cs = new List<float>();

            foreach (Match m in Regex.Matches(str, @"\d\.\d\d\d\d"))
            {
                float c;
                float.TryParse(m.Value, out c);
                Debug.LogError(c);
                cs.Add(c);
            }

            if (ConfidenceText) ConfidenceText.text = (cs[0]*100).ToString("0.000").Substring(0,5) + "% - " + (cs[1] * 100).ToString("0.0000").Substring(0,5) + "%";
        }
        catch (Exception e)
        { Debug.LogError("ERROR: ProcessCI invalid. " + e.ToString()); }
    }
}
