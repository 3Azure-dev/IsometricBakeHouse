using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShopTabController : MonoBehaviour
{
    [Header("Tab Buttons")]
    [SerializeField] private Button shopItemsTabButton;
    [SerializeField] private Button levelUpgradeTabButton;

    [Header("Content Panels")]
    [SerializeField] private GameObject shopItemsPanel;
    [SerializeField] private GameObject levelUpgradePanel;

    [Header("Close Button")]
    [SerializeField] private Button closeLevelUpgradeButton;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.3f;

    private CanvasGroup shopItemsGroup;
    private CanvasGroup levelUpgradeGroup;

    private Coroutine fadeRoutine;

    [Header("Shop Root")]
    [SerializeField] private GameObject shopRootPanel;

    private void Awake()
    {
        shopItemsGroup = GetOrAddCanvasGroup(shopItemsPanel);
        levelUpgradeGroup = GetOrAddCanvasGroup(levelUpgradePanel);

        if (shopItemsTabButton != null)
            shopItemsTabButton.onClick.AddListener(ShowShopItems);

        if (levelUpgradeTabButton != null)
            levelUpgradeTabButton.onClick.AddListener(ShowLevelUpgrade);

        if (closeLevelUpgradeButton != null)
            closeLevelUpgradeButton.onClick.AddListener(CloseLevelUpgrade);
    }

    private void OnEnable()
    {
        HideImmediately(shopItemsPanel, shopItemsGroup);
        HideImmediately(levelUpgradePanel, levelUpgradeGroup);
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject panel)
    {
        if (panel == null)
            return null;

        CanvasGroup group = panel.GetComponent<CanvasGroup>();

        if (group == null)
            group = panel.AddComponent<CanvasGroup>();

        return group;
    }

    private void HideImmediately(GameObject panel, CanvasGroup group)
    {
        if (panel == null || group == null)
            return;

        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        panel.SetActive(false);
    }

    public void ShowShopItems()
    {
        SwitchPanel(shopItemsPanel, shopItemsGroup);
    }

    public void ShowLevelUpgrade()
    {
        SwitchPanel(levelUpgradePanel, levelUpgradeGroup);
    }

    public void CloseLevelUpgrade()
    {
        SwitchPanel(null, null);
    }

    private void SwitchPanel(GameObject targetPanel, CanvasGroup targetGroup)
    {
        // Find the currently visible panel.
        GameObject outgoingPanel = null;
        CanvasGroup outgoingGroup = null;

        if (shopItemsPanel != null && shopItemsPanel.activeSelf &&
            (levelUpgradePanel == null || !levelUpgradePanel.activeSelf ||
             shopItemsGroup.alpha >= levelUpgradeGroup.alpha))
        {
            outgoingPanel = shopItemsPanel;
            outgoingGroup = shopItemsGroup;
        }
        else if (levelUpgradePanel != null && levelUpgradePanel.activeSelf)
        {
            outgoingPanel = levelUpgradePanel;
            outgoingGroup = levelUpgradeGroup;
        }

        // Stop any previous transition.
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        // Hide panels that are not part of this transition.
        if (shopItemsPanel != outgoingPanel && shopItemsPanel != targetPanel)
            HideImmediately(shopItemsPanel, shopItemsGroup);

        if (levelUpgradePanel != outgoingPanel && levelUpgradePanel != targetPanel)
            HideImmediately(levelUpgradePanel, levelUpgradeGroup);

        // If the requested panel is already visible, keep it visible.
        if (outgoingPanel == targetPanel && targetPanel != null)
        {
            outgoingGroup.alpha = 1f;
            outgoingGroup.interactable = true;
            outgoingGroup.blocksRaycasts = true;
            return;
        }

        if (targetPanel != null && targetGroup != null)
        {
            targetPanel.SetActive(true);
            targetGroup.alpha = 0f;
            targetGroup.interactable = false;
            targetGroup.blocksRaycasts = false;
        }

        if (outgoingGroup != null)
        {
            outgoingGroup.interactable = false;
            outgoingGroup.blocksRaycasts = false;
        }

        fadeRoutine = StartCoroutine(
            FadePanels(outgoingPanel, outgoingGroup, targetPanel, targetGroup)
        );
    }

    private IEnumerator FadePanels(
        GameObject outgoingPanel,
        CanvasGroup outgoingGroup,
        GameObject targetPanel,
        CanvasGroup targetGroup)
    {
        float elapsed = 0f;

        float outgoingStartAlpha =
            outgoingGroup != null ? outgoingGroup.alpha : 0f;

        float duration = Mathf.Max(0.01f, fadeDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            if (outgoingGroup != null)
                outgoingGroup.alpha = Mathf.Lerp(outgoingStartAlpha, 0f, progress);

            if (targetGroup != null)
                targetGroup.alpha = Mathf.Lerp(0f, 1f, progress);

            yield return null;
        }

        if (outgoingPanel != null && outgoingGroup != null)
            HideImmediately(outgoingPanel, outgoingGroup);

        if (targetPanel != null && targetGroup != null)
        {
            targetGroup.alpha = 1f;
            targetGroup.interactable = true;
            targetGroup.blocksRaycasts = true;
        }

        fadeRoutine = null;
    }

    private void OnDestroy()
    {
        if (shopItemsTabButton != null)
            shopItemsTabButton.onClick.RemoveListener(ShowShopItems);

        if (levelUpgradeTabButton != null)
            levelUpgradeTabButton.onClick.RemoveListener(ShowLevelUpgrade);

        if (closeLevelUpgradeButton != null)
            closeLevelUpgradeButton.onClick.RemoveListener(CloseLevelUpgrade);
    }

    private bool wasShopOpen;

    private void Update()
    {
        if (shopRootPanel == null)
            return;

        bool isShopOpen = shopRootPanel.activeSelf;

        if (isShopOpen && !wasShopOpen)
        {
            ShowTabSelection();
        }

        wasShopOpen = isShopOpen;
    }

    public void ShowTabSelection()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        HideImmediately(shopItemsPanel, shopItemsGroup);
        HideImmediately(levelUpgradePanel, levelUpgradeGroup);
    }
}