using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExportArchitectureToJSON : MonoBehaviour
{
    string nameFloor;
    List<UnityEntity> listToExport = new List<UnityEntity>();

    public string NameFloor { get => nameFloor; set => nameFloor = value; }
    public List<UnityEntity> ListToExport { get => listToExport; set => listToExport = value; }

    //public List<UnityEntity> getAllEntity()
    //{
    //    List<UnityEntity> allEntity = new List<UnityEntity>();
    //    foreach (UnityEntity entity in ListToExport)
    //    {
    //        allEntity.Add(entity);
    //    }

    //    return allEntity;
    //}
}
