using System;
using UnityEngine;

// The player's level. Same idea as Wallet: Singleton + DontDestroyOnLoad, created on start.
// The level NUMBER is stored in BakeryProgress, so it survives scene changes.
public class PlayerLevel : MonoBehaviour
{
    public static PlayerLevel Instance { get; private set; }

    // UI and scene objects listen to this. It sends the new level.
    public event Action<int> LevelChanged;

    private LevelData data;
    private BakeryProgress subscribedProgress;

    public int Level => BakeryProgress.Instance != null ? BakeryProgress.Instance.Level : 1;
    public int MaxLevel => data != null ? data.maxLevel : 3;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateOnStart()
    {
        if (Instance != null) return;
        new GameObject("PlayerLevel").AddComponent<PlayerLevel>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // The asset must sit in a folder named Resources and be called LevelData.
        data = Resources.Load<LevelData>("LevelData");

        if (data == null)
        {
            Debug.LogWarning("PlayerLevel: no LevelData asset in a Resources folder. Using the GDD defaults.");
            data = ScriptableObject.CreateInstance<LevelData>();
        }
    }

    // BakeryProgress may be created after us, so connect in Start
    private void Start()
    {
        Subscribe();
    }

    private void Subscribe()
    {
        if (subscribedProgress != null || BakeryProgress.Instance == null) return;

        subscribedProgress = BakeryProgress.Instance;
        subscribedProgress.Changed += OnProgressChanged;
    }

    private void OnDestroy()
    {
        if (subscribedProgress != null)
            subscribedProgress.Changed -= OnProgressChanged;
    }

    // Fires for EVERY way the level can change (buying, test buttons, other scripts)
    private void OnProgressChanged(ProgressChange change)
    {
        if (change == ProgressChange.Level)
            LevelChanged?.Invoke(Level);
    }

    // ---------------- ASK QUESTIONS ----------------

    public bool IsLevelReached(int level) => Level >= level;

    public bool IsMaxLevel => Level >= MaxLevel;

    // Coins needed to buy the NEXT level (-1 if already at the top)
    public int NextLevelCost
    {
        get
        {
            if (IsMaxLevel) return -1;

            LevelData.LevelEntry entry = data.GetEntry(Level + 1);
            return entry != null ? entry.coinCost : -1;
        }
    }

    public bool CanBuyNextLevel =>
        !IsMaxLevel &&
        NextLevelCost >= 0 &&
        Wallet.Instance != null &&
        Wallet.Instance.Coins >= NextLevelCost;

    // Has the player reached the level that unlocks this recipe?
    public bool IsRecipeAvailable(FoodType food)
    {
        foreach (LevelData.LevelEntry entry in data.levels)
        {
            if (entry.unlocksRecipe && entry.recipe == food)
                return Level >= entry.level;
        }

        return true; // recipes not in the list (Burger) are not locked by level
    }

    public bool IsEquipmentAvailable(EquipmentType type)
    {
        foreach (LevelData.LevelEntry entry in data.levels)
        {
            if (entry.unlocksEquipment && entry.equipment == type)
                return Level >= entry.level;
        }

        return true;
    }

    // ---------------- BUY A LEVEL ----------------

    // Returns false if the player is at max level or cannot afford it.
    // It does NOT give the recipe or equipment for free, it only makes them available to buy.
    public bool BuyNextLevel()
    {
        if (BakeryProgress.Instance == null || Wallet.Instance == null)
        {
            Debug.LogError("PlayerLevel: BakeryProgress or Wallet is missing.");
            return false;
        }

        if (IsMaxLevel)
        {
            Debug.Log("PlayerLevel: already at the highest level.");
            return false;
        }

        int cost = NextLevelCost;

        if (cost < 0)
        {
            Debug.LogError("PlayerLevel: no fee set for level " + (Level + 1) + " in LevelData.");
            return false;
        }

        if (!Wallet.Instance.SpendCoins(cost))
        {
            Debug.Log("PlayerLevel: not enough coins. Level " + (Level + 1) + " costs " + cost + ".");
            return false;
        }

        BakeryProgress.Instance.SetLevel(Level + 1);

        Debug.Log("PlayerLevel: bought level " + Level + " for " + cost + " coins.");
        return true;
    }

    // ---------------- TEST BUTTONS ----------------
    // Right-click the PlayerLevel object in the Hierarchy (DontDestroyOnLoad group) while playing

    [ContextMenu("TEST: Set Level 1")]
    private void TestLevel1() => SetLevelForTest(1);

    [ContextMenu("TEST: Set Level 2")]
    private void TestLevel2() => SetLevelForTest(2);

    [ContextMenu("TEST: Set Level 3")]
    private void TestLevel3() => SetLevelForTest(3);

    [ContextMenu("TEST: Buy Next Level")]
    private void TestBuyNext() => BuyNextLevel();

    // Sets any level for demos. No coins are spent.
    public void SetLevelForTest(int level)
    {
        if (BakeryProgress.Instance != null)
            BakeryProgress.Instance.SetLevel(level);
    }
}