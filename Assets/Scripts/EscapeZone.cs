using UnityEngine;

// Any creature that touches this is gone and counts as escaped.
// Used for the two level borders and for the pit under the gaps.
[RequireComponent(typeof(Collider2D))]
public class EscapeZone : MonoBehaviour
{
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
}