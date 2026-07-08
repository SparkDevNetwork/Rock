# Conversion Examples

End-to-end examples showing expected output for each phase and block type.

---

## Example 1: Entity Detail Block (NoteTypeDetail)

### Phase 1 Output

> **Block:** `RockWeb/Blocks/Core/NoteTypeDetail.ascx.cs` (310 lines)
>
> **Classification:** Detail
> - Displays a single `NoteType` entity with view and edit modes
> - Fields: Name, EntityType picker, IconCssClass, Color, UserSelectable, RequiresApprovals, IsActive
> - Uses breadcrumbs (`GetBreadCrumbs` override)
> - Has `IsSystem` guard — system note types show a read-only warning and lock structural fields
> - Edit requires entity-level `EDIT` authorization
> - Entity attributes supported
>
> **Unsupported patterns:** None found.
>
> **Base class:** `RockEntityDetailBlockType<NoteType, NoteTypeBag>` (generic — standard IEntity with full CRUD)
>
> **Canonical reference:** `Rock.Blocks/Engagement/AchievementAttemptDetail.cs` (694 lines)

### Phase 2 Output

> **Design proposal:**
> - Standard detail block with view panel and edit panel
> - `IBreadCrumbBlock` — yes, WebForms block sets breadcrumb to note type name
> - `IsSystem` guard: view panel shows `<NotificationBox>` informational warning; edit panel locks Name and EntityType fields
> - EntityType picker populated via `EntityTypeCache` filtered to entities that support notes
> - Entity attributes supported (`LoadAttributesAndValuesForPublicView/Edit`)
> - `SecurityGrantToken` for attribute permissions
> - Delete blocked for system note types
>
> **Questions:**
> 1. The EntityType picker in WebForms shows all entity types. Should the Obsidian version filter to only entities that support notes, or show all?
> 2. The Color field is a free-text hex input in WebForms. Should this become a `ColorPicker` control in Obsidian?

### Phase 2 — User Answers (example)

> 1. Filter to entities that support notes — same behavior as the existing picker in other converted blocks.
> 2. Yes, use a `ColorPicker`.

### Phase 3 Output

> **Plan:**
> - **Type:** Detail — `RockEntityDetailBlockType<NoteType, NoteTypeBag>` with `IBreadCrumbBlock`
> - **Breadcrumbs:** Yes, resolves note type name via `NoteTypeService.GetSelect`
> - **Security:** Entity-level `EDIT` for edit mode; `SecurityGrantToken` for attributes
> - **IsSystem:** `<NotificationBox>` in view panel; locks Name and EntityType in edit panel
> - **Entity attributes:** Yes — `LoadAttributesAndValuesForPublicView/Edit`, `SaveAttributeValues`
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
> - `<fieldset>` root in edit panel — straightforward single-group layout
> - `types.partial.ts` needed for `NavigationUrlKey.ParentPage`
> - EntityType options resolved server-side into `OptionsBag.EntityTypeOptions` (filtered list)
> - `IsSystem` check in `UpdateEntityFromBox` locks Name and EntityType fields even if client sends them
> - `RefreshAttributes` block action included for dynamic attribute support

---

## Example 2: List Block (SnippetTypeList)

### Phase 1 Output

> **Block:** `RockWeb/Blocks/Communication/SnippetTypeList.ascx.cs` (290 lines)
>
> **Classification:** List
> - Renders a `Rock:Grid` of `SnippetType` entities
> - Standard CRUD with add/delete buttons
> - Links to SnippetTypeDetail page via `LinkedPageRoute`
> - Auth checks for add/delete visibility
>
> **Unsupported patterns:** None found.
>
> **Base class:** `RockEntityListBlockType<SnippetType>`
>
> **Canonical reference:** `Rock.Blocks/AI/AIAgentList.cs` (simple entity list with auth checks)

### Phase 2 Output

> **Design proposal:**
> - Grid columns: Name, Description, IsPersonalAllowed, IsShared, IsActive
> - Row click navigates to detail page via `NavigationUrlKey.DetailPage`
> - Add button visibility gated by block-level `EDIT` authorization
> - Delete gated by entity-level `EDIT` authorization
>
> **Questions:**
> 1. The WebForms block has a "Help Text" column — carry forward or drop?
> 2. Should the `IsPersonalAllowed` column render as a boolean toggle or text?

