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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSizeSphere : MonoBehaviour {

    Vector3 InitialSize;

    Vector3 VSize;

    int size = 126;

    public float Speed = 10;

    public GameObject textObj;
    TextMesh text;

    CLayerManager manager;

    private void Awake()
    {
        InitialSize = gameObject.transform.localScale;
        VSize = InitialSize;
        manager = FindObjectOfType<CLayerManager>();

        if(textObj) text = textObj.GetComponent<TextMesh>();
    }
	
	public void UpdateSize()
    {
        CalcSize();
        VSize = InitialSize * size/128;

        if(size == 128)
        {
            VSize.y = 0.001f;
        }

        //gameObject.transform.localScale = VSize;
         UpdateText();
    }

    public void UpdateText()
    {
        if(size == 128)
        {
            text.text = "128";
        }
        else
        {
            text.text = size + "x" + size;
        }
    }

    public void Update()
    {
        Vector3 CurScale = gameObject.transform.localScale;
        if ((CurScale - VSize).sqrMagnitude > 0.000001f)
        {
            gameObject.transform.localScale = Vector3.Lerp(CurScale, VSize, Time.deltaTime * Speed);
        }
    }

    private void CalcSize()
    {
        if (!manager) return;

        int numFlats = 0;
        size = 126;

        foreach (CLayer layer in manager.Layers)
        {
            if(layer.layerType == CLayerType.Conv)
            {
                size = size - 2;
            }
            else if (layer.layerType == CLayerType.Dense)
            {
                numFlats++;
            }
            else if (layer.layerType == CLayerType.Pool)
            {
                size /= 2;
            }
        }

        if (numFlats >= 1) size = 128;
    }
}
