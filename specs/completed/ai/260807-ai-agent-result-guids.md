---
author: Jon Edmiston
date_created: 2026-08-07
summary: >-
  Agent tool results declare a Guid property, document it as required for
  attribute work, and never populate it. AttributeResult has no Guid at all.
  Small additive changes fix both and, in the same helper, stop discarding
  the attribute description, field type, and allowed values that are already
  computed and then dropped.
contributors: []
---

# Expose Entity GUIDs in Agent Tool Results

## Summary

Rock's AI agent tool results have a `Guid` property that is documented as important and is populated nowhere. A second result shape, `AttributeResult`, has no `Guid` property at all.

This spec proposes two small changes: add `Guid` and the other missing descriptive fields to `AttributeResult`, and populate `Guid` in the single-entity `Get` tools. Both are additive or fill a value that is currently null, so nothing breaks.

This is a prerequisite for two planned skills (Core Administration and Workflow Builder), but it is not specific to them. Every existing `Get` tool benefits.

## Motivation

`IdKey` and `Guid` do different jobs, and the agent tools currently only support one of them.

`IdKey` **addresses** an entity in a tool parameter. `Guid` is a **data value the agent has to store inside a field**, because Rock persists several entity references as GUIDs rather than as Ids:

| Where | What holds a GUID |
|---|---|
| `AttributeValue.Value` | For attribute-valued settings, the value *is* an entity GUID |
| `WorkflowActionType.CriteriaAttributeGuid` | A `Guid` column, not an Id |
| `WorkflowActionForm.Actions` | Button definitions embed activity and defined value GUIDs |
| `WorkflowActionForm.ActionAttributeGuid` | A `Guid` column |

The concrete case that surfaced this: the `SetPersonAttribute` workflow action needs a person attribute's GUID in its `PersonAttribute` setting. An agent finds person attributes through `GetPersonAvailableAttributes`, which returns `AttributeResult`, which has no GUID. There is no path today from "find the attribute" to "reference the attribute."

`EntityResultBase` and `KeyNameResult` already declare both properties, so the intent was there. Only the wiring is missing.

## Problem Statement

Two related defects prevent an agent tool from returning an entity's GUID, plus a third that is noted but deliberately left alone.

**1. `EntityResultBase.Guid` is never populated.** `Rock/AI/Agent/Classes/Entity/EntityResultBase.cs:59` declares it with this doc comment:

```csharp
/// <summary>
/// The unique identifier of the entity. This should be filled in whenever
/// possible as it is required when dealing with attribute values.
/// </summary>
public Guid? Guid { get; set; }
```

52 result classes derive from `EntityResultBase`. **Zero result builders assign the property.** A repo-wide search for an assignment returns exactly one hit, inside `KeyNameResult.FromEntity` (`Rock/AI/Agent/Classes/Common/KeyNameResult.cs:134`); every other apparent match is a different property such as `MaritalStatusGuid` or `GroupRoleGuid`. Every result in every skill currently serializes `guid: null`, including the ones whose own documentation says the value is required.

**2. `AttributeResult` has no `Guid` at all.** `Rock/AI/Agent/Classes/Entity/AttributeResult.cs:23` carries `Name`, `Key`, `ValueFormat`, `IsRequired`, and `IsReadOnly`. It does not derive from `EntityResultBase`, so unlike the other 52 it has no `Guid` to populate. It is the return shape of all four existing `Get{Entity}AvailableAttributes` tools.

### A known trap that is deliberately left alone

`KeyNameResult`'s three-argument constructor (`Rock/AI/Agent/Classes/Common/KeyNameResult.cs:111`) accepts a `guid` and never assigns it:

```csharp
public KeyNameResult( int id, Guid guid, string name )
{
    Id = id;
    Name = name;
}
```

Any caller reaching for this overload gets a null `Guid` while believing it set one.

**It is not fixed here.** The only results that would need it are `List` and `Lookup` results, which do not carry `Guid` at all by the scope rule below, so fixing it would change nothing this spec cares about. It has no callers today.

