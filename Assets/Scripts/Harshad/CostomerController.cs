using UnityEngine;
using UnityEngine.AI;

public class CustomerController : MonoBehaviour
{
    [Header("Data assets (drag them into the prefab)")]
    [SerializeField] private CustomerSettings settings;
    [SerializeField] private RecipeDatabase recipeDatabase;

    [Header("UI")]
    [SerializeField] private ThinkingBubble thinkingBubble;

    public CustomerType customerType;
    public float moveSpeed = 5f;
    public float timer;

    private BakeryOrder myOrder;
    private NavMeshAgent agent;
    private Transform mySeat;
    private CustomerState state;

    // My place in the door line (set by the CustomerManager)
    private Vector3 doorTarget;

    // Picked randomly between thinkingTimeMin and thinkingTimeMax each time a customer sits down
    private float currentThinkingTime;

    // The ONE food this customer wants
    private FoodType chosenFood;

    // --------------------------------------------------
    // INFO OTHER SCRIPTS CAN ASK FOR
    // --------------------------------------------------

    public bool IsVIP
    {
        get { return customerType == CustomerType.VIP; }
    }

    // True once the order has really been accepted (the order board uses this to start the timer)
    public bool IsWaitingForFood
    {
        get { return state == CustomerState.WaitingForFood; }
    }

    // True when pressing E on this customer would do something (take the order or serve the food)
    public bool CanInteractWithPlayer
    {
        get { return state == CustomerState.WaitingForPlayer || state == CustomerState.WaitingForFood; }
    }
    // How many seconds the order timer should run for this customer
    // (180 for Normal, 240 for VIP, both from SO_CustomerSettings)
    public float GetOrderTimerSeconds()
    {
        if (settings == null)
        {
            return 0f;
        }

        return IsVIP ? settings.vipOrderTimer : settings.orderTimer;
    }

    private void Awake()
    {
        if (settings == null)
        {
            Debug.LogError(name + " has no CustomerSettings assigned. Drag SO_CustomerSettings into the prefab.");
        }
    }

    // --------------------------------------------------
    // START CUSTOMER
    // --------------------------------------------------

    public void StartCustomer(CustomerType type)
    {
        agent = GetComponent<NavMeshAgent>();

        customerType = type;

        ResetCustomer();

        if (agent != null)
        {
            agent.Warp(transform.position);
        }

        if (CustomerManager.Instance == null)
        {
            Debug.LogError("CustomerManager.Instance was not found.");
            ReturnToPool();
            return;
        }

        // Get in line at the door. The spawner only spawns when there is room.
        if (!CustomerManager.Instance.JoinDoorQueue(this))
        {
            Debug.LogWarning(name + ": the door is full. Going back to the pool.");
            ReturnToPool();
            return;
        }

        Debug.Log(name + " spawned as " + customerType + " and is walking to the door.");
    }

    // --------------------------------------------------
    // CALLED BY THE CUSTOMER MANAGER
    // --------------------------------------------------

    // "This is your place in the door line" (also used when the line moves forward)
    public void SetDoorTarget(Vector3 position)
    {
        if (state != CustomerState.GoingToDoor && state != CustomerState.WaitingAtDoor)
        {
            return;
        }

        doorTarget = position;

        // Forget the old path so the agent walks to the new place
        if (agent != null)
        {
            agent.ResetPath();
        }

        state = CustomerState.GoingToDoor;
    }

    // "A seat at a clear table is free. Walk in and sit."
    public void GoToSeat(Transform seat)
    {
        mySeat = seat;

        if (agent != null)
        {
            agent.ResetPath();
        }

        state = CustomerState.GoingToSeat;

        Debug.Log(name + " leaves the door and walks to a seat.");
    }

    // The day ended: customers still at the door walk away.
    // Customers who are already seated or eating are left alone.
    public void LeaveBecauseDayEnded()
    {
        if (state != CustomerState.GoingToDoor && state != CustomerState.WaitingAtDoor)
        {
            return;
        }

        if (agent != null)
        {
            agent.ResetPath();
        }

        state = CustomerState.Leaving;

        Debug.Log(name + " was sent home because the day ended.");
    }

    // Done eating: leave a dirty plate on the table, free the chair, and walk out
    private void FinishEating()
    {
        if (CustomerManager.Instance != null)
        {
            // The plate stays. The table is NOT clear until the player presses E.
            CustomerManager.Instance.LeaveDirtyPlate(mySeat);

            // The chair itself is free right away
            CustomerManager.Instance.ReleaseSeat(mySeat);
        }

        mySeat = null;

        state = CustomerState.Leaving;

        Debug.Log(name + " finished eating, left a dirty plate and is leaving.");
    }

