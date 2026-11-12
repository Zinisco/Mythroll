
using System;
using System.Collections.Generic;
using UnityEngine;

public class RuneManager : MonoBehaviour
{
    [Header("Available Runes")]
    [SerializeField]
    private List<RuneData> runePool =
        new List<RuneData>();

    private readonly List<RuneData> activeRunes =
        new List<RuneData>();

    public IReadOnlyList<RuneData> ActiveRunes => activeRunes;

    public event Action OnRunesChanged;

    public bool HasRune(RuneData rune)
    {
        return rune != null && activeRunes.Contains(rune);
    }

    public bool AddRune(RuneData rune)
    {
        if (rune == null || HasRune(rune))
            return false;

        activeRunes.Add(rune);

        OnRunesChanged?.Invoke();

        Debug.Log($"Rune acquired: {rune.runeName}");

        return true;
    }

    public List<RuneData> GetRandomRuneChoices(int count = 3)
    {
        List<RuneData> availableRunes = new List<RuneData>();

        foreach (RuneData rune in runePool)
        {
            if (rune != null &&
                !HasRune(rune) &&
                !availableRunes.Contains(rune))
            {
                availableRunes.Add(rune);
            }
        }

        // Shuffle available runes.
        for (int i = availableRunes.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);

            RuneData temp = availableRunes[i];
            availableRunes[i] = availableRunes[randomIndex];
            availableRunes[randomIndex] = temp;
        }

        // Return up to the requested number of choices.
        int choiceCount = Mathf.Min(
            Mathf.Max(0, count),
            availableRunes.Count
        );

        return availableRunes.GetRange(0, choiceCount);
    }

    public void ResetRunes()
    {
        activeRunes.Clear();

        OnRunesChanged?.Invoke();
    }
}
