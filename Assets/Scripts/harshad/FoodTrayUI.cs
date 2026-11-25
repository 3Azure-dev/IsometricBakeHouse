using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Shows the dishes the player is carrying (SCRUM-720).
// One slot per kind of dish, with a number under the icon when there is more than one.
// It builds its own UI in code and is created automatically when the Bakery loads.
// Icons are read from Assets/Resources/FoodIcons/ (Bread, Cupcake, Pizza, Burger, Plate).
// A missing icon shows the dish name instead.
public class FoodTrayUI : MonoBehaviour
{
    private const string BakerySceneName = "Bakery";
    private const int SlotCount = 4;          // there are 4 kinds of dish at most
    private const float SlotSize = 100f;
    private const float Spacing = 10f;
    private const float Margin = 40f;
    private const float CountHeight = 30f;    // space for the number under the icon

    private GameObject[] slotRoots;
    private Image[] icons;
    private TMP_Text[] labels;   // dish name, only when there is no icon
    private TMP_Text[] counts;   // "x2", "x3" ... under the icon
    private PlayerFoodInventory subscribed;

    // ---------- auto-create in the Bakery ----------

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Hook()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != BakerySceneName) return;
        if (FindFirstObjectByType<FoodTrayUI>() != null) return;

        new GameObject("FoodTray", typeof(FoodTrayUI));
    }

    // ---------- build ----------

    private void Awake()
    {
        BuildUI();
    }

    private void BuildUI()
    {
        // Canvas
        GameObject canvasGO = new GameObject("FoodTrayCanvas",
            typeof(Canvas), typeof(CanvasScaler));
        canvasGO.transform.SetParent(transform, false);

        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5;

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // Row at the bottom-left
        GameObject rowGO = new GameObject("Tray", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        rowGO.transform.SetParent(canvasGO.transform, false);

        RectTransform row = rowGO.GetComponent<RectTransform>();
        row.anchorMin = Vector2.zero;
        row.anchorMax = Vector2.zero;
        row.pivot = Vector2.zero;
        row.anchoredPosition = new Vector2(Margin, Margin);
        row.sizeDelta = new Vector2(SlotCount * (SlotSize + Spacing), SlotSize);

        HorizontalLayoutGroup layout = rowGO.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = Spacing;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        Sprite plate = Resources.Load<Sprite>("FoodIcons/Plate");

        slotRoots = new GameObject[SlotCount];
        icons = new Image[SlotCount];
        labels = new TMP_Text[SlotCount];
        counts = new TMP_Text[SlotCount];

        for (int i = 0; i < SlotCount; i++)
        {
            // Slot background
            GameObject slot = new GameObject("Slot" + (i + 1), typeof(RectTransform), typeof(Image));
            slot.transform.SetParent(rowGO.transform, false);
            slot.GetComponent<RectTransform>().sizeDelta = new Vector2(SlotSize, SlotSize);

            Image bg = slot.GetComponent<Image>();
            bg.raycastTarget = false;
            if (plate != null) { bg.sprite = plate; bg.preserveAspect = true; }
            else bg.color = new Color(0f, 0f, 0f, 0.45f);

            // Icon (leaves room at the bottom for the number)
            GameObject iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(slot.transform, false);
            RectTransform iconRT = iconGO.GetComponent<RectTransform>();
            iconRT.anchorMin = Vector2.zero;
            iconRT.anchorMax = Vector2.one;
            iconRT.offsetMin = new Vector2(10f, CountHeight);
            iconRT.offsetMax = new Vector2(-10f, -8f);

            Image icon = iconGO.GetComponent<Image>();
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            // Dish name (shown only when there is no icon)
            GameObject labelGO = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGO.transform.SetParent(slot.transform, false);
            RectTransform labelRT = labelGO.GetComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = new Vector2(4f, CountHeight);
            labelRT.offsetMax = new Vector2(-4f, -4f);

            TMP_Text label = labelGO.GetComponent<TMP_Text>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 22f;
            label.color = Color.white;
            label.raycastTarget = false;

            // Number under the icon
            GameObject countGO = new GameObject("Count", typeof(RectTransform), typeof(TextMeshProUGUI));
            countGO.transform.SetParent(slot.transform, false);
            RectTransform countRT = countGO.GetComponent<RectTransform>();
            countRT.anchorMin = new Vector2(0f, 0f);
            countRT.anchorMax = new Vector2(1f, 0f);
            countRT.pivot = new Vector2(0.5f, 0f);
            countRT.offsetMin = new Vector2(0f, 2f);
            countRT.offsetMax = new Vector2(0f, CountHeight);

            TMP_Text count = countGO.GetComponent<TMP_Text>();
            count.alignment = TextAlignmentOptions.Center;
            count.fontSize = 24f;
            count.fontStyle = FontStyles.Bold;
            count.color = Color.white;
            count.raycastTarget = false;

            slotRoots[i] = slot;
            icons[i] = icon;
            labels[i] = label;
            counts[i] = count;
        }

        Refresh();
    }

    // ---------- listen to the inventory ----------

    private void Update()
    {
        // The inventory may wake up after this UI, so keep trying until it exists
        if (subscribed == null && PlayerFoodInventory.Instance != null)
        {
            subscribed = PlayerFoodInventory.Instance;
            subscribed.Changed += Refresh;
            Refresh();
        }
    }

    private void OnDestroy()
    {
        if (subscribed != null)
            subscribed.Changed -= Refresh;
    }

    private void Refresh()
    {
        if (slotRoots == null) return;

        List<FoodType> foods = subscribed != null
            ? subscribed.GetCarriedFoods()
            : new List<FoodType>();

        // Group the same dishes together: Bread, Bread, Pizza -> Bread x2, Pizza x1
        List<FoodType> order = new List<FoodType>();
        Dictionary<FoodType, int> amounts = new Dictionary<FoodType, int>();

        foreach (FoodType food in foods)
        {
            if (!amounts.ContainsKey(food))
            {
                amounts[food] = 0;
                order.Add(food);
            }
            amounts[food]++;
        }

        for (int i = 0; i < SlotCount; i++)
        {
            bool has = i < order.Count;
            slotRoots[i].SetActive(has);

            if (!has) continue;

            FoodType food = order[i];
            int amount = amounts[food];

            Sprite sprite = Resources.Load<Sprite>("FoodIcons/" + food);

            icons[i].sprite = sprite;
            icons[i].enabled = sprite != null;
            labels[i].text = sprite != null ? "" : food.ToString();
            counts[i].text = amount > 1 ? "x" + amount : "";
        }
    }
}