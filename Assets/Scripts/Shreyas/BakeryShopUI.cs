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

    [Header("Button Labels")]
    [SerializeField] private TMP_Text ovenButtonText;
    [SerializeField] private TMP_Text cupcakeRecipeButtonText;

    [Header("Optional Status Text")]
    [SerializeField] private TMP_Text ovenStatusText;
    [SerializeField] private TMP_Text cupcakeRecipeStatusText;

    [Header("Purchase Events")]
    [SerializeField] private UnityEvent onOvenPurchased;
    [SerializeField] private UnityEvent onCupcakeRecipePurchased;

    private bool ovenPurchased;
    private bool cupcakeRecipePurchased;

    private Wallet subscribedWallet;

    private void Awake()
    {
        if (buyOvenButton != null)
            buyOvenButton.onClick.AddListener(BuyOven);

        if (buyCupcakeRecipeButton != null)
            buyCupcakeRecipeButton.onClick.AddListener(BuyCupcakeRecipe);
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
    }

    private void OnDestroy()
    {
        if (buyOvenButton != null)
            buyOvenButton.onClick.RemoveListener(BuyOven);

        if (buyCupcakeRecipeButton != null)
            buyCupcakeRecipeButton.onClick.RemoveListener(BuyCupcakeRecipe);
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
        if (ovenPurchased)
            return;

        if (!TrySpendCoins(ovenPrice))
            return;

        ovenPurchased = true;

        if (onOvenPurchased != null)
            onOvenPurchased.Invoke();

        RefreshUI();

        Debug.Log("Oven purchased for " + ovenPrice + " coins.");
    }

    public void BuyCupcakeRecipe()
    {
        if (cupcakeRecipePurchased)
            return;

        if (!TrySpendCoins(cupcakeRecipePrice))
            return;

        cupcakeRecipePurchased = true;

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

        if (buyOvenButton != null)
            buyOvenButton.interactable =
                !ovenPurchased && canAffordOven;

        if (buyCupcakeRecipeButton != null)
            buyCupcakeRecipeButton.interactable =
                !cupcakeRecipePurchased && canAffordRecipe;

        if (ovenButtonText != null)
            ovenButtonText.text = ovenPurchased ? "OWNED" : "BUY";

        if (cupcakeRecipeButtonText != null)
            cupcakeRecipeButtonText.text =
                cupcakeRecipePurchased ? "OWNED" : "BUY";

        if (ovenStatusText != null)
            ovenStatusText.text = GetStatus(ovenPurchased, canAffordOven);

        if (cupcakeRecipeStatusText != null)
            cupcakeRecipeStatusText.text =
                GetStatus(cupcakeRecipePurchased, canAffordRecipe);
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
}
