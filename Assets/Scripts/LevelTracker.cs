using System;
using UnityEngine;

// Counts the creatures in one hunt level and says when the level is empty
public class LevelTracker : MonoBehaviour
{
    // the Grandfather dunk listens to this. int = how many were caught in this level
    public static event Action<int> LevelEmpty;

    [Header("Debug - read only")]
    [SerializeField] private int _holesLeft;
    [SerializeField] private int _alive;
    [SerializeField] private int _caught;
    [SerializeField] private int _escaped;

    private bool _fired; // so the event only fires one time

    public int Caught => _caught;

    private void OnEnable()
    {
        MouldZone.CreatureSpawned += OnSpawned;
        IngredientCreature1.CreatureCaught += OnCaught;
        IngredientCreature1.CreatureEscaped += OnEscaped;
    }

    private void OnDisable()
    {
        // static events must always be unsubscribed, or they keep pointing at a dead object
        MouldZone.CreatureSpawned -= OnSpawned;
        IngredientCreature1.CreatureCaught -= OnCaught;
        IngredientCreature1.CreatureEscaped -= OnEscaped;
    }

    private void Start()
    {
        // every child of a zone is a hole
        foreach (MouldZone zone in FindObjectsByType<MouldZone>(FindObjectsSortMode.None))
            _holesLeft += zone.transform.childCount;

        Debug.Log($"LevelTracker: {_holesLeft} holes in this level");
    }

    private void OnSpawned(IngredientCreature1 creature)
    {
        _holesLeft--;
        _alive++;
        Check();
    }

    private void OnCaught(IngredientCreature1 creature)
    {
        _alive--;
        _caught++;
        Check();
    }

    private void OnEscaped(IngredientCreature1 creature)
    {
        _alive--;
        _escaped++;
        Check();
    }

    private void Check()
    {
        Debug.Log($"Holes left {_holesLeft} | alive {_alive} | caught {_caught} | escaped {_escaped}");

        if (_fired || _holesLeft > 0 || _alive > 0) return;

        _fired = true;
        Debug.Log($"LEVEL EMPTY - caught {_caught}");
        LevelEmpty?.Invoke(_caught);
    }
}