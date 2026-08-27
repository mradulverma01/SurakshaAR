import { createClient } from "@supabase/supabase-js";
import type { CertificateVerification, DashboardData } from "./types";

const url = import.meta.env.VITE_SUPABASE_URL as string | undefined;
const anonKey = import.meta.env.VITE_SUPABASE_ANON_KEY as string | undefined;
const supabase = url && anonKey ? createClient(url, anonKey) : undefined;

export const backendConfigured = Boolean(supabase);

const moduleTitles: Record<string, string> = {
  "module.fire.title": "Fire and explosion response",
  "module.gas.title": "Gas leak and confined-space protocol",
};

export function displayModuleTitle(value: string): string {
  return moduleTitles[value] ?? value;
}

export async function signIn(email: string, password: string): Promise<void> {
  if (!supabase) throw new Error("Supabase is not configured");
  const { error } = await supabase.auth.signInWithPassword({ email, password });
  if (error) throw new Error(error.message);
}

export async function signOut(): Promise<void> {
  await supabase?.auth.signOut();
}

export const demoDashboard: DashboardData = {
  isDemo: true,
  workersTrained: 128,
  certificatesIssued: 92,
  passRate: 84,
  pendingSync: 7,
  modulePerformance: [
    { name: "Fire and explosion response", score: 88 },
    { name: "Gas leak and confined space", score: 76 },
  ],
  recentAttempts: [
    {
      id: "7be2-1",
      workerName: "Rajesh Murmu",
      moduleName: "Fire response",
      score: 86,
      passed: true,
      criticalFailure: false,
      completedAt: "2026-08-23T11:26:12Z",
    },
    {
      id: "7be2-2",
      workerName: "Sita Kisku",
      moduleName: "Gas leak protocol",
      score: 92,
      passed: true,
      criticalFailure: false,
      completedAt: "2026-08-23T10:41:00Z",
    },
    {
      id: "7be2-3",
      workerName: "Birsa Hansda",
      moduleName: "Fire response",
      score: 48,
      passed: false,
      criticalFailure: true,
      completedAt: "2026-08-23T09:18:00Z",
    },
  ],
  recentCertificates: [
    {
      code: "CERT-DEAD2026ABCDEF01",
      workerName: "Rajesh Murmu",
      moduleName: "Fire and explosion response",
      score: 86,
      issuedAt: "2026-08-23T11:28:00Z",
      expiresAt: null,
      status: "valid",
    },
  ],
};

export async function loadDashboard(): Promise<DashboardData> {
  if (!supabase) return demoDashboard;
  const { data: session } = await supabase.auth.getSession();
  if (!session.session) throw new Error("Authentication required");
  const [
    { data: attempts, error: attemptsError },
    { data: certificates, error: certificateError },
    { count: certificateCount, error: certificateCountError },
  ] =
    await Promise.all([
      supabase
        .from("training_attempts")
        .select("id, server_score, passed, critical_failure, completed_at, workers!inner(profiles!inner(full_name)), training_modules!inner(title_key)")
        .order("completed_at", { ascending: false })
        .limit(20),
      supabase
        .from("certificates")
        .select("certificate_code, score, issued_at, expires_at, status, workers!inner(profiles!inner(full_name)), training_modules!inner(title_key)")
        .order("issued_at", { ascending: false })
        .limit(12),
      supabase.from("certificates").select("id", { count: "exact", head: true }),
    ]);

  if (attemptsError || certificateError || certificateCountError || !attempts || !certificates) {
    throw new Error("Compliance data could not be loaded");
  }

  const rows = attempts as unknown as Array<{
    id: string;
    server_score: number;
    passed: boolean;
    critical_failure: boolean;
    completed_at: string;
    workers: { profiles: { full_name: string } };
    training_modules: { title_key: string };
  }>;
  const certificateRows = certificates as unknown as Array<{
    certificate_code: string;
    score: number;
    issued_at: string;
    expires_at: string | null;
    status: "valid" | "revoked";
    workers: { profiles: { full_name: string } };
    training_modules: { title_key: string };
  }>;
  const passed = rows.filter((attempt) => attempt.passed).length;
  const moduleScores = new Map<string, number[]>();
  rows.forEach((attempt) => {
    const name = displayModuleTitle(attempt.training_modules.title_key);
    moduleScores.set(name, [...(moduleScores.get(name) ?? []), attempt.server_score]);
  });

  return {
    isDemo: false,
    workersTrained: new Set(rows.map((attempt) => attempt.workers.profiles.full_name)).size,
    certificatesIssued: certificateCount ?? 0,
    passRate: rows.length === 0 ? 0 : Math.round((passed / rows.length) * 100),
    pendingSync: 0,
    modulePerformance: Array.from(moduleScores, ([name, scores]) => ({
      name,
      score: Math.round(scores.reduce((sum, score) => sum + score, 0) / scores.length),
    })),
    recentAttempts: rows.map((attempt) => ({
      id: attempt.id,
      workerName: attempt.workers.profiles.full_name,
      moduleName: displayModuleTitle(attempt.training_modules.title_key),
      score: attempt.server_score,
      passed: attempt.passed,
      criticalFailure: attempt.critical_failure,
      completedAt: attempt.completed_at,
    })),
    recentCertificates: certificateRows.map((certificate) => ({
      code: certificate.certificate_code,
      workerName: certificate.workers.profiles.full_name,
      moduleName: displayModuleTitle(certificate.training_modules.title_key),
      score: certificate.score,
      issuedAt: certificate.issued_at,
      expiresAt: certificate.expires_at,
      status: certificate.status === "valid"
        && certificate.expires_at
        && new Date(certificate.expires_at) <= new Date()
          ? "expired"
          : certificate.status,
    })),
  };
}

export async function verifyCertificate(code: string): Promise<CertificateVerification> {
  if (!url || !anonKey) {
    return { valid: false, certificateCode: code.toUpperCase() };
  }

  const response = await fetch(
    `${url}/functions/v1/verify-certificate?code=${encodeURIComponent(code)}`,
    { headers: { apikey: anonKey, authorization: `Bearer ${anonKey}` } },
  );
  const body = (await response.json()) as CertificateVerification | { error: string };
  if (response.status === 404) return { valid: false, certificateCode: code.toUpperCase() };
  if (!response.ok || "error" in body) throw new Error("Certificate verification failed");
  return { ...body, moduleTitleKey: body.moduleTitleKey ? displayModuleTitle(body.moduleTitleKey) : undefined };
}
