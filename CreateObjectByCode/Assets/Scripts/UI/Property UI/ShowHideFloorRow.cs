using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowHideFloorRow : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _nameFloorText;

    [SerializeField]
    private Button _showButton;

    [SerializeField]
    private Button _hideButton;

    private Create3D _create3D;
    private void Start()
    {
        _showButton.interactable = false;
        _hideButton.interactable = true;

        _create3D = FindObjectOfType<Create3D>();
        if (_create3D == null)
        {
            Debug.Log("Create3D is NULL");
        }
    }

    public void AssignValuesNameFloor(string nameFloor)
    {
        _nameFloorText.text = nameFloor;
    }

    public void ClickShow()
    {
        int elementCount = _create3D._floorDictionary.Count;
        var orderedDictionary = _create3D._floorDictionary.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
        for (int i = 0; i < orderedDictionary.Count; i++)
        {
            int floorIndex = orderedDictionary.Keys.ElementAt(i);
            string nameFloor = $"Floor {floorIndex}";
            if (floorIndex == elementCount)
            {
                nameFloor = "Roof";
            }
            GameObject floor = orderedDictionary[floorIndex];

            if (_nameFloorText.text == nameFloor)
            {
                ShowObject(floor);

                // ẩn / hiện sàn tầng trước đó (ví dụ ẩn/hiện tầng 2 thì ẩn/hiện luôn sàn (top) tầng 1)
                if (floorIndex > 1)
                {
                    int previousFloorIndex = orderedDictionary.Keys.ElementAt(i - 1);
                    string previousNameFloor = $"Floor {previousFloorIndex}";
                    //if (previousFloorIndex == elementCount)
                    //{
                    //    previousNameFloor = "Roof";
                    //}
                    GameObject previousFloor = orderedDictionary[previousFloorIndex];

                    Transform floorTransform = previousFloor.transform;
                    Transform bottomPlaneTransform = floorTransform.Find("Top Plane Container");
                    if (bottomPlaneTransform != null)
                    {
                        GameObject bottomPlaneObject = bottomPlaneTransform.gameObject;
                        ShowObject(bottomPlaneObject);
                    }
                    else
                    {
                        Debug.LogError("Không tìm thấy sàn trên tầng trước!");
                    }
                }
                break;
            }
        }
        _showButton.interactable = false;
        _hideButton.interactable = true;
    }

    public void ClickHide()
    {
        int elementCount = _create3D._floorDictionary.Count;
        var orderedDictionary = _create3D._floorDictionary.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value);

        for (int i = 0; i < orderedDictionary.Count; i++)
        {
            int floorIndex = orderedDictionary.Keys.ElementAt(i);
            string nameFloor = $"Floor {floorIndex}";

            if (floorIndex == elementCount)
            {
                nameFloor = "Roof";
            }
            GameObject floor = orderedDictionary[floorIndex];

            if (_nameFloorText.text == nameFloor)
            {
                HideObject(floor);

                // ẩn / hiện sàn tầng trước đó (ví dụ ẩn/hiện tầng 2 thì ẩn/hiện luôn sàn (top) tầng 1)
                if (floorIndex > 1)
                {
                    int previousFloorIndex = orderedDictionary.Keys.ElementAt(i - 1);
                    //string previousNameFloor = $"Floor {previousFloorIndex}";
                    //if (previousFloorIndex == elementCount)
                    //{
                    //    previousNameFloor = "Roof";
                    //}
                    GameObject previousFloor = orderedDictionary[previousFloorIndex];

                    Transform floorTransform = previousFloor.transform;
                    Transform bottomPlaneTransform = floorTransform.Find("Top Plane Container");
                    if (bottomPlaneTransform != null)
                    {
                        GameObject bottomPlaneObject = bottomPlaneTransform.gameObject;
                        HideObject(bottomPlaneObject);
                    }
                    else
                    {
                        Debug.LogError("Không tìm thấy sàn trên tầng trước!");
                    }
                }
                break;
            }
        }
        _showButton.interactable = true;
        _hideButton.interactable = false;
    }

    void ShowObject(GameObject obj)
    {
        Renderer objectRenderer = obj.GetComponent<Renderer>();
        Renderer[] childRenderers = obj.GetComponentsInChildren<Renderer>();

        objectRenderer.enabled = true;

        foreach (Renderer childRenderer in childRenderers)
        {
            childRenderer.enabled = true;
        }
    }

    void HideObject(GameObject obj)
    {
        Renderer objectRenderer = obj.GetComponent<Renderer>();
        Renderer[] childRenderers = obj.GetComponentsInChildren<Renderer>();

        objectRenderer.enabled = false;

        foreach (Renderer childRenderer in childRenderers)
        {
            childRenderer.enabled = false;
        }
    }
}
