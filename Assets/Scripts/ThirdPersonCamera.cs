using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private CharacterControl chrctl;

    private Vector2 _yLimits = new Vector2(5, 85);
    private float _rotateSpeed = 180;

    [SerializeField] private Transform cameraY;
    
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = chrctl.transform.position;
        float horizontal = Input.GetAxis("Mouse X");
        transform.Rotate(0f, horizontal * _rotateSpeed * Time.deltaTime, 0f);
        float vertical = -Input.GetAxis("Mouse Y");
        cameraY.Rotate(vertical * _rotateSpeed * Time.deltaTime, 0f, 0f);
    }
}
