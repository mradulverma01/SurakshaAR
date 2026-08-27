using NUnit.Framework;
using SurakshaAR.Domain.Training;

namespace SurakshaAR.Domain.Tests;

public sealed class MobileTrainingSessionTests
{
    [Test]
    public void Selecting_fire_module_starts_its_first_training_step()
    {
        var session = new MobileTrainingSession(new[] { FireScenario(), GasScenario() });

        var update = session.SelectModule("fire_001", Context());

        Assert.Multiple(() =>
        {
            Assert.That(update.ModuleId, Is.EqualTo("fire_001"));
            Assert.That(update.Training.State.StepId, Is.EqualTo("identify_hazard"));
            Assert.That(update.ReturnedToLauncher, Is.False);
        });
    }

    [Test]
    public void Launcher_lists_active_fire_and_gas_bundles()
    {
        var session = new MobileTrainingSession(new[] { FireScenario(), GasScenario() });

        Assert.That(session.ActiveModules.Select(scenario => scenario.Id), Is.EqualTo(new[] { "fire_001", "gas_001" }));
    }

    [Test]
    public void Selecting_gas_module_starts_its_first_training_step()
    {
        var session = new MobileTrainingSession(new[] { FireScenario(), GasScenario() });

        var update = session.SelectModule("gas_001", Context());

        Assert.Multiple(() =>
        {
            Assert.That(update.ModuleId, Is.EqualTo("gas_001"));
            Assert.That(update.Training.State.StepId, Is.EqualTo("recognize_hazard_zone"));
            Assert.That(update.ReturnedToLauncher, Is.False);
        });
    }

    [Test]
    public void Interrupted_or_short_hold_does_not_advance_the_fire_session()
    {
        var session = new MobileTrainingSession(new[] { FireScenario() });
        session.SelectModule("fire_001", Context());
        session.Apply(new SemanticInteraction("identify", SemanticInteractionKind.TargetSelected, "hazard"));

        var shortHold = session.Apply(new SemanticInteraction("discharge", SemanticInteractionKind.CompletedHold, "extinguisher_handle", 2));
        var interruptedHold = session.Apply(new SemanticInteraction("discharge", SemanticInteractionKind.InterruptedHold, "extinguisher_handle"));

        Assert.Multiple(() =>
        {
            Assert.That(shortHold.Training.State.StepId, Is.EqualTo("discharge"));
            Assert.That(shortHold.Training.NewEvents.Single().Outcome, Is.EqualTo(ActionOutcome.Rejected));
            Assert.That(interruptedHold.Training.State.StepId, Is.EqualTo("discharge"));
            Assert.That(interruptedHold.Training.NewEvents.Single().Outcome, Is.EqualTo(ActionOutcome.Rejected));
        });
    }

    [Test]
    public void Ordered_waypoints_complete_fire_and_return_to_launcher()
    {
        var session = new MobileTrainingSession(new[] { FireScenario() });
        session.SelectModule("fire_001", Context());
        session.Apply(new SemanticInteraction("identify", SemanticInteractionKind.TargetSelected, "hazard"));
        session.Apply(new SemanticInteraction("discharge", SemanticInteractionKind.CompletedHold, "extinguisher_handle", 3));

        var firstWaypoint = session.Apply(new SemanticInteraction("exit_route", SemanticInteractionKind.WaypointArrived, "exit_a"));
        var completed = session.Apply(new SemanticInteraction("exit_route", SemanticInteractionKind.WaypointArrived, "exit_b"));

        Assert.Multiple(() =>
        {
            Assert.That(firstWaypoint.Training.State.StepId, Is.EqualTo("evacuate"));
            Assert.That(firstWaypoint.Training.NewEvents, Is.Empty);
            Assert.That(completed.ReturnedToLauncher, Is.True);
            Assert.That(completed.ModuleId, Is.Null);
            Assert.That(session.CompletedAttempt!.Passed, Is.True);
        });
    }

