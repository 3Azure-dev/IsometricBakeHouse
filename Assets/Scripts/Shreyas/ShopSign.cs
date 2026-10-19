using System;
using UnityEngine;
using UnityEngine.UI;

public class ShopOpenCloseSign : MonoBehaviour, IInteractable
{
    [Header("Shop State")]
    public bool IsOpen { get; private set; } = false;

    [Header("Sign Images")]
    [SerializeField] private Image signImage;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private Sprite closedSprite;

    public event Action<bool> OnShopStateChanged;

    private void Awake()
    {
        IsOpen = false;
        UpdateSign();
    }

    public void Interact()
    {
        IsOpen = !IsOpen;

        UpdateSign();

        OnShopStateChanged?.Invoke(IsOpen);

        Debug.Log(IsOpen ? "Shop is now OPEN." : "Shop is now CLOSED.");
    }

    private void UpdateSign()
    {
        if (signImage != null)
        {
            signImage.sprite = IsOpen ? openSprite : closedSprite;
            signImage.enabled = true;
        }
    }
}