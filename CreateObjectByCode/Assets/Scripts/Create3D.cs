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
using static UnityEngine.EventSystems.EventTrigger;

public class Create3D : MonoBehaviour
{
    [SerializeField]
    private Text entitiyText;
    [SerializeField]
    private GameObject _doorPrefab;

    private List<ExportArchitectureToJSON> _listToExport = new List<ExportArchitectureToJSON>();
    private List<UnityFloor> _listFloor = new List<UnityFloor>();


    private void Start()
    {
        //readButton.onClick.AddListener(ReadJSON);
        //createButton.onClick.AddListener(CreateAllEntities);
    }
    public void ReadJSON()
    {
        string jsonPath = EditorUtility.OpenFilePanel("Select JSON File", "", "json");

        if (!string.IsNullOrEmpty(jsonPath))
        {
            string json = File.ReadAllText(jsonPath);

            _listToExport.Clear();
            try
            {
                //_listToExport = JsonConvert.DeserializeObject <List<ExportArchitectureToJSON>>(json);

                UnityArchitecture unityArchitecture = JsonConvert.DeserializeObject<UnityArchitecture>(json);

                foreach (UnityFloor floor in unityArchitecture.ListFloor)
                {
                    _listFloor.Add(floor);
                }

                // In ra file text
                //wallEntitiyText.text = "Entity: \n";
                //foreach(var entity in _listAllEntities)
                //{
                //    wallEntitiyText.text += entity.LayerName.ToString();
                //    wallEntitiyText.text += "\n";
                //}
            }
            catch (Exception ex)
            {
                Debug.Log("Lỗi khi đọc tệp JSON: " + ex.Message);
            }
        }
    }
    //float entityHeight = entity.Height;

    //if (entityHeight > maxHeight)
    //{
    //    maxHeight = entityHeight; // Cập nhật chiều cao lớn nhất
    //}

    //CreateWall(entity, wallContainer, floorHeight + entityHeight);
    public void CreateAllEntities()
    {
        float roundHeight = 0;

        foreach (UnityFloor floor in _listFloor)
        {
            GameObject floorContainer = new GameObject("Floor Container");
            GameObject wallContainer = new GameObject("Wall Container");
            GameObject doorContainer = new GameObject("Door Container");
            float floorHeight = 0;

            wallContainer.transform.parent = floorContainer.transform;
            doorContainer.transform.parent = floorContainer.transform;

            foreach (UnityEntity entity in floor.ListEntities)
            {
                floorHeight = 200f;
                //floorHeight = entity.Height;

                if (entity.TypeOfUnityEntity == "Wall" && entity.ObjectType == "LwPolyline")
                {
                    float wallHeight = entity.Height;

                    if (wallHeight > floorHeight) 
                    {
                        floorHeight = wallHeight;
                    }

                    CreateWall(entity, wallContainer, floorHeight, floorHeight + roundHeight);
                }

                if (entity.ObjectType == "Insert" && entity.TypeOfUnityEntity == "Door")
                {
                    CreateDoor(entity, doorContainer, roundHeight);
                }
            }
            roundHeight += floorHeight;
        }
    }

    private void CreateWall(UnityEntity entity, GameObject wallContainer, float height, float roundHeight)
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
            CreateCube(wallContainer, startPoint, endPoint, height, roundHeight);
        }
    }

    private void CreateDoor(UnityEntity entity, GameObject doorContainer, float roundheight)
    {
        string[] values = entity.Coordinates[0].Split(',');
        if (values.Length == 3)
        {
            if (float.TryParse(values[0], out float x) && float.TryParse(values[1], out float z))
            {
                float customScale = 50.0f;
                Vector3 position = new Vector3(x, roundheight / 2, z);
                Quaternion rotation = Quaternion.Euler(-90f, -90f, 0f);
                GameObject door = Instantiate(_doorPrefab, position, rotation);
                door.transform.localScale = _doorPrefab.transform.localScale * customScale;
                door.transform.parent = doorContainer.transform;
            }
            else
            {
                Debug.Log("Wrong syntax of coordinate");
            }
        }
    }

    private void CreateCube(GameObject container, Vector3 startPoint, Vector3 endPoint, float height, float roundHeight)
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
        newPosition.y = roundHeight / 2;
        cube.transform.position = newPosition;

        Renderer cubeRenderer = cube.GetComponent<Renderer>();
        cubeRenderer.material.color = Color.gray;
    }
}
