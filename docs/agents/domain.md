# Domain documentation

This repository uses a single domain context.

## Before exploring

- Read `CONTEXT.md` at the repository root.
- Read ADRs under `docs/adr/` that affect the area being changed.
- If either location does not exist, proceed without calling out its absence.

The domain-modeling workflow creates glossary and ADR files lazily when the team resolves terms or decisions.

## Layout

```text
/
├── CONTEXT.md
├── docs/
│   └── adr/
└── mobile, backend, dashboard
```

## Vocabulary

Use terms as defined in `CONTEXT.md` in issue titles, specifications, tests, and code discussions. Do not replace defined terms with loose synonyms.

If a needed concept is missing, first check whether existing vocabulary already covers it. Use domain modeling when the gap represents a real domain distinction.

## ADR conflicts

Surface any conflict with an existing ADR instead of silently overriding it. Name the ADR and explain why the decision may need to be reopened.
