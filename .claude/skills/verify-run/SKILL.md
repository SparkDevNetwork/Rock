---
name: verify-run
description: >-
  Run the tests in a Rock RMS verification plan (a VERIFICATION file built by /verify-plan)
  against the local Rock site and database, and record PASS/FAIL and notes in the file.
  Checks that the database is in a known state first, runs what an agent can, and leaves the
  rest with specific notes for a human. Runs one test at a time; an experimental parallel mode
  is available only when the user asks for it.
  Use when the user says "run the verification", "run the test plan", "verify these changes",
  "run VERIFICATION.md", "run the agent tests", or "/verify-run". Do NOT use for: building the
  plan (use /verify-plan), running automated unit tests alone (use /test), or code review.
argument-hint: "Path to the plan (default: newest .verification/*/VERIFICATION.md), optionally a filter: a group name, Test IDs, 'blank', 'failed', or 'parallel'."
compatibility: Requires Claude Code with a browser tool (built-in browser or Claude in Chrome), sqlcmd, and a locally running Rock site.
metadata:
  version: "0.1"
  author: "Daniel Hazelbaker"
---

# Verification Plan Runner

You are running a verification plan by hand, the way a careful tester would, and recording the results in the plan file. Your results are trusted for release decisions, so a wrong `PASS` is far worse than a blank row with a good note.

**Input:** $ARGUMENTS

## Reference Routing Table

| Reference | Load when |
|---|---|
| `references/environment.md` | Always, before running anything. Required state, test users, and the preflight checks. |
| `references/ui-recipes.md` | Always, before the first test. Techniques for driving Rock's UI and data that earlier runs learned the hard way. |
| `references/techniques.md` | When a test needs crafted requests (WF-HF, WF-PB, OB-ACT) or a scripted HTTP session. The plan's Techniques section is a copy; this file is the full version. |
| `references/parallel.md` | When running in parallel (see step 3). |

## Helper script

`scripts/plan.js` (Node) reads and edits plan files. Use it instead of editing table rows by hand; plans can be thousands of lines and a broken row corrupts the table.

```bash
node .claude/skills/verify-run/scripts/plan.js list <plan> --status blank                 # select tests
node .claude/skills/verify-run/scripts/plan.js set <plan> <TestId> PASS                    # record a result
node .claude/skills/verify-run/scripts/plan.js set <plan> <TestId> blank "No SMS number configured"
node .claude/skills/verify-run/scripts/plan.js summary <plan>                             # counts by group
node .claude/skills/verify-run/scripts/plan.js validate <plan>                            # structure check
```

`set` refuses `BLOCKED` unless `--human` is passed, which you do only when a human tells you to record it for them.

## Workflow

### 1. Load the plan

Find the plan: the path given, or the newest `.verification/*/VERIFICATION.md`. Read the header sections (How to use, Environment, Scope, Test users, Techniques) in full. For the tests themselves, read the tables, then read each test's instruction section when you reach it (find it by its `### <Test ID>` heading) rather than loading every instruction up front.

Run `plan.js validate <plan>` first; if the structure is broken, stop and report it rather than guessing at rows. Then select the tests to run from the filter in the input (`plan.js list` with `--group` and `--status`):

- Default: every row with a blank Status.
- A group name, a list of Test IDs, or `failed` (re-run FAIL rows).
- Never re-run `PASS` or `BLOCKED` rows unless the user names them.

### 2. Preflight

Run the preflight checks in `references/environment.md` and show the results as a short table. **Stop and ask** on any Fail. On Warns, list them and ask once whether to continue. Record the outcome (date, build hash, database, any accepted warnings) at the top of the Results summary so readers know what the results were run against.

### 3. One at a time (parallel only on request)

Run the tests one at a time. Parallel mode (`references/parallel.md`) is experimental and has never been tried, so use it only when the user asks for it (for example `parallel` in the input). When more than about 200 tests are selected and the user did not mention it, you may say once that an experimental parallel mode exists and ask whether to use it; default to one at a time and do not ask again.

