using NUnit.Framework;
using SurakshaAR.Domain.Persistence;
using SurakshaAR.Domain.Training;
using SurakshaAR.Infrastructure.Persistence;

namespace SurakshaAR.Infrastructure.Tests;

public sealed class JsonAttemptStoreTests
{
    private string directory = null!;

    [SetUp]
    public void SetUp()
    {
        directory = Path.Combine(Path.GetTempPath(), "suraksha-ar-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(directory, true);
    }

    [Test]
    public async Task Attempt_remains_pending_after_restart_until_acknowledged()
    {
        var path = Path.Combine(directory, "attempts.json");
        var result = PassingResult();
        await new JsonAttemptStore(path).Save(result);

        var restarted = new JsonAttemptStore(path);
        var pending = await restarted.Pending(10);
        await restarted.MarkSynced(new[] { result.AttemptId });
        var afterAcknowledgement = await restarted.Pending(10);

        Assert.Multiple(() =>
        {
            Assert.That(pending, Has.Count.EqualTo(1));
            Assert.That(pending.Single().Result, Is.EqualTo(result));
            Assert.That(afterAcknowledgement, Is.Empty);
        });
    }

    [Test]
    public async Task Saving_same_attempt_twice_is_idempotent()
    {
        var store = new JsonAttemptStore(Path.Combine(directory, "attempts.json"));
        var result = PassingResult();

        await store.Save(result);
        await store.Save(result);

        Assert.That(await store.Pending(10), Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Rejected_attempt_is_not_retried()
    {
        var store = new JsonAttemptStore(Path.Combine(directory, "attempts.json"));
        var result = PassingResult();
        await store.Save(result);

        await store.MarkRejected(new[] { result.AttemptId });

        Assert.That(await store.Pending(10), Is.Empty);
    }

    private static AttemptResult PassingResult()
    {
        return new AttemptResult(
            Guid.Parse("7be20000-0000-0000-0000-000000000002"),
            "worker-1",
            "device-1",
            "fire_001",
            1,
            DateTimeOffset.Parse("2026-08-23T11:20:00Z"),
            100,
            true,
            false,
            new[]
            {
                new AttemptEvent(
                    1,
                    "identify_hazard",
                    new TrainingAction("select", "electrical_fire"),
                    ActionOutcome.Accepted,
                    15,
                    false),
            });
    }
}
