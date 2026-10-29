using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;
using System;

[RequireComponent(typeof(WandAim))]
public class WandFire1 : MonoBehaviour

{
    // Automatically calculates how much time is left based on your _nextFireTime
    public float CooldownRemaining => Mathf.Max(0f, _nextFireTime - Time.time);

    // Tracks the maximum duration for the UI bar's "100%" mark
    public float CooldownMax { get; private set; }

    public static event Action<float> WandFired;
    [SerializeField] private WandData _data;   // SO_Wand goes here
    [SerializeField] private Bolt1 _boltPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private int _poolSize = 20;

    [Header("Sticky sauce")]
    [SerializeField] private SpriteRenderer _wandSprite; // optional, turns red
    [SerializeField] private Color _sauceColor = new Color(0.9f, 0.2f, 0.15f);

    private float _sauceUntil;
    private float _sauceExtra;
    private Color _normalColor;

    private bool IsSticky => Time.time < _sauceUntil;

    private WandAim _aim;
    private IObjectPool<Bolt1> _pool;
    private float _nextFireTime;   // earliest time we can shoot again

    private void Awake()
    {
        _aim = GetComponent<WandAim>();

        if (_data == null)
            Debug.LogWarning("WandFire1: no WandData set, so there is no cooldown.", this);

        _pool = new ObjectPool<Bolt1>(
            createFunc: CreateBolt,
            actionOnGet: bolt => bolt.gameObject.SetActive(true),
            actionOnRelease: bolt => bolt.gameObject.SetActive(false),
            actionOnDestroy: bolt => Destroy(bolt.gameObject),
            defaultCapacity: _poolSize,
            maxSize: 50);

        if (_wandSprite != null) _normalColor = _wandSprite.color;
    }

    private Bolt1 CreateBolt()
    {
        Bolt1 bolt = Instantiate(_boltPrefab);
        bolt.SetPool(_pool);
        return bolt;
    }

    private void Update()
    {
       

        if (_wandSprite != null) _wandSprite.color = IsSticky ? _sauceColor : _normalColor;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            TryFire();
    }

    private void TryFire()
    {
        if (Time.time < _nextFireTime) return;

        Fire();

        float cooldown = _data != null ? _data.cooldown : 0f;

        // Preserve the extra cooldown caused by sticky Tomato Sauce.
        if (IsSticky)
            cooldown += _sauceExtra;

        _nextFireTime = Time.time + cooldown;

        // ADD THIS LINE to tell the UI how big the bar should be:
        CooldownMax = cooldown;

        // Notify the cooldown UI using the final cooldown duration.
        WandFired?.Invoke(cooldown);
    }

    private void Fire()
    {
        Bolt1 bolt = _pool.Get();
        float range = _data != null ? _data.range : Mathf.Infinity;
        bolt.Launch(_firePoint.position, _aim.AimDirection, range);
       

        if (BreadSFX.Instance != null)
            BreadSFX.Instance.PlayWandZap();
        // ADD THESE TWO LINES:
      
    }

    public void AddStickySauce(float extraCooldown, float duration)
    {
        _sauceExtra = extraCooldown;
        _sauceUntil = Time.time + duration; // restarts, doesn't stack
        Debug.Log($"Wand sticky: +{extraCooldown}s cooldown for {duration}s");

        // THIS adds 3 seconds to the actual timer
        _nextFireTime += 3f;

        // THIS expands the UI bar so the +3s is visible
        // (It is perfectly fine to READ CooldownRemaining here, just don't assign to it)
        CooldownMax = Mathf.Max(CooldownMax, CooldownRemaining);
    }
}