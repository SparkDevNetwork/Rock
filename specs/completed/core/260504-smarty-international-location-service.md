---
author: Nick Airdo
date_created: 2026-05-04
summary: >-
  Rock v18 removed Bing-based address verification and geocoding, leaving
  international partners (e.g., Hope City Church, Edmonton, Canada) with no
  supported option for non-US address verification. This spec adds a standalone
  "Smarty Streets International" location service component that calls Smarty's
  International Street Address API for non-US addresses, leaves US addresses to
  the existing Smarty Streets component, and surfaces the same verification /
  geocoding configuration surface admins are used to.
contributors: []
related_docs: []
---

# Smarty Streets International Location Service

## Summary

Rock v18 removed Bing Maps as a location service, leaving international partners without address verification or geocoding. The existing Smarty Streets Location Service is limited to US addresses. This spec introduces a new standalone **Smarty Streets International** component that calls Smarty's International Street Address API for non-US addresses while continuing to let the existing US-only component handle US addresses. Admins activate the new component in the Location Services list, provide their own International Auth ID and Auth Token, and gain proximity-based ministry functionality (Group Finder, etc.) for Canadian and other international congregations. A unified single-component approach was evaluated and rejected (see [Considered but Rejected](#considered-but-rejected)).

## Motivation

Hope City Church in Edmonton, Canada opened a support request after upgrading to Rock v18. Their previous workflow used Bing for Canadian address verification and geocoding. With Bing removed and the current Smarty service rejecting non-US addresses, Group Finder proximity searches silently return no results for congregation members whose addresses cannot be geocoded.

Smarty offers an International Street Address API that covers 240+ countries, uses the same Auth ID / Auth Token credential scheme as the US API, and returns structured geocode data suitable for Rock's location model.

## Problem Statement

Rock's `LocationService` geocoding pipeline iterates active `VerificationComponent` instances via MEF. The existing `SmartyStreets` component hard-codes the US Street API endpoint and does not guard against non-US addresses — it simply sends whatever fields are present, and the US API returns no candidates for non-US input. There is no supported path for a non-US address to flow through to a geocoded result.

International partners either leave addresses un-geocoded (breaking proximity features) or cannot upgrade past v17.

## Affected Code Paths

Primary (where the fix lands):

- [`Rock/Address/SmartyStreets.cs`](../Rock/Address/SmartyStreets.cs) — existing US component; reference for structure and field mapping patterns.
- `Rock/Address/SmartyStreetsInternational.cs` — new standalone component class.
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
| 4 | Enable International Geocoding | When **on**, the component sends `geocode=true` on every International Street API request and populates `Location.GeoPoint` from the response. **Requires an additional International Geocoding subscription/add-on from SmartyStreets on top of the base International Address Verification subscription.** When **off** (default), the component omits the `geocode` parameter and does not populate `GeoPoint`, so partners on a verification-only Smarty plan do not get charged for an unused subscription or receive `402 Payment Required` errors. |
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
4. **US address routing.** US addresses bypass the International API. The new Smarty Streets International component is configured alongside the existing US-only Smarty Streets component; Rock's `LocationService` iterates active components, the US component handles US addresses, and the International component handles the remainder. Configuration order is not load-bearing for correctness (see [Routing Mechanics](#routing-mechanics)).
5. **No regression for existing US-only deployments.** Installing the new service alongside the existing US service must not alter US address verification results.
6. **Geocoding-subscription awareness.** When the partner's Smarty plan does not include the International Geocoding add-on, the International Street API returns `402 Payment Required` for requests sent with `geocode=true`. The component must detect this specific status code separately from `401 Unauthorized` (which means the Auth ID / Auth Token are invalid), and on a 402 must log an exception via `Rock.Model.ExceptionLogService.LogException` that names the missing **International Geocoding** subscription explicitly and tells the admin to either disable `Enable International Geocoding` on this component or upgrade their Smarty plan. This way an admin inspecting Rock's Exception Log can tell apart "the Smarty plan needs International Geocoding added" from "the Auth ID / Auth Token are wrong" from a generic connection error, rather than chasing a Rock-side bug. The `resultMsg` itself stays a short HTTP status (e.g., `"Payment Required"` / `"Unauthorized"`) per the resultMsg / Exception Log split documented in Implementation Notes.

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

