using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class LoadUI : MonoBehaviour
{
    [SerializeField]
    private GameObject _rowShowHidePrefab;

    [SerializeField]
    private GameObject _contentShowHide;

    [SerializeField]
    private GameObject _rowStairSidePrefab;

    [SerializeField]
    private GameObject _contentStairSide;

    [SerializeField]
    private GameObject _dropdownPrefab;

    [SerializeField]
    private GameObject _positionDropdown;

    public Dictionary<string, GameObject> _stairDictionary = new Dictionary<string, GameObject>();

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
            Load();
            _create3D._isCreateDone = false;
        }
    }

    void Load()
    {
        List<string> nameFloorList = new List<string>();
        int elementCount = _create3D._floorDictionary.Count;
        foreach (var floor in _create3D._floorDictionary)
        {
            string nameFloor = $"Floor {floor.Key}";
            if (floor.Key == elementCount)
            {
                nameFloor = "Roof";
            }
            GameObject floorObject = floor.Value;
            // Load hide, show floor
            var row = Instantiate(_rowShowHidePrefab, new Vector3(), Quaternion.identity);
            row.transform.SetParent(_contentShowHide.transform);
            row.GetComponent<ShowHideFloorRow>().AssignValuesNameFloor(nameFloor);

            // Load side of stair
            Transform floorTransform = floorObject.transform;
            Transform stairContainerTransform = floorTransform.Find("Stair Container");

            if (stairContainerTransform != null)
            {
                GameObject stairContainer = stairContainerTransform.gameObject;
                if (stairContainer != null)
                {
                    if (stairContainer.transform.Find("Stair") != null)
                    {
                        int indexStair = 1;
                        foreach (Transform child in stairContainer.transform)
                        {
                            if (child.name == "Stair")
                            {
                                GameObject stair = child.gameObject;
                                string stairName = $"{stair.name} {indexStair} (F{floor.Key})";
                                if (floor.Key == elementCount)
                                {
                                    stairName = $"{stair.name} {indexStair} (Roof)";
                                }
                                _stairDictionary.Add(stairName, stair);

                                var rowStair = Instantiate(_rowStairSidePrefab, new Vector3(), Quaternion.identity);
                                rowStair.transform.SetParent(_contentStairSide.transform);
                                rowStair.GetComponent<StairSideRow>().AssignValuesNameFloorAndStair(stairName);
                            }
                            indexStair++;
                        }
                    }
                }
            }
            nameFloorList.Add(nameFloor);
        }
        var dropdown = Instantiate(_dropdownPrefab, _positionDropdown.transform.position, Quaternion.identity);
        dropdown.transform.SetParent(_positionDropdown.transform);
        dropdown.GetComponent<DropdownHandler>().AssignValuesNameFloor(nameFloorList);
    }
}
