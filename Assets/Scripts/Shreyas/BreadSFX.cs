using UnityEngine;

public class BreadSFX : MonoBehaviour
{
    public static BreadSFX Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip wandZap;
    [SerializeField] private AudioClip creatureStunned;
    [SerializeField] private AudioClip creatureCaught;
    [SerializeField] private AudioClip mouldPlaced;

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
    }

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
    private void OnEnable()
    {
        MouldZone.MouldPlaced += HandleMouldPlaced;
    }

    private void OnDisable()
    {
        MouldZone.MouldPlaced -= HandleMouldPlaced;
    }

    private void HandleMouldPlaced(MouldZone zone)
    {
        Debug.Log("Mould sound event received!");

        PlayMouldPlaced();
    }
}