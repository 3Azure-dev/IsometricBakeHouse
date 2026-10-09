
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientShelfUI : MonoBehaviour
{
    public static IngredientShelfUI Instance { get; private set; }

    [Serializable]
    public class IngredientRow
    {
        public IngredientType ingredient;
        public TMP_Text amountText;
        public Button takeButton;
    }

    [Header("Main UI")]
    [SerializeField] private GameObject shelfPanel;
    [SerializeField] private Button closeButton;

    [Header("Ingredient Rows")]
    [SerializeField]
    private List<IngredientRow> ingredientRows =
        new List<IngredientRow>();

    [Header("Optional UI")]
    [SerializeField] private TMP_Text messageText;

    [Header("Cursor")]
    [SerializeField] private bool restoreCursorOnClose = true;

    private IngredientShelf currentShelf;
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
        {
            closeButton.onClick.AddListener(Close);
        }

        foreach (IngredientRow row in ingredientRows)
        {
            if (row == null || row.takeButton == null)
                continue;

            IngredientType selectedIngredient = row.ingredient;

            row.takeButton.onClick.AddListener(() =>
            {
                TakeIngredient(selectedIngredient);
            });
        }
    }

    private void Start()
    {
        if (shelfPanel != null)
        {
            shelfPanel.SetActive(false);
        }

        isOpen = false;
    }

    private void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    public void Open(IngredientShelf shelf)
    {
        if (shelf == null)
        {
            Debug.LogError("IngredientShelfUI: Shelf reference is missing.");
            return;
        }

        if (shelfPanel == null)
        {
            Debug.LogError("IngredientShelfUI: Assign Shelf Panel in the Inspector.");
            return;
        }

        if (!isOpen)
        {
            previousCursorVisible = Cursor.visible;
            previousCursorLockState = Cursor.lockState;
        }

        currentShelf = shelf;
        isOpen = true;

        shelfPanel.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (messageText != null)
        {
            messageText.text = "";
        }

        RefreshAmounts();
    }

    public void Close()
    {
        if (shelfPanel != null)
        {
            shelfPanel.SetActive(false);
        }

        currentShelf = null;

        if (isOpen && restoreCursorOnClose)
        {
            Cursor.visible = previousCursorVisible;
            Cursor.lockState = previousCursorLockState;
        }

        isOpen = false;
    }

    private void TakeIngredient(IngredientType ingredient)
    {
        if (!isOpen || currentShelf == null)
            return;

        bool success = currentShelf.TakeIngredient(ingredient, 1);

        if (success)
        {
            if (messageText != null)
            {
                messageText.text = "Collected " + ingredient + ".";
            }
        }
        else
        {
            if (messageText != null)
            {
                messageText.text = "No " + ingredient + " available.";
            }
        }

        RefreshAmounts();
    }

    public void RefreshAmounts()
    {
        if (currentShelf == null)
            return;

        foreach (IngredientRow row in ingredientRows)
        {
            if (row == null)
                continue;

            int amount = currentShelf.GetAmount(row.ingredient);

            if (row.amountText != null)
            {
                row.amountText.text = amount.ToString();
            }

            if (row.takeButton != null)
            {
                row.takeButton.interactable = amount > 0;
            }
        }
    }

    private void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
