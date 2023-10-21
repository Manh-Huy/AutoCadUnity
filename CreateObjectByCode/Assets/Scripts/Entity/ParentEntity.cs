using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentEntity : MonoBehaviour
{
    string parentLayerName;
    string parentObjectType;
    string architectureEntityName;
    List<EntityInfo> entityInfos = new List<EntityInfo>();


    public List<EntityInfo> EntityInfos { get => entityInfos; set => entityInfos = value; }
    public string ParentLayerName { get => parentLayerName; set => parentLayerName = value; }
    public string ParentObjectType { get => parentObjectType; set => parentObjectType = value; }
    public string ArchitectureEntityName { get => architectureEntityName; set => architectureEntityName = value; }
}
