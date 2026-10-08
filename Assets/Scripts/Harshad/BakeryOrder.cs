using System.Collections.Generic;

public class BakeryOrder
{
    public int orderID;

    public CustomerController customer;

    public List<FoodType> items =
        new List<FoodType>();

    public bool isAccepted;

    public bool isCompleted;
}