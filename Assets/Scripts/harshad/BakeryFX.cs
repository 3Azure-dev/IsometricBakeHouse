using UnityEngine;

// One per Bakery scene. Plays the small game-feel effects (Sprint 6).
// Same idea as HuntFX. Every number is in the Inspector so Pravin can tune it.
public class BakeryFX : MonoBehaviour
{
    public static BakeryFX Instance { get; private set; }

    [Header("Effect prefabs")]
    [SerializeField] private ParticleSystem steam;        // FX_Steam
    [SerializeField] private ParticleSystem coinSparkle;  // FX_CoinSparkle
    [SerializeField] private ParticleSystem puff;         // FX_Puff (angry, dust, plate)

    [Header("1. Food ready - steam")]
    [SerializeField] private float steamHeight = 1.5f;    // above the stove / oven / stone
    [SerializeField] private float steamScale = 1f;

    [Header("2. Order served - coin sparkle")]
    [SerializeField] private float sparkleHeight = 1.2f;  // above the table
    [SerializeField] private float sparkleScale = 1f;

    [Header("3. Furious / walked out - red puff")]
    [SerializeField] private float angryHeight = 2.4f;    // above the customer's head
    [SerializeField] private float angryScale = 0.5f;
    [SerializeField] private Color angryColor = new Color(0.9f, 0.15f, 0.15f);

    [Header("4. Something built - dust puff")]
    [SerializeField] private float dustScale = 1f;
    [SerializeField] private Color dustColor = new Color(0.75f, 0.65f, 0.5f);

    [Header("5. Dirty plates picked up - white puff")]
    [SerializeField] private float plateHeight = 1f;
    [SerializeField] private float plateScale = 0.4f;
    [SerializeField] private Color plateColor = Color.white;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Steam(Vector3 pos) { Play(steam, pos + Vector3.up * steamHeight, null, steamScale); }
    public void CoinSparkle(Vector3 pos) { Play(coinSparkle, pos + Vector3.up * sparkleHeight, null, sparkleScale); }
    public void AngryPuff(Vector3 pos) { Play(puff, pos + Vector3.up * angryHeight, angryColor, angryScale); }
    public void Dust(Vector3 pos) { Play(puff, pos, dustColor, dustScale); }
    public void PlatePuff(Vector3 pos) { Play(puff, pos + Vector3.up * plateHeight, plateColor, plateScale); }

    private void Play(ParticleSystem prefab, Vector3 pos, Color? color, float scale)
    {
        if (prefab == null) return;

        ParticleSystem fx = Instantiate(prefab, pos, prefab.transform.rotation);
        fx.transform.localScale = prefab.transform.localScale * scale;

        if (color.HasValue)
        {
            ParticleSystem.MainModule main = fx.main;
            main.startColor = color.Value;
        }
    }

    // Right-click the component in Play mode to see every effect without playing through
    [ContextMenu("TEST: play all effects")]
    private void TestAll()
    {
        Vector3 p = transform.position;
        Steam(p);
        CoinSparkle(p + Vector3.right * 2f);
        AngryPuff(p + Vector3.right * 4f);
        Dust(p + Vector3.right * 6f);
        PlatePuff(p + Vector3.right * 8f);
    }
}