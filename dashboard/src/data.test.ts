import { describe, expect, it } from "vitest";
import {
  demoDashboard,
  displayModuleTitle,
  filterAttempts,
  filterCertificates,
  getAttemptResult,
  getAttemptStatusConfig,
  getCertificateDisplayStatus,
  getCertificateStatusConfig,
  isDashboardEmpty,
  isValidCertificateCode,
  loadDashboard,
  normalizeCertificateCode,
  toDashboardLoadState,
  verifyCertificate,
} from "./data";
import type { AttemptSummary, CertificateSummary, DashboardData } from "./types";

describe("normalizeCertificateCode", () => {
  it("trims and uppercases", () => {
    expect(normalizeCertificateCode("  cert-dead2026abcdef01  ")).toBe("CERT-DEAD2026ABCDEF01");
  });

  it("handles empty and whitespace", () => {
    expect(normalizeCertificateCode("   ")).toBe("");
    expect(normalizeCertificateCode("")).toBe("");
  });
});

describe("isValidCertificateCode", () => {
  it("accepts valid codes case-insensitive", () => {
    expect(isValidCertificateCode("CERT-DEAD2026ABCDEF01")).toBe(true);
    expect(isValidCertificateCode("cert-dead2026abcdef01")).toBe(true);
    expect(isValidCertificateCode("  CERT-DEAD2026ABCDEF01  ")).toBe(true);
  });

  it("rejects invalid patterns", () => {
    expect(isValidCertificateCode("CERT-UNKNOWN0000000")).toBe(false);
    expect(isValidCertificateCode("CERT-DEAD2026ABCDEF0")).toBe(false); // 15 hex
    expect(isValidCertificateCode("CERT-DEAD2026ABCDEF011")).toBe(false); // 17
    expect(isValidCertificateCode("DEAD2026ABCDEF01")).toBe(false);
    expect(isValidCertificateCode("")).toBe(false);
    expect(isValidCertificateCode("CERT-GHIJ2026ABCDEF01")).toBe(false); // non-hex
  });
});

describe("displayModuleTitle", () => {
  it("maps known title keys", () => {
    expect(displayModuleTitle("module.fire.title")).toBe("Fire and explosion response");
    expect(displayModuleTitle("module.gas.title")).toBe("Gas leak and confined-space protocol");
  });

  it("passes through display names", () => {
    expect(displayModuleTitle("Fire and explosion response")).toBe("Fire and explosion response");
  });

  it("handles empty and unknown with fallback", () => {
    expect(displayModuleTitle("")).toBe("Training module");
    expect(displayModuleTitle("   ")).toBe("Training module");
    expect(displayModuleTitle(null as unknown as string)).toBe("Training module");
    expect(displayModuleTitle(undefined as unknown as string)).toBe("Training module");
    expect(displayModuleTitle("module.unknown.title")).toBe("module.unknown.title");
  });

  it("trims input", () => {
    expect(displayModuleTitle("  module.fire.title  ")).toBe("Fire and explosion response");
  });
});

describe("getCertificateDisplayStatus", () => {
  it("returns revoked regardless of expiry", () => {
    expect(getCertificateDisplayStatus("revoked", null)).toBe("revoked");
    expect(getCertificateDisplayStatus("revoked", "2030-01-01T00:00:00Z")).toBe("revoked");
    expect(getCertificateDisplayStatus("revoked", "2020-01-01T00:00:00Z")).toBe("revoked");
  });

  it("returns expired when past expiry even if stored valid", () => {
    expect(getCertificateDisplayStatus("valid", "2020-01-01T00:00:00Z")).toBe("expired");
    expect(getCertificateDisplayStatus("valid", new Date(Date.now() - 1000).toISOString())).toBe("expired");
  });

  it("returns valid when future expiry", () => {
    expect(getCertificateDisplayStatus("valid", "2030-01-01T00:00:00Z")).toBe("valid");
    expect(getCertificateDisplayStatus("valid", null)).toBe("valid");
    expect(getCertificateDisplayStatus("valid", undefined)).toBe("valid");
  });

  it("handles invalid expiry gracefully", () => {
    expect(getCertificateDisplayStatus("valid", "not-a-date")).toBe("valid");
  });

  it("preserves expired input", () => {
    expect(getCertificateDisplayStatus("expired", null)).toBe("expired");
  });
});

