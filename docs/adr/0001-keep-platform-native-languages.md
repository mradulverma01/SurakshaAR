# Keep platform-native languages

The team will keep Unity and AR Foundation code in C#, Supabase Edge Functions and the React dashboard in TypeScript, and database rules in SQL. Although all six team members currently know Python, replacing these languages would require abandoning Unity or adding separately hosted services while leaving C# and browser code in place. Python will not be part of the production stack.

The team will work in three stable pairs. The Unity pair owns AR interactions and mobile behavior, the backend pair owns Supabase functions and SQL, and the dashboard pair owns React and localization. Each pair will learn against this repository, use its existing tests, and review changes together before taking on its assigned code-review findings.
