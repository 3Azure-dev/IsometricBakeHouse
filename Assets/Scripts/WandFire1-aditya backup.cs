using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

[RequireComponent(typeof(WandAim))]
public class WandFire : MonoBehaviour
{
    [SerializeField] private Bolt _boltPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private int _poolSize = 20;

    private WandAim _aim;
    private IObjectPool<Bolt> _pool;

    private void Awake()
    {
        _aim = GetComponent<WandAim>();

        _pool = new ObjectPool<Bolt>(
            createFunc: CreateBolt,
            actionOnGet: bolt => bolt.gameObject.SetActive(true),
            actionOnRelease: bolt => bolt.gameObject.SetActive(false),
            actionOnDestroy: bolt => Destroy(bolt.gameObject),
            defaultCapacity: _poolSize,
            maxSize: 50);
    }

    private Bolt CreateBolt()
    {
        Bolt bolt = Instantiate(_boltPrefab);
        bolt.SetPool(_pool);
        return bolt;
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            Fire();
    }

    private void Fire()
    {
        Bolt bolt = _pool.Get();
        bolt.Launch(_firePoint.position, _aim.AimDirection);
    }
}