---
author: Jon Edmiston
date_created: 2026-08-07
summary: >-
  The shared conventions every new AI agent skill tool follows: file layout,
  tool name prefixes, annotations, identifiers, paging, helper methods, and
  filter naming. Read out of the existing agent skill code and Rock core
  rather than invented. Companion specs assume everything here and only note
  departures.
contributors: []
---

# Agent Tool Conventions

## Summary

This is the shared reference for AI agent skill tools. It records the conventions already present in the existing skills so that new skills follow them rather than reinventing them, and so a reviewer can tell a deliberate departure from an accident.

Everything here was read out of `rock-plugin-ai-agent` and Rock core. Nothing in it is a new proposal except where a rule is written down for the first time; in those cases the rule describes what the existing code already does.

Two companion specs assume this document and note only their departures: [260807-ai-agent-core-administration-skill.md](260807-ai-agent-core-administration-skill.md) and the Workflow Builder skill spec.

## Motivation

The existing agent skills are internally consistent but the consistency is undocumented. It lives in the code, which means every new skill re-derives it, and a reviewer looking at a new tool has no way to tell whether an unusual choice was reasoned or careless.

One rule in particular is worth writing down because getting it wrong is silent: **`GetPaginatedItems` performs no security filtering.** Using it on a secured database query returns rows the person is not allowed to see, and nothing warns you. That is not an obvious property of a method named "get paginated items," and it is the single highest-value fact in this document.

## Requirements

- A new skill MUST follow the file and class layout below.
- A tool's name prefix MUST match its behavior: bounded reference sets are `Lookup`, growable sets are `List`, single items are `Get`.
- Every tool MUST carry `[Description]` and a unique `[AgentToolGuid]`.
- Every destructive tool MUST carry `[AgentGuardrail]`.
- Tool parameters MUST use IdKeys, named `{entity}IdKey`.
- Result `Guid` MUST be populated explicitly; it is null by default everywhere. This applies to **every** tool type, `Lookup` and `List` included, wherever the model has a `Guid`.
- A tool paging a secured database query MUST use `CursorPaginator` with the person constructor.
- A cursor ordering MUST end with a unique tiebreaker.
- Any tool using page-number paging MUST document why it is not a cursor.
- No tool MAY truncate a result collection to a fixed size.
- A tool that clips a single long value MUST flag it and MUST ship alongside a companion tool that returns the value whole.

## Design

### 1. File and class layout

- One tool per file, named `{Skill}.{ToolName}.cs`, in `Agent/Rock.AI.Agent/Skills/`.
- Each file declares `internal sealed partial class {Skill}` with a single `#region Tool(s)` and an optional `#region Helper Methods`.
- The skill root file `{Skill}.cs` carries `[Description]`, `[AgentSkillGuid]`, and `[EntityTypeGuid]`, derives from `AgentSkillComponent`, and takes `ILogger<{Skill}>` in its constructor.
- Result classes live in `Agent/Rock.AI.Agent/Classes/Skills/{Skill}/{Name}Result.cs`.

Reference: `ConnectionSkill.cs`, `ConnectionSkill.ListConnectionRequests.cs`.

### 2. Tool name prefixes

| Prefix | Meaning | History behavior |
|---|---|---|
| `Lookup` | Bounded reference set, returned whole | `.WithHistoryKey( "..." )` |
| `List` | Filtered set that can be large | Paged, compact history page |
| `Get` | One item in full detail | Full content |
| `AddOrUpdate` | Single write, create or edit | Compact `KeyNameResult` history |
| `Delete` | Single destructive write | Text confirmation |

**`Lookup` versus `List` is decided by whether the set can grow with data, not by whether the tool takes a filter.** A `Lookup` set is bounded by installed code (field types, workflow action components) or by a fixed configuration surface, so it stays whole and goes in history. A `List` set grows as a church adds records, so it pages.

### 3. The `Get{Entity}AvailableAttributes` pattern

An established name, used by `ConnectionSkill`, `ContentChannelSkill`, `AttendeeSkill`, and `WorkflowSkill`. **It exists to support a write.** The description on `GetConnectionRequestAvailableAttributes` says it plainly: "the available attributes that can be set when adding or updating a connection request."

`GetEntityAvailableAttributes` in the Core Administration skill is the **generic** member of this family. Most entities do not justify a dedicated tool, so it covers them by taking an entity type and returning the attributes that apply to every entity of that type.

Two consequences follow.

**Add a dedicated `Get{Entity}AvailableAttributes` whenever the attribute set is qualified.** The generic tool deliberately has no qualifier support, because qualifiers range from none, to one optional, to several required, and a single column and value pair cannot express that. A caller supplying the wrong pair gets a plausible but wrong set.

`GetConnectionRequestAvailableAttributes` earns its place because the set depends on the connection opportunity, so it builds a stub `ConnectionRequest` and calls `LoadAttributes`. `GetDefinedValueAvailableAttributes` does the same with a stub `DefinedValue`, since those attributes are qualified by `DefinedTypeId`. `GetWorkflowActionTypeAvailableAttributes` is qualified by the action component's entity type.

