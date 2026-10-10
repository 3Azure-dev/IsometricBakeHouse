using UnityEngine;
using TMPro;
using System.Collections;

public class CoinCounterUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text coinAmountText;

    private Wallet subscribedWallet;
    private Coroutine popCoroutine;
    private Vector3 originalScale;
    private int displayedCoins;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        // Get the existing Wallet.
        subscribedWallet = Wallet.Instance;

        if (subscribedWallet == null)
        {
            Debug.LogError("CoinCounterUI: Wallet not found!");
            return;
        }

        // Listen for changes to the real coin balance.
        subscribedWallet.CoinsChanged += HandleCoinsChanged;

        // Display the current balance immediately.
        displayedCoins = subscribedWallet.Coins;
        UpdateCoinText(displayedCoins);
    }

    private void OnDisable()
    {
        // Stop listening when the UI is disabled.
        if (subscribedWallet != null)
        {
            subscribedWallet.CoinsChanged -= HandleCoinsChanged;
            subscribedWallet = null;
        }

        if (popCoroutine != null)
        {
            StopCoroutine(popCoroutine);
            popCoroutine = null;
        }

        transform.localScale = originalScale;
    }

    private void HandleCoinsChanged(int newAmount)
    {
        if (newAmount == displayedCoins)
            return;

        displayedCoins = newAmount;

        UpdateCoinText(newAmount);
        PlayPopAnimation();
    }

    private void UpdateCoinText(int amount)
    {
        if (coinAmountText != null)
        {
            coinAmountText.text = "🪙 " + amount;
        }
        else
        {
            Debug.LogError(
                "CoinCounterUI: Assign the TextMeshPro component!"
            );
        }
    }

    private void PlayPopAnimation()
    {
        if (popCoroutine != null)
        {
            StopCoroutine(popCoroutine);
        }

        transform.localScale = originalScale;
        popCoroutine = StartCoroutine(PopAnimation());
    }

    private IEnumerator PopAnimation()
    {
        float duration = 0.12f;
        float elapsed = 0f;

        Vector3 enlargedScale = originalScale * 1.15f;

        // Enlarge the counter.
        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(
                originalScale,
                enlargedScale,
                elapsed / duration
            );

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localScale = enlargedScale;

        // Return to the original size.
        elapsed = 0f;

        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(
                enlargedScale,
                originalScale,
                elapsed / duration
            );

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
        popCoroutine = null;
    }
}