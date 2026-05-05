# Conversion Examples — List Blocks

End-to-end example for an entity list block conversion (SnippetTypeList).

---

## Example: List Block (SnippetTypeList)

### Phase 1 Output

> **Scope:** translation only
> **Figma URL:** none
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
> 1. The WebForms block has a "Help Text" column: carry forward or drop?
> 2. Should the `IsPersonalAllowed` column render as a boolean toggle or text?

### Phase 2: User Answers (example)

> 1. Keep the Help Text column.
> 2. Render as a `BooleanColumn`.

### Phase 3 Output

> **Plan:**
> - **Type:** List: `RockEntityListBlockType<SnippetType>`
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
> - No `gridSettingsModal` needed: small dataset, column filters sufficient
> - `types.partial.ts` needed for `NavigationUrlKey` enum
> - No reorder support (no `Order` column on `SnippetType`)
