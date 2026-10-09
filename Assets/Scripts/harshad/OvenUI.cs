
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OvenUI : MonoBehaviour
{
    public static OvenUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject ovenPanel;
    [SerializeField] private Transform recipeButtonContainer;
    [SerializeField] private Button recipeButtonPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text statusText;

    private OvenController currentOven;
    private bool isOpen;

    private bool previousCursorVisible;
    private CursorLockMode previousCursorLockState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    private void Start()
    {
        if (ovenPanel != null)
            ovenPanel.SetActive(false);
    }

    private void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }


    public void Open(OvenController oven)
    {
        if (oven == null || ovenPanel == null)
            return;

        UnsubscribeFromOven();

        if (!isOpen)
        {
            previousCursorVisible = Cursor.visible;
            previousCursorLockState = Cursor.lockState;
        }

        currentOven = oven;
        currentOven.OnStatusChanged += UpdateStatus;
        currentOven.OnCookingStateChanged += UpdateCookingState;

        isOpen = true;
        ovenPanel.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (statusText != null)
        {
            statusText.text = oven.IsCooking
                ? "The oven is cooking " + oven.CurrentFood + "."
                : "Select a recipe.";
        }

        RefreshRecipeButtons();
    }


    public void Close()
    {
        if (ovenPanel != null)
            ovenPanel.SetActive(false);

        currentOven = null;

        if (isOpen)
        {
            Cursor.visible = previousCursorVisible;
            Cursor.lockState = previousCursorLockState;
        }

        isOpen = false;
    }

    private void RefreshRecipeButtons()
    {
        if (currentOven == null ||
            recipeButtonContainer == null ||
            recipeButtonPrefab == null)
        {
            Debug.LogError(
                "OvenUI is missing a recipe container or button prefab."
            );

            return;
        }

        // Remove old recipe buttons before rebuilding the list.
        for (int i = recipeButtonContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = recipeButtonContainer.GetChild(i);

            if (child.gameObject != recipeButtonPrefab.gameObject)
                Destroy(child.gameObject);
        }

        foreach (FoodType food in Enum.GetValues(typeof(FoodType)))
        {
            if (!currentOven.CanCook(food))
                continue;

            Recipe recipe = RecipeManager.Instance.GetRecipe(food);

            Button button = Instantiate(
                recipeButtonPrefab,
                recipeButtonContainer
            );

            button.gameObject.SetActive(true);

            TMP_Text label = button.GetComponentInChildren<TMP_Text>();

            if (label != null)
            {
                label.text = food + " (" +
                    recipe.cookingTime.ToString("0.#") + " sec)";
            }

            FoodType selectedFood = food;

            button.onClick.AddListener(() =>
            {
                if (currentOven == null)
                    return;

                currentOven.TryStartCooking(selectedFood);
            });
        }
    }

    private void OnEnable()
    {
        if (OvenControllerReferenceIsValid())
        {
            currentOven.OnStatusChanged += UpdateStatus;
            currentOven.OnCookingStateChanged += UpdateCookingState;
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromOven();
    }

    private bool OvenControllerReferenceIsValid()
    {
        return currentOven != null;
    }

    private void UpdateStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    private void UpdateCookingState()
    {
        if (isOpen && currentOven != null)
        {
            RefreshRecipeButtons();
        }
    }

    private void UnsubscribeFromOven()
    {
        if (currentOven != null)
        {
            currentOven.OnStatusChanged -= UpdateStatus;
            currentOven.OnCookingStateChanged -= UpdateCookingState;
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromOven();

        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);

        if (Instance == this)
            Instance = null;
    }
}
