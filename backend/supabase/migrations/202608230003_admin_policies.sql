create or replace function public.current_profile_role()
returns text
language sql
stable
security definer
set search_path = public
as $$
  select role from public.profiles where id = auth.uid()
$$;

create or replace function public.current_organization_id()
returns uuid
language sql
stable
security definer
set search_path = public
as $$
  select organization_id from public.profiles where id = auth.uid()
$$;

revoke all on function public.current_profile_role from public, anon;
revoke all on function public.current_organization_id from public, anon;
grant execute on function public.current_profile_role to authenticated;
grant execute on function public.current_organization_id to authenticated;

create policy "admins read organization profiles"
  on public.profiles for select to authenticated using (
    public.current_profile_role() in ('admin', 'trainer')
    and organization_id = public.current_organization_id()
  );

create policy "admins read organization workers"
  on public.workers for select to authenticated using (
    public.current_profile_role() in ('admin', 'trainer')
    and exists (
      select 1 from public.profiles profile
      where profile.id = workers.id
        and profile.organization_id = public.current_organization_id()
    )
  );

create policy "admins read organization attempts"
  on public.training_attempts for select to authenticated using (
    public.current_profile_role() in ('admin', 'trainer')
    and exists (
      select 1 from public.profiles profile
      where profile.id = training_attempts.worker_id
        and profile.organization_id = public.current_organization_id()
    )
  );

create policy "admins read organization events"
  on public.attempt_events for select to authenticated using (
    public.current_profile_role() in ('admin', 'trainer')
    and exists (
      select 1
      from public.training_attempts attempt
      join public.profiles profile on profile.id = attempt.worker_id
      where attempt.id = attempt_events.attempt_id
        and profile.organization_id = public.current_organization_id()
    )
  );

create policy "admins read organization certificates"
  on public.certificates for select to authenticated using (
    public.current_profile_role() in ('admin', 'trainer')
    and exists (
      select 1 from public.profiles profile
      where profile.id = certificates.worker_id
        and profile.organization_id = public.current_organization_id()
    )
  );
