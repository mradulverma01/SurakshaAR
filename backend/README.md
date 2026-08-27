# Backend

Apply `supabase/migrations/202608230001_initial_schema.sql`, then seed module records with `supabase/seed.sql`. Upload the exact mobile scenario JSON as each module version and calculate its content hash before publishing.

The mobile client receives only the publishable key. Edge Functions hold the service-role key and must recompute attempt results with `functions/_shared/evaluate-attempt.ts` before inserting server scores or certificates.

Publish the exact scenario files after applying migrations and `seed.sql`:

```bash
SUPABASE_URL=... SUPABASE_SERVICE_ROLE_KEY=... npm run publish-scenarios --workspace @suraksha-ar/backend
```
