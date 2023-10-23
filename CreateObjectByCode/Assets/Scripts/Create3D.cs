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
    public Text wallEntitiyText;
    public Text powerEntitiyText;
    public Text doorEntitiyText;
    public Text stairEntitiyText;

    public Button readButton;
    public Button createButton;

    private List<UnityEntity> _listAllEntities = new List<UnityEntity>();
    private List<Door> _listDoorEntities = new List<Door>();
    private List<Power> _listPowerEntities = new List<Power>();
    private List<Stair> _listStairEntities = new List<Stair>();
    private List<Wall> _listWallEntities = new List<Wall>();

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
            try
            {
                //List<UnityEntity> unityEntity = JsonConvert.DeserializeObject<List<UnityEntity>>(json);

                //// Lọc và tạo danh sách Wall Entities
                //List<Wall> wallEntities = unityEntity
                //    .Where(entity => entity is Wall)
                //    .Select(entity => (Wall)entity)
                //    .ToList();

                //_listWallEntities = wallEntities;

                //// Lọc và tạo danh sách Door Entities
                //List<Door> doorEntities = unityEntity
                //    .Where(entity => entity is Door)
                //    .Select(entity => (Door)entity)
                //    .ToList();

                //_listDoorEntities = doorEntities;

                //// Lọc và tạo danh sách Power Entities
                //List<Power> powerEntities = unityEntity
                //    .Where(entity => entity is Power)
                //    .Select(entity => (Power)entity)
                //    .ToList();

                //_listPowerEntities = powerEntities;

                //// Lọc và tạo danh sách Stair Entities
                //List<Stair> stairEntities = unityEntity
                //    .Where(entity => entity is Stair)
                //    .Select(entity => (Stair)entity)
                //    .ToList();

                //_listStairEntities = stairEntities;

                //List<UnityEntity> unityEntities = JsonConvert.DeserializeObject<List<UnityEntity>>(json);

                //foreach (var entity in unityEntities)
                //{
                //    if (entity is Door door)
                //    {
                //        _listDoorEntities.Add(door);
                //    }
                //    else if (entity is Stair stair)
                //    {
                //        _listStairEntities.Add(stair);
                //    }
                //    else if (entity is Power power)
                //    {
                //        _listPowerEntities.Add(power);
                //    }
                //    else if (entity is Wall wall)
                //    {
                //        _listWallEntities.Add(wall);
                //    }
                //}
                //foreach (var entity in unityEntities)
                //{
                //    //_listAllEntities.Add(entity);

                //    //if (entity is Wall wall)
                //    //{
                //    //    _listWallEntities.Add(wall);
                //    //}
                //    //else if (entity is Door door)
                //    //{
                //    //    _listDoorEntities.Add(door);
                //    //}
                //    //else if (entity is Stair stair)
                //    //{
                //    //    _listStairEntities.Add(stair);
                //    //}

                //    if (entity.TypeOfUnityEntity == "Wall")
                //    {
                //        _listWallEntities.Add(entity);
                //    }
                //}

                List<ExportArchitectureToJSON> unityEntity = JsonConvert.DeserializeObject<List<ExportArchitectureToJSON>>(json);

                foreach (var entity in unityEntity)
                {
                   

                }

                // In ra file text
                wallEntitiyText.text = "Entity: \n";
                foreach(var entity in _listAllEntities)
                {
                    wallEntitiyText.text += entity.LayerName.ToString();
                    wallEntitiyText.text += "\n";
                }

                powerEntitiyText.text = "Power: \n";
                foreach (var entity in _listPowerEntities)
                {
                    powerEntitiyText.text += entity.TypeOfUnityEntity.ToString();
                    powerEntitiyText.text += "\n";
                }

                stairEntitiyText.text = "Stair: \n";
                foreach (var entity in _listStairEntities)
                {
                    stairEntitiyText.text += entity.TypeOfUnityEntity.ToString();
                    stairEntitiyText.text += "\n";
                }

                doorEntitiyText.text = "Door: \n";
                foreach (var entity in _listDoorEntities)
                {
                    doorEntitiyText.text += entity.TypeOfUnityEntity.ToString();
                    doorEntitiyText.text += "\n";
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
        foreach (UnityEntity entity in _listAllEntities)
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

        foreach (UnityEntity entity in _listAllEntities)
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
