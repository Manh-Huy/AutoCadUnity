//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class HideOrDisplayFloor : MonoBehaviour
//{
//    public GameObject objectsToHide;

//    private Renderer objectRenderer;
//    private Renderer[] childRenderers;

//    void Start()
//    {
//        objectRenderer = objectsToHide.GetComponent<Renderer>();
//        childRenderers = objectsToHide.GetComponentsInChildren<Renderer>();
//    }

//    public void ToggleStatus()
//    {
//        objectRenderer.enabled = !objectRenderer.enabled;

//        foreach (Renderer childRenderer in childRenderers)
//        {
//            childRenderer.enabled = objectRenderer.enabled;
//        }
//    }
//}
