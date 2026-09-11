---
author: Joshua Henninger
date_created: 2026-09-10
summary: >-
  Evaluation of replacing the Email Builder's hand-built table markup with MJML.
  It would eliminate the nesting-depth bug class and most Outlook and iOS rendering
  fixes at the source, but MJML is a one-way compiler and the editor is a live-DOM
  WYSIWYG, so adoption means rewriting the editor core and accepting fidelity loss.
contributors: []
---

# MJML Email Builder Adoption

## Summary

MJML compiles a small responsive markup language into email HTML with per-component hardening for Outlook, Apple Mail, and Gmail. This spec evaluates adopting it as the Email Builder's rendering layer, cross-referencing MJML's documented components against the Builder's ten component types and its editing model. The conclusion: MJML solves the client-rendering bug class structurally, but at the cost of the editor architecture, per-component margin and border fidelity, and a new parsing layer over Lava. The structural lessons are worth taking; the framework is not.

## Motivation

Issue [#6995](https://github.com/SparkDevNetwork/Rock/issues/6995) (content dropped by iOS Apple Mail at table depth 28) is the latest of roughly eleven client-rendering fixes in the Email Builder over three years. MJML was noticed late in the original Builder development and never evaluated. With the Builder's adapter layer now mature, the question is whether a migration is feasible and whether it would retire the bug class rather than patch it.

## Requirements

An adoption would need to satisfy all of the following to avoid regressing shipped templates:

- MUST represent every Builder component: row, section (six column layouts), title, text, image, button, divider, video, code, rsvp, message.
- MUST preserve every per-instance property the property panels expose, including independent margin, border, border radius, padding, and background on text and title blocks.
- MUST preserve Lava anywhere it appears today, including control flow wrapped around structural elements.
- MUST round-trip: HTML saved by the current Builder opens, edits, and saves without visible change.
- MUST keep drag and drop, inline `contenteditable` text editing, and property-panel binding working.
- MUST run client-side inside the existing Obsidian control; send-time Lava resolution and PreMailer inlining in `Rock/Communication/EmailTransportComponent.cs:783` stay unchanged.

## Proposed Approach

MJML is a one-way compiler: MJML source becomes HTML, and there is no inverse and no incremental patch API. The editor today edits its own HTML output directly (`contenteditable` on the rendered document, drag targets that are part of the output, property panels mutating output DOM in `writeLocalProps`). Adoption therefore requires:

1. Persisting MJML or a JSON model as the source of truth, in a new column. `CommunicationTemplate.MessageMetaData` is already used for send-time recipient metadata and should not be repurposed. `Message` continues to hold compiled HTML for sending.
2. Rewriting the editor core from "mutate the live DOM" to "mutate the model, recompile, re-render". Property panels port cleanly; drag and drop and inline text editing do not.
3. Registering Rock-specific components (`rsvp`, `message`, video overlay) via `registerComponent`, which does work in `mjml-browser` despite its README only disclaiming `.mjmlconfig` file-based registration.
4. Migrating existing HTML. The adapter layer already does the hard part: `createComponentAdapter` in `Rock.JavaScript.Obsidian/Framework/Controls/Internal/EmailEditor/utils.partial.ts:2063` calls `readLocalProps` to extract a typed props object from any historical DOM shape, and `v0` adapters exist for all ten component types. Existing HTML → `readLocalProps` → typed props → emit MJML is a real pipeline, not a general HTML-to-MJML parser (third-party converters all require manual cleanup).

## Feature Coverage

| Builder component | MJML equivalent | Gap |
|---|---|---|
| row | `mj-body` + `mj-wrapper` | Body border and margin need wrapper workarounds |
| section (6 layouts) | `mj-section` + `mj-column` widths | **Sections cannot nest.** One `mj-wrapper` level only |
| title | `mj-text` with heading markup | No border, no border radius, no margin |
| text | `mj-text` | No border, no border radius, no margin |
| image | `mj-image` | None material; Original/Fixed/Responsive map to width and fluid attributes |
| button | `mj-button` | None material; has padding, inner padding, border, radius |
| divider | `mj-divider` | No border radius |
| video | `mj-image` with `href` | None (Builder video is an `<a><img></a>`) |
| code | `mj-raw` | Passed through untouched; keeps any rendering bugs it has today |
| rsvp | custom component | Two buttons plus group and occurrence data |
| message | custom component | Rock-specific |

Per-property gap that matters most: no MJML component supports `margin` separately from `padding`, and `mj-text` supports neither `border` nor `border-radius`. `TextLocalProps` and `TitleLocalProps` carry `paddingPx`, `marginPx`, `border`, `borderRadiusPx`, and `backgroundColor`. Margin is only visually equivalent to padding when there is no border and no background, so any template combining margin with either would render differently.

## Depth Comparison

Counting real tables (Outlook ghost tables sit inside `<!--[if mso | IE]-->` comments, inert to iOS):

| | Tables at deepest structure |
|---|---|
| Builder, three nested sections | 24 to 28 |
| MJML, wrapper + section + column | 3 to 4 |

MJML uses `<div class="mj-column-per-*">` with media queries instead of nested tables. It does not make the iOS threshold less likely; it makes it unreachable.

## What It Would and Would Not Fix

Client-rendering fixes in the Builder's history that MJML's components address at the source: #7004 (iOS Mail trailing space), #6889 (Outlook `rgb()` backgrounds), #6754 (iOS Gmail trailing space), image overflow and cut-off in Outlook, divider color in Outlook, button styling in Outlook, missing DOCTYPE, and #6995.

Recurring fixes it would not address, and would likely aggravate: roughly ten Lava-encoding commits (#6860, #6759, #6679, the `{% raw %}` warnings, button URL Lava). MJML's XML parser strips template tags between structural elements; the documented workaround wraps every one in `<mj-raw>`. That is an additional failure mode in the area with the most recurring bugs.

## Fix Risks

- Nested sections in shipped templates have no representation and must be flattened, a visible layout change.
- Text and title margin, border, and radius are lost or need custom wrapper components that reintroduce nesting.
- Lava control flow around structure must be detected and wrapped; mispairing breaks compilation.
- `mjml-browser` carries roughly 400 to 900 KiB of non-tree-shakeable v3 compatibility and beautify/minify code.
- MJML output is verbose; Gmail clips at 102 KB. Likely comparable to today's output, unmeasured.
- Migration cannot be silent; it needs per-template opt-in with preview and revert.

## Considered but Rejected

### Use MJML server-side at send time only
Rejected. MJML does not accept HTML input, so there is nothing for it to consume unless the editor already produces MJML. The editor is the constraint, not the transport.

### Store MJML alongside HTML and keep the current editor
Rejected. The two representations drift the moment the user edits the HTML surface; there is no way to fold DOM edits back into MJML source.

### Adopt MJML's structural lessons without MJML
Adopted instead. Bound section nesting, emit only the wrapper tables a component's props require, and move to ghost-table columns over time. Tracked in the #6995 spec and its follow-on structural spec.

## Related

- [Email Designer Nested Section iOS Mail Depth](../../260910-email-designer-nested-section-ios-mail-depth.md), the active spec for the 20.1 warning and the deferred structural work
- [GitHub issue #6995](https://github.com/SparkDevNetwork/Rock/issues/6995)
- [MJML documentation](https://documentation.mjml.io/) (component list and schema constraints, read 2026-09-10)
- [mjml-browser README](https://github.com/mjmlio/mjml/blob/master/packages/mjml-browser/README.md) (client-side limitations)
- [mjml-section source](https://github.com/mjmlio/mjml/blob/master/packages/mjml-section/src/index.js) and [mjml-column source](https://github.com/mjmlio/mjml/blob/master/packages/mjml-column/src/index.js) (rendered structure used for the depth comparison)
- [Browser bundle size, mjml issue #2479](https://github.com/mjmlio/mjml/issues/2479)
- [Templating tags with MJML](https://thoughtbot.com/blog/building-templated-emails-with-mjml) (`mj-raw` workaround)

## Rejection

**Rejected on:** 2026-09-10
**Rejected by:** Joshua Henninger

MJML is a one-way compiler and the Email Builder is a live-DOM WYSIWYG that edits its own output, so adoption means rewriting the editor core (drag and drop, inline editing, model persistence) and accepting fidelity loss on per-component margins and text borders, plus a new parse layer over Lava in the area with the most recurring bugs. That cost is not justified by the client-rendering bugs it would fix. The structural lessons (bounded nesting, conditional wrapper emission, ghost-table columns) are being adopted directly instead. Revisit only if a major version is already committing to a model-based editor.
