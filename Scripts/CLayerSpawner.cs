using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CLayerSpawner : MonoBehaviour {

    public GameObject LayerObj;
    public CLayerType layerType;

    /* Spawn a new layer instance when the user removes one. */
    private void OnTriggerExit(Collider other)
    {
        CLayer layer = other.GetComponent<CLayer>();
        if (layer != null)
        {
            if (LayerObj)
            {
                if (layer.isNew && layer.layerType == this.layerType)
                {
                    Instantiate(LayerObj, this.transform.position, this.transform.rotation);
                }
            }
        }
    }
}
