# UI ↔ AR integration contract — Suraksha AR

This contract is for the six-person SIH team. One AR lead with the best laptop owns the AR modules and the final APK. Five others own the app shell, backend, DB, and dashboard. The AR lead builds UI and UX around the app through the seam described here.

## Single source repo

Keep `aditya-zig/SIH` as the source of truth.

- `main` — app, backend, dashboard, domain. Adi leads this.
- `feature/ar-modules` — AR lead's work. Rebase on `main` weekly.

Add collaborators on GitHub: `aditya-zig/SIH` -> Settings -> Collaborators -> `mradulverma01` and the other four.

## Folder ownership

```
mobile/Assets/
  App/
    Application/   # Adi: TrainingCoordinator, BackendBootstrap, TrainingHud
    Domain/        # shared pure C#: MobileTrainingSession, TrainingRuntime, DomainTypes
    Scene/         # shared seam: TrainingSceneController, TrainingTarget
    Infrastructure/# Adi and backend track: JsonTrainingCatalog, JsonAttemptStore, JsonProvisionedWorkerStore
    Editor/        # MvpSceneBuilder. AR lead runs Suraksha AR/Create MVP scenes
  StreamingAssets/
    Scenarios/     # content: fire_001.v1.json, gas_001.v1.json. Edit data, not code
    Localization/  # en.json, hi.json, sat-Olck.json
  Scenes/          # Generated: Launcher.unity (first), FireTraining.unity, GasTraining.unity
  Prefabs/         # Generated: FireScenario.prefab, GasScenario.prefab
backend/
  supabase/migrations  # DB track: already pushed to pdqfhtgqldrucjfevcqw
  functions/           # backend track: sync-attempt, verify-certificate
dashboard/src          # dashboard track
docs/
  INTEGRATION.md       # this file
```

AR lead does not edit `MobileTrainingSession`. App track does not edit `AR/Fire` meshes.

## The seam

Pure C# owns the rules. Unity owns the input.

`MobileTrainingSession` is the only boundary the app uses for training. It is tested without Unity at `mobile/SurakshaAR.Domain.Tests/MobileTrainingSessionTests.cs`.

```
MobileTrainingSession.SelectModule(moduleId, context) -> MobileTrainingUpdate
MobileTrainingSession.Apply(SemanticInteraction) -> MobileTrainingUpdate
MobileTrainingSession.Leave() -> MobileTrainingUpdate with ReturnedToLauncher
MobileTrainingSession.ActiveModules -> launcher list
```

`SemanticInteraction` carries the meaning Unity detected:

```
TargetSelected, CompletedHold(value = hold seconds), InterruptedHold,
ZoneEntered, ZoneExited, WaypointArrived
```

The scenario controls the numbers. `ScenarioInteractionDefinition.Threshold` for holds and `OrderedWaypoints` for evacuation come from `fire_001.v1.json` and `gas_001.v1.json`. Tests in `MobileTrainingSessionTests` check short hold `2` rejected versus `3` accepted, and tap on exit rejected versus `WaypointArrived` accepted.

## What each side provides

### App track provides

`TrainingCoordinator` at `mobile/Assets/App/Application/TrainingCoordinator.cs`. After `OfflineContentInstaller` finishes, it does:

```
catalog = new JsonTrainingCatalog(InstallRoot + "/Scenarios")
bundles = await catalog.List()
launcherSession = new MobileTrainingSession(bundles)
```

It shows the launcher with `fire_001` and `gas_001`, calls `TrainingSceneController.StartAttempt(bundle, context)` on selection, and shows the launcher again on `ReturnedToLauncher` or on `Leave`.

`JsonTrainingCatalog.List` groups `*.v*.json` by module and picks the latest version per module. Tested at `JsonTrainingCatalogTests`.

### AR track provides

Unity adapters that translate ARCore input and camera position into `SemanticInteraction`:

- Handle: on `TouchPhase.Began` on a `TrainingTarget` whose definition kind is `CompletedHold`, start timing. On `TouchPhase.Ended` send `CompletedHold` with duration. On cancel send `InterruptedHold`. The domain checks duration against `Threshold`.
- Exit and zones: do not complete on tap. In `TrainingSceneController.Update`, check `Vector3.Distance(arCamera.transform.position, target.transform.position) < 0.8` and send `WaypointArrived`, `ZoneEntered`, or `ZoneExited` with the target's `interactionId`. Taps on those targets are intentionally rejected.

`TrainingTarget` at `mobile/Assets/App/Scene/TrainingTarget.cs` must have `interactionId` set to the scenario `Interactions` id. `MvpSceneBuilder` at `mobile/Assets/App/Editor/MvpSceneBuilder.cs` already sets this: `Fire` -> `identify_hazard`, `CO2` -> `select_extinguisher`, `Water` -> `wrong_extinguisher`, `Handle` -> `discharge`, `Safe Exit` -> `exit_route` for Fire, and the matching ids for Gas. AR lead adds meshes as children of those targets but keeps the ids.

## Scene flow

```
Launcher.unity (first in Build Settings)
  -> TrainingCoordinator shows launcher
  -> user picks Fire or Gas
  -> TrainingSceneController places anchor via ARRaycastManager, instantiates the prefab matching `bundle.Scene.PrefabId`
  -> holds, waypoints, zones drive MobileTrainingSession
  -> on completion or Leave, TrainingCoordinator shows launcher again
FireTraining.unity, GasTraining.unity remain for direct testing
```

Generate all three with `Suraksha AR/Create MVP scenes` in the editor, or batch:

```
/home/batman/Unity/Hub/Editor/6000.3.22f1/Editor/Unity -batchmode -projectPath mobile -executeMethod SurakshaAR.Editor.MvpSceneBuilder.CreateScenes -quit
```

## Build ownership

Only the AR lead builds the APK to avoid SDK drift. From `mobile`:

```
BuildPlaceholder at mobile/Assets/Editor/BuildPlaceholder.cs for quick 31 MB placeholder
Real build uses the Launcher scene as first entry
```

App, backend, and dashboard tracks verify without Unity via `dotnet test mobile/SurakshaAR.sln`, `npm test --workspace @suraksha-ar/backend`, and `npm run build --workspace @suraksha-ar/dashboard`.

## Backend contract

`persist_evaluated_attempt` expects `p_trainer_id`, `p_worker_id` and checks same `organization_id` and trainer role, then `can_issue_certificates`. It returns `certificateReason: issuer_not_authorized` when the org cannot issue. Public `verify-certificate` returns `issuer` not `workerName`. See `backend/supabase/migrations/202608240001_authorize_certificate_issuance.sql` and the fix for `extensions.gen_random_bytes`.

## Localization

`sat-Olck.json` is `blocked-pending-human-translator`. Do not use Google Translate. The reviewer fills `to-questionnaire-santali-ol-chiki.md` for the 15 required keys. Keep one key deliberately absent to prove English fallback via `JsonLocalizationCatalog`.

## What to do next

1. Commit this file and push `main`.
2. AR lead rebases `feature/ar-modules` on `main`.
3. Dashboard and localization owners work in parallel branches off `main`.
