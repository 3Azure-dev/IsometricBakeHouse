using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DayNightClockUI : MonoBehaviour
{
    [Header("Clock UI")]
    [SerializeField] private TMP_Text dayNumberText;
    [SerializeField] private Image dayNightIconImage;
    [SerializeField] private Sprite sunSprite;
    [SerializeField] private Sprite moonSprite;
    [SerializeField] private TMP_Text timeRemainingText;
    [SerializeField] private Image timeBarFill;

    private DayNightClock clock;

    private void OnEnable()
    {
        TryConnectToClock();
    }

    private void Update()
    {
        // Keep checking until the real timer exists.
        if (clock == null)
        {
            TryConnectToClock();
            return;
        }

        // Update the progress bar smoothly.
        UpdateTimeBar();
    }

    private void TryConnectToClock()
    {
        if (clock != null || DayNightClock.Instance == null)
            return;

        // Get Harshad's real timer.
        clock = DayNightClock.Instance;

        // Subscribe to the existing timer events.
        clock.SecondTicked += OnSecondTicked;
        clock.DayEnded += OnPhaseChanged;
        clock.NightEnded += OnPhaseChanged;

        // Immediately display the current timer state.
        RefreshClockUI();

        Debug.Log("DayNightClockUI connected to the real timer.");
    }

    private void OnDisable()
    {
        if (clock == null)
            return;

        // Unsubscribe to prevent duplicate event subscriptions.
        clock.SecondTicked -= OnSecondTicked;
        clock.DayEnded -= OnPhaseChanged;
        clock.NightEnded -= OnPhaseChanged;

        clock = null;
    }

    private void OnSecondTicked(int secondsLeft)
    {
        // The real timer provides the remaining seconds.
        UpdateTimeText(secondsLeft);
    }

    private void OnPhaseChanged()
    {
        // Refresh when day changes to night or night changes to day.
        RefreshClockUI();
    }

    private void RefreshClockUI()
    {
        if (clock == null)
            return;

        // Display the actual day number.
        if (dayNumberText != null)
        {
            dayNumberText.text = $"Day {clock.DayNumber}";
        }

        // Display the correct sun or moon icon.
        if (dayNightIconImage != null)
        {
            dayNightIconImage.sprite = clock.IsDay
                ? sunSprite
                : moonSprite;
        }

        // Display the actual remaining time.
        UpdateTimeText(Mathf.CeilToInt(clock.TimeLeft));

        UpdateTimeBar();
    }

    private void UpdateTimeText(int secondsLeft)
    {
        if (timeRemainingText == null)
            return;

        int totalSeconds = Mathf.Max(0, secondsLeft);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timeRemainingText.text = $"{minutes:00}:{seconds:00}";
    }

    private void UpdateTimeBar()
    {
        if (clock == null || timeBarFill == null)
            return;

        float duration = clock.PhaseDuration;

        timeBarFill.fillAmount = duration > 0f
            ? Mathf.Clamp01(clock.TimeLeft / duration)
            : 0f;
    }
}