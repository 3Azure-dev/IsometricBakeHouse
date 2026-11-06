using UnityEngine;

public class IngredientCounterUI : MonoBehaviour
{
    [Header("World Ingredient List")]
    [SerializeField] private IngredientList ingredientList;

    [Header("Ingredient Row Prefab")]
    [SerializeField] private IngredientRowUI rowPrefab;

    [Header("Row Container")]
    [SerializeField] private Transform rowsParent;

    private void Start()
    {
        BuildIngredientRows();
    }

    private void BuildIngredientRows()
    {
        if (ingredientList == null)
        {
            Debug.LogError(
                "IngredientCounterUI: Ingredient List is not assigned.",
                this
            );
            return;
        }

        if (rowPrefab == null)
        {
            Debug.LogError(
                "IngredientCounterUI: Row Prefab is not assigned.",
                this
            );
            return;
        }

        Transform parent = rowsParent != null
            ? rowsParent
            : transform;

        foreach (IngredientData ingredient in ingredientList.ingredients)
        {
            if (ingredient == null)
            {
                Debug.LogWarning(
                    "IngredientCounterUI: An ingredient entry is empty.",
                    this
                );
                continue;
            }

            IngredientRowUI row = Instantiate(rowPrefab, parent);

            row.transform.localScale = Vector3.one;
            row.Initialize(ingredient);
        }

        Debug.Log(
            $"IngredientCounterUI: Created " +
            $"{ingredientList.ingredients.Count} ingredient rows.",
            this
        );
    }
}