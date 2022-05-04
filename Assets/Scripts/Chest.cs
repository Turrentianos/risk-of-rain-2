using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [SerializeField] private GameObject itemOrb;

    [SerializeField] private Transform chestLid;

    private const float ClosedAngle = 0f;
    private const float OpenAngle = 55f;
    
    private float _timer = 0f;
    private float _time = 0.5f;

    private void Start()
    {
        enabled = false;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        Vector3 chestLidRotation = chestLid.rotation.eulerAngles;
        chestLidRotation.z = Mathf.LerpAngle(ClosedAngle, OpenAngle, _timer / _time);
        chestLid.rotation = Quaternion.Euler(chestLidRotation);
        if (_timer > _time)
        {
            enabled = false;
            gameObject.layer = LayerMask.NameToLayer("OpenContainers");
            GameObject createdItemOrb = Instantiate(itemOrb, transform.position, transform.rotation);
            createdItemOrb.transform.Rotate(0, 90, 0);
        }
    }

    private void OnDrawGizmos()
    {
        Color color = Color.gray;
        color.a = 0.4f;
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, 1.5f);
    }
}
