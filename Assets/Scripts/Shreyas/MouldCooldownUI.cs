using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MouldCooldownUI : MonoBehaviour
{
    [Header("Cooldown Icon")]
    [SerializeField] private Image darkIcon;
    [SerializeField] private Image cooldownFill;

    [Header("Icon Colours")]
    [SerializeField]
    private Color darkColor =
        new Color(0.25f, 0.25f, 0.25f, 1f);

    [SerializeField] private Color readyColor = Color.white;

    private Coroutine cooldownRoutine;

    private void Awake()
    {
        // Configure the circular fill.
        if (cooldownFill != null)
        {
            cooldownFill.type = Image.Type.Filled;
            cooldownFill.fillMethod = Image.FillMethod.Radial360;
            cooldownFill.fillOrigin = (int)Image.Origin360.Top;
            cooldownFill.fillClockwise = true;
            cooldownFill.fillAmount = 1f;
            cooldownFill.color = readyColor;
        }

        // Keep the dark icon behind the coloured fill.
        if (darkIcon != null)
        {
            darkIcon.color = darkColor;
        }
    }

    private void OnEnable()
    {
        MouldZone.MouldPlaced += HandleMouldPlaced;
    }

    private void OnDisable()
    {
        MouldZone.MouldPlaced -= HandleMouldPlaced;

        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
            cooldownRoutine = null;
        }
    }

    private void HandleMouldPlaced(MouldZone zone)
    {
        if (zone == null)
            return;

        if (cooldownFill == null)
        {
            Debug.LogError(
                "MouldCooldownUI: Cooldown Fill is not assigned!"
            );
            return;
        }

        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
        }

        // Dark icon remains visible while the fill is empty.
        cooldownFill.fillAmount = 0f;

        cooldownRoutine = StartCoroutine(
            RunCooldown(zone.CooldownDuration)
        );

        Debug.Log("Mould cooldown indicator started.");
    }

    private IEnumerator RunCooldown(float duration)
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

        Debug.Log("Mould cooldown indicator is ready.");
    }
}