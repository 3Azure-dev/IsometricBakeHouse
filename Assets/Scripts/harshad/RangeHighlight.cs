using System.Collections.Generic;
using UnityEngine;

public class RangeHighlight : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float pulseSpeed = 3f;

    private readonly HashSet<Collider> playerColliders = new HashSet<Collider>();
    
    // True while the player is inside the blink range.
    public bool PlayerInRange => playerColliders.Count > 0;

    private MaterialPropertyBlock block;
    private int[] colorProperties;   // one colour property per material
    private Color[] originalColors;  // one original colour per material

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        block = new MaterialPropertyBlock();

        if (targetRenderer == null) return;

        Material[] mats = targetRenderer.sharedMaterials;
        colorProperties = new int[mats.Length];
        originalColors = new Color[mats.Length];

        for (int i = 0; i < mats.Length; i++)
        {
            Material m = mats[i];
            if (m == null) continue;

            if (m.HasProperty("_BaseColor"))
                colorProperties[i] = Shader.PropertyToID("_BaseColor");
            else if (m.HasProperty("_Color"))
                colorProperties[i] = Shader.PropertyToID("_Color");

            if (colorProperties[i] != 0)
                originalColors[i] = m.GetColor(colorProperties[i]);
        }
    }

    public void SetPlayerInRange(Collider playerCollider, bool inRange)
    {
        if (inRange) playerColliders.Add(playerCollider);
        else playerColliders.Remove(playerCollider);

        if (playerColliders.Count == 0)
            ClearHighlight();
    }

    private void Update()
    {
        if (playerColliders.Count == 0 || targetRenderer == null || colorProperties == null)
            return;

        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        for (int i = 0; i < colorProperties.Length; i++)
        {
            if (colorProperties[i] == 0) continue;
            targetRenderer.GetPropertyBlock(block, i);
            block.SetColor(colorProperties[i], Color.Lerp(originalColors[i], highlightColor, pulse));
            targetRenderer.SetPropertyBlock(block, i);
        }
    }

    // Puts every material back to its own original colour
    private void ClearHighlight()
    {
        if (targetRenderer == null || colorProperties == null) return;

        for (int i = 0; i < colorProperties.Length; i++)
        {
            if (colorProperties[i] == 0) continue;
            targetRenderer.GetPropertyBlock(block, i);
            block.SetColor(colorProperties[i], originalColors[i]);
            targetRenderer.SetPropertyBlock(block, i);
        }
    }
}