Build `KeyNameResult` references with `FromEntity` or an object initializer that sets `Guid` explicitly. A test asserts the constructor's behavior so the trap is visible rather than surprising. If `List` results ever do carry `Guid`, fix the constructor then.

## Root Cause

Defect 1 is an omission rather than a design decision. `KeyNameResult.FromEntity` (`Rock/AI/Agent/Classes/Common/KeyNameResult.cs:130`) sets `Guid` correctly, which shows the intent, but result builders across the skills use object initializers that set `Id` and `Name` and stop there. Nothing in the type system or the tests catches a missing optional property, so the omission spread silently as skills were added.

Defect 2 is a shape that was never revisited. `AttributeResult` predates the need to reference an attribute by GUID, and because it does not derive from `EntityResultBase` it did not inherit the property when the base class gained it.

## Requirements

- `AttributeResult` MUST expose a nullable `Guid`, populated for every attribute it describes.
- `AttributeResult` MUST expose the attribute's description, order, field type, and allowed values where the field type supplies them.
- `Get{Entity}AvailableAttributes` tools MUST keep returning `helper.GetAvailableAttributes` directly. A tool needing more MUST extend `AttributeResult` in core rather than building its own results, because building its own means duplicating the helper's visibility and authorization filters.
- Single-entity `Get` tools MUST populate `Guid` on the result they build.
- Nested `KeyNameResult` references inside those results MUST carry `Guid`, set with `FromEntity` or an explicit object initializer. The three-argument constructor MUST NOT be used; it discards its `guid` argument.
- A shared result factory used by both `Get` and `List` paths MUST NOT be changed. The `Get` tool sets `Guid` on its own result after calling the factory.
- `List` and `Lookup` tools MUST NOT populate `Guid`, with one structural exception described below.
- All changes MUST be additive. No existing tool signature or result property may change meaning.

## Proposed Fix

### 1. Add `Guid` to `AttributeResult`

`Rock/AI/Agent/Classes/Entity/AttributeResult.cs`

```csharp
/// <summary>
/// The unique identifier of the attribute. Required when a value must
/// reference this attribute, which is how workflow action settings and
/// criteria store attribute references.
/// </summary>
public Guid? Guid { get; set; }
```

Populate it in `AgentToolHelper.GetAvailableAttributes` (`Rock/AI/Agent/AgentToolHelper.cs:662`), which is the single place every `AttributeResult` is constructed. All four existing `Get{Entity}AvailableAttributes` tools gain the property with no per-tool work.

### 2. Add the remaining descriptive fields to `AttributeResult`

`AttributeResult` carries `Name`, `Key`, `ValueFormat`, `IsRequired`, and `IsReadOnly`. That is not enough for a caller deciding what to put in an attribute: it cannot see the attribute's description, its field type, or its allowed values.

Every missing field is **already inside the helper's own loop**, on the `AttributeCache` it is iterating, and is discarded:

| Add | Source | Notes |
|---|---|---|
| `Description` | `AttributeCache.Description` | The single most useful field for choosing a value |
| `Order` | `AttributeCache.Order` | An int |
| `FieldType { IdKey, Name }` | `AttributeCache.FieldType` | A caller cannot currently tell a Person field from a Text field |
| `Values`, `IsCompleteList` | `FieldTypeHints` | **Already computed and thrown away** |

The last row is the sharpest. `GetAvailableAttributes` calls `fieldType.GetFieldHints( a.ConfigurationValues )` and reads `hints.ValueFormat` while discarding `hints.Values` (a `List<ListItemBag>`) and `hints.IsCompleteList` on the same object. The work is done; the result is dropped.

This ships with the `Guid` addition because it is the same class, the same method, and the same commit.

**The payload concern is real but bounded.** `Description` grows the result on entity types with many attributes, Person being the outlier. It is usually short or empty, and `GetEntityAvailableAttributes` pages at 200 for this reason. If it proves heavy in practice, drop `Description` rather than the rest.

