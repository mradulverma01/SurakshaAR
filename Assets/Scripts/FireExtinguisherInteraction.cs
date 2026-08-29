using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class FireExtinguisherInteraction : MonoBehaviour
{
    public static FireExtinguisherInteraction Instance;

    [Header("Fire")]
    [SerializeField] private GameObject fireObject;

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 5f;

    private Camera playerCamera;

    private bool trainingStarted = false;
    private int currentStep = 0;

    private float trainingStartTime;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        playerCamera = Camera.main;
    }

    private void Update()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (Touchscreen.current == null)
            return;

        if (!Touchscreen.current.primaryTouch
            .press.wasPressedThisFrame)
            return;

        Vector2 touchPosition =
            Touchscreen.current.primaryTouch
            .position.ReadValue();

        // IMPORTANT:
        // Don't treat UI touches as AR object touches.
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        CheckForInteraction(touchPosition);
    }

    private void CheckForInteraction(
        Vector2 screenPosition)
    {
        if (playerCamera == null)
            return;

        Ray ray =
            playerCamera.ScreenPointToRay(
                screenPosition
            );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactionDistance))
        {
            if (hit.transform == transform ||
                hit.transform.IsChildOf(transform))
            {
                StartTraining();
            }
        }
    }

    private void StartTraining()
    {
        if (trainingStarted)
            return;

        trainingStarted = true;

        currentStep = 1;

        trainingStartTime = Time.time;

        ShowCurrentStep();
    }

    private void ShowCurrentStep()
    {
        if (SurakshaUI.Instance == null)
            return;

        switch (currentStep)
        {
            case 1:

                SurakshaUI.Instance.ShowExtinguisherStep(
                    1,
                    "Pull the safety pin."
                );

                break;

            case 2:

                SurakshaUI.Instance.ShowExtinguisherStep(
                    2,
                    "Aim the extinguisher at the base of the fire."
                );

                break;

            case 3:

                SurakshaUI.Instance.ShowExtinguisherStep(
                    3,
                    "Squeeze the extinguisher handle."
                );

                break;

            case 4:

                SurakshaUI.Instance.ShowExtinguisherStep(
                    4,
                    "Sweep from side to side until the fire is out."
                );

                break;
        }
    }

    public void PerformStep()
    {
        if (!trainingStarted)
            return;

        currentStep++;

        if (currentStep <= 4)
        {
            ShowCurrentStep();
        }
        else
        {
            CompleteTraining();
        }
    }

    private void CompleteTraining()
    {
        float completionTime =
            Time.time - trainingStartTime;

        int score =
            CalculateScore(completionTime);

        if (fireObject != null)
        {
            fireObject.SetActive(false);
        }

        if (SurakshaUI.Instance != null)
        {
            SurakshaUI.Instance.ShowComplete();
        }

        Debug.Log(
            "FIRE TRAINING COMPLETE"
        );

        Debug.Log(
            "Completion Time: " +
            completionTime.ToString("F1") +
            " seconds"
        );

        Debug.Log(
            "Safety Score: " +
            score +
            "%"
        );
    }

    private int CalculateScore(
        float completionTime)
    {
        int score = 100;

        if (completionTime > 60f)
            score -= 10;

        if (completionTime > 90f)
            score -= 10;

        return Mathf.Clamp(score, 0, 100);
    }
}