/* Copyright © 2019, University of Texas Southwestern Medical Center. All rights reserved.
 *  Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
 *  Department: Lyda Hill Department of Bioinformatics 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CFlowBar : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        CListable Node = other.GetComponent<CListable>();
        if(Node != null && !Node.IsHeld())
        {
            Node.Delete();
        }
    }
}
