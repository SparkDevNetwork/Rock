# edge-cases.md Template

Null cases, error paths, boundary conditions, and "what could break" scenarios. The point is to surface things the tests don't cover and the happy path doesn't exercise, so the conversion handles them deliberately rather than accidentally.

This template is a skeleton. Stub allowed when the block is genuinely simple and `parity-map.md` already covers all branches.

---

## Output location

`/working/{block-name-kebab}/edge-cases.md`

After the conversion ships, `/review-conversion` appends a `## Verification (review-conversion, ...)` section to this file with audit verdicts per `E{N}` row (Must / Should / Note). That section is review's territory — do not pre-populate it during convert-block phases.

---

## When to write a full edge-cases.md

Always full when:
- Phase 1B fires (large or multi-mode block)
- The block touches multiple entity types
- The block has user-supplied input (form fields, page parameters not just IDs)
- The block does file I/O, network calls, or other failable side effects
- The block is reachable from multiple entry points (different roles, different page parameters)

Stub allowed (one paragraph) when:
- Block is a small list with one query and no editing
- Block is a custom widget with no user input

---

## Body

A numbered list. Each row covers one edge case:

```
E1, Page parameter "EntityId" missing
   Trigger: user opens detail page without ?EntityId=N
   WebForms behavior: silently redirects to ParentPage (line 145)
   Risk: Obsidian could throw if the redirect isn't preserved
   Plan: detect missing key in OnLoad equivalent; redirect via NavigationUrlKey.ParentPage
   Test: navigate to /Foo/Detail with no key; should land on parent page

E2, Entity not found (deleted between page load and grid refresh)
   Trigger: user with stale grid clicks a row; entity has been deleted by another user
   WebForms behavior: silent return; user sees blank panel
   Risk: blank panel is bad UX
   Plan: ActionNotFound from block action with friendly message; client redirects to parent
   Test: open detail page, delete entity in another tab, click Save in this tab
```

Required fields per row:
- **#** stable ID (`E1`, `E2`, ...)
- **Trigger** the situation that produces the edge case
- **WebForms behavior** what the original does (silent return, exception, redirect, blank panel)
- **Risk** what's bad about the WebForms behavior, OR confirm "no risk; preserve as-is"
- **Plan** what the Obsidian conversion will do
- **Test** how to manually verify (optional but encouraged for non-obvious cases)

Categories of edge cases (use as prompts):

#### Missing inputs

- Page parameter not present
- Page parameter malformed (`?EntityId=foo`)
- Page parameter zero / empty (often used as "Add" sentinel)
- Required form field empty on save
- Required form field whitespace-only

#### Stale data

- Entity deleted between page load and save
- Entity modified by another user between page load and save (concurrency)
- Cache miss/refresh races
- Reference entity deleted (block was on Person X, person was merged into Person Y)

#### Unauthorized access

- User loses permission between page load and save
- User accesses block via direct URL without going through expected navigation
- Block-level vs entity-level auth: which check fires first, what does the user see

#### Boundary values

- Numeric input at min / max / zero / negative (where it shouldn't be)
- String input at empty / max length / unicode / RTL / control characters
- Date input in distant past / future / null / DST transition / leap year
- Collection at zero / one / "many" / huge

#### Network / external failures

- External service times out (gateway, mail, search)
- Database transient failure
- File I/O failure (disk full, permission denied)
- A dependent block / page returns 500

For each: state whether the WebForms behavior is acceptable, and how Obsidian preserves or improves it.

#### State machine corners

For multi-state blocks, every transition has corners:
- What happens if Save fires while a previous Save is still in-flight (double-click)
- What happens if the user switches modes mid-edit with unsaved changes
- What happens if the modal closes via Escape vs click-outside vs Cancel button

Cross-reference `state-machine.md`; rows from there can become rows here.

#### List-block specifics

- Empty grid (no rows)
- Grid with one row
- Grid where all rows match a single filter value
- Grid sort applied to a column that's all-null
- Pagination at page 0 vs page 1 vs page > maxPage
- Reorder when there's only one row

#### Detail-block specifics

- Entity has no attributes vs has many attributes
- Entity has IsSystem=true (which fields lock?)
- Entity attributes include sensitive types (CodeEditor with raw HTML, EncryptedText with secrets)
- Entity has unsaved changes; user navigates away (route guard? warning prompt?)

---

## Quality checks

- [ ] Every page parameter has at least one missing/malformed-input row
- [ ] Every external dependency (service, cache, page) has at least one failure-mode row
- [ ] Every state transition documented in `state-machine.md` has at least one corner row OR is acknowledged as edge-case-free
- [ ] Every row has a Plan (what Obsidian will do); no row is "TBD"

---

## What this is NOT

- Not a list of WebForms bugs. Bugs the conversion will fix go in `improvement-analysis.md`. This file is for things that *aren't* bugs but could become bugs if mis-handled in the conversion.
- Not a test plan. `test-scenarios.md` formalizes which behaviors must verify. Edge cases inform test scenarios but go further, they include "what could break" thinking that doesn't always become a written test.
