using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

[RequireComponent(typeof(WandAim))]
public class WandFire1 : MonoBehaviour
{
    [SerializeField] private WandData _data;   // SO_Wand goes here
    [SerializeField] private Bolt1 _boltPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private int _poolSize = 20;

    private WandAim _aim;
    private IObjectPool<Bolt1> _pool;
    private float _nextFireTime;   // earliest time we can shoot again

    private void Awake()
    {
        _aim = GetComponent<WandAim>();

        if (_data == null)
            Debug.LogWarning("WandFire1: no WandData set, so there is no cooldown.", this);

        _pool = new ObjectPool<Bolt1>(
            createFunc: CreateBolt,
            actionOnGet: bolt => bolt.gameObject.SetActive(true),
            actionOnRelease: bolt => bolt.gameObject.SetActive(false),
            actionOnDestroy: bolt => Destroy(bolt.gameObject),
            defaultCapacity: _poolSize,
            maxSize: 50);
    }

    private Bolt1 CreateBolt()
    {
        Bolt1 bolt = Instantiate(_boltPrefab);
        bolt.SetPool(_pool);
        return bolt;
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            TryFire();
    }

    private void TryFire()
    {
        if (Time.time < _nextFireTime) return;   // still cooling down

        Fire();

        float cooldown = _data != null ? _data.cooldown : 0f;
        _nextFireTime = Time.time + cooldown;
    }

    private void Fire()
    {
        Bolt1 bolt = _pool.Get();
        float range = _data != null ? _data.range : Mathf.Infinity;
        bolt.Launch(_firePoint.position, _aim.AimDirection, range);

        if (BreadSFX.Instance != null)
            BreadSFX.Instance.PlayWandZap();
    }
}