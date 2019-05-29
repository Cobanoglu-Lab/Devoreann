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
