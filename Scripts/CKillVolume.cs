/* Copyright © 2019, University of Texas Southwestern Medical Center. All rights reserved.
 *  Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
 *  Department: Lyda Hill Department of Bioinformatics 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CKillVolume : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CLayer>())
        {
            Destroy(other.gameObject);
        }
    }
}
