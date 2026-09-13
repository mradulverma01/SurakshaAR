import { createClient } from "@supabase/supabase-js";
import type {
  AttemptSummary,
  CertificateFilter,
  CertificateStatus,
  CertificateSummary,
  CertificateVerification,
  AttemptFilter,
  AttemptResult,
  DashboardData,
  DashboardLoadState,
  StatusConfig,
  StoredCertificateStatus,
} from "./types";

const rawUrl = import.meta.env.VITE_SUPABASE_URL as string | undefined;
const rawAnonKey = import.meta.env.VITE_SUPABASE_ANON_KEY as string | undefined;
const url = rawUrl?.trim() ? rawUrl.trim() : undefined;
const anonKey = rawAnonKey?.trim() ? rawAnonKey.trim() : undefined;
const supabase = url && anonKey ? createClient(url, anonKey) : undefined;

export const backendConfigured = Boolean(supabase);

// Design-system status mapping
// Owl green #58cc02 -> success (Correct/Valid/Passed)
// Cardinal red #ff4b4b -> error (Failed/Revoked/Expired)
// Streak orange #ff9600 -> warning (pending/attention)
// Eel blue #1cb0f6 -> info, Wolf #777777 -> neutral

export const CERTIFICATE_CODE_PATTERN = /^CERT-[A-F0-9]{16}$/;

const moduleTitles: Record<string, string> = {
  "module.fire.title": "Fire and explosion response",
  "module.gas.title": "Gas leak and confined-space protocol",
};

export function normalizeCertificateCode(code: string): string {
  if (typeof code !== "string") return "";
  return code.trim().toUpperCase();
}

export function isValidCertificateCode(code: string): boolean {
  const normalized = normalizeCertificateCode(code);
  return CERTIFICATE_CODE_PATTERN.test(normalized);
}

export function displayModuleTitle(value: string | null | undefined): string {
  if (typeof value !== "string") return "Training module";
  const trimmed = value.trim();
  if (!trimmed) return "Training module";
  return moduleTitles[trimmed] ?? trimmed;
}

export function getCertificateDisplayStatus(
  status: StoredCertificateStatus | CertificateStatus | string,
  expiresAt: string | null | undefined,
): CertificateStatus {
  if (status === "revoked") return "revoked";
  if (status === "expired") return "expired";
  if (expiresAt) {
    const expiry = new Date(expiresAt);
    if (!Number.isNaN(expiry.getTime()) && expiry.getTime() <= Date.now()) {
      return "expired";
    }
  }
  if (status === "valid") return "valid";
  // Fallback for unknown stored values: treat as valid unless expired
  return "valid";
}

export function getAttemptResult(attempt: Pick<AttemptSummary, "passed" | "criticalFailure">): AttemptResult {
  if (attempt.criticalFailure) return "critical_failure";
  return attempt.passed ? "passed" : "failed";
}

export function getAttemptStatusConfig(attempt: AttemptSummary): StatusConfig {
  const result = getAttemptResult(attempt);
  if (result === "passed") return { label: "Passed", tone: "success" };
  if (result === "critical_failure") return { label: "Critical failure", tone: "error" };
  return { label: "Failed", tone: "error" };
}

export function getCertificateStatusConfig(status: CertificateStatus): StatusConfig {
  if (status === "valid") return { label: "Valid", tone: "success" };
  if (status === "revoked") return { label: "Revoked", tone: "error" };
  return { label: "Expired", tone: "warning" };
}

export function isDashboardEmpty(data: DashboardData): boolean {
  return data.recentAttempts.length === 0 && data.recentCertificates.length === 0;
}

export function filterAttempts(attempts: AttemptSummary[], filter: AttemptFilter): AttemptSummary[] {
  if (!Array.isArray(attempts)) return [];
  const search = filter.search?.trim().toLowerCase();
  const moduleName = filter.moduleName?.trim().toLowerCase();
  const result = filter.result ?? "all";

  return attempts.filter((attempt) => {
    if (search) {
      const haystack = `${attempt.workerName} ${attempt.moduleName} ${attempt.id}`.toLowerCase();
      if (!haystack.includes(search)) return false;
    }
    if (moduleName) {
      if (attempt.moduleName.trim().toLowerCase() !== moduleName) return false;
    }
    if (result !== "all") {
      if (getAttemptResult(attempt) !== result) return false;
    }
    return true;
  });
}

