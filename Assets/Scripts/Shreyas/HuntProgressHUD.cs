using TMPro;
using UnityEngine;

public class HuntProgressHUD : MonoBehaviour
{
    [Header("HUD Texts")]
    [SerializeField] private TMP_Text zonesText;
    [SerializeField] private TMP_Text creaturesText;
    [SerializeField] private TMP_Text exitStatusText;

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

        // Also track creatures that split or are popped.
        DoughCreature.Split += OnCreatureSpawned;
        DoughCreature.CopyPopped += OnCreatureRemoved;

        // Listen for the level becoming empty.
        LevelTracker.LevelEmpty += OnLevelEmpty;

        UpdateZonesText();
        UpdateCreaturesText();

        // Keep the exit message hidden until the level is empty.
        if (exitStatusText != null)
        {
            exitStatusText.text = "";
            exitStatusText.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        // Stop listening when this HUD is disabled.
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
        if (zonesText != null)
        {
            zonesText.text = $"Zones {zonesUsed}/{totalZones}";
        }
    }

    private void UpdateCreaturesText()
    {
        if (creaturesText != null)
        {
            creaturesText.text = $"Creatures {creaturesAlive}";
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