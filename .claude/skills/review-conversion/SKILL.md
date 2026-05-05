---
name: review-conversion
description: >-
  Reviews a completed Rock RMS Obsidian block conversion for bugs, missing logic,
  redundant code, and pattern violations. Primary input is the /working/{block-name-kebab}/
  research folder produced by /convert-block — review verifies code against the documented
  spec and writes verdicts back into each artifact. The WebForms .ascx.cs is ground truth
  for spec-gap detection (catches what convert-block missed). When /working/ is absent
  (post-archive scenario), falls back to a fresh-eye audit against WebForms only.
  Use when the user says "review conversion", "review block", "review obsidian block",
  "check the conversion", "compare to webforms", "audit the block", or asks to verify
  a converted block before merging. Use after all conversion files are written.
  Do NOT use for: incomplete conversions, non-conversion code review, or general Rock RMS questions.
argument-hint: "Optional: Category/BlockName (e.g. Core/TagsByLetter). Auto-detects from branch if omitted."
compatibility: Requires Claude Code CLI with git for recovering deleted WebForms files from history.
metadata:
  version: "2.0"
  author: "Maxwell Eley"
---

You are performing a thorough review of a completed Rock RMS Obsidian block conversion. The block to review is: **$ARGUMENTS**

**Primary goal:** Zero functionality loss from the original WebForms block.
**Secondary goals:** Catch bugs, redundant code, pattern violations, and missed modernization opportunities.

**Reframing.** `/working/{block-name-kebab}/` is the spec — `/convert-block` already extracted every behavior, every required improvement, every redundancy, every edge case, every user-confirmed decision. Review's job is verifying the code actually delivers on that spec, then writing verdicts back into the artifacts so the audit trail outlives this session. The WebForms `.ascx.cs` stays loaded as ground truth for **spec-gap detection** — if convert-block missed a behavior, you catch it here. When /working/ is absent, the audit reverts to fresh-eye comparison against WebForms.

---

## Exhaustiveness Mandate

**This is the ONLY review pass.** Every issue must be found in this execution. No follow-up review will happen.

- **When in doubt, flag it.** False positives are cheap. Missed bugs are not.
- **Do not skim.** If you catch yourself summarizing a section as "looks fine", re-read it line by line.
- **Do not stop at "method exists."** Trace the full logic: conditions, defaults, error paths, edge cases. A method that exists but filters differently is a bug.
- **Sweep twice.** After the initial audit, re-read the WebForms source top to bottom. The second pass always finds something.
- **Cite both sides.** Every finding cites a code location AND, when applicable, a /working/ artifact source (e.g., `improvement-analysis.md I14`). Citations make findings actionable and create a traceable audit trail.

---

## Reference Routing Table

| Reference File | Load When |
|---|---|
| `references/review-checklist.md` | Phase 3 (always; sections gated per "covered by /working/" annotations at top of file) |
| `/working/{block-name-kebab}/parity-map.md` | Phase 2A (if present) |
| `/working/{block-name-kebab}/improvement-analysis.md` | Phase 2B (if present) |
| `/working/{block-name-kebab}/redundancy-report.md` | Phase 2B (if present) |
| `/working/{block-name-kebab}/new-features.md` | Phase 2B (if present + redesign or new-feature scope) |
| `/working/{block-name-kebab}/obsidian-pattern-analysis.md` | Phase 2C (if present) |
| `/working/{block-name-kebab}/clarifying-questions.md` | Phase 2C (if present) |
| `/working/{block-name-kebab}/figma-design.md` | Phase 2C (if present + redesign scope) |
| `/working/{block-name-kebab}/data-model.md` | Phase 2D (if Phase 1B fired) |
| `/working/{block-name-kebab}/state-machine.md` | Phase 2D (if Phase 1B fired) |
| `/working/{block-name-kebab}/edge-cases.md` | Phase 2D (if Phase 1B fired) |
| `/working/{block-name-kebab}/completeness-analysis.md` | Phase 2D (if Phase 1B fired) |
| `/working/{block-name-kebab}/test-scenarios.md` | Phase 2E (if Phase 1B fired) |

**Do NOT read reference files upfront.** Read only when entering the relevant phase.

---

## Phase 1 — Load

### 1.1 — Identify the block

