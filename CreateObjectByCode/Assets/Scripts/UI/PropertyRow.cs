using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PropertyRow : MonoBehaviour
{
    private string _nameFloor;

    private GameObject _floor;

    public string NameFloor { get => _nameFloor; set => _nameFloor = value; }
    public GameObject Floor { get => _floor; set => _floor = value; }

}
