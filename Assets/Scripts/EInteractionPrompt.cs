using UnityEngine;

public class EInteractionPrompt : MonoBehaviour
{
    public GameObject prompt;
    public Transform player;
    public float interactionDistance = 2f;

    public Vector3 promptOffset = new Vector3(0f, 2f, 0f);

    void Update()
    {
        if (player == null || prompt == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= interactionDistance)
        {
            prompt.SetActive(true);

            // Move the E above the creature
            prompt.transform.position = transform.position + promptOffset;
        }
        else
        {
            prompt.SetActive(false);
        }
    }
}