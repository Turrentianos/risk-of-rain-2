using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class CharacterControl : MonoBehaviour
{
    private CharacterController _charCtrl;
    private const float ForwardSpeed = 5f;
    private const float RotationSpeed = 180f;
    private const float SidewaySpeed = 3f;
    private const float SprintSpeed = 1.2f;
    private const float JumpSpeed = 5f;
    private Vector3 _velocity;

    private void Awake()
    {
        _charCtrl = GetComponent<CharacterController>();
        Assert.IsNotNull(_charCtrl, name + " has no character controller!");
    }
    
    private void Update()
    {
        
        ArrowMovement();
        Sprint();
        Jump();
        ApplyGravity();
        _charCtrl.Move(_velocity);
        MouseMouvement();
    }

    private void MouseMouvement()
    {
        float horizontal = Input.GetAxis("Mouse X");
        transform.Rotate(0f, horizontal*RotationSpeed*Time.deltaTime, 0f);
        float vertical = Input.GetAxis("Mouse Y");
    }

    private void ApplyGravity()
    {
        if (!_charCtrl.isGrounded && !Input.GetKey(KeyCode.Space))
            _velocity += Physics.gravity*Time.deltaTime;
    }

    private void Sprint()
    {
        if (Input.GetKey(KeyCode.LeftShift))
            _velocity *= SprintSpeed;
    }

    private void Jump()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            _velocity += Vector3.up * (JumpSpeed * Time.deltaTime);
        }
    }

    private void ArrowMovement()
    {
        float sideway = Input.GetAxis("Horizontal");
        float forwardOrBackward = Input.GetAxis("Vertical");

        var selfTransform = transform;
        _velocity = (
            selfTransform.forward * (forwardOrBackward * ForwardSpeed * Time.deltaTime)+
            selfTransform.right * (sideway * SidewaySpeed * Time.deltaTime)
            );
    }

    private bool NotZero(float value)
    {
        return Mathf.Approximately(value, 0f);
    }
}
