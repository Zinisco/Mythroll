
using System.Collections;
using UnityEngine;
using TMPro;

public class FloatingDie : MonoBehaviour
{
    [Header("Model")]
    [SerializeField] private Transform rotationPivot;

    [Header("Physical Face Anchors")]
    [SerializeField] private Transform[] faces = new Transform[6];

    // NEW: TextMeshPro numbers on each face.
    [Header("Face Numbers")]
    [SerializeField] private TMP_Text[] faceLabels = new TMP_Text[6];

    // NEW: Highlight colors.
    [Header("Forge Highlight")]
    [SerializeField] private Color normalFaceColor = Color.black;
    [SerializeField]
    private Color selectedFaceColor =
        new Color(1f, 0.75f, 0.2f);

    [Header("Animation")]
    [SerializeField] private float spinDuration = 1f;
    [SerializeField] private float settleDuration = 0.35f;
    [SerializeField] private float spinSpeed = 900f;

    [Header("Floating")]
    [SerializeField] private float floatHeight = 0.1f;
    [SerializeField] private float floatSpeed = 2f;

    [Header("Hold Visuals")]
    [SerializeField] private Renderer diceRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField]
    private Color heldColor =
        new Color(1f, 0.75f, 0.2f);
    [SerializeField] private float floatTransitionSpeed = 5f;

    public Transform RotationPivot => rotationPivot;

    private bool isInspecting;
    private Vector3 normalScale;
    private Quaternion rotationBeforeInspection;

    private float currentFloatHeight;
    private Material diceMaterial;

    private Vector3 startingPosition;
    private float floatOffset;

    private int[] faceValues = { 1, 2, 3, 4, 5, 6 };

    // NEW: Tracks which face is highlighted.
    private int highlightedFaceIndex = -1;

    private bool isRolling;

    public int CurrentValue { get; private set; }
    public int CurrentFaceIndex { get; private set; }

    public bool IsRolling => isRolling;
    public bool IsHeld { get; private set; }

    private void Awake()
    {
        startingPosition = transform.localPosition;

        floatOffset = Random.Range(0f, Mathf.PI * 2f);

        currentFloatHeight = floatHeight;
        normalScale = transform.localScale;

        if (diceRenderer != null)
        {
            diceMaterial = diceRenderer.material;
            diceMaterial.color = normalColor;
        }

        // NEW: Initialize the visible face numbers.
        UpdateFaceLabels();
    }

    private void Update()
    {
        FloatAnimation();
    }

    private void FloatAnimation()
    {
        if (isInspecting)
            return;

        float targetHeight = IsHeld ? 0f : floatHeight;

        currentFloatHeight = Mathf.MoveTowards(
            currentFloatHeight,
            targetHeight,
            floatTransitionSpeed * Time.deltaTime
        );

        float offset = Mathf.Sin(
            Time.time * floatSpeed + floatOffset
        ) * currentFloatHeight;

        transform.localPosition = startingPosition +
            Vector3.up * offset;
    }

    public void Roll(float customSpinDuration = -1f)
    {
        if (isRolling || IsHeld)
            return;

        int faceIndex = Random.Range(0, 6);

        float duration = customSpinDuration > 0f
            ? customSpinDuration
            : spinDuration;

        StartCoroutine(AnimateRoll(faceIndex, duration));
    }

    private IEnumerator AnimateRoll(int faceIndex, float duration)
    {
        isRolling = true;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float dt = Time.deltaTime;
            elapsed += dt;

            rotationPivot.Rotate(
                Vector3.up,
                spinSpeed * dt,
                Space.Self
            );

            rotationPivot.Rotate(
                Vector3.right,
                spinSpeed * 0.7f * dt,
                Space.Self
            );

            yield return null;
        }

        Transform selectedFace = faces[faceIndex];

        Vector3 localNormal =
            selectedFace.localRotation * Vector3.forward;

        Quaternion targetRotation =
            Quaternion.FromToRotation(
                localNormal,
                Vector3.back
            );

        Quaternion startRotation = rotationPivot.localRotation;

        elapsed = 0f;

        while (elapsed < settleDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / settleDuration
            );

            t = Mathf.SmoothStep(0f, 1f, t);

            rotationPivot.localRotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                t
            );

            yield return null;
        }

        rotationPivot.localRotation = targetRotation;

        CurrentFaceIndex = faceIndex;
        CurrentValue = faceValues[faceIndex];

        isRolling = false;

        Debug.Log($"Rolled {CurrentValue}");
    }

    public void SetHeld(bool held)
    {
        if (isRolling)
            return;

        IsHeld = held;

        if (diceMaterial != null)
        {
            diceMaterial.color = IsHeld
                ? heldColor
                : normalColor;
        }
    }

    public void ForgeFace(int faceIndex, int newValue)
    {
        if (faceIndex < 0 || faceIndex >= faceValues.Length)
            return;

        if (newValue < 1 || newValue > 6)
            return;

        faceValues[faceIndex] = newValue;

        // NEW: Immediately update the physical number.
        UpdateFaceLabels();

        if (CurrentFaceIndex == faceIndex)
        {
            CurrentValue = newValue;
        }
    }

    // NEW: Refreshes all six visible numbers.
    private void UpdateFaceLabels()
    {
        for (int i = 0; i < faceValues.Length; i++)
        {
            if (i >= faceLabels.Length || faceLabels[i] == null)
                continue;

            faceLabels[i].text = faceValues[i].ToString();

            faceLabels[i].color =
                i == highlightedFaceIndex
                    ? selectedFaceColor
                    : normalFaceColor;
        }
    }

    // NEW: Highlight the selected face.
    public void HighlightFace(int faceIndex)
    {
        highlightedFaceIndex =
            faceIndex >= 0 && faceIndex < faceValues.Length
                ? faceIndex
                : -1;

        UpdateFaceLabels();
    }

    // NEW: Remove the highlight.
    public void ClearFaceHighlight()
    {
        highlightedFaceIndex = -1;
        UpdateFaceLabels();
    }

    public void SetBasePosition(Vector3 position)
    {
        startingPosition = position;
        transform.localPosition = position;
    }

    public int GetFaceValue(int faceIndex)
    {
        if (faceIndex < 0 || faceIndex >= faceValues.Length)
            return 0;

        return faceValues[faceIndex];
    }

    public void BeginInspection(
        Vector3 position,
        float scaleMultiplier)
    {
        if (isRolling)
            return;

        isInspecting = true;
        rotationBeforeInspection = rotationPivot.localRotation;

        transform.localPosition = position;
        transform.localScale = normalScale * scaleMultiplier;
    }

    public void EndInspection()
    {
        isInspecting = false;

        rotationPivot.localRotation = rotationBeforeInspection;
        transform.localScale = normalScale;
        transform.localPosition = startingPosition;
    }

    public int GetClickedFace(Vector3 hitNormal)
    {
        int bestFace = -1;
        float bestDot = -1f;

        for (int i = 0; i < faces.Length; i++)
        {
            if (faces[i] == null)
                continue;

            Vector3 faceNormal = faces[i].forward;

            float dot = Vector3.Dot(
                faceNormal,
                hitNormal
            );

            if (dot > bestDot)
            {
                bestDot = dot;
                bestFace = i;
            }
        }

        return bestDot > 0.7f ? bestFace : -1;
    }
}
