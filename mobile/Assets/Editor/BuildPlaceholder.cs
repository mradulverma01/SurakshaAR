using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurakshaAR.Editor
{
    public static class BuildPlaceholder
    {
        [MenuItem("Suraksha AR/Build Placeholder APK")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            var cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.06f, 0.08f);
            camObj.AddComponent<AudioListener>();

            var canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var textObj = new GameObject("Placeholder Text");
            textObj.transform.SetParent(canvasObj.transform, false);
            var rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var text = textObj.AddComponent<UnityEngine.UI.Text>();
            text.text = "Suraksha AR\n\nPlaceholder build\n\nNo modules — just a placeholder.\n\nIf you see this on your phone, the APK pipeline works.";
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 28;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var buildTextObj = new GameObject("Build Info");
            buildTextObj.transform.SetParent(canvasObj.transform, false);
            var buildRect = buildTextObj.AddComponent<RectTransform>();
            buildRect.anchorMin = new Vector2(0, 0);
            buildRect.anchorMax = new Vector2(1, 0);
            buildRect.anchoredPosition = new Vector2(0, 40);
            buildRect.sizeDelta = new Vector2(0, 40);
            var buildText = buildTextObj.AddComponent<UnityEngine.UI.Text>();
            buildText.text = "v0.0.1 placeholder • " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            buildText.alignment = TextAnchor.MiddleCenter;
            buildText.fontSize = 14;
            buildText.color = new Color(1, 1, 1, 0.6f);
            buildText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            const string scenePath = "Assets/Scenes/Placeholder.unity";
            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, scenePath);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(scenePath, true),
            };

            PlayerSettings.productName = "Suraksha AR";
            PlayerSettings.companyName = "SIH";
            PlayerSettings.applicationIdentifier = "com.sih.surakshaar";
            PlayerSettings.bundleVersion = "0.0.1";
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            var buildPath = GetBuildOutputPath("Builds/SurakshaAR-placeholder.apk");
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(buildPath) ?? "Builds");
            var report = BuildPipeline.BuildPlayer(new[] { scenePath }, buildPath, BuildTarget.Android, BuildOptions.None);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                throw new System.Exception("Build failed: " + report.summary.result);
            }

            Debug.Log("Placeholder APK built at " + buildPath);
        }

        private static string GetBuildOutputPath(string fallback)
        {
            var args = System.Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-buildOutput")
                {
                    return args[i + 1];
                }
            }
            return fallback;
        }
    }
}
