using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderBoardUI : MonoBehaviour
{
    public static OrderBoardUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject orderPanel;
    [SerializeField] private TMP_Text customerNameText;
    [SerializeField] private TMP_Text orderText;

    [Header("Buttons")]
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button rejectButton;

    [Header("Order Timer (drag the OrderTimer object here)")]
    [SerializeField] private OrderTimer orderTimer;

    private CustomerController currentCustomer;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    private void Start()
    {
        orderPanel.SetActive(false);

        acceptButton.onClick.AddListener(AcceptOrder);
        rejectButton.onClick.AddListener(RejectOrder);
    }


    public void ShowOrder(CustomerController customer)
    {
        if (customer == null)
        {
            return;
        }

        if (currentCustomer != null && currentCustomer != customer)
        {
            currentCustomer.CancelOrdering();
        }

        currentCustomer = customer;

        customerNameText.text =
            customer.name;

        orderText.text =
            customer.GetOrderText();

        orderPanel.SetActive(true);

        acceptButton.gameObject.SetActive(true);
        rejectButton.gameObject.SetActive(true);

        Debug.Log(
            "Showing order from " +
            customer.name
        );
    }


    private void AcceptOrder()
    {
        if (currentCustomer == null)
        {
            return;
        }

        // Keep our own copy, because currentCustomer is cleared at the end
        CustomerController customer = currentCustomer;

        customer.OrderAccepted();

        // The order timer starts NOW, when the order is taken.
        // 180 sec for a Normal customer, 240 sec for a VIP (from SO_CustomerSettings).
        // We check IsWaitingForFood so the timer only starts if the order was really accepted.
        if (orderTimer != null && customer.IsWaitingForFood)
        {
            orderTimer.StartTimer(customer.GetOrderTimerSeconds());
        }

        Debug.Log(
            customer.name +
            "'s order was accepted."
        );

        orderPanel.SetActive(false);

        currentCustomer = null;
    }


    private void RejectOrder()
    {
        if (currentCustomer == null)
        {
            return;
        }

        currentCustomer.OrderRejected();

        Debug.Log(
            currentCustomer.name +
            "'s order was rejected."
        );

        orderPanel.SetActive(false);

        currentCustomer = null;
    }
}