    // --------------------------------------------------
    // UPDATE
    // --------------------------------------------------

    private void Update()
    {
        // No settings = setup mistake (error already shown in Awake)
        if (settings == null)
        {
            return;
        }

        switch (state)
        {
            case CustomerState.GoingToDoor:

                // Walk to my place in the door line
                if (MoveTo(doorTarget))
                {
                    state = CustomerState.WaitingAtDoor;
                }

                break;

            case CustomerState.WaitingAtDoor:

                // Door customers wait forever (the GDD has no door patience).
                // The CustomerManager sends us to a seat when one is free at a clear table.
                break;

            case CustomerState.GoingToSeat:

                if (MoveTo(mySeat.position))
                {
                    state = CustomerState.Thinking;

                    timer = 0f;

                    // Random time between the min and max from the settings asset
                    currentThinkingTime = Random.Range(
                        settings.thinkingTimeMin,
                        settings.thinkingTimeMax
                    );

                    CreateMyOrder();

                    if (thinkingBubble != null)
                    {
                        thinkingBubble.Show(GetOrderText());
                    }

                    Debug.Log(name + " is sitting and thinking for " + currentThinkingTime + " sec.");
                }

                break;

            case CustomerState.Thinking:

                timer += Time.deltaTime;

                if (timer >= currentThinkingTime)
                {
                    state = CustomerState.WaitingForPlayer;

                    timer = 0f;

                    Debug.Log(name + " finished thinking and is waiting for the player.");
                }

                break;

            case CustomerState.WaitingForPlayer:

                // Patience timer: the customer walks out if the order is not taken in time
                timer += Time.deltaTime;

                if (timer >= settings.orderPatience)
                {
                    if (thinkingBubble != null)
                    {
                        thinkingBubble.Hide();
                    }

                    state = CustomerState.Leaving;

                    Debug.Log(name + " ran out of patience and is leaving.");
                }

                break;

            case CustomerState.Ordering:

                break;

            case CustomerState.WaitingForFood:

                break;

            case CustomerState.Eating:

                timer += Time.deltaTime;

                if (timer >= settings.eatingTime)
                {
                    FinishEating();
                }

                break;

            case CustomerState.Leaving:

                if (MoveTo(CustomerManager.Instance.GetExitPoint().position))
                {
                    // mySeat is empty for customers who left from the door (that is fine)
                    CustomerManager.Instance.ReleaseSeat(mySeat);

                    Debug.Log(name + " reached the exit.");

                    ReturnToPool();
                }

                break;
        }
    }

    // --------------------------------------------------
    // PLAYER INTERACTION
    // --------------------------------------------------

    public void InteractWithPlayer()
    {
        Debug.Log(name + " interacted with player. Current state: " + state);

        if (state == CustomerState.WaitingForPlayer)
        {
            PlayerTakesOrder();

            return;
        }

        if (state == CustomerState.WaitingForFood)
        {
            ReceiveFood();

            return;
        }

        Debug.Log(name + " cannot interact right now.");
    }

    // --------------------------------------------------
    // GIVE ORDER
    // --------------------------------------------------

    public bool CanGiveOrder()
    {
        return state == CustomerState.WaitingForPlayer;
    }

    public void PlayerTakesOrder()
    {
        if (!CanGiveOrder())
        {
            return;
        }

        state = CustomerState.Ordering;

        if (OrderBoardUI.Instance != null)
        {
            OrderBoardUI.Instance.ShowOrder(this);
        }
        else
        {
            Debug.LogError("OrderBoardUI.Instance was not found.");
        }

        Debug.Log(name + " is giving the player the order.");
    }

    // --------------------------------------------------
    // ACCEPT ORDER
    // --------------------------------------------------

    public void OrderAccepted()
    {
        if (state != CustomerState.Ordering)
        {
            Debug.Log(name + " cannot accept order. Current state: " + state);

            return;
        }

        if (BakeryOrderManager.Instance == null)
        {
            Debug.LogError("BakeryOrderManager.Instance was not found.");

            return;
        }

        // Hide the bubble only after we know the order is really accepted
        if (thinkingBubble != null)
        {
            thinkingBubble.Hide();
        }

        myOrder = BakeryOrderManager.Instance.CreateOrder(this, chosenFood);

        state = CustomerState.WaitingForFood;

        Debug.Log(name + "'s order was accepted. Waiting for " + chosenFood);
    }

    // --------------------------------------------------
    // REJECT ORDER
    // --------------------------------------------------

