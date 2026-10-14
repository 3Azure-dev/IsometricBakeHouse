using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance { get; private set; }

    [Header("Settings (drag SO_CustomerSettings here)")]
    [SerializeField] private CustomerSettings settings;

    [Header("Customer Locations")]
    [SerializeField] private Transform counter;
    [SerializeField] private Transform exitPoint;

    [Header("Seats")]
    [Tooltip("OPTION A (new): drag each table here. Its child objects are the seats, " +
             "and only the first 'Seats Per Table' children are used.")]
    [SerializeField] private Transform[] tables;

    [Tooltip("OPTION B (old way): used only if the Tables list above is empty.")]
    [SerializeField] private Transform[] seats;

    // The final list of seats customers can actually use
    private List<Transform> activeSeats = new List<Transform>();

    private List<Transform> occupiedSeats = new List<Transform>();

    private CustomerController customerAtCounter;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        BuildSeatList();
    }

    // --------------------------------------------------
    // BUILD SEAT LIST
    // --------------------------------------------------

    private void BuildSeatList()
    {
        activeSeats.Clear();

        // No tables assigned: use the old seats array exactly like before
        if (tables == null || tables.Length == 0)
        {
            activeSeats.AddRange(seats);
            return;
        }

        if (settings == null)
        {
            Debug.LogError("CustomerManager has no CustomerSettings assigned.");
            return;
        }

        foreach (Transform table in tables)
        {
            if (table == null)
                continue;

            // Use at most 'seatsPerTable' children, or fewer if the table has fewer
            int seatCount = Mathf.Min(settings.seatsPerTable, table.childCount);

            if (table.childCount < settings.seatsPerTable)
            {
                Debug.LogWarning(
                    table.name + " has only " + table.childCount +
                    " seat objects, but Seats Per Table is " + settings.seatsPerTable
                );
            }

            for (int i = 0; i < seatCount; i++)
            {
                activeSeats.Add(table.GetChild(i));
            }
        }

        Debug.Log("Total usable seats: " + activeSeats.Count);
    }

    // --------------------------------------------------
    // SEATS
    // --------------------------------------------------

    public Transform GetFreeSeat()
    {
        foreach (Transform seat in activeSeats)
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

    // --------------------------------------------------
    // COUNTER (unchanged)
    // --------------------------------------------------

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