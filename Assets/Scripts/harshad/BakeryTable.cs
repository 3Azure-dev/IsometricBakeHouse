using System.Collections.Generic;
using UnityEngine;

// One table. It knows its seats, which seats are taken, and whether it has dirty plates.
public class BakeryTable : MonoBehaviour
{
    [Header("Seats and plates (same order: plate 1 belongs to seat 1)")]
    [Tooltip("The seat positions of this table (2 chairs = 2 seats). Put them on the floor, on the NavMesh.")]
    [SerializeField] private Transform[] seats;

    [Tooltip("One dirty plate object for each seat. They start hidden and show when a customer leaves.")]
    [SerializeField] private GameObject[] dirtyPlates;

    private readonly List<Transform> takenSeats = new List<Transform>();

    // plateOnSeat[i] is true while a dirty plate is showing at seat i
    private bool[] plateOnSeat;

    public bool HasDirtyPlates
    {
        get
        {
            if (plateOnSeat == null)
            {
                return false;
            }

            foreach (bool shown in plateOnSeat)
            {
                if (shown)
                {
                    return true;
                }
            }

            return false;
        }
    }

    // A table with dirty plates is NOT clear: nobody can sit there
    public bool IsClear
    {
        get { return !HasDirtyPlates; }
    }

    private void Awake()
    {
        plateOnSeat = new bool[seats != null ? seats.Length : 0];

        // Plates only appear after a customer leaves one
        if (dirtyPlates != null)
        {
            foreach (GameObject plate in dirtyPlates)
            {
                if (plate != null)
                {
                    plate.SetActive(false);
                }
            }
        }
    }

    // --------------------------------------------------
    // SEATS
    // --------------------------------------------------

    // Tries to give out a free seat. Only works while the table is clear.
    public bool TryTakeSeat(int maxSeats, out Transform seat)
    {
        seat = null;

        if (!IsClear)
        {
            return false;
        }

        int usableSeats = Mathf.Min(maxSeats, seats.Length);

        for (int i = 0; i < usableSeats; i++)
        {
            if (seats[i] != null && !takenSeats.Contains(seats[i]))
            {
                takenSeats.Add(seats[i]);
                seat = seats[i];
                return true;
            }
        }

        return false;
    }

    public bool OwnsSeat(Transform seat)
    {
        if (seat == null)
        {
            return false;
        }

        return System.Array.IndexOf(seats, seat) >= 0;
    }

    public void ReleaseSeat(Transform seat)
    {
        takenSeats.Remove(seat);
    }

    // --------------------------------------------------
    // DIRTY PLATES
    // --------------------------------------------------

    // A customer finished eating at this seat: the plate stays on the table
    public void LeaveDirtyPlate(Transform seat)
    {
        if (seat == null)
        {
            return;
        }

        int index = System.Array.IndexOf(seats, seat);

        if (index < 0)
        {
            return;
        }

        plateOnSeat[index] = true;

        if (dirtyPlates != null && index < dirtyPlates.Length && dirtyPlates[index] != null)
        {
            dirtyPlates[index].SetActive(true);
        }

        Debug.Log(name + ": dirty plate at seat " + (index + 1) + ". The table is NOT clear.");
    }

    // The player presses E at the table
    public void Interact()
    {
        if (!HasDirtyPlates)
        {
            Debug.Log(name + ": no dirty plates to clear.");
            return;
        }

        ClearPlates();
    }

    [ContextMenu("Test: clear")]
    public void ClearPlates()
    {
        if (plateOnSeat == null)
        {
            return;
        }

        for (int i = 0; i < plateOnSeat.Length; i++)
        {
            plateOnSeat[i] = false;

            if (dirtyPlates != null && i < dirtyPlates.Length && dirtyPlates[i] != null)
            {
                dirtyPlates[i].SetActive(false);
            }
        }

        Debug.Log(name + ": plates cleared. The table is clear.");
    }

    [ContextMenu("Test: mark dirty")]
    public void MarkDirty()
    {
        if (seats != null && seats.Length > 0)
        {
            LeaveDirtyPlate(seats[0]);
        }
    }
}