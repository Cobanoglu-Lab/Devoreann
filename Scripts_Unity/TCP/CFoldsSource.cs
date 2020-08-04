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
using UnityEngine.UI;

public class CFoldsSource : MonoBehaviour {

    private List<Image> Folds = new List<Image>();
    private List<Vector3> DefaultPositions = new List<Vector3>();
    private List<CFoldsProgress> foldDisplays = new List<CFoldsProgress>();
    private int curFold = 0;

    public float timeToLerp;

    // Use this for initialization
    void Awake()
    {
        Image[] fList = GetComponentsInChildren<Image>();
        foreach (Image img in fList)
        {
            Folds.Add(img);
            DefaultPositions.Add(img.transform.position);
        }
    }

    public void Reset()
    {
        curFold = 0;
        for(int i =0; i < Folds.Count && i < DefaultPositions.Count; ++i)
        {
            StartCoroutine(LerpToPosition(i, DefaultPositions[i]));
        }
    }

    public void Init(CFoldsProgress c1, CFoldsProgress c2, CFoldsProgress c3)
    {
        foldDisplays.Add(c1);
        foldDisplays.Add(c2);
        foldDisplays.Add(c3);
    }

    public void MoveFoldToGPU(int gpu)
    {
        if(curFold < Folds.Count && gpu < foldDisplays.Count)
        {
            Transform newTrans = foldDisplays[gpu].GetNextImgTransform();
            //Folds[curFold].transform.position = newTrans.position;
            //Folds[curFold].transform.rotation = newTrans.rotation;
            StartCoroutine(LerpToPosition(curFold, newTrans.position));
        }
        curFold++;
    }

    public IEnumerator LerpToPosition(int fold, Vector3 target)
    {
        Vector3 startPos = Folds[fold].transform.position;
        Vector3 curPos = startPos;
        float timeElapsed = 0;
        float invTimeToLerp = 1 / timeToLerp;
        while (Vector3.Distance(curPos, target) > 0.0001f)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed * invTimeToLerp;
            //t = Mathf.Sin(t * Mathf.PI * 0.9f);
            curPos = Vector3.Lerp(startPos, target, Mathf.SmoothStep(0.0f, 1.0f, t));
            Folds[fold].transform.position = curPos;
            yield return null;
        }
    }

    public void Refresh(int num)
    {
        foreach (Image img in Folds)
        {
            img.enabled = false;
        }

        for (int i = 0; i < num; ++i)
        {
            Folds[i].enabled = true;
        }
    }

    
}
