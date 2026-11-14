using UnityEngine;
using UnityEngine.UI;

// Fills a SLICED image from the left by changing its width (0 = empty, 1 = full).
[RequireComponent(typeof(Image))]
public class SlicedBarFill : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float fillAmount = 0f;

    [Tooltip("Gap between the fill and the edge of the back, in pixels.")]
    [SerializeField] private float inset = 3f;

    public float FillAmount
    {
        get => fillAmount;
        set
        {
            fillAmount = Mathf.Clamp01(value);
            Apply();
        }
    }

    private void Awake() => Apply();

    // Lets you see the result in edit mode when you drag the slider.
    private void OnValidate() => Apply();

    private void Apply()
    {
        RectTransform rect = (RectTransform)transform;
        Image img = GetComponent<Image>();

        // Stretch over the parent, but only up to fillAmount on the right side.
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(fillAmount, 1f);
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);

        // Nothing to draw when empty.
        if (img != null)
            img.enabled = fillAmount > 0.001f;
    }
}