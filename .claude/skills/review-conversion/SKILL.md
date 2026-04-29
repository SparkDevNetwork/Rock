---
name: review-conversion
description: >-
  Reviews a completed Rock RMS Obsidian block conversion for bugs, missing logic,
  redundant code, and pattern violations by comparing against the original WebForms block.
  Use when the user says "review conversion", "review block", "review obsidian block",
  "check the conversion", "compare to webforms", "audit the block", or asks to verify
  a converted block before merging. Use after all conversion files are written.
  Do NOT use for: incomplete conversions, non-conversion code review, or general Rock RMS questions.
argument-hint: "Optional: Category/BlockName (e.g. Core/TagsByLetter). Auto-detects from branch if omitted."
compatibility: Requires Claude Code CLI with git for recovering deleted WebForms files from history.
metadata:
  version: "1.3"
  author: "Maxwell Eley"
---

You are performing a thorough review of a completed Rock RMS Obsidian block conversion. The block to review is: **$ARGUMENTS**

**Primary goal:** Zero functionality loss from the original WebForms block.
**Secondary goals:** Catch bugs, redundant code, pattern violations, and missed modernization opportunities.

---

## Exhaustiveness Mandate

**This is the ONLY review pass.** Every issue must be found in this execution. No follow-up review will happen.

- **When in doubt, flag it.** False positives are cheap. Missed bugs are not.
- **Do not skim.** If you catch yourself summarizing a section as "looks fine", re-read it line by line.
- **Do not stop at "method exists."** Trace the full logic: conditions, defaults, error paths, edge cases. A method that exists but filters differently is a bug.
- **Sweep twice.** After the initial audit, re-read the WebForms source top to bottom. The second pass always finds something.

---

## Reference Routing Table

| Reference File | Load When |
|---|---|
| `references/review-checklist.md` | Phase 3 (always) |

**Do NOT read reference files upfront.** Read only when entering the relevant phase.

---

## Phase 1 — Load All Files

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

If that fails, try:
```bash
git log --all --diff-filter=D -- "RockWeb/Blocks/[Category]/[BlockName].ascx.cs" --format="%H" -1
```
Then `git show {commit}~1:RockWeb/Blocks/[Category]/[BlockName].ascx.cs`.

If the WebForms files still exist in the working tree, read them directly.

Read the **full `.ascx.cs`** (primary comparison target) and the **`.ascx`** (for markup-embedded logic, event handlers, data binding).

### Phase 1 Gate

Confirm before proceeding:
- [ ] All Obsidian files found and fully read
- [ ] WebForms `.ascx.cs` and `.ascx` recovered and fully read

If anything is missing, report what and ask the user.

---

## Phase 2 — Functional Parity Audit

This is the most important phase. Every method, query, check, and behavior in the WebForms block must have a verified equivalent.

### 2.1 — Build the method map

Go through the **entire** `.ascx.cs` top to bottom. For every method, event handler, property, and behavior:

1. **Name** and what it does (one line)
2. **Obsidian equivalent** — the block action, .obs handler, bag property, or init logic that replaces it
3. **Logic comparison** — not just "present" but **actually correct**: same filters, conditions, result
4. **Verdict** — Matched / Differs / Missing

Do NOT skip any method — even boilerplate like `OnInit` or `OnLoad` may contain hidden logic.

**For each method, also verify:**
- Every **conditional branch** (if/else, switch, ternary) — Obsidian handles every path the same way?
- Every **default value** and **fallback** — null/empty cases produce the same result?
- Every **string literal** — error messages, notifications, format strings match or are intentionally improved?
- Every **event handler** wired in `.ascx` markup — `OnClick`, `OnRowSelected`, `OnGridRebind` — maps to an Obsidian equivalent?

### 2.2 — Trace data queries

For each database query in WebForms:
- LINQ/SQL filters identical?
- `.Include()` / eager loading matches?
- Ordering/sorting preserved?
- `.Select()` projections produce equivalent data?
- Grid column mappings match?

### 2.3 — Trace security checks

- `IsUserAuthorized()` calls preserved?
- Entity-level `IsAuthorized()` preserved?
- `[SecurityAction]` attributes present and matching?
- Admin-only features gated the same way?

### 2.4 — Trace navigation and page parameters

- All linked page navigations preserved?
- Page parameter names match (using `PageParameterKey` constants)?
- Breadcrumb behavior matches?
- Cancel/back navigation works?

### 2.5 — Trace user preferences and state

- `PersonPreference` / `UserPreference` values carried over?
- Grid filter preferences persist?
- ViewState-equivalent state preserved?

### 2.6 — Trace UI behaviors

- Show/hide panel logic matches?
- Validation messages match?
- Success/error notifications preserved?
- Empty-state handling matches?
- Modal dialogs replicated?

### 2.7 — Second sweep

Re-read the WebForms `.ascx.cs` **one more time, top to bottom.** For every line, verify it's accounted for in your method map or is irrelevant WebForms plumbing (`ViewState`, `IsPostBack`).

Look specifically for:
- Static fields and constants that affect behavior
- Small private helper methods (filtering, formatting, validation)
- Attribute decorations (`[LinkedPage]`, `[SecurityAction]`, etc.)
- Property getters/setters with computed logic
- Business rules buried in comments or `#region` blocks

Add anything the method map missed.

### 2.8 — Completeness gate

