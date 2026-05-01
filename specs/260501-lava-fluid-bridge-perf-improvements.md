---
author: Jon Edmiston
date_created: 2026-05-01
summary: >-
  Performance and allocation review of the Lava → Fluid bridge introduced when
  Rock moved from DotLiquid to Fluid. Catalogs hot-path reflection, redundant
  per-render parsing, async-over-sync allocations, and a few latent thread
  safety bugs, with a checkbox per finding so reviewers can pick what to act on.
contributors:
  - Daniel Hazelbaker
---

# Lava → Fluid Bridge: Performance and Allocation Improvements

## Summary

When Rock moved Lava rendering from DotLiquid to Fluid, an abstraction layer was added in `Rock.Lava`, `Rock.Lava.Shared`, and `Rock.Lava.Fluid` to bridge how Rock-flavored Lava maps onto Fluid's parser, AST, and value model. The bridge is functionally correct, but a review of the hot paths surfaced several reflection-per-call patterns, repeated parsing of the same content, redundant per-render allocations, and a few thread-safety bugs that also have a perf cost. This spec catalogs each finding so the team can decide which to address.

Each finding is a checkbox. Tick the items the team agrees to address; leave the rest for later or for explicit rejection.

## Motivation

Lava rendering is one of the most frequently invoked code paths in Rock. Every page render, every communication template, every mobile shell, every check-in label, every workflow attribute that supports Lava ends up in this bridge. Even small per-call costs (one extra reflection lookup, one extra `Task` allocation) compound into measurable CPU and GC pressure under production load. Fixing the items in this spec should reduce both render latency and allocation rate for Rock instances of any size, with the largest wins on shortcode-heavy templates and entity-driven communications.

The findings below were collected by static review of the bridge code only. None of them have been benchmarked yet; the priority ordering reflects expected impact based on call frequency and the cost of the operation involved. A reviewer who disagrees with a priority is encouraged to call it out before implementation.

## Requirements

