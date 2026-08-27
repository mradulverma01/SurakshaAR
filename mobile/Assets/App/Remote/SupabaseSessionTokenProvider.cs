using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SurakshaAR.Infrastructure.Persistence;
using UnityEngine;
using UnityEngine.Networking;

namespace SurakshaAR.Remote
{
    public sealed class SupabaseSessionTokenProvider : ISessionTokenProvider
    {
        private readonly string url;
        private readonly string publishableKey;
        private readonly JsonProvisionedWorkerStore workerStore;
        private string accessToken = string.Empty;
        private string refreshToken;
        private long expiresAt;

        public SupabaseSessionTokenProvider(
            string url,
            string publishableKey,
            JsonProvisionedWorkerStore workerStore)
        {
            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(publishableKey))
            {
                throw new ArgumentException("Supabase URL and publishable key are required.");
            }

            this.url = url.TrimEnd('/');
            this.publishableKey = publishableKey;
            this.workerStore = workerStore ?? throw new ArgumentNullException(nameof(workerStore));
            refreshToken = string.Empty;
            WorkerId = workerStore.Load() ?? string.Empty;
        }

        public string WorkerId { get; private set; }

        public bool HasProvisionedWorker => !string.IsNullOrWhiteSpace(WorkerId);

        public bool HasAuthenticatedSession => !string.IsNullOrWhiteSpace(refreshToken);

        public async Task SignIn(string email, string password, CancellationToken cancellationToken)
        {
            var payload = JsonUtility.ToJson(new PasswordRequest { email = email, password = password });
            await RequestToken("password", payload, cancellationToken);
        }

        public async Task<string> GetAccessToken(CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (!string.IsNullOrWhiteSpace(accessToken) && expiresAt > now + 60)
            {
                return accessToken;
            }

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new InvalidOperationException("The worker must be provisioned online before synchronization.");
            }

            var payload = JsonUtility.ToJson(new RefreshRequest { refresh_token = refreshToken });
            try
            {
                await RequestToken("refresh_token", payload, cancellationToken);
            }
            catch
            {
                accessToken = string.Empty;
                refreshToken = string.Empty;
                throw;
            }
            return accessToken;
        }

        public async Task ProvisionWorker(string workerId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(workerId))
            {
                throw new ArgumentException("A worker id is required.", nameof(workerId));
            }

            var endpoint = url + "/rest/v1/workers?select=id&id=eq." + Uri.EscapeDataString(workerId);
            using (var request = UnityWebRequest.Get(endpoint))
            {
                request.SetRequestHeader("apikey", publishableKey);
                request.SetRequestHeader("authorization", "Bearer " + await GetAccessToken(cancellationToken));
                request.SetRequestHeader("accept", "application/vnd.pgrst.object+json");
                await UnityWebRequestHelper.SendAsync(request, cancellationToken);
                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new InvalidOperationException("The selected worker is not available for this trainer.");
                }
            }

            workerStore.Save(workerId);
            WorkerId = workerId;
        }

        public void Clear()
        {
            accessToken = string.Empty;
            refreshToken = string.Empty;
            WorkerId = string.Empty;
            expiresAt = 0;
            workerStore.Clear();
        }

        private async Task RequestToken(string grantType, string payload, CancellationToken cancellationToken)
        {
            var endpoint = url + "/auth/v1/token?grant_type=" + grantType;
            using (var request = new UnityWebRequest(endpoint, UnityWebRequest.kHttpVerbPOST))
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("content-type", "application/json");
                request.SetRequestHeader("apikey", publishableKey);
                await UnityWebRequestHelper.SendAsync(request, cancellationToken);

                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new InvalidOperationException("Worker provisioning failed: " + request.error);
                }

                var response = JsonUtility.FromJson<TokenResponse>(request.downloadHandler.text);
                if (response == null || string.IsNullOrWhiteSpace(response.access_token))
                {
                    throw new InvalidOperationException("Supabase returned an invalid session.");
                }

                accessToken = response.access_token;
                refreshToken = response.refresh_token;
                expiresAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + response.expires_in;
            }
        }

        [Serializable]
        private sealed class PasswordRequest
        {
            public string email = string.Empty;
            public string password = string.Empty;
        }

        [Serializable]
        private sealed class RefreshRequest
        {
            public string refresh_token = string.Empty;
        }

        [Serializable]
        private sealed class TokenResponse
        {
            public string access_token = string.Empty;
            public string refresh_token = string.Empty;
            public int expires_in;
        }

    }
}
