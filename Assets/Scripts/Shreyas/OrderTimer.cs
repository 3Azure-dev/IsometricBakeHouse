using TMPro;
using UnityEngine;
using UnityEngine.UI;

// SCRUM-529 Customer Ring.
// Goes on the customer's TimerCanvas (it must stay ACTIVE, so NOT on the ring itself).
// One ring + one icon above the customer. It reads the customer every frame:
//   Thinking                 -> grey dots, no ring
//   Waiting to take order    -> dish icon, ring empties over the patience time
//   Ordering (order board)   -> same dish, ring paused (the patience timer is paused too)
//   Waiting for food         -> same dish, ring empties over the food timer (StartTimer seconds: 180, VIP 240)
//   Right dish / wrong dish / timer ran out / walked out -> happy / sad / angry face for 2 sec, no ring
//   Walking, eating, leaving -> nothing
// VIP ring is always gold. Warning colours (orange at half, red at a quarter) are a switch, off by default.
public class OrderTimer : MonoBehaviour
{
    [Header("Ring parts (from the RingRoot you built)")]
    [SerializeField] private GameObject timerVisualRoot;   // RingRoot
    [SerializeField] private Image timerRing;               // TimerRing (Filled, Radial 360, Top)
    [SerializeField] private Image centerIcon;              // CenterIcon
    [SerializeField] private TMP_Text iconLetter;           // IconLetter

    [Header("Old number text (not used any more, hidden automatically)")]
    [SerializeField] private TMP_Text timerText;

    [Header("Optional Automatic Start (old feature)")]
    [SerializeField] private CustomerSettings settings;
    [SerializeField] private bool startAutomatically = false;

    [Header("Customer")]
    [Tooltip("Leave empty: it is found in the parents.")]
    [SerializeField] private CustomerController customer;
    [Tooltip("Turn on only if the ring does not face the camera and nothing else turns it.")]
    [SerializeField] private bool faceCamera = false;

    [Header("Ring colours")]
    [SerializeField] private Color ringColor = new Color(0.96f, 0.90f, 0.77f);      // cream
    [SerializeField] private Color vipRingColor = new Color(1f, 0.82f, 0.25f);      // gold, always for VIPs

    [Header("Warning colours (off by default, Pravin decides)")]
    [SerializeField] private bool useWarningColours = false;
    [SerializeField] private Color warningOrange = new Color(0.95f, 0.60f, 0.15f);  // at half
    [SerializeField] private Color warningRed = new Color(0.85f, 0.20f, 0.20f);     // at a quarter

    [Header("Reaction face")]
    [SerializeField] private float reactionSeconds = 2f;

    [Header("Whitebox icon colours")]
    [SerializeField] private Color thinkingColor = new Color(0.55f, 0.56f, 0.60f);
    [SerializeField] private Color breadColor = new Color(0.85f, 0.63f, 0.31f);
    [SerializeField] private Color cupcakeColor = new Color(0.91f, 0.54f, 0.69f);
    [SerializeField] private Color pizzaColor = new Color(0.91f, 0.38f, 0.24f);
    [SerializeField] private Color happyColor = new Color(0.24f, 0.67f, 0.31f);
    [SerializeField] private Color sadColor = new Color(0.94f, 0.63f, 0.19f);
    [SerializeField] private Color angryColor = new Color(0.85f, 0.23f, 0.23f);

    [Header("Real icons (drag the sprites in later, the letters disappear by themselves)")]
    [SerializeField] private Sprite thinkingSprite;
    [SerializeField] private Sprite breadSprite;
    [SerializeField] private Sprite cupcakeSprite;
    [SerializeField] private Sprite pizzaSprite;
    [SerializeField] private Sprite happySprite;
    [SerializeField] private Sprite sadSprite;
    [SerializeField] private Sprite angrySprite;

    [Header("Icon sizes")]
    [SerializeField] private float whiteboxIconSize = 58f;   // coloured disc with a letter
    [SerializeField] private float realIconSize = 46f;       // real sprite, stays inside the dark disc

    // food timer (started by CustomerController.OrderAccepted)
    private float timeLimit;
    private float timeLeft;
    private bool running;

    // reaction face
    private CustomerReaction lastReaction = CustomerReaction.None;
    private CustomerReaction shownReaction = CustomerReaction.None;
    private float reactionTimeLeft;

    private Sprite defaultIconSprite;
    private int shownKey = -1;
    private Camera cam;

    // --------------------------------------------------
    // SETUP
    // --------------------------------------------------

    private void Awake()
    {
        if (customer == null)
        {
            customer = GetComponentInParent<CustomerController>();
        }

        if (centerIcon != null)
        {
            defaultIconSprite = centerIcon.sprite;
        }

        // No numbers on the ring any more.
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }

        if (timerVisualRoot != null &&
            (timerVisualRoot == gameObject || transform.IsChildOf(timerVisualRoot.transform)))
        {
            Debug.LogError(
                name + ": OrderTimer is on (or inside) the ring. Put it on the canvas, " +
                "otherwise it switches itself off.", this);
        }

