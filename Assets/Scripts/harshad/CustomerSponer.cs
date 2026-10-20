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

    [Header("Shop Sign")]
    [SerializeField] private ShopOpenCloseSign shopSign;

    private float spawnTimer;
    private float nextSpawnTime;
    private bool shopIsOpen;

    private void OnEnable()
    {
        if (shopSign != null)
        {
            // Read the current shop state.
            shopIsOpen = shopSign.IsOpen;

            // Listen for the shop opening or closing.
            shopSign.OnShopStateChanged += HandleShopStateChanged;
        }
        else
        {
            Debug.LogError(
                "CustomerSpawner: Shop Open/Close Sign is not assigned!",
                this
            );

            shopIsOpen = false;
        }
    }

    private void Start()
    {
        SetNextSpawnTime();
    }

    private void OnDisable()
    {
        // Stop listening when this spawner is disabled.
        if (shopSign != null)
        {
            shopSign.OnShopStateChanged -= HandleShopStateChanged;
        }
    }

    private void Update()
    {
        // Do not spawn customers while the shop is closed.
        if (!shopIsOpen)
            return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= nextSpawnTime)
        {
            SpawnCustomer();

            spawnTimer = 0f;

            SetNextSpawnTime();
        }
    }

    private void HandleShopStateChanged(bool isOpen)
    {
        shopIsOpen = isOpen;

        // Start a fresh countdown when the shop changes state.
        spawnTimer = 0f;

        SetNextSpawnTime();

        Debug.Log(
            shopIsOpen
                ? "CustomerSpawner: Shop opened. Customers can spawn."
                : "CustomerSpawner: Shop closed. Customer spawning stopped."
        );
    }

    private void SetNextSpawnTime()
    {
        float minimum = Mathf.Max(0f, minimumSpawnTime);
        float maximum = Mathf.Max(minimum, maximumSpawnTime);

        nextSpawnTime = Random.Range(minimum, maximum);
    }

    private void SpawnCustomer()
    {
        if (spawnPoint == null)
        {
            Debug.LogError("CustomerSpawner: Spawn Point is not assigned!", this);
            return;
        }

        if (CustomerPool.Instance == null)
        {
            Debug.LogError("CustomerSpawner: CustomerPool.Instance was not found!", this);
            return;
        }

        CustomerType type;

        float randomNumber = Random.Range(0f, 100f);

        if (randomNumber < vipChance)
        {
            type = CustomerType.VIP;
        }
        else
        {
            type = CustomerType.Normal;
        }

        GameObject customer = CustomerPool.Instance.GetCustomer(type);

        if (customer == null)
            return;

        customer.transform.position = spawnPoint.position;
        customer.transform.rotation = spawnPoint.rotation;

        CustomerController controller =
            customer.GetComponent<CustomerController>();

        if (controller == null)
        {
            Debug.LogError(
                "CustomerSpawner: Spawned customer has no CustomerController!",
                customer
            );

            customer.SetActive(false);
            return;
        }

        controller.StartCustomer(type);
    }
}