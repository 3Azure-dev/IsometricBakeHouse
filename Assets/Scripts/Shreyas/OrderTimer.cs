using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderTimer : MonoBehaviour
{
    [Header("Settings (drag SO_CustomerSettings here)")]
    [SerializeField] private CustomerSettings settings;

    [Header("Start")]
    [Tooltip("Ticked = the timer runs as soon as the scene starts (old behaviour). " +
             "Unticked = it waits until StartTimer() is called, e.g. when an order is accepted.")]
    [SerializeField] private bool startAutomatically = true;

    public Image timerRing;
    public TMP_Text timerText;

    private float timeLimit;
    private float timeLeft;
    private bool running;

    void Start()
    {
        if (!startAutomatically)
        {
            return;
        }

        if (settings == null)
        {
            Debug.LogError("OrderTimer has no CustomerSettings assigned.");
            return;
        }

        // Default: the normal order timer
        StartTimer(settings.orderTimer);
    }

    // Call this with customer.GetOrderTimerSeconds(): 180 for Normal, 240 for VIP
    public void StartTimer(float seconds)
    {
        timeLimit = seconds;
        timeLeft = seconds;
        running = seconds > 0f;

        // Start with the ring completely full
        timerRing.fillAmount = 1f;
        timerText.text = Mathf.CeilToInt(timeLeft).ToString();
    }

    void Update()
    {
        if (!running)
        {
            return;
        }

        // Count down, never below zero
        timeLeft = Mathf.Max(timeLeft - Time.deltaTime, 0f);

        timerRing.fillAmount = timeLeft / timeLimit;
        timerText.text = Mathf.CeilToInt(timeLeft).ToString();

        if (timeLeft <= 0f)
        {
            running = false;
        }
    }
}