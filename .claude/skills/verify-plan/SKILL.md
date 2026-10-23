---
name: verify-plan
description: >-
  Build a verification plan (a VERIFICATION file) listing every manual test needed to verify a
  set of Rock RMS changes, written so agents can run what they can and a human finishes the rest.
  Accepts unstaged/staged changes, a single commit, a list of commits, or a range between two
  versions or branches (e.g. "19.6 to 20.0", "hotfix-20.1..release-20.0"). Use when the user says
  "build a test plan", "what do we need to test", "verification plan", "list everything to test
  before we commit", "what needs testing before this release", or "/verify-plan". Do NOT use for:
  running the tests (use /verify-run), writing automated unit tests, or code review (use /check
  or /code-review).
argument-hint: "What to cover: 'unstaged' (default), 'staged', a commit hash, several hashes, or a range like '19.6..20.0'. Optionally an output label."
compatibility: Requires Claude Code CLI with git access to the Rock RMS repository.
metadata:
  version: "0.1"
  author: "Daniel Hazelbaker"
---

# Verification Plan Builder

You are writing a verification plan: a single markdown file that lists every test a person would normally perform by hand to verify a set of changes. Agents will run the plan with `/verify-run` and a human will cover whatever agents cannot. The plan must be specific enough that someone who did not write the code can run each test without reading the diff.

**Input:** $ARGUMENTS

## Reference Routing Table

| Reference | Load when |
|---|---|
| `references/scoping.md` | Always, in step 1. Resolving the input, inventorying the change, and the scope confirmation rules. |
| `references/test-design.md` | Always, in step 3. Which tests each kind of change needs. |
| `references/locating-ui.md` | When a test needs a page, block, job, or other entry point to be found. |
| `references/file-format.md` | Always, in step 4. The exact layout of the output file. |
| `../verify-run/references/techniques.md` | When any test needs crafted requests (hidden field, postback, or block action tampering). Copy the techniques the plan uses into it. |
| `../verify-run/references/environment.md` | In step 4, for the Environment and Test users sections. |

## Workflow

### 1. Resolve the input and inventory the change

Follow `references/scoping.md`. The result is a list of commits (or "working tree"), a list of changed files, and an inventory table of those files by layer and domain.

### 2. Confirm scope (required for ranges and large changes)

Scoping rules are in `references/scoping.md`. In short:

- **Version or branch ranges always require explicit confirmation** of scope before any tests are written, no matter the size.
- Other inputs require confirmation only when they cross the size thresholds in `scoping.md`.
- Show the inventory table, then ask using AskUserQuestion. Never silently drop part of the change; anything out of scope is listed in the plan's Scope section as excluded.

### 3. Design the tests

For each in-scope change, read the diff and enough surrounding code to understand what behavior changed. Then follow `references/test-design.md` to decide which tests it needs. Work group by group (groups are defined in `scoping.md`).

Rules that apply to every test:

- **Every test traces to a change.** Record the commit hash(es) and files it covers. If a change gets no test, say why in the Scope section (for example "comment-only change", "generated file").
- **Test behavior, not code.** "Open a group with a weekly schedule, switch it to Named, Save; the schedule shows the named schedule" rather than "verify `ApplyInlineSchedule` works".
- **Every negative test has a control.** A test that proves something is blocked must also show the legitimate version still works, otherwise a broken feature looks like a passing security check.
- **Every test says how to confirm the result**, preferably both in the UI and with a read-only SQL query.
- **Use the standard test users and sample data** from `environment.md`. Do not invent users. When a test needs specific data, give setup steps (UI or SQL) and cleanup steps.
- **Write every test for an agent to run.** Add a `Needs:` line only for outside dependencies a runner cannot create (see `test-design.md`); it is a hint, not a reason to skip.
- **Check existing automated tests** before writing UI tests, following "Existing automated tests" in `test-design.md`. Keep the UI test unless you are certain the automated test covers the same behavior.
- **Prefer fewer, meaningful tests.** One golden path plus the risky edges beats ten near-duplicates. Merge tests that share all setup and differ only in one input into one test with numbered steps.

If a change is unclear (you cannot tell what user-visible behavior it is meant to have), add the test with your best understanding, add a `Question:` line to its instruction section, and list it under Open questions in the Scope section rather than guessing silently.

Tests that could damage data if the change is broken target throwaway records created in setup (named with the `Verif ` prefix), never stock sample data. Sample data is thin (no open batches, kiosks, mobile apps, LMS data and more; see `../verify-run/references/ui-recipes.md`), so a test that needs such data carries the setup, or points at a Shared setup recipe in the plan. Prefer a tested SQL recipe from `ui-recipes.md` when one exists (it is repeatable and easy to clean up), and give the UI path as the alternative.

### 4. Write the file

Follow `references/file-format.md`. Default path: `.verification/<label>/VERIFICATION.md` at the repo root, where `<label>` is the user's label or one derived from the input (`unstaged-20261005`, `commit-aed54c978f`, `19.6-to-20.0`). The `.verification/` folder is git-ignored.

If the file already exists, do not overwrite it. Ask whether to (1) write a new file with a suffix, or (2) update it in place (reconcile): keep existing Test IDs and their Status/Notes, re-check each test against the current code (button names, action names, messages), add new tests with new IDs, and mark tests whose change is gone as removed in Notes instead of deleting them. Reconcile is also the step to run when a plan was written from unstaged changes and the code changed before it was run.

**Large plans** (more than about 150 tests or more than 6 groups): write the header sections yourself into `.verification/<label>/header.md`, then spawn one general-purpose writer agent per group (or per few small groups), all in one message so they run in parallel. Give each writer its group's commits and files, the confirmed scope choices, and the paths of this skill's references, and have it write a fragment to `.verification/<label>/fragments/NN-<group>.md` in the format described at the top of `assemble` in the helper script. Then assemble:

```bash
node .claude/skills/verify-run/scripts/plan.js assemble .verification/<label>/VERIFICATION.md .verification/<label>/header.md .verification/<label>/fragments
```

A writer that spawns its own sub-writers may finish before they do; check that every fragment exists before assembling.

**Always validate** the finished file and fix every problem it reports (missing sections, missing Expected or Covers lines, bad cells, duplicate IDs, forbidden dashes):

```bash
node .claude/skills/verify-run/scripts/plan.js validate .verification/<label>/VERIFICATION.md --new
```

Then check coverage yourself: every in-scope commit appears in at least one Covers line or in the Scope section's Excluded list.

### 5. Report

Tell the user the path, the number of tests by group (`plan.js summary` prints this), the tests with a `Needs:` line, anything excluded from scope, and any open questions. Suggest `/verify-run <path>` as the next step.

## Output Style

- Be terse while working. Do not narrate each file you read.
- Questions to the user go through AskUserQuestion with concrete options.
- Writing in the plan follows the repo's writing rules: no em-dashes or double hyphens in prose.
