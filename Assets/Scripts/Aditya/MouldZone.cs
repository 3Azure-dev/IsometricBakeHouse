using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CircleCollider2D))]
public class MouldZone : MonoBehaviour
{
    // other scripts listen to these (sounds now, level tracker later)
    public static event Action<MouldZone> MouldPlaced;
    public static event Action<IngredientCreature1> CreatureSpawned;
    public static event Action<MouldZone> PlayerEntered; // PlayerRespawn saves a checkpoint here

    [SerializeField] private MouldSettings _settings;
    [SerializeField] private IngredientCreature1[] _creaturePrefabs;

    private static float _nextMouldTime; // one cooldown shared by all zones

    private static ulong _cooldownSceneHandle = ulong.MaxValue;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
   
    private static void ResetMouldCooldownStatics()
    {
        _nextMouldTime = 0f;
        _cooldownSceneHandle = ulong.MaxValue;
    }
    public float CooldownDuration => _settings != null ? _settings.cooldown : 0f;
    private bool _playerInside;
    private bool _used;

    private bool _mouldWaiting; // mould is placed, creatures come when the player leaves

    private void Awake()
    {
        // Reset the shared cooldown once for each loaded scene,
        // not once for every mould zone.
        ulong currentSceneHandle = gameObject.scene.handle.GetRawData();

        if (_cooldownSceneHandle != currentSceneHandle)
        {
            _nextMouldTime = 0f;
            _cooldownSceneHandle = currentSceneHandle;
        }

        CircleCollider2D zone = GetComponent<CircleCollider2D>();
        zone.isTrigger = true;

        if (_settings != null)
        {
            zone.radius = _settings.zoneRadius;
        }
    }
    private void Update()
    {
        // A used mould zone stays disabled for this level.
        if (_used)
            return;

        // The player must be inside the zone.
        if (!_playerInside)
            return;

        // Check that the keyboard is available.
        if (Keyboard.current == null)
            return;

        // Q must be pressed to place mould.
        if (!Keyboard.current.qKey.wasPressedThisFrame)
            return;

        // The cooldown is shared across all mould zones.
        if (Time.time < _nextMouldTime)
        {
            Debug.Log($"{name}: mould is on cooldown.");
            return;
        }

        // Place mould in this unused zone.
        PlaceMould();
    }
    private void PlaceMould()
    {
        _used = true;
        _mouldWaiting = true;
        _nextMouldTime = Time.time + _settings.cooldown;

        MouldPlaced?.Invoke(this);
        Debug.Log($"{name}: mould placed, creatures come when the player leaves the zone");
    }

    private void SpawnCreatures()
    {
        _mouldWaiting = false;

        // every child of this object is a hole
        foreach (Transform hole in transform)
        {
            IngredientCreature1 prefab = _creaturePrefabs[UnityEngine.Random.Range(0, _creaturePrefabs.Length)];
            IngredientCreature1 creature = Instantiate(prefab, hole.position, Quaternion.identity);
            CreatureSpawned?.Invoke(creature);
        }

        Debug.Log($"{name}: {transform.childCount} creatures out");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        _playerInside = true;
        PlayerEntered?.Invoke(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        _playerInside = false;
        if (_mouldWaiting) SpawnCreatures(); // the player left, so the creatures come out
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