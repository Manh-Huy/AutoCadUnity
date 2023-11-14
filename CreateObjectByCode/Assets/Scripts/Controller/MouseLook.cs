using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float _mouseSentivity = 200f;
    public Transform _playerBody;
    public float _xRotation = 0f;

    public bool _isMouseLocked;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _isMouseLocked = true;
    }

    // Update is called once per frame
    void Update()
    {

        // Translation
        /*        float horizontalInput = Input.GetAxis("Horizontal");
                float verticalInput = Input.GetAxis("Vertical");

                Vector3 translation = new Vector3(horizontalInput, 0, verticalInput) * _moveSpeed * Time.deltaTime;
                transform.Translate(translation);*/

        // Mouse Rotation
        float mouseX = Input.GetAxis("Mouse X") * _mouseSentivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSentivity * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        _playerBody.Rotate(Vector3.up * mouseX);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isMouseLocked == true)
            {
                Cursor.lockState = CursorLockMode.None;
                _isMouseLocked = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                _isMouseLocked = true;
            }
        }

    }
}
