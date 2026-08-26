using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceTrainingArea : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject trainingAreaPrefab;
    [SerializeField] private GameObject reticlePrefab;

    [Header("Placement Settings")]
    [SerializeField] private float minimumDistanceFromCamera = 0.5f;
    [SerializeField] private float maximumDistanceFromCamera = 4.0f;

    // How far down the screen we look for the floor.
    // 0.50 = center
    // 0.72 = lower part of screen
    [SerializeField] private float screenHeightForFloor = 0.72f;

    // How horizontal the surface must be.
    // 1.0 = perfectly horizontal
    // 0.85 = slightly tolerant
    [SerializeField] private float minimumFloorAlignment = 0.85f;

    private ARRaycastManager raycastManager;

    private GameObject spawnedArea;
    private GameObject reticle;

    private static readonly List<ARRaycastHit> hits =
        new List<ARRaycastHit>();

    private Pose placementPose;
    private bool validPlacement;

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();

        if (raycastManager == null)
        {
            Debug.LogError(
                "PlaceTrainingArea requires an ARRaycastManager."
            );
        }

        // Create the placement reticle.
        if (reticlePrefab != null)
        {
            reticle = Instantiate(reticlePrefab);
            reticle.SetActive(false);
        }
        else
        {
            Debug.LogWarning(
                "Reticle Prefab has not been assigned."
            );
        }
    }

    private void Update()
    {
        if (raycastManager == null)
            return;

        if (Camera.main == null)
            return;

        FindFloor();

        HandleTouch();
    }

    private void FindFloor()
    {
        validPlacement = false;

        // Target a point lower than the center of the screen.
        // This reduces the chance of hitting furniture/walls
        // when the user is looking toward the floor.
        Vector2 floorTarget = new Vector2(
            Screen.width * 0.5f,
            Screen.height * screenHeightForFloor
        );

        // Use environment depth instead of detected planes.
        bool hitSomething = raycastManager.Raycast(
            floorTarget,
            hits,
            TrackableType.Depth
        );

        if (!hitSomething)
        {
            HideReticle();
            return;
        }

        Pose hitPose = hits[0].pose;

        // ----------------------------------------------------
        // 1. Check distance
        // ----------------------------------------------------

        float distance = Vector3.Distance(
            Camera.main.transform.position,
            hitPose.position
        );

        if (distance < minimumDistanceFromCamera ||
            distance > maximumDistanceFromCamera)
        {
            HideReticle();
            return;
        }

        // ----------------------------------------------------
        // 2. Check surface orientation
        // ----------------------------------------------------

        float floorAlignment = Vector3.Dot(
            hitPose.up,
            Vector3.up
        );

        // Reject walls, tilted objects, chair backs, etc.
        if (floorAlignment < minimumFloorAlignment)
        {
            HideReticle();
            return;
        }

        // ----------------------------------------------------
        // 3. Valid floor candidate
        // ----------------------------------------------------

        validPlacement = true;
        placementPose = hitPose;

        ShowReticle();
    }

    private void ShowReticle()
    {
        if (reticle == null)
            return;

        reticle.SetActive(true);

        reticle.transform.SetPositionAndRotation(
            placementPose.position,
            Quaternion.identity
        );
    }

    private void HideReticle()
    {
        if (reticle != null)
        {
            reticle.SetActive(false);
        }
    }

    private void HandleTouch()
    {
        if (!validPlacement)
            return;

        if (Touchscreen.current == null)
            return;

        if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            PlaceScenario();
        }
    }

    private void PlaceScenario()
    {
        if (trainingAreaPrefab == null)
        {
            Debug.LogError(
                "Training Area Prefab is not assigned."
            );

            return;
        }

        // Only create one training area.
        if (spawnedArea == null)
        {
            spawnedArea = Instantiate(
                trainingAreaPrefab,
                placementPose.position,
                Quaternion.identity
            );
        }
        else
        {
            // If one already exists, move it instead of
            // creating another copy.
            spawnedArea.transform.SetPositionAndRotation(
                placementPose.position,
                Quaternion.identity
            );
        }

        // Hide the reticle after placement.
        HideReticle();
    }
}