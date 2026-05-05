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

## Comments in Converted Code

The WebForms block is a requirements reference for **you**, the author — not a reference for future readers. The converted file should read as if it had been written from scratch against the current Rock/Obsidian patterns. Code comments are not a changelog.

**Default: do not mention WebForms in any comment or XML doc.** A reader opening the Obsidian file a year from now will not have the `.ascx.cs` in front of them, and phrases like "matches WebForms", "mirrors the WebForms behavior", or "preserves NavigateToParentPage semantics" add no information — they just rot.

### Forbidden phrasings (delete on sight)

- `// matches the WebForms …`
- `// mirrors the WebForms …`
- `// preserves the WebForms … semantics`
- `// (matches WebForms [SomeMethodName] behavior)`
- `// replaces the WebForms lazy-loaded …`
- `/// Matches the original WebForms condition.`
- References to specific WebForms symbols: `btnSave_Click`, `ShowReadonlyDetails`, `_ppContact_SelectPerson`, `Page.IsValid`, `NavigateToParentPage`, `NavigateToCurrentPage`, `RegistrationInstanceEditor`, etc.
- File-line citations of the original: `RegistrationInstanceDetail.ascx.cs:283-293`

When you catch yourself writing any of these, rewrite the comment to explain the **intent** without the comparison. If you can't write a non-WebForms version that adds value, delete the comment entirely — most of these are narrating behavior that the code already expresses.

### When referencing WebForms is OK

Only mention WebForms when a future reader genuinely needs that context to understand the code. This is rare. Examples:

- **Documented, surprising divergence.** "Returns null on missing key instead of seeding an Add form — matches the original's hidden-panel behavior. If you change this, update the Add-link callers that pass `RegistrationInstanceId=0`."
- **Root-cause context for a bug fix carried into the conversion.** "The original had an N+1 here that crashed on instances with >1k registrations (issue #6722); this replacement query is intentionally different."
- **A quirky business rule whose origin is non-obvious and not documented elsewhere.** Reference the original file and a one-line reason, or — better — move the explanation into the release-note commit.

**In every other case, comment the intent, not the provenance.** Well-named methods, small functions, and direct code are the primary way to communicate; a WebForms reference is a worse substitute.

### Before/after examples

| Before (delete) | After (keep or drop entirely) |
|---|---|
| `// Sessions auto-enable when MaxAttendees is set — matches WebForms RegistrationInstanceEditor.GetValue().` | `// Session timeout auto-enables when MaxAttendees is set.` |
| `/// Matches the original WebForms condition.` | *(delete — the method name already says what it returns)* |
| `/// Replaces the WebForms lazy-loaded Registrations.Any(...) check with an explicit query.` | *(delete)* |
| `// ParentPage URL carries RegistrationTemplateId (matches WebForms NavigateToParentPage).` | `// ParentPage URL carries RegistrationTemplateId so cancel/delete redirects stay scoped to the template.` |
| `Reason: Preserves the WebForms Copy semantics using the bag layer instead of a deep entity clone.` | `Reason: Carry attributes + picker selections forward on Copy without the navigation-null trap of a deep entity clone.` |

### JSDoc style for `.obs` and `.ts` files

Functions, computed values, watchers, and exported types in the new `.obs` / `.ts` files get a lightweight JSDoc when the intent is non-obvious from the name alone. Default to *adding* one for any function or computed value the reader would otherwise have to read 10+ lines of body to understand; default to *omitting* one for a one-liner whose name already says everything (e.g. `entityKey = computed(() => bag.value?.idKey ?? "")`).

**Required format — multi-line, even for short descriptions:**

```ts
/**
 * Destination status options, excluding the current status.
 */
const statusItems = [...];
```

**Forbidden — single-line `/** ... */`:**

```ts
/** Destination status options, excluding the current status. */ // ← do not write this
```

The single-line form is harder to scan in a long `<script setup>` block and inconsistent with how the rest of the Obsidian codebase documents its functions. Even when the description is one short sentence, use three lines: opening `/**`, ` * description`, closing ` */`.

Other rules that fold into the same style:

- **One sentence by default.** Two if the second clarifies a non-obvious branch or invariant; rarely more.
- **Document intent, not mechanics.** `// computes the slug prefix` is worse than the function name; `// switches to date-only output when the channel type does not include time` adds value.
- **No `@param` / `@returns` blocks** unless the parameter or return shape is non-obvious from the type signature. The TypeScript types are the contract; JSDoc is for the *why*.
- **Apply the `/working/`-artifact rule (below) inside JSDoc, too.** A JSDoc that says "Source values come from data-model.md" is the same anti-pattern as a code comment that says it.
- **Consistency with existing files in the same block.** If a partial uses JSDoc on its watchers, the others should too. Mixed styles within a single block read as half-finished.

### Forbidden: /working/ artifact identifiers

`/working/` is research scaffolding. Its row IDs (`N1`, `N2`, `I12`, `Q3`, `B1`, `S10`, `T8-1`, audit codes, etc.) and filenames (`improvement-analysis.md`, `parity-map.md`, `new-features.md`, `figma-design.md`, `obsidian-pattern-analysis.md`, `redundancy-report.md`, `state-machine.md`) are private to the conversion process. They MUST NOT appear in any committed production file (C#, `.obs`, `.ts`, `.cs` bag files, `.d.ts`). After `/review-conversion` passes the folder is archived under `/specs/completed/` or deleted; once that happens these identifiers become orphan citations a future reader can't resolve.

Forbidden patterns (delete or rewrite on sight):

- `// I13: batch hierarchy lookup …`
- `// Per N2 each section's default expansion …`
- `// Tag flush. Q3 framework hook …`
- `/// (improvement-analysis.md I16).`
- `// Per audit B1 / B2 / B10 we keep the WebForms pills.`
- `// parity-map Trace 4 …`, `// new-features.md row N3 …`, `// figma A2 collapsed-on-edit …`

The fix is the same as the WebForms-reference rule: **rewrite to explain the intent or the underlying constraint**, not the bookkeeping ID. If the reason boils down to "the analysis said so", drop the comment entirely.

| Before (delete) | After (keep or drop entirely) |
|---|---|
| `// I13: batch hierarchy lookup. Resolve every hierarchy key plus the current item in a single round trip.` | `// Resolve every hierarchy key plus the current item in a single round trip; one query per hop fanned out as the hierarchy deepened.` |
| `// Per N2 each section's default expansion is set independently at mount.` | `// Each section's default expansion is set independently at mount: Add → all open; Edit → only Content Body + Attributes open.` |
| `/// (improvement-analysis.md I16).` | `/// Re-checks EDIT on the child before cascading the delete; the parent grid is filtered only by VIEW.` |
| `// Q3 framework hook hands back queued names.` | `// Queued names come from the TagList control's flushTags() expose so the same Save action handles entity + tags atomically.` |
| `// Per audit B1 / B2 / B10 we keep the WebForms pills.` | `// Status, channel name with icon, and any linked event-occurrence pills appear in the panel header.` |

When in doubt, ask: "If a future reader greps the repo for `I12`, will they find anything that explains it?" If the answer is no — and after archival it is no — the ID does not belong in the code.

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

The chop happens at Rock startup, **without a migration**. `BlockTypeService.StagePossibleMigrateWebFormsToObsidianBlock` (called during `RegisterEntityBlockTypes`) finds existing WebForms `BlockType` rows whose `Guid` matches a new entity-based block class's `[BlockTypeGuid]` attribute, swaps `EntityType` over to the new entity, clears `Path`, and re-points every `Block` instance on every page to the now-Obsidian `BlockType`. Existing pages get the new block on the first restart after the C# class lands.

For this to work, the new C# class must reuse the WebForms block's existing `Guid` on the `[BlockTypeGuid]` attribute.

Pattern (verbatim, including the comment for traceability):

```csharp
[Rock.SystemGuid.EntityTypeGuid( "<newly-generated-entity-type-guid>" )]
// was [Rock.SystemGuid.BlockTypeGuid( "<newly-generated-block-type-guid>" )]
[Rock.SystemGuid.BlockTypeGuid( "<existing-webforms-block-type-guid>" )]
public class FooDetail : RockEntityDetailBlockType<...>
```

Rules:

- **`EntityTypeGuid`** → new GUID. The Obsidian entity type is a new record.
- **`BlockTypeGuid`** → the WebForms block's existing GUID (read it from `RockWeb/Blocks/[Category]/[BlockName].ascx.cs`'s own `[Rock.SystemGuid.BlockTypeGuid(...)]` attribute). The chop hijacks that BlockType row.
- **Commented "was" line** → the newly generated BlockTypeGuid that the script produced. Keep it in a `// was [Rock.SystemGuid.BlockTypeGuid( "..." )]` comment immediately above the active attribute. It is informational only — that GUID is never written to the database — but it documents what would have been used had this been a net-new block, and it helps reviewers spot intentional GUID reuse.
- **Use the script:** `node .claude/skills/convert-block/scripts/generate-guids.js` to generate the entity-type GUID and the "was" GUID. The block-type GUID itself comes from the WebForms `.ascx.cs`, not from the script.

