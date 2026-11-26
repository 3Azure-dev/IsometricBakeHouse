using UnityEngine;

public class BakeryAudioManager : MonoBehaviour
{
    public static BakeryAudioManager Instance { get; private set; }

    public enum CookingStation { Stove, Oven, PizzaStone }

    [Header("New Station Cooking Loops (Oven keeps the existing Cooking Start clip)")]
    [SerializeField] private AudioClip stoveLoop;
    [SerializeField] private AudioClip pizzaStoneLoop;
    [Range(0f, 1f)][SerializeField] private float stoveLoopVolume = 0.5f;
    [Range(0f, 1f)][SerializeField] private float pizzaStoneLoopVolume = 0.5f;

    private AudioSource stoveSource, pizzaStoneSource;

    [Header("Main Audio Source (for one-shots)")]
    [SerializeField] private AudioSource audioSource;

    [Header("Cooking Loop Audio Source")]
    [Tooltip("Dedicated source for the looping sizzle sound. Will be created automatically if left empty.")]
    [SerializeField] private AudioSource cookingLoopSource;

    [Header("Bakery Sound Effects")]
    [SerializeField] private AudioClip cookingStart;
    [SerializeField] private AudioClip cookingDone;
    [SerializeField] private AudioClip dishServed;
    [SerializeField] private AudioClip coinsEarned;
    [SerializeField] private AudioClip customerArrives;
    [SerializeField] private AudioClip dirtyPlatePickup;
    [Range(0f, 2f)][SerializeField] private float dirtyPlatePickupVolume = 1f;
    [Header("Individual Volume")]
    [Range(0f, 1f)][SerializeField] private float cookingStartVolume = 0.7f;
    [Range(0f, 1f)][SerializeField] private float cookingDoneVolume = 0.8f;
    [Range(0f, 1f)][SerializeField] private float dishServedVolume = 0.7f;
    [Range(0f, 1f)][SerializeField] private float coinsEarnedVolume = 0.7f;
    [Range(0f, 1f)][SerializeField] private float customerArrivesVolume = 0.7f;

    private Wallet subscribedWallet;
    private int lastKnownCoins;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Setup main audio source
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("BakeryAudioManager requires a main AudioSource.", this);
            enabled = false;
            return;
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;

        // Setup dedicated cooking loop source
        if (cookingLoopSource == null)
        {
            cookingLoopSource = gameObject.AddComponent<AudioSource>();
        }
        cookingLoopSource.loop = true;          // CRUCIAL: Makes the sizzle play continuously
        cookingLoopSource.playOnAwake = false;
        cookingLoopSource.spatialBlend = 0f;    // 2D sound

        stoveSource = CreateLoopSource();
        pizzaStoneSource = CreateLoopSource();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // --- COOKING SOUND METHODS ---

    public void PlayCookingStart(CookingStation station = CookingStation.Oven)
    {
        AudioSource src; AudioClip clip; float vol;

        switch (station)
        {
            case CookingStation.Stove:
                src = stoveSource; clip = stoveLoop; vol = stoveLoopVolume; break;
            case CookingStation.PizzaStone:
                src = pizzaStoneSource; clip = pizzaStoneLoop; vol = pizzaStoneLoopVolume; break;
            default: // Oven: unchanged, uses the existing clip
                src = cookingLoopSource; clip = cookingStart; vol = cookingStartVolume; break;
        }

        if (src == null || clip == null)
        {
            Debug.LogWarning($"BakeryAudioManager: no cooking loop assigned for {station}.", this);
            return;
        }

        if (src.isPlaying && src.clip == clip) return; // already cooking

        src.clip = clip;
        src.volume = vol;
        src.Play();
    }

    public void StopCookingSound(CookingStation station = CookingStation.Oven)
    {
        AudioSource src = station switch
        {
            CookingStation.Stove => stoveSource,
            CookingStation.PizzaStone => pizzaStoneSource,
            _ => cookingLoopSource
        };

        if (src != null) src.Stop();
    }

    public void PlayCookingDone()
    {
        // Plays the "ding" as a one-shot so it doesn't get cut off
        PlaySound(cookingDone, cookingDoneVolume);
    }

    // --- OTHER SOUND METHODS (Unchanged) ---

    public void PlayDishServed()
    {
        PlaySound(dishServed, dishServedVolume);
    }

    public void PlayCoinsEarned()
    {
        PlaySound(coinsEarned, coinsEarnedVolume);
    }

    public void PlayCustomerArrives()
    {
        PlaySound(customerArrives, customerArrivesVolume);
    }

    public void PlayDirtyPlatePickup()
    {
        PlaySound(dirtyPlatePickup, dirtyPlatePickupVolume);
    }
    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip == null)
        {
            Debug.LogWarning("BakeryAudioManager: A sound clip has not been assigned.", this);
            return;
        }

        if (audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    // --- WALLET SUBSCRIPTION (Unchanged) ---

    private void OnEnable() { SubscribeToWallet(); }
    private void Start() { SubscribeToWallet(); }

    private void OnDisable()
    {
        if (subscribedWallet != null)
        {
            subscribedWallet.CoinsChanged -= OnCoinsChanged;
            subscribedWallet = null;
        }
    }

    private void SubscribeToWallet()
    {
        Wallet currentWallet = Wallet.Instance;
        if (currentWallet == subscribedWallet) return;

        if (subscribedWallet != null)
            subscribedWallet.CoinsChanged -= OnCoinsChanged;

        subscribedWallet = currentWallet;

        if (subscribedWallet != null)
        {
            lastKnownCoins = subscribedWallet.Coins;
            subscribedWallet.CoinsChanged += OnCoinsChanged;
        }
    }

    private void OnCoinsChanged(int newBalance)
    {
        if (newBalance > lastKnownCoins)
        {
            PlayCoinsEarned();
        }
        lastKnownCoins = newBalance;
    }

    private AudioSource CreateLoopSource()
    {
        AudioSource src = gameObject.AddComponent<AudioSource>();
        src.loop = true;
        src.playOnAwake = false;
        src.spatialBlend = 0f;
        src.outputAudioMixerGroup = audioSource.outputAudioMixerGroup; // keeps your volume sliders working
        return src;
    }
}