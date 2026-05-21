# parity-map.md Template

The functional parity table is the **shared source of truth** between convert-block and `/review-conversion`. Same column structure, same trace dimensions. After conversion, /review-conversion can verify this table against the code 1-to-1 instead of rebuilding it.

This template is a skeleton, not a contract. The model should adapt rows that don't fit the block; do not pad rows that aren't real.

---

## Output location

`/working/{block-name-kebab}/parity-map.md`

---

## Body

Begin with a one-paragraph header: block name, line count of the source `.ascx.cs`, classification (Detail / List / Custom), and a one-sentence summary of what the block does in user terms.

Then a **per-trace architectural summary** section (added during Phase 3 plan-writing). One bullet per trace, each capturing how that trace's rows collapse into Obsidian patterns at the architectural level. This is where pre-implementation thinking lives — patterns, not row-by-row predictions.

Example bullet (real, from a content-channel-item-detail conversion):

> **Trace 1 (Methods M1-M47).** `OnInit`/`OnLoad` collapse into `GetObsidianBlockInitialization` + `GetEntityBagForView`/`GetEntityBagForEdit`. `lbSave_Click` → `Save` block-action with the post-save chain preserved (slugs, tags, personalization, intents, occurrence join, content-collection queue). `lbDelete_Click`/`mdDelete_SaveClick` → `Delete` block-action. Child/parent grid event handlers → grid `@selectItem`/`@orderChanged`/`@click` emits and matching block actions. `_jsScript` drops entirely.

Then produce **one table per trace dimension**. Rows in each table follow the same column structure:

| # | WebForms Method/Behavior | Obsidian Equivalent (planned) | Verdict (planned) |
|---|---|---|---|

- **#** is a stable row identifier within the trace (`M1`, `M2`, ... for Methods; `Q1`, `Q2`, ... for Queries; etc.). Stability matters because checkpoints and `/review-conversion` cite these row IDs.
- **WebForms Method/Behavior** is the source-of-truth description. One short sentence; cite line numbers when helpful (`btnSave_Click, saves entity, redirects to ParentPage (line 247)`).
- **Obsidian Equivalent (planned)** starts empty and **stays empty through plan-writing and implementation**. `/review-conversion` fills it in post-implementation while walking the actual code. (Earlier versions of this template told Phase 3 to fill it row-by-row; that produced 100+ rows of speculative pre-code mapping that anchored the model toward 1:1 translation and went stale once implementation revealed better shapes. The per-trace summary above is where the same architectural thinking now lives, at the right granularity.)
- **Verdict (planned)** stays empty until `/review-conversion` runs (`Matched` / `Differs` / `Missing`).

---

## The seven trace tables

Order matters. Follow this order so that /review-conversion's Phase 2 reads naturally.

### Trace 1: Methods, event handlers, properties

The master list. Walk the `.ascx.cs` top to bottom. Every method, event handler, lambda passed to a `Page_Load`-style hook, every public/protected property, every private helper used more than once. Lifecycle boilerplate (`OnInit`, `OnLoad` with no logic) can be omitted; lifecycle methods *with* logic are rows.

Also include: every `OnClick`, `OnRowSelected`, `OnCommand`, etc. wired in the `.ascx` markup. These are easy to miss if you only walk the `.ascx.cs`.

### Trace 2: Data queries

Every LINQ chain or raw SQL. Capture: filter expressions (`Where`), eager loading (`Include`), ordering (`OrderBy`/`ThenBy`), projection (`Select`), pagination (`Skip`/`Take`), aggregation (`Count`/`Sum`/etc.). One row per query; if a method has two queries, two rows.

### Trace 3: Security checks

Every `IsUserAuthorized`, every entity-level `IsAuthorized`, every `[SecurityAction]` attribute, every admin-only branch. Include the action being checked (`EDIT`, `ADMINISTRATE`, `VIEW`).

### Trace 4: Navigation and page parameters

Every `NavigateToLinkedPage`, `NavigateToParentPage`, `NavigateToPage`, `Response.Redirect`. Every `[LinkedPage]` attribute. Every `PageParameter` read or write. Every breadcrumb-affecting line.

### Trace 5: User preferences and state

Every `PersonPreference` / `UserPreference` read or write. Every grid filter persistence call. Every `ViewState[...]` access (these become Vue refs or are dropped). Every hidden field used to carry state across postbacks.

