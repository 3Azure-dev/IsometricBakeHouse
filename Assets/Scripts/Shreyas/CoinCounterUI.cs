using UnityEngine;
using TMPro;
using System.Collections;

public class CoinCounterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text coinAmountText;

    private int coins = 250;
    private Coroutine popCoroutine;

    private void Start()
    {
        UpdateCoinText();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            TestCoinChange();
        }
    }

    public void SetCoins(int newAmount)
    {
        if (coins == newAmount)
            return;

        coins = newAmount;
        UpdateCoinText();
        PlayPopAnimation();
    }

    private void UpdateCoinText()
    {
        if (coinAmountText != null)
        {
            coinAmountText.text = "?? " + coins;
        }
    }

    private void PlayPopAnimation()
    {
        if (popCoroutine != null)
            StopCoroutine(popCoroutine);

        popCoroutine = StartCoroutine(PopAnimation());
    }

    private IEnumerator PopAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 enlargedScale = originalScale * 1.15f;

        float duration = 0.12f;
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
    public void TestCoinChange()
    {
        SetCoins(coins + 10);
    }
}