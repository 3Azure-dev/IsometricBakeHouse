using System.Collections.Generic;
using UnityEngine;

public class CustomerPool : MonoBehaviour
{
    public static CustomerPool Instance { get; private set; }

    [Header("Customer Prefabs")]
    [SerializeField] private GameObject normalCustomerPrefab;
    [SerializeField] private GameObject vipCustomerPrefab;

    [Header("Pool Size")]
    [SerializeField] private int normalPoolSize = 4;
    [SerializeField] private int vipPoolSize = 2;

    private Queue<GameObject> normalPool =
        new Queue<GameObject>();

    private Queue<GameObject> vipPool =
        new Queue<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        CreatePool(
            normalCustomerPrefab,
            normalPoolSize,
            normalPool
        );

        CreatePool(
            vipCustomerPrefab,
            vipPoolSize,
            vipPool
        );
    }

    private void CreatePool(
        GameObject prefab,
        int amount,
        Queue<GameObject> pool)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject customer =
                Instantiate(prefab);

            customer.SetActive(false);

            pool.Enqueue(customer);
        }
    }

    public GameObject GetCustomer(CustomerType type)
    {
        Queue<GameObject> pool;

        if (type == CustomerType.VIP)
        {
            pool = vipPool;
        }
        else
        {
            pool = normalPool;
        }

        if (pool.Count == 0)
        {
            Debug.LogWarning(
                "No " + type + " customers available in pool."
            );

            return null;
        }

        GameObject customer =
            pool.Dequeue();

        customer.SetActive(true);

        return customer;
    }

    public void ReturnCustomer(
        GameObject customer,
        CustomerType type)
    {
        customer.SetActive(false);

        if (type == CustomerType.VIP)
        {
            vipPool.Enqueue(customer);
        }
        else
        {
            normalPool.Enqueue(customer);
        }
    }
}