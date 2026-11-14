using UnityEngine;
using UnityEngine.UI;

// Shows the zone icon to the left of the 0/5 counter.
public class ZoneCounterIcon : MonoBehaviour
{
    [Header("Drag the 0/5 text object here")]
    [SerializeField] private RectTransform counterText;

    [Header("Drag the sprite asset here")]
    [SerializeField] private Sprite zoneSprite;   // ICON_Zone

    [Header("Layout")]
    [SerializeField] private Vector2 iconSize = new Vector2(48f, 48f);
    [SerializeField] private float gap = 8f;

    [Tooltip("Positive = move the icon up, negative = down.")]
    [SerializeField] private float yOffset = 10f;

    private void Start()
    {
        if (counterText == null || zoneSprite == null) return;

        Canvas.ForceUpdateCanvases();

        GameObject go = new GameObject("ZoneIcon", typeof(RectTransform), typeof(Image));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(counterText.parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = iconSize;

        // Left-middle of the counter text, shifted left by gap + half the icon.
        Vector3[] c = new Vector3[4];
        counterText.GetWorldCorners(c);
        Vector3 leftMiddle = (c[0] + c[1]) * 0.5f;
        rect.position = leftMiddle + Vector3.left * (gap + iconSize.x * 0.5f) * rect.lossyScale.x;

        Image img = go.GetComponent<Image>();
        img.sprite = zoneSprite;
        img.preserveAspect = true;
        img.raycastTarget = false;
        img.color = Color.white;   // real art, never tinted
    }
}