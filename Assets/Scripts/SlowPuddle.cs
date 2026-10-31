using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SlowPuddle : MonoBehaviour
{
    private float _slowMultiplier = 0.5f;
    private float _slowDuration = 1f;

    // The creature calls this right after dropping the puddle
    public void Setup(float slowMultiplier, float slowDuration, float lifetime)
    {
        _slowMultiplier = slowMultiplier;
        _slowDuration = slowDuration;
        Destroy(gameObject, lifetime); // remove itself after some time
    }

    // Runs every physics step while something is inside the puddle
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController player))
            player.ApplySlow(_slowMultiplier, _slowDuration);
    }
}