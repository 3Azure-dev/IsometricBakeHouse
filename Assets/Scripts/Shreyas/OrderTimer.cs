using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderTimer : MonoBehaviour
{
    public Image timerRing;
    public TMP_Text timerText;

    public float timeLimit = 3f;

    private float timeLeft;

    void Start()
    {
        timeLeft = timeLimit;

        // Start with the ring completely full
        timerRing.fillAmount = 1f;

        // Show 3
        timerText.text = Mathf.CeilToInt(timeLeft).ToString();
    }

    void Update()
    {
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