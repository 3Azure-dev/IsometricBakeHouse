using System;

public enum DeliveryItemType { Blueprint, Material }

// One thing the shop sends: a blueprint (goes to the blueprint inventory)
// or materials (picked up and carried).
[Serializable]
public struct DeliveryItem
{
    public DeliveryItemType type;
    public string itemId;
    public int amount;

    public DeliveryItem(DeliveryItemType type, string itemId, int amount = 1)
    {
        this.type = type;
        this.itemId = itemId;
        this.amount = amount;
    }

    public override string ToString()
    {
        return type + " " + itemId + (amount > 1 ? " x" + amount : "");
    }
}