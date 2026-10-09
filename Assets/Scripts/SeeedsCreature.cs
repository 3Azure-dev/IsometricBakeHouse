using UnityEngine;

// Seeds = a normal creature that also drops slow puddles while running away
public class SeedsCreature : IngredientCreature1
{
    [Header("Slow Trail")]
    [SerializeField] private SlowPuddle _puddlePrefab;

    private float _dropTimer;

    protected override void UpdateFleeing()
    {
        base.UpdateFleeing(); // run away exactly like Flour

        if (CurrentState != State.Fleeing) return; // it just calmed down
        if (!IsGrounded()) return;                 // no puddles in the air

        _dropTimer -= Time.fixedDeltaTime;
        if (_dropTimer > 0f) return;

        _dropTimer = _data.dropInterval;
        DropPuddle();
    }

    private void DropPuddle()
    {
        Vector2 feet = new Vector2(_col.bounds.center.x, _col.bounds.min.y);
        SlowPuddle puddle = Instantiate(_puddlePrefab, feet, Quaternion.identity);
        puddle.Setup(_data.slowMultiplier, _data.slowDuration, _data.puddleLifetime);
    }
}