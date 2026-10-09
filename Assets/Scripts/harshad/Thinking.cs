
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

    public void Show(string order)
    {
        if (orderText != null)
            orderText.text = order;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