1. Only proceed if BOTH `analysis.verification_status` is in the configured **Acceptable Verification Statuses** list AND `analysis.address_precision` is in the configured **Acceptable Address Precisions** list. These are the same gates that control `VerificationResult.Standardized` (see "Result flag logic" below) — the address fields and the result flag are updated together or not at all. If either gate fails, leave all address fields unchanged.
2. Parse `metadata.address_format` on `|` to get the ordered list of line templates. Tokenize each line on word boundaries (split on anything that is not a letter or underscore) so combined forms like `"sub_building_number-premise thoroughfare"` produce clean tokens ( `sub_building_number`, `premise`, `thoroughfare` ) for classification. A line is **not** a street line if any extracted token appears in the non-street set: `building`, `locality`, `dependent_locality`, `double_dependent_locality`, `administrative_area`, `sub_administrative_area`, `postal_code`, `postal_code_short`, `postal_code_extra`, `country`, `country_iso_alpha_2`, `country_iso_alpha_3`. Otherwise the line is a street line.
3. The denylist approach (rather than a street-token allowlist) is deliberate. Smarty composes tokens with separators like hyphens — `"sub_building_number-premise thoroughfare"` is the canonical Canadian example — and ships new street-level token names (e.g., `premise_number`, `sub_building_number`) without warning. An allowlist breaks on both. The well-known non-street tokens are stable across countries and Smarty releases.
4. `building` is in the non-street denylist because Rock sends `Street1`/`Street2` to the API as `address1`/`address2` with no separate building field, so any `building` value in the response is Smarty's own inference from the free-text input — not a distinct Rock field. The building name is analogous to a mailing salutation (business or person name) and is not a component of the postal address. Excluding it produces a clean, consistent standardized street address.
5. Collect only the street lines, in order. Map the first street line's corresponding root `address` field to `Street1`, the second (if present) to `Street2`.
6. If no street lines remain after exclusions (e.g., the entire address resolved to only a building name), leave `Street1` and `Street2` unchanged.

**Worked example 1** — `"4919 20th Ave NW, Calgary AB"`:

`address_format` = `"premise thoroughfare|locality administrative_area  postal_code"`

Line 0 tokens: `premise thoroughfare` — street tokens → street line → `address1` = `"4919 20 Ave NW"` → **`Street1` = `"4919 20 Ave NW"`**

Line 1 tokens: `locality administrative_area postal_code` — locality tokens → not a street line → skip

**`Street2` = unchanged**

---

**Worked example 2** — `"Millwoods Pentacostal Assembly 2225 66 St NW, Edmonton AB"`:

`address_format` = `"building|premise thoroughfare|locality administrative_area  postal_code"`

Line 0 tokens: `building` — `building` is in the non-street denylist → skip

Line 1 tokens: `premise`, `thoroughfare` — no non-street tokens → street line → `address2` = `"2225 66 St NW"` → **`Street1` = `"2225 66 St NW"`**

Line 2 tokens: `locality`, `administrative_area`, `postal_code` — locality tokens → not a street line → skip

**`Street2` = unchanged**

---

**Worked example 3** — `"1504, 6608 28 ave, Edm, Alberta, T6k2r1"` (the Canadian apartment-in-a-building case that motivated the denylist approach):

`address_format` = `"sub_building_number-premise thoroughfare|locality administrative_area  postal_code"`

Line 0 tokens (after word-boundary split on the hyphen): `sub_building_number`, `premise`, `thoroughfare` — no non-street tokens → street line → `address1` = `"1504-6608 28 Ave NW"` → **`Street1` = `"1504-6608 28 Ave NW"`**

Line 1 tokens: `locality`, `administrative_area`, `postal_code` — locality tokens → not a street line → skip

**`Street2` = unchanged**

