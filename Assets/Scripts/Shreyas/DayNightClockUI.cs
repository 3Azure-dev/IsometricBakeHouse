using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DayNightClockUI : MonoBehaviour
{
    [Header("Clock UI")]
    [SerializeField] private TMP_Text dayNumberText;
    [SerializeField] private TMP_Text dayNightIconText;
    [SerializeField] private TMP_Text timeRemainingText;
    [SerializeField] private Image timeBarFill;

    [Header("Fake Day/Night Timer")]
    [SerializeField] private float phaseDurationSeconds = 300f;

    private float remainingSeconds;
    private int currentDay = 1;
    private bool isDaytime = true;

    private void Start()
    {
        // Begin with Day 1.
        currentDay = 1;
        isDaytime = true;

        // Start with five minutes remaining.
        remainingSeconds = phaseDurationSeconds;

        UpdateClockUI();
    }

    private void Update()
    {
        // Reduce the remaining time.
        remainingSeconds -= Time.deltaTime;

        if (remainingSeconds <= 0f)
        {
            SwitchDayNightPhase();
        }

        UpdateClockUI();
    }

    private void SwitchDayNightPhase()
    {
        if (isDaytime)
        {
            // Day ends and night begins.
            isDaytime = false;

            Debug.Log("Night has started.");
        }
        else
        {
            // Night ends and the next day begins.
            isDaytime = true;
            currentDay++;

            Debug.Log("Day " + currentDay + " has started.");
        }

        // Reset the countdown for the new phase.
        remainingSeconds = Mathf.Max(1f, phaseDurationSeconds);
    }

    private void UpdateClockUI()
    {
        // Update the day number.
        if (dayNumberText != null)
        {
            dayNumberText.text = "Day " + currentDay;
        }

        // Swap between the sun and moon.
        if (dayNightIconText != null)
        {
            dayNightIconText.text = isDaytime ? "☀" : "☾";
        }

        // Update the countdown.
        if (timeRemainingText != null)
        {
            int totalSeconds = Mathf.CeilToInt(
                Mathf.Max(0f, remainingSeconds)
            );

            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            timeRemainingText.text =
                $"{minutes:00}:{seconds:00}";
        }

        // Update the horizontal progress bar.
        if (timeBarFill != null)
        {
            float duration = Mathf.Max(1f, phaseDurationSeconds);

            timeBarFill.fillAmount = Mathf.Clamp01(
                remainingSeconds / duration
            );
        }
    }

    private void OnValidate()
    {
        phaseDurationSeconds = Mathf.Max(
            1f,
            phaseDurationSeconds
        );
    }
}