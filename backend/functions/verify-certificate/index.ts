import { createClient } from "npm:@supabase/supabase-js@2";

const headers = {
  "content-type": "application/json",
  "access-control-allow-origin": "*",
  "access-control-allow-headers": "authorization, apikey, content-type",
};

Deno.serve(async (request) => {
  if (request.method === "OPTIONS") {
    return new Response(null, { status: 204, headers });
  }
  if (request.method !== "GET") {
    return new Response(JSON.stringify({ error: "Method not allowed" }), { status: 405, headers });
  }

  const code = new URL(request.url).searchParams.get("code")?.trim().toUpperCase();
  if (!code || !/^CERT-[A-F0-9]{16}$/.test(code)) {
    return new Response(JSON.stringify({ error: "Invalid certificate code" }), { status: 400, headers });
  }

  const url = Deno.env.get("SUPABASE_URL");
  const serviceRoleKey = Deno.env.get("SUPABASE_SERVICE_ROLE_KEY");
  if (!url || !serviceRoleKey) {
    return new Response(JSON.stringify({ error: "Server is not configured" }), { status: 500, headers });
  }

  const supabase = createClient(url, serviceRoleKey, {
    auth: { persistSession: false, autoRefreshToken: false },
  });
  const { data, error } = await supabase
    .from("certificates")
    .select(`
      certificate_code,
      score,
      issued_at,
      expires_at,
      status,
      organizations!certificates_issuer_organization_id_fkey(name),
      training_modules!certificates_module_id_fkey(title_key),
      module_version
    `)
    .eq("certificate_code", code)
    .maybeSingle();

  if (error) {
    return new Response(JSON.stringify({ error: "Verification failed" }), { status: 500, headers });
  }
  if (!data) {
    return new Response(JSON.stringify({ valid: false, certificateCode: code }), { status: 404, headers });
  }

  const certificate = data as unknown as {
    certificate_code: string;
    score: number;
    issued_at: string;
    expires_at: string | null;
    status: "valid" | "revoked";
    module_version: number;
    organizations: { name: string };
    training_modules: { title_key: string };
  };
  const valid = certificate.status === "valid"
    && (!certificate.expires_at || new Date(certificate.expires_at) > new Date());
  return new Response(
    JSON.stringify({
      valid,
      certificateCode: certificate.certificate_code,
      issuer: certificate.organizations.name,
      moduleTitleKey: certificate.training_modules.title_key,
      moduleVersion: certificate.module_version,
      score: certificate.score,
      issuedAt: certificate.issued_at,
      expiresAt: certificate.expires_at,
      status: certificate.status,
    }),
    { status: 200, headers },
  );
});
