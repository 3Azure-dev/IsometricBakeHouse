using UnityEngine;

public class FoodReadyPromptManager : MonoBehaviour
{
    [Header("Food Ready Prompts")]
    // The actual prompt poppers attached to your Stove, Oven, and Pizza Stone
    [SerializeField] private PromptPopper[] foodPrompts;

    [SerializeField] private float promptRange = 3f;  // Max distance to show prompt
    [SerializeField] private Transform player;        // Player transform

    private int nearestPromptIndex = -1;

    private void Update()
    {
        FindNearestReadyFood();
        UpdatePrompts();
    }

    private void FindNearestReadyFood()
    {
        // Auto-find player if not assigned in Inspector
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null) return;
        }

        float nearestDistance = float.MaxValue;
        nearestPromptIndex = -1;

        for (int i = 0; i < foodPrompts.Length; i++)
        {
            if (foodPrompts[i] == null) continue;

            // IMPORTANT: Only check prompts that are currently active (food is actually ready).
            // The PromptPopper script deactivates the GameObject when PopOut() is called.
            if (!foodPrompts[i].gameObject.activeSelf) continue;

            float distance = Vector3.Distance(player.position, foodPrompts[i].transform.position);

            if (distance < nearestDistance && distance <= promptRange)
            {
                nearestDistance = distance;
                nearestPromptIndex = i;
            }
        }
    }

    private void UpdatePrompts()
    {
        // We don't want to hide prompts that aren't ready, 
        // so we only hide the ones that ARE ready but NOT the nearest one.
        for (int i = 0; i < foodPrompts.Length; i++)
        {
            if (foodPrompts[i] == null || !foodPrompts[i].gameObject.activeSelf) continue;

            // If this is the nearest one, ensure it's visible
            if (i == nearestPromptIndex)
            {
                // It's already active, no need to PopIn again (which would restart animation)
            }
            else
            {
                // It's ready, but too far away. Hide it.
                foodPrompts[i].PopOut();
            }
        }
    }
}