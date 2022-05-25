using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Timers;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private BanditController _bandit;
    [SerializeField] private GameObject _groundEnemy;
    [SerializeField] private GameObject _lesserWisp;

    private const float SpawnTimer = 5f;
    private const float SpawnProbability = 0.7f;
    private const float MaxSpawnNumber = 8f;
    private const float IndividualSpawnProbability = 0.5f;
    private float _timer;
    private void Update()
    {
        _timer += Time.deltaTime;


        if (_timer > SpawnTimer && Random.value < SpawnProbability)
        {
            ChooseSpawnPosition();
            SpawnEnemies();
        }

        if (_timer > SpawnTimer)
            _timer = 0;
    }

    private void ChooseSpawnPosition()
    {
        transform.position = _player.position;
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < MaxSpawnNumber; i++)
        {
            if (Random.value < IndividualSpawnProbability)
                InstantiateEnemy();
        }
    }

    private void InstantiateEnemy()
    {
        GameObject createdEnemy = Instantiate(_groundEnemy, RandomPosition(), RandomRotation());
        Enemy enemyScript = createdEnemy.GetComponent<Enemy>();
        
    }

    private Quaternion RandomRotation()
    {
        return Quaternion.Euler(0f, Random.value*360f, 0f);
    }

    private const float MinDistance = 4;
    private const float MaxDistance = 20;
    
    private Vector3 RandomPosition()
    {
        Vector3 position = transform.position + Random.insideUnitSphere.normalized * Random.Range(MinDistance, MaxDistance);
        NavMeshHit navHit;
        while (!NavMesh.SamplePosition(position, out navHit, MaxDistance, -1))
        {
            position = Random.insideUnitSphere.normalized * Random.Range(MinDistance, MaxDistance);
        }
        return navHit.position;
    }
}
