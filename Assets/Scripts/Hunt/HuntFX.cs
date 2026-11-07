using UnityEngine;

// One per hunt scene. Plays the small game-feel effects.
public class HuntFX : MonoBehaviour
{
    public static HuntFX Instance { get; private set; }

    [Header("Effect prefabs")]
    [SerializeField] private ParticleSystem _sparks;
    [SerializeField] private ParticleSystem _puff;

    [SerializeField] private ParticleSystem _blast;

    [Header("Dough blast shake - Pravin can tune")]
    [SerializeField] private float _blastShakeTime = 0.25f;
    [SerializeField] private float _blastShakeStrength = 0.3f;

    [Header("Mould puff - Pravin can tune")]
    [SerializeField] private Color _mouldColor = new Color(0.4f, 0.85f, 0.3f);

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        IngredientCreature1.CreatureCaught += OnCreatureCaught;
        MouldZone.MouldPlaced += OnMouldPlaced;
    }

    private void OnDisable()
    {
        IngredientCreature1.CreatureCaught -= OnCreatureCaught;
        MouldZone.MouldPlaced -= OnMouldPlaced;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Sparks(Vector2 pos) => Play(_sparks, pos, null);
    public void Puff(Vector2 pos, Color color) => Play(_puff, pos, color);

    public void Blast(Vector2 pos)
    {
        Play(_blast, pos, null);
        if (CameraFollow2D.Instance != null)
            CameraFollow2D.Instance.Shake(_blastShakeTime, _blastShakeStrength);
    }

    private void OnCreatureCaught(IngredientCreature1 creature)
    {
        Puff(creature.transform.position, creature.Data.fxColor);
    }

    private void OnMouldPlaced(MouldZone zone)
    {
        // a puff on every hole, so the player sees where creatures will come out
        foreach (Transform hole in zone.transform)
            Puff(hole.position, _mouldColor);
    }

    private void Play(ParticleSystem prefab, Vector2 pos, Color? color)
    {
        if (prefab == null) return;

        ParticleSystem fx = Instantiate(prefab, pos, Quaternion.identity);
        if (color.HasValue)
        {
            ParticleSystem.MainModule main = fx.main;
            main.startColor = color.Value;
        }
    }
}