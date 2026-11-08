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

    // True while a dirty plate is showing at this index (same order as the Seats list)
    public bool IsPlateDirty(int index)
    {
        return plateOnSeat != null && index >= 0 && index < plateOnSeat.Length && plateOnSeat[index];
    }

    // Index of the dirty plate closest to a position (ground distance), or -1 if no plate is dirty.
    public int NearestDirtyPlateIndex(Vector3 position)
    {
        if (plateOnSeat == null || dirtyPlates == null)
        {
            return -1;
        }

        int best = -1;
        float bestSqr = float.MaxValue;

        for (int i = 0; i < plateOnSeat.Length && i < dirtyPlates.Length; i++)
        {
            if (!plateOnSeat[i] || dirtyPlates[i] == null)
            {
                continue;
            }

            Vector3 p = dirtyPlates[i].transform.position;
            float dx = p.x - position.x;
            float dz = p.z - position.z;
            float sqr = dx * dx + dz * dz;

            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = i;
            }
        }

        return best;
    }

    // Which dirty plate should the player's prompt be for?
    // Normally the nearest one. But when the player is "in the middle" (two dirty plates are almost
    // the same distance away, within tieDistance), the plate he is FACING wins.
    // facing = the player's forward direction (height is ignored).
    public int PreferredDirtyPlateIndex(Vector3 position, Vector3 facing, float tieDistance)
    {
        if (plateOnSeat == null || dirtyPlates == null)
        {
            return -1;
        }

        facing.y = 0f;
        bool hasFacing = facing.sqrMagnitude > 0.0001f;
        if (hasFacing)
        {
            facing.Normalize();
        }

        // 1) distance to the nearest dirty plate
        float nearest = float.MaxValue;

        for (int i = 0; i < plateOnSeat.Length && i < dirtyPlates.Length; i++)
        {
            if (!plateOnSeat[i] || dirtyPlates[i] == null)
            {
                continue;
            }

            float dist = GroundDistance(position, dirtyPlates[i].transform.position);

            if (dist < nearest)
            {
                nearest = dist;
            }
        }

        if (nearest == float.MaxValue)
        {
            return -1;
        }

        // 2) among the plates that are about as near as the nearest one, pick the one he faces most
        int best = -1;
        float bestScore = float.MinValue;

        for (int i = 0; i < plateOnSeat.Length && i < dirtyPlates.Length; i++)
        {
            if (!plateOnSeat[i] || dirtyPlates[i] == null)
            {
                continue;
            }

            Vector3 plate = dirtyPlates[i].transform.position;
            float dist = GroundDistance(position, plate);

            if (dist > nearest + tieDistance)
            {
                continue;
            }

            float score;

            if (hasFacing && dist > 0.001f)
            {
                Vector3 toPlate = new Vector3(plate.x - position.x, 0f, plate.z - position.z) / dist;
                score = Vector3.Dot(facing, toPlate) - dist * 0.01f;   // tiny bonus for the nearer plate on a tie
            }
            else
            {
                score = -dist;
            }

            if (score > bestScore)
            {
                bestScore = score;
                best = i;
            }
        }

        return best;
    }

    private static float GroundDistance(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    // Cleans ONE plate (the old version only logged and cleaned nothing).
    public void CleanSpecificPlate(int plateIndex)
    {
        if (!IsPlateDirty(plateIndex))
        {
            return;
        }

        plateOnSeat[plateIndex] = false;

        if (dirtyPlates != null && plateIndex < dirtyPlates.Length && dirtyPlates[plateIndex] != null)
        {
            dirtyPlates[plateIndex].SetActive(false);
        }

        Debug.Log(name + ": cleaned plate at index " + plateIndex + ".");

        if (!HasDirtyPlates)
        {
            Debug.Log(name + ": plates cleared. The table is clear.");
        }
    }
}