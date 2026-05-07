---
author: Nick Airdo
date_created: 2026-05-04
summary: >-
  Rock v18 removed Bing-based address verification and geocoding, leaving
  international partners (e.g., Hope City Church, Edmonton, Canada) with no
  supported option for non-US address verification. This spec adds a Smarty
  International Location Service that calls Smarty's International Street
  Address API, routes US addresses through the existing US Street API, and
  surfaces the same verification/geocoding configuration surface as the current
  US-only service.
contributors: []
related_docs: []
---

# Smarty International Location Service

## Summary

Rock v18 removed Bing Maps as a location service, leaving international partners without address verification or geocoding. The existing Smarty Streets Location Service is limited to US addresses. This spec introduces an International Smarty Streets Location Service (or an updated unified service) that routes non-US addresses through Smarty's International Street Address API while continuing to route US addresses through the existing US Street API. The feature restores proximity-based ministry functions (Group Finder, etc.) for Canadian and other international partners.

## Motivation

Hope City Church in Edmonton, Canada opened a support request after upgrading to Rock v18. Their previous workflow used Bing for Canadian address verification and geocoding. With Bing removed and the current Smarty service rejecting non-US addresses, Group Finder proximity searches silently return no results for congregation members whose addresses cannot be geocoded.

Smarty offers an International Street Address API that covers 240+ countries, uses the same Auth ID / Auth Token credential scheme as the US API, and returns structured geocode data suitable for Rock's location model.

## Problem Statement

Rock's `LocationService` geocoding pipeline iterates active `VerificationComponent` instances via MEF. The existing `SmartyStreets` component hard-codes the US Street API endpoint and does not guard against non-US addresses — it simply sends whatever fields are present, and the US API returns no candidates for non-US input. There is no supported path for a non-US address to flow through to a geocoded result.

International partners either leave addresses un-geocoded (breaking proximity features) or cannot upgrade past v17.

## Affected Code Paths

Primary (where the fix lands):

- [`Rock/Address/SmartyStreets.cs`](../Rock/Address/SmartyStreets.cs) — existing US component; reference for structure and field mapping patterns.
- `Rock/Address/SmartyStreetsInternational.cs` — new component class (Option B) or the file above updated (Option A).
- [`Rock/SystemGuid/Attribute.cs`](../Rock/SystemGuid/Attribute.cs) — new `COUNTRY_ISO3166_ALPHA3` GUID constant (under `#region Country Attributes`) and new GUID for the component's `[SystemGuid.EntityTypeGuid]` attribute.
- [`Rock/SystemKey/CountryAttributeKey.cs`](../Rock/SystemKey/CountryAttributeKey.cs) — new `ISO3166Alpha3` key constant.
- Rock Migrations — (a) `UpdateEntityType` to register the new component, (b) add the `ISO 3166 Alpha-3` attribute to the Countries `DefinedType`, (c) seed `AddDefinedValueAttributeValue` for all ~249 country `DefinedValue`s.

Secondary (verify unaffected):

- [`Rock/Address/SmartyStreets.cs`](../Rock/Address/SmartyStreets.cs) — existing US service; behavior must not change when the new service is configured alongside it.
- [`Rock/Model/Core/Location/LocationService.cs`](../Rock/Model/Core/Location/LocationService.cs) — iterates active `VerificationComponent` instances via MEF (lines 507-612); **no changes needed here**. Country-based routing is handled inside `Verify()` on the component itself, not at the loop level (see "Routing Mechanics" below).
- [`Rock/Address/VerificationComponent.cs`](../Rock/Address/VerificationComponent.cs) — base class; no signature changes expected.

## UI/UX Design

Once the new component class is compiled and decorated with `[Export(typeof(VerificationComponent))]`, it will appear automatically in Rock under **Settings > System > Location Services** alongside the existing Smarty Streets entry. No `DefinedType` or `DefinedValue` is involved — the page is MEF-driven. Admins can reorder the components using the drag handle and toggle them active/inactive from that same page.

