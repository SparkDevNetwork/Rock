# List Block Patterns

Patterns specific to List block conversions. Load this file when the block is classified as **List**.

---

## Grid Builder Field Types

| Method | Use For |
|---|---|
| `AddTextField` | strings, IdKey |
| `AddField` | booleans, numbers, enums, computed values |
| `AddDateTimeField` | `DateTime?` |
| `AddPersonField` | Person navigation properties — use with `PersonColumn` on the frontend, NOT `TextColumn` |
| `AddAttributeFields` / `AddAttributeFieldsFrom` | Entity attribute values |
| `GridBuilderGridOptions.LavaObject` | Expose an entity property for Lava template evaluation |

### Grid Column Rules

**Column type selection** — match the C# grid builder field type to the correct frontend column component:

| C# Grid Builder Method | Frontend Column | Filter |
|---|---|---|
| `AddTextField` (strings, IdKey) | `TextColumn` | `textValueFilter` |
| `AddField` (bool) | `BooleanColumn` | `booleanValueFilter` |
| `AddField` (int, decimal, number) | `NumberColumn` | `numberValueFilter` |
| `AddDateTimeField` | `DateTimeColumn` or `DateColumn` | `dateValueFilter` |
| `AddPersonField` | `PersonColumn` | custom `filterValue` function (see below) |
| `AddField` (computed/complex) | `Column` (generic) | `pickExistingValueFilter` or custom `filterValue` |

**Typed columns handle `dataType` automatically.** Only set `dataType` explicitly on the generic `Column` component when needed.

**`filterValue` / `quickFilterValue`** — required when the column displays HTML, objects, or computed values that can't be filtered as plain text. Applies to: `PersonColumn`, `HtmlColumn`, and any generic `Column` with non-string data.

```typescript
// Function signature for custom filter values
function getCustomFilterValue(row: Record<string, unknown>): string {
    return row["fieldName"] as string ?? "";
}

// Person field filter — extracts searchable text from PersonFieldBag
function getPersonFilterValue(row: Record<string, unknown>): string {
    const person = row["assignedTo"] as PersonFieldBag;
    return !person ? "" : `${person.nickName} ${person.lastName}`;
}
```

**`excludeFromExport`** — use on columns that are purely visual or computed display-only. Security buttons, reorder handles, and delete buttons handle this automatically. Add `:excludeFromExport="true"` for any custom display column that would be meaningless in a CSV export.

**Reference:** `Rock.JavaScript.Obsidian.Blocks/src/Cms/contentChannelItemList.obs` for `excludeFromExport` and custom filter/sort functions.

---

### Special grid fields

| Field name | Purpose | Correct usage |
|---|---|---|
| `isSystem` | **Always include** when the entity has an `IsSystem` property. The grid natively disables the delete button for rows where `isSystem === true`. | `.AddField( "isSystem", a => a.IsSystem )` |
| `isSecurityDisabled` | Disables the security button per row. Must use **entity-level** auth, NOT block-level. Block-level auth is redundant with `IsSecurityColumnVisible` in the options bag. | `.AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )` |
| `SecurityColumn itemTitle` | Check the WebForms `.ascx` for `<Rock:SecurityField TitleField="Name" />` and carry it forward. Without `itemTitle`, the security dialog won't show the entity name. Value is the **camelCase grid field name**. | `<SecurityColumn itemTitle="name" />` |

**WRONG** — do not use block-level auth for `isSecurityDisabled`:
```csharp
// WRONG: same value for every row, redundant with options bag
.AddField( "isSecurityDisabled", _ => !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, ... ) )
```

See `Rock.Blocks/Engagement/StepTypeList.cs` for a complete `GetGridBuilder()` example including `GridBuilderGridOptions`, `LavaObject`, computed counts, and `AddAttributeFieldsFrom`.

---

## Person Fields

