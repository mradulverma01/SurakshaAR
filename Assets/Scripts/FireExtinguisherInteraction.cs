using UnityEngine;
using UnityEngine.InputSystem;

public class FireExtinguisherInteraction : MonoBehaviour
{
    [Header("Fire")]
    [SerializeField] private GameObject fireObject;

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 5f;

    private Camera playerCamera;

    private bool extinguisherSelected = false;

    private int currentStep = 0;

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

        CheckForInteraction(touchPosition);
    }

    private void CheckForInteraction(
        Vector2 screenPosition
    )
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
                SelectExtinguisher();
            }
        }
    }

    private void SelectExtinguisher()
    {
        if (extinguisherSelected)
            return;

        extinguisherSelected = true;

        currentStep = 1;

        if (SurakshaUI.Instance != null)
        {
            SurakshaUI.Instance.ShowExtinguisherStep(
                1,
                "Pull the safety pin."
            );
        }
    }

    public void PerformStep()
    {
        if (!extinguisherSelected)
            return;

        currentStep++;

        switch (currentStep)
        {
            case 2:

                if (SurakshaUI.Instance != null)
                {
                    SurakshaUI.Instance.ShowExtinguisherStep(
                        2,
                        "Aim the extinguisher at the base of the fire."
                    );
                }

                break;

            case 3:

                if (SurakshaUI.Instance != null)
                {
                    SurakshaUI.Instance.ShowExtinguisherStep(
                        3,
                        "Squeeze the extinguisher handle."
                    );
                }

                break;

            case 4:

                if (SurakshaUI.Instance != null)
                {
                    SurakshaUI.Instance.ShowExtinguisherStep(
                        4,
                        "Sweep from side to side until the fire is out."
                    );
                }

                break;

            case 5:

                ExtinguishFire();

                break;
        }
    }

    private void ExtinguishFire()
    {
        if (fireObject != null)
        {
            fireObject.SetActive(false);
        }

        if (SurakshaUI.Instance != null)
        {
            SurakshaUI.Instance.ShowComplete();
        }
    }
}