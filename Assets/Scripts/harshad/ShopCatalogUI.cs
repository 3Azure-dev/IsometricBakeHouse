using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Shop screen part 2: Blueprints (diamonds), Materials (coins), Expansion plots (diamonds),
// Pizza items (coins, locked until level 3).
// Blueprints and materials go to the delivery list and arrive the NEXT day as a box.
public class ShopCatalogUI : MonoBehaviour
{
    private enum Tab { Blueprints, Materials, Plots, Pizza }

    private class Entry
    {
        public ShopItemRow row;
        public Func<string> status;
        public Func<bool> canBuy;
    }

    [Header("Data (drag the assets in)")]
    [Tooltip("All furniture assets, including the new Counter")]
    [SerializeField] private FurnitureData[] furniture;
    [Tooltip("Diamond price of plot 1, 2, 3 (in order)")]
    [SerializeField] private int[] plotDiamondCosts = { 200, 250, 300 };
    [SerializeField] private RecipeData pizzaRecipe;      // SO_Recipe_Pizza (price = Blueprint Cost 200)
    [SerializeField] private EquipmentData pizzaStone;    // SO_Equipment_PizzaStone (price = Cost 500)

    [Header("Tab buttons")]
    [SerializeField] private Button blueprintsTabButton;
    [SerializeField] private Button materialsTabButton;
    [SerializeField] private Button plotsTabButton;
    [SerializeField] private Button pizzaTabButton;

    [Header("List")]
    [SerializeField] private ShopItemRow rowPrefab;
    [SerializeField] private Transform rowContainer;

    [Header("Icons")]
    [SerializeField] private Sprite coinIcon;
    [SerializeField] private Sprite diamondIcon;

    [Header("Optional message line")]
    [SerializeField] private TMP_Text messageText;

    private readonly List<Entry> entries = new List<Entry>();
    private Tab currentTab = Tab.Blueprints;

    private Wallet wallet;
    private PlayerLevel level;
    private BakeryProgress progress;
    private DeliverySystem delivery;

    private void Awake()
    {
        BuildListLayout();

        Hook(blueprintsTabButton, Tab.Blueprints);
        Hook(materialsTabButton, Tab.Materials);
        Hook(plotsTabButton, Tab.Plots);
        Hook(pizzaTabButton, Tab.Pizza);
    }

    private void Hook(Button button, Tab tab)
    {
        if (button != null) button.onClick.AddListener(() => ShowTab(tab));
    }

