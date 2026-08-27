using NUnit.Framework;
using SurakshaAR.Domain.Persistence;
using SurakshaAR.Domain.Training;
using SurakshaAR.Infrastructure.Sync;

namespace SurakshaAR.Infrastructure.Tests;

public sealed class AttemptSyncTests
{
    [Test]
    public async Task Marks_only_server_accepted_attempts_as_synced()
    {
        var accepted = Result("00000000-0000-0000-0000-000000000001");
        var rejected = Result("00000000-0000-0000-0000-000000000002");
        var store = new MemoryStore(accepted, rejected);
        var certificateCode = "CERT-A1B2C3D4E5F60708";
        var remote = new StubRemote(new[] { accepted.AttemptId }, rejected.AttemptId, certificateCode);
        var sync = new AttemptSync(store, remote, 20);

        var report = await sync.SyncPending(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(report.Submitted, Is.EqualTo(2));
            Assert.That(report.Synchronized, Is.EqualTo(1));
            Assert.That(report.RejectedAttemptIds, Is.EqualTo(new[] { rejected.AttemptId }));
            Assert.That(report.CertificateCodes[accepted.AttemptId], Is.EqualTo(certificateCode));
            Assert.That(store.Synced, Is.EqualTo(new[] { accepted.AttemptId }));
            Assert.That(store.Rejected, Is.EqualTo(new[] { rejected.AttemptId }));
        });
    }

    private static AttemptResult Result(string id)
    {
        return new AttemptResult(
            Guid.Parse(id),
            "worker-1",
            "device-1",
            "fire_001",
            1,
            DateTimeOffset.Parse("2026-08-23T11:20:00Z"),
            100,
            true,
            false,
            Array.Empty<AttemptEvent>());
    }

    private sealed class MemoryStore : IAttemptStore
    {
        private readonly IReadOnlyList<PendingAttempt> pending;

        public MemoryStore(params AttemptResult[] results)
        {
            pending = results.Select(result => new PendingAttempt(result, result.StartedAt.AddMinutes(5))).ToArray();
        }

        public IReadOnlyCollection<Guid> Synced { get; private set; } = Array.Empty<Guid>();

        public IReadOnlyCollection<Guid> Rejected { get; private set; } = Array.Empty<Guid>();

        public Task Save(AttemptResult result) => Task.CompletedTask;

        public Task<IReadOnlyList<PendingAttempt>> Pending(int limit) => Task.FromResult(pending);

        public Task MarkSynced(IReadOnlyCollection<Guid> attemptIds)
        {
            Synced = attemptIds;
            return Task.CompletedTask;
        }

        public Task MarkRejected(IReadOnlyCollection<Guid> attemptIds)
        {
            Rejected = attemptIds;
            return Task.CompletedTask;
        }
    }

    private sealed class StubRemote : IAttemptRemote
    {
        private readonly IReadOnlyCollection<Guid> accepted;
        private readonly Guid rejected;
        private readonly string certificateCode;

        public StubRemote(IReadOnlyCollection<Guid> accepted, Guid rejected, string certificateCode)
        {
            this.accepted = accepted;
            this.rejected = rejected;
            this.certificateCode = certificateCode;
        }

        public Task<RemoteSyncResult> Submit(
            IReadOnlyList<PendingAttempt> attempts,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new RemoteSyncResult(
                accepted,
                new[] { rejected },
                new Dictionary<Guid, string> { [accepted.Single()] = certificateCode }));
        }
    }
}
