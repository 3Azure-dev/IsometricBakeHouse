using UnityEngine;

[CreateAssetMenu(fileName = "SO_DayNightSettings", menuName = "Bakery/Day Night Settings")]
public class DayNightSettings : ScriptableObject
{
    [Tooltip("Length of the Day Phase in seconds. GDD: 600 (10 minutes).")]
    public float dayLengthSeconds = 600f;

    [Tooltip("Length of the Night Phase in seconds. GDD: 300 (5 minutes).")]
    public float nightLengthSeconds = 300f;
}