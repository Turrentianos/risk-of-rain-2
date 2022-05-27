using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class BanditController : MonoBehaviour
{
    
    private Camera _camera;
    private void Awake()
    {
        _sweepAnimator = GetComponentInChildren<Animator>();
        _hitMask = LayerMask.GetMask("Ground", "Enemy");
        // Compute maxDistance of a bullet
        _camera = Camera.main;
        Assert.IsNotNull(_camera, "No camera found in Bandit ability controller!!!");
        //_maxDistance = _camera.farClipPlane;
        _remainingBullets = 4;
        _ammoCount.text = _remainingBullets.ToString();
        _invisibleCooldownText.text = "";
        _ultimateCooldownText.text = "";
        _slashCooldownLabel.text = "";
        _character.color = _visibleColor;
        _invisibilityBump = -Physics.gravity.normalized * Mathf.Sqrt(2 * Physics.gravity.magnitude * KnockUpHeight);
    }

    private int _hitMask;
    [SerializeField] private Text _healthLabel;
    [SerializeField] private Slider _healthSlider;
    private float _health = 110;
    private float _maxHealth = 110;
    private float _damage = 12;

    private void Update()
    {
        _healthLabel.text = _health + "/" + _maxHealth;
        _healthSlider.maxValue = _maxHealth;
        _healthSlider.value = _health;
    }

    private float Damage()
    {
        return _damage;
    }

    #region Shoot
    private const float ShootInterval = 0.25f;
    private float _lastShootTime;
    private const float ReloadTime = 1.0f;
    private const int MagazineSize = 4;
    private int _remainingBullets = 4;
    private float _maxDistance = 50f;
    private const float BulletSpeed = 150f;
    [SerializeField]
    private TrailRenderer _shotGunBulletTrail;

    [SerializeField]
    private Transform _initialBulletTransform;

    [SerializeField] private AudioClip _shotgunSound;
    
    public readonly string ShotgunBulletTag = "ShotgunBullet";
    // Shotgun that shoots five pellets equally spread horizontally on the same height 
    public void Mouse0()
    {
        if (Time.time - _lastShootTime > ShootInterval && _remainingBullets > 0)
        {
            _invisible = false;
            AudioSource.PlayClipAtPoint(_shotgunSound, transform.position);
            Ray[] bulletRays = GetShotgunRays();
            for (int i = 0; i < bulletRays.Length; i++)
            {
                // Find collision from cameras point of view
                Ray rayFromCamera = bulletRays[i];
                Vector3 hitPoint;
                if (Physics.Raycast(rayFromCamera, out RaycastHit hit, _maxDistance, _hitMask))
                    hitPoint = hit.point;
                else
                    hitPoint = transform.position + rayFromCamera.direction * _maxDistance;

                TrailRenderer trail = Instantiate(
                        _shotGunBulletTrail,
                        _initialBulletTransform.position,
                        _initialBulletTransform.rotation
                    );
                
                _lastShootTime = Time.time;
                StartCoroutine(SpawnTrail(trail, hitPoint));
            }
            RemoveBullet();
            if (_remainingBullets == MagazineSize - 1)
                StartCoroutine(Reload());
        }
    }

    private IEnumerator Reload()
    {
        float lastReload = 0f;
        while (_remainingBullets < MagazineSize)
        {
            float reloadDelay = _lastShootTime < lastReload ? ReloadTime / 2f : ReloadTime;
            if (Time.time - _lastShootTime > ReloadTime && Time.time - lastReload > reloadDelay)
            {
                lastReload = Time.time;
                LoadBullet();
            }

            yield return null;
        }
    }

    [SerializeField] private Image[] _ammoImages;
    [SerializeField] private Text _ammoCount;
    private void LoadBullet()
    {
        _ammoImages[_remainingBullets].enabled = true;
        _remainingBullets += 1;
        _ammoCount.text = _remainingBullets.ToString();
    }

    private void RemoveBullet()
    {
        _remainingBullets -= 1;
        _ammoImages[_remainingBullets].enabled = false;
        _ammoCount.text = _remainingBullets.ToString();
    }
    
    private IEnumerator SpawnTrail(TrailRenderer trail, Vector3 hitPoint, float maxTime=5f)
    {
        float time = 0;
        Vector3 startPosition = trail.transform.position;
        float totalTime = Vector3.Distance(startPosition, hitPoint) / BulletSpeed;
        while (trail && time < Mathf.Min(maxTime, totalTime))
        {
            trail.transform.position = Vector3.Lerp(startPosition, hitPoint, time / totalTime);
            time += Time.deltaTime;
            yield return null;
        }
        
        if (trail) {
            trail.transform.position = hitPoint;
            Destroy(trail.gameObject, trail.time);   
        }
    }

    // Should return 5 directions, one for each bullet shot but the shotgun 
    private Ray[] GetShotgunRays()
    {
        float[] _bulletSpread = {-1f, -0.5f, 0.5f, 1f}; 
        Ray middleBulletRay = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, _camera.nearClipPlane));
        Ray[] bulletRays = new Ray[5];
        bulletRays[4] = middleBulletRay;
        for (int i = 0; i < _bulletSpread.Length; i++)
        {
            bulletRays[i] = new Ray(
                    middleBulletRay.origin,
                    Quaternion.AngleAxis(_bulletSpread[i], _camera.transform.up) * middleBulletRay.direction
                );
        }
        return bulletRays;
    }

    public float ShotgunDamage()
    {
        return Damage();
    }
    #endregion

    #region Slash
    public readonly string SlashTag = "Slash";

    [SerializeField] private Text _slashCooldownLabel;
    private static readonly int Sweep = Animator.StringToHash("Sweep");
    private const float SlashCooldown = 4f;
    private float _lastSlash;
    private Animator _sweepAnimator; 
    // Slash attack
    public void Mouse2()
    {
        if (Time.time - _lastSlash >= SlashCooldown)
        {
            _invisible = false;
            _lastSlash = Time.time;
            _sweepAnimator.SetTrigger(Sweep);
            cooldownCoroutines[0] = CooldownTimer(_slashCooldownLabel, SlashCooldown);
            StartCoroutine(cooldownCoroutines[0]);
        }
        
    }

    public float SlashDamage()
    {
        return Damage() * 3.6f;
    }
    #endregion

    #region Inivisibility
    private Vector3 _invisibilityBump;
    public Vector3 InvisibilityBump
    {
        get => _invisibilityBump;
    }
    private const float KnockUpHeight = 2f;

    [SerializeField]
    private ParticleSystem defensiveAbilityEffect;

    private const float InvisibilityDuration = 5f;

    private const float InvisibilityCooldown = 6f;
    private float _lastInvisibility = -InvisibilityCooldown;
    
    [SerializeField] private Text _invisibleCooldownText;
    // Small upward force at start and end of ability and invisibility for the duration of the ability or until cancelled
    public void DefensiveAbility()
    {
        if (Time.time - _lastInvisibility >= InvisibilityCooldown)
        {
            _lastInvisibility = Time.time;
            _changeInvisible = true;
            Instantiate(defensiveAbilityEffect, transform.position, Quaternion.Euler(90f, 0, 0));
        
            StartCoroutine(ActivateInvisibility());
            cooldownCoroutines[1] = CooldownTimer(_invisibleCooldownText, InvisibilityCooldown);
            StartCoroutine(cooldownCoroutines[1]);
        }
    }

    private bool _invisible;
    private bool _changeInvisible;
    public bool InvisibleChange
    {
        get => _changeInvisible;
        set => _changeInvisible = value;
    }

    public bool IsInvisible => _invisible;

    [SerializeField]
    private Material _character;
    
    [SerializeField]
    private Color _invisibleColor;
    [SerializeField]
    private Color _visibleColor;

    [SerializeField] private AudioClip _invisibityClip;
    
    private IEnumerator ActivateInvisibility()
    {
        _invisible = true;
        _character.color = _invisibleColor;
        AudioSource.PlayClipAtPoint(_invisibityClip, transform.position);    
        float time = 0f;
        while (_invisible && time < InvisibilityDuration)
        {
            time += Time.deltaTime;
            yield return null;
        }
        AudioSource.PlayClipAtPoint(_invisibityClip, transform.position);
        Instantiate(defensiveAbilityEffect, transform.position, Quaternion.Euler(90f, 0, 0));
        InvisibleChange = true;
        _invisible = false;
        _character.color = _visibleColor;
    }
    #endregion

    #region Ultimate

    [SerializeField] private AudioClip _revolverSound;
    public readonly string RevolverBulletTag = "RevolverBullet";
    public bool reset = false;
    private const float UltimateCooldown = 4f;
    private float _lastUltimate = -UltimateCooldown;
    [SerializeField] private Text _ultimateCooldownText; 
    public void Ultimate()
    {
        _invisible = false;
        if (Time.time - _lastUltimate >= UltimateCooldown)
        {
            AudioSource.PlayClipAtPoint(_revolverSound, transform.position);
            _lastUltimate = Time.time;
            StartCoroutine(AimAndShootRevolver());
            cooldownCoroutines[2] = CooldownTimer(_ultimateCooldownText, UltimateCooldown); 
            StartCoroutine(cooldownCoroutines[2]);
        }
    }

    [SerializeField]
    private Transform _revolver;
    [SerializeField]
    private Transform _initialRevolverTransform;
    private const float RevolverAnimationTime = 0.1f;
    private const float RestingRevolverAngle = 0f;
    private const float AimingRevolverAngle = -90f;
    [SerializeField]
    private TrailRenderer _revolverBulletTrail;
    
    // Shoot one pellet using revolver that resets all character cooldowns
    private IEnumerator AimAndShootRevolver()
    {
        yield return TurnRevolver(RestingRevolverAngle, AimingRevolverAngle);
        
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, _camera.nearClipPlane));
        Vector3 hitPoint;
        if (Physics.Raycast(ray, out RaycastHit hit, _maxDistance, _hitMask))
            hitPoint = hit.point;
        else
            hitPoint = _initialRevolverTransform.position + ray.direction * _maxDistance;
        TrailRenderer trail = Instantiate(
            _revolverBulletTrail,
            _initialRevolverTransform.position,
            _initialRevolverTransform.rotation
        );
        StartCoroutine(SpawnTrail(trail, hitPoint));

        yield return TurnRevolver(AimingRevolverAngle, RestingRevolverAngle);
    }

    private IEnumerator TurnRevolver(float startRotation, float endRotation)
    {
        float time = 0f;
        Vector3 revolverRotation;
        while (time < RevolverAnimationTime)
        {
            time += Time.deltaTime;
            revolverRotation = _revolver.localRotation.eulerAngles;
            revolverRotation.x = Mathf.LerpAngle(startRotation, endRotation, time / RevolverAnimationTime);
            _revolver.localRotation = Quaternion.Euler(revolverRotation);
            yield return null;
        }

        revolverRotation = _revolver.rotation.eulerAngles;
        revolverRotation.x = Mathf.LerpAngle(startRotation, endRotation, 1);
        _revolver.rotation = Quaternion.Euler(revolverRotation);
    }

    public float RevolverDamage()
    {
        return Damage() * 6f;
    }
    #endregion

    private IEnumerator[] cooldownCoroutines = new IEnumerator[3];
    public void Reset()
    {
        // Stop cooldown coroutines
        for (int i = 0; i < cooldownCoroutines.Length; i++)
        {
            if (cooldownCoroutines[i] != null)
                StopCoroutine(cooldownCoroutines[i]);
        }
        
        // Reset cooldown's
        _lastSlash = Time.time - SlashCooldown;
        _slashCooldownLabel.text = "";
        
        _lastInvisibility = Time.time - InvisibilityCooldown;
        _invisibleCooldownText.text = "";

        _lastUltimate = Time.time - UltimateCooldown;
        _ultimateCooldownText.text = "";
        
        // Reload shotgun
        if (_remainingBullets < MagazineSize)
        {
            StopCoroutine(nameof(Reload));
            _remainingBullets = 4;
            for (int i = 0; i < MagazineSize; i++)
                _ammoImages[i].enabled = true;
            _ammoCount.text = MagazineSize.ToString();
        }
    }
    private IEnumerator CooldownTimer(Text text, float cooldown)
    {
        float time = cooldown;
        while (time > 0)
        {
            time -= Time.deltaTime;
            text.text = time.ToString("0.0");
            yield return null;
        }

        text.text = "";
    }

    private void Death()
    {
        SceneManager.LoadScene("RiskOfRain2");
    }

    private void OnCollisionEnter(Collision collision)
    {
        Collider collisionCollider = collision.collider;
        if (collisionCollider.CompareTag("FireBall"))
        {
            _health -= collisionCollider.GetComponent<FireBall>().damage;
            if (_health < 0)
            {
                Death();
            }
            Destroy(collisionCollider.gameObject);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit collision)
    {
        
        if (collision.collider.CompareTag("FallDepth"))
            Death();
    }

    public void AddHeath(float healthUpgrade)
    {
        _health += healthUpgrade;
        _maxHealth += healthUpgrade;
    }
}