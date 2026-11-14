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

    [Tooltip("Highest level the player can buy. Level 4 is the post-launch gate (GDD 1.8.5).")]
    public int maxLevel = 4;

    public List<LevelEntry> levels = new List<LevelEntry>
    {
        new LevelEntry { level = 1, coinCost = 0,    unlocksRecipe = true, recipe = FoodType.Bread,   unlocksEquipment = true, equipment = EquipmentType.Stove },
        new LevelEntry { level = 2, coinCost = 1000, unlocksRecipe = true, recipe = FoodType.Cupcake, unlocksEquipment = true, equipment = EquipmentType.Oven },
        new LevelEntry { level = 3, coinCost = 2000, unlocksRecipe = true, recipe = FoodType.Pizza,   unlocksEquipment = true, equipment = EquipmentType.PizzaStone },
        new LevelEntry { level = 4, coinCost = 3000, unlocksRecipe = false, unlocksEquipment = false }
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