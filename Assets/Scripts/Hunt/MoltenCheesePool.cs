using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MoltenCheesePool : MonoBehaviour
{
    [SerializeField] private MoltenCheeseData _data;
    [SerializeField] private Color _burnColor = new Color(1f, 0.45f, 0.1f);

    private SpriteRenderer _strip;
    private Color _safeColor;
    private double _cycleStart;
    private bool _isBurning;
    private bool _burnedDuringContact;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        _strip = GetComponent<SpriteRenderer>();
        if (_strip != null)
            _safeColor = _strip.color;
    }

    private void OnEnable()
    {
        _cycleStart = Time.fixedTimeAsDouble;
        _isBurning = false;
        _burnedDuringContact = false;

        if (_strip != null)
            _strip.color = _safeColor;
    }

    private void FixedUpdate()
    {
        if (_data == null) return;

        double cycleLength = (double)_data.safePhaseDuration + _data.burnPhaseDuration;
        double phaseTime = (Time.fixedTimeAsDouble - _cycleStart) % cycleLength;
        bool isBurning = phaseTime >= _data.safePhaseDuration;

        if (isBurning == _isBurning) return;

        _isBurning = isBurning;
        if (!_isBurning)
            _burnedDuringContact = false;

        if (_strip != null)
            _strip.color = _isBurning ? _burnColor : _safeColor;
    }

    private void OnTriggerEnter2D(Collider2D other) => ApplyHazard(other);
    private void OnTriggerStay2D(Collider2D other) => ApplyHazard(other);

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController _))
            _burnedDuringContact = false;
    }

    private void ApplyHazard(Collider2D other)
    {
        if (!isActiveAndEnabled || _data == null) return;
        if (!other.TryGetComponent(out PlayerController player)) return;

        player.ApplySlow(_data.slowMultiplier, _data.slowDuration);

        if (!_isBurning || _burnedDuringContact || !player.IsGrounded) return;
        if (!other.TryGetComponent(out PlayerRespawn respawn)) return;

        _burnedDuringContact = true;

        GrandfatherDunk dunk = FindAnyObjectByType<GrandfatherDunk>();
        if (dunk != null)
            dunk.LoseCaught(_data.ingredientsLost);

        if (BreadSFX.Instance != null)
            BreadSFX.Instance.PlayMoltenCheeseBurn();

        respawn.Respawn();
    }
}