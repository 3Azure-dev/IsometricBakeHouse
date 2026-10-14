using UnityEngine;

// A creature that falls in a gap is not escaped.
// It goes back to the hole it spawned from.
// The player goes back to just before the latest zone he entered.
[RequireComponent(typeof(Collider2D))]
public class RespawnPit : MonoBehaviour
{
    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IngredientCreature1 creature))
            creature.ReturnToSpawn();
        else if (other.TryGetComponent(out PlayerRespawn player))
            player.Respawn();
    }
}