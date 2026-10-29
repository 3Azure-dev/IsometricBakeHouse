using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CustomerController : MonoBehaviour
{
    [Header("Data assets (drag them into the prefab)")]
    [SerializeField] private CustomerSettings settings;
    [SerializeField] private RecipeDatabase recipeDatabase;

    [Header("UI")]
    [SerializeField] private ThinkingBubble thinkingBubble;

    [Header("Order Timer UI")]
    [SerializeField] private OrderTimer orderTimerUI;

    public CustomerType customerType;
    public float moveSpeed = 5f;
    public float timer;

    private BakeryOrder myOrder;
    private NavMeshAgent agent;
    private Transform mySeat;
    private CustomerState state;

    // Prevents the arrival bell from playing more than once per customer.
    private bool arrivalBellPlayed;

    // My place in the door line (set by the CustomerManager).
    private Vector3 doorTarget;

    // Picked randomly between thinkingTimeMin and thinkingTimeMax
    // each time a customer sits down.
    private float currentThinkingTime;

    // The ONE food this customer wants.
    private FoodType chosenFood;

    // Seconds left on MY order timer.
    // It starts when the player accepts the order (SCRUM-295).
    private float orderTimeLeft;

    // How my order ended: satisfied, disappointed, furious or walked out (SCRUM-274).
    private CustomerReaction reaction = CustomerReaction.None;

    public CustomerReaction Reaction
    {
        get { return reaction; }
    }

    // --------------------------------------------------
    // INFO OTHER SCRIPTS CAN ASK FOR
    // --------------------------------------------------

    public bool IsVIP
    {
        get { return customerType == CustomerType.VIP; }
    }

    // True once the order has really been accepted.
    public bool IsWaitingForFood
    {
        get { return state == CustomerState.WaitingForFood; }
    }

    // True when pressing E on this customer would do something.
    public bool CanInteractWithPlayer
    {
        get
        {
            return state == CustomerState.WaitingForPlayer ||
                   state == CustomerState.Ordering ||
                   state == CustomerState.WaitingForFood;
        }
    }
    public bool IsWaitingAtDoor
    {
        get { return state == CustomerState.WaitingAtDoor; }
    }

    // How many seconds the order timer should run for this customer.
    // Normal and VIP durations come from SO_CustomerSettings.
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
            Debug.LogError(
                name + " has no CustomerSettings assigned. " +
                "Drag SO_CustomerSettings into the prefab."
            );
        }

        if (orderTimerUI == null)
        {
            orderTimerUI = GetComponentInChildren<OrderTimer>(true);
        }

        if (orderTimerUI != null)
        {
            orderTimerUI.HideTimer();
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

        // Get in line at the door.
        // The arrival bell does NOT play when the customer spawns.
        if (!CustomerManager.Instance.JoinDoorQueue(this))
        {
            Debug.LogWarning(
                name + ": the door is full. Going back to the pool."
            );

            ReturnToPool();
            return;
        }

        Debug.Log(
            name + " spawned as " + customerType +
            " and is walking to the door."
        );
    }

    // --------------------------------------------------
    // CALLED BY THE CUSTOMER MANAGER
    // --------------------------------------------------

    // This is your place in the door line.
    // Also used when the line moves forward.
    public void SetDoorTarget(Vector3 position)
    {
        if (state != CustomerState.GoingToDoor &&
            state != CustomerState.WaitingAtDoor)
        {
            return;
        }

        doorTarget = position;

        // Forget the old path so the agent walks to the new place.
        if (agent != null)
        {
            agent.ResetPath();
        }

        state = CustomerState.GoingToDoor;
    }

    // A seat at a clear table is free.
    // The customer starts walking towards that table.
    public void GoToSeat(Transform seat)
    {
        mySeat = seat;

        if (agent != null)
        {
            agent.ResetPath();
        }

        state = CustomerState.GoingToSeat;

        Debug.Log(name + " leaves the door and starts walking to a seat.");
    }

    // The day ended: customers still at the door walk away.
    // Customers who are already seated or eating are left alone.
    public void LeaveBecauseDayEnded()
    {
        if (state != CustomerState.GoingToDoor &&
            state != CustomerState.WaitingAtDoor)
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

    // --------------------------------------------------
    // CUSTOMER REACTION
    // --------------------------------------------------

    // Shows how the order ended above the customer.
    private void ShowReaction(CustomerReaction newReaction)
    {
        reaction = newReaction;

        if (thinkingBubble == null)
        {
            return;
        }

        switch (newReaction)
        {
            case CustomerReaction.Satisfied:
                thinkingBubble.ShowReaction(
                    "Satisfied!",
                    new Color(0.2f, 0.8f, 0.3f)
                );
                break;

            case CustomerReaction.Disappointed:
                thinkingBubble.ShowReaction(
                    "Disappointed",
                    new Color(1f, 0.6f, 0.1f)
                );
                break;

            case CustomerReaction.Furious:
                thinkingBubble.ShowReaction(
                    "FURIOUS!",
                    new Color(0.9f, 0.15f, 0.15f)
                );
                break;

            case CustomerReaction.WalkedOut:
                thinkingBubble.ShowReaction(
                    "Walked out",
                    new Color(0.6f, 0.6f, 0.6f)
                );
                break;
        }
    }

    // --------------------------------------------------
    // END ORDER UNHAPPY
    // --------------------------------------------------

    // Wrong dish or timer ran out:
    // no coins, and the customer leaves.
    private void EndOrderUnhappy(CustomerReaction badReaction)
    {
        HideOrderTimerUI();

        if (BakeryOrderManager.Instance != null)
        {
            BakeryOrderManager.Instance.RemoveOrder(myOrder);
        }

        myOrder = null;

        ShowReaction(badReaction);

        state = CustomerState.Leaving;

        Debug.Log(
            name + " is " + badReaction +
            ". 0 coins. Leaving."
        );
    }

    // --------------------------------------------------
    // FINISH EATING
    // --------------------------------------------------

    // Done eating: leave a dirty plate on the table,
    // free the chair, and walk out.
    private void FinishEating()
    {
        if (thinkingBubble != null)
        {
            thinkingBubble.Hide();
        }

        if (CustomerManager.Instance != null)
        {
            // The plate stays.
            // The table is NOT clear until the player presses E.
            CustomerManager.Instance.LeaveDirtyPlate(mySeat);

            // The chair itself is free right away.
            CustomerManager.Instance.ReleaseSeat(mySeat);
        }

        mySeat = null;

        state = CustomerState.Leaving;

        Debug.Log(
            name + " finished eating, left a dirty plate and is leaving."
        );
    }

    // --------------------------------------------------
    // UPDATE
    // --------------------------------------------------

    private void Update()
    {
        // No settings = setup mistake.
        if (settings == null)
        {
            return;
        }

        switch (state)
        {
            case CustomerState.GoingToDoor:

                // Walk to my place in the door line.
                // Do NOT play the bell here.
                if (MoveTo(doorTarget))
                {
                    state = CustomerState.WaitingAtDoor;
                }

                break;

            case CustomerState.WaitingAtDoor:

                // Door customers wait until a table becomes available.
                // The CustomerManager sends them to a seat when one is free.
                break;

            case CustomerState.GoingToSeat:

                if (mySeat == null)
                {
                    break;
                }

                // Move towards the assigned table.
                bool reachedSeat = MoveTo(mySeat.position);

                // Play the bell when the customer actually starts moving.
                if (!arrivalBellPlayed &&
                    agent != null &&
                    agent.isOnNavMesh &&
                    agent.velocity.sqrMagnitude > 0.04f)
                {
                    arrivalBellPlayed = true;

                    if (BakeryAudioManager.Instance != null)
                    {
                        BakeryAudioManager.Instance.PlayCustomerArrives();

                        Debug.Log(
                            "Arrival bell: " + name +
                            " is approaching its table."
                        );
                    }
                    else
                    {
                        Debug.LogWarning(
                            "BakeryAudioManager.Instance was not found."
                        );
                    }
                }

                // When the customer reaches the table, begin thinking.
                if (reachedSeat)
                {
                    state = CustomerState.Thinking;
                    timer = 0f;

                    currentThinkingTime = Random.Range(
                        settings.thinkingTimeMin,
                        settings.thinkingTimeMax
                    );

                    CreateMyOrder();

                    if (thinkingBubble != null)
                    {
                        thinkingBubble.Show("Thinking...");
                    }

                    Debug.Log(
                        name + " is sitting and thinking for " +
                        currentThinkingTime + " sec."
                    );
                }

                break;
            case CustomerState.Thinking:

                timer += Time.deltaTime;

                if (timer >= currentThinkingTime)
                {
                    state = CustomerState.WaitingForPlayer;

                    timer = 0f;

                    if (thinkingBubble != null)
                    {
                        thinkingBubble.Show(GetOrderText());
                    }

                    Debug.Log(
                        name + " finished thinking and is waiting for the player."
                    );
                }

                break;

            case CustomerState.WaitingForPlayer:

                // Patience timer:
                // the customer walks out if the order is not taken in time.
                timer += Time.deltaTime;

                if (timer >= settings.orderPatience)
                {
                    ShowReaction(CustomerReaction.WalkedOut);

                    state = CustomerState.Leaving;

                    Debug.Log(
                        name + " ran out of patience and is leaving."
                    );
                }

                break;

            case CustomerState.Ordering:

                break;

            case CustomerState.WaitingForFood:

                // My own order timer.
                // Runs out = furious, 0 coins.
                if (orderTimeLeft > 0f)
                {
                    orderTimeLeft -= Time.deltaTime;

                    if (orderTimeLeft <= 0f)
                    {
                        EndOrderUnhappy(orderTimeoutReaction);
                    }
                }

                break;

            case CustomerState.Eating:

                timer += Time.deltaTime;

                if (timer >= settings.eatingTime)
                {
                    FinishEating();
                }

                break;

            case CustomerState.Leaving:

                if (CustomerManager.Instance == null)
                {
                    Debug.LogError("CustomerManager.Instance was not found.");
                    break;
                }

                Transform exit = CustomerManager.Instance.GetExitPoint();

                if (exit == null)
                {
                    Debug.LogError(
                        "CustomerManager has no Exit Point assigned."
                    );
                    break;
                }

                if (MoveTo(exit.position))
                {
                    // mySeat is empty for customers who left from the door.
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
        Debug.Log(
            name + " interacted with player. Current state: " + state
        );

        if (state == CustomerState.WaitingForPlayer)
        {
            PlayerTakesOrder();
            return;
        }

        if (state == CustomerState.Ordering)
        {
            if (OrderBoardUI.Instance != null)
            {
                OrderBoardUI.Instance.ShowOrder(this);
            }

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
            Debug.Log(
                name + " cannot accept order. Current state: " + state
            );

            return;
        }

        if (BakeryOrderManager.Instance == null)
        {
            Debug.LogError("BakeryOrderManager.Instance was not found.");
            return;
        }

        // Hide the bubble only after we know the order is really accepted.
        if (thinkingBubble != null)
        {
            thinkingBubble.Hide();
        }

        myOrder = BakeryOrderManager.Instance.CreateOrder(this, chosenFood);

        // Start the customer's gameplay timer.
        orderTimeLeft = GetOrderTimerSeconds();

        // Start the visible timer ring with the same duration.
        if (orderTimerUI != null)
        {
            orderTimerUI.StartTimer(orderTimeLeft);
        }
        else
        {
            Debug.LogWarning(
                name + ": No OrderTimer found on the customer prefab."
            );
        }

        state = CustomerState.WaitingForFood;
    }

    // The order panel moved on to another customer without accept or reject.
    // Go back to waiting. The patience timer keeps the time it had.
    public void CancelOrdering()
    {
        if (state == CustomerState.Ordering)
        {
            state = CustomerState.WaitingForPlayer;
        }
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

        HideOrderTimerUI();

        Debug.Log(name + "'s order was rejected.");
    }

    // --------------------------------------------------
    // RECEIVE FOOD
    // --------------------------------------------------

    // Serve the dish and get paid.
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

        // Nothing happens if the player is carrying no food.
        if (!PlayerFoodInventory.Instance.IsCarryingFood())
        {
            Debug.Log(
                name + " needs " + chosenFood +
                ", but the player is not carrying any food."
            );

            return;
        }

        // The player carries food, but not the dish that was ordered.
        if (!PlayerFoodInventory.Instance.HasFood(chosenFood))
        {
            if (settings.wrongDishDisappoints)
            {
                ServeWrongDish();
            }
            else
            {
                Debug.Log(
                    name + " needs " + chosenFood +
                    ", but the player is not carrying it."
                );
            }

            return;
        }

        // Look up the reward BEFORE taking the dish.
        RecipeData recipe = null;

        if (recipeDatabase != null)
        {
            recipe = recipeDatabase.GetRecipe(chosenFood);
        }

        if (recipe == null)
        {
            Debug.LogError(
                "No recipe found for " + chosenFood +
                ". Check SO_RecipeDatabase and the Food Type on the recipe asset."
            );

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
            Debug.LogError(
                "Could not remove " + chosenFood + " from player inventory."
            );

            return;
        }

        // Hide the order timer after the food is successfully served.
        HideOrderTimerUI();

        // Play the sound after food is successfully served.
        if (BakeryAudioManager.Instance != null)
        {
            BakeryAudioManager.Instance.PlayDishServed();
        }

        // Coins come from the recipe asset.
        // A VIP pays a multiple of the dish price.
        int coins = recipe.coinReward;

        if (IsVIP)
        {
            coins *= settings.vipCoinMultiplier;
        }

        Wallet.Instance.AddCoins(coins);

        // Only a VIP gives diamonds, and only when served correctly.
        if (IsVIP)
        {
            Wallet.Instance.AddDiamonds(settings.vipDiamondReward);
        }

        Debug.Log(
            name + " (" + customerType + ") received " +
            chosenFood + " and paid " + coins + " coins."
        );

        // The order is finished, so stop tracking it.
        if (BakeryOrderManager.Instance != null)
        {
            BakeryOrderManager.Instance.RemoveOrder(myOrder);
        }

        myOrder = null;

        ShowReaction(CustomerReaction.Satisfied);

        state = CustomerState.Eating;

        timer = 0f;
    }

    // --------------------------------------------------
    // WRONG DISH
    // --------------------------------------------------

    // The player served a dish that was not ordered.
    // Disappointed, 0 coins, 0 diamonds.
    private void ServeWrongDish()
    {
        List<FoodType> carried =
            PlayerFoodInventory.Instance.GetCarriedFoods();

        FoodType wrongDish = carried[0];

        PlayerFoodInventory.Instance.RemoveFood(wrongDish);

        Debug.Log(
            name + " wanted " + chosenFood + " but got " + wrongDish + "."
        );

        EndOrderUnhappy(CustomerReaction.Disappointed);
    }

    // --------------------------------------------------
    // CREATE ORDER
    // --------------------------------------------------

    // Only choose dishes that have a recipe.
    private void CreateMyOrder()
    {
        FoodType food;

        if (recipeDatabase != null &&
            recipeDatabase.TryGetRandomFood(out food))
        {
            chosenFood = food;
        }
        else
        {
            Debug.LogWarning(
                name + ": no recipes in SO_RecipeDatabase. Defaulting to Bread."
            );

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
        // Reset the bell so it can play for the next customer
        // when this customer object is reused from the pool.
        arrivalBellPlayed = false;

        if (thinkingBubble != null)
        {
            thinkingBubble.Hide();
        }

        mySeat = null;

        myOrder = null;

        timer = 0f;

        orderTimeLeft = 0f;

        reaction = CustomerReaction.None;

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

        HideOrderTimerUI();
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

    // --------------------------------------------------
    // HIDE ORDER TIMER
    // --------------------------------------------------

    private void HideOrderTimerUI()
    {
        if (orderTimerUI != null)
        {
            orderTimerUI.HideTimer();
        }
    }

        [Tooltip("How the customer reacts when the order timer runs out. SCRUM-274 says Furious.")]
    public CustomerReaction orderTimeoutReaction = CustomerReaction.Furious;
}