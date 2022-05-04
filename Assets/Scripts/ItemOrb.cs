using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ItemOrb : MonoBehaviour
{
    private const float Height = 4.5f;
    private const float ForwardDistance = 1f;
    private Rigidbody _itemOrbRigidbody;
    void Start()
    {
        _itemOrbRigidbody = GetComponent<Rigidbody>();
        Assert.IsNotNull(_itemOrbRigidbody);
        _itemOrbRigidbody.AddForce(Vector3.up * Height + transform.forward * ForwardDistance, ForceMode.Impulse);
    }
    
    private void OnCollisionEnter()
    {
        _itemOrbRigidbody.isKinematic = true;
    }
}
