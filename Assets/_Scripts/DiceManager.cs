
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceManager : MonoBehaviour
{
    [Header("Dice Setup")]
    [SerializeField] private FloatingDie dicePrefab;
    [SerializeField] private int startingDice = 5;
    [SerializeField] private int maxDice = 10;

    [Header("Layout")]
    [SerializeField] private float diceSpacing = 1.5f;

    [Header("Slot Machine Animation")]
    [SerializeField] private float firstDieSpinDuration = 1f;
    [SerializeField] private float stopDelay = 0.35f;

    private readonly List<FloatingDie> dice =
        new List<FloatingDie>();

    private bool isRolling;

    public IReadOnlyList<FloatingDie> Dice => dice;
    public bool IsRolling => isRolling;
    public bool HasRolled { get; private set; }

    public event Action OnRollComplete;

    private void Start()
    {
        for (int i = 0; i < startingDice; i++)
        {
            AddDie();
        }
    }

    public void AddDie()
    {
        if (dice.Count >= maxDice)
            return;

        FloatingDie newDie = Instantiate(
            dicePrefab,
            transform
        );

        newDie.name = $"Die_{dice.Count + 1}";

        dice.Add(newDie);
        ArrangeDice();
    }

    private void ArrangeDice()
    {
        int count = dice.Count;
        float totalWidth = (count - 1) * diceSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            Vector3 position = new Vector3(
                startX + i * diceSpacing,
                0f,
                0f
            );

            dice[i].SetBasePosition(position);
        }
    }

    public bool CanRoll()
    {
        if (isRolling)
            return false;

        foreach (FloatingDie die in dice)
        {
            if (!die.IsHeld)
                return true;
        }

        return false;
    }

    public void RollAllDice()
    {
        if (!CanRoll())
            return;

        StartCoroutine(RollSequence());
    }

    private IEnumerator RollSequence()
    {
        isRolling = true;

        int rollingDieIndex = 0;

        // Start every unheld die spinning immediately.
        foreach (FloatingDie die in dice)
        {
            if (die.IsHeld)
                continue;

            // Each die spins longer than the previous one.
            float duration =
                firstDieSpinDuration + rollingDieIndex * stopDelay;

            die.Roll(duration);

            rollingDieIndex++;
        }

        // Wait until all dice have finished settling.
        bool anyRolling = true;

        while (anyRolling)
        {
            anyRolling = false;

            foreach (FloatingDie die in dice)
            {
                if (die.IsRolling)
                {
                    anyRolling = true;
                    break;
                }
            }

            if (anyRolling)
                yield return null;
        }

        isRolling = false;
        HasRolled = true;

        OnRollComplete?.Invoke();
    }

    public void ClearHolds()
    {
        foreach (FloatingDie die in dice)
        {
            die.SetHeld(false);
        }
    }

    public void ResetForSubmission()
    {
        ClearHolds();
        HasRolled = false;
    }

    public bool CanAddDie()
    {
        return dice.Count < maxDice;
    }

}
