using UnityEngine;

// Pizza World hazard. The strip slows movement and gets hot on a repeating cycle.
[RequireComponent(typeof(Collider2D))]
public class MoltenCheesePool : MonoBehaviour
{
    [SerializeField] private MoltenCheeseData _data;
    [SerializeField] private Color _burnColor = new Color(1f, 0.45f, 0.1f);

    private SpriteRenderer _stripSprite;
    private Color _safeColor;
    private double _cycleStart;
    private bool _isBurning;
    private bool _burnedDuringContact;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        _stripSprite = GetComponent<SpriteRenderer>();
        if (_stripSprite != null) _safeColor = _stripSprite.color;
    }

    private void OnEnable()
    {
        _cycleStart = Time.fixedTimeAsDouble;
        _isBurning = false;
        _burnedDuringContact = false;
        if (_stripSprite != null) _stripSprite.color = _safeColor;
    }

    private void FixedUpdate()
    {
        if (_data == null) return;

        double cycleLength = (double)_data.safePhaseDuration + _data.burnPhaseDuration;
        double phaseTime = (Time.fixedTimeAsDouble - _cycleStart) % cycleLength;
        bool isBurning = phaseTime >= _data.safePhaseDuration;
        if (_isBurning == isBurning) return;

        _isBurning = isBurning;
        if (!_isBurning) _burnedDuringContact = false;
        if (_stripSprite != null) _stripSprite.color = _isBurning ? _burnColor : _safeColor;
    }

    private void OnTriggerEnter2D(Collider2D other) => ApplyHazard(other);
    private void OnTriggerStay2D(Collider2D other) => ApplyHazard(other);

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController _)) _burnedDuringContact = false;
    }

    private void ApplyHazard(Collider2D other)
    {
        if (!isActiveAndEnabled || _data == null || !other.TryGetComponent(out PlayerController player)) return;

        player.ApplySlow(_data.slowMultiplier, _data.slowDuration);
        if (!_isBurning || _burnedDuringContact || !player.IsGrounded) return;

        _burnedDuringContact = true;
        if (BreadSFX.Instance != null) BreadSFX.Instance.PlayMoltenCheeseBurn();

        GrandfatherDunk dunk = FindAnyObjectByType<GrandfatherDunk>();
        int lost = dunk != null ? dunk.LoseCaught(_data.ingredientsLost) : 0;
        Debug.Log($"Player burned in molten cheese, lost {lost} ingredient(s)");

        if (other.TryGetComponent(out PlayerRespawn respawn)) respawn.Respawn();
    }
}
