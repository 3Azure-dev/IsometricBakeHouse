using UnityEngine;

public class CupcakePurchaseSpawner : MonoBehaviour
{
    [Header("Cupcake Settings")]
    [SerializeField] private GameObject cupcakePrefab;
    [SerializeField] private Transform spawnPoint;

    private GameObject spawnedCupcake;

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
            Debug.LogError(
                "CupcakePurchaseSpawner: Cupcake prefab is missing!"
            );
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError(
                "CupcakePurchaseSpawner: Spawn point is missing!"
            );
            return;
        }

        spawnedCupcake = Instantiate(
            cupcakePrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        Debug.Log("Cupcake spawned successfully!");
    }
}