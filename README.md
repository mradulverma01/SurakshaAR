# Suraksha AR

Suraksha AR is an offline-first industrial safety training prototype for Smart India Hackathon 2026 problem SIH26041. The worker app uses Unity, AR Foundation, and ARCore. Supabase stores synchronized attempts and issues verifiable training-completion certificates.

The first trainee is a newly recruited coal-mine worker. The MVP contains Fire and Explosion Response plus Gas Leak and Confined-Space Protocol scenarios.

## Repository layout

```text
mobile/       Unity-compatible domain and AR adapter code
backend/      Supabase migrations and Edge Functions
dashboard/    Compliance and certificate verification web app
docs/         Research and operating notes
```

## Development

The Unity-independent training runtime is a .NET Standard library tested with .NET 8:

```bash
dotnet test mobile/SurakshaAR.sln
```

Dashboard commands are documented in `dashboard/README.md` after dependencies are installed.

## Status

This repository is an implementation scaffold and tested domain core. Building an Android APK still requires Unity 6.3 LTS with Android Build Support and an ARCore-certified phone.

After opening `mobile/` in Unity, run `Suraksha AR > Create MVP scenes`. The command creates Fire and Gas prefabs, two AR scenes, and Android build-scene entries. Replace the generated primitive targets with reviewed models and effects before recording the final demo.

Set the Supabase URL and publishable key on `BackendBootstrap` in each generated scene. The trainer signs in once to provision the worker. Worker identity remains on the device for offline training, but refresh tokens are not stored in `PlayerPrefs`; the trainer signs in again before synchronization after an app restart.
