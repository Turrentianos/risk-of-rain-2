using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blade : MonoBehaviour
{
    private BanditController _banditController;
    private void Awake()
    {
        _banditController = GameObject.FindWithTag("Player").GetComponent<BanditController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Enemy"))
            collision.collider.GetComponent<Enemy>().Damage(_banditController.SlashTag);
    }
}
