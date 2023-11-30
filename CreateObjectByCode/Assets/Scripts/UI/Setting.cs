using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Setting : MonoBehaviour
{
    [SerializeField]
    Button _propertyButton;
    [SerializeField]
    Button _floorMovementButton;
    [SerializeField]
    GameObject _propertyUI;
    [SerializeField]
    GameObject _floorMovementUI;

    public bool _isClickPropertyButton = false;
    public bool _isClickFloorMovementButton = false;

    private bool _isButtonsHidden = false;

    void Start()
    {
        _propertyButton.onClick.AddListener(ClickPropertyButton);
        _floorMovementButton.onClick.AddListener(ClickFloorMovementButton);
    }

    public void ClickSettingButton()
    {
        _isButtonsHidden = !_isButtonsHidden;

        _propertyButton.gameObject.SetActive(_isButtonsHidden);
        _floorMovementButton.gameObject.SetActive(_isButtonsHidden);

        if (_isButtonsHidden == false)
        {
            _propertyUI.SetActive(false);
            _floorMovementUI.SetActive(false);
            _isClickPropertyButton = false;
            _isClickFloorMovementButton = false;
        }
    }

    public void ClickPropertyButton()
    {
        _isClickPropertyButton = !_isClickPropertyButton;

        if (_isClickPropertyButton == true)
        {
            _propertyUI.SetActive(true);
        }
        else
        {
            _propertyUI.SetActive(false);
        }
    }

    public void ClickFloorMovementButton()
    {
        _isClickFloorMovementButton = !_isClickFloorMovementButton;

        if (_isClickFloorMovementButton == true)
        {
            _floorMovementUI.SetActive(true);
        }
        else
        {
            _floorMovementUI.SetActive(false);
        }
    }
}
