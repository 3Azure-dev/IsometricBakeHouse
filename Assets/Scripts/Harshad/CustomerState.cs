public enum CustomerState
{
    GoingToSeat,
    Thinking,
    WaitingForPlayer,
    Ordering,
    WaitingForFood,
    Eating,
    Leaving,
    ReturningBecauseNoSeat,
        GoingToDoor,      // walking to my place in the door line
    WaitingAtDoor,    // waiting at the door (forever)

}

// How an order ended (SCRUM-274). Shown above the customer for now: text + colour.
public enum CustomerReaction
{
    None,          // order not finished yet
    Satisfied,     // right dish in time: pays coins
    Disappointed,  // wrong dish: 0 coins
    Furious,       // order timer ran out: 0 coins
    WalkedOut      // order was not taken in time (20 sec patience): 0 coins
}