describe("getAttemptResult", () => {
  it("maps passed, failed, critical_failure", () => {
    expect(getAttemptResult({ passed: true, criticalFailure: false })).toBe("passed");
    expect(getAttemptResult({ passed: false, criticalFailure: false })).toBe("failed");
    expect(getAttemptResult({ passed: false, criticalFailure: true })).toBe("critical_failure");
    expect(getAttemptResult({ passed: true, criticalFailure: true })).toBe("critical_failure");
  });
});

describe("getAttemptStatusConfig", () => {
  it("maps to design-system tones", () => {
    const passed: AttemptSummary = {
      id: "1",
      workerName: "A",
      moduleName: "Fire",
      score: 90,
      passed: true,
      criticalFailure: false,
      completedAt: new Date().toISOString(),
    };
    expect(getAttemptStatusConfig(passed)).toEqual({ label: "Passed", tone: "success" });

    const critical: AttemptSummary = { ...passed, passed: false, criticalFailure: true };
    expect(getAttemptStatusConfig(critical)).toEqual({ label: "Critical failure", tone: "error" });

    const failed: AttemptSummary = { ...passed, passed: false, criticalFailure: false };
    expect(getAttemptStatusConfig(failed)).toEqual({ label: "Failed", tone: "error" });
  });
});

describe("getCertificateStatusConfig", () => {
  it("maps certificate statuses to tones", () => {
    expect(getCertificateStatusConfig("valid")).toEqual({ label: "Valid", tone: "success" });
    expect(getCertificateStatusConfig("revoked")).toEqual({ label: "Revoked", tone: "error" });
    expect(getCertificateStatusConfig("expired")).toEqual({ label: "Expired", tone: "warning" });
  });
});

describe("isDashboardEmpty", () => {
  it("detects empty dashboards", () => {
    const empty: DashboardData = {
      isDemo: true,
      workersTrained: 0,
      certificatesIssued: 0,
      passRate: 0,
      pendingSync: 0,
      modulePerformance: [],
      recentAttempts: [],
      recentCertificates: [],
    };
    expect(isDashboardEmpty(empty)).toBe(true);
    expect(isDashboardEmpty(demoDashboard)).toBe(false);
  });

  it("empty requires both attempts and certificates empty", () => {
    const onlyAttempts: DashboardData = {
      isDemo: false,
      workersTrained: 1,
      certificatesIssued: 0,
      passRate: 100,
      pendingSync: 0,
      modulePerformance: [],
      recentAttempts: [{ id: "1", workerName: "A", moduleName: "M", score: 80, passed: true, criticalFailure: false, completedAt: new Date().toISOString() }],
      recentCertificates: [],
    };
    expect(isDashboardEmpty(onlyAttempts)).toBe(false);
  });
});

describe("toDashboardLoadState", () => {
  it("derives correct states", () => {
    expect(toDashboardLoadState(null, null, true)).toEqual({ status: "loading" });
    expect(toDashboardLoadState(null, "oops", false)).toEqual({ status: "error", error: "oops" });
    expect(toDashboardLoadState(null, null, false)).toEqual({ status: "idle" });

    const empty: DashboardData = {
      isDemo: true,
      workersTrained: 0,
      certificatesIssued: 0,
      passRate: 0,
      pendingSync: 0,
      modulePerformance: [],
      recentAttempts: [],
      recentCertificates: [],
    };
    expect(toDashboardLoadState(empty, null, false)).toEqual({ status: "empty", data: empty });
    expect(toDashboardLoadState(demoDashboard, null, false)).toEqual({ status: "success", data: demoDashboard });
  });
});

