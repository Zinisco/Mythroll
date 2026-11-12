
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlacksmithUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject blacksmithPanel;
    [SerializeField] private GameObject forgePanel;

    [Header("Gameplay UI")]
    [SerializeField] private GameObject gameUI;

    [Header("References")]
    [SerializeField] private MythrollGameManager gameManager;
    [SerializeField] private DiceManager diceManager;
    [SerializeField] private ForgeDieInspector forgeInspector;

    [Header("UI")]
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text addDieCostText;
    [SerializeField] private TMP_Text forgeCostText;
    [SerializeField] private TMP_Text selectedDieText;
    [SerializeField] private TMP_Text selectedFaceText;

    [Header("Buttons")]
    [SerializeField] private Button addDieButton;
    [SerializeField] private Button forgeFaceButton;
    [SerializeField] private Button forgeButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button changeDieButton;
    [SerializeField] private Button nextRoundButton;

    [Header("Forge Value")]
    [SerializeField] private TMP_Dropdown valueDropdown;

    [Header("Upgrade Costs")]
    [SerializeField] private int baseAddDieCost = 20;
    [SerializeField] private int baseForgeCost = 10;
    [SerializeField] private float addDieCostMultiplier = 1.5f;
    [SerializeField] private float forgeCostMultiplier = 1.25f;

    private int dicePurchased;
    private int facesForged;
    private Action onContinue;

    // True throughout the entire Blacksmith visit,
    // including when the main shop panel is hidden.
    public bool IsOpen { get; private set; }

    private int AddDieCost =>
        Mathf.CeilToInt(
            baseAddDieCost *
            Mathf.Pow(addDieCostMultiplier, dicePurchased)
        );

    private int ForgeCost =>
        Mathf.CeilToInt(
            baseForgeCost *
            Mathf.Pow(forgeCostMultiplier, facesForged)
        );

    private void Awake()
    {
        blacksmithPanel.SetActive(false);
        forgePanel.SetActive(false);

        addDieButton.onClick.AddListener(BuyDie);
        forgeFaceButton.onClick.AddListener(OpenForge);
        forgeButton.onClick.AddListener(BuyForge);
        backButton.onClick.AddListener(CloseForge);
        changeDieButton.onClick.AddListener(forgeInspector.ReturnDie);
        nextRoundButton.onClick.AddListener(Continue);

        gameManager.OnGoldChanged += RefreshUI;

        valueDropdown.ClearOptions();
        valueDropdown.AddOptions(new List<string>
        {
            "1", "2", "3", "4", "5", "6"
        });

        valueDropdown.onValueChanged.AddListener(
            _ => RefreshUI()
        );

        forgeInspector.OnSelectionChanged += RefreshUI;
    }

    private void OnDestroy()
    {
        if (forgeInspector != null)
            forgeInspector.OnSelectionChanged -= RefreshUI;

        if (gameManager != null)
            gameManager.OnGoldChanged -= RefreshUI;
    }

    public void Show(Action continueAction)
    {
        onContinue = continueAction;
        IsOpen = true;

        gameUI.SetActive(false);

        forgeInspector.DisableInspection();

        forgePanel.SetActive(false);
        blacksmithPanel.SetActive(true);

        RefreshUI();
    }

    private void OpenForge()
    {
        if (gameManager.Gold < ForgeCost)
            return;

        blacksmithPanel.SetActive(false);
        forgePanel.SetActive(true);

        forgeInspector.EnableInspection();

        RefreshUI();
    }

    private void CloseForge()
    {
        forgeInspector.DisableInspection();

        forgePanel.SetActive(false);
        blacksmithPanel.SetActive(true);

        RefreshUI();
    }

    private void BuyDie()
    {
        if (!diceManager.CanAddDie())
            return;

        if (!gameManager.TrySpendGold(AddDieCost))
            return;

        diceManager.AddDie();
        dicePurchased++;

        RefreshUI();
    }

    private void BuyForge()
    {
        FloatingDie die = forgeInspector.SelectedDie;
        int faceIndex = forgeInspector.SelectedFaceIndex;

        if (die == null || faceIndex < 0)
            return;

        int newValue = valueDropdown.value + 1;

        if (die.GetFaceValue(faceIndex) == newValue)
            return;

        if (!gameManager.TrySpendGold(ForgeCost))
            return;

        die.ForgeFace(faceIndex, newValue);
        facesForged++;

        forgeInspector.RefreshSelection();
    }

    private void RefreshUI()
    {
        if (gameManager == null ||
      diceManager == null ||
      forgeInspector == null)
        {
            Debug.LogError(
                "BlacksmithUI is missing a required reference!"
            );
            return;
        }

        goldText.text = $"Gold: {gameManager.Gold:N0}";

        bool canAddDie = diceManager.CanAddDie();

        addDieCostText.text = canAddDie
            ? $"{AddDieCost} Gold"
            : "MAX DICE";

        addDieButton.interactable =
            canAddDie && gameManager.Gold >= AddDieCost;

        forgeCostText.text = $"{ForgeCost} Gold";
        forgeFaceButton.interactable = gameManager.Gold >= ForgeCost;

        FloatingDie die = forgeInspector.SelectedDie;
        int faceIndex = forgeInspector.SelectedFaceIndex;

        bool hasDie = die != null;
        bool hasFace = hasDie && faceIndex >= 0;

        // Hide forge controls until a die is selected.
        valueDropdown.gameObject.SetActive(hasDie);
        forgeButton.gameObject.SetActive(hasDie);
        changeDieButton.gameObject.SetActive(hasDie);


        changeDieButton.interactable = hasDie;


        selectedDieText.text = hasDie
            ? $"Selected: {die.name}"
            : "Click a die to inspect it";

        selectedFaceText.text = hasFace
            ? $"Selected Face: {die.GetFaceValue(faceIndex)}"
            : "Left-click a face to select it";

        bool validForge = hasFace &&
            die.GetFaceValue(faceIndex) !=
            valueDropdown.value + 1;

        forgeButton.interactable =
            validForge && gameManager.Gold >= ForgeCost;
    }

    private void Continue()
    {
        forgeInspector.DisableInspection();

        forgePanel.SetActive(false);
        blacksmithPanel.SetActive(false);

        IsOpen = false;

        gameUI.SetActive(true);

        onContinue?.Invoke();
    }
}
