using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DunkAimBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private Slider aimSlider;
    [SerializeField] private RectTransform hitZone;
    [SerializeField] private TMP_Text resultText;

    [Header("Result")]
    [SerializeField] private float resultDuration = 1.2f;

    private float aimValue;
    private float hitZoneCenter;
    private float hitZoneSize;
    private float resultTimer;

    private bool dunkActive;
    private bool resultShowing;

    private void Awake()
    {
        if (panelGroup == null)
            panelGroup = GetComponent<CanvasGroup>();

        if (aimSlider != null)
        {
            aimSlider.minValue = 0f;
            aimSlider.maxValue = 1f;
            aimSlider.wholeNumbers = false;
            aimSlider.direction = Slider.Direction.BottomToTop;
            aimSlider.interactable = false;
        }

        if (resultText != null)
            resultText.gameObject.SetActive(false);

        HidePanel();
    }

    private void Update()
    {
        // No time limit before a throw.
        // The result timer starts only after the player throws.
        if (!resultShowing)
            return;

        resultTimer -= Time.unscaledDeltaTime;

        if (resultTimer <= 0f)
            EndDunk();
    }

    public void BeginDunk(float zoneCenter, float zoneSize)
    {
        dunkActive = true;
        resultShowing = false;

        if (resultText != null)
        {
            resultText.text = "";
            resultText.gameObject.SetActive(false);
        }

        SetHitZone(zoneCenter, zoneSize);
        SetAimValue(0.5f);

        ShowPanel();

        Debug.Log("DunkAimBarUI: Panel shown.");
    }

    // Receives a normalized aim value from the fake driver
    // or, later, the real dunk system.
    public void SetAimValue(float value)
    {
        aimValue = Mathf.Clamp01(value);

        if (aimSlider != null)
            aimSlider.SetValueWithoutNotify(aimValue);
    }

    // Receives the hit-zone center and size from the settings.
    public void SetHitZone(float center, float size)
    {
        hitZoneCenter = Mathf.Clamp01(center);
        hitZoneSize = Mathf.Clamp01(size);

        UpdateHitZoneVisual();
    }

    private void UpdateHitZoneVisual()
    {
        if (hitZone == null)
            return;

        RectTransform track =
            hitZone.parent as RectTransform;

        if (track == null)
            return;

        float height = track.rect.height;

        // The height is calculated from the supplied size.
        hitZone.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            height * hitZoneSize
        );

        // Position the zone using the normalized center.
        Vector2 position = hitZone.anchoredPosition;
        position.y = (hitZoneCenter - 0.5f) * height;

        hitZone.anchoredPosition = position;
    }

    // Called when the player throws.

    // Used by the fake driver during testing.
    public void RegisterThrow()
    {
        if (!dunkActive || resultShowing)
            return;

        float distance = Mathf.Abs(aimValue - hitZoneCenter);
        bool hit = distance <= hitZoneSize * 0.5f;

        RegisterThrow(hit);
    }

    // Used by the real Grandfather dunk.
    public void RegisterThrow(bool hit)
    {
        if (!dunkActive || resultShowing)
            return;

        if (resultText != null)
        {
            resultText.text = hit ? "x2!" : "Miss";
            resultText.gameObject.SetActive(true);
        }

        resultShowing = true;
        resultTimer = resultDuration;
    }

    public void EndDunk()
    {
        dunkActive = false;
        resultShowing = false;

        HidePanel();
    }

    private void ShowPanel()
    {
        if (panelGroup == null)
            return;

        panelGroup.alpha = 1f;
        panelGroup.interactable = true;
        panelGroup.blocksRaycasts = true;
    }

    private void HidePanel()
    {
        if (panelGroup == null)
            return;

        panelGroup.alpha = 0f;
        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;
    }
}