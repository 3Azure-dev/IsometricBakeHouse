using System;
using UnityEngine;

public class Wallet : MonoBehaviour
{
    public static Wallet Instance { get; private set; }

    // UI listens to this. It sends the new total.
    public event Action<int> CoinsChanged;

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

    // right click the component in the Inspector to run these
    [ContextMenu("Test: add 10 coins")]
    private void TestAdd() => AddCoins(10);

    [ContextMenu("Test: spend 5 coins")]
    private void TestSpend() => SpendCoins(5);
}