(A whitelist-style classifier, the original spec, would skip Line 0 because the literal token `"sub_building_number-premise"` is not in its set, leaving `Street1` as the un-standardized user input `"1504, 6608 28 ave"`. The denylist + word-boundary tokenization in the algorithm above is what catches this case.)

| International API source | Rock `Location` field | US API equivalent | Notes |
|---|---|---|---|
| Root `address` field for first street line (per algorithm above) | `Street1` | `delivery_line_1` | Only when `analysis.address_precision` is in the configured `Acceptable Address Precisions` list; `building`-only lines are excluded (see algorithm) |
| Root `address` field for second street line (per algorithm above) | `Street2` | `delivery_line_2` | Same gate as `Street1`; leave unchanged if no second street line exists |
| `components.locality` | `City` | `components.city_name` | Always from `components`, never from a root `address` field |
| `components.administrative_area` | `State` | `components.state_abbreviation` | Short form (e.g., `"AB"`); use `administrative_area`, not `administrative_area_iso2` |
| `components.postal_code` | `PostalCode` | `components.zipcode + "-" + components.plus4_code` | Full postal code as-is; no plus-4 concept |
| `components.country_iso_3` | `Country` | (not in response) | ISO 3166-1 alpha-3 (e.g., `"CAN"`); look up alpha-2 via Countries `DefinedType` attribute — see "ISO 3166 Country Code Lookup" below |
| `metadata.latitude` | `GeoPoint` (via `SetLocationPointFromLatLong`) | `metadata.latitude` | Only when `Enable International Geocoding` is true and `metadata.geocode_precision` is in the acceptable list |
| `metadata.longitude` | `GeoPoint` (via `SetLocationPointFromLatLong`) | `metadata.longitude` | Same condition as above |
| `metadata.geocode_precision` | `GeocodeAttemptedResult` | `metadata.precision` | Stored for audit; compared against `Acceptable Geocode Precisions` setting. Valid values: `None`, `AdministrativeArea`, `Locality`, `PostalCode`, `Thoroughfare`, `Premise` |
| `analysis.verification_status` | `StandardizeAttemptedResult` | `analysis.dpv_match_code` | **Standardization gate** alongside `analysis.address_precision`; both must be in their configured acceptable lists for `VerificationResult.Standardized` to fire and for address fields to be updated. Compared against `Acceptable Verification Statuses` setting. Valid values: `Verified`, `Partial`, `Ambiguous`, `None` |
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

**4. Migration — seed alpha-3 values for every existing country `DefinedValue`**

The migration must populate `[AttributeValue]` rows for the new alpha-3 attribute on every country `DefinedValue` Rock currently ships (approximately 249 entries, including territories and dependencies such as Åland Islands, Guernsey, Isle of Man, and Jersey, not only the 193 UN member states). The implementer should source the complete alpha-2 → alpha-3 mapping from the authoritative ISO 3166-1 standard or a well-known open-source dataset (e.g., the `iso-codes` Debian package).

Use a single efficient `Sql()` call rather than ~249 individual `RockMigrationHelper.AddDefinedValueAttributeValue` invocations. A `VALUES` table-valued constructor of `(alpha2, alpha3)` pairs joined against `[DefinedValue].[Value]` covers all countries in one statement, is idempotent via `WHERE NOT EXISTS`, and avoids needing to hardcode per-country DefinedValue GUIDs. The GUID-per-country approach is impractical here because Rock's countries were originally seeded by a pre-2014 helper (`UpdateDefinedValueByName_pre20140819`) that generated random GUIDs at insert time, so there is no canonical, well-known GUID per country to reference.

The `Down()` migration calls `RockMigrationHelper.DeleteAttribute( Rock.SystemGuid.Attribute.COUNTRY_ISO3166_ALPHA3 )`, which removes the attribute definition and (via cascading delete on `[AttributeValue]`) all seeded alpha-3 values in one step.

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
| `analysis.verification_status` is in the configured **Acceptable Verification Statuses** list AND `analysis.address_precision` is in the configured **Acceptable Address Precisions** list | `Standardized` — location fields (`Street1`, `Street2`, `City`, `State`, `PostalCode`, `Country`) are updated at the same time |
| `metadata.geocode_precision` is in the configured **Acceptable Geocode Precisions** list AND `Enable International Geocoding` is true | `Geocoded` — `GeoPoint` is updated at the same time |
| HTTP status is not 200 | `ConnectionError` |
| API returns an empty candidate list | `None` |

