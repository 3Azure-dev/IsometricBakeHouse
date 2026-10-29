using UnityEngine;

// Sugar: stands still, then dashes away in short bursts.
public class SugarCreaturedashbackup : IngredientCreature1
{
    [Header("Sugar dash - Pravin can tune these")]
    [SerializeField] private float _dashSpeed = 12f;
    [SerializeField] private float _dashTime = 0.3f;
    [SerializeField] private float _restTime = 1f; // stands still between dashes

    private float _dashTimer;
    private float _restTimer;
    private float _dashDirection;

    // Idle: stand still. Dash when the player is close and the rest is over.
    protected override void UpdateIdle()
    {
        StopMovingSideways();

        _restTimer -= Time.fixedDeltaTime;
        if (_restTimer > 0f) return;

        if (DistanceToPlayer() < _data.detectRange)
        {
            _dashDirection = Mathf.Sign(transform.position.x - _player.position.x);
            _dashTimer = _dashTime;
            ChangeState(State.Fleeing);
        }
    }

    // Fleeing: one short dash, then back to standing still.
    protected override void UpdateFleeing()
    {
        _dashTimer -= Time.fixedDeltaTime;

        if (_dashTimer <= 0f && IsGrounded())
        {
            _restTimer = _restTime;
            ChangeState(State.Idle);
            return;
        }

        _rb.linearVelocity = new Vector2(_dashDirection * _dashSpeed, _rb.linearVelocity.y);

        if (IsGrounded() && (WallAhead(_dashDirection) || GapAhead(_dashDirection)))
            Jump();
    }
}