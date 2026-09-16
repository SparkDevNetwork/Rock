---
author: Daniel Hazelbaker
date_created: 2026-05-06
summary: >-
  Rock parses user-agent strings ad hoc at multiple call sites with no shared
  caching. UAParser's Parse runs ~15,000 regex evaluations per invocation, so
  any hot-path consumer pays heavily and there is no shared result cache.
  This spec defines a single Rock-owned helper plus a fresh Rock-owned POCO
  (designed from observed call-site needs, not as a UAParser mirror) so that
  UAParser becomes an internal implementation detail of one helper. The
  existing partial cache in Rock.Net.ClientInformation is the seed for the
  new helper. The existing Rock.Web.BrowserInfo / BrowserOS / BrowserDevice /
  BrowserUserAgent types are not reused (only two in-repo callers, both
  migratable) and the entire BrowserClient family is marked obsolete as part
  of this work. Rock.Model.InteractionDeviceType.GetClientType is also
  obsoleted; its regex logic is absorbed into the new POCO's ClientType
  property so the result piggybacks on the user-agent-string cache.
contributors: []
related_docs:
  - specs/260501-lava-engine-abstraction-perf-improvements.md
---

# Rock User-Agent Helper

## Summary

Rock parses user-agent strings in roughly a dozen places. One place (`Rock.Net.ClientInformation`) has a private bounded `ConcurrentDictionary` cache; the rest do not. Every other call site re-runs UAParser (~15,000 regex evaluations per invocation, ~0.9 ms per call on a stock modern Firefox UA) on every request. This spec proposes:

1. A new public parser interface `IUserAgentParser` with an internal implementation, registered as a singleton via DI. Holds a process-wide cache.
2. A new Rock-owned POCO designed from the observed call-site field-usage audit, not by mirroring UAParser. UAParser may be replaced later (it is 10+ years old and the User-Agent header is being deprecated in favor of client hints), so the POCO is shaped around what Rock actually needs, with sensible types (e.g., nullable ints for version segments) instead of pass-throughs of UAParser's string-segmented shape.
3. Per-instance memoization on `ClientInformation` so repeated access inside a single request resolves without dictionary lookups.
4. Migration of every internal caller to the new API.

The existing `Rock.Web.BrowserInfo`, `BrowserOS`, `BrowserDevice`, `BrowserUserAgent` classes are **not reused**. They were quick UAParser mirrors. Test-deprecation pass found only two in-repo callers (`Bio.ascx.cs`, `HtmlContentDetail.ascx.cs`); both have clean migration paths described in the audit below. The whole family is marked `[Obsolete]` + `[RockObsolete( "X.Y" )]` as part of this work. Plugins that still reference them keep compiling for the deprecation window.

## Motivation

- **Performance.** UAParser's `Parse` is the heaviest per-render operation on web pages that surface `Client.*` merge fields. Folding caching into a shared helper amortizes the cost across every consumer instead of forcing each one to solve it independently.
- **Consistency.** Different call sites currently make different choices about what they extract from the parsed result and how they normalize it (some lower-case `OSFamily`, some do not; some call the full `Parse()`, some call `ParseOS()` and `ParseUserAgent()` separately, paying the regex cost twice). A shared helper defines one canonical shape.
- **Bounded memory.** A naive `ConcurrentDictionary<string, ClientInfo>` grows without bound; a botnet or fuzzed UA header could blow up the working set. The helper needs a cap and an eviction policy.
- **Decouple from UAParser.** Several public surfaces today expose `UAParser.ClientInfo` directly. A Rock-owned POCO lets us replace UAParser later (for example, with a client-hints-aware library) without breaking plugins.

## Measured Cost

Benchmark data point: a known-good modern user-agent string,

```
Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:150.0) Gecko/20100101 Firefox/150.0
```

clocks in at **~0.9 ms per `Parse()` call** with no cache. Every page request triggers at least one user-agent parse (typically several, because `RockRequestContext.GetCommonMergeFields` reaches `ClientInformation.Browser` twice and `LavaHelper.GetCommonMergeFields` calls `Parse` again on the same UA). A simple per-request dedupe alone would already win meaningfully; a process-wide cache compounds the win across requests.

`Parser.GetDefault()` is itself internally cached, so the cost is in `Parse()`, not in the parser construction.

## Goals

- One Rock-owned API that every internal user-agent consumer goes through.
- Process-wide cache keyed by raw user-agent string, bounded so it cannot be weaponized.
- **Eliminate every public exposure of `UAParser` types.** UAParser becomes an internal implementation detail of one helper. Plugins should be able to compile against Rock without referencing UAParser at all.
- **A POCO designed from observed need, not from UAParser's shape.** Each property earns its place in the type because at least one real call site reads it; types are picked for the data (e.g., nullable ints for version components), not to match UAParser's string-everything style.
- Migration path that does not silently break existing plugins or Lava templates.

## Non-Goals

- Replacing UAParser today. The helper abstracts the choice but does not require swapping the parser as part of this work.
- Client-hints (`Sec-CH-UA*`) parsing. Modern browsers are migrating away from the User-Agent header; that is a separate, larger discussion (and is part of the motivation for owning our own POCO so we can fold it in later).
- Reshaping the existing `Rock.Web.BrowserClient` / `BrowserInfo` / `BrowserOS` / `BrowserDevice` / `BrowserUserAgent` types. They are not reused. Marking them obsolete is in scope; modifying their behavior is not.

## Design Direction

This spec used to propose extending the existing `Rock.Web.BrowserInfo` family. That direction is rejected because those types are mirrors of UAParser's shape (string-segmented version components, four trailing fields whose meaning depends on what UAParser happens to populate). They were added quickly, have effectively no in-repo callers, and would constrain the new POCO to a shape we did not design.

Instead: build a fresh POCO with the smallest surface that covers actual observed usage. The "UAParser Field Usage by Project" audit below is the source of truth for which properties the POCO must expose. If a UAParser field is not used anywhere in Rock today, the POCO does not include it.

## Audit of Current Usage

Confirmed call sites that touch UAParser, with current behavior. This is the migration surface.

| File | Current pattern | Shared cache? |
|---|---|---|
| [Rock/Net/ClientInformation.cs](../Rock/Net/ClientInformation.cs) | `internal static GetClientInfoForUserAgent` with `ConcurrentDictionary` capped at 10K, full `Clear()` on overflow. | Yes (only this file) |
| [Rock/Lava/LavaHelper.cs](../Rock/Lava/LavaHelper.cs) (`GetCommonMergeFields`, line ~180) | `Parser.GetDefault().Parse(request.UserAgent)` per render. | No |
| [Rock/Lava/Filters/LavaFilters.cs](../Rock/Lava/Filters/LavaFilters.cs) (`BROWSER` filter, line ~3579) | `Parser.GetDefault().Parse(...)` per filter call; returns `ClientInfo` directly to Lava. | No |
| [Rock/Web/BrowserClient.cs](../Rock/Web/BrowserClient.cs) (`BrowserInfo`) | Calls `ClientInformation.GetClientInfoForUserAgent` (cached). Public ctor accepts `ClientInfo`. | Yes, via `ClientInformation` |
| [Rock/Tasks/AddShortLinkInteraction.cs](../Rock/Tasks/AddShortLinkInteraction.cs) | `Parser.GetDefault().Parse(userAgent)` per task execution. | No |
| [Rock/Model/Core/Interaction/InteractionService.cs](../Rock/Model/Core/Interaction/InteractionService.cs) (`ParseUserAgentString`, line ~367) | Static `_uaParser` field; calls `ParseOS` and `ParseUserAgent` separately, paying the regex cost twice. | No |
| [Rock/Model/Core/SignatureDocument/SignatureDocument.Logic.cs](../Rock/Model/Core/SignatureDocument/SignatureDocument.Logic.cs) (`GetFormattedUserAgent`) | Same pattern as `InteractionService`: `ParseOS` + `ParseUserAgent` separately. | No |
| [Rock/Personalization/PersonalizationRequestFilters/BrowserRequestFilter.cs](../Rock/Personalization/PersonalizationRequestFilters/BrowserRequestFilter.cs) | `_uaParser.ParseUserAgent(...)` (returns just `UAParser.UserAgent`, not a full `ClientInfo`). | No |
| [Rock.SendGrid/Webhook/SendGridEvent.cs](../Rock.SendGrid/Webhook/SendGridEvent.cs) | Per-instance `_clientInfo` field caches within an event; `Parser.GetDefault()` per fresh event. | Per-instance only |
| [Rock.Tests.Integration/TestData/TestDataHelper.Web.cs](../Rock.Tests.Integration/TestData/TestDataHelper.Web.cs) | Test code; static `_uaParser` field. | No |

Hot spot worth calling out: `RockRequestContext.GetCommonMergeFields` reads `ClientInformation.Browser` twice ([line 1015, line 1020](../Rock/Net/RockRequestContext.cs)). Each access today is a dictionary lookup. With per-instance memoization this becomes a single field read.

