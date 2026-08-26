using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class FloorCalibration : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject reticlePrefab;
    [SerializeField] private GameObject trainingAreaPrefab;

    [Header("Scanning")]
    [SerializeField] private float screenHeightTarget = 0.72f;
    [SerializeField] private float minimumDistance = 0.5f;
    [SerializeField] private float maximumDistance = 4.0f;

    [Header("Stability")]
    [SerializeField] private int requiredSamples = 30;
    [SerializeField] private float maximumPositionVariation = 0.04f;

    private ARRaycastManager raycastManager;

    private GameObject reticle;
    private GameObject spawnedTrainingArea;

    private static readonly List<ARRaycastHit> hits =
        new List<ARRaycastHit>();

    private readonly List<Vector3> samples =
        new List<Vector3>();

    private Vector3 currentPosition;

    private bool validFloor;
    private bool floorStable;
    private bool floorLocked;

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();

        if (reticlePrefab != null)
        {
            reticle = Instantiate(reticlePrefab);
            reticle.SetActive(false);
        }
    }

    private void Start()
    {
        if (SurakshaUI.Instance != null)
        {
            SurakshaUI.Instance.ShowScanning();
        }
    }

    private void Update()
    {
        if (floorLocked)
            return;

        if (raycastManager == null)
            return;

        if (Camera.main == null)
            return;

        FindFloor();

        if (validFloor)
        {
            CollectSample();

            UpdateStability();

            UpdateReticle();

            UpdateUI();

            HandleCalibrationTap();
        }
        else
        {
            ClearSamples();

            floorStable = false;

            HideReticle();

            if (SurakshaUI.Instance != null)
            {
                SurakshaUI.Instance.ShowScanning();
            }
        }
    }

    private void FindFloor()
    {
        validFloor = false;

        Vector2 floorTarget = new Vector2(
            Screen.width * 0.5f,
            Screen.height * screenHeightTarget
        );

        bool hitSomething = raycastManager.Raycast(
            floorTarget,
            hits,
            TrackableType.Depth
        );

        if (!hitSomething)
            return;

        Pose hitPose = hits[0].pose;

        float distance = Vector3.Distance(
            Camera.main.transform.position,
            hitPose.position
        );

        if (distance < minimumDistance ||
            distance > maximumDistance)
        {
            return;
        }

        float floorAlignment = Vector3.Dot(
            hitPose.up,
            Vector3.up
        );

        if (floorAlignment < 0.85f)
            return;

        currentPosition = hitPose.position;

        validFloor = true;
    }

    private void CollectSample()
    {
        samples.Add(currentPosition);

        if (samples.Count > requiredSamples)
        {
            samples.RemoveAt(0);
        }
    }

    private void UpdateStability()
    {
        floorStable = false;

        if (samples.Count < requiredSamples)
            return;

        Vector3 average = Vector3.zero;

        foreach (Vector3 sample in samples)
        {
            average += sample;
        }

        average /= samples.Count;

        foreach (Vector3 sample in samples)
        {
            if (Vector3.Distance(
                sample,
                average
            ) > maximumPositionVariation)
            {
                return;
            }
        }

        currentPosition = average;

        floorStable = true;
    }

    private void UpdateUI()
    {
        if (SurakshaUI.Instance == null)
            return;

        float progress =
            Mathf.Clamp01(
                (float)samples.Count /
                requiredSamples
            );

        if (floorStable)
        {
            SurakshaUI.Instance.ShowFloorReady();
        }
        else
        {
            SurakshaUI.Instance.ShowFloorDetected(
                progress
            );
        }
    }

    private void HandleCalibrationTap()
    {
        if (!floorStable)
            return;

        if (Touchscreen.current == null)
            return;

        if (!Touchscreen.current.primaryTouch
            .press.wasPressedThisFrame)
            return;

        LockFloor();
    }

    private void LockFloor()
    {
        floorLocked = true;

        HideReticle();

        if (SurakshaUI.Instance != null)
        {
            SurakshaUI.Instance.ShowFloorLocked();
        }

        if (trainingAreaPrefab != null)
        {
            spawnedTrainingArea = Instantiate(
                trainingAreaPrefab,
                currentPosition,
                Quaternion.identity
            );

            Invoke(
                nameof(StartFireTraining),
                1.5f
            );
        }
    }

    private void StartFireTraining()
    {
        if (SurakshaUI.Instance != null)
        {
            SurakshaUI.Instance.ShowFireTraining();
        }
    }

    private void UpdateReticle()
    {
        if (reticle == null)
            return;

        reticle.SetActive(true);

        reticle.transform.SetPositionAndRotation(
            currentPosition,
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

    private void ClearSamples()
    {
        samples.Clear();
    }

    public bool IsFloorLocked()
    {
        return floorLocked;
    }

    public Vector3 GetLockedFloorPosition()
    {
        return currentPosition;
    }
}