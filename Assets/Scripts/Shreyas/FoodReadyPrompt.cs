using UnityEngine;
using UnityEngine.Serialization;

// SCRUM-519 stove / oven prompt.
// The green pin shows ONLY while the dish is ready (and the player is in range).
// After the dish is taken the pin pops out and ONE of the pickup sounds plays.
// Works with StoveController and OvenController (both implement IFoodReadyProvider).
public class FoodReadyPrompt : ProximityPromptBase
{
    public enum PickupSoundMode
    {
        RandomNoRepeat,   // random, but never the same clip twice in a row
        CycleInOrder      // 1, 2, 3, 1, 2, 3 ...
    }

    [Header("References")]
    [FormerlySerializedAs("stoveController")]
    [SerializeField] private MonoBehaviour foodProvider;   // StoveController or OvenController

    [Header("Range (same as the blink)")]
    [SerializeField] private RangeHighlight rangeHighlight;

    [Header("Pickup sounds (one is played each time the dish is taken)")]
    [SerializeField] private AudioClip[] pickupSounds;
    [SerializeField] private PickupSoundMode soundMode = PickupSoundMode.RandomNoRepeat;
    [Range(0f, 1f)]
    [SerializeField] private float pickupVolume = 1f;

    private IFoodReadyProvider provider;
    private int lastIndex = -1;

    // Only while the dish is ready. Idle and cooking stay hidden.
    protected override bool ConditionMet
    {
        get { return provider != null && provider.IsFoodReady; }
    }

    protected override void Awake()
    {
        base.Awake();

        if (rangeHighlight == null)
            rangeHighlight = GetComponent<RangeHighlight>();

        provider = foodProvider as IFoodReadyProvider;

        if (provider == null)
            Debug.LogError("FoodReadyPrompt: Food Provider is missing or doesn't implement IFoodReadyProvider!", this);
    }

    private void OnEnable()
    {
        if (provider != null) provider.FoodTaken += OnFoodTaken;
    }

    private void OnDisable()
    {
        if (provider != null) provider.FoodTaken -= OnFoodTaken;
    }

    private void OnFoodTaken()
    {
        AudioClip clip = PickClip();
        if (clip == null) return;

        // Play at the camera so the volume does not depend on how far the stove is.
        Camera cam = Camera.main;
        Vector3 pos = cam != null ? cam.transform.position : transform.position;
        AudioSource.PlayClipAtPoint(clip, pos, pickupVolume);
    }

    private AudioClip PickClip()
    {
        if (pickupSounds == null || pickupSounds.Length == 0) return null;

        int count = pickupSounds.Length;
        int index;

        if (count == 1)
        {
            index = 0;
        }
        else if (soundMode == PickupSoundMode.CycleInOrder)
        {
            index = (lastIndex + 1) % count;
        }
        else if (lastIndex < 0)
        {
            index = Random.Range(0, count);
        }
        else
        {
            // pick from the other clips only, so it never repeats back to back
            index = Random.Range(0, count - 1);
            if (index >= lastIndex) index++;
        }

        lastIndex = index;
        return pickupSounds[index];
    }

    protected override bool IsPlayerInRange()
    {
        // Blink range = prompt range = pickup range
        if (rangeHighlight != null) return rangeHighlight.PlayerInRange;
        return base.IsPlayerInRange();
    }
}