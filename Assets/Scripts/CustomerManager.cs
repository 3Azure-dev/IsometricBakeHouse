using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance { get; private set; }

    [Header("Customer Locations")]
    [SerializeField] private Transform counter;
    [SerializeField] private Transform[] seats;
    [SerializeField] private Transform exitPoint;

    private List<Transform> occupiedSeats =
        new List<Transform>();

    private CustomerController customerAtCounter;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public Transform GetFreeSeat()
    {
        foreach (Transform seat in seats)
        {
            if (!occupiedSeats.Contains(seat))
            {
                occupiedSeats.Add(seat);
                return seat;
            }
        }

        return null;
    }

    public void ReleaseSeat(Transform seat)
    {
        if (seat == null)
            return;

        if (occupiedSeats.Contains(seat))
        {
            occupiedSeats.Remove(seat);
        }
    }

    public bool IsCounterFree()
    {
        return customerAtCounter == null;
    }

    public bool TryTakeCounter(CustomerController customer)
    {
        if (customerAtCounter != null)
            return false;

        customerAtCounter = customer;
        return true;
    }

    public void LeaveCounter(CustomerController customer)
    {
        if (customerAtCounter == customer)
        {
            customerAtCounter = null;
        }
    }

    public Transform GetCounter()
    {
        return counter;
    }

    public Transform GetExitPoint()
    {
        return exitPoint;
    }
}