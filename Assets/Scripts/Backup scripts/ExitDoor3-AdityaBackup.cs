using UnityEngine;
using UnityEngine.InputSystem;

// Exit door of a hunt level (same setup in Bread, Cupcake, Pizza).
//   Door sprite  : always visible. Brown -> flashes red on E while locked -> green once unlocked.
//   Status icon  : small padlock / open-door icon above the door, only while the player is near.
//   E prompt     : only while the player is near AND the door is unlocked, above the icon.
[RequireComponent(typeof(SpriteRenderer))]
public class ExitDoor1AdityaBackup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject ePrompt;
    [SerializeField] private LevelCompleteUI levelCompleteUI;

    [Header("Door colours (tint on the whitebox door sprite)")]
    [SerializeField] private Color lockedColor = new Color(0.55f, 0.35f, 0.15f); // brown
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private Color unlockedColor = Color.green;
    [SerializeField] private float flashDuration = 0.3f;

    [Header("Status icon (small, above the door)")]
    [SerializeField] private Sprite lockedIconSprite;   // padlock
    [SerializeField] private Sprite openIconSprite;     // open door
    [Tooltip("Uniform world size of the icon (not affected by the door's stretched scale).")]
    [SerializeField] private float iconScale = 0.4f;
    [Tooltip("World units between the door's top edge and the icon centre.")]
    [SerializeField] private float iconGapAboveDoor = 0.5f;
    [SerializeField] private float fadeDuration = 0.25f;

    [Header("E prompt")]
    [Tooltip("World units between the icon and the E prompt centre. Increase if they overlap.")]
    [SerializeField] private float promptGapAboveIcon = 1.5f;

    private SpriteRenderer doorRenderer;
    private SpriteRenderer iconRenderer;

    private bool playerInRange;
    private bool levelCompleted;
    private bool unlocked;
    private float flashTimer;

    private void Awake()
    {
        // The door itself: keep whatever whitebox sprite is assigned in the Inspector.
        doorRenderer = GetComponent<SpriteRenderer>();
        doorRenderer.color = lockedColor;

        // Status icon: NOT a child of the door, so the door's 1.5 x 3 scale doesn't stretch it.
        GameObject icon = new GameObject("DoorStatusIcon");
        icon.transform.SetParent(transform.parent, true);
        icon.transform.localScale = Vector3.one * iconScale;

        iconRenderer = icon.AddComponent<SpriteRenderer>();
        iconRenderer.sprite = lockedIconSprite;
        iconRenderer.sortingLayerID = doorRenderer.sortingLayerID;
        iconRenderer.sortingOrder = doorRenderer.sortingOrder + 1;
        iconRenderer.color = new Color(1f, 1f, 1f, 0f);
    }

    private void OnEnable() => GrandfatherDunk.HuntFinished += Unlock;
    private void OnDisable() => GrandfatherDunk.HuntFinished -= Unlock;

    private void Start()
    {
        // Place everything relative to the door's top edge in world space.
        Bounds b = doorRenderer.bounds;
        Vector3 iconPos = new Vector3(b.center.x, b.max.y + iconGapAboveDoor, transform.position.z);
        iconRenderer.transform.position = iconPos;

        if (ePrompt != null)
        {
            // Detach from the stretched door, keep its current world size.
            ePrompt.transform.SetParent(transform.parent, true);
            ePrompt.transform.position = iconPos + Vector3.up * promptGapAboveIcon;
            ePrompt.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (iconRenderer != null)
            Destroy(iconRenderer.gameObject);
    }

    private void Unlock()
    {
        if (unlocked) return; // don't replay if it runs twice

        unlocked = true;
        Debug.Log("Exit door unlocked");

        if (iconRenderer != null)
            iconRenderer.sprite = openIconSprite;

        if (BreadSFX.Instance != null)
            BreadSFX.Instance.PlayExitDoorUnlock();

        RefreshPrompt();
    }

    [ContextMenu("Test: unlock door")]
    private void TestUnlock() => Unlock();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        RefreshPrompt();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        RefreshPrompt();
    }

    private void RefreshPrompt()
    {
        if (ePrompt != null)
            ePrompt.SetActive(playerInRange && unlocked && !levelCompleted);
    }

    private void Update()
    {
        UpdateIconFade();
        UpdateDoorColor();

        if (!playerInRange || levelCompleted) return;
        if (Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame) return;

        if (!unlocked)
        {
            flashTimer = flashDuration;   // locked: flash red
            return;
        }

        levelCompleted = true;
        RefreshPrompt();

        if (levelCompleteUI != null)
            levelCompleteUI.ShowLevelComplete();
    }

    private void UpdateDoorColor()
    {
        if (unlocked)
        {
            doorRenderer.color = unlockedColor;   // stays green
            return;
        }

        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            doorRenderer.color = flashColor;
        }
        else
        {
            doorRenderer.color = lockedColor;
        }
    }

    private void UpdateIconFade()
    {
        float target = playerInRange ? 1f : 0f;
        float step = fadeDuration > 0f ? Time.deltaTime / fadeDuration : 1f;

        Color c = iconRenderer.color;
        c.a = Mathf.MoveTowards(c.a, target, step);
        iconRenderer.color = c;
    }
}