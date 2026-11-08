using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Shared logic for "world-space E prompt" behaviours.
// A prompt is shown only when:  ConditionMet (game state)  AND  player is within range.
// Pressing the interact key while it is showing fires onInteract.
// The label text can change while the prompt is visible (override CurrentLabel).
public abstract class ProximityPromptBase : MonoBehaviour
{
    [Header("Prompt")]
    [SerializeField] protected GameObject promptRoot;   // The world-space Canvas (must NOT be this same GameObject)
    [SerializeField] private TMP_Text label;            // Optional: the label text inside the pill
    [SerializeField] private string labelText = "Interact";

    [Header("Interaction")]
    [SerializeField] private Transform player;          // Leave empty to auto-find the object tagged "Player"
    [SerializeField] private Transform rangeCenter;     // Leave empty to measure from this object
    [SerializeField] private float range = 2f;          // World units, measured on the ground plane (XZ)
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private UnityEvent onInteract;     // Wire the existing "take food" / "clean plates" method here

    [Header("Debug")]
    [SerializeField] private bool debugForceShow;

    private PromptPopper popper;
    private bool isShowing;

    // Each prompt says when its game-state condition is true.
    public virtual bool ConditionMet => false;

    // Text shown in the label pill. Override to change it with the game state.
    protected virtual string CurrentLabel => labelText;

    protected virtual void Awake()
    {
        if (rangeCenter == null) rangeCenter = transform;

        if (promptRoot == null)
        {
            Debug.LogError($"{name}: Prompt Root is NOT assigned in Inspector!", this);
            return;
        }

        if (promptRoot == gameObject)
        {
            Debug.LogError($"{name}: Prompt Root is the same GameObject as this script. Put this script on the stove/table instead, otherwise it disables itself.", this);
            promptRoot = null;
            return;
        }

        popper = promptRoot.GetComponent<PromptPopper>();
        promptRoot.SetActive(false);

        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
            else Debug.LogError($"{name}: No Player assigned and no object tagged 'Player' found.", this);
        }
    }

    private void Update()
    {
        if (promptRoot == null || player == null || rangeCenter == null) return;

        bool shouldShow = debugForceShow || (ConditionMet && IsPlayerInRange());

        if (shouldShow && !isShowing) Show();
        else if (!shouldShow && isShowing) Hide();

        if (isShowing)
        {
            RefreshLabel();

            if (Input.GetKeyDown(interactKey))
                onInteract.Invoke();
        }
    }

    private bool IsPlayerInRange()
    {
        Vector3 a = player.position;
        Vector3 b = rangeCenter.position;
        a.y = 0f;
        b.y = 0f;
        return (a - b).sqrMagnitude <= range * range;
    }

    private void Show()
    {
        isShowing = true;

        string text = CurrentLabel;
        if (label != null && !string.IsNullOrEmpty(text))
            label.text = text;

        promptRoot.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(promptRoot.GetComponent<RectTransform>());

        if (popper != null) popper.PopIn();
    }

    private void Hide()
    {
        isShowing = false;

        // PromptPopper deactivates the canvas itself once the pop-out finishes.
        if (popper != null) popper.PopOut();
        else promptRoot.SetActive(false);
    }

    // Updates the text while the prompt stays on screen (e.g. "Cook" -> "Take food").
    private void RefreshLabel()
    {
        if (label == null) return;

        string text = CurrentLabel;
        if (string.IsNullOrEmpty(text) || label.text == text) return;

        label.text = text;
        LayoutRebuilder.ForceRebuildLayoutImmediate(promptRoot.GetComponent<RectTransform>());
    }

    // Kept public so existing code can still call it.
    public void HidePrompt()
    {
        if (promptRoot == null || !isShowing) return;
        Hide();
    }

    private void OnDrawGizmosSelected()
    {
        Transform c = rangeCenter != null ? rangeCenter : transform;
        Gizmos.color = new Color(0.11f, 0.62f, 0.46f, 1f);
        Gizmos.DrawWireSphere(c.position, range);
    }
}