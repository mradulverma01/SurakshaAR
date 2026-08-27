using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SurakshaAR.Domain.Persistence;

namespace SurakshaAR.Infrastructure.Sync
{
    public sealed class RemoteSyncResult
    {
        public RemoteSyncResult(
            IReadOnlyCollection<Guid> acceptedAttemptIds,
            IReadOnlyCollection<Guid> rejectedAttemptIds,
            IReadOnlyDictionary<Guid, string> certificateCodes)
        {
            AcceptedAttemptIds = acceptedAttemptIds ?? throw new ArgumentNullException(nameof(acceptedAttemptIds));
            RejectedAttemptIds = rejectedAttemptIds ?? throw new ArgumentNullException(nameof(rejectedAttemptIds));
            CertificateCodes = certificateCodes ?? throw new ArgumentNullException(nameof(certificateCodes));
        }

        public IReadOnlyCollection<Guid> AcceptedAttemptIds { get; }

        public IReadOnlyCollection<Guid> RejectedAttemptIds { get; }

        public IReadOnlyDictionary<Guid, string> CertificateCodes { get; }
    }

    public interface IAttemptRemote
    {
        Task<RemoteSyncResult> Submit(
            IReadOnlyList<PendingAttempt> attempts,
            CancellationToken cancellationToken);
    }

    public sealed class SyncReport
    {
        public SyncReport(
            int submitted,
            int synchronized,
            IReadOnlyCollection<Guid> rejectedAttemptIds,
            IReadOnlyDictionary<Guid, string> certificateCodes)
        {
            Submitted = submitted;
            Synchronized = synchronized;
            RejectedAttemptIds = rejectedAttemptIds;
            CertificateCodes = certificateCodes;
        }

        public int Submitted { get; }

        public int Synchronized { get; }

        public IReadOnlyCollection<Guid> RejectedAttemptIds { get; }

        public IReadOnlyDictionary<Guid, string> CertificateCodes { get; }
    }

    public interface IAttemptSync
    {
        Task<SyncReport> SyncPending(CancellationToken cancellationToken);
    }

    public sealed class AttemptSync : IAttemptSync
    {
        private readonly IAttemptStore store;
        private readonly IAttemptRemote remote;
        private readonly int batchSize;

        public AttemptSync(IAttemptStore store, IAttemptRemote remote, int batchSize)
        {
            if (batchSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(batchSize));
            }

            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.remote = remote ?? throw new ArgumentNullException(nameof(remote));
            this.batchSize = batchSize;
        }

        public async Task<SyncReport> SyncPending(CancellationToken cancellationToken)
        {
            var attempts = await store.Pending(batchSize).ConfigureAwait(false);
            if (attempts.Count == 0)
            {
                return new SyncReport(
                    0,
                    0,
                    Array.Empty<Guid>(),
                    new Dictionary<Guid, string>());
            }

            var remoteResult = await remote.Submit(attempts, cancellationToken).ConfigureAwait(false);
            var submittedIds = new HashSet<Guid>(attempts.Select(attempt => attempt.Result.AttemptId));
            var accepted = remoteResult.AcceptedAttemptIds.Where(submittedIds.Contains).Distinct().ToArray();
            if (accepted.Length > 0)
            {
                await store.MarkSynced(accepted).ConfigureAwait(false);
            }

            var rejected = remoteResult.RejectedAttemptIds.Where(submittedIds.Contains).Distinct().ToArray();
            if (rejected.Length > 0)
            {
                await store.MarkRejected(rejected).ConfigureAwait(false);
            }

            return new SyncReport(
                attempts.Count,
                accepted.Length,
                rejected,
                remoteResult.CertificateCodes);
        }
    }
}
