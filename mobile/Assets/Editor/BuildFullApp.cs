using UnityEditor;
using UnityEngine;

namespace SurakshaAR.Editor
{
    public static class BuildFullApp
    {
        [MenuItem("Suraksha AR/Build Full App APK (All Phases)")]
        public static void Build()
        {
            var scenes = new[]
            {
                "Assets/Scenes/Launcher.unity",
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/Dashboard.unity",
                "Assets/Scenes/FireTraining.unity",
                "Assets/Scenes/GasTraining.unity",
                "Assets/Scenes/Settings.unity",
                "Assets/Scenes/LoginMenu.unity",
            };

            PlayerSettings.productName = "Suraksha AR";
            PlayerSettings.companyName = "SIH";
            PlayerSettings.applicationIdentifier = "com.sih.surakshaar";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            System.IO.Directory.CreateDirectory("Builds");
            var report = BuildPipeline.BuildPlayer(scenes, "Builds/SurakshaAR-full.apk", BuildTarget.Android, BuildOptions.None);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                throw new System.Exception("Build failed: " + report.summary.result);
            }

            Debug.Log("Full APK built at Builds/SurakshaAR-full.apk with all 5 phases");
        }
    }
}
