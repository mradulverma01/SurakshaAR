using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SurakshaAR.Editor
{
    public static class BuildDevelopmentApp
    {
        public static void Build()
        {
            var scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
            if (scenes.Length == 0)
            {
                throw new BuildFailedException("No enabled scenes are configured in EditorBuildSettings.");
            }

            Directory.CreateDirectory("Builds");
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
            var report = BuildPipeline.BuildPlayer(
                scenes,
                "Builds/SurakshaAR-dev.apk",
                BuildTarget.Android,
                BuildOptions.Development | BuildOptions.AllowDebugging);

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException($"Android development build failed: {report.summary.result}");
            }

            Debug.Log($"Android development APK built at {report.summary.outputPath}");
        }
    }
}
