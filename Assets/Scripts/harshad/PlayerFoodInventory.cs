using System.Collections.Generic;
using UnityEngine;

public class PlayerFoodInventory : MonoBehaviour
{
    public static PlayerFoodInventory Instance { get; private set; }

    [Header("Inventory")]
    [SerializeField] private int maximumFoodCapacity = 4;

    // static: the Bakery scene is rebuilt after every Main Menu / hunt trip,
    // so the list must live outside the scene object
    private static List<FoodType> carriedFoods = new List<FoodType>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnStart()
    {
        carriedFoods = new List<FoodType>();
        SaveSystem.GameReset -= ClearOnNewGame;
        SaveSystem.GameReset += ClearOnNewGame;
    }

    // New Game: nothing is carried
    private static void ClearOnNewGame()
    {
        carriedFoods.Clear();
        if (Instance != null) Instance.Changed?.Invoke();
    }

    // Used by SaveSystem
    public static List<FoodType> SnapshotCarried()
    {
        return new List<FoodType>(carriedFoods);
    }

    public static void RestoreCarried(List<FoodType> foods)
    {
        carriedFoods = new List<FoodType>(foods);
        if (Instance != null) Instance.Changed?.Invoke();
    }
    public event System.Action Changed;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    // --------------------------------------------------
    // CHECK IF CARRYING ANY FOOD
    // --------------------------------------------------

    public bool IsCarryingFood()
    {
        return carriedFoods.Count > 0;
    }


    // --------------------------------------------------
    // CHECK IF INVENTORY IS FULL
    // --------------------------------------------------

    public bool IsFull()
    {
        return carriedFoods.Count >=
               maximumFoodCapacity;
    }


    // --------------------------------------------------
    // GET CURRENT FOOD COUNT
    // --------------------------------------------------

    public int GetFoodCount()
    {
        return carriedFoods.Count;
    }


    // --------------------------------------------------
    // TAKE FOOD
    // --------------------------------------------------

    public bool TakeFood(FoodType food)
    {
        if (IsFull())
        {
            Debug.Log(
                "Food inventory is full."
            );

            return false;
        }

        carriedFoods.Add(food);
        Changed?.Invoke();

        Debug.Log(
            "Player picked up " +
            food +
            ". Food count: " +
            carriedFoods.Count
        );

        return true;
    }


    // --------------------------------------------------
    // CHECK FOR SPECIFIC FOOD
    // --------------------------------------------------

    public bool HasFood(FoodType food)
    {
        return carriedFoods.Contains(food);
    }


    // --------------------------------------------------
    // REMOVE SPECIFIC FOOD
    // --------------------------------------------------

    public bool RemoveFood(FoodType food)
    {
        if (!carriedFoods.Contains(food))
        {
            return false;
        }

        carriedFoods.Remove(food);
        Changed?.Invoke();

        Debug.Log(
            "Player delivered " +
            food
        );

        return true;
    }


    // --------------------------------------------------
    // GET ALL CARRIED FOOD
    // --------------------------------------------------

    public List<FoodType> GetCarriedFoods()
    {
        return new List<FoodType>(
            carriedFoods
        );
    }


    // --------------------------------------------------
    // CLEAR INVENTORY
    // --------------------------------------------------

    public void ClearInventory()
    {
        carriedFoods.Clear();
        Changed?.Invoke();

        Debug.Log(
            "Food inventory cleared."
        );
    }
}