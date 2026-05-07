---
author: Jon Edmiston
date_created: 2026-05-01
summary: >-
  Performance and allocation review of the engine-agnostic Lava abstraction
  that sits between Rock and the active Lava engine (LavaService, LavaHelper,
  LavaEngineBase, WebsiteLavaTemplateCacheService, ResolveMergeFields, and
  related plumbing). Catalogs reflection-per-call hotspots, redundant
  pre-flight checks, per-render allocation, mutable-input bugs, and a few
  thread-safety issues — each as a checkbox so reviewers can pick what to act
  on.
contributors:
  - Daniel Hazelbaker
related_docs:
  - specs/completed/lava/260501-lava-fluid-bridge-perf-improvements.md
---

# Lava Engine Abstraction: Performance and Allocation Improvements

## Summary

This is the companion to the Fluid-bridge spec. Where that one targeted the `Rock.Lava.Fluid` translation layer, this one targets the engine-agnostic abstraction that lives above it: `LavaService`, `LavaEngineBase`, `LavaHelper`, `WebsiteLavaTemplateCacheService`, `WebsiteLavaShortcodeProvider`, `WebsiteLavaFileSystem`, `LavaServiceProvider`, the `ResolveMergeFields` extension family, and the small helpers they share. Every Lava call in Rock goes through this layer before it reaches Fluid, so per-render costs here multiply by the same call frequency.

Each finding is a checkbox. Tick the items the team agrees to address; leave the rest for later or for explicit rejection.

## Motivation

The Fluid-bridge findings improve the parser-and-renderer path. They do not help if the abstraction layer above wastes the same allocations on every render. A page that calls `string.ResolveMergeFields(...)` ten times pays the upstream cost ten times before any Fluid code runs. The biggest single offender is the per-render reflection in `Rock.Common.ObjectExtensions.GetPropertyValue`, which is the path used to access anonymous-type properties from Lava. Other significant items are mutable-input bugs (the engine mutating its own input parameters), per-render `Dictionary` allocations from default-context conversion, and per-shortcode O(n) cache scans.

As with the bridge spec, none of these have been benchmarked. The priority ordering reflects expected impact based on call frequency and operation cost. Reviewers are encouraged to challenge the priority before implementation.

The companion bridge spec has now been completed; several of the proposed fixes below have been updated to follow patterns established there — most notably caching `PropertyInfo` rather than building compiled getter delegates (F2/F11), using the immutable-snapshot / copy-on-write pattern for write-rare/read-frequent dictionaries (F8), and walking `Split` arrays with an `int` index counter rather than allocating intermediate lists or using `IndexOf` cursors (F15c).

## Requirements

- Each finding MUST be addressable independently.
- Functional behavior MUST remain identical. These are perf and allocation changes only.
- Public APIs in `Rock.Lava`, `Rock.Lava.Shared`, and `Rock` (the LavaExtensions namespace) MUST remain backward compatible.
- Thread-safety fixes MUST land regardless of perf impact.
- Each accepted finding SHOULD be benchmarked before and after using a representative template (recommend: a CMS page render, an entity-driven communication, and a workflow with embedded Lava).

## Findings

### P0 — Major hot-path issues

#### [x] A1. ObjectExtensions.GetPropertyValue uses reflection on every property access

**Where:** [Rock.Common/ExtensionMethods/ObjectExtensions.cs:39-69](../Rock.Common/ExtensionMethods/ObjectExtensions.cs)

```csharp
public static object GetPropertyValue( this object rootObj, string propertyPathName ) {
    var propPath = propertyPathName.Split( ... ).ToList<string>();
    ...
    PropertyInfo property = objType.GetProperty( propPath.First() );
    if ( property != null ) {
        obj = property.GetValue( obj );
        objType = property.PropertyType;
    }
    ...
}
```

This is the path the Fluid bridge's `DynamicMemberAccessor` uses for anonymous types. Every property lookup on an anonymous object passed to Lava goes through `Type.GetProperty` plus `PropertyInfo.GetValue` plus a per-step `Split.ToList()` plus `propPath.First()` and `propPath.Skip(1).ToList()` allocations.

