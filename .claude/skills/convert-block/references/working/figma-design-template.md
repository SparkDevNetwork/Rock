# figma-design.md Template

Captures the Figma design provided alongside this conversion. Used when the conversion is **translation + redesign** (a Figma URL was attached to the prompt that invoked the skill). The artifact is the bridge between visual design and Obsidian implementation.

This file is **only produced when a Figma URL was captured in Phase 1A.0**. If no Figma link was present, this artifact does not exist for the conversion.

---

## Output location

`/working/{block-name-kebab}/figma-design.md`

After the conversion ships, `/review-conversion` appends a `## Verification (review-conversion, ...)` section to this file with audit verdicts on each `A{N}` annotation and `FR{N}` frame mapping. That section is review's territory — do not pre-populate it during convert-block phases.

---

## How to populate

The model uses the Figma MCP tools (provided by the user's environment) in this order:

1. `get_design_context` — pull the structured component tree, frames, and layout hierarchy from the linked node.
2. `get_metadata` — capture component names, frame IDs, and page structure.
3. `get_screenshot` — capture visuals for reference. One per major frame; do NOT bulk-load every frame.
4. `get_variable_defs` — extract design tokens (colors, spacing, typography) the design references.
5. `use_figma` / `get_libraries` — for any aspect not covered by the structured tools above.

**Save screenshots to disk; reference them by path.** Do not embed screenshots inline in this artifact. Each screenshot can be 100+ KB and bloats the context window.

Suggested screenshot location: `/working/{block-name-kebab}/figma/{frame-name}.png`

---

## Body

### 1. Source

```
Figma URL: {full URL captured in 1A.0}
File ID:   {extracted from URL}
Page:      {page name}
Frames in scope: {N}
Captured at: {ISO-8601 datetime when figma-design.md was first written}
```

The "captured at" timestamp matters because Figma files change. If the URL points to a moving target (no node-id pin), recording when the design was read prevents future confusion if the design diverges from the converted block.

### 2. Frame inventory

One row per major frame the conversion will implement. Skip detail frames, status icons, and other low-level fragments unless they're being implemented as standalone components.

| # | Frame name | Frame ID | Represents | Screenshot |
|---|---|---|---|---|
| FR1 | "View — Detail" | 1:234 | Read-mode for the entity (replaces WebForms view panel) | `figma/view-detail.png` |
| FR2 | "Edit — Detail" | 1:567 | Edit-mode form (replaces WebForms edit panel) | `figma/edit-detail.png` |
| FR3 | "Comments tab" | 2:891 | New: inline comments per item (no WebForms equivalent) | `figma/comments-tab.png` |

Frame IDs use Figma's stable `nodeId` so `/review-conversion` can re-fetch any frame later if the design changes.

### 3. Component inventory

Map each Figma component to the Obsidian framework control that implements it. The conversion should use the framework component when one exists; only describe a custom build when no framework match is reasonable.

| Figma component | Obsidian control | Notes |
|---|---|---|
| "Button — Primary" | `<RockButton :btn-type="BtnType.Primary">` | — |
| "Status badge" | `<HighlightLabel>` with appropriate variant | Confirm color tokens map |
| "Input — Text" | `<TextBox>` | — |
| "Input — DatePicker" | `<DatePicker>` | — |
| "Section header" | `<ContentSection>` heading slot | Detail blocks default |
| "Tabs" | `<TabbedContent>` | URL sync expected unless design says otherwise |
| "Comment thread" | (no framework match — new partial component) | See `new-features.md` N1 |

If a Figma component has no framework match AND should NOT become a new shared component, note "block-local" so the conversion keeps it scoped to one `.partial.obs`.

### 4. Design-token inventory

Map Figma variables to Rock CSS variables. Values that don't have a Rock equivalent get flagged for `/css-cleanup` post-conversion (where new utility classes / variables can be proposed).

| Figma variable | Rock CSS var | Notes |
|---|---|---|
| `color/text/primary` | `var(--text-color)` | — |
| `color/surface/elevated` | `var(--panel-bg)` | — |
| `color/brand/accent` | `var(--brand-primary)` | — |
| `radius/medium` | `var(--border-radius)` | — |
| `space/4` | `0.5rem` | No Rock var; use raw value or propose new var via `/css-cleanup` |

If the design uses tokens that conflict with existing Rock variables (e.g., a different "primary" hue), call it out. The conversion should NOT silently override the theme.

### 5. Frame-to-panel mapping

How each frame becomes a file. This is the explicit handoff to the plan template's §3.

| Frame | Implements | Target file |
|---|---|---|
| FR1 (View — Detail) | view panel | `viewPanel.partial.obs` |
| FR2 (Edit — Detail) | edit panel | `editPanel.partial.obs` |
| FR3 (Comments tab) | new feature N1, see new-features.md | `commentsPanel.partial.obs` (block-local; new for this conversion) |

If a single frame splits across multiple files (e.g., a frame depicts the whole detail view but its content lives in three partials), one row per target file.

### 6. Behaviors implied by the design but NOT in WebForms

This is the bridge to `new-features.md`. Anything the design depicts that the WebForms block cannot do becomes a candidate new feature. Keep this list short and concrete; it's a hand-off, not the full spec.

| Behavior | Frame source | new-features.md row |
|---|---|---|
| Inline comments per item | FR3 | N1 |
| Status badge updates in real time | FR1 (annotation) | N2 |
| Drag-to-reorder field groups | FR2 (interaction notes) | N3 |

Each row here MUST appear as a row in `new-features.md`. If a candidate has no acceptance criteria yet, write the row in `new-features.md` with `Acceptance criteria: TBD (Phase 2)` rather than dropping it.

### 7. Frames considered out of scope

Sometimes a Figma file contains frames for related blocks, future state, or marketing comps. Be explicit about what is NOT in scope so the user can confirm.

| Frame | Reason out of scope |
|---|---|
| "Mobile — Comments" | Block ships web-only this phase |
| "Settings panel" | Belongs to a sibling block |
| "Empty state — confetti" | Polish; deferred to follow-up |

Phase 2 confirms this list with the user.

---

## Quality checks

- [ ] Figma URL captured and the source section is filled in
- [ ] Every major frame in the file is either inventoried or listed as out-of-scope
- [ ] Every framework-mappable component has a row pointing at the right Obsidian control
- [ ] Design tokens that exist in Rock are mapped; tokens without Rock equivalents are flagged for `/css-cleanup`
- [ ] Every new-feature candidate has a row in `new-features.md` (not just here)
- [ ] Screenshots saved to disk and referenced by path, not embedded inline

---

## What this is NOT

- Not a full design spec. The Figma file IS the spec; this artifact is a structured index into it.
- Not a place for implementation code. Component-to-control mapping is the goal; actual `.obs` markup belongs in the implementation phase.
- Not a place for new feature acceptance criteria. Those go in `new-features.md`.
- Not load-bearing if no Figma URL was provided. Skip this template entirely for translation-only conversions.
