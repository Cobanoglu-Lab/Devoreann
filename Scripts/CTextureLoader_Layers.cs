using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTextureLoader_Layers : MonoBehaviour
{

    const string directory = "/Images";//H:/VR_DevelopmentIDE/VR_DevelopmentIDE/VR_DL/
    string m_Path;
    const string TPrefix = "activation";
    const string APrefix = "all_layer";
    const string InputFile = "input_image";

    CLayerManager LayerManager;

    CLayer[] ActiveLayers;
    List<Texture> Images;

    CInputTexture inputTexture; // TODO

    //bool bIsLookingForTex = true;

    CAllLayerManager activations;

    public Texture2D fillerTex_activations;
    public Texture2D fillerTex_layer;

    private void Awake()
    {
        LayerManager = FindObjectOfType<CLayerManager>();
        inputTexture = FindObjectOfType<CInputTexture>();
        m_Path = Application.dataPath + directory;
        Debug.Log("mPath" + m_Path);

        activations = FindObjectOfType<CAllLayerManager>();
    }

    /* Find and load a local image into the layer array. */
    IEnumerator LoadLocalImage(int imgID, int ContainerID)
    {
        bool bIsLookingForTex = true;
        imgID += 2; // b/c 2 existing in prebuild model
        string img = m_Path + "/" + TPrefix + imgID + ".png";

        while (bIsLookingForTex)
        {
            if (System.IO.File.Exists(img))
            {
                bIsLookingForTex = false;
                //Debug.LogWarning("Found");
                break;
            }
            else
            {
                CLayer activeLayer = LayerManager.GetLayer(ContainerID);
                activeLayer.SetTexture(fillerTex_layer);
                yield break; // Stop the coroutine
                //Debug.LogWarning("Waiting");
            }
            yield return new WaitForSeconds(0.2f);
        }
        bIsLookingForTex = true;

        while (bIsLookingForTex)
        {
            WWW www = new WWW(img);
            while (!www.isDone)
                yield return null;
            if (www.texture)
            {
                CLayer activeLayer = LayerManager.GetLayer(ContainerID);
                Texture2D newTex = www.texture;
                newTex.wrapMode = TextureWrapMode.Clamp;
                newTex.filterMode = FilterMode.Point;
                activeLayer.SetTexture(newTex);
                bIsLookingForTex = false;
            }
            else { Debug.LogError("ERROR: Image not found #" + imgID); }
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator LoadActivation(int imgID)
    {
        string img = m_Path + "/" + APrefix + imgID + ".png";
        bool bIsLookingForTex = true;

        while (bIsLookingForTex)
        {
            if (System.IO.File.Exists(img))
            {
                bIsLookingForTex = false;
                break;
            }
            else
            {
                if (activations) activations.Add(fillerTex_activations);
                yield break; // Stop the coroutine
            }
            yield return new WaitForSeconds(0.2f);
        }
        bIsLookingForTex = true;

        while (bIsLookingForTex)
        {
            WWW www = new WWW(img);
            while (!www.isDone)
                yield return null;
            if (www.texture)
            {
                Texture2D newTex = www.texture;
                newTex.wrapMode = TextureWrapMode.Clamp;
                newTex.filterMode = FilterMode.Point;

                if (activations) activations.Add(newTex);

                bIsLookingForTex = false;
            }
            else { Debug.LogError("ERROR: Image not found #" + imgID); }
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator LoadAllActivations(List<int> activationIndices)
    {
        foreach (int index in activationIndices)
        {
            yield return StartCoroutine(LoadActivation(index)); // Waits for completion.
        }
    }

    void Start()
    {
        if (Application.isEditor)
        {
            Debug.LogError("Running in Editor Mode.");
            m_Path = "C:/Users/VR_Demo/Desktop/VR_DevelopmentIDE/VR_DevelopmentIDE/VR_DL/Package/Images";//"H:/VR_DevelopmentIDE/VR_DevelopmentIDE/VR_DL/Pathology/Images";
        }
        //CUIManager UI = FindObjectOfType<CUIManager>();
        //UI.UpdateProgressText(m_Path + "/" + InputFile + ".png");

        //StartCoroutine(LoadInputImage());
    }

    public void LoadAllLayerActivations()
    {
        List<int> activationIndices = new List<int>();

        if (activations) activations.Clear();

        if (!LayerManager) return;
        if (LayerManager.Layers.Count == 0)
        {
            Debug.LogError("Error: No active layers found. ");
        }

        //StartCoroutine(LoadInputImage());

        // Load images into layer array:
        int imgIndex = 0;
        for (int i = 0; i < LayerManager.Layers.Count; i++)
        {
            StartCoroutine(LoadLocalImage(imgIndex, i));

            LayerManager.Layers[i].bakedIndex = imgIndex;

            //StartCoroutine(LoadActivationToList(imgIndex)); // Add layer activation for each layer
            activationIndices.Add(imgIndex);


            // Skip auto-generated activation layers from Conv/Dense layers
            CLayer CurLayer = LayerManager.Layers[i];
            if (CurLayer.layerType == CLayerType.Conv || CurLayer.layerType == CLayerType.Dense)
            {
                LayerManager.Layers[i].actIndex = imgIndex + 1;
                //StartCoroutine(LoadActivationToList(imgIndex + 1)); // Add corresponding activation
                activationIndices.Add(imgIndex + 1);
                imgIndex += 2; // Skip to the next image
            }
            else
            {
                imgIndex++;
            }
        }

        StartCoroutine(LoadAllActivations(activationIndices));

        if (activations) activations.SetVisibility(true); // Always activations after the first time
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
