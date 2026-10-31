using UnityEngine;

public class FoodReadyPrompt : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag the Stove/Oven GameObject here")]
    [SerializeField] private MonoBehaviour cookingStation;

    [Tooltip("The UI Canvas with the Icon + 'E' text")]
    [SerializeField] private GameObject promptRoot;

    private IFoodReadyProvider provider;

    private void Awake()
    {
        // Try to cast the station to our interface
        if (cookingStation != null)
        {
            provider = cookingStation as IFoodReadyProvider;
            if (provider == null)
            {
                Debug.LogWarning($"FoodReadyPrompt: {cookingStation.name} needs to implement IFoodReadyProvider!");
            }
        }

        // Start hidden
        if (promptRoot != null) promptRoot.SetActive(false);
    }

    private void Update()
    {
        // Check if the stove says food is ready
        bool isReady = provider != null && provider.IsFoodReady;

        // Show or hide the prompt
        if (promptRoot != null && promptRoot.activeSelf != isReady)
        {
            promptRoot.SetActive(isReady);
        }
    }
}