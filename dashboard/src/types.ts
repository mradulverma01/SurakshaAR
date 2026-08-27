export type AttemptSummary = {
  id: string;
  workerName: string;
  moduleName: string;
  score: number;
  passed: boolean;
  criticalFailure: boolean;
  completedAt: string;
};

export type DashboardData = {
  isDemo: boolean;
  workersTrained: number;
  certificatesIssued: number;
  passRate: number;
  pendingSync: number;
  modulePerformance: Array<{ name: string; score: number }>;
  recentAttempts: AttemptSummary[];
  recentCertificates: Array<{
    code: string;
    workerName: string;
    moduleName: string;
    score: number;
    issuedAt: string;
    expiresAt: string | null;
    status: "valid" | "revoked" | "expired";
  }>;
};

export type CertificateVerification = {
  valid: boolean;
  certificateCode: string;
  issuer?: string;
  moduleTitleKey?: string;
  moduleVersion?: number;
  score?: number;
  issuedAt?: string;
  expiresAt?: string | null;
  status?: "valid" | "revoked";
};
