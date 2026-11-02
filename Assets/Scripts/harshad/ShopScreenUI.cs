using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// The whole Computer Shop screen (SCRUM-380, layout by Pravin, Nov 2):
//   Left: Shop Items | Equipment | Level upgrade
//   Right: tabs of the open section, then the item list.
//
//   Shop Items    -> Blueprint (diamonds), Material (coins), Recipe (coins, level locked)
//   Equipment     -> one list: Oven, Pizza Stone (coins, level locked)
//   Level upgrade -> Levels (coins), Plots (empty for now, SCRUM-376)
//
// Blueprints and materials go to the delivery list and arrive the next day as a box.
public class ShopScreenUI : MonoBehaviour
{
    private enum Section { ShopItems, Equipment, LevelUpgrade }

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
    [Tooltip("The 5 furniture assets, including the Counter")]
    [SerializeField] private FurnitureData[] furniture;
    [Tooltip("Recipe tab: SO_Recipe_Cupcake, SO_Recipe_Pizza")]
    [SerializeField] private RecipeData[] recipes;
    [Tooltip("Equipment list: Oven and Pizza Stone, with their type")]
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

    [Header("Purchase events (copy the old BakeryShopUI events here)")]
    [SerializeField] private UnityEvent onOvenPurchased;
    [SerializeField] private UnityEvent onCupcakeRecipePurchased;

    private static readonly string[][] TabNames =
    {
        new[] { "Blueprint", "Material", "Recipe" },   // Shop Items
        new string[0],                                  // Equipment: one list, no tabs
        new[] { "Levels", "Plots" }                     // Level upgrade
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

    // Makes the list fit the panel without any Inspector layout values
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

        list.spacing = 10f;
        list.padding = new RectOffset(10, 10, 10, 10);
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

        if (wallet != null)
        {
            wallet.CoinsChanged += OnNumberChanged;
            wallet.DiamondsChanged += OnNumberChanged;
        }

        if (level != null) level.LevelChanged += OnNumberChanged;
        if (progress != null) progress.Changed += OnProgressChanged;
        if (delivery != null) delivery.ItemOrdered += OnItemOrdered;

        // Every time the shop opens it starts on Shop Items > Blueprint
        ShowSection(Section.ShopItems);
    }

    private void OnDisable()
    {
        if (wallet != null)
        {
            wallet.CoinsChanged -= OnNumberChanged;
            wallet.DiamondsChanged -= OnNumberChanged;
        }

        if (level != null) level.LevelChanged -= OnNumberChanged;
        if (progress != null) progress.Changed -= OnProgressChanged;
        if (delivery != null) delivery.ItemOrdered -= OnItemOrdered;

        wallet = null; level = null; progress = null; delivery = null;
    }

    private void OnNumberChanged(int value) { Refresh(); }
    private void OnProgressChanged(ProgressChange change) { Refresh(); }
    private void OnItemOrdered(DeliveryItem item) { Refresh(); }

    // ---------------- SECTIONS AND TABS ----------------

    private void ShowSection(Section section)
    {
        currentSection = section;
        currentTab = 0;

        Paint(shopItemsButton, section == Section.ShopItems);
        Paint(equipmentButton, section == Section.Equipment);
        Paint(levelUpgradeButton, section == Section.LevelUpgrade);

        // Show only the tab buttons this section needs. One list (Equipment) has no tabs.
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

        for (int i = 0; i < tabButtons.Length; i++)
        {
            Paint(tabButtons[i], i == index);
        }

        Rebuild();
    }

