using UnityEngine;

// A shell the Egg throws along the floor. It stuns the player on touch.
// The player can jump over it.
[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class EggShell : MonoBehaviour
{
    [SerializeField] private float _spinSpeed = 360f; // degrees per second

    private float _stunDuration;

    // The Egg calls this right after throwing the shell
    public void Setup(float direction, float speed, float stunDuration, float lifetime)
    {
        _stunDuration = stunDuration;

        // let the physics engine move and spin it, so touches are never missed
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.right * (direction * speed);
        rb.angularVelocity = -direction * _spinSpeed;

        Destroy(gameObject, lifetime); // remove itself if it hits nothing
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            // Play the sound when the Eggshell hits the player.
            if (BreadSFX.Instance != null)
            {
                BreadSFX.Instance.PlayEggshellHit();
            }

            // Stun the player.
            player.Stun(_stunDuration);

            // Remove the Eggshell after the hit.
            Destroy(gameObject);
        }
    }
}