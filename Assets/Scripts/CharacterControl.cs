using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class CharacterControl : MonoBehaviour
{
    private CharacterController _charCtrl;
    [SerializeField]
    private Transform cameraOrientation;
    
    private bool _freeRunning = true;
    private const float Acceleration = 5f;
    private const float RotationSpeed = 180f;

    private Vector3 _inCombatAccelerations = new Vector3(3f, 0f, 5f);
    // private const float ForwardAcceleration = 5f;
    // private const float SideWayAcceleration = 3f;
    
    private const float SprintSpeedUp = 1.2f;
    private bool _sprint = false;
    
    private const float DragOnGround = 10f;

    private const float OpeningRadius = 1.5f;
    
    private const float JumpHeight = 3f;
    private Vector3 _jumpForce;
    private bool _jump = false;

    private Vector3 _inputVector;
    private Vector3 _velocity;

    private bool _openRequest = false;
    private int _closedContainersLayerMask;
    private void Awake()
    {
        _charCtrl = GetComponent<CharacterController>();
        Assert.IsNotNull(_charCtrl, name + " has no character controller!");
    }

    private void Start()
    {
        _jumpForce = -Physics.gravity.normalized * Mathf.Sqrt(2 * Physics.gravity.magnitude * JumpHeight);
        _closedContainersLayerMask = LayerMask.GetMask("ClosedContainers");
    }

    private void Update()
    {
        _inputVector.x = Input.GetAxis("Horizontal");
        _inputVector.z = Input.GetAxis("Vertical");

        if (Input.GetButtonDown("Jump"))
        {
            _jump = true;
        }
        
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _sprint = true;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            _openRequest = true;
        }
    }

    private void FixedUpdate()
    {
        ApplyGravity();
        ApplyMovement();
        ApplyGroundDrag();
        ApplySpeedLimitation();
        ApplyJump();
        MouseMovement();
        _charCtrl.Move(_velocity * Time.deltaTime);
        CheckForContainers();
    }

    private void CheckForContainers()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, OpeningRadius, _closedContainersLayerMask);
        if (colliders.Length != 0 && _openRequest)
        {
            float minDistance = Mathf.Infinity;
            float distance;
            int closest = -1;
            for (int i = 0; i < colliders.Length; i++)
            {
                distance = Vector3.Distance(colliders[i].transform.position, transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = i;
                }
            }
            colliders[closest].gameObject.GetComponent<Chest>().enabled = true;
        }

        _openRequest = false;
    }

    private void ApplySpeedLimitation()
    {
        float tempY = _velocity.y;
        _velocity.y = 0;
        if (_freeRunning)
            _velocity = Vector3.ClampMagnitude(_velocity, MaxRunningSpeed);
        else
        {
            _velocity.z = Mathf.Clamp(_velocity.z, 0f, MaxForwardRunningSpeed);
            _velocity.x = Mathf.Clamp(_velocity.x, 0, MaxSideWayRunningSpeed);
        }

        _velocity.y = tempY;
    }

    private const float MaxRunningSpeed = (30.0f * 1000) / (60 * 60); // km/h
    private const float MaxForwardRunningSpeed = (30.0f * 1000) / (60 * 60); // km/h
    private const float MaxSideWayRunningSpeed = (20.0f * 1000) / (60 * 60); // km/h

    private void MouseMovement()
    {
        // float horizontal = Input.GetAxis("Mouse X");
        // transform.Rotate(0f, horizontal*RotationSpeed*Time.deltaTime, 0f);
        transform.rotation = cameraOrientation.rotation;
    }

    private void ApplyGroundDrag()
    {
        if (_charCtrl.isGrounded)
        {
            _velocity *= 1 - Time.deltaTime * DragOnGround;
        }
    }
    private void ApplyGravity()
    {
        // Todo: ask if there is a need to do something when grounded in the gravity function
        if (!_charCtrl.isGrounded)
            _velocity += Physics.gravity * Time.deltaTime;
    }

    private void ApplyJump()
    {
        if (_jump && _charCtrl.isGrounded)
        { // TODO: check if jump is straight up or depends on the floors angle (aka characters transform up) 
            _velocity += _jumpForce;
        }

        _jump = false;
    }

    private void ApplyMovement()
    {
        
        if (_sprint) // Apply sprint
            _inputVector *= SprintSpeedUp;
        
        // Todo: change the rotation to the cameras rotation since movement directions...
        // ...depend only on the cameras rotation
        Vector3 v = _charCtrl.transform.rotation * _inputVector;
        
        if (_freeRunning) // Running when out off combat depends on the direction of the movement keys
            _velocity += v * Acceleration;
        else // In combat the character always faces forward 
            _velocity += Vector3.Scale(v, _inCombatAccelerations);

        _sprint = false;
    }
}
