using UnityEngine;

[CreateAssetMenu(fileName = "NewPlotData", menuName = "Shop/Plot Data")]
public class PlotDataSO : ScriptableObject
{
    public int plotNumber;
    public int costInDiamonds;
}