---
name: bugfix
description: >-
  Fix a known bug in the Rock RMS codebase. Guides Claude through root cause analysis,
  minimal correct fix, and a release-note commit message. Use when the user says "fix this bug",
  "bugfix", "this is broken", "debug this", describes a bug with file paths or issue numbers,
  or pastes an error/stack trace with intent to fix. Also use when a bug is found by another
  skill (e.g. /review-conversion, /check) and the user wants it fixed.
  Do NOT use for: finding bugs (use /check or /review-conversion), adding features, or refactoring.
argument-hint: "Describe the bug, include file paths and/or issue number if available (e.g. 'The grid filter on Finance/TransactionList resets on page load. Fixes #6694')"
compatibility: Requires Claude Code CLI with access to the Rock RMS codebase.
metadata:
  version: "1.1"
  author: "Maxwell Eley"
---

# Rock RMS Bugfix Workflow

You are fixing a known bug in the Rock RMS codebase. Your job is to understand the root cause, apply the minimal correct fix, and produce a commit message that meets Rock's release-note conventions.

**The bug report:** $ARGUMENTS

---

## Scale to the Bug

Not every bug needs the full investigation process. Match effort to complexity:

- **Obvious fix** (user gives exact file, line, and what's wrong — e.g., "line 42 uses `>` but should be `>=`"): Skip straight to Phase 2. Fix it, verify, output the commit message. Don't load reference files or write a root cause summary for something self-evident.
- **Located but unclear** (user knows the area but not the exact cause — e.g., "the save button on GroupDetail doesn't persist the description"): Run Phase 1 but keep it proportional. You may not need every sub-step.
- **Vague or complex** (user describes a symptom with no location — e.g., "after merging two people, their giving history disappears"): Run the full workflow. Load the reference file. These are the bugs that benefit most from systematic investigation.

Use your judgment. The phases below are a toolkit, not a mandatory checklist.

---

## Reference Routing Table

| Reference File | Load When |
|---|---|
| `references/rock-bug-patterns.md` | When the root cause isn't immediately obvious — especially for stale data, NullRef, cache, or cross-layer bugs |

**Do NOT read reference files for obvious fixes.** Load only when investigation is needed.

---

## Phase 1 — Triage

The goal is to understand the bug well enough to fix it correctly on the first attempt. Resist the urge to jump straight to a patch — most regressions come from fixing symptoms instead of causes.

### 1.1 — Parse the Report

Extract from `$ARGUMENTS`:
- **Symptom**: What's broken? (wrong output, crash, UI glitch, data issue)
- **Location**: File paths, block names, entity names, or areas mentioned
- **Issue number**: GitHub issue `#XXXX` if provided (for the commit message)
- **Reproduction context**: Any steps, conditions, or data states that trigger the bug

### 1.2 — Locate the Code

Start by finding the relevant files. The bug report may give you exact paths, or you may need to search:

- **Block name given** → search `Rock.Blocks/` for the C# class and `Rock.JavaScript.Obsidian.Blocks/src/` for the `.obs` component. Check both — the bug could be server-side, client-side, or a mismatch between them.
- **Entity/model name given** → search `Rock/Model/` for the entity, then find which blocks or services consume it.
- **Error message or stack trace given** → grep for the exact error string or the deepest Rock method in the stack trace. That's your starting point, not the top of the stack.
- **Vague description only** → search for keywords from the symptom across the codebase. Cast a wide net first (grep for the feature name), then narrow down by reading the most relevant hits.

### 1.3 — Investigate the Root Cause

This is the most important step. Don't jump to conclusions — walk the code systematically.

**Before starting, read `references/rock-bug-patterns.md`.** It catalogs Rock-specific bug patterns organized by symptom (stale data, NullRef, security bypass, UI not updating, wrong query results, thread-safety, data loss, cross-layer mismatches). Match the reported symptom to these patterns — they'll shortcut your investigation significantly.

**Step A: Find the entry point.** Identify the method, event handler, block action, or API endpoint where the buggy behavior starts. For Obsidian blocks, this is often a `BlockAction` method on the C# side or a watcher/event handler on the Vue side. For WebForms, look at event handlers like `btnSave_Click` or page lifecycle methods.

**Step B: Walk the execution path.** Follow the logic from the entry point through every service call, query, and conditional branch. Read each method you encounter — don't assume what it does based on its name. Pay special attention to:
- Conditional branches that might take an unexpected path given the bug's trigger conditions
- Values that could be null, empty, zero, or an unexpected type
- Queries that might return no results, multiple results, or stale data
- Properties being set but never persisted (missing `SaveChanges`)
- Order-of-operations issues where something is read before it's written

**Step C: Check recent history.** If the bug feels like a regression (it "used to work"), run `git log --oneline -20` on the affected file(s) to see recent changes. Read the diffs of suspicious commits — often the bug is a side effect of a recent change that looked correct in isolation.

**Step D: Search for related code.** Before concluding you've found the bug, grep for other callers of the affected method or other places that set the same property. The bug might be in a caller, not the method itself. Also check if there's a cache layer (Rock caches aggressively) that could be serving stale data.

**Step E: Confirm root cause vs. symptom.** Ask yourself: "If I fix this specific line, will the bug be fully resolved, or will it just move the failure to a different place?" A symptom fix looks like adding a null check to suppress an exception. A root cause fix looks like ensuring the value is populated correctly upstream so the null never occurs. Prefer the latter.

**Step F: Trace across layers (if the bug spans C# ↔ TypeScript ↔ SQL).** Many Rock bugs look correct in each layer individually but break at the boundary. If the bug involves an Obsidian block:
1. Read the C# block class — check what the `BlockAction` methods return and what the bags contain.
2. Read the `.obs` Vue component — check what it sends in `invokeBlockAction` and what it does with the response.
3. Verify property names match exactly (C# PascalCase → auto-generated camelCase in TypeScript).
4. Verify types survive the JSON round-trip (C# `int?` → JSON `null` vs `0`; `decimal` precision; `DateTime` string format).
5. If a query is involved, check that the LINQ translates to the SQL you expect — sometimes EF generates surprising joins or filters.

### 1.4 — Identify the Domain

Determine the Rock RMS domain for the commit message based on where the bug lives. Use the file path as the primary signal:

| Path contains | Domain |
|---|---|
| `/AI/` | AI |
| `/Api/` or API controllers | API |
| `/Cms/` or `/Blocks/Cms/` | CMS |
| `/CheckIn/` | Check-in |
| `/Communication/` | Communication |
| `/Connection/` | Connection |
| `/Core/` | Core |
| `/Crm/` | CRM |
| `/Engagement/` | Engagement |
| `/Event/` | Event |
| `/WebFarm/` | Farm |
| `/Finance/` | Finance |
| `/Group/` | Group |
| `/Lava/` | Lava |
| `/Lms/` | LMS |
| `/Mobile/` | Mobile |
| `/Prayer/` | Prayer |
| `/Reporting/` | Reporting |
| `/Workflow/` | Workflow |
| Unclear or cross-cutting | Other |

Valid domains (exactly one): `AI`, `API`, `CMS`, `Check-in`, `Communication`, `Connection`, `Core`, `CRM`, `Engagement`, `Event`, `Farm`, `Finance`, `Group`, `Lava`, `LMS`, `Mobile`, `Prayer`, `Reporting`, `Workflow`, `Other`

### 1.5 — Summarize Before Fixing

Before writing any code, state:
1. **Root cause** — one sentence explaining why the bug happens
2. **Fix approach** — what you'll change and why that addresses the root cause (not just the symptom)
3. **Blast radius** — what else could be affected by this change (callers, cache, UI, API consumers)

If the root cause is ambiguous or you find multiple possible causes, say so and explain your reasoning before proceeding.

---

## Phase 2 — Fix

### 2.1 — Apply the Minimal Correct Fix

Change only what's necessary to resolve the root cause. Follow Rock conventions:

- Always use braces, even for single-line `if`/`for`/`else`.
- Use early returns to keep code flat.
- Use `RockDateTime` instead of `DateTime`.
- Don't add optional parameters to public methods — add an overload if needed.
- Wrap any `System.Web` usage in `#if WEBFORMS`.
- If the fix involves a query change, verify the LINQ pattern follows Rock conventions (avoid `Guid` in `.Where()` when `Id` is available, use service-layer predicates over inline duplication).

### 2.2 — Add an Engineering Note (If Non-Obvious)

If the fix would confuse a future reader — because the root cause is subtle, the fix looks wrong at first glance, or there's a non-obvious constraint — add an engineering note:

```csharp
/*
    [Date] - [Author]

    [Why this fix exists and what it prevents.]

    Reason: [One-line summary.]
*/
```

Skip the note if the fix is self-explanatory (e.g., changing `>` to `>=`).

### 2.3 — Verify

Run through this checklist after applying the fix:

- [ ] **Re-read every modified file** — not just the changed lines, but the surrounding method to confirm the fix fits the context.
- [ ] **Public API preserved** — did you change a method signature, remove a parameter, or alter return type? If so, check for callers (grep for the method name). Adding an overload is safer than modifying the existing signature.
- [ ] **Non-buggy case unaffected** — does the fix change behavior for inputs that were already working correctly? Walk through the happy path mentally.
- [ ] **Null safety** — if you added a null check, does it handle the null case correctly (early return, default value, exception)? If you removed one, are you sure the value can never be null?
- [ ] **Cache coherence** — if the bug involved cached data, does the fix invalidate the right cache at the right time? Is there a second cache layer you missed?
- [ ] **Both layers** — if the bug spans C# and TypeScript, did you fix both sides? A C# fix alone may leave the Vue component in an inconsistent state.
- [ ] **Run `/build`** to confirm the fix compiles. Suggest the user run `/test` if relevant tests exist.

### 2.4 — Escalate If Needed

Some bugs can't be fixed with a code change alone. Stop and tell the user if the fix requires:

- **A database migration** — schema change, new column, index, or data backfill. Suggest using `/migration` or `/plugin-migration`.
- **A SQL data fix** — corrupted data in production that needs a script. Suggest using `/sql`.
- **A breaking API change** — the fix would change a public method signature that plugins depend on. Discuss the deprecation approach with the user.
- **A Rock framework change** — the bug is in `Rock.dll` core infrastructure, not in a block or service. These changes have a much larger blast radius and need extra review.
- **A cache architecture change** — the fix requires changing how or when a cache is populated/invalidated at a systemic level, not just adding a `FlushItem()` call.

---

## Phase 3 — Commit Message

Output a ready-to-use commit message following Rock's release-note convention.

### Format

```
+ (Domain) Fixed [concise description of what was broken]. (Fixes #XXXX)
```

**Rules:**
- Start with `Fixed` (past tense) — this classifies it as a Bug Fix in release notes.
- The description should be readable as a standalone release note — someone scanning the changelog should understand what was wrong without seeing the code.
- Include `(Fixes #XXXX)` only if an issue number was provided in the bug report.
- Use exactly one domain from the list above.
- If the fix is trivial (typo, unused import, whitespace) and doesn't warrant a release note, use the `-` prefix instead:
  ```
  - Fixed typo in variable name.
  ```

### Examples

```
+ (Core) Fixed the friendly schedule text display for single-date schedules. (Fixes #6694)
+ (Finance) Fixed ACH transactions failing when the gateway returns a null reference number.
+ (CRM) Fixed duplicate detection not considering middle name during merge.
+ (Group) Fixed group member role dropdown not respecting the configured sort order.
- Fixed incorrect variable name in ConnectionRequestService.
```

### Output

Present the commit message in a code block so the user can copy it directly. If you're uncertain about the domain or description, offer alternatives and let the user choose.

---

## Examples

**Example 1 — Obvious fix (skip straight to Phase 2):**

User says: *"The save button on GroupDetail doesn't persist the description field. The description textarea is reading from `bag.name` instead of `bag.description`."*

The user identified the exact cause. Go directly to Phase 2 — fix the property reference, re-read the surrounding method, run `/build`, and output the commit message. No need to load reference files or write a root cause summary.

**Example 2 — Vague/complex bug (full workflow):**

User says: *"After merging two people, their giving history disappears."*

This needs the full workflow. Load `references/rock-bug-patterns.md` — the "PersonAlias vs Person confusion" pattern is likely relevant. Trace the merge logic to find where `PersonId` is used instead of `PersonAliasId`, confirm by checking the FK relationships, then apply the minimal fix in Phase 2.

---

## Troubleshooting

These are common problems you may hit during the bugfix workflow itself:

**Fix doesn't compile.** Run `/build`, read the exact error. Common causes: missing `using` statement for a type you referenced, wrong parameter type in a method call, or a property name that doesn't exist on the bag. Fix the compilation error — don't skip the build check.

**Root cause unclear after investigation.** Widen the search: check `git log --oneline -20` on affected files for recent regressions, grep for other callers of the suspect method, and look one layer up (the bug may be in a caller, not the method itself). If still unclear, ask the user for reproduction steps or specific data conditions that trigger it.

**Multiple possible root causes.** State all candidates with your reasoning for each. Recommend fixing the most likely one first, and explicitly note the others so the user can investigate further if the first fix doesn't resolve it. Don't silently pick one and ignore the rest.
