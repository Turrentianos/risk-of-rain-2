using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class BanditController : MonoBehaviour
{
    
    private Camera _camera;
    private void Start()
    {
        _hitMask = LayerMask.GetMask("Ground", "Enemy");
        // Compute maxDistance of a bullet
        _camera = Camera.main;
        Assert.IsNotNull(_camera, "No camera found in Bandit ability controller!!!");
        _maxDistance = _camera.farClipPlane;
        _remainingBullets = 4;
        _ammoCount.text = _remainingBullets.ToString();
        _invisibleCooldownText.text = "";
        _character.color = _visibleColor;
        _invisibilityBump = -Physics.gravity.normalized * Mathf.Sqrt(2 * Physics.gravity.magnitude * KnockUpHeight);
    }

    private int _hitMask;
    private float _health = 110;
    private float _damage = 12;
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
    private float _maxDistance;
    private const float BulletSpeed = 100f;
    [SerializeField]
    private TrailRenderer _shotGunBulletTrail;

    [SerializeField]
    private Transform _initialBulletTransform;

    public readonly string ShotgunBulletTag = "ShotgunBullet";
    // Shotgun that shoots five pellets equally spread horizontally on the same height 
    public void Mouse0()
    {
        if (Time.time - _lastShootTime > ShootInterval && _remainingBullets > 0)
        {
            _invisible = false;
            Ray[] bulletRays = GetShotgunRays();
            for (int i = 0; i < bulletRays.Length; i++)
            {
                Ray ray = bulletRays[i];
                LayerMask mask = LayerMask.GetMask("Ground");
                Vector3 hitPoint;
                if (Physics.Raycast(ray, out RaycastHit hit, _maxDistance, _hitMask))
                    hitPoint = hit.point;
                else
                    hitPoint = ray.direction * _maxDistance;
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
            if (Time.time - _lastShootTime > ReloadTime && Time.time - lastReload > ReloadTime)
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
        float totalTime = Vector3.Distance(startPosition, hitPoint)/BulletSpeed;
        while (time < totalTime && time < maxTime)
        {
            trail.transform.position = Vector3.Lerp(startPosition, hitPoint, time/totalTime);
            time += Time.deltaTime;

            yield return null;
        }

        trail.transform.position = hitPoint;
        Destroy(trail.gameObject, trail.time);
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
    // Slash attack
    public void Mouse2()
    {
        _invisible = false;
        throw new System.NotImplementedException();
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
        
            StartCoroutine(CooldownTimer(_invisibleCooldownText, InvisibilityCooldown));
        }
    }

    private bool _invisible;
    private bool _changeInvisible;
    public bool InvisibleChange
    {
        get => _changeInvisible;
        set => _changeInvisible = value;
    }

    [SerializeField]
    private Material _character;
    
    [SerializeField]
    private Color _invisibleColor;
    [SerializeField]
    private Color _visibleColor;

    private IEnumerator ActivateInvisibility()
    {
        _invisible = true;
        _character.color = _invisibleColor;
        float time = 0f;
        while (_invisible && time < InvisibilityDuration)
        {
            time += Time.deltaTime;
            yield return null;
        }
        Instantiate(defensiveAbilityEffect, transform.position, Quaternion.Euler(90f, 0, 0));
        InvisibleChange = true;
        _invisible = false;
        _character.color = _visibleColor;
    }
    #endregion

    // Shoot one pellet using revolver that resets all character cooldowns

    #region Ultimate
    
    public readonly string RevolverBulletTag = "RevolverBullet";
    private bool _reset = false;
    private const float UltimateCooldown = 4f;
    private float _lastUltimate = -UltimateCooldown;
    [SerializeField] private Text _ultimateCooldownText; 
    public void Ultimate()
    {
        _invisible = false;
        if (Time.time - _lastUltimate >= UltimateCooldown)
        {
            _lastUltimate = Time.time;
            StartCoroutine(AimRevolver());
            StartCoroutine(CooldownTimer(_ultimateCooldownText, UltimateCooldown));
        }
    }

    [SerializeField]
    private Transform _revolver;
    [SerializeField]
    private Transform _initialRevolverTransform;
    private const float RevolverAnimationTime = 0.35f;
    private const float RestingRevolverAngle = 0f;
    private const float AimingRevolverAngle = -90f;
    [SerializeField]
    private TrailRenderer _revolverBulletTrail;
    private IEnumerator AimRevolver()
    {
        yield return TurnRevolver(RestingRevolverAngle, AimingRevolverAngle);
        
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, _camera.nearClipPlane));
        Vector3 hitPoint;
        if (Physics.Raycast(ray, out RaycastHit hit, _maxDistance, _hitMask))
            hitPoint = hit.point;
        else
            hitPoint = ray.direction * _maxDistance;
        TrailRenderer trail = Instantiate(
            _revolverBulletTrail,
            _initialRevolverTransform.position,
            _initialRevolverTransform.rotation
        );
        StartCoroutine(SpawnTrail(trail, hitPoint));

        yield return TurnRevolver(AimingRevolverAngle, RestingRevolverAngle);
    }

    private IEnumerator TurnRevolver(float start, float end)
    {
        float time = 0f;
        Vector3 revolverRotation;
        while (time < RevolverAnimationTime)
        {
            time += Time.deltaTime;
            revolverRotation = _revolver.localRotation.eulerAngles;
            revolverRotation.x = Mathf.LerpAngle(start, end, time / RevolverAnimationTime);
            _revolver.localRotation = Quaternion.Euler(revolverRotation);
            yield return null;
        }

        revolverRotation = _revolver.rotation.eulerAngles;
        revolverRotation.x = Mathf.LerpAngle(start, end, 1);
        _revolver.rotation = Quaternion.Euler(revolverRotation);
    }

    public float RevolverDamage()
    {
        return Damage() * 6f;
    }
    #endregion
    
    private IEnumerator CooldownTimer(Text text, float cooldown)
    {
        float time = cooldown;
        while (!_reset && time > 0)
        {
            time -= Time.deltaTime;
            text.text = time.ToString("0.0");
            yield return null;
        }

        text.text = "";
    }
    
    
}