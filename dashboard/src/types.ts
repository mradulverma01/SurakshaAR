export type AttemptSummary = {
  id: string;
  workerName: string;
  moduleName: string;
  score: number;
  passed: boolean;
  criticalFailure: boolean;
  completedAt: string;
};

export type ModulePerformance = {
  name: string;
  score: number;
};

export type CertificateStatus = "valid" | "revoked" | "expired";
/** Status values persisted by the backend. Expiry is derived from expiresAt. */
export type StoredCertificateStatus = "valid" | "revoked";

export type CertificateSummary = {
  code: string;
  workerName: string;
  moduleName: string;
  score: number;
  issuedAt: string;
  expiresAt: string | null;
  status: CertificateStatus;
};

export type DashboardData = {
  isDemo: boolean;
  workersTrained: number;
  certificatesIssued: number;
  passRate: number;
  pendingSync: number;
  modulePerformance: ModulePerformance[];
  recentAttempts: AttemptSummary[];
  recentCertificates: CertificateSummary[];
};

export type CertificateVerification = {
  valid: boolean;
  certificateCode: string;
  issuer?: string;
  /** Raw backend title key, retained for API compatibility. */
  moduleTitleKey?: string;
  moduleVersion?: number;
  score?: number;
  issuedAt?: string;
  expiresAt?: string | null;
  /** Display status; expiry is derived from expiresAt after the backend response. */
  status?: CertificateStatus;
};

export type AttemptResult = "passed" | "failed" | "critical_failure";

export type AttemptFilter = {
  search?: string;
  moduleName?: string;
  result?: "all" | AttemptResult;
};

export type CertificateFilter = {
  search?: string;
  moduleName?: string;
  status?: "all" | CertificateStatus;
};

export type DashboardLoadState =
  | { status: "idle" }
  | { status: "loading" }
  | { status: "success"; data: DashboardData }
  | { status: "empty"; data: DashboardData }
  | { status: "error"; error: string };

export type StatusTone = "success" | "warning" | "error" | "info" | "neutral";

export type StatusConfig = {
  label: string;
  tone: StatusTone;
};