Standardization requires BOTH the verification status and the address precision returned by Smarty to be on their respective configured "Acceptable" lists. This mirrors how the existing US `SmartyStreets` component uses `Acceptable DPV Codes` as a standardization gate, so the admin UI checkboxes do what they look like they do. If either list is empty (admin cleared all the checkboxes), no standardization will occur, identical to the US service's behavior.

The success-case `resultMsg` records both `verification_status` and `address_precision` (e.g., `"VerificationStatus:Verified; AddressPrecision:DeliveryPoint; GeocodePrecision:Premise"`) so the Service Log and the `Location.StandardizeAttemptedResult` / `Location.GeocodeAttemptedResult` columns reflect what Smarty actually returned, regardless of whether the gates passed.

---

## Routing Mechanics

`LocationService` (lines 507-612 of `Rock/Model/Core/Location/LocationService.cs`) iterates every active `VerificationComponent` registered via MEF. It has no country awareness — it simply calls `component.Verify( location, out resultMsg )` on each active service until both `standardized` and `geocoded` flags are true.

Country-based routing is therefore handled **inside `Verify()`** on the new standalone Smarty Streets International component, not at the loop level.

### US-address skip guard

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

Returning `VerificationResult.None` without setting a `ConnectionError` flag lets `LocationService` continue the loop. If the US Smarty component is also active, it will handle the address either before or after this component runs. If the US component is not configured, this early return leaves the address un-standardized, which is the correct behavior (the International component should not pretend to handle US addresses).

### Two-service configuration

Both Smarty Streets components are active in Rock Admin under `Settings > System > Location Services`. **The configuration order does not affect correctness.** The US-skip guard inside the International component's `Verify()` is sufficient to prevent it from ever processing a US address, regardless of which component runs first.

`LocationService` only breaks out of the loop when both `standardized` and `geocoded` are `true` (lines 607-610). If the US component standardizes but does not geocode, or geocodes but does not standardize, the loop continues to the next component. The US-skip guard therefore matters in all of these cases, not just when the International component runs first:

- If the US component runs first and sets both flags, the loop breaks before the International component is called.
- If the US component runs first but only partially succeeds (standardized but not geocoded, or connection error), `LocationService` continues to the International component. The skip guard returns `VerificationResult.None` immediately, preventing the International component from re-standardizing the US address or attempting to geocode it via the International API.
- If the International component runs first, the skip guard returns `VerificationResult.None` immediately, and `LocationService` continues to the US component.

In all cases, a US address is standardized exactly once by the US Street API and never touches the International API.

This ordering-independence relies on one assumption that must be preserved: **the existing `SmartyStreets.cs` component does not write `Location.Country`**. The US Street API response contains no country field, and the current implementation does not set `Location.Country` from any other source. As long as this remains true, a US address will always have an empty or `"US"` country value when the International component's skip guard evaluates it. If a future change to `SmartyStreets.cs` were to write `Location.Country`, this guarantee would need to be re-evaluated.

If the US service is not configured (international-only installation), US addresses will be returned as `VerificationResult.None` and left un-geocoded, which is the expected behavior for a church with no US congregation members.

---

## Proposed Design

A new standalone component, `Rock.Address.SmartyStreetsInternational`, sits alongside the existing `Rock.Address.SmartyStreets` component:

- Decorated with `[Description( "Address verification service from SmartyStreets International Address Verification" )]` and `[ExportMetadata( "ComponentName", "Smarty Streets International" )]`.
- Always calls Smarty's International Street API.
- Carries its own Auth ID / Auth Token attributes. Partners enter International credentials here rather than reusing or sharing the existing component's US credentials. (There is no "Use Managed API Key" option; the International API is not part of the Spark-managed Smarty key.)
- Detects US addresses at the top of `Verify()` and returns `VerificationResult.None` so the existing US-only `SmartyStreets` component handles them.
- Geocoding is gated on the component's `Enable International Geocoding` attribute. When enabled, the component sends `geocode=true` to the International API. When disabled, the component omits the parameter so partners on a verification-only Smarty plan do not get charged for an unused subscription.

