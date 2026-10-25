using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WandCooldownUI : MonoBehaviour
{
    [Header("Wand Cooldown UI")]
    [SerializeField] private Image cooldownFill;

    private Coroutine cooldownRoutine;

    private void OnEnable()
    {
        // Listen for the wand firing event.
        WandFire1.WandFired += OnWandFired;

        if (cooldownFill != null)
        {
            cooldownFill.fillAmount = 1f;
        }
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
    }

    private void OnWandFired(float cooldownDuration)
    {
        if (cooldownFill == null)
        {
            Debug.LogWarning(
                "WandCooldownUI: Cooldown Fill is missing."
            );
            return;
        }

        // Restart the indicator if another firing event arrives.
        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
            cooldownRoutine = null;
        }

        cooldownFill.fillAmount = 0f;

        cooldownRoutine = StartCoroutine(
            FillCooldown(cooldownDuration)
        );
    }

    private IEnumerator FillCooldown(float duration)
    {
        if (duration <= 0f)
        {
            cooldownFill.fillAmount = 1f;
            cooldownRoutine = null;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            cooldownFill.fillAmount =
                Mathf.Clamp01(elapsed / duration);

            yield return null;
        }

        cooldownFill.fillAmount = 1f;
        cooldownRoutine = null;
    }
}