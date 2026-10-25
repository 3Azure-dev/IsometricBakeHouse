using System;
using UnityEngine;

// Dough: while running it can split into a copy. After the split the real one runs faster.
public class DoughCreature : IngredientCreature1
{
    public static event Action<IngredientCreature1> Split; // LevelTracker counts the copy

    public static event Action<IngredientCreature1> CopyPopped; // LevelTracker removes the copy

    [Header("Dough split - Pravin can tune these")]
    [SerializeField, Range(0f, 1f)] private float _splitChance = 0.3f;
    [SerializeField] private float _splitCheckTime = 1f;
    [SerializeField] private float _fastMultiplier = 1.5f;

    [Header("Copy blast - Pravin can tune these")]
    [SerializeField] private float _blastRange = 5f;
    [SerializeField] private float _blastForce = 10f;
    [SerializeField] private float _blastUp = 5f;
    [SerializeField] private float _blastTime = 0.3f;

    private bool _hasSplit;
    private bool _isCopy;
    private float _splitTimer;

    public override void Stun()
    {
        if (!_isCopy)
        {
            base.Stun();
            return;
        }
        Blast();
    }

    private void Blast()
    {
        if (_player != null && DistanceToPlayer() < _blastRange
            && _player.TryGetComponent(out PlayerController player))
        {
            float dir = Mathf.Sign(_player.position.x - transform.position.x);
            player.Knockback(new Vector2(dir * _blastForce, _blastUp), _blastTime);
        }

        Debug.Log($"{name}: copy blasted");
        CopyPopped?.Invoke(this);
        gameObject.SetActive(false);
    }

    protected override void UpdateFleeing()
    {
        base.UpdateFleeing();
        if (CurrentState != State.Fleeing) return;

        if (_hasSplit && !_isCopy)
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x * _fastMultiplier, _rb.linearVelocity.y);

        if (_hasSplit) return;

        _splitTimer -= Time.fixedDeltaTime;
        if (_splitTimer > 0f) return;
        _splitTimer = _splitCheckTime;

        if (UnityEngine.Random.value < _splitChance) MakeCopy();
    }

    private void MakeCopy()
    {
        _hasSplit = true;

        float dir = Mathf.Sign(transform.position.x - _player.position.x);
        Vector3 pos = transform.position - new Vector3(dir * 0.6f, 0f, 0f);

        DoughCreature copy = Instantiate(this, pos, Quaternion.identity);
        copy._hasSplit = true;
        copy._isCopy = true;
        copy.name = "Creature_Dough(Copy)";

        Split?.Invoke(copy);
        Debug.Log($"{name} split into a copy");
    }

    protected override void Catch()
    {
        if (_isCopy)
        {
            Debug.Log($"{name}: it was the copy, poof");
            CopyPopped?.Invoke(this);
            gameObject.SetActive(false); // gone, not caught or escaped
            return;
        }
        base.Catch();
    }
}