**A dependent gap that is not fixed here.** `FieldType.GetFieldHints()` is `virtual` returning `null` on the base class and is overridden by **4 of the 173 field types**, none of them Single Select. So `Values` will be populated for almost nothing until those overrides exist. Implementing it across all 173 is a separate Rock core project. Implementing it on the roughly eight field types with genuinely enumerable values (single select, multi select, boolean, defined value, and their kin) is small and covers nearly all agent authoring. Recommended as an immediate follow-up, not a prerequisite.

### 3. Populate `Guid` in `Get` detail results

Every `Get` tool that returns one entity sets `Guid` on the result it builds:

`GetConnectionRequest`, `GetContentChannel`, `GetContentChannelItem`, `GetEventItemOccurrence`, `GetRegistrationInstance`, `GetRegistrationRegistrant`, `GetBenevolenceRequest`, `GetNote`, `GetPersonProfile`, `GetMyProfile`, `GetCurrentPerson`.

The remaining `Get` tools return summaries, insights, or attribute definitions. Summaries and insights are aggregates with no single source entity; attribute definitions are covered by changes 1 and 2.

**Nested `KeyNameResult` references inside those results carry `Guid` too**, set in the object initializer.

Do **not** convert those initializers to `KeyNameResult.FromEntity`. `FromEntity` derives the name from `entity.ToString()`, so replacing `Name = x.Name` with it would silently change the rendered name wherever the two differ. It is also unusable inside a LINQ to SQL projection, which its own doc comment states, and several of these sites are projections. `FromEntity` stays correct where a reference is being built fresh from an entity in hand.

**Two of the eleven route through a shared factory.** `GetCurrentPerson` builds its result with `PersonResult.Basic`, which is also used for nested person references inside list results. Changing the factory would leak `Guid` into those lists, so `GetCurrentPerson` sets it on its own result afterward instead. `GetPersonProfile` and `GetMyProfile` both use `PersonSkill.GetPrimaryPersonResult`, which is only ever a `Get` path, so that one is changed directly.

### Scope: `Get` yes, `List` no

**`Get` detail tools populate `Guid`. `List` and `Lookup` tools do not.**

A list result carries IdKey, Name, and whatever is needed to choose between rows. A GUID is 36 characters on every row, roughly 1.8 KB on a 50-row page, spent on something the caller usually does not need yet.

This holds even where it looks wrong. `ListDefinedValues` is the tempting case, because an agent browsing it is often choosing a defined value precisely in order to write that GUID into a workflow action setting. It still does not carry one. The agent picks a value out of the page and calls `GetDefinedValue` on it, paying one extra call for the row it actually uses rather than a GUID on every row of every page it discards.

Holding the line matters more than the single call it costs. A per-tool judgment about whether the GUID is "the thing being selected" is exactly the kind of rule that erodes: every list has some caller who wants a GUID, and the shape of `List` results stops being predictable.

**The one exception is structural: a `List` with no `Get` partner.** `ListEntityTypes` in the planned Core Administration skill is the only current case. There is no `GetEntityType`, so dropping `Guid` would put it out of reach entirely rather than one call away. The test is mechanical, not a judgment about how badly a caller wants the value. If the partner `Get` exists, the list does not carry it.

## Affected Code Paths

**Primary, where the fix lands:**

- `Rock/AI/Agent/Classes/Entity/AttributeResult.cs` — new properties
- `Rock/AI/Agent/AgentToolHelper.cs:662` — populate them

**Secondary, one result builder each:**

- The 11 single-entity `Get` tools listed in change 3, across `ConnectionSkill`, `ContentChannelSkill`, `EventCalendarSkill`, `EventRegistrationSkill`, `FinanceSkill`, `NoteSkill`, `PersonSkill`, `AttendeeSkill`, and `SystemUtilitySkill`.

