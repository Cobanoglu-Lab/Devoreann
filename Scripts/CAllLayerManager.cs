/* Copyright © 2019, University of Texas Southwestern Medical Center. All rights reserved.
 *  Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
 *  Department: Lyda Hill Department of Bioinformatics 
 */

using System.Collections;
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