### 4. Run each test

For each selected test, in table order:

1. If the test has a `Needs:` line, check whether the dependency exists. If it does not, leave the row blank with a note naming it and move on; otherwise run the test normally. Never skip a test only because it looks hard. Read its instruction section and any setup it refers to ("Uses the setup from ...", "Uses Setup: ..."). Before the first test of a Subject, record baselines (counts and highest Ids of the tables its tests touch) so cleanup can be checked.
2. Do the setup, the steps, and check every **Expected** item. Confirm results in the database with read-only SQL whenever the test gives a query, and in the UI.
3. For security tests, run the control (the legitimate request) as well. A blocked request only counts as PASS if the control works.
4. Decide:
   - `PASS`: every Expected item was observed.
   - `FAIL`: an Expected item was contradicted. Notes say what happened (message text, status code, data observed). Before recording it, rule out your own setup (a hand-built request body, stale cache after SQL, queued work that has not run yet: see "Things that look like failures but are not" in `ui-recipes.md`). Then check whether the behavior predates the change (`git log` and `git blame -L` on the code involved, `git show <commit>^:<path>`) and say in Notes whether it is caused by the change or pre-existing.
   - Blank: you could not perform the test. Notes name what was missing (data, setting, service, skill, or tool), specifically enough that someone could teach the next agent. Never set `BLOCKED`.
5. **Record the result immediately** with `plan.js set`. Results must survive an interruption. Never edit Description or the other columns.
6. Do the test's cleanup, and any cleanup for its Subject once all of that Subject's tests are done.

**Report progress to the user every 15 tests** (and when a group finishes). Runs can take hours, and finding progress in a long plan file is hard. Keep it to two or three lines: tests done out of selected, counts so far (PASS, FAIL, blank), the Test IDs of any new FAILs, and the group you are on. For example: "30 of 112 done (27 PASS, 1 FAIL, 2 blank). New FAIL: GroupDetail-04. Now on Finance." Do not wait for a reply; keep running.

Notes also record what you learned that differs from the instructions: a different page, a missing button, an extra required field. If the code differs from the test, follow the code and say so.

Do not fix code during a run. If you find a defect, record it in Notes, finish the run, and report it at the end.

### 5. Results summary

Replace the Results summary section with:

- The preflight record from step 2.
- The count table from `plan.js summary`.
- Bullets for every FAIL (Test ID and one line), defects found outside the tests' own checks, and blank rows grouped by the reason they were skipped.

### 6. Teach the next run

Read the Notes you wrote. For each lesson that would help on any future plan (how to drive a control, a setup quirk, a data trick), propose an addition to `references/ui-recipes.md` in the right section. Show the proposed bullets to the user and add them only after they agree. Skip lessons that only apply to this one change.

### 7. Report

Tell the user the counts, the FAILs, the most common reasons for blank rows, and what is left for a human (the blank rows, grouped by reason).

## Rules

- **Routine setup is pre-approved; do not stop to ask.** Inside the local environment you may, without asking: create test pages under Installed Plugins and add blocks to them, change block settings on those pages, create `Verif ` data through the UI or SQL, add and remove temporary person-specific security rules, configure Rock-side things the sample data lacks (open batches, kiosk devices and configurations, a mobile application), clear the cache, and restart the site. Clean up what you created. Ask only when a step goes beyond this list or beyond what the test describes.
- **Only touch local environments.** Log in only to the local site from the environment check, only with the test users in `environment.md`. Never enter other credentials anywhere.
- **Database writes only for setup and cleanup** that a test describes, scoped to the rows it created. Never run broad updates or deletes. Clear the cache after SQL changes to cached entities.
- **Do not change global configuration** (global attributes, security roles of test users, site settings) unless a test says to, and restore it afterward.
- **Nothing leaves the machine.** Skip any step that would send real email or SMS to a real address, charge a real payment, or publish to an outside service; leave the row blank with a note.
- Writing in the file follows the repo's writing rules: no em-dashes or double hyphens in prose.
