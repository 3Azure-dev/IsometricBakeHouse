using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoveUI : MonoBehaviour
{
    public static StoveUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject stovePanel;

    [SerializeField] private TMP_Text titleText;

    [Header("Food Buttons")]
    [SerializeField] private Button breadButton;
    [SerializeField] private Button cupcakeButton;
    [SerializeField] private Button pizzaButton;
    [SerializeField] private Button burgerButton;

    [Header("Close Button")]
    [SerializeField] private Button closeButton;


    private StoveController currentStove;


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
        stovePanel.SetActive(false);

        breadButton.onClick.AddListener(
            CookBread
        );

        cupcakeButton.onClick.AddListener(
            CookCupcake
        );

        pizzaButton.onClick.AddListener(
            CookPizza
        );

        burgerButton.onClick.AddListener(
            CookBurger
        );

        closeButton.onClick.AddListener(
            CloseUI
        );
    }


    // --------------------------------------------------
    // OPEN UI
    // --------------------------------------------------

    public void Open(StoveController stove)
    {
        if (stove == null)
        {
            return;
        }

        currentStove = stove;

        titleText.text =
            "Choose Food";

        stovePanel.SetActive(true);

        Debug.Log(
            "Stove UI opened."
        );
    }


    // --------------------------------------------------
    // COOK BREAD
    // --------------------------------------------------

    private void CookBread()
    {
        SelectFood(FoodType.Bread);
    }


    // --------------------------------------------------
    // COOK CUPCAKE
    // --------------------------------------------------

    private void CookCupcake()
    {
        SelectFood(FoodType.Cupcake);
    }


    // --------------------------------------------------
    // COOK PIZZA
    // --------------------------------------------------

    private void CookPizza()
    {
        SelectFood(FoodType.Pizza);
    }


    // --------------------------------------------------
    // COOK BURGER
    // --------------------------------------------------

    private void CookBurger()
    {
        SelectFood(FoodType.Burger);
    }


    // --------------------------------------------------
    // SELECT FOOD
    // --------------------------------------------------

    private void SelectFood(FoodType food)
    {
        if (currentStove == null)
        {
            return;
        }

        currentStove.StartCooking(
            food
        );

        CloseUI();
    }


    // --------------------------------------------------
    // CLOSE UI
    // --------------------------------------------------

    private void CloseUI()
    {
        stovePanel.SetActive(false);

        currentStove = null;

        Debug.Log(
            "Stove UI closed."
        );
    }
}