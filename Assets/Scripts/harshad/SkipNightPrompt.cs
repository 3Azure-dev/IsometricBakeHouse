using UnityEngine;
using UnityEngine.UI;

// The "Skip the night?" window. The Open/Close sign shows it at night.
public class SkipNightPrompt : MonoBehaviour
{
    public static SkipNightPrompt Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private bool previousCursorVisible;
    private CursorLockMode previousCursorLockState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (yesButton != null) yesButton.onClick.AddListener(OnYes);
        if (noButton != null) noButton.onClick.AddListener(Hide);

        if (panel != null) panel.SetActive(false);
    }

    private void Update()
    {
        if (panel == null || !panel.activeSelf) return;

        // Close with Esc, or if the night ended by itself while the window was open
        bool nightIsOver = DayNightClock.Instance == null || !DayNightClock.Instance.IsNight;

        if (Input.GetKeyDown(KeyCode.Escape) || nightIsOver)
        {
            Hide();
        }
    }

    public void Show()
    {
        if (panel == null)
        {
            Debug.LogError("SkipNightPrompt: assign the Panel in the Inspector.");
            return;
        }

        if (panel.activeSelf) return;

        previousCursorVisible = Cursor.visible;
        previousCursorLockState = Cursor.lockState;

        panel.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Hide()
    {
        if (panel == null || !panel.activeSelf) return;

        panel.SetActive(false);

        Cursor.visible = previousCursorVisible;
        Cursor.lockState = previousCursorLockState;
    }

    private void OnYes()
    {
        Hide();

        if (DayNightClock.Instance != null)
        {
            DayNightClock.Instance.SkipNight();
        }
    }
}