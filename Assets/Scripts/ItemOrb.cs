using UnityEngine;
using UnityEngine.Assertions;

public class ItemOrb : MonoBehaviour
{
    private const float Height = 4.5f;
    private const float ForwardDistance = 1f;
    private Rigidbody _itemOrbRigidbody;
    private SphereCollider _itemOrbSphereCollider;
    private BanditController _banditController;
    void Start()
    {
        _banditController = GameObject.FindWithTag("Player").GetComponent<BanditController>(); 
        _itemOrbRigidbody = GetComponent<Rigidbody>();
        _itemOrbSphereCollider = GetComponent<SphereCollider>();
        Assert.IsNotNull(_itemOrbRigidbody);
        _itemOrbRigidbody.AddForce(Vector3.up * Height + transform.forward * ForwardDistance, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.collider.name);
        Destroy(_itemOrbRigidbody);
        _itemOrbSphereCollider.isTrigger = true;
        _itemOrbSphereCollider.radius = 1.0f;
        gameObject.layer = LayerMask.NameToLayer("ItemOrbReadyToPickUp");
    }

    [SerializeField] private AudioClip _upgradeClip;
    private const float HealthUpgrade = 25;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _banditController.AddHeath(HealthUpgrade);
        }
        AudioSource.PlayClipAtPoint(_upgradeClip, transform.position, 0.5f);
        Destroy(gameObject);
    }
}
