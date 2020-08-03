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

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(OVRGrabbable))]

public class CListable : MonoBehaviour {
    public new Rigidbody rigidbody;
    public OVRGrabbable grabbable;

    public CLayer Layer;

    CListableManager listableManager;

    [HideInInspector]
    public CLayerType type;

    [HideInInspector]
    public int ListableIndex;

    [HideInInspector]
    public bool bActiveListable = false;

    //public float Speed = 10;
    float Speed;

    [HideInInspector]
    public Vector3 TargetLoc;

    public void DisableGravity()
    {
        rigidbody.isKinematic = false;
        rigidbody.useGravity = false;
    }

    private void Awake()
    {
        Layer = GetComponent<CLayer>();

        listableManager = FindObjectOfType<CListableManager>();
        grabbable = GetComponent<OVRGrabbable>();
        rigidbody = GetComponent<Rigidbody>();

        if(listableManager) Speed = listableManager.Speed;

        TargetLoc = transform.position;
        bActiveListable = false;
    }

    private void Start()
    {
        type = Layer.layerType;
    }

    private void Update()
    {
        Vector3 position = transform.position;

        if (bActiveListable && !grabbable.isGrabbed && (TargetLoc - position).sqrMagnitude > 0.000001f)
        {
            rigidbody.position = Vector3.Lerp(position, TargetLoc, Time.deltaTime * Speed);
            rigidbody.rotation = listableManager.transform.rotation*Quaternion.Euler(-90, 0, 180);
        }   
    }

    private void OnTriggerEnter(Collider other)
    {
        CListableManager manager = other.GetComponent<CListableManager>();

        if (manager)
        {
            rigidbody.isKinematic = true;
            bActiveListable = true;
            manager.AddNode(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CListableManager manager = other.GetComponent<CListableManager>();

        if (manager)
        {
            bActiveListable = false;
            manager.RemoveNode(this);
        }
    }

    public bool IsHeld()
    {
        return grabbable.isGrabbed;
    }

    public void Delete()
    {
        if (listableManager)
        {
            bActiveListable = false;
            listableManager.RemoveNode(this);
        }
        Destroy(this.gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponent<CListableManager>() != null)
        {
            if (grabbable && grabbable.isGrabbed)
            {
                listableManager.MoveNode(this);
            }
            else
            {
                rigidbody.isKinematic = true;
            }
        }
    }
}

