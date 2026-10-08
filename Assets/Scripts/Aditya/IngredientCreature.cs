using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class IngredientCreature1 : MonoBehaviour
{
    public enum State { Idle, Fleeing, Stunned, Caught }

    [Header("Data - all game feel numbers live in this asset")]
    [SerializeField] private IngredientData _data;

    [Header("Technical")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _lookAhead = 0.3f;

    [Header("Debug - read only")]
    [SerializeField] private State _state = State.Idle;

    private Rigidbody2D _rb;
    private Collider2D _col;
    private Transform _player;
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

    private void Update()
    {
        if (_state != State.Stunned) return;
        if (DistanceToPlayer() > _data.catchRange) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            Catch();
    }

    private void ChangeState(State newState)
    {
        if (newState == _state) return;
        _state = newState;
        Debug.Log($"{name} -> {newState}");
    }

    public void Stun()
    {
        if (_state == State.Caught) return;   // already caught, ignore
        _stunTimer = _data.stunDuration;      // zapping again resets the timer
        ChangeState(State.Stunned);
    }

    private void Catch()
    {
        ChangeState(State.Caught);
        Inventory1.Instance.Add(_data, _data.ingredientsPerCatch);
        gameObject.SetActive(false);
    }

    // ---------- States ----------

    private void UpdateIdle()
    {
        StopMovingSideways();
        if (DistanceToPlayer() < _data.detectRange)
            ChangeState(State.Fleeing);
    }

    private void UpdateFleeing()
    {
        if (DistanceToPlayer() > _data.safeRange)
        {
            ChangeState(State.Idle);
            return;
        }

        // +1 if player is on our left (run right), -1 if on our right (run left)
        float direction = Mathf.Sign(transform.position.x - _player.position.x);
        _rb.linearVelocity = new Vector2(direction * _data.fleeSpeed, _rb.linearVelocity.y);

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

    private void Jump()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _data.jumpForce);
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

    private bool CeilingAhead(float dir)
    {
        Bounds b = _col.bounds;

        // how high the jump goes: v^2 / (2 * gravity)
        float gravity = Mathf.Abs(Physics2D.gravity.y) * _rb.gravityScale;
        float jumpHeight = (_data.jumpForce * _data.jumpForce) / (2f * gravity);

        // a box sitting above the head, reaching forward toward the gap
        Vector2 center = new Vector2(b.center.x + dir * b.size.x, b.max.y + jumpHeight * 0.5f);
        Vector2 size = new Vector2(b.size.x * 2f, jumpHeight);
        return Physics2D.OverlapBox(center, size, 0f, _groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        if (_data == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _data.detectRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _data.safeRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _data.catchRange);
    }
}