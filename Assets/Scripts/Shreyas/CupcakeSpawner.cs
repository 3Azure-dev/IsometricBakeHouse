using UnityEngine;

public class CupcakePurchaseSpawner : MonoBehaviour
{
    [Header("Cupcake Settings")]
    [SerializeField] private GameObject cupcakePrefab;
    [SerializeField] private Transform spawnPoint;

    private GameObject spawnedCupcake;

    private void OnEnable()
    {
        if (BakeryProgress.Instance != null)
            BakeryProgress.Instance.Changed += OnProgressChanged;
    }

    private void OnDisable()
    {
        if (BakeryProgress.Instance != null)
            BakeryProgress.Instance.Changed -= OnProgressChanged;
    }

    private void Start()
    {
        // After a scene reload, show the Cupcake again if the recipe is already unlocked
        SpawnIfUnlocked();
    }

    private void OnProgressChanged(ProgressChange change)
    {
        if (change == ProgressChange.Recipe) SpawnIfUnlocked();
    }

    private void SpawnIfUnlocked()
    {
        if (BakeryProgress.Instance != null &&
            BakeryProgress.Instance.IsRecipeUnlocked(FoodType.Cupcake))
        {
            SpawnCupcake();
        }
    }

    // Still called by the shop's "On Cupcake Recipe Purchased" event
    public void SpawnCupcake()
    {
        // Prevent spawning duplicate cupcakes.
        if (spawnedCupcake != null)
        {
            Debug.Log("Cupcake has already been spawned.");
            return;
        }

        if (cupcakePrefab == null)
        {
            Debug.LogError("CupcakePurchaseSpawner: Cupcake prefab is missing!");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("CupcakePurchaseSpawner: Spawn point is missing!");
            return;
        }

        spawnedCupcake = Instantiate(cupcakePrefab, spawnPoint.position, spawnPoint.rotation);

        Debug.Log("Cupcake spawned successfully!");
    }
}