**Proposed fix:** Replace the helper internals with a per-`Type` cache of `Dictionary<string, PropertyInfo>` keyed by property name (case-insensitive), held in a `static ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>>`. The lookup becomes a dictionary hit; the actual `PropertyInfo.GetValue( obj )` call stays. This matches the decision made for F2 in the completed bridge spec: building compiled `Func<object, object>` getters at lookup time was rejected because the cost of building them outweighs the per-call savings at this layer; caching the reflection lookup itself is the win. F11 reinforced the same conclusion at the `LavaTypeMemberAccessor.GetValue` layer (rejected building a delegate over an already-cached `PropertyInfo`). For path traversal, drop the `.ToList<string>()` and walk the `string[]` returned by `Split(...)` directly using an `int` index counter — matching the F15c precedent — instead of repeatedly allocating new lists via `propPath.Skip(1).ToList()` per step.

#### [x] A2. LavaHelper.GetLavaProperties recomputes per call

**Where:** [Rock/Lava/LavaHelper.cs:277-280](../Rock/Lava/LavaHelper.cs)

```csharp
public static List<PropertyInfo> GetLavaProperties( Type type ) {
    return type.GetProperties().Where( p => IsLavaProperty( p ) ).ToList();
}
```

`IsLavaProperty` is cached per-`PropertyInfo` (good), but the surrounding `GetProperties().Where().ToList()` is not. Every call walks all properties of the type and allocates a new list. This is called from filter and shortcode paths.

**Proposed fix:** Wrap with `ConcurrentDictionary<Type, List<PropertyInfo>>`.

#### [ ] A3. WebsiteLavaShortcodeProvider.GetShortcodeDefinition does O(n) scan per render

**Where:** [Rock/Lava/WebsiteLavaShortcodeProvider.cs:92-125](../Rock/Lava/WebsiteLavaShortcodeProvider.cs)

```csharp
var shortcodeDefinition = LavaShortcodeCache.All()
    .Where( c => c.TagName != null && c.TagName.Equals( shortcodeName, StringComparison.OrdinalIgnoreCase ) )
    .FirstOrDefault();
```

This factory runs every time a shortcode renders. With N shortcodes registered, every shortcode invocation does an O(N) case-insensitive linear scan of `LavaShortcodeCache.All()`. Rock instances commonly have 30-50 shortcodes; templates that use many shortcodes pay N×M scans.

**Proposed fix:** Build a case-insensitive `Dictionary<string, LavaShortcodeCache>` at the provider level, invalidated on `WebsiteLavaShortcodeProvider.ClearCache()`.

**Status:** Rejected. `LavaShortcode` is a database-driven entity, and `LavaShortcodeCache` inherits from `ModelCache<LavaShortcodeCache, LavaShortcode>`. When an admin or plugin adds, edits, deletes, or renames a shortcode, Rock's cache infrastructure **evicts** the affected `LavaShortcodeCache` instance — `SetFromEntity` is a call-once initializer, not a mutation; the next access constructs a brand-new instance. `WebsiteLavaShortcodeProvider.ClearCache()` is only called explicitly at registration time (boot, deliberate engine restart), not on every shortcode mutation, so a provider-level secondary lookup would silently go stale on adds, deletes, `TagName` renames, and any other property update. The only correctness-safe fix is to push the tag-name lookup down into `LavaShortcodeCache` / the `ModelCache` base infrastructure, where the same hooks that invalidate `All()` would invalidate the lookup. That is significantly more invasive than this spec contemplates, has design implications across every entity cache, and is not justified for an O(N=30-50) scan. Leave the linear scan alone.

#### [ ] A4. UAParser called per render with no result cache

**Where:** [Rock/Lava/LavaHelper.cs:164-180](../Rock/Lava/LavaHelper.cs)

```csharp
Parser uaParser = Parser.GetDefault();
ClientInfo client = uaParser.Parse( request.UserAgent );
```

`Parser.GetDefault()` is internally cached. The actual `Parse` call runs ~15,000 regex evaluations against the user-agent string. For high-traffic pages this is one of the heaviest ops in `GetCommonMergeFields`. There is no per-request or per-UA-string cache.

**Proposed fix:** Cache `ClientInfo` keyed by user-agent string in a `ConcurrentDictionary` with bounded size (LRU semantics, or just a size cap with eviction-by-clear). Even a simple `HttpContext.Current.Items["__cachedClientInfo"]` would dedupe within a single request.

