# SIH26041 implementation specification

## Goal

Build a product-shaped prototype for a newly recruited coal-mine worker. It must demonstrate two interactive AR safety modules on Android 10 or later without a headset.

## Required flows

- Fire and explosion response through hazard identification, extinguisher selection and use, and evacuation.
- Gas leak and confined-space protocol through hazard-zone recognition, withdrawal, reporting, PPE selection, buddy confirmation, and exit.
- Deterministic assessment from worker actions, including critical failures.
- Offline scenario loading and attempt persistence, followed by idempotent synchronization.
- Server-side score recomputation and one certificate per passing attempt.
- Public QR verification without personal data in the QR payload.
- Hindi localization first, Santali in Ol Chiki second, and English fallback.
- Web compliance dashboard for attempts and certificates.

## Trust boundaries

The APK must not decide whether a certificate is trusted. The server loads the exact module version and recomputes the result from ordered events. A certificate is a verifiable training-completion record. Statutory recognition requires an authorized issuing organization.

## Acceptance

- The pure C# runtime passes correct progression, critical-failure, out-of-order action, and deterministic replay tests.
- A saved attempt survives an application restart and remains pending until the server acknowledges it.
- Duplicate attempt submission cannot create a second attempt or certificate.
- The dashboard typechecks and builds for desktop and mobile layouts.
- Unity-specific scripts remain isolated from the training runtime.

## Current hardware limitation

This development machine has no Unity editor or Android SDK. The repository can test the domain, storage, backend evaluator, and dashboard here. APK and AR tracking verification require Unity 6.3 LTS and an ARCore-certified phone.
