# Issue tracker: GitHub

Issues and specs for this repo live as GitHub issues. Use the `gh` CLI for all operations.

A GitHub remote must be configured before issue operations can run.

## Conventions

- Create an issue with `gh issue create --title "..." --body "..."`. Use a heredoc for multi-line bodies.
- Read an issue with `gh issue view <number> --comments`, also fetching labels when needed.
- List issues with `gh issue list --state open --json number,title,body,labels,comments` and appropriate label or state filters.
- Comment with `gh issue comment <number> --body "..."`.
- Apply or remove labels with `gh issue edit <number> --add-label "..."` or `--remove-label "..."`.
- Close with `gh issue close <number> --comment "..."`.

Infer the repository from `git remote -v`. The `gh` CLI does this automatically inside a configured clone.

## Pull requests as a triage surface

**PRs as a request surface: no.**

When changed to `yes`, external pull requests use the same labels and states as issues. Read them with `gh pr view`, inspect changes with `gh pr diff`, and use the corresponding `gh pr` commands for comments, labels, and closure.

GitHub shares one number space across issues and pull requests. Resolve an ambiguous number with `gh pr view <number>` and fall back to `gh issue view <number>`.

## Publishing and fetching

When a skill says "publish to the issue tracker", create a GitHub issue.

When a skill says "fetch the relevant ticket", run `gh issue view <number> --comments`.

## Wayfinding operations

Wayfinder uses a single map issue with child issues as decision tickets.

- The map is an issue labelled `wayfinder:map` containing Notes, Decisions so far, Not yet specified, and Out of scope.
- A child ticket is linked as a GitHub sub-issue. If sub-issues are unavailable, add it to a task list in the map and put `Part of #<map>` at the top of the child.
- Child tickets use one `wayfinder:<type>` label: `research`, `prototype`, `grilling`, or `task`.
- Blocking uses GitHub's native issue dependencies. Add an edge through the dependencies API using the blocker's numeric database ID, not its issue number or node ID.
- If native dependencies are unavailable, put `Blocked by: #<number>` near the top of the child body.
- The frontier contains open map children with no open blocker and no assignee. The first child in map order wins.
- Claim a ticket first with `gh issue edit <number> --add-assignee @me`.
- Resolve by commenting with the answer, closing the ticket, and appending a linked one-line gist to the map's Decisions so far.