    private void Paint(Button button, bool selected)
    {
        if (button == null || button.targetGraphic == null) return;

        button.targetGraphic.color = selected ? selectedColor : normalColor;
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
                else Say("Expansion plots are coming soon.");   // SCRUM-376 fills this tab
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

    // One place for every rule: owned, locked by level, waiting, can afford
    private ItemState Evaluate(bool owned, int lockedLevel, int price, bool diamonds, string waitText = null)
    {
        if (owned) return new ItemState("OWNED", false, false);

        if (lockedLevel > 0) return new ItemState("LOCKED - LEVEL " + lockedLevel, false, false);

        bool afford = diamonds ? CanAffordDiamonds(price) : CanAffordCoins(price);

        if (waitText != null) return new ItemState(waitText, false, !afford);

        if (afford) return new ItemState("BUY", true, false);

        return new ItemState(diamonds ? "NEED DIAMONDS" : "NEED COINS", false, true);
    }

    // ---------------- SHOP ITEMS: BLUEPRINT (diamonds, not locked by level) ----------------

    private void BuildBlueprints()
    {
        foreach (FurnitureData f in furniture)
        {
            if (f == null) continue;

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
        if (string.IsNullOrEmpty(f.itemId))
        {
            Say(f.furnitureName + " has no Item Id. Fill it in on the asset.");
            return;
        }

        if (progress != null && progress.HasBlueprint(f.itemId)) return;
        if (IsOrdered(DeliveryItemType.Blueprint, f.itemId)) return;

        if (wallet == null || delivery == null || !wallet.SpendDiamonds(f.blueprintDiamonds))
        {
            Say("Not enough diamonds.");
            return;
        }

        delivery.Order(DeliveryItemType.Blueprint, f.itemId);
        Say(f.furnitureName + " blueprint bought. It arrives tomorrow.");
        Refresh();
    }

    // ---------------- SHOP ITEMS: MATERIAL (coins) ----------------

    private void BuildMaterials()
    {
        foreach (FurnitureData f in furniture)
        {
            if (f == null) continue;

            FurnitureData item = f;

            Add(item.furnitureName, item.icon, item.cost, false,
                () => BuyMaterials(item),
                () => Evaluate(false, 0, item.cost, false));
        }
    }

    private void BuyMaterials(FurnitureData f)
    {
        if (wallet == null || delivery == null || !wallet.SpendCoins(f.cost))
        {
            Say("Not enough coins.");
            return;
        }

        delivery.Order(DeliveryItemType.Material, f.materialId, f.materialBoxes);
        Say(f.furnitureName + " materials bought. They arrive tomorrow.");
        Refresh();
    }

    // ---------------- SHOP ITEMS: RECIPE (coins, locked by level) ----------------

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

    // 0 = not locked
    private int RecipeLockedLevel(RecipeData recipe)
    {
        if (level == null || level.IsRecipeAvailable(recipe.foodType)) return 0;

        return level.GetRequiredLevel(recipe.foodType);
    }

    private void BuyRecipe(RecipeData recipe)
    {
        if (RecipeLockedLevel(recipe) > 0) return;
        if (progress == null || progress.IsRecipeUnlocked(recipe.foodType)) return;

        if (wallet == null || !wallet.SpendCoins(recipe.blueprintCost))
        {
            Say("Not enough coins.");
            return;
        }

        progress.UnlockRecipe(recipe.foodType);

        if (recipe.foodType == FoodType.Cupcake)
            onCupcakeRecipePurchased?.Invoke();

        Say(recipe.recipeName + " recipe bought.");
        Refresh();
    }

    // ---------------- EQUIPMENT (one list, coins, locked by level) ----------------

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

    private void BuyEquipment(EquipmentEntry entry)
    {
        if (EquipmentLockedLevel(entry) > 0) return;
        if (progress == null || progress.HasEquipment(entry.type)) return;

        if (wallet == null || !wallet.SpendCoins(entry.data.cost))
        {
            Say("Not enough coins.");
            return;
        }

        progress.AddEquipment(entry.type);

        if (entry.type == EquipmentType.Oven)
            onOvenPurchased?.Invoke();

        Say(entry.data.equipmentName + " bought.");
        Refresh();
    }

    // ---------------- LEVEL UPGRADE: LEVELS (coins, in order) ----------------

    private void BuildLevels()
    {
        if (level == null) return;

        for (int lv = 2; lv <= level.MaxLevel; lv++)
        {
            int target = lv;
            int price = level.GetLevelCost(target);

            Add("Level " + (target - 1) + " to " + target, null, Mathf.Max(0, price), false,
                () => BuyLevel(target),
                () =>
                {
                    if (level.Level >= target) return new ItemState("OWNED", false, false);

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

    // ---------------- HELPERS ----------------

    private bool CanAffordCoins(int price) => wallet != null && wallet.Coins >= price;
    private bool CanAffordDiamonds(int price) => wallet != null && wallet.CanAffordDiamonds(price);

    private bool IsOrdered(DeliveryItemType type, string id)
    {
        if (delivery == null) return false;

        foreach (DeliveryItem item in delivery.GetPending())
        {
            if (item.type == type && item.itemId == id) return true;
        }

        return false;
    }
}