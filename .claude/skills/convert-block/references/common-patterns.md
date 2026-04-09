# Common Patterns Reference

Patterns used across all block types during Obsidian conversion.

---

## Project Goal

Convert all remaining WebForms blocks (`.ascx` / `.ascx.cs`) to the Obsidian framework.

---

## Do Not Replicate From WebForms

These patterns appear in WebForms code but must **not** be carried forward. Fix them during conversion.

| WebForms pattern | What to do instead |
|---|---|
| N+1 queries (service calls inside loops) | Pre-fetch into dictionary/list before the loop, or use `.Include()` |
| `new XService( rockContext ).Get( id )` for cached entities | Use `XCache.Get( id )` — see "Cache vs Service" below |
| Deep null-check nesting (`if (x != null) { if (x.Y != null) { ... } }`) | Use `x?.Y?.Z` with `??` fallback or early return |
| String concatenation for building strings | Use `$"interpolation {here}"` |
| Manual `foreach` building a list | Use LINQ `.Select()` / `.Where()` / `.ToList()` when clearer |
| Commented-out code blocks | Delete them — they're in git history if needed |
| `ViewState["key"]` for state management | Vue component state or block config |
| `Page.IsPostBack` / `!IsPostBack` guards | Not needed in Obsidian — remove entirely |
| Magic strings for property/column names | Use `nameof()` where applicable |
| `string.Format( "{0} {1}", a, b )` | Use `$"{a} {b}"` |
| Unused `using` statements / `#region` blocks | Do not carry forward — write clean imports |
| `try { } catch { }` with empty catch | Either handle the exception or add an intentional-ignore comment |

**Rule of thumb:** If you catch yourself writing code that looks like it was written in 2012, stop and check if there's a modern C# / Obsidian way to do it.

---

## Files Created Per Conversion

1. `Rock.Blocks/[Category]/[BlockName].cs` — C# block logic (PascalCase filename)
2. `Rock.JavaScript.Obsidian.Blocks/src/[Category]/[blockName].obs` — Vue SFC (camelCase filename)
3. `Rock.JavaScript.Obsidian.Blocks/src/[Category]/[BlockName]/types.partial.ts` — NavigationUrlKey enum (only if nav URLs used), or other declared types / enums
4. `Rock.ViewModels/Blocks/[Category]/[BlockName]/[BlockName]OptionsBag.cs` — always
5. Other bags as needed: `Bag`, `RequestBag`, `ResponseBag`
6. `.d.ts` placeholder for each bag in `Rock.JavaScript.Obsidian/Framework/ViewModels/Blocks/[Category]/[BlockName]/`
7. Delete `RockWeb/Blocks/[Category]/[BlockName].ascx` and `.ascx.cs`

---

## Folder Locations Quick Reference

| What | Where |
|---|---|
| C# block logic | `Rock.Blocks/[Category]/` |
| C# ViewModels (bags) | `Rock.ViewModels/Blocks/[Category]/[BlockName]/` |
| Obsidian .obs component | `Rock.JavaScript.Obsidian.Blocks/src/[Category]/` |
| Obsidian partial files | `Rock.JavaScript.Obsidian.Blocks/src/[Category]/[BlockName]/` |
| Auto-generated TS types | `Rock.JavaScript.Obsidian/Framework/ViewModels/Blocks/[Category]/[BlockName]/` |
| WebForms files (to chop) | `RockWeb/Blocks/[Category]/` |

---

## Base Class Selection

| Situation | Base Class |
|---|---|
| Standard Rock entity list (IEntity) | `RockEntityListBlockType<TEntity>` |
| Custom data list (non-entity POCO, needs extra fields) | `RockListBlockType<YourPoco>` |
| Standard Rock entity detail (view/edit single entity) | `RockEntityDetailBlockType<TEntity, TBag>` |
| Non-entity detail (custom object, not IEntity) | `RockDetailBlockType` |
| Standalone — no grid, no entity detail | `RockBlockType` |

Use `RockListBlockType<T>` instead of `RockEntityListBlockType<TEntity>` when grid rows need computed data beyond EF projection (e.g., joined counts). Declare a public nested POCO inside the block class. **Reference:** `Rock.Blocks/Engagement/StepTypeList.cs`

---

## GUID Assignment Rules

- **Always generate new GUIDs** — never reuse WebForms GUIDs.
- `EntityTypeGuid` → new GUID for the block's entity type registration.
- `BlockTypeGuid` → new GUID for the block type.
- **Use the script:** `node .claude/skills/convert-block/scripts/generate-guids.js` to generate both GUIDs.

