using System;
using System.Collections.Generic;
using System.Linq;
using SurakshaAR.Domain.Training;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

#nullable enable

namespace SurakshaAR.Scene
{
    [DisallowMultipleComponent]
    public sealed class TrainingSceneController : MonoBehaviour
    {
        private static readonly List<ARRaycastHit> ArHits = new List<ARRaycastHit>();

        [SerializeField]
        private ARRaycastManager raycastManager = null!;

        [SerializeField]
        private ARPlaneManager planeManager = null!;

        [SerializeField]
        private Camera arCamera = null!;

        [SerializeField]
        private GameObject scenarioPrefab = null!;

        [SerializeField]
        private List<GameObject> scenarioPrefabs = new List<GameObject>();

        private MobileTrainingSession? session;
        private ScenarioBundle? scenario;
        private GameObject? anchorObject;
        private GameObject? activePrefab;

        private readonly HoldTracker holdTracker = new HoldTracker();
        private readonly ProximityTracker proximity = new ProximityTracker();

        public event Action<TrainingUpdate>? Updated;

        public bool IsPlaced => anchorObject != null;

        public void StartAttempt(ScenarioBundle bundle, AttemptContext context)
        {
            scenario = bundle ?? throw new ArgumentNullException(nameof(bundle));
            session = new MobileTrainingSession(new[] { bundle });
            activePrefab = ResolvePrefab(bundle);
            var update = session.SelectModule(bundle.Id, context);
            Updated?.Invoke(update.Training);
        }

        private GameObject ResolvePrefab(ScenarioBundle bundle)
        {
            var match = scenarioPrefabs.FirstOrDefault(prefab => prefab != null && prefab.name == bundle.Scene.PrefabId);
            if (match != null)
            {
                return match;
            }

            if (scenarioPrefab != null)
            {
                return scenarioPrefab;
            }

            throw new InvalidOperationException("No scenario prefab is configured for " + bundle.Id);
        }

        public AttemptResult FinishAttempt()
        {
            return session?.CompletedAttempt ?? throw new InvalidOperationException("No training attempt is active.");
        }

        public AttemptResult LeaveAttempt()
        {
            if (session == null)
            {
                throw new InvalidOperationException("No training attempt is active.");
            }

            var update = session.Leave();
            Updated?.Invoke(update.Training);
            var result = session.CompletedAttempt ?? throw new InvalidOperationException("Leave did not produce a result.");
            ResetScene();
            return result;
        }

        public void ResetScene()
        {
            if (anchorObject != null)
            {
                Destroy(anchorObject);
                anchorObject = null;
            }

            session = null;
            scenario = null;
            activePrefab = null;
            holdTracker.Reset();
            proximity.Clear();
            planeManager.enabled = true;
        }

        private void Update()
        {
            if (session == null || scenario == null)
            {
                return;
            }

            if (!IsPlaced)
            {
                if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    TryPlaceScenario(Input.GetTouch(0).position);
                }
                return;
            }

            CheckProximityInteractions();
            HandleHoldInput();
            HandleTapInput();
        }

        private void TryPlaceScenario(Vector2 screenPoint)
        {
            if (!raycastManager.Raycast(screenPoint, ArHits, TrackableType.PlaneWithinPolygon))
            {
                return;
            }

            var pose = ArHits[0].pose;
            anchorObject = new GameObject("TrainingScenarioAnchor");
            anchorObject.transform.SetPositionAndRotation(pose.position, pose.rotation);
            anchorObject.AddComponent<ARAnchor>();
            var prefab = activePrefab ?? scenarioPrefab;
            Instantiate(prefab, anchorObject.transform);
            planeManager.enabled = false;
        }

        private void HandleTapInput()
        {
            if (Input.touchCount != 1)
            {
                return;
            }

            var touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Began)
            {
                return;
            }

            var ray = arCamera.ScreenPointToRay(touch.position);
            if (!Physics.Raycast(ray, out var hit))
            {
                return;
            }

            var target = hit.collider.GetComponentInParent<TrainingTarget>();
            if (target == null || string.IsNullOrWhiteSpace(target.InteractionId))
            {
                return;
            }

            var definition = scenario!.Interactions.SingleOrDefault(d => d.Id == target.InteractionId);
            if (definition == null)
            {
                return;
            }

            if (definition.Kind == SemanticInteractionKind.CompletedHold)
            {
                holdTracker.Begin(definition.Id, definition.TargetId, Time.time);
                return;
            }

            if (definition.Kind == SemanticInteractionKind.WaypointArrived
                || definition.Kind == SemanticInteractionKind.ZoneEntered
                || definition.Kind == SemanticInteractionKind.ZoneExited)
            {
                return;
            }

            Emit(new SemanticInteraction(definition.Id, definition.Kind, definition.TargetId));
        }

        private void HandleHoldInput()
        {
            if (!holdTracker.IsActive)
            {
                return;
            }

            if (Input.touchCount != 1)
            {
                if (holdTracker.TryCancel(out var id, out var target))
                {
                    Emit(new SemanticInteraction(id, SemanticInteractionKind.InterruptedHold, target));
                }
                return;
            }

            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended)
            {
                if (holdTracker.TryComplete(Time.time, out var id, out var target, out var duration))
                {
                    Emit(new SemanticInteraction(id, SemanticInteractionKind.CompletedHold, target, duration));
                }
            }
            else if (touch.phase == TouchPhase.Canceled)
            {
                if (holdTracker.TryCancel(out var id, out var target))
                {
                    Emit(new SemanticInteraction(id, SemanticInteractionKind.InterruptedHold, target));
                }
            }
        }

        private void CheckProximityInteractions()
        {
            if (anchorObject == null || scenario == null)
            {
                return;
            }

            foreach (var definition in scenario.Interactions)
            {
                if (definition.Kind != SemanticInteractionKind.WaypointArrived
                    && definition.Kind != SemanticInteractionKind.ZoneEntered
                    && definition.Kind != SemanticInteractionKind.ZoneExited)
                {
                    continue;
                }

                var targetObject = anchorObject.GetComponentsInChildren<TrainingTarget>()
                    .FirstOrDefault(t => t.InteractionId == definition.Id);
                if (targetObject == null)
                {
                    continue;
                }

                if (!proximity.ShouldEmitOnEnter(definition.Id, arCamera.transform.position, targetObject.transform.position))
                {
                    continue;
                }

                SemanticInteraction interaction;
                if (definition.Kind == SemanticInteractionKind.WaypointArrived)
                {
                    if (string.IsNullOrWhiteSpace(targetObject.TargetId))
                    {
                        continue;
                    }

                    interaction = new SemanticInteraction(definition.Id, definition.Kind, targetObject.TargetId);
                }
                else
                {
                    interaction = new SemanticInteraction(definition.Id, definition.Kind, definition.TargetId);
                }

                Emit(interaction);
                break;
            }
        }

        private void Emit(SemanticInteraction interaction)
        {
            var update = session!.Apply(interaction);
            Updated?.Invoke(update.Training);
            if (update.ReturnedToLauncher)
            {
                ResetScene();
            }
        }
    }
}