Zero changes to the existing US `SmartyStreets` component. The new component is independently shippable, easy to test in isolation, and trivially disabled for US-only installations.

## Implementation Notes

- **Class name and attributes.**
  ```csharp
  [Description( "Address verification service from SmartyStreets International Address Verification" )]
  [Export( typeof( VerificationComponent ) )]
  [ExportMetadata( "ComponentName", "Smarty Streets International" )]
  [Rock.SystemGuid.EntityTypeGuid( "{new-guid-here}" )]
  public class SmartyStreetsInternational : VerificationComponent
  ```
  Generate a new uppercase GUID for the `EntityTypeGuid` and register it in `Rock/SystemGuid/`.
- **API.** Smarty's International Street Address API base URL: `https://international-street.api.smarty.com/v1/verify`.
- **Required query parameters:** `auth-id`, `auth-token`, `address1`, `country`. Optional: `address2`, `locality`, `administrative_area`, `postal_code`.
- **Geocoding is opt-in on the request AND opt-in on the Smarty plan.** The International Street API does NOT return `metadata.latitude`, `metadata.longitude`, or `metadata.geocode_precision` by default. Two conditions must both be true to get geocoding:
  1. The component sends `geocode=true` on the request (gated on the `Enable International Geocoding` attribute).
  2. The partner's Smarty plan includes the **International Geocoding** add-on subscription.

  If `geocode=true` is sent on a plan that does not include geocoding, the API returns `402 Payment Required` rather than a verification-only response. (`401 Unauthorized` is a separate failure mode that means the credentials themselves are bad.) The component distinguishes these two status codes in `resultMsg` so admins can tell "the Smarty plan needs International Geocoding added" apart from "the Auth ID / Auth Token are wrong" apart from a generic connection error. This is a material behavioral difference from the existing US `SmartyStreets.cs` component, where the US Street API returns lat/long in the default response with no extra query parameter or subscription tier.

  Gating sketch:

  ```csharp
  // Only send geocode=true when the admin has explicitly opted in via the
  // attribute (which implies the partner's Smarty plan has the International
  // Geocoding add-on). Sending it without the subscription returns 402.
  if ( GetAttributeValue( AttributeKey.EnableInternationalGeocoding ).AsBoolean() )
  {
      query["geocode"] = "true";
  }
  ```

  And on the response side, `metadata.latitude`, `metadata.longitude`, and `metadata.geocode_precision` are only read and applied to `Location.GeoPoint` when the same attribute is on.
- **Response.** The API returns a `precision` field (e.g., `DeliveryPoint`, `Premise`, `Thoroughfare`, `Locality`, `AdministrativeArea`, `None`) and a `verification_status` field (`Verified`, `Partial`, `Ambiguous`, `None`). When `geocode=true` is honored, the response also includes `metadata.latitude`, `metadata.longitude`, `metadata.geocode_precision`, and `metadata.geocode_classification`.
- **Country detection.** Check `Location.Country`. If null, empty, `"US"`, or `"USA"`, treat as US (skip via the routing guard above). Otherwise pass through to the International API.
- **Response models.** The existing `CandidateAddress` / `Components` / `Metadata` / `Analysis` inner classes in `SmartyStreets.cs` are US-specific; a parallel set of inner classes is needed for the international response shape (see "Location Field Mapping" above).
- **`SupportsGeocoding` override.** The component overrides `VerificationComponent.SupportsGeocoding` to return the value of the `Enable International Geocoding` attribute. This lets Rock's `LocationService.Verify` loop skip the geocoding pass for this component entirely when the admin has the attribute disabled, instead of dispatching a `Verify()` call whose geocode result would be discarded anyway.

  ```csharp
  public override bool SupportsGeocoding
  {
      get { return GetAttributeValue( AttributeKey.EnableInternationalGeocoding ).AsBoolean(); }
  }
  ```
