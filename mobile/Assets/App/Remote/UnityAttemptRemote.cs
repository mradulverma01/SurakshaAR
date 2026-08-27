using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SurakshaAR.Domain.Persistence;
using SurakshaAR.Infrastructure.Sync;
using UnityEngine;
using UnityEngine.Networking;

namespace SurakshaAR.Remote
{
    public interface ISessionTokenProvider
    {
        Task<string> GetAccessToken(CancellationToken cancellationToken);
    }

    public sealed class UnityAttemptRemote : IAttemptRemote
    {
        private readonly string endpoint;
        private readonly string publishableKey;
        private readonly ISessionTokenProvider tokenProvider;

        public UnityAttemptRemote(
            string supabaseUrl,
            string publishableKey,
            ISessionTokenProvider tokenProvider)
        {
            endpoint = supabaseUrl.TrimEnd('/') + "/functions/v1/sync-attempt";
            this.publishableKey = publishableKey;
            this.tokenProvider = tokenProvider;
        }

        public async Task<RemoteSyncResult> Submit(
            IReadOnlyList<PendingAttempt> attempts,
            CancellationToken cancellationToken)
        {
            var token = await tokenProvider.GetAccessToken(cancellationToken);
            var accepted = new List<Guid>();
            var rejected = new List<Guid>();
            var certificateCodes = new Dictionary<Guid, string>();

            foreach (var attempt in attempts)
            {
                var payload = SyncPayload.From(attempt);
                var body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload));
                using (var request = new UnityWebRequest(endpoint, UnityWebRequest.kHttpVerbPOST))
                {
                    request.uploadHandler = new UploadHandlerRaw(body);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("content-type", "application/json");
                    request.SetRequestHeader("apikey", publishableKey);
                    request.SetRequestHeader("authorization", "Bearer " + token);

                    await UnityWebRequestHelper.SendAsync(request, cancellationToken);
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        accepted.Add(attempt.Result.AttemptId);
                        var response = JsonUtility.FromJson<SyncResponse>(request.downloadHandler.text);
                        if (response != null && !string.IsNullOrWhiteSpace(response.certificateCode))
                        {
                            certificateCodes[attempt.Result.AttemptId] = response.certificateCode;
                        }
                    }
                    else if (request.responseCode == 400 || request.responseCode == 422 || request.responseCode == 403)
                    {
                        rejected.Add(attempt.Result.AttemptId);
                    }
                    else
                    {
                        throw new InvalidOperationException("Attempt synchronization failed: " + request.error);
                    }
                }
            }

            return new RemoteSyncResult(accepted, rejected, certificateCodes);
        }

        [Serializable]
        private sealed class SyncPayload
        {
            public string attemptId = string.Empty;
            public string workerId = string.Empty;
            public string deviceId = string.Empty;
            public string moduleId = string.Empty;
            public int moduleVersion;
            public string startedAt = string.Empty;
            public string completedAt = string.Empty;
            public int clientScore;
            public EventPayload[] events = Array.Empty<EventPayload>();

            public static SyncPayload From(PendingAttempt pending)
            {
                return new SyncPayload
                {
                    attemptId = pending.Result.AttemptId.ToString(),
                    workerId = pending.Result.WorkerId,
                    deviceId = pending.Result.DeviceId,
                    moduleId = pending.Result.ModuleId,
                    moduleVersion = pending.Result.ModuleVersion,
                    startedAt = pending.Result.StartedAt.ToString("O"),
                    completedAt = pending.SavedAt.ToString("O"),
                    clientScore = pending.Result.Score,
                    events = pending.Result.Events.Select(EventPayload.From).ToArray(),
                };
            }
        }

        [Serializable]
        private sealed class EventPayload
        {
            public int sequence;
            public string stepId = string.Empty;
            public string kind = string.Empty;
            public string targetId = string.Empty;

            public static EventPayload From(SurakshaAR.Domain.Training.AttemptEvent attemptEvent)
            {
                return new EventPayload
                {
                    sequence = attemptEvent.Sequence,
                    stepId = attemptEvent.StepId,
                    kind = attemptEvent.Action.Kind,
                    targetId = attemptEvent.Action.TargetId,
                };
            }
        }

        [Serializable]
        private sealed class SyncResponse
        {
            public bool accepted;
            public string certificateCode = string.Empty;
        }
    }
}
