using UnityEngine;
using System.Collections;

public class BreadSFX : MonoBehaviour
{
    public static BreadSFX Instance { get; private set; }

    private AudioSource _burnSource;
    private Coroutine _burnFade;
    private float _burnBaseVolume;

    [Header("Audio Source")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Shared Hunt Sounds")]
    [SerializeField] private AudioClip wandZap;
    [SerializeField] private AudioClip creatureStunned;
    [SerializeField] private AudioClip creatureCaught;
    [SerializeField] private AudioClip mouldPlaced;

    [Header("Cupcake Sounds")]
    [SerializeField] private AudioClip eggshellThrow;
    [SerializeField] private AudioClip eggshellHit;
    [SerializeField] private AudioClip sugarDash;
    [SerializeField] private AudioClip butterTrail;
    [SerializeField] private AudioClip cupcakeCreatureStunned;
    [SerializeField] private AudioClip cupcakeCreatureCaught;

    [Header("Pizza Creature Sounds")]
    [SerializeField] private AudioClip pizzaCreatureStunned;
    [SerializeField] private AudioClip pizzaCreatureCaught;

    [Header("Hazard Sounds")]
    [SerializeField] private AudioClip jamPatchStick;

    [Header("Dough Clone Sounds")]
    [SerializeField] private AudioClip doughCloneBlast;

    [Header("Sauce Sounds")]
    [SerializeField] private AudioClip sauceDropletHit;

    [Header("Cheese Sounds")]
    [SerializeField] private AudioClip cheesePopOut;

    [Header("Cheese Sounds")]
    [SerializeField] private AudioClip moltenCheeseBurn;

    [Header("Dunk Sounds")]
    [SerializeField] private AudioClip dunkHit;
    [SerializeField] private AudioClip dunkMiss;

    [Header("Exit Door Sounds")]
    [SerializeField] private AudioClip exitDoorUnlock;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
        }

        if (sfxSource == null)
        {
            Debug.LogError(
                "BreadSFX: Please add an AudioSource component.",
                this
            );
        }

        if (sfxSource != null)
        {
            _burnSource = gameObject.AddComponent<AudioSource>();
            _burnSource.playOnAwake = false;
            _burnSource.loop = false;
            _burnSource.spatialBlend = 0f;
            _burnSource.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup; // keeps your volume sliders working
            _burnBaseVolume = sfxSource.volume;
        }
    }

    private void OnEnable()
    {
        MouldZone.MouldPlaced += HandleMouldPlaced;
    }

    private void OnDisable()
    {
        MouldZone.MouldPlaced -= HandleMouldPlaced;
    }

    // Shared hunt sounds

    public void PlayWandZap()
    {
        PlaySound(wandZap);
    }

    public void PlayCreatureStunned()
    {
        PlaySound(creatureStunned);
    }

    public void PlayCreatureCaught()
    {
        PlaySound(creatureCaught);
    }

    public void PlayMouldPlaced()
    {
        PlaySound(mouldPlaced);
    }

    // Cupcake sounds

    public void PlayEggshellThrow()
    {
        PlaySound(eggshellThrow);
    }

    public void PlayEggshellHit()
    {
        PlaySound(eggshellHit);
    }

    public void PlaySugarDash()
    {
        PlaySound(sugarDash);
    }

    public void PlayButterTrail()
    {
        PlaySound(butterTrail);
    }

    public void PlayCupcakeCreatureStunned()
    {
        PlaySound(cupcakeCreatureStunned != null ? cupcakeCreatureStunned : creatureStunned);
    }

    public void PlayCupcakeCreatureCaught()
    {
        PlaySound(cupcakeCreatureCaught != null ? cupcakeCreatureCaught : creatureCaught);
    }


    // Pizza creature sounds

    public void PlayPizzaCreatureStunned()
    {
        PlaySound(pizzaCreatureStunned);
    }

    public void PlayPizzaCreatureCaught()
    {
        PlaySound(pizzaCreatureCaught);
    }

    public void PlayDoughCloneBlast()
    {
        PlaySound(doughCloneBlast);
    }

    public void PlaySauceDropletHit()
    {
        PlaySound(sauceDropletHit);
    }

    public void PlayCheesePopOut()
    {
        PlaySound(cheesePopOut);
    }

    public void PlayMoltenCheeseBurn()
    {
        if (_burnSource == null || moltenCheeseBurn == null)
        {
            Debug.LogWarning("BreadSFX: molten cheese burn clip or source is missing.", this);
            return;
        }

        if (_burnFade != null) { StopCoroutine(_burnFade); _burnFade = null; }

        _burnSource.Stop();
        _burnSource.volume = _burnBaseVolume;
        _burnSource.clip = moltenCheeseBurn;
        _burnSource.Play();
    }

    public void FadeOutMoltenCheeseBurn(float duration = 0.6f)
    {
        if (_burnSource == null || !_burnSource.isPlaying) return;
        if (_burnFade != null) StopCoroutine(_burnFade);
        _burnFade = StartCoroutine(FadeBurn(duration));
    }

    private IEnumerator FadeBurn(float duration)
    {
        float start = _burnSource.volume;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            _burnSource.volume = Mathf.Lerp(start, 0f, t / duration);
            yield return null;
        }
        _burnSource.Stop();
        _burnSource.volume = _burnBaseVolume;
        _burnFade = null;
    }

    private void HandleMouldPlaced(MouldZone zone)
    {
        PlayMouldPlaced();
    }

    public void PlayJamPatchStick()
    {
        PlaySound(jamPatchStick);
    }

    public void PlayDunkHit()
    {
        PlaySound(dunkHit);
    }

    public void PlayDunkMiss()
    {
        PlaySound(dunkMiss);
    }

    public void PlayExitDoorUnlock()
    {
        PlaySound(exitDoorUnlock);
    }

    private void PlaySound(AudioClip clip)
    {
        if (sfxSource == null || clip == null)
        {
            Debug.LogWarning(
                "BreadSFX: AudioSource or AudioClip is missing.",
                this
            );
            return;
        }

        sfxSource.PlayOneShot(clip);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}