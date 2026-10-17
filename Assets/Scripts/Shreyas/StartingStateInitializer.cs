using UnityEngine;

public class StartingStateInitializer : MonoBehaviour
{
    [SerializeField] private StartingState startingState;
    [SerializeField] private GameObject ingredientCountsUI;

    // Prevents resources from being granted more than once
    // during the same Play session.
    private static bool initialized;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetInitialization()
    {
        initialized = false;
    }

    // Called when the player interacts with the Shelf.
    // Called whenever the player interacts with the Shelf.
    public void ApplyStartingStateOnce()
    {
        // If the starting resources were already granted,
        // just show or hide the ingredient counter.
        if (initialized)
        {
            ToggleIngredientCountsUI();
            return;
        }

        if (startingState == null)
        {
            Debug.LogError("StartingState asset is missing!", this);
            return;
        }

        if (Inventory1.Instance == null)
        {
            Debug.LogError("Inventory1.Instance was not found!", this);
            return;
        }

        if (Wallet.Instance == null)
        {
            Debug.LogError("Wallet.Instance was not found!", this);
            return;
        }

        // Set starting coins.
        Wallet.Instance.SetStartingCoins(
            startingState.startingCoins
        );

        // Add starting ingredients.
        foreach (StartingIngredient item in startingState.ingredients)
        {
            if (item == null || item.ingredient == null)
            {
                Debug.LogWarning("An ingredient is not assigned.", this);
                continue;
            }

            if (item.amount <= 0)
                continue;

            Inventory1.Instance.Add(
                item.ingredient,
                item.amount
            );
        }

        initialized = true;

        // Show the counters after the first interaction.
        if (ingredientCountsUI != null)
            ingredientCountsUI.SetActive(true);

        Debug.Log("Starting state applied successfully!", this);
    }

    private void ToggleIngredientCountsUI()
    {
        if (ingredientCountsUI == null)
        {
            Debug.LogWarning("Ingredient Counts UI is not assigned!", this);
            return;
        }

        bool showUI = !ingredientCountsUI.activeSelf;

        ingredientCountsUI.SetActive(showUI);

        Debug.Log(showUI
            ? "Ingredient counter opened."
            : "Ingredient counter closed.");
    }

    private void Awake()
    {
        // Keep the counters hidden until the player interacts
        // with the Shelf.
        if (ingredientCountsUI != null)
            ingredientCountsUI.SetActive(false);
    }
}