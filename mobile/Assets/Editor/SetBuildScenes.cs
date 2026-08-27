using UnityEditor;

namespace SurakshaAR.Editor
{
    public static class SetBuildScenes
    {
        [MenuItem("Suraksha AR/Set Build Scenes (All Phases)")]
        public static void SetAll()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/Launcher.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Dashboard.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/FireTraining.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/GasTraining.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Settings.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/LoginMenu.unity", true),
            };
            UnityEngine.Debug.Log("Build scenes set to all 7 phases.");
        }
    }
}
