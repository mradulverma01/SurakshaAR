using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SurakshaAR.Content;
using SurakshaAR.Domain.Catalog;
using SurakshaAR.Domain.Persistence;
using SurakshaAR.Domain.Training;
using SurakshaAR.Infrastructure.Catalog;
using SurakshaAR.Infrastructure.Persistence;
using SurakshaAR.Infrastructure.Sync;
using SurakshaAR.Scene;
using UnityEngine;

namespace SurakshaAR.Application
{
    public sealed class TrainingCoordinator : MonoBehaviour
    {
        [SerializeField]
        private OfflineContentInstaller contentInstaller = null!;

        [SerializeField]
        private TrainingSceneController trainingScene = null!;

        [SerializeField]
        private string workerId = string.Empty;

        private IAttemptStore? attemptStore;
        private IAttemptRemote? attemptRemote;
        private ITrainingCatalog? catalog;
        private MobileTrainingSession? launcherSession;
        private IReadOnlyList<ScenarioBundle> availableBundles = Array.Empty<ScenarioBundle>();
        private bool contentReady;
        private bool launcherReady;
        private bool isTrainingActive;
        private bool attemptSaved;
        private bool synchronizing;
        private float nextSyncAt;

        public event Action<AttemptResult>? AttemptSaved;

        public event Action<SyncReport>? Synchronized;

        private void Update()
        {
            if (Time.unscaledTime < nextSyncAt)
            {
                return;
            }

            nextSyncAt = Time.unscaledTime + 30f;
            TrySynchronize();
        }

        private async void Start()
        {
            trainingScene.Updated += HandleTrainingUpdate;
            try
            {
                await contentInstaller.Install();
                contentReady = true;
                await TryPrepareLauncher();
            }
            catch (Exception error)
            {
                Debug.LogException(error, this);
            }
        }

        private void OnGUI()
        {
            if (!launcherReady || isTrainingActive)
            {
                if (isTrainingActive && GUI.Button(new Rect(24f, Screen.height - 80f, 160f, 40f), "Leave training"))
                {
                    LeaveTraining();
                }
                return;
            }

            var panel = new Rect(24f, 24f, Mathf.Min(520f, Screen.width - 48f), 220f + availableBundles.Count * 48f);
            GUI.Box(panel, "Choose training");

            var y = panel.y + 36f;
            foreach (var bundle in availableBundles)
            {
                if (GUI.Button(new Rect(panel.x + 18f, y, panel.width - 36f, 36f), bundle.Id))
                {
                    SelectModule(bundle.Id);
                }
                y += 48f;
            }

            if (string.IsNullOrWhiteSpace(workerId))
            {
                GUI.Label(new Rect(panel.x + 18f, y + 8f, panel.width - 36f, 28f), "Provision a worker first.");
            }
        }

        public void ConfigureRemote(IAttemptRemote remote)
        {
            attemptRemote = remote ?? throw new ArgumentNullException(nameof(remote));
            TrySynchronize();
        }

        public void ProvisionWorker(string provisionedWorkerId)
        {
            if (string.IsNullOrWhiteSpace(provisionedWorkerId))
            {
                throw new ArgumentException("A provisioned worker id is required.", nameof(provisionedWorkerId));
            }

            workerId = provisionedWorkerId;
            _ = TryPrepareLauncher();
        }

        public async Task<SyncReport> Sync(CancellationToken cancellationToken)
        {
            if (attemptStore == null || attemptRemote == null)
            {
                throw new InvalidOperationException("Local storage and the authenticated remote must be configured first.");
            }

            var report = await new AttemptSync(attemptStore, attemptRemote, 20).SyncPending(cancellationToken);
            Synchronized?.Invoke(report);
            return report;
        }

        private async Task TryPrepareLauncher()
        {
            if (!contentReady || string.IsNullOrWhiteSpace(workerId) || launcherReady)
            {
                return;
            }

            var installRoot = contentInstaller.InstallRoot;
            catalog = new JsonTrainingCatalog(Path.Combine(installRoot, "Scenarios"));
            attemptStore = new JsonAttemptStore(Path.Combine(Application.persistentDataPath, "attempts.json"));
            availableBundles = await catalog.List().ConfigureAwait(false);
            if (availableBundles.Count == 0)
            {
                var fallback = new[] { "fire_001", "gas_001" };
                var bundles = new List<ScenarioBundle>();
                foreach (var id in fallback)
                {
                    try { bundles.Add(await catalog.Get(id).ConfigureAwait(false)); } catch { }
                }
                availableBundles = bundles;
            }

            launcherSession = new MobileTrainingSession(availableBundles);
            launcherReady = true;
        }

        private void SelectModule(string moduleId)
        {
            if (launcherSession == null || catalog == null || string.IsNullOrWhiteSpace(workerId))
            {
                return;
            }

            var bundle = availableBundles.SingleOrDefault(b => b.Id == moduleId);
            if (bundle == null)
            {
                Debug.LogWarning("Selected training module is not available offline: " + moduleId);
                return;
            }

            isTrainingActive = true;
            attemptSaved = false;
            var context = new AttemptContext(
                Guid.NewGuid(),
                workerId,
                SystemInfo.deviceUniqueIdentifier,
                DateTimeOffset.UtcNow);
            trainingScene.StartAttempt(bundle, context);
        }

        private void LeaveTraining()
        {
            if (!isTrainingActive)
            {
                return;
            }

            try
            {
                var result = trainingScene.LeaveAttempt();
                _ = SaveResult(result);
            }
            catch (Exception error)
            {
                Debug.LogWarning("Leave failed: " + error.Message);
            }
            finally
            {
                isTrainingActive = false;
            }
        }

        private async void HandleTrainingUpdate(TrainingUpdate update)
        {
            if (!update.State.Completed || attemptSaved || attemptStore == null)
            {
                if (update.State.Completed)
                {
                    isTrainingActive = false;
                }
                return;
            }

            attemptSaved = true;
            try
            {
                var result = trainingScene.FinishAttempt();
                await SaveResult(result).ConfigureAwait(false);
            }
            catch (Exception error)
            {
                attemptSaved = false;
                Debug.LogException(error, this);
            }
            finally
            {
                isTrainingActive = false;
            }
        }

        private async Task SaveResult(AttemptResult result)
        {
            if (attemptStore == null)
            {
                return;
            }

            await attemptStore.Save(result).ConfigureAwait(false);
            AttemptSaved?.Invoke(result);
            TrySynchronize();
        }

        private async void TrySynchronize()
        {
            if (synchronizing
                || attemptStore == null
                || attemptRemote == null
                || Application.internetReachability == NetworkReachability.NotReachable)
            {
                return;
            }

            synchronizing = true;
            try
            {
                await Sync(CancellationToken.None);
            }
            catch (Exception error)
            {
                Debug.LogWarning("Pending attempts remain local: " + error.Message);
            }
            finally
            {
                synchronizing = false;
            }
        }
    }
}
