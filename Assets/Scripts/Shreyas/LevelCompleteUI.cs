using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LevelCompleteUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text ingredientsText;
    [SerializeField] private IngredientData flourIngredient;

    [SerializeField] private float fadeDuration = 0.5f;

    private void Start()
    {
        panel.SetActive(false);
    }

    public void ShowLevelComplete()
    {
        int count = Inventory1.Instance.GetTotalIngredientCount();

        ingredientsText.text = "Ingredients Collected: " + count;

        panel.SetActive(true);

        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            canvasGroup.alpha = Mathf.Lerp(
                0f,
                1f,
                time / fadeDuration
            );

            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void BackToBakery()
    {
        SceneManager.LoadScene("Bakery");
    }
}