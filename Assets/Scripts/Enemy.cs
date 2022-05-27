using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [SerializeField] private NavMeshAgent _agent;

    
    [SerializeField] 
    private Canvas _healthBarCanvas;
    
    [SerializeField]
    private Slider _healthBar;

    private Transform _player;
    private Transform _camera;
    private BanditController _bandit;

    private const float MaxHealth = 80;
    private float _health = 80;
    private float _damage = 12;
    private float _lastAttack;
    private const float AttackRate = 2f;
    private bool _playerSeen;
    private float _distanceToPlayer;
    private bool _inAttackRange;
    private const float AttackRange = 8;
    private const float MaxSightRange = 10;

    private void Awake()
    {
        GameObject playerGameObject = GameObject.FindWithTag("Player");
        _player = playerGameObject.transform;
        _bandit = playerGameObject.GetComponent<BanditController>();
        _camera = Camera.main.transform;
    }

    public bool active = true;
    private void Update()
    {
        if (active)
        {
            _playerSeen = SearchPlayer();
            _inAttackRange = CheckInAttackRange();
            if (!_playerSeen && !_inAttackRange) RandomWalk();
            if (_playerSeen && !_inAttackRange) MoveToPlayer();
            if (_playerSeen && _inAttackRange)
            {
                if (Time.time - _lastAttack > AttackRate) AttackPlayer();
                StrafeAroundPlayer();
            }    
        }
    }

    private void LateUpdate()
    {
        if (_healthBarCanvas)
            _healthBarCanvas.transform.LookAt(_camera.position);
    }

    private bool CheckInAttackRange()
    {
        _distanceToPlayer = Vector3.Distance(transform.position, _player.position);
        return _distanceToPlayer < AttackRange && _playerSeen;
    }

    private bool _strafing;
    private const float StrafingDistance = 3f;
    private void StrafeAroundPlayer()
    {
        if (!_strafing || _agent.remainingDistance < _agent.stoppingDistance)
        {
            _agent.isStopped = true;
            _strafing = _agent.SetDestination(transform.position + transform.right * StrafingDistance + Vector3.forward * (Random.value - 0.5f) );
            _agent.isStopped = false;
        }
    }

    [SerializeField] private GameObject _fireBall;
    [SerializeField] private Transform _fireBallStart;
    [SerializeField] private AudioClip _fireBallShootingSound;
    private void AttackPlayer()
    {
        AudioSource.PlayClipAtPoint(_fireBallShootingSound, _fireBallStart.position); 
        GameObject createdFireBall = Instantiate(_fireBall, _fireBallStart.position, _fireBallStart.rotation);
        FireBall fireBallScript = createdFireBall.GetComponentInChildren<FireBall>();
        fireBallScript.damage = _damage;
        _lastAttack = Time.time;
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
    
    private bool SearchPlayer()
    {
        if (!_bandit.IsInvisible)
        {
            Vector3 enemyPos = transform.position;
            Vector3 playerPos = _player.position;
            Vector3 direction = (playerPos - enemyPos).normalized;
            Ray ray = new Ray(enemyPos, direction);
            if (Vector3.Angle(direction, transform.forward) < 120 && Physics.Raycast(ray, MaxSightRange))
            {
                return true;
            }    
        }
        return false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        _health -= GetDamage(collision.collider);
        if (_health < MaxHealth)
        {
            _healthBarCanvas.gameObject.SetActive(true);
            _healthBar.value = _health / MaxHealth;
        }
        
        
        
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
            Destroy(damageCollider.gameObject, 0.1f);
            return _bandit.ShotgunDamage();
        } 
        if (damageCollider.CompareTag(_bandit.RevolverBulletTag))
        {
            Destroy(damageCollider.gameObject, 0.1f);
            float damage = _bandit.RevolverDamage();
            if (_health - damage <= 0) _bandit.Reset();
            return damage;
        }
        if (damageCollider.CompareTag(_bandit.SlashTag))
        {
            return _bandit.SlashDamage();
        }

        return 0;
    }
}
