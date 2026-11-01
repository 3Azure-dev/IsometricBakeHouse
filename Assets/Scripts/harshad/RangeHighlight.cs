using System.Collections.Generic;
using UnityEngine;

public class RangeHighlight : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float pulseSpeed = 3f;

    private readonly HashSet<Collider> playerColliders = new HashSet<Collider>();
    private MaterialPropertyBlock block;
    private int colorProperty;
    private Color originalColor;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        block = new MaterialPropertyBlock();

        if (targetRenderer != null && targetRenderer.sharedMaterial != null)
        {
            Material material = targetRenderer.sharedMaterial;

            if (material.HasProperty("_BaseColor"))
                colorProperty = Shader.PropertyToID("_BaseColor");
            else if (material.HasProperty("_Color"))
                colorProperty = Shader.PropertyToID("_Color");
            else
                Debug.LogWarning("The object's material has no _BaseColor or _Color property.", this);

            if (colorProperty != 0)
                originalColor = material.GetColor(colorProperty);
        }
    }

    public void SetPlayerInRange(Collider playerCollider, bool inRange)
    {
        if (inRange)
            playerColliders.Add(playerCollider);
        else
            playerColliders.Remove(playerCollider);

        if (playerColliders.Count == 0)
            ApplyColor(originalColor);
    }

    private void Update()
    {
        if (playerColliders.Count == 0 || targetRenderer == null || colorProperty == 0)
            return;

        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        ApplyColor(Color.Lerp(originalColor, highlightColor, pulse));
    }

    private void ApplyColor(Color color)
    {
        if (targetRenderer == null || colorProperty == 0)
            return;

        targetRenderer.GetPropertyBlock(block);
        block.SetColor(colorProperty, color);
        targetRenderer.SetPropertyBlock(block);
    }
}