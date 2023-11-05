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
using Newtonsoft.Json.Linq;
using System.Net;
using System.Drawing;
using SystemColor = System.Drawing.Color;
using UnityColor = UnityEngine.Color;

public class Create3D : MonoBehaviour
{
    [SerializeField]
    private GameObject _houseObject;

    [SerializeField]
    private GameObject _camera;
    [SerializeField]
    private GameObject _doorPrefab;

    [SerializeField]
    private GameObject _stairPrefab;

    [SerializeField]
    private GameObject _windowPrefab;

    [SerializeField]
    private GameObject _powerPrefab;

    [SerializeField]
    private GameObject _roofTopPrefab;

    private bool _hasRoofTop = false;

    private List<UnityFloor> _listFloor = new List<UnityFloor>();
    List<Vector3> __listAllVerticesOfWall = new List<Vector3>();
    public Vector3 _centerPoint = new Vector3();
    List<GameObject> _listWall = new List<GameObject>();


    private void Start()
    {
        ReadJSON();
        CreateAllEntities();
    }
    public void ReadJSON()
    {
        string jsonPath = "Build\\house2.json";
        //string jsonPath = "house2.json";

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


                if (unityArchitecture.TypeOfRoof == "Rooftop")
                {
                    _hasRoofTop = true;
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
        int floorIndex = 0;
        float groundHeight = 0;

        List<Vector3> coordinatesLastFloorOfWallList = new List<Vector3>();

        foreach (UnityFloor floor in _listFloor)
        {
            GameObject wallContainer = new GameObject("Wall Container");
            GameObject doorContainer = new GameObject("Door Container");
            GameObject stairContainer = new GameObject("Stair Container");
            GameObject windowContainer = new GameObject("Window Container");
            GameObject powerContainer = new GameObject("Power Container");
            float floorHeight = 0;



            // stair
            List<Vector3> verticeStairsList = new List<Vector3>();
            Vector3 stairPosition = new Vector3();
            float stairLength; // chiều dài của cầu thang
            float stairWidth;  // chiều rộng của cầu thang

            List<Vector3> coordinatesWallEachFloorList = new List<Vector3>();

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

                    // check đã đến tầng cuối chưa. Nếu rồi thì lấy tọa độ của tường để dựng roof
                    if (floorIndex == _listFloor.Count - 1) // && cần thêm dk nhà này có rooftop không?
                    {
                        coordinatesLastFloorOfWallList.AddRange(AddAllVector3ofEntitiesToList(entity, 0));
                    }

                    coordinatesWallEachFloorList.AddRange(AddAllVector3ofEntitiesToList(entity, 0));
                }

                if (entity.ObjectType == "Insert" && entity.TypeOfUnityEntity == "Door")
                {
                    CreateDoor(floor.ListEntities, entity, doorContainer, groundHeight);
                }

                if (entity.ObjectType == "Line" && entity.TypeOfUnityEntity == "Stair")
                {
                    verticeStairsList.AddRange(AddAllVector3ofEntitiesToList(entity, 0));
                }

                if (entity.TypeOfUnityEntity == "Window" && entity.ObjectType == "Insert")
                {
                    CreateWindow(floor.ListEntities, entity, windowContainer, groundHeight + (floorHeight / 2));
                }

                if (entity.TypeOfUnityEntity == "Power" && entity.ObjectType == "Insert")
                {
                    CreatePower(floor.ListEntities, entity, wallContainer, powerContainer, groundHeight + 90);
                }
            }

            // create Stair
            if (verticeStairsList.Count > 0)
            {
                stairPosition = (Vector3)CalculateDimensionAndCenterPoint(verticeStairsList, "centerCoordinates");
                stairLength = (float)CalculateDimensionAndCenterPoint(verticeStairsList, "length");
                stairWidth = (float)CalculateDimensionAndCenterPoint(verticeStairsList, "width") / 2f;
                CreateStair(stairPosition, stairLength, stairWidth, stairContainer, floorHeight, groundHeight);
            }

            // Find center point and change position each floor
            Vector3 centerPointEachFloor = (Vector3)CalculateDimensionAndCenterPoint(coordinatesWallEachFloorList, "centerCoordinates");

            if (floorIndex == 0) // tầng 1
            {
                _centerPoint = centerPointEachFloor;
            }
            /*
             * B1: tạo object cha (floorContainer)
             * B2: di chuyển nó đến vị trí trọng tâm của tầng đó
             * B3: đưa các object con vào object cha
             * B4: di chuyển object cha vào vị trí trung tâm của tầng 1
             */
            GameObject floorContainer = new GameObject("Floor Container"); //B1
            floorContainer.transform.position = centerPointEachFloor; //B2

            //B3
            wallContainer.transform.parent = floorContainer.transform;
            doorContainer.transform.parent = floorContainer.transform;
            stairContainer.transform.parent = floorContainer.transform;
            windowContainer.transform.parent = floorContainer.transform;
            powerContainer.transform.parent = floorContainer.transform;

            // Vì roof là object con của tầng cuối cùng nên tạo CreateRoof tại đây
            // create Roof
            if (coordinatesLastFloorOfWallList.Count > 0)
            {
                Vector3 roofPosition = (Vector3)CalculateDimensionAndCenterPoint(coordinatesLastFloorOfWallList, "centerCoordinates");
                float roofLength = (float)CalculateDimensionAndCenterPoint(coordinatesLastFloorOfWallList, "length"); // chiều dài của roof
                float roofWidth = (float)CalculateDimensionAndCenterPoint(coordinatesLastFloorOfWallList, "width"); // chiều rộng của roof
                // đang set mặc định chiều cao của rooftop =  75% (trung bình cộng chiều cao các tầng ngôi nhà)
                // vị trí y (chiều cao đặt roof) = groundHeight + floorHeight vì nó nằm trên tầng cuối cùng
                CreateRoof(floorContainer, roofPosition, roofLength, roofWidth, (groundHeight / _listFloor.Count) * 0.75f, (groundHeight + floorHeight));
            }

            floorContainer.transform.position = _centerPoint; //B4

            // đưa floorContainer vào object cha (House) và đưa nó vào vị trí trong camera cho dễ nhìn
            floorContainer.transform.parent = _houseObject.transform;
            _houseObject.transform.position = _camera.transform.position;


            floorIndex++;
            // cộng với chiều cao tầng này để bắt đầu dựng tầng sau
            groundHeight += floorHeight;
        }
    }

    #region Function Create Wall
    private void CreateWall(UnityEntity entity, GameObject wallContainer, float height, float groundHeight)
    {
        string color = entity.Color;


        List<Vector3> verticesList = AddAllVector3ofEntitiesToList(entity, groundHeight);
        __listAllVerticesOfWall.AddRange(verticesList);

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
            CreateCube(wallContainer, startPoint, endPoint, height, groundHeight, color);
        }
    }

    #endregion

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
        MeshRenderer doorRenderer = door.GetComponent<MeshRenderer>();
        if (entity.Color != "")
        {
            SystemColor systemColor = GetColor(entity.Color);
            UnityColor resultColor = new Color32(systemColor.R, systemColor.G, systemColor.B, systemColor.A);
            doorRenderer.material.color = resultColor;
        }
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
        for (int i = 0; i < __listAllVerticesOfWall.Count; i++)
        {
            Vector3 startPoint;
            Vector3 endPoint;

            if (i == (__listAllVerticesOfWall.Count - 1))
            {
                startPoint = __listAllVerticesOfWall[i];
                endPoint = __listAllVerticesOfWall[0];
            }
            else
            {
                startPoint = __listAllVerticesOfWall[i];
                endPoint = __listAllVerticesOfWall[i + 1];
            }

            if ((positionDoor == startPoint && verticeWall == endPoint)
                || (positionDoor == endPoint && verticeWall == startPoint))
            {
                return true;
            }

        }
        return false;
    }
    private float CalculateDistance(float x1, float y1, float x2, float y2)
    {
        float deltaX = x2 - x1;
        float deltaY = y2 - y1;

        // Sử dụng công thức khoảng cách Euclid: sqrt((x2 - x1)^2 + (y2 - y1)^2)
        float distance = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

        return distance;
    }

    #endregion

    #region Function Create Stair
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
            if (listPointOfWall[0].x == positionWindow.x && listPointOfWall[1].x == positionWindow.x)
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

        MeshRenderer windowRenderer = window.GetComponent<MeshRenderer>();
        if (entity.Color != "")
        {
            SystemColor systemColor = GetColor(entity.Color);
            UnityColor resultColor = new Color32(systemColor.R, systemColor.G, systemColor.B, systemColor.A);
            windowRenderer.material.color = resultColor;
        }
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

    #region Function Create Roof

    /*
     * scale roof tỉ lệ với cube (1,1,1) là (0.25, 0.25, 0.925)
     * cube (x,y,z) -> roof (x,z,y) -> salce roof = (x là chiều ngang, y là chiều chiều dài, z là chiều cao)
     */
    private void CreateRoof(GameObject container, Vector3 position, float roofLength, float roofWidth, float roofHeight, float groundHeight)
    {
        // create roof
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "Roof";
        float thicknessOfbottomRoof = 1f;

        roof.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        roof.transform.localScale = new Vector3(roofWidth, roofLength, thicknessOfbottomRoof);
        roof.transform.position = new Vector3(position.x, (groundHeight + thicknessOfbottomRoof / 2f), position.z);

        Renderer cubeRenderer = roof.GetComponent<Renderer>();
        cubeRenderer.material.color = UnityColor.gray;

        // create rooftop
        if (_hasRoofTop == true)
        {
            // tỉ lệ về độ lớn theo chiều x và y của rooftop và gameobject(1,1,1) là 0.25 / 1 = 0.25
            // tỉ lệ về độ lớn theo chiều z của rooftop và gameobject(1,1,1) là 0.925 / 1 = 0.925
            float scaleXYRatio = 0.25f;
            float scaleZRatio = 0.925f;
            position.y = groundHeight;
            Quaternion rotation = Quaternion.Euler(-90, 0f, 0f);
            GameObject roofTop = Instantiate(_roofTopPrefab, position, rotation);

            roofTop.transform.localScale = new Vector3(roofWidth * scaleXYRatio, roofLength * scaleXYRatio, roofHeight * scaleZRatio);
            roofTop.transform.parent = roof.transform;
        }

        roof.transform.parent = container.transform;
    }

    #endregion

    #region Functions Create Power 
    private void CreatePower(List<UnityEntity> listEntities, UnityEntity entity, GameObject wallContainer, GameObject powerContainer, float groundHeight)
    {
        List<Vector3> listPointOfWall = new List<Vector3>();
        Vector3 positionPower = new Vector3();
        Quaternion rotation = Quaternion.Euler(-90f, -90f, 0f);

        string[] powerValue = entity.Coordinates[0].Split(',');
        if (powerValue.Length == 3)
        {
            if (float.TryParse(powerValue[0], out float xPower) && float.TryParse(powerValue[1], out float zPower))
            {
                positionPower.x = xPower;
                positionPower.y = groundHeight;
                positionPower.z = zPower;

                /*foreach (UnityEntity unityEntity in listEntities)
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
                                }
                            }
                        }
                    }
                }*/
            }
        }

        GameObject power = Instantiate(_powerPrefab, positionPower, rotation);
        float customScale = 70.0f;
        power.transform.localScale = _windowPrefab.transform.localScale * customScale;
        //power.transform.localScale += new Vector3(60f, -3f, 10f);
        power.transform.parent = powerContainer.transform;

        BoxCollider boxCollider = power.AddComponent<BoxCollider>();
        boxCollider.size = new Vector3(2.2f, 0.25f, 0.4f);
        boxCollider.center = new Vector3(0, 0.3f, -0.1f);

        //HandleCollision(_listWall, power);
    }

    private void HandleCollision(List<GameObject> listWall, GameObject power)
    {
        // Kiểm tra va chạm hoặc nằm đè vào cube
        BoxCollider powerCollider = power.GetComponent<BoxCollider>();
        if (powerCollider != null)
        {
            foreach (GameObject wall in listWall)
            {
                MeshCollider wallCollider = wall.GetComponent<MeshCollider>();
                if (wallCollider != null)
                {
                    if (powerCollider.bounds.Intersects(wallCollider.bounds))
                    {
                        Debug.Log("AAAAAA");
                        Vector3 newPosition = power.transform.position;
                        newPosition.z += 180f;
                        power.transform.position = newPosition;
                    }
                }
            }
        }

    }
    #endregion

    #region Other Functions

    private object CalculateDimensionAndCenterPoint(List<Vector3> pointList, string dimension)
    {
        if (pointList.Count < 2)
        {
            throw new ArgumentException("Point list must contain at least two points!");
        }

        float maxX = float.MinValue;
        float maxZ = float.MinValue;
        float minX = float.MaxValue;
        float minZ = float.MaxValue;

        foreach (Vector3 point in pointList)
        {
            // Tìm số lớn nhất và bé nhất trên trục x
            maxX = Mathf.Max(maxX, point.x);
            minX = Mathf.Min(minX, point.x);

            // Tìm số lớn nhất và bé nhất trên trục z
            maxZ = Mathf.Max(maxZ, point.z);
            minZ = Mathf.Min(minZ, point.z);
        }

        Vector2 maxPoint = new Vector2(maxX, maxZ); // điểm trên cùng bên phải của wall
        Vector2 minPoint = new Vector2(minX, minZ); // điểm dưới cùng bên trái của wall

        // tính kích thước hình chiếu trên Ox của đoạn thẳng được tạo ra từ maxVector và minVector
        float projectionSizeX = maxPoint.x - minPoint.x;
        // tính kích thước hình chiếu trên Oz của đoạn thẳng được tạo ra từ maxVector và minVector
        float projectionSizeZ = maxPoint.y - minPoint.y;

        // tính trọng tâm của hình (tạo độ trung điểm của đường chéo lớn nhất)
        // (đường chéo lớn nhất được tạo thành từ điểm trên cùng bên phải đến điểm dưới cùng bên trái)
        Vector2 midPoint = (maxPoint + minPoint) / 2;

        // chuyển về vector3
        Vector3 centerPoint = new Vector3(midPoint.x, 0, midPoint.y);

        if (dimension == "length")
        {
            return projectionSizeX > projectionSizeZ ? projectionSizeX : projectionSizeZ;
        }
        else if (dimension == "width")
        {
            return projectionSizeX > projectionSizeZ ? projectionSizeZ : projectionSizeX;
        }
        else if (dimension == "centerCoordinates")
        {
            return centerPoint;
        }
        else
        {
            throw new ArgumentException("Invalid dimension. Use 'length' or 'width' or 'centerCoordinates'!");
        }
    }

    private List<Vector3> AddAllVector3ofEntitiesToList(UnityEntity entity, float yValue)
    {
        List<Vector3> verticesList = new List<Vector3>();

        foreach (string coordinate in entity.Coordinates)
        {
            // Tách các giá trị từ dòng dữ liệu
            string[] values = coordinate.Split(',');

            if (values.Length == 2 || values.Length == 3)
            {
                if (float.TryParse(values[0], out float x) && float.TryParse(values[1], out float z))
                {
                    Vector3 vertex = new Vector3(x, yValue, z);
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

    private void CreateCube(GameObject container, Vector3 startPoint, Vector3 endPoint, float height, float groundHeight, string color)
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

        if (color == "")
        {
            cubeRenderer.material.color = UnityColor.gray;
        }
        else
        {
            SystemColor systemColor = GetColor(color);
            UnityColor resultColor = new Color32(systemColor.R, systemColor.G, systemColor.B, systemColor.A);
            cubeRenderer.material.color = resultColor;
        }
    }

    public SystemColor GetColor(string color)
    {
        SystemColor resultColor = SystemColor.Gray;
        string[] colorValues = color.Split(", ");

        if (colorValues.Length == 1)
        {
            string result = colorValues[0];
            resultColor = SystemColor.FromName(result);
        }
        else if (colorValues.Length == 3)
        {
            int r = int.Parse(colorValues[0]);
            int g = int.Parse(colorValues[1]);
            int b = int.Parse(colorValues[2]);
            resultColor = SystemColor.FromArgb(r, g, b);
        }
        return resultColor;
    }

    #endregion

}