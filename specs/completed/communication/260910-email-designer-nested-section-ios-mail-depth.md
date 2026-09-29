---
author: Joshua Henninger
date_created: 2026-09-10
summary: >-
  Content inside three nested Email Designer sections is dropped by iOS Apple Mail
  because each nesting level adds seven tables. For 20.1, warn and highlight rather
  than cap, so churches can flatten templates on their own schedule. Records the
  MJML evaluation and the structural follow-ups deferred to develop.
contributors: []
---

# Email Designer Nested Section iOS Mail Depth

## Summary

The Email Designer wraps every component in three nested tables and every section in seven, so three nested sections put content at a table depth of 28. iOS Apple Mail silently drops content past a threshold somewhere between 14 and 28, leaving the middle of the email blank while header and footer render. The designer places no limit on nesting and gives no signal that it is risky.

For hotfix 20.1 this spec adds a dismissible warning at the top of the control plus an opt-in highlight of the offending sections. It deliberately does not cap nesting, so existing templates keep working in the editor while churches fix them. It also records why MJML was evaluated and rejected, and which structural reductions are worth pursuing in develop.

## Motivation

GitHub issue [#6995](https://github.com/SparkDevNetwork/Rock/issues/6995) (Asana DEV-15221) reports a reproducible blank-body email on iPhone with a valid, balanced HTML payload. Three years of Email Builder history show roughly eleven client-rendering fixes (Outlook backgrounds, iOS Mail trailing space, image overflow, divider color), so this is a recurring class, not a one-off. During analysis a full MJML migration was considered as a way to retire the class wholesale; the findings belong on record so the question is not re-litigated from scratch.

## Problem Statement

A section nested inside a section nested inside a section produces content at table depth 28. iOS Apple Mail renders nothing at that depth. The same message renders in macOS Apple Mail, Gmail for iOS, and desktop browsers. Nothing in the designer indicates the structure is unsafe.

## Reproduction

1. Create a new Email Designer communication on the stock template.
2. Add a full-width section. Inside it add a second section. Inside that a third. Single column each, no styling.
3. Put a heading and a paragraph in the innermost section.
4. Send to an address read in Apple Mail on an iPhone.

Header and footer render; the middle content area is blank. Preview text confirms the copy is in the payload.

## Root Cause

Every component is built by `createElementWrappers` as a margin-wrapper table containing a border-wrapper table containing a padding-wrapper table (`Rock.JavaScript.Obsidian/Framework/Controls/Internal/EmailEditor/utils.partial.ts:659`). The trio is intentional: it models the CSS box model (independent margin, border, padding) in table markup, and the border-wrapper's `border-collapse: separate` plus `overflow: hidden` is what makes border-radius clip correctly.

A section pays that trio for itself, adds a `section-row` table for its columns, and then each column `<td>` contains a dropzone built by another full trio (`utils.partial.ts:3860`).

| Structure | Tables |
|---|---|
| Chrome (row trio plus outer wrapper) | 4 |
| Each section nesting level (trio 3, section-row 1, column trio 3) | 7 |
| Leaf component trio | 3 |
| Single section, one leaf | 14 (renders per report) |
| Three nested sections, one leaf | 28 (dropped per report) |

The true iOS threshold is unknown inside the band 14 to 28. Two nested sections land at 21, also unknown.

## Affected Code Paths

Primary (where the fix lands):

- `Rock.JavaScript.Obsidian/Framework/Controls/Internal/EmailEditor/emailIFrame.partial.obs` (nesting observer, runtime stylesheet rule, new prop and emit)
- `Rock.JavaScript.Obsidian/Framework/Controls/Internal/EmailEditor/emailDesigner.partial.obs` (prop and emit relay, `cssVariables` allow-list)
- `Rock.JavaScript.Obsidian/Framework/Controls/Internal/EmailEditor/emailEditor.partial.obs` (banner and highlight switch)
- `Rock.JavaScript.Obsidian/Framework/Controls/Internal/EmailEditor/utils.partial.ts` (detection helper and threshold constant)

Secondary (unchanged, but relevant to the deferred work):

- `utils.partial.ts:3851` `getSectionComponentHelper` (section version `v17.3-alpha`, migration shape)
- `utils.partial.ts:735` `findComponentInnerWrappers` (strict three-wrapper reader)
- `emailIFrame.partial.obs:521` `getHtml` (serialization-time cleanup on a clone)

## Workarounds

User-side. Flatten the template so no section sits inside another section, or move the inner content into columns of a single section. Nothing in the product signals this today, which is what the fix addresses.

## Requirements

- The editor MUST warn when any section is nested inside another section.
- The warning MUST appear near the top of the control, MUST use `NotificationBox` with `alertType="warning"`, and MUST be dismissible for the current editing session.
- The warning MUST reappear if the offending count drops to zero and later rises again.
- The warning MUST offer a switch that highlights every offending section in the canvas using the warning color. The switch defaults to on; after that it keeps whatever state the person set for the rest of the editing session, including across the warning hiding and reappearing, so a person who turned it off is not nudged into dismissing the warning.
- Dismissing the warning MUST turn highlighting off, since the switch is inside the banner and would otherwise leave highlights with no visible control.
- Highlighting MUST be runtime-only. No class, attribute, or style used for it may survive `getHtml`, even if the user saves while highlighting is on.
- The editor MUST NOT block, remove, or restructure nested sections in 20.1. Existing templates open and save exactly as before.
- Detection MUST recompute on load and on any structural change (drop, delete, clone, move, replace) without user action.
- The nesting threshold MUST live in a single named constant so a depth bisect can tune it without touching logic.
- The behavior applies in both `template` and `email` usage types.

## Proposed Fix

Detection runs inside the iframe, where the live document is. A document-level `MutationObserver` on `body` (`childList`, `subtree`, batched with `requestAnimationFrame`) mirrors the existing `sectionComponentMutationObserver` lifecycle: created in the load handler after the other observers, disconnected in `onBeforeUnmount`. A helper in `utils.partial.ts` returns every `.component-section` whose count of `.component-section` ancestors meets the threshold. Rows are excluded; they are body chrome. The initial pass runs immediately before the existing `emit("emailDocumentUpdated")` so the banner is correct on open. The iframe emits `nestedSectionsChanged` with the count.

Highlighting toggles a runtime class `${RockRuntimeClassCssClassPrefix}-nested-section` on each offending section's runtime wrapper element (the same element the selection highlight styles; the section table itself before wrapping), driven by a new `isNestedSectionHighlightEnabled` prop (default `false`). The rule lives in the existing `rock-runtime-element` stylesheet at `emailIFrame.partial.obs:803` and paints an absolutely positioned `::before` overlay (2px border plus a translucent fill, `pointer-events: none`, `z-index: 1`) rather than an outline. An inset outline on the table was tried first and was invisible in practice: nested content is positioned and paints over it. The overlay sits above descendants and adds no layout. The wrapper's `::after` is already used for hover labels. `removeTemporaryClasses` strips any class starting with the prefix, `removeTemporaryWrappers` unwraps the wrapper, and `removeTemporaryElements` drops the stylesheet, which is what guarantees nothing leaks into saved HTML.

The color is a fixed warning orange (`#c4631c`, the light-theme value of `--color-warning-strong`) rather than the theme token: the email canvas is always light regardless of Rock's theme, and the dark theme's token is a pale tint that disappears on white.

The banner is the first child of `.email-editor-wrapper` in `emailEditor.partial.obs`. That wrapper is a column flex with `.email-editor` at `flex-grow: 1`, so the alert takes natural height and the canvas fills the rest with no layout change. Inside it an `InlineSwitch` drives the highlight. Dismiss state resets when the count returns to zero.

Copy: heading "Nested sections may not display in some mail apps." Body: "This email has {n} section(s) placed inside other sections. Apple Mail on iPhone and iPad can leave that content blank. Moving the content into fewer nested sections resolves it." Switch label: "Highlight affected sections."

Threshold: any section with at least one section ancestor (two levels, about 21 tables). That depth is unproven either way, so the copy hedges. A false alarm costs a dismissible banner; a miss costs a blank email.

## Fix Risks

- Observer cost. A body-level subtree observer fires on every text edit. Mitigated by rAF batching and by early-exiting when no mutation added or removed an element that is or contains `.component-section`.
- Cascade with selection. Both rules use `outline`; they target different elements, so no conflict. If a future change moves the selection outline onto the component table, the two will need explicit precedence.
- False positives at two levels until the threshold is bisected. Accepted, see Open Questions.
- No behavior change to persisted HTML, so no round-trip or downgrade risk.

## Verification Steps

1. Open a communication with the stock template. No banner appears.
2. Drop a section into a section. Banner appears with count 1 and the highlight switch on; the inner section shows the warning overlay. Toggle off; the overlay clears. Toggle on; it returns.
3. Nest a third section. Count becomes 2; both nested sections highlight.
4. Save with highlighting on. Inspect saved HTML: no `rock-runtime-class-nested-section`, no runtime stylesheet.
5. Delete the nested sections. Banner disappears. Nest one again. Banner returns even if it had been dismissed.
6. Turn highlighting on, then dismiss the banner. The highlights clear with it. Keep editing; the banner stays dismissed until the count returns to zero and rises again.
7. Repeat in a Communication Template (`usageType="template"`).

## Out of Scope

- Capping or preventing section nesting.
- Restructuring persisted HTML to reduce table depth.
- Determining the exact iOS Apple Mail depth threshold.
- Any change to send-time processing (Lava resolution, PreMailer inlining).

## Considered but Rejected

### Migrate the Email Builder to MJML
Rejected for now. MJML would eliminate this bug by construction (sections cannot nest; max depth is wrapper, section, column) and its hardened components map to most of the client-rendering fixes in this control's history. But MJML is a one-way compiler and the editor is a live-DOM WYSIWYG that edits its own output, so adoption means persisting MJML or a JSON model and rewriting drag, drop, and inline editing against it. No MJML component supports margin separately from padding, and `mj-text` supports neither border nor border-radius, so text and title blocks that set margin with a background or border would regress. Lava control flow around structure must be wrapped in `mj-raw`, adding a parse layer to the area with the most recurring Lava-encoding bugs. Nested sections have no MJML representation and would be flattened visibly. `rsvp` and `message` need custom components. The migration path is unusually viable because the adapter layer already extracts typed props from every historical HTML version (`v0` adapters exist for all ten component types), but it is a v22-class initiative, not a hotfix.

### Cap section nesting at drop time
Rejected for 20.1. Zero regression risk and it is the direct MJML lesson, but it does nothing for templates that are already nested and would make them feel broken until edited. Warn first; revisit a cap once churches have had a release to flatten.

### Flag only three-deep sections
Rejected. Matches the reproduced case exactly and avoids false alarms, but leaves two-deep templates unwarned if the real threshold is below 21. The cost asymmetry favors warning early.

### Highlight with `border` instead of `outline`
Rejected. Border changes table box sizing and shifts layout inside the canvas; outline does not. Same reasoning the selection highlight already relies on.

### Reduce table depth structurally in 20.1
Deferred to develop. Even collapsing every trio to two tables and dropping the column trio leaves three nested sections at roughly 14 to 16, right at the edge of the only depth proven safe. Structural collapse buys about one extra nesting level of headroom; it does not make arbitrary nesting safe.

## Open Questions

- Depth bisect. Someone with the reporter's repro should send the stock template with content at depths 16, 18, 21, and 24 to Apple Mail on iOS. The result sets the threshold constant and decides whether the two-level warning is accurate or conservative.

## Potential Future Work

Neither item below is scheduled. They were drafted as standalone specs during this work and folded back here so nothing sits in `specs/` looking in flight. Both build on the depth math above and on the MJML lessons recorded in the rejected evaluation.

### Section depth reduction (develop)

Elide the column dropzone wrapper trio at save time when it carries no inline styles, and rebuild it on load through a new section version, so the strict structure reader and every property panel keep working unchanged. Drops a section level from 7 tables to 4 in the common case.

| Structure | Today | After |
|---|---|---|
| Per section level | 7 | 4 (unstyled column) |
| Single section, one leaf | 14 | 11 |
| Two nested sections | 21 | 15 |
| Three nested sections | 28 | 19 |

Why the column trio is safe to elide: it is created by bare `createElementWrappers` (`utils.partial.ts:3860`) with the unsuffixed classes `margin-wrapper`, `border-wrapper`, `padding-wrapper`, and no stylesheet rule targets those bare classes; leaf components are styled through `-for-{type}` rules instead. The column trio is therefore styled exclusively inline (background color, border radius, padding, alignment from the column property group; column border is not exposed), which makes "does this wrapper do anything" decidable from the element alone.

Persist: in `getHtml`, after the existing temporary-wrapper and temporary-class cleanup runs on the cloned document, replace each `td.section-column` trio that has no inline `style`, no `bgcolor`, and no attributes beyond the fixed `border`, `cellpadding`, `cellspacing`, `width`, `role`, and classes with its `div.dropzone`. The live editor document is never touched. Load: bump the section version past `v17.3-alpha` and, in `getSectionComponentHelper().migrate`, wrap any `td.section-column` whose first child is `div.dropzone` in a fresh trio. Migration already runs unconditionally on load, so saved templates upgrade the first time they open.

Requirements: keep the trio verbatim when anything is styled; persist, load, persist must be byte-identical; no visual change in the editor or in mail clients; Jest coverage for both the elided and the kept case plus idempotence.

Risks: downgrade (an older Rock's `findComponentInnerWrappers` returns `null` for a column without the trio and the column panel breaks; Rock does not support downgrade but the release note should say so) and any plugin that queries `.section-column > .margin-wrapper`, none known in core.

Later phases, scoped only: merge the border and padding wrappers into one table per component (a single `<td>` can carry border, radius, `overflow: hidden`, background, and padding, but this moves the element that carries `bgcolor`, which was itself the subject of #6889, so it needs Litmus or Email on Acid renders first); then Outlook ghost tables inside `<!--[if mso | IE]-->` with `div`-based columns, which rewrites the `small-N` responsive grid and is not a near-term candidate.

Rejected variants: collapsing at serialization without re-inflating on load (the strict reader breaks the column panel on the next open); creating wrappers on demand in the live editor (every `writeLocalProps` binds to the full trio by structure, so the DOM shape would change while the user types); eliding leaf component trios (their wrappers are styled through stylesheet rules as well as inline, so "unstyled" needs a different decision procedure; sections first).

Caveat that keeps this paired with the cap below: collapse buys roughly one extra nesting level of headroom. Even with every trio reduced, three nested sections sit near the only depth proven safe, so it does not make arbitrary nesting safe.

### Section nesting cap (one release after the warning)

Refuse the drop placeholder when a section would land past the threshold, the guard MJML gets from its schema. Applies equally to dropping a new layout, moving an existing section, cloning one, and dragging a saved section from the Sections panel, since those routes share the placement code. Forward-looking only: existing nested sections in loaded templates are never removed, moved, or restructured; the 20.1 warning keeps covering them.

Design: reuse `NestedSectionWarningMinimumAncestorCount` and `countSectionAncestors` so the cap and the warning can never disagree. In the iframe's drag-over placement path (the block that positions `draggingPlaceholderElement`) and in `onIFrameComponentTypeDragDrop`, when the dragged type is a section layout, compute the ancestor count the section would have at the candidate dropzone (the nearest `.component-section` ancestor's count plus one); if it meets the threshold, skip placeholder insertion, mark the dropzone with a runtime class such as `${RockRuntimeClassCssClassPrefix}-drop-refused` for an inline "not allowed" cue, and make the drop a no-op. Nothing is written to the document, so `getHtml` is unaffected and there is no migration.

Timing: no earlier than the release after this warning ships, so churches with nested templates have had a version to flatten them before the structure is blocked outright. If the depth bisect moves the threshold first, the warning moves with it through the shared constant.

Risks: existing three-deep templates stay broken on iOS until edited (the warning is the mitigation); a person who dismissed the warning and then cannot drop where expected may be confused (the inline cue, plus the banner returning whenever the count rises from zero, addresses this); a threshold deeper than two levels would make the cap stricter than necessary until the constant is tuned.

Rejected variants: shipping the cap in 20.1 alongside the warning (no release in between for churches to flatten); capping by measured table depth instead of section nesting level (table depth is an implementation detail that changes as the depth-reduction phases land, and users think in sections); silently flattening nested sections on load (rewrites approved templates without consent).

## Related

- [GitHub issue #6995](https://github.com/SparkDevNetwork/Rock/issues/6995) (open, no comments as of 2026-09-10; requirements live in this spec)
- [Asana DEV-15221](https://app.asana.com/1/20866866924293/project/1208321217019996/task/1217848502343497) (synchronized mirror of the GitHub issue, Version v20.1, Pipeline Stage In Progress)
- [MJML Email Builder Adoption](../../rejected/communication/260910-mjml-email-builder-adoption.md) (full evaluation and rejection record for the MJML alternative)
- [MJML documentation](https://documentation.mjml.io/) and [mjml-browser](https://github.com/mjmlio/mjml/blob/master/packages/mjml-browser/README.md) (reference material for the rejected alternative)
- Prior client-rendering fixes in this control: #7004, #6889, #6754
