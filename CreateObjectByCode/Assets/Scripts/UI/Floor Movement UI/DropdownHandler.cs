﻿using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropdownHandler : MonoBehaviour
{
    public Text text;
    [SerializeField]
    private Button _leftButton;
    [SerializeField]
    private Button _rightButton;
    [SerializeField]
    private Button _frontButton;
    [SerializeField]
    private Button _backButton;

    private Create3D _create3D;
    private SliderHandler _sliderHandler;

    public string _nameFloorIndex;

    private bool _isMovingLeft = false;
    private bool _isMovingRight = false;

    private bool _isMovingForward = false;
    private bool _isMovingBackward = false;

    void Start()
    {
        _create3D = FindObjectOfType<Create3D>();
        _sliderHandler = FindObjectOfType<SliderHandler>();

        if (_create3D == null)
        {
            Debug.Log("The Create3D is NULL");
        }

        if (_sliderHandler == null)
        {
            Debug.Log("The SliderHandler is NULL");
        }

        // Add EventTriggers directly to the buttons
        AddEventTrigger(_leftButton, () => _isMovingLeft = true, () => _isMovingLeft = false);
        AddEventTrigger(_rightButton, () => _isMovingRight = true, () => _isMovingRight = false);
        AddEventTrigger(_frontButton, () => _isMovingForward = true, () => _isMovingForward = false);
        AddEventTrigger(_backButton, () => _isMovingBackward = true, () => _isMovingBackward = false);
    }

    void Update()
    {
        float _speed = _sliderHandler.MovementSpeed();
        GameObject floor;
        if (_isMovingLeft || Input.GetKey(KeyCode.Alpha4) || Input.GetKey(KeyCode.Keypad4))
        {
            floor = TakeFloorFromNameFloor(_nameFloorIndex);
            // Di chuyển sang trái
            floor.transform.Translate(Vector3.left * _speed * Time.deltaTime);
        }

        if (_isMovingRight || Input.GetKey(KeyCode.Alpha6) || Input.GetKey(KeyCode.Keypad6))
        {
            floor = TakeFloorFromNameFloor(_nameFloorIndex);
            // Di chuyển sang phải
            floor.transform.Translate(Vector3.right * _speed * Time.deltaTime);
        }

        if (_isMovingForward || Input.GetKey(KeyCode.Alpha8) || Input.GetKey(KeyCode.Keypad8))
        {
            floor = TakeFloorFromNameFloor(_nameFloorIndex);
            // Di chuyển về phía trước
            // nếu là roof vì trục y là z và z là y nên lên trước là Vector3.down
            if (_nameFloorIndex == "Roof")
            {
                floor.transform.Translate(Vector3.down * _speed * Time.deltaTime);
            }
            else
            {
                floor.transform.Translate(Vector3.forward * _speed * Time.deltaTime);
            }
        }

        if (_isMovingBackward || Input.GetKey(KeyCode.Alpha5) || Input.GetKey(KeyCode.Keypad5))
        {
            floor = TakeFloorFromNameFloor(_nameFloorIndex);
            // Di chuyển về phía sau
            // nếu là roof vì trục y là z và z là y nên lên trước là Vector3.down
            if (_nameFloorIndex == "Roof")
            {
                floor.transform.Translate(Vector3.up * _speed * Time.deltaTime);
            }
            else
            {
                floor.transform.Translate(Vector3.back * _speed * Time.deltaTime);
            }
        }
    }
    void AddEventTrigger(Button button, UnityEngine.Events.UnityAction downAction, UnityEngine.Events.UnityAction upAction)
    {
        EventTrigger eventTrigger = button.gameObject.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = button.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry entryDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        entryDown.callback.AddListener((eventData) => downAction.Invoke());
        eventTrigger.triggers.Add(entryDown);

        EventTrigger.Entry entryUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        entryUp.callback.AddListener((eventData) => upAction.Invoke());
        eventTrigger.triggers.Add(entryUp);
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
    }

    void DropdownItemSelected(Dropdown dropdown)
    {
        int index = dropdown.value;

        _nameFloorIndex = dropdown.options[index].text;
        text.text = _nameFloorIndex;
    }

    GameObject TakeFloorFromNameFloor(string nameFloorIndex)
    {
        int elementCount = _create3D._floorDictionary.Count;
        foreach (var floor in _create3D._floorDictionary)
        {
            string nameFloor = $"Floor {floor.Key}";
            if (floor.Key == elementCount)
            {
                nameFloor = "Roof";
            }
            if (nameFloor == nameFloorIndex)
            {
                return floor.Value;
            }
        }
        return null;
    }
}
