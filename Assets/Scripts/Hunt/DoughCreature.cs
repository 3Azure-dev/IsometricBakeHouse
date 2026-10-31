using System.Collections;
using UnityEngine;

// Dough: every few seconds it leaves a look-alike clone.
// The clone runs at the player and blasts when close or when shot (creeper style, no melee).
// Clones are not one of the level's creatures.
public class DoughCreature : IngredientCreature1
{
    [Header("Clone - Pravin can tune these (T31, T32)")]
    [SerializeField] private float _cloneInterval = 5f;
    [SerializeField] private float _cloneLifetime = 3f;

    [Header("Clone chase - Pravin can tune these")]
    [SerializeField] private float _chaseSpeed = 6f;
    [Tooltip("Fuse starts when the player is this close. Keep it smaller than Blast Range")]
    [SerializeField] private float _fuseRange = 1.5f;
    [Tooltip("Seconds of flashing before the blast. 0 = blast right away")]
    [SerializeField] private float _fuseTime = 0.5f;
    [SerializeField] private float _flashSpeed = 12f;
    [SerializeField] private Color _flashColor = Color.white;

    [Header("Clone blast - Pravin can tune these (T38)")]
    [SerializeField] private float _blastRange = 3f;
    [SerializeField] private float _blastForce = 10f;
    [SerializeField] private float _blastUp = 2f;
    [SerializeField] private float _blastTime = 0.3f;

    private bool _isClone;
    private bool _fusing;
    private bool _blasted;
    private float _cloneTimer;

    protected override void Awake()
    {
        base.Awake();
        _cloneTimer = _cloneInterval;
    }

    public override void Stun()
    {
        if (_isClone) { Blast(); return; }
        base.Stun();
    }

    public override void Escape()
    {
        if (_isClone) { Destroy(gameObject); return; }
        base.Escape();
    }

    protected override void Catch()
    {
        if (_isClone) return;
        base.Catch();
    }

    protected override void UpdateIdle()
    {
        if (_isClone) { Chase(); return; }
        base.UpdateIdle();
    }

    protected override void UpdateFleeing()
    {
        if (_isClone) { Chase(); return; }

        base.UpdateFleeing();
        if (CurrentState != State.Fleeing) return;

        _cloneTimer -= Time.fixedDeltaTime;
        if (_cloneTimer > 0f) return;

        _cloneTimer = _cloneInterval;
        MakeClone();
    }

    private void Chase()
    {
        if (_player == null) return;
        if (_fusing) { StopMovingSideways(); return; }

        if (DistanceToPlayer() < _fuseRange)
        {
            StartCoroutine(Fuse());
            return;
        }

        float dir = Mathf.Sign(_player.position.x - transform.position.x);
        _rb.linearVelocity = new Vector2(dir * _chaseSpeed, _rb.linearVelocity.y);

        if (IsGrounded() && (WallAhead(dir) || (GapAhead(dir) && !CeilingAhead(dir))))
            Jump();
    }

    private IEnumerator Fuse()
    {
        _fusing = true;

        SpriteRenderer sprite = GetComponentInChildren<SpriteRenderer>();
        Color normal = sprite != null ? sprite.color : Color.white;

        for (float t = 0f; t < _fuseTime; t += Time.deltaTime)
        {
            if (sprite != null)
                sprite.color = Color.Lerp(normal, _flashColor, Mathf.PingPong(t * _flashSpeed, 1f));
            yield return null;
        }

        Blast();
    }

    private void MakeClone()
    {
        float dir = Mathf.Sign(transform.position.x - _player.position.x);
        Vector3 pos = transform.position - new Vector3(dir * 0.6f, 0f, 0f);

        DoughCreature clone = Instantiate(this, pos, Quaternion.identity);
        clone._isClone = true;
        clone.name = "Creature_Dough(Clone)";
        Destroy(clone.gameObject, _cloneLifetime);
    }

    private void Blast()
    {
        if (_blasted) return;
        _blasted = true;

        if (_player != null && DistanceToPlayer() < _blastRange
            && _player.TryGetComponent(out PlayerController player))
        {
            float dir = Mathf.Sign(_player.position.x - transform.position.x);
            player.Knockback(new Vector2(dir * _blastForce, _blastUp), _blastTime);
        }
        Destroy(gameObject);
    }
}