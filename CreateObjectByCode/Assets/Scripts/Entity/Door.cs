using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : UnityEntity
{
    public Door(int? id, string layerName, string objectType, List<string> coordinates) : base(id, layerName, objectType, coordinates)
    {
    }
    public Door(string typeOfUnityEntity) : base(typeOfUnityEntity) { }
}
