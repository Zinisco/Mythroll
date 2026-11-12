
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class ForgeDieInspector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private DiceManager diceManager;

    [Header("Inspection")]
    [SerializeField] private Transform inspectionPoint;
    [SerializeField] private float inspectionScale = 2f;
    [SerializeField] private float rotationSpeed = 0.3f;

    [Header("Raycast")]
    [SerializeField] private LayerMask diceLayer = ~0;
    [SerializeField] private float raycastDistance = 100f;

    public FloatingDie SelectedDie { get; private set; }
    public int SelectedFaceIndex { get; private set; } = -1;
    public bool IsActive { get; private set; }

    public event Action OnSelectionChanged;

    private Vector3 inspectionLocalPosition;

    public void EnableInspection()
    {
        IsActive = true;
        SelectedFaceIndex = -1;

        OnSelectionChanged?.Invoke();
    }

    public void DisableInspection()
    {
        if (!IsActive && SelectedDie == null)
            return;

        IsActive = false;
        ReturnDie();
    }

    private void Update()
    {
        if (!IsActive || Mouse.current == null)
            return;

        if (SelectedDie != null &&
            Mouse.current.rightButton.isPressed)
        {
            RotateSelectedDie();
        }

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = mainCamera.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            raycastDistance,
            diceLayer))
            return;

        FloatingDie clickedDie =
            hit.collider.GetComponentInParent<FloatingDie>();

        if (clickedDie == null)
            return;

        if (SelectedDie == null)
        {
            SelectDie(clickedDie);
        }
        else if (clickedDie == SelectedDie)
        {
            SelectFace(hit.normal);
        }
    }

    private void SelectDie(FloatingDie die)
    {
        SelectedDie = die;
        SelectedFaceIndex = -1;

        inspectionLocalPosition =
            diceManager.transform.InverseTransformPoint(
                inspectionPoint.position
            );

        SelectedDie.BeginInspection(
            inspectionLocalPosition,
            inspectionScale
        );

        foreach (FloatingDie other in diceManager.Dice)
        {
            if (other != SelectedDie)
                other.gameObject.SetActive(false);
        }

        OnSelectionChanged?.Invoke();
    }

    private void RotateSelectedDie()
    {
        Vector2 mouseDelta =
            Mouse.current.delta.ReadValue();

        Transform pivot = SelectedDie.RotationPivot;

        pivot.Rotate(
            mainCamera.transform.up,
            -mouseDelta.x * rotationSpeed,
            Space.World
        );

        pivot.Rotate(
            mainCamera.transform.right,
            mouseDelta.y * rotationSpeed,
            Space.World
        );
    }

    // UPDATED: Highlight the face the player clicks.
    private void SelectFace(Vector3 hitNormal)
    {
        if (SelectedDie == null)
            return;

        SelectedFaceIndex =
            SelectedDie.GetClickedFace(hitNormal);

        SelectedDie.HighlightFace(SelectedFaceIndex);

        OnSelectionChanged?.Invoke();
    }

    // UPDATED: Clear highlight before returning the die.
    public void ReturnDie()
    {
        if (SelectedDie != null)
        {
            SelectedDie.ClearFaceHighlight();
            SelectedDie.EndInspection();
        }

        foreach (FloatingDie die in diceManager.Dice)
        {
            die.gameObject.SetActive(true);
        }

        SelectedDie = null;
        SelectedFaceIndex = -1;

        OnSelectionChanged?.Invoke();
    }

    public void RefreshSelection()
    {
        OnSelectionChanged?.Invoke();
    }
}
