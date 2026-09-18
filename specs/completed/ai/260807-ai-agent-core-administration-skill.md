---
author: Jon Edmiston
date_created: 2026-08-07
summary: >-
  A read-only AI agent skill exposing Rock's core configuration metadata:
  defined types and values, entity types, categories, field types, attribute
  definitions, and system communications. Thirteen tools, no writes. Nothing
  in it is workflow-specific, so any future page, block, report, or Lava
  authoring skill can build on the same surface.
contributors: []
---

# Core Administration Agent Skill

## Summary

This spec defines `CoreAdministrationSkill`, a read-only AI agent skill covering Rock's configuration metadata. Thirteen tools expose defined types and values, entity types, categories, field types, attribute definitions, and system communications.

Nothing here is workflow-specific. It exists as its own skill because a workflow builder, a page builder, a report builder, and a Lava authoring skill all need the same reference data, and none of them should own it.

The skill is R&D and targets the `rock-plugin-ai-agent` plugin. It may move into Rock core later. One Rock core change is a hard prerequisite: `AttributeResult` must gain a `Guid`, specified in [260807-ai-agent-result-guids.md](completed/ai/260807-ai-agent-result-guids.md).

## Motivation

An agent that configures anything in Rock spends most of its tool calls resolving references. To set a workflow action's "Connection Status" setting it must find the right defined value. To create an attribute it must find the right field type. To file a workflow it must find the right category. None of that is the interesting part of the task, and all of it has to be correct or the write fails.

Two problems made this worth specifying rather than growing organically.

**Reference lookups were about to be duplicated.** The Workflow Builder skill needed defined values, categories, field types, and entity types. So would a page builder. Building them per skill would produce four subtly different `ListCategories` implementations.

**Class name strings broke the first demo.** The agent wrote `Rock.Field.Types.SingleSelectFieldType` into an import. The real class is `SelectSingleFieldType`. The resulting error named a foreign key constraint rather than the misspelling, and the failure cost more time than any bug in the demo. Documenting the correct spelling would help an agent verify; removing the parameter entirely makes the mistake unrepresentable.

## Requirements

**Surface**

- The skill MUST be read-only. No tool may write.
- No tool MAY accept a fully qualified class name as a parameter. `IdKey` identifies everything.
- Class names MAY be returned as output where reading a configuration requires recognizing them.

**Result shape**

- A `List` or `Lookup` result MUST carry IdKey, Name, and only what is needed to choose between rows. Detail belongs to the matching `Get`.
- `List` and `Lookup` results MUST NOT carry `Guid` when a partner `Get` exists.
- Exactly one attribute-definition shape MUST exist across the surface: the core `AttributeResult`.

**Paging and security**

- Every tool whose result set grows with data MUST page.
- No tool MAY truncate a result set to a fixed size. Results are returned whole, paged, or refused with an error naming the parameter that would narrow them.
- Security filtering MUST run across the whole collection before paging, never after.
- A tool reading a secured entity from a database query MUST use `CursorPaginator`, because `GetPaginatedItems` performs no authorization.
- Every tool using page-number paging MUST document why it is not a cursor.

## Design

### Skill declaration

```csharp
[Description( "Provides access to Rock's core configuration metadata: defined types and values, entity types, categories, field types, attributes, and system communications." )]
[AgentSkillGuid( "6DBD6867-2E0B-4D2E-9BF9-B34B77E4E94B" )]
[EntityTypeGuid( "55EB1E6F-EFBF-4E9C-BA11-DBC7147DA342" )]
internal sealed partial class CoreAdministrationSkill : AgentSkillComponent
```

`GetEntityAvailableAttributes` and `GetDefinedValueAvailableAttributes` keep the write-support naming from the established `Get{Entity}AvailableAttributes` pattern even though this skill has no writes, because the writes they support live in other skills.

### Tool inventory

