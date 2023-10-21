using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityInfo : MonoBehaviour
{
    private int? id;
    private string layerName;
    private string objectType;
    private List<string> coordinates = null;

    public string LayerName { get => layerName; set => layerName = value; }
    public string ObjectType { get => objectType; set => objectType = value; }
    public List<string> Coordinates { get => coordinates; set => coordinates = value; }
    public int? Id { get => id; set => id = value; }

    public EntityInfo(int? id, string layerName, string objectType, List<string> coordinates)
    {
        this.Id = id;
        this.LayerName = layerName;
        this.ObjectType = objectType;
        this.Coordinates = coordinates;
    }
}
