using UnityEngine;

public class BakeryAudioManager : MonoBehaviour
{
    public static BakeryAudioManager Instance { get; private set; }

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
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // --- COOKING SOUND METHODS ---

    public void PlayCookingStart()
    {
        if (cookingStart != null && cookingLoopSource != null)
        {
            cookingLoopSource.clip = cookingStart;
            cookingLoopSource.volume = cookingStartVolume;
            cookingLoopSource.Play(); // Starts looping infinitely
        }
    }

    public void StopCookingSound()
    {
        if (cookingLoopSource != null)
        {
            cookingLoopSource.Stop(); // Instantly cuts the sizzle
        }
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
}