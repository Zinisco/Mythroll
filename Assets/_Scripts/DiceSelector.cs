
using UnityEngine;
using UnityEngine.InputSystem;

public class DiceSelector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private DiceManager diceManager;

    [Header("Raycast")]
    [SerializeField] private LayerMask diceLayer = ~0;
    [SerializeField] private float raycastDistance = 100f;

    [Header("Blacksmith")]
    [SerializeField] private BlacksmithUI blacksmithUI;

    private void Update()
    {
        if (blacksmithUI != null && blacksmithUI.IsOpen)
            return;

        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Debug.Log("Mouse click detected!");

        if (diceManager.IsRolling)
        {
            Debug.Log("Cannot hold while rolling.");
            return;
        }

        if (!diceManager.HasRolled)
        {
            Debug.Log("Roll dice before holding.");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        Debug.DrawRay(
            ray.origin,
            ray.direction * raycastDistance,
            Color.red,
            2f
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            raycastDistance,
            diceLayer))
        {
            Debug.Log("Raycast hit: " + hit.collider.name);

            FloatingDie die =
                hit.collider.GetComponentInParent<FloatingDie>();

            if (die != null)
            {
                die.SetHeld(!die.IsHeld);

                Debug.Log(
                    $"{die.name} held: {die.IsHeld}"
                );
            }
            else
            {
                Debug.Log(
                    "Hit object does not have FloatingDie."
                );
            }
        }
        else
        {
            Debug.Log("Raycast hit nothing.");
        }
    }
}
