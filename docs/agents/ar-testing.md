# AR testing without an APK

## Purpose

Agents validate AR modules through the training behavior they produce. They do not open Unity, build an APK, or install one on a phone for routine changes.

Read `CONTEXT.md` and `docs/INTEGRATION.md` before changing the training flow. The integration document names the boundary between Unity input and the training runtime.

## What terminal tests cover

`dotnet test mobile/SurakshaAR.sln` runs the Unity-independent projects only:

- `mobile/Assets/App/Domain/`, including `TrainingRuntime` and `MobileTrainingSession`
- `mobile/Assets/App/Infrastructure/`, including scenario loading and local persistence

The solution does not compile or execute `mobile/Assets/App/Scene/` or `mobile/Assets/App/Application/`. A passing .NET suite proves the scenario rules and semantic interactions. It does not prove that Unity detected a touch, placed an anchor, found a collider, rendered a scene, or tracked a physical environment.

Use the root scripts when the change also affects the backend or dashboard:

```bash
npm ci
dotnet test mobile/SurakshaAR.sln
npm run typecheck
npm test
npm run build
```

Run `npm ci` after cloning or when `package-lock.json` changes.

Run the smallest relevant check while iterating. Run the full set before completing a change that crosses mobile, backend, or dashboard boundaries.

## AR interaction contract

Unity turns device input and camera position into `SemanticInteraction` values. `MobileTrainingSession` owns validation, step order, scoring, critical failures, and completion.

`TrainingSceneController` must emit the interaction declared in the selected scenario bundle:

- A target selection emits `TargetSelected` with the configured interaction id and target id.
- A completed hold emits `CompletedHold` with the measured hold duration. A cancelled hold emits `InterruptedHold`.
- A waypoint, zone entry, or zone exit emits its declared interaction kind. Waypoints use the target id from the scene object because the session checks their order.

The scenario JSON under `mobile/Assets/StreamingAssets/Scenarios/` is the source of truth for interaction ids, kinds, target ids, hold thresholds, and waypoint order. `TrainingTarget.InteractionId` must match one of those interaction ids. Generated prefabs receive these values in `mobile/Assets/App/Editor/MvpSceneBuilder.cs`.

## Writing agent-run tests

When a change affects an AR module's behavior, add or update a test that sends the matching `SemanticInteraction` to `MobileTrainingSession`. Keep the test in `mobile/SurakshaAR.Domain.Tests/` when it uses no Unity type.

Each changed flow needs assertions for the changed rule and its unsafe or invalid counterpart. Use the real interaction vocabulary. Common cases include:

- A correct sequence reaches the intended next step or completes the training attempt.
- An out-of-order interaction records a rejection and does not advance the step.
- A wrong extinguisher or unsafe hazard entry records a critical failure and blocks a passing result.
- A short or interrupted hold does not advance the hold step.
- An out-of-order waypoint does not advance the route.

`MobileTrainingSessionTests.cs` already demonstrates Fire and Gas flows with this approach. Extend the closest test instead of introducing a second test vocabulary.

When a Unity adapter contains a decision that needs terminal coverage, move that decision into a Unity-independent class under `mobile/Assets/App/Domain/`. Give it plain inputs such as interaction id, target id, time, or distance, and have it return a `SemanticInteraction` or no result. Test that class through the .NET solution. Leave `MonoBehaviour`, `Input`, `Physics`, `ARRaycastManager`, `ARPlaneManager`, cameras, and prefab instantiation in the Unity adapter.

## Unity and device boundary

Unity checks still matter, but they run only where Unity is available. A Unity owner should verify these after changes to `mobile/Assets/App/Scene/`, `mobile/Assets/App/Editor/`, prefabs, scenes, XR settings, or AR Foundation packages:

- The generated scene has the expected `TrainingTarget` ids and target ids.
- Touch, hold, waypoint, and zone input emit the intended semantic interaction.
- Scenario placement succeeds on a detected plane and the active prefab matches the scenario's `prefabId`.
- The HUD receives updates for accepted, rejected, and critical interactions.

An ARCore device check is still required before release. It validates plane detection, camera permissions, tracking loss, anchor behavior, performance, and the Android build. Do not describe a terminal test as proof of any of those conditions.

## Completion report

Report the commands run, the behavior each new or changed test covers, and any Unity or device validation that remains. State the unverified boundary plainly rather than reporting that the whole AR module works.
