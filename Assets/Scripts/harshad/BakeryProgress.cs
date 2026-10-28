using System;
using System.Collections.Generic;
using UnityEngine;

public enum EquipmentType { Stove, Oven, PizzaStone }

public enum ProgressChange { Equipment, Recipe, Level, BuiltItems, Plots, Delivery }

// One piece of furniture the player built: which item, where, and how it is turned
[Serializable]
public struct BuiltItem
{
    public string itemId;
    public Vector3 position;
    public Vector3 eulerAngles;
}

// Keeps the bakery's progress across scene changes.
// Made the same way as Wallet: Singleton + DontDestroyOnLoad, created on start.
public class BakeryProgress : MonoBehaviour
{
    public static BakeryProgress Instance { get; private set; }

    public const int MaxLevel = 3;
    public const int MaxPlots = 3;

    // UI and scene objects listen to this. It says WHAT changed.
    public event Action<ProgressChange> Changed;

    private readonly HashSet<EquipmentType> equipment = new HashSet<EquipmentType>();
    private readonly HashSet<FoodType> unlockedRecipes = new HashSet<FoodType>();
    private readonly List<BuiltItem> builtItems = new List<BuiltItem>();
    private readonly HashSet<int> plots = new HashSet<int>();
    private readonly List<string> deliveryBox = new List<string>();

    public int Level { get; private set; } = 1;

    // Runs once when the game starts, before any scene loads
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateOnStart()
    {
        if (Instance != null) return;
        new GameObject("BakeryProgress").AddComponent<BakeryProgress>();
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

        // What the player owns at the very start of the game
        equipment.Add(EquipmentType.Stove);
        unlockedRecipes.Add(FoodType.Bread);
    }

    // ---------------- EQUIPMENT ----------------

    public bool HasEquipment(EquipmentType type) => equipment.Contains(type);

    public void AddEquipment(EquipmentType type)
    {
        if (equipment.Add(type))
        {
            Debug.Log("BakeryProgress: owns " + type);
            Changed?.Invoke(ProgressChange.Equipment);
        }
    }

    // ---------------- RECIPES ----------------

    public bool IsRecipeUnlocked(FoodType food) => unlockedRecipes.Contains(food);

    public void UnlockRecipe(FoodType food)
    {
        if (unlockedRecipes.Add(food))
        {
            Debug.Log("BakeryProgress: unlocked recipe " + food);
            Changed?.Invoke(ProgressChange.Recipe);
        }
    }

    // ---------------- LEVEL ----------------

    public void LevelUp() => SetLevel(Level + 1);

    // The one place where the level changes. Clamped to 1..MaxLevel.
    public void SetLevel(int level)
    {
        level = Mathf.Clamp(level, 1, MaxLevel);

        if (level == Level) return;

        Level = level;
        Debug.Log("BakeryProgress: level " + Level);
        Changed?.Invoke(ProgressChange.Level);
    }
    // ---------------- BUILT FURNITURE ----------------

    public void AddBuiltItem(string itemId, Vector3 position, Quaternion rotation)
    {
        builtItems.Add(new BuiltItem
        {
            itemId = itemId,
            position = position,
            eulerAngles = rotation.eulerAngles
        });

        Changed?.Invoke(ProgressChange.BuiltItems);
    }

    // A copy, so nobody edits the saved list by accident
    public List<BuiltItem> GetBuiltItems() => new List<BuiltItem>(builtItems);

    // ---------------- EXPANSION PLOTS (1, 2, 3) ----------------

    public bool HasPlot(int plot) => plots.Contains(plot);

    public void AddPlot(int plot)
    {
        if (plot < 1 || plot > MaxPlots) return;

        if (plots.Add(plot))
            Changed?.Invoke(ProgressChange.Plots);
    }

    public List<int> GetPlots() => new List<int>(plots);

    // ---------------- DELIVERY BOX (SCRUM-374) ----------------

    public void AddToDelivery(string itemId)
    {
        deliveryBox.Add(itemId);
        Changed?.Invoke(ProgressChange.Delivery);
    }

    public List<string> GetDelivery() => new List<string>(deliveryBox);

    // Call this when the player collects everything from the box
    public List<string> TakeAllFromDelivery()
    {
        List<string> items = new List<string>(deliveryBox);
        deliveryBox.Clear();
        Changed?.Invoke(ProgressChange.Delivery);
        return items;
    }

    // ---------------- TEST BUTTONS ----------------
    // Right-click the BakeryProgress object in the Hierarchy while the game is playing

    [ContextMenu("TEST: Give Oven")]
    private void TestGiveOven() => AddEquipment(EquipmentType.Oven);

    [ContextMenu("TEST: Unlock Cupcake")]
    private void TestUnlockCupcake() => UnlockRecipe(FoodType.Cupcake);

    [ContextMenu("TEST: Level Up")]
    private void TestLevelUp() => LevelUp();

    [ContextMenu("TEST: Buy Next Plot")]
    private void TestBuyPlot()
    {
        for (int i = 1; i <= MaxPlots; i++)
        {
            if (!HasPlot(i)) { AddPlot(i); return; }
        }
    }
}