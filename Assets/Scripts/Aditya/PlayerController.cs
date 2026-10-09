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

    public PlayerState CurrentState { get; private set; }

    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private float _moveInput;
    private bool _jumpPressed;
    private bool _isGrounded;

    private float _slowMultiplier = 1f; // 1 = normal speed, 0.5 = half speed
    private float _slowTimer;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();
    }

    void Update()
    {
        _moveInput = 0f;
        if (Keyboard.current.aKey.isPressed) _moveInput -= 1f;
        if (Keyboard.current.dKey.isPressed) _moveInput += 1f;

        if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
            _jumpPressed = true;
    }

    void FixedUpdate()
    {
        _isGrounded = CheckGround();

        // count the slow down, then go back to normal speed
        if (_slowTimer > 0f)
        {
            _slowTimer -= Time.fixedDeltaTime;
            if (_slowTimer <= 0f) _slowMultiplier = 1f;
        }

        _rb.linearVelocity = new Vector2(_moveInput * _moveSpeed * _slowMultiplier, _rb.linearVelocity.y);

        if (_jumpPressed && _isGrounded)
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);

        _jumpPressed = false;

        UpdateState();
    }

    private bool CheckGround()
    {
        Vector2 feet = new Vector2(_col.bounds.center.x, _col.bounds.min.y);
        return Physics2D.OverlapBox(feet, _groundCheckSize, 0f, _groundLayer);
    }

    private void UpdateState()
    {
        if (_isGrounded)
            CurrentState = Mathf.Abs(_moveInput) > 0.01f ? PlayerState.Running : PlayerState.Idle;
        else
            CurrentState = _rb.linearVelocity.y > 0f ? PlayerState.Jumping : PlayerState.Falling;
    }

    // Anything can call this to slow the player for a short time
    public void ApplySlow(float multiplier, float duration)
    {
        _slowMultiplier = multiplier;
        _slowTimer = duration;
    }

    void OnDrawGizmosSelected()
    {
        if (_col == null) _col = GetComponent<CapsuleCollider2D>();
        Gizmos.color = Color.green;
        Vector2 feet = new Vector2(_col.bounds.center.x, _col.bounds.min.y);
        Gizmos.DrawWireCube(feet, _groundCheckSize);
    }
}