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
using UnityEngine.UIElements;

public class Create3D : MonoBehaviour
{
    [SerializeField]
    private GameObject _doorPrefab;

    [SerializeField]
    private GameObject _stairPrefab;

    [SerializeField]
    private GameObject _windowPrefab;

    private List<UnityFloor> _listFloor = new List<UnityFloor>();
    List<Vector3> listAllVerticesOfWall = new List<Vector3>();


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

            _listFloor.Clear();
            try
            {
                UnityArchitecture unityArchitecture = JsonConvert.DeserializeObject<UnityArchitecture>(json);

                foreach (UnityFloor floor in unityArchitecture.ListFloor)
                {
                    _listFloor.Add(floor);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Lỗi khi đọc tệp JSON: " + ex.Message);
            }
        }
    }

    public void CreateAllEntities()
    {
        float groundHeight = 0;

        foreach (UnityFloor floor in _listFloor)
        {
            GameObject floorContainer = new GameObject("Floor Container");
            GameObject wallContainer = new GameObject("Wall Container");
            GameObject doorContainer = new GameObject("Door Container");
            GameObject stairContainer = new GameObject("Stair Container");
            GameObject windowContainer = new GameObject("Window Container");
            float floorHeight = 0;

            wallContainer.transform.parent = floorContainer.transform;
            doorContainer.transform.parent = floorContainer.transform;
            stairContainer.transform.parent = floorContainer.transform;
            windowContainer.transform.parent = floorContainer.transform;

            // stair
            List<Vector3> verticeStairsList = new List<Vector3>();
            Vector3 stairPosition = new Vector3();
            float stairLength; // chiều dài của cầu thang
            float stairWidth;  // chiều rộng của cầu thang



            foreach (UnityEntity entity in floor.ListEntities)
            {
                //float entityHeight = entity.Height;

                //if (entityHeight > floorHeight)
                //{
                //    // floorHeight là chiều cao lớn nhất trong tầng đó (bthg là chiều cao của tường lớn nhất)
                //    floorHeight = entityHeight;
                //}

                floorHeight = 100f; // thay thế các dòng comment trên (set cứng)

                if (entity.TypeOfUnityEntity == "Wall" && entity.ObjectType == "LwPolyline")
                {
                    //float wallHeight = entityHeight;
                    float wallHeight = 100f; // thay thế dòng trên (set cứng)

                    CreateWall(entity, wallContainer, wallHeight, groundHeight);
                }

                if (entity.ObjectType == "Insert" && entity.TypeOfUnityEntity == "Door")
                {
                    CreateDoor(floor.ListEntities, entity, doorContainer, groundHeight);
                }

                if (entity.ObjectType == "Line" && entity.TypeOfUnityEntity == "Stair")
                {
                    verticeStairsList.AddRange(AddAllVector3ofEntitiesToList(entity));
                }

                if (entity.TypeOfUnityEntity == "Window" && entity.ObjectType == "Insert")
                {
                    CreateWindow(floor.ListEntities, entity, windowContainer, groundHeight + (floorHeight / 2));
                }
            }

            if (verticeStairsList.Count > 0)
            {
                stairPosition = CalculateCenterCoordinates(verticeStairsList);
                stairLength = CalculateNthMaxDistance(verticeStairsList, 1);
                stairWidth = CalculateNthMaxDistance(verticeStairsList, 2) / 2f;
                CreateStair(stairPosition, stairLength, stairWidth, stairContainer, floorHeight, groundHeight);
            }


            // cộng với chiều cao tầng này để bắt đầu dựng tầng sau
            groundHeight += floorHeight;
        }
    }

    private void CreateWall(UnityEntity entity, GameObject wallContainer, float height, float groundHeight)
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
                    Vector3 vertex = new Vector3(x, groundHeight, z);
                    verticesList.Add(vertex);
                }
            }
            else
            {
                Debug.Log("Wrong syntax of coordinate");
            }
        }

        listAllVerticesOfWall.AddRange(verticesList);

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
            CreateCube(wallContainer, startPoint, endPoint, height, groundHeight);
        }
    }

    #region Functions Create Door

    private void CreateDoor(List<UnityEntity> listEntities, UnityEntity entity, GameObject doorContainer, float groundheight)
    {
        List<Vector3> listPoint = new List<Vector3>();
        Vector3 positionDoor = new Vector3();
        Vector3 positionResult = new Vector3();
        float distanceResult = float.MaxValue;
        Quaternion rotation = Quaternion.Euler(-90f, -90f, 0f);
        string[] doorValue = entity.Coordinates[0].Split(',');
        if (doorValue.Length == 3)
        {
            if (float.TryParse(doorValue[0], out float xDoor) && float.TryParse(doorValue[1], out float zDoor))
            {
                positionDoor.x = xDoor;
                positionDoor.y = groundheight;
                positionDoor.z = zDoor;

                foreach (UnityEntity unityEntity in listEntities)
                {
                    if (unityEntity.TypeOfUnityEntity == "Wall" && unityEntity.ObjectType == "LwPolyline")
                    {
                        foreach (string coordinate in unityEntity.Coordinates)
                        {
                            string[] lineValues = coordinate.Split(',');
                            if (lineValues.Length == 2)
                            {
                                if (float.TryParse(lineValues[0], out float xWall) && float.TryParse(lineValues[1], out float zWall))
                                {
                                    if (xDoor == xWall || zDoor == zWall)
                                    {
                                        Vector3 point = new Vector3(xWall, groundheight, zWall);
                                        listPoint.Add(point);
                                    }
                                }
                            }
                        }
                    }
                }

                listPoint = HandleListPoint(listPoint, positionDoor);


                /*float customScale = 50.0f;
                Vector3 position = new Vector3(xDoor, groundheight, zDoor);
                Quaternion rotation = Quaternion.Euler(-90f, -90f, 0f);
                GameObject door = Instantiate(_doorPrefab, position, rotation);
                door.transform.localScale = _doorPrefab.transform.localScale * customScale;
                door.transform.parent = doorContainer.transform;*/
            }
            else
            {
                Debug.Log("Wrong syntax of coordinate");
            }
        }
        foreach (Vector3 point in listPoint)
        {
            float distance = CalculateDistance(positionDoor.x, positionDoor.z, point.x, point.z);
            if (distance != 0)
            {
                if (distance < distanceResult && !isWall(positionDoor, point))
                {
                    distanceResult = distance;
                    positionResult = point;
                }
            }
        }



        if (positionDoor.x == positionResult.x && positionDoor.z < positionResult.z)
        {
            rotation = Quaternion.Euler(-90f, -90f, -90f);
        }
        else if (positionDoor.x == positionResult.x && positionDoor.z > positionResult.z)
        {
            rotation = Quaternion.Euler(-90f, -90f, 90f);
        }
        else if (positionDoor.x > positionResult.x && positionDoor.z == positionResult.z)
        {
            rotation = Quaternion.Euler(-90f, -90f, 180f);
        }
        else if (positionDoor.x < positionResult.x && positionDoor.z == positionResult.z)
        {
            rotation = Quaternion.Euler(-90f, -90f, 0f);
        }
        float customScale = 50.0f;
        GameObject door = Instantiate(_doorPrefab, positionDoor, rotation);
        door.transform.localScale = _doorPrefab.transform.localScale * customScale;
        door.transform.parent = doorContainer.transform;

    }

    private List<Vector3> HandleListPoint(List<Vector3> list, Vector3 positionDoor)
    {
        List<Vector3> result = new List<Vector3>(4);
        SortVector3ListByX(list);
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].x == positionDoor.x && list[i].z == positionDoor.z)
            {
                if (i == 0)
                {
                    result.Add(list[i + 1]);
                }
                else if (i == list.Count - 1)
                {
                    result.Add(list[i - 1]);
                }
                else
                {
                    result.Add(list[i + 1]);
                    result.Add(list[i - 1]);
                }
            }
        }


        SortVector3ListByZ(list);
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].x == positionDoor.x && list[i].z == positionDoor.z)
            {
                if (i == 0)
                {
                    result.Add(list[i + 1]);
                }
                else if (i == list.Count - 1)
                {
                    result.Add(list[i - 1]);
                }
                else
                {
                    result.Add(list[i + 1]);
                    result.Add(list[i - 1]);
                }
            }
        }

        result = RemoveDuplicates(result);

        return result;
    }

    private List<Vector3> RemoveDuplicates(List<Vector3> list)
    {
        List<Vector3> uniqueList = new List<Vector3>();

        foreach (Vector3 item in list)
        {
            if (!uniqueList.Contains(item))
            {
                uniqueList.Add(item);
            }
        }

        return uniqueList;
    }

    public List<Vector3> SortVector3ListByX(List<Vector3> list)
    {
        list.Sort(CompareVector3ByX);
        return list;
    }

    private int CompareVector3ByX(Vector3 a, Vector3 b)
    {
        if (a.x < b.x)
            return -1;
        else if (a.x > b.x)
            return 1;
        else
        {
            if (a.z < b.z)
                return -1;
            else if (a.z > b.z)
                return 1;
            else
                return 0;
        }
    }

    public List<Vector3> SortVector3ListByZ(List<Vector3> list)
    {
        list.Sort(CompareVector3ByZ);
        return list;
    }

    private int CompareVector3ByZ(Vector3 a, Vector3 b)
    {
        if (a.z < b.z)
            return -1;
        else if (a.z > b.z)
            return 1;
        else
        {
            if (a.x < b.x)
                return -1;
            else if (a.x > b.x)
                return 1;
            else
                return 0;
        }
    }

    private bool isWall(Vector3 positionDoor, Vector3 verticeWall)
    {
        for (int i = 0; i < listAllVerticesOfWall.Count; i++)
        {
            Vector3 startPoint;
            Vector3 endPoint;

            if (i == (listAllVerticesOfWall.Count - 1))
            {
                startPoint = listAllVerticesOfWall[i];
                endPoint = listAllVerticesOfWall[0];
            }
            else
            {
                startPoint = listAllVerticesOfWall[i];
                endPoint = listAllVerticesOfWall[i + 1];
            }

            if ((positionDoor == startPoint && verticeWall == endPoint)
                || (positionDoor == endPoint && verticeWall == startPoint))
            {
                return true;
            }

        }
        return false;
    }

    #endregion

    #region Functions Create Stair

    private float CalculateDistance(float x1, float y1, float x2, float y2)
    {
        float deltaX = x2 - x1;
        float deltaY = y2 - y1;

        // Sử dụng công thức khoảng cách Euclid: sqrt((x2 - x1)^2 + (y2 - y1)^2)
        float distance = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

        return distance;
    }

    /*
    nếu floorHeight = 200:
    scale y wall: 1 --> 200
    scale y stair: 35--> ? (35 * 200)

    position y wall: scale = 1 --> position y = 0.5
    =>position y wall = scale y wall / 2
    position y stair: scale = 35 --> position y = 0.4375
    =>positiion y stair = scale y stair * 0.4375 / 35

    floorHeight = 200 
    => position y stair = 35 * 200 * 0.4375 / 35 = 200 * 0.4375;
    => scale y stair = 35 * 200 

    ==> position y stair = floorHeight * 0.4375;
    ==> scale y stair = floorHeight * 35
     */
    private void CreateStair(Vector3 position, float stairLength, float stairWidth, GameObject containerStair, float floorHeight, float groundHeight)
    {
        position.y = groundHeight + floorHeight * 0.4375f;
        Quaternion rotation = Quaternion.Euler(0, 90f, 0f);
        GameObject stair = Instantiate(_stairPrefab, position, rotation);

        //Vector3 defaultScale = _stairPrefab.transform.localScale;
        //stair.transform.localScale = defaultScale * floorHeight / defaultScale.y;
        stair.transform.localScale = new Vector3(stairWidth * 35, floorHeight * 35, stairLength * 35);
        stair.transform.parent = containerStair.transform;
    }

    private List<Vector3> AddAllVector3ofEntitiesToList(UnityEntity entity)
    {
        List<Vector3> verticesList = new List<Vector3>();

        foreach (string coordinate in entity.Coordinates)
        {
            // Tách các giá trị từ dòng dữ liệu
            string[] values = coordinate.Split(',');

            if (values.Length == 3)
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

        return verticesList;
    }

    // Tính độ dài lớn nhất (n= 0), nhì (n = 1), ba (n = 2), ... giữa các điểm trong list
    public float CalculateNthMaxDistance(List<Vector3> pointList, int n)
    {
        if (n <= 0)
        {
            throw new ArgumentException("Parameter 'n' must be greater than 0.");
        }

        List<float> distances = new List<float>();

        for (int i = 0; i < pointList.Count; i++)
        {
            for (int j = i + 1; j < pointList.Count; j++)
            {
                Vector3 pointA = pointList[i];
                Vector3 pointB = pointList[j];

                float distance = Vector2.Distance(new Vector2(pointA.x, pointA.y), new Vector2(pointB.x, pointB.y));
                distances.Add(distance);
            }
        }

        // Sort the distances in descending order.
        distances.Sort((a, b) => -a.CompareTo(b));

        if (n <= distances.Count)
        {
            return distances[n - 1];
        }
        else
        {
            throw new ArgumentException("Parameter 'n' exceeds the number of distances in the list.");
        }
    }

    private Vector3 CalculateCenterCoordinates(List<Vector3> verticesList)
    {
        Vector3 center = Vector3.zero;
        foreach (Vector3 coord in verticesList)
        {
            center += coord;
        }
        center /= verticesList.Count;
        return center; // vector output sẽ co x và y là trọng tâm còn z có mặc định là 0
    }

    private void CreateCube(GameObject container, Vector3 startPoint, Vector3 endPoint, float height, float groundHeight)
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
        newPosition.y = groundHeight + (height) / 2;
        cube.transform.position = newPosition;

        Renderer cubeRenderer = cube.GetComponent<Renderer>();
        cubeRenderer.material.color = Color.gray;
    }

    #endregion

    #region Functions Create Window
    private void CreateWindow(List<UnityEntity> listEntities, UnityEntity entity, GameObject windowContainer, float groundHeight)
    {
        List<Vector3> listPointOfWall = new List<Vector3>();
        Vector3 positionWindow = new Vector3();
        Quaternion rotation = Quaternion.Euler(-90f, -90f, 0f);


        string[] windowValue = entity.Coordinates[0].Split(',');
        if (windowValue.Length == 3)
        {
            if (float.TryParse(windowValue[0], out float xWindow) && float.TryParse(windowValue[1], out float zWindow))
            {
                positionWindow.x = xWindow;
                positionWindow.y = groundHeight;
                positionWindow.z = zWindow;

                foreach (UnityEntity unityEntity in listEntities)
                {
                    if (unityEntity.TypeOfUnityEntity == "Wall" && unityEntity.ObjectType == "LwPolyline")
                    {
                        foreach (string coordinate in unityEntity.Coordinates)
                        {
                            string[] lineValues = coordinate.Split(',');
                            if (lineValues.Length == 2)
                            {
                                if (float.TryParse(lineValues[0], out float xWall) && float.TryParse(lineValues[1], out float zWall))
                                {
                                    if (xWindow == xWall || zWindow == zWall)
                                    {
                                        Vector3 point = new Vector3(xWall, 0, zWall);
                                        listPointOfWall.Add(point);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        listPointOfWall = HandleListPointOfWall(listPointOfWall, positionWindow);

        if (listPointOfWall.Count == 2)
        {
            if(listPointOfWall[0].x == positionWindow.x && listPointOfWall[1].x == positionWindow.x)
            {
                rotation = Quaternion.Euler(-90f, 0f, 180f);
            }
        }
        else if (listPointOfWall.Count == 4)
        {
        }


        GameObject window = Instantiate(_windowPrefab, positionWindow, rotation);
        float customScale = 50.0f;
        window.transform.localScale = _windowPrefab.transform.localScale * customScale;
        window.transform.localScale += new Vector3(60f, -3f, 10f);
        window.transform.parent = windowContainer.transform;
    }

    private List<Vector3> HandleListPointOfWall(List<Vector3> list, Vector3 positionWindow)
    {
        List<Vector3> result = new List<Vector3>(4);
        int countX = 0; int countZ = 0;


        foreach (Vector3 point in list)
        {
            if (point.x == positionWindow.x) countX++;
            if (point.z == positionWindow.z) countZ++;
        }


        if (countX >= 2)
        {
            SortVector3ListByX(list);
            for (int i = 0; i < list.Count - 1; i++)
            {
                if (list[i].x == positionWindow.x && list[i + 1].x == positionWindow.x &&
                    list[i].z < positionWindow.z && list[i + 1].z > positionWindow.z)
                {
                    result.Add(list[i]);
                    result.Add(list[i + 1]);
                }
            }
        }


        if (countZ >= 2)
        {
            SortVector3ListByZ(list);
            for (int i = 0; i < list.Count - 1; i++)
            {
                if (list[i].x < positionWindow.x && list[i + 1].x > positionWindow.x &&
                    list[i].z == positionWindow.z && list[i + 1].z == positionWindow.z)
                {
                    result.Add(list[i]);
                    result.Add(list[i + 1]);
                }
            }
        }

        result = RemoveDuplicates(result);

        return result;
    }
    #endregion





}