When the WebForms grid has a column bound to a person (e.g., `CreatedByPersonAlias.Person.FullName`), use `AddPersonField` in the grid builder and `PersonColumn` on the frontend — **not** `AddTextField` with `TextColumn`. The `PersonColumn` requires a `PersonFieldBag` filter value function since its data is an object, not a string. Import `PersonFieldBag` from `@Obsidian/ViewModels/Core/Grid/personFieldBag`.

---

## Server-Side Grid Filters

### Decision: column filters vs. server-side filters

- **Column filters only** — when data is small-to-moderate and all WebForms filters simply narrow visible rows. No `PreferenceKey`, no `gridSettingsModal`.
- **Server-side filters** — when the entity list can grow to thousands of records, or when the WebForms block had filters that reduced the query at the DB level.

### Filter serialization conventions

| Filter type | Stored as | C# read | TS read |
|---|---|---|---|
| `string` | Plain string | `.GetValue(...)` | `preferences.getValue(...)` |
| `bool` | `"True"` / `"False"` | `.AsBoolean()` | `asBooleanOrNull(...)` — write with `asTrueOrFalseString(...)` |
| `Guid?` | Guid string | `.AsGuidOrNull()` | stored as string |
| `int?` | `"123"` | `.AsIntegerOrNull()` | `toNumberOrNull(...)` |
| `List<int>` | `"1|2|3"` | `.SplitDelimitedValues().Select(...)` | `.split("|").filter(...)` |
| `ListItemBag` | JSON string | `.FromJsonOrNull<ListItemBag>()` | `safeParseJson(...)` — **never** use `JSON.parse` (throws on bad data) |
| `SlidingDateRange` | Framework string | `.ToSlidingDateRangeBagOrNull()` | `parseSlidingDateRangeString(...)` |
| `PersonPicker` value | `ListItemBag` JSON string | `.FromJsonOrNull<ListItemBag>()` — **resolve the alias guid** (see below) | `safeParseJson<ListItemBag>(...)` |

Key rules:
- Define `PreferenceKey` constants using kebab-case strings.
- Expose each filter as a `protected` property reading from `GetBlockPersonPreferences()`.
- Only apply a `.Where()` clause when the filter has a value — never filter on empty/null.
- The gridSettingsModal emits `update:modelValue` only when values actually changed (use `deepEqual`).
- **gridSettings initialization:** Use `safeParseJson` (from `@Obsidian/Utility/stringUtils`) for JSON values, `asBooleanOrNull` / `asTrueOrFalseString` (from `@Obsidian/Utility/booleanUtils`) for booleans. Never hand-roll `JSON.parse(... || "null")` or `=== "True"` comparisons.
- **`PersonPicker` emits a `PersonAlias` Guid, not a `Person` Guid.** Comparing it to `Person.Guid` silently matches zero rows. Always resolve to `PersonId` via `new PersonAliasService( rockContext ).GetPersonId( aliasGuid )` before filtering. See `common-patterns.md` § *PersonPicker — Emits a PersonAlias Guid, NOT a Person Guid* for the full pattern.

**Reference:** `Rock.Blocks/Communication/CommunicationList.cs` + `Rock.JavaScript.Obsidian.Blocks/src/Communication/CommunicationList/`

---

## ContextAware Blocks

```csharp
[ContextAware]
public class MyList : RockEntityListBlockType<SomeEntity>
{
    // In GetListQueryable:
    var person = RequestContext.GetContextEntity<Person>();
}
```

Use `[ContextAware( typeof( Person ) )]` to declare a specific expected type. Use plain `[ContextAware]` for open context.

---

## ReorderItem Block Action

```csharp
[BlockAction]
public BlockActionResult ReorderItem( string key, string beforeKey )
{
    var qry = GetListQueryable( RockContext );
    qry = GetOrderedListQueryable( qry, RockContext );
    var items = GetListItems( qry, RockContext );

    if ( !items.ReorderEntity( key, beforeKey ) )
    {
        return ActionBadRequest( "Invalid reorder attempt." );
    }

    RockContext.SaveChanges();
    return ActionOk();
}
```
