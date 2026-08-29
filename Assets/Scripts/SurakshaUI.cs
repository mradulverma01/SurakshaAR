using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SurakshaUI : MonoBehaviour
{
    public static SurakshaUI Instance;

    [Header("UI References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text progressText;

    [Header("Action Button")]
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text actionButtonText;

    private void Awake()
    {
        Instance = this;

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnActionButtonPressed);
            actionButton.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        ShowScanning();
    }

    // =========================================================
    // FLOOR CALIBRATION
    // =========================================================

    public void ShowScanning()
    {
        HideActionButton();

        SetUI(
            "SURAKSHA AR",
            "SCANNING FLOOR",
            "Point your camera at a clear area of floor.",
            "Scanning..."
        );
    }

    public void ShowFloorDetected(float progress)
    {
        HideActionButton();

        int percentage =
            Mathf.RoundToInt(progress * 100f);

        SetUI(
            "SURAKSHA AR",
            "FLOOR DETECTED",
            "Keep your phone steady.",
            "Stability: " + percentage + "%"
        );
    }

    public void ShowFloorReady()
    {
        HideActionButton();

        SetUI(
            "SURAKSHA AR",
            "FLOOR READY",
            "Tap the screen to lock this training area.",
            "Ready"
        );
    }

    public void ShowFloorLocked()
    {
        HideActionButton();

        SetUI(
            "SURAKSHA AR",
            "FLOOR LOCKED",
            "Training area is being prepared.",
            "100%"
        );
    }

    // =========================================================
    // FIRE TRAINING
    // =========================================================

    public void ShowFireTraining()
    {
        HideActionButton();

        SetUI(
            "FIRE SAFETY",
            "FIRE DETECTED",
            "Tap the fire extinguisher to begin.",
            ""
        );
    }

    public void ShowExtinguisherStep(
        int step,
        string instruction
    )
    {
        ShowActionButton("CONTINUE");

        SetUI(
            "FIRE SAFETY",
            "STEP " + step + " / 4",
            instruction,
            "Training in progress"
        );
    }

    public void ShowComplete()
    {
        HideActionButton();

        SetUI(
            "FIRE SAFETY",
            "FIRE EXTINGUISHED",
            "Excellent! The fire response procedure is complete.",
            "TRAINING COMPLETE"
        );
    }

    // =========================================================
    // ACTION BUTTON
    // =========================================================

    private void OnActionButtonPressed()
    {
        if (FireExtinguisherInteraction.Instance != null)
        {
            FireExtinguisherInteraction.Instance.PerformStep();
        }
    }

    private void ShowActionButton(string text)
    {
        if (actionButton == null)
            return;

        actionButton.gameObject.SetActive(true);

        if (actionButtonText != null)
        {
            actionButtonText.text = text;
        }
    }

    private void HideActionButton()
    {
        if (actionButton != null)
        {
            actionButton.gameObject.SetActive(false);
        }
    }

    // =========================================================
    // GENERAL UI
    // =========================================================

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
    public void ContinueTraining()
   {
    Debug.Log("CONTINUE BUTTON PRESSED");

    FireExtinguisherInteraction fire =
        FindFirstObjectByType<FireExtinguisherInteraction>();

    if (fire != null)
    {
        Debug.Log("FireExtinguisherInteraction FOUND");
        fire.PerformStep();
    }
    else
    {
        Debug.LogError(
            "FireExtinguisherInteraction NOT FOUND!"
        );
    }
    }
}