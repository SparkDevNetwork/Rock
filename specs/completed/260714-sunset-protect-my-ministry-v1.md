---
author: Nick Airdo
date_created: 2026-07-14
summary: >-
  Retire the original Protect My Ministry (v1) background check component in
  Rock v20. Delete the PMM v1 code files, add a plugin data migration that
  removes the PMM EntityType, admin Page/Route/Block/BlockType, DVR Jurisdiction
  DefinedType, and component configuration attributes, and deactivate + rename
  the shared "Background Check" WorkflowType to "Background Check (PMM Legacy)"
  without losing historical Workflow rows. Keep historical PMM background-check
  documents (stored as a bare BinaryFile Guid) viewable via the profile UI by
  introducing an internal `PROTECT_MY_MINISTRY_PROVIDER_LEGACY` Guid constant
  and a small legacy branch in `GetBackgroundCheck.ashx` that redirects to
  `GetFile.ashx` after enforcing the currently-active provider's VIEW auth.
contributors:
  - Claude
---

# Sunset Protect My Ministry (v1) Background Check Component

## Summary

The original Protect My Ministry (PMM v1) background check component has shipped with Rock since v2 and was announced for sunset removal in v20 via [the community post](https://community.rockrms.com/connect/protect-my-ministry-v3-plugin). Going forward, Checkr is the stock (out-of-the-box) background check provider, and Rock supports additional third-party background check providers as plugins (including PMM v3). This spec removes the PMM v1 code from the Rock codebase, deletes the associated database artifacts via a new plugin data migration, and keeps historical PMM background-check documents viewable via the person profile UI. Where the removal would otherwise break neighbor code, we substitute a legacy Guid constant and a legacy branch in the `GetBackgroundCheck.ashx` handler so the sunset is safe for any Rock instance whether or not PMM was ever configured as the default provider. If PMM was still configured as the default at the moment the migration runs, an `[ExceptionLog]` row is written so the operator sees a breadcrumb of what was removed.

## Motivation

- PMM v1 has been superseded by Checkr (the stock background check provider) and by third-party background check plugins such as PMM v3; keeping PMM v1 in core carries maintenance and support surface for zero benefit.
- The `Rock/Security/BackgroundCheck/ProtectMyMinistry.cs` component and its `~/Blocks/Security/BackgroundCheck/ProtectMyMinistrySettings.ascx` admin block are legacy WebForms code that has not been converted to Obsidian and will not be.
- The v20 major-version bump is the appropriate window to complete the removal — the sunset was already announced to the community.

## Guiding Principle: Remove the Write Path, Preserve the Read Path

PMM v1 has been in production since Rock v2 (2014). Over the last ~12 years, active Rock instances have accumulated an unknown but non-trivial number of `[BinaryFile]` records (the actual background-check report PDFs) referenced by `[AttributeValue]` rows on the person "Background Check Document" attribute. Those historical AttributeValue rows are stored in PMM's original single-Guid format (just the `BinaryFile.Guid`, no provider prefix) that predates the current `{EntityTypeId},{RecordKey|BinaryFileGuid}` convention Checkr and third-party providers use.

The sunset therefore has two very different halves:

- **Write path — removed.** No new PMM v1 background check requests can be initiated. No new AttributeValue rows in the legacy Guid-only format will ever be created. This is why the component, the admin block, the callback webhook, the admin Page/Route/BlockType, the DVR Jurisdiction DefinedType, and the PMM-owned Attribute rows can all be deleted with confidence.
- **Read path — preserved.** Historical AttributeValue rows already in the database MUST continue to render as a working "View" button on the person profile, and clicking that button MUST continue to stream the historical PDF from `[BinaryFile]`. That is what a Rock admin (and the person they are looking at) will expect for records of past background checks. Removing that capability would be a silent data loss from a user-experience perspective, even though the raw bytes still exist in the database.

Every artifact we intentionally retain (the `PROTECT_MY_MINISTRY_PROVIDER_LEGACY` internal const, the legacy branch in `GetBackgroundCheck.ashx`, the still-present WorkflowType row renamed to `"Background Check (PMM Legacy)"`, the `[Obsolete]`-marked public const) exists to serve exactly one purpose: making the read path work for data that already exists. None of them make it possible to create new PMM data. The Technical Architect should evaluate each retention item against this principle — if it is only about reading historical data, it stays; if it enables new write activity, it goes.

## Requirements

- MUST NOT break Rock instances that never used PMM.
- MUST NOT lose the ability to read historical background-check documents on instances that used PMM in the past. Specifically: the person profile "Background Check Document" attribute's "View" button MUST continue to open the historical PDF for any `[AttributeValue]` row stored in PMM's legacy single-Guid format.
- MUST NOT preserve any code path that writes new PMM v1 data. The component, the admin block, the callback webhook, the admin page, and the PMM-owned Attribute rows are all removed. Nothing in the retained code re-creates any of them.
- MUST NOT delete the "Background Check" WorkflowType (Guid `16D12EF7-C546-4039-9036-B73D118EDC90`) — historical `Workflow` rows FK to it. Rename it to `"Background Check (PMM Legacy)"` and set `IsActive = 0` instead.
- MUST NOT touch the `[BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF]` "Background Check Types" DefinedType or the `PMMPackageName / DefaultCounty / SendHomeCounty / DefaultState / SendHomeState / MVRJurisdiction / SendHomeStateMVR` attributes attached to its DefinedValues — Checkr still reads and writes them ([Rock.Checkr/Checkr.cs:541](../Rock.Checkr/Checkr.cs:541), [Rock.Checkr/Checkr.cs:614](../Rock.Checkr/Checkr.cs:614)).
- MUST enforce an auth gate at least as strict as PMM's original `this.IsAuthorized(VIEW, currentPerson)` when serving a legacy PMM background-check document; delegate that check to the currently-active background check component if one is configured.
- MUST write to `[ExceptionLog]` when `core_DefaultBackgroundCheckProvider` still points at the PMM v1 type name at migration time, and clear the setting.
- SHOULD NOT expose the retained PMM Guid to the plugin surface — it is Rock-internal implementation detail once the component is gone.

## Artifact Inventory

### Owned by PMM (safe to remove)

| Kind | Guid / Path | Notes |
|---|---|---|
| EntityType | `C16856F4-3C6B-4AFB-A0B8-88A303508206` | `Rock.Security.BackgroundCheck.ProtectMyMinistry` component. |
| BlockType | `AF36FA7E-BD2A-42A3-AF30-2FEBC1C46663` | `~/Blocks/Security/BackgroundCheck/ProtectMyMinistrySettings.ascx`. |
| Block instance | `63AA839B-B6A1-4A57-A0DC-2F5B6DDA71BE` | "PMM Settings" block placed on the PMM admin page. |
| Page | `E7F4B733-60FF-4FA3-AB17-0832E123F6F2` | Protect My Ministry admin page. |
| PageRoute | `2BB14E39-6AEE-4379-8B92-ACB5EF3F700B` | `admin/system/protect-my-ministry`. |
| DefinedType | `2F8821E8-05B9-4CD5-9FA4-303662AAC85D` | PMM DVR Jurisdiction Codes + its DefinedValues. |
| Component-config Attributes | `UserName`, `Password`, `Active`, `Order`, `TestMode`, `RequestURL`, `ReturnURL` on `EntityTypeId = <PMM's Id>`, plus container-side componentized attributes qualified by the PMM EntityType Id. | Plus every matching `AttributeValue` row. |

### Shared with Other Background Check Providers (MUST NOT be deleted)

- **SystemSetting `core_DefaultBackgroundCheckProvider`.** This is a system-wide setting that names whichever background check provider is currently the default (Checkr, a third-party plugin, or historically PMM v1). It is not owned by PMM. The migration only *clears the value* if that value still equals the PMM v1 type name — the setting itself remains so that Checkr and third-party plugins continue to write it.
- **DefinedType `BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF` — "Background Check Types".** Shared by every background check provider.
- **Attributes on its DefinedValues:** `PMMPackageName`, `DefaultCounty`, `SendHomeCounty`, `DefaultState`, `SendHomeState`, `MVRJurisdiction`, `SendHomeStateMVR`. The names begin with `PMM` for historical reasons but Checkr reads and writes them ([Rock.Checkr/Checkr.cs:541](../Rock.Checkr/Checkr.cs:541), [Rock.Checkr/Checkr.cs:614](../Rock.Checkr/Checkr.cs:614), [Rock.Checkr/Checkr.cs:638](../Rock.Checkr/Checkr.cs:638)), and third-party providers reuse the same schema.

### Intentionally NOT deleted (with rationale)

Each item below is retained specifically to preserve either the read path for historical data or compile-time compatibility for external plugins that referenced these names in the past. None of them enable new PMM write activity.

- **WorkflowType `16D12EF7-C546-4039-9036-B73D118EDC90`** — the "Background Check" WorkflowType. **Why kept:** Historical `Workflow` rows FK to it via `Workflow.WorkflowTypeId`. On any Rock instance that has ever run a PMM background check, one or more Workflow rows point at this WorkflowType — deleting it would either fail on a FK constraint or, worse, orphan the audit history. Renaming to `"Background Check (PMM Legacy)"` and setting `IsActive = 0` makes it unmistakably clear to admins that the workflow type is retired without discarding history. Bonus: Checkr's provider-switch logic ([Rock.Blocks/Security/BackgroundCheck/CheckrSettings.cs:384](../Rock.Blocks/Security/BackgroundCheck/CheckrSettings.cs:384)) still looks it up by Guid to rename it when Checkr becomes the default; leaving the row present makes that codepath idempotent.

- **`Rock.SystemGuid.EntityType.PROTECT_MY_MINISTRY_PROVIDER`** (existing `public const string`). **Why kept:** Compile-time compatibility for external plugins. Any third-party plugin or block author who wrote `Rock.SystemGuid.EntityType.PROTECT_MY_MINISTRY_PROVIDER` into their code over the last decade will still compile against Rock v20. We mark it `[Obsolete]` + `[RockObsolete("20.0")]` so those authors get a compiler warning with a migration hint, but we do not break their build. This is a pure string constant — retaining it does not resurrect the component or enable any write activity.

- **`Rock.SystemGuid.EntityType.PROTECT_MY_MINISTRY_PROVIDER_LEGACY`** (new `internal const string`). **Why added:** This is the read-path Guid used inside the Rock assembly by `Rock/Field/Types/BackgroundCheckFieldType.cs` to (a) detect that a historical AttributeValue is a legacy PMM document (bare-Guid format) and (b) stamp `ProviderEntityTypeGuid` onto the returned public value so the "View" URL is not empty. Making it `internal` deliberately keeps it invisible to external plugins — external plugins have no business detecting the legacy format, and hiding it prevents anyone from mistaking it for a supported way to write new PMM-format data.

- **`Rock.SystemGuid.DefinedType.PROTECT_MY_MINISTRY_MVR_JURISDICTION_CODES`** (existing `public const string`). **Why kept:** Same compile-time-compatibility argument as `PROTECT_MY_MINISTRY_PROVIDER`. The DefinedType row itself is deleted by the migration, so the constant points at nothing — but any plugin that referenced the name still compiles. Removing the constant with no replacement would break plugin authors for no observable benefit. Cost of keeping it is one line.

- **`Rock.SystemGuid.WorkflowType.PROTECTMYMINISTRY`** (existing `public const string`). **Why kept:** [Rock.Blocks/Security/BackgroundCheck/CheckrSettings.cs](../Rock.Blocks/Security/BackgroundCheck/CheckrSettings.cs) uses this Guid at runtime when the admin promotes Checkr to be the default provider — the code finds the (still-present, now-renamed-and-inactive) PMM WorkflowType and re-renames it. This is a first-party use of the constant inside Rock's own assembly, not merely a plugin-compatibility concern. Deleting the const would break the CheckrSettings block.

- **Hard-coded PMM Guid in [RockWeb/App_Code/GetBackgroundCheck.ashx.cs](../RockWeb/App_Code/GetBackgroundCheck.ashx.cs)** — a `private static readonly Guid ProtectMyMinistryLegacyProviderGuid` local to the file. **Why hard-coded rather than referencing the SystemGuid constant:** The `App_Code` directory in a WebForms project compiles into a separate assembly (`App_Code.dll`) that has no compile-time reference to Rock's internal API. It therefore cannot see the new `internal const PROTECT_MY_MINISTRY_PROVIDER_LEGACY`. Making that const `public` just so App_Code can see it would leak the legacy Guid onto the plugin surface, which we specifically want to avoid. The single-line hard-coded copy at the top of the handler is the tightest scope we can give the read-path detection.

- **Legacy branch in [GetBackgroundCheck.ashx.cs](../RockWeb/App_Code/GetBackgroundCheck.ashx.cs) `ProcessRequest`** — the code that redirects to `~/GetFile.ashx?guid={binaryFileGuid}` when it sees the legacy PMM `EntityTypeGuid`. **Why kept:** This is the actual mechanism that makes the "View" button on a historical PMM document work. Before this change the ashx would `Type.GetType(EntityTypeCache.Get(guid).AssemblyName)` to reflect over the PMM component and call its `GetReportUrl(recordKey)`. The component is gone, the EntityType row is gone, so that reflection path throws. The legacy branch replaces "reflect over the removed component" with "we already know what the removed component's `GetReportUrl` did — inline that behavior." The auth gate mirrors PMM's original `IsAuthorized(VIEW, currentPerson)` check by delegating to the currently-active background check component's VIEW auth (see the [Post-Migration User-Visible Behavior](#post-migration-user-visible-behavior) section for the full matrix). If we removed this branch, every historical "View" click would hit "That Wasn't Supposed To Happen…" — a visible regression for admins.

- **`Rock.Client/CodeGenerated/SystemGuid/RockSystemGuids.cs` and the two Obsidian TypeScript SystemGuid files** — auto-generated from the C# source. **Why not touched by hand:** They will regenerate on the next codegen run. Hand-editing them would be lost the next time codegen runs.

- **Historic Checkr migrations 01, 02, 06, 09** — reference PMM only via string constants (`WorkflowType.PROTECTMYMINISTRY`, defined-value Guids). **Why not modified:** They only run on fresh installs or as historical bookkeeping. String constants are preserved so they continue to compile, and the SQL they run is idempotent (uses `IF EXISTS` / `WHERE Guid = …` checks).

## Code — Files to DELETE

| # | File | Why |
|---|---|---|
| 1 | `Rock/Security/BackgroundCheck/ProtectMyMinistry.cs` | The PMM v1 component itself. |
| 2 | `RockWeb/Blocks/Security/BackgroundCheck/ProtectMyMinistrySettings.ascx` | Admin block markup — the containing admin page is being deleted. |
| 3 | `RockWeb/Blocks/Security/BackgroundCheck/ProtectMyMinistrySettings.ascx.cs` | Admin block code-behind. |
| 4 | `RockWeb/Webhooks/ProtectMyMinistry.ashx` | PMM callback webhook — no longer needed once the component is gone. |

## Code — Files to MODIFY

| # | File | Change |
|---|---|---|
| 5 | `Rock/SystemGuid/EntityType.cs` | Mark the existing `PROTECT_MY_MINISTRY_PROVIDER` const `[Obsolete(...)]` + `[RockObsolete("20.0")]`. Add a new `internal const string PROTECT_MY_MINISTRY_PROVIDER_LEGACY` with the same Guid — Rock-assembly only, hidden from plugins. **Why two constants:** the obsolete one keeps existing external plugin code compiling (any plugin that referenced the name over the last decade still builds against Rock v20, with a compiler warning pointing them at the removal); the new `internal` one gives Rock-internal read-path code a name that is honest about what it points at (a removed component). Keeping only the `[Obsolete]` const would either force our own read-path code to accept a warning on every use or force us to `#pragma warning disable` at every call site. |
| 6 | `Rock/Field/Types/BackgroundCheckFieldType.cs` | Update all four `PROTECT_MY_MINISTRY_PROVIDER` references to `PROTECT_MY_MINISTRY_PROVIDER_LEGACY`. In `ParseForPublicValue`'s single-Guid branch, always set `IsLegacyProtectMyMinistry = true` and `ProviderEntityTypeGuid = PROTECT_MY_MINISTRY_PROVIDER_LEGACY.AsGuid()`; only populate `ProviderEntityTypeId` / `ProviderName` from `EntityTypeCache` when the row still exists (fall back to `"Protect My Ministry (Legacy)"` otherwise). **Why this fieldtype must still recognize the legacy format:** historical `[AttributeValue]` rows on the person "Background Check Document" attribute contain a bare BinaryFile Guid with no comma-delimited provider prefix. The Obsidian client-side field renders its "View" link from `configurationValues[originalProviderEntityTypeGuid]`; if the fieldtype does not populate that Guid the resulting URL becomes `EntityTypeGuid=&RecordKey=…`, the handler cannot dispatch, and the "View" button breaks for every historical document. Reading the legacy format is a hard requirement of the read path. |
| 7 | `Rock/Web/UI/Controls/BackgroundCheckDocument.cs` | Remove `ProtectMyMinistry` from the private `BackgroundCheckTypes` enum and simplify `GetControlType()`. Drop the `else` fallback in the `BinaryFileId` setter that used to pre-select the PMM item in the ComponentPicker — the PMM item no longer exists post-sunset. |
| 8 | `RockWeb/App_Code/GetBackgroundCheck.ashx.cs` | Add a hard-coded `private static readonly Guid ProtectMyMinistryLegacyProviderGuid` (App_Code cannot see the `internal` const in Rock). New short-circuit at the top of `ProcessRequest`: if `EntityTypeGuid` matches the legacy PMM Guid, check `BackgroundCheckContainer.GetActiveComponent().IsAuthorized(VIEW, currentPerson)` and either 401 or 302 to `~/GetFile.ashx?guid={binaryFileGuid}`. **Why this branch is required and why the Guid is hard-coded:** the pre-existing ashx logic reflects over the component's assembly and invokes `GetReportUrl(recordKey)` on an instance. Both the component and the EntityType row are gone, so that reflection path throws for every legacy PMM document — a visible regression on any instance with historical data. The legacy branch inlines what PMM's original `GetReportUrl` did (redirect to `GetFile.ashx?guid=…` after an auth check). The Guid is hard-coded because `App_Code` compiles into a separate assembly that has no access to Rock's `internal` symbols; making the const `public` just to satisfy this file would leak the legacy Guid onto the plugin surface. |
| 9 | `Rock/Web/Utilities/RockUpdateHelper.cs` and `Rock.Update/Helpers/RockUpdateHelper.cs` | Delete the `PMMUserName` telemetry lookup — it always returned null after removal and its EntityType is gone. |
| 10 | `Rock.CodeGeneration/Pages/ModelGenerationPage.xaml.cs` | Remove the stale exclusion string `"Rock.Security.BackgroundCheck.ProtectMyMinistry._httpStatusCode"` from the singleton-field allow-list. |
| 11 | `Rock.Checkr/Migrations/08_CheckrSetDefaultBackgroundCheckProvider.cs` | Drop the `else` that previously fell back to setting PMM as the default when `[BackgroundCheck]` had rows. Only the fresh-install branch (Checkr as default) remains. Removes the last `typeof(Rock.Security.BackgroundCheck.ProtectMyMinistry)` reference in the codebase and lets Migration 08 continue to compile without PMM. |

## Code — Files intentionally NOT touched

- `Rock/SystemGuid/DefinedType.cs` — `PROTECT_MY_MINISTRY_MVR_JURISDICTION_CODES` constant is kept for readability of historical references; never harmful, no plugin surface impact.
- `Rock/SystemGuid/WorkflowType.cs` — `PROTECTMYMINISTRY` constant remains because [Rock.Blocks/Security/BackgroundCheck/CheckrSettings.cs](../Rock.Blocks/Security/BackgroundCheck/CheckrSettings.cs) still uses it when Checkr becomes the default provider.
- Historic Checkr migrations 01, 02, 06, 09 — reference PMM only via string constants that are preserved.
- `Rock.Client/CodeGenerated/SystemGuid/RockSystemGuids.cs` and the two Obsidian TypeScript SystemGuid files — auto-generated; will regenerate cleanly from the source.
- `[BC2FDF9A-…]` "Background Check Types" DefinedType and its attributes (`PMMPackageName`, `DefaultCounty`, etc.) — shared with Checkr, must remain.

## Data Migration — New File

Add `Rock/Plugin/HotFixes/###_SunsetProtectMyMinistry.cs` with `[MigrationNumber(###, "20.0")]`. The literal `###` is a placeholder — the concrete number will be assigned at commit time as the next available HotFix number.

Steps performed in `Up()`, each in its own private method for clarity:

1. **`LogExceptionIfPmmIsStillTheDefaultProvider`** — if the SystemSetting `core_DefaultBackgroundCheckProvider` currently equals `"Rock.Security.BackgroundCheck.ProtectMyMinistry"`, insert a row into `[ExceptionLog]` describing that PMM was removed while still being the configured default. Uses `SYSDATETIME()` and `NEWID()` for the required audit columns.
2. **`ClearDefaultBackgroundCheckProviderIfPmm`** — blank the SystemSetting's value only if it points at PMM. Other provider selections are left alone.
3. **`DeactivateAndRenamePmmWorkflowType`** — `UPDATE [WorkflowType] SET [Name] = 'Background Check (PMM Legacy)', [IsActive] = 0 WHERE [Guid] = '16D12EF7-…'`. Preserves historical Workflow rows.
4. **`DeletePmmComponentAttributesAndValues`** — deletes all `[AttributeValue]` + `[Attribute]` rows where `EntityTypeId = <PMM's EntityType Id>` (component config: `UserName`, `Password`, `Active`, `Order`, `TestMode`, `RequestURL`, `ReturnURL`). Also deletes container-side componentized attributes where `EntityTypeQualifierColumn = 'EntityTypeId'` and the qualifier value matches the PMM EntityType Id. String comment inside the `Sql( $@"…" )` verbatim block uses `""Active""` / `""Order""` (doubled quotes) so it compiles.
5. **`DeletePmmAdminPageAndBlocks`** — calls `RockMigrationHelper.DeleteBlock`, `DeleteBlockType`, `DeletePageRoute`, `DeletePage` in that order. Cleans up related `[Auth]` rows as a side effect of `DeleteBlock`.
6. **`DeletePmmMvrJurisdictionDefinedType`** — `RockMigrationHelper.DeleteDefinedType(2F8821E8-…)`. Cascades to its DefinedValues.
7. **`DeletePmmEntityType`** — `RockMigrationHelper.DeleteEntityType(C16856F4-…)`. Runs last, after all attribute cleanup so nothing FK-references it.

`Down()` is a no-op with a comment — plug-in migrations do not support down.

## Post-Migration User-Visible Behavior

- Admin Tools → System Settings → Background Check no longer lists a "Protect My Ministry" page. The `admin/system/protect-my-ministry` route 404s.
- Workflows named "Background Check" tied to the PMM WorkflowType are renamed to `"Background Check (PMM Legacy)"` and marked inactive.
- Person → Extended Attributes → Safety & Security → Background Check Document with a legacy PMM value: the "View" link still works. The click routes through the new legacy branch in `GetBackgroundCheck.ashx`, which:
  1. Delegates auth to the currently-active background check component (Checkr, PMM v3, etc.), returning **401** if the current person is not authorized on it.
  2. 302-redirects to `~/GetFile.ashx?guid={binaryFileGuid}`, which then applies its own BinaryFile / BinaryFileType auth as a safety net.
- If no background check provider is currently active on the instance, the redirect still happens and `GetFile.ashx`'s BinaryFile-level auth is the only gate. This is a deliberately weaker fallback for the edge case where the admin has removed all providers.
- If PMM was configured as the default at the moment the migration ran, a single `[ExceptionLog]` entry appears on the next startup explaining what happened and pointing the operator at the Background Check admin UI to select a supported provider.

## Rollout Considerations

- **Change ordering.** The ###-plugin-data-migration must land in the same release as the code deletions (items #1–#4) because Migration 08 (Checkr) no longer references the PMM type name after item #11, and items #6 and #8 rely on the legacy Guid path added in this change. Shipping the migration without the code changes (or vice versa) is unsupported.
- **Plugin compatibility.** External plugins that referenced `Rock.SystemGuid.EntityType.PROTECT_MY_MINISTRY_PROVIDER` continue to compile (the const is `[Obsolete]` but still present). External plugins that referenced `typeof(Rock.Security.BackgroundCheck.ProtectMyMinistry)` will fail to compile — this is expected and appropriate for a sunset release.
- **Historical data.** No `[AttributeValue]` rows storing background check document Guids are deleted. No `[BinaryFile]` rows containing report PDFs are deleted. No `[Workflow]` rows are deleted. Only PMM's own configuration and its admin surface are removed.

## Acceptance Criteria

1. Rock builds cleanly with all four PMM code files removed.
2. The ###-plugin-data-migration runs to completion on an instance that has never used PMM (no-op on the ExceptionLog; deletes are all `IF EXISTS`-safe or use `RockMigrationHelper` helpers that already null-guard).
3. The ###-plugin-data-migration runs to completion on an instance that WAS using PMM as the default provider, and:
   - Writes exactly one `[ExceptionLog]` row.
   - Clears `core_DefaultBackgroundCheckProvider`.
   - Renames + deactivates the WorkflowType.
   - Removes the EntityType, BlockType, Block, Page, PageRoute, DVR Jurisdiction DefinedType, and PMM's component-config attributes.
4. After migration, on an instance that had legacy PMM background-check documents:
   - The person's Safety & Security → Background Check Document card still shows the "View" button.
   - Clicking "View" downloads the historical file if the current person is authorized on the active background check provider.
   - Clicking "View" returns 401 (via the ashx) or 403 (via GetFile.ashx) if the current person is not authorized.
5. Switching the default background check provider via the Checkr Settings block continues to work end-to-end, including its rename of the legacy WorkflowType (idempotent — it will find the already-renamed row).