### Phase 2 — User Answers (example)

> 1. Keep the Help Text column.
> 2. Render as a `BooleanColumn`.

### Phase 3 Output

> **Plan:**
> - **Type:** List — `RockEntityListBlockType<SnippetType>`
> - **Filters:** Column filters only (small dataset)
> - **Security:** Block-level `EDIT` for add button, entity-level `EDIT` for delete
>
> **Files to create:**
> 1. `Rock.ViewModels/Blocks/Communication/SnippetTypeList/SnippetTypeListOptionsBag.cs`
> 2. `Rock.JavaScript.Obsidian/Framework/ViewModels/Blocks/Communication/SnippetTypeList/snippetTypeListOptionsBag.d.ts`
> 3. `Rock.Blocks/Communication/SnippetTypeList.cs`
> 4. `Rock.JavaScript.Obsidian.Blocks/src/Communication/snippetTypeList.obs`
> 5. `Rock.JavaScript.Obsidian.Blocks/src/Communication/SnippetTypeList/types.partial.ts`
>
> **Files to delete:**
> - `RockWeb/Blocks/Communication/SnippetTypeList.ascx`
> - `RockWeb/Blocks/Communication/SnippetTypeList.ascx.cs`
>
> **Key decisions:**
> - No `gridSettingsModal` needed — small dataset, column filters sufficient
> - `types.partial.ts` needed for `NavigationUrlKey` enum
> - No reorder support (no `Order` column on `SnippetType`)

---

## Example 3: Custom Block (PersonalDevices)

### Phase 1 Output

> **Block:** `RockWeb/Blocks/Crm/PersonalDevices.ascx.cs` (312 lines)
>
> **Classification:** Custom
> - Does not display a standard entity grid or detail view
> - Shows personal device registrations for the context person
> - Has custom block actions for device management
> - No edit/save pattern — uses custom buttons and actions
>
> **Unsupported patterns:** None found.
>
> **Base class:** `RockBlockType`
>
> **Canonical reference:** `Rock.Blocks/Crm/PersonalDevices.cs` (custom block with block actions)

### Phase 2 Output

> **Design proposal:**
> - Standalone block using `RockBlockType` base class
> - Displays device list via custom block action returning data
> - Delete/manage actions via `invokeBlockAction`
> - Context person resolved via `RequestContext`
>
> **Questions:**
> 1. The WebForms block uses context person. Should we also support a `PersonId` page parameter as a fallback?
> 2. Device notifications section — carry forward as-is or simplify the UI?

### Phase 2 — User Answers (example)

> 1. Context person only — no page parameter fallback needed.
> 2. Carry forward as-is.

### Phase 3 Output

> **Plan:**
> - **Type:** Custom — `RockBlockType`
> - **Context:** Person via `RequestContext.CurrentPerson`
>
> **Files to create:**
> 1. `Rock.ViewModels/Blocks/Crm/PersonalDevices/PersonalDevicesOptionsBag.cs`
> 2. `Rock.ViewModels/Blocks/Crm/PersonalDevices/PersonalDevicesResponseBag.cs`
> 3. `Rock.JavaScript.Obsidian/Framework/ViewModels/Blocks/Crm/PersonalDevices/personalDevicesOptionsBag.d.ts`
> 4. `Rock.JavaScript.Obsidian/Framework/ViewModels/Blocks/Crm/PersonalDevices/personalDevicesResponseBag.d.ts`
> 5. `Rock.Blocks/Crm/PersonalDevices.cs`
> 6. `Rock.JavaScript.Obsidian.Blocks/src/Crm/personalDevices.obs`
>
> **Files to delete:**
> - `RockWeb/Blocks/Crm/PersonalDevices.ascx`
> - `RockWeb/Blocks/Crm/PersonalDevices.ascx.cs`
>
> **Key decisions:**
> - No partials — single `.obs` file is sufficient for this block's complexity
> - `ResponseBag` used for the custom block action that returns device data
> - No `types.partial.ts` — no navigation URLs
