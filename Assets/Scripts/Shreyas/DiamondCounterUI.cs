using UnityEngine;
using TMPro;
using System.Collections;

public class DiamondCounterUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text diamondAmountText;

    [Header("Animation Settings")]
    [SerializeField] private Color flashColor = new Color(1f, 0.9f, 0.2f, 1f); // Gold/Yellow flash

    private Wallet subscribedWallet;
    private Coroutine popCoroutine;
    private Vector3 originalScale;
    private Color originalColor;
    private int displayedDiamonds;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        subscribedWallet = Wallet.Instance;

        if (subscribedWallet == null)
        {
            Debug.LogError("DiamondCounterUI: Wallet not found!");
            return;
        }

        // Listen for changes to the diamond balance.
        subscribedWallet.DiamondsChanged += HandleDiamondsChanged;

        // Display the current balance immediately.
        displayedDiamonds = subscribedWallet.Diamonds;
        UpdateDiamondText(displayedDiamonds);

        if (diamondAmountText != null)
        {
            originalColor = diamondAmountText.color;
        }
    }

    private void OnDisable()
    {
        if (subscribedWallet != null)
        {
            subscribedWallet.DiamondsChanged -= HandleDiamondsChanged;
            subscribedWallet = null;
        }

        if (popCoroutine != null)
        {
            StopCoroutine(popCoroutine);
            popCoroutine = null;
        }

        transform.localScale = originalScale;

        if (diamondAmountText != null)
        {
            diamondAmountText.color = originalColor;
        }
    }

    private void HandleDiamondsChanged(int newAmount)
    {
        // TICKET REQUIREMENT: Only pop/flash when diamonds go UP
        if (newAmount > displayedDiamonds)
        {
            PlayPopAnimation();
        }

        displayedDiamonds = newAmount;
        UpdateDiamondText(newAmount);
    }

    private void UpdateDiamondText(int amount)
    {
        if (diamondAmountText != null)
        {
            diamondAmountText.text = "Diamonds " + amount;
        }
        else
        {
            Debug.LogError("DiamondCounterUI: Assign the TextMeshPro component!");
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

        // Flash the text color
        if (diamondAmountText != null)
        {
            diamondAmountText.color = flashColor;
        }

        // Enlarge the counter.
        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(originalScale, enlargedScale, elapsed / duration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localScale = enlargedScale;

        // Return to the original size.
        elapsed = 0f;

        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(enlargedScale, originalScale, elapsed / duration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localScale = originalScale;

        // Reset color back to normal
        if (diamondAmountText != null)
        {
            diamondAmountText.color = originalColor;
        }

        popCoroutine = null;
    }
} // <--- THIS IS THE FINAL CLOSING BRACE FOR THE CLASS