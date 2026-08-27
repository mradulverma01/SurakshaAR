# Branch reconciliation handoff

This document records the branch state on 2026-08-27 after reconciliation. All three branches have been merged to `main` at `89e9f13` and pushed to `origin/main`.

## Current state

`main` at `89e9f13` now contains the reconciled application.

- `preserve/local-unity-state-2026-08-25` at `109163f` preserved Unity project settings, XR/ARCore config, package lock, and local development artifacts.
- `feat/full-app-phases-1-5` at `840a3e3` added launcher, semantic MobileTrainingSession, worker provisioning separation, Fire/Gas deliberate actions, and certificate authorization.
- Integration branch `integrate/full-app-preservation` merged both at `3e26a75`, then fixed defects at `85bc2b6`.
- `main` fast-forward merge at `89e9f13` includes all of the above plus reconciliation fixes.

Branches remain intact for history. No force push, reset, or branch deletion was used.

## What main now contains

- Mobile domain: `TrainingRuntime`, `MobileTrainingSession`, `ScenarioBundle` with `ScenarioSceneReference` and `ScenarioInteractionDefinition` (thresholds, ordered waypoints).
- Launcher: `TrainingCoordinator` lists `JsonTrainingCatalog.List()` bundles, shows OnGUI launcher, starts `TrainingSceneController.StartAttempt` and returns via `Leave` or completion.
- AR adapters: `TrainingSceneController` handles AR placement, tap, hold timing (Began->Ended = CompletedHold, Canceled -> InterruptedHold), and edge-triggered zone/waypoint detection via camera position.
- Provisioning: `SupabaseSessionTokenProvider` with `JsonProvisionedWorkerStore` validates worker via REST and persists only workerId.
- Sync: `UnityAttemptRemote` posts workerId + events to `sync-attempt`, handles 403/400/422 as rejected.
- Scenes: `Launcher.unity`, `FireTraining.unity`, `GasTraining.unity` (and UI shells MainMenu, Dashboard, Settings, LoginMenu, Placeholder) committed with meta. `MvpSceneBuilder` generates Launcher+2 training prefabs/scenes.
- Prefabs: `FireScenario.prefab`, `GasScenario.prefab`, `TrainingTarget*.mat`, `UI/*.prefab` now tracked (removed from `.gitignore`) for clean clones.
- Backend: `sync-attempt` checks same organization and trainer role, returns 403 `42501`, persists attempt even when issuer not authorized with reason `issuer_not_authorized`. `verify-certificate` returns issuer name without worker identity.
- Migrations: `202608230001` through `202608240002_fix_pgcrypto` (can_issue_certificates, issuer_organization_id, extensions.gen_random_bytes).

## Fixes applied during reconciliation

- `SurakshaAR.Scene.asmdef` now references `Unity.XR.ARSubsystems`.
- `MvpSceneBuilder` removed deleted `TrainingCoordinator.moduleId` write, now sets `scenarioPrefabs` list.
- `TrainingSceneController` now debounces holds, zones, and waypoints; waypoint index guarded against out-of-range; hold cancel correctly emits InterruptedHold.
- `MobileTrainingSession.ApplyWaypoint` guards `index >= OrderedWaypoints.Count`.
- `UnityAttemptRemote` treats 403 as rejected.
- `BuildMvp` builds Launcher, FireTraining, GasTraining with Launcher first.
- `XRGeneralSettingsPerBuildTarget.asset` enables AutomaticLoading and AutomaticRunning.
- `.gitignore` no longer ignores `Assets/Prefabs`; those assets are now committed.

## GitHub state

- PR #9 `feat/full-app-phases-1-5 -> main` is merged (auto-closed after main push). No open PRs remain.
- Issues closed: #2, #3, #5, #6, #7, #8. Issue #4 remains `ready-for-human` awaiting reviewed Santali Ol Chiki translations. Issue #1 remains open until #4 completes.
- No protection rules on `main`, but push was checked against open PR and branch status before merging.

## Test status on main

- `dotnet test mobile/SurakshaAR.sln` passed 27 tests (16 domain, 11 infrastructure).
- `npm test` passed 5 tests (3 backend, 2 dashboard).
- `npm run typecheck` passed.
- `npm run build` passed (dashboard 218 kB).
- Unity editor and Android builds remain unverified (no Unity executable on this host).

## Remaining work

- Issue #4: human reviewer must fill `to-questionnaire-santali-ol-chiki.md` (15 required keys) and approve Ol Chiki text. Do not machine-translate.
- Backend `BackendService` and `TrainingUIBridge` remain stubs (unused, not referenced by scenes). Remove or implement if needed, but not blocking.
- `CreateCoreScenes` buttons still have empty onClick wiring; UI shells are visual stubs.
- Verify certificate legacy entries were backfilled with issuer_organization_id but not re-validated against `can_issue_certificates`; consider revocation strategy if required.
- Final Android acceptance requires Unity 6000.3 LTS with Android Build Support and ARCore device.

## How to continue

- Base new work on `main` at `89e9f13`. Do not rebase the preserved or feature branches.
- For new training content, edit `StreamingAssets/Scenarios/*.json` and run `Suraksha AR/Create MVP scenes`.
- Supabase: apply migrations in order, set `SUPABASE_URL` and `SUPABASE_SERVICE_ROLE_KEY` for Edge Functions, publish scenarios via backend script.
