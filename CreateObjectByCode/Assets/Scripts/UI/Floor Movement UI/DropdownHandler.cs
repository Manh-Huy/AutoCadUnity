using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownHandler : MonoBehaviour
{
    public Text text;
    [SerializeField]
    private float _speed = 4.5f;
    [SerializeField]
    private Button _leftButton;
    [SerializeField]
    private Button _rightButton;
    [SerializeField]
    private Button _frontButton;
    [SerializeField]
    private Button _backButton;

    private Create3D _create3D;

    private bool _isLoad = false;

    private List<PropertyRow> _propertyRows;

    string _nameFloorIndex;

    void Start()
    {
        _create3D = FindObjectOfType<Create3D>();
        if (_create3D == null)
        {
            Debug.Log("The Create3D is NULL");
        }

        
    }

    public void AssignValuesNameFloor(List<string> nameFloorList)
    {
        var dropdown = transform.GetComponent<Dropdown>();
        dropdown.options.Clear();

        foreach (var floor in nameFloorList)
        {
            dropdown.options.Add(new Dropdown.OptionData() { text = floor });
        }
        DropdownItemSelected(dropdown);
        dropdown.onValueChanged.AddListener(delegate { DropdownItemSelected(dropdown); });
        _isLoad = true;
    }

    void DropdownItemSelected(Dropdown dropdown)
    {
        int index = dropdown.value;

        _nameFloorIndex = dropdown.options[index].text;
        text.text = _nameFloorIndex;

        if (_isLoad == true)
        {
            foreach (PropertyRow floor in _create3D._propertyRowList)
            {
                if (floor.NameFloor == _nameFloorIndex)
                {
                    Debug.Log(floor.NameFloor);
                    GameObject floorObject = floor.Floor;

                    if (Input.GetKeyDown(KeyCode.Alpha4))
                    {
                        Debug.Log("Floor ii move left");
                        MoveLeft(floorObject);
                    }

                    if (Input.GetKeyDown(KeyCode.Alpha6))
                    {
                        MoveRight(floorObject);
                    }

                    if (Input.GetKeyDown(KeyCode.Alpha8))
                    {
                        MoveFront(floorObject);
                    }

                    if (Input.GetKeyDown(KeyCode.Alpha5))
                    {
                        MoveBack(floorObject);
                    }

                    // Gán sự kiện click cho các nút
                    _leftButton.onClick.AddListener(() => MoveLeft(floorObject));
                    _rightButton.onClick.AddListener(() => MoveRight(floorObject));
                    _frontButton.onClick.AddListener(() => MoveFront(floorObject));
                    _backButton.onClick.AddListener(() => MoveBack(floorObject));
                }
            }
        }
    }

    void MoveLeft(GameObject objectToMove)
    {
        objectToMove.transform.Translate(Vector3.left * _speed * Time.deltaTime);
    }
    void MoveRight(GameObject objectToMove)
    {
        objectToMove.transform.Translate(Vector3.right * _speed * Time.deltaTime);
    }
    void MoveFront(GameObject objectToMove)
    {
        objectToMove.transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }
    void MoveBack(GameObject objectToMove)
    {
        objectToMove.transform.Translate(Vector3.back * _speed * Time.deltaTime);
    }

    private void Update()
    {
        if (_isLoad == true)
        {
            foreach (PropertyRow floor in _create3D._propertyRowList)
            {
                if (floor.NameFloor == _nameFloorIndex)
                {
                    Debug.Log(floor.NameFloor);
                    GameObject floorObject = floor.Floor;

                    if (Input.GetKeyDown(KeyCode.Keypad4))
                    {
                        Debug.Log("Floor ii move left");
                        MoveLeft(floorObject);
                    }

                    if (Input.GetKeyDown(KeyCode.Keypad6))
                    {
                        MoveRight(floorObject);
                    }

                    if (Input.GetKeyDown(KeyCode.Keypad8))
                    {
                        MoveFront(floorObject);
                    }

                    if (Input.GetKeyDown(KeyCode.Keypad5))
                    {
                        MoveBack(floorObject);
                    }

                    // Gán sự kiện click cho các nút
                    _leftButton.onClick.AddListener(() => MoveLeft(floorObject));
                    _rightButton.onClick.AddListener(() => MoveRight(floorObject));
                    _frontButton.onClick.AddListener(() => MoveFront(floorObject));
                    _backButton.onClick.AddListener(() => MoveBack(floorObject));
                }
            }
        }
    }
}