- **Error diagnostics split between `resultMsg` and `Rock.Model.ExceptionLogService`.** The component's `resultMsg` lands in `Location.StandardizeAttemptedResult` / `Location.GeocodeAttemptedResult` (both `nvarchar(200)`) and in `ServiceLog.Result`. It is reserved for short, scannable status text the admin sees on the Location record: `"Payment Required"`, `"Unauthorized"`, `"Not Configured"`, `"No Match"`, or the success-case `"VerificationStatus:X; AddressPrecision:Y; GeocodePrecision:Z"`. Full diagnostic detail — which Smarty subscription tier is missing, raw response body for unrecognized HTTP statuses, the credential-misconfigured path, etc. — goes to Rock's Exception Log via `Rock.Model.ExceptionLogService.LogException( $"{{GetType().Name}}: ..." )`, prefixed with the component class name so admins can filter the Exception Log by source. This mirrors the existing US `SmartyStreets.cs` pattern of `resultMsg = response.StatusDescription` on HTTP failure, plus the structured Exception Log entries that the US component does not currently produce. **Long `resultMsg` strings must be avoided**: the 200-character `nvarchar` limit on the two Location columns would otherwise fail entity validation on `SaveChanges`.
- **Migration entity-type registration.** Register the component's `EntityType` in the migration using `UpdateEntityType`, following the same pattern used when `Rock.Address.SmartyStreets` was originally added:

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
- **Country detection edge cases.** Addresses with an empty `Country` field are assumed US. If a non-US address is entered without a country, it may be sent to the US API and fail silently. Mitigation: document the assumption; consider a future block setting to override the default-country assumption.
- **Credential duplication.** Partners must enter Auth ID / Auth Token on both the US and International components. Mitigation: document this clearly in the admin UI description text. The International credentials are a separate Smarty subscription tier and cannot be assumed equivalent to the US credentials anyway, so this is structural rather than incidental.
- **International Verification vs. International Geocoding are separate Smarty subscription tiers.** Verification is the base product; Geocoding is an opt-in add-on. Partners must confirm both are on their Smarty plan if they want full proximity-search behavior. Mitigation: detect `402 Payment Required` responses when `geocode=true` is sent (and `401 Unauthorized` for credential errors) and surface a `resultMsg` that names the missing **International Geocoding** subscription specifically, so admins know to upgrade their Smarty plan rather than chasing a Rock-side bug.
- **Geocode precision vs. US precision naming.** The International API uses different precision labels (`DeliveryPoint`, `Premise`, `Thoroughfare`, `Locality`, `AdministrativeArea`) than the US API's `Zip7`/`Zip8`/`Zip9`. Avoid reusing the same `DefinedValue`-backed precision list for both services without accounting for the label differences.

## Verification Steps

1. Confirm the new `Smarty Streets International` entry appears in `Settings > System > Location Services` after the migration runs, alongside the existing `Smarty Streets` entry.
2. Configure the new component with valid International Smarty credentials (Auth ID / Auth Token from a plan that includes International Verification) and an active Canadian address. Confirm `Location.GeoPoint` is populated and `Location.Country` is set to `"CA"` (assuming the plan also includes International Geocoding).
3. Configure both components active simultaneously. Submit a US address. Confirm the US component standardizes/geocodes it and the International component returns `VerificationResult.None` with the "Skipped: US address handled by US Smarty Streets service" `resultMsg`.
4. Repeat step 3 with the configuration order reversed in Rock Admin. Confirm the result is identical (the skip guard is ordering-independent).
5. Submit a non-US address with only the International component active. Confirm verification and geocoding succeed.
6. Set `Enable International Geocoding` to false on the International component. Submit a Canadian address. Confirm `Location.GeoPoint` is not populated and the API request did not include `geocode=true`.
7. Set `Acceptable Verification Statuses` to `Verified` only on the International component. Submit a `Partial` address. Confirm the location standardization fields are not updated and `resultMsg` records the rejected `verification_status`.
8. Submit a Canadian postal code without a street address. Confirm the response populates city/province if the API returns them.
9. Submit a US address to a system where ONLY the International component is configured (no US service). Confirm the address is returned as `VerificationResult.None` with the skip-guard `resultMsg`, and `LocationService` does not attempt to call the International API for it.
10. Simulate a `402 Payment Required` response from the International API with `Enable International Geocoding` on. Confirm `resultMsg` names the missing International Geocoding subscription explicitly. Repeat with a simulated `401 Unauthorized` and confirm `resultMsg` instead points at invalid credentials (so admins can tell the two failure modes apart at a glance).
11. Confirm no regression for existing US-only deployments by running the existing location-service integration tests.
12. Confirm the new `Country` ISO 3166-1 alpha-3 attribute and the seeded alpha-3 values appear on the Countries `DefinedType` after migration runs.

