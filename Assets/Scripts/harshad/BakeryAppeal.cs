using UnityEngine;

// SCRUM-382: bakery appeal and tips (GDD 1.8.5, section 11).
// Appeal = total appeal points of everything built. Tip chance = appeal x 2%, max 30%.
public static class BakeryAppeal
{
    // Adds up the appeal of every built item
    public static int GetTotal(CustomerSettings settings)
    {
        if (settings == null || settings.furnitureList == null) return 0;
        if (BakeryProgress.Instance == null) return 0;

        int total = 0;

        foreach (BuiltItem built in BakeryProgress.Instance.GetBuiltItems())
        {
            foreach (FurnitureData data in settings.furnitureList)
            {
                if (data != null && data.itemId == built.itemId)
                {
                    total += data.appeal;
                    break;
                }
            }
        }

        return total;
    }

    // Chance between 0 and maxTipChance
    public static float GetTipChance(CustomerSettings settings, int appeal)
    {
        return Mathf.Min(appeal * settings.tipChancePerPoint, settings.maxTipChance);
    }

    // Rolls the tip. Returns the coins to add (0 = no tip).
    public static int RollTip(CustomerSettings settings, int dishPrice)
    {
        int appeal = GetTotal(settings);
        float chance = GetTipChance(settings, appeal);
        float roll = Random.value;
        int tip = Mathf.RoundToInt(dishPrice * settings.tipSize);

        bool gotTip = roll < chance;

        Debug.Log("Tip: appeal " + appeal + ", chance " + Mathf.RoundToInt(chance * 100f) +
                  "%, roll " + roll.ToString("0.00") +
                  (gotTip ? " -> TIP " + tip + " coins" : " -> no tip"));

        return gotTip ? tip : 0;
    }
}