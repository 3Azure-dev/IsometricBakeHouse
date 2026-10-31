using UnityEngine;
using TMPro;

public class ResourceUI : MonoBehaviour
{
    public PlayerResources resources;
    public TMP_Text sackCountText;
    public TMP_Text coinCountText;

    void Update()
    {
        sackCountText.text = resources.sacks.ToString();
        coinCountText.text = resources.coins.ToString();
    }
}