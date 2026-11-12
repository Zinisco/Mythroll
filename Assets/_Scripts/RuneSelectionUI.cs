
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RuneSelectionUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private GameObject skipPanel;

    [Header("References")]
    [SerializeField] private RuneManager runeManager;

    [Header("Rune Choices")]
    [SerializeField] private Button[] runeButtons;
    [SerializeField] private TMP_Text[] runeNameTexts;
    [SerializeField] private TMP_Text[] runeDescriptionTexts;
    [SerializeField] private Image[] runeIcons;

    [Header("Skip Buttons")]
    [SerializeField] private Button skipButton;
    [SerializeField] private Button extraRollButton;
    [SerializeField] private Button extraSubmitButton;
    [SerializeField] private Button backButton;

    private List<RuneData> currentChoices;
    private Action onComplete;
    private Action onExtraRoll;
    private Action onExtraSubmit;
    private bool selectionMade;

    private void Awake()
    {
        selectionPanel.SetActive(false);
        skipPanel.SetActive(false);

        for (int i = 0; i < runeButtons.Length; i++)
        {
            int index = i;

            runeButtons[i].onClick.AddListener(
                () => SelectRune(index)
            );
        }

        skipButton.onClick.AddListener(OpenSkipOptions);
        backButton.onClick.AddListener(CloseSkipOptions);

        extraRollButton.onClick.AddListener(ChooseExtraRoll);
        extraSubmitButton.onClick.AddListener(ChooseExtraSubmit);
    }

    public void Show(
        Action completeAction,
        Action extraRollAction,
        Action extraSubmitAction)
    {
        onComplete = completeAction;
        onExtraRoll = extraRollAction;
        onExtraSubmit = extraSubmitAction;

        selectionMade = false;

        currentChoices = runeManager.GetRandomRuneChoices(3);

        selectionPanel.SetActive(true);
        skipPanel.SetActive(false);

        RefreshChoices();
    }

    private void RefreshChoices()
    {
        for (int i = 0; i < runeButtons.Length; i++)
        {
            bool hasRune = i < currentChoices.Count;

            runeButtons[i].gameObject.SetActive(hasRune);

            if (!hasRune)
                continue;

            RuneData rune = currentChoices[i];

            runeNameTexts[i].text = rune.runeName;
            runeDescriptionTexts[i].text = rune.description;

            if (runeIcons != null && i < runeIcons.Length &&
                runeIcons[i] != null)
            {
                runeIcons[i].sprite = rune.icon;
                runeIcons[i].enabled = rune.icon != null;
            }
        }
    }

    private void SelectRune(int index)
    {
        if (selectionMade || index >= currentChoices.Count)
            return;

        RuneData selectedRune = currentChoices[index];

        if (!runeManager.AddRune(selectedRune))
            return;

        FinishSelection();
    }

    private void OpenSkipOptions()
    {
        selectionPanel.SetActive(false);
        skipPanel.SetActive(true);
    }

    private void CloseSkipOptions()
    {
        skipPanel.SetActive(false);
        selectionPanel.SetActive(true);
    }

    private void ChooseExtraRoll()
    {
        if (selectionMade)
            return;

        onExtraRoll?.Invoke();
        FinishSelection();
    }

    private void ChooseExtraSubmit()
    {
        if (selectionMade)
            return;

        onExtraSubmit?.Invoke();
        FinishSelection();
    }

    private void FinishSelection()
    {
        if (selectionMade)
            return;

        selectionMade = true;

        selectionPanel.SetActive(false);
        skipPanel.SetActive(false);

        onComplete?.Invoke();
    }
}
