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

#### [x] F1. Block content is re-parsed on every render

**Where:** [Rock.Lava.Fluid/Parser/FluidLavaBlockStatement.cs:141-174](../Rock.Lava.Fluid/Parser/FluidLavaBlockStatement.cs)

`ILiquidFrameworkElementRenderer.Render` calls `_parser.Grammar.Parse(blockContext, ref parseResult)` against the block's source text every time the block is rendered. `WriteToAsync` separately calls `LavaFluidParser.ParseToTokens(_blockContent)` on every render at line 109. For any template that uses shortcodes or custom blocks (most Rock templates) this is the largest single sink in the pipeline.

**Proposed fix:** Parse `_blockContent` once at construction (or lazily under `Lazy<T>`) and cache `IReadOnlyList<Statement>` plus the token list as fields. Wrap the synchronous `task.Wait()` at line 164 in the same `IsCompletedSuccessfully` fast-path used in `FluidEngine.OnRenderTemplate` to avoid the `Task` allocation on the hot path.

#### [x] F2. LavaDataWrapper does reflection on every property access

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

**Proposed fix:** Cache the reflection information per Type. A static `ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>>` keyed by type avoids the repeated `GetType().GetProperty(key)` and `GetProperties()` calls. `PropertyInfo.GetValue` itself stays — compiled delegates/lambdas were considered but the cost of building them outweighs any per-call savings for this layer; caching the reflection lookup is the win.

#### [x] F3. LavaTypeMemberAccessor uses PropertyInfo.GetValue per call

**Where:** [Rock.Lava.Fluid/LavaObjectMemberAccessStrategy.cs:228-233](../Rock.Lava.Fluid/LavaObjectMemberAccessStrategy.cs)

Each `[LavaType]`-decorated class registers a `LavaTypeMemberAccessor` per property; that accessor stores `PropertyInfo` and uses `_info.GetValue(obj)` per call. Same pattern, same fix.

**Proposed fix:** Compile the property getter into a `Func<object, object>` once in the constructor and call the delegate at access time.

#### [x] F4. LavaDataHelper.GetLavaTypeInfo recomputes per call and is uncached

**Where:** [Rock.Lava.Shared/Utility/LavaDataHelper.cs:37-76](../Rock.Lava.Shared/Utility/LavaDataHelper.cs)

`GetLavaTypeInfo` invokes `type.GetProperties()` up to three times per call (lines 55, 61, 66) and is called from `LavaDataObjectInternal.GetInstanceProperties`. The Internal helper caches per-instance, but the underlying type info is not cached globally, so each new instance pays the full reflection cost.

**Proposed fix:** Back the lookup with a `ConcurrentDictionary<Type, LavaDataTypeInfo>` and call `GetProperties()` only once per type.

#### [x] F5. ToRealObjectValue uses reflection per dictionary unwrap

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

#### [x] F6a. Filter invocation uses MethodInfo.Invoke and per-call array

**Where:** [Rock.Lava.Fluid/FluidEngine.cs:441-501](../Rock.Lava.Fluid/FluidEngine.cs)

The wrapper allocates `new object[lavaFilterMethodParameters.Length]` per filter call and uses `lavaFilterMethod.Invoke(null, lavaFilterMethodArguments)`. A typical template invokes filters dozens of times per render, and an extremely complex template may reach the low hundreds. `MethodInfo.Invoke` is significantly slower than a direct method call because the runtime performs argument validation, value-type boxing/unboxing, security checks, and `TargetInvocationException` wrapping on every call.

**Proposed fix:** Replace the per-call `MethodInfo.Invoke` with a compiled delegate built once at filter registration. `Expression.Lambda<Func<object[], object>>(...).Compile()` generates IL that contains a direct `call` opcode to the target method — the same call the runtime would have ended up making, just without the reflection machinery wrapping it. Conceptually:

```csharp
// Built once, at AddFilter / RegisterFilters time:
ParameterExpression argsParam = Expression.Parameter( typeof( object[] ), "args" );
Expression[] convertedArgs = lavaFilterMethodParameters
    .Select( ( p, i ) => Expression.Convert(
        Expression.ArrayIndex( argsParam, Expression.Constant( i ) ),
        p.ParameterType ) )
    .ToArray();
Expression body = Expression.Convert(
    Expression.Call( lavaFilterMethod, convertedArgs ),
    typeof( object ) );
Func<object[], object> compiledFilter =
    Expression.Lambda<Func<object[], object>>( body, argsParam ).Compile();
```