The new component's configuration attributes (accessible via the edit icon on that page) should expose the following properties:

| # | Property | Notes |
|---|---|---|
| 1 | Active | Standard on/off toggle |
| 2 | Auth ID | Smarty credential |
| 3 | Auth Token | Smarty credential |
| 4 | Enable Geocoding | Whether to populate lat/long from the API response |
| **Acceptable Verification Statuses** | | |
| 5 | Ambiguous | Accept addresses with ambiguous verification status |
| 6 | Partial | Accept partially-verified addresses |
| 7 | Verified | Accept fully-verified addresses |
| **Acceptable Address Precisions** | | |
| 8 | Thoroughfare | Street-level match |
| 9 | Premise | Premise-level match |
| 10 | Delivery Point | Delivery-point-level match |
| **Acceptable Geocode Precisions** | | |
| 11 | Administrative Area | State/province-level geocode |
| 12 | Locality | City/municipality-level geocode |
| 13 | Postal Code | Postal-code-level geocode |
| 14 | Thoroughfare | Street-level geocode |
| 15 | Premise | Premise-level geocode |
| 16 | Delivery Point | Delivery-point-level geocode |

These settings mirror the structure of the existing US Smarty service to keep the admin experience consistent.

## Acceptance Criteria

1. **Behavioral parity with the US service.** Verification, geocoding, and status filtering work identically to the current `SmartyStreetsLocationService` for any fields the International API exposes.
2. **Country field population.** The `Location.Country` field is populated from the verification response's country data when it is absent or blank before verification.
3. **International postal code lookup.** Non-US postal codes resolve to city/state equivalents via the International API's postal-code endpoint (if Smarty exposes one) or from the address verification response.
4. **US address routing.** US addresses bypass the International API and use the US Street API in both supported configurations:
   - A unified Smarty Streets Location Service that handles both US and international addresses.
   - A standalone International Location Service configured alongside the existing US-only service (Rock calls services in priority order; the US service handles US addresses first; the international service handles the remainder).
5. **No regression for existing US-only deployments.** Installing the new service alongside the existing US service must not alter US address verification results.

## Location Field Mapping

The table below defines how Rock's `Location` model fields map to the International Street API request parameters and back from the API response. It also shows the equivalent mapping in the existing US service (`SmartyStreets.cs`) for comparison.

### Request (Rock Location → API)

| Rock `Location` field | International API parameter | US API equivalent | Notes |
|---|---|---|---|
| `Street1` | `address1` | `street` | Required |
| `Street2` | `address2` | `street2` | Optional |
| `City` | `locality` | `city` | Optional but improves match rate |
| `State` | `administrative_area` | `state` | Optional |
| `PostalCode` | `postal_code` | `zipcode` | Optional |
| `Country` | `country` | (not sent) | **Required** for international; ISO 3166-1 alpha-2 preferred (e.g., `"CA"`). If `Location.Country` is null, empty, `"US"`, or `"USA"`, the component routes to the US Street API instead (see "Routing Mechanics"). |
| `Name` | (not sent) | `addressee` | The international API does not have an addressee field. |

### Response (API → Rock Location)

The International API returns pre-assembled mailing lines in the root-level `address1` through `address12` fields, and individual parsed values in `components`. **Do not use root `address` fields for `City`, `State`, or `PostalCode` — always read those from `components` directly.** The root `address` fields are only used for street lines, and only when the precision is sufficient.

The reason: root `address` fields mirror `metadata.address_format` lines verbatim. For a Canadian address the format is `"premise thoroughfare|locality administrative_area  postal_code"`, which means `address1` = the street line and `address2` = the city/province/postal line. Writing `address2` to `Street2` would duplicate the locality data that is already being written to `City`, `State`, and `PostalCode` from `components`.

**Standardization and geocoding are evaluated independently.** Either can succeed without the other. The street line algorithm below governs standardization only (`Street1`, `Street2`, `City`, `State`, `PostalCode`, `Country` and `VerificationResult.Standardized`). Geocoding (`GeoPoint` and `VerificationResult.Geocoded`) is gated separately on `metadata.geocode_precision` vs. the configured Acceptable Geocode Precisions list — see "Result flag logic" below.

