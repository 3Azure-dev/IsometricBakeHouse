using UnityEngine;

// Any creature that touches this is gone and counts as escaped.
// Used for the two level borders, and for escape holes in the ground (Cupcake world).
[RequireComponent(typeof(Collider2D))]
public class EscapeZone : MonoBehaviour
{
    [Tooltip("Tick for a hole in the ground. Fleeing creatures run INTO it instead of jumping over it.")]
    [SerializeField] protected bool _isHole = false;

    public bool IsHole => _isHole;

    private Collider2D _col;

    private void Awake()
    {
        _col = GetComponent<Collider2D>();
    }

    private void Reset()
    {
        // runs when the script is added in the editor
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IngredientCreature1 creature))
            creature.Escape();
    }

    // A player who drops fully into a hole goes back to the latest zone
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!_isHole) return;
        if (!other.TryGetComponent(out PlayerRespawn player)) return;

        // only when his head is below the top of the hole, so walking past the edge is safe
        if (other.bounds.max.y < _col.bounds.max.y)
            player.Respawn();
    }
}