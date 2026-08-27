import { createClient } from "npm:@supabase/supabase-js@2";
import {
  evaluateAttempt,
  type Scenario,
  type SubmittedEvent,
} from "../_shared/evaluate-attempt.ts";

type SyncPayload = {
  attemptId: string;
  workerId: string;
  deviceId: string;
  moduleId: string;
  moduleVersion: number;
  startedAt: string;
  completedAt: string;
  clientScore: number;
  events: SubmittedEvent[];
};

const jsonHeaders = { "content-type": "application/json" };

function response(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), { status, headers: jsonHeaders });
}

function isPayload(value: unknown): value is SyncPayload {
  if (!value || typeof value !== "object") return false;
  const payload = value as Partial<SyncPayload>;
  return (
    typeof payload.attemptId === "string" &&
    typeof payload.workerId === "string" &&
    typeof payload.deviceId === "string" &&
    typeof payload.moduleId === "string" &&
    Number.isInteger(payload.moduleVersion) &&
    typeof payload.startedAt === "string" &&
    typeof payload.completedAt === "string" &&
    Number.isInteger(payload.clientScore) &&
    Array.isArray(payload.events)
  );
}

async function evidenceHash(payload: SyncPayload): Promise<string> {
  const canonical = JSON.stringify({
    moduleId: payload.moduleId,
    moduleVersion: payload.moduleVersion,
    events: [...payload.events].sort((left, right) => left.sequence - right.sequence),
  });
  const digest = await crypto.subtle.digest("SHA-256", new TextEncoder().encode(canonical));
  return Array.from(new Uint8Array(digest), (byte) => byte.toString(16).padStart(2, "0")).join("");
}

Deno.serve(async (request) => {
  if (request.method !== "POST") return response({ error: "Method not allowed" }, 405);

  const authorization = request.headers.get("authorization");
  if (!authorization) return response({ error: "Authentication required" }, 401);

  const url = Deno.env.get("SUPABASE_URL");
  const serviceRoleKey = Deno.env.get("SUPABASE_SERVICE_ROLE_KEY");
  if (!url || !serviceRoleKey) return response({ error: "Server is not configured" }, 500);

  const supabase = createClient(url, serviceRoleKey, {
    auth: { persistSession: false, autoRefreshToken: false },
  });
  const token = authorization.replace(/^Bearer\s+/i, "");
  const { data: userData, error: userError } = await supabase.auth.getUser(token);
  if (userError || !userData.user) return response({ error: "Invalid session" }, 401);

  let body: unknown;
  try {
    body = await request.json();
  } catch {
    return response({ error: "Invalid JSON" }, 400);
  }
  if (!isPayload(body)) return response({ error: "Invalid attempt payload" }, 400);

  const { data: module, error: moduleError } = await supabase
    .from("training_modules")
    .select("id, module_versions!inner(version, scenario_json)")
    .eq("slug", body.moduleId)
    .eq("module_versions.version", body.moduleVersion)
    .single();
  if (moduleError || !module) return response({ error: "Unknown module version" }, 422);

  const version = module.module_versions[0];
  if (!version) return response({ error: "Unknown module version" }, 422);

  let evaluation;
  try {
    evaluation = evaluateAttempt(version.scenario_json as Scenario, body.events);
  } catch (error) {
    return response({ error: error instanceof Error ? error.message : "Invalid event stream" }, 422);
  }

  const { data, error } = await supabase.rpc("persist_evaluated_attempt", {
    p_attempt_id: body.attemptId,
    p_trainer_id: userData.user.id,
    p_worker_id: body.workerId,
    p_module_id: module.id,
    p_module_version: body.moduleVersion,
    p_device_id: body.deviceId,
    p_started_at: body.startedAt,
    p_completed_at: body.completedAt,
    p_client_score: body.clientScore,
    p_server_score: evaluation.score,
    p_passed: evaluation.passed,
    p_critical_failure: evaluation.criticalFailure,
    p_evidence_hash: await evidenceHash(body),
    p_events: evaluation.events,
  });
  if (error) {
    return response(
      { error: error.code === "42501" ? "Trainer is not authorized for this worker" : "Attempt could not be stored" },
      error.code === "42501" ? 403 : 500,
    );
  }

  return response(data);
});