---

## Entity Lookup

```csharp
var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );
```

---

## Cache vs Service

**Use cache classes for read-only lookups.** If a Rock entity has a cache class, prefer it over querying with a service. The WebForms block may have used a service — convert it to cache during the Obsidian migration.

Common cache classes: `DefinedTypeCache`, `DefinedValueCache`, `CampusCache`, `GroupTypeCache`, `EntityTypeCache`, `CategoryCache`, `PageCache`, `PersonAliasCache`, `SiteCache`.

```csharp
// RIGHT — cache for read-only lookups
var definedType = DefinedTypeCache.Get( SystemGuid.DefinedType.SOME_TYPE );
var campuses = CampusCache.All().Where( c => c.IsActive == true );
var definedValue = DefinedValueCache.Get( id );
var items = DefinedTypeCache.Get( definedTypeGuid ).DefinedValues.ToListItemBagList();

// WRONG — service query for data available in cache
var definedType = new DefinedTypeService( RockContext ).Get( guid );
var campuses = new CampusService( RockContext ).Queryable().Where( c => c.IsActive == true ).ToList();
```

**Use service when:** writing/updating entities, performing complex joins not available in cache, or querying entities that don't have a cache class.

---

## RockContext Usage

Prioritize using the `RockContext` inherited from the base class.

---

## Authorization Check

**Block-level security** — for block-wide features (add-button visibility, security column, admin-only buttons):
```csharp
BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
```

**Entity-level security** — for row-level access, detail block edits, delete actions:
```csharp
entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
```

Use entity-level security whenever access depends on the specific record. Use block-level security for block-wide features. Occasionally both are combined with `||`.

**Reference:** `Rock.Blocks/AI/AIAgentList.cs`, `Rock.Blocks/AI/AIAgentDetail.cs`

---

## Navigation — `((Key))` Placeholder

Backend:
```csharp
[NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "EntityId", "((Key))" )
```
Frontend:
```typescript
window.location.href = config.navigationUrls[NavigationUrlKey.DetailPage].replace("((Key))", key);
```

---

## `onConfigurationValuesChanged`

Always include unless there is a specific documented reason not to:
```typescript
onConfigurationValuesChanged(useReloadBlock());
```

---

## `v-for` Key Fallback

When a `v-for` item's key property could be null or undefined, use the **loop index** as the fallback — **never** use `''` (empty string causes duplicate keys and Vue warnings).

```html
<!-- RIGHT — index fallback -->
<div v-for="(item, index) in items" :key="item.idKey ?? index">

<!-- WRONG — empty string causes duplicate keys -->
<div v-for="item in items" :key="item.idKey ?? ''">
```

When using `index` as fallback, you must destructure the index from `v-for`: `(item, index) in items`.

---

## Chopping WebForms

Delete the `.ascx` and `.ascx.cs` files. The old WebForms GUIDs do not need to be preserved.

---

## Common Import Paths (TypeScript)

```typescript
// Block utilities
import { onConfigurationValuesChanged, useConfigurationValues, useInvokeBlockAction,
    useReloadBlock, useEntityDetailBlock, setPropertiesBoxValue } from "@Obsidian/Utility/block";
import { propertyRef, updateRefValue, useVModelPassthrough } from "@Obsidian/Utility/component";
import { alert, confirm } from "@Obsidian/Utility/dialogs";
import { enumToListItemBag } from "@Obsidian/Utility/enumUtils";
import { deepEqual } from "@Obsidian/Utility/util";
import { usePersonPreferences } from "@Obsidian/Utility/block";

// Grid
import Grid, { TextColumn, NumberColumn, BooleanColumn, DateTimeColumn, DateColumn,
    DeleteColumn, ReorderColumn,
    textValueFilter, numberValueFilter, booleanValueFilter, dateValueFilter,
    dateRangeValueFilter } from "@Obsidian/Controls/grid";

// UI Controls
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import TextBox from "@Obsidian/Controls/textBox.obs";
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import Modal from "@Obsidian/Controls/modal.obs";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import AttributeValuesContainer from "@Obsidian/Controls/attributeValuesContainer.obs";
import ValueDetailList from "@Obsidian/Controls/valueDetailList.obs";
import ContentSectionContainer from "@Obsidian/Controls/contentSectionContainer.obs";
import ContentSection from "@Obsidian/Controls/contentSection.obs";
import ContentStack from "@Obsidian/Controls/contentStack.obs";

// Templates
import DetailBlock from "@Obsidian/Templates/detailBlock";
import Block from "@Obsidian/Templates/block";

// ViewModels
import { ListBlockBox } from "@Obsidian/ViewModels/Blocks/listBlockBox";
import { DetailBlockBox } from "@Obsidian/ViewModels/Blocks/detailBlockBox";
import { ValidPropertiesBox } from "@Obsidian/ViewModels/Utility/validPropertiesBox";
import { GridDataBag } from "@Obsidian/ViewModels/Core/Grid/gridDataBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

// Enums
import { AlertType } from "@Obsidian/Enums/Controls/alertType";
import { BtnType } from "@Obsidian/Enums/Controls/btnType";
import { DetailPanelMode } from "@Obsidian/Enums/Controls/detailPanelMode";
import { EntityType } from "@Obsidian/SystemGuids/entityType";
```

