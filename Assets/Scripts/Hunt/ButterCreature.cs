using UnityEngine;

// Butter: slides (slow to speed up, slow to stop) and drops butter blobs
// that stop the player walking forward.
public class ButterCreature : IngredientCreature1
{
    [Header("Butter - Pravin can tune these")]
    [SerializeField] private ButterTrail _trailPrefab;
    [Tooltip("Lower = more slippery")]
    [SerializeField] private float _slideGrip = 10f;

    private float _dropTimer;

    protected override void UpdateFleeing()
    {
        if (DistanceToPlayer() > _data.safeRange && IsGrounded())
        {
            ChangeState(State.Idle);
            return;
        }

        float direction = Mathf.Sign(transform.position.x - _player.position.x);

        // slide: speed builds up bit by bit instead of changing instantly
        float targetSpeed = direction * _data.fleeSpeed;
        float newX = Mathf.MoveTowards(_rb.linearVelocity.x, targetSpeed, _slideGrip * Time.fixedDeltaTime);
        _rb.linearVelocity = new Vector2(newX, _rb.linearVelocity.y);

        if (IsGrounded())
        {
            if (WallAhead(direction)) Jump();
            else if (GapAhead(direction) && !CeilingAhead(direction)) Jump();

            DropTrail(direction);
        }
    }

    // stopping is also a slide, not an instant stop
    protected override void StopMovingSideways()
    {
        float newX = Mathf.MoveTowards(_rb.linearVelocity.x, 0f, _slideGrip * Time.fixedDeltaTime);
        _rb.linearVelocity = new Vector2(newX, _rb.linearVelocity.y);
    }

    private void DropTrail(float direction)
    {
        _dropTimer -= Time.fixedDeltaTime;
        if (_dropTimer > 0f) return;
        _dropTimer = _data.dropInterval;

        // spawn at the creature's feet first
        Vector2 feet = new Vector2(_col.bounds.center.x, _col.bounds.min.y);
        ButterTrail trail = Instantiate(_trailPrefab, feet, Quaternion.identity);

        // then lift it so its bottom edge sits exactly on the floor, whatever its size
        Renderer rend = trail.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            float gap = trail.transform.position.y - rend.bounds.min.y;
            trail.transform.position += Vector3.up * gap;
        }

        trail.Setup(direction, _data.puddleLifetime);

        // Play the sound when the Butter trail is created.
        if (BreadSFX.Instance != null)
        {
            BreadSFX.Instance.PlayButterTrail();
        }
    }
}