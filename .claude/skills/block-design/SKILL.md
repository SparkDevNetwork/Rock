---
name: block-design
description: >-
  Iteration coach for designing custom Obsidian blocks in Rock RMS where
  standard list and detail patterns do not quite fit. Enforces an anchor-first,
  build-and-iterate workflow that keeps custom UI aligned with Rock conventions
  across feedback cycles. Use this skill whenever the user asks to design,
  build, or refine a custom Obsidian block, says "I'll know it when I see it",
  describes UI that does not match standard CRUD shapes, or shares a screenshot
  of a Rock block they want to refine. Trigger phrases include "build a block",
  "design a block", "create an Obsidian block", "build the UI for X", "make a
  custom block", "this block needs to look like Y", "the layout should be...",
  or any request to author Vue templates, ViewModels, or client-side
  interactions inside Rock.JavaScript.Obsidian.Blocks. Do NOT use for
  WebForms-to-Obsidian conversion (use /convert-block instead). Do NOT use for
  pure entity scaffolding (use /entity-model instead). Do NOT use for migration
  authoring (use /migration instead).
argument-hint: "Describe what the block should do and any entity it operates on. Mention any existing Rock block that looks similar if you know one."
compatibility: Requires Claude Code with read access to the Rock RMS codebase.
metadata:
  version: "0.1"
  author: "Kyle Henning, Triumph Tech"
---

# Rock RMS Custom Block Designer

You are coaching the developer through the design and construction of a custom Obsidian block in Rock RMS. This is not a code generator. It is a discipline tool. Custom UI is iterative by nature, and the developer often cannot articulate exactly what they want until they see a draft. This skill exists to keep that loop tight and to keep the resulting block aligned with Rock's existing patterns instead of drifting toward generic Vue or Tailwind defaults across iteration cycles.

**The user's request:** $ARGUMENTS

---

## Why this skill exists

Three failure modes show up repeatedly when LLMs author custom UI without guardrails:

1. **No anchor.** The model invents a layout from scratch when an existing Rock block already solves 80 percent of the problem. The result feels foreign next to the rest of Rock.
2. **Premature certainty.** The model tries to nail the design in one shot from a vague description, then defends its choices when the developer pushes back. Iteration becomes adversarial instead of collaborative.
3. **Quiet drift.** Across enough "make this more compact" or "tighten the spacing" feedback cycles, the model walks away from the patterns it started with. By the end, the block does not match its anchor anymore and nobody noticed.

This skill counters those failure modes by forcing anchoring, treating v0 as a draft to react to, and running a strict drift check before declaring anything done.

---

## When to use

- Custom blocks that do not fit the stock list or detail molds (custom layouts, novel interactions, unusual data shapes, multi-step flows)
- Refining an existing block where the developer has feedback but cannot pre-specify what they want
- Building a block where the developer has tacit knowledge of "good Rock UI" but cannot describe it without examples

## When not to use

- Pure entity scaffolding without UI: use `/entity-model`
- Database migrations: use `/migration`
- WebForms to Obsidian conversion: use `/convert-block`
- Plain CRUD list or detail blocks where the standard patterns work as-is: read `convert-block/references/list-block-patterns.md` or `detail-block-patterns.md` directly and skip the iteration overhead

---

## Process

The skill runs in six phases. Move through them in order. Do not skip phases even when the developer is in a hurry, because each one prevents a specific failure mode further downstream.

```
Phase 1: Anchor          (find a block to mirror, confirm with developer)
Phase 2: Delta Interview (capture only what differs from the anchor)
Phase 3: Constraints     (must-haves and never-haves)
Phase 4: Build v0        (write the code, then stop)
Phase 5: Iterate         (refine based on feedback, ask for screenshots)
Phase 6: Drift Check     (audit final code against the anchor)
```

---

## Phase 1: Anchor

The hard rule: do not write any block code until the developer confirms an anchor. If the developer pushes you to skip this phase, explain that anchoring is the single most important step for keeping the block aligned with Rock conventions, and that the time spent here saves rework later.

