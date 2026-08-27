import { useEffect, useState } from "react";
import { QRCodeSVG } from "qrcode.react";
import { backendConfigured, demoDashboard, loadDashboard, signIn, signOut, verifyCertificate } from "./data";
import type { CertificateVerification, DashboardData } from "./types";

function Metric({ label, value, unit }: { label: string; value: number; unit?: string }) {
  return (
    <article className="metric">
      <span>{label}</span>
      <strong>{value}{unit}</strong>
    </article>
  );
}

function Dashboard() {
  const [data, setData] = useState<DashboardData>(demoDashboard);
  const [error, setError] = useState<string>();
  const [authenticated, setAuthenticated] = useState(!backendConfigured);

  const refresh = () => {
    setError(undefined);
    loadDashboard().then((next) => {
      setData(next);
      setAuthenticated(true);
    }).catch((reason: unknown) => {
      setError(reason instanceof Error ? reason.message : "Dashboard failed to load");
    });
  };

  useEffect(() => {
    refresh();
  }, []);

  if (backendConfigured && !authenticated) {
    return <Login error={error} onAuthenticated={refresh} />;
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
      {error && <div className="error-strip">{error}</div>}

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
        <div className="table-wrap">
          <table>
            <thead><tr><th>Worker</th><th>Module</th><th>Score</th><th>Result</th><th>Completed</th></tr></thead>
            <tbody>
              {data.recentAttempts.map((attempt) => (
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
        <div className="certificate-grid">
          {data.recentCertificates.map((certificate) => (
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
