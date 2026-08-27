# AR vocational training research

## Product decision

Build the SIH26041 prototype with Unity 6.3 LTS, AR Foundation, and the ARCore XR Plugin. Use Supabase for synchronized records and certificate verification. Firebase is an alternative only if the team cannot make the Supabase adapter work in Unity.

The first trainee is a newly recruited coal-mine worker. Public DGMS material provides more usable entrant, fire, evacuation, mine-gas, and self-rescuer requirements for that worker than the public material found for steel or mica processing.

Language order is Hindi, Santali in Ol Chiki, then English fallback. A Santali speaker must review procedural translations before release.

## AR implementation

AR Foundation supplies Unity interfaces for plane detection, raycasting, anchors, image tracking, depth, and light estimation. Android requires the ARCore XR Plugin. Package versions should match.

Android 10 does not guarantee ARCore support. Google certifies individual device models based on camera, sensor, and processing requirements. The APK must check availability at startup and the team must test every demo phone against Google's supported-device list.

Primary sources:

- [AR Foundation and ARCore features](https://developers.google.com/ar/develop/unity-arf/features)
- [Enable ARCore in Unity](https://developers.google.com/ar/develop/unity-arf/enable-arcore)
- [ARCore-supported devices](https://developers.google.com/ar/devices)
- [Unity AR Foundation codelab](https://codelabs.developers.google.com/arcore-unity-ar-foundation)
- [Unity 6 support](https://unity.com/releases/unity-6/support)

Sceneform is archived and should not be used for new work.

- [Sceneform archive notice](https://developers.google.com/sceneform/develop)

## Offline behavior

Android's offline-first guidance makes local storage the source read by higher layers. Network synchronization updates that local source when connectivity returns. In this product, scenario files and required assets ship with the APK, attempts save locally, and a pending queue retries later. Certificate issuance remains server-side.

- [Android offline-first architecture](https://developer.android.com/topic/architecture/data-layer/offline-first)

Supabase provides Postgres, Auth, Storage, and Edge Functions. Its C# client is community-maintained. The implementation therefore keeps Supabase behind an adapter. The APK may contain the publishable key with Row Level Security enabled. It must never contain the service-role key.

- [Supabase database overview](https://supabase.com/docs/guides/database/overview)
- [Supabase C# introduction](https://supabase.com/docs/reference/csharp/introduction)
- [Supabase Row Level Security](https://supabase.com/docs/guides/database/postgres/row-level-security)
- [Supabase Storage quickstart](https://supabase.com/docs/guides/storage/quickstart)

## Safety and legal framing

DGMS publishes the Mines Vocational Training Rules 1966 and Coal Mines Regulations 2017. Their training and emergency material supports practical fire response, exit knowledge, mine-gas recognition, withdrawal, reporting, ventilation controls, and self-rescuer training.

- [DGMS legislation library](https://www.dgms.gov.in/)
- [Mines Vocational Training Rules 1966](https://www.dgms.gov.in/writereaddata/UploadFile/MineVocational966.pdf)
- [Coal Mines Regulations 2017](https://dgms.gov.in/writereaddata/UploadFile/Coal_Mines_Regulation_2017_Noti.pdf)

The Occupational Safety, Health and Working Conditions Code 2020 came into force on 21 November 2025 through S.O. 5321(E). The Central Rules 2026 followed in May 2026. The pitch should cite the current framework and use older DGMS material as procedure and training reference where it remains applicable.

- [OSHWC commencement notification](https://egazette.gov.in/WriteReadData/2025/267884.pdf)
- [Ministry of Labour labour codes](https://labour.gov.in/labour-codes)

The prototype issues a verifiable training-completion certificate. It must not claim statutory recognition until an authorized employer, training center, or regulator approves the program and issuance process.

## Santali and Hindi

Unicode assigns Ol Chiki to U+1C50 through U+1C7F. Android includes Ol Chiki script support and Noto font coverage on current versions, but the team must bundle and test the required TextMeshPro font assets. Use the locale tag `sat-Olck` and an English fallback.

- [Unicode Ol Chiki chart](https://www.unicode.org/charts/PDF/U1C50.pdf)
- [Unicode 5.1 release](https://www.unicode.org/versions/Unicode5.1.0/)
- [Android UnicodeScript API](https://developer.android.com/reference/java/lang/Character.UnicodeScript)
- [Unity Localization](https://docs.unity3d.com/Packages/com.unity.localization@1.5/manual/index.html)
- [TextMeshPro font fallbacks](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.2/manual/FontAssetsFallback.html)

## Comparable projects

- [CDC VR Mine Rescue Training](https://github.com/CDCgov/vr-mine-rescue-training) has useful scenario, execution, and debrief boundaries. Its headset and desktop assumptions do not fit this mobile product.
- [SIH railway AR navigation](https://github.com/Mrunalisa/SIH-1710-Enhancing-Navigation-for-Railway-Station-Facilities-and-Locations) documents ARCore anchors and navigation. It does not supply training assessment or offline certification.
- [AURA](https://github.com/QuantumCoderrr/AURA) demonstrates an industrial-maintenance pitch and hotspot interface, but its AR viewport is a web simulation rather than tracked mobile AR.
- [Voco App](https://github.com/uzibytes/Voco_App) is an older Flutter learning prototype for visually impaired users. It contains no AR, safety curriculum, offline queue, certificate system, or compliance dashboard, so it is not a suitable codebase.

## Claims excluded from the pitch

Do not repeat the problem statement's claim that classroom retention falls below 20 percent after one week. No primary mining-safety study supporting that exact number was found.

Do not repeat the claim that DGMS recorded 48 fatal mine accidents in Jharkhand in 2022 and 2023. Public parliamentary and ministry data do not support that exact state-level figure. The product case does not depend on either statistic.
