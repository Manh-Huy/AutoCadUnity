using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CreateObject : MonoBehaviour
{
    private ButtonClickHandler buttonClickHandler;
    // Start is called before the first frame update
    void Start()
    {
        buttonClickHandler = GameObject.Find("Canvas").GetComponent<ButtonClickHandler>();

        if (buttonClickHandler == null )
        {
            Debug.Log("ButtonClickHandler is NULL");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (buttonClickHandler.isCreatTank == true)
        {
            float rectangleScaleX = buttonClickHandler.scaleX;
            float rectangleScaleY = buttonClickHandler.scaleY;
            float rectangleScaleZ = buttonClickHandler.scaleZ;

            // Tạo đối tượng hình chữ nhật
            GameObject rectangle = GameObject.CreatePrimitive(PrimitiveType.Cube);

            // Đặt vị trí của hình chữ nhật
            rectangle.transform.position = new Vector3(0f, 0f, 0f);
            rectangle.transform.localScale = new Vector3(rectangleScaleX, rectangleScaleY, rectangleScaleZ); // (2f, 0.5f, 1f)

            // Tạo đối tượng hình vuông
            GameObject square = GameObject.CreatePrimitive(PrimitiveType.Cube);

            square.transform.localScale = new Vector3(rectangleScaleX / 2f, rectangleScaleY * 2f, rectangleScaleZ);

            Vector3 positionSquare = new Vector3();
            positionSquare.x = square.transform.localScale.x / 2f;
            positionSquare.y = rectangle.transform.localScale.y / 2f + square.transform.localScale.y / 2f;
            positionSquare.z = 0f;

            square.transform.position = rectangle.transform.position + positionSquare;

            // Tạo đối tượng hình chữ nhật 2
            GameObject rectangle2 = GameObject.CreatePrimitive(PrimitiveType.Cube);

            rectangle2.transform.localScale = new Vector3(rectangleScaleX / 2f, rectangleScaleY, rectangleScaleZ);

            Vector3 positionrectangle2 = new Vector3();
            positionrectangle2.x = -rectangle2.transform.localScale.x / 2f;
            positionrectangle2.y = rectangle.transform.localScale.y / 2f + rectangle2.transform.localScale.y / 2f;
            positionrectangle2.z = 0f;

            rectangle2.transform.position = rectangle.transform.position + positionrectangle2;

            // Tạo 4 đối tượng hình trụ
            GameObject cylinder1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            GameObject cylinder2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            GameObject cylinder3 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            GameObject cylinder4 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

            Vector3 localScaleCylinder = new Vector3(rectangleScaleX / 2f, rectangleScaleY * 0.4f, rectangleScaleZ);
            cylinder1.transform.localScale = localScaleCylinder;
            cylinder2.transform.localScale = localScaleCylinder;
            cylinder3.transform.localScale = localScaleCylinder;
            cylinder4.transform.localScale = localScaleCylinder;


            Quaternion rotationCylinder = Quaternion.Euler(new Vector3(90f, 0f, 0f));
            cylinder1.transform.rotation = rotationCylinder;
            cylinder2.transform.rotation = rotationCylinder;
            cylinder3.transform.rotation = rotationCylinder;
            cylinder4.transform.rotation = rotationCylinder;

            Vector3 positionCylinder1 = new Vector3();
            positionCylinder1.x = rectangle.transform.localScale.x / 2f;
            positionCylinder1.y = 0f;
            positionCylinder1.z = rectangle.transform.localScale.z / 2f + cylinder1.transform.localScale.y; // chia 2 ra kq sai??????
            cylinder1.transform.position = rectangle.transform.position + positionCylinder1;

            Vector3 positionCylinder2 = new Vector3();
            positionCylinder2.x = -rectangle.transform.localScale.x / 2f;
            positionCylinder2.y = 0f;
            positionCylinder2.z = rectangle.transform.localScale.z / 2f + cylinder2.transform.localScale.y;
            cylinder2.transform.position = rectangle.transform.position + positionCylinder2;

            Vector3 positionCylinder3 = new Vector3();
            positionCylinder3.x = rectangle.transform.localScale.x / 2f;
            positionCylinder3.y = 0f;
            positionCylinder3.z = -(rectangle.transform.localScale.z / 2f + cylinder3.transform.localScale.y);
            cylinder3.transform.position = rectangle.transform.position + positionCylinder3;

            Vector3 positionCylinder4 = new Vector3();
            positionCylinder4.x = -rectangle.transform.localScale.x / 2f;
            positionCylinder4.y = 0f;
            positionCylinder4.z = -(rectangle.transform.localScale.z / 2f + cylinder4.transform.localScale.y);
            cylinder4.transform.position = rectangle.transform.position + positionCylinder4;

            // Tạo trục súng
            GameObject axisGun = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

            axisGun.transform.localScale = new Vector3(rectangleScaleX / 10f, rectangleScaleX / 10f, rectangleScaleX / 10f); // (0.2, 0.2, 0.2)

            Vector3 positionAxisGun = new Vector3();
            positionAxisGun.x = square.transform.position.x;
            positionAxisGun.y = square.transform.position.y + square.transform.localScale.y / 2f + axisGun.transform.localScale.y / 2f;
            positionAxisGun.z = square.transform.position.z;
            axisGun.transform.position = positionAxisGun;

            // Tạo súng
            GameObject gun = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

            gun.transform.localScale = new Vector3(rectangleScaleX / 10f + 0.1f, rectangleScaleX / 10f + 0.1f, rectangleScaleX / 10f + 0.1f);
            Quaternion rotationGun = Quaternion.Euler(new Vector3(0f, 0f, 90f));
            gun.transform.rotation = rotationGun;

            Vector3 positionGun = new Vector3();
            positionGun.x = axisGun.transform.position.x + (gun.transform.localScale.y / 3f);
            positionGun.y = axisGun.transform.position.y + (gun.transform.localScale.z);
            positionGun.z = axisGun.transform.position.z;
            gun.transform.position = positionGun;


            // Đặt màu sắc
            Renderer rectangleRenderer = rectangle.GetComponent<Renderer>();
            Renderer squareRenderer = square.GetComponent<Renderer>();
            Renderer rectangle2Renderer = rectangle2.GetComponent<Renderer>();
            Renderer cyleider1Renderer = cylinder1.GetComponent<Renderer>();
            Renderer cyleider2Renderer = cylinder2.GetComponent<Renderer>();
            Renderer cyleider3Renderer = cylinder3.GetComponent<Renderer>();
            Renderer cyleider4Renderer = cylinder4.GetComponent<Renderer>();
            Renderer axisGunRenderer = axisGun.GetComponent<Renderer>();
            Renderer gunRenderer = gun.GetComponent<Renderer>();

            rectangleRenderer.material.color = Color.blue;
            squareRenderer.material.color = Color.red;
            rectangle2Renderer.material.color = Color.green;
            cyleider1Renderer.material.color = Color.yellow;
            cyleider2Renderer.material.color = Color.yellow;
            cyleider3Renderer.material.color = Color.yellow;
            cyleider4Renderer.material.color = Color.yellow;
            axisGunRenderer.material.color = Color.green;
            gunRenderer.material.color = Color.cyan;

            // Tạo empty object "Tank"
            GameObject tank = new GameObject("Tank");

            // Gắn các đối tượng vào empty object "Tank"
            rectangle.transform.parent = tank.transform;
            square.transform.parent = tank.transform;
            rectangle2.transform.parent = tank.transform;
            cylinder1.transform.parent = tank.transform;
            cylinder2.transform.parent = tank.transform;
            cylinder3.transform.parent = tank.transform;
            cylinder4.transform.parent = tank.transform;
            axisGun.transform.parent = tank.transform;
            gun.transform.parent = tank.transform;


            buttonClickHandler.DontCreateTank();
        }
    }
}
