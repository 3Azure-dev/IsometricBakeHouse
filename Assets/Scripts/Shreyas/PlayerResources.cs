using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    public int sacks = 0;
    public int coins = 0;
    public void AddSack(int amount)
    {
        sacks += amount;
    }
    public bool UseSack(int amount)
    {
        if (sacks < amount)
        {
            return false;
        }
        sacks -= amount;
        return true;
    }
    public bool SpendCoins(int amount)
    {
        if(coins < amount)
        {
            return false;
        }
        coins -= amount;
        return true;
    }
    
        
    
    
}