    // Makes the list fit the panel without any Inspector layout values
    private void BuildListLayout()
    {
        if (rowContainer == null) return;

        // Content: full width, grows downward
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

        // The scroll view: vertical only, solid background, drawn on top of the two selection buttons
        ScrollRect scroll = rowContainer.GetComponentInParent<ScrollRect>();

        if (scroll != null)
        {
            scroll.horizontal = false;
            scroll.vertical = true;

            Image background = scroll.GetComponent<Image>();

            if (background != null)
            {
                Color color = background.color;
                color.a = 1f;
                background.color = color;
            }

            scroll.transform.SetAsLastSibling();
        }

        // The tab buttons sit above the list, also on top
        if (blueprintsTabButton != null) blueprintsTabButton.transform.SetAsLastSibling();
        if (materialsTabButton != null) materialsTabButton.transform.SetAsLastSibling();
        if (plotsTabButton != null) plotsTabButton.transform.SetAsLastSibling();
        if (pizzaTabButton != null) pizzaTabButton.transform.SetAsLastSibling();
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

        ShowTab(currentTab);
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

    // ---------------- TABS ----------------

    private void ShowTab(Tab tab)
    {
        currentTab = tab;

        foreach (Transform child in rowContainer)
        {
            Destroy(child.gameObject);
        }

        entries.Clear();

        switch (tab)
        {
            case Tab.Blueprints: BuildBlueprints(); break;
            case Tab.Materials: BuildMaterials(); break;
            case Tab.Plots: BuildPlots(); break;
            case Tab.Pizza: BuildPizza(); break;
        }

        Refresh();
    }

    private void Add(string title, Sprite icon, int price, bool diamonds,
                     Action onBuy, Func<string> status, Func<bool> canBuy)
    {
        ShopItemRow row = Instantiate(rowPrefab, rowContainer);
        row.gameObject.SetActive(true);
        row.Setup(title, icon, price, diamonds ? diamondIcon : coinIcon, onBuy);

        entries.Add(new Entry { row = row, status = status, canBuy = canBuy });
    }

    private void Refresh()
    {
        foreach (Entry entry in entries)
        {
            if (entry.row == null) continue;

            entry.row.SetState(entry.status(), entry.canBuy());
        }
    }

    private void Say(string message)
    {
        Debug.Log("Shop: " + message);
        if (messageText != null) messageText.text = message;
    }

    // ---------------- 1. BLUEPRINTS (diamonds, not locked by level) ----------------

    private void BuildBlueprints()
    {
        foreach (FurnitureData f in furniture)
        {
            if (f == null) continue;

            FurnitureData item = f;

            Add(item.furnitureName + " Blueprint", item.icon, item.blueprintDiamonds, true,
                () => BuyBlueprint(item),
                () => BlueprintStatus(item),
                () => !HasOrOrdered(DeliveryItemType.Blueprint, item.itemId) && CanAffordDiamonds(item.blueprintDiamonds));
        }
    }

    private string BlueprintStatus(FurnitureData f)
    {
        if (progress != null && progress.HasBlueprint(f.itemId)) return "OWNED";
        if (IsOrdered(DeliveryItemType.Blueprint, f.itemId)) return "ARRIVES TOMORROW";
        if (!CanAffordDiamonds(f.blueprintDiamonds)) return "NOT ENOUGH DIAMONDS";
        return "BUY";
    }

    private void BuyBlueprint(FurnitureData f)
    {
        if (HasOrOrdered(DeliveryItemType.Blueprint, f.itemId)) return;

        if (string.IsNullOrEmpty(f.itemId))
        {
            Say(f.furnitureName + " has no Item Id. Fill it in on the asset.");
            return;
        }

        if (wallet == null || delivery == null || !wallet.SpendDiamonds(f.blueprintDiamonds))
        {
            Say("Not enough diamonds.");
            return;
        }

        delivery.Order(DeliveryItemType.Blueprint, f.itemId);
        Say(f.furnitureName + " blueprint bought. It arrives tomorrow.");
        Refresh();
    }

    // ---------------- 2. MATERIALS (coins) ----------------

    private void BuildMaterials()
    {
        foreach (FurnitureData f in furniture)
        {
            if (f == null) continue;

            FurnitureData item = f;

            Add(item.furnitureName + " Materials", item.icon, item.cost, false,
                () => BuyMaterials(item),
                () => CanAffordCoins(item.cost) ? "BUY" : "NOT ENOUGH COINS",
                () => CanAffordCoins(item.cost));
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

    // ---------------- 3. EXPANSION PLOTS (diamonds, in order) ----------------

    private void BuildPlots()
    {
        for (int i = 0; i < plotDiamondCosts.Length; i++)
        {
            int plot = i + 1;
            int price = plotDiamondCosts[i];

            Add("Expansion Plot " + plot, null, price, true,
                () => BuyPlot(plot, price),
                () => PlotStatus(plot, price),
                () => PlotAvailable(plot) && CanAffordDiamonds(price));
        }
    }

    private bool PlotAvailable(int plot)
    {
        if (progress == null || progress.HasPlot(plot)) return false;
        return plot == 1 || progress.HasPlot(plot - 1);   // in order
    }

    private string PlotStatus(int plot, int price)
    {
        if (progress != null && progress.HasPlot(plot)) return "OWNED";
        if (!PlotAvailable(plot)) return "BUY PLOT " + (plot - 1) + " FIRST";
        if (!CanAffordDiamonds(price)) return "NOT ENOUGH DIAMONDS";
        return "BUY";
    }

    private void BuyPlot(int plot, int price)
    {
        if (!PlotAvailable(plot)) return;

        if (wallet == null || !wallet.SpendDiamonds(price))
        {
            Say("Not enough diamonds.");
            return;
        }

        progress.AddPlot(plot);
        Say("Expansion plot " + plot + " bought.");
        Refresh();
    }

    // ---------------- 4. PIZZA ITEMS (coins, locked until level 3) ----------------

    private void BuildPizza()
    {
        if (pizzaRecipe != null)
        {
            Add("Pizza Recipe", pizzaRecipe.icon, pizzaRecipe.blueprintCost, false,
                BuyPizzaRecipe, PizzaRecipeStatus, CanBuyPizzaRecipe);
        }

        if (pizzaStone != null)
        {
            Add("Pizza Stone", pizzaStone.icon, pizzaStone.cost, false,
                BuyPizzaStone, PizzaStoneStatus, CanBuyPizzaStone);
        }
    }

    private bool PizzaUnlocked => level == null || level.IsRecipeAvailable(FoodType.Pizza);
    private bool HasPizzaRecipe => progress != null && progress.IsRecipeUnlocked(FoodType.Pizza);
    private bool HasPizzaStone => progress != null && progress.HasEquipment(EquipmentType.PizzaStone);

    private bool CanBuyPizzaRecipe() => PizzaUnlocked && !HasPizzaRecipe && CanAffordCoins(pizzaRecipe.blueprintCost);
    private bool CanBuyPizzaStone() => PizzaUnlocked && !HasPizzaStone && CanAffordCoins(pizzaStone.cost);

    private string PizzaRecipeStatus()
    {
        if (HasPizzaRecipe) return "OWNED";
        if (!PizzaUnlocked) return "LOCKED - LEVEL " + (level != null ? level.GetRequiredLevel(FoodType.Pizza) : 3);
        return CanAffordCoins(pizzaRecipe.blueprintCost) ? "BUY" : "NOT ENOUGH COINS";
    }

    private string PizzaStoneStatus()
    {
        if (HasPizzaStone) return "OWNED";
        if (!PizzaUnlocked) return "LOCKED - LEVEL " + (level != null ? level.GetRequiredLevel(FoodType.Pizza) : 3);
        return CanAffordCoins(pizzaStone.cost) ? "BUY" : "NOT ENOUGH COINS";
    }

    private void BuyPizzaRecipe()
    {
        if (!CanBuyPizzaRecipe()) return;
        if (!wallet.SpendCoins(pizzaRecipe.blueprintCost)) return;

        progress.UnlockRecipe(FoodType.Pizza);
        Say("Pizza recipe bought.");
        Refresh();
    }

    private void BuyPizzaStone()
    {
        if (!CanBuyPizzaStone()) return;
        if (!wallet.SpendCoins(pizzaStone.cost)) return;

        progress.AddEquipment(EquipmentType.PizzaStone);
        Say("Pizza Stone bought.");
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

    private bool HasOrOrdered(DeliveryItemType type, string id)
    {
        if (type == DeliveryItemType.Blueprint && progress != null && progress.HasBlueprint(id))
            return true;

        return IsOrdered(type, id);
    }
}