Then at invocation time, `lavaFilterMethod.Invoke( null, lavaFilterMethodArguments )` becomes `compiledFilter( lavaFilterMethodArguments )`. The actual method still gets called — that's the whole point — but via a direct delegate call instead of through `MethodInfo.Invoke`. Local benchmarking against the Rock filter set measured roughly an 8x improvement on the invoke step itself.

What does **not** change: the `object[]` argument array is still allocated per call (the cast/unbox steps inside the delegate still need somewhere to read from), and the per-argument cast/unbox work itself is unchanged — it just happens inline in the generated IL instead of inside the reflection invoke. The boxing of value-type return values also stays.

**Optional further step (separate from this finding):** generate per-arity strongly-typed wrappers — `Func<object, object>`, `Func<object, object, object>`, etc. — keyed on the method's parameter count, so the caller can pass arguments positionally without ever allocating the `object[]`. This adds complexity (one wrapper shape per supported arity) and given that filters run dozens to low hundreds of times per render, the array allocation is unlikely to be the dominant cost after the base fix. Only worth doing if post-fix profiling proves otherwise.

#### [x] F6b. Filter registration sort calls GetParameters O(n log n) times

**Where:** [Rock.Lava.Fluid/FluidEngine.cs:397](../Rock.Lava.Fluid/FluidEngine.cs)

The LINQ chain `OrderBy(x => x.Name).ThenByDescending(x => x.GetParameters().Count())` invokes `GetParameters()` O(n log n) times during sort.

**Proposed fix:** Materialize `(method, parameters)` tuples first, then sort against the materialized parameter list.

### P1 — Thread-safety bugs (with perf side-effects)

#### [x] F7. Per-render mutation of shared TemplateOptions

**Where:** [Rock.Lava.Fluid/FluidEngine.cs:658-665](../Rock.Lava.Fluid/FluidEngine.cs)

```csharp
if ( parameters.Culture != null )
    templateContext.FluidContext.Options.CultureInfo = parameters.Culture;
if ( parameters.TimeZone != null )
    templateContext.FluidContext.Options.TimeZone = parameters.TimeZone;
```

`templateContext.FluidContext.Options` is the engine's single `_templateOptions` instance. Two concurrent renders with different cultures will race; one render can observe the other's culture mid-render. This is a correctness bug first, perf bug second.

**Confirmation that Options is shared, not per-context:**

- The engine holds a single `private TemplateOptions _templateOptions = null;` field at [Rock.Lava.Fluid/FluidEngine.cs:39](../Rock.Lava.Fluid/FluidEngine.cs).
- [`GetTemplateOptions()` at Rock.Lava.Fluid/FluidEngine.cs:235-262](../Rock.Lava.Fluid/FluidEngine.cs) lazily creates that single instance and returns the same reference on every call.
- [`OnCreateRenderContext()` at Rock.Lava.Fluid/FluidEngine.cs:74](../Rock.Lava.Fluid/FluidEngine.cs) builds `new global::Fluid.TemplateContext( options )` for every render, passing the shared `_templateOptions`.
- Reflection on `Fluid.dll` confirms `TemplateContext.Options` is an auto-property over `<Options>k__BackingField`. The constructor stores the passed reference unchanged, so `templateContext.FluidContext.Options` is always the same `_templateOptions` instance for every concurrent render. A live reflection test creating two contexts from the same options instance returned `ReferenceEquals(ctxA.Options, ctxB.Options) == true` and `ReferenceEquals(ctxA.Options, _templateOptions) == true`.
- That same reflection scan shows `TemplateContext` itself exposes its own per-context `CultureInfo` and `TimeZone` auto-properties (distinct backing fields from `Options.CultureInfo` and `Options.TimeZone`), which is what the proposed fix writes to instead.

**Proposed fix:** Set the values on the `TemplateContext` itself (`fluidContext.CultureInfo`, `fluidContext.TimeZone`) rather than on the shared `Options`.

**Required verification:** This change MUST be covered by unit tests. Concurrency stress is **not** the goal — instead, add two new tests modeled on the existing per-render-timezone tests in [Rock.Tests/Lava/Filters/DateFilterTests.cs:571,599](../Rock.Tests/Lava/Filters/DateFilterTests.cs):

