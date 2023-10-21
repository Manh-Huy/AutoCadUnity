using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Linq;
using UnityEditor;

public class Create3D : MonoBehaviour
{
    public Text resultAllEntityText;
    public Text resultUniqueEntityText;
    public Button readButton;
    public Button createButton;

    private List<EntityInfo> _listAllEntities = new List<EntityInfo>();
    private List<EntityInfo> _listUniqueEntities = new List<EntityInfo>(); //không cần thiết lắm (quăng)
    public GameObject _doorPrefab;



    Vector3 targetScale = new Vector3(1f, 1f, 1f); // Kích thước mục tiêu
    Vector3 initialDoorScale = new Vector3(50f, 50f, 50f); // Kích thước ban đầu của cửa



    private void Start()
    {
        readButton.onClick.AddListener(ReadJSON);
        createButton.onClick.AddListener(CreateAllEntities);
    }
    private void ReadJSON()
    {
        string jsonPath = EditorUtility.OpenFilePanel("Select JSON File", "", "json");

        if (!string.IsNullOrEmpty(jsonPath))
        {
            string json = File.ReadAllText(jsonPath);

            _listAllEntities.Clear();
            _listUniqueEntities.Clear();

            try
            {
                Document document = JsonConvert.DeserializeObject<Document>(json);

                _listAllEntities = document.getAllEntity();

                _listUniqueEntities = _listAllEntities
                    .GroupBy(entity => new { entity.LayerName, entity.ObjectType })
                    .Select(group => new EntityInfo(null, group.Key.LayerName, group.Key.ObjectType, null))
                    .ToList();


                resultAllEntityText.text = "All Entities:\n";
                foreach (EntityInfo entity in _listAllEntities)
                {
                    string coordinates = "";
                    resultAllEntityText.text += $"{entity.LayerName} ({entity.ObjectType}) \n";

                    if(entity.ObjectType == "LwPolyline")
                    {
                        foreach (string temp in entity.Coordinates)
                        {
                            coordinates += temp + "\n";
                        }
                        resultAllEntityText.text += coordinates;
                    }
                    if (entity.ObjectType == "Insert")
                    {
                        foreach (string temp in entity.Coordinates)
                        {
                            coordinates += temp + "\n";
                        }
                        resultAllEntityText.text += coordinates;
                    }
                }

                // In danh sách phần tử trong _listUniqueEntities vào resultUniqueEntityText
                resultUniqueEntityText.text = "Unique Entities:\n";
                foreach (EntityInfo entity in _listUniqueEntities)
                {
                    resultUniqueEntityText.text += $"{entity.LayerName} ({entity.ObjectType})\n";
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Lỗi khi đọc tệp JSON: " + ex.Message);
            }
        }
    }

    private void CreateAllEntities()
    {
        // lấy vi trí và kích thước của tường trước để dựng cái đối tượng insert
        int isHaveWalls = 0;
        foreach (EntityInfo entity in _listAllEntities)
        {
            if (entity.ObjectType == "LwPolyline")
            {
                isHaveWalls++;
                List<Vector3> verticesList = new List<Vector3>();

                for (int i = 0; i < 2; i++) // Chỉ lấy 2 phần tử đầu
                {
                    string[] coordinateValues = entity.Coordinates[i].Split(',');

                    if (coordinateValues.Length == 2 && float.TryParse(coordinateValues[0], out float x) && float.TryParse(coordinateValues[1], out float z))
                    {
                        Vector3 vertex = new Vector3(x, 0, z);
                        verticesList.Add(vertex);
                    }
                    else
                    {
                        Debug.Log("Wrong syntax of coordinate of Walls");
                        return;
                    }
                }
                for (int i = 0; i < verticesList.Count; i++)
                {
                    Vector3 startPoint;
                    Vector3 endPoint;
                    if (i == (verticesList.Count - 1))
                    {
                        startPoint = verticesList[i];
                        endPoint = verticesList[0];
                    }
                    else
                    {
                        startPoint = verticesList[i];
                        endPoint = verticesList[i + 1];
                    }
                    // Tính toán vị trí và kích thước của cube
                    Vector3 centerPosition = (startPoint + endPoint) / 2f;
                    float distance = Vector3.Distance(startPoint, endPoint);
                }
            }
        }
        if (isHaveWalls == 0)
        {
            Debug.Log("There are no walls in the architecture");
            return;
        }

        GameObject cubeContainer = new GameObject("CubeContainer");

        foreach (EntityInfo entity in _listAllEntities)
        {
            if (entity.ObjectType == "LwPolyline")
            {
                List<Vector3> verticesList = new List<Vector3>();

                foreach (string coordinate in entity.Coordinates)
                {
                    // Tách các giá trị từ dòng dữ liệu
                    string[] values = coordinate.Split(',');

                    if (values.Length == 2)
                    {
                        if (float.TryParse(values[0], out float x) && float.TryParse(values[1], out float z))
                        {
                            Vector3 vertex = new Vector3(x, 0, z);
                            verticesList.Add(vertex);
                        }
                    }
                    else
                    {
                        Debug.Log("Wrong syntax of coordinate");
                    }
                }

                for (int i = 0; i < verticesList.Count; i++)
                {
                    Vector3 startPoint;
                    Vector3 endPoint;

                    if (i == (verticesList.Count - 1))
                    {
                        startPoint = verticesList[i];
                        endPoint = verticesList[0];
                    }
                    else
                    {
                        startPoint = verticesList[i];
                        endPoint = verticesList[i + 1];
                    }
                    CreateCube(cubeContainer, startPoint, endPoint, 200f);
                }
            }

            if (entity.ObjectType == "Insert" && entity.LayerName == "Doors")
            {
                string[] values = entity.Coordinates[0].Split(',');
                if (values.Length == 3)
                {
                    if (float.TryParse(values[0], out float x) && float.TryParse(values[1], out float z))
                    {
                        float customScale = 50.0f;
                        Vector3 position = new Vector3(x, 0, z);
                        Quaternion rotation = Quaternion.Euler(-90f, -90f, 0f);
                        GameObject door = Instantiate(_doorPrefab, position, rotation);
                        door.transform.localScale = _doorPrefab.transform.localScale * customScale;
                    }
                    else
                    {
                        Debug.Log("Wrong syntax of coordinate");
                    }
                }
            }
        }
    }

    private void CreateCube(GameObject container, Vector3 startPoint, Vector3 endPoint, float height)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // Tính toán vị trí và kích thước của cube
        Vector3 centerPosition = (startPoint + endPoint) / 2f;
        cube.transform.position = centerPosition;
        float distance = Vector3.Distance(startPoint, endPoint);
        cube.transform.localScale = new Vector3(2f, height, distance);

        // Xoay cube để nó hướng từ điểm đầu đến điểm cuối
        cube.transform.LookAt(endPoint);

        // Chuyển cube vào empty GameObject (container)
        cube.transform.parent = container.transform;

        // Chuyển cube sang Layer 3D (0 là mặc định)
        cube.layer = 0;

        // đặt lại vị trí cube trên mặt đất (trên (0,0,0))
        Vector3 newPosition = cube.transform.position;
        newPosition.y = height / 2;
        cube.transform.position = newPosition;

        Renderer cubeRenderer = cube.GetComponent<Renderer>();
        cubeRenderer.material.color = Color.gray;
    }


}