Verify counts before proceeding:
1. **Methods/handlers** in WebForms = rows in parity table (excluding pure lifecycle boilerplate with no logic)
2. **Database queries** — each has a traced equivalent
3. **Security checks** — each is verified
4. **Linked page attributes** — each maps to a navigation

If any count is off, find what was missed before continuing.

### Phase 2 Output

**Produce the Functional Parity Table now:**

| # | WebForms Method/Behavior | Obsidian Equivalent | Verdict |
|---|---|---|---|
| 1 | [method] — [what it does] | [block action / handler / bag property] | Matched / Differs / Missing |

Every "Differs" or "Missing" row becomes a Critical finding.

**Do NOT proceed to Phase 3 until this table is complete.**

---

## Phase 3 — Code Quality Review

**Load `references/review-checklist.md` now.** This phase catches bugs and pattern issues in the Obsidian code itself, independent of the WebForms comparison.

### 3.1 — Performance scan

- **N+1 queries:** `.Get()` or `.Queryable()` inside loops — must pre-fetch
- **Cache misuse:** Using services for entities with cache classes (`DefinedTypeCache`, `CampusCache`, `GroupTypeCache`, `EntityTypeCache`, `CategoryCache`) — use cache for read-only lookups
- **Lazy-load traps:** Navigation properties in loops without `.Include()` or pre-fetching
- **Unnecessary round-trips:** Logic that could be in the initial bag instead of a separate block action
- **WebForms carry-forward:** `ViewState` remnants, `string.Format`, deep null-check nesting, empty catch blocks

Note: these may also exist in the original WebForms block. Flag them regardless — conversion is the time to modernize, not carry forward anti-patterns.

### 3.2 — Bug scan

- Null reference risks after `.Get()` calls
- Type mismatches between C# bags and `.d.ts` files (especially nullables)
- Missing `.Value` on nullable types
- Incorrect entity lookups (Id vs IdKey, Person vs PersonAlias)
- `parseInt()` in TypeScript without NaN handling
- `new RockContext()` instead of base class context
- `DateTime` instead of `RockDateTime`
- Incorrect async/await patterns

### 3.3 — Redundancy scan

- Unused bag properties (defined but never populated or consumed)
- Dead code paths
- Duplicate logic between C# and TypeScript
- Block actions that could be folded into initial load

### 3.4 — Pattern violations

- Missing braces on single-line if/for/else
- Not using early returns
- Public visibility where private/internal would suffice
- Missing `PageParameterKey` or `AttributeKey` constants
- Boolean properties that don't start with `Is` or `Has`

### 3.5 — Obsidian-specific checks

- Bag properties use correct types (IdKey `string` for entity references, not raw `int`)
- Block actions return `BlockActionResult` wrapping bags
- Options bag has static config, main bag has dynamic data
- Vue component uses `useInvokeBlockAction`, `useConfigurationValues`, etc.
- Reactive state is correct (no prop mutation, proper `ref`/`computed`)
- Grid columns use correct components for data types (see checklist Section 6)
- Grid columns have `filterValue`/`quickFilterValue` where needed

---

## Phase 4 — Report

**This is the plan.** Present it and wait for the user to approve before making any changes.

### Summary

> **Block:** [Category]/[BlockName]
> **Verdict:** PASS | PASS WITH NOTES | NEEDS FIXES
> **Files reviewed:** [count]
> **Functional parity:** [X] / [Y] matched ([Z] missing, [W] differs)
> **Findings:** [X] critical, [Y] warning, [Z] note

### Functional Parity Table

(Reproduce from Phase 2)

### Findings

Group by severity. Omit empty sections.

#### Critical (must fix)

```
[CRITICAL-1] Short title
File: [path]:[line]
WebForms: [what the original did]
Obsidian: [what the conversion does or doesn't do]
Fix: [specific planned fix]
```

#### Warning (should fix)

```
[WARN-1] Short title
File: [path]:[line]
Detail: [explanation]
Fix: [specific planned fix]
```

#### Note (consider)

```
[NOTE-1] Short title
File: [path]:[line]
Detail: [explanation]
```

**After presenting the report, wait for the user to approve before proceeding to Phase 5.**

If the verdict is PASS (no Critical or Warning findings), you're done — skip Phase 5.

---

## Phase 5 — Fix

**Only after the user approves the report.**

### 5.1 — Implement fixes

For each Critical and Warning finding, in priority order:
1. Implement the fix in the relevant file(s)
2. If a fix spans multiple files (e.g., bag property → C# + TypeScript), make all changes together
3. Verify the fix doesn't break adjacent logic

### 5.2 — Verify fixes

After all fixes:
1. Re-read each modified file to confirm correctness
2. Check fixes don't introduce new issues (e.g., new bag property populated in C# AND consumed in TypeScript)
3. Flag if `.d.ts` files need regeneration (don't modify them directly)

### 5.3 — Summary

```
Fixed [X] / [Y] findings:
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

---

## Examples

```
User: "/review-conversion Core/TagsByLetter"
Flow:
1. Load all Obsidian files + recover WebForms from git
2. Build method map, trace queries/security/navigation
3. Second sweep + completeness gate
4. Code quality review against checklist
5. Present report → user approves → implement fixes → summary
```

```
User: "/review-conversion"  (auto-detect from branch)
Branch: feature-v19-claude-followingbyentitylava
Flow: Extract block name → same as above
```