export function filterCertificates(
  certificates: CertificateSummary[],
  filter: CertificateFilter,
): CertificateSummary[] {
  if (!Array.isArray(certificates)) return [];
  const search = filter.search?.trim().toLowerCase();
  const moduleName = filter.moduleName?.trim().toLowerCase();
  const status = filter.status ?? "all";

  return certificates.filter((certificate) => {
    if (search) {
      const haystack = `${certificate.code} ${certificate.workerName} ${certificate.moduleName}`.toLowerCase();
      if (!haystack.includes(search)) return false;
    }
    if (moduleName) {
      if (certificate.moduleName.trim().toLowerCase() !== moduleName) return false;
    }
    if (status !== "all" && certificate.status !== status) return false;
    return true;
  });
}

export function toDashboardLoadState(
  data: DashboardData | null,
  error: string | null,
  loading: boolean,
): DashboardLoadState {
  if (loading) return { status: "loading" };
  if (error) return { status: "error", error };
  if (!data) return { status: "idle" };
  if (isDashboardEmpty(data)) return { status: "empty", data };
  return { status: "success", data };
}

export async function signIn(email: string, password: string): Promise<void> {
  if (!supabase) throw new Error("Supabase is not configured");
  const normalizedEmail = email.trim();
  if (!normalizedEmail || !password) throw new Error("Email and password are required");
  const { error } = await supabase.auth.signInWithPassword({ email: normalizedEmail, password });
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
    { name: "Rescue and evacuation", score: 81 },
  ],
  recentAttempts: [
    {
      id: "7be2-1",
      workerName: "Rajesh Murmu",
      moduleName: "Fire and explosion response",
      score: 86,
      passed: true,
      criticalFailure: false,
      completedAt: "2026-08-23T11:26:12Z",
    },
    {
      id: "7be2-2",
      workerName: "Sita Kisku",
      moduleName: "Gas leak and confined-space protocol",
      score: 92,
      passed: true,
      criticalFailure: false,
      completedAt: "2026-08-23T10:41:00Z",
    },
    {
      id: "7be2-3",
      workerName: "Birsa Hansda",
      moduleName: "Fire and explosion response",
      score: 48,
      passed: false,
      criticalFailure: true,
      completedAt: "2026-08-23T09:18:00Z",
    },
    {
      id: "7be2-4",
      workerName: "Anil Topno",
      moduleName: "Gas leak and confined-space protocol",
      score: 64,
      passed: false,
      criticalFailure: false,
      completedAt: "2026-08-23T08:55:00Z",
    },
    {
      id: "7be2-5",
      workerName: "Sunita Hansda",
      moduleName: "Fire and explosion response",
      score: 98,
      passed: true,
      criticalFailure: false,
      completedAt: "2026-08-22T16:12:00Z",
    },
    {
      id: "7be2-6",
      workerName: "Joseph Minz",
      moduleName: "Gas leak and confined-space protocol",
      score: 73,
      passed: true,
      criticalFailure: false,
      completedAt: "2026-08-22T14:03:00Z",
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
    {
      code: "CERT-BEEF2026ABCDEF02",
      workerName: "Sita Kisku",
      moduleName: "Gas leak and confined-space protocol",
      score: 92,
      issuedAt: "2026-08-23T10:45:00Z",
      expiresAt: "2027-08-23T10:45:00Z",
      status: "valid",
    },
    {
      code: "CERT-CAFE2025ABCDEF03",
      workerName: "Birsa Hansda",
      moduleName: "Fire and explosion response",
      score: 88,
      issuedAt: "2025-06-15T09:00:00Z",
      expiresAt: "2026-01-15T09:00:00Z",
      status: "expired",
    },
    {
      code: "CERT-DEAD2025ABCDEF04",
      workerName: "Anil Topno",
      moduleName: "Gas leak and confined-space protocol",
      score: 79,
      issuedAt: "2025-08-10T08:00:00Z",
      expiresAt: "2027-08-10T08:00:00Z",
      status: "revoked",
    },
  ],
};

