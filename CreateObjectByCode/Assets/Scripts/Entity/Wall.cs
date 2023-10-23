using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : UnityEntity
{
    int doday;

    public int Doday { get => doday; set => doday = value; }

    public Wall(int? id, string layerName, string objectType, List<string> coordinates) : base(id, layerName, objectType, coordinates)
    {
    }

    public Wall(string typeOfUnityEntity) : base(typeOfUnityEntity) { }
}
