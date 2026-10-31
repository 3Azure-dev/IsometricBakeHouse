
using TMPro;
using UnityEngine;

public class ThinkingBubble : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private TMP_Text orderText;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(
                transform.position - targetCamera.transform.position
            );
        }
    }

    // The text colour the bubble has in the prefab (remembered the first time we change it)
    private Color normalColor;
    private bool normalColorSaved;

    private void SaveNormalColor()
    {
        if (!normalColorSaved && orderText != null)
        {
            normalColor = orderText.color;
            normalColorSaved = true;
        }
    }

    // Shows the dish name (normal colour)
    public void Show(string order)
    {
        SaveNormalColor();

        if (orderText != null)
        {
            orderText.text = order;

            if (normalColorSaved)
                orderText.color = normalColor;
        }

        gameObject.SetActive(true);
    }

    // Shows how the order ended, e.g. "Satisfied!" in green (SCRUM-274)
    public void ShowReaction(string text, Color color)
    {
        SaveNormalColor();

        if (orderText != null)
        {
            orderText.text = text;
            orderText.color = color;
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