        HideTimer();
        SetRoot(false);
    }

    private void OnEnable()
    {
        lastReaction = customer != null ? customer.Reaction : CustomerReaction.None;
        reactionTimeLeft = 0f;
        shownKey = -1;
    }

    private void Start()
    {
        if (!startAutomatically)
            return;

        if (settings == null)
        {
            Debug.LogError(name + ": OrderTimer has no CustomerSettings assigned.");
            return;
        }

        StartTimer(settings.orderTimer);
    }

    // --------------------------------------------------
    // CALLED BY CustomerController (same as before)
    // --------------------------------------------------

    // The food timer: the player accepted the order.
    public void StartTimer(float seconds)
    {
        if (seconds <= 0f)
        {
            HideTimer();
            return;
        }

        timeLimit = seconds;
        timeLeft = seconds;
        running = true;
    }

    // Stops the food timer. The ring itself is shown or hidden by Update, so a reaction face can still show.
    public void HideTimer()
    {
        running = false;
        timeLeft = 0f;
    }

    // --------------------------------------------------
    // UPDATE
    // --------------------------------------------------

    private void Update()
    {
        if (running)
        {
            timeLeft = Mathf.Max(timeLeft - Time.deltaTime, 0f);

            if (timeLeft <= 0f)
            {
                running = false;
            }
        }

        if (customer == null)
        {
            SetRoot(false);
            return;
        }

        // A new reaction starts the face for a couple of seconds.
        CustomerReaction reaction = customer.Reaction;

        if (reaction != lastReaction)
        {
            lastReaction = reaction;

            if (reaction != CustomerReaction.None)
            {
                shownReaction = reaction;
                reactionTimeLeft = reactionSeconds;
            }
            else
            {
                reactionTimeLeft = 0f;
            }
        }

        if (reactionTimeLeft > 0f)
        {
            reactionTimeLeft -= Time.deltaTime;
            ShowReactionFace(shownReaction);
            SetRing(false, 0f);
            SetRoot(true);
            return;
        }

        // Waiting for food: ring empties over the food timer.
        if (running || customer.IsWaitingForFood)
        {
            ShowDish();
            SetRing(true, timeLimit > 0f ? timeLeft / timeLimit : 1f);
            SetRoot(true);
            return;
        }

        // Waiting to take the order (or ordering): ring empties over the patience time.
        // While ordering, the customer's patience timer does not move, so the ring is paused.
        if (customer.IsWaitingForPlayer || customer.IsOrdering)
        {
            float patience = customer.PatienceSeconds;
            float fill = patience > 0f ? 1f - Mathf.Clamp01(customer.timer / patience) : 1f;

            ShowDish();
            SetRing(true, fill);
            SetRoot(true);
            return;
        }

        if (customer.IsThinking)
        {
            ShowThinking();
            SetRing(false, 0f);
            SetRoot(true);
            return;
        }

        // Walking, eating, leaving: nothing.
        SetRoot(false);
    }

    private void LateUpdate()
    {
        if (!faceCamera)
            return;

        if (cam == null)
            cam = Camera.main;

        if (cam != null)
            transform.rotation = cam.transform.rotation;
    }

    // --------------------------------------------------
    // WHAT TO SHOW
    // --------------------------------------------------

    private void ShowThinking()
    {
        if (shownKey == 1)
            return;

        shownKey = 1;
        SetIcon("...", thinkingColor, thinkingSprite);
    }

    private void ShowDish()
    {
        FoodType food = customer.GetChosenFood();
        int key = 100 + (int)food;

        if (key == shownKey)
            return;

        shownKey = key;

        string foodName = food.ToString();
        string letter = foodName.Length > 0 ? foodName.Substring(0, 1).ToUpperInvariant() : "?";

        switch (foodName.ToLowerInvariant())
        {
            case "bread":
                SetIcon(letter, breadColor, breadSprite);
                break;

            case "cupcake":
                SetIcon(letter, cupcakeColor, cupcakeSprite);
                break;

            case "pizza":
                SetIcon(letter, pizzaColor, pizzaSprite);
                break;

            default:
                SetIcon(letter, thinkingColor, null);
                break;
        }
    }

    private void ShowReactionFace(CustomerReaction reaction)
    {
        int key = 10 + (int)reaction;

        if (key == shownKey)
            return;

        shownKey = key;

        switch (reaction)
        {
            case CustomerReaction.Satisfied:
                SetIcon(":)", happyColor, happySprite);
                break;

            case CustomerReaction.Disappointed:
                SetIcon(":(", sadColor, sadSprite);
                break;

            default:   // Furious and WalkedOut both show the angry face
                SetIcon(">:(", angryColor, angrySprite);
                break;
        }
    }

    // A real sprite replaces the coloured disc and hides the letter.
    private void SetIcon(string letter, Color discColor, Sprite sprite)
    {
        if (centerIcon != null)
        {
            float size = sprite != null ? realIconSize : whiteboxIconSize;
            centerIcon.rectTransform.sizeDelta = new Vector2(size, size);
            centerIcon.rectTransform.anchoredPosition = Vector2.zero;

            if (sprite != null)
            {
                centerIcon.sprite = sprite;
                centerIcon.color = Color.white;
                centerIcon.preserveAspect = true;
            }
            else
            {
                centerIcon.sprite = defaultIconSprite;
                centerIcon.color = discColor;
            }
        }

        if (iconLetter != null)
        {
            iconLetter.gameObject.SetActive(sprite == null);
            iconLetter.text = letter;
        }
    }

    private void SetRing(bool visible, float fill)
    {
        if (timerRing == null)
            return;

        if (timerRing.gameObject.activeSelf != visible)
        {
            timerRing.gameObject.SetActive(visible);
        }

        if (!visible)
            return;

        fill = Mathf.Clamp01(fill);
        timerRing.fillAmount = fill;

        Color color = ringColor;

        if (customer != null && customer.IsVIP)
        {
            color = vipRingColor;   // a VIP ring is always gold
        }
        else if (useWarningColours)
        {
            if (fill <= 0.25f)
                color = warningRed;
            else if (fill <= 0.5f)
                color = warningOrange;
        }

        timerRing.color = color;
    }

    private void SetRoot(bool visible)
    {
        if (timerVisualRoot != null && timerVisualRoot.activeSelf != visible)
        {
            timerVisualRoot.SetActive(visible);
        }
    }
}