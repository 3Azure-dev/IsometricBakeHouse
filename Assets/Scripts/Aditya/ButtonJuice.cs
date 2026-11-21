using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Hover pop + click wobble for any button or menu prop. Works while paused.
public class ButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float popSpeed = 12f;

    [Header("Click wobble")]
    [SerializeField] private float wobbleAngle = 6f;
    [SerializeField] private float wobbleTime = 0.35f;

    private Vector3 baseScale;
    private Vector3 targetScale;
    private Selectable selectable;
    private Coroutine wobble;

    // locked or greyed buttons don't react
    private bool CanReact => selectable == null || selectable.interactable;

    private void Awake()
    {
        baseScale = transform.localScale;
        targetScale = baseScale;
        selectable = GetComponent<Selectable>();
    }

    private void OnDisable()
    {
        // panel closed mid-hover: snap back
        transform.localScale = baseScale;
        transform.localRotation = Quaternion.identity;
        targetScale = baseScale;
        wobble = null;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, popSpeed * Time.unscaledDeltaTime);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CanReact) targetScale = baseScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = baseScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!CanReact || !isActiveAndEnabled) return;
        if (wobble != null) StopCoroutine(wobble);
        wobble = StartCoroutine(Wobble());
    }

    private IEnumerator Wobble()
    {
        float t = 0f;
        while (t < wobbleTime)
        {
            t += Time.unscaledDeltaTime;
            float fade = 1f - t / wobbleTime;
            float angle = Mathf.Sin(t * 40f) * wobbleAngle * fade;
            transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }
        transform.localRotation = Quaternion.identity;
        wobble = null;
    }
}