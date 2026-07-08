---
title: Connection Request Entry
last_updated: 2026-07-07
related_specs:
  - specs/completed/connection/260622-connection-request-entry.md
related_files:
  - Rock.Blocks/Connection/ConnectionRequestEntry.cs
  - Rock.JavaScript.Obsidian.Blocks/src/Connection/connectionRequestEntry.obs
  - Rock/Model/CRM/Person/Person.cs
  - Rock.Migrations/Migrations/202607062123251_AddConnectionRequestEntryBlock.cs
---

# Connection Request Entry

## Overview

`Connection Request Entry` is a public-facing Obsidian block (`Rock.Blocks/Connection/ConnectionRequestEntry.cs`) that gives an external visitor one self-service form to say who they are and pick several ways to get involved at once. On submit it matches or creates the visitor's Person (and optional spouse), saves their contact info and address, and creates one `ConnectionRequest` per selected opportunity. It ships seeded onto a new external page, "Get Connected" at `/connect/get-connected`.

## Why It Exists

Rock's existing `ConnectionOpportunitySignup` block signs a person up for exactly one pre-pinned opportunity and collects only name, email, phone, and comments. Churches wanted a single general-purpose "get involved" page where a guest can choose from several opportunities in one pass while staff control which fields are asked for and which are required. This block is the multi-select, fully-configurable answer to that gap.

## Mental Model

Think of it as a guest-facing intake form with three moving parts layered on top of the standard Obsidian block lifecycle:

- **A configurable form surface.** Almost every field is governed by a Show / Hide / Required setting, and all visible copy (banner, section titles and descriptions, success text) is a block setting. The block computes each field's visibility server-side and the Vue component renders accordingly.
- **A match-or-create identity step.** A logged-in visitor is used directly; an anonymous visitor is matched against existing People by name/email/phone, and only created when no match is found. This is the same proven pattern `ConnectionOpportunitySignup` and `PrayerRequestEntry` use.
- **A fan-out of connection requests.** One submission produces N requests, one per selected opportunity, plus an optional extra for a configured first-time-guest opportunity. Each request takes its status and connector from its opportunity's connection type defaults.

The guiding principle throughout the server code is "do not trust the client." Visibility settings are not just a UI concern; the server re-checks them before writing any field, and re-validates every submitted opportunity against the configured Connection Types before building a request.

## What You Need to Know

- **`Connection Types` is the one required setting.** It determines which opportunities are offered (active opportunities under those types). With it unset the block renders nothing for visitors and shows an admin-only warning instead of an empty form (`ConnectionRequestEntry.cs:512`).

- **Server-side visibility enforcement is a security boundary, not a nicety.** `CreateNewPerson` and the spouse/attribute save paths only carry a submitted value onto the record when `IsFieldShown(...)` is true for that field (`ConnectionRequestEntry.cs:1169`). A crafted payload cannot set a field the form hid (Email, Gender, Birthdate, person attributes, opportunity attributes). When adding a new field, wire it through `IsFieldShown` on the save path too, or you open exactly the hole this pattern closes.

- **Submitted opportunities are re-validated.** Never assume the posted opportunity list is legitimate. Each is checked with `IsOpportunitySelectable` (active, active connection type, and that type is in the configured `Connection Types` set) before a request is built (`ConnectionRequestEntry.cs:1368`). The first-time-guest opportunity is admin-configured, so it is trusted and exempt from the membership check, but still must be active (`ConnectionRequestEntry.cs:1393`).

- **At least one opportunity is required, with one exception.** Submission fails unless a selectable opportunity was chosen, OR the visitor checked first-time-guest and a First Time Guest Opportunity is configured, in which case that request alone is valid (`ConnectionRequestEntry.cs:1091`).

- **Only public attributes in the configured category are accepted.** Person attributes are filtered to `IsPublic` attributes in the `Person Attribute Category`; opportunity request attributes are filtered to the public set the form offered (`ConnectionRequestEntry.cs:1227`, `ConnectionRequestEntry.cs:1377`). Anything else in the payload is dropped.

- **Preferred Service Time persists to a first-class Person column.** When a `Preferred Service Time` schedule category is configured, the chosen schedule is written to `Person.PreferredServiceTimeScheduleId` (`ConnectionRequestEntry.cs:1131`). See Data Model below. It is only touched when the field is offered, so leaving the setting blank never clears an existing value.

