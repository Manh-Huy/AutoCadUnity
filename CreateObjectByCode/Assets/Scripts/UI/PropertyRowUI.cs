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

    private PropertyRow _propertyRow;
    private void Start()
    {
        _buttonHideDisplayFloorText.text = "Hide";

        _propertyRow = FindObjectOfType<PropertyRow>();
        if (_propertyRow == null)
        {
            Debug.Log("PropertyRow is NULL");
        }
    }

    public void AssignValues(string nameFloor)
    {
        _nameFloorText.text = nameFloor;
    }

    public void ClickHideOrDisplay()
    {
        if (_buttonHideDisplayFloorText.text == "Hide")
        {
            _buttonHideDisplayFloorText.text = "Display";
        }
        else
        {
            _buttonHideDisplayFloorText.text = "Hide";
        }
        _propertyRow.ToggleStatus();
    }
}
