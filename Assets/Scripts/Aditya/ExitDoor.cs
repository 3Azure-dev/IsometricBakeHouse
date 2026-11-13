using UnityEngine;
using UnityEngine.InputSystem;

// Exit door of a hunt level.
//   Player near + hunt NOT finished -> LOCKED sprite fades in, E does nothing.
//   Player near + hunt finished     -> OPEN sprite fades in, E completes the level.
[RequireComponent(typeof(SpriteRenderer))]
public class ExitDoor1 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject ePrompt;
    [SerializeField] private LevelCompleteUI levelCompleteUI;

    [Header("Door sprites (drag the sprite assets here)")]
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite openSprite;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Tooltip("ON: the door icon only appears while the player is near it.\nOFF: the door is always visible.")]
    [SerializeField] private bool showOnlyWhenNear = true;

    private SpriteRenderer lockedRenderer;
    private SpriteRenderer openRenderer;

    private bool playerInRange;
    private bool levelCompleted;
    private bool unlocked;

    private void Awake()
    {
        // Locked door uses the SpriteRenderer on this object.
        lockedRenderer = GetComponent<SpriteRenderer>();
        lockedRenderer.sprite = lockedSprite;

        // Open door uses an auto-created child with the same position/size.
        GameObject child = new GameObject("OpenDoorSprite");
        child.transform.SetParent(transform, false);
        openRenderer = child.AddComponent<SpriteRenderer>();
        openRenderer.sprite = openSprite;
        openRenderer.sortingLayerID = lockedRenderer.sortingLayerID;
        openRenderer.sortingOrder = lockedRenderer.sortingOrder + 1;

        // Start locked.
        SetAlpha(lockedRenderer, showOnlyWhenNear ? 0f : 1f);
        SetAlpha(openRenderer, 0f);
    }

    private void OnEnable() => GrandfatherDunk.HuntFinished += Unlock;
    private void OnDisable() => GrandfatherDunk.HuntFinished -= Unlock;

    private void Start()
    {
        if (ePrompt != null)
            ePrompt.SetActive(false);
    }

    private void Unlock()
    {
        unlocked = true;
        Debug.Log("Exit door unlocked");

        if (playerInRange && ePrompt != null)
            ePrompt.SetActive(true);
    }

    [ContextMenu("Test: unlock door")]
    private void TestUnlock() => Unlock();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        if (unlocked && ePrompt != null)
            ePrompt.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (ePrompt != null)
            ePrompt.SetActive(false);
    }

    private void Update()
    {
        UpdateFade();

        if (unlocked && playerInRange && !levelCompleted &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            levelCompleted = true;

            if (ePrompt != null)
                ePrompt.SetActive(false);

            if (levelCompleteUI != null)
                levelCompleteUI.ShowLevelComplete();
        }
    }

    private void UpdateFade()
    {
        bool visible = !showOnlyWhenNear || playerInRange;

        float lockedTarget = (visible && !unlocked) ? 1f : 0f;
        float openTarget = (visible && unlocked) ? 1f : 0f;

        float step = fadeDuration > 0f ? Time.deltaTime / fadeDuration : 1f;

        FadeTowards(lockedRenderer, lockedTarget, step);
        FadeTowards(openRenderer, openTarget, step);
    }

    private static void FadeTowards(SpriteRenderer r, float target, float step)
    {
        if (r == null) return;

        Color c = r.color;
        if (Mathf.Approximately(c.a, target)) return;

        c.a = Mathf.MoveTowards(c.a, target, step);
        r.color = c;
    }

    private static void SetAlpha(SpriteRenderer r, float alpha)
    {
        if (r != null)
            r.color = new Color(1f, 1f, 1f, alpha);
    }
}