    [Test]
    public void Out_of_order_waypoint_records_rejection_without_advancing()
    {
        var session = new MobileTrainingSession(new[] { FireScenario() });
        session.SelectModule("fire_001", Context());
        session.Apply(new SemanticInteraction("identify", SemanticInteractionKind.TargetSelected, "hazard"));
        session.Apply(new SemanticInteraction("discharge", SemanticInteractionKind.CompletedHold, "extinguisher_handle", 3));

        var update = session.Apply(new SemanticInteraction("exit_route", SemanticInteractionKind.WaypointArrived, "exit_b"));

        Assert.Multiple(() =>
        {
            Assert.That(update.ReturnedToLauncher, Is.False);
            Assert.That(update.Training.State.StepId, Is.EqualTo("evacuate"));
            Assert.That(update.Training.NewEvents.Single().Outcome, Is.EqualTo(ActionOutcome.Rejected));
        });
    }

    [Test]
    public void Gas_session_accepts_zone_entry_and_exit_interactions()
    {
        var session = new MobileTrainingSession(new[] { GasScenario() });
        session.SelectModule("gas_001", Context());

        var entered = session.Apply(new SemanticInteraction("enter_hazard", SemanticInteractionKind.ZoneEntered, "methane_hazard_zone"));
        var exited = session.Apply(new SemanticInteraction("leave_hazard", SemanticInteractionKind.ZoneExited, "safe_zone"));

        Assert.Multiple(() =>
        {
            Assert.That(entered.Training.State.StepId, Is.EqualTo("withdraw"));
            Assert.That(exited.ReturnedToLauncher, Is.True);
            Assert.That(session.CompletedAttempt!.Passed, Is.True);
        });
    }

    [Test]
    public void Critical_semantic_interaction_marks_the_attempt_as_failed()
    {
        var session = new MobileTrainingSession(new[] { FireScenario() });
        session.SelectModule("fire_001", Context());

        var update = session.Apply(new SemanticInteraction("wrong_extinguisher", SemanticInteractionKind.TargetSelected, "water_extinguisher"));

        Assert.Multiple(() =>
        {
            Assert.That(update.Training.CriticalFailure, Is.True);
            Assert.That(update.Training.NewEvents.Single().Outcome, Is.EqualTo(ActionOutcome.Penalized));
            Assert.That(session.CompletedAttempt, Is.Null);
        });
    }

    [Test]
    public void Cannot_replace_an_active_attempt_with_another_module()
    {
        var session = new MobileTrainingSession(new[] { FireScenario(), GasScenario() });
        session.SelectModule("fire_001", Context());

        Assert.That(
            () => session.SelectModule("gas_001", Context()),
            Throws.InvalidOperationException.With.Message.EqualTo("Finish the active training attempt before selecting another module."));
    }

    [Test]
    public void Leaving_a_session_returns_to_the_launcher_with_a_failed_attempt()
    {
        var session = new MobileTrainingSession(new[] { FireScenario() });
        session.SelectModule("fire_001", Context());

        var update = session.Leave();

        Assert.Multiple(() =>
        {
            Assert.That(update.ReturnedToLauncher, Is.True);
            Assert.That(update.ModuleId, Is.Null);
            Assert.That(update.Training.State.StepId, Is.EqualTo("abandoned"));
            Assert.That(update.Training.CueKey, Is.Null);
            Assert.That(update.Training.NewEvents, Is.Empty);
            Assert.That(session.CompletedAttempt!.Passed, Is.False);
        });
    }

