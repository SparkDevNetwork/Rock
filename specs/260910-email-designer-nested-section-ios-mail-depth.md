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

## Future Work

Deferred to develop, ordered by risk:

1. Elide the column dropzone trio at persist when it carries no inline styles. The column trio is created with bare `margin-wrapper` / `border-wrapper` / `padding-wrapper` classes and no stylesheet rule targets the bare classes, so the decision is purely local. Requires a new section version with load-time re-inflation because `findComponentInnerWrappers` is strict. Saves three tables per nesting level. Unit-testable in Jest for persist, load, persist idempotence.
2. Merge border and padding wrappers into one table per component. Plausible on one `<td>` carrying border, radius, background, and padding, but changes which element carries `bgcolor` in Outlook. Needs client renders before touching.
3. Outlook ghost tables inside MSO conditional comments and `div`-based columns. Rewrites the grid. Not a near-term candidate.

## Related

- [GitHub issue #6995](https://github.com/SparkDevNetwork/Rock/issues/6995) (open, no comments as of 2026-09-10; requirements live in this spec)
- [Asana DEV-15221](https://app.asana.com/1/20866866924293/project/1208321217019996/task/1217848502343497) (synchronized mirror of the GitHub issue, Version v20.1, Pipeline Stage In Progress)
- [Email Designer Section Depth Reduction](260910-email-designer-section-depth-reduction.md) (the deferred structural work, specified for develop)
- [Email Designer Section Nesting Cap](260910-email-designer-section-nesting-cap.md) (the cap, scheduled one release after this warning)
- [MJML Email Builder Adoption](rejected/communication/260910-mjml-email-builder-adoption.md) (full evaluation and rejection record for the MJML alternative)
- [MJML documentation](https://documentation.mjml.io/) and [mjml-browser](https://github.com/mjmlio/mjml/blob/master/packages/mjml-browser/README.md) (reference material for the rejected alternative)
- Prior client-rendering fixes in this control: #7004, #6889, #6754
