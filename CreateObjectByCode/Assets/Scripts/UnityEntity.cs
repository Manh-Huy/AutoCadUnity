using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityEntity : MonoBehaviour
{
    private int? id;
    private string layerName;
    private string objectType;
    private List<string> coordinates = null;
    string typeOfUnityEntity;
    string color;
    float height;

    public UnityEntity()
    {
    }

    public UnityEntity(int? id, string layerName, string objectType, List<string> coordinates)
    {
    }

    public UnityEntity(string _typeOfUnityEntity)
    {
        this.typeOfUnityEntity = _typeOfUnityEntity;
    }

    public int? Id { get => id; set => id = value; }
    public string LayerName { get => layerName; set => layerName = value; }
    public string ObjectType { get => objectType; set => objectType = value; }
    public List<string> Coordinates { get => coordinates; set => coordinates = value; }
    public string TypeOfUnityEntity { get => typeOfUnityEntity; set => typeOfUnityEntity = value; }
    public string Color { get => color; set => color = value; }
    public float Height { get => height; set => height = value; }
}
