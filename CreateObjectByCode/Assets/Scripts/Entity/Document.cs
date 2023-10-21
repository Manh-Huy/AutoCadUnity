using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Document : MonoBehaviour
{
    private List<ParentEntity> parentEntity = new List<ParentEntity>();


    public List<ParentEntity> ParentEntity { get => parentEntity; set => parentEntity = value; }

    public List<EntityInfo> getAllEntity()
    {
        List<EntityInfo> allEntity = new List<EntityInfo>();
        foreach (ParentEntity parentEntity in ParentEntity)
        {
            foreach (EntityInfo childEntity in parentEntity.EntityInfos)
            {
                allEntity.Add(childEntity);
            }
        }

        return allEntity;
    }
}
