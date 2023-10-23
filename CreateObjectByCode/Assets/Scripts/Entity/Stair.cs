using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stair : UnityEntity
{
    public Stair(int? id, string layerName, string objectType, List<string> coordinates) : base(id, layerName, objectType, coordinates)
    {
    }

    public Stair(string typeOfUnityEntity) : base(typeOfUnityEntity) { }
}
