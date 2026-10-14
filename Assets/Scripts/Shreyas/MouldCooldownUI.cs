using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MouldCooldownUI : MonoBehaviour
{
    [SerializeField] private Image cooldownFill;

    private Coroutine cooldownRoutine;

    private void OnEnable()
    {
        MouldZone.MouldPlaced += OnMouldPlaced;

        if (cooldownFill != null)
        {
            cooldownFill.fillAmount = 1f;
        }
    }

    private void OnDisable()
    {
        MouldZone.MouldPlaced -= OnMouldPlaced;

        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
            cooldownRoutine = null;
        }
    }

    private void OnMouldPlaced(MouldZone zone)
    {
        if (cooldownFill == null || zone == null)
        {
            Debug.LogWarning(
                "MouldCooldownUI: Cooldown Fill or MouldZone is missing."
            );
            return;
        }

        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
        }

        cooldownFill.fillAmount = 0f;

        cooldownRoutine = StartCoroutine(
            FillCooldown(zone.CooldownDuration)
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