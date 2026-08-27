create or replace function public.reject_module_version_change()
returns trigger
language plpgsql
as $$
begin
  raise exception 'Published module versions are immutable';
end;
$$;

create trigger module_versions_are_immutable
before update or delete on public.module_versions
for each row execute function public.reject_module_version_change();
