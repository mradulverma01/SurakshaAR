import { describe, expect, it } from "vitest";
import { evaluateAttempt, type Scenario, type SubmittedEvent } from "../functions/_shared/evaluate-attempt.js";

const fireScenario: Scenario = {
  id: "fire_001",
  version: 1,
  passScore: 70,
  steps: [
    { id: "identify", score: 15, accept: [{ kind: "select", targetId: "electrical_fire" }] },
    {
      id: "extinguisher",
      score: 20,
      accept: [{ kind: "select", targetId: "co2" }],
      wrongActions: [{ kind: "select", targetId: "water", penalty: 25, critical: true }],
    },
    { id: "pin", score: 25, accept: [{ kind: "interact", targetId: "pin" }] },
    { id: "exit", score: 40, accept: [{ kind: "waypoint", targetId: "exit_a" }] },
  ],
};

function event(sequence: number, stepId: string, kind: string, targetId: string): SubmittedEvent {
  return { sequence, stepId, kind, targetId };
}

describe("evaluateAttempt", () => {
  it("passes a complete correct event stream", () => {
    const result = evaluateAttempt(fireScenario, [
      event(1, "identify", "select", "electrical_fire"),
      event(2, "extinguisher", "select", "co2"),
      event(3, "pin", "interact", "pin"),
      event(4, "exit", "waypoint", "exit_a"),
    ]);

    expect(result).toMatchObject({ score: 100, passed: true, criticalFailure: false });
  });

  it("rejects certification after a critical action even when all steps finish", () => {
    const result = evaluateAttempt(fireScenario, [
      event(1, "identify", "select", "electrical_fire"),
      event(2, "extinguisher", "select", "water"),
      event(3, "extinguisher", "select", "co2"),
      event(4, "pin", "interact", "pin"),
      event(5, "exit", "waypoint", "exit_a"),
    ]);

    expect(result).toMatchObject({ score: 75, passed: false, criticalFailure: true });
  });

  it("rejects a non-contiguous event stream", () => {
    expect(() =>
      evaluateAttempt(fireScenario, [event(2, "identify", "select", "electrical_fire")]),
    ).toThrow("contiguous");
  });
});
