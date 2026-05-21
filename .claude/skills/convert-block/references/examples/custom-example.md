# Conversion Examples — Custom Blocks

End-to-end example for a custom (non-detail, non-list) block conversion (PersonalDevices).

---

## Example: Custom Block (PersonalDevices)

### Phase 1 Output

> **Scope:** translation only
> **Figma URL:** none
> **Block:** `RockWeb/Blocks/Crm/PersonalDevices.ascx.cs` (312 lines)
>
> **Classification:** Custom
> - Does not display a standard entity grid or detail view
> - Shows personal device registrations for the context person
> - Has custom block actions for device management
> - No edit/save pattern: uses custom buttons and actions
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
> 2. Device notifications section: carry forward as-is or simplify the UI?

### Phase 2: User Answers (example)

> 1. Context person only: no page parameter fallback needed.
> 2. Carry forward as-is.

### Phase 3 Output

> **Plan:**
> - **Type:** Custom: `RockBlockType`
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
> - No partials: single `.obs` file is sufficient for this block's complexity
> - `ResponseBag` used for the custom block action that returns device data
> - No `types.partial.ts`: no navigation URLs
