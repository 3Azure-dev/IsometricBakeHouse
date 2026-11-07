using UnityEngine;

// Tomato Sauce: runs like Flour, throws bouncy droplets back at the player,
// and runs faster downhill
public class SauceCreature : IngredientCreature1
{
    [Header("Sauce droplets")]
    [SerializeField] private SauceDroplet _dropletPrefab;

    [Header("Downhill - Pravin can tune these")]
    [SerializeField] private float _downhillMultiplier = 1.3f;
    [Tooltip("Slopes flatter than this (degrees) count as flat ground")]
    [SerializeField] private float _minSlopeAngle = 5f;

    private float _dropTimer;

    protected override void UpdateFleeing()
    {
        base.UpdateFleeing();
        if (CurrentState != State.Fleeing) return;

        RunFasterDownhill();

        _dropTimer -= Time.fixedDeltaTime;
        if (_dropTimer > 0f) return;

        _dropTimer = _data.dropInterval;
        DropDroplets();
    }

    private void RunFasterDownhill()
    {
        float runX = _rb.linearVelocity.x;
        if (runX == 0f || !IsGrounded()) return;

        Bounds b = _col.bounds;
        RaycastHit2D hit = Physics2D.Raycast(b.center, Vector2.down, b.extents.y + 0.5f, _groundLayer);
        if (!hit) return;

        float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

        // the slope's normal leans toward the low side, so same sign as our run = going down
        bool downhill = slopeAngle > _minSlopeAngle && Mathf.Sign(hit.normal.x) == Mathf.Sign(runX);
        if (!downhill) return;

        _rb.linearVelocity = new Vector2(runX * _downhillMultiplier, _rb.linearVelocity.y);
    }

    private void DropDroplets()
    {
        float runDir = Mathf.Sign(transform.position.x - _player.position.x);

        for (int i = 0; i < _data.dropletsPerDrop; i++)
        {
            SauceDroplet drop = Instantiate(_dropletPrefab, transform.position, Quaternion.identity);
            Physics2D.IgnoreCollision(drop.GetComponent<Collider2D>(), _col);

            Vector2 velocity = new Vector2(
                -runDir * _data.dropletBackSpeed * Random.Range(0.7f, 1.3f),
                _data.dropletUpSpeed * Random.Range(0.8f, 1.2f));

            drop.Launch(velocity, _data.extraCooldown, _data.debuffTime, _data.dropletLifetime);
        }
    }
}