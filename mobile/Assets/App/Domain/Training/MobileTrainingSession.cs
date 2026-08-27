using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace SurakshaAR.Domain.Training
{
    public sealed class MobileTrainingUpdate
    {
        public MobileTrainingUpdate(string? moduleId, TrainingUpdate training, bool returnedToLauncher)
        {
            ModuleId = moduleId;
            Training = training ?? throw new ArgumentNullException(nameof(training));
            ReturnedToLauncher = returnedToLauncher;
        }

        public string? ModuleId { get; }

        public TrainingUpdate Training { get; }

        public bool ReturnedToLauncher { get; }
    }

    public sealed class MobileTrainingSession
    {
        private readonly IReadOnlyDictionary<string, ScenarioBundle> scenarios;
        private readonly IReadOnlyList<ScenarioBundle> activeModules;
        private ITrainingRuntime? runtime;
        private ScenarioBundle? scenario;
        private TrainingUpdate? latestUpdate;
        private string? moduleId;
        private AttemptResult? completedAttempt;

        public MobileTrainingSession(IReadOnlyList<ScenarioBundle> scenarios)
        {
            if (scenarios == null)
            {
                throw new ArgumentNullException(nameof(scenarios));
            }

            activeModules = scenarios.ToArray();
            this.scenarios = activeModules.ToDictionary(scenario => scenario.Id, StringComparer.Ordinal);
        }

        public IReadOnlyList<ScenarioBundle> ActiveModules => activeModules;

        public MobileTrainingUpdate SelectModule(string selectedModuleId, AttemptContext context)
        {
            if (runtime != null)
            {
                throw new InvalidOperationException("Finish the active training attempt before selecting another module.");
            }

            if (string.IsNullOrWhiteSpace(selectedModuleId))
            {
                throw new ArgumentException("A training module id is required.", nameof(selectedModuleId));
            }

            if (!scenarios.TryGetValue(selectedModuleId, out var scenario))
            {
                throw new ArgumentException("The selected training module is not available offline.", nameof(selectedModuleId));
            }

            runtime = new TrainingRuntime();
            this.scenario = scenario;
            moduleId = scenario.Id;
            completedAttempt = null;
            latestUpdate = runtime.Begin(scenario, context);
            return new MobileTrainingUpdate(moduleId, latestUpdate, false);
        }

        public MobileTrainingUpdate Apply(SemanticInteraction interaction)
        {
            EnsureActiveAttempt();
            if (interaction == null)
            {
                throw new ArgumentNullException(nameof(interaction));
            }

            var definition = scenario!.Interactions.SingleOrDefault(candidate => candidate.Id == interaction.InteractionId);
            if (definition == null)
            {
                return RecordRejectedInteraction(interaction);
            }

            if (interaction.Kind == SemanticInteractionKind.InterruptedHold
                && definition.Kind == SemanticInteractionKind.CompletedHold)
            {
                return ApplyAction(new TrainingAction("hold_interrupted", definition.TargetId));
            }

            if (definition.Kind != interaction.Kind)
            {
                return RecordRejectedInteraction(interaction);
            }

            if (definition.Kind != SemanticInteractionKind.WaypointArrived
                && !string.Equals(definition.TargetId, interaction.TargetId, StringComparison.Ordinal))
            {
                return RecordRejectedInteraction(interaction);
            }

            if (definition.Kind == SemanticInteractionKind.CompletedHold && interaction.Value < definition.Threshold)
            {
                return ApplyAction(new TrainingAction("hold_interrupted", definition.TargetId));
            }

            if (definition.Kind == SemanticInteractionKind.WaypointArrived)
            {
                return ApplyWaypoint(definition, interaction);
            }

            return ApplyAction(new TrainingAction(definition.ActionKind, definition.TargetId));
        }

        public AttemptResult? CompletedAttempt => completedAttempt;

        public MobileTrainingUpdate Leave()
        {
            EnsureActiveAttempt();
            completedAttempt = runtime!.Finish();
            latestUpdate = new TrainingUpdate(
                new TrainingState("abandoned", false),
                null,
                completedAttempt.Score,
                completedAttempt.CriticalFailure,
                Array.Empty<AttemptEvent>());
            runtime = null;
            scenario = null;
            moduleId = null;
            waypointIndices.Clear();
            return new MobileTrainingUpdate(null, latestUpdate, true);
        }

        private MobileTrainingUpdate ApplyWaypoint(ScenarioInteractionDefinition definition, SemanticInteraction interaction)
        {
            var index = waypointIndices.TryGetValue(definition.Id, out var currentIndex) ? currentIndex : 0;
            if (index >= definition.OrderedWaypoints.Count)
            {
                return RecordRejectedInteraction(interaction);
            }

            var expectedWaypoint = definition.OrderedWaypoints[index];
            if (!string.Equals(expectedWaypoint, interaction.TargetId, StringComparison.Ordinal))
            {
                return RecordRejectedInteraction(interaction);
            }

            index++;
            waypointIndices[definition.Id] = index;
            if (index < definition.OrderedWaypoints.Count)
            {
                return new MobileTrainingUpdate(
                    moduleId,
                    new TrainingUpdate(
                        latestUpdate!.State,
                        latestUpdate.CueKey,
                        latestUpdate.Score,
                        latestUpdate.CriticalFailure,
                        Array.Empty<AttemptEvent>()),
                    false);
            }

            return ApplyAction(new TrainingAction(definition.ActionKind, definition.TargetId));
        }

        private readonly Dictionary<string, int> waypointIndices = new Dictionary<string, int>(StringComparer.Ordinal);

        private MobileTrainingUpdate RecordRejectedInteraction(SemanticInteraction interaction)
        {
            return ApplyAction(new TrainingAction("invalid_interaction", interaction.TargetId));
        }

        private MobileTrainingUpdate ApplyAction(TrainingAction action)
        {
            latestUpdate = runtime!.Apply(action);
            if (!latestUpdate.State.Completed)
            {
                return new MobileTrainingUpdate(moduleId, latestUpdate, false);
            }

            completedAttempt = runtime.Finish();
            runtime = null;
            scenario = null;
            moduleId = null;
            waypointIndices.Clear();
            return new MobileTrainingUpdate(null, latestUpdate, true);
        }

        private void EnsureActiveAttempt()
        {
            if (runtime == null || scenario == null || latestUpdate == null)
            {
                throw new InvalidOperationException("Select a training module before sending interactions.");
            }
        }
    }
}