| # | Tool | Kind | Paging | History |
|---|---|---|---|---|
| 1 | `ListDefinedTypes` | List | page number | no |
| 2 | `GetDefinedType` | Get | none | full |
| 3 | `ListDefinedValues` | List | page number | compact |
| 4 | `GetDefinedValue` | Get | none | full |
| 5 | `GetDefinedValueAvailableAttributes` | Get | none | full |
| 6 | `ListEntityTypes` | List | page number | **no** |
| 7 | `ListCategories` | List | page number | compact |
| 8 | `GetCategory` | Get | none | full |
| 9 | `LookupFieldTypes` | Lookup | none | keyed |
| 10 | `GetFieldType` | Get | none | full |
| 11 | `GetEntityAvailableAttributes` | Get | page number | full |
| 12 | `ListSystemCommunications` | List | **cursor** | compact |
| 13 | `GetSystemCommunication` | Get | none | full |

### Two rules applied everywhere

**List results carry identity only.** A `List` or `Lookup` returns IdKey, Name, and whatever is needed to choose between rows. Everything else belongs to the matching `Get`. This is why tools 8 and 13 exist: `ListCategories` and `ListSystemCommunications` were returning detail that had no business in a multi-row result. A list result is paid for once per row; a `Get` is paid for once.

**Class name strings are output, never input.** No tool in this skill or the Workflow Builder skill accepts a fully qualified class name. `Class` is still returned by `GetFieldType`, and `ClassName` by the Workflow Builder's `LookupWorkflowActionComponents`, because reading a configuration means recognizing them. Neither is ever passed back.

---

### 1. ListDefinedTypes

`[AgentToolGuid( "53DDA7C1-00A5-4531-8A5D-07FBC6721798" )]`

A `List` rather than a `Lookup` because churches create defined types freely, so the set grows with data.

```csharp
public AgentToolResult ListDefinedTypes( string partialName = null, string categoryIdKey = null, int pageNumber = 1 )
```

| Input | Required | Notes |
|---|---|---|
| `partialName` | No | |
| `categoryIdKey` | No | `DefinedType.CategoryId` is an FK to `Category` |
| `pageNumber` | No | |

**Output.** `IdKey, Name, Category { IdKey, Name }`. Nothing else. `Description`, `HelpText`, `FieldTypeClass`, `IsSystem`, `IsActive`, `CategorizedValuesEnabled`, and `ValueCount` all belong to tool 2.

**Volume.** Core declares 108 defined type SystemGuid constants. A live instance runs 150 to 200 stock plus whatever the church adds.

**Paging.** Page number, 50 per page. Source is `DefinedTypeCache.All()`.

**Why not a cursor.** Deliberate, not an oversight. The source is a cache and is already materialized in memory, so any security filtering runs across the whole collection before paging and there are no database round trips to avoid. `CursorPaginator` solves a problem this tool does not have, and it requires an `IQueryable` besides.

**History.** None. Even trimmed, a few hundred rows is more than chat history should carry for reference data the agent can re-fetch.

---

### 2. GetDefinedType

`[AgentToolGuid( "366B42BD-9D92-4042-8B20-04EE6B0142C7" )]`

The configuration of one type. It returns neither values nor attribute definitions: use tool 3 for values and tool 5 for definitions.

```csharp
public AgentToolResult GetDefinedType( string definedTypeIdKey )
```

**Output.** `IdKey, Guid, Name, Description, HelpText, Category { IdKey, Name }, FieldTypeClass, IsSystem, IsActive, CategorizedValuesEnabled, EnableSecurityOnValues, ValueCount`.

**Volume.** One item. **Paging.** None. **History.** Full content.

`EnableSecurityOnValues` is returned because it changes how tool 3 behaves. A caller needs to know whether the value list it receives was security-filtered.

The result carries `.WithInstructions( $"Call the {nameof( ListDefinedValues )} function to retrieve this type's values." )`.

---

### 3. ListDefinedValues

`[AgentToolGuid( "0351DA93-E519-48D6-BB05-21D93A9583CA" )]`

