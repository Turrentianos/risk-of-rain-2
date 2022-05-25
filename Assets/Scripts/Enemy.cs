using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent _agent;

    private Transform _player;
    private BanditController _bandit;
    
    private float _health = 80;
    private float _damage = 12;
    private bool _playerSeen;
    private float _distanceToPlayer;
    private bool _inAttackRange;
    private const float AttackRange = 3;
    private const float MaxSightRange = 5;

    private void Awake()
    {
        GameObject playerGameObject = GameObject.FindWithTag("Player");
        _player = playerGameObject.transform;
        _bandit = playerGameObject.GetComponent<BanditController>();
    }

    private void Update()
    {
        SearchPlayer();
        CheckInAttackRange();
        if (!_playerSeen && !_inAttackRange) RandomWalk();
        if (_playerSeen && !_inAttackRange) MoveToPlayer();
        if (_playerSeen && _inAttackRange)
        {
            AttackPlayer();
            StrafeAroundPlayer();
        }
    }

    private void CheckInAttackRange()
    {
        _distanceToPlayer = Vector3.Distance(transform.position, _player.position);
        _inAttackRange = _distanceToPlayer < AttackRange;
    }

    private bool _strafing;
    private void StrafeAroundPlayer()
    {
        
        if (!_strafing || _agent.isStopped)
            _strafing = _agent.SetDestination(transform.position + transform.right * Random.Range(-WalkDistance, WalkDistance));
    }

    private void AttackPlayer()
    {
        //TODO: Create projectile toward player
    }

    private void MoveToPlayer()
    {
        _agent.SetDestination(_player.position);
    }

    private const float WalkDistance = 15;
    private bool _destinationFound;
    private void RandomWalk()
    {
        if (_agent.remainingDistance < _agent.stoppingDistance) {
            Vector3 destination = SearchDestination();
            _agent.SetDestination(destination);
        }
    }

    
    private Vector3 SearchDestination()
    {
        Vector3 randDirection = Random.insideUnitSphere * WalkDistance;
        randDirection += transform.position;
        NavMesh.SamplePosition(randDirection, out NavMeshHit navHit, WalkDistance, -1);
        
        return navHit.position;
    }
    
    private void SearchPlayer()
    {
        Vector3 enemyPos = transform.position;
        Vector3 playerPos = _player.position;
        Vector3 direction = (playerPos - enemyPos).normalized;
        Ray ray = new Ray(enemyPos, direction);
        if (Vector3.Angle(direction, transform.forward) < 120 && Physics.Raycast(ray, MaxSightRange))
        {
            _playerSeen = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.collider.tag);
        _health -= GetDamage(collision.collider);
        if (_health <= 0)
        {
            Death();
        }
    }

    private void Death()
    {
        Destroy(gameObject);
    }

    private float GetDamage(Collider damageCollider)
    {
        if (damageCollider.CompareTag(_bandit.ShotgunBulletTag))
        {
            return _bandit.ShotgunDamage();
        } 
        else if (damageCollider.CompareTag(_bandit.RevolverBulletTag))
        {
            return _bandit.RevolverDamage();
        }
        else if (damageCollider.CompareTag(_bandit.SlashTag))
        {
            return _bandit.SlashDamage();
        }

        return 0;
    }
}