- Each finding MUST be addressable independently. No PR should be forced to bundle multiple findings unless explicitly noted as related.
- Functional behavior of Lava rendering MUST remain identical. These are perf and allocation changes, not feature changes.
- Public APIs in `Rock.Lava`, `Rock.Lava.Shared`, and `Rock.Lava.Fluid` MUST stay backward compatible (per Rock's prime directive). Internal helpers and private fields are fair game.
- Thread-safety fixes MUST be made even if the perf impact is negligible. Correctness wins over perf.
- Each accepted finding SHOULD be benchmarked before and after to confirm the expected improvement, using a representative template.

## Findings

Each finding has a checkbox, an estimated impact, an affected location, and a proposed fix. Reviewers should tick the items the team agrees to address.

### P0 — Major hot-path issues

#### [ ] F1. Block content is re-parsed on every render

**Where:** [Rock.Lava.Fluid/Parser/FluidLavaBlockStatement.cs:141-174](../Rock.Lava.Fluid/Parser/FluidLavaBlockStatement.cs)

`ILiquidFrameworkElementRenderer.Render` calls `_parser.Grammar.Parse(blockContext, ref parseResult)` against the block's source text every time the block is rendered. `WriteToAsync` separately calls `LavaFluidParser.ParseToTokens(_blockContent)` on every render at line 109. For any template that uses shortcodes or custom blocks (most Rock templates) this is the largest single sink in the pipeline.

**Proposed fix:** Parse `_blockContent` once at construction (or lazily under `Lazy<T>`) and cache `IReadOnlyList<Statement>` plus the token list as fields. Wrap the synchronous `task.Wait()` at line 164 in the same `IsCompletedSuccessfully` fast-path used in `FluidEngine.OnRenderTemplate` to avoid the `Task` allocation on the hot path.

#### [ ] F2. LavaDataWrapper does reflection on every property access

**Where:** [Rock.Lava.Shared/Core/LavaDataWrapper.cs:103-113](../Rock.Lava.Shared/Core/LavaDataWrapper.cs)

```csharp
public object GetValue( string key ) {
    var property = _baseObject.GetType().GetProperty( key );
    if ( property == null ) return null;
    var value = property.GetValue( _baseObject );
    return GetWrappedObject( value );
}
```

`Type.GetProperty` plus `PropertyInfo.GetValue` runs on every merge-field lookup. `GetAvailableKeys` (line 132) likewise calls `GetProperties()` per wrapper instance instead of caching by Type.

**Proposed fix:** Static `ConcurrentDictionary<Type, Dictionary<string, Func<object, object>>>` of compiled getters via `Expression.Lambda` or `Delegate.CreateDelegate(getMethod)`. Roughly 10x faster than `PropertyInfo.GetValue` and zero allocation per call.

#### [ ] F3. LavaTypeMemberAccessor uses PropertyInfo.GetValue per call

**Where:** [Rock.Lava.Fluid/LavaObjectMemberAccessStrategy.cs:228-233](../Rock.Lava.Fluid/LavaObjectMemberAccessStrategy.cs)

Each `[LavaType]`-decorated class registers a `LavaTypeMemberAccessor` per property; that accessor stores `PropertyInfo` and uses `_info.GetValue(obj)` per call. Same pattern, same fix.

**Proposed fix:** Compile the property getter into a `Func<object, object>` once in the constructor and call the delegate at access time.

#### [ ] F4. LavaDataHelper.GetLavaTypeInfo recomputes per call and is uncached

**Where:** [Rock.Lava.Shared/Utility/LavaDataHelper.cs:37-76](../Rock.Lava.Shared/Utility/LavaDataHelper.cs)

`GetLavaTypeInfo` invokes `type.GetProperties()` up to three times per call (lines 55, 61, 66) and is called from `LavaDataObjectInternal.GetInstanceProperties`. The Internal helper caches per-instance, but the underlying type info is not cached globally, so each new instance pays the full reflection cost.

**Proposed fix:** Back the lookup with a `ConcurrentDictionary<Type, LavaDataTypeInfo>` and call `GetProperties()` only once per type.

#### [ ] F5. ToRealObjectValue uses reflection per dictionary unwrap

**Where:** [Rock.Lava.Fluid/FluidExtensions.cs:229-238](../Rock.Lava.Fluid/FluidExtensions.cs)

```csharp
if ( value is DictionaryValue ) {
    var dictionary = value.ToObjectValue();
    var fieldInfo = dictionary.GetType().GetField( "_dictionary",
        BindingFlags.NonPublic | BindingFlags.Instance );
    return fieldInfo.GetValue( dictionary );
}
```

`ToRealObjectValue` is invoked from `FluidRenderContext.GetFieldPrivate`, `GetScopeDefinedValues`, `ConvertFluidFilterArguments`, and elsewhere. Reflecting on the same private field over and over is pure waste.

**Proposed fix:** Cache the `FieldInfo` (or, better, a compiled `Func<DictionaryValue, object>`) in a `static readonly` field initialized once.

#### [ ] F6. Filter invocation uses MethodInfo.Invoke and per-call array

**Where:** [Rock.Lava.Fluid/FluidEngine.cs:441-501](../Rock.Lava.Fluid/FluidEngine.cs)

The wrapper allocates `new object[lavaFilterMethodParameters.Length]` per filter call and uses `lavaFilterMethod.Invoke(null, args)`. Templates can apply filters thousands of times per render.

**Proposed fix:** Build a compiled `Func<object[], object>` (or strongly-typed delegate) at registration via `Expression.Lambda(...).Compile()`. The arg array still allocates, but the reflection invoke cost is gone. If we want to push further, we can generate per-arity wrappers to remove the array allocation.

Also at [line 397](../Rock.Lava.Fluid/FluidEngine.cs): the LINQ chain `OrderBy(x => x.Name).ThenByDescending(x => x.GetParameters().Count())` invokes `GetParameters()` O(n log n) times during sort. Materialize `(method, parameters)` tuples first.

### P1 — Thread-safety bugs (with perf side-effects)

#### [ ] F7. Per-render mutation of shared TemplateOptions

**Where:** [Rock.Lava.Fluid/FluidEngine.cs:658-665](../Rock.Lava.Fluid/FluidEngine.cs)

```csharp
if ( parameters.Culture != null )
    templateContext.FluidContext.Options.CultureInfo = parameters.Culture;
if ( parameters.TimeZone != null )
    templateContext.FluidContext.Options.TimeZone = parameters.TimeZone;
```

`templateContext.FluidContext.Options` is the engine's single `_templateOptions` instance. Two concurrent renders with different cultures will race; one render can observe the other's culture mid-render. This is a correctness bug first, perf bug second.

**Proposed fix:** Set the values on the `TemplateContext` itself (`fluidContext.CultureInfo`, `fluidContext.TimeZone`) rather than on the shared `Options`.

#### [ ] F8. _map and factory dictionaries read lock-free while being written

**Where:**
- [Rock.Lava.Fluid/LavaObjectMemberAccessStrategy.cs:32](../Rock.Lava.Fluid/LavaObjectMemberAccessStrategy.cs)
- [Rock.Lava.Fluid/Parser/FluidLavaTagStatement.cs:32](../Rock.Lava.Fluid/Parser/FluidLavaTagStatement.cs)
- [Rock.Lava.Fluid/Parser/FluidLavaBlockStatement.cs:36](../Rock.Lava.Fluid/Parser/FluidLavaBlockStatement.cs)

`_map` and the `_factoryMethods` dictionaries are `Dictionary<>` (not concurrent). Tag/block factory methods are written under a `lock` but read without one during render. Most registrations happen at startup, but dynamic shortcodes can be re-registered at runtime, so the race is real.

**Proposed fix:** Switch all three to `ConcurrentDictionary<>`. While here, replace the `ContainsKey` followed by `[]` indexing in both factory wrappers with a single `TryGetValue`.

#### [ ] F9. GetTemplateOptions is not thread-safe

**Where:** [Rock.Lava.Fluid/FluidEngine.cs:235-262](../Rock.Lava.Fluid/FluidEngine.cs)

The `if (_templateOptions == null) { ... }` initialization can let two threads each construct a `TemplateOptions`, register filters twice, and lose one. Not a hot path, but a real startup race.

**Proposed fix:** `Lazy<TemplateOptions>` or a lock with a double-check.

### P2 — Per-context / per-render allocations

#### [ ] F10. FluidRenderContext constructor sets four constants on every render

**Where:** [Rock.Lava.Fluid/FluidRenderContext.cs:38-48](../Rock.Lava.Fluid/FluidRenderContext.cs)

`Blank/blank/Empty/empty` get `SetValue` calls on every new context. Since `ModelNamesComparer` is `StringComparer.Ordinal`, both case variants are needed, but they're constants and don't have to be set per render.

**Proposed fix:** Register the four values once on the engine's `TemplateOptions` so each new context inherits them.

#### [ ] F11. Reflection-based access to TemplateContext.LocalScope per read

**Where:** [Rock.Lava.Fluid/FluidRenderContext.cs:204](../Rock.Lava.Fluid/FluidRenderContext.cs)

The `PropertyInfo` is cached as `static`, but `_contextScopeInternalField.GetValue(_context)` is invoked at lines 131 and 252 on every merge-field read/write. `PropertyInfo.GetValue` is slow.

**Proposed fix:**

```csharp
private static readonly Func<TemplateContext, Scope> _getLocalScope =
    (Func<TemplateContext, Scope>) Delegate.CreateDelegate(
        typeof( Func<TemplateContext, Scope> ),
        _contextScopeInternalField.GetGetMethod( true ) );
```

Then call `_getLocalScope(_context)` at the call sites.

#### [ ] F12. ParseTemplate copies the Statements list for no apparent reason

**Where:** [Rock.Lava.Fluid/FluidEngine.cs:633](../Rock.Lava.Fluid/FluidEngine.cs)

```csharp
template = new FluidTemplate( new List<Statement>( fluidTemplateObject.Statements ) );
```

The parser already returned a `FluidTemplate`. We're copying its statement list into another `FluidTemplate`.

**Proposed fix:** Return `fluidTemplateObject` directly. If there is a real reason for the copy that is not visible in the surrounding code, document it inline.

#### [ ] F13. Async-over-sync allocations

**Where:**
- [Rock.Lava.Fluid/FluidEngine.cs:701-705](../Rock.Lava.Fluid/FluidEngine.cs) — `task.AsTask().GetAwaiter().GetResult()`. ValueTask has its own `GetAwaiter()`; drop `AsTask()` to skip the Task allocation.
- [Rock.Lava.Fluid/FluidExtensions.cs:317](../Rock.Lava.Fluid/FluidExtensions.cs) — `arg.Expression.EvaluateAsync(context).Result`. `.Result` wraps exceptions in `AggregateException` and forces Task materialization. Use `.GetAwaiter().GetResult()`.
- See also F1 — same pattern in `FluidLavaBlockStatement` render.

**Proposed fix:** Use ValueTask's own awaiter where possible and the `IsCompletedSuccessfully` fast-path everywhere a sync-over-async pattern exists.

#### [ ] F14. GetScopeAggregatedValues allocates two dictionaries per call

**Where:** [Rock.Lava.Fluid/FluidRenderContext.cs:284-323](../Rock.Lava.Fluid/FluidRenderContext.cs)

`GetScopeDefinedValues` returns its own dictionary, then `GetScopeAggregatedValues` allocates another and copies. The inner method also uses `properties.Where(...)` which allocates a deferred enumerator.

**Proposed fix:** Inline the prefix check and write directly into a single dictionary.

#### [ ] F15. Small but called every render

These are individually cheap but called per render or per merge-field operation.

- [ ] **F15a.** [FluidRenderContext.cs:167](../Rock.Lava.Fluid/FluidRenderContext.cs) — `",".ToCharArray()` allocates per call. Use a `private static readonly char[]` or the `Split(',')` overload.
- [ ] **F15b.** [LavaToLiquidTemplateConverter.cs:106](../Rock.Lava/Engine/LavaToLiquidTemplateConverter.cs) — `inputTemplate?.Replace("elseif", "elsif")` always allocates a new string. Guard with `IndexOf("elseif", StringComparison.Ordinal) < 0` first.
- [ ] **F15c.** [LavaDataObject.cs:788](../Rock.Lava.Shared/Core/LavaDataObject.cs) — `propPath.Split('.').ToList()` then `propPath.First()` and `propPath.Skip(1).ToList()` per traversal step. For path "a.b.c" this allocates 6+ lists. Replace with an int cursor and `IndexOf('.')`.
- [ ] **F15d.** [LavaDataObject.cs:973-990](../Rock.Lava.Shared/Core/LavaDataObject.cs) — `GetDynamicMemberNames` allocates a new list; `AvailableKeys` then `.ToList()` again. Cache the property name list per type.
- [ ] **F15e.** [LavaObjectMemberAccessStrategy.cs:127-134](../Rock.Lava.Fluid/LavaObjectMemberAccessStrategy.cs) — `RegisterLavaTypeProperties` calls `type.GetProperties()` three times. Materialize once.
- [ ] **F15f.** [LavaObjectMemberAccessStrategy.cs:66](../Rock.Lava.Fluid/LavaObjectMemberAccessStrategy.cs) — `type.Name.Contains("AnonymousType")` per first-time access. Cache per-Type.

### P3 — Worth knowing, low cost

#### [ ] F16. Value converter ordering and basic-type short-circuit

**Where:** [Rock.Lava.Fluid/FluidEngine.cs:106-107](../Rock.Lava.Fluid/FluidEngine.cs)

The existing TODO already calls this out. Adding `if (value is string || value is int || value is bool) return null;` at the top of each converter would skip O(n_converters) per common value.

#### [ ] F17. LavaDataDictionary.AvailableKeys allocates per call

**Where:** [Rock.Lava.Shared/Core/LavaDataDictionary.cs:365](../Rock.Lava.Shared/Core/LavaDataDictionary.cs)

Returns `new List<string>(_dictionary.Keys)` every call. If the engine reads it more than once during a render, that's wasted. Note: changing the return type would break the public API, so the fix is either to expose an alternate accessor or to cache the materialized list and invalidate on writes.

#### [ ] F18. LavaDataObject IDictionary.Count and .Values do full reflection traversal

**Where:** [Rock.Lava.Shared/Core/LavaDataObject.cs:338-346](../Rock.Lava.Shared/Core/LavaDataObject.cs)

`((ICollection)ldo).Count` calls `GetProperties().Count()`, materializing all properties via reflection just to compute a count. Cache the count, or compute it from the cached `_instancePropertyInfoLookup` plus `_dynamicMembers.Count`.

## Out of Scope

This spec covers only the bridge layer. The following are explicitly **not** addressed:

- The Fluid library itself (we are pinned to a specific version; upstream perf work is upstream's call).
- The DotLiquid → Fluid migration's correctness gaps (separate ongoing work).
- Filter implementations in `Rock.Lava/Filters/Generic/*` — those have their own perf characteristics that warrant a separate review.
- Lava template authoring guidance for end users (a docs concern, not a code concern).
- Rendering paths that bypass the Lava engine (Lava-to-HTML helpers in WebForms blocks, etc.).

## Verification Steps

For each accepted finding:

1. Author a benchmark using BenchmarkDotNet that exercises the affected path with a representative template (recommend: a simple merge-field template, a shortcode-heavy template, and a mixed workflow communication).
2. Capture before/after numbers for both mean time and allocated bytes.
3. Confirm the existing Lava test suite passes (`Rock.Tests.Integration` covers most Lava behaviors).
4. Spot-check a handful of common templates in a running Rock instance to confirm output is byte-identical.

For thread-safety findings (F7, F8, F9), add a stress test that concurrently renders templates with differing cultures/timezones and asserts each render observes its own configured value.

## Considered but Rejected

### Replace the bridge with direct Fluid usage

Considered. Rejected, at least for now. The bridge does real work (Lava-to-Liquid pre-processing, Lava shorthand comments, shortcode tag syntax `{[ ... ]}`, Lava operator semantics, custom block factory model). Removing it would force rewrites of every Lava extension Rock has shipped. The findings above are localized; the bridge stays.

### Pre-compile every public Lava template at startup

Considered. Rejected. Rock has thousands of template fragments stored in defined types, persisted templates, attribute values, and report fields. Eager compilation would slow startup and consume memory for templates that may never render. The existing `FluidTemplateCache` (lazy, modification-aware) is the correct shape; F1 fixes the inner block-content case that the outer cache cannot reach.

### Replace LavaDataObject's DynamicObject base with a hand-rolled implementation

Considered. Rejected for this spec. `DynamicObject` is the right abstraction for the public API surface, and the perf wins from F2-F5 are achievable without changing the inheritance shape. If profiling after those land still shows DynamicObject as a bottleneck, revisit.

## Related

- DotLiquid → Fluid migration discussion (internal).
- Fluid issue [#811](https://github.com/sebastienros/fluid/issues/811) (referenced in `FluidEngine.OnRenderTemplate` comment).
- Asana task referenced at [LavaToLiquidTemplateConverter.cs:103](../Rock.Lava/Engine/LavaToLiquidTemplateConverter.cs) for the `elseif` replace decision.