**Street line selection algorithm (standardization only):**

1. Only proceed if `analysis.address_precision` is in the component's configured **Acceptable Address Precisions** list. This is the same gate that controls `VerificationResult.Standardized` (see "Result flag logic" below) — the address fields and the result flag are updated together or not at all. If `address_precision` is not in the acceptable list, leave all address fields unchanged.
2. Parse `metadata.address_format` on `|` to get the ordered list of line templates. A line is a **street line** if it contains only street-level tokens from this set: `premise`, `premise_extra`, `sub_building`, `thoroughfare`, `dependent_thoroughfare`, `post_box`. A line is **not** a street line if it contains `building`, `locality`, `administrative_area`, `postal_code`, or similar.
3. `building` is always excluded from street line classification. Rock sends `Street1`/`Street2` to the API as `address1`/`address2` with no separate building field, so any `building` value in the response is Smarty's own inference from the free-text input — not a distinct Rock field. The building name is analogous to a mailing salutation (business or person name) and is not a component of the postal address. Excluding it produces a clean, consistent standardized street address.
4. Collect only the street lines, in order. Map the first street line's corresponding root `address` field to `Street1`, the second (if present) to `Street2`.
5. If no street lines remain after exclusions (e.g., the entire address resolved to only a building name), leave `Street1` and `Street2` unchanged.

**Worked example 1** — `"4919 20th Ave NW, Calgary AB"`:

`address_format` = `"premise thoroughfare|locality administrative_area  postal_code"`

Line 0 tokens: `premise thoroughfare` — street tokens → street line → `address1` = `"4919 20 Ave NW"` → **`Street1` = `"4919 20 Ave NW"`**

Line 1 tokens: `locality administrative_area postal_code` — locality tokens → not a street line → skip

**`Street2` = unchanged**

---

**Worked example 2** — `"Millwoods Pentacostal Assembly 2225 66 St NW, Edmonton AB"`:

`address_format` = `"building|premise thoroughfare|locality administrative_area  postal_code"`

Line 0 tokens: `building` — excluded (building is never a street line) → skip

Line 1 tokens: `premise thoroughfare` — street tokens → street line → `address2` = `"2225 66 St NW"` → **`Street1` = `"2225 66 St NW"`**

Line 2 tokens: `locality administrative_area postal_code` — locality tokens → not a street line → skip

**`Street2` = unchanged**

| International API source | Rock `Location` field | US API equivalent | Notes |
|---|---|---|---|
| Root `address` field for first street line (per algorithm above) | `Street1` | `delivery_line_1` | Only when `analysis.address_precision` is in the configured `Acceptable Address Precisions` list; `building`-only lines are excluded (see algorithm) |
| Root `address` field for second street line (per algorithm above) | `Street2` | `delivery_line_2` | Same gate as `Street1`; leave unchanged if no second street line exists |
| `components.locality` | `City` | `components.city_name` | Always from `components`, never from a root `address` field |
| `components.administrative_area` | `State` | `components.state_abbreviation` | Short form (e.g., `"AB"`); use `administrative_area`, not `administrative_area_iso2` |
| `components.postal_code` | `PostalCode` | `components.zipcode + "-" + components.plus4_code` | Full postal code as-is; no plus-4 concept |
| `components.country_iso_3` | `Country` | (not in response) | ISO 3166-1 alpha-3 (e.g., `"CAN"`); look up alpha-2 via Countries `DefinedType` attribute — see "ISO 3166 Country Code Lookup" below |
| `metadata.latitude` | `GeoPoint` (via `SetLocationPointFromLatLong`) | `metadata.latitude` | Only when `Enable Geocoding` is true and `metadata.geocode_precision` is in the acceptable list |
| `metadata.longitude` | `GeoPoint` (via `SetLocationPointFromLatLong`) | `metadata.longitude` | Same condition as above |
| `metadata.geocode_precision` | `GeocodeAttemptedResult` | `metadata.precision` | Stored for audit; compared against `Acceptable Geocode Precisions` setting. Valid values: `None`, `AdministrativeArea`, `Locality`, `PostalCode`, `Thoroughfare`, `Premise` |
| `analysis.verification_status` | `StandardizeAttemptedResult` | `analysis.dpv_match_code` | Compared against `Acceptable Verification Statuses` setting. Valid values: `Verified`, `Partial`, `Ambiguous`, `None` |
| `analysis.address_precision` | (secondary audit detail in `resultMsg`) | (no equivalent) | Achieved address precision; valid values: `DeliveryPoint`, `Premise`, `Thoroughfare`, `Locality`, `AdministrativeArea`, `None`. Include alongside `verification_status` in the `resultMsg` string for the Rock Service Log |
| (no equivalent) | `Barcode` | `delivery_point_barcode` | International API has no barcode concept; leave `Barcode` unchanged |
| (no equivalent) | `County` | `metadata.county_name` | International API has no county concept; leave `County` unchanged |

