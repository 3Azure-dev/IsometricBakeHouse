using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// Shows the dishes the player is carrying (SCRUM-720).
// One slot per dish. Slots with no dish are hidden.
public class FoodTrayUI : MonoBehaviour
{
    [Header("Slots (one Image per slot, in order)")]
    [SerializeField] private Image[] slotIcons;

    [Header("Icon for each dish")]
    [SerializeField] private Sprite breadIcon;
    [SerializeField] private Sprite cupcakeIcon;
    [SerializeField] private Sprite pizzaIcon;
    [SerializeField] private Sprite burgerIcon;

    private PlayerFoodInventory subscribed;

    private void Start()
    {
        Subscribe();
        Refresh();
    }

    private void Update()
    {
        // The inventory can be created after this UI, so keep trying until it exists
        if (subscribed == null && PlayerFoodInventory.Instance != null)
        {
            Subscribe();
            Refresh();
        }
    }

    private void Subscribe()
    {
        if (subscribed != null || PlayerFoodInventory.Instance == null) return;

        subscribed = PlayerFoodInventory.Instance;
        subscribed.Changed += Refresh;
    }

    private void OnDestroy()
    {
        if (subscribed != null)
            subscribed.Changed -= Refresh;
    }

    private void Refresh()
    {
        List<FoodType> foods = subscribed != null
            ? subscribed.GetCarriedFoods()
            : new List<FoodType>();

        for (int i = 0; i < slotIcons.Length; i++)
        {
            if (slotIcons[i] == null) continue;

            if (i < foods.Count)
            {
                slotIcons[i].sprite = IconFor(foods[i]);
                slotIcons[i].preserveAspect = true;
                slotIcons[i].gameObject.SetActive(true);
            }
            else
            {
                slotIcons[i].gameObject.SetActive(false);
            }
        }
    }

    private Sprite IconFor(FoodType food)
    {
        switch (food)
        {
            case FoodType.Bread: return breadIcon;
            case FoodType.Cupcake: return cupcakeIcon;
            case FoodType.Pizza: return pizzaIcon;
            case FoodType.Burger: return burgerIcon;
        }
        return null;
    }
}