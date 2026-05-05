# Conversion Examples — Detail Blocks

End-to-end examples for entity detail block conversions. Two examples: a small detail block (NoteTypeDetail) and a large multi-panel detail block with Phase 1B fan-out (ContentChannelItemDetail).

---

## Example 1: Entity Detail Block (NoteTypeDetail)

### Phase 1 Output

> **Block:** `RockWeb/Blocks/Core/NoteTypeDetail.ascx.cs` (310 lines)
>
> **Classification:** Detail
> - Displays a single `NoteType` entity with view and edit modes
> - Fields: Name, EntityType picker, IconCssClass, Color, UserSelectable, RequiresApprovals, IsActive
> - Uses breadcrumbs (`GetBreadCrumbs` override)
> - Has `IsSystem` guard: system note types show a read-only warning and lock structural fields
> - Edit requires entity-level `EDIT` authorization
> - Entity attributes supported
>
> **Unsupported patterns:** None found.
>
> **Base class:** `RockEntityDetailBlockType<NoteType, NoteTypeBag>` (generic: standard IEntity with full CRUD)
>
> **Canonical reference:** `Rock.Blocks/Engagement/AchievementAttemptDetail.cs` (694 lines)

### Phase 2 Output

> **Design proposal:**
> - Standard detail block with view panel and edit panel
> - `IBreadCrumbBlock`: yes, WebForms block sets breadcrumb to note type name
> - `IsSystem` guard: view panel shows `<NotificationBox>` informational warning; edit panel locks Name and EntityType fields
> - EntityType picker populated via `EntityTypeCache` filtered to entities that support notes
> - Entity attributes supported (`LoadAttributesAndValuesForPublicView/Edit`)
> - `SecurityGrantToken` for attribute permissions
> - Delete blocked for system note types
>
> **Questions:**
> 1. The EntityType picker in WebForms shows all entity types. Should the Obsidian version filter to only entities that support notes, or show all?
> 2. The Color field is a free-text hex input in WebForms. Should this become a `ColorPicker` control in Obsidian?

### Phase 2: User Answers (example)

> 1. Filter to entities that support notes: same behavior as the existing picker in other converted blocks.
> 2. Yes, use a `ColorPicker`.

### Phase 3 Output

> **Plan:**
> - **Type:** Detail: `RockEntityDetailBlockType<NoteType, NoteTypeBag>` with `IBreadCrumbBlock`
> - **Breadcrumbs:** Yes, resolves note type name via `NoteTypeService.GetSelect`
> - **Security:** Entity-level `EDIT` for edit mode; `SecurityGrantToken` for attributes
> - **IsSystem:** `<NotificationBox>` in view panel; locks Name and EntityType in edit panel
> - **Entity attributes:** Yes: `LoadAttributesAndValuesForPublicView/Edit`, `SaveAttributeValues`
>
> **Files to create:**
> 1. `Rock.ViewModels/Blocks/Core/NoteTypeDetail/NoteTypeBag.cs`
> 2. `Rock.ViewModels/Blocks/Core/NoteTypeDetail/NoteTypeDetailOptionsBag.cs`
> 3. `Rock.JavaScript.Obsidian/Framework/ViewModels/Blocks/Core/NoteTypeDetail/noteTypeBag.d.ts`
> 4. `Rock.JavaScript.Obsidian/Framework/ViewModels/Blocks/Core/NoteTypeDetail/noteTypeDetailOptionsBag.d.ts`
> 5. `Rock.Blocks/Core/NoteTypeDetail.cs`
> 6. `Rock.JavaScript.Obsidian.Blocks/src/Core/noteTypeDetail.obs`
> 7. `Rock.JavaScript.Obsidian.Blocks/src/Core/NoteTypeDetail/viewPanel.partial.obs`
> 8. `Rock.JavaScript.Obsidian.Blocks/src/Core/NoteTypeDetail/editPanel.partial.obs`
> 9. `Rock.JavaScript.Obsidian.Blocks/src/Core/NoteTypeDetail/types.partial.ts`
>
> **Files to delete:**
> - `RockWeb/Blocks/Core/NoteTypeDetail.ascx`
> - `RockWeb/Blocks/Core/NoteTypeDetail.ascx.cs`
>
> **Key decisions:**
> - `<fieldset>` root in edit panel: straightforward single-group layout
> - `types.partial.ts` needed for `NavigationUrlKey.ParentPage`
> - EntityType options resolved server-side into `OptionsBag.EntityTypeOptions` (filtered list)
> - `IsSystem` check in `UpdateEntityFromBox` locks Name and EntityType fields even if client sends them
> - `RefreshAttributes` block action included for dynamic attribute support