**Status:** Rejected (deferred to its own spec). User-agent parsing and caching is a cross-cutting concern that shows up in places beyond Lava (analytics, interaction logging, device-type detection, etc.). A proper fix is a standard Rock helper for parsing user-agent strings with the right caching/LRU/bounding semantics, used everywhere — not a one-off cache inside `LavaHelper`. Tracked in [260506-rock-user-agent-helper.md](260506-rock-user-agent-helper.md).

#### [x] A5. SetMergeFields(LavaDataDictionary) allocates an AvailableKeys list per call

**Where:**
- [Rock.Lava/Core/LavaRenderContextBase.cs:155-158](../Rock.Lava/Core/LavaRenderContextBase.cs) — `SetMergeFields(LavaDataDictionary)` dispatch.
- [Rock.Lava/Engine/LavaEngineBase.cs:657-670](../Rock.Lava/Engine/LavaEngineBase.cs) — the default-context-to-engine-context conversion branch (test-only, see below).

The original framing of this finding was "engine converts a default `LavaRenderContext` to an engine-specific one and walks dictionaries twice." Investigation surfaced two corrections:

1. **The default-to-engine conversion branch in `LavaEngineBase.RenderTemplate` only fires from unit tests.** A grep for direct construction of `LavaRenderContext` (`new LavaRenderContext(...)` or `LavaRenderContext.FromMergeValues(...)`) finds zero production callers. Every production path goes through `LavaService.NewRenderContext(...)` → `engine.NewRenderContext(...)` → `OnCreateRenderContext()`, which the engine overrides to return an engine-specific context (e.g., `FluidRenderContext`). The strict-type check `parameters.Context.GetType() == typeof( LavaRenderContext )` only matches in `Rock.Tests/Lava/Filters/DateFilterTests.cs`, `Rock.Tests/Lava/LavaTestHelper.cs`, and `Rock.Tests/Lava/RenderTests.cs`. So per-render cost in this branch is irrelevant to production performance, and the proposed structural fix (engine-specific contexts up front from `LavaService`) buys nothing in production — just adds indirection.

2. **`SetMergeFields(LavaDataDictionary)` is called from real per-render hot paths**, regardless of the conversion branch above. The base method dispatches through the `ILavaDataDictionary` overload at [LavaRenderContextBase.cs:120-131](../Rock.Lava/Core/LavaRenderContextBase.cs), which iterates `fieldValues.AvailableKeys`. `LavaDataDictionary.AvailableKeys` returns `new List<string>(_dictionary.Keys)` per call (the same allocation noted as F17 in the bridge spec). Production callers that hit this path on every render include `SetCultureBlock`, `ObserveBlock`, `CacheBlock` (all do `newContext.SetMergeFields( context.GetMergeFields() )` where `GetMergeFields()` returns a `LavaDataDictionary`), and `DynamicShortcode.SetMergeFields(parms)`. The internal-fields path does *not* pay this — `SetInternalFields(LavaDataDictionary)` already dispatches through the `IDictionary<string, object>` overload, which `foreach`-iterates the dictionary directly.

**Proposed fix:**

*Tactical (real production win):* Change `SetMergeFields(LavaDataDictionary)` at [Rock.Lava/Core/LavaRenderContextBase.cs:155-158](../Rock.Lava/Core/LavaRenderContextBase.cs) to dispatch through the `IDictionary<string, object>` overload instead of the `ILavaDataDictionary` overload. `LavaDataDictionary` already implements `IDictionary<string, object>`, so the cast is free; this skips the `AvailableKeys` materialization and reuses the `foreach` path the internal-fields code already uses. One `List<string>` allocation eliminated per call from `SetCultureBlock`, `ObserveBlock`, `CacheBlock`, `DynamicShortcode`, and the test-only conversion branch.

*Annotation only:* Add a comment to the conversion branch in `LavaEngineBase.RenderTemplate` at [Rock.Lava/Engine/LavaEngineBase.cs:657-670](../Rock.Lava/Engine/LavaEngineBase.cs) noting that the strict-type match against `LavaRenderContext` only fires in unit-test paths today (`DateFilterTests`, `LavaTestHelper`, `RenderTests`). This documents the intent for future readers and prevents anyone from optimizing this branch under the impression that it carries production traffic. The previously-considered structural fix (engine-specific contexts up front from `LavaService`) is dropped — it would add complexity for no production benefit.

