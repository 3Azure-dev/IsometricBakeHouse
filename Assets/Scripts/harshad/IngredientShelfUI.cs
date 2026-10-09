
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientShelfUI : MonoBehaviour
{
    public static IngredientShelfUI Instance { get; private set; }

    [Header("UI Settings")]
    [SerializeField] private Vector2 panelSize = new Vector2(520, 600);
    [SerializeField] private Vector2 rowSize = new Vector2(440, 42);

    private GameObject panel;
    private Transform rowsContainer;
    private IngredientShelf currentShelf;

    private readonly Dictionary<IngredientType, TMP_Text> amountTexts =
        new Dictionary<IngredientType, TMP_Text>();

    private bool isOpen;

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
        CreateUI();
        Close();
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
            return;

        currentShelf = shelf;
        isOpen = true;

        panel.SetActive(true);
        RefreshAmounts();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Close()
    {
        isOpen = false;

        if (panel != null)
            panel.SetActive(false);

        currentShelf = null;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void CreateUI()
    {
        // Create a Canvas.
        GameObject canvasObject = new GameObject(
            "GeneratedShelfCanvas",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Create the panel.
        panel = CreateUIObject("ShelfPanel", canvas.transform);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.12f, 0.10f, 0.08f, 0.96f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = panelSize;
        panelRect.anchoredPosition = Vector2.zero;

        // Title.
        TMP_Text title = CreateText(
            "Title",
            panel.transform,
            "INGREDIENT SHELF",
            30
        );

        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.sizeDelta = new Vector2(-20, 50);
        titleRect.anchoredPosition = new Vector2(0, -12);

        // Scroll area.
        GameObject scrollObject = CreateUIObject(
            "IngredientScrollArea",
            panel.transform
        );

        RectTransform scrollRect = scrollObject.GetComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0, 0);
        scrollRect.anchorMax = new Vector2(1, 1);
        scrollRect.offsetMin = new Vector2(15, 70);
        scrollRect.offsetMax = new Vector2(-15, -75);

        ScrollRect scroll = scrollObject.AddComponent<ScrollRect>();
        scroll.horizontal = false;

        Image scrollImage = scrollObject.AddComponent<Image>();
        scrollImage.color = new Color(0.18f, 0.16f, 0.13f, 0.5f);

        GameObject viewport = CreateUIObject(
            "Viewport",
            scrollObject.transform
        );

        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = Color.white;

        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        scroll.viewport = viewportRect;

        GameObject content = CreateUIObject(
            "RowsContainer",
            viewport.transform
        );

        rowsContainer = content.transform;

        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout =
            content.AddComponent<VerticalLayoutGroup>();

        layout.spacing = 8;
        layout.padding = new RectOffset(8, 8, 8, 8);
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter =
            content.AddComponent<ContentSizeFitter>();

        fitter.verticalFit =
            ContentSizeFitter.FitMode.PreferredSize;

        scroll.content = contentRect;

        // Create one row for every ingredient enum value.
        foreach (IngredientType ingredient
                 in System.Enum.GetValues(typeof(IngredientType)))
        {
            CreateIngredientRow(ingredient);
        }

        // Close button.
        Button closeButton = CreateButton(
            "CloseButton",
            panel.transform,
            "CLOSE",
            new Vector2(120, 42)
        );

        RectTransform closeRect =
            closeButton.GetComponent<RectTransform>();

        closeRect.anchorMin = new Vector2(1, 0);
        closeRect.anchorMax = new Vector2(1, 0);
        closeRect.pivot = new Vector2(1, 0);
        closeRect.anchoredPosition = new Vector2(-15, 12);

        closeButton.onClick.AddListener(Close);
    }

    private void CreateIngredientRow(IngredientType ingredient)
    {
        GameObject row = CreateUIObject(
            ingredient + "Row",
            rowsContainer
        );

        Image rowImage = row.AddComponent<Image>();
        rowImage.color = new Color(0.28f, 0.24f, 0.19f, 1f);

        LayoutElement rowLayout = row.AddComponent<LayoutElement>();
        rowLayout.preferredHeight = rowSize.y;
        rowLayout.minHeight = rowSize.y;

        TMP_Text nameText = CreateText(
            "IngredientName",
            row.transform,
            ingredient.ToString(),
            20
        );

        RectTransform nameRect = nameText.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0);
        nameRect.anchorMax = new Vector2(0.48f, 1);
        nameRect.offsetMin = new Vector2(10, 0);
        nameRect.offsetMax = Vector2.zero;

        TMP_Text amountText = CreateText(
            "AmountText",
            row.transform,
            "0",
            20
        );

        RectTransform amountRect =
            amountText.GetComponent<RectTransform>();

        amountRect.anchorMin = new Vector2(0.48f, 0);
        amountRect.anchorMax = new Vector2(0.68f, 1);
        amountRect.offsetMin = Vector2.zero;
        amountRect.offsetMax = Vector2.zero;

        amountText.alignment = TextAlignmentOptions.Center;

        amountTexts[ingredient] = amountText;

        Button takeButton = CreateButton(
            "TakeButton",
            row.transform,
            "TAKE 1",
            new Vector2(100, 34)
        );

        RectTransform takeRect =
            takeButton.GetComponent<RectTransform>();

        takeRect.anchorMin = new Vector2(1, 0.5f);
        takeRect.anchorMax = new Vector2(1, 0.5f);
        takeRect.pivot = new Vector2(1, 0.5f);
        takeRect.anchoredPosition = new Vector2(-8, 0);

        takeButton.onClick.AddListener(
            () => TakeIngredient(ingredient)
        );
    }

    private void TakeIngredient(IngredientType ingredient)
    {
        if (currentShelf == null)
            return;

        if (currentShelf.TakeIngredient(ingredient, 1))
        {
            RefreshAmounts();
        }
    }

    private void RefreshAmounts()
    {
        if (currentShelf == null)
            return;

        foreach (IngredientType ingredient
                 in System.Enum.GetValues(typeof(IngredientType)))
        {
            if (amountTexts.TryGetValue(ingredient, out TMP_Text text))
            {
                text.text = currentShelf.GetAmount(ingredient).ToString();
            }
        }
    }

    private GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform)
        );

        obj.transform.SetParent(parent, false);
        return obj;
    }

    private TMP_Text CreateText(
        string objectName,
        Transform parent,
        string value,
        float fontSize)
    {
        GameObject obj = CreateUIObject(objectName, parent);

        TMP_Text text = obj.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.raycastTarget = false;

        return text;
    }

    private Button CreateButton(
        string objectName,
        Transform parent,
        string label,
        Vector2 size)
    {
        GameObject obj = CreateUIObject(objectName, parent);

        Image image = obj.AddComponent<Image>();
        image.color = new Color(0.45f, 0.31f, 0.18f, 1f);

        Button button = obj.AddComponent<Button>();
        button.targetGraphic = image;

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.45f, 0.31f, 0.18f);
        colors.highlightedColor = new Color(0.60f, 0.43f, 0.25f);
        colors.pressedColor = new Color(0.32f, 0.22f, 0.13f);
        button.colors = colors;

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = size;

        TMP_Text text = CreateText(
            "Label",
            obj.transform,
            label,
            16
        );

        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        text.alignment = TextAlignmentOptions.Center;

        return button;
    }
}
