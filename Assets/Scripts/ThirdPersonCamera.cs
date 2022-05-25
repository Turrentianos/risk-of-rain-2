using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private CharacterControl chrctl;

    private Vector2 _yLimits = new Vector2(360 - 85, 85);
    private const float HeightAbove = 1f;
    private const float HeightExtremes = 0.4f;
    private float _rotateSpeed = 180;
    private float _between;
    private Camera _camera;
    [SerializeField] private Transform cameraY;

    Vector3 CameraHalfExtends
    {
        get
        {
            Vector3 halfExtends;
            halfExtends.y =
                _camera.nearClipPlane *
                Mathf.Tan(0.5f * Mathf.Deg2Rad * _camera.fieldOfView);
            halfExtends.x = halfExtends.y * _camera.aspect;
            halfExtends.z = 0f;
            return halfExtends;
        }
    }

    private float Distance
    {
        get => -_camera.transform.localPosition.z;
    }
    
    void Start()
    {
        _camera = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        _between = (_yLimits.x + _yLimits.y) / 2;
    }

    void LateUpdate()
    {
        SetCameraPosition();
        InitialRotationCamera();
        ClampCameraYRotation();
        AdjustCameraHeight();
        ReduceDistanceOnCollision();
        ClearNearPlane();
        
    }

    private void ReduceDistanceOnCollision()
    {
        Vector3 focusPoint = transform.position;
        Vector3 lookDirection = (focusPoint - _camera.transform.position).normalized;
        Vector3 cameraLocalPosition = _camera.transform.localPosition;
        if (Physics.Raycast(focusPoint, -lookDirection, out RaycastHit hitInfo, Distance, LayerMask.GetMask("Ground")))
            cameraLocalPosition.z = -hitInfo.distance;
        _camera.transform.localPosition = cameraLocalPosition;
    }

    private void OnDrawGizmos()
    {
        Vector3 focusPoint = transform.position;
        Vector3 lookDirection = (focusPoint - _camera.transform.position).normalized;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(focusPoint, focusPoint+lookDirection);
        
        Quaternion lookRotation = _camera.transform.rotation;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(focusPoint, focusPoint+lookRotation * Vector3.forward);
    }

    private void ClearNearPlane()
    {
        Quaternion lookRotation = _camera.transform.rotation;
        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 focusPoint = transform.position;
        Vector3 cameraLocalPosition = _camera.transform.localPosition;
        if (Physics.BoxCast(
                focusPoint, CameraHalfExtends, -lookDirection,
                out RaycastHit hitInfo, lookRotation, 
                Distance - _camera.nearClipPlane, LayerMask.GetMask("Ground")))
        {
            cameraLocalPosition.z = -(hitInfo.distance + _camera.nearClipPlane);
        }
        _camera.transform.localPosition = cameraLocalPosition;
    }

    private void SetCameraPosition()
    {
        transform.position = chrctl.transform.position;
        Vector3 localPosition = _camera.transform.localPosition;
        localPosition.z = -4;
        _camera.transform.localPosition = localPosition;
    }
    private void InitialRotationCamera()
    {
        float horizontal = Input.GetAxis("Mouse X");
        transform.Rotate(0f, horizontal * _rotateSpeed * Time.deltaTime, 0f);
        
        float vertical = -Input.GetAxis("Mouse Y");
        cameraY.Rotate(vertical * _rotateSpeed * Time.deltaTime, 0f, 0f);
    }
    private void AdjustCameraHeight()
    {
        float angle = cameraY.localRotation.eulerAngles.x;
        float rescaledAngle = ((angle > _yLimits.y ? angle - 360 : angle) + 360 - _yLimits.x) / (_yLimits.y + 360 - _yLimits.x);
        Vector3 localPosition = _camera.transform.localPosition;
        if (rescaledAngle > 0.5f)
        {
            localPosition.y = Mathf.Lerp(HeightAbove, HeightExtremes, (rescaledAngle - 0.5f) / 0.5f);
        }
        else
        {
            localPosition.y = Mathf.Lerp(HeightAbove, HeightExtremes, (0.5f - rescaledAngle) / 0.5f);
        }
        _camera.transform.localPosition = localPosition;
    }

    // Clamp the rotation cameraY to the closest limit.
    private void ClampCameraYRotation()
    {
        float x = cameraY.localRotation.eulerAngles.x;
        x = x < _between ? Mathf.Min(x, _yLimits.y) : Mathf.Max(x, _yLimits.x);
        cameraY.localRotation = Quaternion.Euler(x, 0f, 0f);
    }
}
