using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DunkResultBadge : MonoBehaviour
{
    [Header("Drag the sprite assets here")]
    [SerializeField] private Sprite goodSprite;   // UI_Badge_Good
    [SerializeField] private Sprite missSprite;   // UI_Badge_Miss
    [SerializeField] private float showTime = 1f;

    private Image image;

    private void Awake()
    {
        GetImage().enabled = false;
    }

    // Safe even if called before Awake.
    private Image GetImage()
    {
        if (image == null)
            image = GetComponent<Image>();
        return image;
    }

    public void Show(bool good)
    {
        Image img = GetImage();
        img.sprite = good ? goodSprite : missSprite;
        img.color = Color.white;
        img.enabled = true;

        CancelInvoke(nameof(Hide));
        Invoke(nameof(Hide), showTime);
    }

    public void Hide()
    {
        CancelInvoke(nameof(Hide));
        GetImage().enabled = false;
    }
}