---

## Example 2: Large Multi-Panel Detail Block (ContentChannelItemDetail)

This example shows the path for a large block: full /working/ tree, Phase 1B fan-out, three checkpoints, and post-conversion archival. Compressed for readability.

### Phase 1A Output

> **Scope:** translation only
> **Figma URL:** none
> **Block:** `RockWeb/Blocks/Cms/ContentChannelItemDetail.ascx.cs` (2014 lines)
> **/working/ folder:** `/working/content-channel-item-detail/`
> **Classification:** Detail
> - Single `ContentChannelItem` entity with view, edit, edit-attributes, child-item modal, version-history modal, and approval workflow modal modes (6 operating modes)
> - Touches 8 entity types: `ContentChannelItem`, `ContentChannel`, `ContentChannelType`, `ContentChannelItemSlug`, `ContentChannelItemAssociation`, `Person`, `PersonAlias`, `Tag`
> - Uses entity attributes (`LoadAttributes` + `SaveAttributeValues`)
> - Has `IBreadCrumbBlock` and structured tag editing
> - Detail forms in WebForms use raw `<fieldset>` + `<div class="row">` (replace with ContentSectionContainer composition)
> - Tabs section uses manual `<ul class="nav-tabs">` (replace with TabbedContent for URL sync)
>
> **Cross-cutting concerns flagged:**
> - View/edit bag split: block reads `RawTemplateContent` and internal `ItemGlobalKey` only on edit, must not leak into view bag
> - Cross-block IdKey: links to `ContentChannelDynamicView.ascx.cs`, still WebForms with `.AsInteger()` parsing, sibling needs idKey support
> - Enum mirror drift: ContentChannelStatus, ContentChannelTypeIncludeTime, read C# source byte-for-byte for the .d.ts
> - Inline styles: 12 `style="..."` instances in the .ascx, defer to /css-cleanup post-conversion
> - Anonymous responses: Save returns a shaped object, promote to a typed response bag
> - Footer-action placement: Save / Cancel currently bound in editPanel, move to root via DetailBlock footer slots
> - v-model adapters: LayoutPicker + StatusPicker need adapters, extract to utils.partial.ts
> - Framework-API rediscovery: custom `BuildSlugSuggestions` overlaps with existing utility
>
> **Base class:** `RockEntityDetailBlockType<ContentChannelItem, ContentChannelItemBag>` with `IBreadCrumbBlock`
> **Canonical reference:** `Rock.Blocks/Engagement/AchievementAttemptDetail.cs`
> **parity-map.md:** 87 rows across 7 traces
> **Phase 1B:** fired (2014 lines > 1000; 6 modes ≥ 3; 8 entities ≥ 4)

### Phase 1B Fan-Out

Eight subagents launched in parallel:

```
✓ State & Logic Cartographer  → state-machine.md (6 states + 14 transitions), logic-graph.md
✓ Data Modeler               → data-model.md (8 entities, view/edit field split, sibling-block scan)
✓ Completeness Sweep         → completeness-analysis.md (3 ViewState flags, 1 hidden modal trigger)
✓ Improvement Analyst        → improvement-analysis.md (14 rows: 2 P0, 8 P1, 4 P2)
✓ Redundancy Detector        → redundancy-report.md (5 rows: 2 dead helpers, 3 framework-API rediscoveries)
✓ Edge-Case Hunter           → edge-cases.md (12 rows)
✓ Obsidian Pattern Reviewer  → obsidian-pattern-analysis.md (TabbedContent, ContentSectionContainer, 4 utils.partial.ts converters)
✓ Test Scenario Deriver      → test-scenarios.md (18 scenarios: 6 happy path, 8 edge, 4 permission)
```

Reconciliation: improvement-analysis.md item I8 ("move tag editing to a child component") and obsidian-pattern-analysis.md §2 ("tag editing as a partial") agreed without conflict.

### Phase 2: Clarifying Questions

