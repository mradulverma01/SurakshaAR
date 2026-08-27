using System;
using System.IO;
using System.Threading.Tasks;
using SurakshaAR.Content;
using SurakshaAR.Domain.Training;
using SurakshaAR.Infrastructure.Localization;
using SurakshaAR.Scene;
using UnityEngine;

namespace SurakshaAR.Application
{
    public sealed class TrainingHud : MonoBehaviour
    {
        [SerializeField]
        private OfflineContentInstaller contentInstaller = null!;

        [SerializeField]
        private TrainingSceneController trainingScene = null!;

        [SerializeField]
        private Font devanagariFont = null!;

        [SerializeField]
        private Font olChikiFont = null!;

        private JsonLocalizationCatalog? catalog;
        private string locale = "hi";
        private string? cueKey;
        private string cueText = "Loading training...";

        private void Awake()
        {
            trainingScene.Updated += HandleUpdate;
        }

        private async void Start()
        {
            try
            {
                await contentInstaller.Install();
                catalog = new JsonLocalizationCatalog(Path.Combine(contentInstaller.InstallRoot, "Localization"));
                await RefreshCue();
            }
            catch (Exception error)
            {
                cueText = error.Message;
            }
        }

        private void OnDestroy()
        {
            trainingScene.Updated -= HandleUpdate;
        }

        private async void HandleUpdate(TrainingUpdate update)
        {
            cueKey = update.State.Completed ? "status.complete" : update.CueKey;
            await RefreshCue();
        }

        private async Task RefreshCue()
        {
            if (catalog == null || string.IsNullOrWhiteSpace(cueKey))
            {
                return;
            }

            try
            {
                cueText = await catalog.Get(locale, cueKey);
            }
            catch (Exception error)
            {
                cueText = error.Message;
            }
        }

        private void OnGUI()
        {
            var scale = Mathf.Max(1f, Screen.width / 1080f);
            var panel = new Rect(24f * scale, 24f * scale, Mathf.Min(Screen.width - 48f * scale, 720f * scale), 180f * scale);
            GUI.Box(panel, GUIContent.none);

            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(28f * scale),
                wordWrap = true,
                normal = { textColor = Color.white },
                font = locale == "sat-Olck" ? olChikiFont : locale == "hi" ? devanagariFont : null,
            };
            GUI.Label(new Rect(panel.x + 20f, panel.y + 18f, panel.width - 40f, 92f * scale), cueText, style);

            var hindiButton = new GUIStyle(GUI.skin.button) { font = devanagariFont };
            var santaliButton = new GUIStyle(GUI.skin.button) { font = olChikiFont };
            if (GUI.Button(new Rect(panel.x + 20f, panel.yMax - 48f * scale, 100f * scale, 34f * scale), "हिन्दी", hindiButton))
            {
                SelectLocale("hi");
            }
            if (GUI.Button(new Rect(panel.x + 132f * scale, panel.yMax - 48f * scale, 120f * scale, 34f * scale), "ᱥᱟᱱᱛᱟᱲᱤ", santaliButton))
            {
                SelectLocale("sat-Olck");
            }
            if (GUI.Button(new Rect(panel.x + 264f * scale, panel.yMax - 48f * scale, 100f * scale, 34f * scale), "English"))
            {
                SelectLocale("en");
            }
        }

        private async void SelectLocale(string selectedLocale)
        {
            locale = selectedLocale;
            await RefreshCue();
        }
    }
}
