using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SurakshaAR.Domain.Training;

namespace SurakshaAR.Domain.Persistence
{
    public sealed class PendingAttempt
    {
        public PendingAttempt(AttemptResult result, DateTimeOffset savedAt)
        {
            Result = result;
            SavedAt = savedAt;
        }

        public AttemptResult Result { get; }

        public DateTimeOffset SavedAt { get; }
    }

    public interface IAttemptStore
    {
        Task Save(AttemptResult result);

        Task<IReadOnlyList<PendingAttempt>> Pending(int limit);

        Task MarkSynced(IReadOnlyCollection<Guid> attemptIds);

        Task MarkRejected(IReadOnlyCollection<Guid> attemptIds);
    }
}
