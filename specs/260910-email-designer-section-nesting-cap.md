---
author: Joshua Henninger
date_created: 2026-09-10
summary: >-
  Refuse dropping a section past the nesting threshold in the Email Designer,
  one release after the 20.1 warning ships, so churches have had a version to
  flatten templates before the structure is blocked outright.
contributors: []
---

# Email Designer Section Nesting Cap

## Summary

The 20.1 hotfix warns about nested sections and highlights them, but still lets the user build them. This spec adds the guard that MJML gets from its schema: past the threshold, the drop placeholder is refused and the section cannot be placed. It reuses the threshold constant and detection helper from 20.1, touches nothing persisted, and is deliberately scheduled one release after the warning.

## Motivation

Bounded structure is the real fix for the iOS Apple Mail depth class; warnings only inform. The cap was rejected for 20.1 because templates already nested three deep would feel broken until edited, with no release in between for churches to act. Shipping the warning first, then the cap, gives them that window. The cap also protects the headroom won by the depth-reduction spec from being spent on deeper nesting.

## Requirements

- Dragging a section (any of the six layouts) over a dropzone whose resulting nesting would meet or exceed the threshold MUST NOT show a drop placeholder, and dropping there MUST be a no-op.
- The same rule MUST apply to moving an existing section, cloning a section, and dragging a saved section from the Sections panel.
- Existing nested sections in loaded templates MUST NOT be removed, moved, or restructured. The cap is forward-looking only; the 20.1 warning continues to cover existing content.
- The threshold MUST be the same `NestedSectionWarningMinimumAncestorCount` constant used by the warning, so the two never disagree.
- The user SHOULD get a brief inline cue at the refused target (the existing placeholder styling with a "not allowed" state) rather than a modal.
- The cap MUST ship no earlier than the release after the 20.1 warning.

## Design

Detection reuses `countSectionAncestors` from `utils.partial.ts` (exported alongside `findNestedSectionElements`). The guard lives in the iframe's drag-over placement path in `emailIFrame.partial.obs` (the block that positions `draggingPlaceholderElement`, around the `insertBefore` calls near line 730 and the drag-over handling near line 1570), and in `onIFrameComponentTypeDragDrop`. When the dragged component type is a section layout, compute the ancestor count the section would have at the candidate dropzone: the count for the nearest `.component-section` ancestor of the dropzone plus one. If it meets the threshold, skip placeholder insertion and mark the dropzone with a runtime class (`${RockRuntimeClassCssClassPrefix}-drop-refused`) for the cue; on drop, return without inserting. Clone and Sections-panel paths route through the same placement code and inherit the check.

Nothing is written to the document, so `getHtml` is unaffected and there is no migration.

## Fix Risks

- Existing three-deep templates remain broken on iOS until edited. The 20.1 warning is the mitigation and stays in place.
- A user who cannot drop where they expect and has dismissed the warning may be confused. The inline cue addresses this; the warning banner reappears whenever the count rises from zero.
- If the depth bisect shows the real threshold is deeper than two levels, the cap would be stricter than necessary. Because it reads the shared constant, tuning it is a one-line change made once for both features.

## Verification Steps

1. Stock template: drop a section into the body. Allowed.
2. Drop a section into that section's column. With the default threshold (any nested section), refused: no placeholder, drop is a no-op, cue shown.
3. Open a template that already has three nested sections. All render and are editable; the 20.1 banner shows; nothing is removed.
4. Attempt to clone the innermost existing nested section. Refused.
5. Drag a saved section from the Sections panel into a nested column. Refused.
6. Raise the constant by one, rebuild, repeat step 2. Allowed at one level, refused at two.

## Out of Scope

- Changing the warning's copy or behavior.
- Reducing table depth; see the depth-reduction spec.
- Any server-side validation of nesting.

## Considered but Rejected

### Ship the cap in 20.1 with the warning
Rejected. Without a release in between, churches with nested templates get a broken-feeling editor and a warning at the same time, with no chance to flatten first.

### Cap by measured table depth instead of section nesting level
Rejected. Table depth is an implementation detail that changes as the depth-reduction phases land; users think in sections. Nesting level is stable, explainable, and shared with the warning.

### Silently flatten nested sections on load
Rejected. That rewrites approved templates without consent and changes their layout. Warn and block, never restructure.

## Open Questions

- Target release. One after the 20.1 warning is the floor; whether that is 20.2 or 21 depends on the release calendar.
- Whether the bisect result moves the threshold before the cap ships. If so, the warning moves with it.

## Related

- [Email Designer Nested Section iOS Mail Depth](260910-email-designer-nested-section-ios-mail-depth.md), the 20.1 warning this follows
- [Email Designer Section Depth Reduction](260910-email-designer-section-depth-reduction.md), the companion structural change
- [MJML Email Builder Adoption](rejected/communication/260910-mjml-email-builder-adoption.md), where the bounded-structure lesson comes from
- [GitHub issue #6995](https://github.com/SparkDevNetwork/Rock/issues/6995)
