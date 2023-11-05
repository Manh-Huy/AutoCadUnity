using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController _controller;

    public float _speed = 12f;
    public float gravity = -9.81f;

    Vector3 _velocity;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        _controller.Move(move * _speed * Time.deltaTime);

        _velocity.y += gravity * Time.deltaTime;
        if(Input.GetKey(KeyCode.Space))
        {
            _controller.Move(-_velocity * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.C))
        {
            _controller.Move(_velocity * Time.deltaTime);
        }
    }
}