The common path for defined values, and the reason tool 2 does not carry them.

```csharp
public AgentToolResult ListDefinedValues( string definedTypeIdKey, string partialValue = null, string categoryIdKey = null, bool includeInactive = false, int pageNumber = 1 )
```

| Input | Required | Notes |
|---|---|---|
| `definedTypeIdKey` | **Yes** | |
| `partialValue` | No | Matches `Value` and `Description` |
| `categoryIdKey` | No | Only meaningful when the type has `CategorizedValuesEnabled` |
| `includeInactive` | No | Defaults to false |
| `pageNumber` | No | |

**Output.** `IdKey, Value, Description, Category { IdKey, Name }, AttributeValues[]`.

- `Category` appears only when the defined type has `CategorizedValuesEnabled`. Emitting a null category on every row of a type that does not use them is noise.
- `AttributeValues` uses `GetGridAttributeValueResults`, matching `ListConnectionRequests` and `ListGroups`. Values, not definitions; definitions come from tool 5.
- **No `Guid`.** Several workflow actions store defined value GUIDs as setting values, so a caller will need one, but it gets it from tool 4. That is one extra call on the value actually being used rather than a GUID on every row of every page.

**Volume.** Up to roughly 250 for the longest core types, unbounded in principle.

**Paging.** Page number, 50 per page. Source is `DefinedValueCache.All()` filtered by `DefinedTypeId`.

**Why not a cursor.** Deliberate, not an oversight. Security filtering on a paged list is normally the trigger for `CursorPaginator`, which fetches, drops unauthorized rows, and refetches to avoid repeated database round trips. That reasoning applies to a query, not a cache. `DefinedValueCache` is already materialized, so the whole collection is filtered once and then paged, with nothing to round-trip.

**History.** Compact page of `KeyNameResult`.

**Security.** Conditional. When the parent type has `EnableSecurityOnValues`, filter by `IsAuthorized( VIEW, currentPerson )` across the whole collection before paging.

Named `ListDefinedValues`, not `ListDefinedValuesForDefinedType`. The parent is a required parameter, not part of the name, matching `ListGroupMembers( groupIdKey )`.

---

### 4. GetDefinedValue

`[AgentToolGuid( "BF14C7EA-98DC-4FFF-8485-F9952B2F4B8B" )]`

```csharp
public AgentToolResult GetDefinedValue( string definedValueIdKey )
```

**Output.** `IdKey, Guid, Value, Description, Order, IsActive, DefinedType { IdKey, Name }, Category { IdKey, Name }, AttributeValues[]`.

**Volume.** One item. **Paging.** None. **History.** Full content.

---

### 5. GetDefinedValueAvailableAttributes

`[AgentToolGuid( "542ED067-19EA-4DEE-B8DA-47FBB47C467D" )]`

The attribute definitions shared by every value of a defined type.

```csharp
public AgentToolResult GetDefinedValueAvailableAttributes( string definedTypeIdKey = null, string definedValueIdKey = null )
```

Exactly one of the two is required. A value resolves to its type; the definitions are the same either way, because definitions are per type rather than per value.

**Output.** The standard `helper.GetAvailableAttributes` shape, a collection of `AttributeResult`. No new result class.

**Volume.** Typically 0 to 5. Most defined types have no attributes on their values.

**Paging.** None. **History.** Full content.

**Implementation.** Build a stub `DefinedValue` with the resolved `DefinedTypeId`, call `LoadAttributes`, and pass it to `helper.GetAvailableAttributes`. That is what `GetConnectionRequestAvailableAttributes` does with a stub `ConnectionRequest`.

This is a dedicated tool rather than a use of tool 11 because defined value attributes are qualified by `DefinedTypeId`, and tool 11 has no qualifier support.

---

### 6. ListEntityTypes

`[AgentToolGuid( "7BD8DF7C-09AA-4809-8364-37D594370E99" )]`

```csharp
public AgentToolResult ListEntityTypes( string partialName = null, int pageNumber = 1 )
```