`InteractionService.ParseUserAgentString` and `SignatureDocument.GetFormattedUserAgent` both call `ParseOS()` and `ParseUserAgent()` as two separate UAParser methods. Each runs its own regex set. A single `Parse()` returns equivalent data and halves the work.

## UAParser Field Usage by Project

This is the source of truth for the POCO design: every UAParser member that any Rock-owned code reads, grouped by project. The new POCO must support every "Field used" entry below; UAParser fields *not* listed do not need to ship in v1.

### Rock.SendGrid

Only used in `Rock.SendGrid.Webhook.SendGridEvent`. Parses the `UserAgent` string sent by SendGrid as part of the event POCO.

| Field used | Notes |
|---|---|
| `ClientInfo.OS.Family` | Surfaced as `SendGridEvent.ClientOs` (string property). |
| `ClientInfo.UA.Family` | Surfaced as `SendGridEvent.ClientBrowser` (string property). |
| `ClientInfo.Device.Family` | Surfaced as `SendGridEvent.ClientDeviceType` (string property). |
| `ClientInfo.Device.Brand` | Surfaced as `SendGridEvent.ClientDeviceBrand` (string property). |

**POCO requirements driven by this project:** `OSFamily`, `BrowserFamily` (or equivalent), `DeviceFamily`, `DeviceBrand`. All as strings.

### Rock.Tests.Integration

Test code only (one usage in `TestDataHelper.Web`). Rewrite freely; does not constrain the POCO.

### Rock.Blocks

References the UAParser package but no source file actually uses it. **Stale package reference**; drop it.

### RockWeb (legacy WebForms site)

| File | Field used | Notes |
|---|---|---|
| `RockWeb/Blocks/Communication/GetCommunication.ashx.cs` | `ClientInfo.OS.ToString()`, `ClientInfo.UA.ToString()` | Want a formatted version string for both OS and browser. |
| `RockWeb/Blocks/Security/CaptivePortal.ascx.cs` | `ClientInfo.OS.Family`, `OS.Major`, `OS.Minor`, `OS.Patch`, `OS.PatchMinor` | Builds a four-component version string like `"10.4.78.23"`. |

**POCO requirements driven by this project:**

