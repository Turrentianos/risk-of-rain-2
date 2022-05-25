using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class CharacterControl : MonoBehaviour
{
    private CharacterController _charCtrl;
    [SerializeField]
    private Transform cameraOrientation;
    
    private bool _freeRunning = true;
    private const float TimeToGoOutOfCombat = 0.5f;
    private float _lastAction;
    private bool _outOfCombat = true;
    private const float Acceleration = 5f;
    private const float RotationSpeed = 180f;

    private readonly Vector3 _inCombatAccelerations = new Vector3(3f, 1f, 5f);
    // private const float ForwardAcceleration = 5f;
    // private const float SideWayAcceleration = 3f;
    
    private const float SprintSpeedUp = 2f;
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
    
    private bool _mouse0 = false;
    private bool _mouse2 = false;
    private bool _defensiveAbility = false;
    private bool _ultimate = false;
    private Vector3 _lookDirection;
    [SerializeField]
    private BanditController _banditController;

    private Camera _camera;
    private void Start()
    {
        _camera = Camera.main;
        _charCtrl = GetComponent<CharacterController>();
        Assert.IsNotNull(_charCtrl, name + " has no character controller!");
        _jumpForce = -Physics.gravity.normalized * Mathf.Sqrt(2 * Physics.gravity.magnitude * JumpHeight);
        _closedContainersLayerMask = LayerMask.GetMask("ClosedContainers");
    }

    private void Update()
    {
        _inputVector.x = Input.GetAxis("Horizontal");
        _inputVector.z = Input.GetAxis("Vertical");
        
        // Use or operator to not set to false if a value is true (aka fixedUpdate has not been called yet) 
        _jump |= Input.GetButtonDown("Jump");
        _sprint = Input.GetKey(KeyCode.LeftShift);
        _openRequest |= Input.GetKeyDown(KeyCode.E);
        _mouse0 |= Input.GetKeyDown(KeyCode.Mouse0); // Left mouse click
        _mouse2 |= Input.GetKeyDown(KeyCode.Mouse1); // Right mouse click
        _ultimate |= Input.GetKeyDown(KeyCode.R);
        _defensiveAbility |= Input.GetKeyDown(KeyCode.LeftControl);
        
        if (_mouse0 | _mouse2 | _ultimate | _defensiveAbility)
        {
            _outOfCombat = false;
            _lastAction = Time.time;
        } else if (!_outOfCombat && Time.time - _lastAction > TimeToGoOutOfCombat)
        {
            _outOfCombat = true;
        }
        SetCameraFieldOfView();
    }

    private void FixedUpdate()
    {
        ApplyGravity();
        ApplyMovement();
        ApplyGroundDrag();
        ApplySpeedLimitation();
        ApplyJump();
        MouseMovement();
        CallAbilities();
        _charCtrl.Move(_velocity * Time.deltaTime);
        CheckForContainers();
    }

    private void CallAbilities()
    {
        if (_mouse0)
        {
            _banditController.Mouse0();
            _mouse0 = false;
        }
        
        if (_mouse2)
        {
            _banditController.Mouse2();
            _mouse2 = false;
        }

        if (_ultimate)
        {
            _banditController.Ultimate();
            _ultimate = false;
        }
        
        if (_defensiveAbility)
        {
            _banditController.DefensiveAbility();
            _defensiveAbility = false;
        }
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

    private const float MaxRunningSpeed = (25.0f * 1000) / (60 * 60); // km/h
    private const float MaxForwardRunningSpeed = (25.0f * 1000) / (60 * 60); // km/h
    private const float MaxSideWayRunningSpeed = (20.0f * 1000) / (60 * 60); // km/h

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

        if (_banditController.InvisibleChange)
        {
            _velocity.y = _banditController.InvisibilityBump.y;
            _banditController.InvisibleChange = false;
        }
        _jump = false;
    }

    private const float SprintFieldOfView = 60f;
    private const float NormalFieldOfView = 45f;
    private bool _transitioning;
    private void SetCameraFieldOfView()
    {
        float newFov = _sprint ? SprintFieldOfView : NormalFieldOfView;
        
        if (!_transitioning && !Mathf.Approximately(newFov, _camera.fieldOfView))
            StartCoroutine(ChangeFieldOfView(_camera.fieldOfView, newFov, 0.5f));
    }

    private IEnumerator ChangeFieldOfView(float previousFov, float newFov, float duration)
    {
        _transitioning = true;
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            _camera.fieldOfView = Mathf.Lerp(previousFov, newFov, time / duration);
            yield return null;
        }

        _camera.fieldOfView = newFov;
        yield return new WaitForSeconds(0.5f);
        _transitioning = false;
    }
    private void ApplyMovement()
    {
        if (_charCtrl.isGrounded) // Character cannot change direction if he has already jumped
        {
            if (_sprint) // Apply sprint
            {
                _inputVector *= SprintSpeedUp;
            }
            // Todo: change the rotation to the cameras rotation since movement directions...
            // ...depend only on the cameras rotation
            Vector3 v = _charCtrl.transform.rotation * _inputVector;
            if (_freeRunning) // Running when out off combat depends on the direction of the movement keys
                _velocity += v * Acceleration;
            else // In combat the character always faces forward 
                _velocity += Vector3.Scale(v, _inCombatAccelerations);
            
        }
    }
}
