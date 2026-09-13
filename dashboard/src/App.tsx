import { useEffect, useMemo, useState } from "react";
import { QRCodeSVG } from "qrcode.react";
import {
  backendConfigured,
  demoDashboard,
  filterAttempts,
  filterCertificates,
  loadDashboard,
  signIn,
  signOut,
  toDashboardLoadState,
  verifyCertificate,
} from "./data";
import type { AttemptFilter, CertificateFilter, CertificateVerification, DashboardData } from "./types";

function Metric({ label, value, unit }: { label: string; value: number; unit?: string }) {
  return (
    <article className="metric">
      <span>{label}</span>
      <strong>{value}{unit}</strong>
    </article>
  );
}

function Dashboard() {
  const [data, setData] = useState<DashboardData | null>(backendConfigured ? null : demoDashboard);
  const [error, setError] = useState<string>();
  const [loading, setLoading] = useState(true);
  const [authenticated, setAuthenticated] = useState(!backendConfigured);
  const [attemptFilter, setAttemptFilter] = useState<AttemptFilter>({});
  const [certificateFilter, setCertificateFilter] = useState<CertificateFilter>({});

  const refresh = async () => {
    setLoading(true);
    setError(undefined);
    try {
      const next = await loadDashboard();
      setData(next);
      setAuthenticated(true);
    } catch (reason: unknown) {
      setError(reason instanceof Error ? reason.message : "Dashboard failed to load");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void refresh();
  }, []);

  const state = toDashboardLoadState(data, error ?? null, loading);
  const attempts = useMemo(() => filterAttempts(data?.recentAttempts ?? [], attemptFilter), [data, attemptFilter]);
  const certificates = useMemo(() => filterCertificates(data?.recentCertificates ?? [], certificateFilter), [data, certificateFilter]);
  const modules = useMemo(
    () => Array.from(new Set((data?.recentAttempts ?? []).map((attempt) => attempt.moduleName))),
    [data],
  );

  if (backendConfigured && !authenticated) {
    return <Login error={error} onAuthenticated={refresh} />;
  }
  if (!data) {
    return <main className="shell"><p className="empty">{state.status === "error" ? state.error : "Loading compliance records..."}</p></main>;
  }

  return (
    <main className="shell">
      <header className="masthead">
        <div>
          <p className="eyebrow">Jharkhand mine training network</p>
          <h1>Compliance room</h1>
        </div>
        <div className="header-actions">
          <div className="system-state"><i /> Offline records syncing normally</div>
          {backendConfigured && <button className="text-button" onClick={() => signOut().then(() => setAuthenticated(false))}>Sign out</button>}
        </div>
      </header>

      {data.isDemo && <div className="demo-strip">Demo data. Connect Supabase to show organization records.</div>}
      {loading && <div className="demo-strip" role="status">Loading compliance records...</div>}
      {error && <div className="error-strip" role="alert">{error}</div>}
      {state.status === "empty" && <p className="empty">No compliance records have synced yet.</p>}

      <section className="metrics" aria-label="Training metrics">
        <Metric label="Workers trained" value={data.workersTrained} />
        <Metric label="Certificates issued" value={data.certificatesIssued} />
        <Metric label="Pass rate" value={data.passRate} unit="%" />
        <Metric label="Awaiting sync" value={data.pendingSync} />
      </section>

      <div className="dashboard-grid">
        <section className="panel performance">
          <div className="panel-heading">
            <h2>Module performance</h2>
            <span>Average server score</span>
          </div>
          {data.modulePerformance.length === 0 && <p className="empty">Module averages appear after attempts sync.</p>}
          {data.modulePerformance.map((module) => (
            <div className="performance-row" key={module.name}>
              <div><span>{module.name}</span><strong>{module.score}%</strong></div>
              <div className="track"><i style={{ width: `${module.score}%` }} /></div>
            </div>
          ))}
        </section>

        <aside className="panel proof-card">
          <p className="eyebrow">Verification test</p>
          <QRCodeSVG value={`${window.location.origin}/verify/CERT-DEAD2026ABCDEF01`} size={126} />
          <h2>Scan the training record</h2>
          <p>The QR contains an opaque certificate code, never worker contact details.</p>
          <a href="/verify/CERT-DEAD2026ABCDEF01">Open demo certificate</a>
        </aside>
      </div>

      <section className="panel attempts">
        <div className="panel-heading">
          <h2>Recent attempts</h2>
          <span>Server-validated results</span>
        </div>
        <div>
          <label>Search attempts <input value={attemptFilter.search ?? ""} onChange={(event) => setAttemptFilter((current) => ({ ...current, search: event.target.value }))} placeholder="Worker, module, or ID" /></label>
          <label>Module <select value={attemptFilter.moduleName ?? ""} onChange={(event) => setAttemptFilter((current) => ({ ...current, moduleName: event.target.value }))}><option value="">All modules</option>{modules.map((module) => <option key={module} value={module}>{module}</option>)}</select></label>
          <label>Result <select value={attemptFilter.result ?? "all"} onChange={(event) => setAttemptFilter((current) => ({ ...current, result: event.target.value as AttemptFilter["result"] }))}><option value="all">All results</option><option value="passed">Passed</option><option value="failed">Failed</option><option value="critical_failure">Critical failure</option></select></label>
        </div>
        <div className="table-wrap">
          <table>
            <thead><tr><th>Worker</th><th>Module</th><th>Score</th><th>Result</th><th>Completed</th></tr></thead>
            <tbody>
              {attempts.length === 0 && <tr><td colSpan={5}><p className="empty">No attempts match these filters.</p></td></tr>}
              {attempts.map((attempt) => (
                <tr key={attempt.id}>
                  <td>{attempt.workerName}</td>
                  <td>{attempt.moduleName}</td>
                  <td className="score">{attempt.score}%</td>
                  <td><span className={`status ${attempt.passed ? "pass" : "fail"}`}>{attempt.passed ? "Passed" : attempt.criticalFailure ? "Critical failure" : "Failed"}</span></td>
                  <td>{new Intl.DateTimeFormat("en-IN", { dateStyle: "medium", timeStyle: "short" }).format(new Date(attempt.completedAt))}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>

      <section className="panel certificates-panel">
        <div className="panel-heading">
          <h2>Issued certificates</h2>
          <span>QR links expose only opaque codes</span>
        </div>
        <div>
          <label>Search certificates <input value={certificateFilter.search ?? ""} onChange={(event) => setCertificateFilter((current) => ({ ...current, search: event.target.value }))} placeholder="Worker, module, or code" /></label>
          <label>Module <select value={certificateFilter.moduleName ?? ""} onChange={(event) => setCertificateFilter((current) => ({ ...current, moduleName: event.target.value }))}><option value="">All modules</option>{modules.map((module) => <option key={module} value={module}>{module}</option>)}</select></label>
          <label>Status <select value={certificateFilter.status ?? "all"} onChange={(event) => setCertificateFilter((current) => ({ ...current, status: event.target.value as CertificateFilter["status"] }))}><option value="all">All statuses</option><option value="valid">Valid</option><option value="revoked">Revoked</option><option value="expired">Expired</option></select></label>
        </div>
        <div className="certificate-grid">
          {certificates.length === 0 && <p className="empty">No certificates match these filters.</p>}
          {certificates.map((certificate) => (
            <article className="certificate-item" key={certificate.code}>
              <QRCodeSVG value={`${window.location.origin}/verify/${certificate.code}`} size={86} />
              <div>
                <span className={`status ${certificate.status === "valid" ? "pass" : "fail"}`}>{certificate.status}</span>
                <h3>{certificate.workerName}</h3>
                <p>{certificate.moduleName} / {certificate.score}%</p>
                <a href={`/verify/${certificate.code}`}>{certificate.code}</a>
              </div>
            </article>
          ))}
        </div>
      </section>
    </main>
  );
}

function Login({ error, onAuthenticated }: { error?: string; onAuthenticated: () => void }) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [pending, setPending] = useState(false);
  const [loginError, setLoginError] = useState<string>();

  const submit = async (event: React.FormEvent) => {
    event.preventDefault();
    setPending(true);
    setLoginError(undefined);
    try {
      await signIn(email, password);
      onAuthenticated();
    } catch (reason) {
      setLoginError(reason instanceof Error ? reason.message : "Sign in failed");
    } finally {
      setPending(false);
    }
  };

  return (
    <main className="login-shell">
      <form className="login-panel" onSubmit={submit}>
        <p className="eyebrow">Restricted compliance system</p>
        <h1>Shift access</h1>
        <label>Email<input type="email" value={email} onChange={(event) => setEmail(event.target.value)} required /></label>
        <label>Password<input type="password" value={password} onChange={(event) => setPassword(event.target.value)} required /></label>
        {(loginError || error) && <p className="form-error">{loginError ?? error}</p>}
        <button className="primary-button" disabled={pending}>{pending ? "Checking..." : "Enter compliance room"}</button>
      </form>
    </main>
  );
}

function Verification({ code }: { code: string }) {
  const [result, setResult] = useState<CertificateVerification>();
  const [error, setError] = useState<string>();

  useEffect(() => {
    verifyCertificate(code).then(setResult).catch((reason: unknown) => {
      setError(reason instanceof Error ? reason.message : "Verification failed");
    });
  }, [code]);

  return (
    <main className="verification-shell">
      <a className="back" href="/">Suraksha AR compliance</a>
      <section className={`certificate ${result?.valid ? "is-valid" : ""}`}>
        {!result && !error && <p>Checking certificate...</p>}
        {error && <><p className="stamp failed">Check failed</p><h1>{error}</h1></>}
        {result && (
          <>
            <p className={`stamp ${result.valid ? "" : "failed"}`}>{result.valid ? "Verified training record" : "Certificate not verified"}</p>
            <h1>{result.issuer ?? "No valid record"}</h1>
            {result.moduleTitleKey && (
              <>
                <p className="course">{result.moduleTitleKey}</p>
                <dl>
                  <div><dt>Score</dt><dd>{result.score}%</dd></div>
                  <div><dt>Module version</dt><dd>{result.moduleVersion}</dd></div>
                  <div><dt>Issued</dt><dd>{new Intl.DateTimeFormat("en-IN", { dateStyle: "long" }).format(new Date(result.issuedAt!))}</dd></div>
                  <div><dt>Expires</dt><dd>{result.expiresAt ? new Intl.DateTimeFormat("en-IN", { dateStyle: "long" }).format(new Date(result.expiresAt)) : "No expiry"}</dd></div>
                  <div><dt>Status</dt><dd>{result.status}</dd></div>
                  <div><dt>Certificate</dt><dd>{result.certificateCode}</dd></div>
                </dl>
              </>
            )}
            <p className="certificate-note">This verifies a server-validated training attempt. Recognition depends on the issuing organization.</p>
          </>
        )}
      </section>
    </main>
  );
}

export default function App() {
  const match = window.location.pathname.match(/^\/verify\/([^/]+)$/);
  return match?.[1] ? <Verification code={decodeURIComponent(match[1])} /> : <Dashboard />;
}
