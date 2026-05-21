# new-features.md Template

Captures functionality being added to the Obsidian block that does NOT exist in the WebForms source. New features are a distinct category from improvements (which fix existing behavior) and from carry-forward (which preserve WebForms behavior).

This file is **only produced when scope includes new features** — either the user described new functionality in `$ARGUMENTS`, or `figma-design.md` flagged Obsidian-only behavior. If the conversion is pure translation (with or without redesign), stub this file with "No new features in scope" and continue.

---

## Output location

`/working/{block-name-kebab}/new-features.md`

After the conversion ships, `/review-conversion` appends a `## Verification (review-conversion, ...)` section to this file with row-by-row Applied / Missed / Partial verdicts (cross-referenced against parity-map Trace 8). That section is review's territory — do not pre-populate it during convert-block phases.

---

## Body

### 1. Source declaration

State where the new-feature scope came from:

```
Source signals captured:
- $ARGUMENTS phrasing: "{verbatim text the user wrote}"
- figma-design.md FR3, FR4: behaviors with no WebForms equivalent
- (any attached design doc / spec)
```

If the only source is the Figma design, that's fine — say so. If the only source is the user's prompt, that's also fine — say so. If neither, the file shouldn't exist; it should be a stub.

### 2. Candidate features

A numbered list. Stable IDs `N1`, `N2`, ... so the parity-map's Trace 8, the plan's §2/§4, and `/review-conversion` can all cite the same row.

| # | Feature | Source | In-scope for this PR? | Acceptance criteria |
|---|---|---|---|---|
| N1 | Inline comment editor | Figma FR3 + user prompt | TBD (Phase 2) | User can add/edit/delete comments on items; comments persist via new entity; comments shown in view mode read-only |
| N2 | Real-time status badge | Figma FR1 (annotation: "auto-updates every 30s") | TBD (Phase 2) | Status badge polls every 30s and reflects entity status changes without page reload |
| N3 | Drag-to-reorder field groups | Figma FR2 (interaction notes) | TBD (Phase 2) | User can drag section headers to reorder; order persists per user as a preference |

**Required fields per row:**

- **#** — stable ID (`N1`, `N2`, ...)
- **Feature** — one short sentence stating what the feature does
- **Source** — where the requirement came from (user prompt, specific Figma frame, spec doc)
- **In-scope for this PR?** — starts as `TBD (Phase 2)`. After Phase 2 confirms with the user, becomes `Yes` or `No (deferred to follow-up #issue)`. Drives whether the feature gets implemented in this PR.
- **Acceptance criteria** — concrete, testable behaviors. `/review-conversion` uses these to verify the feature shipped correctly. Avoid vague criteria like "works well"; prefer specific ones like "POST returns 200 with the new comment shape" or "delete button removes the row and shows a toast."

### 3. Acceptance criteria detail (optional, per row)

For features that warrant more than a one-line acceptance criteria, expand below the table:

```
N1 — Inline comment editor
  Acceptance criteria:
  1. New entity: ContentChannelItemComment, FK to ContentChannelItem, audit columns inherited from Model<T>
  2. Add comment: user with EDIT auth on the parent item can post a comment; comment shows immediately
  3. Edit own comment: only the author can edit; non-authors see read-only
  4. Delete: author OR ADMINISTRATE-on-parent can delete; deletion is soft (IsDeleted flag), not hard
  5. View mode: comments render below the entity in chronological order, paginated 20 per page
  6. Permissions: Block-level ADMINISTRATE bypass for moderation
  7. Migration: new entity table + entity type + block setting "AllowComments"
```

Use this section for features whose acceptance criteria don't fit in a single table cell.

### 4. Out-of-scope items (deferred to follow-up)

After Phase 2 the user confirms which features ship in this PR vs. follow up. Features the user defers go here with a clear rationale, not just "no":

| # | Feature | Why deferred | Follow-up |
|---|---|---|---|
| N3 | Drag-to-reorder field groups | Persistence requires a new preference type that isn't in scope for this PR | Spawned task: "Implement field-group reorder preference for {Block}" |

If `mcp__ccd_session__spawn_task` is available, file the deferred feature as a separate task. Otherwise note the spawn here so the user can spin it up manually.

### 5. Cross-references

This artifact connects to several others:

- **`figma-design.md`** — section 6 "Behaviors implied by the design but NOT in WebForms" enumerates Figma-sourced candidates. Every row there must have a row here (or be explicitly out of scope).
- **`parity-map.md` Trace 8** — every in-scope feature here gets a row in Trace 8 with the planned Obsidian implementation. Trace 8 is what `/review-conversion` verifies post-conversion.
- **`plan.md` §2** — design decisions reference these rows when the conversion adds files or partials specifically for new features.
- **`plan.md` §4 Step 7.5** — implementation step for Obsidian-only behaviors cites these rows.

---

## Quality checks

- [ ] Every Figma-sourced candidate from `figma-design.md` § 6 has a row here
- [ ] Every user-prompt-described feature has a row here
- [ ] Every in-scope row has concrete, testable acceptance criteria (not "TBD" beyond Phase 2)
- [ ] Every out-of-scope row has a follow-up task spawned or recorded
- [ ] No row is silently dropped between Figma capture and Phase 2 confirmation

---

## What this is NOT

- Not a list of improvements to existing WebForms behavior (those go in `improvement-analysis.md`).
- Not a list of carry-forward behaviors (those are the parity-map's Traces 1-7).
- Not a license to scope-creep. New features come from explicit signals (user prompt or Figma design); the model should not invent features.
- Not a complete spec. Acceptance criteria here are the verification handle; full requirements (data model, security, etc.) live in `data-model.md` and the relevant design decisions in `plan.md` §2.
