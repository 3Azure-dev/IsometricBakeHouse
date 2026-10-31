using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Put this on a world button of the World Map (the "Worlds" panel at the Deep Freezer door).
// The button is greyed out and cannot be clicked until the player reaches the level
// that unlocks that world (Cupcake = Level 2, Pizza = Level 3). The numbers come
// from the LevelData asset, so nothing is hard-coded here.
public class WorldUnlockButton : MonoBehaviour
{
    [Header("Which world is this button for?")]
    [SerializeField] private FoodType world = FoodType.Bread;

    [Header("UI (the Button on this object is used if left empty)")]
    [SerializeField] private Button button;

    [Tooltip("Optional: a padlock image that shows only while locked")]
    [SerializeField] private GameObject lockIcon;

    [Tooltip("Optional: shows 'Level 2' while locked, empty when unlocked")]
    [SerializeField] private TMP_Text lockText;

    [SerializeField, Range(0f, 1f)] private float lockedAlpha = 0.5f;

    private PlayerLevel subscribedLevel;
    private CanvasGroup group;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        group = GetComponent<CanvasGroup>();

        if (group == null)
        {
            group = gameObject.AddComponent<CanvasGroup>();
        }
    }

    // The panel is hidden until the player opens it, so OnEnable refreshes it every time it opens
    private void OnEnable()
    {
        subscribedLevel = PlayerLevel.Instance;

        if (subscribedLevel != null)
        {
            subscribedLevel.LevelChanged += OnLevelChanged;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (subscribedLevel != null)
        {
            subscribedLevel.LevelChanged -= OnLevelChanged;
            subscribedLevel = null;
        }
    }

    private void OnLevelChanged(int newLevel)
    {
        Refresh();
    }

    private void Refresh()
    {
        PlayerLevel level = PlayerLevel.Instance;

        // No PlayerLevel (should not happen) = do not lock anything
        bool unlocked = level == null || level.IsRecipeAvailable(world);

        if (button != null)
        {
            button.interactable = unlocked;
        }

        if (group != null)
        {
            group.alpha = unlocked ? 1f : lockedAlpha;
        }

        if (lockIcon != null)
        {
            lockIcon.SetActive(!unlocked);
        }

        if (lockText != null)
        {
            lockText.text = unlocked || level == null
                ? ""
                : "Level " + level.GetRequiredLevel(world);
        }
    }
}