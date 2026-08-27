import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { createClient } from "@supabase/supabase-js";

const url = process.env.SUPABASE_URL;
const serviceRoleKey = process.env.SUPABASE_SERVICE_ROLE_KEY;
if (!url || !serviceRoleKey) {
  throw new Error("SUPABASE_URL and SUPABASE_SERVICE_ROLE_KEY are required");
}

const supabase = createClient(url, serviceRoleKey, {
  auth: { persistSession: false, autoRefreshToken: false },
});

const scenarioDirectory = resolve(import.meta.dirname, "../../mobile/Assets/StreamingAssets/Scenarios");
for (const fileName of ["fire_001.v1.json", "gas_001.v1.json"]) {
  const contents = await readFile(resolve(scenarioDirectory, fileName), "utf8");
  const scenario = JSON.parse(contents) as { id: string; version: number };
  const contentHash = createHash("sha256").update(contents).digest("hex");

  const { data: module, error: moduleError } = await supabase
    .from("training_modules")
    .select("id")
    .eq("slug", scenario.id)
    .single();
  if (moduleError || !module) throw moduleError ?? new Error(`Missing module ${scenario.id}`);

  const { data: existing, error: existingError } = await supabase
    .from("module_versions")
    .select("content_hash")
    .eq("module_id", module.id)
    .eq("version", scenario.version)
    .maybeSingle();
  if (existingError) throw existingError;
  if (existing) {
    if (existing.content_hash !== contentHash) {
      throw new Error(`${scenario.id} v${scenario.version} is already published with different content`);
    }
    process.stdout.write(`Already published ${scenario.id} v${scenario.version} ${contentHash}\n`);
    continue;
  }

  const { error } = await supabase.from("module_versions").insert({
    module_id: module.id,
    version: scenario.version,
    scenario_json: scenario,
    content_hash: contentHash,
  });
  if (error) throw error;

  process.stdout.write(`Published ${scenario.id} v${scenario.version} ${contentHash}\n`);
}
