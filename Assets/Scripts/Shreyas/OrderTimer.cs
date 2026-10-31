using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderTimer : MonoBehaviour
{
    [Header("Timer Visuals")]
    [SerializeField] private GameObject timerVisualRoot;

    [SerializeField] private Image timerRing;
    [SerializeField] private TMP_Text timerText;

    [Header("Optional Automatic Start")]
    [SerializeField] private CustomerSettings settings;
    [SerializeField] private bool startAutomatically = false;

    private float timeLimit;
    private float timeLeft;
    private bool running;

    private void Awake()
    {
        HideTimer();
    }

    private void Start()
    {
        if (!startAutomatically)
            return;

        if (settings == null)
        {
            Debug.LogError(
                name + ": OrderTimer has no CustomerSettings assigned."
            );
            return;
        }

        StartTimer(settings.orderTimer);
    }

    public void StartTimer(float seconds)
    {
        if (seconds <= 0f)
        {
            HideTimer();
            return;
        }

        timeLimit = seconds;
        timeLeft = seconds;
        running = true;

        if (timerVisualRoot != null)
            timerVisualRoot.SetActive(true);

        UpdateTimerVisuals();
    }

    public void HideTimer()
    {
        running = false;
        timeLeft = 0f;

        if (timerVisualRoot != null)
        {
            timerVisualRoot.SetActive(false);
        }
    }

    private void Update()
    {
        if (!running)
            return;

        timeLeft = Mathf.Max(
            timeLeft - Time.deltaTime,
            0f
        );

        UpdateTimerVisuals();

        if (timeLeft <= 0f)
            running = false;
    }

    private void UpdateTimerVisuals()
    {
        if (timerRing != null)
        {
            timerRing.fillAmount = timeLimit > 0f
                ? Mathf.Clamp01(timeLeft / timeLimit)
                : 0f;
        }

        if (timerText != null)
        {
            timerText.text =
                Mathf.CeilToInt(timeLeft).ToString();
        }
    }
}