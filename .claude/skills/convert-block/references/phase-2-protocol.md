# Phase 2 Protocol — Propose Design and Ask Clarifying Questions

Phase 2 is the bridge between research and the plan. The model presents a design proposal, calls out unsupported patterns and required performance fixes, and asks the user a curated set of clarifying questions whose answers feed Phase 3 (plan).

Read `clarifying-questions.md` (template: `references/working/clarifying-questions-template.md`). This file is the audit trail of every design decision the user makes.

---

## Unsupported pattern callouts

If Phase 1A found unsupported patterns, present them first:

> **Patterns requiring special attention:**
> - `[pattern]`, found at line X. Proposed replacement: [approach]

If the block relies heavily on `System.Web` throughout, flag that it may need redesign rather than line-by-line conversion.

---

## Performance fix proposals

If Phase 1A identified performance issues, present the fix strategy for each:

> **Performance fixes:**
> - N+1 at line X: [pre-fetch into dictionary / use `.Include()` / batch query]
> - Cache misuse at line Y: [replace `XService` with `XCache.Get()`]

These fixes are required, not optional.

---

## Design proposal

Present:
- Block type and base class with reasoning
- Overall architecture (files to create, partials needed)
- Key behaviors being carried forward from WebForms
- Significant improvements being applied (cite `improvement-analysis.md` rows)
- Significant redundancies being dropped (cite `redundancy-report.md` rows)

If scope includes redesign or new features, also present:
- **Frame-to-panel mapping** (cite `figma-design.md` § 5)
- **New features in scope for THIS PR** (cite `new-features.md` rows; the "In-scope for this PR?" column gets confirmed in this phase)

---

## Clarifying questions

**You must ask questions; do not silently assume.** The user would rather answer a quick question than debug a wrong assumption.

### Always ask (unless the answer is 100% unambiguous from the code)

| Topic | Why it matters |
|---|---|
| **View vs edit field split** (detail blocks) | Sensitive fields (API keys, secrets, OAuth credentials) must not leak into view-mode bags. Propose a split based on `data-model.md` and confirm. |
| **Which fields to show on the view panel** (detail blocks) | WebForms often dumps everything; Obsidian should be curated. Propose a list. |
| **Grid columns and ordering** (list blocks) | Column selection directly affects UX. Propose what WebForms had and ask if any should be dropped/added/reordered. |
| **Filter approach** (list blocks) | Column-only vs. server-side filters is an architectural decision. State your recommendation and confirm. |
| **Entity attributes** (detail blocks) | If the entity has `LoadAttributes()` in WebForms, confirm whether attributes should be on view, edit, or both. |
| **Sibling-block ID-format mismatches** | If `data-model.md` flagged a linked-to block that's still on WebForms with `.AsInteger()`, propose updating that sibling to accept idKey AND ask whether to scope it into this PR. |
| **Behaviors that look buggy or intentional** | If WebForms code does something odd (filter by hardcoded ID, unusual sort, hidden field), ask whether it's intentional or a bug to fix. |
| **Figma frame coverage** (only if Figma URL present) | A Figma file often contains frames for related blocks, future state, or marketing comps. Confirm which frames in `figma-design.md` are in scope for this PR. |
| **New features in PR vs follow-up** (only if `new-features.md` is non-empty) | New features inflate PR size. State which `new-features.md` rows ship in this PR and which become follow-up issues. Update the "In-scope for this PR?" column based on the answer. |
| **Carry-forward conflicts with redesign** (only if Figma URL present) | If the Figma design drops a WebForms behavior depicted in the parity-map, confirm row by row whether the drop is intentional. The conversion should never silently abandon WebForms behavior. |

### Infer from the code (don't ask)

| Topic | How to infer |
|---|---|
| Base class selection | Determined by classification, use the table |
| Navigation / linked pages | Copy from WebForms `AttributeKey` and `GetAttributeValue` calls |
| Security model (block vs entity) | Match what WebForms does |
| Breadcrumb implementation | If WebForms has `IBreadCrumbBlock` or sets breadcrumb text, include it |
| IsSystem guard | If entity has `IsSystem` property, include the guard |

### Format

Present questions as numbered items with your proposed answer in brackets. Record both the question and the user's answer in `clarifying-questions.md`.

> 1. **View panel fields**, I'd show Name, Description, IsActive, Campus. WebForms also shows InternalCode and CreatedDate. **[Proposed: exclude both]**
> 2. **Grid filters**, WebForms has 3 server-side filters (Status, Campus, Date Range) that reduce the DB query. Keep all server-side? **[Proposed: yes]**
> 3. **Line 142 oddity**, WebForms filters by `CategoryId == 5` (hardcoded). Intentional or bug? **[Proposed: replace with block setting]**

Wait for the user to answer before continuing to Phase 3.
