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
  - specs/260501-lava-fluid-bridge-perf-improvements.md
---

# Lava Engine Abstraction: Performance and Allocation Improvements

## Summary

This is the companion to the Fluid-bridge spec. Where that one targeted the `Rock.Lava.Fluid` translation layer, this one targets the engine-agnostic abstraction that lives above it: `LavaService`, `LavaEngineBase`, `LavaHelper`, `WebsiteLavaTemplateCacheService`, `WebsiteLavaShortcodeProvider`, `WebsiteLavaFileSystem`, `LavaServiceProvider`, the `ResolveMergeFields` extension family, and the small helpers they share. Every Lava call in Rock goes through this layer before it reaches Fluid, so per-render costs here multiply by the same call frequency.

Each finding is a checkbox. Tick the items the team agrees to address; leave the rest for later or for explicit rejection.

## Motivation

The Fluid-bridge findings improve the parser-and-renderer path. They do not help if the abstraction layer above wastes the same allocations on every render. A page that calls `string.ResolveMergeFields(...)` ten times pays the upstream cost ten times before any Fluid code runs. The biggest single offender is the per-render reflection in `Rock.Common.ObjectExtensions.GetPropertyValue`, which is the path used to access anonymous-type properties from Lava. Other significant items are mutable-input bugs (the engine mutating its own input parameters), per-render `Dictionary` allocations from default-context conversion, and per-shortcode O(n) cache scans.

As with the bridge spec, none of these have been benchmarked. The priority ordering reflects expected impact based on call frequency and operation cost. Reviewers are encouraged to challenge the priority before implementation.

## Requirements

- Each finding MUST be addressable independently.
- Functional behavior MUST remain identical. These are perf and allocation changes only.
- Public APIs in `Rock.Lava`, `Rock.Lava.Shared`, and `Rock` (the LavaExtensions namespace) MUST remain backward compatible.
- Thread-safety fixes MUST land regardless of perf impact.
- Each accepted finding SHOULD be benchmarked before and after using a representative template (recommend: a CMS page render, an entity-driven communication, and a workflow with embedded Lava).

## Findings

### P0 — Major hot-path issues

#### [ ] A1. ObjectExtensions.GetPropertyValue uses reflection on every property access

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

**Proposed fix:** Replace the helper internals with a per-`(Type, propertyName)` cache of compiled `Func<object, object>` getters. Path traversal can use an `int` cursor and `IndexOf('.')` instead of repeated list rebuilds. This is the same pattern as findings F2/F3 in the bridge spec but lives in `Rock.Common`, so it benefits more callers.

#### [ ] A2. LavaHelper.GetLavaProperties recomputes per call

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

**Proposed fix:** Add a name → shortcode lookup dictionary on `LavaShortcodeCache`, or build a static `Dictionary<string, LavaShortcodeCache>` (case-insensitive) at the provider level invalidated on `ClearCache()`.

#### [ ] A4. UAParser called per render with no result cache

**Where:** [Rock/Lava/LavaHelper.cs:164-180](../Rock/Lava/LavaHelper.cs)

```csharp
Parser uaParser = Parser.GetDefault();
ClientInfo client = uaParser.Parse( request.UserAgent );
```

`Parser.GetDefault()` is internally cached. The actual `Parse` call runs ~15,000 regex evaluations against the user-agent string. For high-traffic pages this is one of the heaviest ops in `GetCommonMergeFields`. There is no per-request or per-UA-string cache.

**Proposed fix:** Cache `ClientInfo` keyed by user-agent string in a `ConcurrentDictionary` with bounded size (LRU semantics, or just a size cap with eviction-by-clear). Even a simple `HttpContext.Current.Items["__cachedClientInfo"]` would dedupe within a single request.

#### [ ] A5. LavaEngineBase converts a default context to engine context by walking dictionaries twice

**Where:** [Rock.Lava/Engine/LavaEngineBase.cs:657-670](../Rock.Lava/Engine/LavaEngineBase.cs)

