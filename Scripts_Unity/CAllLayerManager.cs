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

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAllLayerManager : MonoBehaviour {

    private Material material;
    public Texture texture;

    new Renderer renderer;
    Material mActive;
    List<Material> mList = new List<Material>(); // Layer contains multiple materials

    List<Texture2D> activations = new List<Texture2D>();

    CLayerManager layerManager;

    CLayer activeLayer;

    int activeIndex = -1;

    public bool bUseNormal = true;
    bool bButtonPressed = false;
    bool bLastFrame = false;

    private void Awake()
    {
        layerManager = FindObjectOfType<CLayerManager>();
        renderer = GetComponent<Renderer>();
    }

    public void SetTexture(Texture2D newTexture)
    {
        if (!renderer) return;
        renderer.GetMaterials(mList);
        if (mList.Count == 1)
        {
            mActive = mList[0];
            mActive.mainTexture = newTexture;
        }
    }

    public void Add(Texture2D texture)
    {
        activations.Add(texture);
        SetTexture(texture);
    }

    public void Clear()
    {
        activations.Clear();
    }

    public void SetLayer(CLayer layer)
    {
        activeLayer = layer;
        activeIndex = layer.GetIndex();
        if(activeIndex >=0 && activeIndex < activations.Count)
        {
            SetTexture(activations[activeIndex]);
        }
    }

    public void Refresh()
    {
        if (!activeLayer) return;

        activeIndex = activeLayer.GetIndex();
        if (activeIndex >= 0 && activeIndex < activations.Count)
        {
            SetTexture(activations[activeIndex]);
        }
    }

    public void Update()
    {
        // If either is pressed:
        bButtonPressed = (OVRInput.Get(OVRInput.Button.Two) || OVRInput.Get(OVRInput.Button.Four));

        // Toggle:
        if(bButtonPressed != bLastFrame)
        {
            bUseNormal = !bUseNormal;
            Refresh();
        }

        bLastFrame = bButtonPressed;
    }

    public void SetVisibility(bool bIsVisible)
    {
        renderer.enabled = bIsVisible;
    }
}
