using System;
using System.IO;
using System.Threading;
using SurakshaAR.Infrastructure.Persistence;
using SurakshaAR.Remote;
using UnityEngine;

namespace SurakshaAR.Application
{
    public sealed class BackendBootstrap : MonoBehaviour
    {
        [SerializeField]
        private TrainingCoordinator coordinator = null!;

        [SerializeField]
        private string supabaseUrl = string.Empty;

        [SerializeField]
        private string publishableKey = string.Empty;

        private SupabaseSessionTokenProvider? session;
        private string email = string.Empty;
        private string password = string.Empty;
        private string workerId = string.Empty;
        private string status = "Provision this device while online.";
        private bool signingIn;

        private void Start()
        {
            if (string.IsNullOrWhiteSpace(supabaseUrl) || string.IsNullOrWhiteSpace(publishableKey))
            {
                status = "Add the Supabase URL and publishable key in the inspector.";
                return;
            }

            session = new SupabaseSessionTokenProvider(
                supabaseUrl,
                publishableKey,
                new JsonProvisionedWorkerStore(Path.Combine(Application.persistentDataPath, "provisioned-worker.json")));
            if (session.HasProvisionedWorker)
            {
                coordinator.ProvisionWorker(session.WorkerId);
                status = "Offline training is available. Sign in before synchronization.";
            }
        }

        private void OnGUI()
        {
            if (session == null || session.HasAuthenticatedSession)
            {
                return;
            }

            var panel = new Rect(24f, Screen.height - 260f, Mathf.Min(520f, Screen.width - 48f), 236f);
            GUI.Box(panel, "Trainer provisioning");
            GUI.Label(new Rect(panel.x + 18f, panel.y + 34f, 90f, 28f), "Email");
            email = GUI.TextField(new Rect(panel.x + 108f, panel.y + 34f, panel.width - 126f, 28f), email);
            GUI.Label(new Rect(panel.x + 18f, panel.y + 70f, 90f, 28f), "Password");
            password = GUI.PasswordField(new Rect(panel.x + 108f, panel.y + 70f, panel.width - 126f, 28f), password, '*');
            GUI.Label(new Rect(panel.x + 18f, panel.y + 106f, 90f, 28f), "Worker ID");
            workerId = GUI.TextField(new Rect(panel.x + 108f, panel.y + 106f, panel.width - 126f, 28f), workerId);
            GUI.Label(new Rect(panel.x + 18f, panel.y + 144f, panel.width - 36f, 28f), status);

            GUI.enabled = !signingIn;
            if (GUI.Button(new Rect(panel.x + 18f, panel.y + 182f, panel.width - 36f, 34f), "Provision worker"))
            {
                Provision();
            }
            GUI.enabled = true;
        }

        private async void Provision()
        {
            if (session == null || signingIn)
            {
                return;
            }

            signingIn = true;
            status = "Signing in...";
            try
            {
                await session.SignIn(email, password, CancellationToken.None);
                password = string.Empty;
                await session.ProvisionWorker(workerId, CancellationToken.None);
                ConnectProvisionedWorker();
                status = "Provisioned. Offline training is available.";
            }
            catch (Exception error)
            {
                status = error.Message;
            }
            finally
            {
                signingIn = false;
            }
        }

        private void ConnectProvisionedWorker()
        {
            coordinator.ProvisionWorker(session!.WorkerId);
            coordinator.ConfigureRemote(new UnityAttemptRemote(supabaseUrl, publishableKey, session));
        }
    }
}