Building a stub entity and passing it to `helper.GetAvailableAttributes` is the pattern. It yields the standard output shape with no bespoke result class. **Name the tool after a real entity.** Inventing an entity name in a tool name teaches a concept that does not exist, and the next reader goes looking for it.

**Never inline attribute definitions into an entity result.** `AttributeResult` appears in zero result classes across the existing skills. It is only ever returned directly by a `Get{Entity}AvailableAttributes` tool. An entity result carries `AttributeValues`, meaning values. Definitions and values are different things. Where an entity's definitions are needed, add a `Get{Entity}AvailableAttributes` tool rather than nesting them inside the entity's own result.

### 4. Annotations

`[Description]` is required. `[AgentToolGuid]` is required and must be unique. `[AgentPurpose]`, `[AgentUsage]`, and `[AgentToolPrerequisite]` are optional and repeatable. `[AgentGuardrail]` is required on every destructive tool, following `NoteSkill.DeleteNote`.

### 5. Identifiers

Parameters are IdKeys, named `{entity}IdKey`. Results inherit `EntityResultBase`, which supplies `IdKey` (computed from an internal `Id` marked `[JsonIgnore]`) and a nullable `Guid`. `KeyNameResult` is the lightweight reference shape and carries `IdKey`, `Guid`, and `Name`.

`EntityResultBase` and `KeyNameResult` both already expose `Guid`. It defaults to null, so **populate it explicitly.**

**Every result that represents an entity carries its `Guid`, in every tool type.** An earlier rule limited this to `Get` tools on payload grounds; it was reversed. See "Scope, and why it was reversed" in [260807-ai-agent-result-guids.md](completed/ai/260807-ai-agent-result-guids.md).

Two practical notes:

1. **Take the guid from something already in hand.** In a projection that dereferences a navigation property for `Name`, the guid is one more column on a join that already exists. Do not add a cache or service call to fetch one.
2. **Where the result does not represent a record, name the field for what it identifies.** `WorkflowActionComponentResult` carries `EntityTypeGuid`, not `Guid`, because an action component has no identity of its own.

### 6. Paging

Two mechanisms exist. Pick by data source, not by preference.

| Source | Mechanism | Parameter | Helper |
|---|---|---|---|
| `IQueryable` over the database | Cursor | `string cursor = null` | `helper.GetCursorPaginatedItems( query, paginator, cursor )` |
| In-memory cache collection | Page number | `int pageNumber = 1` | `helper.GetPaginatedItems( items, pageNumber )` |
| Bounded reference set | None | none | `Success( x ).WithHistoryKey( ... )` |

Default page size is 50 (`AgentToolHelper.DefaultPageSize`); both helpers accept an optional override. `PaginatedResult<T>` returns `nextCursor`, `pageNumber`, `pageSize`, `returnedItemCount`, and `hasMoreItems`. `CursorPaginator<T>` encrypts the cursor and enforces entity security while filling pages.

#### Choosing, in order

1. **One item?** A `Get`. No paging.
2. **Does the set grow only when code is installed?** Field types, action components. Return it whole. No paging, no cap.
3. **Is the source a cache collection?** Filter for security across the whole set, then `helper.GetPaginatedItems( items, pageNumber )`.
4. **Is the source a database query?** `CursorPaginator` with the person constructor.

#### Why step 4 is not a preference

**`GetPaginatedItems` performs no security filtering.** It is a bare `Skip`/`Take` and takes no person at all. `CursorPaginator` is the only paginator that authorizes per item:

```csharp
if ( typeof( ISecured ).IsAssignableFrom( typeof( T ) ) && EnforceEntitySecurity )
    filteredBatch = filteredBatch.Where( item => ( ( ISecured ) item ).IsAuthorized( Authorization.VIEW, _person ) );
```

`IsAuthorized` cannot be translated to SQL, so an `IQueryable` cannot be filtered before paging. The only options are to materialize the whole table, which defeats paging, or to fetch a batch, drop what the person cannot see, and refetch until the page fills. That refetch loop is `FillPage`, capped at `MaxCursorFillAttempts = 25`.

**Using `GetPaginatedItems` on a secured database query returns rows the person is not allowed to see, and nothing warns you.**

Step 3 is safe for the same reason step 4 is not: a cache collection is already materialized, so it can be filtered in full and then paged.

#### Every `pageNumber` tool must say why it is not a cursor

A spec entry that uses page-number paging carries a "Why not a cursor" note of one or two sentences naming the source and the reason.

This is a documentation requirement, not a formality. Cursor paging is the more sophisticated mechanism, and it is what a reader reaches for when they see security filtering on a paged list. Without the note, page number reads as an omission.

