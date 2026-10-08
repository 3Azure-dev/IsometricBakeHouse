using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class CustomerController : MonoBehaviour
{
    private BakeryOrder myOrder;
    private NavMeshAgent agent;
    private List<FoodType> chosenFoods =
        new List<FoodType>();
    public CustomerType customerType;

    public float moveSpeed = 5f;

    private Transform mySeat;

    private CustomerState state;

    public float timer;

    public float thinkingTime = 5f;

    public float eatingTime = 5f;


   
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        FindSeat();

        if (mySeat == null)
        {
            Debug.LogWarning(
                name + " could not find a free seat."
            );

            return;
        }

        state = CustomerState.GoingToSeat;
    }

    private void Update()
    {
        switch (state)
        {
            case CustomerState.GoingToSeat:

                if (MoveTo(mySeat.position))
                {
                    state = CustomerState.Thinking;
                    timer = 0f;

                    Debug.Log(
                        name + " is sitting and thinking."
                    );
                }

                break;


            case CustomerState.Thinking:

                timer += Time.deltaTime;

                if (timer >= thinkingTime)
                {
                    state = CustomerState.WaitingForCounter;

                    timer = 0f;

                    Debug.Log(
                        name + " finished thinking."
                    );
                }

                break;


            case CustomerState.WaitingForCounter:

                if (CustomerManager.Instance.IsCounterFree())
                {
                    if (CustomerManager.Instance.TryTakeCounter(this))
                    {
                        state = CustomerState.GoingToCounter;
                    }
                }

                break;

            case CustomerState.GoingToCounter:

                if (MoveTo(
                    CustomerManager.Instance
                    .GetCounter()
                    .position))
                {
                    CreateMyOrder();

                    state = CustomerState.Ordering;

                    Debug.Log(
                        name + " is ready to give the order."
                    );
                }

                break;


            case CustomerState.Ordering:

                /*
                 * Later the Order Board will control
                 * whether the player accepts or rejects
                 * this customer's order.
                 */

                break;


            case CustomerState.ReturningToSeat:

                if (MoveTo(mySeat.position))
                {
                    state = CustomerState.WaitingForFood;

                    Debug.Log(
                        name + " is waiting for food."
                    );
                }

                break;


            case CustomerState.WaitingForFood:

                /*
                 * The Order Board will tell this
                 * customer when the order is ready.
                 */

                break;


            case CustomerState.GoingToCollect:

                if (MoveTo(
                    CustomerManager.Instance
                    .GetCounter()
                    .position))
                {
                    CustomerManager.Instance
                        .LeaveCounter(this);

                    state = CustomerState.ReturningAfterCollect;

                    Debug.Log(
                        name + " collected the order."
                    );
                }

                break;


            case CustomerState.ReturningAfterCollect:

                if (MoveTo(mySeat.position))
                {
                    state = CustomerState.Eating;
                    timer = 0f;

                    Debug.Log(
                        name + " is eating."
                    );
                }

                break;


            case CustomerState.Eating:

                timer += Time.deltaTime;

                if (timer >= eatingTime)
                {
                    state = CustomerState.Leaving;

                    CustomerManager.Instance
                        .ReleaseSeat(mySeat);
                }

                break;


            case CustomerState.Leaving:

                if (MoveTo(
                    CustomerManager.Instance
                    .GetExitPoint()
                    .position))
                {
                    Debug.Log(
                        name + " reached the exit."
                    );
                }

                break;
        }
    }

    private void FindSeat()
    {
        mySeat =
            CustomerManager.Instance.GetFreeSeat();
    }

    public void OrderAccepted()
    {
        if (state != CustomerState.Ordering)
            return;

        CustomerManager.Instance
            .LeaveCounter(this);

        state = CustomerState.ReturningToSeat;

        Debug.Log(
            name + "'s order was accepted."
        );
    }

    public void OrderRejected()
    {
        if (state != CustomerState.Ordering)
            return;

        CustomerManager.Instance
            .LeaveCounter(this);

        state = CustomerState.Leaving;

        Debug.Log(
            name + "'s order was rejected."
        );
    }

    public void OrderCompleted()
    {
        if (state != CustomerState.WaitingForFood)
            return;

        state = CustomerState.GoingToCollect;

        Debug.Log(
            name + "'s order is ready."
        );
    }

    private void CreateMyOrder()
    {
        chosenFoods.Clear();

        int numberOfItems =
            Random.Range(1, 4);

        for (int i = 0; i < numberOfItems; i++)
        {
            FoodType food =
                (FoodType)Random.Range(
                    0,
                    System.Enum.GetValues(
                        typeof(FoodType)
                    ).Length
                );

            if (!chosenFoods.Contains(food))
            {
                chosenFoods.Add(food);
            }
        }

        myOrder =
            BakeryOrderManager.Instance.CreateOrder(
                this,
                chosenFoods
            );

        Debug.Log(
            name +
            " wants " +
            GetOrderText()
        );
    }

    private string GetOrderText()
    {
        string text = "";

        for (int i = 0; i < chosenFoods.Count; i++)
        {
            text += chosenFoods[i].ToString();

            if (i < chosenFoods.Count - 1)
            {
                text += ", ";
            }
        }

        return text;
    }

    private bool MoveTo(Vector3 targetPosition)
    {
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
}