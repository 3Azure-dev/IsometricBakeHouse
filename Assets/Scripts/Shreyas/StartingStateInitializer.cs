using UnityEngine;

public class StartingStateInitializer : MonoBehaviour
{
    [SerializeField] private StartingState startingState;

    private static bool initialized;

    // Reset the guard when a new Play session begins.
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetInitialization()
    {
        initialized = false;
    }

    private void Start()
    {
        // Prevent starting resources from being added again
        // when returning to or reloading a scene.
        if (initialized)
            return;

        if (startingState == null)
        {
            Debug.LogError(
                "StartingStateInitializer: Starting State asset is missing.");
            return;
        }

        if (Inventory1.Instance == null)
        {
            Debug.LogError(
                "StartingStateInitializer: Inventory1.Instance was not found.");
            return;
        }

        if (Wallet.Instance == null)
        {
            Debug.LogError(
                "StartingStateInitializer: Wallet.Instance was not found.");
            return;
        }

        // Set the starting coin balance.
        Wallet.Instance.SetStartingCoins(
            startingState.startingCoins);

        // Add each starting ingredient.
        foreach (StartingIngredient item in startingState.ingredients)
        {
            if (item.ingredient == null)
            {
                Debug.LogWarning(
                    "Starting State contains an ingredient with no asset assigned.");
                continue;
            }

            if (item.amount <= 0)
                continue;

            Inventory1.Instance.Add(
                item.ingredient,
                item.amount);
        }

        // Do not initialize again during scene changes.
        initialized = true;

        Debug.Log("Starting state applied successfully.");
    }
}