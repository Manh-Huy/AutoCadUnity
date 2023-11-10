using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using RuntimeHandle;
using Unity.VisualScripting;
using TMPro;

public class SelectTransformGizmo : MonoBehaviour
{
    [SerializeField] private string _selectableTag = "Selectable";
    [SerializeField] private Material _highlightMat;
    [SerializeField] private Material _selectedMat;
     private Material _defaultMat;
    [SerializeField] private Camera _controllerCam;
    [SerializeField] private GameObject _hideText;
    private Transform _selection;

    public bool _isPressed = false;

    void Start() 
    {
        _controllerCam = GameObject.Find("First Person Player").GetComponentInChildren<Camera>();
    }

    void Update()
    {
        
        if (_selection != null)
        {
            Renderer selectedRenderer = _selection.GetComponent<Renderer>();
            selectedRenderer.material = _defaultMat;
            _selection = null;
            _hideText.SetActive(false);
        }

        Ray ray = _controllerCam.ViewportPointToRay(new Vector3(0.5f,0.5f,0));
        RaycastHit hit;
        //Lại Gần Selectable
        if(Physics.Raycast(ray, out hit, 500f))
        {
            Transform selectedGameObject = hit.transform;
            if (selectedGameObject.CompareTag(_selectableTag))
            {
                Renderer selectionRenderer = selectedGameObject.GetComponent<Renderer>();

                //Highlight
                if(selectionRenderer != null  )
                {
                    _defaultMat = selectionRenderer.material;
                    selectionRenderer.material = _highlightMat;
                    _hideText.SetActive(true);

                }
                _selection = selectedGameObject;

        
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && _selection != null)
        {
            _isPressed = true;
            _selection.GetComponent<Renderer>().material = _selectedMat;
            _hideText.SetActive(false);
        }

    }

}
