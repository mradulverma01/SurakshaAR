using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Editor
{
    public static class CreateCoreScenes
    {
        [MenuItem("Suraksha AR/Create Core App Scenes (Phases 2-4)")]
        public static void CreateAll()
        {
            CreateMainMenu();
            CreateDashboard();
            CreateSettings();
            CreateLoginMenu();
            CreateFireTrainingShell();
            CreateGasTrainingShell();
            CreateReusablePrefabs();
            Debug.Log("Core app scenes created. See Assets/Scenes/");
        }

        private static void CreateMainMenu()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cam = new GameObject("Main Camera"); cam.AddComponent<Camera>(); cam.tag = "MainCamera"; cam.AddComponent<AudioListener>();
            var canvasObj = new GameObject("Canvas"); var canvas = canvasObj.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvasObj.AddComponent<CanvasScaler>(); canvasObj.AddComponent<GraphicRaycaster>();
            CreateButton(canvasObj.transform, "Start Training", new Vector2(0, 40), "NavigationController.GoToTraining");
            CreateButton(canvasObj.transform, "View Certificate", new Vector2(0, -20), "NavigationController.GoToDashboard");
            CreateButton(canvasObj.transform, "Settings", new Vector2(0, -80), "NavigationController.GoToSettings");
            CreateLabel(canvasObj.transform, "Suraksha AR", 32, new Vector2(0, 120));
            EnsureFolder("Assets/Scenes"); EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        }

        private static void CreateDashboard()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cam = new GameObject("Main Camera"); cam.AddComponent<Camera>(); cam.tag = "MainCamera"; cam.AddComponent<AudioListener>();
            var canvasObj = new GameObject("Canvas"); var canvas = canvasObj.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvasObj.AddComponent<CanvasScaler>(); canvasObj.AddComponent<GraphicRaycaster>();
            CreateLabel(canvasObj.transform, "Training Progress", 24, new Vector2(0, 160));
            CreateCard(canvasObj.transform, "Fire Response", "5/10 chapters", new Vector2(0, 80));
            CreateCard(canvasObj.transform, "Gas Leak Protocol", "0/8 chapters", new Vector2(0, 10));
            CreateButton(canvasObj.transform, "Sync Progress", new Vector2(0, -80), "");
            CreateButton(canvasObj.transform, "Back to Menu", new Vector2(0, -140), "NavigationController.GoToMainMenu");
            EnsureFolder("Assets/Scenes"); EditorSceneManager.SaveScene(scene, "Assets/Scenes/Dashboard.unity");
        }

        private static void CreateSettings()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cam = new GameObject("Main Camera"); cam.AddComponent<Camera>(); cam.tag = "MainCamera"; cam.AddComponent<AudioListener>();
            var canvasObj = new GameObject("Canvas"); var canvas = canvasObj.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvasObj.AddComponent<CanvasScaler>(); canvasObj.AddComponent<GraphicRaycaster>();
            CreateLabel(canvasObj.transform, "Settings", 24, new Vector2(0, 140));
            CreateLabel(canvasObj.transform, "Trainer: [Name]", 14, new Vector2(0, 80));
            CreateLabel(canvasObj.transform, "Worker ID: [Device ID]", 14, new Vector2(0, 50));
            CreateButton(canvasObj.transform, "Change Trainer", new Vector2(0, 0), "");
            CreateButton(canvasObj.transform, "Clear Local Data", new Vector2(0, -40), "");
            CreateButton(canvasObj.transform, "Back", new Vector2(0, -100), "NavigationController.GoToMainMenu");
            EnsureFolder("Assets/Scenes"); EditorSceneManager.SaveScene(scene, "Assets/Scenes/Settings.unity");
        }

        private static void CreateLoginMenu()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cam = new GameObject("Main Camera"); cam.AddComponent<Camera>(); cam.tag = "MainCamera"; cam.AddComponent<AudioListener>();
            var canvasObj = new GameObject("Canvas"); var canvas = canvasObj.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvasObj.AddComponent<CanvasScaler>(); canvasObj.AddComponent<GraphicRaycaster>();
            CreateLabel(canvasObj.transform, "Suraksha AR", 28, new Vector2(0, 140));
            CreateInputField(canvasObj.transform, "Email", new Vector2(0, 60));
            CreateInputField(canvasObj.transform, "Password", new Vector2(0, 10));
            CreateButton(canvasObj.transform, "Login", new Vector2(0, -50), "");
            CreateLabel(canvasObj.transform, "Offline Mode", 12, new Vector2(0, -100));
            EnsureFolder("Assets/Scenes"); EditorSceneManager.SaveScene(scene, "Assets/Scenes/LoginMenu.unity");
        }

        private static void CreateFireTrainingShell()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cam = new GameObject("Main Camera"); cam.AddComponent<Camera>(); cam.tag = "MainCamera"; cam.AddComponent<AudioListener>();
            var canvasObj = new GameObject("Canvas"); var canvas = canvasObj.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvasObj.AddComponent<CanvasScaler>(); canvasObj.AddComponent<GraphicRaycaster>();
            CreateLabel(canvasObj.transform, "Fire Response Training", 18, new Vector2(0, 150));
            CreateLabel(canvasObj.transform, "Chapter 3/10", 12, new Vector2(0, 120));
            CreateProgressBar(canvasObj.transform, new Vector2(0, 90));
            CreateLabel(canvasObj.transform, "[Instructions from backend]", 14, new Vector2(0, 20));
            var arContent = new GameObject("ARContent"); arContent.transform.SetParent(canvasObj.transform, false);
            CreateLabel(arContent.transform, "ARContent (Mradul fills this)", 10, Vector2.zero);
            EnsureFolder("Assets/Scenes"); EditorSceneManager.SaveScene(scene, "Assets/Scenes/FireTraining.unity");
        }

        private static void CreateGasTrainingShell()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cam = new GameObject("Main Camera"); cam.AddComponent<Camera>(); cam.tag = "MainCamera"; cam.AddComponent<AudioListener>();
            var canvasObj = new GameObject("Canvas"); var canvas = canvasObj.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvasObj.AddComponent<CanvasScaler>(); canvasObj.AddComponent<GraphicRaycaster>();
            CreateLabel(canvasObj.transform, "Gas Training", 18, new Vector2(0, 150));
            CreateProgressBar(canvasObj.transform, new Vector2(0, 90));
            var arContent = new GameObject("ARContent"); arContent.transform.SetParent(canvasObj.transform, false);
            CreateLabel(arContent.transform, "ARContent (Mradul fills this)", 10, Vector2.zero);
            EnsureFolder("Assets/Scenes"); EditorSceneManager.SaveScene(scene, "Assets/Scenes/GasTraining.unity");
        }

        private static void CreateReusablePrefabs()
        {
            EnsureFolder("Assets/Prefabs"); EnsureFolder("Assets/Prefabs/UI");
            CreatePrefab("Assets/Prefabs/UI/ProgressBar.prefab", "ProgressBar");
            CreatePrefab("Assets/Prefabs/UI/SyncIndicator.prefab", "Syncing...");
            CreatePrefab("Assets/Prefabs/UI/ConfirmDialog.prefab", "Confirm?");
            CreatePrefab("Assets/Prefabs/UI/LoadingSpinner.prefab", "Loading...");
        }

        private static void CreatePrefab(string path, string label)
        {
            var go = new GameObject(label); var txt = new GameObject("Text"); txt.transform.SetParent(go.transform, false);
            var t = txt.AddComponent<Text>(); t.text = label; t.alignment = TextAnchor.MiddleCenter; t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            PrefabUtility.SaveAsPrefabAsset(go, path); Object.DestroyImmediate(go);
        }

        private static void CreateButton(Transform parent, string label, Vector2 pos, string onClick)
        {
            var go = new GameObject("Button_" + label); go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>(); rect.anchoredPosition = pos; rect.sizeDelta = new Vector2(260, 36);
            var img = go.AddComponent<Image>(); img.color = new Color(0.16f, 0.45f, 1f);
            var btn = go.AddComponent<Button>();
            var txtObj = new GameObject("Text"); txtObj.transform.SetParent(go.transform, false);
            var txt = txtObj.AddComponent<Text>(); txt.text = label; txt.alignment = TextAnchor.MiddleCenter; txt.color = Color.white; txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var txtRect = txtObj.GetComponent<RectTransform>(); txtRect.anchorMin = Vector2.zero; txtRect.anchorMax = Vector2.one; txtRect.offsetMin = Vector2.zero; txtRect.offsetMax = Vector2.zero;
        }

        private static void CreateLabel(Transform parent, string label, int size, Vector2 pos)
        {
            var go = new GameObject("Label_" + label); go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>(); rect.anchoredPosition = pos; rect.sizeDelta = new Vector2(400, 30);
            var txt = go.AddComponent<Text>(); txt.text = label; txt.alignment = TextAnchor.MiddleCenter; txt.fontSize = size; txt.color = Color.white; txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static void CreateInputField(Transform parent, string placeholder, Vector2 pos)
        {
            var go = new GameObject("Input_" + placeholder); go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>(); rect.anchoredPosition = pos; rect.sizeDelta = new Vector2(260, 30);
            var img = go.AddComponent<Image>(); img.color = new Color(1, 1, 1, 0.1f);
            var input = go.AddComponent<InputField>();
            var placeholderObj = new GameObject("Placeholder"); placeholderObj.transform.SetParent(go.transform, false);
            var ph = placeholderObj.AddComponent<Text>(); ph.text = placeholder; ph.color = new Color(1, 1, 1, 0.5f); ph.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var textObj = new GameObject("Text"); textObj.transform.SetParent(go.transform, false);
            var t = textObj.AddComponent<Text>(); t.color = Color.white; t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            input.placeholder = ph; input.textComponent = t;
        }

        private static void CreateCard(Transform parent, string title, string subtitle, Vector2 pos)
        {
            var go = new GameObject("Card_" + title); go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>(); rect.anchoredPosition = pos; rect.sizeDelta = new Vector2(340, 50);
            var img = go.AddComponent<Image>(); img.color = new Color(1, 1, 1, 0.08f);
            CreateLabel(go.transform, title, 14, new Vector2(0, 8)); CreateLabel(go.transform, subtitle, 10, new Vector2(0, -12));
        }

        private static void CreateProgressBar(Transform parent, Vector2 pos)
        {
            var go = new GameObject("ProgressBar"); go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>(); rect.anchoredPosition = pos; rect.sizeDelta = new Vector2(300, 12);
            var img = go.AddComponent<Image>(); img.color = new Color(1, 1, 1, 0.15f);
        }

        private static void EnsureFolder(string path)
        {
            var pieces = path.Split('/'); var cur = pieces[0];
            for (var i = 1; i < pieces.Length; i++) { var next = cur + "/" + pieces[i]; if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(cur, pieces[i]); cur = next; }
        }
    }
}
