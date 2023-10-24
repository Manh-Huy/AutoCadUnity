using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonClickHandler : MonoBehaviour
{
    [SerializeField]
    private InputField inputFieldScaleX;
    [SerializeField]
    private InputField inputFieldScaleY;
    [SerializeField]
    private InputField inputFieldScaleZ;
    //private Text resultText;

    public float scaleX;
    public float scaleY;
    public float scaleZ;

    private int checkData;

    public bool isCreatTank = false;
    public void TakeScale()
    {
        string inputTextScaleX = inputFieldScaleX.text;
        string inputTextScaleY = inputFieldScaleY.text;
        string inputTextScaleZ = inputFieldScaleZ.text;

        checkData = 0;

        // Kiểm tra xem dữ liệu nhập vào có phải là số không
        if (float.TryParse(inputTextScaleX, out float numberX))
        {
            scaleX = numberX;
        }
        else
        {
            checkData++;
        }

        if (float.TryParse(inputTextScaleY, out float numberY))
        {
            scaleY = numberY;
        }
        else
        {
            checkData++;
        }

        if (float.TryParse(inputTextScaleZ, out float numberZ))
        {
            scaleZ = numberZ;
        }
        else
        {
            checkData++;
        }

        if (checkData == 0)
        {
            isCreatTank = true;
        }
        else
        {
            Debug.Log("Du lieu khong hop le!");
        }
    }

    public void DeleteTankIfExist()
    {
        GameObject tank = GameObject.Find("Tank");

        if (tank != null)
        {
            // Xóa đối tượng "Tank" và tất cả các con của nó
            Destroy(tank);
        }
        else
        {
            // Nếu không tìm thấy đối tượng "Tank," thông báo trong debug.log
            Debug.Log("Khong co doi tuong de xoa");
        }
    }

    public void DontCreateTank()
    {
        isCreatTank = false;
    }
}
