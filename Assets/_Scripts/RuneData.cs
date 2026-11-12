
using UnityEngine;

public enum RuneEffectType
{
    SixesBonus,
    PairBonus,
    FourOfAKindBonus,
    OddBonus,
    ExtraGold,
    MultiplierBonus
}

[CreateAssetMenu(
    fileName = "NewRune",
    menuName = "Mythroll/Rune"
)]
public class RuneData : ScriptableObject
{
    [Header("Rune Information")]
    public string runeName;

    [TextArea(2, 4)]
    public string description;

    public Sprite icon;

    [Header("Rune Effect")]
    public RuneEffectType effectType;

    [Min(0)]
    public int effectValue = 10;
}
