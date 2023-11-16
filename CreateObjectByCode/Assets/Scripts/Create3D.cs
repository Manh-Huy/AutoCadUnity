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

    public List<PropertyRow> _propertyRowList = new List<PropertyRow>();
    public bool _isCreateDone = false;


    private void Start()
    {
        ReadJSON();
        CreateAllEntities();
        _isCreateDone = true;
    }
    public void ReadJSON()
    {
        //string jsonPath = "Build\\PlaneFloor1.json";
        //string jsonPath = "Build\\house2.json";
        string jsonPath = "F:\\Desktop\\house2.json";

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
        float groundHeight = 20f;
        float planeHeight = 5f;

        List<Vector3> coordinatesLastFloorOfWallList = new List<Vector3>();

        foreach (UnityFloor floor in _listFloor)
        {
            GameObject bottomPlaneContainer = new GameObject("Bottom Plane Container");
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

            {
                List<Vector2> listPoint = new List<Vector2>();

                foreach (UnityEntity entity in floor.ListEntities)
                {
                    if (entity.TypeOfUnityEntity == "Wall" && (entity.ObjectType == "LwPolyline" || entity.ObjectType == "Line"))
                    {
                        foreach (string coordinate in entity.Coordinates)
                        {
                            string[] pointValues = coordinate.Split(',');
                            if (pointValues.Length == 2)
                            {
                                if (float.TryParse(pointValues[0], out float xWall) && float.TryParse(pointValues[1], out float zWall))
                                {
                                    listPoint.Add(new Vector2(xWall, zWall));
                                }
                            }
                        }
                    }
                }

                List<Vector2> mainPlane = new List<Vector2>();
                Vector2 maxPoint = FindPointMaxOfPlane(listPoint);
                mainPlane.Add(maxPoint);
                mainPlane = FindOtherPointOfPlane(listPoint, mainPlane);

                float width = CalculateDistance(mainPlane[0].x, mainPlane[0].y, mainPlane[1].x, mainPlane[1].y);
                float length = CalculateDistance(mainPlane[1].x, mainPlane[1].y, mainPlane[2].x, mainPlane[2].y);

                GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mainPlane = ModifyDataInPoint(mainPlane);
                Vector3 centerPosition = CalculateCenterPositionEachPlane(mainPlane, groundHeight);
                plane.transform.localPosition = centerPosition;
                plane.transform.localScale = new Vector3(length, planeHeight, width);
                // Chuyển cube sang Layer 3D (0 là mặc định)
                plane.layer = 0;

                Renderer cubeRenderer = plane.GetComponent<Renderer>();
                cubeRenderer.material.color = UnityColor.gray;
                plane.transform.parent = bottomPlaneContainer.transform;
                Renderer rendererbottomPlaneContainer = bottomPlaneContainer.AddComponent<MeshRenderer>();

                listPoint = DeletePointInMainPlane(listPoint, mainPlane);

                float topLimit = mainPlane[0].y;
                float bottomLimit = mainPlane[1].y;
                float leftLimit = mainPlane[2].x;
                float rightLimit = mainPlane[0].x;

                #region Bottom Plane
                List<Vector2> pointBottomPlane = new List<Vector2>();
                foreach (Vector2 point in listPoint)
                {
                    if (point.y < bottomLimit)
                    {
                        pointBottomPlane.Add(point);
                    }
                }

                pointBottomPlane = CleanListTopAndBottomToCreatePlane(pointBottomPlane);

                if (pointBottomPlane.Count > 0)
                {
                    for (int i = 0; i < pointBottomPlane.Count - 1; i++)
                    {
                        if (pointBottomPlane[i].y == pointBottomPlane[i + 1].y)
                        {
                            CreateBottomPlane(bottomLimit, bottomPlaneContainer, pointBottomPlane[i], pointBottomPlane[i + 1], groundHeight);
                        }
                    }
                }
                #endregion

                #region Left Plane 
                // cái left này có bug khi bước nhảy là 1 như top, bottom
                List<Vector2> pointLeftPlane = new List<Vector2>();
                foreach (Vector2 point in listPoint)
                {
                    if (point.x < leftLimit)
                    {
                        pointLeftPlane.Add(point);
                    }
                }

                if (pointLeftPlane.Count > 0)
                {
                    for (int i = 0; i < pointBottomPlane.Count; i += 2)
                    {
                        CreateLeftPlane(leftLimit, bottomPlaneContainer, pointLeftPlane[i], pointLeftPlane[i + 1], groundHeight);
                    }
                }
                #endregion

                #region Top Plane 
                List<Vector2> pointTopPlane = new List<Vector2>();
                foreach (Vector2 point in listPoint)
                {
                    if (point.y > topLimit)
                    {
                        pointTopPlane.Add(point);
                    }
                }

                pointTopPlane = CleanListTopAndBottomToCreatePlane(pointTopPlane);

                if (pointTopPlane.Count > 0)
                {
                    for (int i = 0; i < pointTopPlane.Count - 1; i++)
                    {
                        if (pointTopPlane[i].y == pointTopPlane[i + 1].y)
                        {
                            CreateTopPlane(topLimit, bottomPlaneContainer, pointTopPlane[i], pointTopPlane[i + 1], groundHeight);
                        }
                    }
                }
                #endregion
            }

            foreach (UnityEntity entity in floor.ListEntities)
            {
                //float entityHeight = entity.Height;

                //if (entityHeight > floorHeight)
                //{
                //    // floorHeight là chiều cao lớn nhất trong tầng đó (bthg là chiều cao của tường lớn nhất)
                //    floorHeight = entityHeight;
                //}

                // floorHeight += planeHeight
                floorHeight = 100f + planeHeight; // thay thế các dòng comment trên (set cứng)
                float wallHeight = 100f; // thay thế dòng trên (set cứng)

                if (entity.TypeOfUnityEntity == "Wall" && (entity.ObjectType == "LwPolyline" || entity.ObjectType == "Line"))
                {
                    //float wallHeight = entityHeight;

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
                    CreateDoor(floor.ListEntities, entity, doorContainer, groundHeight, wallHeight);
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

            // create PlaneTop
            GameObject topPlaneContainer = Instantiate(bottomPlaneContainer);
            topPlaneContainer.name = "Top Plane Container";
            Vector3 currentPosition = topPlaneContainer.transform.localPosition;
            topPlaneContainer.transform.localPosition = new(currentPosition.x, floorHeight - planeHeight, currentPosition.z);
            

            // create Stair
            if (verticeStairsList.Count > 0)
            {
                stairPosition = (Vector3)CalculateDimensionAndCenterPoint(verticeStairsList, "centerCoordinates");
                stairLength = (float)CalculateDimensionAndCenterPoint(verticeStairsList, "length");
                stairWidth = (float)CalculateDimensionAndCenterPoint(verticeStairsList, "width") / 2f;
                CreateStair(stairPosition, stairLength, stairWidth, stairContainer, floorHeight, groundHeight);

                // Đục lỗ cầu thang (tạo cube cho tầng trên để biết tầng dưới nó có cầu thang)
                GameObject identifyLocationStair = GameObject.CreatePrimitive(PrimitiveType.Cube);
                identifyLocationStair.name = "Identify Stair";
                float scaleYidentifyLocationStair = 2f;

                identifyLocationStair.transform.localScale = new Vector3(stairLength, scaleYidentifyLocationStair, stairWidth);
                identifyLocationStair.transform.position = new Vector3(stairPosition.x, (groundHeight + floorHeight), stairPosition.z);

                Renderer identifyLocationStairRenderer = identifyLocationStair.GetComponent<Renderer>();
                identifyLocationStairRenderer.material.color = UnityColor.green;

                identifyLocationStairRenderer.transform.parent = stairContainer.transform;
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
            bottomPlaneContainer.transform.parent = floorContainer.transform;
            wallContainer.transform.parent = floorContainer.transform;
            doorContainer.transform.parent = floorContainer.transform;
            stairContainer.transform.parent = floorContainer.transform;
            windowContainer.transform.parent = floorContainer.transform;
            powerContainer.transform.parent = floorContainer.transform;
            topPlaneContainer.transform.parent = floorContainer.transform;

            // Vì roof là object con của tầng cuối cùng nên tạo CreateRoof tại đây
            // create Roof
            if (coordinatesLastFloorOfWallList.Count > 0)
            {
                Vector3 roofPosition = (Vector3)CalculateDimensionAndCenterPoint(coordinatesLastFloorOfWallList, "centerCoordinates");
                roofPosition.y -= planeHeight;
                float roofLength = (float)CalculateDimensionAndCenterPoint(coordinatesLastFloorOfWallList, "length"); // chiều dài của roof
                float roofWidth = (float)CalculateDimensionAndCenterPoint(coordinatesLastFloorOfWallList, "width"); // chiều rộng của roof
                // đang set mặc định chiều cao của rooftop =  75% (trung bình cộng chiều cao các tầng ngôi nhà)
                // vị trí y (chiều cao đặt roof) = groundHeight + floorHeight vì nó nằm trên tầng cuối cùng
                CreateRoof(floorContainer, roofPosition, roofLength, roofWidth, ((groundHeight + floorHeight) / _listFloor.Count) * 0.75f, (groundHeight + floorHeight));
            }

            floorContainer.transform.position = _centerPoint; //B4

            // đưa floorContainer vào object cha (House) và đưa nó vào vị trí trong camera cho dễ nhìn
            floorContainer.transform.parent = _houseObject.transform;
            _houseObject.transform.position = _camera.transform.position;

            // Ẩn tầng
            Renderer renderer = floorContainer.AddComponent<MeshRenderer>();
            PropertyRow propertyRow = new PropertyRow();
            propertyRow.NameFloor = "Floor " + (floorIndex + 1);
            propertyRow.Floor = floorContainer;
            _propertyRowList.Add(propertyRow);

            if (floorIndex == _listFloor.Count - 1)
            {
                Transform floorContainerTransform = floorContainer.transform;
                Transform roofTransform = floorContainerTransform.Find("Roof");
                if (roofTransform != null)
                {
                    GameObject roofObject = roofTransform.gameObject;

                    // Đặt đối tượng Roof ra cùng cấp với floorContainer
                    roofObject.transform.parent = _houseObject.transform;

                    PropertyRow propertyRowRoof = new PropertyRow();
                    propertyRowRoof.NameFloor = "Roof";
                    propertyRowRoof.Floor = roofObject;
                    _propertyRowList.Add(propertyRowRoof);
                }
                else
                {
                    Debug.LogError("Không tìm thấy roof!");
                }
            }

            floorIndex++;
            // cộng với chiều cao tầng này để bắt đầu dựng tầng sau
            groundHeight += floorHeight;
        }
    }

    #region Functions Create Plane

    private Vector2 FindPointMaxOfPlane(List<Vector2> listPoint)
    {
        bool isFind = false;
        bool dontFoundInZ = false;
        bool dontFoundInX = true;
        float maxX = float.MinValue;
        float maxZ = float.MinValue;
        float xTop = float.MaxValue;
        float zTop = float.MaxValue;
        Vector2 maxPoint = new Vector2(maxX, maxZ);

        // điểm này là điêm giả tưởng có thể nó sẽ không tồn tại trong bản vẽ
        // , dùng nó để khi duyệt Z không được, ta sẽ back lại và duyệt X
        Vector2 maximumPoint = new Vector2(float.MinValue, float.MinValue);

        foreach (Vector2 point in listPoint)
        {
            // Tìm số lớn nhất và bé nhất trên trục x
            maximumPoint.x = Mathf.Max(maximumPoint.x, point.x);

            // Tìm số lớn nhất và bé nhất trên trục z
            maximumPoint.y = Mathf.Max(maximumPoint.y, point.y);
        }

        while (isFind == false)
        {
            maxX = float.MinValue;
            maxZ = float.MinValue;
            int countFor1 = 0;
            int countFor2 = 0;
            foreach (Vector2 point in listPoint)
            {
                if (point.x > maxX && point.x < xTop) maxX = point.x;
                if (point.y > maxZ && point.y < zTop) maxZ = point.y;
                countFor1++;
            }
            maxPoint = new Vector2(maxX, maxZ);
            if (dontFoundInZ == false)
            {
                foreach (Vector2 point in listPoint)
                {
                    countFor2++;
                    if (maxPoint == point)
                    {
                        return maxPoint;
                    }
                }
                if (maxPoint.y == float.MinValue)
                {
                    dontFoundInZ = true;
                    dontFoundInX = false;
                    zTop = maximumPoint.y;
                }
                else
                {
                    zTop = maxPoint.y;
                }
            }
            if (dontFoundInX == false)
            {
                foreach (Vector2 point in listPoint)
                {
                    countFor2++;
                    if (maxPoint == point)
                    {
                        return maxPoint;
                    }
                }
                if (maxPoint.x == float.MinValue)
                {
                    return new Vector2(0f, 0f);
                }
                else
                {
                    xTop = maxPoint.x;
                }
            }


        }
        return maxPoint;
    }

    private List<Vector2> FindOtherPointOfPlane(List<Vector2> listPoint, List<Vector2> mainPlane)
    {
        int countPointMainPlane = 1;

        if (countPointMainPlane == 1)
        {
            Vector2 currentPoint = mainPlane[mainPlane.Count - 1];
            foreach (Vector2 point in listPoint)
            {
                if (point.x == currentPoint.x && point.y < currentPoint.y)
                {
                    currentPoint = point;
                }
            }
            mainPlane.Add(currentPoint);
            countPointMainPlane++;
        }

        if (countPointMainPlane == 2)
        {
            Vector2 currentPoint = mainPlane[mainPlane.Count - 1];
            foreach (Vector2 point in listPoint)
            {
                if (point.y == currentPoint.y && point.x < currentPoint.x)
                {
                    currentPoint = point;
                }
            }
            mainPlane.Add(currentPoint);
            countPointMainPlane++;
        }

        if (countPointMainPlane == 3)
        {
            Vector2 currentPoint = mainPlane[mainPlane.Count - 1];
            foreach (Vector2 point in listPoint)
            {
                if (point.x == currentPoint.x && point.y > currentPoint.y)
                {
                    currentPoint = point;
                }
            }
            if (currentPoint.y != mainPlane[0].y)
            {
                currentPoint.y = mainPlane[0].y;
            }
            mainPlane.Add(currentPoint);
            countPointMainPlane++;
        }

        return mainPlane;
    }

    private Vector3 CalculateCenterPositionEachPlane(List<Vector2> mainPlane, float height)
    {


        float xPosition = (mainPlane[0].x + mainPlane[1].x + mainPlane[2].x + mainPlane[3].x) / 4;
        float zPosition = (mainPlane[0].y + mainPlane[1].y + mainPlane[2].y + mainPlane[3].y) / 4;

        return new Vector3(xPosition, height, zPosition);
    }

    private List<Vector2> ModifyDataInPoint(List<Vector2> mainPlane)
    {
        List<Vector2> newList = new List<Vector2>();
        foreach (Vector2 point in mainPlane)
        {
            float xValue = (float)Math.Round(point.x, 2);
            float yValue = (float)Math.Round(point.y, 2);
            newList.Add(new Vector2(xValue, yValue));
        }

        return newList;
    }

    private List<Vector2> DeletePointInMainPlane(List<Vector2> listPoint, List<Vector2> mainPlane)
    {
        List<Vector2> newList = new List<Vector2>();
        foreach (Vector2 point in listPoint)
        {
            if (CheckPointInsideRectangle(point, mainPlane))
            {
            }
            else
            {
                newList.Add(point);
            }
        }
        return newList;
    }

    private bool CheckPointInsideRectangle(Vector2 point, List<Vector2> mainPlane)
    {
        // Kiểm tra xem điểm có nằm trong hình chữ nhật hay không bằng cách so sánh giá trị x và y
        float minX = Math.Min(Math.Min(mainPlane[0].x, mainPlane[1].x), Math.Min(mainPlane[2].x, mainPlane[3].x));
        float maxX = Math.Max(Math.Max(mainPlane[0].x, mainPlane[1].x), Math.Max(mainPlane[2].x, mainPlane[3].x));
        float minY = Math.Min(Math.Min(mainPlane[0].y, mainPlane[1].y), Math.Min(mainPlane[2].y, mainPlane[3].y));
        float maxY = Math.Max(Math.Max(mainPlane[0].y, mainPlane[1].y), Math.Max(mainPlane[2].y, mainPlane[3].y));

        bool isInsideX = (point.x >= minX && point.x <= maxX);
        bool isInsideY = (point.y >= minY && point.y <= maxY);

        return isInsideX && isInsideY;
    }

    private List<Vector2> CleanListTopAndBottomToCreatePlane(List<Vector2> listPoint)
    {
        // List này nhận các cặp với nhau
        List<Vector2> listEven = new List<Vector2>();
        // List này nhận các phần tử nằm riêng lẻ
        List<Vector2> listOdd = new List<Vector2>();


        List<Vector2> result = new List<Vector2>();

        for (int i = 0; i < listPoint.Count - 1; i++)
        {
            if (listPoint[i].y == listPoint[i + 1].y)
            {
                listEven.Add(listPoint[i]);
                listEven.Add(listPoint[i + 1]);
                i++;
            }
            else
            {
                listOdd.Add(listPoint[i]);
            }
        }

        result.AddRange(listEven);
        result.AddRange(listOdd);

        result.Sort((p1, p2) =>
        {
            int compareY = p1.y.CompareTo(p2.y);
            if (compareY == 0)
            {
                return p1.x.CompareTo(p2.x);
            }
            return compareY;
        });

        return result;
    }

    private void CreateBottomPlane(float limit, GameObject bottomPlaneContainer, Vector2 point1, Vector2 point2, float groundHeight)
    {
        List<Vector2> bottomPlane = new List<Vector2>();
        Vector2 point4 = new Vector2(point1.x, limit);
        Vector2 point3 = new Vector2(point2.x, limit);

        bottomPlane.Add(point1);
        bottomPlane.Add(point2);
        bottomPlane.Add(point3);
        bottomPlane.Add(point4);

        float width = CalculateDistance(point1.x, point1.y, point4.x, point4.y);
        float length = CalculateDistance(point1.x, point1.y, point2.x, point2.y);

        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Vector3 centerPosition = CalculateCenterPositionEachPlane(bottomPlane, groundHeight);
        plane.transform.localPosition = centerPosition;
        plane.transform.localScale = new Vector3(length, 5f, width);


        // Chuyển cube sang Layer 3D (0 là mặc định)
        plane.layer = 0;

        Renderer cubeRenderer = plane.GetComponent<Renderer>();
        cubeRenderer.material.color = UnityColor.gray;
        plane.transform.parent = bottomPlaneContainer.transform;
    }

    private void CreateLeftPlane(float limit, GameObject bottomPlaneContainer, Vector2 point1, Vector2 point2, float groundHeight)
    {
        List<Vector2> leftPlane = new List<Vector2>();
        Vector2 point4 = new Vector2(limit, point1.y);
        Vector2 point3 = new Vector2(limit, point2.y);

        leftPlane.Add(point1);
        leftPlane.Add(point2);
        leftPlane.Add(point3);
        leftPlane.Add(point4);

        float width = CalculateDistance(point1.x, point1.y, point4.x, point4.y);
        float length = CalculateDistance(point1.x, point1.y, point2.x, point2.y);

        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Vector3 centerPosition = CalculateCenterPositionEachPlane(leftPlane, groundHeight);
        plane.transform.localPosition = centerPosition;
        plane.transform.localScale = new Vector3(width, 5f, length);


        // Chuyển cube sang Layer 3D (0 là mặc định)
        plane.layer = 0;

        Renderer cubeRenderer = plane.GetComponent<Renderer>();
        cubeRenderer.material.color = UnityColor.gray;
        plane.transform.parent = bottomPlaneContainer.transform;
    }

    private void CreateTopPlane(float limit, GameObject bottomPlaneContainer, Vector2 point1, Vector2 point2, float groundHeight)
    {
        List<Vector2> topPlane = new List<Vector2>();
        Vector2 point4 = new Vector2(point1.x, limit);
        Vector2 point3 = new Vector2(point2.x, limit);

        topPlane.Add(point1);
        topPlane.Add(point2);
        topPlane.Add(point3);
        topPlane.Add(point4);

        float width = CalculateDistance(point1.x, point1.y, point4.x, point4.y);
        float length = CalculateDistance(point1.x, point1.y, point2.x, point2.y);

        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Vector3 centerPosition = CalculateCenterPositionEachPlane(topPlane, groundHeight);
        plane.transform.localPosition = centerPosition;
        plane.transform.localScale = new Vector3(length, 5f, width);


        // Chuyển cube sang Layer 3D (0 là mặc định)
        plane.layer = 0;

        Renderer cubeRenderer = plane.GetComponent<Renderer>();
        cubeRenderer.material.color = UnityColor.gray;
        plane.transform.parent = bottomPlaneContainer.transform;
    }

    #endregion

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
                if (verticesList.Count > 2)
                {
                    startPoint = verticesList[i];
                    endPoint = verticesList[0];
                }
                else
                {
                    break;
                }
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

    private void CreateDoor(List<UnityEntity> listEntities, UnityEntity entity, GameObject doorContainer, float groundheight, float wallHeight)
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
        Vector3 currentScale = door.transform.localScale;
        door.transform.localScale = new Vector3(currentScale.x * 50f, distanceResult * 50f, wallHeight * 50f);
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
        Quaternion rotation = Quaternion.identity;

        if (stairLength > stairWidth) // cầu thang nằm ngang
        {
            rotation = Quaternion.Euler(0, 90f, 0f);

        }
        else if (stairLength < stairWidth) // cầu thang nằm dọc
        {
            rotation = Quaternion.Euler(0, 180f, 0f);
        }
        //Quaternion rotation = Quaternion.Euler(0, 90f, 0f);
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
        roof.transform.localScale = new Vector3(roofLength, roofWidth, thicknessOfbottomRoof);
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

            roofTop.transform.localScale = new Vector3(roofLength * scaleXYRatio, roofWidth * scaleXYRatio, roofHeight * scaleZRatio);
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
            //return projectionSizeX > projectionSizeZ ? projectionSizeX : projectionSizeZ;
            return projectionSizeX;
        }
        else if (dimension == "width")
        {
            //return projectionSizeX > projectionSizeZ ? projectionSizeZ : projectionSizeX;
            return projectionSizeZ;
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