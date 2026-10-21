using UnityEngine;

[CreateAssetMenu(fileName ="New Combat Art", menuName = "Combat Art")]
public class CombatArt : ScriptableObject
{
    public string artName;
    public string description;
    public float damageMultiplier;
    public string debuffStat;
    public float debuffMultiplier;
    public float delay1;
    public float delay2;
}