### P1 — Mutable-input and thread-safety bugs

#### [x] A6. LavaService.RenderTemplate mutates the input parameters object

**Where:** [Rock/Lava/LavaService.cs:481-498](../Rock/Lava/LavaService.cs)

```csharp
if ( page != null ) {
    string cacheKey;
    if ( string.IsNullOrEmpty( parameters.CacheKey ) ) {
        cacheKey = _engine.TemplateCacheService.GetCacheKeyForTemplate( inputTemplate );
    } else {
        cacheKey = parameters.CacheKey;
    }
    parameters.CacheKey = GetWebTemplateCacheKey( cacheKey, page.Site?.Theme );
}
```

The caller's `LavaRenderParameters` is mutated. Two concurrent renders that share a parameters reference (or a caller that reuses the object across themes) will see the cache key change underneath them. This is the same input-mutation pattern as F7 in the bridge spec but at a different layer.

**Proposed fix:** Clone the parameters before mutating. `LavaRenderParameters.Clone()` already exists.

**Required verification:** Add a unit test that captures the input `LavaRenderParameters` reference, renders a template that exercises the page/cache-key path, and asserts the input parameters object is unchanged after the render. This is the same shape of guardrail used for F7 in the completed bridge spec — prove the per-render value reaches the engine without leaking back into the caller's object.

#### [x] A7. LavaService.SetCurrentEngine sets _engine = null before assigning the new instance

**Where:** [Rock/Lava/LavaService.cs:84-99](../Rock/Lava/LavaService.cs)

```csharp
lock ( _initializationLock ) {
    _engine = null;
    if ( lavaEngineType != null ) {
        var engine = NewEngineInstance( lavaEngineType, options );
        _engine = engine;
    }
}
```

The lock protects writers from each other, but `GetCurrentEngine()` reads `_engine` without entering the lock and can observe `null` mid-swap. Any concurrent render during an engine swap can hit a NRE inside `LavaService` because every render method does `if ( _engine == null ) return null;`.

**Proposed fix:** Build the new engine first, then publish it with a single assignment (the lock already serializes writers; readers just need the assignment to be the last operation). Drop the `_engine = null` line entirely so concurrent readers observe either the previous engine or the new one — never `null` mid-swap. Marking `_engine` `volatile` is not necessary: in production the engine is initialized exactly once per process at startup, so the swap window is degenerate; the only paths that re-initialize are unit tests, and those don't run concurrent renders against the engine being swapped. If lavaEngineType is `null`, then set `_engine = null` as an else statement.

#### [x] A8. LavaServiceProvider._services is not thread-safe

**Where:** [Rock.Lava/Core/LavaServiceProvider.cs:29-91](../Rock.Lava/Core/LavaServiceProvider.cs)

`_services` is `Dictionary<>` mutated by `RegisterService` and read by `GetService`. Registrations happen at startup so the race is narrow, but `GetService` is also called from runtime paths that may race with a re-registration during diagnostics or test setup.

**Proposed fix:** Use the immutable-snapshot / copy-on-write pattern from finding F8 in the completed bridge spec rather than `ConcurrentDictionary<>`. Registrations happen at startup and from rare diagnostic/test paths; reads happen on every Lava-adjacent operation. Keep the field as a plain `Dictionary<>`, take a lock on write, build a new dictionary initialized from the current one with the new entry added/replaced, and atomically reassign the field. Readers capture the field reference once into a local and look up against the immutable snapshot — no lock, no per-bucket overhead.

#### [ ] A9. LavaEngineBase.RenderTemplate(string, parameters) always clones parameters

**Where:** [Rock.Lava/Engine/LavaEngineBase.cs:537-545](../Rock.Lava/Engine/LavaEngineBase.cs)

```csharp
if ( parameters == null ) {
    activeParameters = new LavaRenderParameters();
} else {
    activeParameters = parameters.Clone();
}
```

Every render allocates a fresh `LavaRenderParameters`. The clone exists to protect against the mutation pattern in A6 and elsewhere — once those mutation bugs are fixed, the unconditional clone can become an opt-in.

**Proposed fix:** Once A6 lands, only clone when the engine genuinely needs to mutate (e.g., when adding a derived cache key).