type AttemptRow = {
  id: unknown;
  server_score: unknown;
  passed: unknown;
  critical_failure: unknown;
  completed_at: unknown;
  workers: unknown;
  training_modules: unknown;
};

type CertificateRow = {
  certificate_code: unknown;
  score: unknown;
  issued_at: unknown;
  expires_at: unknown;
  status: unknown;
  workers: unknown;
  training_modules: unknown;
};

function getWorkerName(value: unknown): string {
  if (value && typeof value === "object") {
    const workers = value as Record<string, unknown>;
    const profiles = workers["profiles"] as Record<string, unknown> | undefined;
    if (profiles && typeof profiles["full_name"] === "string" && profiles["full_name"].trim()) {
      return profiles["full_name"].trim();
    }
    // Handle array form from Supabase (when not using !inner correctly)
    if (Array.isArray(workers["profiles"]) && workers["profiles"][0]) {
      const first = workers["profiles"][0] as Record<string, unknown>;
      if (typeof first["full_name"] === "string" && first["full_name"].trim()) {
        return first["full_name"].trim();
      }
    }
  }
  return "Unknown worker";
}

function getTitleKey(value: unknown): string {
  if (value && typeof value === "object") {
    const modules = value as Record<string, unknown>;
    if (typeof modules["title_key"] === "string" && modules["title_key"].trim()) {
      return modules["title_key"].trim();
    }
    if (Array.isArray(modules) && modules[0]) {
      const first = modules[0] as Record<string, unknown>;
      if (typeof first["title_key"] === "string" && first["title_key"].trim()) {
        return first["title_key"].trim();
      }
    }
    // Supabase sometimes returns training_modules as object with title_key nested differently
    const maybeModules = modules["training_modules"] as unknown;
    if (maybeModules && typeof maybeModules === "object") {
      return getTitleKey(maybeModules);
    }
  }
  // If value itself looks like a title_key or display name, pass through
  if (typeof value === "string" && value.trim()) return value.trim();
  return "module.unknown.title";
}

function clampScore(value: unknown): number {
  if (typeof value !== "number" || !Number.isFinite(value)) return 0;
  if (value < 0) return 0;
  if (value > 100) return 100;
  return Math.round(value);
}

