using System.Collections.Generic;
using UnityEngine;

public class BakeryOrderManager : MonoBehaviour
{
    public static BakeryOrderManager Instance { get; private set; }

    private List<BakeryOrder> orders =
        new List<BakeryOrder>();

    private int nextOrderID = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public BakeryOrder CreateOrder(
        CustomerController customer,
        List<FoodType> items)
    {
        BakeryOrder order =
            new BakeryOrder();

        order.orderID = nextOrderID++;

        order.customer = customer;

        order.items =
            new List<FoodType>(items);

        order.isAccepted = false;

        order.isCompleted = false;

        orders.Add(order);

        Debug.Log(
            "Created Order #" +
            order.orderID
        );

        return order;
    }

    public BakeryOrder GetOrderForCustomer(
        CustomerController customer)
    {
        foreach (BakeryOrder order in orders)
        {
            if (order.customer == customer &&
                !order.isCompleted)
            {
                return order;
            }
        }

        return null;
    }

    public void AcceptOrder(int orderID)
    {
        BakeryOrder order =
            GetOrder(orderID);

        if (order == null)
            return;

        order.isAccepted = true;

        Debug.Log(
            "Order #" +
            orderID +
            " accepted."
        );

        if (order.customer != null)
        {
            order.customer.OrderAccepted();
        }
    }

    public void RejectOrder(int orderID)
    {
        BakeryOrder order =
            GetOrder(orderID);

        if (order == null)
            return;

        Debug.Log(
            "Order #" +
            orderID +
            " rejected."
        );

        if (order.customer != null)
        {
            order.customer.OrderRejected();
        }

        orders.Remove(order);
    }

    public void CompleteOrder(int orderID)
    {
        BakeryOrder order =
            GetOrder(orderID);

        if (order == null)
            return;

        if (!order.isAccepted)
            return;

        order.isCompleted = true;

        Debug.Log(
            "Order #" +
            orderID +
            " completed."
        );

        if (order.customer != null)
        {
            order.customer.OrderCompleted();
        }
    }

    private BakeryOrder GetOrder(int orderID)
    {
        foreach (BakeryOrder order in orders)
        {
            if (order.orderID == orderID)
            {
                return order;
            }
        }

        return null;
    }
}