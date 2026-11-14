using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One line in the shop list: icon, name, price + coin/diamond icon, status, Buy button.
// It sets up its own layout in code, so no Layout Element values are needed in the prefab.
public class ShopItemRow : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Image currencyIcon;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button buyButton;
    private Color normalPriceColor = Color.white;

    private void Awake()
    {
        if (priceText != null) normalPriceColor = priceText.color;

        BuildLayout();
    }

    private void BuildLayout()
    {
        // Row height
        LayoutElement rowSize = GetOrAdd<LayoutElement>(gameObject);
        rowSize.minHeight = 80f;
        rowSize.preferredHeight = 80f;

        // Left to right
        HorizontalLayoutGroup group = GetOrAdd<HorizontalLayoutGroup>(gameObject);
        group.spacing = 10f;
        group.padding = new RectOffset(10, 10, 5, 5);
        group.childAlignment = TextAnchor.MiddleLeft;
        group.childControlWidth = true;
        group.childControlHeight = true;
        group.childForceExpandWidth = false;
        group.childForceExpandHeight = true;

        // Fixed widths, and the name takes all the space that is left
        SetWidth(iconImage, 50f, 0f);
        SetWidth(nameText, 0f, 1f);
        SetWidth(priceText, 50f, 0f);
        SetWidth(currencyIcon, 30f, 0f);
        SetWidth(statusText, 150f, 0f);
        SetWidth(buyButton, 90f, 0f);

        FitText(nameText, 14f, 24f);
        FitText(priceText, 14f, 24f);
        FitText(statusText, 12f, 18f);
    }

    private static void SetWidth(Component part, float preferred, float flexible)
    {
        if (part == null) return;

        LayoutElement element = GetOrAdd<LayoutElement>(part.gameObject);
        element.preferredWidth = preferred;
        element.flexibleWidth = flexible;
        element.minWidth = 0f;
    }

    private static void FitText(TMP_Text text, float min, float max)
    {
        if (text == null) return;

        text.enableAutoSizing = true;
        text.fontSizeMin = min;
        text.fontSizeMax = max;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.textWrappingMode = TextWrappingModes.NoWrap;
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component
    {
        T found = target.GetComponent<T>();
        return found != null ? found : target.AddComponent<T>();
    }

    public void Setup(string title, Sprite icon, int price, Sprite currencySprite, Action onBuy)
    {
        if (nameText != null) nameText.text = title;
        if (priceText != null) priceText.text = price.ToString();

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        if (currencyIcon != null)
        {
            currencyIcon.sprite = currencySprite;
            currencyIcon.enabled = currencySprite != null;
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => onBuy());
        }
    }

    // canBuy false = the button is greyed out
    // canBuy false = the button is greyed out. priceRed = the player cannot afford it
    public void SetState(string status, bool canBuy, bool priceRed)
    {
        if (statusText != null) statusText.text = status;
        if (buyButton != null) buyButton.interactable = canBuy;
        if (priceText != null) priceText.color = priceRed ? Color.red : normalPriceColor;
    }
}