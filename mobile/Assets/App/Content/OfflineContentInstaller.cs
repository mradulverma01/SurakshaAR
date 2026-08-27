using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SurakshaAR.Content
{
    public sealed class OfflineContentInstaller : MonoBehaviour
    {
        private static readonly IReadOnlyList<string> BundledFiles = new[]
        {
            "Scenarios/fire_001.v1.json",
            "Scenarios/gas_001.v1.json",
            "Localization/en.json",
            "Localization/hi.json",
            "Localization/sat-Olck.json",
        };

        public event Action<string>? Installed;

        public event Action<Exception>? InstallationFailed;

        private Task? installation;

        public string InstallRoot => Path.Combine(Application.persistentDataPath, "TrainingContent");

        private async void Start()
        {
            try
            {
                await Install();
                Installed?.Invoke(InstallRoot);
            }
            catch (Exception error)
            {
                InstallationFailed?.Invoke(error);
            }
        }

        public Task Install()
        {
            installation ??= InstallFiles();
            return installation;
        }

        private async Task InstallFiles()
        {
            foreach (var relativePath in BundledFiles)
            {
                var destination = Path.Combine(InstallRoot, relativePath);
                if (File.Exists(destination))
                {
                    continue;
                }

                var directory = Path.GetDirectoryName(destination);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var source = Application.streamingAssetsPath.TrimEnd('/') + "/" + relativePath;
                using (var request = UnityWebRequest.Get(source))
                {
                    var completion = new TaskCompletionSource<bool>();
                    request.SendWebRequest().completed += _ => completion.TrySetResult(true);
                    await completion.Task;
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        throw new IOException("Bundled content could not be installed: " + relativePath);
                    }

                    File.WriteAllBytes(destination, request.downloadHandler.data);
                }
            }
        }
    }
}
