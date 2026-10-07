using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderBoardUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject orderPanel;
    [SerializeField] private TMP_Text customerNameText;
    [SerializeField] private TMP_Text orderText;

    [Header("Buttons")]
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button rejectButton;
    [SerializeField] private Button completeButton;

    private BakeryOrder currentOrder;

    private void Start()
    {
        orderPanel.SetActive(true);

        acceptButton.onClick.AddListener(AcceptOrder);
        rejectButton.onClick.AddListener(RejectOrder);
        completeButton.onClick.AddListener(CompleteOrder);
    }

    private void Update()
    {
        if (currentOrder == null)
        {
            CheckForNewOrder();
        }
    }

    private void CheckForNewOrder()
    {
        CustomerController customer =
            FindOrderingCustomer();

        if (customer == null)
            return;

        BakeryOrder order =
            BakeryOrderManager.Instance
            .GetOrderForCustomer(customer);

        if (order == null)
            return;

        if (order.isAccepted)
            return;

        ShowNewOrder(order);
    }

    private CustomerController FindOrderingCustomer()
    {
        CustomerController[] customers =
            FindObjectsOfType<CustomerController>();

        foreach (CustomerController customer in customers)
        {
            BakeryOrder order =
                BakeryOrderManager.Instance
                .GetOrderForCustomer(customer);

            if (order != null &&
                !order.isAccepted)
            {
                return customer;
            }
        }

        return null;
    }

    private void ShowNewOrder(BakeryOrder order)
    {
        currentOrder = order;

        orderPanel.SetActive(true);

        customerNameText.text =
            order.customer.name;

        orderText.text =
            GetOrderText(order);

        acceptButton.gameObject.SetActive(true);
        rejectButton.gameObject.SetActive(true);
        completeButton.gameObject.SetActive(true);
    }

    private void AcceptOrder()
    {
        if (currentOrder == null)
            return;

        BakeryOrderManager.Instance
            .AcceptOrder(currentOrder.orderID);

        acceptButton.gameObject.SetActive(true);
        rejectButton.gameObject.SetActive(true);
        completeButton.gameObject.SetActive(true);

        Debug.Log(
            "Order accepted. Prepare the food."
        );
    }

    private void RejectOrder()
    {
        if (currentOrder == null)
            return;

        BakeryOrderManager.Instance
            .RejectOrder(currentOrder.orderID);

        currentOrder = null;

        orderPanel.SetActive(true);
    }

    private void CompleteOrder()
    {
        if (currentOrder == null)
            return;

        BakeryOrderManager.Instance
            .CompleteOrder(currentOrder.orderID);

        currentOrder = null;

        orderPanel.SetActive(true);
    }
        
    private string GetOrderText(BakeryOrder order)
    {
        string text = "";

        for (int i = 0; i < order.items.Count; i++)
        {
            text += order.items[i].ToString();

            if (i < order.items.Count - 1)
            {
                text += "\n";
            }
        }

        return text;
    }
}