// This is just a contract. It tells Unity "I can tell you if food is ready."
public interface IFoodReadyProvider
{
    bool IsFoodReady { get; }
}