using UnityEngine;

[CreateAssetMenu(fileName ="New Combat Art", menuName = "Combat Art")]
public class CombatArt : ScriptableObject
{
    public string artName;
    public string description;
    public float multiplier;
    public float delay1;
    public float delay2;
}
