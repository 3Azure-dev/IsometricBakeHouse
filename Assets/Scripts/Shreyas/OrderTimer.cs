using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderTimer : MonoBehaviour
{
    [Header("Settings (drag SO_CustomerSettings here)")]
    [SerializeField] private CustomerSettings settings;

    public Image timerRing;
    public TMP_Text timerText;

    // Read from the settings asset in Start()
    private float timeLimit;

    private float timeLeft;

    void Start()
    {
        if (settings == null)
        {
            Debug.LogError("OrderTimer has no CustomerSettings assigned.");
            return;
        }

        timeLimit = settings.orderTimer;

        timeLeft = timeLimit;

        // Start with the ring completely full
        timerRing.fillAmount = 1f;

        timerText.text = Mathf.CeilToInt(timeLeft).ToString();
    }

    void Update()
    {
        // Setup error or invalid time: do nothing
        if (settings == null || timeLimit <= 0f)
        {
            return;
        }

        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;

            // Prevent the value from going below zero
            timeLeft = Mathf.Max(timeLeft, 0);

            // Update the circular ring
            timerRing.fillAmount = timeLeft / timeLimit;

            // Update the TMP countdown
            timerText.text = Mathf.CeilToInt(timeLeft).ToString();
        }
        else
        {
            timerRing.fillAmount = 0f;
            timerText.text = "0";
        }
    }
}