| Input | Required | Notes |
|---|---|---|
| `partialName` | No | Matches `Name` and `FriendlyName`. In practice always supplied |
| `pageNumber` | No | |

**Output.** `IdKey, Guid, Name, FriendlyName, IsEntity, IsSecured`.

`Name` is the full class name (`Rock.Model.Workflow`), which is what `DataView.EntityTypeId` and the attribute tools resolve against. It is output only; no tool accepts it as a parameter.

**This is the only `List` in the skill that carries `Guid`, and only because it has no `Get` partner.** There is no `GetEntityType`, so dropping it here would leave no way to obtain an entity type GUID at all, and workflow action settings store them. If a `GetEntityType` is ever added, `Guid` moves there and this exception ends.

**Rows must come from `EntityTypeCache.All()`.** `Guid`, `FriendlyName`, and `IsSecured` exist only on the `EntityType` table, so that cache is the only valid source.

**Volume.** A stock instance registers well over 1,000 entity types.

**Paging.** Page number, 50 per page.

**Why not a cursor.** Rows come from `EntityTypeCache.All()`, an in-memory collection rather than an `IQueryable`, so `CursorPaginator` cannot be used at all. `EntityType` also derives from `Entity<T>` rather than `Model<T>`, so it is not `ISecured` and the security argument that forces a cursor elsewhere does not arise.

**History.** None. The result is expected to be large, and it is reference data the agent can re-fetch.

Filtering by kind of entity type is deferred; see Out of Scope.

---

### 7. ListCategories

`[AgentToolGuid( "8B1EFF0E-AAE0-43BF-A2DA-D1C71EADF28B" )]`

```csharp
public AgentToolResult ListCategories( string entityTypeIdKey, string partialName = null, string parentCategoryIdKey = null, int pageNumber = 1 )
```

**Output.** `IdKey, Name, ParentCategory { IdKey, Name }`.

`ParentCategory` stays because without it a flat list of a deep tree is unreadable. Everything else, including `Description`, `Order`, `IconCssClass`, and `Guid`, moves to tool 8.

**Volume.** Workflow categories in a large church reach several hundred. Most entity types have fewer than 20.

**Paging.** Page number, 50 per page. Source is `CategoryCache.All()`.

**Why not a cursor.** Security filtering on a paged list is normally the trigger for `CursorPaginator`, but that rule is about database queries. A cache is already materialized, so the whole collection is filtered once and then paged, with no round trips to save.

**History.** Compact page of `KeyNameResult`.

**Security.** Filter by `IsAuthorized( VIEW, currentPerson )` across the whole collection before paging, never after. `GetPaginatedItems` does no security filtering and takes no person. Filtering after paging yields short pages and a wrong `HasMoreItems`.

---

### 8. GetCategory

`[AgentToolGuid( "9E3E5A2C-6D67-4F3B-B0AC-11A02C43B0E1" )]`

The detail behind a category, so tool 7 can stay to identity only.

```csharp
public AgentToolResult GetCategory( string categoryIdKey )
```

**Output.** `IdKey, Guid, Name, Description, Order, IconCssClass, HighlightColor, EntityType { IdKey, Name }, ParentCategory { IdKey, Name }, ChildCategoryCount`.

**Volume.** One item. **Paging.** None. **History.** Full content.

---

### 9. LookupFieldTypes

`[AgentToolGuid( "04F39FBF-A3B4-4F1F-88E7-49E1D3AE73A7" )]`

```csharp
public AgentToolResult LookupFieldTypes( string partialName = null )
```

**Output.** `IdKey, Name`. `Class`, `Description`, and `Guid` belong to tool 10.

**Volume.** 173 field type classes in core plus a handful per plugin.

**Paging.** None, and no cap. The set is bounded by installed code: churches do not create field types, and only a plugin install changes the count. With two fields per row the whole set is small.

**History.** `.WithHistoryKey( "field-types" )`. This is the one lookup small enough to keep, and it is consulted constantly.

