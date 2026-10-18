using System;
using UnityEngine;

public class Wallet : MonoBehaviour
{
    public static Wallet Instance { get; private set; }

    // UI listens to this. It sends the new total.
    public event Action<int> CoinsChanged;

    // Diamonds (second currency, SCRUM-377). Same idea as coins: UI listens to this event.
    public event Action<int> DiamondsChanged;

    [SerializeField] private int _startingCoins = 0;

    public int Coins { get; private set; }

    // runs once when the game starts, before any scene loads
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateOnStart()
    {
        if (Instance != null) return;
        new GameObject("Wallet").AddComponent<Wallet>();
    }

    private void Awake()
    {
        // only one Wallet may exist, and it survives scene changes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Coins = _startingCoins;
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;

        Coins += amount;
        CoinsChanged?.Invoke(Coins);
        Debug.Log($"Wallet: +{amount}, total {Coins}");
    }

    // returns false if the player cannot afford it
    public bool SpendCoins(int amount)
    {
        if (amount <= 0 || amount > Coins) return false;

        Coins -= amount;
        CoinsChanged?.Invoke(Coins);
        Debug.Log($"Wallet: -{amount}, total {Coins}");
        return true;
    }

    // --------------------------------------------------
    // DIAMONDS (SCRUM-377). They start at 0 and live in this same Wallet,
    // so they survive scene changes (a hunt) exactly like coins.
    // --------------------------------------------------

    public int Diamonds { get; private set; }

    // Same idea as SetStartingCoins: the Starting State asset can call this later
    public void SetStartingDiamonds(int amount)
    {
        Diamonds = Mathf.Max(0, amount);
        DiamondsChanged?.Invoke(Diamonds);
    }

    public void AddDiamonds(int amount)
    {
        if (amount <= 0) return;

        Diamonds += amount;
        DiamondsChanged?.Invoke(Diamonds);
        Debug.Log($"Wallet: +{amount} diamonds, total {Diamonds}");
    }

    public bool CanAffordDiamonds(int amount)
    {
        return amount > 0 && amount <= Diamonds;
    }

    // returns false if the player cannot afford it
    public bool SpendDiamonds(int amount)
    {
        if (!CanAffordDiamonds(amount)) return false;

        Diamonds -= amount;
        DiamondsChanged?.Invoke(Diamonds);
        Debug.Log($"Wallet: -{amount} diamonds, total {Diamonds}");
        return true;
    }

    [ContextMenu("Test: add 10 diamonds")]
    private void TestAddDiamonds() => AddDiamonds(10);

    [ContextMenu("Test: spend 5 diamonds")]
    private void TestSpendDiamonds() => SpendDiamonds(5);

    // right click the component in the Inspector to run these
    [ContextMenu("Test: add 10 coins")]
    private void TestAdd() => AddCoins(10);

    [ContextMenu("Test: spend 5 coins")]
    private void TestSpend() => SpendCoins(5);
}