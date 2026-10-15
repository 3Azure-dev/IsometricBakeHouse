using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum DayPhase
{
    Day,
    Night
}

// The bakery clock: counts down the Day, then the Night, then starts the next day.
// It survives scene changes (like the Wallet), so it keeps ticking while the player hunts.
public class DayNightClock : MonoBehaviour
{
    public static DayNightClock Instance { get; private set; }

    [Header("Settings (drag SO_DayNightSettings here)")]
    [SerializeField] private DayNightSettings settings;

    [Header("Hunt levels")]
    [Tooltip("A scene whose name starts with this text is a hunt level (the GameManager uses the same rule).")]
    [SerializeField] private string huntScenePrefix = "IngredientWorld";

    // EVENTS (Observer pattern). Other scripts subscribe to these.
    public event Action<int> SecondTicked;   // seconds left in this phase, once per second
    public event Action DayEnded;            // day -> night
    public event Action NightEnded;          // night -> next day (DayNumber has already gone up)

    public DayPhase Phase { get; private set; }
    public int DayNumber { get; private set; }
    public float TimeLeft { get; private set; }

    public bool IsDay { get { return Phase == DayPhase.Day; } }
    public bool IsNight { get { return Phase == DayPhase.Night; } }

    // The night clock stops while the player hunts. The day clock never stops.
    public bool IsPausedByHunt { get { return IsNight && inHunt; } }

    private bool inHunt;
    private int lastWholeSecond;

    // --------------------------------------------------
    // SETUP
    // --------------------------------------------------

    private void Awake()
    {
        // Only one clock may exist. When the bakery scene loads again after a hunt,
        // its copy of the clock destroys itself and the original keeps going.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);   // works only on a ROOT object in the Hierarchy

        Phase = DayPhase.Day;
        DayNumber = 1;

        if (settings == null)
        {
            Debug.LogError("DayNightClock has no DayNightSettings assigned.");
            enabled = false;
            return;
        }

        TimeLeft = Mathf.Max(1f, settings.dayLengthSeconds);
        lastWholeSecond = Mathf.CeilToInt(TimeLeft);

        inHunt = IsHuntScene(SceneManager.GetActiveScene());
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            Instance = null;
        }
    }

    // --------------------------------------------------
    // UPDATE
    // --------------------------------------------------

    private void Update()
    {
        // Night + hunting = the clock waits. In the day it never waits.
        if (IsPausedByHunt)
        {
            return;
        }

        TimeLeft -= Time.deltaTime;

        // Fire the "every second" event
        int wholeSeconds = Mathf.CeilToInt(TimeLeft);

        if (TimeLeft > 0f && wholeSeconds != lastWholeSecond)
        {
            lastWholeSecond = wholeSeconds;
            SecondTicked?.Invoke(wholeSeconds);
        }

        if (TimeLeft <= 0f)
        {
            SwitchPhase();
        }
    }

    private void SwitchPhase()
    {
        if (Phase == DayPhase.Day)
        {
            Phase = DayPhase.Night;
            TimeLeft = Mathf.Max(1f, settings.nightLengthSeconds);

            Debug.Log("DayNightClock: Day " + DayNumber + " ended. Night begins.");

            DayEnded?.Invoke();
        }
        else
        {
            Phase = DayPhase.Day;
            DayNumber++;
            TimeLeft = Mathf.Max(1f, settings.dayLengthSeconds);

            Debug.Log("DayNightClock: Night ended. Day " + DayNumber + " begins.");

            NightEnded?.Invoke();
        }

        // Show the new phase's starting time right away
        lastWholeSecond = Mathf.CeilToInt(TimeLeft);
        SecondTicked?.Invoke(lastWholeSecond);
    }

    // --------------------------------------------------
    // HUNT DETECTION
    // --------------------------------------------------

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        SetInHunt(IsHuntScene(newScene));
    }

    private bool IsHuntScene(Scene scene)
    {
        return !string.IsNullOrEmpty(huntScenePrefix) &&
               scene.name != null &&
               scene.name.StartsWith(huntScenePrefix, StringComparison.Ordinal);
    }

    // The clock finds hunts by scene name by itself. Call this by hand only if a hunt
    // is ever loaded in a way that does not change the active scene (for example additively).
    public void SetInHunt(bool value)
    {
        if (inHunt == value)
        {
            return;
        }

        inHunt = value;

        if (IsNight)
        {
            Debug.Log(value
                ? "DayNightClock: hunt started. Night clock PAUSED at " + FormatTime(Mathf.CeilToInt(TimeLeft))
                : "DayNightClock: back in the bakery. Night clock RUNNING from " + FormatTime(Mathf.CeilToInt(TimeLeft)));
        }
        else
        {
            Debug.Log("DayNightClock: hunt " + (value ? "started" : "ended") + ". The day clock keeps running.");
        }
    }

    // --------------------------------------------------
    // HELPERS
    // --------------------------------------------------

    // 125 seconds -> "2:05"
    public static string FormatTime(int totalSeconds)
    {
        return (totalSeconds / 60) + ":" + (totalSeconds % 60).ToString("00");
    }

    // Right-click the component in the Inspector (in Play mode) to jump to the next phase
    [ContextMenu("Test: end this phase now")]
    private void TestEndPhase()
    {
        if (!Application.isPlaying || settings == null)
        {
            return;
        }

        SwitchPhase();
    }
}