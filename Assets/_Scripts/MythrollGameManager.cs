
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class MythrollGameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DiceManager diceManager;
    [SerializeField] private ScoringManager scoringManager;

    [Header("Buttons")]
    [SerializeField] private Button rollButton;
    [SerializeField] private Button submitButton;

    [Header("UI")]
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text rollsText;
    [SerializeField] private TMP_Text submitsText;
    [SerializeField] private TMP_Text combinationBreakdownText;

    [Header("Efficiency Rewards")]
    [SerializeField] private int goldPerUnusedRoll = 1;
    [SerializeField] private int goldPerUnusedSubmit = 5;

    [Header("Blacksmith")]
    [SerializeField] private BlacksmithUI blacksmithUI;

    public int Gold => gold;

    [Header("Round Settings")]
    [SerializeField] private int startingRolls = 8;
    [SerializeField] private int startingSubmits = 3;
    [SerializeField]
    private int[] roundTargets =
        { 40, 150, 400, 1000, 2500 };

    [Header("Round Win Screen")]
    [SerializeField] private RoundWinUI roundWinUI;


    [Header("Permanent Run Upgrades")]
    [SerializeField] private int bonusRolls;
    [SerializeField] private int bonusSubmits;

    [Header("Rune Selection")]
    [SerializeField] private RuneSelectionUI runeSelectionUI;


    public event System.Action OnGoldChanged;

    private int currentRound;
    private int roundScore;
    private int gold;
    private int rollsLeft;
    private int submitsLeft;

    private bool roundComplete;
    private bool gameOver;

    private void Start()
    {
        rollButton.onClick.AddListener(TryRoll);
        submitButton.onClick.AddListener(SubmitDice);

        diceManager.OnRollComplete += HandleRollComplete;

        StartRound();
    }

    private void OnDestroy()
    {
        if (diceManager != null)
            diceManager.OnRollComplete -= HandleRollComplete;
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TryRoll();
        }
    }

    private void StartRound()
    {
        roundScore = 0;
        rollsLeft = startingRolls + bonusRolls;
        submitsLeft = startingSubmits + bonusSubmits;

        roundComplete = false;

        diceManager.ResetForSubmission();

        UpdateUI();
    }

    private void TryRoll()
    {
        if (gameOver || roundComplete)
            return;

        if (rollsLeft <= 0 || submitsLeft <= 0)
            return;

        if (!diceManager.CanRoll())
            return;

        rollsLeft--;

        diceManager.RollAllDice();

        UpdateUI();
    }

    private void HandleRollComplete()
    {
        UpdateUI();
    }

    private void SubmitDice()
    {
        if (gameOver || roundComplete)
            return;

        if (diceManager.IsRolling ||
            !diceManager.HasRolled ||
            submitsLeft <= 0)
            return;

        int points = scoringManager.CalculateScore();
        int earnedGold = scoringManager.CalculateGold();

        roundScore += points;
        gold += earnedGold;
        OnGoldChanged?.Invoke();
        submitsLeft--;

        diceManager.ResetForSubmission();

        if (roundScore >= roundTargets[currentRound])
        {
            CompleteRound();
        }
        else if (submitsLeft == 0 || rollsLeft == 0)
        {
            gameOver = true;
        }

        UpdateUI();
    }


    private void CompleteRound()
    {
        roundComplete = true;

        // Calculate efficiency rewards.
        int rollBonus = rollsLeft * goldPerUnusedRoll;
        int submitBonus = submitsLeft * goldPerUnusedSubmit;

        // Add rewards to the player's gold.
        gold += rollBonus + submitBonus;

        OnGoldChanged?.Invoke();

        bool finalRound =
            currentRound == roundTargets.Length - 1;

        if (finalRound)
            gameOver = true;

        // Pass the calculated bonuses to the victory screen.
        roundWinUI.Show(
            currentRound + 1,
            roundScore,
            roundTargets[currentRound],
            rollsLeft,
            submitsLeft,
            rollBonus,
            submitBonus,
            gold,
            finalRound,
            OpenRuneSelection
        );
    }
    private void UpdateUI()
    {
        roundText.text =
            $"Round {currentRound + 1} / {roundTargets.Length}";

        scoreText.text =
            $"Score: {roundScore:N0} / {roundTargets[currentRound]:N0}";

        if (diceManager.HasRolled &&
       !diceManager.IsRolling &&
       !roundComplete &&
       !gameOver)
        {
            combinationBreakdownText.text =
    scoringManager.GetCombinationBreakdown();
        }
        else
        {
            combinationBreakdownText.text =
                "Current Combinations\n\n—";
        }

        goldText.text = $"Gold: {gold:N0}";
        rollsText.text = $"Rolls: {rollsLeft}";
        submitsText.text = $"Submits: {submitsLeft}";

        rollButton.interactable =
            !gameOver &&
            !roundComplete &&
            rollsLeft > 0 &&
            submitsLeft > 0 &&
            diceManager.CanRoll();

        submitButton.interactable =
            !gameOver &&
            !roundComplete &&
            !diceManager.IsRolling &&
            diceManager.HasRolled &&
            submitsLeft > 0;
    }

    public bool TrySpendGold(int amount)
    {
        if (amount < 0 || gold < amount)
            return false;

        gold -= amount;

        UpdateUI();
        OnGoldChanged?.Invoke();

        return true;
    }


    private void NextRound()
    {
        if (!roundComplete || gameOver)
            return;

        currentRound++;
        StartRound();
    }


    private void OpenBlacksmith()
    {
        if (!roundComplete || gameOver)
            return;

        blacksmithUI.Show(NextRound);
    }


    private void OpenRuneSelection()
    {
        if (!roundComplete || gameOver)
            return;

        runeSelectionUI.Show(
            OpenBlacksmith,
            AddPermanentRoll,
            AddPermanentSubmit
        );
    }

    private void AddPermanentRoll()
    {
        bonusRolls++;

        Debug.Log(
            $"Permanent roll upgrade! Bonus rolls: {bonusRolls}"
        );
    }

    private void AddPermanentSubmit()
    {
        bonusSubmits++;

        Debug.Log(
            $"Permanent submit upgrade! Bonus submits: {bonusSubmits}"
        );
    }

}