function isValidIsoDate(value: unknown): boolean {
  if (typeof value !== "string" || !value) return false;
  const d = new Date(value);
  return !Number.isNaN(d.getTime());
}

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

  if (attemptsError || certificateError || certificateCountError) {
    throw new Error("Compliance data could not be loaded");
  }

  const attemptRows: AttemptRow[] = Array.isArray(attempts) ? (attempts as AttemptRow[]) : [];
  const certificateRows: CertificateRow[] = Array.isArray(certificates) ? (certificates as CertificateRow[]) : [];

  const normalizedAttempts: AttemptSummary[] = attemptRows
    .filter((row) => row && typeof row === "object")
    .map((row) => {
      const id = typeof row.id === "string" && row.id.trim() ? row.id.trim() : `unknown-${Math.random().toString(36).slice(2, 6)}`;
      const score = clampScore(row.server_score);
      const passed = row.passed === true;
      const criticalFailure = row.critical_failure === true;
      const completedAt = isValidIsoDate(row.completed_at) ? (row.completed_at as string) : new Date().toISOString();
      const workerName = getWorkerName(row.workers);
      // training_modules may be under row.training_modules or row itself may have nested shape
      const titleKey = getTitleKey(row.training_modules ?? row);
      const moduleName = displayModuleTitle(titleKey);
      return { id, workerName, moduleName, score, passed, criticalFailure, completedAt };
    });

  const normalizedCertificates: CertificateSummary[] = certificateRows
    .filter((row) => row && typeof row === "object")
    .map((row) => {
      const code = typeof row.certificate_code === "string" ? normalizeCertificateCode(row.certificate_code) : "CERT-UNKNOWN";
      const score = clampScore(row.score);
      const issuedAt = isValidIsoDate(row.issued_at) ? (row.issued_at as string) : new Date().toISOString();
      const expiresAt = typeof row.expires_at === "string" && isValidIsoDate(row.expires_at) ? (row.expires_at as string) : row.expires_at === null ? null : null;
      const rawStatus = typeof row.status === "string" ? row.status : "valid";
      const workerName = getWorkerName(row.workers);
      const titleKey = getTitleKey(row.training_modules ?? row);
      const moduleName = displayModuleTitle(titleKey);
      const status = getCertificateDisplayStatus(rawStatus as StoredCertificateStatus, expiresAt);
      return { code, workerName, moduleName, score, issuedAt, expiresAt, status };
    });

  const passedCount = normalizedAttempts.filter((attempt) => attempt.passed).length;
  const moduleScores = new Map<string, number[]>();
  normalizedAttempts.forEach((attempt) => {
    const existing = moduleScores.get(attempt.moduleName) ?? [];
    existing.push(attempt.score);
    moduleScores.set(attempt.moduleName, existing);
  });

  const modulePerformance = Array.from(moduleScores.entries(), ([name, scores]) => ({
    name,
    score: Math.round(scores.reduce((sum, score) => sum + score, 0) / scores.length),
  })).filter((entry) => Number.isFinite(entry.score));

  const workersTrained = new Set(
    normalizedAttempts.map((attempt) => attempt.workerName).filter((name) => name !== "Unknown worker"),
  ).size;

  return {
    isDemo: false,
    workersTrained,
    certificatesIssued: typeof certificateCount === "number" && Number.isFinite(certificateCount) ? certificateCount : normalizedCertificates.length,
    passRate: normalizedAttempts.length === 0 ? 0 : Math.round((passedCount / normalizedAttempts.length) * 100),
    pendingSync: 0,
    modulePerformance,
    recentAttempts: normalizedAttempts,
    recentCertificates: normalizedCertificates,
  };
}

export async function verifyCertificate(code: string): Promise<CertificateVerification> {
  const certificateCode = normalizeCertificateCode(code);

  // Safe fallback when backend is not configured: never claim valid without server check.
  if (!url || !anonKey) {
    return { valid: false, certificateCode };
  }

  // Quick local validation avoids unnecessary network for obviously invalid codes.
  if (!CERTIFICATE_CODE_PATTERN.test(certificateCode)) {
    return { valid: false, certificateCode };
  }

  let response: Response;
  try {
    response = await fetch(
      `${url}/functions/v1/verify-certificate?code=${encodeURIComponent(certificateCode)}`,
      { headers: { apikey: anonKey, authorization: `Bearer ${anonKey}` } },
    );
  } catch {
    throw new Error("Certificate verification failed");
  }

  let body: unknown;
  try {
    body = await response.json();
  } catch {
    throw new Error("Certificate verification failed");
  }

  if (response.status === 404) {
    return { valid: false, certificateCode };
  }

  if (typeof body === "object" && body !== null && "error" in body) {
    // Backend returns 400 for invalid format: treat as not verified, not a hard error.
    if (response.status === 400) {
      return { valid: false, certificateCode };
    }
    throw new Error("Certificate verification failed");
  }

  if (!response.ok) {
    throw new Error("Certificate verification failed");
  }

  const verification = body as CertificateVerification;
  // Type-safe transform: display title instead of raw title_key, preserve contract.
  const expiresAt = verification.expiresAt ?? null;
  return {
    ...verification,
    certificateCode: normalizeCertificateCode(verification.certificateCode ?? certificateCode),
    // Expiry is derived locally because the persisted status intentionally has no
    // `expired` value. This keeps the backend contract intact while making the
    // verification result agree with the dashboard certificate status.
    status: getCertificateDisplayStatus(verification.status ?? "valid", expiresAt),
    moduleTitleKey: verification.moduleTitleKey ? displayModuleTitle(verification.moduleTitleKey) : undefined,
  };
}
