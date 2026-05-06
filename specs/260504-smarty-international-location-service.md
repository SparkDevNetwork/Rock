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
- `Rock/SystemGuid/` — new GUID constant for the new component's `[SystemGuid]` attribute.
- Rock Migrations — seed the new component's `EntityType`, `Attribute` values, and any `DefinedValue` entries.

Secondary (verify unaffected):

- [`Rock/Address/SmartyStreets.cs`](../Rock/Address/SmartyStreets.cs) — existing US service; behavior must not change when the new service is configured alongside it.
- [`Rock/Model/Core/Location/LocationService.cs`](../Rock/Model/Core/Location/LocationService.cs) — iterates active `VerificationComponent` instances via MEF (lines 507-612); **no changes needed here**. Country-based routing is handled inside `Verify()` on the component itself, not at the loop level (see "Routing Mechanics" below).
- [`Rock/Address/VerificationComponent.cs`](../Rock/Address/VerificationComponent.cs) — base class; no signature changes expected.

## UI/UX Design

Add an **International Smarty Streets Location Service** `DefinedValue` under the `Location Services` `DefinedType` (or update the existing Smarty value to include international settings). The component's attribute editor exposes the following configuration properties:

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

The International API returns pre-assembled, correctly formatted mailing lines in the root-level `address1` through `address12` fields. These are ready to use directly — **no manual component assembly is needed.** The `metadata.address_format` field is a display template that explains how Smarty composed those lines (useful for debugging), but the implementation should read from `address1`/`address2`, not parse `address_format`.

Per the Smarty docs, `address1`-`address12` contain correctly formatted address lines only when `analysis.address_precision` = `DeliveryPoint` or `Premise`. At lower precisions the fields may contain standardized or original input data. Rock's implementation should only write these fields to `Location.Street1`/`Street2` when precision is `DeliveryPoint` or `Premise`; otherwise leave the street fields unchanged.

The `building` component (`"Millwoods Pentacostal Assembly"` in the Canada example) may appear as `address1` when the address resolves to a named building. Rock already stores this kind of name in `Location.Name`, so if `address1` matches `location.Name` (case-insensitive trim), the implementation should skip it and promote `address2` → `Street1`, `address3` → `Street2` to avoid duplicating the name in the address lines.

| International API source | Rock `Location` field | US API equivalent | Notes |
|---|---|---|---|
| `address1` (see note above re: `building`) | `Street1` | `delivery_line_1` | Only map when `analysis.address_precision` = `DeliveryPoint` or `Premise`; skip if value matches `Location.Name` and promote next line |
| `address2` (or `address3` if `address1` was skipped) | `Street2` | `delivery_line_2` | May be absent; leave `Street2` unchanged if empty |
| `components.locality` | `City` | `components.city_name` | City / municipality |
| `components.administrative_area` | `State` | `components.state_abbreviation` | Province / state; use `administrative_area` (short form), not `administrative_area_iso2` |
| `components.postal_code` | `PostalCode` | `components.zipcode + "-" + components.plus4_code` | International API returns the full postal code as-is; no plus-4 concept |
| `components.country_iso_3` | `Country` | (not in response) | ISO 3166-1 alpha-3 (e.g., `"CAN"`); normalize to alpha-2 (e.g., `"CA"`) before storing, to match Rock's existing country convention |
| `metadata.latitude` | `GeoPoint` (via `SetLocationPointFromLatLong`) | `metadata.latitude` | Only set when `Enable Geocoding` is true and `metadata.geocode_precision` is in the acceptable list |
| `metadata.longitude` | `GeoPoint` (via `SetLocationPointFromLatLong`) | `metadata.longitude` | Same condition as above |
| `metadata.geocode_precision` | `GeocodeAttemptedResult` | `metadata.precision` | Stored for audit; compared against `Acceptable Geocode Precisions` setting. Valid values: `None`, `AdministrativeArea`, `Locality`, `PostalCode`, `Thoroughfare`, `Premise` |
| `analysis.verification_status` | `StandardizeAttemptedResult` | `analysis.dpv_match_code` | Compared against `Acceptable Verification Statuses` setting. Valid values: `Verified`, `Partial`, `Ambiguous`, `None` |
| `analysis.address_precision` | (secondary audit detail in `resultMsg`) | (no equivalent) | Achieved address precision; valid values: `DeliveryPoint`, `Premise`, `Thoroughfare`, `Locality`, `AdministrativeArea`, `None`. Include alongside `verification_status` in the `resultMsg` string for the Rock Service Log |
| (no equivalent) | `Barcode` | `delivery_point_barcode` | International API has no barcode concept; leave `Barcode` unchanged |
| (no equivalent) | `County` | `metadata.county_name` | International API has no county concept; leave `County` unchanged |

### Result flag logic

| Condition | `VerificationResult` flags set |
|---|---|
| `verification_status` is in `Acceptable Verification Statuses` | `Standardized` |
| `geocode_precision` is in `Acceptable Geocode Precisions` AND `Enable Geocoding` is true | `Geocoded` |
| HTTP status is not 200 | `ConnectionError` |
| API returns an empty candidate list | `None` |

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

Configure both components in Rock Admin under `General Settings > Location Services`:

1. **Smarty Streets** (existing) — order 1, active. Handles US addresses.
2. **Smarty Streets International** (new) — order 2, active. The US-skip logic above ensures it never attempts a US address even if the US service failed to geocode one (e.g., a connection error), since the country field will still be US/empty.

If the US service is not configured (international-only installation), set the international component to order 1. US addresses will be returned as `VerificationResult.None` and left un-geocoded, which is the expected behavior for a church that has no US congregation members.

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
- Decorate the new component class with `[SystemGuid.EntityTypeGuid( "..." )]` matching the pattern in `SmartyStreets.cs` (GUID `4278E7EF-221B-45E6-B9C6-5D11884389EF`). Generate a new GUID for the international component.

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
