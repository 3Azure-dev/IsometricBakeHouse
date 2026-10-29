using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WandCooldownUI : MonoBehaviour
{
    [Header("Wand Cooldown UI")]
    [SerializeField] private Image cooldownFill;
    [SerializeField] private TMP_Text cooldownTimerText;

    private Coroutine cooldownRoutine;

    private void OnEnable()
    {
        // Listen for the wand firing event.
        WandFire1.WandFired += OnWandFired;

        // Start with a full bar and hidden timer.
        if (cooldownFill != null)
        {
            cooldownFill.fillAmount = 1f;
        }

        HideTimer();
    }

    private void OnDisable()
    {
        // Stop listening when this UI is disabled.
        WandFire1.WandFired -= OnWandFired;

        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
            cooldownRoutine = null;
        }

        HideTimer();
    }

    private void OnWandFired(float cooldownDuration)
    {
        if (cooldownFill == null)
        {
            Debug.LogWarning(
                "WandCooldownUI: Cooldown Fill is missing."
            );
        }

        // Stop an existing cooldown display if necessary.
        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
            cooldownRoutine = null;
        }

        // Handle a cooldown of zero or less.
        if (cooldownDuration <= 0f)
        {
            if (cooldownFill != null)
            {
                cooldownFill.fillAmount = 1f;
            }

            HideTimer();
            return;
        }

        // Empty the bar before it refills.
        if (cooldownFill != null)
        {
            cooldownFill.fillAmount = 0f;
        }

        // Show the countdown text.
        if (cooldownTimerText != null)
        {
            cooldownTimerText.gameObject.SetActive(true);
            cooldownTimerText.text = $"{cooldownDuration:0.0}s";
        }

        // Start the cooldown animation and countdown.
        cooldownRoutine = StartCoroutine(
            FillCooldown(cooldownDuration)
        );
    }

    private IEnumerator FillCooldown(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float remaining = Mathf.Max(0f, duration - elapsed);

            // Fill the vertical bar from bottom to top.
            if (cooldownFill != null)
            {
                cooldownFill.fillAmount =
                    Mathf.Clamp01(elapsed / duration);
            }

            // Update the countdown.
            if (cooldownTimerText != null)
            {
                cooldownTimerText.text = $"{remaining:0.0}s";
            }

            yield return null;
        }

        // Cooldown is complete.
        if (cooldownFill != null)
        {
            cooldownFill.fillAmount = 1f;
        }

        cooldownRoutine = null;

        // Hide the timer when the bar is full.
        HideTimer();
    }

    private void HideTimer()
    {
        if (cooldownTimerText != null)
        {
            cooldownTimerText.text = "";
            cooldownTimerText.gameObject.SetActive(false);
        }
    }
}