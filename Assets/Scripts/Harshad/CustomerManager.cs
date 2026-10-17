using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance { get; private set; }

    [Header("Settings (drag SO_CustomerSettings here)")]
    [SerializeField] private CustomerSettings settings;

    [Header("Places")]
    [SerializeField] private Transform counter;
    [SerializeField] private Transform exitPoint;

    [Header("Door line: empty objects on the floor. Slot 1 is the FRONT of the line.")]
    [SerializeField] private Transform[] doorSlots;

    [Header("Tables")]
    [SerializeField] private BakeryTable[] tables;

    // The door line. Queue = first in, first out.
    private readonly Queue<CustomerController> doorQueue = new Queue<CustomerController>();

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

    private void Start()
    {
        Debug.Log("CustomerManager: " + tables.Length + " tables, the door holds " + DoorCapacity);
    }

    private void Update()
    {
        SeatWaitingCustomers();
    }

    // --------------------------------------------------
    // DOOR LINE
    // --------------------------------------------------

    // Max at the door = the number from the settings, but never more than the slots you placed
    public int DoorCapacity
    {
        get
        {
            if (settings == null || doorSlots == null)
            {
                return 0;
            }

            return Mathf.Min(settings.maxCustomersAtDoor, doorSlots.Length);
        }
    }

    public int DoorFreeSpots
    {
        get { return DoorCapacity - doorQueue.Count; }
    }

    // A new customer joins the end of the line
    public bool JoinDoorQueue(CustomerController customer)
    {
        if (doorQueue.Count >= DoorCapacity)
        {
            return false;
        }

        doorQueue.Enqueue(customer);

        customer.SetDoorTarget(doorSlots[doorQueue.Count - 1].position);

        return true;
    }

    // Day ended: everybody still standing at the door walks away
    public void SendDoorQueueHome()
    {
        while (doorQueue.Count > 0)
        {
            doorQueue.Dequeue().LeaveBecauseDayEnded();
        }
    }

    // --------------------------------------------------
    // SEATING
    // --------------------------------------------------

    // Every frame: while a seat at a clear table is free, the front customer walks in
    private void SeatWaitingCustomers()
    {
        while (doorQueue.Count > 0)
        {
            Transform seat = TakeFreeSeat();

            if (seat == null)
            {
                // No free seat at a clear table. Door customers wait (forever).
                return;
            }

            CustomerController customer = doorQueue.Dequeue();

            customer.GoToSeat(seat);

            MoveDoorLineForward();
        }
    }

    // Everybody behind the customer who just left steps forward one place
    private void MoveDoorLineForward()
    {
        int slot = 0;

        foreach (CustomerController waiting in doorQueue)
        {
            waiting.SetDoorTarget(doorSlots[slot].position);
            slot++;
        }
    }

    private Transform TakeFreeSeat()
    {
        int maxSeats = settings != null ? settings.seatsPerTable : 2;

        foreach (BakeryTable table in tables)
        {
            if (table == null)
            {
                continue;
            }

            Transform seat;

            if (table.TryTakeSeat(maxSeats, out seat))
            {
                return seat;
            }
        }

        return null;
    }

    public void ReleaseSeat(Transform seat)
    {
        if (seat == null)
        {
            return;
        }

        foreach (BakeryTable table in tables)
        {
            if (table != null && table.OwnsSeat(seat))
            {
                table.ReleaseSeat(seat);
                return;
            }
        }
    }

    // A customer finished eating and left: the plate stays on the table
    public void LeaveDirtyPlate(Transform seat)
    {
        if (seat == null)
        {
            return;
        }

        foreach (BakeryTable table in tables)
        {
            if (table != null && table.OwnsSeat(seat))
            {
                table.LeaveDirtyPlate(seat);
                return;
            }
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