**Status:** Rejected. A benchmark of the `Clone()` operation comes in at single-digit nanoseconds, so the per-render cost is negligible. A deep audit additionally surfaced two production callsites in `LavaEngineBase.RenderTemplate` (lines 583-585 and 650-653) and one test-only site (line 669) where the engine mutates `Context` when the caller's was null — these mutations are protected today only by this unconditional clone. Removing the clone would either leak a fresh `NewRenderContext()` back onto the caller's parameters object or require restructuring the method to thread the resolved context as a separate local. Not worth the complexity for a single-digit-ns saving. Leave the clone in place.

### P2 — Per-render allocations

#### [ ] A10. ResolveMergeFieldsInternal does the IsLavaTemplate regex check then the engine does it again

**Where:** [Rock/Utility/ExtensionMethods/LavaExtensions.cs:591-626](../Rock/Utility/ExtensionMethods/LavaExtensions.cs)

`ResolveMergeFieldsInternal` calls `content.IsLavaTemplate()` (which runs the `_hasLavaTags` regex plus the `_lavaCommentMatchGroupsRegex` regex) before the engine runs. The engine's parser then re-scans the content. The pre-check is intentional (skip the engine entirely for non-templates) but the regex is loose: `(?<=\{)[\S\s]+(?<=\})` matches anything between braces, including JSON, CSS, GUIDs in literal text, and so on. False positives still pay the engine cost.

**Proposed fix:** Switch the pre-check to `IsStrictLavaTemplate` which requires `{{`, `{%`, or `{[` patterns. This was already added (and is internal) but is not used by the public extension. Audit for behavior change risk before flipping.

**Status:** Rejected. The cost is real — a benchmark on a standard-size Lava template comes in around 0.1ms, which is meaningful — but the risk profile is wrong for a perf-focused cleanup. Lava is too central to Rock's functionality and we have no control over the Lava templates running in the wild (admin-authored content, partner customizations, persisted attribute values, communication templates, defined-value templates, etc.). The loose regex is loose by design, and any template that the new strict check disagrees with would silently start rendering different output. There is no unit-test surface that can verify "every real-world template still parses the same," so this change cannot be safely validated. A 0.1ms saving is not worth the chance of regressing arbitrary customer content. Revisit only as part of a deliberate behavioral change with a migration plan, not as a perf fix.

#### [ ] A11. Two duplicate Lava-comment regex implementations

**Where:**
- [Rock/Lava/LavaHelper.cs:707-820](../Rock/Lava/LavaHelper.cs)
- [Rock.Lava/Engine/LavaToLiquidTemplateConverter.cs:109-166](../Rock.Lava/Engine/LavaToLiquidTemplateConverter.cs)

Both files define very similar Lava-comment-stripping regex pipelines. Both are compiled and both are invoked on the same templates (the helper from Rock-side pre-flight, the converter from inside the engine parse). Templates with comments pay the regex cost twice.

**Proposed fix:** Consolidate to a single implementation in `Rock.Lava` (or `Rock.Lava.Shared`) and have both call sites use it. Removing the duplication also reduces the chance the two implementations drift.

**Status:** Rejected. Same reasoning as A10. The two regex pipelines are *similar* but not necessarily identical, and consolidating them risks subtly changing how comment stripping behaves for arbitrary customer-authored templates. Lava is too central to Rock's functionality and there is no unit-test surface that can verify "every real-world template still has its comments stripped the same way." For a perf-focused cleanup the regression risk is too high. Revisit only as part of a deliberate behavioral change with a migration plan and template-corpus testing, not as a perf fix.

#### [x] A12. ResolveMergeFieldsWithCurrentLavaEngine sets enabled commands as a separate step

**Where:** [Rock/Utility/ExtensionMethods/LavaExtensions.cs:645-647](../Rock/Utility/ExtensionMethods/LavaExtensions.cs)

```csharp
var context = LavaService.NewRenderContext( mergeObjects );
context.SetEnabledCommands( enabledLavaCommands, "," );
```