This is the tool that prevents the `SingleSelectFieldType` failure, but the mechanism is not what it first appears. It does not work by returning the correct `Class` string. It works because `fieldTypeIdKey` is the only way to name a field type in any write, so a wrong class string has nowhere to go.

---

### 10. GetFieldType

`[AgentToolGuid( "CD8C8E44-F60C-4F1A-A480-683C600C526E" )]`

```csharp
public AgentToolResult GetFieldType( string fieldTypeIdKey )
```

**Output.** `IdKey, Guid, Name, Class, Description, ConfigurationKeys[] { Key, Description, ExampleValue }`.

**Volume.** One item, typically 0 to 8 configuration keys. **Paging.** None.

**`IFieldType.ConfigurationKeys()` is not reliably populated.** Little in Rock consumes it, so many field types never filled it in, and where it exists it returns bare key names with no indication of what a key does or what value format it takes.

Because bare keys are not sufficient, hand-authored supplements are **required rather than optional** for the field types workflow authoring actually touches: text, memo, single select, multi select, boolean, date, date/time, integer, decimal, person, group, campus, defined value, file, image, email, phone, URL. Roughly 20, covering nearly all workflow attribute creation. Where a supplement does not exist, the tool says the key is undocumented rather than inventing a description.

The clean long-term fix is a new `IFieldType` member describing qualifiers, similar to `GetFieldHints()`. See Out of Scope.

---

### 11. GetEntityAvailableAttributes

`[AgentToolGuid( "2A0EF1D6-8C10-4E9C-BCDB-0FCA3FEC0998" )]`

The generic member of the `Get{Entity}AvailableAttributes` family, for entities that never justify a dedicated tool.

It is deliberately not named `GetEntityTypeAvailableAttributes`. That reads as the partner of a `GetEntityType`, implying it returns attributes *of* an entity type rather than *for* entities of that type.

```csharp
public AgentToolResult GetEntityAvailableAttributes( string entityTypeIdKey, string partialName = null, int pageNumber = 1 )
```

**Output.** The standard `helper.GetAvailableAttributes` shape, a collection of `AttributeResult`. No custom result class.

**Volume.** Most entity types have fewer than 30 unqualified attributes. Person is the outlier and runs into the hundreds on a mature instance.

**Paging.** Page number, **200 per page** rather than the default 50.

Paging here departs from the rest of the family. Every other `Get{Entity}AvailableAttributes` tool returns its whole set, because each is scoped to one entity and the ceiling is knowable. This one takes any entity type in the system, so its ceiling is the worst case across all of them, and no argument the caller supplies changes that. `partialName` narrows the result but the caller cannot know in advance whether it narrowed enough. That is the same "grows with data, so it pages" rule the `List` tools follow, applied to a `Get` with a `List`-shaped result.

**Page size is 200, not the default 50.** `AttributeResult` is five small fields plus a GUID, so 200 rows run roughly 25 KB. Fifty would put Person at six or more round trips for what is usually a single reference lookup. `helper.GetPaginatedItems` already takes an optional `pageSize`, so this needs no core change.

**Why not a cursor.** `helper.GetAvailableAttributes` reads `entity.Attributes.Values`, an in-memory dictionary that `LoadAttributes` has already filled. There is no `IQueryable` for `CursorPaginator` to seek over, and no round trip for it to save. A cursor here would be an encrypted offset and nothing more.

**Security.** Handled inside `helper.GetAvailableAttributes`, which drops anything failing `IsAuthorized( VIEW, currentPerson )` before this tool sees the collection. Filtering precedes paging by construction.

**This tool returns only attributes that apply to all entities of the type.** Qualified sets get dedicated tools, as tool 5 does for defined values. See Considered but Rejected.

---

### 12. ListSystemCommunications

`[AgentToolGuid( "83AFE4C8-F8BC-4BF8-A7D6-6FDCF8AD8561" )]`

