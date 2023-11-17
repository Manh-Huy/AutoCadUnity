using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PropertyRowUI : MonoBehaviour
{
    // floor show, hide
    [SerializeField]
    private TMP_Text _nameFloorText;

    [SerializeField]
    private TMP_Text _buttonShowFloorText;

    [SerializeField]
    private TMP_Text _buttonHideFloorText;

    [SerializeField]
    private Button _showButton;

    [SerializeField]
    private Button _hideButton;

    // stair side
    [SerializeField]
    private TMP_Text _nameFloorAndStairText;

    [SerializeField]
    private TMP_Text _buttonLeftSideStairText;

    [SerializeField]
    private TMP_Text _buttonRightSideStairText;

    [SerializeField]
    private Button _leftSideStairButton;

    [SerializeField]
    private Button _rightSideStairButton;

    private Create3D _create3D;
    private PropertyLoad _propertyLoad;
    private void Start()
    {
        _buttonShowFloorText.text = "Show";
        _buttonHideFloorText.text = "Hide";
        _showButton.interactable = false;
        _hideButton.interactable = true;

        _buttonLeftSideStairText.text = "Left/Front";
        _buttonRightSideStairText.text = "Right/Back";

        _create3D = FindObjectOfType<Create3D>();
        if (_create3D == null)
        {
            Debug.Log("Create3D is NULL");
        }

        _propertyLoad = FindObjectOfType<PropertyLoad>();
        if (_propertyLoad == null)
        {
            Debug.Log("PropertyLoad is NULL");
        }
    }

    public void AssignValuesNameFloor(string nameFloor)
    {
        _nameFloorText.text = nameFloor;
    }

    public void AssignValuesNameFloorAndStair(string nameFloor)
    {
        _nameFloorAndStairText.text = nameFloor;
    }

    public void ClickShow()
    {
        for (int i = 0; i < _create3D._propertyRowList.Count; i++)
        {
            PropertyRow floor = _create3D._propertyRowList[i];

            if (_nameFloorText.text == floor.NameFloor)
            {
                ShowObject(floor.Floor);

                // ẩn / hiện sàn tầng trước đó (ví dụ ẩn/hiện tầng 2 thì ẩn/hiện luôn sàn (top) tầng 1)
                if (i > 0)
                {
                    PropertyRow previousFloor = _create3D._propertyRowList[i - 1];
                    Transform floorTransform = previousFloor.Floor.transform;
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
        for (int i = 0; i < _create3D._propertyRowList.Count; i++)
        {
            PropertyRow floor = _create3D._propertyRowList[i];

            if (_nameFloorText.text == floor.NameFloor)
            {
                HideObject(floor.Floor);

                // ẩn / hiện sàn tầng trước đó (ví dụ ẩn/hiện tầng 2 thì ẩn/hiện luôn sàn (top) tầng 1)
                if (i > 0)
                {
                    PropertyRow previousFloor = _create3D._propertyRowList[i - 1];
                    Transform floorTransform = previousFloor.Floor.transform;
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

    public void ClickLeft()
    {
        string targetStairName = _nameFloorAndStairText.text;
        if (_propertyLoad._stairDictionary.ContainsKey(targetStairName))
        {
            GameObject targetStair = _propertyLoad._stairDictionary[targetStairName];
        }
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
