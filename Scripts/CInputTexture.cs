/* Copyright © 2019, University of Texas Southwestern Medical Center. All rights reserved.
 *  Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
 *  Department: Lyda Hill Department of Bioinformatics 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ! DEPRECATED
public class CInputTexture : MonoBehaviour {

    new Renderer renderer;
    List<Material> mList = new List<Material>(); // Layer contains multiple materials
    Material mActive;

    public int index = 0;


    private void Awake()
    {
        // Handle Textures:
        renderer = GetComponent<Renderer>();
    }

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
}
