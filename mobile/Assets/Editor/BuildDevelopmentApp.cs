using System.IO;
using DiagnosticsProcess = System.Diagnostics.Process;
using DiagnosticsProcessStartInfo = System.Diagnostics.ProcessStartInfo;
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
              if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
              {
                  throw new BuildFailedException("Could not switch the active build target to Android.");
              }
              PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
              PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
              Debug.Log($"Android build settings: backend={PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android)}, architectures={PlayerSettings.Android.targetArchitectures}");
              var gradleProjectPath = Path.Combine("Builds", "SurakshaAR-dev-gradle");
              if (Directory.Exists(gradleProjectPath))
              {
                  Directory.Delete(gradleProjectPath, true);
              }
              EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
              var report = BuildPipeline.BuildPlayer(
                scenes,
                gradleProjectPath,
                BuildTarget.Android,
                BuildOptions.Development | BuildOptions.AllowDebugging);

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException($"Android development build failed: {report.summary.result}");
            }

            var androidPlayerPath = Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines", "AndroidPlayer");
            var javaPath = Path.Combine(androidPlayerPath, "OpenJDK", "bin", "java");
            var gradleLauncherPath = Path.Combine(androidPlayerPath, "Tools", "gradle", "lib", "gradle-launcher-9.1.0.jar");
            var launcherPath = Path.Combine(gradleProjectPath, "launcher");
            var gradleApkPath = Path.GetFullPath(Path.Combine(launcherPath, "build", "outputs", "apk", "debug", "launcher-debug.apk"));
            var apkPath = Path.GetFullPath(Path.Combine("Builds", "SurakshaAR-dev.apk"));
            var gradleLogPath = Path.GetFullPath(Path.Combine("Builds", "SurakshaAR-gradle-build.log"));
            var unityPid = DiagnosticsProcess.GetCurrentProcess().Id;
            var deferredCommand = $"while kill -0 {unityPid} 2>/dev/null; do sleep 1; done; {javaPath} -Xmx768m -classpath {gradleLauncherPath} org.gradle.launcher.GradleMain --no-daemon --max-workers=1 -Dorg.gradle.jvmargs=-Xmx768m -Dorg.gradle.parallel=false assembleDebug >> {gradleLogPath} 2>&1 && cp {gradleApkPath} {apkPath}";
            var gradleProcess = new DiagnosticsProcess
            {
                StartInfo = new DiagnosticsProcessStartInfo
                {
                    FileName = "/bin/sh",
                    Arguments = $"-c \"{deferredCommand}\"",
                    WorkingDirectory = Path.GetFullPath(launcherPath),
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            if (!gradleProcess.Start())
            {
                throw new BuildFailedException("Could not schedule the constrained Gradle build.");
            }
            Debug.Log($"Unity Android export succeeded; constrained Gradle build scheduled after editor exit for {apkPath}");
        }
    }
}
