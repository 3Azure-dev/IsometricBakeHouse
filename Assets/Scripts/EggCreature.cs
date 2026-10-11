using UnityEngine;

// Egg: runs away like Flour, and throws shells back at the player's legs.
public class EggCreature : IngredientCreature1
{
    [Header("Egg shell - Pravin can tune these")]
    [SerializeField] private EggShell _shellPrefab;
    [SerializeField] private float _throwInterval = 2f;   // seconds between throws
    [SerializeField] private float _shellSpeed = 8f;
    [SerializeField] private float _playerStunTime = 1f;  // how long the player is frozen
    [SerializeField] private float _shellLifetime = 3f;

    private float _throwTimer;

    protected override void UpdateFleeing()
    {
        base.UpdateFleeing(); // run away exactly like Flour

        if (CurrentState != State.Fleeing) return; // it just calmed down
        if (!IsGrounded()) return;                 // no throwing in the air

        _throwTimer -= Time.fixedDeltaTime;
        if (_throwTimer > 0f) return;

        _throwTimer = _throwInterval;
        ThrowShell();
    }

    private void ThrowShell()
    {
        float toPlayer = Mathf.Sign(_player.position.x - transform.position.x);

        // leg level: just above the floor
        Vector2 spot = new Vector2(_col.bounds.center.x, _col.bounds.min.y + 0.2f);

        EggShell shell = Instantiate(_shellPrefab, spot, Quaternion.identity);
        shell.Setup(toPlayer, _shellSpeed, _playerStunTime, _shellLifetime);
    }
}