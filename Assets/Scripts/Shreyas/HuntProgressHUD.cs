using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HuntProgressHUD : MonoBehaviour
{
    [Header("HUD Texts")]
    [SerializeField] private TMP_Text zonesText;
    [SerializeField] private TMP_Text zonesCountText;
    [SerializeField] private TMP_Text creaturesText;
    [SerializeField] private TMP_Text exitStatusText;

    [Header("Zones Progress Bar")]
    [SerializeField] private Image zonesFill;

    private int zonesUsed;
    private int creaturesAlive;
    private int totalZones;

    private void OnEnable()
    {
        // Count the mould zones in this level.
        totalZones = FindObjectsByType<MouldZone>().Length;

        // Listen for mould placement and creature events.
        MouldZone.MouldPlaced += OnMouldPlaced;
        MouldZone.CreatureSpawned += OnCreatureSpawned;

        IngredientCreature1.CreatureCaught += OnCreatureRemoved;
        IngredientCreature1.CreatureEscaped += OnCreatureRemoved;

        // Track creatures that split or are popped.
        DoughCreature.Split += OnCreatureSpawned;
        DoughCreature.CopyPopped += OnCreatureRemoved;

        // Listen for the level becoming empty.
        LevelTracker.LevelEmpty += OnLevelEmpty;

        UpdateZonesText();
        UpdateCreaturesText();

        // Hide the exit message until the level is empty.
        if (exitStatusText != null)
        {
            exitStatusText.text = "";
            exitStatusText.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from events.
        MouldZone.MouldPlaced -= OnMouldPlaced;
        MouldZone.CreatureSpawned -= OnCreatureSpawned;

        IngredientCreature1.CreatureCaught -= OnCreatureRemoved;
        IngredientCreature1.CreatureEscaped -= OnCreatureRemoved;

        DoughCreature.Split -= OnCreatureSpawned;
        DoughCreature.CopyPopped -= OnCreatureRemoved;

        LevelTracker.LevelEmpty -= OnLevelEmpty;
    }

    private void OnMouldPlaced(MouldZone zone)
    {
        zonesUsed++;

        UpdateZonesText();
    }

    private void OnCreatureSpawned(IngredientCreature1 creature)
    {
        creaturesAlive++;

        UpdateCreaturesText();
    }

    private void OnCreatureRemoved(IngredientCreature1 creature)
    {
        creaturesAlive = Mathf.Max(0, creaturesAlive - 1);

        UpdateCreaturesText();
    }

    private void UpdateZonesText()
    {
        // Display the title.
        if (zonesText != null)
        {
            zonesText.text = zonesCountText != null
                ? "Zones"
                : $"Zones {zonesUsed}/{totalZones}";
        }

        // Display the number to the right of the bar.
        if (zonesCountText != null)
        {
            zonesCountText.text = $"{zonesUsed}/{totalZones}";
        }

        // Fill the bar according to the number of zones used.
        if (zonesFill != null)
        {
            zonesFill.fillAmount = totalZones > 0
                ? Mathf.Clamp01((float)zonesUsed / totalZones)
                : 0f;
        }
    }

    private void UpdateCreaturesText()
    {
        if (creaturesText != null)
        {
            creaturesText.text = $"Creatures Left {creaturesAlive}";
        }
    }

    private void OnLevelEmpty(int caughtCount)
    {
        if (exitStatusText != null)
        {
            exitStatusText.text = "Exit open";
            exitStatusText.gameObject.SetActive(true);
        }
    }
}