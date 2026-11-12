
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class RoundWinUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject roundWinPanel;

    [Header("Text")]
    [SerializeField] private TMP_Text victoryTitleText;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text rollBonusText;
    [SerializeField] private TMP_Text submitBonusText;
    [SerializeField] private TMP_Text totalBonusText;
    [SerializeField] private TMP_Text totalGoldText;

    [Header("Button")]
    [SerializeField] private Button nextRoundButton;

    private Action onContinue;

    private void Awake()
    {
        roundWinPanel.SetActive(false);

        nextRoundButton.onClick.AddListener(Continue);
    }


    public void Show(
        int round,
        int finalScore,
        int target,
        int unusedRolls,
        int unusedSubmits,
        int rollBonus,
        int submitBonus,
        int totalGold,
        bool finalRound,
        Action continueAction)
    {
        onContinue = continueAction;

        victoryTitleText.text = finalRound
            ? "MYTHROLL COMPLETE!"
            : $"ROUND {round} CLEARED!";

        finalScoreText.text =
            $"{finalScore:N0} / {target:N0}";

        rollBonusText.text =
     $"{unusedRolls} x " +
     $"{(unusedRolls > 0 ? rollBonus / unusedRolls : 0)} Gold " +
     $"= {rollBonus} Gold";

        submitBonusText.text =
            $"{unusedSubmits} x " +
            $"{(unusedSubmits > 0 ? submitBonus / unusedSubmits : 0)} Gold " +
            $"= {submitBonus} Gold";

        totalBonusText.text =
            $"{rollBonus + submitBonus} Gold";

        totalGoldText.text =
            $"{totalGold:N0}";

        nextRoundButton.gameObject.SetActive(!finalRound);

        roundWinPanel.SetActive(true);
    }

    private void Continue()
    {
        roundWinPanel.SetActive(false);

        onContinue?.Invoke();
    }
}
