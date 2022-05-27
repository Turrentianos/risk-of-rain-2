using UnityEngine;

public class FireBall : MonoBehaviour
{
    public float damage;
    [SerializeField] private AudioClip _fireballExplosion;
    private Transform _player;
    private float _timer;
    private float _totalTime;
    private const float MaxTime = 12f;
    private const float FireBallSpeed = 10f;
    private Rigidbody _rigidbody;
    private Vector3 _startPosition;
    private Vector3 _towardPlayer;
    private Vector3 _endPosition;

    private void Awake()
    {
        _player = GameObject.FindWithTag("Player").transform;
        _rigidbody = gameObject.GetComponent<Rigidbody>();
        _startPosition = transform.position;
        _towardPlayer = (_player.position + (Vector3.up * 0.1f) - _startPosition).normalized;
        _endPosition = MaxTime * FireBallSpeed * _towardPlayer + _startPosition;
        if (Physics.Raycast(_startPosition, _towardPlayer, out RaycastHit hit, MaxTime * FireBallSpeed,
                LayerMask.GetMask("Ground")))
            _endPosition = hit.point;

        _timer = 0f; 
        _totalTime = Vector3.Distance(_startPosition, _endPosition) / FireBallSpeed;
    }


    private void FixedUpdate()
    {
            
        _rigidbody.MovePosition(Vector3.Lerp(_startPosition, _endPosition, _timer / _totalTime));
        _timer += Time.deltaTime;

        if (_timer > _totalTime)
            Destroy(gameObject);

    }

    private void OnCollisionEnter()
    {
        AudioSource.PlayClipAtPoint(_fireballExplosion, transform.position);
    }
}