- **New-person defaults are settings, not hardcoded.** Connection Status (Prospect), Record Status (Pending), and Record Source (Serving Connection) are block settings matching `ConnectionOpportunitySignup`. They cannot be derived from existing configuration (Rock has no global Person connection/record-status default, and a connection type carries no person-level default). Record Source additionally honors a session `RecordSource` before falling back to the setting (`ConnectionRequestEntry.cs:994`).

- **The record fan-out is transactional.** All requests for a submission are added, saved, and have their attribute values written inside one `WrapTransaction` (`ConnectionRequestEntry.cs:1412`), so a partial submission does not leave orphaned requests.

## Common Scenarios

- **Adding a new configurable field.** Add the `[...Field]` attribute with a `Show/Hide/Required` `CustomDropdownListField` (or a boolean for non-gated fields), expose its visibility on the box in `SetFieldVisibility`, render it in `connectionRequestEntry.obs` bound to `fieldRules(...)`, and gate its save with `IsFieldShown(...)`. Miss the last step and the field is writable even when hidden.

- **Offering the form only to certain campuses.** The Pick a Campus section renders only when more than one active campus exists; the default is context campus, then the person's primary campus, then the first active campus (`ConnectionRequestEntry.cs:548`).

- **Redirecting after submit.** Set `Optional Redirect URL`. Blank shows the built-in success state instead. The URL is returned in the result bag and the Vue side runs it through `makeUrlRedirectSafe`.

## Key Architectural Decisions

- **Modeled on `ConnectionOpportunitySignup`.** The identity match-or-create, attribute handling, and new-person defaults deliberately mirror the existing single-opportunity block so the two behave consistently and the defaults line up. The main departure is multi-select opportunities and full field configurability.

- **Distrust the client, re-derive on the server.** Documented inline as an engineering note (`ConnectionRequestEntry.cs:1335`): the submitted opportunity list and attribute values are validated against the block's configuration before anything is written. The `IsFieldShown` gating on every save path is the same decision applied to person/spouse fields.

- **Preferred Service Time as a first-class Person FK.** Rather than a person attribute or transient workflow context, the visitor's chosen service time is stored as `Person.PreferredServiceTimeScheduleId`, a real nullable FK to `Schedule`. This was a PO decision so the value is queryable and reportable. It uses `ON DELETE SET NULL` so deleting a schedule clears the reference without deleting people.

- **Single EF migration, no plugin hotfix.** This is a net-new v20 feature on `develop`, so the schema change (the Person FK) and the block/page seeding live together in one EF migration. Because Obsidian entity types register at app startup (after migrations run), the migration calls `RockMigrationHelper.UpdateEntityType(...)` to insert the block's entity type before `AddOrUpdateEntityBlockType(...)`, the standard core pattern for seeding an Obsidian block from an EF migration.

- **Conditional, structural page seeding.** The Get Connected page is only seeded when the stock pieces it slots into exist (the Connect parent page, the Left Sidebar layout, and the Page Menu block type). If the external site has diverged from stock, seeding is skipped and the block type stays registered for manual placement, avoiding a page inconsistent with the church's site.

## Considered but Rejected

- **Storing Preferred Service Time as a person attribute or transient value.** Rejected in favor of the first-class FK because the PO wanted the value queryable and reportable. The Family Pre-Registration precedent (planned-visit time as transient workflow context) was considered and set aside for this reason.

- **Hardcoding the new-person defaults.** Considered dropping the three defined-value settings and baking in Prospect / Pending / Serving Connection. Rejected by the PO (2026-07-07): keep them configurable, matching the sibling blocks.

- **A plugin hotfix for the block/page seeding.** Considered during development. Rejected because this is unreleased v20 work; hotfixes are for patching released versions, and the entity-type timing issue is solved by `UpdateEntityType` in the EF migration.

## Technical Reference

### Data Model

The block adds one column to `Person`:

| Column | Type | Notes |
|---|---|---|
| `PreferredServiceTimeScheduleId` | `int` null | FK to `[Schedule]`, `ON DELETE SET NULL`. Mapped in `PersonConfiguration` with `WillCascadeOnDelete(false)`; the `SET NULL` behavior is applied via raw SQL in the migration, matching Rock's Campus-FK convention. Navigation property: `Person.PreferredServiceTimeSchedule` (`Rock/Model/CRM/Person/Person.cs`). |

