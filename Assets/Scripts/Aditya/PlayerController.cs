using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState { Idle, Running, Jumping, Falling }

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private float _jumpForce = 14f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.8f, 0.1f);

    [Header("Stun Visual")]
    [SerializeField] private GameObject _stunIndicator;

    public PlayerState CurrentState { get; private set; }

    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private float _moveInput;
    private bool _jumpPressed;
    private bool _isGrounded;

    private float _slowMultiplier = 1f;
    private float _slowTimer;

    private float _blockedDirection;
    private float _blockTimer;

    private float _stunTimer;
    private float _noJumpTimer;

    private float _knockbackTimer;

    public bool IsStunned => _stunTimer > 0f;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();

        // Hide the stun icon when the level starts.
        UpdateStunIndicator();
    }

    void Update()
    {
        _moveInput = 0f;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.aKey.isPressed) _moveInput -= 1f;
        if (Keyboard.current.dKey.isPressed) _moveInput += 1f;

        if (Keyboard.current.spaceKey.wasPressedThisFrame ||
            Keyboard.current.wKey.wasPressedThisFrame)
        {
            _jumpPressed = true;
        }
    }

    void FixedUpdate()
    {
        _isGrounded = CheckGround();

        // Knockback: let the push play out and ignore input.
        if (_knockbackTimer > 0f)
        {
            _knockbackTimer -= Time.fixedDeltaTime;
            _jumpPressed = false;
            UpdateState();
            return;
        }

        // Count the slow down, then return to normal speed.
        if (_slowTimer > 0f)
        {
            _slowTimer -= Time.fixedDeltaTime;

            if (_slowTimer <= 0f)
                _slowMultiplier = 1f;
        }

        // While blocked, ignore input in the blocked direction.
        float move = _moveInput;

        if (_blockTimer > 0f)
        {
            _blockTimer -= Time.fixedDeltaTime;

            if (move != 0f && Mathf.Sign(move) == _blockedDirection)
                move = 0f;
        }

        // While stunned, disable walking and jumping.
        if (_stunTimer > 0f)
        {
            _stunTimer -= Time.fixedDeltaTime;
            move = 0f;
            _jumpPressed = false;
        }

        // Update the icon as the stun timer counts down.
        UpdateStunIndicator();

        // While stuck in jam, disable jumping.
        if (_noJumpTimer > 0f)
        {
            _noJumpTimer -= Time.fixedDeltaTime;
            _jumpPressed = false;
        }

        _rb.linearVelocity = new Vector2(
            move * _moveSpeed * _slowMultiplier,
            _rb.linearVelocity.y
        );

        if (_jumpPressed && _isGrounded)
        {
            _rb.linearVelocity = new Vector2(
                _rb.linearVelocity.x,
                _jumpForce
            );
        }

        _jumpPressed = false;

        UpdateState();
    }

    private bool CheckGround()
    {
        Vector2 feet = new Vector2(
            _col.bounds.center.x,
            _col.bounds.min.y
        );

        return Physics2D.OverlapBox(
            feet,
            _groundCheckSize,
            0f,
            _groundLayer
        );
    }

    private void UpdateState()
    {
        if (_isGrounded)
        {
            CurrentState = Mathf.Abs(_moveInput) > 0.01f
                ? PlayerState.Running
                : PlayerState.Idle;
        }
        else
        {
            CurrentState = _rb.linearVelocity.y > 0f
                ? PlayerState.Jumping
                : PlayerState.Falling;
        }
    }

    // Show or hide the icon based on the current stun state.
    private void UpdateStunIndicator()
    {
        if (_stunIndicator == null)
            return;

        bool shouldBeVisible = IsStunned;

        if (_stunIndicator.activeSelf != shouldBeVisible)
        {
            _stunIndicator.SetActive(shouldBeVisible);
        }
    }

    // Anything can call this to slow the player.
    public void ApplySlow(float multiplier, float duration)
    {
        _slowMultiplier = multiplier;
        _slowTimer = duration;
    }

    // Anything can call this to block movement in one direction.
    public void BlockDirection(float direction, float duration)
    {
        _blockedDirection = Mathf.Sign(direction);
        _blockTimer = duration;
    }

    // Anything can call this to stop the player jumping.
    public void BlockJump(float duration)
    {
        _noJumpTimer = duration;
    }

    // Anything can call this to freeze the player.
    public void Stun(float duration)
    {
        _stunTimer = Mathf.Max(0f, duration);

        // Show the icon immediately when the stun begins.
        UpdateStunIndicator();

        Debug.Log($"Player stunned for {duration} sec");
    }

    // Anything can call this to push the player.
    public void Knockback(Vector2 velocity, float duration)
    {
        _rb.linearVelocity = velocity;
        _knockbackTimer = duration;
    }

    void OnDrawGizmosSelected()
    {
        if (_col == null)
            _col = GetComponent<CapsuleCollider2D>();

        if (_col == null)
            return;

        Gizmos.color = Color.green;

        Vector2 feet = new Vector2(
            _col.bounds.center.x,
            _col.bounds.min.y
        );

        Gizmos.DrawWireCube(feet, _groundCheckSize);
    }
}