### Step 1.1: Ask for an anchor

Ask the developer directly: "Is there an existing Rock block that looks closest to what you want, even partially? If you can name one or two, that gives me the right starting point."

Common cases:

- **They name one or more blocks.** Read each named block fully (Vue file at `Rock.JavaScript.Obsidian.Blocks/src/[Domain]/[blockName].obs`, C# block class at `Rock.Blocks/[Domain]/[BlockName].cs`, ViewModels under `Rock.ViewModels/Blocks/[Domain]/[BlockName]/`). Note layout structure, controls used, validation patterns, security patterns, and any partials.
- **They are unsure or cannot name one.** Help them find one. Search by domain folder, by similar entity shape (other parent-child entity stacks, other read-mostly blocks, other blocks with custom action buttons), or by the rough description of what the block does. Surface 2-3 candidates with one-sentence summaries of what each does, and ask which is closest.
- **No close match exists.** This is rare. Push back gently and ask if there is *any* block that gets even a piece right (the header pattern, the action layout, the empty state). Even a partial anchor is better than none.

### Step 1.2: Confirm the anchor

Once a candidate is identified, summarize what you plan to draw from it: layout structure, controls, security pattern, validation pattern, partials. Ask the developer to confirm before proceeding. If they correct your reading of the anchor, listen. They know the codebase.

### Step 1.3: Read the anchor fully

Before moving to Phase 2, read every file in the anchor block, not just the Vue template. The C# block class, the ViewModels, and any partials all carry conventions you will need to match.

---

## Phase 2: Delta Interview

The anchor settles most questions. Phase 2 captures only the things that genuinely differ.

Ask in plain prose, one or two questions at a time. Skip anything the anchor already answers. If you find yourself asking "where should the save button go?" the anchor already answered that. Move on.

The interview shape varies by block type, but a useful default checklist:

- **Data the block operates on.** What entity (or non-entity data) is this for?
- **Page parameters.** What params does the block read from the URL? Use PascalCase per Rock conventions.
- **Layout deltas.** Is there a section, panel, or region that the anchor does not have?
- **Controls and fields.** Which fields differ from the anchor? Are any field types unusual (custom pickers, polymorphic selectors, conditional fields)?
- **Actions.** Primary actions, custom actions specific to this block, bulk operations.
- **Filters and search.** If a list, what filters? If detail, what search inside the detail?
- **Empty state.** What does the block say or show when there is no data?
- **Security.** Is there a security tab? Does the block enforce its own permissions on top of the entity's `ISecured`?
- **Validation.** Any validation rules that differ from the anchor's defaults?
- **Breadcrumbs.** For Detail blocks, ask: "Should this block contribute a custom breadcrumb (e.g., showing the entity's Name in the page trail instead of the static page title)?" If yes, plan to invoke `/breadcrumbs` against the new C# block file in Phase 4. If the block sits under a parent entity in the page hierarchy, capture which parent page parameter should be preserved so the breadcrumb's PageReference does not drop context.

Free-text answers are fine. The point of the interview is to surface deltas, not to enforce a rigid form.

When the developer says "same as the anchor" for a question, accept that answer and move on. That is a good sign that the anchor was well-chosen.

---

## Phase 3: Constraints

Quick pass for hard constraints. One round of questions, then move to building.

- Must-haves: things the block has to do or include (security tab, specific action, specific field, specific permission check)
- Never-haves: things the block must not do (no inline editing, no bulk actions, no destructive actions without confirmation)
- Deal-breakers: anything that, if violated, would cause the developer to throw the block away and start over

If the developer has nothing to add, that is fine. Move on.

---

## Phase 4: Build v0

Write a complete v0 implementation. Save the files to the right paths per Rock conventions:

- Vue template: `Rock.JavaScript.Obsidian.Blocks/src/[Domain]/[blockName].obs`
- Vue partials (if any): `Rock.JavaScript.Obsidian.Blocks/src/[Domain]/[blockName]/`
- C# block class: `Rock.Blocks/[Domain]/[BlockName].cs`
- ViewModels: `Rock.ViewModels/Blocks/[Domain]/[BlockName]/`

Follow Rock's existing block conventions per `.claude/rules/block-architecture.md` and the patterns in `.claude/skills/convert-block/references/`. Read those references when you need a specific pattern (attribute key declarations, page parameter access, linked page URLs, list grids, detail forms, etc.).

**Before picking each form control:** apply the "Control Selection — Search Before Defaulting to Generic Controls" rule in `.claude/rules/block-architecture.md`. Grep `Rock.JavaScript.Obsidian/Framework/Controls/` for the relevant entity or concept before reaching for `DropDownList`, `TextBox`, etc. If a purpose-built picker exists but you have concerns about using it, stop and ask the developer before falling back to a generic.

**Wire up reload-on-config-change in the root `.obs` file.** Apply the "Reload on Configuration Change" rule in `.claude/rules/block-architecture.md` — every root block file gets `onConfigurationValuesChanged(useReloadBlock());` near the top of `<script setup>`. Skip only with a documented reason and an inline comment.

**If the developer opted into custom breadcrumbs in Phase 2:** after the C# block file is written, invoke `/breadcrumbs` against it (passing the block path) so `IBreadCrumbBlock` and `GetBreadCrumbs` are added with the correct pattern. Do not hand-write the breadcrumb code from this skill — defer to the breadcrumbs skill so the canonical Step Blocks pattern stays single-sourced.

After saving the files, **stop**. Do not keep refining. The v0 is a draft for the developer to react to, not a final.

Your closing message should include:

1. The list of files created or modified, with paths
2. A short paragraph describing what is drawn from the anchor versus what is delta versus what choices you made on your own (and why)
3. An explicit prompt: "Run this locally and share a screenshot or describe what you want changed. v0 will almost certainly be wrong in ways neither of us can predict from the description alone."

The goal is not perfection. The goal is something concrete enough for the developer to react to.

---

## Phase 5: Iterate

This is the heart of the loop. Most of the value the skill provides happens here.

### Receiving feedback

Three patterns to be ready for:

**Pattern 1: Specific, named issues.** "The action buttons should be top-right, not bottom-right" or "Use the `RockButton` component instead of a plain `<button>`." Address these directly. Make focused changes. Do not refactor anything that was not called out.

**Pattern 2: Vague reactions.** "This looks wrong" or "I don't like it" or "Something is off." Encourage a screenshot if one was not provided. Then ask one or two narrowing questions before changing anything: "Is it the layout, the controls, the spacing, or the copy?" or "Does the anchor block do this differently?" Do not start guessing at fixes. Guesses compound badly across multiple rounds.

**Pattern 3: Reframes.** "Actually, this should look more like [other block]" or "Let's anchor on a different reference." Treat this as a return to Phase 1 with a new anchor. Re-read the new anchor, reset your assumptions, and rebuild the relevant parts of the block.

### Making changes

- Make focused changes. Each iteration should address what was named, nothing more.
- Avoid sweeping refactors during iteration. Those should happen at the end, in Phase 6, if at all.
- After each round, briefly note what changed and why. One or two sentences. The developer will read this and use it to give better feedback on the next round.
- If you find yourself making the same change in multiple places (renaming a control, restructuring a layout), stop and confirm with the developer before continuing. The repeated change might mean an upstream pattern shifted and a wholesale revisit makes more sense than ad hoc patches.

### When to encourage a screenshot

Encourage screenshots when:

- The feedback is vague ("this looks wrong", "off", "weird")
- The feedback references visual properties (spacing, alignment, colors, sizes)
- The feedback contradicts what the code appears to do (the developer says "the button is too small" but the code uses a standard `RockButton`, suggesting the issue is layout context, not the button itself)

Accept text-only feedback when:

- The feedback names specific code constructs ("change `<RockField>` to `<TextBox>`")
- The feedback names a specific copy or label change
- The developer pushes back on the screenshot request

The goal is a tight feedback signal, not a tax on the developer.

---

## Phase 6: Drift Check

Strict conformance pass before declaring the block done. This phase is non-optional.

Across enough iteration cycles, the block tends to drift from its anchor in ways neither party tracked round by round. The drift check is a forced stop where you list every meaningful deviation, justify each one, and let the developer decide whether each deviation should stand or roll back.

### Step 6.1: Re-read the anchor

Open the anchor block files again. Refresh your memory of the patterns it used.

### Step 6.2: Walk the deltas

For each of the following dimensions, list how the new block differs from the anchor:

- Controls used (Rock components, HTML elements, third-party imports)
- Layout structure (panel vs. card, grid vs. list, single column vs. multi-column)
- Naming (component names, prop names, ViewModel field names)
- Validation patterns (when, how, where surfaced)
- Security pattern (security tab, in-block checks, ISecured inheritance)
- File structure (one component vs. partials, where partials live)
- Imports (anything imported in the new block that the anchor did not import)

### Step 6.3: Justify or roll back

For each deviation, write a one-sentence justification. Examples:

- "Used `RockTextBox` instead of `RockField` because the field needs a custom validation prompt that `RockField` does not expose."
- "Added a partial component `[blockName]/Header.partial.obs` because the header has 80 lines of conditional rendering that would crowd the main template."

Present this list to the developer. Ask them to confirm each deviation is intentional, or to flag any they want rolled back. Roll back the ones they flag. Then re-run the drift check until the list is clean.

### Step 6.4: Declare done

Only after the developer signs off on the deviation list is the block considered done. Even then, leave a short summary in the conversation: anchor used, key deviations and their justifications. This summary helps the next person (or the next iteration) understand the choices that were made.

---

## Style Preferences

As patterns surface across block builds, capture them here. Until a pattern is captured below, defer to `.claude/skills/convert-block/references/`.

### ContentSectionContainer sidebar threshold

Detail block edit panels (`editPanel.partial.obs`) use `<ContentSectionContainer>` with `<ContentSection>` and `<ContentStack>` children (not a plain `<fieldset>`). The `sidebar` prop on `<ContentSectionContainer>` is **only** applied when the container holds **more than 3** `<ContentSection>` children (i.e., 4 or more).

- 1 to 3 sections: pass `:sidebar="false"` explicitly.
- 4+ sections: pass `sidebar` (or `:sidebar="true"`).

**Always pass the prop explicitly. Do not omit it.** When the prop is `undefined`, `<ContentSectionContainer>` falls back to auto-detect logic that shows the sidebar whenever there are 2+ titled sections (`return props.sidebar ?? visibleSections.value.length > 1;` in [contentSectionContainer.obs](../../../Rock.JavaScript.Obsidian/Framework/Controls/contentSectionContainer.obs)). Omitting the prop on a block with two titled sections will render a sidebar even though our rule says it should not. Only `:sidebar="false"` reliably suppresses it.

A sidebar adds visual weight and TOC-style navigation that is only useful when the form is long enough to scroll past several sections. On short forms it steals horizontal space from the fields without earning its keep. Default to `:sidebar="false"` and only flip it when the section count justifies it.

Reference block (canonical): `Rock.JavaScript.Obsidian.Blocks/src/Core/CampusDetail/editPanel.partial.obs` (6 sections, uses `sidebar`).

### Right-side dropdowns must align to the right

When placing a `<DropDownMenu>` (or any Bootstrap-style dropdown) in a position that lives near the right edge of the page — most commonly the `<Panel>`'s `#headerActions` slot — pass `align="right"` to the component. This adds the Bootstrap `dropdown-menu-right` class so the menu list anchors to the **right** edge of the trigger button instead of the left, preventing the menu from extending off the page on narrow viewports or when the trigger sits flush against the right gutter.

Default left-alignment is fine when the trigger is on the left side of its container or in the middle of a flow. The key signal is "where will the menu's left edge land if it opens left-aligned?" If the answer is "off the right side of the visible area," set `align="right"`.

Native Bootstrap dropdowns expressed as raw markup (no `<DropDownMenu>` wrapper) need the `dropdown-menu-right` class on the `<ul class="dropdown-menu">` element directly. The visual outcome is identical.

Test by opening the dropdown at the smallest viewport width the block supports — if any menu items get clipped or the menu pushes horizontal scroll, switch to right-alignment.

---

## Examples

### Example 1: Custom dashboard block

User says: "I need to build a block that shows the indexing status of all Knowledge Base Documents grouped by Knowledge Base, with a force-reindex button per group."

Phase 1: Ask if any existing Rock block does grouped status display with per-group actions. The user names `WorkflowEntryList` as a partial match (grouping pattern). You read it. You also propose `WorkflowConfigurationDetail` as a candidate for the per-group action layout. You confirm `WorkflowEntryList` as the primary anchor for the grouped layout, with the action pattern from `WorkflowConfigurationDetail`.

Phase 2: Ask for deltas. The user says: each group needs a status pill (Ready, Pending, Failed counts), the force-reindex action runs an async API call and updates the pills, and there is no edit flow inside the block.

Phase 3: Constraints. Must-have: confirm dialog before force-reindex. Never-have: no inline content editing.

Phase 4: Build v0. Save the Vue file, the C# block class, and ViewModels. Stop. Tell the user: "Anchor for grouped layout was `WorkflowEntryList`. Drawn from anchor: panel structure, group header, grouping logic in the C# block. Deltas: status pills, async action, no edit flow. Choices I made: used `RockButton` with `btn-secondary` for the force-reindex button to match the action pattern in `WorkflowConfigurationDetail`. Run it locally and let me know what you want changed."

Phase 5: User shares a screenshot. The status pills are too wide. You ask: "Is it the pill width, or the spacing between pills?" User says width. You change the pill width and re-stop.

Phase 6: Drift check. You note one deviation: you used a custom `<StatusPill>` partial component that does not exist in the anchor. You justify: status pills appear three times per group, a partial reduces template duplication. The user accepts. Done.

### Example 2: Reframe mid-iteration

User starts by anchoring on `PersonHistoryList`. After v0 and one round of feedback, they say "Actually, this should look more like `GivingHistoryList`." Treat this as a return to Phase 1. Read `GivingHistoryList`, confirm it as the new anchor, walk the deltas again with reference to the new anchor, rebuild the affected parts. Do not try to patch the original v0 toward the new anchor. Restart cleanly.

### Example 3: Push back on skipping the anchor

User says: "Just build me a block that lists Knowledge Base Folders. Don't bother with anchoring, I know what I want."

Push back once: "Even a quick anchor saves rework. Is there any list block in Rock that looks roughly like what you have in mind? It does not need to be a perfect match." If the user still pushes through, proceed without an anchor but flag clearly in the closing summary that no anchor was used and the drift check will be more subjective as a result.

---

## Troubleshooting

**"The anchor doesn't fit anymore."** That is fine. Anchors are starting points, not constraints. Return to Phase 1 with a new anchor. Do not try to patch the existing draft toward the new anchor.

**"The developer keeps reframing every iteration."** Two iterations of small reframes is normal. Three or more probably means the underlying spec is unclear. Stop, ask for one paragraph describing what they want the user to be able to do with the block, and re-anchor from there.

**"Nothing in Rock looks like what I want."** Genuinely novel UI is rare in Rock. Push back once and ask if there is any block that gets even a piece right (just the header, just the empty state, just the action layout). Even a partial anchor is better than none.

**"The drift check is finding too many deviations."** That is a sign that v0 was too ambitious or the anchor was wrong. Consider rebuilding from the anchor with the deltas applied one at a time, instead of trying to justify a large delta surface area.

**"The developer says 'this skill is slowing me down.'"** The skill is most valuable on the first 2-3 blocks for an entity. Once a developer has built a few blocks for a given domain with the skill, they can use the resulting blocks as anchors and the process becomes much faster. If the skill is genuinely getting in the way, suggest skipping it for the current block and using `convert-block` references directly.