Everything else the block writes uses existing tables: `Person`, `PersonAlias`, `Group`/`GroupMember`/`GroupLocation` (family and address, via `PersonService.SaveNewPerson`), `PhoneNumber`, and `ConnectionRequest` plus its attribute values.

### Block Actions and Server Surface

- `GetObsidianBlockInitialization()` (`ConnectionRequestEntry.cs:489`) builds the initialization box: banner/section copy, per-field visibility, campus list, dropdown options, available opportunities with their public request attributes, person attributes, captcha state, and prefilled values for a logged-in visitor.
- `Save(ConnectionRequestEntryRequestBag)` (`ConnectionRequestEntry.cs:1070`) is the one write action. Order: captcha check, opportunity/name/email validation, match-or-create Person, save phone/photo/preferred-service-time, `SaveChanges`, then address, person attributes, spouse, and the connection-request fan-out.

### Person Match-or-Create

1. Logged-in visitor: use `GetCurrentPerson()` directly (`ConnectionRequestEntry.cs:1108`).
2. Otherwise `PersonService.FindPerson(new PersonService.PersonMatchQuery(firstName, lastName, email, mobilePhone), updatePrimaryEmail: false)`.
3. On no match, `CreateNewPerson` builds the Person (record type Person, connection/record status and record source from settings, email active + `EmailAllowed`) and persists via `PersonService.SaveNewPerson(person, rockContext, campusId, false)`, which auto-creates the family and known-relationships groups (`ConnectionRequestEntry.cs:1164`).
4. Address saves as a Home `GroupLocation` on the family; person attribute values save from the Additional Information section.

Spouse handling (`ConnectionRequestEntry.cs:1248`): only when Marital Status is shown and Married, and both spouse name fields are shown and provided. A logged-in visitor's existing spouse is loaded via `Person.GetSpouse()` and updated in place rather than duplicated.

### ConnectionRequest Creation

`BuildConnectionRequest` (`ConnectionRequestEntry.cs:1456`) sets `PersonAliasId`, `ConnectionOpportunityId`, `ConnectionTypeId`, `ConnectionState.Active`, the connection type's default status (`ConnectionStatuses.First(s => s.IsDefault)`), campus, `GetDefaultConnectorPersonAliasId(campusId)`, and comments. Requests are added, saved, and have attribute values written inside `RockContext.WrapTransaction` (`ConnectionRequestEntry.cs:1412`). Standard `ConnectionRequest` save hooks fire the normal activity/workflow plumbing; this block adds no custom trigger.

### Field Visibility

`GetFieldVisibility` (`ConnectionRequestEntry.cs:938`) maps the `Show/Hide/Required` setting string to the `ConnectionRequestEntryFieldVisibility` enum (Optional / Required / Hidden). `IsFieldShown` (`ConnectionRequestEntry.cs:956`) is the shorthand used to gate every save path. On the Vue side, `fieldRules(...)` turns Required into the `required` validation rule; note both `PhoneNumberBoxWithSms` instances must bind `:rules` for the Required setting to actually enforce.

### Affected Blocks and UI Surfaces

- C#: `Rock.Blocks/Connection/ConnectionRequestEntry.cs`.
- Vue: `Rock.JavaScript.Obsidian.Blocks/src/Connection/connectionRequestEntry.obs` plus the opportunity card partial under `connectionRequestEntry/`.
- Seeded page: "Get Connected" (`/connect/get-connected`) under the external site.

### Captcha

When `Enable Captcha` is on (and not globally disabled), the captcha widget stands in for the Submit button until solved (invisible mode resolves on its own). Server-side, `Save` rejects the submission when `RequestContext.IsCaptchaValid` is false before any records are created (`ConnectionRequestEntry.cs:1082`).

### File Index

- Block: `Rock.Blocks/Connection/ConnectionRequestEntry.cs`
- Vue component: `Rock.JavaScript.Obsidian.Blocks/src/Connection/connectionRequestEntry.obs`
- Bags: `Rock.ViewModels/Blocks/Connection/ConnectionRequestEntry/`
- Field-visibility enum: `Rock.Enums/Blocks/Connection/ConnectionRequestEntry/ConnectionRequestEntryFieldVisibility.cs`
- Person FK: `Rock/Model/CRM/Person/Person.cs`
- Migration: `Rock.Migrations/Migrations/202607062123251_AddConnectionRequestEntryBlock.cs`

## Related Specs

- [Connection Request Entry](../../specs/completed/connection/260622-connection-request-entry.md) — 2026-06-22 (Joshua Henninger)
