using UnityEngine;

// Moves between its start and an end point, waits, and comes back. Carries the player.
[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Vector2 _offset = new Vector2(0f, 3f); // end point, from the start
    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _waitTime = 1f;

    private Rigidbody2D _rb;
    private Vector2 _start;
    private Vector2 _end;
    private Vector2 _target;
    private float _waitTimer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _start = _rb.position;
        _end = _start + _offset;
        _target = _end;
    }

    private void FixedUpdate()
    {
        if (_waitTimer > 0f)
        {
            _waitTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector2 next = Vector2.MoveTowards(_rb.position, _target, _speed * Time.fixedDeltaTime);
        _rb.MovePosition(next);

        if (Vector2.Distance(next, _target) < 0.01f)
        {
            _target = _target == _end ? _start : _end;
            _waitTimer = _waitTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D hit)
    {
        if (hit.collider.CompareTag("Player")) hit.transform.SetParent(transform);
    }

    private void OnCollisionExit2D(Collision2D hit)
    {
        if (hit.collider.CompareTag("Player")) hit.transform.SetParent(null);
    }

    private void OnDrawGizmos()
    {
        Vector3 from = Application.isPlaying ? (Vector3)_start : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(from, from + (Vector3)_offset);
        Gizmos.DrawWireCube(from + (Vector3)_offset, new Vector3(1f, 0.2f, 0f));
    }
}