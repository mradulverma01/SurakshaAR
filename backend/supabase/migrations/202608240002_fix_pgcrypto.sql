drop function if exists public.persist_evaluated_attempt(
  uuid, uuid, uuid, uuid, integer, text, timestamptz, timestamptz,
  integer, integer, boolean, boolean, text, jsonb
);

create function public.persist_evaluated_attempt(
  p_attempt_id uuid,
  p_trainer_id uuid,
  p_worker_id uuid,
  p_module_id uuid,
  p_module_version integer,
  p_device_id text,
  p_started_at timestamptz,
  p_completed_at timestamptz,
  p_client_score integer,
  p_server_score integer,
  p_passed boolean,
  p_critical_failure boolean,
  p_evidence_hash text,
  p_events jsonb
)
returns jsonb
language plpgsql
security definer
set search_path = public, extensions
as $$
declare
  trainer_organization_id uuid;
  worker_organization_id uuid;
  existing_worker_id uuid;
  existing_evidence_hash text;
  existing_server_score integer;
  existing_passed boolean;
  existing_critical_failure boolean;
  issued_code text;
  issuer_authorized boolean;
  inserted_count integer;
begin
  select organization_id
  into trainer_organization_id
  from public.profiles
  where id = p_trainer_id
    and role in ('trainer', 'admin');

  if trainer_organization_id is null then
    raise exception 'Trainer is not authorized to submit attempts' using errcode = '42501';
  end if;

  select profile.organization_id
  into worker_organization_id
  from public.workers worker
  join public.profiles profile on profile.id = worker.id
  where worker.id = p_worker_id;

  if worker_organization_id is null or worker_organization_id <> trainer_organization_id then
    raise exception 'Trainer and worker must belong to the same organization' using errcode = '42501';
  end if;

  select can_issue_certificates
  into issuer_authorized
  from public.organizations
  where id = worker_organization_id;

  select worker_id, evidence_hash, server_score, passed, critical_failure
  into existing_worker_id, existing_evidence_hash, existing_server_score, existing_passed, existing_critical_failure
  from public.training_attempts
  where id = p_attempt_id;

  if existing_worker_id is not null then
    if existing_worker_id <> p_worker_id then
      raise exception 'Attempt id belongs to another worker';
    end if;
    if existing_evidence_hash <> p_evidence_hash then
      raise exception 'Attempt id was submitted with different evidence';
    end if;

    select certificate_code into issued_code
    from public.certificates
    where attempt_id = p_attempt_id;

    return jsonb_build_object(
      'attemptId', p_attempt_id,
      'accepted', true,
      'certificateCode', issued_code,
      'certificateReason', case when existing_passed and issued_code is null then 'issuer_not_authorized' end,
      'serverScore', existing_server_score,
      'passed', existing_passed,
      'criticalFailure', existing_critical_failure
    );
  end if;

  insert into public.training_attempts (
    id,
    worker_id,
    module_id,
    module_version,
    device_id,
    started_at,
    completed_at,
    client_score,
    server_score,
    passed,
    critical_failure,
    evidence_hash
  ) values (
    p_attempt_id,
    p_worker_id,
    p_module_id,
    p_module_version,
    p_device_id,
    p_started_at,
    p_completed_at,
    p_client_score,
    p_server_score,
    p_passed,
    p_critical_failure,
    p_evidence_hash
  ) on conflict (id) do nothing;

  get diagnostics inserted_count = row_count;
  if inserted_count = 0 then
    return public.persist_evaluated_attempt(
      p_attempt_id,
      p_trainer_id,
      p_worker_id,
      p_module_id,
      p_module_version,
      p_device_id,
      p_started_at,
      p_completed_at,
      p_client_score,
      p_server_score,
      p_passed,
      p_critical_failure,
      p_evidence_hash,
      p_events
    );
  end if;

  insert into public.attempt_events (
    attempt_id,
    sequence_no,
    step_id,
    action_kind,
    target_id,
    outcome,
    score_delta,
    critical
  )
  select
    p_attempt_id,
    (event->>'sequence')::integer,
    event->>'stepId',
    event->>'kind',
    event->>'targetId',
    event->>'outcome',
    (event->>'scoreDelta')::integer,
    (event->>'critical')::boolean
  from jsonb_array_elements(p_events) event;

  if p_passed and issuer_authorized then
    issued_code := 'CERT-' || upper(encode(extensions.gen_random_bytes(8), 'hex'));
    insert into public.certificates (
      certificate_code,
      worker_id,
      attempt_id,
      module_id,
      module_version,
      score,
      issuer_organization_id
    ) values (
      issued_code,
      p_worker_id,
      p_attempt_id,
      p_module_id,
      p_module_version,
      p_server_score,
      worker_organization_id
    );
  end if;

  return jsonb_build_object(
    'attemptId', p_attempt_id,
    'accepted', true,
    'certificateCode', issued_code,
    'certificateReason', case when p_passed and not issuer_authorized then 'issuer_not_authorized' end,
    'serverScore', p_server_score,
    'passed', p_passed,
    'criticalFailure', p_critical_failure
  );
end;
$$;

revoke all on function public.persist_evaluated_attempt(
  uuid, uuid, uuid, uuid, integer, text, timestamptz, timestamptz,
  integer, integer, boolean, boolean, text, jsonb
) from public, anon, authenticated;

grant execute on function public.persist_evaluated_attempt(
  uuid, uuid, uuid, uuid, integer, text, timestamptz, timestamptz,
  integer, integer, boolean, boolean, text, jsonb
) to service_role;