describe("filterAttempts", () => {
  const attempts: AttemptSummary[] = [
    { id: "a1", workerName: "Rajesh Murmu", moduleName: "Fire and explosion response", score: 86, passed: true, criticalFailure: false, completedAt: "2026-08-23T11:26:12Z" },
    { id: "a2", workerName: "Sita Kisku", moduleName: "Gas leak and confined-space protocol", score: 92, passed: true, criticalFailure: false, completedAt: "2026-08-23T10:41:00Z" },
    { id: "a3", workerName: "Birsa Hansda", moduleName: "Fire and explosion response", score: 48, passed: false, criticalFailure: true, completedAt: "2026-08-23T09:18:00Z" },
    { id: "a4", workerName: "Anil Topno", moduleName: "Gas leak and confined-space protocol", score: 64, passed: false, criticalFailure: false, completedAt: "2026-08-23T08:55:00Z" },
  ];

  it("returns all when no filter", () => {
    expect(filterAttempts(attempts, {})).toHaveLength(4);
  });

  it("searches case-insensitive across worker, module, id", () => {
    expect(filterAttempts(attempts, { search: "rajesh" })).toHaveLength(1);
    expect(filterAttempts(attempts, { search: "GAS" })).toHaveLength(2);
    expect(filterAttempts(attempts, { search: "a3" })).toHaveLength(1);
    expect(filterAttempts(attempts, { search: "  SITA  " })).toHaveLength(1);
    expect(filterAttempts(attempts, { search: "nonexistent" })).toHaveLength(0);
  });

  it("filters by module", () => {
    expect(filterAttempts(attempts, { moduleName: "Fire and explosion response" })).toHaveLength(2);
    expect(filterAttempts(attempts, { moduleName: "fire and explosion response" })).toHaveLength(2); // case-insensitive
    expect(filterAttempts(attempts, { moduleName: "  Gas leak and confined-space protocol  " })).toHaveLength(2);
  });

  it("filters by result", () => {
    expect(filterAttempts(attempts, { result: "passed" })).toHaveLength(2);
    expect(filterAttempts(attempts, { result: "failed" })).toHaveLength(1);
    expect(filterAttempts(attempts, { result: "critical_failure" })).toHaveLength(1);
    expect(filterAttempts(attempts, { result: "all" })).toHaveLength(4);
  });

  it("combines filters", () => {
    expect(filterAttempts(attempts, { search: "fire", result: "critical_failure" })).toHaveLength(1);
    expect(filterAttempts(attempts, { search: "gas", result: "passed" })).toHaveLength(1);
    expect(filterAttempts(attempts, { moduleName: "Fire and explosion response", result: "passed" })).toHaveLength(1);
  });

  it("handles non-array gracefully", () => {
    expect(filterAttempts(null as unknown as AttemptSummary[], { search: "a" })).toEqual([]);
  });

  it("returns empty when no match", () => {
    expect(filterAttempts(attempts, { search: "xyz", moduleName: "Fire and explosion response" })).toEqual([]);
  });
});

describe("filterCertificates", () => {
  const certificates: CertificateSummary[] = [
    { code: "CERT-DEAD2026ABCDEF01", workerName: "Rajesh Murmu", moduleName: "Fire and explosion response", score: 86, issuedAt: "2026-08-23T11:28:00Z", expiresAt: null, status: "valid" },
    { code: "CERT-BEEF2026ABCDEF02", workerName: "Sita Kisku", moduleName: "Gas leak and confined-space protocol", score: 92, issuedAt: "2026-08-23T10:45:00Z", expiresAt: "2027-08-23T10:45:00Z", status: "valid" },
    { code: "CERT-CAFE2025ABCDEF03", workerName: "Birsa Hansda", moduleName: "Fire and explosion response", score: 88, issuedAt: "2025-06-15T09:00:00Z", expiresAt: "2026-01-15T09:00:00Z", status: "expired" },
    { code: "CERT-DEAD2025ABCDEF04", workerName: "Anil Topno", moduleName: "Gas leak and confined-space protocol", score: 79, issuedAt: "2025-08-10T08:00:00Z", expiresAt: "2027-08-10T08:00:00Z", status: "revoked" },
  ];

  it("searches across code, worker, module", () => {
    expect(filterCertificates(certificates, { search: "DEAD2026" })).toHaveLength(1);
    expect(filterCertificates(certificates, { search: "sita" })).toHaveLength(1);
    expect(filterCertificates(certificates, { search: "fire" })).toHaveLength(2);
    expect(filterCertificates(certificates, { search: "CERT-" })).toHaveLength(4);
  });

  it("filters by status", () => {
    expect(filterCertificates(certificates, { status: "valid" })).toHaveLength(2);
    expect(filterCertificates(certificates, { status: "revoked" })).toHaveLength(1);
    expect(filterCertificates(certificates, { status: "expired" })).toHaveLength(1);
    expect(filterCertificates(certificates, { status: "all" })).toHaveLength(4);
  });

  it("filters by module", () => {
    expect(filterCertificates(certificates, { moduleName: "Fire and explosion response" })).toHaveLength(2);
  });

  it("combines search and status", () => {
    expect(filterCertificates(certificates, { search: "fire", status: "valid" })).toHaveLength(1);
    expect(filterCertificates(certificates, { search: "fire", status: "expired" })).toHaveLength(1);
  });

  it("trims and handles empty search", () => {
    expect(filterCertificates(certificates, { search: "   " })).toHaveLength(4);
    expect(filterCertificates(certificates, {})).toHaveLength(4);
  });

  it("handles non-array", () => {
    expect(filterCertificates(null as unknown as CertificateSummary[], {})).toEqual([]);
  });
});

