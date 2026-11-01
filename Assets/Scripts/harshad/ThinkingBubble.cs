using TMPro;
using UnityEngine;

public class ThinkingBubble : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private TMP_Text orderText;

    [Header("Readability")]
    [Tooltip("Font size for all bubble text. Set this in the Inspector!")]
    [SerializeField] private int fontSize = 40;

    private Color normalColor;
    private bool normalColorSaved;

    private void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - targetCamera.transform.position);
        }
    }

    private void SaveNormalColor()
    {
        if (!normalColorSaved && orderText != null)
        {
            normalColor = orderText.color;
            normalColorSaved = true;
        }
    }

    // 1. Shows "..." while sitting down
    // 1. Shows "..." while sitting down
    public void ShowThinking()
    {
        SaveNormalColor();
        if (orderText != null)
        {
            orderText.text = "...";
            // DELETED: orderText.fontSize = 60; 
            orderText.color = normalColor;
        }
        gameObject.SetActive(true);
    }

    // 2. Shows the food name/icon when waiting
    public void ShowOrder(UnityEngine.Sprite foodIcon)
    {
        SaveNormalColor();
        if (orderText != null)
        {
            orderText.text = "FOOD";
            // DELETED: orderText.fontSize = 50; 
            orderText.color = normalColor;
        }
        gameObject.SetActive(true);
    }

    // 3. Shows colored text for reactions
    public void ShowReaction(string text, Color color)
    {
        SaveNormalColor();
        if (orderText != null)
        {
            string displayText = text;
            if (text.Contains("Satisf")) displayText = "Satisfied!";
            else if (text.Contains("Disapp")) displayText = "Disappointed";
            else if (text.Contains("FURIOUS") || text.Contains("Furi")) displayText = "FURIOUS!";
            else if (text.Contains("Walked")) displayText = "Walked out";

            orderText.text = displayText;
            // DELETED: orderText.fontSize = 40; 
            orderText.color = color;
        }
        gameObject.SetActive(true);
    }

    // Legacy fallback
    public void Show(string order)
    {
        SaveNormalColor();
        if (orderText != null)
        {
            orderText.text = order;
            // DELETED: orderText.fontSize = 40;
            orderText.color = normalColor;
        }
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}