    [Test]
    public void Fire_training_requires_correct_extinguisher_aim_hold_and_ordered_evacuation()
    {
        var session = new MobileTrainingSession(new[] { FullFireScenario() });
        session.SelectModule("fire_001", Context());

        var identified = session.Apply(new SemanticInteraction("identify_hazard", SemanticInteractionKind.TargetSelected, "electrical_fire"));
        Assert.That(identified.Training.State.StepId, Is.EqualTo("select_extinguisher"));

        var earlyAim = session.Apply(new SemanticInteraction("aim", SemanticInteractionKind.TargetSelected, "fire_base"));
        Assert.Multiple(() =>
        {
            Assert.That(earlyAim.Training.State.StepId, Is.EqualTo("select_extinguisher"));
            Assert.That(earlyAim.Training.NewEvents.Single().Outcome, Is.EqualTo(ActionOutcome.Rejected));
        });

        var selected = session.Apply(new SemanticInteraction("select_extinguisher", SemanticInteractionKind.TargetSelected, "co2_extinguisher"));
        Assert.That(selected.Training.State.StepId, Is.EqualTo("remove_pin"));

        session.Apply(new SemanticInteraction("remove_pin", SemanticInteractionKind.TargetSelected, "extinguisher_pin"));

        var earlyDischarge = session.Apply(new SemanticInteraction("discharge", SemanticInteractionKind.CompletedHold, "extinguisher_handle", 3));
        Assert.Multiple(() =>
        {
            Assert.That(earlyDischarge.Training.State.StepId, Is.EqualTo("aim"));
            Assert.That(earlyDischarge.Training.NewEvents.Single().Outcome, Is.EqualTo(ActionOutcome.Rejected));
        });

        session.Apply(new SemanticInteraction("aim", SemanticInteractionKind.TargetSelected, "fire_base"));

        var shortHold = session.Apply(new SemanticInteraction("discharge", SemanticInteractionKind.CompletedHold, "extinguisher_handle", 2));
        Assert.Multiple(() =>
        {
            Assert.That(shortHold.Training.State.StepId, Is.EqualTo("discharge"));
            Assert.That(shortHold.Training.NewEvents.Single().Outcome, Is.EqualTo(ActionOutcome.Rejected));
        });

        var discharged = session.Apply(new SemanticInteraction("discharge", SemanticInteractionKind.CompletedHold, "extinguisher_handle", 3));
        Assert.That(discharged.Training.State.StepId, Is.EqualTo("evacuate"));

        var tapExit = session.Apply(new SemanticInteraction("exit_route", SemanticInteractionKind.TargetSelected, "safe_exit_a"));
        Assert.That(tapExit.Training.NewEvents.Single().Outcome, Is.EqualTo(ActionOutcome.Rejected));

        var completed = session.Apply(new SemanticInteraction("exit_route", SemanticInteractionKind.WaypointArrived, "safe_exit_a"));

        Assert.Multiple(() =>
        {
            Assert.That(completed.ReturnedToLauncher, Is.True);
            Assert.That(session.CompletedAttempt!.Passed, Is.True);
        });
    }

    [Test]
    public void Gas_training_requires_reporting_ppe_buddy_and_ordered_exit_with_zone_handling()
    {
        var session = new MobileTrainingSession(new[] { FullGasScenario() });
        session.SelectModule("gas_001", Context());

        var earlyHazardEntry = session.Apply(new SemanticInteraction("enter_hazard_zone", SemanticInteractionKind.ZoneEntered, "methane_hazard_zone"));
        Assert.Multiple(() =>
        {
            Assert.That(earlyHazardEntry.Training.CriticalFailure, Is.True);
            Assert.That(earlyHazardEntry.Training.NewEvents.Single().Outcome, Is.EqualTo(ActionOutcome.Penalized));
        });

        session = new MobileTrainingSession(new[] { FullGasScenario() });
        session.SelectModule("gas_001", Context());

        session.Apply(new SemanticInteraction("recognize_hazard_zone", SemanticInteractionKind.TargetSelected, "methane_hazard_zone"));
        var withdrawn = session.Apply(new SemanticInteraction("withdraw", SemanticInteractionKind.ZoneExited, "safe_zone"));
        Assert.That(withdrawn.Training.State.StepId, Is.EqualTo("report_hazard"));

        var earlyPpe = session.Apply(new SemanticInteraction("select_ppe", SemanticInteractionKind.TargetSelected, "approved_self_rescuer"));
        Assert.Multiple(() =>
        {
            Assert.That(earlyPpe.Training.State.StepId, Is.EqualTo("report_hazard"));
            Assert.That(earlyPpe.Training.NewEvents.Single().Outcome, Is.EqualTo(ActionOutcome.Rejected));
        });

        session.Apply(new SemanticInteraction("report_hazard", SemanticInteractionKind.TargetSelected, "supervisor_radio"));
        session.Apply(new SemanticInteraction("select_ppe", SemanticInteractionKind.TargetSelected, "approved_self_rescuer"));
        session.Apply(new SemanticInteraction("buddy_check", SemanticInteractionKind.TargetSelected, "buddy_present"));

        var tapExit = session.Apply(new SemanticInteraction("exit_route", SemanticInteractionKind.TargetSelected, "safe_exit_a"));
        Assert.That(tapExit.Training.NewEvents.Single().Outcome, Is.EqualTo(ActionOutcome.Rejected));

        var done = session.Apply(new SemanticInteraction("exit_route", SemanticInteractionKind.WaypointArrived, "safe_exit_a"));
        Assert.Multiple(() =>
        {
            Assert.That(done.ReturnedToLauncher, Is.True);
            Assert.That(session.CompletedAttempt!.Passed, Is.True);
        });
    }