describe("demoDashboard", () => {
  it("contains varied demo data for all status conventions", () => {
    expect(demoDashboard.isDemo).toBe(true);
    expect(demoDashboard.recentAttempts.length).toBeGreaterThanOrEqual(4);
    expect(demoDashboard.recentCertificates.length).toBeGreaterThanOrEqual(3);
    const attemptResults = new Set(demoDashboard.recentAttempts.map(getAttemptResult));
    expect(attemptResults.has("passed")).toBe(true);
    expect(attemptResults.has("failed")).toBe(true);
    expect(attemptResults.has("critical_failure")).toBe(true);
    const certStatuses = new Set(demoDashboard.recentCertificates.map((c) => c.status));
    expect(certStatuses.has("valid")).toBe(true);
    expect(certStatuses.has("expired")).toBe(true);
    expect(certStatuses.has("revoked")).toBe(true);
  });

  it("demo metrics are plausible", () => {
    expect(demoDashboard.workersTrained).toBeGreaterThan(0);
    expect(demoDashboard.certificatesIssued).toBeGreaterThan(0);
    expect(demoDashboard.passRate).toBeGreaterThanOrEqual(0);
    expect(demoDashboard.passRate).toBeLessThanOrEqual(100);
    expect(demoDashboard.modulePerformance.length).toBeGreaterThan(0);
    demoDashboard.modulePerformance.forEach((m) => {
      expect(m.score).toBeGreaterThanOrEqual(0);
      expect(m.score).toBeLessThanOrEqual(100);
    });
  });

  it("recent items have valid ISO dates", () => {
    demoDashboard.recentAttempts.forEach((a) => {
      expect(() => new Date(a.completedAt)).not.toThrow();
      expect(Number.isNaN(new Date(a.completedAt).getTime())).toBe(false);
    });
    demoDashboard.recentCertificates.forEach((c) => {
      expect(Number.isNaN(new Date(c.issuedAt).getTime())).toBe(false);
      if (c.expiresAt) expect(Number.isNaN(new Date(c.expiresAt).getTime())).toBe(false);
    });
  });
});

describe("loadDashboard", () => {
  it("returns demo data when backend is not configured", async () => {
    await expect(loadDashboard()).resolves.toEqual(demoDashboard);
  });

  it("demo fallback contains expected keys", async () => {
    const data = await loadDashboard();
    expect(data).toHaveProperty("isDemo");
    expect(data).toHaveProperty("workersTrained");
    expect(data).toHaveProperty("recentAttempts");
    expect(data).toHaveProperty("recentCertificates");
    expect(Array.isArray(data.recentAttempts)).toBe(true);
    expect(Array.isArray(data.recentCertificates)).toBe(true);
  });
});

describe("verifyCertificate", () => {
  it("does not fabricate verification without a configured backend", async () => {
    await expect(verifyCertificate("cert-dead2026abcdef01")).resolves.toEqual({
      valid: false,
      certificateCode: "CERT-DEAD2026ABCDEF01",
    });
  });

  it("does not verify an unknown demonstration code", async () => {
    await expect(verifyCertificate("CERT-UNKNOWN0000000")).resolves.toEqual({
      valid: false,
      certificateCode: "CERT-UNKNOWN0000000",
    });
  });

  it("normalizes lower-case and whitespace codes without backend", async () => {
    await expect(verifyCertificate("  cert-dead2026abcdef01  ")).resolves.toEqual({
      valid: false,
      certificateCode: "CERT-DEAD2026ABCDEF01",
    });
  });

  it("rejects empty and invalid formats as not verified, not as error", async () => {
    await expect(verifyCertificate("")).resolves.toEqual({ valid: false, certificateCode: "" });
    await expect(verifyCertificate("   ")).resolves.toEqual({ valid: false, certificateCode: "" });
    await expect(verifyCertificate("INVALID")).resolves.toEqual({ valid: false, certificateCode: "INVALID" });
    await expect(verifyCertificate("CERT-123")).resolves.toEqual({ valid: false, certificateCode: "CERT-123" });
  });

  it("handles non-hex characters as invalid but safe", async () => {
    await expect(verifyCertificate("CERT-GHIJ2026ABCDEF01")).resolves.toEqual({
      valid: false,
      certificateCode: "CERT-GHIJ2026ABCDEF01",
    });
  });
});