- **Test 1** — sets `LavaRenderParameters.TimeZone` to a value that differs from `RockDateTime.OrgTimeZoneInfo`, renders a template that surfaces the timezone (e.g., a date filter), and asserts the output reflects the explicit timezone.
- **Test 2** — sets `LavaRenderParameters.TimeZone` to the timezone that matches `RockDateTime.OrgTimeZoneInfo` (the "current" timezone) and asserts the output reflects that value as well.

Together these prove the per-render value reaches the renderer correctly in both the "same as current" and "different from current" cases. The same shape of test should be added for `LavaRenderParameters.Culture`.

**Note discovered while writing these tests (issue, not a bug):** Rock-side filters and value types do not currently honor the per-render `LavaRenderParameters.TimeZone` value. The Rock `Date` filter routes `DateTime` input through `LavaDateTime.ConvertToRockOffset(dt)`, which uses the global `RockDateTime.OrgTimeZoneInfo` directly; the `DateTimeOffset` input path uses the offset already embedded in the value; and `LavaDateTimeValue.WriteToAsync` accepts a `cultureInfo` but no timezone. A grep of `Rock.Lava.Fluid` for any read of `context.TimeZone` / `Options.TimeZone` returns only the assignment line itself, so today the parameter is a "dead write" from Rock's perspective.

This is intentionally left as-is for now because Rock inherently uses `DateTime` values in the Rock organization timezone for almost everything (database columns store `DateTime` rather than `DateTimeOffset`, so the org timezone is implicit on every read). `DateTimeOffset` only appears at boundaries that need it (third-party libraries like Fluid; DTOs serialized over the wire). Honoring per-render `TimeZone` would be a behavior change requiring a deliberate design decision about whether and where Rock should re-anchor `DateTime` values away from the org timezone, which is well outside the scope of this perf-and-allocation spec.

The F7 thread-safety fix is still correct as written: writing to a shared `TemplateOptions` instance across concurrent renders is undefined behavior even when no one downstream reads the value. The "differs from current" test added below has been kept as a guardrail for current functionality (the parameter being set must not break rendering); the matching-timezone test was dropped because it cannot distinguish behavior from no parameter being set.

As a consequence, only **three** F7 tests are added (one TimeZone "differs from current" guardrail, two Culture tests for matching/differing). The Culture parameter is honored end to end via `LavaDateTimeValue.WriteToAsync`, so both Culture cases are observable.

#### [x] F8. _map and factory dictionaries read lock-free while being written

**Where:**
- [Rock.Lava.Fluid/LavaObjectMemberAccessStrategy.cs:32](../Rock.Lava.Fluid/LavaObjectMemberAccessStrategy.cs)
- [Rock.Lava.Fluid/Parser/FluidLavaTagStatement.cs:32](../Rock.Lava.Fluid/Parser/FluidLavaTagStatement.cs)
- [Rock.Lava.Fluid/Parser/FluidLavaBlockStatement.cs:36](../Rock.Lava.Fluid/Parser/FluidLavaBlockStatement.cs)

`_map` and the `_factoryMethods` dictionaries are `Dictionary<>` (not concurrent). Tag/block factory methods are written under a `lock` but read without one during render. Most registrations happen at startup, but dynamic shortcodes can be re-registered at runtime, so the race is real.

**Proposed fix:** Use the **immutable-snapshot / copy-on-write** pattern Microsoft uses in places like `MemoryCache` and DI registration:

1. Keep the field as a plain `Dictionary<>` (do NOT switch to `ConcurrentDictionary<>`). The dictionary instance is treated as write-once: once published to the field, it is never mutated again.
2. On write (registration), continue to take the existing `lock`, but inside the lock:
   - Allocate a new `Dictionary<>` initialized from the current `_map` / `_factoryMethods`.
   - Add or replace the entry on the new dictionary.
   - Atomically reassign the field to the new dictionary.
3. On read, capture the field reference once into a local and use that local for the lookup. Because the captured dictionary is no longer mutated by anyone, the read is safe without a lock.
4. While here, replace any `ContainsKey` + `[]` indexer pattern with a single `TryGetValue` (e.g., the factory wrappers in [FluidLavaTagStatement.cs:85-89](../Rock.Lava.Fluid/Parser/FluidLavaTagStatement.cs) and [FluidLavaBlockStatement.cs](../Rock.Lava.Fluid/Parser/FluidLavaBlockStatement.cs)).