    private static AttemptContext Context()
    {
        return new AttemptContext(Guid.NewGuid(), "worker-1", "device-1", DateTimeOffset.Parse("2026-08-24T12:00:00Z"));
    }

    private static ScenarioBundle FireScenario()
    {
        return new ScenarioBundle(
            "fire_001",
            1,
            0,
            new[]
            {
                new ScenarioStep(
                    "identify_hazard",
                    0,
                    new[] { new AcceptedAction("select", "hazard") },
                    new[] { new WrongAction("select", "water_extinguisher", 0, true) }),
                new ScenarioStep("discharge", 0, new[] { new AcceptedAction("hold", "extinguisher_handle") }, Array.Empty<WrongAction>()),
                new ScenarioStep("evacuate", 0, new[] { new AcceptedAction("waypoint_sequence", "safe_exit") }, Array.Empty<WrongAction>()),
            },
            new ScenarioSceneReference("FireScene", "fire_prefab"),
            new[]
            {
                new ScenarioInteractionDefinition("identify", SemanticInteractionKind.TargetSelected, "select", "hazard"),
                new ScenarioInteractionDefinition("wrong_extinguisher", SemanticInteractionKind.TargetSelected, "select", "water_extinguisher"),
                new ScenarioInteractionDefinition("discharge", SemanticInteractionKind.CompletedHold, "hold", "extinguisher_handle", 3),
                new ScenarioInteractionDefinition("exit_route", SemanticInteractionKind.WaypointArrived, "waypoint_sequence", "safe_exit", orderedWaypoints: new[] { "exit_a", "exit_b" }),
            });
    }

    private static ScenarioBundle FullFireScenario()
    {
        return new ScenarioBundle(
            "fire_001",
            1,
            0,
            new[]
            {
                new ScenarioStep("identify_hazard", 0, new[] { new AcceptedAction("select", "electrical_fire") }, Array.Empty<WrongAction>()),
                new ScenarioStep("select_extinguisher", 0, new[] { new AcceptedAction("select", "co2_extinguisher") }, new[] { new WrongAction("select", "water_extinguisher", 0, true) }),
                new ScenarioStep("remove_pin", 0, new[] { new AcceptedAction("interact", "extinguisher_pin") }, Array.Empty<WrongAction>()),
                new ScenarioStep("aim", 0, new[] { new AcceptedAction("aim", "fire_base") }, Array.Empty<WrongAction>()),
                new ScenarioStep("discharge", 0, new[] { new AcceptedAction("hold", "extinguisher_handle") }, Array.Empty<WrongAction>()),
                new ScenarioStep("evacuate", 0, new[] { new AcceptedAction("waypoint_sequence", "safe_exit_a") }, Array.Empty<WrongAction>()),
            },
            new ScenarioSceneReference("FireScene", "fire_prefab"),
            new[]
            {
                new ScenarioInteractionDefinition("identify_hazard", SemanticInteractionKind.TargetSelected, "select", "electrical_fire"),
                new ScenarioInteractionDefinition("select_extinguisher", SemanticInteractionKind.TargetSelected, "select", "co2_extinguisher"),
                new ScenarioInteractionDefinition("wrong_extinguisher", SemanticInteractionKind.TargetSelected, "select", "water_extinguisher"),
                new ScenarioInteractionDefinition("remove_pin", SemanticInteractionKind.TargetSelected, "interact", "extinguisher_pin"),
                new ScenarioInteractionDefinition("aim", SemanticInteractionKind.TargetSelected, "aim", "fire_base"),
                new ScenarioInteractionDefinition("discharge", SemanticInteractionKind.CompletedHold, "hold", "extinguisher_handle", 3),
                new ScenarioInteractionDefinition("exit_route", SemanticInteractionKind.WaypointArrived, "waypoint_sequence", "safe_exit_a", orderedWaypoints: new[] { "safe_exit_a" }),
            });
    }

