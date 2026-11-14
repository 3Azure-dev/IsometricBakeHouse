using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MouldCooldownUI : MonoBehaviour
{
    [SerializeField] private Image cooldownFill;

    [Header("Mould icon (drag the sprite asset here)")]
    [SerializeField] private Sprite mouldSprite;   // ICON_Mould
    [SerializeField] private Vector2 iconSize = new Vector2(48f, 48f);
    [SerializeField] private float iconGap = 8f;

    private GameObject iconObject;
    private Coroutine cooldownRoutine;

    private void Start()
    {
        CreateIcon();
    }

    // Builds a UI Image to the left of the bar, hidden until a cooldown starts.
    // Builds a UI Image above the bar, hidden until a cooldown starts.
    private void CreateIcon()
    {
        if (mouldSprite == null || cooldownFill == null) return;

        Canvas.ForceUpdateCanvases();

        RectTransform bar = cooldownFill.rectTransform;

        iconObject = new GameObject("MouldIcon", typeof(RectTransform), typeof(Image));
        RectTransform rect = iconObject.GetComponent<RectTransform>();
        rect.SetParent(bar.parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = iconSize;

        // Top-middle of the bar in world space, then shift up by gap + half the icon height.
        Vector3[] c = new Vector3[4];
        bar.GetWorldCorners(c);
        Vector3 topMiddle = (c[1] + c[2]) * 0.5f;
        rect.position = topMiddle + Vector3.up * (iconGap + iconSize.y * 0.5f) * rect.lossyScale.y;

        Image img = iconObject.GetComponent<Image>();
        img.sprite = mouldSprite;
        img.preserveAspect = true;
        img.raycastTarget = false;
        img.color = Color.white; // real art, never tinted

        iconObject.SetActive(false); // hidden until the cooldown is active
    }

    private void SetIconVisible(bool visible)
    {
        if (iconObject != null)
            iconObject.SetActive(visible);
    }

    private void OnEnable()
    {
        MouldZone.MouldPlaced += OnMouldPlaced;

        if (cooldownFill != null)
        {
            cooldownFill.fillAmount = 1f;
        }

        SetIconVisible(false);
    }

    private void OnDisable()
    {
        MouldZone.MouldPlaced -= OnMouldPlaced;

        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
            cooldownRoutine = null;
        }

        SetIconVisible(false);
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
        SetIconVisible(true); // cooldown started

        cooldownRoutine = StartCoroutine(
            FillCooldown(zone.CooldownDuration)
        );
    }

    private IEnumerator FillCooldown(float duration)
    {
        if (duration <= 0f)
        {
            cooldownFill.fillAmount = 1f;
            SetIconVisible(false);
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
        SetIconVisible(false); // cooldown finished
        cooldownRoutine = null;
    }
}