```csharp
if ( parameters.Context.GetType() == typeof( LavaRenderContext ) ) {
    callParameters = parameters.Clone();
    var engineContext = NewRenderContext();
    engineContext.SetInternalFields( parameters.Context.GetInternalFields() );
    engineContext.SetMergeFields( parameters.Context.GetMergeFields() );
    engineContext.SetEnabledCommands( parameters.Context.GetEnabledCommands() );
    callParameters.Context = engineContext;
}
```

`GetMergeFields` and `GetInternalFields` materialize a fresh `LavaDataDictionary` (allocating a `Dictionary<string, object>`), then `SetMergeFields` iterates that dictionary and re-emits each entry into the engine context. So every default-context render allocates a dictionary just to read it back.

**Proposed fix:** Add a context-bridge fast path that copies fields without round-tripping through `LavaDataDictionary`. Better still, make `LavaService.RenderTemplate` always create an engine-specific context up front so this conversion is unnecessary.

### P1 — Mutable-input and thread-safety bugs

#### [ ] A6. LavaService.RenderTemplate mutates the input parameters object

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

#### [ ] A7. LavaService.SetCurrentEngine sets _engine = null before assigning the new instance

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

**Proposed fix:** Build the new engine first, then publish it with a single assignment (the lock already serializes writers; readers just need the assignment to be the last operation). Mark `_engine` `volatile` (or use `Volatile.Write`) to ensure publishing visibility.

#### [ ] A8. LavaServiceProvider._services is not thread-safe

**Where:** [Rock.Lava/Core/LavaServiceProvider.cs:29-91](../Rock.Lava/Core/LavaServiceProvider.cs)

`_services` is `Dictionary<>` mutated by `RegisterService` and read by `GetService`. Registrations happen at startup so the race is narrow, but `GetService` is also called from runtime paths that may race with a re-registration during diagnostics or test setup.

**Proposed fix:** `ConcurrentDictionary<>`.

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

### P2 — Per-render allocations

#### [ ] A10. ResolveMergeFieldsInternal does the IsLavaTemplate regex check then the engine does it again

**Where:** [Rock/Utility/ExtensionMethods/LavaExtensions.cs:591-626](../Rock/Utility/ExtensionMethods/LavaExtensions.cs)

`ResolveMergeFieldsInternal` calls `content.IsLavaTemplate()` (which runs the `_hasLavaTags` regex plus the `_lavaCommentMatchGroupsRegex` regex) before the engine runs. The engine's parser then re-scans the content. The pre-check is intentional (skip the engine entirely for non-templates) but the regex is loose: `(?<=\{)[\S\s]+(?<=\})` matches anything between braces, including JSON, CSS, GUIDs in literal text, and so on. False positives still pay the engine cost.

**Proposed fix:** Switch the pre-check to `IsStrictLavaTemplate` which requires `{{`, `{%`, or `{[` patterns. This was already added (and is internal) but is not used by the public extension. Audit for behavior change risk before flipping.

#### [ ] A11. Two duplicate Lava-comment regex implementations

**Where:**
- [Rock/Lava/LavaHelper.cs:707-820](../Rock/Lava/LavaHelper.cs)
- [Rock.Lava/Engine/LavaToLiquidTemplateConverter.cs:109-166](../Rock.Lava/Engine/LavaToLiquidTemplateConverter.cs)

Both files define very similar Lava-comment-stripping regex pipelines. Both are compiled and both are invoked on the same templates (the helper from Rock-side pre-flight, the converter from inside the engine parse). Templates with comments pay the regex cost twice.

**Proposed fix:** Consolidate to a single implementation in `Rock.Lava` (or `Rock.Lava.Shared`) and have both call sites use it. Removing the duplication also reduces the chance the two implementations drift.

#### [ ] A12. ResolveMergeFieldsWithCurrentLavaEngine sets enabled commands as a separate step

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

#### [ ] A15. LavaService.RenderTemplate always inspects HttpContext.Current.Handler

**Where:** [Rock/Lava/LavaService.cs:479-495](../Rock/Lava/LavaService.cs)

