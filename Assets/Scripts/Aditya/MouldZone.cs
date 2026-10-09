using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CircleCollider2D))]
public class MouldZone : MonoBehaviour
{
    // other scripts listen to these (sounds now, level tracker later)
    public static event Action<MouldZone> MouldPlaced;
    public static event Action<IngredientCreature1> CreatureSpawned;

    [SerializeField] private MouldSettings _settings;
    [SerializeField] private IngredientCreature1[] _creaturePrefabs;

    private static float _nextMouldTime; // one cooldown shared by all zones
    private bool _playerInside;
    private bool _used;

    private void Awake()
    {
        _nextMouldTime = 0f;
        CircleCollider2D zone = GetComponent<CircleCollider2D>();
        zone.isTrigger = true;
        zone.radius = _settings.zoneRadius;
    }

    private void Update()
    {
        if (!_playerInside || _used) return;
        if (!Keyboard.current.qKey.wasPressedThisFrame) return;

        if (Time.time < _nextMouldTime)
        {
            Debug.Log("Mould is on cooldown");
            return;
        }

        PlaceMould();
    }

    private void PlaceMould()
    {
        _used = true;
        _nextMouldTime = Time.time + _settings.cooldown;

        // every child of this object is a hole
        foreach (Transform hole in transform)
        {
            IngredientCreature1 prefab = _creaturePrefabs[UnityEngine.Random.Range(0, _creaturePrefabs.Length)];
            IngredientCreature1 creature = Instantiate(prefab, hole.position, Quaternion.identity);
            CreatureSpawned?.Invoke(creature);
        }

        MouldPlaced?.Invoke(this);
        Debug.Log($"{name}: mould placed, {transform.childCount} creatures out");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _playerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _playerInside = false;
    }

    private void OnDrawGizmos()
    {
        // green = ready, grey = used
        Gizmos.color = _used ? Color.grey : Color.green;
        float radius = _settings != null ? _settings.zoneRadius : 4f;
        Gizmos.DrawWireSphere(transform.position, radius);

        Gizmos.color = Color.yellow;
        foreach (Transform hole in transform)
            Gizmos.DrawSphere(hole.position, 0.2f);
    }
}