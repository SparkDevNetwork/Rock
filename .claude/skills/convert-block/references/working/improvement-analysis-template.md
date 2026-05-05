# improvement-analysis.md Template

The list of inefficiencies, suboptimal patterns, and redesign opportunities the conversion will fix. The conversion is **licensed and expected** to ship better-than-WebForms output; this file is the audit trail of what was improved and why.

This artifact is **always full**, even on small blocks. Legacy code has issues at every size. If you genuinely find nothing, the file says "No improvements identified beyond standard modernization (string interpolation, early returns, null-conditional access). All applied implicitly during code generation."

This template uses guidance language. Soft language ("the model should consider", "default to flag unless...") is intentional, improvements are judgment calls.

---

## Output location

`/working/{block-name-kebab}/improvement-analysis.md`

After the conversion ships, `/review-conversion` appends a `## Verification (review-conversion, ...)` section to this file with row-by-row Applied / Missed / Partial verdicts. That section is review's territory — do not pre-populate it during convert-block phases.

---

## Body

### Improvements

A numbered list. Each row covers one improvement:

```
I1, Replace N+1 query in BindGrid()
   Severity: P1 (perf, runs on every grid rebind)
   Source: parity-map.md Q1
   Current: foreach loop calling new PersonAliasService(rockContext).Get(personAliasId) per row
   Proposed: pre-fetch into a Dictionary<int, PersonAlias> once, look up in the loop
   Rationale: List can grow to 10k+ rows; N+1 produces 10k roundtrips
   Effort: small
```

Required fields per row:
- **#** stable ID (`I1`, `I2`, ..., checkpoints cite these)
- **Severity** one of `P0` (security or broken-feature class), `P1` (pattern violation that triggers manual rewrite later), `P2` (quality smell)
- **Source** which artifact / parity-map row this improvement responds to
- **Current** what WebForms does
- **Proposed** what the conversion will do
- **Rationale** one or two sentences on why
- **Effort** rough estimate (`small` ≤ 30 minutes of work, `medium` ≤ a few hours, `large` enough to ask "is this in scope?")

Categories of improvements to look for are documented in **`references/improvement-checklist.md`**. That file paraphrases the criteria from `review-conversion/references/review-checklist.md` Sections 2 and 7, plus conversion-specific recurring patterns. Use it as the prompt set; do not re-derive categories here.

A condensed cheat sheet of the most common categories:
- **Security (P0, required):** view/edit bag split, cross-block ID mismatch, missing entity-level auth, HTML rendered without encoding.
- **Performance (P1):** N+1 queries, cache misuse, lazy-load traps, in-memory filtering of large collections.
- **Idiomatic Obsidian (P1):** hand-rolled markup vs framework component (TabbedContent, Modal, Panel), `<fieldset>` vs ContentSection on detail blocks, anonymous response objects, entity-actions in child panels instead of root-bound DetailBlock footer slots.
- **Framework rediscovery (P2):** custom helpers that overlap with existing Rock APIs.
- **Modernization (P2):** string interpolation, `?.`/`??`, early returns, dead-code removal.
- **Deferred to /css-cleanup (P1, flag don't fix):** inline styles, hard-coded tokens, raw hex colors.

If a category isn't in the cheat sheet, check `improvement-checklist.md`; if it's not there either, the analyst is welcome to flag it on their own judgment per the "Beyond the existing checklists" note in that file.

### Out-of-scope items

If the analysis surfaces issues that would widen this conversion's blast radius beyond the block (e.g., the same legacy bug exists in three other blocks), do NOT add them as in-scope rows. If `mcp__ccd_session__spawn_task` is available, use it to file the find as a separate task. Otherwise note the spawn here so the user can spin up the follow-up manually:

```
Spawned: "Fix N+1 in PersonAliasService consumers across Cms blocks" (2026-05-04)
```

This keeps `improvement-analysis.md` focused on this block's improvements.

---

## Quality checks

- [ ] Every P0 row is in scope for this conversion (security and broken-feature fixes are required, not optional)
- [ ] Every P1 row has either a planned fix in this conversion OR an explicit deferral with rationale
- [ ] Performance issues from parity-map.md Trace 2 / data-model.md §3 have rows
- [ ] Out-of-scope items are spawned as separate tasks, not silently dropped

---

## What this is NOT

- Not a list of bugs WebForms has that we're carrying forward (those go in `edge-cases.md` if they're carried; `improvement-analysis.md` is for things we're fixing).
- Not a redundancy report (`redundancy-report.md` covers duplicate / dead / hand-rolled-where-utility-exists code).
- Not a license to redesign the user-facing behavior. Improvements preserve the user-visible behavior; redesigns require Phase 2 confirmation.
