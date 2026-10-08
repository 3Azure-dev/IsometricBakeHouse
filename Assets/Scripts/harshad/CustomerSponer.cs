using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;

    [Header("Timing")]
    [SerializeField] private float minimumSpawnTime = 5f;
    [SerializeField] private float maximumSpawnTime = 10f;

    [Header("VIP")]
    [Range(0f, 100f)]
    [SerializeField] private float vipChance = 10f;

    private float spawnTimer;
    private float nextSpawnTime;

    private void Start()
    {
        SetNextSpawnTime();
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= nextSpawnTime)
        {
            SpawnCustomer();

            spawnTimer = 0f;

            SetNextSpawnTime();
        }
    }

    private void SetNextSpawnTime()
    {
        nextSpawnTime =
            Random.Range(
                minimumSpawnTime,
                maximumSpawnTime
            );
    }

    private void SpawnCustomer()
    {
        CustomerType type;

        float randomNumber =
            Random.Range(0f, 100f);

        if (randomNumber <= vipChance)
        {
            type = CustomerType.VIP;
        }
        else
        {
            type = CustomerType.Normal;
        }

        GameObject customer =
            CustomerPool.Instance.GetCustomer(type);

        if (customer == null)
            return;

        customer.transform.position =
            spawnPoint.position;

        customer.transform.rotation =
            spawnPoint.rotation;

        CustomerController controller =
            customer.GetComponent<CustomerController>();

        controller.StartCustomer(type);
    }
}