using UnityEngine;
using UnityEngine.UI;

// Slides a tiled RawImage forever (main menu wallpaper).
[RequireComponent(typeof(RawImage))]
public class UVScroller : MonoBehaviour
{
    [Tooltip("Tiles per second. Positive = moves right, negative = moves left")]
    [SerializeField] private float speedX = 0.3f;
    [SerializeField] private float speedY = 0f;

    private RawImage rawImage;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
    }

    private void Update()
    {
        Rect uv = rawImage.uvRect;

        // unscaled so it still moves if the game is paused
        uv.x -= speedX * Time.unscaledDeltaTime;
        uv.y -= speedY * Time.unscaledDeltaTime;

        // keep the numbers small, the tile repeats anyway
        uv.x = Mathf.Repeat(uv.x, 1f);
        uv.y = Mathf.Repeat(uv.y, 1f);

        rawImage.uvRect = uv;
    }
}