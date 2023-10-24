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
                _listToExport = JsonConvert.DeserializeObject <List<ExportArchitectureToJSON>>(json);

                foreach (ExportArchitectureToJSON exportArchitecture in _listToExport)
                {
                    entitiyText.text += exportArchitecture.NameFloor + "\n";

                    foreach (UnityEntity entities in exportArchitecture.ListToExport)
                    {
                        entitiyText.text += entities.LayerName + "\n";
                    }
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

    // create wall-lwpolyline and insert entitiy-Door
    public void CreateAllEntities()
    {
        GameObject cubeContainer = new GameObject("CubeContainer");

        foreach (ExportArchitectureToJSON floor in _listToExport)
        {
            foreach (UnityEntity entities in floor.ListToExport)
            {
                if (entities.TypeOfUnityEntity == "Wall" && entities.ObjectType == "LwPolyline")
                {
                    List<Vector3> verticesList = new List<Vector3>();

                    foreach (string coordinate in entities.Coordinates)
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

                if (entities.ObjectType == "Insert" && entities.TypeOfUnityEntity == "Door")
                {
                    string[] values = entities.Coordinates[0].Split(',');
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
