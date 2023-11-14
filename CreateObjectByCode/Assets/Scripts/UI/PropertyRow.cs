using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PropertyRow : MonoBehaviour
{
    private string _nameFloor;

    private GameObject _floor;

    private int _floorIndex;

    public string NameFloor { get => _nameFloor; set => _nameFloor = value; }
    public GameObject Floor { get => _floor; set => _floor = value; }
    public int FloorIndex { get => _floorIndex; set => _floorIndex = value; }


    private Renderer objectRenderer;
    private Renderer[] childRenderers;

    void Start()
    {
        objectRenderer = _floor.GetComponent<Renderer>();
        childRenderers = _floor.GetComponentsInChildren<Renderer>();
    }

    public void ToggleStatus()
    {
        objectRenderer.enabled = !objectRenderer.enabled;

        foreach (Renderer childRenderer in childRenderers)
        {
            childRenderer.enabled = objectRenderer.enabled;
        }
    }
}
