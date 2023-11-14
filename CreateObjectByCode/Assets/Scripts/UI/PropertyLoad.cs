using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropertyLoad : MonoBehaviour
{
    [SerializeField]
    private GameObject _rowPrefab;

    [SerializeField]
    private GameObject _content;

    private Create3D _create3D;


    // Start is called before the first frame update
    void Start()
    {
        _create3D = FindObjectOfType<Create3D>();

        if (_create3D == null)
        {
            Debug.Log("The Create3D is NULL");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_create3D._isCreateDone == true)
        {
            LoadProperty();
            _create3D._isCreateDone = false;
        }
    }

    void LoadProperty()
    {
        foreach (PropertyRow floor in _create3D._propertyRowList)
        {
            var row = Instantiate(_rowPrefab, new Vector3(), Quaternion.identity);
            row.transform.SetParent(_content.transform);
            row.GetComponent<PropertyRowUI>().AssignValues(floor.NameFloor);
        }
    }
}
