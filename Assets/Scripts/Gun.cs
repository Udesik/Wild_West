using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;
using System.Collections;

public class Gun : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Player _player;
    [SerializeField] private Bullet _bulletPrefab;
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _bulletPoint;

    [SerializeField] private float _shootForce;
    [SerializeField] private float _spread;
    [SerializeField] private float _coolDown;

    [Header("Pool")]
    [SerializeField] private int _poolSize;
    [SerializeField] private int _poolCapacity;

    private bool _canShoot = true;
    private ObjectPool<Bullet> _pool;
    private PlayerInput _playerInput;

    private static readonly int StartCoolDownHash = Animator.StringToHash("StartCoolDown");

    private void Awake()
    {
        _pool = new ObjectPool<Bullet>(
            createFunc: () => Instantiate(_bulletPrefab),
            actionOnGet: (bullet) => OnGetFromPool(bullet),
            actionOnRelease: (bullet) => OnReleaseToPool(bullet),
            actionOnDestroy: (bullet) => Destroy(bullet.gameObject),
            collectionCheck: false,
            defaultCapacity: _poolCapacity,
            maxSize: _poolSize
        );
    }

    private void Start()
    {
        _playerInput = _player.GetPlayerInput();
        _playerInput.Player.Shoot.performed += Shoot;
    }

    private void Shoot(InputAction.CallbackContext context)
    {
        if (_canShoot)
        {
            _pool.Get();
            _animator.SetTrigger(StartCoolDownHash);
            StartCoroutine(CoolDown());
        }
    }

    private void OnGetFromPool(Bullet bullet)
    {
        bullet.transform.position = _bulletPoint.position;
        bullet.gameObject.SetActive(true);
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100);
        }

        Vector3 dirWithoutSpread = targetPoint - _bulletPoint.position;

        float x = Random.Range(-_spread, _spread);
        float y = Random.Range(-_spread, _spread);

        Vector3 dirwithSpread = new Vector3(x, y, 0) + dirWithoutSpread;

        bullet.gameObject.SetActive(true);
        bullet.transform.forward = dirwithSpread.normalized;
        bullet.Died += ReleaseBullet;
        bullet.GetComponent<Rigidbody>().AddForce(dirWithoutSpread * _shootForce, ForceMode.Impulse);
        bullet.StartLifeTime();
    }

    private void OnReleaseToPool(Bullet bullet)
    {
        bullet.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        bullet.Died -= ReleaseBullet;
        bullet.gameObject.SetActive(false);
    }

    private void ReleaseBullet(Bullet bullet)
    {
        _pool.Release(bullet);
    }

    private IEnumerator CoolDown()
    {
        _canShoot = false;
        yield return new WaitForSeconds(_coolDown);
        _canShoot = true;
    }
}
