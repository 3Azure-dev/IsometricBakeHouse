using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// SCRUM: Save and load progress between play sessions.
// Saves: coins, diamonds, ingredients (bag + shelf), level, recipes, equipment, day number.
// Not saved this sprint: built furniture and plots.
public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    [Serializable]
    public class IngredientEntry
    {
        public string key;
        public int amount;
    }

    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public int coins;
        public int diamonds;
        public int level = 1;
        public int dayNumber = 1;
        public List<string> equipment = new List<string>();
        public List<string> recipes = new List<string>();
        public List<IngredientEntry> ingredients = new List<IngredientEntry>();
    }

    [Tooltip("Drag ALL 9 ingredient assets here, so saved names can be turned back into assets")]
    [SerializeField] private IngredientData[] allIngredients;

    [SerializeField] private string bakerySceneName = "Bakery";

    [Tooltip("How often the shelf stock is remembered (seconds), so a quit during a hunt still saves it")]
    [SerializeField] private float shelfCacheSeconds = 2f;

    private bool scheduled;      // a load is already planned
    private bool skipLoad;       // New Game was pressed
    private bool ready;          // the bakery has started, so saving is safe
    private bool subscribedToClock;

    private readonly Dictionary<string, int> shelfCache = new Dictionary<string, int>();

    private static string SavePath
    {
        get { return Path.Combine(Application.persistentDataPath, "save.json"); }
    }

    public static bool HasSave
    {
        get { return File.Exists(SavePath); }
    }

    // ---------------- SETUP ----------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        TryScheduleLoad(SceneManager.GetActiveScene());
        StartCoroutine(CacheShelfRoutine());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryScheduleLoad(scene);
    }

    private void TryScheduleLoad(Scene scene)
    {
        if (scheduled || scene.name != bakerySceneName) return;

        scheduled = true;
        StartCoroutine(LoadNextFrame());
    }

    // Wait one frame so Inventory1 and DayNightClock have woken up
    private IEnumerator LoadNextFrame()
    {
        yield return null;

        if (!skipLoad && HasSave)
            Load();

        ready = true;
        SubscribeToClock();
    }

    private void SubscribeToClock()
    {
        if (subscribedToClock || DayNightClock.Instance == null) return;

        // Autosave at every new day, so a crash loses at most one day
        DayNightClock.Instance.NightEnded += Save;
        subscribedToClock = true;
    }

    // ---------------- NEW GAME ----------------

    // Hook this to the New Game button (before it loads the Bakery)
    public void NewGame()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);

        skipLoad = true;
        Debug.Log("SaveSystem: save deleted. New game.");
    }

    // ---------------- SAVE ----------------

    private void OnApplicationQuit()
    {
        Save();
    }

    public void Save()
    {
        // Never save before the bakery has started, or we would overwrite the file with an empty game
        if (!ready) return;
        if (Wallet.Instance == null || BakeryProgress.Instance == null) return;

        CacheShelf();

        SaveData data = new SaveData();

        data.coins = Wallet.Instance.Coins;
        data.diamonds = Wallet.Instance.Diamonds;
        data.level = BakeryProgress.Instance.Level;

        if (DayNightClock.Instance != null)
            data.dayNumber = DayNightClock.Instance.DayNumber;

        foreach (EquipmentType type in Enum.GetValues(typeof(EquipmentType)))
        {
            if (BakeryProgress.Instance.HasEquipment(type))
                data.equipment.Add(type.ToString());
        }

        foreach (FoodType food in Enum.GetValues(typeof(FoodType)))
        {
            if (BakeryProgress.Instance.IsRecipeUnlocked(food))
                data.recipes.Add(food.ToString());
        }

        // Ingredients = what is in the bag + what is on the shelf
        Dictionary<string, int> totals = new Dictionary<string, int>();

        if (Inventory1.Instance != null)
        {
            foreach (KeyValuePair<IngredientData, int> pair in Inventory1.Instance.GetAllCounts())
            {
                if (pair.Key == null) continue;
                AddTo(totals, Key(pair.Key), pair.Value);
            }
        }

        foreach (KeyValuePair<string, int> pair in shelfCache)
            AddTo(totals, pair.Key, pair.Value);

        foreach (KeyValuePair<string, int> pair in totals)
        {
            if (pair.Value <= 0) continue;
            data.ingredients.Add(new IngredientEntry { key = pair.Key, amount = pair.Value });
        }

        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));

        Debug.Log("SaveSystem: saved. Coins " + data.coins + ", diamonds " + data.diamonds +
                  ", level " + data.level + ", day " + data.dayNumber + ". File: " + SavePath);
    }

    // ---------------- LOAD ----------------

    public void Load()
    {
        if (!HasSave) return;

        SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));

        if (data == null)
        {
            Debug.LogWarning("SaveSystem: the save file could not be read.");
            return;
        }

        if (Wallet.Instance != null)
        {
            Wallet.Instance.SetStartingCoins(data.coins);
            Wallet.Instance.SetStartingDiamonds(data.diamonds);
        }

        if (BakeryProgress.Instance != null)
        {
            BakeryProgress.Instance.SetLevel(data.level);

            foreach (string name in data.equipment)
            {
                if (Enum.TryParse(name, out EquipmentType type))
                    BakeryProgress.Instance.AddEquipment(type);
            }

            foreach (string name in data.recipes)
            {
                if (Enum.TryParse(name, out FoodType food))
                    BakeryProgress.Instance.UnlockRecipe(food);
            }
        }

        if (DayNightClock.Instance != null)
            DayNightClock.Instance.SetDayNumber(data.dayNumber);

        if (Inventory1.Instance != null)
        {
            foreach (IngredientEntry entry in data.ingredients)
            {
                IngredientData ingredient = FindIngredient(entry.key);

                if (ingredient != null)
                    Inventory1.Instance.Add(ingredient, entry.amount);
                else
                    Debug.LogWarning("SaveSystem: no ingredient asset for '" + entry.key + "'. Is it in the All Ingredients list?");
            }
        }

        // The loaded save must not be replaced by the starting state at the first shelf visit
        StartingStateInitializer.MarkInitialized();

        Debug.Log("SaveSystem: loaded. Coins " + data.coins + ", day " + data.dayNumber + ", level " + data.level);
    }

    // ---------------- SHELF STOCK ----------------

    // The shelf only exists in the Bakery, so remember its stock while we can see it
    private IEnumerator CacheShelfRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(shelfCacheSeconds);
            CacheShelf();
        }
    }

    private void CacheShelf()
    {
        IngredientShelf shelf = FindFirstObjectByType<IngredientShelf>();

        if (shelf == null || allIngredients == null) return;

        shelfCache.Clear();

        foreach (IngredientData ingredient in allIngredients)
        {
            if (ingredient == null) continue;

            if (Enum.TryParse(ingredient.ingredientName.Replace(" ", ""), true, out IngredientType type))
                shelfCache[Key(ingredient)] = shelf.GetAmount(type);
        }
    }

    // ---------------- HELPERS ----------------

    private static string Key(IngredientData ingredient)
    {
        return ingredient.ingredientName.Replace(" ", "").ToLowerInvariant();
    }

    private IngredientData FindIngredient(string key)
    {
        if (allIngredients == null) return null;

        foreach (IngredientData ingredient in allIngredients)
        {
            if (ingredient != null && Key(ingredient) == key)
                return ingredient;
        }

        return null;
    }

    private static void AddTo(Dictionary<string, int> dict, string key, int amount)
    {
        dict[key] = (dict.ContainsKey(key) ? dict[key] : 0) + amount;
    }
}