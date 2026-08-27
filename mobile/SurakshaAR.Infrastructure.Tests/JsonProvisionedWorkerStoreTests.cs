using NUnit.Framework;
using SurakshaAR.Infrastructure.Persistence;

namespace SurakshaAR.Infrastructure.Tests;

public sealed class JsonProvisionedWorkerStoreTests
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
    public void Provisioned_worker_survives_a_store_restart()
    {
        var path = Path.Combine(directory, "worker.json");
        new JsonProvisionedWorkerStore(path).Save("worker-1");

        var restarted = new JsonProvisionedWorkerStore(path);

        Assert.That(restarted.Load(), Is.EqualTo("worker-1"));
    }

    [Test]
    public void Clearing_a_store_removes_the_provisioned_worker()
    {
        var store = new JsonProvisionedWorkerStore(Path.Combine(directory, "worker.json"));
        store.Save("worker-1");

        store.Clear();

        Assert.That(store.Load(), Is.Null);
    }

    [Test]
    public void Store_writes_only_the_worker_identity()
    {
        var path = Path.Combine(directory, "worker.json");
        new JsonProvisionedWorkerStore(path).Save("worker-1");

        var json = File.ReadAllText(path);

        Assert.Multiple(() =>
        {
            Assert.That(json, Does.Contain("WorkerId"));
            Assert.That(json, Does.Not.Contain("Token"));
        });
    }
}
