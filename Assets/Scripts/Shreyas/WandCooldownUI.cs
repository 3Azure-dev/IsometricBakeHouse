using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WandCooldownUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WandFire1 wand; // Drag your Wand GameObject here
    [SerializeField] private Image cooldownFill;
    [SerializeField] private TMP_Text cooldownTimerText;

    [Header("Wand icon (drag the sprite asset here)")]
    [SerializeField] private Sprite wandSprite;
    [SerializeField] private Vector2 iconSize = new Vector2(64f, 64f);
    [Tooltip("How far above the timer text the icon sits (in canvas units).")]
    [SerializeField] private float iconOffsetY = 60f;

    [Header("Visuals")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color penaltyColor = new Color(1f, 0.3f, 0.3f); // Red flash
    [SerializeField] private float flashTime = 0.5f;

    private GameObject wandIconObject;
    private float lastRemaining;
    private float flashTimer;

    private void Awake()
    {
        CreateWandIcon();
    }

    // Builds a UI Image next to the timer text and gives it the sprite.
    private void CreateWandIcon()
    {
        if (wandSprite == null || cooldownTimerText == null) return;

        RectTransform textRect = cooldownTimerText.rectTransform;

        wandIconObject = new GameObject("WandIcon", typeof(RectTransform), typeof(Image));
        RectTransform rect = wandIconObject.GetComponent<RectTransform>();
        rect.SetParent(textRect.parent, false);

        // Match the timer's anchoring, then sit above it.
        rect.anchorMin = textRect.anchorMin;
        rect.anchorMax = textRect.anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = textRect.anchoredPosition + new Vector2(0f, iconOffsetY);
        rect.sizeDelta = iconSize;

        Image img = wandIconObject.GetComponent<Image>();
        img.sprite = wandSprite;
        img.preserveAspect = true;
        img.raycastTarget = false;
        img.color = Color.white; // real art, never tinted

        wandIconObject.SetActive(false); // hidden until on cooldown
    }

    private void Update()
    {
        if (wand == null) return;

        float remaining = wand.CooldownRemaining;
        float max = wand.CooldownMax;

        // Detect if time was ADDED (e.g., tomato hit +3s)
        if (remaining > lastRemaining + 0.05f)
        {
            flashTimer = flashTime;
        }
        lastRemaining = remaining;

        bool isOnCooldown = remaining > 0.05f;

        // Bar
        if (cooldownFill != null)
        {
            float fill = max > 0f ? 1f - Mathf.Clamp01(remaining / max) : 1f;
            cooldownFill.fillAmount = fill;
            cooldownFill.color = (flashTimer > 0f) ? penaltyColor : normalColor;
        }

        // Timer text
        if (cooldownTimerText != null)
        {
            cooldownTimerText.gameObject.SetActive(isOnCooldown);
            cooldownTimerText.text = isOnCooldown ? $"{remaining:0.0}s" : "";
        }

        // Wand icon: visible only while on cooldown
        if (wandIconObject != null)
            wandIconObject.SetActive(isOnCooldown);

        if (flashTimer > 0f) flashTimer -= Time.deltaTime;
    }
}