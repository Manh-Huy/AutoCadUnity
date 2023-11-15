using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PropertyRowUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _nameFloorText;

    [SerializeField]
    private TMP_Text _buttonHideDisplayFloorText;

    private Create3D _create3D;
    private void Start()
    {
        _buttonHideDisplayFloorText.text = "Hide";

        _create3D = FindObjectOfType<Create3D>();
        if (_create3D == null)
        {
            Debug.Log("Create3D is NULL");
        }
    }

    public void AssignValues(string nameFloor)
    {
        _nameFloorText.text = nameFloor;
    }

    public void ClickHideOrDisplay()
    {
        foreach (PropertyRow floor in _create3D._propertyRowList)
        {
            if (_nameFloorText.text == floor.NameFloor)
            {
                ToggleStatus(floor.Floor);
                break;
            }
        }
        
        if (_buttonHideDisplayFloorText.text == "Hide")
        {
            _buttonHideDisplayFloorText.text = "Display";
        }
        else
        {
            _buttonHideDisplayFloorText.text = "Hide";
        }
    }

    void ToggleStatus(GameObject floor)
    {
        Renderer objectRenderer = floor.GetComponent<Renderer>();
        Renderer[] childRenderers = floor.GetComponentsInChildren<Renderer>();

        objectRenderer.enabled = !objectRenderer.enabled;

        foreach (Renderer childRenderer in childRenderers)
        {
            childRenderer.enabled = objectRenderer.enabled;
        }
    }
}
