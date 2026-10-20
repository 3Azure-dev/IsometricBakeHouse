using UnityEngine;

public class OvenPurchaseSpawner : MonoBehaviour
{
    [Header("Oven Settings")]
    [SerializeField] private GameObject ovenPrefab;
    [SerializeField] private Transform spawnPoint;

    private GameObject spawnedOven;

    public void SpawnOven()
    {
        // Prevent duplicate spawning.
        if (spawnedOven != null)
        {
            Debug.Log("Oven has already been spawned.");
            return;
        }

        if (ovenPrefab == null)
        {
            Debug.LogError("OvenPurchaseSpawner: Oven prefab is missing!");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("OvenPurchaseSpawner: Spawn point is missing!");
            return;
        }

        // Spawn the oven immediately at the designated position.
        spawnedOven = Instantiate(
            ovenPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        Debug.Log("Oven spawned successfully!");
    }
}