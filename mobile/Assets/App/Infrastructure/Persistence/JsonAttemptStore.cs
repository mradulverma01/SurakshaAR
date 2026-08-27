using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SurakshaAR.Domain.Persistence;
using SurakshaAR.Domain.Training;

namespace SurakshaAR.Infrastructure.Persistence
{
    public sealed class JsonAttemptStore : IAttemptStore
    {
        private readonly string path;
        private readonly SemaphoreSlim gate = new SemaphoreSlim(1, 1);

        public JsonAttemptStore(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("A path is required.", nameof(path));
            }

            this.path = path;
        }

        public async Task Save(AttemptResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            await gate.WaitAsync().ConfigureAwait(false);
            try
            {
                var document = await Load().ConfigureAwait(false);
                if (document.Attempts.All(row => row.AttemptId != result.AttemptId))
                {
                    document.Attempts.Add(AttemptRow.From(result, DateTimeOffset.UtcNow));
                    await Write(document).ConfigureAwait(false);
                }
            }
            finally
            {
                gate.Release();
            }
        }

        public async Task<IReadOnlyList<PendingAttempt>> Pending(int limit)
        {
            if (limit < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(limit));
            }

            await gate.WaitAsync().ConfigureAwait(false);
            try
            {
                var document = await Load().ConfigureAwait(false);
                return document.Attempts
                    .Where(row => !row.Synced && !row.Rejected)
                    .OrderBy(row => row.SavedAt)
                    .Take(limit)
                    .Select(row => new PendingAttempt(row.ToResult(), row.SavedAt))
                    .ToArray();
            }
            finally
            {
                gate.Release();
            }
        }

        public async Task MarkSynced(IReadOnlyCollection<Guid> attemptIds)
        {
            await MarkTerminal(attemptIds, rejected: false).ConfigureAwait(false);
        }

        public async Task MarkRejected(IReadOnlyCollection<Guid> attemptIds)
        {
            await MarkTerminal(attemptIds, rejected: true).ConfigureAwait(false);
        }

        private async Task MarkTerminal(IReadOnlyCollection<Guid> attemptIds, bool rejected)
        {
            if (attemptIds == null)
            {
                throw new ArgumentNullException(nameof(attemptIds));
            }

            await gate.WaitAsync().ConfigureAwait(false);
            try
            {
                var ids = new HashSet<Guid>(attemptIds);
                var document = await Load().ConfigureAwait(false);
                var changed = false;
                foreach (var row in document.Attempts.Where(row => ids.Contains(row.AttemptId)))
                {
                    changed |= rejected ? !row.Rejected : !row.Synced;
                    row.Rejected = rejected;
                    row.Synced = !rejected;
                }

                if (changed)
                {
                    await Write(document).ConfigureAwait(false);
                }
            }
            finally
            {
                gate.Release();
            }
        }

        private async Task<AttemptDocument> Load()
        {
            var backupPath = path + ".bak";
            var readablePath = File.Exists(path) ? path : backupPath;
            if (!File.Exists(readablePath))
            {
                return new AttemptDocument();
            }

            var json = await ReadText(readablePath).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<AttemptDocument>(json) ?? new AttemptDocument();
        }

        private async Task Write(AttemptDocument document)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var temporaryPath = path + ".tmp";
            var backupPath = path + ".bak";
            var json = JsonConvert.SerializeObject(document);
            await WriteText(temporaryPath, json).ConfigureAwait(false);

            if (File.Exists(path))
            {
                File.Replace(temporaryPath, path, backupPath);
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
                return;
            }

            File.Move(temporaryPath, path);
        }

        private static async Task<string> ReadText(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        private static async Task WriteText(string filePath, string contents)
        {
            using (var writer = new StreamWriter(filePath, false))
            {
                await writer.WriteAsync(contents).ConfigureAwait(false);
            }
        }

        public sealed class AttemptDocument
        {
            public List<AttemptRow> Attempts { get; set; } = new List<AttemptRow>();
        }

        public sealed class AttemptRow
        {
            public Guid AttemptId { get; set; }

            public string WorkerId { get; set; } = string.Empty;

            public string DeviceId { get; set; } = string.Empty;

            public string ModuleId { get; set; } = string.Empty;

            public int ModuleVersion { get; set; }

            public DateTimeOffset StartedAt { get; set; }

            public DateTimeOffset SavedAt { get; set; }

            public int Score { get; set; }

            public bool Passed { get; set; }

            public bool CriticalFailure { get; set; }

            public bool Synced { get; set; }

            public bool Rejected { get; set; }

            public List<EventRow> Events { get; set; } = new List<EventRow>();

            public static AttemptRow From(AttemptResult result, DateTimeOffset savedAt)
            {
                return new AttemptRow
                {
                    AttemptId = result.AttemptId,
                    WorkerId = result.WorkerId,
                    DeviceId = result.DeviceId,
                    ModuleId = result.ModuleId,
                    ModuleVersion = result.ModuleVersion,
                    StartedAt = result.StartedAt,
                    SavedAt = savedAt,
                    Score = result.Score,
                    Passed = result.Passed,
                    CriticalFailure = result.CriticalFailure,
                    Events = result.Events.Select(EventRow.From).ToList(),
                };
            }

            public AttemptResult ToResult()
            {
                return new AttemptResult(
                    AttemptId,
                    WorkerId,
                    DeviceId,
                    ModuleId,
                    ModuleVersion,
                    StartedAt,
                    Score,
                    Passed,
                    CriticalFailure,
                    Events.OrderBy(row => row.Sequence).Select(row => row.ToEvent()).ToArray());
            }
        }

        public sealed class EventRow
        {
            public int Sequence { get; set; }

            public string StepId { get; set; } = string.Empty;

            public string Kind { get; set; } = string.Empty;

            public string TargetId { get; set; } = string.Empty;

            public ActionOutcome Outcome { get; set; }

            public int ScoreDelta { get; set; }

            public bool Critical { get; set; }

            public static EventRow From(AttemptEvent attemptEvent)
            {
                return new EventRow
                {
                    Sequence = attemptEvent.Sequence,
                    StepId = attemptEvent.StepId,
                    Kind = attemptEvent.Action.Kind,
                    TargetId = attemptEvent.Action.TargetId,
                    Outcome = attemptEvent.Outcome,
                    ScoreDelta = attemptEvent.ScoreDelta,
                    Critical = attemptEvent.Critical,
                };
            }

            public AttemptEvent ToEvent()
            {
                return new AttemptEvent(
                    Sequence,
                    StepId,
                    new TrainingAction(Kind, TargetId),
                    Outcome,
                    ScoreDelta,
                    Critical);
            }
        }
    }
}
