using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace SurakshaAR.Domain.Training
{
    public interface ITrainingRuntime
    {
        TrainingUpdate Begin(ScenarioBundle scenario, AttemptContext context);

        TrainingUpdate Apply(TrainingAction action);

        AttemptResult Finish();
    }

    public sealed class TrainingRuntime : ITrainingRuntime
    {
        private readonly List<AttemptEvent> events = new List<AttemptEvent>();
        private ScenarioBundle? scenario;
        private AttemptContext? context;
        private int stepIndex;
        private int score;
        private bool criticalFailure;

        public TrainingUpdate Begin(ScenarioBundle scenario, AttemptContext context)
        {
            if (this.scenario != null)
            {
                throw new InvalidOperationException("This runtime already owns an attempt.");
            }

            this.scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            return Update(Array.Empty<AttemptEvent>());
        }

        public TrainingUpdate Apply(TrainingAction action)
        {
            EnsureStarted();
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (stepIndex >= scenario!.Steps.Count)
            {
                return Record(action, ActionOutcome.Rejected, 0, false);
            }

            var step = scenario.Steps[stepIndex];
            if (step.AcceptedActions.Any(accepted => accepted.Matches(action)))
            {
                score += step.Score;
                var update = Record(action, ActionOutcome.Accepted, step.Score, false);
                stepIndex++;
                return Update(update.NewEvents);
            }

            var wrong = step.WrongActions.FirstOrDefault(candidate => candidate.Matches(action));
            if (wrong != null)
            {
                score -= wrong.Penalty;
                criticalFailure |= wrong.Critical;
                return Record(action, ActionOutcome.Penalized, -wrong.Penalty, wrong.Critical);
            }

            return Record(action, ActionOutcome.Rejected, 0, false);
        }

        public AttemptResult Finish()
        {
            EnsureStarted();
            var completed = stepIndex == scenario!.Steps.Count;
            return new AttemptResult(
                context!.AttemptId,
                context.WorkerId,
                context.DeviceId,
                scenario.Id,
                scenario.Version,
                context.StartedAt,
                score,
                completed && score >= scenario.PassScore && !criticalFailure,
                criticalFailure,
                events.ToArray());
        }

        private TrainingUpdate Record(
            TrainingAction action,
            ActionOutcome outcome,
            int scoreDelta,
            bool critical)
        {
            var stepId = stepIndex < scenario!.Steps.Count ? scenario.Steps[stepIndex].Id : "completed";
            var attemptEvent = new AttemptEvent(events.Count + 1, stepId, action, outcome, scoreDelta, critical);
            events.Add(attemptEvent);
            return Update(new[] { attemptEvent });
        }

        private TrainingUpdate Update(IReadOnlyList<AttemptEvent> newEvents)
        {
            var completed = stepIndex >= scenario!.Steps.Count;
            var stepId = completed ? "completed" : scenario.Steps[stepIndex].Id;
            var cueKey = completed ? null : scenario.Steps[stepIndex].CueKey;
            return new TrainingUpdate(
                new TrainingState(stepId, completed),
                cueKey,
                score,
                criticalFailure,
                newEvents);
        }

        private void EnsureStarted()
        {
            if (scenario == null || context == null)
            {
                throw new InvalidOperationException("Begin must be called before using the runtime.");
            }
        }
    }
}
