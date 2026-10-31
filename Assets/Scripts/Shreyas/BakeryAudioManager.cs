using UnityEngine;
using UnityEngine.Audio;

public class BakeryAudioManager : MonoBehaviour
{
    public static BakeryAudioManager Instance { get; private set; }
    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("Bakery Sound Effects")]
    [SerializeField] private AudioClip cookingStart;
    [SerializeField] private AudioClip cookingDone;
    [SerializeField] private AudioClip dishServed;
    [SerializeField] private AudioClip coinsEarned;
    [SerializeField] private AudioClip customerArrives;

    [Header("Individual Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float cookingStartVolume = 0.7f;

    [Range(0f, 1f)]
    [SerializeField] private float cookingDoneVolume = 0.8f;

    [Range(0f, 1f)]
    [SerializeField] private float dishServedVolume = 0.7f;

    [Range(0f, 1f)]
    [SerializeField] private float coinsEarnedVolume = 0.7f;

    [Range(0f, 1f)]
    [SerializeField] private float customerArrivesVolume = 0.7f;

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

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError(
                "BakeryAudioManager requires an AudioSource.",
                this
            );

            enabled = false;
            return;
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    // ADD OnDestroy() HERE
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void PlayCookingStart()
    {
        PlaySound(cookingStart, cookingStartVolume);
    }

    public void PlayCookingDone()
    {
        PlaySound(cookingDone, cookingDoneVolume);
    }

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
            Debug.LogWarning(
                "BakeryAudioManager: A sound clip has not been assigned.",
                this);
            return;
        }

        if (audioSource == null)
            return;

        // PlayOneShot allows sounds to overlap
        // without interrupting one another.
        audioSource.PlayOneShot(clip, volume);
    }

    private void OnEnable()
    {
        SubscribeToWallet();
    }

    private void Start()
    {
        // Retry in case the Wallet wasn't available earlier.
        SubscribeToWallet();
    }

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

        if (currentWallet == subscribedWallet)
            return;

        if (subscribedWallet != null)
        {
            subscribedWallet.CoinsChanged -= OnCoinsChanged;
        }

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