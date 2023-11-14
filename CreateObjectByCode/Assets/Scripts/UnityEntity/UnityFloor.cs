using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityFloor : MonoBehaviour
{
    int order;
    List<UnityEntity> listEntities;

    public int Order { get => order; set => order = value; }
    public List<UnityEntity> ListEntities { get => listEntities; set => listEntities = value; }
}
