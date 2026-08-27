create extension if not exists pgcrypto;

create table public.organizations (
  id uuid primary key default gen_random_uuid(),
  name text not null,
  sector text not null,
  location text,
  created_at timestamptz not null default now()
);

create table public.profiles (
  id uuid primary key references auth.users(id) on delete cascade,
  full_name text not null,
  role text not null check (role in ('worker', 'trainer', 'admin')),
  organization_id uuid references public.organizations(id),
  preferred_language text not null default 'hi',
  created_at timestamptz not null default now()
);

create table public.workers (
  id uuid primary key references public.profiles(id) on delete cascade,
  employee_code text unique,
  mine_or_unit text,
  department text
);

create table public.training_modules (
  id uuid primary key default gen_random_uuid(),
  slug text unique not null,
  title_key text not null,
  active boolean not null default true,
  created_at timestamptz not null default now()
);

create table public.module_versions (
  module_id uuid not null references public.training_modules(id) on delete cascade,
  version integer not null check (version > 0),
  scenario_json jsonb not null,
  content_hash text not null,
  published_at timestamptz not null default now(),
  primary key (module_id, version)
);

create table public.training_attempts (
  id uuid primary key,
  worker_id uuid not null references public.workers(id),
  module_id uuid not null references public.training_modules(id),
  module_version integer not null,
  device_id text not null,
  started_at timestamptz not null,
  completed_at timestamptz not null,
  client_score integer not null,
  server_score integer not null,
  passed boolean not null,
  critical_failure boolean not null,
  evidence_hash text not null,
  received_at timestamptz not null default now(),
  foreign key (module_id, module_version)
    references public.module_versions(module_id, version)
);

create table public.attempt_events (
  id bigint generated always as identity primary key,
  attempt_id uuid not null references public.training_attempts(id) on delete cascade,
  sequence_no integer not null,
  step_id text not null,
  action_kind text not null,
  target_id text not null,
  outcome text not null check (outcome in ('accepted', 'penalized', 'rejected')),
  score_delta integer not null,
  critical boolean not null,
  unique (attempt_id, sequence_no)
);

create table public.certificates (
  id uuid primary key default gen_random_uuid(),
  certificate_code text unique not null,
  worker_id uuid not null references public.workers(id),
  attempt_id uuid unique not null references public.training_attempts(id),
  module_id uuid not null references public.training_modules(id),
  module_version integer not null,
  score integer not null,
  issued_at timestamptz not null default now(),
  expires_at timestamptz,
  status text not null default 'valid' check (status in ('valid', 'revoked'))
);

create index training_attempts_worker_received_idx
  on public.training_attempts (worker_id, received_at desc);
create index certificates_worker_issued_idx
  on public.certificates (worker_id, issued_at desc);

alter table public.organizations enable row level security;
alter table public.profiles enable row level security;
alter table public.workers enable row level security;
alter table public.training_modules enable row level security;
alter table public.module_versions enable row level security;
alter table public.training_attempts enable row level security;
alter table public.attempt_events enable row level security;
alter table public.certificates enable row level security;

create policy "published modules are readable"
  on public.training_modules for select to authenticated using (active);
create policy "published module versions are readable"
  on public.module_versions for select to authenticated using (
    exists (
      select 1 from public.training_modules module
      where module.id = module_id and module.active
    )
  );
create policy "workers read own profile"
  on public.profiles for select to authenticated using (id = auth.uid());
create policy "workers read own worker record"
  on public.workers for select to authenticated using (id = auth.uid());
create policy "workers read own attempts"
  on public.training_attempts for select to authenticated using (worker_id = auth.uid());
create policy "workers read own attempt events"
  on public.attempt_events for select to authenticated using (
    exists (
      select 1 from public.training_attempts attempt
      where attempt.id = attempt_id and attempt.worker_id = auth.uid()
    )
  );
create policy "workers read own certificates"
  on public.certificates for select to authenticated using (worker_id = auth.uid());
