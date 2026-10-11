using UnityEngine;

// A blob of butter. While the player touches it,
// he can't walk forward through it. He can go back or jump over.
[RequireComponent(typeof(Collider2D))]
public class ButterTrail : MonoBehaviour
{
    private float _blockDirection = 1f;

    // The creature calls this right after dropping the blob
    public void Setup(float blockDirection, float lifetime)
    {
        _blockDirection = blockDirection;
        Destroy(gameObject, lifetime); // remove itself after some time
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController player))
            player.BlockDirection(_blockDirection, 0.1f);
    }
}