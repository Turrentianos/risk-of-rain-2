using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CharacterControl : MonoBehaviour
{
    private CharacterController _charCtrl;
    private const float ForwardSpeed = 5f;
    private const float SidewaySpeed = 3f;
    private const float SprintSpeed = 1.2f;
    private const float JumpSpeed = 5f;
    private Vector3 _velocity;

    private void Awake()
    {
        _charCtrl = GetComponent<CharacterController>();
        Assert.IsNotNull(_charCtrl, name + " has no character controller!");
    }
    
    private void Start()
    {
        
    }
    
    private void Update()
    {
        UpdateVelocity();
        UpdateSprint();
        UpdateJump();
        ApplyGravity();
        _charCtrl.Move(_velocity);
    }

    private void ApplyGravity()
    {
        if (!_charCtrl.isGrounded && !Input.GetKey(KeyCode.Space))
            _velocity += Physics.gravity*Time.deltaTime;
    }

    private void UpdateSprint()
    {
        if (Input.GetKey(KeyCode.LeftShift))
            _velocity *= SprintSpeed;
    }

    private void UpdateJump()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            _velocity += Vector3.up * JumpSpeed * Time.deltaTime;
        }
    }

    private void UpdateVelocity()
    {
        float sideway = Input.GetAxis("Horizontal");
        float forwardOrBackward = Input.GetAxis("Vertical");
        
        _velocity = (
            transform.forward * forwardOrBackward * ForwardSpeed * Time.deltaTime+
            transform.right * sideway * SidewaySpeed * Time.deltaTime
            );
    }

    private bool NotZero(float value)
    {
        return Mathf.Approximately(value, 0f);
    }
}
