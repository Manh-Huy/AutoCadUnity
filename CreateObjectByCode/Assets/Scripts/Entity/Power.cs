using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Power : UnityEntity
{
    public Power(int? id, string layerName, string objectType, List<string> coordinates) : base(id, layerName, objectType, coordinates)
    {
    }

    public Power(string typeOfUnityEntity) : base(typeOfUnityEntity) { }
}
