using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class JamPatch : MonoBehaviour
{
    [SerializeField, Range(0.1f, 1f)] private float _slowMultiplier = 0.5f;
    [SerializeField] private float _slowDuration = 0.3f;
    [SerializeField] private bool _blocksJump = true;

    [Header("Walk sound - Pravin can tune these")]
    [SerializeField] private float _stepInterval = 0.4f;      // seconds between squelch sounds
    [SerializeField] private float _minMoveDistance = 0.005f; // below this = standing still

    private Collider2D _col;
    private float _nextStepTime;
    private float _lastPlayerX;

    private void Awake() => _col = GetComponent<Collider2D>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController _))
            _lastPlayerX = other.transform.position.x;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            player.ApplySlow(_slowMultiplier, _slowDuration);
            if (_blocksJump) player.BlockJump(_slowDuration);

            // sound only while the player's feet are really inside the jam
            Bounds pb = other.bounds;
            Vector2 feet = new Vector2(pb.center.x, pb.min.y + 0.05f);
            bool feetInside = _col.OverlapPoint(feet);

            // and only while the player is actually walking
            float x = other.transform.position.x;
            bool isMoving = Mathf.Abs(x - _lastPlayerX) > _minMoveDistance;
            _lastPlayerX = x;

            if (feetInside && isMoving && Time.time >= _nextStepTime)
            {
                _nextStepTime = Time.time + _stepInterval;
                if (BreadSFX.Instance != null)
                    BreadSFX.Instance.PlayJamPatchStick();
            }
        }
    }
}