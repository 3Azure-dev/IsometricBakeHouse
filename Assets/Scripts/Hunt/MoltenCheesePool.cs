using UnityEngine;

// Molten cheese (Pizza world). Slows the player a bit.
// Too long on the ground in it = burn: lose ingredients from this level and respawn at the last zone.
[RequireComponent(typeof(Collider2D))]
public class MoltenCheesePool : MonoBehaviour
{
    [SerializeField] private MoltenCheeseData _data;
    [SerializeField] private Color _burnColor = new Color(1f, 0.45f, 0.1f);

    private float _heat;
    private SpriteRenderer _playerSprite;
    private Color _playerColor;

    private void Awake() => GetComponent<Collider2D>().isTrigger = true;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerController player)) return;

        player.ApplySlow(_data.slowMultiplier, _data.slowDuration);

        bool grounded = player.CurrentState == PlayerController.PlayerState.Idle
                     || player.CurrentState == PlayerController.PlayerState.Running;

        if (grounded) _heat += Time.fixedDeltaTime;
        else _heat = Mathf.Max(0f, _heat - _data.coolSpeed * Time.fixedDeltaTime);

        Tint(other, _heat / _data.burnTime);

        if (_heat >= _data.burnTime) Burn(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerController _)) return;
        _heat = 0f;
        Tint(other, 0f);
    }

    private void Burn(Collider2D player)
    {
        _heat = 0f;
        Tint(player, 0f);

        GrandfatherDunk dunk = FindAnyObjectByType<GrandfatherDunk>();
        int lost = dunk != null ? dunk.LoseCaught(_data.ingredientsLost) : 0;
        Debug.Log($"Player burned in molten cheese, lost {lost} ingredient(s)");

        if (player.TryGetComponent(out PlayerRespawn respawn)) respawn.Respawn();
    }

    private void Tint(Collider2D player, float amount)
    {
        if (_playerSprite == null)
        {
            _playerSprite = player.GetComponentInChildren<SpriteRenderer>();
            if (_playerSprite == null) return;
            _playerColor = _playerSprite.color;
        }
        _playerSprite.color = Color.Lerp(_playerColor, _burnColor, amount);
    }
}