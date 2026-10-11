using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

// Runs the Grandfather dunk when the level is empty.
public class GrandfatherDunk : MonoBehaviour
{
    public enum State { Waiting, Ready, Aiming, Result }

    [SerializeField] private DunkData _data;

    [Header("Scene objects")]
    [SerializeField] private Transform _aimPivot;
    [SerializeField] private Transform _pot;

    [Header("Aim angles (degrees)")]
    [SerializeField] private float _minAngle = 20f;
    [SerializeField] private float _maxAngle = 80f;
    [SerializeField] private float _potDistance = 5f;

    [Header("Sack throw")]
    [SerializeField] private Transform _sack;
    [SerializeField] private float _throwTime = 0.6f;  // seconds in the air
    [SerializeField] private float _arcHeight = 1.5f;  // how much the throw bends

    [Header("Testing")]
    [SerializeField] private bool _alwaysGrandfatherDay = true; // until the day timer exists

    [SerializeField] private bool _testLoop = false; // dunk restarts after every throw

    [Header("Debug - read only")]
    [SerializeField] private State _state = State.Waiting;
    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _aim; // 0 = bottom, 1 = top

    private float _phase;

    public static event Action<bool> DunkFinished; // true = hit

    public static event Action HuntFinished;

    private readonly List<MonoBehaviour> _frozenScripts = new List<MonoBehaviour>();    
    
    // what the player caught in this level: ingredient -> how many
    private readonly Dictionary<IngredientData, int> _caughtThisLevel = new Dictionary<IngredientData, int>();

    private void OnEnable()
    {
        LevelTracker.LevelEmpty += OnLevelEmpty;
        IngredientCreature1.CreatureCaught += OnCreatureCaught;
    }

    private void OnDisable()
    {
        LevelTracker.LevelEmpty -= OnLevelEmpty;
        IngredientCreature1.CreatureCaught -= OnCreatureCaught;
    }

    private void OnCreatureCaught(IngredientCreature1 creature)
    {
        IngredientData data = creature.Data;

        if (!_caughtThisLevel.ContainsKey(data))
            _caughtThisLevel[data] = 0;

        _caughtThisLevel[data] += data.ingredientsPerCatch;
    }

    private void Start()
    {
        _aimPivot.gameObject.SetActive(false);
        _pot.gameObject.SetActive(false);
        _sack.gameObject.SetActive(false);
    }

    private void OnLevelEmpty(int caught)
    {
        // no dunk today: the hunt is over right away
        if (caught < 1 || !_alwaysGrandfatherDay)
        {
            HuntFinished?.Invoke();
            return;
        }

        _state = State.Ready; // wait for the player to reach the dunk spot
        Debug.Log("Grandfather is waiting near the exit");
    }

    [ContextMenu("Test: Start Dunk")]
    private void StartAiming()
    {
        if (_state == State.Aiming || _state == State.Result) return;

        // the aim starts from the player
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) _aimPivot.position = player.transform.position;

        if (player != null) FreezePlayer(player);

        // put the pot where the hit zone is, so they always match
        float potAngle = Mathf.Lerp(_minAngle, _maxAngle, _data.hitZoneCenter);
        Vector3 direction = Quaternion.Euler(0f, 0f, potAngle) * Vector3.right;
        _pot.position = _aimPivot.position + direction * _potDistance;

        _aimPivot.gameObject.SetActive(true);
        _pot.gameObject.SetActive(true);

        _speed = 1f;
        _phase = 0f;
        _state = State.Aiming;