> 1. **View/edit field split**, `data-model.md` proposes view-only excludes `RawTemplateContent`, `ItemGlobalKey`, `ApprovalState` history blob. Confirm? **[Proposed: yes]**
> 2. **Sibling block**, `data-model.md` flagged `ContentChannelDynamicView.ascx.cs` (still WebForms, parses `?ItemId=N` with `.AsInteger()`). Update sibling to accept idKey in this PR, or follow-up? **[Proposed: in this PR, small change, blocks the new Obsidian list otherwise]**
> 3. **Tabbed content**, replace manual `<ul class="nav-tabs">` with `<TabbedContent>` (preserves URL sync)? **[Proposed: yes]**
> 4. **Tag editing UI**, currently inline; move to `tagsPanel.partial.obs`? **[Proposed: yes, keeps editPanel under 600 lines]**

User answered: 1 yes, 2 yes, 3 yes, 4 yes.

### Phase 3: Plan

`/working/content-channel-item-detail/plan.md`, 218 lines, structured §1-§6:
- §1, Block summary
- §2, 6 design decisions (cites clarifying-questions.md and obsidian-pattern-analysis.md)
- §3, 16 files to create, 2 to delete, 1 to edit (the WebForms sibling)
- §4, 10 implementation steps
- §5, 3 checkpoints (after C# block; after .obs + partials; final)
- §6, Open issues: none at plan approval

parity-map.md per-trace architectural summary added (1 bullet per trace) above the seven trace tables. Row-by-row "Obsidian Equivalent (planned)" cells stay empty; `/review-conversion` fills them post-implementation.

### Implementation

**Step 6 (C# block written), Checkpoint 1:**
- Read `parity-map.md`, `data-model.md`, `improvement-analysis.md`
- Ran review-checklist §1, §3 (C#), §5 (C# block structure)
- **Verdict: DRIFT FOUND.** improvement-analysis.md I3 (replace `BuildSlugSuggestions` with framework helper) was missed in the C# block. Fixed inline. Re-checkpoint passed.

**Step 8 (.obs + partials written), Checkpoint 2:**
- Read `parity-map.md`, `state-machine.md`, `obsidian-pattern-analysis.md`
- Ran review-checklist §3 (TS/Vue), §5 (Vue component)
- **Verdict: PASS.** Silent.

**Sibling-block edit, framework-edit gate:**
- Pause: "this conversion needs to edit `RockWeb/Blocks/Cms/ContentChannelDynamicView.ascx.cs` (outside the block's own folders) because Phase 2 Q2 confirmed in-scope sibling fix. Proceed?" User confirmed. Edit applied.

**Step 10 (final), Checkpoint 3:**
- Read all /working/ artifacts
- Ran review-checklist §2 Performance, §7 Modernization, full §1 parity completeness
- Cross-cutting checks: view/edit bag split confirmed; cross-block ID format consistent; TS enum values match C# byte-for-byte; deduplication sweep extracted `listItemToContentChannel` to utils.partial.ts (was inline in 2 panels)
- **Verdict: PASS.** Validation script passed.

### Post-conversion (after /review-conversion)

User reports: `/review-conversion` returned PASS. Convert-block offers:

> The /working/content-channel-item-detail/ folder captures the full research. Want me to archive it as a frozen spec under /specs/completed/CMS/?
> 1. Yes, archive now
> 2. Not yet, wait until the PR merges
> 3. No, leave it at /working/

User picks 1.

Compose `/specs/completed/CMS/260504-convert-content-channel-item-detail.md` per `references/conversion-spec-format.md`. Move the /working/ folder to `/specs/completed/CMS/260504-convert-content-channel-item-detail/working/`. Append a row to `/specs/completed/INDEX.md`.

Final tree:

```
/specs/completed/CMS/
├── 260504-convert-content-channel-item-detail.md     ← compacted spec.md
└── 260504-convert-content-channel-item-detail/
    └── working/
        ├── parity-map.md
        ├── state-machine.md
        ├── logic-graph.md
        ├── data-model.md
        ├── completeness-analysis.md
        ├── improvement-analysis.md
        ├── redundancy-report.md
        ├── edge-cases.md
        ├── obsidian-pattern-analysis.md
        ├── clarifying-questions.md
        ├── test-scenarios.md
        └── plan.md
```