**Why this over `ConcurrentDictionary<>`:** these maps are populated rarely — `_map` only when a `[LavaType]`-decorated type is first encountered (roughly a dozen across Rock), and `_factoryMethods` only when tags/blocks/shortcodes register. Reads vastly outnumber writes. `ConcurrentDictionary<>` carries per-bucket locking and a heavier read path; the snapshot pattern gives lock-free, allocation-free reads with the cost of a one-time dictionary copy on the rare write.

#### [x] F9. GetTemplateOptions is not thread-safe

**Where:** [Rock.Lava.Fluid/FluidEngine.cs:235-262](../Rock.Lava.Fluid/FluidEngine.cs)

The `if (_templateOptions == null) { ... }` initialization can let two threads each construct a `TemplateOptions`, register filters twice, and lose one. Not a hot path, but a real startup race.

**Proposed fix:** `Lazy<TemplateOptions>` or a lock with a double-check.

### P2 — Per-context / per-render allocations

#### [x] F10. FluidRenderContext constructor sets four constants on every render

**Where:** [Rock.Lava.Fluid/FluidRenderContext.cs:38-48](../Rock.Lava.Fluid/FluidRenderContext.cs)

`Blank/blank/Empty/empty` get `SetValue` calls on every new context. Since `ModelNamesComparer` is `StringComparer.Ordinal`, both case variants are needed, but they're constants and don't have to be set per render.

**Proposed fix:** Register the four values once on the engine's `TemplateOptions` so each new context inherits them.

**Required verification:** This is a functional change (the keywords must still resolve in both casings after registration moves from per-context to per-engine). The existing tests cover this:

- [Rock.Tests/Lava/LiquidKeywordTests.cs:248-273](../Rock.Tests/Lava/LiquidKeywordTests.cs) — `Empty_UpperCaseOrLowerCase_IsNotCaseSensitive` exercises `Empty` and `empty` via `{% if Items == ... %}`.
- [Rock.Tests/Lava/LiquidKeywordTests.cs:278-303](../Rock.Tests/Lava/LiquidKeywordTests.cs) — `Blank_UpperCaseOrLowerCase_IsNotCaseSensitive` exercises `Blank` and `blank` the same way.

Confirm both still pass after the move. No new tests required unless these break under the change.

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

**Status:** Rejected. For the same reason as F2, building a delegate is not actually faster than the cached `PropertyInfo.GetValue` call at this layer. The reflection lookup is already cached in a `static` field, so the remaining per-call cost is the `GetValue` invocation itself, which a delegate would not meaningfully improve.

#### [x] F12. ParseTemplate copies the Statements list for no apparent reason

**Where:** [Rock.Lava.Fluid/FluidEngine.cs:633](../Rock.Lava.Fluid/FluidEngine.cs)

```csharp
template = new FluidTemplate( new List<Statement>( fluidTemplateObject.Statements ) );
```

The parser already returned a `FluidTemplate`. We're copying its statement list into another `FluidTemplate`.

**Proposed fix:** Return `fluidTemplateObject` directly. If there is a real reason for the copy that is not visible in the surrounding code, document it inline.

#### [x] F13. Async-over-sync allocations

**Where:**
- [Rock.Lava.Fluid/FluidEngine.cs:701-705](../Rock.Lava.Fluid/FluidEngine.cs) — `task.AsTask().GetAwaiter().GetResult()`. ValueTask has its own `GetAwaiter()`; drop `AsTask()` to skip the Task allocation.
- [Rock.Lava.Fluid/FluidExtensions.cs:317](../Rock.Lava.Fluid/FluidExtensions.cs) — `arg.Expression.EvaluateAsync(context).Result`. `.Result` wraps exceptions in `AggregateException` and forces Task materialization. Use `.GetAwaiter().GetResult()`.
- See also F1 — same pattern in `FluidLavaBlockStatement` render.

**Proposed fix:** Use ValueTask's own awaiter where possible and the `IsCompletedSuccessfully` fast-path everywhere a sync-over-async pattern exists.

#### [x] F14. GetScopeAggregatedValues allocates two dictionaries per call

**Where:** [Rock.Lava.Fluid/FluidRenderContext.cs:284-323](../Rock.Lava.Fluid/FluidRenderContext.cs)

`GetScopeDefinedValues` returns its own dictionary, then `GetScopeAggregatedValues` allocates another and copies. The inner method also uses `properties.Where(...)` which allocates a deferred enumerator.

