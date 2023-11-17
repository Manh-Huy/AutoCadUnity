using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StairSideRow : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _nameFloorAndStairText;

    [SerializeField]
    private Button _leftFrontSideStairButton;

    [SerializeField]
    private Button _rightBackSideStairButton;

    private PropertyLoad _propertyLoad;
    private void Start()
    {
        _leftFrontSideStairButton.interactable = false;
        _rightBackSideStairButton.interactable = true;

        _propertyLoad = FindObjectOfType<PropertyLoad>();
        if (_propertyLoad == null)
        {
            Debug.Log("PropertyLoad is NULL");
        }
    }

    public void AssignValuesNameFloorAndStair(string nameFloor)
    {
        _nameFloorAndStairText.text = nameFloor;
    }

    public void ClickLeftOrFront()
    {
        string targetStairName = _nameFloorAndStairText.text;
        if (_propertyLoad._stairDictionary.ContainsKey(targetStairName))
        {
            GameObject targetStair = _propertyLoad._stairDictionary[targetStairName];

            Quaternion stairRotation = targetStair.transform.rotation;

            if (Mathf.Approximately(stairRotation.eulerAngles.y, 270f))
            {
                targetStair.transform.rotation = Quaternion.Euler(stairRotation.eulerAngles.x, 90f, stairRotation.eulerAngles.z);
            }
            else if (Mathf.Approximately(stairRotation.eulerAngles.y, 0f))
            {
                targetStair.transform.rotation = Quaternion.Euler(stairRotation.eulerAngles.x, 180f, stairRotation.eulerAngles.z);
            }
        }

        _leftFrontSideStairButton.interactable = false;
        _rightBackSideStairButton.interactable = true;
    }

    public void ClickRightOrBack()
    {
        string targetStairName = _nameFloorAndStairText.text;
        if (_propertyLoad._stairDictionary.ContainsKey(targetStairName))
        {
            GameObject targetStair = _propertyLoad._stairDictionary[targetStairName];

            Quaternion stairRotation = targetStair.transform.rotation;

            if (Mathf.Approximately(stairRotation.eulerAngles.y, 90f))
            {
                targetStair.transform.rotation = Quaternion.Euler(stairRotation.eulerAngles.x, 270f, stairRotation.eulerAngles.z);
            }
            else if (Mathf.Approximately(stairRotation.eulerAngles.y, 180f))
            {
                targetStair.transform.rotation = Quaternion.Euler(stairRotation.eulerAngles.x, 0f, stairRotation.eulerAngles.z);
            }
        }

        _leftFrontSideStairButton.interactable = true;
        _rightBackSideStairButton.interactable = false;
    }
}
