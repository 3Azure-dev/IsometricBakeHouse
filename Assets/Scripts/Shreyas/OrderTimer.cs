using TMPro;
using UnityEngine;
using UnityEngine.UI;

// SCRUM-529 Customer Ring.
// Goes on the customer root or TimerCanvas (it must stay ACTIVE, so NOT on the ring itself).
// One ring + one icon above the customer. It reads the customer every frame:
//   Thinking                   -> ICON_Thinking ("..."), no ring
//   Decided (waiting for you)  -> ICON_Idea (light bulb), POPS in, ring empties over the patience time
//   Order board open           -> still the light bulb, ring paused (the patience timer is paused too)
//   Order ACCEPTED, food wait  -> the recipe icon (bread / cupcake / pizza), POPS in,
//                                 ring empties over the food timer (StartTimer seconds: 180, VIP 240)
//   Right dish / wrong dish / timer ran out / walked out -> happy / sad / angry face for 2 sec, no ring
//   Walking, eating, leaving   -> nothing
// So the recipe icon only shows for orders the player has accepted.
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
    [Tooltip("Optional: the canvas to turn towards the camera (e.g. OrderBubbwidget). Leave empty if something else already does it. Never put the customer itself here.")]
    [SerializeField] private Transform faceCameraTarget;

    [Header("Ring colours")]
    [SerializeField] private Color ringColor = new Color(0.96f, 0.90f, 0.77f);      // cream
    [SerializeField] private Color vipRingColor = new Color(1f, 0.82f, 0.25f);      // gold, always for VIPs

    [Header("Warning colours (off by default, Pravin decides)")]
    [SerializeField] private bool useWarningColours = false;
    [SerializeField] private Color warningOrange = new Color(0.95f, 0.60f, 0.15f);  // at half
    [SerializeField] private Color warningRed = new Color(0.85f, 0.20f, 0.20f);     // at a quarter

    [Header("Reaction face")]
    [SerializeField] private float reactionSeconds = 2f;

    [Header("Pop transition (light bulb and recipe icon)")]
    [Tooltip("Scale 0 -> a bit bigger than normal -> normal. About 0.2 to 0.3 seconds.")]
    [SerializeField] private float popDuration = 0.25f;
    [SerializeField] private float popOvershoot = 1.2f;

    [Header("Whitebox icon colours")]
    [SerializeField] private Color thinkingColor = new Color(0.55f, 0.56f, 0.60f);
    [SerializeField] private Color ideaColor = new Color(1f, 0.82f, 0.25f);
    [SerializeField] private Color breadColor = new Color(0.85f, 0.63f, 0.31f);
    [SerializeField] private Color cupcakeColor = new Color(0.91f, 0.54f, 0.69f);
    [SerializeField] private Color pizzaColor = new Color(0.91f, 0.38f, 0.24f);
    [SerializeField] private Color happyColor = new Color(0.24f, 0.67f, 0.31f);
    [SerializeField] private Color sadColor = new Color(0.94f, 0.63f, 0.19f);
    [SerializeField] private Color angryColor = new Color(0.85f, 0.23f, 0.23f);

    [Header("Icon sizes")]
    [SerializeField] private float whiteboxIconSize = 58f;   // coloured disc with a letter
    [SerializeField] private float realIconSize = 46f;       // real sprite, stays inside the dark disc

    [Header("Real icons (drag the sprites in, the letters disappear by themselves)")]
    [SerializeField] private Sprite thinkingSprite;   // ICON_Thinking
    [SerializeField] private Sprite ideaSprite;       // ICON_Idea (light bulb)
    [SerializeField] private Sprite breadSprite;
    [SerializeField] private Sprite cupcakeSprite;
    [SerializeField] private Sprite pizzaSprite;
    [SerializeField] private Sprite happySprite;
    [SerializeField] private Sprite sadSprite;
    [SerializeField] private Sprite angrySprite;

    // food timer (started by CustomerController.OrderAccepted)
    private float timeLimit;
    private float timeLeft;
    private bool running;

    // reaction face
    private CustomerReaction lastReaction = CustomerReaction.None;
    private CustomerReaction shownReaction = CustomerReaction.None;
    private float reactionTimeLeft;

    // pop transition (1 = finished)
    private float popProgress = 1f;
    private Vector3 iconBaseScale = Vector3.one;
    private Vector3 letterBaseScale = Vector3.one;

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
            iconBaseScale = centerIcon.transform.localScale;
        }

        if (iconLetter != null)
        {
            letterBaseScale = iconLetter.transform.localScale;
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
                name + ": OrderTimer is on (or inside) the ring. Put it on the canvas or the customer, " +
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
        popProgress = 1f;
        ApplyIconScale(1f);
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

        // SetRoot(true) comes first in each branch: when the ring appears again, the icon pops in again.
        if (reactionTimeLeft > 0f)
        {
            reactionTimeLeft -= Time.deltaTime;

            SetRoot(true);
            ShowReactionFace(shownReaction);
            SetRing(false, 0f);
        }
        else if (running || customer.IsWaitingForFood)
        {
            // Order ACCEPTED: the recipe icon, ring empties over the food timer.
            SetRoot(true);
            ShowDish();
            SetRing(true, timeLimit > 0f ? timeLeft / timeLimit : 1f);
        }
        else if (customer.IsWaitingForPlayer || customer.IsOrdering)
        {
            // Customer DECIDED, not accepted yet: the light bulb, ring empties over the patience time.
            // While the order board is open the patience timer does not move, so the ring is paused.
            float patience = customer.PatienceSeconds;
            float fill = patience > 0f ? 1f - Mathf.Clamp01(customer.timer / patience) : 1f;

            SetRoot(true);
            ShowIdea();
            SetRing(true, fill);
        }
        else if (customer.IsThinking)
        {
            SetRoot(true);
            ShowThinking();
            SetRing(false, 0f);
        }
        else
        {
            // Walking, eating, leaving: nothing.
            SetRoot(false);
        }

        UpdatePop();
    }

    private void LateUpdate()
    {
        if (faceCameraTarget == null)
            return;

        if (cam == null)
            cam = Camera.main;

        if (cam != null)
            faceCameraTarget.rotation = cam.transform.rotation;
    }

    // --------------------------------------------------
    // WHAT TO SHOW
    // --------------------------------------------------

    private void ShowThinking()
    {
        if (shownKey == 1)
            return;

        shownKey = 1;
        SetIcon("...", thinkingColor, thinkingSprite, false);
    }

    private void ShowIdea()
    {
        if (shownKey == 2)
            return;

        shownKey = 2;
        SetIcon("!", ideaColor, ideaSprite, true);   // light bulb pops in
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

        // the recipe icon pops in
        switch (foodName.ToLowerInvariant())
        {
            case "bread":
                SetIcon(letter, breadColor, breadSprite, true);
                break;

            case "cupcake":
                SetIcon(letter, cupcakeColor, cupcakeSprite, true);
                break;

            case "pizza":
                SetIcon(letter, pizzaColor, pizzaSprite, true);
                break;

            default:
                SetIcon(letter, thinkingColor, null, true);
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
                SetIcon(":)", happyColor, happySprite, false);
                break;

            case CustomerReaction.Disappointed:
                SetIcon(":(", sadColor, sadSprite, false);
                break;

            default:   // Furious and WalkedOut both show the angry face
                SetIcon(">:(", angryColor, angrySprite, false);
                break;
        }
    }

    // A real sprite replaces the coloured disc and hides the letter.
    private void SetIcon(string letter, Color discColor, Sprite sprite, bool pop)
    {
        if (centerIcon != null)
        {
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

        if (pop)
        {
            popProgress = 0f;
            ApplyIconScale(0f);   // start at 0 right now, so the new icon never flashes at full size
        }
        else
        {
            popProgress = 1f;
            ApplyIconScale(1f);   // an icon without a pop must never stay half-scaled from an earlier pop
        }
    }

    // --------------------------------------------------
    // POP: scale 0 -> popOvershoot -> 1
    // --------------------------------------------------

    private void UpdatePop()
    {
        if (popProgress >= 1f)
            return;

        popProgress = Mathf.Min(1f, popProgress + (popDuration > 0f ? Time.deltaTime / popDuration : 1f));

        float k = popProgress;
        float s;

        if (k < 0.6f)
        {
            float t = k / 0.6f;
            s = popOvershoot * (1f - (1f - t) * (1f - t));      // grow fast, a bit past normal size
        }
        else
        {
            s = Mathf.Lerp(popOvershoot, 1f, (k - 0.6f) / 0.4f); // settle back to normal size
        }

        ApplyIconScale(s);
    }

    private void ApplyIconScale(float s)
    {
        if (centerIcon != null)
            centerIcon.transform.localScale = iconBaseScale * s;

        if (iconLetter != null)
            iconLetter.transform.localScale = letterBaseScale * s;
    }

    // --------------------------------------------------
    // RING
    // --------------------------------------------------

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
        if (timerVisualRoot == null || timerVisualRoot.activeSelf == visible)
            return;

        timerVisualRoot.SetActive(visible);

        if (visible)
        {
            shownKey = -1;   // the ring appears again: the icon pops in again
        }
    }
}