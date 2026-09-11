---
author: Joshua Henninger
date_created: 2026-09-10
summary: >-
  Reduce the table depth the Email Designer emits per section by eliding the
  unstyled column wrapper trio at save time and re-inflating it on load through a
  new section version. Saves three tables per nesting level with no visual change,
  and establishes the persist/inflate pattern for later wrapper merges.
contributors: []
---

# Email Designer Section Depth Reduction

## Summary

Each section nesting level in the Email Designer costs seven tables, three of which are the column dropzone's margin, border, and padding wrappers. In most templates those three carry no styles at all. This spec removes them from saved HTML when they are unstyled and rebuilds them on load, so the editor's strict structure reader and every property panel keep working unchanged. Nesting depth drops from 7 to 4 tables per level for the common case. Two riskier follow-on phases (merging border and padding wrappers, Outlook ghost tables) are scoped but not committed.

## Motivation

Issue [#6995](https://github.com/SparkDevNetwork/Rock/issues/6995) showed iOS Apple Mail dropping content at table depth 28. Hotfix 20.1 warns about nested sections but leaves the markup alone. The 20.1 spec also established that structural collapse alone cannot make arbitrary nesting safe: even aggressive collapsing leaves three nested sections at 14 to 16 tables, right at the edge of the only depth proven to render. What collapse does buy is headroom, roughly one extra nesting level, which turns two-deep templates (21 tables today) into a comfortable 12 to 15. That is worth having regardless of where the eventual nesting cap lands.

## Requirements

- Saved HTML for a section column MUST omit the column wrapper trio when none of its three tables carries an inline style, `bgcolor`, or non-default attribute.
- Saved HTML MUST keep the trio verbatim when any of those carries a value. No property may be lost.
- Loading HTML with an elided trio MUST rebuild it before any property panel or drag operation runs, so `findComponentInnerWrappers` never sees a column without all three wrappers.
- Persist → load → persist MUST be idempotent, byte-for-byte on the affected markup.
- Rendered output in the editor and in mail clients MUST be visually identical to today for unstyled columns.
- The section component version MUST be bumped so migration is unconditional on load and old markup is upgraded once.
- The elision MUST be covered by Jest tests in `Rock.JavaScript.Obsidian/Tests`.

## Design

### Why the column trio is safe to elide

The column dropzone wrappers are created by bare `createElementWrappers` at `Rock.JavaScript.Obsidian/Framework/Controls/Internal/EmailEditor/utils.partial.ts:3860`, so they carry the classes `margin-wrapper`, `border-wrapper`, and `padding-wrapper` with no `-for-{type}` suffix. Leaf components are styled through stylesheet rules such as `.padding-wrapper-for-button>tbody>tr>td`; no rule anywhere targets the bare classes. The column trio is therefore styled exclusively inline, which makes "does this wrapper do anything" decidable from the element alone. Column properties that can populate it: background color, border radius, padding, and text alignment (`properties/sectionColumnPropertyGroup.partial.obs`). Column border is not exposed, so the border wrapper only ever carries a radius.

### Persist

`getHtml` in `emailIFrame.partial.obs` already clones the document and runs `removeTemporaryWrappers`, `removeTemporaryClasses`, and friends on the clone. Add a pass after those: for each `td.section-column`, if its direct `table.margin-wrapper` and the nested border and padding wrappers have no inline `style`, no `bgcolor`, and no attributes beyond the fixed `border`, `cellpadding`, `cellspacing`, `width`, `role`, and class list, replace the trio with its `div.dropzone`. The live editor document is never touched.

### Load

Bump the section version from `v17.3-alpha` to the next version. In `getSectionComponentHelper().migrate`, add a step that, for each `td.section-column` whose first element child is `div.dropzone` rather than `table.margin-wrapper`, wraps the dropzone in a fresh `createElementWrappers` trio. Migration already runs unconditionally on load (`emailIFrame.partial.obs`, the `isMigrationRequired` / `migrate` calls in the load handler), so every saved template upgrades the first time it is opened.

### Depth effect

| Structure | Today | After phase 1 |
|---|---|---|
| Per section level | 7 | 4 (unstyled column) |
| Single section, one leaf | 14 | 11 |
| Two nested sections | 21 | 15 |
| Three nested sections | 28 | 19 |

### Later phases (scoped, not committed)

**Phase 2: merge border and padding wrappers per component.** A single `<td>` can carry border, radius, `overflow: hidden`, background, and padding. Text and title components today put border and radius on the border-wrapper td and background and padding on the padding-wrapper td (`utils.partial.ts` text adapter, `v19.3`). Merging changes which element carries `bgcolor`, which was itself the subject of the Outlook fix in #6889. Requires Litmus or Email on Acid renders before touching. Saves one table per component and per section level.

**Phase 3: Outlook ghost tables and `div`-based columns.** MJML's pattern: real layout in `<div>`s with media queries, Outlook-only tables inside `<!--[if mso | IE]-->`. Rewrites the responsive grid (`small-N` classes and `display: inline-block` on `<td>`). Not a near-term candidate.

## Fix Risks

- Downgrade. HTML saved by the new version has columns without the trio; an older Rock's `findComponentInnerWrappers` returns `null` and the column property panel breaks. Rock does not support downgrade, but this needs a line in the release note.
- Third-party consumers of saved HTML that query `.section-column > .margin-wrapper` would break. None are known in core; plugins are unlikely to depend on this internal shape.
- Migration cost on load is one `createElementWrappers` per unstyled column, negligible.
- Phase 2 carries real cross-client risk and is gated on renders.

## Verification Steps

1. Jest: build a section with an unstyled column, run the persist pass, assert the trio is gone and `div.dropzone` sits directly in the column td.
2. Jest: build a section with a column that has background color, run the persist pass, assert the trio is untouched.
3. Jest: persist → migrate → persist, assert byte-identical markup for both cases.
4. Editor: open a saved template with nested sections, confirm the column property panel loads and edits background, radius, padding, and alignment.
5. Editor: save with highlighting from the 20.1 warning enabled, confirm no runtime classes leak (unchanged behavior, regression check).
6. Send a two-level nested template to Apple Mail on iOS and confirm content renders (should now be at 15 tables).

## Out of Scope

- Any change to the nesting warning shipped in 20.1.
- The nesting cap; see the companion spec.
- Phases 2 and 3 beyond the scoping above.
- Determining the exact iOS threshold; see the 20.1 spec's Open Questions.

## Considered but Rejected

### Collapse at serialization without re-inflating on load
Rejected. `findComponentInnerWrappers` (`utils.partial.ts:735`) returns `null` if any of the three wrappers is missing, and the column property group reads through it strictly. Saved HTML is what the editor reloads, so the next open would break the column panel.

### Create wrappers on demand in the live editor
Rejected. Every `writeLocalProps` and property control binds to the full trio by structure. Adding and removing wrappers mid-edit as values change means the DOM shape changes while the user types a padding value, and every adapter would need to tolerate the partial shape.

### Elide leaf component trios too
Deferred. Leaf wrappers are styled through `-for-{type}` stylesheet rules as well as inline, so "unstyled" requires consulting the document stylesheet, not just the element. Doable, but a different decision procedure and a larger blast radius. Sections first.

## Related

- [Email Designer Nested Section iOS Mail Depth](260910-email-designer-nested-section-ios-mail-depth.md), the 20.1 warning and the depth math this builds on
- [Email Designer Section Nesting Cap](260910-email-designer-section-nesting-cap.md), the companion behavior change
- [MJML Email Builder Adoption](rejected/communication/260910-mjml-email-builder-adoption.md), source of the structural lessons
- [GitHub issue #6995](https://github.com/SparkDevNetwork/Rock/issues/6995)
