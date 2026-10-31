using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Bakery/Level Data")]
public class LevelData : ScriptableObject
{
    [Serializable]
    public class LevelEntry
    {
        public int level;
        [Tooltip("Coins to BUY this level (from the level before it)")]
        public int coinCost;

        public bool unlocksRecipe;
        public FoodType recipe;

        public bool unlocksEquipment;
        public EquipmentType equipment;
    }

    [Tooltip("Highest level that is built. Level 4 is post-launch, so 3.")]
    public int maxLevel = 3;

    public List<LevelEntry> levels = new List<LevelEntry>
    {
        new LevelEntry { level = 1, coinCost = 0,    unlocksRecipe = true, recipe = FoodType.Bread,   unlocksEquipment = true, equipment = EquipmentType.Stove },
        new LevelEntry { level = 2, coinCost = 1000, unlocksRecipe = true, recipe = FoodType.Cupcake, unlocksEquipment = true, equipment = EquipmentType.Oven },
        new LevelEntry { level = 3, coinCost = 2000, unlocksRecipe = true, recipe = FoodType.Pizza,   unlocksEquipment = true, equipment = EquipmentType.PizzaStone }
    };

    public LevelEntry GetEntry(int level)
    {
        foreach (LevelEntry entry in levels)
        {
            if (entry.level == level) return entry;
        }

        return null;
    }
}