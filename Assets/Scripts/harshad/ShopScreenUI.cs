using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShopScreenUI : MonoBehaviour
{
    private enum Section { ShopItems, Equipment, LevelUpgrade }

    // GDD: nothing unlocks after Level 3 → Level 4+ never triggers dots
    private const int MaxUnlockLevel = 3;

    private int lastViewedLevelRecipe = 1;
    private int lastViewedLevelEquipment = 1;

    [Serializable]
    public class EquipmentEntry
    {
        public EquipmentData data;
        public EquipmentType type;
    }

    private struct ItemState
    {
        public string status;
        public bool canBuy;
        public bool priceRed;

        public ItemState(string status, bool canBuy, bool priceRed)
        {
            this.status = status;
            this.canBuy = canBuy;
            this.priceRed = priceRed;
        }
    }

    private class Entry
    {
        public ShopItemRow row;
        public Func<ItemState> state;
    }

    [Header("Data (drag the assets in)")]
    [SerializeField] private FurnitureData[] furniture;
    [SerializeField] private RecipeData[] recipes;
    [SerializeField] private EquipmentEntry[] equipment;

    [Header("Left section buttons")]
    [SerializeField] private Button shopItemsButton;
    [SerializeField] private Button equipmentButton;
    [SerializeField] private Button levelUpgradeButton;

    [Header("Tab buttons on the right (3 buttons, they get their names from code)")]
    [SerializeField] private Button[] tabButtons;

    [Header("List")]
    [SerializeField] private ShopItemRow rowPrefab;
    [SerializeField] private Transform rowContainer;

    [Header("Icons")]
    [SerializeField] private Sprite coinIcon;
    [SerializeField] private Sprite diamondIcon;

    [Header("Button colours")]
    [SerializeField] private Color selectedColor = new Color(0.88f, 0.49f, 0.38f);
    [SerializeField] private Color normalColor = Color.white;

    [Header("Optional message line")]
    [SerializeField] private TMP_Text messageText;

    [Header("SCRUM-376: Plots Panel")]
    [SerializeField] private GameObject plotsPanel;

    [Header("Purchase events")]
    [SerializeField] private UnityEvent onOvenPurchased;
    [SerializeField] private UnityEvent onCupcakeRecipePurchased;

    private static readonly string[][] TabNames =
    {
        new[] { "Blueprint", "Material", "Recipe" },
        new string[0],
        new[] { "Levels", "Plots" }
    };

    private readonly List<Entry> entries = new List<Entry>();

    private Section currentSection = Section.ShopItems;
    private int currentTab;

    private Wallet wallet;
    private PlayerLevel level;
    private BakeryProgress progress;
    private DeliverySystem delivery;

    // ---------------- SETUP ----------------

    private void Awake()
    {
        BuildListLayout();

        if (shopItemsButton != null)
            shopItemsButton.onClick.AddListener(() => ShowSection(Section.ShopItems));

        if (equipmentButton != null)
            equipmentButton.onClick.AddListener(() => ShowSection(Section.Equipment));

        if (levelUpgradeButton != null)
            levelUpgradeButton.onClick.AddListener(() => ShowSection(Section.LevelUpgrade));

        if (tabButtons != null)
        {
            for (int i = 0; i < tabButtons.Length; i++)
            {
                int index = i;
                if (tabButtons[i] != null)
                    tabButtons[i].onClick.AddListener(() => ShowTab(index));
            }
        }
    }

    private void BuildListLayout()
    {
        if (rowContainer == null) return;

        RectTransform content = rowContainer as RectTransform;

        if (content != null)
        {
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, content.sizeDelta.y);
        }

        VerticalLayoutGroup list = rowContainer.GetComponent<VerticalLayoutGroup>();
        if (list == null) list = rowContainer.gameObject.AddComponent<VerticalLayoutGroup>();

        list.spacing = 16f;
        list.padding = new RectOffset(16, 16, 16, 16);
        list.childControlWidth = true;
        list.childControlHeight = true;
        list.childForceExpandWidth = true;
        list.childForceExpandHeight = false;

        ContentSizeFitter fitter = rowContainer.GetComponent<ContentSizeFitter>();
        if (fitter == null) fitter = rowContainer.gameObject.AddComponent<ContentSizeFitter>();

        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = rowContainer.GetComponentInParent<ScrollRect>();

        if (scroll != null)
        {
            scroll.horizontal = false;
            scroll.vertical = true;
        }
    }

    private void OnEnable()
    {
        wallet = Wallet.Instance;
        level = PlayerLevel.Instance;
        progress = BakeryProgress.Instance;
        delivery = DeliverySystem.Instance;

        lastViewedLevelRecipe = Mathf.Max(1, PlayerPrefs.GetInt("LastViewedLevel_Recipe", 1));
        lastViewedLevelEquipment = Mathf.Max(1, PlayerPrefs.GetInt("LastViewedLevel_Equipment", 1));

        if (wallet != null)
        {
            wallet.CoinsChanged += OnWalletChanged;
            wallet.DiamondsChanged += OnWalletChanged;
        }
        if (level != null) level.LevelChanged += OnLevelChanged;
        if (progress != null) progress.Changed += OnProgressChanged;
        if (delivery != null) delivery.ItemOrdered += OnItemOrdered;

        ShowSection(Section.ShopItems);
    }

    private void OnDisable()
    {
        if (wallet != null)
        {
            wallet.CoinsChanged -= OnWalletChanged;
            wallet.DiamondsChanged -= OnWalletChanged;
        }
        if (level != null) level.LevelChanged -= OnLevelChanged;
        if (progress != null) progress.Changed -= OnProgressChanged;
        if (delivery != null) delivery.ItemOrdered -= OnItemOrdered;

        wallet = null; level = null; progress = null; delivery = null;
    }

    private void OnWalletChanged(int v) { Refresh(); }
    private void OnLevelChanged(int v) { Refresh(); CheckForNewUnlocks(); }
    // CRITICAL FIX: purchases change progress -> re-check dots so the dot HIDES on buy
    private void OnProgressChanged(ProgressChange c) { Refresh(); CheckForNewUnlocks(); }
    private void OnItemOrdered(DeliveryItem i) { Refresh(); }

    // ---------------- SECTIONS AND TABS ----------------

    private void ShowSection(Section section)
    {
        currentSection = section;
        currentTab = 0;

        if (rowContainer != null) rowContainer.gameObject.SetActive(true);
        if (plotsPanel != null) plotsPanel.SetActive(false);

        Paint(shopItemsButton, section == Section.ShopItems);
        Paint(equipmentButton, section == Section.Equipment);
        Paint(levelUpgradeButton, section == Section.LevelUpgrade);

        string[] names = TabNames[(int)section];

        for (int i = 0; i < tabButtons.Length; i++)
        {
            if (tabButtons[i] == null) continue;

            bool used = names.Length > 1 && i < names.Length;
            tabButtons[i].gameObject.SetActive(used);

            if (used)
            {
                TMP_Text label = tabButtons[i].GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = names[i];
            }
        }

        ShowTab(0);
    }

    private void ShowTab(int index)
    {
        currentTab = index;
        for (int i = 0; i < tabButtons.Length; i++) Paint(tabButtons[i], i == index);

        if (level != null)
        {
            int lv = Mathf.Max(1, level.Level);

            if (currentSection == Section.ShopItems && index == 2)
            {
                lastViewedLevelRecipe = lv;
                PlayerPrefs.SetInt("LastViewedLevel_Recipe", lv);
                SetVerticalNewDot(shopItemsButton, false);
            }
            else if (currentSection == Section.Equipment)
            {
                lastViewedLevelEquipment = lv;
                PlayerPrefs.SetInt("LastViewedLevel_Equipment", lv);
                SetVerticalNewDot(equipmentButton, false);
            }
        }

        RefreshHorizontalDots();
        Rebuild();
    }

    private void Paint(Button button, bool selected)
    {
        if (button == null || button.targetGraphic == null) return;
        button.targetGraphic.color = selected ? selectedColor : normalColor;
    }

    // --- HELPER METHODS FOR NEW DOTS ---
    private void SetVerticalNewDot(Button button, bool show)
    {
        if (button == null) return;
        Transform dot = button.transform.Find("NewDot");
        if (dot != null) dot.gameObject.SetActive(show);
    }

    private void SetHorizontalNewDot(int tabIndex, bool show)
    {
        if (tabIndex < 0 || tabIndex >= tabButtons.Length || tabButtons[tabIndex] == null) return;
        Transform dot = tabButtons[tabIndex].transform.Find("NewDot");
        if (dot != null) dot.gameObject.SetActive(show);
    }

    private void Rebuild()
    {
        foreach (Transform child in rowContainer)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        entries.Clear();
        Say("");

        switch (currentSection)
        {
            case Section.ShopItems:
                if (currentTab == 0) BuildBlueprints();
                else if (currentTab == 1) BuildMaterials();
                else BuildRecipes();
                break;

            case Section.Equipment:
                BuildEquipment();
                break;

            case Section.LevelUpgrade:
                if (currentTab == 0) BuildLevels();
                else BuildPlots();
                break;
        }

        Refresh();
    }

    private void Add(string title, Sprite icon, int price, bool diamonds,
                     Action onBuy, Func<ItemState> state)
    {
        ShopItemRow row = Instantiate(rowPrefab, rowContainer);
        row.gameObject.SetActive(true);
        row.Setup(title, icon, price, diamonds ? diamondIcon : coinIcon, onBuy);

        entries.Add(new Entry { row = row, state = state });
    }

    private void Refresh()
    {
        foreach (Entry entry in entries)
        {
            if (entry.row == null) continue;
            ItemState state = entry.state();
            entry.row.SetState(state.status, state.canBuy, state.priceRed);
        }
    }

    private void Say(string message)
    {
        if (message.Length > 0) Debug.Log("Shop: " + message);
        if (messageText != null) messageText.text = message;
    }

    private ItemState Evaluate(bool owned, int lockedLevel, int price, bool diamonds, string waitText = null)
    {
        if (owned) return new ItemState("PURCHASED", false, false);
        if (lockedLevel > 0) return new ItemState("LOCKED - LEVEL " + lockedLevel, false, false);

        bool afford = diamonds ? CanAffordDiamonds(price) : CanAffordCoins(price);

        if (waitText != null) return new ItemState(waitText, false, !afford);
        if (afford) return new ItemState("BUY", true, false);

        return new ItemState("", false, false);
    }

    // ---------------- SHOP ITEMS: BLUEPRINT ----------------

    private void BuildBlueprints()
    {
        foreach (FurnitureData f in furniture)
        {
            if (IsHidden(f)) continue;
            FurnitureData item = f;

            Add(item.furnitureName, item.icon, item.blueprintDiamonds, true,
                () => BuyBlueprint(item),
                () =>
                {
                    bool owned = progress != null && progress.HasBlueprint(item.itemId);
                    string wait = IsOrdered(DeliveryItemType.Blueprint, item.itemId) ? "ARRIVES TOMORROW" : null;
                    return Evaluate(owned, 0, item.blueprintDiamonds, true, wait);
                });
        }
    }

    private void BuyBlueprint(FurnitureData f)
    {
        if (string.IsNullOrEmpty(f.itemId)) { Say(f.furnitureName + " has no Item Id."); return; }
        if (progress != null && progress.HasBlueprint(f.itemId)) return;
        if (IsOrdered(DeliveryItemType.Blueprint, f.itemId)) return;

        if (wallet == null || delivery == null || !wallet.SpendDiamonds(f.blueprintDiamonds)) { Say("Not enough diamonds."); return; }

        delivery.Order(DeliveryItemType.Blueprint, f.itemId);
        Say(f.furnitureName + " blueprint bought. It arrives tomorrow.");
        Refresh();
    }

    // ---------------- SHOP ITEMS: MATERIAL ----------------

    private void BuildMaterials()
    {
        foreach (FurnitureData f in furniture)
        {
            if (IsHidden(f)) continue;
            FurnitureData item = f;

            Add(item.furnitureName, item.icon, item.cost, false,
                () => BuyMaterials(item),
                () => Evaluate(false, 0, item.cost, false));
        }
    }

    private void BuyMaterials(FurnitureData f)
    {
        if (wallet == null || delivery == null || !wallet.SpendCoins(f.cost)) { Say("Not enough coins."); return; }

        delivery.Order(DeliveryItemType.Material, f.materialId, f.materialBoxes);
        Say(f.furnitureName + " materials bought. They arrive tomorrow.");
        Refresh();
    }

    // ---------------- SHOP ITEMS: RECIPE ----------------

    private void BuildRecipes()
    {
        foreach (RecipeData r in recipes)
        {
            if (r == null) continue;
            RecipeData recipe = r;

            Add(recipe.recipeName, recipe.icon, recipe.blueprintCost, false,
                () => BuyRecipe(recipe),
                () =>
                {
                    bool owned = progress != null && progress.IsRecipeUnlocked(recipe.foodType);
                    int locked = RecipeLockedLevel(recipe);
                    return Evaluate(owned, locked, recipe.blueprintCost, false);
                });
        }
    }

    private int RecipeLockedLevel(RecipeData recipe)
    {
        if (level == null || level.IsRecipeAvailable(recipe.foodType)) return 0;
        return level.GetRequiredLevel(recipe.foodType);
    }

    // 🔴 FIXED VERSION: Buying counts as "seeing" the unlock level
    private void BuyRecipe(RecipeData recipe)
    {
        if (RecipeLockedLevel(recipe) > 0) return;
        if (progress == null || progress.IsRecipeUnlocked(recipe.foodType)) return;

        if (wallet == null || !wallet.SpendCoins(recipe.blueprintCost))
        {
            Say("Not enough coins.");
            return;
        }

        int req = level != null ? level.GetRequiredLevel(recipe.foodType) : 1;
        if (req <= 0) req = level != null ? level.Level : 1;

        progress.UnlockRecipe(recipe.foodType);

        // KEY FIX: buying counts as "seeing" this unlock level,
        // so the dot can NEVER appear or stay after a purchase
        if (level != null && req > lastViewedLevelRecipe)
        {
            lastViewedLevelRecipe = req;
            PlayerPrefs.SetInt("LastViewedLevel_Recipe", lastViewedLevelRecipe);
        }

        if (recipe.foodType == FoodType.Cupcake)
            onCupcakeRecipePurchased?.Invoke();

        Say(recipe.recipeName + " recipe bought.");
        Refresh();
        CheckForNewUnlocks();
    }

    // ---------------- EQUIPMENT ----------------

    private void BuildEquipment()
    {
        foreach (EquipmentEntry e in equipment)
        {
            if (e == null || e.data == null) continue;
            EquipmentEntry entry = e;

            Add(entry.data.equipmentName, entry.data.icon, entry.data.cost, false,
                () => BuyEquipment(entry),
                () =>
                {
                    bool owned = progress != null && progress.HasEquipment(entry.type);
                    int locked = EquipmentLockedLevel(entry);
                    return Evaluate(owned, locked, entry.data.cost, false);
                });
        }
    }

    private int EquipmentLockedLevel(EquipmentEntry entry)
    {
        if (level == null || level.IsEquipmentAvailable(entry.type)) return 0;
        return Mathf.Max(2, entry.data.unlockLevel);
    }

    // 🔴 FIXED VERSION: Buying counts as "seeing" the unlock level
    private void BuyEquipment(EquipmentEntry entry)
    {
        if (EquipmentLockedLevel(entry) > 0) return;
        if (progress == null || progress.HasEquipment(entry.type)) return;

        if (wallet == null || !wallet.SpendCoins(entry.data.cost))
        {
            Say("Not enough coins.");
            return;
        }

        int req = Mathf.Max(2, entry.data.unlockLevel);

        progress.AddEquipment(entry.type);

        // KEY FIX: buying counts as "seeing" this unlock level
        if (level != null && req > lastViewedLevelEquipment)
        {
            lastViewedLevelEquipment = req;
            PlayerPrefs.SetInt("LastViewedLevel_Equipment", lastViewedLevelEquipment);
        }

        if (entry.type == EquipmentType.Oven)
            onOvenPurchased?.Invoke();

        Say(entry.data.equipmentName + " bought.");
        Refresh();
        CheckForNewUnlocks();
    }

    // ---------------- LEVEL UPGRADE: LEVELS ----------------

    private void BuildLevels()
    {
        if (level == null) return;

        for (int lv = 2; lv <= level.MaxLevel; lv++)
        {
            int target = lv;
            int price = level.GetLevelCost(target);

            Add("Level " + target, null, Mathf.Max(0, price), false,
                () => BuyLevel(target),
                () =>
                {
                    if (level.Level >= target) return new ItemState("PURCHASED", false, false);
                    bool next = level.Level + 1 == target;
                    string wait = next ? null : "BUY LEVEL " + (target - 1) + " FIRST";
                    return Evaluate(false, 0, Mathf.Max(0, price), false, wait);
                });
        }
    }

    private void BuyLevel(int target)
    {
        if (level == null || level.Level + 1 != target) return;

        if (!level.BuyNextLevel())
        {
            Say("Not enough coins.");
            return;
        }

        Say("You reached level " + level.Level + ".");
        Refresh();
    }

    // ---------------- LEVEL UPGRADE: PLOTS ----------------

    private void BuildPlots()
    {
        Add("Expansion Plot 1", null, 200, true, () => BuyPlot(1), () => Evaluate(IsPlotUnlocked(1), 0, 200, true));
        Add("Expansion Plot 2", null, 250, true, () => BuyPlot(2), () => Evaluate(IsPlotUnlocked(2), 0, 250, true));
        Add("Expansion Plot 3", null, 300, true, () => BuyPlot(3), () => Evaluate(IsPlotUnlocked(3), 0, 300, true));
    }

    private void BuyPlot(int plotNumber)
    {
        if (plotNumber > 1 && !IsPlotUnlocked(plotNumber - 1)) { Say("You must buy Plot " + (plotNumber - 1) + " first!"); return; }
        if (IsPlotUnlocked(plotNumber)) return;

        ExpansionPlotManager plotManager = FindAnyObjectByType<ExpansionPlotManager>();
        if (plotManager != null)
        {
            if (plotManager.BuyPlot(plotNumber)) Say("Expansion Plot " + plotNumber + " unlocked!");
            else Say("Not enough diamonds or plot already bought!");
        }
        else { Say("Plot Manager not found!"); }

        Refresh();
        Rebuild();
    }

    private bool IsPlotUnlocked(int plotNumber)
    {
        ExpansionPlotManager plotManager = FindAnyObjectByType<ExpansionPlotManager>();
        if (plotManager == null) return false;
        return plotManager.IsPlotUnlocked(plotNumber);
    }

    // ---------------- HELPERS ----------------

    private bool CanAffordCoins(int price) => wallet != null && wallet.Coins >= price;
    private bool CanAffordDiamonds(int price) => wallet != null && wallet.CanAffordDiamonds(price);

    private bool IsOrdered(DeliveryItemType type, string id)
    {
        if (delivery == null) return false;
        foreach (DeliveryItem item in delivery.GetPending())
            if (item.type == type && item.itemId == id) return true;
        return false;
    }

    private bool IsHidden(FurnitureData f)
    {
        return f == null || f.furnitureName.ToLower().Contains("counter");
    }

    // ---------------- NEW DOT LOGIC ----------------

    private void CheckForNewUnlocks()
    {
        if (level == null) return;

        int dotLevel = Mathf.Min(level.Level, MaxUnlockLevel);

        bool newRecipe = dotLevel > lastViewedLevelRecipe;
        bool newEquipment = dotLevel > lastViewedLevelEquipment;

        SetVerticalNewDot(shopItemsButton, newRecipe);
        SetVerticalNewDot(equipmentButton, newEquipment);

        RefreshHorizontalDots();
    }

    private void RefreshHorizontalDots()
    {
        if (currentSection == Section.ShopItems && level != null)
        {
            int dotLevel = Mathf.Min(level.Level, MaxUnlockLevel);
            bool newRecipe = dotLevel > lastViewedLevelRecipe;

            SetHorizontalNewDot(0, false);
            SetHorizontalNewDot(1, false);
            SetHorizontalNewDot(2, newRecipe);
        }
        else
        {
            SetHorizontalNewDot(0, false);
            SetHorizontalNewDot(1, false);
            SetHorizontalNewDot(2, false);
        }
    }
}