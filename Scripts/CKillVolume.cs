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
