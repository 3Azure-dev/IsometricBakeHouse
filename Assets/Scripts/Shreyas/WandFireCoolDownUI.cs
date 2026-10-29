using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WandCooldownUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WandFire1 wand; // Drag your Wand GameObject here
    [SerializeField] private Image cooldownFill;
    [SerializeField] private TMP_Text cooldownTimerText;

    [Header("Visuals")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color penaltyColor = new Color(1f, 0.3f, 0.3f); // Red flash
    [SerializeField] private float flashTime = 0.5f;

    private float lastRemaining;
    private float flashTimer;

    private void Update()
    {
        if (wand == null) return;

        // 1. Read the ACTUAL time from the wand
        float remaining = wand.CooldownRemaining;
        float max = wand.CooldownMax;

        // 2. Detect if time was ADDED (e.g., tomato hit +3s)
        if (remaining > lastRemaining + 0.05f)
        {
            flashTimer = flashTime; // Trigger the red flash!
        }
        lastRemaining = remaining;

        bool isOnCooldown = remaining > 0.05f;

        // 3. Update the Bar
        if (cooldownFill != null)
        {
            // (1 - remaining/max) makes it fill UP from 0 to 1 as time passes, 
            // matching your original visual style.
            float fill = max > 0f ? 1f - Mathf.Clamp01(remaining / max) : 1f;
            cooldownFill.fillAmount = fill;

            // Apply color flash
            cooldownFill.color = (flashTimer > 0f) ? penaltyColor : normalColor;
        }

        // 4. Update the Text
        if (cooldownTimerText != null)
        {
            if (isOnCooldown)
            {
                cooldownTimerText.gameObject.SetActive(true);
                cooldownTimerText.text = $"{remaining:0.0}s";
            }
            else
            {
                cooldownTimerText.gameObject.SetActive(false);
                cooldownTimerText.text = "";
            }
        }

        // Tick down the flash timer
        if (flashTimer > 0f) flashTimer -= Time.deltaTime;
    }
}