**Proposed fix:** Inline the prefix check and write directly into a single dictionary.

#### [ ] F15. Small but called every render

These are individually cheap but called per render or per merge-field operation.

- [x] **F15a.** [FluidRenderContext.cs:167](../Rock.Lava.Fluid/FluidRenderContext.cs) — `",".ToCharArray()` allocates per call. Use a `private static readonly char[]` or the `Split(',')` overload.
- [x] **F15b.** [LavaToLiquidTemplateConverter.cs:106](../Rock.Lava/Engine/LavaToLiquidTemplateConverter.cs) — `inputTemplate?.Replace("elseif", "elsif")` always allocates a new string. Guard with `IndexOf("elseif", StringComparison.Ordinal) < 0` first.
- [x] **F15c.** [LavaDataObject.cs:788](../Rock.Lava.Shared/Core/LavaDataObject.cs) — `propPath.Split('.').ToList()` then `propPath.First()` and `propPath.Skip(1).ToList()` per traversal step. For path "a.b.c" this allocates 6+ lists. Drop the trailing `.ToList<string>()` and use the `string[]` returned by `Split(...)` directly, then walk it with an `int` index counter instead of re-allocating via `Skip(1).ToList()` each iteration. The loop condition becomes `pathIndex < propPath.Length`, the current segment is `propPath[pathIndex]`, and the step becomes `pathIndex++`. This matches Rock's existing index-counter convention and avoids introducing a `cursor`/`IndexOf('.')` parser pattern.
- [ ] **F15d.** [LavaDataObject.cs:973-990](../Rock.Lava.Shared/Core/LavaDataObject.cs) — `GetDynamicMemberNames` allocates a new list; `AvailableKeys` then `.ToList()` again. Cache the property name list per type. **Rejected:** the member list is intentionally dynamic (the method name says so). Caching by type would not honor runtime additions/removals.
- [x] **F15e.** [LavaObjectMemberAccessStrategy.cs:127-134](../Rock.Lava.Fluid/LavaObjectMemberAccessStrategy.cs) — `RegisterLavaTypeProperties` calls `type.GetProperties()` three times. Materialize once.
- [x] **F15f.** [LavaObjectMemberAccessStrategy.cs:66](../Rock.Lava.Fluid/LavaObjectMemberAccessStrategy.cs) — `type.Name.Contains("AnonymousType")` per first-time access. Cache per-Type.

### P3 — Worth knowing, low cost

#### [ ] F16. Value converter ordering and basic-type short-circuit

**Where:** [Rock.Lava.Fluid/FluidEngine.cs:106-107](../Rock.Lava.Fluid/FluidEngine.cs)

The existing TODO already calls this out. Adding `if (value is string || value is int || value is bool) return null;` at the top of each converter would skip O(n_converters) per common value.

**Status:** Rejected. The proposed change might actually be slower in practice (added type checks on every value), and standing up a benchmark suite to confirm the few-nanosecond win is not justified for the expected impact.

#### [ ] F17. LavaDataDictionary.AvailableKeys allocates per call

**Where:** [Rock.Lava.Shared/Core/LavaDataDictionary.cs:365](../Rock.Lava.Shared/Core/LavaDataDictionary.cs)

Returns `new List<string>(_dictionary.Keys)` every call. If the engine reads it more than once during a render, that's wasted. Note: changing the return type would break the public API, so the fix is either to expose an alternate accessor or to cache the materialized list and invalidate on writes.

**Status:** Rejected. The accessor returns a writable `List<string>`. Caching it would let one caller mutate the list and have the modified list returned to the next caller, which is a correctness regression. The allocation is the price of returning an isolated copy.

#### [ ] F18. LavaDataObject IDictionary.Count and .Values do full reflection traversal

**Where:** [Rock.Lava.Shared/Core/LavaDataObject.cs:338-346](../Rock.Lava.Shared/Core/LavaDataObject.cs)

`((ICollection)ldo).Count` calls `GetProperties().Count()`, materializing all properties via reflection just to compute a count. Cache the count, or compute it from the cached `_instancePropertyInfoLookup` plus `_dynamicMembers.Count`.

**Status:** Rejected. `GetProperties()` returns a dynamic list that can change as members are added or removed on the object. Caching the count or list would require building cache-invalidation logic on every member mutation, and the maintenance cost outweighs the per-call savings.

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