        StartCoroutine(SpeedUp());
    }

    private void Update()
    {
        if (_state != State.Aiming) return;

        // PingPong goes 0 -> 1 -> 0 -> 1 forever
        _phase += Time.deltaTime * _speed / _data.swingTime;
        _aim = Mathf.PingPong(_phase, 1f);

        float angle = Mathf.Lerp(_minAngle, _maxAngle, _aim);
        _aimPivot.rotation = Quaternion.Euler(0f, 0f, angle);

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            Throw();
    }

    // the dunk starts when the player stands in the dunk spot
    private void OnTriggerStay2D(Collider2D other)
    {
        if (_state != State.Ready) return;
        if (other.CompareTag("Player")) StartAiming();
    }

    // Coroutine: waits, makes the aim faster, repeats
    private IEnumerator SpeedUp()
    {
        while (_state == State.Aiming)
        {
            yield return new WaitForSeconds(_data.speedUpEvery);
            _speed = Mathf.Min(_speed * (1f + _data.speedUpAmount), _data.maxSpeed);
        }
    }

    private void Throw()
    {
        _state = State.Result; // one try only

        // hit if the aim is inside the hit zone
        float distance = Mathf.Abs(_aim - _data.hitZoneCenter);
        bool hit = distance <= _data.hitZoneSize / 2f;

        StartCoroutine(ThrowSack(hit));
    }

    // Coroutine: the sack flies in a curve, then the result shows
// Coroutine: the sack is lobbed up above the pot, then comes down
    private IEnumerator ThrowSack(bool hit)
    {
        _aimPivot.gameObject.SetActive(false);

        Vector3 start = _aimPivot.position;
        Vector3 pot = _pot.position;
        Vector3 end; // where the sack lands

        if (hit)
        {
            end = pot; // drops into the pot
        }
        else
        {
            // aimed too low = flies past the pot, aimed too high = falls short
            float off = _aim - _data.hitZoneCenter;
            float missX = -Mathf.Sign(off) * (1f + Mathf.Abs(off) * 6f);
            end = new Vector3(pot.x + missX, start.y, 0f); // lands on the floor beside the pot
        }
        
        // the top of the throw is exactly _arcHeight above the pot
        float topY = pot.y + _arcHeight;
        float bendY = 2f * topY - (start.y + end.y) / 2f;
        Vector3 bend = new Vector3((start.x + end.x) / 2f, bendY, 0f);
        _sack.gameObject.SetActive(true);

        float t = 0f; // 0 = start of the throw, 1 = landed
        while (t < 1f)
        {
            t += Time.deltaTime / _throwTime;
            float u = 1f - t;
            _sack.position = u * u * start + 2f * u * t * bend + t * t * end; // curve formula
            yield return null; // wait one frame
        }

        _sack.gameObject.SetActive(false);

        Debug.Log(hit ? "DUNK HIT" : "DUNK MISS");
        if (hit) GiveBonus();

        if (_pot.TryGetComponent(out SpriteRenderer sr))
            sr.color = hit ? Color.green : Color.red;

        StartCoroutine(Finish(hit));
    }

    // 2x means: add the same amount one more time
    private void GiveBonus()
    {
        foreach (KeyValuePair<IngredientData, int> entry in _caughtThisLevel)
        {
            int extra = entry.Value * (_data.bonusMultiplier - 1);
            Inventory1.Instance.Add(entry.Key, extra);
            Debug.Log($"Dunk bonus: +{extra} {entry.Key.name}");
        }
    }

    // Coroutine: show the result for 1 sec, then clean up
    private IEnumerator Finish(bool hit)
    {
        yield return new WaitForSeconds(1f);

        // testing: skip the ending and start the dunk again
        if (_testLoop)
        {
            if (_pot.TryGetComponent(out SpriteRenderer potSprite))
                potSprite.color = Color.white;

            UnfreezePlayer();
            _state = State.Waiting;
            StartAiming();
            yield break; // stop this coroutine here
        }

        _aimPivot.gameObject.SetActive(false);
        _pot.gameObject.SetActive(false);
        UnfreezePlayer();

        DunkFinished?.Invoke(hit);
        HuntFinished?.Invoke();
    }

    // Switch off every script on the player (movement, wand) and stop him
    private void FreezePlayer(GameObject player)
    {
        foreach (MonoBehaviour script in player.GetComponentsInChildren<MonoBehaviour>())
        {
            if (!script.enabled) continue;
            script.enabled = false;
            _frozenScripts.Add(script);
        }

        if (player.TryGetComponent(out Rigidbody2D rb))
            rb.linearVelocity = Vector2.zero;
    }

    private void UnfreezePlayer()
    {
        foreach (MonoBehaviour script in _frozenScripts)
            if (script != null) script.enabled = true;

        _frozenScripts.Clear();
    }
}