---
author: Jon Edmiston
date_created: 2026-04-28
summary: >-
  After upgrading System.Linq.Dynamic.Core from 1.6.6 to 1.7.0, Lava entity
  commands that call .ToString() on a property whose name matches a CLR
  predefined type (Guid, DateTime, TimeSpan, etc.) inside a select: clause
  now fail to parse. Restore prior behavior by passing a ParsingConfig to
  the Dynamic.Core calls in RockEntityBlock.
contributors:
  - Daniel Hazelbaker
  - Kyle Henning
---

# Lava Entity Command `.ToString()` Regression in `select:` Clauses

## Summary

A recent NuGet package bump from `System.Linq.Dynamic.Core` 1.6.6 to 1.7.0 introduced a parsing regression in Rock's `{% entity %}` Lava commands. Property tokens whose name collides with a CLR predefined type (most commonly `Guid`) are now resolved as the type instead of the entity property, so `Guid.ToString()` becomes a static method call on `System.Guid` and fails. The fix is to mirror the existing `EntitySearchHelper` pattern by passing a shared `ParsingConfig` into every Dynamic.Core call site in `RockEntityBlock`.

## Requirements

- Lava entity commands MUST continue to support `.ToString()` (and other inherited `object` methods) on entity properties inside `select:`, `where:`, `groupby:`, and `selectmany:` clauses.
- Property identifiers whose names collide with CLR predefined types (`Guid`, `DateTime`, `TimeSpan`, `String`, etc.) MUST resolve to the entity property, matching pre-1.7.0 behavior.
- The fix MUST NOT alter the behavior of the Entity Search subsystem, which already passes its own `ParsingConfig`.
- The fix MUST apply to both the Fluid execution path (`RockEntityBlock.cs`) and the legacy DotLiquid path (`RockLiquid/Blocks/RockEntity.cs`) for Rock versions where DotLiquid is still active.

## Problem Statement

Lava authors who use `{% campus %}`, `{% person %}`, or any other `{% entity %}` block with a `select:` clause that calls `.ToString()` on the entity's `Guid` property are now seeing parse errors that did not occur in prior Rock versions. The same regression affects any property name that shadows a predefined CLR type.

## Reproduction

```liquid
{% campus where:'IsActive == true' select:'new ( Id, Guid.ToString() AS Guid, Name )' sort:'Name' securityenabled:'false' %}
{% endcampus %}
```

Worked under `System.Linq.Dynamic.Core` 1.6.6. Fails under 1.7.0.

Affected versions: any Rock build that pulls `System.Linq.Dynamic.Core` 1.7.0 or later. The regression entered `develop` with commit `fcb660f62f` on 2025-11-17.

## Root Cause

`select:` strings are parsed by `System.Linq.Dynamic.Core` at [Rock/Lava/Blocks/RockEntityBlock.cs:501](../Rock/Lava/Blocks/RockEntityBlock.cs):

```csharp
resultsQry = queryResult.Cast( entityType ).Select( parms["select"] );
```

No `ParsingConfig` is supplied, so library defaults apply.

In 1.7.0 the parser tightened identifier resolution. The bare token `Guid` is ambiguous between two interpretations:

