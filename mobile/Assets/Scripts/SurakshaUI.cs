using TMPro;
using UnityEngine;

public class SurakshaUI : MonoBehaviour
{
    public static SurakshaUI Instance;

    [Header("UI References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text progressText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ShowScanning();
    }

    public void ShowScanning()
    {
        SetUI(
            "SURAKSHA AR",
            "🔍 SCANNING FLOOR",
            "Point your camera at a clear area of floor.",
            "Scanning..."
        );
    }

    public void ShowFloorDetected(float progress)
    {
        int percentage = Mathf.RoundToInt(progress * 100f);

        SetUI(
            "SURAKSHA AR",
            "🟢 FLOOR DETECTED",
            "Keep your phone steady.",
            "Stability: " + percentage + "%"
        );
    }

    public void ShowFloorReady()
    {
        SetUI(
            "SURAKSHA AR",
            "✓ FLOOR READY",
            "Tap the screen to lock this training area.",
            "Ready"
        );
    }

    public void ShowFloorLocked()
    {
        SetUI(
            "SURAKSHA AR",
            "🔒 FLOOR LOCKED",
            "Training area is being prepared.",
            "100%"
        );
    }

    public void ShowFireTraining()
    {
        SetUI(
            "FIRE SAFETY",
            "🔥 FIRE DETECTED",
            "Tap the fire extinguisher to begin.",
            ""
        );
    }

    public void ShowExtinguisherStep(
        int step,
        string instruction
    )
    {
        SetUI(
            "FIRE SAFETY",
            "🧯 STEP " + step + " / 4",
            instruction,
            "Training in progress"
        );
    }

    public void ShowComplete()
    {
        SetUI(
            "FIRE SAFETY",
            "✓ FIRE EXTINGUISHED",
            "Excellent! The fire response procedure is complete.",
            "TRAINING COMPLETE"
        );
    }

    private void SetUI(
        string title,
        string status,
        string instruction,
        string progress
    )
    {
        if (titleText != null)
            titleText.text = title;

        if (statusText != null)
            statusText.text = status;

        if (instructionText != null)
            instructionText.text = instruction;

        if (progressText != null)
            progressText.text = progress;
    }
}