```csharp
public AgentToolResult ListSystemCommunications( string partialName = null, string categoryIdKey = null, string cursor = null )
```

**Output.** `IdKey, Title, Category { IdKey, Name }`. `Subject`, `IsActive`, `IsSystem`, and the channel flags belong to tool 13. `Body` is never returned by either.

**Volume.** Core declares 45 SystemCommunication SystemGuid constants. A live instance runs 50 to 120.

**Paging.** Cursor, 50 per page. Order by `Title`, then `Id`.

**This is the only cursor tool in either skill, and the only one that must be.** There is no `SystemCommunicationCache`, so it genuinely queries the database, and `SystemCommunication` derives from `Model<T>`, which implements `ISecured`. Database query plus a secured entity is exactly the case `CursorPaginator` exists for: `GetPaginatedItems` performs no authorization, and `IsAuthorized` cannot be translated to SQL, so the fetch-filter-refetch loop in `FillPage` is the only correct mechanism.

Do not assume an entity is unsecured without checking. Nearly every Rock entity derives from `Model<T>` and is therefore `ISecured`.

**Security.** Enforced by the paginator. Construct it with the person overload:

```csharp
new CursorPaginator<SystemCommunication>( currentPerson, qry => qry.OrderBy( c => c.Title ).ThenBy( c => c.Id ) )
```

The `.ThenBy( c => c.Id )` is required. Without a unique tiebreaker, two identically titled communications produce the same cursor and a row is silently skipped or repeated.

**History.** Compact page of `KeyNameResult`.

---

### 13. GetSystemCommunication

`[AgentToolGuid( "1D1D0F7C-6B22-4C4E-9E4B-BC5A0A9F1D74" )]`

The detail behind a system communication, so tool 12 can stay to identity only.

```csharp
public AgentToolResult GetSystemCommunication( string systemCommunicationIdKey )
```

**Output.** `IdKey, Guid, Title, Subject, From, FromName, To, Cc, Bcc, Category { IdKey, Name }, IsActive, IsSystem, HasSmsMessage, HasPushMessage, BodyLength`.

**Never return `Body`.** It is large and no authoring task needs it. `BodyLength` tells a caller whether the template has content without paying for it.

**Volume.** One item. **Paging.** None. **History.** Full content.

---

### Result classes

Under `Agent/Rock.AI.Agent/Classes/Skills/CoreAdministrationSkill/`:

`DefinedTypeResult`, `DefinedTypeDetailResult`, `DefinedValueResult`, `DefinedValueDetailResult`, `EntityTypeResult`, `CategoryResult`, `CategoryDetailResult`, `FieldTypeResult`, `FieldTypeDetailResult`, `SystemCommunicationResult`, `SystemCommunicationDetailResult`.

No enums, and no custom attribute shape. Tools 5 and 11 both return the core `AttributeResult`, leaving exactly one attribute-definition shape across both skills.

All result classes derive from `EntityResultBase` where they represent an entity, so `IdKey` and `Guid` come free. `Guid` must be populated explicitly; it is null by default.

### Decisions without precedent

Everything below has **no precedent** in the existing agent skills, listed so it gets reviewed deliberately rather than discovered during implementation. Verified as precedented and therefore not listed: `pageNumber` paging, cursor paging, `partialName` filtering, refusing a call with `helper.AddError`, and returning grid attribute values in a `List`.

1. **A generic `Get{Entity}AvailableAttributes`.** Tool 11 takes an `entityTypeIdKey` rather than being written per entity. Every existing one is entity-specific.
2. **An additive change to a core result class.** Adding `Guid` to `AttributeResult` touches Rock core rather than the plugin. Small and non-breaking, but it needs sign-off. See [260807-ai-agent-result-guids.md](completed/ai/260807-ai-agent-result-guids.md).
3. **A paged `Get`, and a non-default page size.** Tool 11 pages, and at 200 rather than 50. No other `Get` in either skill pages, and no tool in either skill overrides `DefaultPageSize`.
4. **Hand-authored supplements inside a tool.** Nothing else in either skill carries authored reference content in tool code. Forced by `ConfigurationKeys()` being unreliable.

