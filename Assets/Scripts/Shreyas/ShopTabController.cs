using UnityEngine;
using UnityEngine.UI;

public class ShopTabController : MonoBehaviour
{
    [Header("Tab Buttons")]
    [SerializeField] private Button shopItemsTabButton;
    [SerializeField] private Button levelUpgradeTabButton;

    [Header("Panels")]
    [SerializeField] private GameObject shopItemsPanel;
    [SerializeField] private GameObject levelUpgradePanel;

    private void Awake()
    {
        if (shopItemsTabButton != null)
            shopItemsTabButton.onClick.AddListener(ShowShopItems);

        if (levelUpgradeTabButton != null)
            levelUpgradeTabButton.onClick.AddListener(ShowLevelUpgrade);
    }

    private void OnEnable()
    {
        // Show only the two tabs when the shop opens.
        shopItemsPanel.SetActive(false);
        levelUpgradePanel.SetActive(false);
    }
    private void OnDestroy()
    {
        if (shopItemsTabButton != null)
            shopItemsTabButton.onClick.RemoveListener(ShowShopItems);

        if (levelUpgradeTabButton != null)
            levelUpgradeTabButton.onClick.RemoveListener(ShowLevelUpgrade);
    }

    public void ShowShopItems()
    {
        shopItemsPanel.SetActive(true);
        levelUpgradePanel.SetActive(false);
    }

    public void ShowLevelUpgrade()
    {
        shopItemsPanel.SetActive(false);
        levelUpgradePanel.SetActive(true);
    }
}