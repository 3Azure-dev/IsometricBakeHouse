using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class IngredientCreature1 : MonoBehaviour
{
    public enum State { Idle, Fleeing, Stunned, Caught }

    // the level tracker listens to these
    public static event Action<IngredientCreature1> CreatureCaught;
    public static event Action<IngredientCreature1> CreatureEscaped;

    [Header("Data - all game feel numbers live in this asset")]
    [SerializeField] protected IngredientData _data;

    [Header("Technical")]
    [SerializeField] protected LayerMask _groundLayer;
    [SerializeField] private float _lookAhead = 0.3f;
    [Tooltip("How deep below the feet a creature looks for an escape hole")]
    [SerializeField] private float _holeProbeDepth = 6f;

    [Header("Debug - read only")]
    [SerializeField] private State _state = State.Idle;

    protected Rigidbody2D _rb;
    protected Vector3 _spawnPoint;

    protected Collider2D _col;
    protected Transform _player;
    protected float _stunTimer;

    public State CurrentState => _state;

    public IngredientData Data => _data;

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _spawnPoint = transform.position;
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

        _rb.gravityScale = _data.gravityScale;
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

        PullIntoHole(); // runs last, so it wins over each creature's own movement
    }

    private void Update()
    {
        if (_state != State.Stunned) return;
        if (DistanceToPlayer() > _data.catchRange) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            Catch();
    }

    protected virtual void ChangeState(State newState)
    {
        if (newState == _state) return;
        _state = newState;
        Debug.Log($"{name} -> {newState}");
    }

    public virtual void Stun()
    {
        if (_state == State.Caught) return;

        if (_state != State.Stunned && BreadSFX.Instance != null)
        {
            if (IsPizzaLevel())
            {
                BreadSFX.Instance.PlayPizzaCreatureStunned();
            }
            else
            {
                BreadSFX.Instance.PlayCreatureStunned();
            }
        }

        _stunTimer = _data.stunDuration;
        ChangeState(State.Stunned);
    }

    protected virtual void Catch()
    {
        ChangeState(State.Caught);
        Debug.Log
        (
            $"CAUGHT: Creature={name}, " +
            $"DataAsset={_data.name}, " +
            $"Ingredient={_data.ingredientName}"
        );

        Inventory1.Instance.Add(_data, _data.ingredientsPerCatch);
        CreatureCaught?.Invoke(this);

        if (BreadSFX.Instance != null)
        {
            if (IsPizzaLevel())
            {
                BreadSFX.Instance.PlayPizzaCreatureCaught();
            }
            else
            {
                BreadSFX.Instance.PlayCreatureCaught();
            }
        }

        gameObject.SetActive(false);
    }

    public void ReturnToSpawn()
    {
        _rb.linearVelocity = Vector2.zero;
        transform.position = _spawnPoint;
    }

    [ContextMenu("Test: Escape")]
    public virtual void Escape()
    {
        if (_state == State.Caught) return;
        CreatureEscaped?.Invoke(this);
        gameObject.SetActive(false);
    }

    // ---------- States ----------

    private bool IsPizzaLevel()
    {
        return SceneManager.GetActiveScene().name.Contains("Pizza");
    }

    protected virtual void UpdateIdle()
    {
        StopMovingSideways();
        if (DistanceToPlayer() < _data.detectRange)
            ChangeState(State.Fleeing);
    }

    protected virtual void UpdateFleeing()
    {
        if (DistanceToPlayer() > _data.safeRange && IsGrounded())
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

    protected virtual void UpdateStunned()
    {
        StopMovingSideways();
        _stunTimer -= Time.fixedDeltaTime;
        if (_stunTimer <= 0f)
            ChangeState(State.Fleeing);
    }

    // ---------- Helpers ----------

    protected virtual float DistanceToPlayer()
    {
        if (_player == null) return Mathf.Infinity;
        return Vector2.Distance(transform.position, _player.position);
    }

    protected virtual void StopMovingSideways()
    {
        _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
    }

    protected void Jump()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _data.jumpForce);
    }

    protected bool IsGrounded()
    {
        Bounds b = _col.bounds;
        Vector2 feet = new Vector2(b.center.x, b.min.y);
        return Physics2D.OverlapBox(feet, new Vector2(b.size.x * 0.9f, 0.1f), 0f, _groundLayer);
    }

    protected bool WallAhead(float dir)
    {
        Bounds b = _col.bounds;
        Vector2 origin = new Vector2(b.center.x, b.min.y + 0.1f); // ray at foot height
        return Physics2D.Raycast(origin, Vector2.right * dir, b.extents.x + _lookAhead, _groundLayer);
    }

    protected bool GapAhead(float dir)
    {
        Bounds b = _col.bounds;
        Vector2 origin = new Vector2(b.center.x + dir * (b.extents.x + _lookAhead), b.min.y + 0.1f);
        if (Physics2D.Raycast(origin, Vector2.down, 1f, _groundLayer)) return false; // ground below = no gap

        // an escape hole is not treated as a gap, so every creature (Sugar, Butter, Egg...)
        // keeps running and drops into it instead of jumping over
        return !EscapeHoleAhead(dir);
    }

    // Is the gap ahead an escape hole? (an EscapeZone with Is Hole ticked)
    protected bool EscapeHoleAhead(float dir)
    {
        Bounds b = _col.bounds;
        return EscapeHoleAt(b.center.x + dir * (b.extents.x + _lookAhead));
    }

    // Is there an escape hole straight under this x position?
    protected bool EscapeHoleAt(float x)
    {
        Bounds b = _col.bounds;
        // look straight down, up to _holeProbeDepth below the feet
        Vector2 center = new Vector2(x, b.min.y - _holeProbeDepth * 0.5f);
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, new Vector2(0.5f, _holeProbeDepth), 0f);

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent(out EscapeZone zone) && zone.IsHole)
                return true;
        }
        return false;
    }

    // In the air over an escape hole? Drop straight down into it.
    // Fast creatures (Sugar dash, Butter slide) would otherwise fly over small holes.
    private void PullIntoHole()
    {
        if (_state == State.Caught || IsGrounded()) return;
        if (!EscapeHoleAt(_col.bounds.center.x)) return;

        _rb.linearVelocity = new Vector2(0f, Mathf.Min(_rb.linearVelocity.y, 0f));
    }

    protected bool CeilingAhead(float dir)
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