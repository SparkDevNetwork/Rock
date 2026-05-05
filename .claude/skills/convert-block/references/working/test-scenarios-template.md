# test-scenarios.md Template

Behaviors that must verify after the conversion ships. The point is to give `/review-conversion` (and any future bug fixer) a checklist of "did this conversion preserve user-facing behavior X?", written in language a tester or developer can follow without reading the WebForms source.

This is the artifact most likely to be re-read months after the conversion lands. Optimize for that audience.

---

## Output location

`/working/{block-name-kebab}/test-scenarios.md`

---

## When to write a full test-scenarios.md

Always full when:
- Phase 1B fires
- The block has 3+ user-facing actions (Save / Delete / Publish / Deploy / Reorder / etc.)
- The block has multiple operating modes
- The block has user-supplied input that affects branching logic

Stub allowed when:
- Block is a list with a single grid and no inline editing
- Block is a custom widget with one clear interaction

A stub lists the 1-3 happy-path scenarios and notes "no edge-case scenarios distinct from `parity-map.md`."

---

## Body

### Happy paths

A numbered list of the user's primary success flows. Format:

```
T1, Save an existing entity
   Setup: navigate to detail page with valid ?EntityId=N; open Edit; change Name from "Foo" to "Bar"
   Action: click Save
   Expect: notification "Saved" appears; view panel shows "Bar"; URL is unchanged; refresh confirms persistence.

T2, Add a new entity
   Setup: navigate to detail page with ?EntityId=0
   Action: enter required fields; click Save
   Expect: redirect to detail page with new ?EntityId=N; view panel renders with new entity's data.

T3, Delete an entity
   Setup: navigate to detail page with valid ?EntityId=N; click Delete
   Action: confirm delete in modal
   Expect: redirect to ParentPage; deleted entity no longer appears in any list.
```

Required fields per scenario:
- **#** stable ID (`T1`, `T2`, ...)
- **Setup** the prerequisite state, in concrete terms
- **Action** what the user does
- **Expect** what the user observes (UI state) AND what the database shows (where applicable)

### Edge-case paths

For every row in `edge-cases.md` that's behavior-visible, derive a test scenario. Cross-reference the edge-case ID:

```
T4, Save with stale entity (cross-references E2)
   Setup: open detail page with ?EntityId=N; in another tab, delete entity N; back in original tab, edit a field
   Action: click Save
   Expect: notification "This entity has been deleted by another user. You will be redirected." appears for 2s; redirect to ParentPage.

T5, Open detail page with no key (cross-references E1)
   Setup: navigate to detail page route without any query parameters
   Action: page loads
   Expect: silent redirect to ParentPage. (Verify no error notification, no console error, no flash of empty content.)
```

Edge-case scenarios may be terser than happy paths, the WebForms behavior was usually silent, so the assertion is "does the same silent thing happen?".

### Permission scenarios

For each authorization check in `parity-map.md` Trace 3:

```
T6, Block-level EDIT denied
   Setup: log in as a user without EDIT on the block; navigate to the list view
   Expect: Add button is hidden; existing rows show no Edit button.

T7, Entity-level EDIT denied
   Setup: log in as a user with block-level VIEW but not entity-level EDIT on entity N; navigate to detail page for N
   Expect: view panel renders; Edit button is hidden; direct POST to the SaveAction returns 403 (use browser network panel to confirm).
```

### Cross-block / sibling scenarios

For every sibling-block mismatch flagged in `data-model.md` §5, derive a scenario that covers the link:

```
T8, List → still-WebForms detail link (cross-references data-model.md §5 mismatch)
   Setup: navigate to the new Obsidian list block
   Action: click a row to navigate to the still-WebForms detail block
   Expect: detail block loads with the correct entity (idKey was decoded by the updated WebForms parser at MobilePageDetail.ascx.cs:34-38).
```

These scenarios are critical because the bug they catch (the P0 finding F2) is invisible from inside this block alone.

### Performance scenarios (optional)

For every N+1 fix in `improvement-analysis.md`, derive a scenario that the perf is fixed:

```
T9, Grid render time with 1000 rows
   Setup: seed 1000 rows of the entity into a test environment
   Action: load the list block
   Expect: SQL profiler shows < 5 queries (was ~1000 in WebForms due to N+1). UI renders in < 2s.
```

These are harder to automate but valuable for the conversion's release notes.

---

## Quality checks

- [ ] Every primary user flow has a happy-path scenario
- [ ] Every edge-cases.md row that's user-visible has a scenario
- [ ] Every Trace 3 (security) row has a scenario
- [ ] Every sibling-block mismatch has a scenario
- [ ] Scenarios are written in language someone unfamiliar with the code can follow

---

## What this is NOT

- Not unit tests. These are end-to-end behavioral scenarios.
- Not a substitute for `/review-conversion`. /review-conversion verifies code structure; test scenarios verify behavior.
- Not exhaustive. The goal is the most important behaviors, not every possible interaction. If you find yourself writing T20+ on a small block, you're probably over-specifying.
