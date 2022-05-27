using System;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [SerializeField] private GameObject itemOrb;

    [SerializeField] private Transform chestLid;

    [SerializeField] private Transform itemOrbStart;

    [SerializeField] private Transform itemOrbParent;
    
    private const float ClosedAngle = 0f;
    private const float OpenAngle = 55f;
    private LayerMask _mask;
    private float _timer = 0f;
    private float _time = 0.5f;

    private void Start()
    {
        enabled = false;
        _mask = LayerMask.NameToLayer("OpenContainers");
    }

    [SerializeField] private AudioClip _chestClip;
    private void Update()
    {
        // OpenChest and deactivate component when chest is fully open also change layer to OpenContainers
        AudioSource.PlayClipAtPoint(_chestClip, transform.position);
        _timer += Time.deltaTime;
        Vector3 chestLidRotation = chestLid.rotation.eulerAngles;
        chestLidRotation.z = Mathf.LerpAngle(ClosedAngle, OpenAngle, _timer / _time);
        chestLid.rotation = Quaternion.Euler(chestLidRotation);
        if (_timer > _time)
        {
            enabled = false;
            gameObject.layer = _mask;
            GameObject createdItemOrb = Instantiate(itemOrb, itemOrbParent);
            createdItemOrb.transform.position = itemOrbStart.position;
            createdItemOrb.transform.rotation = itemOrbStart.rotation;
            createdItemOrb.transform.Rotate(0, 90, 0);
        }
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Color color = Color.gray;
        color.a = 0.4f;
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, 1.5f);
    }
#endif
}
