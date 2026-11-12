
using System.Text;
using UnityEngine;

public class ScoringManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DiceManager diceManager;
    [SerializeField] private RuneManager runeManager;

    private readonly int[] matchBonuses =
    {
        0, 0, 10, 40, 150, 500,
        1500, 5000, 15000, 50000, 150000
    };

    private readonly int[] goldRewards =
    {
        1, 1, 2, 5, 10, 20,
        35, 50, 75, 100, 150
    };

    private int[] GetDiceCounts()
    {
        int[] counts = new int[7];

        foreach (FloatingDie die in diceManager.Dice)
        {
            counts[die.CurrentValue]++;
        }

        return counts;
    }

    public int CalculateScore()
    {
        int[] counts = GetDiceCounts();

        int total = 0;
        int matchingGroups = 0;

        int multiplier = 1;

        for (int face = 1; face <= 6; face++)
        {
            int matches = counts[face];

            if (matches < 2)
                continue;

            matchingGroups++;

            int matchBonus = matchBonuses[matches];
            int diceValue = face * matches;

            // Apply rune effects to this matching group.
            foreach (RuneData rune in runeManager.ActiveRunes)
            {
                switch (rune.effectType)
                {
                    case RuneEffectType.SixesBonus:
                        if (face == 6)
                            diceValue *= rune.effectValue;
                        break;

                    case RuneEffectType.PairBonus:
                        if (matches == 2)
                            matchBonus += rune.effectValue;
                        break;

                    case RuneEffectType.FourOfAKindBonus:
                        if (matches >= 4)
                            matchBonus += rune.effectValue;
                        break;

                    case RuneEffectType.OddBonus:
                        if (face % 2 != 0)
                            matchBonus += rune.effectValue;
                        break;
                }
            }

            total += matchBonus + diceValue;
        }

        if (matchingGroups >= 2)
        {
            multiplier = 1 << (matchingGroups - 1);

            foreach (RuneData rune in runeManager.ActiveRunes)
            {
                if (rune.effectType == RuneEffectType.MultiplierBonus)
                    multiplier += rune.effectValue;
            }
        }

        return total * multiplier;
    }

    public int CalculateGold()
    {
        int[] counts = GetDiceCounts();

        int largestMatch = 0;

        for (int face = 1; face <= 6; face++)
        {
            if (counts[face] > largestMatch)
                largestMatch = counts[face];
        }

        int gold = goldRewards[largestMatch];

        foreach (RuneData rune in runeManager.ActiveRunes)
        {
            if (rune.effectType == RuneEffectType.ExtraGold)
                gold += rune.effectValue;
        }

        return gold;
    }

    public string GetCombinationBreakdown()
    {
        int[] counts = GetDiceCounts();

        StringBuilder matchBonusText = new StringBuilder();
        StringBuilder diceValueText = new StringBuilder();

        int matchingGroups = 0;
        int multiplier = 1;

        for (int face = 1; face <= 6; face++)
        {
            int matches = counts[face];

            if (matches < 2)
                continue;

            if (matchingGroups > 0)
            {
                matchBonusText.Append(" + ");
                diceValueText.Append(" + ");
            }

            int matchBonus = matchBonuses[matches];
            int diceValue = face * matches;

            foreach (RuneData rune in runeManager.ActiveRunes)
            {
                switch (rune.effectType)
                {
                    case RuneEffectType.SixesBonus:
                        if (face == 6)
                            diceValue *= rune.effectValue;
                        break;

                    case RuneEffectType.PairBonus:
                        if (matches == 2)
                            matchBonus += rune.effectValue;
                        break;

                    case RuneEffectType.FourOfAKindBonus:
                        if (matches >= 4)
                            matchBonus += rune.effectValue;
                        break;

                    case RuneEffectType.OddBonus:
                        if (face % 2 != 0)
                            matchBonus += rune.effectValue;
                        break;
                }
            }

            matchBonusText.Append(matchBonus.ToString("N0"));
            diceValueText.Append(diceValue.ToString("N0"));

            matchingGroups++;
        }

        if (matchingGroups == 0)
            return "Current Combinations\n\nNo Matches";

        if (matchingGroups >= 2)
        {
            multiplier = 1 << (matchingGroups - 1);

            foreach (RuneData rune in runeManager.ActiveRunes)
            {
                if (rune.effectType == RuneEffectType.MultiplierBonus)
                    multiplier += rune.effectValue;
            }
        }

        return
            "Current Combinations\n\n" +
            $"Match Bonus: {matchBonusText}\n" +
            $"Dice Value: {diceValueText}\n" +
            $"Multiplier: ×{multiplier}";
    }
}
