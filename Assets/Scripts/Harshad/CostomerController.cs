using UnityEngine;
using UnityEngine.AI;

public class CustomerController : MonoBehaviour
{
    private BakeryOrder myOrder;

    private NavMeshAgent agent;

    public CustomerType customerType;

    public float moveSpeed = 5f;

    private Transform mySeat;

    private CustomerState state;

    public float timer;

    public float thinkingTime = 5f;

    public float eatingTime = 5f;

    // The ONE food this customer wants
    private FoodType chosenFood;

    [SerializeField]
    private ThinkingBubble thinkingBubble;
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
            Debug.Log(
                name +
                " could not find a free seat. Returning to exit."
            );

            state =
                CustomerState.ReturningBecauseNoSeat;

            return;
        }

        state =
            CustomerState.GoingToSeat;

        Debug.Log(
            name +
            " spawned as " +
            customerType
        );
    }


    // --------------------------------------------------
    // UPDATE
    // --------------------------------------------------

    private void Update()
    {
        switch (state)
        {
            case CustomerState.GoingToSeat:

                if (MoveTo(mySeat.position))
                {
                    state = CustomerState.Thinking;

                    timer = 0f;

                    CreateMyOrder();

                    if (thinkingBubble != null)
                    {
                        thinkingBubble.Show(GetOrderText());
                    }

                    Debug.Log(name + " is sitting and thinking.");
                }

                break;

            case CustomerState.Thinking:

                timer += Time.deltaTime;

                if (timer >= thinkingTime)
                {
                    state = CustomerState.WaitingForPlayer;

                    timer = 0f;

                    Debug.Log(
                        name + " finished thinking and is waiting for the player."
                    );
                }

                break;


            case CustomerState.WaitingForPlayer:

                break;


            case CustomerState.Ordering:

                break;


            case CustomerState.WaitingForFood:

                break;


            case CustomerState.Eating:

                timer += Time.deltaTime;

                if (timer >= eatingTime)
                {
                    state =
                        CustomerState.Leaving;

                    Debug.Log(
                        name +
                        " finished eating and is leaving."
                    );
                }

                break;


            case CustomerState.Leaving:

                if (MoveTo(
                    CustomerManager.Instance
                        .GetExitPoint()
                        .position))
                {
                    CustomerManager.Instance
                        .ReleaseSeat(mySeat);

                    Debug.Log(
                        name +
                        " reached the exit."
                    );

                    ReturnToPool();
                }

                break;


            case CustomerState.ReturningBecauseNoSeat:

                if (MoveTo(
                    CustomerManager.Instance
                        .GetExitPoint()
                        .position))
                {
                    Debug.Log(
                        name +
                        " returned because no seats were available."
                    );

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
        mySeat =
            CustomerManager.Instance.GetFreeSeat();
    }


    // --------------------------------------------------
    // PLAYER INTERACTION
    // --------------------------------------------------

    public void InteractWithPlayer()
    {
        Debug.Log(
            name +
            " interacted with player. Current state: " +
            state
        );

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

        Debug.Log(
            name +
            " cannot interact right now."
        );
    }


    // --------------------------------------------------
    // GIVE ORDER
    // --------------------------------------------------

    public bool CanGiveOrder()
    {
        return state ==
            CustomerState.WaitingForPlayer;
    }


    public void PlayerTakesOrder()
    {
        if (!CanGiveOrder())
        {
            return;
        }

        state =
            CustomerState.Ordering;

        if (OrderBoardUI.Instance != null)
        {
            OrderBoardUI.Instance.ShowOrder(this);
        }
        else
        {
            Debug.LogError(
                "OrderBoardUI.Instance was not found."
            );
        }

        Debug.Log(
            name +
            " is giving the player the order."
        );
    }


    // --------------------------------------------------
    // ACCEPT ORDER
    // --------------------------------------------------

    public void OrderAccepted()
    {

        if (thinkingBubble != null)
        {
            thinkingBubble.Hide();
        }

        if (state != CustomerState.Ordering)
        {
            Debug.Log(
                name +
                " cannot accept order. Current state: " +
                state
            );

            return;
        }

        if (BakeryOrderManager.Instance == null)
        {
            Debug.LogError(
                "BakeryOrderManager.Instance was not found."
            );

            return;
        }

        myOrder =
            BakeryOrderManager.Instance.CreateOrder(
                this,
                chosenFood
            );

        state =
            CustomerState.WaitingForFood;

        Debug.Log(
            name +
            "'s order was accepted."
        );

        Debug.Log(
            name +
            " is waiting for " +
            chosenFood
        );
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

        state =
            CustomerState.Leaving;

        Debug.Log(
            name +
            "'s order was rejected."
        );
    }


    // --------------------------------------------------
    // RECEIVE FOOD
    // --------------------------------------------------

    public void ReceiveFood()
    {
        Debug.Log(
            name +
            " is trying to receive food."
        );

        if (state != CustomerState.WaitingForFood)
        {
            Debug.Log(
                name +
                " is not waiting for food."
            );

            return;
        }

        if (PlayerFoodInventory.Instance == null)
        {
            Debug.LogError(
                "PlayerFoodInventory.Instance was not found."
            );

            return;
        }

        if (!PlayerFoodInventory.Instance.HasFood(
            chosenFood))
        {
            Debug.Log(
                name +
                " needs " +
                chosenFood +
                ", but the player is not carrying it."
            );

            return;
        }

        bool removed =
            PlayerFoodInventory.Instance
                .RemoveFood(chosenFood);

        if (!removed)
        {
            Debug.LogError(
                "Could not remove " +
                chosenFood +
                " from player inventory."
            );

            return;
        }

        Debug.Log(
            name +
            " received " +
            chosenFood
        );

        state =
            CustomerState.Eating;

        timer = 0f;

        Debug.Log(
            name +
            " is now eating."
        );
    }


    // --------------------------------------------------
    // CREATE ORDER
    // --------------------------------------------------

    private void CreateMyOrder()
    {
        int foodCount =
            System.Enum.GetValues(
                typeof(FoodType)
            ).Length;

        chosenFood =
            (FoodType)Random.Range(
                0,
                foodCount
            );

        Debug.Log(
            name +
            " wants " +
            chosenFood
        );
    }


    // --------------------------------------------------
    // GET ORDER TEXT
    // --------------------------------------------------

    public string GetOrderText()
    {
        return chosenFood.ToString();
    }


    // --------------------------------------------------
    // GET ORDER FOOD
    // --------------------------------------------------

    public FoodType GetChosenFood()
    {
        return chosenFood;
    }


    // --------------------------------------------------
    // NAVIGATION
    // --------------------------------------------------

    private bool MoveTo(
        Vector3 targetPosition)
    {
        if (agent == null)
        {
            Debug.LogError(
                name +
                " does not have a NavMeshAgent."
            );

            return false;
        }

        if (!agent.isOnNavMesh)
        {
            return false;
        }

        if (!agent.hasPath)
        {
            agent.SetDestination(
                targetPosition
            );
        }

        if (!agent.pathPending &&
            agent.remainingDistance <=
            agent.stoppingDistance)
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

        chosenFood =
            FoodType.Bread;

        state =
            CustomerState.GoingToSeat;

        if (agent == null)
        {
            agent =
                GetComponent<NavMeshAgent>();
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
            Debug.LogError(
                "CustomerPool.Instance was not found."
            );

            return;
        }

        CustomerPool.Instance.ReturnCustomer(
            gameObject,
            customerType
        );
    }
}