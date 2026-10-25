using UnityEngine;

// Grandfather comes visitsPerBlock times in every daysPerBlock days
public static class GrandfatherVisits
{
    public static int currentBlock = -1;
    public static int visitsThisBlock = 0;
    public static int lastVisitDay = -1;

    public static int Today
    {
        get { return DayNightClock.Instance != null ? DayNightClock.Instance.DayNumber : 1; }
    }

    public static int GetBlock(int day, int daysPerBlock)
    {
        return (day - 1) / Mathf.Max(1, daysPerBlock);
    }

    public static bool ShouldCome(DunkData data)
    {
        int day = Today;
        int days = Mathf.Max(1, data.daysPerBlock);
        int block = GetBlock(day, days);

        if (block != currentBlock)
        {
            currentBlock = block;
            visitsThisBlock = 0;
        }

        int visitsLeft = data.visitsPerBlock - visitsThisBlock;
        int daysLeft = days - ((day - 1) % days);

        Debug.Log("Grandfather: day " + day + ", visits " + visitsThisBlock + "/" + data.visitsPerBlock);

        if (visitsLeft <= 0) return false;
        if (lastVisitDay == day) return false;
        if (daysLeft <= visitsLeft) return true; // must come before the block ends

        return Random.value < data.grandfatherChance;
    }

    public static void RecordVisit()
    {
        visitsThisBlock++;
        lastVisitDay = Today;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void ResetVisits()
    {
        currentBlock = -1;
        visitsThisBlock = 0;
        lastVisitDay = -1;
    }
}