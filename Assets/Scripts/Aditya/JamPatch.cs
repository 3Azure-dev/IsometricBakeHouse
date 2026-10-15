using UnityEngine;

// A jam patch placed in the level (Cupcake world).
// While the player stands in it, he moves slower. Creatures are not slowed.
// Pravin can tune both values in the Inspector.
[RequireComponent(typeof(Collider2D))]
public class JamPatch : MonoBehaviour
{
    [SerializeField, Range(0.1f, 1f)] private float _slowMultiplier = 0.5f; // 0.5 = half speed
    [SerializeField] private float _slowDuration = 0.3f; // slow lasts this long after leaving the jam
    [SerializeField] private bool _blocksJump = true;    // can't jump while in the jam

    // Runs every physics step while something is inside the jam
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            player.ApplySlow(_slowMultiplier, _slowDuration);
            if (_blocksJump) player.BlockJump(_slowDuration);
        }
    }
}