- A formatted-version string accessor on the OS (`OSVersion.ToString()` for the dotted form, or `GetOSFamilyVersion()` for the family-prefixed form).
- A formatted-version string accessor on the Browser/UA equivalent.
- Numeric access to OS major/minor/patch/patchMinor (provided by `UserAgentVersion`'s `int? Major/Minor/Patch/PatchMinor`).

This is the call site that tested the version-shape question most directly: `CaptivePortal` reads four discrete version components and builds a dotted string from them. With `UserAgentVersion.ToString()` producing the dotted form, that whole call site collapses to one line.

### Rock (main project)

The Rock project is the largest UAParser consumer. Audit complete.

| File | Field(s) used | Notes |
|---|---|---|
| `Rock/Lava/Filters/LavaFilters.cs` (`Client:'BROWSER'` filter) | Returns full `UAParser.ClientInfo` to Lava templates. **In practice only `ClientInfo.ToString()` works** because `UAParser.ClientInfo` is not registered as a Lava-safe type ([LavaEngineFactory.cs:236-241](../Rock/Lava/LavaEngineFactory.cs:236)). Lava documentation (external) historically suggested individual property access (`.OS`, `.UA`, `.Device`, etc.), but that has been broken since the Fluid migration. | Filter switches to return `UserAgentInfo` (registered as a Lava-safe type in Phase 1). The bare `{{ '' \| Client:'BROWSER' }}` `ToString()` output is preserved (parity test required). The documented per-property access (`.OSFamily`, etc.) starts working for the first time. |
| `Rock/Lava/Filters/LavaFilters.Person.cs` | Unused `using UAParser` | Just delete the using statement. No code change. |
| `Rock/Lava/LavaHelper.cs` (`GetCommonMergeFields`) | `ClientInfo.OS.Family`, `ClientInfo.Device.Family` | Surfaced as `OSFamily` (lower-cased) and `DeviceFamily` Lava merge fields. |
| `Rock/Tasks/AddShortLinkInteraction.cs` | `ClientInfo.OS.ToString()`, `ClientInfo.UA.ToString()` | Persisted as `clientOs` / `clientBrowser` on the interaction row. |
| `Rock/Model/Core/Interaction/InteractionService.cs` (`ParseUserAgentString`) | Equivalent of `ClientInfo.OS.ToString()` and `ClientInfo.UserAgent.ToString()` (the file accesses these via different code patterns, but the end-result strings are what is noted here). | Persisted to `Interaction.InteractionDeviceType`. **Persistence parity required.** |
| `Rock/Model/Core/SignatureDocument/SignatureDocument.Logic.cs` (`GetFormattedUserAgent`) | Equivalent of `ClientInfo.OS.ToString()` and `ClientInfo.UserAgent.ToString()` (different code pattern, same end-result strings). | Persisted in signature document audit text. **Persistence parity required.** |
| `Rock/Personalization/PersonalizationRequestFilters/BrowserRequestFilter.cs` | Equivalent of `ClientInfo.UserAgent.Family` and `ClientInfo.UserAgent.Major`. | Compares browser family + major version against a configured threshold. The major version is read as a string today and string-compared via Rock's `CompareTo` extension; with `int? Major` on the new version POCO this becomes a direct numeric comparison. |
| `Rock/Web/BrowserClient.cs` (`BrowserClient`, `BrowserInfo`, `BrowserOS`, `BrowserDevice`, `BrowserUserAgent`) | All UAParser fields. | **Verification complete.** Only two in-repo callers, both migratable (see "BrowserClient consumers" sub-table below). Whole family is in scope for `[Obsolete]` + `[RockObsolete( "X.Y" )]`. |
| `Rock/Net/ClientInformation.cs` (the `Browser` property itself) | Returns full `UAParser.ClientInfo` to all callers. The fan-out is captured in the next sub-table. | The static cache fields are removed; their role is taken over by the singleton `UserAgentParser`'s internal cache. The `Browser` property is the central public leak; replaced by a new `BrowserInfo` property returning `UserAgentInfo`, with `[Obsolete]` on the original. The deprecated `Browser` getter reads `BrowserInfo?.OriginalClientInfo` (the internal-only deprecation-window holdover). |

#### Fan-out from `ClientInformation.Browser`

Every place that reads `ClientInformation.Browser` to get at parsed UA data. These are the consumers that drive the migration of the public surface.

| File | Field(s) used | Notes |
|---|---|---|
| `Rock/Blocks/RockBlockType.cs` (`IsBrowserSupported`) | `ClientInfo.UA.Family`, `ClientInfo.UA.Major` (parsed to integer) | Internal browser-support check. **Reinforces the version POCO over `System.Version`:** with `int? Major` on the new version POCO, the parse step disappears and the comparison becomes direct. |
| `Rock/Blocks/Types/Mobile/Cms/DailyChallengeEntry.cs` (`GetChallengeDayInteraction`) | `ClientInfo.String` (the raw UA string the parser was given) | Passed to `InteractionService.CreateInteraction()`, which re-parses it. Two-step roundabout for getting the UA back out. The new POCO exposes the raw input as `UserAgent`, so this becomes `info.UserAgent`. **Or:** these callers could skip the parsed object entirely and use `RequestContext.ClientInformation.UserAgent` directly, which they already had access to. Worth flagging as a small follow-up cleanup. |
| `Rock/Blocks/Types/Mobile/Cms/DailyChallengeEntry.cs` (`CreateDayCompleteInteraction`) | `ClientInfo.String` | Same pattern as above. |
| `Rock/Blocks/Types/Mobile/Prayer/PrayerSession.cs` (`BuildContent`) | `ClientInfo.String` | Passed to `PrayerRequestService.EnqueuePrayerInteraction`, which eventually reaches `InteractionService.ParseUserAgentString` (already covered). Same simplification opportunity. |
| `Rock/Net/RockRequestContext.cs` (`GetCommonMergeFields`) | `ClientInformation.Browser.OS.Family`, `ClientInformation.Browser.Device.Family` | Read twice in the same method (the per-instance memoization on the new property collapses these into a single field read). Maps to `info.OSFamily` / `info.DeviceFamily` on the new POCO. |
| `Rock/Personalization/PersonalizationRequestFilters/BrowserRequestFilter.cs` (`IsMatch( RockRequestContext )` overload) | `request.ClientInformation.Browser.UA.Family`, `.Major` | Same as the previously-reported `IsMatch( HttpRequest )` overload, just a different entry path. Both overloads collapse to the same call against the new POCO. |

#### Consumers of the `Rock.Web.BrowserClient` family

A test-deprecation pass found exactly two in-repo callers of `BrowserClient` / `BrowserInfo` / wrapper types. Both have a clean migration path. Plugin-compat verification is complete: the working hypothesis (mark the whole family `[Obsolete]`) is confirmed.

| File | Usage | Migration |
|---|---|---|
| `RockWeb/Blocks/Crm/PersonProfile/Bio.ascx.cs` (`rptPhones_ItemDataBound`) | Reads `RockPage.IsMobileRequest`, which is `RockPage.ClientType == "Mobile"`, and `RockPage.ClientType` calls `InteractionDeviceType.GetClientType(ua)`. | Add a `ClientType` property to the new POCO that absorbs the regex logic from `InteractionDeviceType.GetClientType` (see below). `RockPage.ClientType` and `RockPage.IsMobileRequest` keep their public shape; their bodies switch to read from `ClientInformation.BrowserInfo.ClientType`. Bio.ascx.cs needs no change. |
| `RockWeb/Blocks/Cms/HtmlContentDetail.ascx.cs` (`ShowView`) | Exposes the entire `BrowserClient` object as a Lava merge field named `CurrentBrowser`. Hot path (most pages have at least one HtmlContentDetail block). The merge field is rarely used by templates, but we cannot prove zero use in user-land Lava. | Define a small private `[LavaType]` wrapper class inside `HtmlContentDetail.ascx.cs` that re-exposes the `BrowserClient` shape on top of the public `UserAgentInfo` surface. The wrapper outlives the deprecation window (end-user Lava templates cannot be deprecated on the same schedule as Rock APIs), so it must use only the public POCO. See the migration plan (Phase 5 step 25) for the full field mapping and a noted gap on non-numeric version segments. |

#### Absorbed: `InteractionDeviceType.GetClientType`

`Rock.Model.InteractionDeviceType.GetClientType( string userAgent )` ([source](../Rock/Model/Core/InteractionDeviceType/InteractionDeviceType.Logic.cs:39)) takes a raw UA string and returns one of six values: `"Mobile"`, `"Tablet"`, `"Crawler"`, `"Outlook"`, `"Desktop"`, or `"None"` (for null/empty input). Implementation is four pre-compiled static regexes plus one inline regex (the inline `@"microsoft office"` regex is reconstructed on every call — and the regex itself has no actual regex features, just a literal substring match, so it can be replaced with `IndexOf( "microsoft office", StringComparison.OrdinalIgnoreCase ) >= 0` while we're absorbing the logic).

**Intentional behavior change:** the existing `Regex( @"microsoft office" )` runs case-sensitive (the default for `Regex`), so it only matched the literal lowercase substring. Real-world Outlook calendar-feed UAs typically use capital letters (e.g., `"Microsoft Office/16.0 ..."`), so the existing Outlook branch is effectively dead code in practice. The replacement uses `OrdinalIgnoreCase` and starts catching those UAs for the first time. This means a small number of UAs that previously fell through to `"Desktop"` will now correctly classify as `"Outlook"` after this change. We are accepting this as an intentional fix to the original buggy regex rather than preserving the broken case-sensitive behavior. (The persistence-parity tests against the original regex's output are *not* a constraint here — we are deliberately diverging on this branch.)

Sixteen files call this method. Absorbing it into the new POCO buys two things:

1. The result piggybacks on the UA-string cache (currently every call re-runs the regexes from scratch).
2. The 16 call sites get a one-line migration to `info.ClientType`, with the underlying method marked `[Obsolete]` for the deprecation window.

**Plan:**

- Add `ClientType { get; }` to the new POCO, marked `[RockInternal( "X.Y", true )]`. Values: `"Mobile"`, `"Tablet"`, `"Crawler"`, `"Outlook"`, `"Desktop"`, `"None"` (preserved exactly to keep `Interaction.InteractionDeviceType` row continuity in the database).
- Move the four pre-compiled regexes into the new POCO's parsing path (or a private helper). Replace the inline `@"microsoft office"` regex with a `string.IndexOf( "microsoft office", StringComparison.OrdinalIgnoreCase ) >= 0` check (the original "regex" had no regex features and was being reconstructed on every call).
- Mark `InteractionDeviceType.GetClientType` `[Obsolete( "Use RockApp.Current.GetRequiredService<IUserAgentParser>().Parse(userAgent).ClientType instead." )]` + `[RockObsolete( "X.Y" )]`. Keep the body working through the deprecation window (delegate to the new helper).
- The 16 call sites migrate during this work. Most are inside the same files we are already touching; the rest are one-line swaps.

This pulls `ClientType` out of the deferred list and into v1. `IsMobile`/`IsTablet`/`IsBot` remain deferred — if a caller wants them later they are one comparison against `ClientType` (and that comparison is the most likely shape for a future replacement of `ClientType` itself, see below).

**Why `[RockInternal]` and not fully `public`:** returning a string from `ClientType` is fragile. We may eventually want a strongly-typed enum, or to split `ClientType` into `IsMobile`/`IsTablet`/`IsBot`/`IsOutlook` booleans (a more useful surface for callers than a magic-string check). `[RockInternal]` lets us swap the shape without a public-API deprecation cycle. `keepInternalForever: true` because RockWeb (cross-project) needs access for the `CaptivePortal` block.

**Why not fully `internal`:** three of the four current RockWeb call sites are HTTP handlers (`GetCommunication.ashx.cs`, `GetPersonGroupScheduleFeed.cs`, `GetEventCalendarFeed.cs`) that have no `RockPage` context, so they cannot reach `RockPage.ClientType`. They each independently derive `deviceOs` / `deviceApplication` / `deviceClientType` from the request UA and pass them to `InteractionService.AddInteraction`. **Future cleanup (out of scope here):** those three could be migrated to `InteractionService.CreateInteraction(InteractionInfo)`, which derives the strings internally from the UA. Once that is done, no RockWeb code needs `ClientType` directly and the POCO property can drop the `[RockInternal]` attribute and become fully `internal`.

<!-- Daniel is auditing this; entries above this comment are confirmed. Below are unverified findings to be moved up (or removed) as the audit progresses. -->

*All Rock-project files have been audited. This section is intentionally empty.*

### Consolidated POCO requirements

Properties and methods the POCO exposes, locked from the completed audit:

| Member | Type | Driven by |
|---|---|---|
| `UserAgent` (raw input) | `string` | All consumers (round-tripping, persistence). Three call sites (`DailyChallengeEntry` x2, `PrayerSession`) read the parsed object solely to recover the original UA string via `ClientInfo.String`. |
| `OSFamily` | `string` | SendGrid, RockWeb CaptivePortal, LavaHelper, RockRequestContext. |
| `OSVersion` | `UserAgentVersion` (Rock-owned: `int? Major/Minor/Patch/PatchMinor` plus private string segments backing `ToString()` for non-numeric fidelity) | RockWeb CaptivePortal (4-component dotted), GetCommunication (formatted display), InteractionService/SignatureDocument/AddShortLinkInteraction (persisted display). |
| `BrowserFamily` | `string` | SendGrid, BrowserRequestFilter, RockBlockType. |
| `BrowserVersion` | `UserAgentVersion` (same shape as `OSVersion`) | RockWeb GetCommunication (formatted display), BrowserRequestFilter (numeric major-version comparison), **RockBlockType.IsBrowserSupported (numeric major-version comparison)**. |
| `DeviceFamily` | `string` | SendGrid, LavaHelper, RockRequestContext. |
| `DeviceBrand` | `string` | SendGrid. |
| `DeviceModel` | `string` | HtmlContentDetail `CurrentBrowser` Lava merge field surfaces `BrowserInfo.Device.Model`; the wrapper needs a source. |
| `GetOSFamilyVersion()` | `string` method returning `"{OSFamily} {OSVersion.ToString()}"` with a trailing space trimmed when the version is empty | InteractionService, SignatureDocument, AddShortLinkInteraction (persisted). RockWeb GetCommunication (display). HtmlContentDetail wrapper. Replaces today's `_uaParser.ParseOS(ua).ToString()` shape. |
| `GetBrowserFamilyVersion()` | `string` method, same shape as above but for browser | Same set of consumers. Replaces today's `_uaParser.ParseUserAgent(ua).ToString()` shape. |
| `ToString()` override | `string` matching `UAParser.ClientInfo.ToString()` exactly: `"{OSFamilyVersion} {DeviceFamily} {BrowserFamilyVersion}"` (space-separated, OS first, falling back to `"Other"` for any missing component, e.g. `"Windows 10 Other Chrome 91.0.4472"` or `"Other Other Other"` for empty input) | HtmlContentDetail wrapper's `BrowserInfo.String`, Lava `Client:'BROWSER'` bare-form output. |
| `ClientType` | `string` ("Mobile" / "Tablet" / "Crawler" / "Outlook" / "Desktop" / "None"); marked `[RockInternal( "X.Y", true )]` | Bio.ascx.cs (via `RockPage.IsMobileRequest`), HtmlContentDetail (via `BrowserClient.ClientType` Lava field), and the 16 call sites of `InteractionDeviceType.GetClientType` that migrate during this work. Logic absorbed from `InteractionDeviceType.GetClientType`. **Marked `[RockInternal]` rather than fully public** because returning strings is a fragile long-term API — we may want to switch to an enum (or split into `IsMobile`/`IsTablet`/`IsBot`/`IsOutlook` booleans) without going through a deprecation cycle. `keepInternalForever: true` because RockWeb needs cross-project access. |
| Deferred: `IsUnknown` | `bool` | No call site demands it yet. **Not implemented in v1.** Documented as a regular C# comment (not XMLDoc, not engineering note) on the POCO class noting it as a candidate future addition. |
| Deferred: `IsBot`, `IsMobile`, `IsTablet` | `bool` | No audited call site reads these directly (the few that want this kind of check go through `ClientType` already). **Not implemented in v1.** Documented as a regular C# comment (same convention as above). |

The persistence-format strings (OS, Browser) need to byte-match what UAParser produces today, otherwise `Interaction.InteractionDeviceType` and `SignatureDocument` history fragment into duplicates. UAParser format: `"Family Major.Minor.Patch"` with empty trailing components dropped. The version POCO's `ToString()` mirrors this exactly.

### Decision: version shape

**Use a Rock-owned version POCO, not `System.Version`.** Three independent arguments from the audit:

- `BrowserRequestFilter.IsMatch` reads major version as a string today and uses Rock's `CompareTo` extension to compare against a configured int. With `int? Major`, this collapses to a direct numeric comparison.
- `RockBlockType.IsBrowserSupported` parses major version to int by hand. Same simplification.
- `CaptivePortal.ascx.cs` builds `"10.4.78.23"` from four discrete components. The version POCO's `ToString()` collapses that call site to one line.

`System.Version` is rejected because it cannot represent "Major-only" (Firefox 150 → `Version("150.0")`?) and rejects non-numeric segments. Even if non-numeric segments are rare in real-world UAs today, throwing away UAParser's tolerance for them without confirmation is a one-way door.

Shape:

```csharp
namespace Rock.Net
{
    [LavaType]
    public sealed class UserAgentVersion
    {
        // Privately-stored original string segments preserve fidelity for
        // ToString(). The RFC for user-agent strings allows non-numeric
        // segments (e.g. "Chrome/132.8.28-rc7.8" is valid), so the int?
        // properties below may be null for valid versions; ToString() is
        // backed by the string segments and round-trips correctly.
        private readonly string _major;
        private readonly string _minor;
        private readonly string _patch;
        private readonly string _patchMinor;

        /// <summary>The major version segment as an integer, or null when the segment is missing or non-numeric.</summary>
        public int? Major { get; }

        /// <summary>The minor version segment as an integer, or null when the segment is missing or non-numeric.</summary>
        public int? Minor { get; }

        /// <summary>The patch version segment as an integer, or null when the segment is missing or non-numeric.</summary>
        public int? Patch { get; }

        /// <summary>The patch-minor version segment as an integer, or null when the segment is missing or non-numeric.</summary>
        public int? PatchMinor { get; }

        /// <summary>
        /// Returns the dotted-version form with empty trailing components
        /// dropped, using the original string segments so non-numeric
        /// segments are preserved. Examples: "10", "10.0.4.78",
        /// "132.8.28-rc7.8", "" (when no version data was captured).
        /// </summary>
        public override string ToString();
    }
}
```

Why no public `Raw` property: the original-segment strings are stored privately and exposed via `ToString()`. Callers that need the dotted-version string call `ToString()`; callers that need numeric comparisons read `Major`/`Minor`/etc. There is no observed call site that wants the raw string segments individually as strings, so we do not expose them as such. (If a future caller needs raw individual segments, they can be promoted to public `string MajorRaw` etc. without breaking existing consumers.)


### Public-surface leaks summary

The leaks the new POCO and helper replace:

1. **`Rock.Net.ClientInformation.Browser`** returns `UAParser.ClientInfo`. Mark `[Obsolete]` + `[RockObsolete( "X.Y" )]`, add a new `BrowserInfo` property returning the new POCO.
2. **Lava `Client:'BROWSER'` filter** returns `ClientInfo` to templates. Property access is silently dead today (UAParser is not Lava-safe). Switch the filter to return the new POCO; the new POCO is decorated `[LavaType]` so Lava traversal works. Only `ToString()` parity matters.
3. **`Rock.Personalization.BrowserRequestFilter`** has a private `IsMatch( UAParser.UserAgent )` overload. Internal-only; re-shape to read from the new POCO.
4. **`Rock.Web.BrowserClient` family** — verified safe to obsolete (only two in-repo callers, both migratable; see migration plan).
5. **`Rock.Model.InteractionDeviceType.GetClientType`** — regex logic absorbed into the new POCO's `ClientType` property; method itself marked `[Obsolete]`.

UAParser package references after this work: only `Rock\Rock.csproj` keeps the reference (the new helper uses UAParser internally). `Rock.Blocks.csproj` and `Rock.SendGrid.csproj` drop their references.

## Proposed Design

The plan has three pieces:

1. A new parser interface plus an internal implementation, registered as a singleton via DI.
2. A new POCO, designed from the call-site audit above, not by mirroring UAParser.
3. Extending `Rock.Net.ClientInformation` with a new property that returns the POCO.

### 1. The parser interface and implementation

```csharp
namespace Rock.Net
{
    /// <summary>
    /// Parses raw user-agent strings into a Rock-owned result. Consumers
    /// should resolve this via DI (e.g. RockApp.Current.GetRequiredService
    /// <IUserAgentParser>()) rather than calling the underlying parser
    /// library directly. UAParser is an internal implementation detail.
    /// </summary>
    public interface IUserAgentParser
    {
        /// <summary>
        /// Parses the given user-agent string. Returns a non-null result
        /// even for null or whitespace input so callers can chain into
        /// fields without null-guarding.
        /// </summary>
        UserAgentInfo Parse( string userAgent );
    }

    /// <summary>
    /// Default implementation of <see cref="IUserAgentParser"/>. Holds a
    /// process-wide cache keyed by the raw user-agent string. Registered
    /// as a singleton in the DI container so the cache is shared across
    /// every consumer in the process.
    /// </summary>
    internal sealed class UserAgentParser : IUserAgentParser
    {
        // Cache, parser invocation, eviction policy described in
        // the "Caching" section below.
        public UserAgentInfo Parse( string userAgent ) { ... }
    }
}
```

Why an interface plus an internal implementation rather than a static class:

- **Testability.** Tests can register a mock `IUserAgentParser` in the DI container, or pass one into a fixture, without touching static state.
- **Future flexibility.** The implementation can later move into a separate library (or be replaced when UAParser itself is replaced) by swapping the registration. With a static helper that move is a public-API breaking change; with the interface, the implementation is invisible to consumers.
- **Consistency with Rock conventions.** The codebase already uses this pattern for cross-cutting services: `IConnectionStringProvider`, `IRockRequestContextAccessor`, `ICaptchaProvider`, `IChatProvider`, `IRockContextFactory`, etc. ([Rock.WebStartup/RockApplicationStartupHelper.cs:350+](../Rock.WebStartup/RockApplicationStartupHelper.cs:350)).

The interface is `public` (so plugins and other Rock projects can resolve it). The implementation is `internal sealed` (so plugins cannot construct or extend it). Both live in `Rock.Net` namespace, in `Rock\Rock.csproj` for now. After migration, the implementation file is the **only** file in the solution (outside `Rock\Rock.csproj`'s package reference) that has a `using UAParser`.

### 1a. DI registration

Add to `Rock.WebStartup\RockApplicationStartupHelper.cs` alongside the existing service registrations (line 350+):

```csharp
sc.AddSingleton<IUserAgentParser, UserAgentParser>();
```

Singleton because the cache must be shared across every resolution. Resolving a singleton from `Microsoft.Extensions.DependencyInjection` is a dictionary lookup — fast enough that hot-path callers do not need to cache the resolved reference locally, though they may.

### 1b. How callers reach it

The canonical form is:

```csharp
var info = RockApp.Current.GetRequiredService<IUserAgentParser>().Parse( userAgent );
```

For one-off uses this is read directly inline. For hot-path callers (e.g. the per-instance memoization on `ClientInformation.BrowserInfo`) the resolved instance can be cached locally if profiling shows it matters; the audit so far suggests it does not.

### 2. The new POCO

Designed from the audit, not from UAParser. Type names (`UserAgentInfo`, `UserAgentVersion`) and the flat property layout are locked. Sketch:

```csharp
namespace Rock.Net
{
    /// <summary>
    /// Rock-owned details parsed from a user-agent string. Returned by
    /// <see cref="IUserAgentParser.Parse(string)"/> and by
    /// <see cref="ClientInformation.BrowserInfo"/>.
    /// </summary>
    [LavaType]
    public sealed class UserAgentInfo
    {
        /// <summary>The raw user-agent string this was parsed from.</summary>
        public string UserAgent { get; }

        /// <summary>OS family, e.g. "Windows", "iOS", "Mac OS X". Empty when unknown.</summary>
        public string OSFamily { get; }

        /// <summary>OS version. Null when unknown.</summary>
        public UserAgentVersion OSVersion { get; }

        /// <summary>Browser family, e.g. "Chrome", "Firefox". Empty when unknown.</summary>
        public string BrowserFamily { get; }

        /// <summary>Browser version. Null when unknown.</summary>
        public UserAgentVersion BrowserVersion { get; }

        /// <summary>Device family, e.g. "iPhone", "Other". Empty when unknown.</summary>
        public string DeviceFamily { get; }

        /// <summary>Device brand, e.g. "Apple", "Samsung". Empty when unknown.</summary>
        public string DeviceBrand { get; }

        /// <summary>Device model. Empty when unknown.</summary>
        public string DeviceModel { get; }

        /// <summary>
        /// One of "Mobile", "Tablet", "Crawler", "Outlook", "Desktop", "None".
        /// Logic absorbed from the obsolete InteractionDeviceType.GetClientType.
        /// </summary>
        [RockInternal( "X.Y", true )]
        public string ClientType { get; }

        /// <summary>
        /// Returns "{OSFamily} {OSVersion}" with the trailing space trimmed
        /// when the version is empty (matches the format previously produced
        /// by UAParser's OS.ToString() that is persisted to the database).
        /// </summary>
        public string GetOSFamilyVersion();

        /// <summary>
        /// Returns "{BrowserFamily} {BrowserVersion}" with the trailing
        /// space trimmed when the version is empty (matches the format
        /// previously produced by UAParser's UserAgent.ToString() that is
        /// persisted to the database).
        /// </summary>
        public string GetBrowserFamilyVersion();

        /// <summary>
        /// Returns the space-separated "{OS} {Device} {Browser}" display string,
        /// matching today's UAParser.ClientInfo.ToString() output. Used by
        /// Lava {{ '' | Client:'BROWSER' }} bare-form rendering and by the
        /// HtmlContentDetail CurrentBrowser wrapper's String field.
        /// </summary>
        public override string ToString();

        /// <summary>
        /// Internal-only holdover during the deprecation window for
        /// ClientInformation.Browser. Removed in Phase 6 together with the
        /// obsolete property it backs. Do not use from new code.
        /// </summary>
        [Obsolete( "Internal-only deprecation-window holdover. Will be removed when ClientInformation.Browser is removed." )]
        internal UAParser.ClientInfo OriginalClientInfo { get; }

        // Deferred to v2 (no audited call site needs them today; callers that
        // want this kind of check go through ClientType already):
        //   IsUnknown   -- distinguishes "Other-everywhere" parses from real ones
        //   IsBot       -- crawler/spider check (likely ClientType == "Crawler")
        //   IsMobile    -- likely ClientType == "Mobile"
        //   IsTablet    -- likely ClientType == "Tablet"
    }
}
```


### 3. Extend `Rock.Net.ClientInformation`

```csharp
public class ClientInformation
{
    // Existing.
    public string IpAddress { get; }
    public string UserAgent { get; }
    public IpGeolocation Geolocation { get; }

    // New: Rock-owned, no UAParser exposure.
    public UserAgentInfo BrowserInfo
    {
        get
        {
            if ( !_browserInfoResolved )
            {
                _browserInfo = RockApp.Current.GetRequiredService<IUserAgentParser>().Parse( UserAgent );
                _browserInfoResolved = true;
            }
            return _browserInfo;
        }
    }

    // Obsolete. Backed by an internal-only deprecation-window holdover on
    // UserAgentInfo (see UserAgentInfo.OriginalClientInfo). Both properties
    // are removed together in Phase 6.
    [Obsolete( "Use BrowserInfo instead. The new property returns a Rock-owned type that does not depend on UAParser." )]
    [RockObsolete( "X.Y" )]
    public UAParser.ClientInfo Browser
    {
        get
        {
#pragma warning disable CS0618 // OriginalClientInfo is obsolete; sole legitimate caller.
            return BrowserInfo?.OriginalClientInfo;
#pragma warning restore CS0618
        }
    }
}
```

The `Browser` property keeps compiling for plugins through the deprecation window. New code reads `BrowserInfo`. Per-instance memoization (the `_browserInfoResolved` flag) collapses the `RockRequestContext.GetCommonMergeFields` double-read hot path. The `BrowserInfo.OriginalClientInfo` holdover (see the `UserAgentInfo` sketch above) carries the parsed `UAParser.ClientInfo` for as long as the obsolete `Browser` property exists; both are removed in Phase 6.

### 4. Lava `Client:'BROWSER'` filter

The filter returns the new POCO. **The Lava-shape compatibility concern raised earlier is moot:** `UAParser.ClientInfo` is not a Lava-safe type today and has never been registered. Verified at [Rock/Lava/LavaEngineFactory.cs:236-241](../Rock/Lava/LavaEngineFactory.cs:236), where exactly three types are registered explicitly (`Common.Mobile.DeviceData`, `Utility.RockColor`, `Utilities.ColorPair`) — third-party types Rock cannot decorate. Rock-controlled types use the `[LavaType]` attribute instead, which is the path the new POCOs take.

That means today's filter returns a `ClientInfo` object whose property access is silently dead under Fluid. `{{ '' | Client:'BROWSER' | Property:'OS.Family' }}` returns nothing. The only output that works is the bare `{{ '' | Client:'BROWSER' }}` form, which falls back to `ToString()` — UAParser's space-separated `"{OS} {Device.Family} {UA}"` line (e.g., `"Windows 10 Other Chrome 91.0.4472"`).

So:

- We can pick whatever POCO shape we want without breaking real-world Lava templates (any template that depended on property access has been broken since the Fluid migration).
- The new POCO is decorated with `[LavaType]`. The filter actually becomes useful for the first time.
- The only behavior we need to preserve is the bare-`ToString()` output. The new POCO's `ToString()` produces the same space-separated `"{OS} {Device} {Browser}"` line UAParser does today.

Net effect: the Lava-adapter complication is removed from the spec entirely.

### Caching

- Storage: `ConcurrentDictionary<string, UserAgentInfo>` keyed by raw UA string, held as an instance field on the singleton `UserAgentParser` implementation. Singleton lifetime guarantees one cache per process.
- Cap: 10,000 entries (matches today's behavior in `ClientInformation`).
- Eviction: on overflow, `Clear()`. This is what `ClientInformation` does today and it is acceptable, because:
  - Cache misses fall back to the underlying parser (correct, just slower).
  - The realistic memory footprint at the cap is roughly 5 MB. Not worth a more sophisticated LRU.
  - A botnet that successfully poisons the cache only forces an occasional clear, not unbounded growth.
- Thread safety: `ConcurrentDictionary.GetOrAdd` plus `Clear()`. The race where two threads parse the same UA simultaneously is harmless (they store the same value) and `Clear()` is concurrent-safe.

We explicitly choose this over `MemoryCache` (heavier; sliding expiration adds ticks every read) and over a hand-rolled LRU (more code than the win is worth). If telemetry later shows cache thrash, the eviction can change without touching the public API.

### Fallback semantics

- **Null/whitespace UA**: `IUserAgentParser.Parse` returns a non-null POCO with empty `OSFamily` / `BrowserFamily` / `DeviceFamily` / `DeviceBrand` / `DeviceModel`, null `OSVersion` / `BrowserVersion`, and `ClientType = "None"` (matching today's `InteractionDeviceType.GetClientType` behavior for empty input). Callers can chain into `.OSFamily.ToLower()` without null-guarding. The deprecated `ClientInformation.Browser` continues to return `null` in this case for behavior parity during the deprecation window.
- **Garbage / unrecognized UA**: UAParser returns "Other" on every part. The POCO reflects that. `ClientType` falls through to `"Desktop"` (matching today's `InteractionDeviceType.GetClientType` final-fallback behavior).

### Where things live

| Type | Project / Namespace | Visibility | Status |
|---|---|---|---|
| `Rock.Net.IUserAgentParser` (interface) | `Rock` project, `Rock.Net` namespace | `public` | New |
| `Rock.Net.UserAgentParser` (implementation) | `Rock` project, `Rock.Net` namespace | `internal sealed` | New |
| `Rock.Net.UserAgentInfo` (result POCO) | `Rock` project, `Rock.Net` namespace | `public sealed` | New |
| `Rock.Net.UserAgentVersion` (version POCO) | `Rock` project, `Rock.Net` namespace | `public sealed` | New |
| DI registration | `Rock.WebStartup\RockApplicationStartupHelper.cs` | n/a | New (`AddSingleton<IUserAgentParser, UserAgentParser>()`) |
| `Rock.Net.ClientInformation` | `Rock` project, `Rock.Net` namespace | unchanged | Existing; new `BrowserInfo` property added, old `Browser` property obsoleted |
| `Rock.Web.BrowserClient`, `BrowserInfo`, `BrowserOS`, `BrowserDevice`, `BrowserUserAgent` | `Rock` project, `Rock.Web` namespace | unchanged | Existing; whole family marked `[Obsolete]` + `[RockObsolete( "X.Y" )]`. Not reused. Verified safe to obsolete. |
| `Rock.Model.InteractionDeviceType.GetClientType` | `Rock` project, `Rock.Model` namespace | unchanged | Existing; marked `[Obsolete]` + `[RockObsolete( "X.Y" )]`. Logic absorbed into the new POCO's `ClientType` property. |
| UAParser package reference | Only `Rock\Rock.csproj` | n/a | Removed from `Rock.Blocks.csproj` (unused) and `Rock.SendGrid.csproj` (migrated) |

## Migration Plan

Sequenced so the shared infrastructure lands first, then the consumers migrate one at a time, then the deprecation window closes.

### Phase 1: Build the helper and the POCO

1. Add `Rock.Net.UserAgentVersion`, decorated `[LavaType]`. The version POCO has `int? Major/Minor/Patch/PatchMinor`, private string segments backing `ToString()` for non-numeric-version fidelity, and a UAParser-format `ToString()`.
2. Add `Rock.Net.UserAgentInfo`, decorated `[LavaType]`. This includes:
   - The flat string properties (`OSFamily`, `BrowserFamily`, `DeviceFamily`, `DeviceBrand`, `DeviceModel`).
   - `OSVersion` and `BrowserVersion` returning `UserAgentVersion`.
   - `ClientType` (string, marked `[RockInternal( "X.Y", true )]`), absorbing the logic from `InteractionDeviceType.GetClientType`. Move the four pre-compiled regexes into the new POCO's parsing path. Replace the inline `microsoft office` regex with a case-**insensitive** `IndexOf` substring check (`StringComparison.OrdinalIgnoreCase`). This is an **intentional behavior change** from the existing case-sensitive `Regex( @"microsoft office" )` — see the "Absorbed: InteractionDeviceType.GetClientType" section above for rationale.
   - `GetOSFamilyVersion()` and `GetBrowserFamilyVersion()` methods returning `"{Family} {Version}"` with the trailing space dropped when version is empty (matching today's UAParser-format strings persisted by `InteractionService` / `SignatureDocument` / `AddShortLinkInteraction`).
   - `ToString()` override matching today's `UAParser.ClientInfo.ToString()` byte-for-byte: space-separated `"{OS} {Device.Family} {UA}"` with `"Other"` fallbacks for missing parts (e.g., `"Windows 10 Other Chrome 91.0.4472"`, or `"Other Other Other"` for empty input). Used by Lava `Client:'BROWSER'` bare-form output and by HtmlContentDetail's `BrowserInfo.String` wrapper field.
   - `internal [Obsolete] OriginalClientInfo { get; }` returning the underlying `UAParser.ClientInfo`. Internal-only, marked obsolete from day one to discourage new internal callers; sole legitimate consumer is the obsolete `ClientInformation.Browser` property. Removed in Phase 6 together with the `Browser` property.
   - The deferred-property comments (`IsBot`, `IsMobile`, `IsTablet`, `IsUnknown`) as plain C# `//` comments documenting candidate future additions.

   `[LavaType]` is the path Rock-controlled types take. `LavaEngineFactory.InitializeLavaSafeTypes` is reserved for third-party types we cannot decorate.
3. Add `IUserAgentParser` (public interface) and `UserAgentParser` (`internal sealed` implementation) in `Rock.Net`. The implementation holds the cache.
4. Register the parser as a singleton in `Rock.WebStartup\RockApplicationStartupHelper.cs` alongside the existing service registrations: `sc.AddSingleton<IUserAgentParser, UserAgentParser>();`.

### Phase 2: Migrate consumers with no public-surface impact

5. `Rock/Tasks/AddShortLinkInteraction.cs`: switch to `RockApp.Current.GetRequiredService<IUserAgentParser>().Parse(...)`. Read `info.GetOSFamilyVersion()` for `clientOs` and `info.GetBrowserFamilyVersion()` for `clientBrowser`. Drop `using UAParser`.
6. `Rock/Lava/LavaHelper.GetCommonMergeFields`: switch to `RockApp.Current.GetRequiredService<IUserAgentParser>().Parse(...)`. Read `info.OSFamily.ToLower()` and `info.DeviceFamily`. Drop `using UAParser`.
7. `Rock.SendGrid/Webhook/SendGridEvent.cs`: replace the private `_clientInfo` / `GetClientInfo()` pair with a private `_browserInfo` field populated from `RockApp.Current.GetRequiredService<IUserAgentParser>().Parse(UserAgent)`. The public `ClientOs`/`ClientBrowser`/`ClientDeviceType`/`ClientDeviceBrand` properties read from `_browserInfo.OSFamily`, `_browserInfo.BrowserFamily`, `_browserInfo.DeviceFamily`, `_browserInfo.DeviceBrand`. Drop the UAParser package reference from `Rock.SendGrid.csproj`.
8. `RockWeb/Blocks/Communication/GetCommunication.ashx.cs`: switch to `RockApp.Current.GetRequiredService<IUserAgentParser>().Parse(...)`. Read `info.GetOSFamilyVersion()` and `info.GetBrowserFamilyVersion()`.
9. `RockWeb/Blocks/Security/CaptivePortal.ascx.cs`: switch to `RockApp.Current.GetRequiredService<IUserAgentParser>().Parse(...)`. Replace the four-component manual concat with `info.OSVersion.ToString()` (the version POCO renders the same dotted form). Note: this site uses the version-only `ToString()`, not `GetOSFamilyVersion()`, because it builds its own family-prefixed string downstream.
10. `Rock.Tests.Integration/TestData/TestDataHelper.Web.cs`: switch to `RockApp.Current.GetRequiredService<IUserAgentParser>().Parse(...)`.
11. `Rock/Lava/Filters/LavaFilters.Person.cs`: drop the unused `using UAParser`.
12. Delete the now-stale UAParser package reference from `Rock.Blocks.csproj`.

### Phase 3: Migrate the public surfaces (obsolete + new)

13. `Rock.Net.ClientInformation`:
    - Remove the static `_cachedBrowserInfo` and `_uaParser` fields (replaced by the singleton `UserAgentParser`'s internal cache).
    - Add new property `public UserAgentInfo BrowserInfo { get; }` with per-instance memoization.
    - Mark the existing `Browser` property `[Obsolete(...)]` + `[RockObsolete( "X.Y" )]`. Body reads `BrowserInfo?.OriginalClientInfo` (with `#pragma warning disable CS0618` around the call). Plugins still compile during the deprecation window.
    - Delete the now-internal `GetClientInfoForUserAgent` method.
14. Migrate the in-repo callers of `ClientInformation.Browser` to `ClientInformation.BrowserInfo` so the codebase has zero `[Obsolete]` warnings on `Browser` after Phase 3 lands. This covers:
    - `Rock/Blocks/RockBlockType.cs` (`IsBrowserSupported`): read from `BrowserInfo.BrowserFamily` and `BrowserInfo.BrowserVersion.Major` (numeric comparison; the manual int-parse goes away).
    - `Rock/Blocks/Types/Mobile/Cms/DailyChallengeEntry.cs` (`GetChallengeDayInteraction`, `CreateDayCompleteInteraction`): replace `ClientInfo.String` with `RequestContext.ClientInformation.UserAgent` directly. The parsed object is no longer needed in these methods.
    - `Rock/Blocks/Types/Mobile/Prayer/PrayerSession.cs` (`BuildContent`): same simplification as above.
    - `Rock/Net/RockRequestContext.cs` (`GetCommonMergeFields`): switch the two `Browser.OS.Family` / `Browser.Device.Family` reads to `BrowserInfo.OSFamily` / `BrowserInfo.DeviceFamily`. The double-read collapses with per-instance memoization.
15. `Rock.Lava.Filters.LavaFilters.cs` (`Client:'BROWSER'` filter): change the filter to return the new POCO (already decorated `[LavaType]` in Phase 1). Drop `using UAParser`. Only output parity needed is the bare `ToString()` form (the property-access form is broken today).
16. `Rock.Personalization.BrowserRequestFilter`:
    - Change the private `IsMatch( UAParser.UserAgent ua )` overload to read from the new POCO directly (`BrowserFamily` + `BrowserVersion.Major`).
    - The public `IsMatch(HttpRequest)` body switches from `_uaParser.ParseUserAgent(...)` to `RockApp.Current.GetRequiredService<IUserAgentParser>().Parse(httpRequest.UserAgent)`.
    - The public `IsMatch( RockRequestContext )` overload switches from `request.ClientInformation.Browser.UA` to `request.ClientInformation.BrowserInfo`.
    - Delete the static `_uaParser` field.
    - Drop `using UAParser`.

### Phase 4: Migrate the persistence sites

17. `Rock.Model.InteractionService.ParseUserAgentString`: switch to a single `RockApp.Current.GetRequiredService<IUserAgentParser>().Parse(...)` call. Read `info.GetOSFamilyVersion()` for `deviceOs`, `info.GetBrowserFamilyVersion()` for `deviceApplication`, and `info.ClientType` for `deviceClientType`. Drop the static `_uaParser` field. The result is persisted in `Interaction.InteractionDeviceType` rows; UAParser-format byte-for-byte parity matters here (see Risks).
18. `Rock.Model.SignatureDocument.GetFormattedUserAgent`: same pattern. Same parity concern.

### Phase 5: Deprecate `BrowserClient` family and `InteractionDeviceType.GetClientType`

19. Mark `Rock.Web.BrowserClient`, `BrowserInfo`, `BrowserOS`, `BrowserDevice`, `BrowserUserAgent` `[Obsolete]` + `[RockObsolete( "X.Y" )]`. They keep compiling for the deprecation window.
20. Mark `Rock.Model.InteractionDeviceType.GetClientType` `[Obsolete( "Use RockApp.Current.GetRequiredService<IUserAgentParser>().Parse(userAgent).ClientType instead." )]` + `[RockObsolete( "X.Y" )]`. Body delegates to the new helper.
21. Migrate the 16 in-repo call sites of `GetClientType` to `RockApp.Current.GetRequiredService<IUserAgentParser>().Parse(...).ClientType`. Most are inside files we are already touching this work; the rest are one-line swaps.
22. `RockWeb/Blocks/Crm/PersonProfile/Bio.ascx.cs`: no source change. `RockPage.ClientType` and `RockPage.IsMobileRequest` keep their public shape; their bodies switch to read from `ClientInformation.BrowserInfo.ClientType`.
23. `RockWeb/Blocks/Cms/HtmlContentDetail.ascx.cs`: replace the `BrowserClient` instance assigned to the `CurrentBrowser` Lava merge field with a small private `[LavaType]` wrapper class declared inside the same file. The wrapper outlives the deprecation window (the Lava merge field is end-user-facing and cannot be deprecated on the same schedule as Rock APIs), so it must rely only on the public `UserAgentInfo` surface — not on the internal `OriginalClientInfo` holdover, which goes away in Phase 6.

    **Field mapping (public-API-only):**
    
    | `BrowserClient` member | Source on `UserAgentInfo` |
    |---|---|
    | `ClientType` | `info.ClientType` |
    | `IsMobile` | `info.ClientType == "Mobile"` |
    | `BrowserInfo.String` | `info.ToString()` |
    | `BrowserInfo.OS.Family` | `info.OSFamily` |
    | `BrowserInfo.OS.Major/Minor/Patch/PatchMinor` | `info.OSVersion?.Major?.ToString() ?? ""` etc. — **see gap below** |
    | `BrowserInfo.Device.Family/Brand/Model` | `info.DeviceFamily/DeviceBrand/DeviceModel` |
    | `BrowserInfo.UserAgent.Family` | `info.BrowserFamily` |
    | `BrowserInfo.UserAgent.Major/Minor/Patch` | `info.BrowserVersion?.Major?.ToString() ?? ""` etc. — **see gap below** |

    **Accepted gap: non-numeric version segments.** The wrapper renders `""` for any version segment that the parser stored as a non-numeric string (e.g., the `"rc7"` part of `Chrome/132.8.28-rc7.8`), where today's `BrowserClient` would return the original string. The intersection of (1) Lava template reads `CurrentBrowser` per-segment, (2) the UA has a non-numeric segment in that position is vanishingly small, so we accept the loss rather than expand the public `UserAgentVersion` surface. See the Resolved trail for details. Lava templates that reference `CurrentBrowser` continue to work unchanged for the common case (numeric segments).

### Phase 6: After the deprecation window

24. Remove `ClientInformation.Browser`, `UserAgentInfo.OriginalClientInfo`, the `BrowserClient` family, and `InteractionDeviceType.GetClientType`. At this point UAParser is referenced from exactly one file (`Rock/Net/UserAgentParser.cs`). The HtmlContentDetail private wrapper either stays (if the merge field is still in production templates) or comes out (if dropping the merge field is acceptable). Decision deferred to that release.

## Backward Compatibility

What plugins keep being able to compile against during the deprecation window:

| Surface | Behavior during window | After window |
|---|---|---|
| `request.ClientInformation.Browser` (returns `UAParser.ClientInfo`) | Compiles, still works, marked obsolete with a pointer to `BrowserInfo`. Backed by an internal-only `UserAgentInfo.OriginalClientInfo` holdover (also marked `[Obsolete]`) so the cached parse result is reused without a separate cache. | Removed (along with `OriginalClientInfo`). |
| `request.ClientInformation.BrowserInfo` (returns `UserAgentInfo`) | New, no UAParser dependency. | Same. |
| `Rock.Web.BrowserInfo` (and ctor taking `ClientInfo`) | Compiles unchanged; marked `[Obsolete]`. | Removed. |
| `RockPage.BrowserClient.BrowserInfo` | Compiles unchanged; marked `[Obsolete]`. | Removed. |
| `RockPage.ClientType`, `RockPage.IsMobileRequest` | Unchanged shape. Body now reads from `ClientInformation.BrowserInfo.ClientType`. | Unchanged. |
| `Rock.Model.InteractionDeviceType.GetClientType( string )` | Compiles unchanged, marked `[Obsolete]`. Body delegates to the new helper. | Removed. |
| Lava `{{ CurrentBrowser ... }}` merge field on HtmlContentDetail | Unchanged shape via a private `[LavaType]` wrapper inside `HtmlContentDetail.ascx.cs`. | Unchanged (wrapper stays as long as the merge field exists). |
| Lava `{{ '' \| Client:'BROWSER' }}` (bare, `ToString()` form) | Returns the new POCO. The `ToString()` output matches today's UAParser-formatted line. | Same. |
| Lava `{{ '' \| Client:'BROWSER' \| Property:'OS.Family' }}` etc. (property access) | Broken today (UAParser type is not Lava-safe); newly works after this change because the new POCO is decorated `[LavaType]`. | Same. |

After the deprecation window, plugins that still reference `UAParser` types from `ClientInformation.Browser` fail to compile. The migration target is one line: `.Browser` → `.BrowserInfo` plus updating to the new POCO's property names.

## Risks

- **Persistence-format parity (`GetOSFamilyVersion()`, `GetBrowserFamilyVersion()`).** `InteractionService.ParseUserAgentString` and `SignatureDocument.GetFormattedUserAgent` persist UAParser's `OS.ToString()` / `UserAgent.ToString()` output to the database. UAParser's format is `"Family Major.Minor.Patch"` with empty trailing components dropped. The new POCO's `GetOSFamilyVersion()` / `GetBrowserFamilyVersion()` methods (and the underlying `UserAgentVersion.ToString()` they call) must produce **byte-identical** output to UAParser's, otherwise existing rows fragment into duplicates with the new rows.
- **Persistence-format parity (`ClientType`).** Same concern for the third persisted column: today's `InteractionDeviceType.GetClientType` returns one of six values, and the new POCO's `ClientType` must produce identical results — **with one intentional exception**: the case-insensitive `microsoft office` substring check (see the "Absorbed: InteractionDeviceType.GetClientType" section). UAs containing `"Microsoft Office"` with capital letters that previously fell through to `"Desktop"` will now correctly classify as `"Outlook"`. This is a deliberate fix to a latent bug. All other branches of the absorption are pure code-moves (same regexes, same fallthrough order), so the parity risk for those is accidental drift during the move.
- **Lava `ToString()` parity.** The `Client:'BROWSER'` filter's bare-form output (`{{ '' | Client:'BROWSER' }}` with no further accessor) needs to match today's UAParser-formatted line. Property-access forms are not a parity concern because they have been broken since the Fluid migration (UAParser types are not Lava-safe).
- **HtmlContentDetail `CurrentBrowser` shape parity.** Lava templates that consume the `CurrentBrowser` merge field need to see the same property names and values as today's `BrowserClient` shape (with the accepted exception of non-numeric version segments — see the Resolved trail).
- **Cache poisoning.** A malicious actor could send millions of distinct UA strings to force repeated `Clear()` cycles. The cap of 10K bounds memory; the `Parse()` cost on a clear-and-refill is the same as no cache. Acceptable.

## Open Questions

(None remaining at spec-finalization time. Items below are resolved and kept for the audit trail.)

### Resolved

- **POCO type names.** Locked: `UserAgentInfo` (main POCO), `UserAgentVersion` (version sub-type), `IUserAgentParser` / `UserAgentParser` (interface and implementation). `BrowserInfo` was avoided because it collides with the existing `Rock.Web.BrowserInfo`.
- **POCO shape.** Locked: flat property layout (`OSFamily`, `BrowserFamily`, `DeviceFamily`, etc.), except `OSVersion` and `BrowserVersion` which return the `UserAgentVersion` sub-type because version is its own structured concept. Flat layout keeps IntelliSense readable without nesting.
- **Plugin-compat verification on `Rock.Web.BrowserClient` family.** Test-deprecation pass found exactly two in-repo callers (`Bio.ascx.cs`, `HtmlContentDetail.ascx.cs`), both with clean migration paths. Whole family is in scope for `[Obsolete]` marking.
- **Version type.** Rock-owned `UserAgentVersion` with `int? Major/Minor/Patch/PatchMinor` plus private string segments backing `ToString()` for non-numeric-version fidelity. `System.Version` rejected (cannot represent "Major-only" cleanly, rejects non-numeric segments).
- **`ClientType`.** Pulled out of "deferred." Becomes a v1 property on the POCO; absorbs the regex logic from `InteractionDeviceType.GetClientType` so 16 call sites + the Bio.ascx.cs path migrate cleanly. Marked `[RockInternal( "X.Y", true )]` because returning a magic-string is a fragile public API; we want the freedom to switch to an enum or split into booleans without a public-API deprecation cycle.
- **`IsBot`, `IsMobile`, `IsTablet`, `IsUnknown`.** Still deferred. Any caller that wants these can compare against `ClientType`. Documented as plain C# `//` comments on the POCO class as candidate future additions.
- **Deprecation-window holdover for `ClientInformation.Browser`.** Single cache, with `internal [Obsolete]` `UserAgentInfo.OriginalClientInfo` carrying the `ClientInfo`. Both removed together in Phase 6.
- **Non-numeric version-segment fidelity in the HtmlContentDetail `CurrentBrowser` wrapper.** Accept the loss. The wrapper renders `""` for any version segment that the parser stored as a non-numeric string (e.g., the `"rc7"` part of `Chrome/132.8.28-rc7.8`). Justification: non-numeric segments are extremely rare in real-world UAs (especially in major or minor positions), the `CurrentBrowser` merge field is rarely used by Lava templates in the first place, and the intersection of the two — a Lava template that reads a per-segment field on a UA whose corresponding segment is non-numeric — is vanishingly small. `UserAgentVersion` keeps `int? Major/Minor/Patch/PatchMinor` only; the private string segments stay private. If a future caller surfaces a real need, promoting the segments to public is a non-breaking change.

## Out of Scope

- Replacing UAParser with a different library. The helper abstracts the choice but does not require changing it.
- Client-hints (`Sec-CH-UA*`) parsing. Modern browsers are migrating away from the User-Agent header; that is a separate, larger discussion.
- Reshaping the existing `Rock.Web.BrowserClient` / `BrowserInfo` / `BrowserOS` / `BrowserDevice` / `BrowserUserAgent` types. Marking them obsolete is in scope; reshaping their behavior is not.
- Migrating the three RockWeb `.ashx` handlers (`GetCommunication.ashx.cs`, `GetPersonGroupScheduleFeed.cs`, `GetEventCalendarFeed.cs`) to `InteractionService.CreateInteraction(InteractionInfo)` so they no longer read `ClientType` directly. That refactor would let the new POCO's `ClientType` graduate from `[RockInternal]` to fully `internal`. Tracked as a future cleanup.

## Affected Code Paths

Full list (also serves as the implementation checklist):

**New files:**

- `Rock/Net/IUserAgentParser.cs` — public parser interface.
- `Rock/Net/UserAgentParser.cs` — `internal sealed` parser implementation; holds the cache.
- `Rock/Net/UserAgentInfo.cs` — public Rock-owned result POCO.
- `Rock/Net/UserAgentVersion.cs` — public Rock-owned version POCO.

**Existing files modified:**

- [Rock.WebStartup/RockApplicationStartupHelper.cs](../Rock.WebStartup/RockApplicationStartupHelper.cs) (register the parser as a singleton: `sc.AddSingleton<IUserAgentParser, UserAgentParser>();`)
- [Rock/Net/ClientInformation.cs](../Rock/Net/ClientInformation.cs) (cache moves out; new `BrowserInfo` property; obsolete `Browser`)
- [Rock/Web/UI/RockPage.cs](../Rock/Web/UI/RockPage.cs) (`ClientType`, `IsMobileRequest` bodies switch to read from `ClientInformation.BrowserInfo.ClientType`)
- [Rock/Model/Core/InteractionDeviceType/InteractionDeviceType.Logic.cs](../Rock/Model/Core/InteractionDeviceType/InteractionDeviceType.Logic.cs) (mark `GetClientType` `[Obsolete]`; body delegates to the new helper)
- [Rock/Lava/LavaHelper.cs](../Rock/Lava/LavaHelper.cs)
- [Rock/Lava/Filters/LavaFilters.cs](../Rock/Lava/Filters/LavaFilters.cs) (`Client:'BROWSER'` filter return type)
- [Rock/Lava/Filters/LavaFilters.Person.cs](../Rock/Lava/Filters/LavaFilters.Person.cs) (drop unused `using UAParser`)
- [Rock/Tasks/AddShortLinkInteraction.cs](../Rock/Tasks/AddShortLinkInteraction.cs)
- [Rock/Model/Core/Interaction/InteractionService.cs](../Rock/Model/Core/Interaction/InteractionService.cs) (persistence parity)
- [Rock/Model/Core/SignatureDocument/SignatureDocument.Logic.cs](../Rock/Model/Core/SignatureDocument/SignatureDocument.Logic.cs) (persistence parity)
- [Rock/Personalization/PersonalizationRequestFilters/BrowserRequestFilter.cs](../Rock/Personalization/PersonalizationRequestFilters/BrowserRequestFilter.cs) (private overload signature change; static field removed)
- [Rock/Blocks/RockBlockType.cs](../Rock/Blocks/RockBlockType.cs) (`IsBrowserSupported` reads from new POCO; `Major` int parse goes away)
- [Rock/Blocks/Types/Mobile/Cms/DailyChallengeEntry.cs](../Rock/Blocks/Types/Mobile/Cms/DailyChallengeEntry.cs) (use `ClientInformation.UserAgent` directly instead of round-tripping through the parsed object)
- [Rock/Blocks/Types/Mobile/Prayer/PrayerSession.cs](../Rock/Blocks/Types/Mobile/Prayer/PrayerSession.cs) (same simplification)
- [Rock/Net/RockRequestContext.cs](../Rock/Net/RockRequestContext.cs) (the two `ClientInformation.Browser.*` reads in `GetCommonMergeFields` switch to `BrowserInfo`)
- The 16 in-repo callers of `InteractionDeviceType.GetClientType` (most overlap with files already in this list; the rest are one-line swaps)
- [Rock.SendGrid/Webhook/SendGridEvent.cs](../Rock.SendGrid/Webhook/SendGridEvent.cs)
- [Rock.SendGrid/Rock.SendGrid.csproj](../Rock.SendGrid/Rock.SendGrid.csproj) (drop UAParser package reference)
- [Rock.Blocks/Rock.Blocks.csproj](../Rock.Blocks/Rock.Blocks.csproj) (drop unused UAParser package reference)
- [RockWeb/Blocks/Communication/GetCommunication.ashx.cs](../RockWeb/Blocks/Communication/GetCommunication.ashx.cs)
- [RockWeb/Blocks/Security/CaptivePortal.ascx.cs](../RockWeb/Blocks/Security/CaptivePortal.ascx.cs)
- [RockWeb/Blocks/Cms/HtmlContentDetail.ascx.cs](../RockWeb/Blocks/Cms/HtmlContentDetail.ascx.cs) (introduce a private `[LavaType]` wrapper for the `CurrentBrowser` merge field)
- [Rock.Tests.Integration/TestData/TestDataHelper.Web.cs](../Rock.Tests.Integration/TestData/TestDataHelper.Web.cs)

**Marked `[Obsolete]`:**

- [Rock/Web/BrowserClient.cs](../Rock/Web/BrowserClient.cs) — `BrowserClient`, `BrowserInfo`, `BrowserOS`, `BrowserDevice`, `BrowserUserAgent`. Whole file. Not reshaped.
- `InteractionDeviceType.GetClientType( string )` (in the file already listed above).
- `ClientInformation.Browser` (in the file already listed above).
- `UserAgentInfo.OriginalClientInfo` — internal, marked obsolete from day one. Sole legitimate consumer is the obsolete `ClientInformation.Browser`. Both are removed in Phase 6.

## Related

- [Lava Engine Abstraction: Performance and Allocation Improvements](completed/lava/260501-lava-engine-abstraction-perf-improvements.md) (finding A4 deferred to this spec).