## Out of Scope

- **Writes of any kind.** Configuration writes belong to the skills that need them.
- **Filtering entity types by kind.** There is no way to ask for "only models" or "only field types" in this version. Rock has no classification column, so any such filter must be computed from cache and container membership. Deferred to its own spec. Groundwork worth keeping: classification needs no CLR type resolution, because `FieldType.Class` matches `EntityType.Name` as a string, `BlockType.EntityTypeId` is a direct Id, and containers already map components by `EntityTypeCache.GetId( type.FullName )`. Walk the containers, which are small, not the entity types, which are not.
- **A new `IFieldType` member describing configuration qualifiers.** The clean fix for tool 10, but it means implementing across all 173 field types. A Rock core project, logged as a follow-up, and it does not gate this work.
- **System communication `Body` content.** Never returned. Authoring tasks need to know a template exists, not what it says.

## Considered but Rejected

### Accept a class name as a tool parameter

Rejected. It is how the first demo failed: `Rock.Field.Types.SingleSelectFieldType` does not exist, the real name is `SelectSingleFieldType`, and the error named a foreign key constraint rather than the misspelling. Documenting the correct spelling helps an agent verify. Removing the parameter makes the mistake unrepresentable, which is strictly better.

### Return defined values inline from `GetDefinedType`

Rejected. Values are the common path and types are the rare one, so inlining would put a paged-size collection inside a single-item result on every call. Tool 3 exists precisely so tool 2 can stay small.

### Qualifier parameters on `GetEntityAvailableAttributes`

Rejected. Attribute qualifiers vary from none, to one optional, to one required, to several required, depending on the entity. A single `qualifierColumn` and `qualifierValue` pair cannot express that, and a caller has no way to know which combination a given entity type needs. Supplying the wrong pair returns a plausible but wrong set, which is worse than returning none. Qualified cases get dedicated tools instead, as tool 5 does for defined values.

### Refuse-and-instruct on `GetEntityAvailableAttributes`

Rejected in favor of paging. An earlier form errored above roughly 150 results and told the caller to narrow with `partialName`. Paging is strictly better: the caller gets an answer instead of an error, and nothing is unreachable.

### Conditional history on `ListEntityTypes`

Rejected. Storing the result only when a single page returns under 25 rows would save a little context, but conditional history behavior is hard to reason about later and the saving is small.

### A `ValueAttributes[]` array inline on `GetDefinedType`

Rejected. It invented a third attribute shape alongside `AttributeResult` and `EntityTypeAttributeResult`. Definitions and values are different things, and definitions belong in a `Get{Entity}AvailableAttributes` tool. Became tool 5.

## Open Questions

1. **Skill naming.** `CoreAdministrationSkill` is broad. If page, block, report, and Lava authoring land here later it fits. Fine to defer while this is R&D.
2. **Does `ListDefinedTypes` need `pageNumber` in practice?** It pages because the set grows with data, which is the correct rule. If live instances run well under 100 types, a filtered single page will be the norm and the paging will be inert. Harmless either way, but worth measuring before adding more filters.
3. **Should `GetFieldType` supplements live in the tool or in the knowledge pack?** They are currently specced inside the tool. An argument exists for putting them in a field type article instead, so the knowledge pack owns all authored content and the tool stays purely reflective. Depends on whether a non-workflow skill ever needs them.

## Related

- [260807-ai-agent-result-guids.md](completed/ai/260807-ai-agent-result-guids.md) — **prerequisite.** `AttributeResult` must gain a `Guid` before tools 5 and 11 are useful for writes.
- [260807-ai-agent-tool-conventions.md](260807-ai-agent-tool-conventions.md) — the shared conventions this spec assumes throughout: naming prefixes, the paging decision order, identifier rules, and the never-cap rule.
