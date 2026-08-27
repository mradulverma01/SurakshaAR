using System.Collections.Generic;
using SurakshaAR.Application;
using SurakshaAR.Content;
using SurakshaAR.Scene;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Management;

namespace SurakshaAR.Editor
{
    public static class MvpSceneBuilder
    {
        private const string PrefabDirectory = "Assets/Prefabs";
        private const string SceneDirectory = "Assets/Scenes";

        [MenuItem("Suraksha AR/Create MVP scenes")]
        public static void CreateScenes()
        {
            EnsureFolder(PrefabDirectory);
            EnsureFolder(SceneDirectory);
            EnsureFolder("Assets/XR/Settings");
            EnableArCore();
            EnsureFolder("Assets/Fonts/Generated");
            CreateFontAsset("Assets/Fonts/NotoSansDevanagari.ttf", "Assets/Fonts/Generated/NotoSansDevanagari.asset");
            CreateFontAsset("Assets/Fonts/NotoSansOlChiki.ttf", "Assets/Fonts/Generated/NotoSansOlChiki.asset");

            var firePrefab = CreateScenarioPrefab(
                "FireScenario",
                new[]
                {
                    Target("Fire", "identify_hazard", "select", "electrical_fire", new Vector3(0f, 0.25f, 1.5f), Color.red),
                    Target("CO2 Extinguisher", "select_extinguisher", "select", "co2_extinguisher", new Vector3(-0.8f, 0.25f, 1.1f), Color.white),
                    Target("Water Extinguisher", "wrong_extinguisher", "select", "water_extinguisher", new Vector3(0.8f, 0.25f, 1.1f), Color.blue),
                    Target("Safety Pin", "remove_pin", "interact", "extinguisher_pin", new Vector3(-0.8f, 0.65f, 1.1f), Color.yellow),
                    Target("Fire Base", "aim", "aim", "fire_base", new Vector3(0f, 0.05f, 1.5f), new Color(1f, 0.45f, 0f)),
                    Target("Handle", "discharge", "hold", "extinguisher_handle", new Vector3(-0.8f, 0.9f, 1.1f), Color.gray),
                    Target("Safe Exit", "exit_route", "waypoint_sequence", "safe_exit_a", new Vector3(1.2f, 0.5f, 2.4f), Color.green),
                });

            var gasPrefab = CreateScenarioPrefab(
                "GasScenario",
                new[]
                {
                    Target("Methane Hazard", "recognize_hazard_zone", "select", "methane_hazard_zone", new Vector3(0f, 0.35f, 1.6f), new Color(0.75f, 0.2f, 0.85f)),
                    Target("Unsafe Gas Entry", "enter_hazard_zone", "waypoint_enter", "methane_hazard_zone", new Vector3(0f, 0.05f, 1.6f), Color.red),
                    Target("Safe Zone", "withdraw", "waypoint_enter", "safe_zone", new Vector3(-1.1f, 0.1f, 0.8f), Color.green),
                    Target("Supervisor Radio", "report_hazard", "interact", "supervisor_radio", new Vector3(-0.7f, 0.35f, 1.1f), Color.cyan),
                    Target("Self Rescuer", "select_ppe", "select", "approved_self_rescuer", new Vector3(0.7f, 0.3f, 1.1f), Color.yellow),
                    Target("Buddy", "buddy_check", "confirm", "buddy_present", new Vector3(1.1f, 0.75f, 1.6f), Color.white),
                    Target("Safe Exit", "exit_route", "waypoint_sequence", "safe_exit_a", new Vector3(0f, 0.5f, 2.6f), Color.green),
                });

            var fireScene = CreateTrainingScene("FireTraining", "fire_001", firePrefab);
            var gasScene = CreateTrainingScene("GasTraining", "gas_001", gasPrefab);
            var launcherScene = CreateLauncherScene("Launcher", new[] { firePrefab, gasPrefab });
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(launcherScene, true),
                new EditorBuildSettingsScene(fireScene, true),
                new EditorBuildSettingsScene(gasScene, true),
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static GameObject CreateScenarioPrefab(string name, IReadOnlyList<TargetDefinition> targets)
        {
            var root = new GameObject(name);
            foreach (var definition in targets)
            {
                var targetObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                targetObject.name = definition.Name;
                targetObject.transform.SetParent(root.transform, false);
                targetObject.transform.localPosition = definition.Position;
                targetObject.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
                targetObject.GetComponent<Renderer>().sharedMaterial = Material(definition.Color);

                var target = targetObject.AddComponent<TrainingTarget>();
                SetReference(target, "interactionId", definition.InteractionId);
                SetReference(target, "actionKind", definition.Kind);
                SetReference(target, "targetId", definition.TargetId);
            }

            var path = PrefabDirectory + "/" + name + ".prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static string CreateTrainingScene(string sceneName, string moduleId, GameObject scenarioPrefab)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var sessionObject = new GameObject("AR Session");
            sessionObject.AddComponent<ARSession>();

            var originObject = new GameObject("XR Origin");
            var origin = originObject.AddComponent<XROrigin>();
            var planeManager = originObject.AddComponent<ARPlaneManager>();
            var raycastManager = originObject.AddComponent<ARRaycastManager>();

            var cameraObject = new GameObject("AR Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(originObject.transform, false);
            var camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<ARCameraManager>();
            cameraObject.AddComponent<ARCameraBackground>();
#pragma warning disable 618
            cameraObject.AddComponent<ARPoseDriver>();
#pragma warning restore 618
            origin.Camera = camera;

            var applicationObject = new GameObject("Training Application");
            var installer = applicationObject.AddComponent<OfflineContentInstaller>();
            var sceneController = applicationObject.AddComponent<TrainingSceneController>();
            var coordinator = applicationObject.AddComponent<TrainingCoordinator>();
            var hud = applicationObject.AddComponent<TrainingHud>();
            var backend = applicationObject.AddComponent<BackendBootstrap>();

            SetReference(sceneController, "raycastManager", raycastManager);
            SetReference(sceneController, "planeManager", planeManager);
            SetReference(sceneController, "arCamera", camera);
            SetReference(sceneController, "scenarioPrefab", scenarioPrefab);
            SetList(sceneController, "scenarioPrefabs", new[] { scenarioPrefab });
            SetReference(coordinator, "contentInstaller", installer);
            SetReference(coordinator, "trainingScene", sceneController);
            SetReference(coordinator, "workerId", string.Empty);
            SetReference(hud, "contentInstaller", installer);
            SetReference(hud, "trainingScene", sceneController);
            SetReference(hud, "devanagariFont", AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/NotoSansDevanagari.ttf"));
            SetReference(hud, "olChikiFont", AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/NotoSansOlChiki.ttf"));
            SetReference(backend, "coordinator", coordinator);

            var path = SceneDirectory + "/" + sceneName + ".unity";
            EditorSceneManager.SaveScene(scene, path);
            return path;
        }

        private static string CreateLauncherScene(string sceneName, IReadOnlyList<GameObject> prefabs)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var sessionObject = new GameObject("AR Session");
            sessionObject.AddComponent<ARSession>();

            var originObject = new GameObject("XR Origin");
            var origin = originObject.AddComponent<XROrigin>();
            var planeManager = originObject.AddComponent<ARPlaneManager>();
            var raycastManager = originObject.AddComponent<ARRaycastManager>();

            var cameraObject = new GameObject("AR Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(originObject.transform, false);
            var camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<ARCameraManager>();
            cameraObject.AddComponent<ARCameraBackground>();
#pragma warning disable 618
            cameraObject.AddComponent<ARPoseDriver>();
#pragma warning restore 618
            origin.Camera = camera;

            var applicationObject = new GameObject("Training Application");
            var installer = applicationObject.AddComponent<OfflineContentInstaller>();
            var sceneController = applicationObject.AddComponent<TrainingSceneController>();
            var coordinator = applicationObject.AddComponent<TrainingCoordinator>();
            var hud = applicationObject.AddComponent<TrainingHud>();
            var backend = applicationObject.AddComponent<BackendBootstrap>();

            SetReference(sceneController, "raycastManager", raycastManager);
            SetReference(sceneController, "planeManager", planeManager);
            SetReference(sceneController, "arCamera", camera);
            SetReference(sceneController, "scenarioPrefab", prefabs[0]);
            SetList(sceneController, "scenarioPrefabs", prefabs);
            SetReference(coordinator, "contentInstaller", installer);
            SetReference(coordinator, "trainingScene", sceneController);
            SetReference(coordinator, "workerId", string.Empty);
            SetReference(hud, "contentInstaller", installer);
            SetReference(hud, "trainingScene", sceneController);
            SetReference(hud, "devanagariFont", AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/NotoSansDevanagari.ttf"));
            SetReference(hud, "olChikiFont", AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/NotoSansOlChiki.ttf"));
            SetReference(backend, "coordinator", coordinator);

            var path = SceneDirectory + "/" + sceneName + ".unity";
            EditorSceneManager.SaveScene(scene, path);
            return path;
        }

        private static Material Material(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader) { color = color };
            var path = AssetDatabase.GenerateUniqueAssetPath("Assets/Prefabs/TrainingTarget.mat");
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void CreateFontAsset(string fontPath, string destinationPath)
        {
            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(destinationPath) != null)
            {
                return;
            }

            var font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
            if (font == null)
            {
                throw new System.IO.FileNotFoundException("Required localization font is missing.", fontPath);
            }

            var fontAsset = TMP_FontAsset.CreateFontAsset(font);
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            AssetDatabase.CreateAsset(fontAsset, destinationPath);
        }

        private static void EnableArCore()
        {
            XRGeneralSettingsPerBuildTarget perBuildTarget;
            var settingsGuids = AssetDatabase.FindAssets("t:XRGeneralSettingsPerBuildTarget");
            if (settingsGuids.Length == 0)
            {
                perBuildTarget = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                AssetDatabase.CreateAsset(perBuildTarget, "Assets/XR/Settings/XRGeneralSettingsPerBuildTarget.asset");
                EditorBuildSettings.AddConfigObject(XRGeneralSettings.settingsKey, perBuildTarget, true);
            }
            else
            {
                var settingsPath = AssetDatabase.GUIDToAssetPath(settingsGuids[0]);
                perBuildTarget = AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(settingsPath);
                EditorBuildSettings.AddConfigObject(XRGeneralSettings.settingsKey, perBuildTarget, true);
            }

            if (!perBuildTarget.HasManagerSettingsForBuildTarget(BuildTargetGroup.Android))
            {
                perBuildTarget.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);
            }

            var settings = perBuildTarget.SettingsForBuildTarget(BuildTargetGroup.Android);
            var manager = perBuildTarget.ManagerSettingsForBuildTarget(BuildTargetGroup.Android);

            if (!XRPackageMetadataStore.AssignLoader(
                manager,
                "UnityEngine.XR.ARCore.ARCoreLoader",
                BuildTargetGroup.Android))
            {
                throw new System.InvalidOperationException("ARCore loader could not be enabled.");
            }

            EditorUtility.SetDirty(settings);
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(perBuildTarget);
            AssetDatabase.SaveAssets();
        }

        private static void SetReference(Object target, string propertyName, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetReference(Object target, string propertyName, string value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetList(Object target, string propertyName, IReadOnlyList<GameObject> values)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(propertyName);
            property.arraySize = values.Count;
            for (var index = 0; index < values.Count; index++)
            {
                property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static TargetDefinition Target(
            string name,
            string interactionId,
            string kind,
            string targetId,
            Vector3 position,
            Color color)
        {
            return new TargetDefinition(name, interactionId, kind, targetId, position, color);
        }

        private static void EnsureFolder(string path)
        {
            var pieces = path.Split('/');
            var current = pieces[0];
            for (var index = 1; index < pieces.Length; index++)
            {
                var next = current + "/" + pieces[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, pieces[index]);
                }
                current = next;
            }
        }

        private sealed class TargetDefinition
        {
            public TargetDefinition(string name, string interactionId, string kind, string targetId, Vector3 position, Color color)
            {
                Name = name;
                InteractionId = interactionId;
                Kind = kind;
                TargetId = targetId;
                Position = position;
                Color = color;
            }

            public string Name { get; }
            public string InteractionId { get; }
            public string Kind { get; }
            public string TargetId { get; }
            public Vector3 Position { get; }
            public Color Color { get; }
        }
    }
}
