using UnityEngine;
using UnityEngine.InputSystem;

// Exit door of a hunt level (same setup in Bread, Cupcake, Pizza).
//   Door sprite  : always visible. Brown -> flashes red on E while locked -> green once unlocked.
//   Status icon  : padlock / open-door icon on the LEFT side of the door, only while the player is within range.
//   E prompt     : only while the player is within range AND the door is unlocked. Gently pulses.
[RequireComponent(typeof(SpriteRenderer))]
public class ExitDoor1 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject ePrompt;
    [SerializeField] private LevelCompleteUI levelCompleteUI;

    [Header("Door colours (tint on the whitebox door sprite)")]
    [SerializeField] private Color lockedColor = new Color(0.55f, 0.35f, 0.15f); // brown
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private Color unlockedColor = Color.green;
    [SerializeField] private float flashDuration = 0.3f;

    [Header("Range")]
    [Tooltip("How close the player must be (world units, measured from the door's edge to the player) " +
             "for the icon / E prompt to appear and for E to work.")]
    [SerializeField] private float interactRange = 3.5f;

    [Header("Status icon (left side of the door)")]
    [SerializeField] private Sprite lockedIconSprite;   // padlock
    [SerializeField] private Sprite openIconSprite;     // open door
    [Tooltip("Uniform world size of the icon.")]
    [SerializeField] private float iconScale = 0.6f;
    [Tooltip("World units to the LEFT of the door's left edge. 0 = icon centre on the edge, negative = overlaps the door.")]
    [SerializeField] private float iconGapLeftOfDoor = 0.6f;
    [Tooltip("Height of the icon relative to the door's centre. 0 = middle of the door.")]
    [SerializeField] private float iconHeightOnDoor = 0.4f;
    [SerializeField] private float fadeDuration = 0.25f;

    [Header("E prompt")]
    [Tooltip("Multiplies the E canvas's current size. 0.5 = half size.")]
    [SerializeField] private float promptScale = 0.5f;
    [Tooltip("World units between the door's top edge and the BOTTOM of the E prompt.")]
    [SerializeField] private float promptGapAboveDoor = 0.2f;

    [Header("E prompt pulse (subtle)")]
    [Tooltip("0.08 = grows/shrinks by 8%.")]
    [SerializeField] private float pulseAmount = 0.08f;
    [Tooltip("Pulse speed. 3 is a calm, slow breathing.")]
    [SerializeField] private float pulseSpeed = 3f;

    private SpriteRenderer doorRenderer;
    private SpriteRenderer iconRenderer;
    private Transform player;
    private Vector3 promptBaseScale = Vector3.one;

    private bool playerInRange;
    private bool levelCompleted;
    private bool unlocked;
    private float flashTimer;

    private void Awake()
    {
        // The door itself: keep whatever whitebox sprite is assigned in the Inspector.
        doorRenderer = GetComponent<SpriteRenderer>();
        doorRenderer.color = lockedColor;

        // Status icon: NOT a child of the door, so the door's stretched scale doesn't distort it.
        GameObject icon = new GameObject("DoorStatusIcon");
        icon.transform.SetParent(transform.parent, true);
        icon.transform.localScale = Vector3.one * iconScale;

        iconRenderer = icon.AddComponent<SpriteRenderer>();
        iconRenderer.sprite = lockedIconSprite;
        iconRenderer.sortingLayerID = doorRenderer.sortingLayerID;
        iconRenderer.sortingOrder = doorRenderer.sortingOrder + 1;   // in front of the door
        iconRenderer.color = new Color(1f, 1f, 1f, 0f);
    }

    private void OnEnable() => GrandfatherDunk.HuntFinished += Unlock;
    private void OnDisable() => GrandfatherDunk.HuntFinished -= Unlock;

    private void Start()
    {
        FindPlayer();

        Bounds b = doorRenderer.bounds;

        // Icon on the left side of the door.
        iconRenderer.transform.position = new Vector3(
            b.min.x - iconGapLeftOfDoor,
            b.center.y + iconHeightOnDoor,
            transform.position.z);

        // E prompt: smaller, bottom edge just above the door.
        if (ePrompt != null)
        {
            ePrompt.transform.SetParent(transform.parent, true);   // detach from the stretched door
            ePrompt.transform.localScale *= promptScale;           // make it smaller

            // Put the prompt directly above the door first...
            ePrompt.transform.position = new Vector3(b.center.x, b.max.y, transform.position.z);

            // ...then lift it so its BOTTOM edge is promptGapAboveDoor above the door's top edge.
            RectTransform rt = ePrompt.transform as RectTransform;
            if (rt != null)
            {
                float bottomY = rt.TransformPoint(new Vector3(0f, rt.rect.yMin, 0f)).y;
                float lift = (b.max.y + promptGapAboveDoor) - bottomY;
                ePrompt.transform.position += Vector3.up * lift;
            }

            promptBaseScale = ePrompt.transform.localScale;        // pulse is based on the new, smaller size
            ePrompt.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (iconRenderer != null)
            Destroy(iconRenderer.gameObject);
    }

    private void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
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
    }

    [ContextMenu("Test: unlock door")]
    private void TestUnlock() => Unlock();

    private void Update()
    {
        UpdateRange();
        UpdatePrompt();
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

        if (levelCompleteUI != null)
            levelCompleteUI.ShowLevelComplete();
    }

    // Distance from the player to the door's edge (not its centre), so wide / tall doors behave the same.
    private void UpdateRange()
    {
        if (player == null)
        {
            FindPlayer();
            playerInRange = false;
            return;
        }

        Vector2 closest = doorRenderer.bounds.ClosestPoint(player.position);
        float dist = Vector2.Distance(closest, player.position);
        playerInRange = dist <= interactRange;
    }

    private void UpdatePrompt()
    {
        if (ePrompt == null) return;

        bool show = playerInRange && unlocked && !levelCompleted;
        if (ePrompt.activeSelf != show)
            ePrompt.SetActive(show);

        if (show)
        {
            // Subtle magnify / de-magnify.
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            ePrompt.transform.localScale = promptBaseScale * pulse;
        }
        else
        {
            ePrompt.transform.localScale = promptBaseScale;
        }
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