`NewRenderContext(mergeObjects, enabledCommands)` exists. Calling them separately means walking the merge fields twice (once for set, once for the engine's internal copy) and setting enabled commands as a second mutation.

**Proposed fix:** Use the overload that accepts both in a single call.

#### [ ] A13. ResolveMergeFieldsWithCurrentLavaEngine reads global default per render

**Where:** [Rock/Utility/ExtensionMethods/LavaExtensions.cs:640-643](../Rock/Utility/ExtensionMethods/LavaExtensions.cs)

```csharp
if ( enabledLavaCommands == null ) {
    enabledLavaCommands = GlobalAttributesCache.Value( "DefaultEnabledLavaCommands" );
}
```

Every render that does not explicitly pass enabled commands reads the global attribute. `GlobalAttributesCache.Value` is itself cached, but the lookup still costs one cache hit plus the string comparison overhead per render.

**Proposed fix:** Memoize within the engine: when `LavaEngineBase` initializes, capture the default once and reuse it. Invalidate on `ClearTemplateCache` or when global attributes change.

**Status:** Rejected. Same double-cache invalidation problem as A3. `GlobalAttributesCache.Value("DefaultEnabledLavaCommands")` is already a cached read; memoizing it inside the engine builds a secondary cache whose contents must be invalidated whenever the underlying global attribute changes. That requires building hooks into the global-attributes change path that fire on edits to that specific attribute. The infrastructure cost outweighs the saving (one cache hit + a string comparison per render that doesn't pass enabled commands).

#### [ ] A14. WebsiteLavaTemplateCacheService.IsCacheEnabled reads request cookies per call

**Where:** [Rock/Lava/WebsiteLavaTemplateCacheService.cs:234-246](../Rock/Lava/WebsiteLavaTemplateCacheService.cs)

```csharp
if ( HttpContext.Current?.Request != null ) {
    var isCachedEnabled = HttpContext.Current.Request.Cookies.Get( RockCache.CACHE_CONTROL_COOKIE );
    if ( isCachedEnabled != null && !isCachedEnabled.Value.AsBoolean() ) { return false; }
}
return true;
```

Cookies access in ASP.NET takes a lock and copies. This runs on every cache lookup (every template render).

**Proposed fix:** Cache the cookie value in `HttpContext.Current.Items` for the request lifetime. The flag is per-request, not per-render.

**Status:** Rejected (with caveat). The proper fix is more complex than this spec suggests — caching cookie values for a request's lifetime is a cross-cutting concern that needs to integrate with how Rock manages per-request state generally, not a one-off helper inside the Lava cache service. Worth flagging as a future Rock-wide improvement (per-request memoization of frequently-read request-scoped values: cookies, headers, host info, etc.), but out of scope for a Lava-focused perf pass.

#### [ ] A15. LavaService.RenderTemplate always inspects HttpContext.Current.Handler

**Where:** [Rock/Lava/LavaService.cs:479-495](../Rock/Lava/LavaService.cs)

`var page = HttpContext.Current?.Handler as RockPage;` runs on every template render, even from background jobs / workflows / API requests where `RockPage` is impossible. The cast itself is cheap, but `HttpContext.Current` access is a `[ThreadStatic]` lookup with a small but real cost.

**Proposed fix:** When the engine has been initialized for a non-web host (jobs, workflows), short-circuit this path. Or: wrap behind `RockApp.Current.IsWebHost` check (or an equivalent flag).

**Status:** Rejected. There is no good way around this that doesn't incur the same `[ThreadStatic]` cost. `RockApp.Current` is itself accessed through similar thread-static / per-call infrastructure, so wrapping the `HttpContext.Current` check behind an `IsWebHost` flag just trades one thread-static read for another. The cast itself is essentially free; the only real cost is the `HttpContext.Current` access, and we cannot avoid that without restructuring the LavaService API to know its hosting context at construction time. That restructure is well outside a perf-focused cleanup.

#### [ ] A16. LavaService.RemoveTemplateCacheEntry enumerates all sites per call

**Where:** [Rock/Lava/LavaService.cs:572-598](../Rock/Lava/LavaService.cs)

For each invalidation, the method calls `SiteCache.All()`, distinct-projects themes, then loops to remove a key per theme. `SiteCache.All()` is cached but the projection still allocates. This runs whenever a Lava-bearing setting changes (saving a block, editing a workflow attribute, modifying a defined value template, etc.).

**Proposed fix:** Cache the distinct theme list at module level with invalidation on `SiteCache` clear. Net win is small per call but invalidations happen in bursts.

**Status:** Rejected. These cache removals happen extremely rarely — typically only as part of an admin save operation (saving a block, editing a workflow attribute, modifying a defined-value template, etc.) — and the work each call does is already small. Same double-cache invalidation problem as A3 / A13: a module-level cache of distinct themes would need hooks into `SiteCache` invalidation to stay correct. The frequency does not justify the additional invalidation infrastructure.

#### [ ] A17. LavaHelper.GetCommonMergeFields reads SystemSetting per render

**Where:** [Rock/Lava/LavaHelper.cs:207](../Rock/Lava/LavaHelper.cs)

```csharp
mergeFields.Add( "ExperienceMode",
    Rock.Web.SystemSettings.GetValue( SystemKey.SystemSetting.TRAILBLAZER_MODE ).AsBoolean()
        ? "Trailblazer" : "Essentials" );
```

`SystemSettings.GetValue` is cached but allocates a string per call and parses it as a boolean per call. Setting changes infrequently; reading it on every render adds up.

**Proposed fix:** Cache the resolved string ("Trailblazer" or "Essentials") at module level with invalidation on `SystemSettings` clear.

**Status:** Rejected. Same double-cache invalidation problem as A3 / A13 / A16. `SystemSettings.GetValue` is already a cached read; a module-level cache of the resolved string would need hooks into `SystemSettings` change notifications to stay correct. The per-render cost (one cache hit, one string allocation, one boolean parse) does not justify building that secondary-cache invalidation infrastructure.

### P3 — Worth knowing, low cost

#### [ ] A18. LavaHelper.GetCurrentPerson walks merge fields then HttpContext.Items

**Where:** [Rock/Lava/LavaHelper.cs:288-311](../Rock/Lava/LavaHelper.cs)

Called by many filters and shortcodes. Each call does a merge-field lookup followed (if null) by an `HttpContext.Items.Contains` check. Fine on its own, but if 10 filters call this, that's 10 merge-field walks for the same value.

**Proposed fix:** Cache the resolved `Person` in an internal context field on first lookup; subsequent calls read the cached value directly.

**Status:** Rejected. Full caching is not safe — a Lava template can rebind `CurrentPerson` mid-render via `{% assign CurrentPerson = newPerson %}`, so the merge-field lookup must run on every call. The only piece that could be cached is the fallback `HttpContext.Current?.Items.Contains("CurrentPerson")` branch, which would be stored as an internal merge field for subsequent calls in the same render. That trade — "walk merge fields, then check `HttpContext.Items.Contains`" becomes "walk merge fields, then walk internal merge fields" — is essentially zero net work, since both fallback lookups are O(1) dictionary hits of similar cost. Not worth the added complexity for no measurable gain.

#### [ ] A19. LavaService methods have heavy null-engine boilerplate

**Where:** Throughout [Rock/Lava/LavaService.cs](../Rock/Lava/LavaService.cs)

Every method begins with `if ( _engine == null ) return null;` (or `return;`). When the engine is configured (the normal case), this is a wasted volatile read on every Lava call. Minor — but the engine is set exactly once at startup, so the null check could be replaced with an assertion once we trust the lifecycle.

**Proposed fix:** Replace runtime null checks with a one-time assertion at engine setup (and let calls fail fast if the engine is missing). Low ROI; only worth doing if the surrounding methods are touched anyway.

**Status:** Rejected. The `_engine == null` check isn't defending against a lifecycle assertion that could be moved to startup — it's defending against any consumer of the `Rock.Lava` library that calls `LavaService` methods before initializing an engine. Removing the check would convert a graceful "return null" into a NullReferenceException at the call site. The arguably correct behavior is to throw `InvalidOperationException` ("Lava engine has not been initialized") instead of silently returning null, but that is a behavior change orthogonal to perf and would not change the per-call cost. Leave the null checks in place for now; revisit as part of a deliberate API-cleanliness pass if and when one happens.

#### [ ] A20. WebsiteLavaShortcodeProvider.RegisterShortcodes scans all loaded assemblies

**Where:** [Rock/Lava/WebsiteLavaShortcodeProvider.cs:42](../Rock/Lava/WebsiteLavaShortcodeProvider.cs)

`Rock.Reflection.FindTypes(typeof(ILavaShortcode))` scans every loaded assembly. Runs at startup and on every cache clear. Acceptable today; flag if startup time becomes a concern.

**Proposed fix:** None proposed unless this becomes a measured bottleneck. Listed for visibility.

**Status:** Rejected. Worth flagging, but not actionable today. Cache clear is rare (a debugging / diagnostic operation, not a hot path), startup cost is a one-time hit, and there is no immediately better alternative — short of pre-registering shortcode types at compile time, every approach still has to discover plugin-supplied `ILavaShortcode` implementations from loaded assemblies. Revisit only if startup time becomes a measured concern.

#### [x] A21. LavaHelper.ParseCommandMarkup builds regex matches per call without compiling

**Where:** [Rock/Lava/LavaHelper.cs:564-612](../Rock/Lava/LavaHelper.cs)

```csharp
var markupParms = Regex.Matches( resolvedMarkup, @"\S+:('[^']+'|\d+)" )
    .Cast<Match>()
    .Select( m => m.Value )
    .ToList();
```

The regex is not compiled and is recreated per call. Called from every Lava command tag/block render that needs parameter extraction.

**Proposed fix:** Promote to a `private static readonly Regex _markupParamsRegex = new Regex(@"...", RegexOptions.Compiled);`.

## Out of Scope

This spec does not address:

- The Fluid-bridge layer (covered by `specs/260501-lava-fluid-bridge-perf-improvements.md`).
- Filter implementations in `Rock/Lava/Filters/*` and `Rock.Lava/Filters/*` (each filter has its own perf characteristics; needs a separate review).
- The `RockLiquid` legacy DotLiquid path. `ForwardedDotLiquid.cs` in `Rock/Lava/` is a no-op stub for plugins; it's already obsolete and slated for removal in v18.
- Lava authoring guidance for end users.
- New caching infrastructure (e.g., Redis-backed template cache) — those are feature changes, not perf fixes.

## Verification Steps

For each accepted finding:

1. Author a benchmark using BenchmarkDotNet that exercises the affected path with a representative scenario (recommend: a CMS page render with a common-merge-fields call, an entity-driven communication, and a workflow with embedded Lava).
2. Capture before/after numbers for both mean time and allocated bytes.
3. Confirm the existing Lava test suite passes.
4. Spot-check a handful of common templates in a running Rock instance to confirm output is byte-identical.

For thread-safety findings (A6, A7, A8), add a stress test that concurrently renders templates while another thread reconfigures the engine or registers a new shortcode, and assert each render observes a consistent state.

## Considered but Rejected

### Replace LavaServiceProvider with Microsoft.Extensions.DependencyInjection

Considered, and the existing comment in `LavaServiceProvider.cs:25` already notes it as a future direction. Rejected for this spec because the migration would touch every service registration in the codebase and the perf wins from A8 are achievable with a single-line `ConcurrentDictionary` swap. Worth doing eventually, but not as a perf fix.

### Move all merge-field collection out of `GetCommonMergeFields` into a per-request `RockRequestContext`

Considered. The note at line 93-98 of `LavaHelper.cs` explicitly couples this method's behavior to `RockRequestContext`, and a partial migration is already in flight. Out of scope for this spec; the perf improvements here are independent of the structural move.

### Remove the engine-context conversion path in `LavaEngineBase.RenderTemplate`

Considered. The default `LavaRenderContext` exists so callers outside the engine can build a context without depending on a specific engine implementation. Removing the conversion would break that contract. Finding A5 fixes the perf without removing the API.

### Consolidate `LavaService` and `LavaEngineBase` into one class

Considered. Rejected. The split exists to support multiple engine instances (used by tests and by mobile-app rendering) while keeping a single global `LavaService` for the website. Combining them would force the test/mobile paths to go through the website-specific singleton.

## Related

- Companion spec (completed): [Lava → Fluid Bridge: Performance and Allocation Improvements](completed/lava/260501-lava-fluid-bridge-perf-improvements.md)
- `LavaServiceProvider.cs:25` — pre-existing note about migrating to Microsoft.Extensions.DependencyInjection.
- Asana task referenced at [LavaToLiquidTemplateConverter.cs:103](../Rock.Lava/Engine/LavaToLiquidTemplateConverter.cs) for the comment-regex/`elseif` decisions.
