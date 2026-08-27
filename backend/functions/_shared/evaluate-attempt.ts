export type ScenarioAction = {
  kind: string;
  targetId: string;
};

export type WrongScenarioAction = ScenarioAction & {
  penalty: number;
  critical: boolean;
};

export type ScenarioStep = {
  id: string;
  score: number;
  accept: ScenarioAction[];
  wrongActions?: WrongScenarioAction[];
};

export type Scenario = {
  id: string;
  version: number;
  passScore: number;
  steps: ScenarioStep[];
};

export type SubmittedEvent = {
  sequence: number;
  stepId: string;
  kind: string;
  targetId: string;
};

export type EvaluatedEvent = SubmittedEvent & {
  outcome: "accepted" | "penalized" | "rejected";
  scoreDelta: number;
  critical: boolean;
};

export type AttemptEvaluation = {
  score: number;
  passed: boolean;
  criticalFailure: boolean;
  events: EvaluatedEvent[];
};

function matches(action: ScenarioAction, event: SubmittedEvent): boolean {
  return action.kind === event.kind && action.targetId === event.targetId;
}

export function evaluateAttempt(
  scenario: Scenario,
  submittedEvents: SubmittedEvent[],
): AttemptEvaluation {
  if (scenario.steps.length === 0) {
    throw new Error("Scenario has no steps");
  }

  const events = [...submittedEvents].sort((left, right) => left.sequence - right.sequence);
  events.forEach((event, index) => {
    if (event.sequence !== index + 1) {
      throw new Error("Attempt event sequence must be contiguous and start at one");
    }
  });

  let stepIndex = 0;
  let score = 0;
  let criticalFailure = false;
  const evaluated: EvaluatedEvent[] = [];

  for (const event of events) {
    const step = scenario.steps[stepIndex];
    if (!step) {
      evaluated.push({ ...event, outcome: "rejected", scoreDelta: 0, critical: false });
      continue;
    }

    if (event.stepId !== step.id) {
      evaluated.push({ ...event, outcome: "rejected", scoreDelta: 0, critical: false });
      continue;
    }

    if (step.accept.some((action) => matches(action, event))) {
      score += step.score;
      stepIndex += 1;
      evaluated.push({ ...event, outcome: "accepted", scoreDelta: step.score, critical: false });
      continue;
    }

    const wrongAction = step.wrongActions?.find((action) => matches(action, event));
    if (wrongAction) {
      score -= wrongAction.penalty;
      criticalFailure ||= wrongAction.critical;
      evaluated.push({
        ...event,
        outcome: "penalized",
        scoreDelta: -wrongAction.penalty,
        critical: wrongAction.critical,
      });
      continue;
    }

    evaluated.push({ ...event, outcome: "rejected", scoreDelta: 0, critical: false });
  }

  return {
    score,
    passed: stepIndex === scenario.steps.length && score >= scenario.passScore && !criticalFailure,
    criticalFailure,
    events: evaluated,
  };
}