**Downstream consumers:** none. Nothing outside the agent skills reads these result classes.

## Fix Risks

**Payload.** 36 characters per entity on `Get` results. Negligible; a `Get` returns one entity.

**Security.** None. A GUID is not a secret. It is already visible in Rock's UI and URLs, and `IdKey` is already returned everywhere. Exposing it grants no access that authorization does not already govern.

**Backward compatibility.** None broken. Both changes are additive or fill a property that is currently null. No consumer can be relying on `guid: null` as meaningful, and no method signature changes.

**The one thing to watch.** `Description` grows every `AvailableAttributes` result, and Person is the outlier with hundreds of attributes. It is usually short or empty, and `GetEntityAvailableAttributes` pages at 200 partly for this reason, but it is the field to drop first if payload proves heavy.

## Verification Steps

1. Assert `AgentToolHelper.GetAvailableAttributes` returns a non-null `Guid` for every attribute it describes, along with `Description`, `Order`, and `FieldType`.
2. Assert `new KeyNameResult( 1, someGuid, "x" ).Guid` is **null**, pinning the known trap so the behavior is documented rather than surprising.
3. For each of the 11 tools in change 3, assert the returned `guid` matches the source entity's `Guid`.
4. Grep for `new KeyNameResult {` object initializers that set `Id` and `Name` but not `Guid`. Add `Guid` to the ones reached by a `Get` detail tool. Leave the rest alone: several are inside `List` tools, where `Guid` is out of scope by the rule above.
5. Confirm no `List` or `Lookup` result gained a `Guid` as a side effect of step 4.
6. Confirm `PersonResult.Basic` was **not** changed. It is shared between `GetCurrentPerson`, which is a `Get`, and nested person references inside list results. `GetCurrentPerson` sets `Guid` on the result after calling the factory so the list paths are untouched.

## Out of Scope

- Populating `Guid` on `List` and `Lookup` results. Covered by the scope rule above.
- Summary and insight results. They aggregate across entities and have no single GUID to report.
- Making `EntityResultBase.Guid` non-nullable. See Open Questions.

## Considered but Rejected

### Make `AttributeResult` derive from `EntityResultBase`

Rejected. It would drag in `CreatedDateTime`, `ModifiedDateTime`, `CreatedByPerson`, `ModifiedByPerson`, and `AttributeValues`, none of which make sense on an attribute definition. One added property is the right size of change.

### Add an `includeGuids` parameter to `List` tools

Rejected for now. It would let a caller opt in per call without weakening the scope rule, but it is a parameter that exists to solve a problem nobody has reported. Worth revisiting if the extra `Get` call per selected row shows up as real chattiness once the Workflow Builder skill is running.

### Carry `Guid` on `ListDefinedValues` as a one-off exception

Rejected. It is the strongest case for a content-based exception and it is still not strong enough. Once "is the GUID the thing being selected here?" becomes the test, the shape of a `List` result stops being predictable and every list acquires an advocate. The structural test (does a partner `Get` exist?) is the only one that stays decidable.

## Open Questions

1. **Should `EntityResultBase.Guid` be non-nullable?** It is `Guid?` today, which makes "not populated" and "no GUID" look identical. Making it required would surface omissions at compile time, but it is a breaking change across 52 classes and probably not worth it.
2. **Should the 11 `Get` tools be covered by a shared helper rather than 11 edits?** A `PopulateEntityFields( result, entity )` helper would prevent the next omission, but it touches result construction in nine skills. Worth considering if a fourth defect of this shape appears.

## Related

- [260807-ai-agent-core-administration-skill.md](../../260807-ai-agent-core-administration-skill.md) — depends on this spec; its `GetEntityAvailableAttributes` and `GetDefinedValueAvailableAttributes` tools require `AttributeResult.Guid`.
- [260807-ai-agent-tool-conventions.md](../../260807-ai-agent-tool-conventions.md) — the shared conventions both new skills follow, including the identifier rules this spec implements.
