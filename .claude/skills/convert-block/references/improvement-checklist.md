# Improvement Checklist

The job description for the **Improvement Analyst** subagent during Phase 1B. Used to populate `/working/{block-name-kebab}/improvement-analysis.md`.

This is a thin pointer file. The detailed criteria live in `review-conversion/references/review-checklist.md` Sections 2 and 7. Do not duplicate. When that checklist evolves, this file's references stay correct.

---

## Subagent prompt template

When SKILL.md spawns the Improvement Analyst subagent, brief it with:

```
Role: Improvement Analyst for the convert-block skill, Phase 1B.

Block: {Category}/{BlockName} ({line count} lines, classified {Detail|List|Custom}).
Source files: RockWeb/Blocks/{Category}/{BlockName}.ascx.cs and .ascx.

Output: write /working/{block-name-kebab}/improvement-analysis.md following
references/working/improvement-analysis-template.md.

Required input reading:
1. The .ascx.cs and .ascx files
2. /working/{block-name-kebab}/parity-map.md (already produced by Phase 1A)
3. references/improvement-checklist.md (this file)

Use review-conversion's review-checklist.md Sections 2 and 7 as the criteria
catalog. Read it once at the start; do not duplicate it into the artifact.

Your job: produce the improvement-analysis.md artifact, with one row per
identified improvement, severity-classified (P0/P1/P2). Cover the categories
below.
```

---

## Categories (cross-referenced to review-checklist.md)

### From Section 2 (Performance and Modernization)

- **N+1 and Query Performance**, service `.Get()` / `.Queryable()` in loops; `new XService(rockContext)` per iteration; missing `.Include()`; large `WHERE IN` lists. Severity P1 default.
- **Cache vs Service**, `DefinedTypeCache` / `DefinedValueCache` / `CampusCache` / `GroupTypeCache` / `EntityTypeCache` / `CategoryCache` opportunities. Severity P1 default.
- **WebForms carry-forward**, `string.Format`, deep null nesting, empty catch, `ViewState`, `IsPostBack`, commented-out code. Severity P2 default; collectively P1 if pervasive.
- **Grid/Template (list blocks)**, wrong column components, missing `filterValue` / `quickFilterValue`, missing `:excludeFromExport`, `v-for :key` empty-string fallback. Severity P1 default.

### From Section 7 (Modernization Checks)

- **Query patterns to modernize**, `.ToList()` followed by in-memory filtering, redundant round-trips, missing IQueryable subqueries. Severity P1.
- **Code patterns to modernize**, string interpolation, `?.` / `??`, early returns, intentional-ignore comments on empty catch. Severity P2.
- **Architecture to modernize**, over-eager re-render in Vue, multiple block actions where one suffices, custom modal show/hide instead of `<Modal>`. Severity P1.

### Conversion-specific patterns (recurring failure modes)

These patterns recur across WebForms-to-Obsidian conversions and map specifically to the Improvement Analyst's scope:

- **View/edit bag split (P0)**, sensitive fields leaking into view-mode bags. Always P0; required fix. (Also enforced as a cross-cutting check in `checkpoint-protocol.md`.)
- **Framework controls vs hand-rolled (P1)**, TabbedContent / Modal / Panel replaced by raw HTML. Flag any `<ul class="nav-tabs">`, hand-rolled modals, hand-rolled panels.
- **ContentSection composition (P1)**, `<fieldset>` + `<div class="row">` instead of ContentSection / ContentStack on detail blocks.
- **Inline styles + design tokens (P1, deferred)**, flag for /css-cleanup post-conversion; do NOT fix in this conversion.
- **Anonymous block-action responses (P1)**, `ActionOk(new { ... })` carrying non-trivial shape; promote to a typed bag.
- **Entity-actions in child panels (P1)**, Save/Cancel/etc. in panels with `@emit` chains instead of root-bound DetailBlock footer slots.
- **Framework-API rediscovery (P2)**, hand-rolled helpers that overlap with existing framework APIs. Note: helper deduplication and v-model adapter duplication go in `redundancy-report.md`, not here.
- **Framework-edit awareness (P2)**, flag any framework file edit that the conversion would need; the framework-edit gate in `checkpoint-protocol.md` fires later.

### Beyond the existing checklists

The Improvement Analyst is encouraged to flag anything else that meets the bar of "the converted block is materially better than the WebForms original". Not every observation is an improvement; not every improvement is in scope. Use judgment, justify each row, defer to Phase 2 when the user's input is needed.

---

## Severity assignments (as a reference)

| Class | Severity | Examples |
|---|---|---|
| Security or broken-feature | P0 | View bag leaks API keys; cross-block ID mismatch breaks navigation |
| Pattern violation that triggers manual rewrite later | P1 | Hand-rolled tabs without URL sync; fieldset edit panel; N+1 query |
| Quality smell that does not break behavior | P2 | `string.Format` instead of interpolation; deep null nesting; commented-out code |

P0 rows are required to fix in this conversion. P1 rows default to fix; explicit deferral with rationale is allowed. P2 rows can be batch-deferred (e.g., "modernization sweep deferred to Stage X"). Document deferrals so `/review-conversion` doesn't flag them as misses.

---

## What NOT to put in improvement-analysis.md

- **Bugs in the original that we're carrying forward.** If WebForms had a bug we're keeping for a reason, document in `edge-cases.md` or `clarifying-questions.md`, not here.
- **Redundancies (duplicate / dead / hand-rolled).** Those go in `redundancy-report.md`. Improvements *change* surviving code; redundancies *delete* code.
- **Style nits the conversion handles automatically.** `nameof()` over magic strings, brace style, var consistency, these are generated correctly by default. Don't pad the analysis with them unless the WebForms code violates them in a way the conversion needs to fix deliberately.

---

## Calibration after each conversion

After `/review-conversion` runs, compare its findings to the rows in `improvement-analysis.md`:
- Issues review-conversion found that the analyst missed → broaden the categories above
- Rows in the analysis that review-conversion didn't flag → either the analyst was right (good) or the criteria were too aggressive (tighten)
- Issues review-conversion flagged that the analyst correctly deferred → confirm the deferral reason still holds
