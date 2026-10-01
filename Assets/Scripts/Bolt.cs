using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody2D))]
public class Bolt : MonoBehaviour
{
    [SerializeField] private float _speed = 20f;
    [SerializeField] private LayerMask _groundLayer;

    private Rigidbody2D _rb;
    private TrailRenderer _trail;
    private Camera _cam;
    private IObjectPool<Bolt> _pool;
    private bool _released;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _trail = GetComponent<TrailRenderer>();
        _cam = Camera.main;
    }

    // the wand tells each bolt which pool it belongs to
    public void SetPool(IObjectPool<Bolt> pool) => _pool = pool;

    public void Launch(Vector2 position, Vector2 direction)
    {
        transform.position = position;
        _rb.position = position;
        transform.right = direction;
        _trail.Clear();          // stops a streak from the bolt's last position
        _released = false;
        _rb.linearVelocity = direction * _speed;
    }

    private void Update()
    {
        // 0 to 1 = on screen. A bit past that = off screen.
        Vector3 vp = _cam.WorldToViewportPoint(transform.position);
        if (vp.x < -0.1f || vp.x > 1.1f || vp.y < -0.1f || vp.y > 1.1f)
            Despawn();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // is the thing we hit on the Ground layer?
        if ((_groundLayer.value & (1 << other.gameObject.layer)) != 0)
            Despawn();
    }

    private void Despawn()
    {
        if (_released) return;   // stops it being returned twice
        _released = true;

        if (_pool != null) _pool.Release(this);
        else Destroy(gameObject);
    }
}