---

## Canonical Reference Blocks

Consult these blocks when you're unsure about a specific pattern. They are the source of truth for established conventions — not a required read on every conversion.

| Block Type | C# | Frontend | Key patterns |
|---|---|---|---|
| Simple entity list | `Rock.Blocks/AI/AIAgentList.cs` | `Rock.JavaScript.Obsidian.Blocks/src/AI/aiAgentList.obs` | `RockEntityListBlockType<T>`, `GetGridBuilder()`, authorization checks, 178 lines |
| Entity list with POCO | `Rock.Blocks/Engagement/StepTypeList.cs` | `Rock.JavaScript.Obsidian.Blocks/src/Engagement/stepTypeList.obs` | `RockListBlockType<POCO>`, `GridBuilderGridOptions.LavaObject`, computed counts, preferences |
| Entity detail | `Rock.Blocks/Engagement/AchievementAttemptDetail.cs` | `Rock.JavaScript.Obsidian.Blocks/src/Engagement/AchievementAttemptDetail/` | `RockEntityDetailBlockType`, `IBreadCrumbBlock`, `SecurityGrantToken`, entity attrs, 694 lines |
| Non-entity detail | `Rock.Blocks/Bus/QueueDetail.cs` | `Rock.JavaScript.Obsidian.Blocks/src/Bus/queueDetail.obs` | `RockDetailBlockType`, no entity CRUD |
| Custom / standalone | `Rock.Blocks/Crm/PersonalDevices.cs` | `Rock.JavaScript.Obsidian.Blocks/src/Crm/personalDevices.obs` | `RockBlockType`, custom block actions |
| Server-side filters | `Rock.Blocks/Communication/CommunicationList.cs` | `Rock.JavaScript.Obsidian.Blocks/src/Communication/CommunicationList/` | `PreferenceKey`, `gridSettingsModal`, filter serialization (note: uses `RockBlockType` not a list base class — reference for filter patterns only) |
| gridSettingsModal | (same folder) | `Rock.JavaScript.Obsidian.Blocks/src/Communication/CommunicationList/gridSettingsModal.partial.obs` | Settings modal pattern, `deepEqual` guard |

---

## Enum Management

See **CLAUDE.md → Enums** for file location, namespace, and `[EnumDomain]` attribute rules.

### Using existing enums in TypeScript
```typescript
import { MyEnum, MyEnumDescription } from "@Obsidian/Enums/[Domain]/myEnum";
import { enumToListItemBag } from "@Obsidian/Utility/enumUtils";

const myEnumItems = enumToListItemBag(MyEnumDescription);
```

### Migrating an enum from `Rock/` to `Rock.Enums/`
1. Create it in `Rock.Enums/` with the same values + namespace + `[EnumDomain]`.
2. Delete the old file.
3. Add `[assembly: TypeForwardedTo( typeof( Rock.Model.EnumName ) )]` in `Rock/Properties/AssemblyInfo.cs`.

---

## ListItemBag Conversions

```csharp
// From cache
var item = DefinedValueCache.Get( id ).ToListItemBag();
var campusItems = CampusCache.All().ToListItemBagList();

// Manual
var item = new ListItemBag { Value = entity.Guid.ToString(), Text = entity.Name };

// From query
var items = service.Queryable()
    .Select( e => new ListItemBag { Value = e.Guid.ToString(), Text = e.Name } )
    .ToList();
```

`Value` should be a Guid or IdKey string. Use Guid for picker values unless IdKey is specifically required.
