
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "SO_StartingState",
    menuName = "BakeHouse/Starting State"
)]
public class StartingState : ScriptableObject
{
    [Header("Starting Wallet")]
    public int startingCoins = 0;

    // ✅ ADD THIS LINE: Starting diamonds (defaults to 0 per ticket)
    public int startingDiamonds = 0;

    [Header("Starting Ingredients")]
    public List<StartingIngredient> ingredients =
        new List<StartingIngredient>();
}

[Serializable]
public class StartingIngredient
{
    public IngredientData ingredient;
    [Min(0)] public int amount = 8;
}