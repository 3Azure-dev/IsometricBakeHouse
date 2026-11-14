using TMPro;
using UnityEngine;

public class DayNightClockLabel : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    private DayNightClock clock;

    private void Start()
    {
        clock = DayNightClock.Instance;

        if (clock == null)
        {
            Debug.LogWarning("DayNightClockUI: no DayNightClock found in the scene.");
            return;
        }

        clock.SecondTicked += OnSecondTicked;

        Refresh(Mathf.CeilToInt(clock.TimeLeft));
    }

    // The clock outlives this scene, so we MUST unsubscribe,
    // or it would keep calling a destroyed object and throw errors.
    private void OnDestroy()
    {
        if (clock != null)
        {
            clock.SecondTicked -= OnSecondTicked;
        }
    }

    private void OnSecondTicked(int secondsLeft)
    {
        Refresh(secondsLeft);
    }

    private void Refresh(int secondsLeft)
    {
        if (label == null)
        {
            return;
        }

        string phase = clock.IsDay ? "DAY" : "NIGHT";

        label.text = "Day " + clock.DayNumber + "  |  " + phase + "  " + DayNightClock.FormatTime(secondsLeft);
    }
}