### Trace 6: UI behaviors

Every panel show/hide, every conditional visibility, every notification (`maNotification.Show`, etc.), every modal popup, every empty-state message, every validation message. The behaviors users see and developers easily skip.

### Trace 7: Second sweep

After Trace 1-6 are drafted, re-read the `.ascx.cs` top to bottom. Capture anything missed:

- Static fields and constants that affect behavior
- Small private helper methods (filtering, formatting, validation)
- Attribute decorations (`[LinkedPage]`, `[ContextAware]`, etc.)
- Property getters/setters with computed logic
- Business rules buried in comments or `#region` blocks
- Behaviors hidden in event-handler defaults (e.g., a grid's `OnRowDataBound` setting visibility)

Trace 7 has its own section. Don't fold it into Trace 1; the separation lets /review-conversion's "second sweep" phase verify the same dimension.

If the second sweep finds nothing, the section is allowed to be a single line: "Second sweep complete; no additional rows."

### Trace 8: Obsidian-only behaviors (only when scope includes new features)

Behaviors the converted Obsidian block will have that the WebForms block does NOT have. Sourced from `new-features.md` (which itself is sourced from the user's prompt and/or `figma-design.md`).

**Skip this trace entirely if scope is "translation only".** Pure translations have no Trace 8 rows.

Format mirrors the other traces but with two adaptations:

| # | WebForms Method/Behavior | Obsidian Equivalent (planned) | Verdict (planned) |
|---|---|---|---|
| T8-1 | (none — new feature, see new-features.md N1) | `commentsPanel.partial.obs` + `ContentChannelItemComment` entity + 2 block actions (`AddComment`, `DeleteComment`); persistence via new EF migration | Implemented |
| T8-2 | (none — new feature, see new-features.md N2) | `useStatusPolling()` composable in `commentsPanel.partial.obs`; 30s interval; cancel on unmount | Implemented |

Adaptations:
- The "WebForms Method/Behavior" cell is filled with `(none — new feature, see new-features.md N{X})`. Cite the new-features.md row by its stable ID.
- The "Verdict (planned)" vocabulary is the same: `Matched` / `Differs` / `Missing` — but here "Matched" means "implemented per the acceptance criteria in new-features.md", not "matches WebForms".

`/review-conversion` reads Trace 8 alongside `new-features.md` to verify acceptance criteria post-conversion.

---

## What goes in /completeness-analysis.md instead

If Phase 1B fires, the Completeness Sweep subagent produces `completeness-analysis.md` with implicit / hidden behavior that the parity map missed (silent error swallowing, ViewState flags, hidden control state, postback timing tricks). That file complements Trace 7; it is *not* a duplicate. Trace 7 is for things the parity-table format already covers; completeness-analysis.md is for things that don't fit the table at all.

If Phase 1B does not fire (small block), Trace 7 carries the second-sweep findings and `completeness-analysis.md` is a stub.

---

## Quality checks

Before considering parity-map.md done:

- [ ] Method count in WebForms ≈ row count in Trace 1 (allowing for omitted lifecycle boilerplate)
- [ ] Every database query has a row in Trace 2
- [ ] Every `IsUserAuthorized` / entity `IsAuthorized` has a row in Trace 3
- [ ] Every `[LinkedPage]` attribute and every `NavigateTo*` call has a row in Trace 4
- [ ] Every preference / state mutation has a row in Trace 5
- [ ] Every notification, validation, modal, panel toggle has a row in Trace 6
- [ ] Trace 7 has run
- [ ] Trace 8 has a row for every in-scope row in `new-features.md` (or "translation only — no Trace 8" stated explicitly)
- [ ] After Phase 3 plan-writing: per-trace architectural summary is present (one bullet per trace covering rows in that trace)

If a count is off, the parity map is incomplete. Fix it before moving to Phase 1B or Phase 2.

---

## Format consistency

The column structure is shared with `/review-conversion`'s Phase 2 output. `/review-conversion` reads this file (if present), writes its row-by-row findings into the empty "Obsidian Equivalent (planned)" and "Verdict (planned)" cells, and saves the file back. Keep the column order intact and the verdict vocabulary (`Matched` / `Differs` / `Missing`) unchanged so the round-trip works without translation.
