using System.Collections.Generic;
using UnityEngine;

public class PlayerFoodInventory : MonoBehaviour
{
    public static PlayerFoodInventory Instance { get; private set; }

    [Header("Inventory")]
    [SerializeField] private int maximumFoodCapacity = 4;

    private List<FoodType> carriedFoods =
        new List<FoodType>();


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

        Debug.Log(
            "Food inventory cleared."
        );
    }
}