`var page = HttpContext.Current?.Handler as RockPage;` runs on every template render, even from background jobs / workflows / API requests where `RockPage` is impossible. The cast itself is cheap, but `HttpContext.Current` access is a `[ThreadStatic]` lookup with a small but real cost.

**Proposed fix:** When the engine has been initialized for a non-web host (jobs, workflows), short-circuit this path. Or: wrap behind `RockApp.Current.IsWebHost` check (or an equivalent flag).

#### [ ] A16. LavaService.RemoveTemplateCacheEntry enumerates all sites per call

**Where:** [Rock/Lava/LavaService.cs:572-598](../Rock/Lava/LavaService.cs)

For each invalidation, the method calls `SiteCache.All()`, distinct-projects themes, then loops to remove a key per theme. `SiteCache.All()` is cached but the projection still allocates. This runs whenever a Lava-bearing setting changes (saving a block, editing a workflow attribute, modifying a defined value template, etc.).

**Proposed fix:** Cache the distinct theme list at module level with invalidation on `SiteCache` clear. Net win is small per call but invalidations happen in bursts.

#### [ ] A17. LavaHelper.GetCommonMergeFields reads SystemSetting per render

**Where:** [Rock/Lava/LavaHelper.cs:207](../Rock/Lava/LavaHelper.cs)

```csharp
mergeFields.Add( "ExperienceMode",
    Rock.Web.SystemSettings.GetValue( SystemKey.SystemSetting.TRAILBLAZER_MODE ).AsBoolean()
        ? "Trailblazer" : "Essentials" );
```

`SystemSettings.GetValue` is cached but allocates a string per call and parses it as a boolean per call. Setting changes infrequently; reading it on every render adds up.

**Proposed fix:** Cache the resolved string ("Trailblazer" or "Essentials") at module level with invalidation on `SystemSettings` clear.

### P3 — Worth knowing, low cost

#### [ ] A18. LavaHelper.GetCurrentPerson walks merge fields then HttpContext.Items

**Where:** [Rock/Lava/LavaHelper.cs:288-311](../Rock/Lava/LavaHelper.cs)

Called by many filters and shortcodes. Each call does a merge-field lookup followed (if null) by an `HttpContext.Items.Contains` check. Fine on its own, but if 10 filters call this, that's 10 merge-field walks for the same value.

**Proposed fix:** Cache the resolved `Person` in an internal context field on first lookup; subsequent calls read the cached value directly.

#### [ ] A19. LavaService methods have heavy null-engine boilerplate

**Where:** Throughout [Rock/Lava/LavaService.cs](../Rock/Lava/LavaService.cs)

Every method begins with `if ( _engine == null ) return null;` (or `return;`). When the engine is configured (the normal case), this is a wasted volatile read on every Lava call. Minor — but the engine is set exactly once at startup, so the null check could be replaced with an assertion once we trust the lifecycle.

**Proposed fix:** Replace runtime null checks with a one-time assertion at engine setup (and let calls fail fast if the engine is missing). Low ROI; only worth doing if the surrounding methods are touched anyway.

#### [ ] A20. WebsiteLavaShortcodeProvider.RegisterShortcodes scans all loaded assemblies

**Where:** [Rock/Lava/WebsiteLavaShortcodeProvider.cs:42](../Rock/Lava/WebsiteLavaShortcodeProvider.cs)

`Rock.Reflection.FindTypes(typeof(ILavaShortcode))` scans every loaded assembly. Runs at startup and on every cache clear. Acceptable today; flag if startup time becomes a concern.

**Proposed fix:** None proposed unless this becomes a measured bottleneck. Listed for visibility.

#### [ ] A21. LavaHelper.ParseCommandMarkup builds regex matches per call without compiling

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

- Companion spec: [Lava → Fluid Bridge: Performance and Allocation Improvements](260501-lava-fluid-bridge-perf-improvements.md)
- `LavaServiceProvider.cs:25` — pre-existing note about migrating to Microsoft.Extensions.DependencyInjection.
- Asana task referenced at [LavaToLiquidTemplateConverter.cs:103](../Rock.Lava/Engine/LavaToLiquidTemplateConverter.cs) for the comment-regex/`elseif` decisions.
