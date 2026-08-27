insert into public.training_modules (slug, title_key)
values
  ('fire_001', 'module.fire.title'),
  ('gas_001', 'module.gas.title')
on conflict (slug) do nothing;