If `$ARGUMENTS` is provided and is not empty or "undefined", use it as `[Category]/[BlockName]`.

Otherwise, auto-detect from the branch name:
```bash
git branch --show-current
```
Extract the block name from `feature-v{version}-claude-{blocknamelower}`. Use `Glob` to find the matching C# block file under `Rock.Blocks/`.

If the block cannot be resolved, ask the user. Do not guess.

### 1.2 — Read all Obsidian files

Find and read **every file** — do not skim or skip any.

| File | Path | Required |
|---|---|---|
| C# block | `Rock.Blocks/[Category]/[BlockName].cs` | Yes |
| Bags | `Rock.ViewModels/Blocks/[Category]/[BlockName]/*.cs` | Yes |
| .d.ts types | `Rock.JavaScript.Obsidian/Framework/ViewModels/Blocks/[Category]/[BlockName]/*.d.ts` | Yes |
| .obs component | `Rock.JavaScript.Obsidian.Blocks/src/[Category]/[blockName].obs` | Yes |
| Partials | `Rock.JavaScript.Obsidian.Blocks/src/[Category]/[BlockName]/*.partial.obs` and `*.partial.ts` | If they exist |

### 1.3 — Recover the original WebForms block

The WebForms files were likely deleted during conversion. Recover from git:

```bash
git show develop:RockWeb/Blocks/[Category]/[BlockName].ascx.cs
git show develop:RockWeb/Blocks/[Category]/[BlockName].ascx
```

If that fails:
```bash
git log --all --diff-filter=D -- "RockWeb/Blocks/[Category]/[BlockName].ascx.cs" --format="%H" -1
```
Then `git show {commit}~1:RockWeb/Blocks/[Category]/[BlockName].ascx.cs`.

If the WebForms files still exist in the working tree, read them directly.

Read the **full `.ascx.cs`** (primary comparison target for spec-gap detection) and the **`.ascx`** (for markup-embedded logic, event handlers, data binding).

### 1.4 — Inventory the /working/ folder

Compute the kebab-case folder name from the block name (`ContentChannelItemDetail` → `content-channel-item-detail`). Then list `/working/{block-name-kebab}/` and classify each artifact present by tier:

| Tier | Always-produced | Phase-1B-conditional | Scope-conditional |
|---|---|---|---|
| Files | parity-map.md, improvement-analysis.md, redundancy-report.md, obsidian-pattern-analysis.md, clarifying-questions.md, plan.md | data-model.md, logic-graph.md, state-machine.md, completeness-analysis.md, edge-cases.md, test-scenarios.md | figma-design.md, new-features.md |

```bash
ls /working/{block-name-kebab}/ 2>/dev/null
```

Report the inventory in the Phase 1 Gate. Three scenarios:

- **Full /working/ present** (always-produced + Phase-1B + scope artifacts as relevant). The common case after a `/convert-block` run on a non-trivial block. Phase 2 runs all five sub-passes.
- **Partial /working/ present** (only always-produced; no Phase-1B artifacts). Smaller block where Phase 1B did not fire. Phase 2A, 2B, 2C run; 2D and 2E fall back to fresh-eye where their artifacts are absent.
- **No /working/** (post-archive, or convert-block was skipped). Phase 2 collapses to the fresh-eye fallback documented in 2A. Phase 2B-2E are skipped.

For any *always-produced* artifact that is missing without an obvious reason (the folder exists but improvement-analysis.md is absent), surface this in the Phase 1 Gate — convert-block likely did not complete, and review will fall back to fresh-eye for that dimension. Do not silently skip.

Do not read every artifact upfront — wait until each Phase 2 sub-pass loads its own.

### Phase 1 Gate

Confirm before proceeding:
- [ ] All Obsidian files found and fully read
- [ ] WebForms `.ascx.cs` and `.ascx` recovered and fully read
- [ ] /working/ inventory taken; scenario classified (full / partial / absent)
- [ ] Any unexpected gaps in always-produced artifacts surfaced

If anything is missing, report what and ask the user.

---

## Phase 2 — Spec-driven audit

Five focused audit passes. Each loads its own artifacts, walks the relevant code, fills verdicts back into the artifact, and feeds findings into the Phase 4 report.

The exhaustiveness mandate stays: **2A always walks the WebForms `.ascx.cs` top to bottom** to catch behaviors not in any /working/ artifact. The other passes verify documented expectations against code.

### 2A — Parity verification + spec-gap pass

**Inputs:** `parity-map.md` (if present), full WebForms `.ascx.cs` and `.ascx`.

**Two paths:**

- **parity-map.md present.** Use its existing rows (M1-M{n}, Q1-Q{n}, S1-S{n}, N1-N{n}, P1-P{n}, U1-U{n}, Z1-Z{n}, T8-{n}) as the structure. The "WebForms Method/Behavior" column and per-trace architectural summary at the top are already filled by `/convert-block`. Walk the converted code and fill the empty "Obsidian Equivalent (planned)" + "Verdict (planned)" columns — `Matched` / `Differs` / `Missing`.

- **parity-map.md absent.** Build the table from scratch by walking `.ascx.cs` top to bottom. Use the seven trace dimensions documented in `convert-block`'s `references/working/parity-map-template.md`: methods, queries, security, navigation, state, UI behaviors, second sweep (plus Trace 8 if new features were in scope).

**Spec-gap pass (always runs).** After filling verdicts (or building the table), independently walk the WebForms `.ascx.cs` top to bottom one more time, looking for:
- Methods, event handlers, properties not represented in any Trace 1 row
- LINQ chains or SQL not in any Trace 2 row
- `IsAuthorized` / `IsUserAuthorized` calls not in any Trace 3 row
- `NavigateTo*` / `[LinkedPage]` / page parameter reads not in any Trace 4 row
- `ViewState`, hidden fields, `PersonPreference` not in any Trace 5 row
- Notifications, modals, panel visibility, validation not in any Trace 6 row
- Static fields, attributes, computed property logic, `#region` business rules not in any Trace 7 row

For each gap: append a new row to the appropriate trace with the next sequential ID, fill the "Obsidian Equivalent (planned)" column based on what the code actually does (or "Missing" if absent), and surface a finding (the conversion likely missed it too).

**Output:**
- If parity-map.md was loaded: write the filled-in table back to `/working/{block-name-kebab}/parity-map.md`. Preserve the per-trace architectural summary at the top and the existing trace structure. Fill columns; add new rows for spec gaps. Column header text "(planned)" stays as-is for stability.
- Reproduce the full table inline in the Phase 4 report.
- Every "Differs" or "Missing" row becomes a Critical finding (unless intentional per `clarifying-questions.md`, in which case it's a Note with the user-confirmed reason).

### 2B — Required-changes verification

**Inputs:** `improvement-analysis.md`, `redundancy-report.md`, `new-features.md` (if scope includes new features). All conditional on convert-block having produced them.

**For each `I{N}` row in improvement-analysis.md.** Read the row's "Source" (cited WebForms line numbers), "Current behavior", "Proposed change", and severity (P0/P1/P2). Walk the converted code looking for evidence the proposed fix is present.
- Applied → verdict `Applied`.
- Not applied + P0 → verdict `Missed`, surface as **Critical**.
- Not applied + P1 → verdict `Missed`, surface as **Warning**.
- Not applied + P2 → verdict `Missed`, surface as **Note**.
- Partially applied (some conditions handled, others not) → verdict `Partial`, surface at the original severity.

**For each `R{N}` row in redundancy-report.md.** The row identifies dead/duplicate/hand-rolled code that should be dropped or replaced. Confirm the old code is gone OR the framework API is now used.
- Removed/replaced → verdict `Applied`.
- Carried into Obsidian → verdict `Missed`, surface as **Warning** (or **Critical** if the duplicate causes bugs).

**For each `N{N}` row in new-features.md** marked "In-scope for this PR? Yes". Read the acceptance criteria. Verify each criterion is met in the implementation. Cross-reference the corresponding `T8-{N}` row in parity-map.md Trace 8 for the implementation pointer.
- All criteria met → verdict `Applied`.
- Some criteria met → verdict `Partial`, **Warning**.
- None met → verdict `Missed`, **Critical**.

**Output (per artifact).** Append a `## Verification (review-conversion, {YYYY-MM-DD})` section with a short table:

| ID | Verdict | Note |
|---|---|---|
| I14 | Applied | view/edit split confirmed at `ContentChannelItemDetail.cs:284` |
| I16 | Missed | EDIT auth check absent from `DeleteChildItem` block action |

Then write the file back. Do not modify the original convert-block content above the Verification section.

### 2C — Design verification

**Inputs:** `obsidian-pattern-analysis.md`, `clarifying-questions.md`, `figma-design.md` (if redesign scope).

**obsidian-pattern-analysis.md.** Read each section (canonical reference, base class, component selection, form layout, modals, buttons, etc.). For each design recommendation, verify the converted code matches.
- Hand-rolled where canonical exists → **Warning** ("Used `<RadioButtonList :horizontal>` for approval toggle; pattern analysis § 8 specifies `<ButtonGroup>`").
- Wrong base class → **Critical**.
- Wrong canonical reference followed → **Warning**.

**clarifying-questions.md.** For each `Q{N}`, read the user's answer. Verify the code honors it.
- Code matches answer → verdict `Honored`.
- Code contradicts answer → verdict `Violated`, **Critical** (these are explicit user decisions, not soft preferences).
- Question deferred to follow-up → verdict `Deferred`, no finding.

**figma-design.md.** Spot-check (component-level, not pixel-perfect):
- For each `A{N}` annotation: is the design intent reflected in the markup?
- For each `FR{N}` frame in scope: does the corresponding partial use the components mapped in the artifact?
- Component swaps that diverge from the canonical reference because the design required it: did the conversion follow the design or the canonical reference?

**Output.** Append `## Verification (review-conversion, {YYYY-MM-DD})` to each artifact with the row-by-row verdicts.

### 2D — Correctness-in-detail verification

**Inputs:** `data-model.md`, `state-machine.md`, `edge-cases.md`, `completeness-analysis.md`. All conditional on Phase 1B having fired.

**data-model.md §4 view/edit field split.** Extract the actual bag structure from the C# code. Confirm no edit-only field (template strings, OAuth credentials, raw HTML) appears in the view-mode bag path. **P0 security check** — leak = **Critical**.

**data-model.md §5 sibling-block scan.** If a mismatch was flagged (e.g., a still-WebForms detail block that uses `.AsInteger()`), confirm the fix landed OR the deferral matches `clarifying-questions.md`.

**data-model.md cross-language types.** For any TS-side enum, verify it imports from `@Obsidian/Enums/[Domain]/[enumName]` if the auto-generated TS twin exists. If a local re-declaration was unavoidable, verify values match the C# source byte-for-byte.

**state-machine.md.** For each state and transition, confirm the Vue component handles the state's chrome (which panels, which buttons, which notifications) and the transition trigger (which event, which block action).

**edge-cases.md.** For each `E{N}` (Must/Should/Note severity), grep the code for the explicit handling.
- Must-case missed → **Critical**.
- Should-case missed → **Warning**.
- Note-case missed → **Note**.

**completeness-analysis.md.** For each `C{N}`, verify the subtle/hidden behavior is preserved. Common patterns: silent error swallowing → explicit `try/catch` + notification; ViewState flags → reactive state; postback timing tricks → `onMounted` / watcher.

**Output.** Append `## Verification (review-conversion, {YYYY-MM-DD})` to each artifact with row-by-row verdicts.

### 2E — Test-scenarios surface

**Inputs:** `test-scenarios.md` (if Phase 1B fired).

These are user-run manual tests, not auto-runnable. Don't verdict them. Instead, surface them in the Phase 4 report under "Manual verification suggested" so the user knows what to exercise after the review fixes land. Cite each `T{N}` ID and which `E{N}` / `M{N}` / `Q{N}` row it covers.

### Phase 2 Output

A complete parity table (from 2A), plus annotated /working/ artifacts (from 2B, 2C, 2D), plus the test-scenario suggestions list (from 2E). All rolled into the Phase 4 report.

**Do NOT proceed to Phase 3 until Phase 2 is complete.**

---

## Phase 3 — Code quality (focused)

**Load `references/review-checklist.md` now.** This phase catches bugs and pattern issues in the Obsidian code that the per-block /working/ artifacts didn't already capture. Scope is narrower than v1.x.

The checklist has section-level annotations describing whether each section is "covered by /working/ (spot-check only)" or "always run (full sweep)". Honor those annotations — running a full sweep on improvement-analysis.md territory just duplicates 2B.

**Sections to spot-check (covered by /working/ when artifacts present):**
- §1 Functional Parity — covered by 2A.
- §2 Performance and Modernization — covered by 2B (improvement-analysis.md).
- §3 Bug Patterns — mostly covered by 2B; spot-check for issues introduced after convert-block research (e.g., new helper functions added during implementation).

**Sections to run full sweep (no /working/ equivalent):**
- §4 Rock RMS Patterns — naming, code style, block architecture, entity-handling conventions.
- §5 Obsidian-Specific — bag types, block action shapes, reactive state correctness, options-vs-main-bag separation.
- §6 Grid Column Type Matrix — highly specialized; convert-block's research doesn't enumerate these.
- §7 Modernization Checks — general C#/TS modernization not flagged in improvement-analysis.md.

**When /working/ is absent**, all checklist sections run full sweep (the v1 behavior).

---

## Phase 4 — Report and write-back

**This is the plan.** Present it and wait for the user to approve before applying any fix from Phase 5.

### 4.1 — Summary

```
Block:           [Category]/[BlockName]
Verdict:         PASS | PASS WITH NOTES | NEEDS FIXES
Files reviewed:  [count]
/working/ used:  Full | Partial | Absent
Functional parity: [X] / [Y] matched ([Z] missing, [W] differs, [V] spec-gaps)
Spec compliance:   [X] / [Y] /working/ requirements applied
Findings:        [X] critical, [Y] warning, [Z] note
```

### 4.2 — Functional parity table

Reproduce the table from Phase 2A inline. Highlight spec-gap rows (the ones added by 2A's gap pass) — these mean both convert-block AND the conversion missed the WebForms behavior.

### 4.3 — Findings

Group by severity. Omit empty sections. Every finding cites a code location AND, when applicable, a /working/ artifact source.

#### Critical (must fix)

```
[CRITICAL-1] Short title
File: Rock.Blocks/Cms/ContentChannelItemDetail.cs:284
Source: improvement-analysis.md I14 (P0); data-model.md §4
Detail: One or two sentences. What WebForms did, what Obsidian does or
        doesn't do, why this matters.
Fix: The specific planned change.
```

#### Warning (should fix)

```
[WARN-1] Short title
File: [path]:[line]
Source: [/working/ citation if applicable, else "fresh-eye finding"]
Detail: explanation
Fix: specific planned fix
```

#### Note (consider)

```
[NOTE-1] Short title
File: [path]:[line]
Source: [citation or "fresh-eye finding"]
Detail: explanation
```

### 4.4 — Manual verification suggested

Bullet list of `T{N}` test-scenario IDs to exercise post-fix. Each cites which behavior it verifies. Skip this section if test-scenarios.md is absent or empty.

### 4.5 — Write-back to /working/ (always, before Phase 5)

After presenting the report, immediately persist the audit results so they outlive this session. This happens BEFORE the user approves Phase 5 fixes — the verdicts are facts about the current code, independent of whether fixes get applied:

- `parity-map.md` — verdict columns filled (per 2A output).
- `improvement-analysis.md` / `redundancy-report.md` / `new-features.md` — `## Verification (review-conversion, {YYYY-MM-DD})` section appended (per 2B output).
- `obsidian-pattern-analysis.md` / `clarifying-questions.md` / `figma-design.md` — same convention (per 2C).
- `data-model.md` / `state-machine.md` / `edge-cases.md` / `completeness-analysis.md` — same convention (per 2D).
- **Write a new `/working/{block-name-kebab}/review-findings.md`** — single-page audit summary listing every finding with full citations, ordered by severity. This is the file the user opens to see "what did review say?" in one place.

Annotations are *additive* — never overwrite the convert-block original content. A reader can always see what convert-block researched (above the `## Verification` line) vs. what review verified (below it).

If /working/ is absent, write only `review-findings.md` to a sensible location (the user can move it). Do not synthesize fake convert-block artifacts.

**After write-back, wait for the user to approve before proceeding to Phase 5.**

If the verdict is PASS (no Critical or Warning findings), you're done — skip Phase 5.

---

## Phase 5 — Fix (incremental)

**Only after the user approves the report.** Per durable user feedback (memory: "Apply review-conversion fixes incrementally"), do NOT auto-apply every finding. The user picks which findings to fix; each fix is its own approval.

### 5.1 — Implement approved fixes

For each Critical and Warning finding the user approves, in priority order:
1. Implement the fix in the relevant file(s).
2. If a fix spans multiple files (e.g., bag property → C# + TypeScript), make all changes together.
3. Verify the fix doesn't break adjacent logic.

### 5.2 — Verify fixes

After each fix:
1. Re-read each modified file to confirm correctness.
2. Check the fix doesn't introduce new issues (e.g., new bag property populated in C# AND consumed in TypeScript).
3. Flag if `.d.ts` files need regeneration (don't modify them directly).

### 5.3 — Update /working/ verdicts

For each fixed finding, update the corresponding /working/ entry:
- `parity-map.md` — change verdict from `Differs` / `Missing` to `Matched (fixed in review)`.
- `## Verification` sections in improvement/redundancy/new-features/etc. — change verdict from `Missed` / `Partial` to `Applied (fixed in review)`.
- `review-findings.md` — mark the finding as FIXED.

This way the artifacts reflect final state, not pre-fix state.

### 5.4 — Summary

```
Fixed [X] / [Y] approved findings:
- [CRITICAL-1] Title — FIXED
- [WARN-1] Title — FIXED
- [WARN-2] Title — NEEDS MANUAL FIX (reason)
```

Mark any findings that need design decisions as NEEDS MANUAL FIX with explanation.

---

## Troubleshooting

### Cannot recover WebForms files from git
The block was never committed to `develop`, or the branch has no common ancestor. Ask the user for paths or try `git log --all -- "*[BlockName].ascx.cs"` to search all branches.

### Block name doesn't match any files
Use `Glob` to search broadly: `Rock.Blocks/**/*.cs` filtered by partial name. Confirm with user.

### Very large block with many partials
Review in order: C# block first (most bugs live here), then bags, then .obs, then partials. Do not skip any file.

### /working/ folder is incomplete
If always-produced artifacts are missing without explanation, the conversion may have skipped phases. Surface in Phase 1 Gate; review will fall back to fresh-eye for missing dimensions, but warn the user that coverage will be lighter than a full /working/-driven audit.

### /working/ folder is gitignored
Edits to /working/ artifacts will be local-only. Intentional in many setups. Mention in the Phase 4 report so the user knows the audit trail won't propagate via the PR.

### Conflicting evidence (artifact says one thing, code does another, user clearly intended a third)
Trust the user. Cite both `clarifying-questions.md` and the artifact, surface as ESCALATE-style finding ("artifact and code disagree, neither matches user's confirmed answer"), and ask before fixing.

---

## Examples

```
User: "/review-conversion Cms/ContentChannelItemDetail"
Flow:
1. Phase 1: load Obsidian + recover WebForms + inventory /working/
   (full /working/ folder present with 14 artifacts)
