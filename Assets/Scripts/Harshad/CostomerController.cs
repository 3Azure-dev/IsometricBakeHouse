using UnityEngine;
using UnityEngine.AI;

public class CustomerController : MonoBehaviour
{
    [Header("Settings (drag SO_CustomerSettings here)")]
    [SerializeField] private CustomerSettings settings;

    [Header("Recipes (drag SO_RecipeDatabase here)")]
    [SerializeField] private RecipeDatabase recipeDatabase;

    [SerializeField]
    private ThinkingBubble thinkingBubble;

    private BakeryOrder myOrder;
    private NavMeshAgent agent;

    public CustomerType customerType;
    public float moveSpeed = 5f;

    private Transform mySeat;
    private CustomerState state;

    public float timer;

    // Picked randomly between thinkingTimeMin and thinkingTimeMax each time a customer sits down
    private float currentThinkingTime;

    // The ONE food this customer wants
    private FoodType chosenFood;

    // The spawner uses this to count customers who are still walking in
    public bool IsEnteringBakery
    {
        get { return state == CustomerState.GoingToSeat; }
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

        FindSeat();

        if (mySeat == null)
        {
            Debug.Log(name + " could not find a free seat. Returning to exit.");

            state = CustomerState.ReturningBecauseNoSeat;

            return;
        }

        state = CustomerState.GoingToSeat;

        Debug.Log(name + " spawned as " + customerType);
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

                // NEW: customer gets tired of waiting for the player
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
                    state = CustomerState.Leaving;

                    Debug.Log(name + " finished eating and is leaving.");
                }

                break;

            case CustomerState.Leaving:

                if (MoveTo(CustomerManager.Instance.GetExitPoint().position))
                {
                    CustomerManager.Instance.ReleaseSeat(mySeat);

                    Debug.Log(name + " reached the exit.");

                    ReturnToPool();
                }

                break;

            case CustomerState.ReturningBecauseNoSeat:

                if (MoveTo(CustomerManager.Instance.GetExitPoint().position))
                {
                    Debug.Log(name + " returned because no seats were available.");

                    ReturnToPool();
                }

                break;
        }
    }

    // --------------------------------------------------
    // FIND SEAT
    // --------------------------------------------------

    private void FindSeat()
    {
        mySeat = CustomerManager.Instance.GetFreeSeat();
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

        Debug.Log(name + "'s order was accepted.");

        Debug.Log(name + " is waiting for " + chosenFood);
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
    // RECEIVE FOOD
    // --------------------------------------------------

    public void ReceiveFood()
    {
        if (state != CustomerState.WaitingForFood)
        {
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

        // The coin amount now comes from the recipe asset
        Wallet.Instance.AddCoins(recipe.coinReward);

        Debug.Log(name + " received " + chosenFood + " and paid " + recipe.coinReward + " coins.");

        state = CustomerState.Eating;

        timer = 0f;
    }
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

        state = CustomerState.GoingToSeat;

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