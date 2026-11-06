using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "SO_IngredientList",
    menuName = "Tiny Bake House/Ingredient List"
)]
public class IngredientList : ScriptableObject
{
    [Header("Ingredients Available in This World")]
    public List<IngredientData> ingredients = new List<IngredientData>();
}