    public void OrderRejected()
    {
        if (state != CustomerState.Ordering)
        {
            return;
        }

        if (thinkingBubble != null)
        {
            thinkingBubble.Hide();
        }

        state = CustomerState.Leaving;

        Debug.Log(name + "'s order was rejected.");
    }

    // --------------------------------------------------
    // RECEIVE FOOD (serve the dish and get paid)
    // --------------------------------------------------

    public void ReceiveFood()
    {
        if (state != CustomerState.WaitingForFood)
        {
            return;
        }

        if (settings == null)
        {
            Debug.LogError(name + " has no CustomerSettings assigned.");
            return;
        }

        if (PlayerFoodInventory.Instance == null)
        {
            Debug.LogError("PlayerFoodInventory.Instance was not found.");
            return;
        }

        // Nothing happens if the player is not holding the ordered dish
        if (!PlayerFoodInventory.Instance.HasFood(chosenFood))
        {
            Debug.Log(name + " needs " + chosenFood + ", but the player is not carrying it.");
            return;
        }

        // Look up the reward BEFORE taking the dish, so a setup mistake
        // does not eat the player's food for nothing
        RecipeData recipe = null;

        if (recipeDatabase != null)
        {
            recipe = recipeDatabase.GetRecipe(chosenFood);
        }

        if (recipe == null)
        {
            Debug.LogError("No recipe found for " + chosenFood + ". Check SO_RecipeDatabase and the Food Type on the recipe asset.");
            return;
        }

        if (Wallet.Instance == null)
        {
            Debug.LogError("Wallet.Instance was not found!");
            return;
        }

        bool removed = PlayerFoodInventory.Instance.RemoveFood(chosenFood);

        if (!removed)
        {
            Debug.LogError("Could not remove " + chosenFood + " from player inventory.");
            return;
        }

        // Coins come from the recipe asset. A VIP pays a multiple of the dish price (2x in the GDD).
        int coins = recipe.coinReward;

        if (IsVIP)
        {
            coins *= settings.vipCoinMultiplier;
        }

        Wallet.Instance.AddCoins(coins);

        // TODO (diamonds): a VIP should also give settings.vipDiamondReward diamonds.
        // The Wallet only has coins right now. Once it has an AddDiamonds method, call it here.

        Debug.Log(name + " (" + customerType + ") received " + chosenFood + " and paid " + coins + " coins.");

        state = CustomerState.Eating;

        timer = 0f;
    }

    // --------------------------------------------------
    // CREATE ORDER (only dishes that have a recipe)
    // --------------------------------------------------

    private void CreateMyOrder()
    {
        FoodType food;

        if (recipeDatabase != null && recipeDatabase.TryGetRandomFood(out food))
        {
            chosenFood = food;
        }
        else
        {
            Debug.LogWarning(name + ": no recipes in SO_RecipeDatabase. Defaulting to Bread.");
            chosenFood = FoodType.Bread;
        }

        Debug.Log(name + " wants " + chosenFood);
    }

    // --------------------------------------------------
    // GET ORDER TEXT / FOOD
    // --------------------------------------------------

    public string GetOrderText()
    {
        return chosenFood.ToString();
    }

    public FoodType GetChosenFood()
    {
        return chosenFood;
    }

    // --------------------------------------------------
    // NAVIGATION
    // --------------------------------------------------

    private bool MoveTo(Vector3 targetPosition)
    {
        if (agent == null)
        {
            Debug.LogError(name + " does not have a NavMeshAgent.");

            return false;
        }

        if (!agent.isOnNavMesh)
        {
            return false;
        }

        if (!agent.hasPath)
        {
            agent.SetDestination(targetPosition);
        }

        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.ResetPath();

            return true;
        }

        return false;
    }

    // --------------------------------------------------
    // RESET CUSTOMER
    // --------------------------------------------------

    private void ResetCustomer()
    {
        if (thinkingBubble != null)
        {
            thinkingBubble.Hide();
        }

        mySeat = null;

        myOrder = null;

        timer = 0f;

        chosenFood = FoodType.Bread;

        state = CustomerState.GoingToDoor;

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (agent != null)
        {
            agent.ResetPath();
        }
    }

    // --------------------------------------------------
    // RETURN TO POOL
    // --------------------------------------------------

    private void ReturnToPool()
    {
        if (CustomerPool.Instance == null)
        {
            Debug.LogError("CustomerPool.Instance was not found.");

            return;
        }

        CustomerPool.Instance.ReturnCustomer(gameObject, customerType);
    }
}