**Do NOT write a Rock.Migrations migration to register the block.** The chop is automatic. Writing `AddOrUpdateEntityBlockType` is correct for net-new blocks (no WebForms predecessor); it is wrong for a conversion because it would create a second BlockType row alongside the WebForms one and existing pages would not pick up the Obsidian block.

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

### Always import existing TS enums; never redeclare them in `types.partial.ts`

`Rock.Enums/` is auto-generated to `Rock.JavaScript.Obsidian/Framework/Enums/` by Rock.CodeGeneration. Most enums you'll need on the TS side already exist as `@Obsidian/Enums/[Domain]/[enumName]` and ship full descriptions, value maps, and a TypeScript type alias.

**Before defining or redefining an enum in `types.partial.ts`, check whether the C# source lives in `Rock.Enums/` (or has a forwarder) and whether the auto-generated TS file exists:**

```bash
# Look up the C# source — anything under Rock.Enums/ has a generated TS twin.
ls Rock.Enums/[Domain]/[EnumName].cs

# Look up the generated TS twin.
ls Rock.JavaScript.Obsidian/Framework/Enums/[Domain]/[enumName].ts
```

If the TS file exists, import from it:

```typescript
import { ContentChannelDateType } from "@Obsidian/Enums/Cms/contentChannelDateType";
import { ContentLibraryItemExperienceLevel } from "@Obsidian/Enums/Cms/contentLibraryItemExperienceLevel";
```

Defining a parallel `export const enum ContentChannelDateType { ... }` in `types.partial.ts` is wrong even when the values are correct: the enum drifts the moment the C# source changes, the TS-side `*Description` map and the framework's `enumToListItemBag` helper aren't usable on the local copy, and Code Generation will eventually flag the duplication.

`types.partial.ts` is reserved for **block-local declarations** that have no C# source — typically `NavigationUrlKey`, modal-toggle string-literal enums, internal grid row shapes, and small helper types. Cross-language enums always come from `@Obsidian/Enums/`.

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

---

## PersonPicker — Emits a PersonAlias Guid, NOT a Person Guid

**This has bitten us multiple times.** The `<PersonPicker>` control emits a `ListItemBag` whose `.value` is the selected person's **`PrimaryAliasGuid`** (i.e. a `PersonAlias.Guid`) — *not* the `Person.Guid`.

Comparing the emitted value to `Person.Guid` on the server silently matches zero rows (no exception — it just filters everything out).

### Correct server-side handling

Resolve the alias guid to a `PersonId` first so the filter catches every registration/record the person is associated with, regardless of which `PersonAlias` was current at the time:

```csharp
var aliasGuid = FilterRegisteredBy?.Value.AsGuidOrNull();

if ( aliasGuid.HasValue )
{
    var personId = new PersonAliasService( rockContext ).GetPersonId( aliasGuid.Value );

    if ( personId.HasValue )
    {
        qry = qry.Where( r => r.PersonAlias != null && r.PersonAlias.PersonId == personId.Value );
    }
    else
    {
        // Selected person alias didn't resolve — no rows should match.
        qry = qry.Where( _ => false );
    }
}
```

### Wrong (silently matches nothing)

```csharp
// BUG: PersonPicker emits PersonAlias.Guid, not Person.Guid.
qry = qry.Where( r => r.PersonAlias.Person.Guid == aliasGuid.Value );
```

### Rules of thumb

- **Never** compare a PersonPicker value to `Person.Guid`.
- Prefer resolving to `PersonId` via `new PersonAliasService( rockContext ).GetPersonId( aliasGuid )` — catches all of the person's aliases.
- If you only want to match the primary alias, compare against `PersonAlias.Guid` directly — but this is almost always narrower than intended.
- The same rule applies to any control that emits `primaryAliasGuid` (e.g. pickers that wrap `PersonPicker`). When in doubt, check the control's `selectPerson` / emit path.