### ISO 3166 Country Code Lookup

The International API always returns `components.country_iso_3` as an ISO 3166-1 alpha-3 code (e.g., `"CAN"`, `"EGY"`, `"GBR"`). Rock's Countries `DefinedType` (`D7979EA1-44E9-46E2-BF37-DDAF7F741378`) stores two-character alpha-2 values in `DefinedValue.Value` (e.g., `"CA"`, `"EG"`, `"GB"`). There is no reliable algorithmic conversion between the two — the codes are independently assigned with no common pattern (e.g., `ALA` → `AX` for Åland Islands).

Rather than a static dictionary in code, the mapping is stored as a new well-known `Attribute` on the Countries `DefinedType`, following the exact pattern used for `PostalCodeLabel` and the Languages ISO attributes. This makes the mapping data-driven, admin-inspectable, and available to any future code that needs alpha-3 lookups — not just this component.

**Four deliverables are required:**

**1. New key in `Rock/SystemKey/CountryAttributeKey.cs`**

Add alongside the existing `PostalCodeLabel`, `CityLabel`, etc.:

```csharp
/// <summary>
/// The ISO 3166-1 alpha-3 code for this country.
/// Used to map responses from the Smarty International Street Address API
/// (which returns alpha-3) back to Rock's alpha-2 country values.
/// </summary>
public const string ISO3166Alpha3 = "core_CountryISO3166Alpha3";
```

**2. New GUID in `Rock/SystemGuid/Attribute.cs`** (under the `#region Country Attributes` block)

```csharp
/// <summary>
/// Country - ISO 3166-1 Alpha-3 Code
/// </summary>
public const string COUNTRY_ISO3166_ALPHA3 = "{new-guid-here}";
```

Generate a new uppercase GUID following the existing convention in that file.

**3. Migration — add the `Attribute` to the `DefinedType`**

```csharp
RockMigrationHelper.AddDefinedTypeAttribute(
    Rock.SystemGuid.DefinedType.LOCATION_COUNTRIES,
    Rock.SystemGuid.FieldType.TEXT,
    "ISO 3166 Alpha-3",
    Rock.SystemKey.CountryAttributeKey.ISO3166Alpha3,
    "The ISO 3166-1 alpha-3 country code (e.g., \"CAN\" for Canada). Used to match responses from international address verification services.",
    0,
    true,
    string.Empty,
    false,
    true,
    Rock.SystemGuid.Attribute.COUNTRY_ISO3166_ALPHA3 );
```

**4. Migration — seed `AddDefinedValueAttributeValue` for all existing country `DefinedValue`s**

For every country `DefinedValue` in Rock's Countries `DefinedType`, add its alpha-3 value using `AddDefinedValueAttributeValue`. ISO 3166-1 covers approximately 249 entries including territories and dependencies — not just 193 UN member states (e.g., `ALA` for Åland Islands, `GGY` for Guernsey, `IMN` for Isle of Man, `JEY` for Jersey). The implementer must source the complete mapping from the authoritative ISO 3166-1 standard or a well-known open-source dataset (e.g., the `iso-codes` Debian package). The call pattern matches the Languages migration exactly:

```csharp
// Canada
RockMigrationHelper.AddDefinedValueAttributeValue(
    "{guid-of-canada-defined-value}",
    Rock.SystemGuid.Attribute.COUNTRY_ISO3166_ALPHA3,
    "CAN" );

// United States
RockMigrationHelper.AddDefinedValueAttributeValue(
    "{guid-of-us-defined-value}",
    Rock.SystemGuid.Attribute.COUNTRY_ISO3166_ALPHA3,
    "USA" );

// ... one call per country DefinedValue
```

The `Down()` migration calls `RockMigrationHelper.DeleteAttribute( Rock.SystemGuid.Attribute.COUNTRY_ISO3166_ALPHA3 )`.

**Component lookup in `Verify()`**

Once the attribute is seeded, the component resolves the alpha-3 code to alpha-2 by querying the Countries `DefinedType` cache:

```csharp
var iso3 = candidate.components?.country_iso_3;
if ( !string.IsNullOrWhiteSpace( iso3 ) )
{
    var countryValue = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.LOCATION_COUNTRIES )
        ?.DefinedValues
        .FirstOrDefault( dv => dv.GetAttributeValue( Rock.SystemKey.CountryAttributeKey.ISO3166Alpha3 )
            .Equals( iso3, StringComparison.OrdinalIgnoreCase ) );

    location.Country = countryValue?.Value ?? iso3;
}
```

If no matching `DefinedValue` is found (e.g., a territory whose alpha-3 was not seeded), the raw alpha-3 value is stored as a fallback rather than leaving `Location.Country` blank.

### Result flag logic

| Condition | `VerificationResult` flags set |
|---|---|
| `analysis.address_precision` is in the configured **Acceptable Address Precisions** list | `Standardized` — location fields (`Street1`, `Street2`, `City`, `State`, `PostalCode`, `Country`) are updated at the same time |
| `metadata.geocode_precision` is in the configured **Acceptable Geocode Precisions** list AND `Enable Geocoding` is true | `Geocoded` — `GeoPoint` is updated at the same time |
| HTTP status is not 200 | `ConnectionError` |
| API returns an empty candidate list | `None` |

`analysis.verification_status` and `analysis.address_precision` are both recorded in `resultMsg` for the Rock Service Log regardless of whether the flags are set. `verification_status` alone does not gate any field updates or result flags — `address_precision` is the sole standardization gate.

---

## Routing Mechanics

`LocationService` (lines 507-612 of `Rock/Model/Core/Location/LocationService.cs`) iterates every active `VerificationComponent` registered via MEF. It has no country awareness — it simply calls `component.Verify( location, out resultMsg )` on each active service until both `standardized` and `geocoded` flags are true.

Country-based routing is therefore handled **inside `Verify()`** on the new component, not at the loop level.

### Option B — standalone international component

At the top of `Verify()`, inspect `location.Country`:

```csharp
// Determine whether this address should be handled by the US Street API or the
// International Street API. US addresses (empty country, "US", or "USA") are
// skipped here; the existing SmartyStreets US component handles them.
var country = location.Country?.Trim();
var isUsAddress = string.IsNullOrWhiteSpace( country )
    || country.Equals( "US", StringComparison.OrdinalIgnoreCase )
    || country.Equals( "USA", StringComparison.OrdinalIgnoreCase );

if ( isUsAddress )
{
    resultMsg = "Skipped: US address handled by US Smarty Streets service.";
    return VerificationResult.None;
}
```

Returning `VerificationResult.None` without setting a `ConnectionError` flag lets `LocationService` continue the loop. If the US Smarty component is also active and has a higher priority order, it will already have handled the address before the international component is called. If the US component is not configured, the international component's early return leaves the address un-standardized, which is the correct behavior (the component should not pretend to handle US addresses).

### Option A — unified component

The same country detection runs at the top of `Verify()`, but instead of returning early, the method branches to the appropriate API endpoint:

```csharp
if ( isUsAddress )
{
    return VerifyWithUsStreetApi( location, out resultMsg );
}
else
{
    return VerifyWithInternationalApi( location, out resultMsg );
}
```

This requires the unified component to carry the full US Street API implementation (currently in `SmartyStreets.cs`) plus the new International API implementation. The `SmartyStreets.cs` component would either be deprecated or left active for installs that do not want the unified version.

### Two-service configuration (Option B recommended setup)

Configure both components active in Rock Admin under `General Settings > Location Services`. **The order does not affect correctness** — the US-skip guard inside the International component's `Verify()` is sufficient to prevent it from ever processing a US address, regardless of which component runs first.

`LocationService` only breaks out of the loop when both `standardized` and `geocoded` are `true` (lines 607-610). If the US component standardizes but does not geocode, or geocodes but does not standardize, the loop continues to the next component. The US-skip guard therefore matters in all of these cases, not just when the International component runs first:

- If the US component runs first and sets both flags, the loop breaks before the International component is called.
- If the US component runs first but only partially succeeds (standardized but not geocoded, or connection error), `LocationService` continues to the International component. The skip guard returns `VerificationResult.None` immediately, preventing the International component from re-standardizing the US address or attempting to geocode it via the International API.
- If the International component runs first, the skip guard returns `VerificationResult.None` immediately, and `LocationService` continues to the US component.

In all cases, a US address is standardized exactly once by the US Street API and never touches the International API.

This ordering-independence relies on one assumption that must be preserved: **the existing `SmartyStreets.cs` component does not write `Location.Country`**. The US Street API response contains no country field, and the current implementation does not set `Location.Country` from any other source. As long as this remains true, a US address will always have an empty or `"US"` country value when the International component's skip guard evaluates it. If a future change to `SmartyStreets.cs` were to write `Location.Country`, this guarantee would need to be re-evaluated.

If the US service is not configured (international-only installation), US addresses will be returned as `VerificationResult.None` and left un-geocoded, which is the expected behavior for a church with no US congregation members.

---

## Proposed Design

### Option A: Unified Service (preferred)

Update `SmartyStreetsLocationService` to detect the address country before calling the API:

- If `Country` is `"US"` or empty (default assumption), call the existing US Street API endpoint.
- Otherwise, call the International Street Address API endpoint.

Add the international configuration attributes to the existing component's attribute set. Mark them as visible only when a country other than US is detected, or always show them and let admins configure both tiers.

**Pros:** Single component to configure; admins do not need to understand Rock's service priority ordering.
**Cons:** Increases complexity of an already-established component; international attributes may confuse US-only administrators.

### Option B: Standalone International Service (simpler, lower risk)

Create a new `SmartyStreetsInternationalLocationService` that:

- Always calls the International Street API.
- Detects US addresses and delegates to the US Street API (or explicitly skips, relying on the existing US service being configured at a higher priority).

**Pros:** Zero changes to the existing US service; easier to test in isolation; easy to disable for US-only installations.
**Cons:** Admins must configure two services and understand priority ordering; credentials are entered twice.

### Recommendation

Implement **Option B** first, as it is lower risk and independently shippable. If partner feedback indicates that two-service configuration is too burdensome, Option A can be pursued in a follow-up.

## Implementation Notes

- Smarty's International Street Address API base URL: `https://international-street.api.smarty.com/v1/verify`
- Required query parameters: `auth-id`, `auth-token`, `address1`, `country`. Optional: `address2`, `locality`, `administrative_area`, `postal_code`.
- The API returns a `precision` field (e.g., `DeliveryPoint`, `Premise`, `Thoroughfare`, `Locality`, `AdministrativeArea`, `None`) and a `verification_status` field (`Verified`, `Partial`, `Ambiguous`, `None`).
- Geocode coordinates are returned in the `metadata` object as `latitude` and `longitude`.
- Country detection: check `Location.Country`. If null or `"US"` (or `"USA"`), treat as US. Otherwise pass through to the International API.
- The existing `CandidateAddress` / `Components` / `Metadata` / `Analysis` inner classes in `SmartyStreets.cs` are US-specific; a parallel set of inner classes is needed for the international response shape (see "Location Field Mapping" above).
- Decorate the new component class with `[Rock.SystemGuid.EntityTypeGuid( "..." )]`. Generate a new uppercase GUID for the international component and register it in `Rock/SystemGuid/`.
- Register the component's `EntityType` in the migration using `UpdateEntityType`, following the same pattern used when `Rock.Address.SmartyStreets` was originally added:

```csharp
RockMigrationHelper.UpdateEntityType(
    "Rock.Address.SmartyStreetsInternational",
    "Smarty Streets International",
    "Rock.Address.SmartyStreetsInternational, Rock, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
    false,
    true,
    "{new-guid-here}" );
```

## Fix Risks

- **API contract differences.** The International API response schema differs from the US API. Deserializing into the wrong model silently drops fields. Mitigation: use separate response models and map only the fields Rock's location model requires.
- **Country detection edge cases.** Addresses with an empty `Country` field are assumed US. If a non-US address is entered without a country, it may be sent to the US API and fail silently. Mitigation: document the assumption; consider a block setting to override the default-country assumption.
- **Credential duplication (Option B).** Partners must enter Auth ID / Auth Token in two components. Mitigation: document this clearly in the admin UI description text; consider a future unified-credentials approach.
- **International API availability and rate limits.** Smarty International is a separate subscription tier. Partners must confirm their Smarty plan includes international access. Mitigation: return a meaningful error message when the API responds with a 401 or 402 rather than silently failing.
- **Geocode precision vs. US precision naming.** The International API uses different precision labels than the US API. Avoid reusing the same `DefinedValue`-backed precision list for both services without accounting for the label differences.

## Verification Steps

1. Configure the new International Location Service with valid Smarty credentials and an active Canadian address. Confirm `Location.GeoPoint` is populated and `Location.Country` is set to `"CA"`.
2. Configure the International service alongside the existing US service (US at higher priority). Submit a US address. Confirm the US service handles it (verified via API log) and the International service is not called.
3. Submit a non-US address with only the International service active. Confirm verification and geocoding succeed.
4. Set `Enable Geocoding` to false. Confirm lat/long is not populated even when the API returns coordinates.
5. Set `Acceptable Verification Statuses` to `Verified` only. Submit a `Partial` address. Confirm the location is not updated.
6. Submit a Canadian postal code without a street address. Confirm the response populates city/province if the API returns them.
7. Submit a US address to the International service alone (no US service configured). Confirm it routes to the US Street API (Option A) or returns a meaningful not-handled result (Option B).
8. Confirm no changes to existing US Smarty service behavior by running the existing location-service integration tests.
9. Confirm the new `DefinedValue` is seeded correctly by the migration and appears in the `Location Services` list in Rock Admin.

## Out of Scope

- A new Rock-native Bing replacement for international geocoding. Smarty is the designated path.
- Bulk re-geocoding of existing un-geocoded international addresses. That is a separate workflow.
- UI changes to address entry forms to enforce country selection. Current forms are unchanged.
- Support for Smarty's international postal-code-only lookup endpoint if not already covered by the street verification response.
- Rate-limit management or request queuing for high-volume geocoding scenarios.

## Related

- Asana task: [Hope City — International / Canadian address verification post-v18](https://app.asana.com/1/20866866924293/project/1208321217019996/task/1214019459422702?focus=true)
- Existing US component: [`Rock/Address/SmartyStreets.cs`](../Rock/Address/SmartyStreets.cs)
- `LocationService` verification loop: [`Rock/Model/Core/Location/LocationService.cs`](../Rock/Model/Core/Location/LocationService.cs) lines 507-612
- Base component class: [`Rock/Address/VerificationComponent.cs`](../Rock/Address/VerificationComponent.cs)
- Smarty International Street Address API docs: https://www.smarty.com/docs/cloud/international-street-api
