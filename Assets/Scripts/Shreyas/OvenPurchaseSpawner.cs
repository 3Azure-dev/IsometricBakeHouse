using UnityEngine;

public class OvenPurchaseSpawner : MonoBehaviour
{
    [Header("Oven Settings")]
    [SerializeField] private GameObject ovenPrefab;
    [SerializeField] private Transform spawnPoint;

    private GameObject spawnedOven;

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
        // After a scene reload, show the Oven again if the player already owns it
        SpawnIfOwned();
    }

    private void OnProgressChanged(ProgressChange change)
    {
        if (change == ProgressChange.Equipment) SpawnIfOwned();
    }

    private void SpawnIfOwned()
    {
        if (BakeryProgress.Instance != null &&
            BakeryProgress.Instance.HasEquipment(EquipmentType.Oven))
        {
            SpawnOven();
        }
    }

    // Still called by the shop's "On Oven Purchased" event
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

        spawnedOven = Instantiate(ovenPrefab, spawnPoint.position, spawnPoint.rotation);

        Debug.Log("Oven spawned successfully!");
    }
}