using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BakeryShopUI : MonoBehaviour
{
    [Header("Item Prices")]
    [SerializeField] private int ovenPrice = 400;
    [SerializeField] private int cupcakeRecipePrice = 150;

    [Header("Buy Buttons")]
    [SerializeField] private Button buyOvenButton;
    [SerializeField] private Button buyCupcakeRecipeButton;

    [Header("Level")]
    [SerializeField] private Button buyLevelButton;
    [SerializeField] private TMP_Text levelButtonText;   // shows "BUY LEVEL 2 - 1000"
    [SerializeField] private TMP_Text levelText;         // shows "Level 1"

    [Header("Button Labels")]
    [SerializeField] private TMP_Text ovenButtonText;
    [SerializeField] private TMP_Text cupcakeRecipeButtonText;

    [Header("Optional Status Text")]
    [SerializeField] private TMP_Text ovenStatusText;
    [SerializeField] private TMP_Text cupcakeRecipeStatusText;

    [Header("Purchase Events")]
    [SerializeField] private UnityEvent onOvenPurchased;
    [SerializeField] private UnityEvent onCupcakeRecipePurchased;

    // These now come from BakeryProgress, so they survive the scene reload
    private bool ovenPurchased =>
        BakeryProgress.Instance != null &&
        BakeryProgress.Instance.HasEquipment(EquipmentType.Oven);

    private bool cupcakeRecipePurchased =>
        BakeryProgress.Instance != null &&
        BakeryProgress.Instance.IsRecipeUnlocked(FoodType.Cupcake);

    private Wallet subscribedWallet;

    private PlayerLevel subscribedLevel;

    private void Awake()
    {
        if (buyOvenButton != null)
            buyOvenButton.onClick.AddListener(BuyOven);

        if (buyCupcakeRecipeButton != null)
            buyCupcakeRecipeButton.onClick.AddListener(BuyCupcakeRecipe);

        if (buyLevelButton != null)
            buyLevelButton.onClick.AddListener(BuyLevel);
    }

    private void OnEnable()
    {
        SubscribeToWallet();
        RefreshUI();
    }

    private void Start()
    {
        // Try again in case the Wallet was not available earlier. 
        SubscribeToWallet();
        RefreshUI();
    }

    private void OnDisable()
    {
        if (subscribedWallet != null)
        {
            subscribedWallet.CoinsChanged -= OnCoinsChanged;
            subscribedWallet = null;
        }

        if (subscribedLevel != null)
        {
            subscribedLevel.LevelChanged -= OnLevelChanged;
            subscribedLevel = null;
        }

    }

    private void OnDestroy()
    {
        if (buyOvenButton != null)
            buyOvenButton.onClick.RemoveListener(BuyOven);

        if (buyCupcakeRecipeButton != null)
            buyCupcakeRecipeButton.onClick.RemoveListener(BuyCupcakeRecipe);

        if (buyLevelButton != null)
            buyLevelButton.onClick.RemoveListener(BuyLevel);
    }

    private void SubscribeToWallet()
    {
        Wallet currentWallet = Wallet.Instance;

        if (currentWallet == subscribedWallet)
            return;

        if (subscribedWallet != null)
            subscribedWallet.CoinsChanged -= OnCoinsChanged;

        subscribedWallet = currentWallet;

        if (subscribedWallet != null)
            subscribedWallet.CoinsChanged += OnCoinsChanged;
    }

    private void OnCoinsChanged(int newBalance)
    {
        RefreshUI();
    }

    public void BuyOven()
    {
        if (PlayerLevel.Instance != null &&
    !PlayerLevel.Instance.IsEquipmentAvailable(EquipmentType.Oven))
        {
            Debug.Log("The Oven unlocks at level 2.");
            return;
        }

        if (ovenPurchased)
            return;

        if (!TrySpendCoins(ovenPrice))
            return;

        if (BakeryProgress.Instance != null)
            BakeryProgress.Instance.AddEquipment(EquipmentType.Oven);

        if (onOvenPurchased != null)
            onOvenPurchased.Invoke();

        RefreshUI();

        Debug.Log("Oven purchased for " + ovenPrice + " coins.");
    }

    private void SubscribeToLevel()
    {
        PlayerLevel current = PlayerLevel.Instance;

        if (current == subscribedLevel)
            return;

        if (subscribedLevel != null)
            subscribedLevel.LevelChanged -= OnLevelChanged;

        subscribedLevel = current;

        if (subscribedLevel != null)
            subscribedLevel.LevelChanged += OnLevelChanged;
    }

    private void OnLevelChanged(int newLevel)
    {
        RefreshUI();
    }

    public void BuyCupcakeRecipe()
    {

        if (PlayerLevel.Instance != null &&
    !PlayerLevel.Instance.IsRecipeAvailable(FoodType.Cupcake))
        {
            Debug.Log("The Cupcake recipe unlocks at level 2.");
            return;
        }

        if (cupcakeRecipePurchased)
            return;

        if (!TrySpendCoins(cupcakeRecipePrice))
            return;

        if (BakeryProgress.Instance != null)
            BakeryProgress.Instance.UnlockRecipe(FoodType.Cupcake);

        if (onCupcakeRecipePurchased != null)
            onCupcakeRecipePurchased.Invoke();

        RefreshUI();

        Debug.Log(
            "Cupcake recipe purchased for " +
            cupcakeRecipePrice +
            " coins."
        );
    }

    private bool TrySpendCoins(int price)
    {
        if (Wallet.Instance == null)
        {
            Debug.LogError("BakeryShopUI: Wallet was not found.");
            return false;
        }

        if (Wallet.Instance.Coins < price)
        {
            Debug.Log("Not enough coins to purchase this item.");
            RefreshUI();
            return false;
        }

        return Wallet.Instance.SpendCoins(price);
    }

    private void RefreshUI()
    {
        Wallet wallet = Wallet.Instance;

        bool canAffordOven =
            wallet != null && wallet.Coins >= ovenPrice;

        bool canAffordRecipe =
            wallet != null && wallet.Coins >= cupcakeRecipePrice;

        PlayerLevel level = PlayerLevel.Instance;

        bool ovenUnlocked =
            level == null || level.IsEquipmentAvailable(EquipmentType.Oven);

        bool recipeUnlocked =
            level == null || level.IsRecipeAvailable(FoodType.Cupcake);

        if (buyOvenButton != null)
            buyOvenButton.interactable =
                !ovenPurchased && canAffordOven && ovenUnlocked;

        if (buyCupcakeRecipeButton != null)
            buyCupcakeRecipeButton.interactable =
                !cupcakeRecipePurchased && canAffordRecipe && recipeUnlocked;

        if (ovenButtonText != null)
            ovenButtonText.text =
                ovenPurchased ? "OWNED" : (ovenUnlocked ? "BUY" : "LOCKED");

        if (cupcakeRecipeButtonText != null)
            cupcakeRecipeButtonText.text =
                cupcakeRecipePurchased ? "OWNED" : (recipeUnlocked ? "BUY" : "LOCKED");

        if (ovenStatusText != null)
            ovenStatusText.text = GetStatus(ovenPurchased, canAffordOven);

        if (cupcakeRecipeStatusText != null)
            cupcakeRecipeStatusText.text =
                GetStatus(cupcakeRecipePurchased, canAffordRecipe);

        if (level != null)
        {
            if (levelText != null)
                levelText.text = "Level " + level.Level;

            if (buyLevelButton != null)
                buyLevelButton.interactable = level.CanBuyNextLevel;

            if (levelButtonText != null)
                levelButtonText.text = level.IsMaxLevel
                    ? "MAX LEVEL"
                    : "BUY LEVEL " + (level.Level + 1) + " - " + level.NextLevelCost;
        }
    }

    private string GetStatus(bool purchased, bool canAfford)
    {
        if (purchased)
            return "OWNED";

        if (Wallet.Instance == null)
            return "WALLET UNAVAILABLE";

        if (!canAfford)
            return "NOT ENOUGH COINS";

        return "AVAILABLE";
    }

    public void BuyLevel()
    {
        if (PlayerLevel.Instance == null)
        {
            Debug.LogError("BakeryShopUI: PlayerLevel was not found.");
            return;
        }

        PlayerLevel.Instance.BuyNextLevel();
        RefreshUI();
    }
}
