using UnityEngine;

// Tomato Sauce: runs like Flour and throws bouncy droplets back at the player
public class SauceCreature : IngredientCreature1
{
    [Header("Sauce droplets")]
    [SerializeField] private SauceDroplet _dropletPrefab;

    private float _dropTimer;

    protected override void UpdateFleeing()
    {
        base.UpdateFleeing();
        if (CurrentState != State.Fleeing) return;

        _dropTimer -= Time.fixedDeltaTime;
        if (_dropTimer > 0f) return;

        _dropTimer = _data.dropInterval;
        DropDroplets();
    }

    private void DropDroplets()
    {
        float runDir = Mathf.Sign(transform.position.x - _player.position.x);

        for (int i = 0; i < _data.dropletsPerDrop; i++)
        {
            SauceDroplet drop = Instantiate(_dropletPrefab, transform.position, Quaternion.identity);
            Physics2D.IgnoreCollision(drop.GetComponent<Collider2D>(), _col);

            Vector2 velocity = new Vector2(
                -runDir * _data.dropletBackSpeed * Random.Range(0.7f, 1.3f),
                _data.dropletUpSpeed * Random.Range(0.8f, 1.2f));

            drop.Launch(velocity, _data.extraCooldown, _data.debuffTime, _data.dropletLifetime);
        }
    }
}