using UnityEngine;

// Dough: every few seconds it leaves a look-alike clone that also runs.
// Clones are not one of the level's creatures. Hitting one blasts the player back.
public class DoughCreature : IngredientCreature1
{
    [Header("Clone - Pravin can tune these (T31, T32)")]
    [SerializeField] private float _cloneInterval = 5f;
    [SerializeField] private float _cloneLifetime = 3f;

    [Header("Clone blast - Pravin can tune these (T38)")]
    [SerializeField] private float _blastRange = 3f;
    [SerializeField] private float _blastForce = 10f;
    [SerializeField] private float _blastUp = 2f;
    [SerializeField] private float _blastTime = 0.3f; // force x time = about 3 units

    private bool _isClone;
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

    protected override void UpdateFleeing()
    {
        base.UpdateFleeing();
        if (_isClone || CurrentState != State.Fleeing) return;

        _cloneTimer -= Time.fixedDeltaTime;
        if (_cloneTimer > 0f) return;

        _cloneTimer = _cloneInterval;
        MakeClone();
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
        if (_player != null && DistanceToPlayer() < _blastRange
            && _player.TryGetComponent(out PlayerController player))
        {
            float dir = Mathf.Sign(_player.position.x - transform.position.x);
            player.Knockback(new Vector2(dir * _blastForce, _blastUp), _blastTime);
        }
        Destroy(gameObject);
    }
}