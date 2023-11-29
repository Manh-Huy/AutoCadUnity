using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class FloorMovementLoad : MonoBehaviour
{
    private DropdownHandler _dropdownHandler;
    private Create3D _create3D;

    // Start is called before the first frame update
    void Start()
    {
        _create3D = FindObjectOfType<Create3D>();
        _dropdownHandler = FindObjectOfType<DropdownHandler>();

        if (_create3D == null)
        {
            Debug.Log("The Create3D is NULL");
        }

        if (_dropdownHandler == null)
        {
            Debug.Log("The DropdownHandler is NULL");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_create3D._isCreateDone == true)
        {
            LoadFloorMovement();
            _create3D._isCreateDone = false;
        }
    }

    void LoadFloorMovement()
    {
        List<string> nameFloorList = new List<string>();
        foreach (PropertyRow floor in _create3D._propertyRowList)
        {
            nameFloorList.Add(floor.NameFloor);
        }
        _dropdownHandler.AssignValuesNameFloor(nameFloorList);
    }
}
