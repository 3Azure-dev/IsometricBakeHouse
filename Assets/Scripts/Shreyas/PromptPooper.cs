using System.Collections;
using UnityEngine;

// Goes on the world-space Canvas root of the prompt.
// PopIn: activates the canvas and fades/scales it in.
// PopOut: fades/scales it out, then deactivates the canvas.
public class PromptPopper : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;      // Auto-added to the canvas root if empty
    [SerializeField] private RectTransform bubble;   // The [E] + label container (child of the canvas). Empty = first child
    [SerializeField] private float popDuration = 0.18f;
    [SerializeField] private float startScale = 0.6f;
    [SerializeField] private float bobHeight = 4f;  // In canvas units
    [SerializeField] private float bobSpeed = 5f;
    [SerializeField] private bool faceCamera = true;

    private Camera cam;
    private Vector2 basePos;
    private Coroutine routine;

    // new fields, next to the others
    [SerializeField] private Transform followTarget;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);



    private void Awake()
    {
        if (group == null)
        {
            group = GetComponent<CanvasGroup>();
            if (group == null) group = gameObject.AddComponent<CanvasGroup>();
        }

        if (bubble == null && transform.childCount > 0)
            bubble = transform.GetChild(0) as RectTransform;

        if (bubble != null) basePos = bubble.anchoredPosition;

        cam = Camera.main;
        group.alpha = 0f;
    }

    public void PopIn()
    {
        gameObject.SetActive(true);
        Run(1f, false);
    }

    public void PopOut()
    {
        if (!gameObject.activeInHierarchy) return;
        Run(0f, true);
    }

    private void Run(float target, bool disableAtEnd)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Animate(target, disableAtEnd));
    }

    private IEnumerator Animate(float target, bool disableAtEnd)
    {
        float from = group.alpha;
        float duration = Mathf.Max(0.01f, popDuration * Mathf.Abs(target - from));
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            Apply(Mathf.Lerp(from, target, t / duration));
            yield return null;
        }

        Apply(target);
        routine = null;

        if (disableAtEnd) gameObject.SetActive(false);
    }

    private void Apply(float a)
    {
        group.alpha = a;
        if (bubble != null) bubble.localScale = Vector3.one * Mathf.Lerp(startScale, 1f, a);
    }

    private void LateUpdate()
    {
        // in LateUpdate(), as the first lines
        if (followTarget != null)
            transform.position = followTarget.position + worldOffset;

        if (bubble != null && group.alpha > 0.01f)
            bubble.anchoredPosition = basePos + Vector2.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);

        if (bubble == (RectTransform)transform)
            bubble = transform.childCount > 0 ? transform.GetChild(0) as RectTransform : null;
    }



}