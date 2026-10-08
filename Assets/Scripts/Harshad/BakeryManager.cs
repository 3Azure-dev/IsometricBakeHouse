using System.Collections.Generic;
using UnityEngine;

public class BakeryOrderManager : MonoBehaviour
{
    public static BakeryOrderManager Instance { get; private set; }

    private List<BakeryOrder> orders =
        new List<BakeryOrder>();

    private int nextOrderID = 1;


    // --------------------------------------------------
    // AWAKE
    // --------------------------------------------------

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
    // CREATE ORDER
    // --------------------------------------------------

    public BakeryOrder CreateOrder(
        CustomerController customer,
        FoodType item)
    {
        BakeryOrder order =
            new BakeryOrder();

        order.orderID =
            nextOrderID++;

        order.customer =
            customer;

        order.item =
            item;

        order.isAccepted =
            false;

        orders.Add(order);

        Debug.Log(
            "Created Order #" +
            order.orderID +
            " for " +
            item
        );

        return order;
    }


    // --------------------------------------------------
    // GET CUSTOMER ORDER
    // --------------------------------------------------

    public BakeryOrder GetOrderForCustomer(
        CustomerController customer)
    {
        foreach (BakeryOrder order in orders)
        {
            if (order.customer == customer)
            {
                return order;
            }
        }

        return null;
    }


    // --------------------------------------------------
    // ACCEPT ORDER
    // --------------------------------------------------

    public void AcceptOrder(int orderID)
    {
        BakeryOrder order =
            GetOrder(orderID);

        if (order == null)
        {
            return;
        }

        if (order.isAccepted)
        {
            return;
        }

        order.isAccepted =
            true;

        Debug.Log(
            "Order #" +
            orderID +
            " accepted."
        );
    }


    // --------------------------------------------------
    // REJECT ORDER
    // --------------------------------------------------

    public void RejectOrder(int orderID)
    {
        BakeryOrder order =
            GetOrder(orderID);

        if (order == null)
        {
            return;
        }

        Debug.Log(
            "Order #" +
            orderID +
            " rejected."
        );

        orders.Remove(order);
    }


    // --------------------------------------------------
    // GET ORDER
    // --------------------------------------------------

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