2. Phase 2A: fill parity-map.md verdicts; spec-gap pass adds 2 new rows
3. Phase 2B: verify I14 (P0) applied — yes; I16 (P0) applied — NO → CRITICAL
4. Phase 2C: clarifying-questions Q2 (add view panel) — code matches → Honored
5. Phase 2D: edge-cases E8 (no APPROVE auth) — handling missing → CRITICAL
6. Phase 2E: surface T1, T4, T19 as manual tests
7. Phase 3: spot-check checklist §1-3, full sweep §4-7
8. Phase 4: present report (3 critical, 5 warning, 8 note); write back to /working/
9. User approves CRITICAL-1, CRITICAL-3, WARN-2; defers others
10. Phase 5: fix the three; update verdicts; summary
```

```
User: "/review-conversion"  (auto-detect from branch)
Branch: feature-v19-claude-followingbyentitylava
Flow: Extract block name → Glob to locate Rock.Blocks/Following/FollowingByEntityLava.cs
      → /working/following-by-entity-lava/ found, partial (no Phase 1B artifacts)
      → Phase 2A, 2B, 2C run; 2D and 2E fall back / skip
      → rest of flow as above
```

```
User: "/review-conversion Cms/OldArchivedBlock"
Flow: /working/old-archived-block/ ABSENT (post-archive scenario).
      Phase 2 collapses to 2A "build from scratch" path.
      Phase 3 runs full-sweep checklist.
      Phase 4 writes review-findings.md to a sensible location only.
```
