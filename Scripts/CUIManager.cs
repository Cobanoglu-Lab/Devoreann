/* Copyright © 2019, University of Texas Southwestern Medical Center. All rights reserved.
 *  Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
 *  Department: Lyda Hill Department of Bioinformatics 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CUIManager : MonoBehaviour {

    // Progress of the computation.
    public GameObject progressTextObj;


    TextMesh progressText;


    public void Awake()
    {
        progressText = progressTextObj.GetComponent<TextMesh>();
        if (!progressText) { Debug.LogError("Error: No Progress Text Object."); }
    }

    /* Update the main display text. 
	 * @text the text to display. 
	 */
    public void UpdateProgressText(string text)
    {
        if (progressText)
        {
            progressText.text = text;
        }
    }
}
