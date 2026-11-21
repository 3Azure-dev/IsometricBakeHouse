using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class SauceDroplet : MonoBehaviour
{
    private float _extraCooldown;
    private float _debuffTime;
    private bool _hit;
    public void Launch(Vector2 velocity, float extraCooldown, float debuffTime, float lifetime)
    {
        GetComponent<Rigidbody2D>().linearVelocity = velocity;
        _extraCooldown = extraCooldown;
        _debuffTime = debuffTime;
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter2D(Collision2D hit)
    {
        if (_hit) return;
        if (!hit.collider.CompareTag("Player")) return;
        _hit = true;

        WandFire1 wand = hit.collider.GetComponentInChildren<WandFire1>();
        if (wand == null) wand = FindAnyObjectByType<WandFire1>();
        if (wand != null) wand.AddStickySauce(_extraCooldown, _debuffTime);

        if (BreadSFX.Instance != null)
            BreadSFX.Instance.PlaySauceDropletHit();

        Destroy(gameObject);
    }
}