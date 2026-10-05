using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class IngredientCreature : MonoBehaviour
{
    public enum State { Idle, Fleeing, Stunned, Caught }

    [Header("Data")]
    [SerializeField] private IngredientData _data;


    [Header("Flee")]
    [SerializeField] private float _detectRange = 4f; // starts fleeing inside this
    [SerializeField] private float _safeRange = 7f;   // calms down outside this


    [Header("Jump")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _jumpForce = 8f;
    [SerializeField] private float _lookAhead = 0.3f;

    [Header("Stun")]
    [SerializeField] private float _stunDuration = 2f;
    [SerializeField] private float _catchRange = 1.5f; // how close the player must be to press E


    [Header("Debug - read only")]
    [SerializeField] private State _state = State.Idle;

    private Rigidbody2D _rb;
    private Collider2D _col;
    private Transform _player;
    private float _fleeSpeed;

    private float _stunTimer;

    public State CurrentState => _state;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (_data == null)
        {
            Debug.LogError($"{name}: no IngredientData assigned!");
            enabled = false;
            return;
        }
        _fleeSpeed = _data.fleeSpeed;

        GameObject playerObj = GameObject.FindWithTag("Player"); // runs once, not every frame
        if (playerObj != null) _player = playerObj.transform;
        else Debug.LogError($"{name}: no object tagged Player found!");
    }

    private void FixedUpdate()
    {
        switch (_state)
        {
            case State.Idle:    UpdateIdle();    break;
            case State.Fleeing: UpdateFleeing(); break;
            case State.Stunned: UpdateStunned(); break;
            case State.Caught:  break;
        }
    }

    private void ChangeState(State newState)
    {
        if (newState == _state) return;
        _state = newState;
        Debug.Log($"{name} -> {newState}");
    }

    public void Stun()
    {
        if (_state == State.Caught) return; // already caught, ignore
        _stunTimer = _stunDuration;         // zapping again resets the timer
        ChangeState(State.Stunned);
    }

    private void Catch()
    {
        ChangeState(State.Caught);
        Inventory.Instance.Add(_data);   // 1 ingredient for now, catch bonus comes later
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_state != State.Stunned) return;
        if (DistanceToPlayer() > _catchRange) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            Catch();
    }

    // ---------- States ----------

    private void UpdateIdle()
    {
        StopMovingSideways();
        if (DistanceToPlayer() < _detectRange)
            ChangeState(State.Fleeing);
    }

    private void UpdateFleeing()
    {
        if (DistanceToPlayer() > _safeRange)
        {
            ChangeState(State.Idle);
            return;
        }

        // +1 if player is on our left (run right), -1 if on our right (run left)
        float direction = Mathf.Sign(transform.position.x - _player.position.x);
        _rb.linearVelocity = new Vector2(direction * _fleeSpeed, _rb.linearVelocity.y);

        if (IsGrounded())
        {
            if (WallAhead(direction))
                Jump();
            else if (GapAhead(direction))
            {
                if (CeilingAhead(direction))
                    StopMovingSideways(); // no room to jump: wait at the edge
                else
                    Jump();
            }
        }
    }

    private void UpdateStunned()
    {
        StopMovingSideways();
        _stunTimer -= Time.fixedDeltaTime;
        if (_stunTimer <= 0f)
            ChangeState(State.Fleeing);
    }

    // ---------- Helpers ----------

    private float DistanceToPlayer()
    {
        if (_player == null) return Mathf.Infinity;
        return Vector2.Distance(transform.position, _player.position);
    }

    private void StopMovingSideways()
    {
        _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _safeRange);
    }

        private bool IsGrounded()
    {
        Bounds b = _col.bounds;
        Vector2 feet = new Vector2(b.center.x, b.min.y);
        return Physics2D.OverlapBox(feet, new Vector2(b.size.x * 0.9f, 0.1f), 0f, _groundLayer);
    }

    private bool WallAhead(float dir)
    {
        Bounds b = _col.bounds;
        Vector2 origin = new Vector2(b.center.x, b.min.y + 0.1f); // ray at foot height
        return Physics2D.Raycast(origin, Vector2.right * dir, b.extents.x + _lookAhead, _groundLayer);
    }

    private bool GapAhead(float dir)
    {
        Bounds b = _col.bounds;
        Vector2 origin = new Vector2(b.center.x + dir * (b.extents.x + _lookAhead), b.min.y + 0.1f);
        return !Physics2D.Raycast(origin, Vector2.down, 1f, _groundLayer); // nothing below = gap
    }

    private void Jump()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
    }

        private bool CeilingAhead(float dir)
    {
        Bounds b = _col.bounds;

        // how high the jump goes: v^2 / (2 * gravity)
        float gravity = Mathf.Abs(Physics2D.gravity.y) * _rb.gravityScale;
        float jumpHeight = (_jumpForce * _jumpForce) / (2f * gravity);

        // a box sitting above the head, reaching forward toward the gap
        Vector2 center = new Vector2(b.center.x + dir * b.size.x, b.max.y + jumpHeight * 0.5f);
        Vector2 size = new Vector2(b.size.x * 2f, jumpHeight);
        return Physics2D.OverlapBox(center, size, 0f, _groundLayer);
    }
}