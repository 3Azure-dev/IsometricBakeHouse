using UnityEngine;
using TMPro;
using System.Collections;

public class CoinCounterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text coinAmountText;

    private int coins;
    private Coroutine popCoroutine;
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        Wallet wallet = Wallet.Instance;

        if (wallet == null)
        {
            Debug.LogError("CoinCounterUI: Wallet.Instance was not found.");
            return;
        }

        // Subscribe to the existing Wallet event.
        wallet.CoinsChanged += HandleCoinsChanged;

        // Display the current balance immediately.
        coins = wallet.Coins;
        UpdateCoinText();
    }

    private void OnDisable()
    {
        // Unsubscribe when this UI is disabled or destroyed.
        if (Wallet.Instance != null)
        {
            Wallet.Instance.CoinsChanged -= HandleCoinsChanged;
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
        if (coins == newAmount)
            return;

        coins = newAmount;

        UpdateCoinText();
        PlayPopAnimation();
    }

    private void UpdateCoinText()
    {
        if (coinAmountText == null)
        {
            Debug.LogError(
                "CoinCounterUI: Assign the TextMeshPro component in the Inspector."
            );
            return;
        }

        coinAmountText.text = "🪙 " + coins;
    }

    private void PlayPopAnimation()
    {
        if (popCoroutine != null)
        {
            StopCoroutine(popCoroutine);
            popCoroutine = null;
        }

        transform.localScale = originalScale;
        popCoroutine = StartCoroutine(PopAnimation());
    }

    private IEnumerator PopAnimation()
    {
        float duration = 0.12f;
        Vector3 enlargedScale = originalScale * 1.15f;
        float elapsed = 0f;

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