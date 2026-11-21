using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// On a World Map card: rains that world's ingredients, then loads the world.
public class WorldShower : MonoBehaviour
{
    [SerializeField] private string sceneName = "IngredientWorld_Bread";
    [SerializeField] private Sprite[] icons;            // this world's ingredients
    [SerializeField] private RectTransform area;        // the full-screen WorldsPanel

    [Header("Shower")]
    [SerializeField] private int count = 40;
    [SerializeField] private float duration = 1.2f;
    [SerializeField] private Vector2 sizeRange = new Vector2(70f, 120f);
    [SerializeField] private Vector2 fallSpeedRange = new Vector2(900f, 1500f);
    [SerializeField] private float maxSpin = 360f;

    private static bool busy; // stops double clicks on any card

    public void Go()
    {
        if (busy || icons == null || icons.Length == 0 || area == null) return;
        StartCoroutine(Rain());
    }

    private IEnumerator Rain()
    {
        busy = true;

        float w = area.rect.width;
        float h = area.rect.height;

        RectTransform[] drops = new RectTransform[count];
        float[] speeds = new float[count];
        float[] spins = new float[count];

        for (int i = 0; i < count; i++)
        {
            GameObject go = new GameObject("Drop", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(area, false);

            Image img = go.GetComponent<Image>();
            img.sprite = icons[Random.Range(0, icons.Length)];
            img.preserveAspect = true;
            img.raycastTarget = false;

            RectTransform rt = go.GetComponent<RectTransform>();
            float size = Random.Range(sizeRange.x, sizeRange.y);
            rt.sizeDelta = new Vector2(size, size);
            // start above the top edge, spread out so they don't fall as one line
            rt.anchoredPosition = new Vector2(Random.Range(-w / 2f, w / 2f), h / 2f + Random.Range(50f, h * 0.6f));

            drops[i] = rt;
            speeds[i] = Random.Range(fallSpeedRange.x, fallSpeedRange.y);
            spins[i] = Random.Range(-maxSpin, maxSpin);
        }

        float t = 0f;
        while (t < duration)
        {
            float dt = Time.unscaledDeltaTime;
            t += dt;

            for (int i = 0; i < count; i++)
            {
                drops[i].anchoredPosition += Vector2.down * speeds[i] * dt;
                drops[i].Rotate(0f, 0f, spins[i] * dt);
            }
            yield return null;
        }

        busy = false;
        SceneManager.LoadScene(sceneName);
    }
}