/* Copyright © 2019, University of Texas Southwestern Medical Center. All rights reserved.
 *  Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
 *  Department: Lyda Hill Department of Bioinformatics 
 *  
 *  Copyright © 2019, University of Texas Southwestern Medical Center. All rights reserved.
Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
Department: Lyda Hill Department of Bioinformatics.
This software and any related documentation constitutes published and/or unpublished works and may contain valuable trade secrets and proprietary information belonging to The University of Texas Southwestern Medical Center (UT SOUTHWESTERN).  None of the foregoing material may be copied, duplicated or disclosed without the express written permission of UT SOUTHWESTERN.  IN NO EVENT SHALL UT SOUTHWESTERN BE LIABLE TO ANY PARTY FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES, INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS DOCUMENTATION, EVEN IF UT SOUTHWESTERN HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.  UT SOUTHWESTERN SPECIFICALLY DISCLAIMS ANY WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE AND ACCOMPANYING DOCUMENTATION, IF ANY, PROVIDED HEREUNDER IS PROVIDED "AS IS". UT SOUTHWESTERN HAS NO OBLIGATION TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
This software contains copyrighted materials from Oculus, Unity Technologies, Keras, TensorFlow, gRPC, NumPy, Matplotlib, OpenCV, Pyprind, Nvidia CUDA, wiki.unity3d.com, PyCharm, Visual Studio Community, and Google. Corresponding terms and conditions apply.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CListableManager : MonoBehaviour {

    List<CListable> Nodes;
    Vector3 Center;
    Vector3 FarLeft;

    CLayerManager layerManager;
    CSizeSphere sizeSphere;

    public float Spacing = 0.2f; // Distance between nodes
    public float Speed = 10f;

    private void Awake()
    {
       Center = transform.position;
        Nodes = new List<CListable>();
        FarLeft = Center;

        layerManager = FindObjectOfType<CLayerManager>();
        sizeSphere = FindObjectOfType<CSizeSphere>();
    }

    public void AddNode(CListable Node)
    {
        //Node.DisableGravity();
        if (Nodes.Count == 0)
        {
            Nodes.Add(Node);
            layerManager.Layers.Add(Node.Layer);
            CenterNodes();
            return;
        }
        
        // Check position of insert:
        float distFromLeft = Vector3.Dot((Node.transform.position - FarLeft), transform.right) // From center
                             + ((Nodes.Count) * Spacing * 0.5f); // Add half of new range.
        int index = (int)(distFromLeft/Spacing);

        if(index <= 0)
        {
            AddNode(Node, 0);
            CenterNodes();
            return;
        }
        else if(index > Nodes.Count)
        {
            Nodes.Add(Node);
            layerManager.Layers.Add(Node.Layer);
            CenterNodes();
            return;
        }

        Debug.LogWarning("Dist from Left = " + distFromLeft + " "+index);
        AddNode(Node, index);
    }

    public void RemoveNode(CListable Node)
    {
        Nodes.Remove(Node);
        layerManager.Layers.Remove(Node.Layer);
        CenterNodes();
    }

    // Maintain the size of the list but reposition elements.
    public void MoveNode(CListable Node)
    {
        // Adjust the position of the left or right element:
        float distFromTarget = Vector3.Dot((Node.transform.position - Node.TargetLoc), transform.right.normalized);
        if(distFromTarget > Spacing * 0.5f)
        {
            SwapNodes(Node.ListableIndex, Node.ListableIndex + 1);
            CenterNodes();
        }
        else if(distFromTarget < -Spacing * 0.5f)
        {
            SwapNodes(Node.ListableIndex, Node.ListableIndex - 1);
            CenterNodes();
        }
    }

    private void SwapNodes(int thisIndex, int desiredIndex)
    {
        if (desiredIndex >= Nodes.Count || desiredIndex < 0) return;
        else
        {
            CListable temp = Nodes[desiredIndex];
            Nodes[desiredIndex] = Nodes[thisIndex];
            Nodes[thisIndex] = temp;

            CLayer temp2 = layerManager.Layers[desiredIndex];
            layerManager.Layers[desiredIndex] = layerManager.Layers[thisIndex];
            layerManager.Layers[thisIndex] = temp2;
        }
    }

    void RemoveNode(int index)
    {
        Nodes.RemoveAt(index);
        layerManager.Layers.RemoveAt(index);
        CenterNodes();
    }

    void AddNode(CListable Node, int index)
    {
        Nodes.Insert(index, Node);
        layerManager.Layers.Insert(index, Node.Layer);

        // Scoot Adjacent Nodes 
        CenterNodes();
    }

    void CenterNodes()
    {
        FarLeft = Center + ((Nodes.Count-1) * Spacing *-0.5f) *transform.right;

        PositionDenseNodes();

        // Equally space objects
        for(int i=0; i < Nodes.Count; i++)
        {
            Nodes[i].ListableIndex = i;
            float offset = (i * Spacing); // Sign & offset of translation from center

            Nodes[i].TargetLoc = FarLeft + transform.right*offset;
        }

        if(sizeSphere) sizeSphere.UpdateSize();
    }

    void PositionDenseNodes()
    {
        List<CListable> temp = new List<CListable>();

        int numRemoved = 0;

        for(int e = 0; e < Nodes.Count; e++)
        {
            if(Nodes[e].type == CLayerType.Dense)
            {
                numRemoved++;
                temp.Add(Nodes[e]);
                Nodes.RemoveAt(e);
                e--;
            }
        }

        foreach (CListable listable in temp)
        {
            Nodes.Add(listable);
        }

        if(numRemoved > 0)
        {
            layerManager.Layers.Clear();
            foreach (CListable node in Nodes)
            {
                layerManager.Layers.Add(node.Layer);
            }
        }
    }
}
