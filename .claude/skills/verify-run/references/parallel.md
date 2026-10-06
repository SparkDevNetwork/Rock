# Parallel Runs

**Experimental.** This mode has never been run. Use it only when the user asks for it, and tell them it is untested. Watch the first run closely and record what went wrong in this file.

For plans with hundreds of agent tests. One orchestrator (you) splits the work, workers run tests and write results to staging files, and the orchestrator merges them into the plan. Workers never edit the plan file.

## Before starting

1. Preflight has passed (SKILL.md step 2). Workers do not repeat it.
2. Ask the user how many workers to use. Suggest one per group, capped at 4: more workers mostly add database and login contention.

## Partitioning

- **The unit of work is a group**, or a set of whole Subjects within a large group. Never split one Subject's tests across workers; they share setup and often depend on each other.
- **Sort tests into three classes** by reading their setup and steps:
  - **Parallel-safe:** read-only checks, probes that expect a refusal and confirm nothing changed, and tests that create and delete their own `Verif ` data.
  - **Serial:** tests that edit shared rows (Ted, Pete, the Decker family, stock batches, global block settings), depend on row counts that other workers could change, or switch global features (search index, SMTP or SMS transports, system settings).
  - **Restart:** tests that change cached data through SQL (Auth rows, attribute values, schedules, group types) and then need a cache clear or site restart, and anything that restarts the site. A restart ends every worker's sessions and drains the background queue, so these never run during the parallel phase.
- Only parallel-safe tests go to workers. Run serial tests yourself after the parallel phase, then the restart tests together so there are as few restarts as possible. When in doubt, classify a test as serial: a wrongly classified test shows up as a flaky result, which is worse than a slow one.
- Balance by test count, but keep the rules above first.

## Logins

The built-in browser has one cookie jar, so workers cannot log in as different users in it at the same time.

- Give the browser to at most one worker (the one with the most WebForms UI tests, which need it most).
- Other workers use scripted HTTP sessions (`techniques.md`) for logins and block actions, and the browser only if the orchestrator assigns it to them after the first worker finishes.
- If too many tests need the browser, run those groups serially instead.

## Worker prompt

Spawn each worker with the Agent tool (general-purpose), in one message so they run concurrently. Each prompt contains:

- The plan path, the Test IDs assigned (in order), and whether the worker may use the browser.
- The staging file path: `.verification/<label>/staging/<worker-name>.md`.
- An instruction to read `.claude/skills/verify-run/SKILL.md` step 4 and its Rules section, plus `references/ui-recipes.md` and `references/techniques.md`, and to follow them exactly, except: write results to the staging file instead of the plan.
- The preflight results, so the worker does not repeat them.

## Progress updates

Workers do not talk to the user. The orchestrator reports to the user each time a worker finishes (its test count and any FAILs, read from its staging file), and keeps the every-15-tests updates from SKILL.md for the tests it runs itself.

## Staging file format

The worker appends one row per test as soon as it finishes it:

```markdown
| Test ID | Status | Notes |
|---|---|---|
| TransactionList-01 | PASS |  |
| TransactionList-02 | FAIL | Move to Batch listed closed batches too. |
```

After its last test, the worker adds a `## Lessons` section with bullets for anything that belongs in `ui-recipes.md`, and a `## Defects` section for defects found outside the tests' own checks.

## Merging

When all workers finish (or one stops early):

1. Apply the staging files with the helper (it updates only Status and Notes, keeps a FAIL when two files disagree, and lists Test IDs it could not find):

   ```bash
   node .claude/skills/verify-run/scripts/plan.js merge <plan> .verification/<label>/staging
   ```

2. Rows that were assigned but have no staging row stay blank; record "Not reached in the parallel run (worker <name> stopped)" with `plan.js set`.
3. A Test ID reported in two staging files is a partitioning mistake. Add a Note to that row saying so.
4. Gather the Lessons and Defects sections for SKILL.md steps 5 and 6.
5. Keep the staging folder until the user has reviewed the results, then offer to delete it.

## Future option: one database per worker

The most isolated setup is a copy of the database and a copy of the site (on its own port) per worker. Workers then cannot interfere, restarts only affect their own site, and almost no tests need classifying. It costs setup time (restores, site copies, connection strings) and is not automated yet. If a developer has set this up, `.verification/environment.json` can list one `siteUrl` and database per worker and each worker gets its own.
