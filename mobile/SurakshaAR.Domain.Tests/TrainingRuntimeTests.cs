using NUnit.Framework;
using SurakshaAR.Domain.Training;

namespace SurakshaAR.Domain.Tests;

public sealed class TrainingRuntimeTests
{
    [Test]
    public void Correct_fire_sequence_passes()
    {
        var runtime = StartedRuntime(FireScenario(), out _);

        ApplyCorrectFireSequence(runtime);
        var result = runtime.Finish();

        Assert.Multiple(() =>
        {
            Assert.That(result.Score, Is.EqualTo(100));
            Assert.That(result.Passed, Is.True);
            Assert.That(result.CriticalFailure, Is.False);
            Assert.That(result.Events, Has.Count.EqualTo(6));
        });
    }

    [Test]
    public void Critical_wrong_extinguisher_prevents_passing()
    {
        var runtime = StartedRuntime(FireScenario(), out _);
        runtime.Apply(new TrainingAction("select", "electrical_fire"));

        var update = runtime.Apply(new TrainingAction("select", "water_extinguisher"));
        runtime.Apply(new TrainingAction("select", "co2_extinguisher"));
        runtime.Apply(new TrainingAction("interact", "extinguisher_pin"));
        runtime.Apply(new TrainingAction("aim", "fire_base"));
        runtime.Apply(new TrainingAction("hold", "extinguisher_handle"));
        runtime.Apply(new TrainingAction("waypoint_sequence", "safe_exit_a"));
        var result = runtime.Finish();

        Assert.Multiple(() =>
        {
            Assert.That(update.CriticalFailure, Is.True);
            Assert.That(result.Score, Is.EqualTo(75));
            Assert.That(result.Passed, Is.False);
            Assert.That(result.CriticalFailure, Is.True);
        });
    }

    [Test]
    public void Out_of_order_action_records_rejection_without_advancing()
    {
        var runtime = StartedRuntime(FireScenario(), out _);

        var rejected = runtime.Apply(new TrainingAction("aim", "fire_base"));
        var accepted = runtime.Apply(new TrainingAction("select", "electrical_fire"));

        Assert.Multiple(() =>
        {
            Assert.That(rejected.State.StepId, Is.EqualTo("identify_hazard"));
            Assert.That(rejected.Score, Is.Zero);
            Assert.That(rejected.NewEvents.Single().Outcome, Is.EqualTo(ActionOutcome.Rejected));
            Assert.That(accepted.State.StepId, Is.EqualTo("select_extinguisher"));
            Assert.That(accepted.Score, Is.EqualTo(15));
        });
    }

    [Test]
    public void Same_context_and_actions_replay_to_same_result()
    {
        var attemptId = Guid.Parse("7be20000-0000-0000-0000-000000000001");
        var context = new AttemptContext(attemptId, "worker-1", "device-1", DateTimeOffset.Parse("2026-08-23T11:20:00Z"));
        var first = new TrainingRuntime();
        var second = new TrainingRuntime();
        first.Begin(FireScenario(), context);
        second.Begin(FireScenario(), context);

        ApplyCorrectFireSequence(first);
        ApplyCorrectFireSequence(second);

        Assert.That(second.Finish(), Is.EqualTo(first.Finish()));
    }

    private static TrainingRuntime StartedRuntime(ScenarioBundle scenario, out AttemptContext context)
    {
        context = new AttemptContext(Guid.NewGuid(), "worker-1", "device-1", DateTimeOffset.Parse("2026-08-23T11:20:00Z"));
        var runtime = new TrainingRuntime();
        runtime.Begin(scenario, context);
        return runtime;
    }

    private static void ApplyCorrectFireSequence(ITrainingRuntime runtime)
    {
        runtime.Apply(new TrainingAction("select", "electrical_fire"));
        runtime.Apply(new TrainingAction("select", "co2_extinguisher"));
        runtime.Apply(new TrainingAction("interact", "extinguisher_pin"));
        runtime.Apply(new TrainingAction("aim", "fire_base"));
        runtime.Apply(new TrainingAction("hold", "extinguisher_handle"));
        runtime.Apply(new TrainingAction("waypoint_sequence", "safe_exit_a"));
    }

    private static ScenarioBundle FireScenario()
    {
        return new ScenarioBundle(
            "fire_001",
            1,
            70,
            new[]
            {
                Step("identify_hazard", 15, Accepted("select", "electrical_fire")),
                new ScenarioStep(
                    "select_extinguisher",
                    20,
                    new[] { Accepted("select", "co2_extinguisher") },
                    new[] { new WrongAction("select", "water_extinguisher", 25, true) }),
                Step("remove_pin", 10, Accepted("interact", "extinguisher_pin")),
                Step("aim", 15, Accepted("aim", "fire_base")),
                Step("discharge", 20, Accepted("hold", "extinguisher_handle")),
                Step("evacuate", 20, Accepted("waypoint_sequence", "safe_exit_a")),
            });
    }

    private static ScenarioStep Step(string id, int score, AcceptedAction accepted)
    {
        return new ScenarioStep(id, score, new[] { accepted }, Array.Empty<WrongAction>());
    }

    private static AcceptedAction Accepted(string kind, string targetId)
    {
        return new AcceptedAction(kind, targetId);
    }
}