1. The predefined CLR type `System.Guid` (in Dynamic.Core's default predefined-types set).
2. The Campus entity's `Guid` property exposed via the implicit `it` parameter.

Under 1.7.0 defaults, the predefined type wins. `Guid.ToString()` is therefore parsed as a static call on `System.Guid` (which has no parameterless static `ToString()`), not as `it.Guid.ToString()`. The parser throws.

The same regression affects any property whose name matches a predefined type: `DateTime`, `DateTimeOffset`, `TimeSpan`, `String`, `Math`, `Convert`, `Uri`, the numeric type aliases, and so on.

A secondary contributor: `ParsingConfig.AllowEqualsAndToStringMethodsOnObject` defaults to `false`, which blocks inherited `object.ToString()` and `object.Equals()` calls in some scenarios.

## Affected Code Paths

Primary (where the fix lands):

- [Rock/Lava/Blocks/RockEntityBlock.cs](../Rock/Lava/Blocks/RockEntityBlock.cs), the Fluid engine path for `{% entity %}` blocks. Four Dynamic.Core call sites: `.Where(...)`, `.GroupBy(...)`, `.Select(...)`, `.SelectMany(...)`.
- [Rock/Lava/RockLiquid/Blocks/RockEntity.cs](../Rock/Lava/RockLiquid/Blocks/RockEntity.cs), the legacy DotLiquid sibling. Same four call sites.

Secondary (verify unaffected):

- [Rock/Core/EntitySearch/EntitySearchHelper.cs:45](../Rock/Core/EntitySearch/EntitySearchHelper.cs), already passes its own `ParsingConfig` and is not subject to this regression.

## Workarounds

User-side: disambiguate the property reference with the `it.` prefix.

```liquid
select:'new ( Id, it.Guid.ToString() AS Guid, Name )'
```

This forces property-on-`it` resolution and bypasses the type lookup. It works without any code change but requires every existing Lava template author to find and update affected expressions.

## Proposed Fix

Define a shared `ParsingConfig` on each entity block class and pass it into every Dynamic.Core call site:

```csharp
private static readonly ParsingConfig _parsingConfig = new ParsingConfig
{
    PrioritizePropertyOrFieldOverTheType = true,
    AllowEqualsAndToStringMethodsOnObject = true
};
```

Apply at the four parsing sites in each file:

- `.Where( _parsingConfig, ... )`
- `.GroupBy( _parsingConfig, ... )`
- `.Select( _parsingConfig, ... )`
- `.SelectMany( _parsingConfig, ... )`

Settings rationale:

| Setting | Reason |
|---|---|
| `PrioritizePropertyOrFieldOverTheType = true` | Restores pre-1.7.0 behavior. Instance properties on `it` win over predefined types of the same name. This is the actual regression fix. |
| `AllowEqualsAndToStringMethodsOnObject = true` | Re-enables inherited `ToString()` and `Equals()` calls. Lava authors have always been able to call these. |

## Fix Risks

- The 1.7.0 default exists for security hardening. Letting properties shadow types is a small attack surface (a maliciously named property could shadow a type the author intended to reference). For Lava entity commands this is a non-issue: the entity model is fixed and authored by Rock core, not by template authors.
- `AllowEqualsAndToStringMethodsOnObject = true` permits calling `.ToString()` and `.Equals()` on any reference type during parsing. This was already implicitly allowed pre-1.7.0, so no new attack surface is opened.
- No public API surface change. The `ParsingConfig` is a private static field.
- No effect on Entity Search, which already configures its own parser.
- EF6 may still fail to translate `Guid.ToString()` to T-SQL even after the parser accepts it. That is a separate concern (see Out of Scope).

## Verification Steps

1. Confirm reproduction on `develop` at the current 1.7.0 version, without the fix, using the Lava sample in the Reproduction section.
2. Apply the `ParsingConfig` to `RockEntityBlock` and rerun the sample. Confirm it parses and returns expected rows.
3. Regression sweep against representative `{% entity %}` patterns:
   - `select:` with anonymous `new (...)` projections that reference `Guid`, `DateTime`, and at least one non-colliding property.
   - `where:` clauses referencing properties named `DateTime` or `Guid`.
   - `groupby:` plus `select:` aggregations.
   - `selectmany:` with a navigation property.
4. Confirm Entity Search flows behave identically before and after by running representative entity searches.
5. Repeat verification against the DotLiquid path in `RockEntity.cs` for builds where DotLiquid is still active.
6. Run existing Lava and entity-related unit tests.

## Out of Scope

- EF6 SQL translation of `Guid.ToString()`. If the parser accepts the expression but EF6 cannot translate it, that is a separate issue with separate options (materialize-then-project, `SqlFunctions.StringConvert`, etc.).
- Removal of the legacy `RockEntity.cs` DotLiquid path. Tracked separately.
- Broader review of which `ParsingConfig` defaults Rock should adopt across other Dynamic.Core consumers in the codebase.

## Considered but Rejected

### Roll back `System.Linq.Dynamic.Core` to 1.6.6

Rejected. The 1.7.0 bump rode in on the broader NuGet update commit (`fcb660f62f`) that pulled in security fixes and dependency alignment across the solution. Reverting one package risks dependency conflicts and forfeits unrelated fixes. Adopting an explicit `ParsingConfig` is a smaller, more durable change.

### Document the `it.` prefix as the supported pattern and close the issue as "by design"

Rejected. Pushes a breaking change onto every Lava template author in the field, including plugins and customer-authored templates we do not control. Backward compatibility takes priority for a behavior that worked across many shipped versions.

### Pass a `ParsingConfig` only to `.Select(...)`, since the report mentions `select:` only

Rejected. The same identifier-resolution rule applies to `Where`, `GroupBy`, and `SelectMany`. Skipping the other three sites just defers the same bug to the next reporter.

### Use a custom `IDynamicLinqCustomTypeProvider` to remove `Guid` and friends from the predefined-types set

Rejected. Solves the symptom (the type is not findable) but introduces a divergent type-resolution surface from Entity Search and surprises any Lava expression that legitimately wants to reference a CLR type. The `PrioritizePropertyOrFieldOverTheType` flag is the supported, documented lever.

## Related

- `fcb660f62f`, "First pass on updating NuGet packages" (2025-11-17), the commit that introduced the regression.
- `cc92ca68a6`, "update to System.Linq.Dynamic.Core" (2021-05-21), the original switch from `System.Linq.Dynamic` to `System.Linq.Dynamic.Core`.
- [Rock/Core/EntitySearch/EntitySearchHelper.cs](../Rock/Core/EntitySearch/EntitySearchHelper.cs), reference implementation for `ParsingConfig` usage.
- [Rock/Core/EntitySearch/DynamicLinqCustomTypeProvider.cs](../Rock/Core/EntitySearch/DynamicLinqCustomTypeProvider.cs), companion type provider used by Entity Search.
