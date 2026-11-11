using UnityEngine;

// Game feel only: bob while walking, squash when stopping, breathe when idle.
// Put this on a "Visual" object ABOVE the model, inside the prefab.
// It only changes this object's local position and scale. Nothing else.
public class VisualMotion : MonoBehaviour
{
    [Header("Walking bob")]
    [SerializeField] private float bobHeight = 0.08f;       // how high, in metres
    [SerializeField] private float bobFrequency = 2.5f;     // bobs per second
    [SerializeField] private bool sharpBounce = false;      // false = calm sway, true = hop
    [SerializeField] private float walkSpeedThreshold = 0.3f; // m/s above which we count as walking

    [Header("Idle breathing")]
    [SerializeField] private float breathAmount = 0.02f;    // 0.02 = 2% bigger/smaller
    [SerializeField] private float breathFrequency = 0.5f;  // breaths per second

    [Header("Squash when stopping (customers)")]
    [SerializeField] private bool squashOnStop = true;      // turn OFF on the player
    [SerializeField] private float squashAmount = 0.15f;    // 0.15 = 15% flatter
    [SerializeField] private float squashDuration = 0.25f;  // seconds

    [Header("Smoothing")]
    [SerializeField] private float blendSpeed = 6f;         // walk <-> idle blend per second

    private Transform body;           // the object that actually moves (our parent)
    private Vector3 startLocalPos;
    private Vector3 startLocalScale;
    private Vector3 lastBodyPos;
    private float sampleTimer;
    private float speed;
    private float walkBlend;
    private bool wasWalking;
    private float bobPhase;
    private float breathPhase;
    private float squashTimer = -1f;

    private const float SampleInterval = 0.1f;
    private const float MaxSensibleSpeed = 20f;   // bigger = teleport/warp, ignore

    private void Awake()
    {
        startLocalPos = transform.localPosition;
        startLocalScale = transform.localScale;
        body = transform.parent;
    }

    private void OnEnable()
    {
        // Customers come from a pool: start clean every time.
        body = transform.parent;
        lastBodyPos = body != null ? body.position : Vector3.zero;
        sampleTimer = 0f;
        speed = 0f;
        walkBlend = 0f;
        wasWalking = false;
        squashTimer = -1f;
        breathPhase = Random.value * Mathf.PI * 2f; // customers do not breathe in sync
        transform.localPosition = startLocalPos;
        transform.localScale = startLocalScale;
    }

    private void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f)
            return;

        MeasureSpeed(dt);

        bool walking = speed > walkSpeedThreshold;
        walkBlend = Mathf.MoveTowards(walkBlend, walking ? 1f : 0f, blendSpeed * dt);

        // Just stopped after walking: small squash ("sits down").
        if (wasWalking && !walking && squashOnStop)
            squashTimer = 0f;
        wasWalking = walking;

        // 1) Walking bob (up and down)
        bobPhase += dt * bobFrequency * Mathf.PI * 2f * walkBlend;
        float wave = Mathf.Sin(bobPhase);
        float bob = sharpBounce ? Mathf.Abs(wave) : (wave * 0.5f + 0.5f);
        float yOffset = bob * bobHeight * walkBlend;

        // 2) Idle breathing (tiny scale up and down), fades out while walking
        breathPhase += dt * breathFrequency * Mathf.PI * 2f;
        float breath = Mathf.Sin(breathPhase) * breathAmount * (1f - walkBlend);

        // 3) Squash: down and back up
        float squash = 0f;
        if (squashTimer >= 0f)
        {
            squashTimer += dt;
            float t = Mathf.Clamp01(squashTimer / Mathf.Max(0.01f, squashDuration));
            squash = Mathf.Sin(t * Mathf.PI) * squashAmount;
            if (t >= 1f)
                squashTimer = -1f;
        }

        // Taller + thinner when breathing in, flatter + wider when squashed.
        float scaleY = 1f + breath - squash;
        float scaleXZ = 1f - breath * 0.5f + squash * 0.5f;

        transform.localPosition = startLocalPos + Vector3.up * yOffset;
        transform.localScale = new Vector3(
            startLocalScale.x * scaleXZ,
            startLocalScale.y * scaleY,
            startLocalScale.z * scaleXZ);
    }

    // Speed of the parent, measured over short windows so physics steps do not make it flicker.
    private void MeasureSpeed(float dt)
    {
        if (body == null)
            return;

        sampleTimer += dt;
        if (sampleTimer < SampleInterval)
            return;

        float measured = Vector3.Distance(body.position, lastBodyPos) / sampleTimer;
        speed = measured > MaxSensibleSpeed ? 0f : measured;
        lastBodyPos = body.position;
        sampleTimer = 0f;
    }

    // Other scripts may call this later (for example when a customer really sits down).
    public void Squash()
    {
        squashTimer = 0f;
    }

    [ContextMenu("TEST: Squash")]
    private void TestSquash()
    {
        Squash();
    }
}