using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SurakshaAR.Editor
{
    public static class BuildMvp
    {
        [MenuItem("Suraksha AR/Build MVP AAB")]
        public static void Build()
        {
            BuildWithOutput(GetBuildOutputPath("Builds/SurakshaAR-mvp.aab"));
        }

        public static void BuildAab()
        {
            BuildWithOutput(GetBuildOutputPath("Builds/SurakshaAR-mvp.aab"));
        }

        private static void BuildWithOutput(string outputPath)
        {
            MvpSceneBuilder.CreateScenes();

            var scenes = new[]
            {
                "Assets/Scenes/Launcher.unity",
                "Assets/Scenes/FireTraining.unity",
                "Assets/Scenes/GasTraining.unity",
            };

            foreach (var s in scenes)
            {
                if (!File.Exists(s))
                    throw new FileNotFoundException("Required scene missing after CreateScenes: " + s);
            }

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(scenes[0], true),
                new EditorBuildSettingsScene(scenes[1], true),
            };

            PlayerSettings.productName = "Suraksha AR";
            PlayerSettings.companyName = "SIH";
            PlayerSettings.applicationIdentifier = "com.sih.surakshaar";
            PlayerSettings.bundleVersion = "0.0.1";
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel30;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var isAab = outputPath.EndsWith(".aab", StringComparison.OrdinalIgnoreCase);
            EditorUserBuildSettings.buildAppBundle = isAab;

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None,
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new Exception("MVP build failed: " + report.summary.result + " at " + outputPath);

            Debug.Log("MVP build succeeded at " + outputPath + " (" + report.summary.totalSize + " bytes)");
        }

        private static string GetBuildOutputPath(string fallback)
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
                if (args[i] == "-buildOutput")
                    return args[i + 1];
            return fallback;
        }
    }
}