## Out of Scope

- A new Rock-native Bing replacement for international geocoding. Smarty is the designated path.
- Bulk re-geocoding of existing un-geocoded international addresses. That is a separate workflow.
- UI changes to address entry forms to enforce country selection. Current forms are unchanged.
- Support for Smarty's international postal-code-only lookup endpoint if not already covered by the street verification response.
- Rate-limit management or request queuing for high-volume geocoding scenarios.
- A unified single Smarty component that handles both US and International addresses behind one set of credentials and one admin entry. See [Considered but Rejected](#considered-but-rejected) below.

## Considered but Rejected

### Unified Smarty Streets Service (single component, country-branching inside `Verify()`)

Rejected. The original draft of this spec proposed updating the existing `Rock.Address.SmartyStreets` component to detect the address country and branch internally to either the US Street API or the International Street API. A single admin entry would expose both sets of attributes, and partners would configure one component with one set of credentials.

Reasons for rejection:

1. **Credentials are not shared between Smarty tiers.** The US Smarty subscription and the International Verification subscription use different Auth ID / Auth Token credential pairs in the common case (partners often have one but not the other, and the Spark-managed US key has no International counterpart). A unified component still needs two credential fields, which defeats the "one set of credentials" benefit.
2. **Increased complexity in an already-established component.** The existing `SmartyStreets.cs` is mature and stable. Mixing in a second API surface, second response model, second precision vocabulary, and second result-flag-gating logic raises the surface area for regressions in the US flow that the majority of partners depend on.
3. **Confusing admin UI for US-only partners.** A unified component would expose 16+ attributes, most of which are inert for US-only installations. Two separate components (with distinct names and descriptions in the Location Services list) make the available capabilities obvious at a glance. The screenshot in `Settings > System > Location Services` shows two clearly-labeled entries rather than one component with a long settings panel.
4. **Harder to disable selectively.** A two-component layout lets a US-only installation simply leave the International component inactive (or uninstalled) without configuration. A unified component would always show International attributes regardless.
5. **Two-component layout is correctness-equivalent and ordering-independent.** Per [Routing Mechanics](#routing-mechanics), the US-skip guard inside the International component's `Verify()` makes configuration order irrelevant, removing the main perceived benefit of a unified approach (avoiding priority-ordering pitfalls).

The unified option remains revisitable if partner feedback indicates that two-component configuration is materially burdensome in practice, but at this point there is no evidence that it is.

## Related

- Asana task: [Hope City — International / Canadian address verification post-v18](https://app.asana.com/1/20866866924293/project/1208321217019996/task/1214019459422702?focus=true)
- Existing US component: [`Rock/Address/SmartyStreets.cs`](../Rock/Address/SmartyStreets.cs)
- `LocationService` verification loop: [`Rock/Model/Core/Location/LocationService.cs`](../Rock/Model/Core/Location/LocationService.cs) lines 507-612
- Base component class: [`Rock/Address/VerificationComponent.cs`](../Rock/Address/VerificationComponent.cs)
- Smarty International Address Verification product page: https://www.smarty.com/products/international-address-verification
- Smarty International Geocoding (separate subscription required for lat/long): https://www.smarty.com/products/international-geocoding
- Smarty International Street Address API docs: https://www.smarty.com/docs/cloud/international-street-api