The note is nearly always the same: the source is a cache, it is already materialized, so filtering happens across the whole collection before paging and there are no round trips to save. Write it anyway. A reader who has to reconstruct that reasoning will assume it was never done.

#### What the cursor is

An encrypted token holding the sort-key values of the last row on the page. The next page means "everything ordered after this position." It is keyset paging, not offset paging.

- **It must have a unique tiebreaker.** End the ordering with `.ThenBy( x => x.Id )`. Without it, two rows sharing a sort value produce identical cursors and the seek predicate silently skips or repeats a row.
- **It is opaque to the model.** `Encryption.EncryptString`, not plain base64. The agent passes back what it was handed and cannot construct or read one.
- **There is no random access and no total count.** `PaginatedResult` reports `HasMoreItems`, because a seek approach cannot cheaply count.

The usual argument for cursors, that deep pages stay fast, barely applies here. An agent rarely walks past page two, and `Skip(50)` costs nothing. Security is the reason. Stability under concurrent writes is a real secondary benefit for churning data, but it is not what forces the choice.

#### Never cap

**Do not truncate a result to a fixed number of items.** A cap is a correctness hole, not a size optimization. The caller has no way to reach the items past the cap and, in practice, no reliable way to notice they are missing.

Every result set is one of three things, and none of them cap:

1. **Bounded by installed code.** Field types, workflow action components. These move only when a plugin is installed, and then by a handful. Return them whole.
2. **Bounded by data.** Anything that grows as a church adds records. These page.
3. **Bounded by data but structurally not pageable.** A `Get` returns one item and does not page, so it cannot cap either. When such a tool would return an unreasonable payload, **refuse and instruct** rather than truncate: return an error that states the real count and names the parameter that would narrow it.

The refuse-and-instruct pattern already exists. `ConnectionSkill.ListConnectionRequests` does exactly this:

```csharp
if ( !hasAnyFilters )
{
    helper.AddError( "At least one filter parameter must be provided to limit the results returned." );
}
```

An error that says "Person has 347 attributes, narrow with partialName or a qualifier" is useful. A silent cut at 200 is not.

If a collection nested inside a `Get` result is large enough to be a problem, that is a signal the collection should be its own `List` tool, not that the parent should cap it.

**Clipping a single long value is a different thing and is allowed.** A 40 KB Lava template inside an otherwise normal result can be returned clipped, as long as the clip is marked with an `IsTruncated` flag on that value and a companion tool can return it whole. That is visible and recoverable. Dropping rows from a collection is neither, which is what this rule forbids.

**The companion is not optional.** Ship the clipping tool and the tool that returns the value whole together. A clip with no recovery path is data loss wearing a flag.

### 7. Helper methods

`GetRequiredEntity<T>`, `GetOptionalEntity<T>`, `TryGetRequiredEntity<T>`, `WhereOptionalIdKey`, `UpdateProperty`, `UpdateNavigationProperty`, `SetAttributeValues`, `GetAvailableAttributes`, `SaveChangesIfNoErrors`, `AddError`, `AddInstructions`, `HasErrors`, `ErrorResult`.

Writes open their own context with `using var rockContext = RockApp.Current.CreateRockContext();` and construct the helper as `new AgentToolHelper( rockContext, AgentRequestContext, _logger )`. Reads use `new AgentToolHelper( AgentRequestContext, _logger )`.

`SetOrClear<T>` is the parameter type for an optional update that must be able to null a value. Use it wherever clearing is meaningful.

### 8. Filter naming

`partialName` is the established substring filter name (`GroupSkill.ListGroups`). Use it. Do not introduce `searchTerm`.

### 9. Cross-cutting requirements

1. **Configuration skills are admin surface.** Person-level `IsAuthorized` filtering is not sufficient gating for write and delete tools. Gate the skills themselves.
2. **Cache invalidation after every write.** A workflow type saved without cache invalidation runs with no attributes at all and raises no error. The symptom looks like a broken workflow, not a stale cache, which is what makes it expensive. Every write and delete tool must flush the affected caches before returning success.
3. **Chain forward on errors**, matching the existing tools: `.WithInstructions( $"Call the {nameof( LookupWorkflowActionComponents )} function to determine the available actions." )`. Given how many values must come from one specific place, this pattern carries unusual weight in configuration skills.
4. **Never truncate silently.** Results are returned whole, paged, or refused with an error that names the narrowing parameter. A partial answer that looks complete is worse than no answer.
5. **A miss is a result, not an error.** Return `NoData()` and echo what was searched for.
6. **Validate before writing, not after.** Every write tool checks its references against the corresponding lookup tool and returns an error naming the bad value. The first demo's foreign key error named a constraint rather than the bad reference, and that cost more time than the bug did.

## Related

- [260807-ai-agent-core-administration-skill.md](260807-ai-agent-core-administration-skill.md) — applies these conventions across 13 read-only tools.
- [260807-ai-agent-result-guids.md](completed/ai/260807-ai-agent-result-guids.md) — implements the identifier rule in section 5.