    private static ScenarioBundle FullGasScenario()
    {
        return new ScenarioBundle(
            "gas_001",
            1,
            0,
            new[]
            {
                new ScenarioStep("recognize_hazard_zone", 0, new[] { new AcceptedAction("select", "methane_hazard_zone") }, new[] { new WrongAction("waypoint_enter", "methane_hazard_zone", 0, true) }),
                new ScenarioStep("withdraw", 0, new[] { new AcceptedAction("waypoint_enter", "safe_zone") }, Array.Empty<WrongAction>()),
                new ScenarioStep("report_hazard", 0, new[] { new AcceptedAction("interact", "supervisor_radio") }, Array.Empty<WrongAction>()),
                new ScenarioStep("select_ppe", 0, new[] { new AcceptedAction("select", "approved_self_rescuer") }, Array.Empty<WrongAction>()),
                new ScenarioStep("buddy_check", 0, new[] { new AcceptedAction("confirm", "buddy_present") }, Array.Empty<WrongAction>()),
                new ScenarioStep("exit", 0, new[] { new AcceptedAction("waypoint_sequence", "safe_exit_a") }, Array.Empty<WrongAction>()),
            },
            new ScenarioSceneReference("GasScene", "gas_prefab"),
            new[]
            {
                new ScenarioInteractionDefinition("recognize_hazard_zone", SemanticInteractionKind.TargetSelected, "select", "methane_hazard_zone"),
                new ScenarioInteractionDefinition("enter_hazard_zone", SemanticInteractionKind.ZoneEntered, "waypoint_enter", "methane_hazard_zone"),
                new ScenarioInteractionDefinition("withdraw", SemanticInteractionKind.ZoneExited, "waypoint_enter", "safe_zone"),
                new ScenarioInteractionDefinition("report_hazard", SemanticInteractionKind.TargetSelected, "interact", "supervisor_radio"),
                new ScenarioInteractionDefinition("select_ppe", SemanticInteractionKind.TargetSelected, "select", "approved_self_rescuer"),
                new ScenarioInteractionDefinition("buddy_check", SemanticInteractionKind.TargetSelected, "confirm", "buddy_present"),
                new ScenarioInteractionDefinition("exit_route", SemanticInteractionKind.WaypointArrived, "waypoint_sequence", "safe_exit_a", orderedWaypoints: new[] { "safe_exit_a" }),
            });
    }

    private static ScenarioBundle GasScenario()
    {
        return new ScenarioBundle(
            "gas_001",
            1,
            0,
            new[]
            {
                new ScenarioStep("recognize_hazard_zone", 0, new[] { new AcceptedAction("zone_enter", "methane_hazard_zone") }, Array.Empty<WrongAction>()),
                new ScenarioStep("withdraw", 0, new[] { new AcceptedAction("zone_exit", "safe_zone") }, Array.Empty<WrongAction>()),
            },
            new ScenarioSceneReference("GasScene", "gas_prefab"),
            new[]
            {
                new ScenarioInteractionDefinition("enter_hazard", SemanticInteractionKind.ZoneEntered, "zone_enter", "methane_hazard_zone"),
                new ScenarioInteractionDefinition("leave_hazard", SemanticInteractionKind.ZoneExited, "zone_exit", "safe_zone"),
            });
    }
}
