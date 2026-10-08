
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
        orderPanel.SetActive(true);

        acceptButton.onClick.AddListener(AcceptOrder);
        rejectButton.onClick.AddListener(RejectOrder);
    }


    public void ShowOrder(CustomerController customer)
    {
        if (customer == null)
        {
            return;
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

        currentCustomer.OrderAccepted();

        Debug.Log(
            currentCustomer.name +
            "'s order was accepted."
        );

        orderPanel.SetActive(true);

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

        orderPanel.SetActive(true);

        currentCustomer = null;
    }
}
