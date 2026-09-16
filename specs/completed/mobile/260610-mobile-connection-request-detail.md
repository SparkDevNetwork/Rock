---
author: Panha Sim
date_created: 2026-06-10
summary: >-
  Backend (Rock RMS server) spec for the new mobile Connection Request Detail
  block, block 5 in the Connections revamp port. It is the screen opened when a
  request is tapped in the Connection Opportunity Detail list (block 3) or My
  Connection Requests (block 4). Covers only the server-side mobile RockBlockType:
  loading one request's detail (opportunity, status, state, connector, campus,
  placement group, comments, custom attributes, manual workflows, celebration,
  request source, contact info, and reminder/activity counts), the edit/save
  path plus targeted status/state/connector quick-actions, manual workflow
  launch, and celebration upsert, plus security, configuration, and the
  contract. The activity view (list/add/edit/delete) is carved out to a
  separate later spec. The logic is adapted from the web Connections
  Hub docked request-detail panel. A new BlockTypeGuid ships alongside the
  existing native ConnectionRequestDetail block, which is left intact. The mobile
  shell UI is specified separately in the RM repo.
contributors: []
---

# Connection Request Detail (Mobile) Backend

## Summary

This is the backend half of the new mobile **Connection Request Detail** block, block 5 in the Connections revamp port (after the [Connection Type List](260608-mobile-connection-type-list.md), the [Connection Type Detail](260608-mobile-connection-type-detail.md), the [Connection Opportunity Detail](260609-mobile-connection-opportunity-detail.md), and [My Connection Requests](260609-mobile-my-connection-requests.md)). It is the screen a connector lands on after tapping a request in the [Connection Opportunity Detail](260609-mobile-connection-opportunity-detail.md) request list or in [My Connection Requests](260609-mobile-my-connection-requests.md); both navigate with the `ConnectionRequest` IdKey page parameter. This spec covers only the server-side code in the Develop repo: a new mobile `RockBlockType` that loads one request's full detail, supports editing it, lists and mutates its activities, launches its manual workflows, and reads/writes its celebration; plus security, configuration delivery, and GUID registration. The mobile shell (header, sections, edit cover sheet, activity list, workflow buttons, navigation) is specified separately in [the mobile shell spec](../../RM/specs/260610-mobile-connection-request-detail-shell.md) in the RM repo. The two halves share one `BlockTypeGuid`.

**The mobile layout is now locked** (from the Quick UI breakdown, 2026-06-17; see the shell spec). This backend spec pins the contract, the server actions, the data each action returns or accepts, security, and the conventions; the per-field/cover-sheet UI decomposition lives in the shell spec. The server contract is independent of the shell's editor styling.

**Source of truth.** Unlike the first four blocks, there is **no standalone web `ConnectionRequestDetail` Obsidian block** in the revamp. The revamped request-detail UI lives in the web **Connections Hub** docked panel ([connectionRequestDockedPanel.partial.obs](../Rock.JavaScript.Obsidian.Blocks/src/Engagement/ConnectionsHub/connectionRequestDockedPanel.partial.obs)) served by [ConnectionsHub.cs](../Rock.Blocks/Engagement/ConnectionsHub.cs) (block `8674FB3A-9E0E-421C-821C-2DA862A20ED2`). That panel and its bags are the canonical logic this block adapts. The existing native mobile detail block (see below) is the reference for the mobile-shaped pieces (attribute DTOs, workflow-entry navigation, placement member attributes).

## Motivation

- Core revamped Connections on the web. After the per-type, per-opportunity, and request-list navigation (blocks 1 to 4), a connector opens an individual **request** to work it: read who it is, change its status/state, reassign the connector, set a placement group, edit comments and custom attributes, log activities, launch manual workflows, and record a celebration. The web does all of this in the Connections Hub docked panel; this block ports that to a full mobile screen.
- A **native** mobile `ConnectionRequestDetail` block already exists ([server](../Rock/Blocks/Types/Mobile/Connection/ConnectionRequestDetail.cs), `MOBILE_CONNECTION_CONNECTION_REQUEST_DETAIL`; [shell](../../RM/Rock.Mobile/Blocks/Connection/ConnectionRequestDetail.cs), `EF537CC9-...`). It predates the revamp: it has no celebration, keys off a `connectionRequestGuid` (Guid) rather than the `ConnectionRequest` IdKey the new list blocks pass, still exposes a legacy `HeaderTemplate` / `ActivityTemplate` Lava surface, and does not match the revamped status/state and activity-feed model. Per the initiative's locked convention (each block is new), this block ships a **new** `BlockTypeGuid` mirroring the revamped web panel and leaves the existing block intact for backward compatibility (decided with Panha 2026-06-10).
- Backward compatibility is a hard rule, so the new block ships alongside the existing one rather than replacing it.

## Scope

- In scope (v1 = the named feature set, editable):
  - **Load** one request's detail: requester (name + photo + metadata `PersonConnectionStatus`/`Gender` + phone/email for contact), connection **opportunity** (name + icon), connection **type**, **status**, **state** (with follow-up date when Future Follow Up), **connector**, **campus**, **request source**, **placement group** (assigned group + role + status + serving metric), **comments**, **due status**, **custom attributes**, the **manual workflows** available to launch, the **celebration** text, the **activity count**, and the requester's incomplete past-due **reminder count**.
  - **Edit / save** of the core request fields: status, state (+ follow-up date), connector, campus, **request source**, placement group (group + role + status + group-member attributes), comments, and the request's custom attribute values. Editing is per-field on the shell (per the mockup); the server contract is unchanged.
  - **Activities**: surface the **activity count** on the load, plus a full **activity feed** (`GetActivities`) and **add / edit / delete** activity actions (`AddActivity` / `UpdateActivity` / `DeleteActivity`). The feed merges logged activities, the requester's activities on other requests of the same type (when the type enables the full activity list), system updates parsed from history, sent communications, and viewable request notes, ordered newest first. The footer "Activity (N)" pill opens the in-block view.
  - **Workflows**: list the request's manual connection workflows and launch one (returning whether it needs an interactive entry form).
  - **Celebration**: read the celebration text and upsert/clear it (the `CELEBRATION_NOTE`), graduating blocks 3/4's display-only has-celebration flag to a viewable + editable note.
  - **Reminders** (added 2026-06-17): surface `ReminderCount` + `AreRemindersEnabled`; the Reminder card opens the linked Reminder block (a `ReminderPage` block setting) in a cover sheet, which owns the form (person + date + type + detail) and the save.
  - **Contact** (added 2026-06-17): expose the requester's phone + email so the shell can build native Call / SMS / Email links.
  - **Request source** (added 2026-06-17): load + edit the request's `ConnectionTypeSource`.
  - **Connection request note** (added 2026-06-17): write + save a `CONNECTION_REQUEST_NOTE` via a ported `SaveNote`, gated by `CanEditConnectionRequestNote` (the Note card).
  - **Transfer**: move a request to another opportunity of the same type (`GetTransferDetails` / `GetTransferConnectors` / `TransferRequest`), optionally updating status / campus / connector per the target opportunity's transfer configuration, clearing the placement group, and logging a `Transferred` activity.
  - **Update requester**: reassign the request to a different requester (`UpdateRequester`), setting the request's `PersonAliasId` to the chosen person (the request always has a requester, so it cannot be cleared).
  - **Placement group member attributes**: a lazy `GetPlacementGroupMemberAttributes` action returns the group-member attributes for a chosen group + role (seeded with the request's saved values) as the placement editor's selection changes.
  - Security, configuration delivery (campus list + linked pages), SystemGuid registration, and the contract returned.
- Out of scope (this spec):
  - All mobile UI, which lives in the shell spec.
  - **Partial web-panel parity** (scope decision 2026-06-10, REVISED 2026-06-17 from the mockup): still deferred are **person notes** on the requester, the requester's **other open requests** (`AdditionalRequests`), and the **AI summary**. The request **source** and **reminders** moved IN v1 on 2026-06-17 (see Resolved Questions). The contract is shaped so the remaining deferrals can be added later without reshaping it.
  - Later/sibling blocks: **Add Request** (the existing `AddConnectionRequest` block, `1380115A-...`, is left intact; a revamped add flow is a separate future block).
  - **In-app** communication flows (the web panel's SMS/email composer). v1 ships lightweight native `tel:` / `sms:` / `mailto:` links instead (moved in 2026-06-17); the richer in-app comms stay out.

## Requirements

### Functional (server) — load

- The block MUST resolve the target `ConnectionRequest` from the `ConnectionRequest` IdKey on the request bag (the shell passes the `ConnectionRequest` page parameter it was launched with by block 3 or block 4). Resolve via `ConnectionRequestService.Get( key, !PageCache.Layout.Site.DisablePredictableIds )` (the overload transparently accepts an Id, IdKey, or Guid, so a Guid from the old navigation path still resolves).
- The block MUST authorize **on the request entity**, matching the web `ConnectionsHub` exactly (decided 2026-06-16 for web parity; this is the complement of [Connection Opportunity Detail](260609-mobile-connection-opportunity-detail.md)'s deliberately type-level paging gate, which deferred per-request security to this block):
  - **Read** (load, activity list refresh): call `connectionRequest.IsAuthorized( Authorization.VIEW, currentPerson )` on the resolved entity. Rock's auth inheritance handles `ConnectionType.EnableRequestSecurity` transparently — when on, the request's own auth rules are consulted; when off, the request defers up to the opportunity and then the type. **No explicit branching on the flag is needed on read.** Mirrors `GetConnectionRequestDetails` ([ConnectionsHub.cs:5629](../Rock.Blocks/Engagement/ConnectionsHub.cs#L5629)).
  - **Write** (save, activity add/edit/delete, transfer, update requester, workflow launch, celebration upsert): port the web `CanEditSpecifiedConnectionRequest( ConnectionRequest, out BlockActionResult, bool? enableRequestSecurity = null )` from [ConnectionsHub.cs:2360](../Rock.Blocks/Engagement/ConnectionsHub.cs#L2360) as a private helper on the mobile block. In code the helper is named **`CanEditConnectionRequest( ConnectionRequest, out BlockActionResult )`** (renamed on the mobile block; the `enableRequestSecurity` override parameter is dropped, the flag is read from the request's type). The logic is:
    1. If `ConnectionType.EnableRequestSecurity == true`: `cr.IsAuthorized( EDIT, currentPerson )`; else `opportunity.IsAuthorized( EDIT, currentPerson )`.
    2. If step 1 fails, grant EDIT when **any** of: the current person IS the request's assigned connector (`cr.ConnectorPersonAlias?.PersonId == currentPerson.Id`); OR is an active, non-archived member of one of the opportunity's `ConnectionOpportunityConnectorGroups` AND the campus rule passes — `activeCampusCount == 1`, the connector group has no `CampusId`, the request has no `CampusId`, or `g.CampusId == cr.CampusId`.
    3. Else return `ActionForbidden` (matching the web; not `ActionUnauthorized`).
  - The `CanEdit` flag returned in the edit options bag MUST be the same `CanEditConnectionRequest` result so the shell hides edit affordances for users who would be refused on save.
  - A missing request, or an inactive opportunity/type, MUST return `ActionBadRequest`.
  - **Pagination note (does not apply here, called out for completeness):** porting the connector-fallback grant to this block has no impact on block 3's offset/limit paging because block 3 deliberately authorizes only at the type/opportunity level for its list query (it never calls per-request `IsAuthorized`). The web has the same split — `GetGridData` is coarse, `GetConnectionRequestDetails` is fine — and accepts the cosmetic wart that, when `EnableRequestSecurity = on`, the list may show a row the detail then refuses to open. Mobile faithfully reproduces this.
- The load action MUST return, for the resolved request:
  - Requester display name and photo URL (full public URL when a photo exists, else null; the shell supplies the avatar fallback, as in blocks 3/4).
  - Opportunity name + icon, connection type name.
  - Connection `state` and (when Future Follow Up) the follow-up date; current `status` (name + highlight color); `campus` name; `connector` display name; created date; due date; computed `DueStatus` (same buckets as blocks 3/4).
  - Comments as plain text (markdown column, so `ConvertMarkdownToHtml().StripHtml()`), consistent with blocks 3/4.
  - Placement group summary (assigned group name + icon + the member role/status), when one is assigned.
  - The request's **custom attributes** for view (`GetPublicAttributesForView` / `GetPublicAttributeValuesForView`), shaped as the mobile attribute DTO (see Attributes).
  - The **manual workflows** available for this request (the opportunity's and type's `ConnectionWorkflow`s with `TriggerType == Manual`, filtered by `ManualTriggerFilterConnectionStatusId` and `WorkflowType.IsActive` + `IsAuthorized(VIEW)`), as a launchable list.
  - The **celebration** text, if a non-empty `CELEBRATION_NOTE` exists.
  - The **activity count** (the display bag also carries `ActivityCount`; the full feed loads via the `GetActivities` action, see Activities).
- The block MUST also expose the **edit option lists** the shell needs to populate the edit UI, but via a separate lazy `GetEditOptions` action invoked when the edit sheet opens (resolved 2026-06-17, was OQ1), NOT on the load response: the available `ConnectionStatuses` for the type (name + color + order + note-required-on-completion + default), the selectable `ConnectionStates` (honoring whether Future Follow Up is enabled for the type), the connectors available for assignment, the placement groups available for the opportunity (with their roles and per-role statuses), the request sources for the type (its `ConnectionTypeSource` rows), and the activity types for the type (its active `ConnectionActivityType` rows, used by the Add / Edit Activity sheets). It also returns an `IsSequentialStatusMode` flag. These mirror the v1 subset of the web `ConnectionRequestDetailOptionsBag`. (The assigned group's manual requirements ride on the load display bag, not here; campuses are delivered via configuration, not this action.)

### Functional (server) — edit / save

v1 exposes BOTH a save and the web's targeted quick-actions (resolved 2026-06-17, Panha): the shell's per-field editors (campus / request source / placement / comments / attributes) commit via `SaveRequest`, while prominent inline affordances (the status chip, the state row, the connector row, and the dedicated Connect button) call the targeted actions. Every write path enforces EDIT via the ported `CanEditConnectionRequest` helper (see the auth bullet in the load section, which covers EnableRequestSecurity AND the connector-fallback grant).

- **`SaveRequest`** (per-field on select: the shell calls it on each field-picker selection or editor confirm, sending the full bag with the one changed value, so a single save is small and idempotent) updates the editable fields of one request: status (+ optional status-history note), state (+ follow-up date), connector, campus, request source, placement group (group + role + status + placement group-member attribute values), comments, and the request's custom attribute values. Mirrors the web `SaveConnectionRequest`: it MUST `LoadAttributes` before applying values, apply attribute values via `SetPublicAttributeValues( ..., enforceSecurity: true )`, and persist within a `rockContext.WrapTransaction(...)` that calls `SaveChanges()` then `SaveAttributeValues(rockContext)` (the established pattern from the existing block and `AddConnectionRequest`). NOTE: the web folds the note-required and completion guards into its quick-actions, NOT into its bulk `SaveConnectionRequest` ([ConnectionsHub.cs:5166](../Rock.Blocks/Engagement/ConnectionsHub.cs#L5166) does neither). Because mobile `SaveRequest` can also change status/state, it MUST replicate the status note-required guard itself (the status-rule bullet below) so a bulk save cannot bypass it. It does NOT run completion here: `SaveRequest` rejects a state of `Connected` with `ActionBadRequest( "A request cannot be connected here. Use the Connect action." )`, so completion (with its placement-group, requirement, group-member, and activity handling) always flows through `UpdateState`.
- **`ChangeStatus`** (targeted) ports the web `ChangeRequestStatus` ([ConnectionsHub.cs:5224](../Rock.Blocks/Engagement/ConnectionsHub.cs#L5224)): validate the new status belongs to the request's type, enforce note-required-on-completion (reject with `ActionBadRequest( "A note is required." )` when the current status `IsNoteRequiredOnCompletion` and no note is supplied), set the status + `ConnectionStatusHistoryNote`, save.
- **`UpdateState`** (targeted) ports the web `UpdateRequestStates` ([ConnectionsHub.cs:4813](../Rock.Blocks/Engagement/ConnectionsHub.cs#L4813)): require a follow-up date when the new state is Future Follow Up; when transitioning to `Connected`, run the completion handling below. This is also the action the shell's dedicated **Connect** button calls (state to `Connected` plus the manual requirements list). A separate `MarkConnected` endpoint is NOT added: the existing mobile block's `MarkRequestConnected` is the reference for the requirement-checkbox UI, but the server path is unified under `UpdateState` to match the web (whose completion IS a state change). `UpdateState` also rejects connecting an `Inactive` request with `ActionBadRequest( "An inactive request cannot be connected." )` (web parity with `ConnectionRequestService.CanConnect`).
- **`ReassignConnector`** (targeted) ports the web `ReassignConnector`: validate the chosen connector is one of the request's available connectors, set the connector person-alias, save. When the connector actually changes, it also records an `Assigned` `ConnectionRequestActivity` (existing-block parity); `SaveRequest` does the same when its connector field changes.
- **Status rules** (resolved 2026-06-17, web parity, was Open Question 3): the block enforces note-required-on-completion server-side (the `ActionBadRequest` above, matching the web at [:5245](../Rock.Blocks/Engagement/ConnectionsHub.cs#L5245) and [:4752](../Rock.Blocks/Engagement/ConnectionsHub.cs#L4752)) AND surfaces `IsNoteRequiredOnCompletion` on the status options so the shell can pre-validate the note before the call.
- **Completing a request** (state to `Connected`, resolved 2026-06-17 with full requirement override, was Open Question 4): port the web's completion handling. When the type `RequiresPlacementGroupToConnect` and the request has no assigned group, reject with `ActionBadRequest`. Then call a ported `TryMarkRequestConnected( request, manualRequirementsMet, currentPerson, rockContext, out error )` (adapted from the existing mobile block's `TryMarkRequestConnected`, NOT the web `ConnectionsHub`): a `GroupMember` is created for the assigned group when none exists, and its **manual** group requirements are enforced — one that `MustMeetRequirementToAddMember` and is not acknowledged blocks completion, while acknowledged ones are recorded as `GroupMemberRequirement` rows on the member. The acknowledgements come from the `GroupMemberRequirements` override list the shell sends (the manual-requirement checkboxes), mapped to a `Guid -> bool` dictionary (`Meets` / `MeetsWithWarning` count as met). This matches the existing mobile block (whose `_manualRequirements` checkbox dictionary at [ConnectionRequestDetail.cs:1395](../../RM/Rock.Mobile/Blocks/Connection/ConnectionRequestDetail.cs#L1395) is the shell reference).
- `RockDateTime` only; the campus filter and any Guid inputs are resolved to ids before LINQ `.Where()` where a cached id is available ([CLAUDE.md](../CLAUDE.md)).

### Functional (server) — activities (in-block view, shipped in v1)

- The activity **view** (feed + add/edit/delete) is an **in-block view** (a cover sheet within block 5, NOT a separate block; resolved 2026-06-17, "view not a block"), opened from the footer "Activity (N)" pill. It ships in v1.
- The display bag carries the **activity count** (`ActivityCount`, a single `ConnectionRequestActivityService` count for the request) for the pill label.
- **`GetActivities`** returns the full feed (VIEW auth), porting the web `GetActivityEntries`. Each entry is a `ConnectionRequestActivityBag` tagged with an `ActivityEntryType` (Activity, SystemUpdate, Communication, RequestNote). The feed merges: logged `ConnectionRequestActivity` entries on this request; the requester's activities on their other requests of the same type (only when the type's `EnableFullActivityList` is set); system updates parsed from `History` via raw SQL (mapped to a `SystemUpdateType`); sent email / SMS communications via raw SQL (with attachments); and viewable `CONNECTION_REQUEST_NOTE` request notes. Entries are ordered newest first. Per-entry `IsEditable` reflects `CanCurrentPersonEditActivity` (system activity types, which have a null `ConnectionTypeId`, are never editable).
- **`AddActivity`** ports the single-request slice of the web `AddActivityForRequests`: validate the activity type (active, and global or of the request's type), resolve the optional connector against the available connectors, add a `ConnectionRequestActivity`, and optionally create a person note per the type's `PersonNoteCreationBehavior` (`AlwaysCreateAPersonNote`, or `AskAtActivityCreation` + the `AddPersonNote` flag).
- **`UpdateActivity`** ports the web `UpdateActivity`: the activity must belong to the loaded request and pass `CanCurrentPersonEditActivity`; updates note + connector, and the activity type only when the current type is not a global/system type.
- **`DeleteActivity`** ports the web `DeleteActivity`: same ownership + `CanCurrentPersonEditActivity` gate, then delete.

### Functional (server) — reminders, contact, request source (added 2026-06-17)

- **Reminders**: the load MUST return `AreRemindersEnabled` (the type's `EnabledFeatureFlags.Reminder`, [ConnectionsHub.cs:2859](../Rock.Blocks/Engagement/ConnectionsHub.cs#L2859)) and `ReminderCount` (the current person's incomplete, past-due reminders tracked against the **requester's PersonAlias**, web parity: `!IsComplete && ReminderDate < RockDateTime.Now && PersonAlias.PersonId == currentPerson.Id && EntityId == request.PersonAliasId`). The Reminder card opens the configured **`ReminderPage`** (a `LinkedPage` to the page hosting the Reminder block) in a shell cover sheet (parity with the existing block's `AddReminderAsync`), passing the requester's PersonAlias (the load returns `RequesterPersonAliasGuid` for this), so **no server reminder action is added** (the Reminder block owns the form and the save). `AreRemindersConfigured` (ReminderPage set) rides on configuration.
- **Contact**: the load MUST return the requester's phone and email (null when absent) so the shell can build native `tel:` / `sms:` / `mailto:` links. No server comms action (native links only).
- **Request source**: the load MUST return the request's current `ConnectionTypeSource` (name) for display; `GetEditOptions` MUST return the type's `ConnectionTypeSource` rows; `SaveRequest` MUST accept and persist the chosen source. (`ConnectionTypeSource` is a per-`ConnectionType` admin-managed row, not a DefinedValue, as in block 6.)
- **Connection request note**: a new **`SaveNote`** action MUST port the web `SaveNote( ConnectionRequestNoteBag )` ([ConnectionsHub.cs:5736](../Rock.Blocks/Engagement/ConnectionsHub.cs#L5736)): resolve the request (require VIEW), then add (empty `NoteIdKey`) or load-and-edit a `Note` with `NoteTypeId` = `CONNECTION_REQUEST_NOTE` and `EntityId` = request.Id, authorize `EDIT` on the note, set `Text` = `NoteText`, save. The load MUST return `CanEditConnectionRequestNote` (the web's `tempNote.IsAuthorized( EDIT )` flag, [ConnectionsHub.cs:2873](../Rock.Blocks/Engagement/ConnectionsHub.cs#L2873)) so the shell gates the Note card. The saved note appears in the activity feed as a `RequestNote` entry.

### Functional (server) — workflows

- The block MUST list the request's **manual** connection workflows (see load) and expose a **launch** action that runs one against the request, mirroring the existing mobile block's `LaunchWorkflow` and the web `LaunchWorkflowForRequests`.
- The launch action MUST report whether the launched workflow has an **active entry form** for the current person. When it does, the shell navigates to a Workflow entry page (a `LinkedPage` block setting), passing the workflow type; when it does not, the workflow runs in the background and the shell shows a success/error message. The action returns the entry-form flag plus any error messages (the existing `ConnectionWorkflowLaunchedViewModel` shape).
- Launching requires `EDIT` (or at least `VIEW` of the workflow type per the web filter); follow the existing block's authorization.

### Functional (server) — celebration

- The block MUST return the request's celebration text (the non-empty `CELEBRATION_NOTE` text, `NoteType` GUID `A480E95A-F941-473E-B200-50D2658C9417`) on load, and MUST expose an **upsert** action that creates, updates, or clears that note for the request, mirroring the web `UpsertCelebrationText`. Clearing (empty text) SHOULD remove the note so the badge/section disappears, consistent with how blocks 3/4 derive the has-celebration flag from a non-empty note.
- Editing the celebration requires `EDIT` authorization. The type's `AreCelebrationsEnabled` flag SHOULD be surfaced so the shell can hide the celebration affordance when the feature is off (note: blocks 3/4 deliberately did **not** consult this flag for the read-only badge, but here, where celebration is editable, the edit affordance should respect it).

### Non-functional / conventions (server)

- A single new `BlockTypeGuid` MUST be embedded identically as a string literal in both repos (shared with the mobile block in the shell spec), with new `EntityTypeGuid` + `BlockTypeGuid` SystemGuid constants.
- Server block: `RockBlockType`, `[SupportedSiteTypes( Model.SiteType.Mobile )]`, under `Develop/Rock.Blocks/Mobile/Connection/` (namespace `Rock.Blocks.Mobile.Connection`; new blocks go in the `Rock.Blocks` project, following `Rock.Blocks/Mobile/CheckIn/CheckIn.cs`).
- The block declares `RequiredMobileVersion => new Version( 1, 20 )` (mobile shell v20). The feature ships in Rock core v20.
- Leave the existing server mobile block (`MOBILE_CONNECTION_CONNECTION_REQUEST_DETAIL`, shell GUID `EF537CC9-...`) and the existing `AddConnectionRequest` block (`1380115A-...`) intact.
- Follow [Develop/CLAUDE.md](../CLAUDE.md) and [data-model.md](../.claude/rules/data-model.md): `RockContext` lifetime, `RockDateTime`, no `System.Web` in shared code, batched note fetch (no per-row queries), avoid `Guid` in `.Where()` when a cached id is available. Attributes declared per [block-architecture.md](../.claude/rules/block-architecture.md) (vertical `FieldAttribute` by property; keys in a nested `AttributeKey` class; page-parameter keys in `PageParameterKey`).

## Design

### Server block identity and placement

| Piece | Path | Notes |
|---|---|---|
| New server block | `Develop/Rock.Blocks/Mobile/Connection/ConnectionRequestDetailV2.cs` | Class `ConnectionRequestDetailV2` (locked 2026-06-16), `[DisplayName("Connection Request Detail V2")]`, `EntityTypeGuid` `8B53B246-526F-4B3E-AF5B-4C36763E9DC9`, `BlockTypeGuid` `74DDC1A2-2025-4072-8F47-DF7A5A76CF83`. |
| Existing server block | `Develop/Rock/Blocks/Types/Mobile/Connection/ConnectionRequestDetail.cs` (`MOBILE_CONNECTION_CONNECTION_REQUEST_DETAIL`) | Untouched. Already native (its Lava `ActivityTemplate` is unused in shell V4); strong reference for the mobile-shaped pieces. |

The user-facing `DisplayName` is "Connection Request Detail V2" (it will coexist with the existing native block "Connection Request Detail" in the `Mobile > Connection` category, as with blocks 1 to 4 and their `V2` suffixed counterparts). The class name is `ConnectionRequestDetailV2` in both repos (locked 2026-06-16, per the locked V2 naming convention; see blocks 1's `ConnectionTypeListV2` and block 2's `ConnectionOpportunityListV2`). The server namespace is `Rock.Blocks.Mobile.Connection` (new project location, decided 2026-06-10), where no legacy class collides; the V2 suffix is required on the SHELL side where `Rock.Mobile.Blocks.Connection` still holds the old `ConnectionRequestDetail`, and the server matches the shell name for consistency. The same `BlockTypeGuid` literal is shared with the mobile block (see shell spec). This is separate from the web `ConnectionsHub` block whose logic we adapt.

### Block settings (attributes)

Declared per [block-architecture.md](../.claude/rules/block-architecture.md): each `FieldAttribute` assigned vertically by property, keys in a nested `AttributeKey` static class. v1 carries forward the linked pages the existing native block already uses (so navigation out of the screen works) plus the standard config:

| Setting | Field type | Key | Default | Purpose |
|---|---|---|---|---|
| Person Profile Page | `LinkedPage` | `PersonProfilePage` | none | Opened when the requester is tapped; the requester's `PersonGuid` is passed. |
| Group Detail Page | `LinkedPage` | `GroupDetailPage` | none | Opened when the placement group is tapped; the group's identifier is passed. |
| Workflow Page | `LinkedPage` | `WorkflowPage` | none | Opened when a launched manual workflow needs an interactive entry form; the workflow type is passed. |
| Reminder Page | `LinkedPage` | `ReminderPage` | none | Links to the page hosting the **Reminder block**; opened in a cover sheet by the Reminder quick-action, passing the requester's `PersonAlias` (`RequesterPersonAliasGuid`) + request context (added 2026-06-17, parity with the existing block's reminder page). |

There is no `HeaderTemplate` / `ActivityTemplate` Lava setting: the new block renders natively (the initiative's locked "no admin Lava row/section templates" rule); the parity `HeaderTemplate` setting is NOT kept (resolved 2026-06-17). The campus list is delivered via configuration (all active campuses from `CampusCache.All()`), not a setting. The `ReminderPage` setting links to the page hosting the Reminder block (the Reminder card opens it in a cover sheet; resolved 2026-06-17). Transfer ships in v1 but needs no block setting of its own (it runs entirely through the `GetTransferDetails` / `GetTransferConnectors` / `TransferRequest` actions), so the existing native block's transfer settings are not carried over.

### Data flow

```mermaid
sequenceDiagram
    participant M as Mobile block (shell spec)
    participant S as Server mobile block (this spec)
    participant DB as RockContext

    Note over M: OnLoadAsync + pull-to-refresh
    M->>S: BlockAction GetRequestDetail({ ConnectionRequestIdKey })
    Note right of S: resolve by IdKey, authorize (request VIEW), require active opp/type
    S->>DB: request + status/state/connector/campus/placement + attributes + workflows + celebration + activities
    DB-->>S: rows
    S-->>M: GetRequestDetailResponseBag { Request (+ ActivityCount, ReminderCount, contact), Celebration, CanEdit }

    Note over M: Open a field editor (loads options lazily)
    M->>S: BlockAction GetEditOptions({ ConnectionRequestIdKey })  (VIEW auth; returns CanEdit)
    S-->>M: statuses, states, connectors, placement groups (+ requirements), activity types, flags

    Note over M: Edit cover sheet -> Save (bulk)
    M->>S: BlockAction SaveRequest(saveBag)  (EDIT auth)
    S->>DB: update fields + attribute values (WrapTransaction)
    S-->>M: refreshed detail (or success + reload)

    Note over M: Inline quick-actions + Connect button
    M->>S: BlockAction ChangeStatus / UpdateState(+GroupMemberRequirements) / ReassignConnector
    S->>DB: targeted update (note-required + completion guards)
    S-->>M: refreshed slice

    Note over M: Launch workflow / Upsert celebration / Save note (reminder opens via cover sheet)
    M->>S: BlockAction LaunchWorkflow / UpsertCelebration / SaveNote
    S->>DB: mutate
    S-->>M: launch result (entry-form flag) / refreshed celebration / saved note

    Note over M: Activity view (footer pill) + Transfer / Update Requester (overflow)
    M->>S: BlockAction GetActivities / AddActivity / UpdateActivity / DeleteActivity / TransferRequest / UpdateRequester
    S->>DB: read feed / mutate
    S-->>M: activity feed / refreshed detail
```

Static configuration (campus list + linked pages) is delivered through `GetMobileConfigurationValues()`. Everything per-request flows through block actions. The detail is loaded once on entry (and on pull-to-refresh); each mutation returns the affected slice (or signals the shell to reload), the same shape the existing native block uses.

### What the load returns (adapted from the web panel)

The load assembles the same data the web `GetConnectionRequestDetails` returns into its `ConnectionRequestDetailBox` (`ConnectionRequestDetailsBag` + `ConnectionRequestDetailOptionsBag`), narrowed to the v1 scope. Adaptations from the web:

- **Comments** returned as plain text (markdown stripped), like blocks 3/4 (the web returns raw markdown for a rich editor).
- **Attributes** shaped as the mobile attribute DTO (`ClientEditableAttributeValueViewModel`, see Attributes), not the Obsidian `Dictionary<string, PublicAttributeBag>` + values shape.
- **Workflows** returned as a launchable list (name + the connection-workflow identifier needed by the launch action), mirroring the web `ActionItems` / `ConnectionWorkflowBag`.
- **Celebration** returned as the note text (a string), not a flag.
- Deferred web fields are not populated: `PersonNotes`, `AdditionalRequests`, and the AI summary. (Request source and `ReminderCount` ARE now populated, moved into v1 on 2026-06-17.)

The edit option lists (statuses, states, connectors, placement groups, request sources) mirror the v1 subset of `ConnectionRequestDetailOptionsBag`. RESOLVED 2026-06-17 (was OQ1): they are split into a lazy `GetEditOptions` action invoked when the edit sheet opens, NOT returned on the initial load. The load returns the lightweight display data + activity count + celebration + `CanEdit`; the heavier options fetch lazily, mirroring how block 2 lazy-loads its Details metrics. (The existing native block returns edit details eagerly via `GetRequestEditDetails`; v1 diverges to keep the initial load light.)

### Edit / save

Mirrors the web `SaveConnectionRequest( ValidPropertiesBox<ConnectionRequestBag> )`. The mobile save bag carries the editable fields (status guid + optional history note, state + follow-up date, connector person-alias, campus guid, request source, placement group guid + role guid + member status, comments, request attribute values, placement group-member attribute values). The server resolves Guids to ids, applies values, runs the attribute save inside `WrapTransaction`, and re-reads the request for the response. Status/state-specific rules (note-required-on-completion, complete-with-requirements) are handled per Requirements.

v1 ships BOTH `SaveRequest` AND the web's finer-grained mutations `ChangeStatus` / `UpdateState` / `ReassignConnector` (resolved 2026-06-17, Panha picked "also add quick-actions"). `SaveRequest` is the commit path for the shell's per-field editors (campus / request source / placement / comments / attributes); the quick-actions back the status chip, state row, connector row, and the Connect button. (The shell uses per-field editors, not one bulk form, per the mockup; the server contract is the same either way.) The contract carries a full save bag plus the targeted update bags. The completion path (state to `Connected`) flows through `UpdateState` with the `GroupMemberRequirements` override list, so the dedicated Connect button does not need its own endpoint. See the edit/save Requirements for the per-action guards each must enforce.

### Activities (in-block view, shipped in v1)

The activity view (feed + add/edit/delete) is an in-block cover sheet in block 5, opened from the footer "Activity (N)" pill. The display bag carries `ActivityCount` (a single `ConnectionRequestActivityService` count); the full feed loads on demand via `GetActivities`, which ports the web `GetActivityEntries` into a list of `ConnectionRequestActivityBag` entries (each tagged with an `ActivityEntryType`: Activity, SystemUpdate, Communication, RequestNote). The projection merges logged activities, the requester's activities on other requests of the same type (when `EnableFullActivityList` is set), system updates parsed from `History` via raw SQL (mapped to `SystemUpdateType`), sent email / SMS communications via raw SQL, and viewable `CONNECTION_REQUEST_NOTE` request notes, newest first. `AddActivity` / `UpdateActivity` / `DeleteActivity` port the single-request slices of the web `AddActivityForRequests` / `UpdateActivity` / `DeleteActivity`, including the web's `PersonNoteCreationBehavior` / `AddPersonNote` person-note toggle on add. Per-entry editability uses `CanCurrentPersonEditActivity` (system activity types, with a null `ConnectionTypeId`, are never editable). The existing block's `.ActivityDetailView.cs` is the reference for the mobile-shaped pieces.

### Workflows

Manual workflows are gathered exactly as the existing mobile block does (`opportunity.ConnectionWorkflows.Union( opportunity.ConnectionType.ConnectionWorkflows )` where `TriggerType == Manual`, active, status-filter match, `IsAuthorized(VIEW)`), deduped, ordered by name. The launch action runs the workflow and returns whether it has an active entry form (`HasActiveEntryForm`) plus errors. The shell navigates to the `WorkflowPage` linked page for entry forms, else shows a toast. Reuse the existing `LaunchWorkflow` shape rather than the web's bulk `LaunchWorkflowForRequests` (single request here).

### Celebration

On load, query the single `CELEBRATION_NOTE` for the request (`NoteTypeCache.Get( SystemGuid.NoteType.CELEBRATION_NOTE.AsGuid() ).Id`, `NoteService` where `NoteTypeId == ... && EntityId == request.Id`) and return its text when non-empty. The upsert action creates/updates that note (or deletes it when text is cleared), mirroring the web. This is the same note blocks 3/4 detect for their badge, so a celebration written here makes the badge appear in those lists.

### Attributes

Use the established **mobile** attribute pattern, not the Obsidian dictionary shape: return `ClientEditableAttributeValueViewModel` (in `Rock.Common.Mobile/ViewModel/`) for view and edit, via `GetPublicAttributesForView/Edit` + `GetPublicAttributeValuesForView/Edit`, and apply with `SetPublicAttributeValues` + `SaveAttributeValues`. The shell renders/edits each value with its FieldType editor (`FieldType.GetEditView` / `SetEditValue` / `GetEditValue`) or the `AttributeValueEditor` control. This matches the existing mobile `ConnectionRequestDetail` and `AddConnectionRequest` blocks.

### Placement group

Display the assigned group (name + icon + the member role/status). Editing a placement group mirrors the web `PlacementGroupDetailsBag`: select a group available for the opportunity, a role, a status, and (optionally) edit the group-member attribute values for that placement. Member **requirements** (`GroupMemberRequirementBag`) gate completion when the type `RequiresPlacementGroupToConnect`; v1 ships the full manual-requirement override (resolved 2026-06-17). The assigned group's requirements come down on the load display bag's `PlacementGroupRequirements` (populated when the current person can edit), and the manual acknowledgements come back up on the Connect (`UpdateState`) call. The existing mobile block's placement-member attribute handling and `TryMarkRequestConnected` (with manual requirements) are the reference.

### Contract returned

The bags and enums live in `Rock.Common.Mobile` (RM repo) and are defined in full in the [shell spec](../../RM/specs/260610-mobile-connection-request-detail-shell.md). The server references the built `Rock.Common.Mobile.dll` in `RockWeb/Bin`. Property names MUST match the mobile definitions exactly. At a high level the server populates:

- **`GetRequestDetailResponseBag`** (load) { the request display bag (which includes `ActivityCount`), the celebration text, and `CanEdit` }. Edit options load lazily (resolved 2026-06-17, was Open Question 1), so they do NOT ride on this bag. The full activity feed loads via `GetActivities`.
- **`GetEditOptionsResponseBag`** (lazy, fetched when the first field editor opens) mirroring the v1 subset of `ConnectionRequestDetailOptionsBag`: `CanEdit`, statuses (name + color + order + `IsNoteRequiredOnCompletion` + default), selectable states (honoring future-follow-up enabled), connectors, placement groups (with their roles and per-role statuses), request sources (the type's `ConnectionTypeSource` rows), activity types (the type's active `ConnectionActivityType` rows, as `ActivityTypeItemBag`), and the flags `AreCelebrationsEnabled` / `IsFutureFollowUpEnabled` / `RequiresPlacementGroupToConnect` / `IsSequentialStatusMode`. (Campuses are delivered via configuration, not this bag; the assigned group's manual `GroupMemberRequirement`s ride on the load display bag; reminder types are owned by the linked Reminder block, not here.)
- A **request display bag** mirroring the v1 subset of the web `ConnectionRequestDetailsBag` (requester name/photo + `RequesterPersonGuid` + `RequesterPersonAliasGuid` (the reminder target) + metadata `PersonConnectionStatus`/`Gender` + phone/email, opportunity name/icon, type name, state + follow-up, status name/color + `StatusGuid`, campus, connector, request source name, due status, dates, comments plain text, placement group summary + serving metric, attributes + `EditableAttributeKeys`, the assigned group's `PlacementGroupRequirements` (when editable), manual workflows, `ActivityCount`, `ReminderCount`, `AreRemindersEnabled`, `CanEditConnectionRequestNote`). It also carries the current campus/connector/source/status/placement Guids so the per-field `SaveRequest` can pass through unedited fields. (Celebration text rides on the response bag's `CelebrationText`, alongside the display bag.)
- A **save request bag** mirroring the v1 subset of the web `ConnectionRequestBag` (the bulk `SaveRequest` payload).
- **Quick-action bags** (resolved 2026-06-17): a status-change bag (request IdKey + status guid + optional note), a state-update bag (request IdKey + `ConnectionState` + optional follow-up date + the `GroupMemberRequirements` list used on completion), and a connector-reassign bag (request IdKey + connector person-alias), mirroring the web `ConnectionRequestUpdateBag` / `UpdateConnectionRequestStatesBag`.
- **Activity feed bags** for the in-block view: `GetActivitiesRequestBag` / `GetActivitiesResponseBag`, `ConnectionRequestActivityBag` (with the `ActivityEntryType` and `SystemUpdateType` enums), and the `AddActivityRequestBag` / `UpdateActivityRequestBag` / `DeleteActivityRequestBag` payloads for the add/edit/delete actions. The activity types the sheets pick from ride on `GetEditOptionsResponseBag` as `ActivityTypeItemBag`.
- **Transfer bags**: `GetTransferDetailsRequestBag` / `GetTransferDetailsResponseBag` (with `TransferOpportunityItemBag`), `GetTransferConnectorsRequestBag`, and `TransferRequestBag`.
- An **update-requester** bag (`UpdateRequesterRequestBag`, request IdKey + requester person Guid) for `UpdateRequester`.
- **Placement group member attributes** bags: `GetPlacementGroupMemberAttributesRequestBag` / `GetPlacementGroupMemberAttributesResponseBag`.
- A **launch-workflow result** (entry-form flag + errors) and an **upsert-celebration** request (IdKey + text).
- A **connection request note** bag (`ConnectionRequestIdKey` + optional `NoteIdKey` + `NoteText`) for `SaveNote`. (Reminders have no bag/action here: the Reminder card opens the linked Reminder block via the `ReminderPage` setting.)
- `Configuration` (from `GetMobileConfigurationValues`) { `Campuses` (active campuses from `CampusCache.All()` as `ListItemViewModel`), the linked-page Guids (PersonProfile / GroupDetail / Workflow / ReminderPage), and `AreRemindersConfigured` (true when `ReminderPage` is set) }.

`ConnectionState` and `DueStatus` already exist in `Rock.Common.Mobile/Enums/` and are reused (do not redefine). The full-requirement-override decision (2026-06-17) means placement requirements now cross the wire, so v1 adds the shared **`MeetsGroupRequirement`** enum (parity with `Rock.Model.MeetsGroupRequirement`) and a shared **`GroupMemberRequirementBag`** { `GroupRequirementGuid` (a `Guid`, not an IdKey), `GroupMemberRequirementState`, plus the display-side `Label`, `IsManual`, `MustMeetRequirementToAddMember` } for the manual-requirement checkboxes the Connect flow sends. The activity view adds the `ActivityEntryType` and `SystemUpdateType` enums (block-namespaced). The connection request note adds a `ConnectionRequestNoteBag`; reminders add no contract (the Reminder card opens the linked Reminder block via the `ReminderPage` setting). The shell spec is the source of truth for all new shared types.

## Open Questions (backend)

All backend open questions are resolved (2026-06-17). The mobile UI layout is now locked too (from the Quick UI breakdown, see the [shell spec](../../RM/specs/260610-mobile-connection-request-detail-shell.md)). The only item still open is in the shell spec: the footer overflow-menu contents. The activity view (feed + add/edit/delete) shipped in v1 as an in-block view. Nothing here blocks the rest of the block 5 server contract.

## Resolved Questions (backend)

- **Block class / namespace name** (resolved 2026-06-16): `ConnectionRequestDetailV2` in both repos, display name "Connection Request Detail V2". Old `ConnectionRequestDetail` (`EF537CC9-...`) left intact. Follows the locked V2 naming convention used by blocks 1's `ConnectionTypeListV2` and block 2's `ConnectionOpportunityListV2`.
- **Honor per-request security (`EnableRequestSecurity`)** (resolved 2026-06-16): YES, full web parity. Read calls `connectionRequest.IsAuthorized( VIEW )` directly on the request entity; Rock's auth inheritance handles the flag for free (request-level when on, opportunity-level inheritance when off). Write uses a ported helper (named `CanEditConnectionRequest` on the mobile block) with the same flag-aware branching AND the connector + connector-group fallback grant. See the Requirements section's auth bullet. This is the planned complement to block 3 ([Connection Opportunity Detail](260609-mobile-connection-opportunity-detail.md)) which deliberately authorizes only type/opportunity-level for its paging gate. No impact on block 3's offset/limit paging because the per-request check happens only on the single-request endpoints here.
- **Edit option delivery** (resolved 2026-06-17, was OQ1): **lazy**. Load returns the display bag (incl. `ActivityCount`) + celebration + `CanEdit`; a separate `GetEditOptions` action returns the heavy lists (statuses/states/connectors/placement groups/request sources + flags) when the editors open. Mirrors block 2's lazy Details metrics.
- **Edit granularity** (resolved 2026-06-17, was OQ2; refined same day by the mockup): **both** (Panha chose "also add quick-actions"). `SaveRequest` is the commit path for the shell's per-field editors (the mockup uses per-field editors, not one bulk form); targeted `ChangeStatus` / `UpdateState` / `ReassignConnector` back the status chip, state row, connector row, and the Connect button. The contract carries the full save bag plus the three update bags.
- **Status rule enforcement** (resolved 2026-06-17, was OQ3): web parity. The block enforces note-required-on-completion server-side (`ActionBadRequest`, matching the web at [:5245](../Rock.Blocks/Engagement/ConnectionsHub.cs#L5245) / [:4752](../Rock.Blocks/Engagement/ConnectionsHub.cs#L4752)) AND surfaces `IsNoteRequiredOnCompletion` as an options flag so the shell pre-validates the note before the call.
- **Complete-with-requirements depth** (resolved 2026-06-17, was OQ4): **full override** (Panha chose "full manual-requirement override"). A ported `TryMarkRequestConnected( request, manualRequirementsMet, currentPerson, rockContext, out error )` (adapted from the existing mobile block, not the web `ConnectionsHub`): the assigned group's **manual** requirements are enforced — one that `MustMeetRequirementToAddMember` and is not acknowledged blocks completion, acknowledged ones are recorded on the new member — from the `GroupMemberRequirements` list the shell's checkboxes send. Matches the existing mobile block. Adds shared `MeetsGroupRequirement` enum + `GroupMemberRequirementBag` to the contract.
- **Connect / completion shape** (resolved 2026-06-17): a dedicated **Connect** primary button in the shell (existing-mobile-block parity, [ConnectionRequestDetail.cs:1549](../../RM/Rock.Mobile/Blocks/Connection/ConnectionRequestDetail.cs#L1549)), backed server-side by `UpdateState( Connected, groupMemberRequirements )`. No separate `MarkConnected` endpoint is added; the web models completion as a state change, so the button reuses the state quick-action.
- **Activity feed scope** (resolved 2026-06-17, was OQ5; SUPERSEDED same day by the mockup): the activity **view** is an **in-block view** of block 5, opened from the footer "Activity (N)" pill (the Note card is a separate note-write path, not the activity view). It ships in v1: `GetActivities` returns the full feed (activities, cross-request activities when `EnableFullActivityList` is set, system updates from history, communications, and request notes), and `AddActivity` / `UpdateActivity` / `DeleteActivity` mutate it.
- **Transfer** (resolved 2026-06-17, was OQ6): **shipped in v1**. The block carries over a transfer flow (`GetTransferDetails` / `GetTransferConnectors` / `TransferRequest`) adapted from the web `ConnectionsHub`: move the request to another opportunity of its type, optionally updating status / campus / connector per the target's transfer configuration, clearing the placement group, and logging a `Transferred` activity.
- **`HeaderTemplate` parity** (resolved 2026-06-17, was OQ7): **fully native**, no Lava `HeaderTemplate` block setting, per the initiative's locked "no admin Lava row/section templates" rule.
- **Layout + scope additions from the mockup** (resolved 2026-06-17, Quick UI breakdown): the shell layout is locked (see shell spec). Three features moved INTO v1, superseding their earlier deferrals: **contact actions** (native `tel:`/`sms:`/`mailto:` links over the requester's phone/email), **reminders** (`ReminderCount` + `AreRemindersEnabled` on the load; final shape = a `ReminderPage` LinkedPage to the Reminder block, see next bullet), and **request source** (display + edit via the type's `ConnectionTypeSource`). Editing is per-field / save-on-select on the shell; the server contract adds the request-source field + the reminder/contact/count display fields (plus `SaveNote`; reminders link out to the Reminder block, see next bullet) and keeps the activity feed as an in-block view. Person notes, `AdditionalRequests`, and AI summary remain deferred. (The reminder delivery, the Note card, and the activity-view shape were refined the same day; see the next bullet.)
- **Reminder / Note / activity-view shapes** (resolved 2026-06-17, second mockup pass): the **Reminder** card opens the linked **Reminder block** via a `ReminderPage` block setting (the Reminder block owns the person/date/type/detail form and the save; block 5 has no reminder action; a briefly-considered native form was dropped). The **Note** card writes a **connection request note** (`CONNECTION_REQUEST_NOTE`) via a ported `SaveNote` (`ConnectionRequestNoteBag`), gated by `CanEditConnectionRequestNote`; it surfaces in the activity feed as a `RequestNote`. The **activity view** is an **in-block view** (not a separate block / LinkedPage), opened from the footer "Activity (N)" pill; its feed (`GetActivities`) + add/edit/delete (`AddActivity` / `UpdateActivity` / `DeleteActivity`) ship in v1. Detail-list editing is save-on-select (status/state/connector quick-actions; campus/source via `SaveRequest`; comments/attributes/placement use confirm-editors).

## Considered but Rejected (backend)

### Revamp the existing native block in place
Rejected (product decision 2026-06-10). Even though the existing `ConnectionRequestDetail` block is already native (not a Lava-hybrid list block), Panha chose to ship a **new** block with a new `BlockTypeGuid` and leave the existing one intact, consistent with the initiative's "each block is new" convention and preserving backward compatibility for any deployment using the existing block, its `HeaderTemplate`, and its `connectionRequestGuid` navigation contract.

### Full web-panel parity for v1
Rejected for v1 (scope decision 2026-06-10), PARTIALLY REVISED 2026-06-17 from the mockup. Request source and reminders (plus native-link contact actions) are now IN v1. Still out: person notes, the requester's other open requests (`AdditionalRequests`), and the AI summary. The contract is shaped so the rest can be added later without reshaping it.

### Extract a shared service for the detail/save logic
Rejected for now (product decision), matching the initiative. The web detail/save/activity/workflow logic is block-private to `ConnectionsHub` (and partly duplicated already in the existing mobile block). We adapt it into the new mobile server block rather than refactoring the shipping web block. A future cleanup could unify a `ConnectionRequestClientService`.

### Reuse the existing mobile block's contract verbatim
Rejected. The existing block predates the revamp (no celebration, Guid-keyed navigation, legacy Lava surface, pre-revamp status/state model). The new block mirrors the revamped web panel's model and uses the `ConnectionRequest` IdKey the new list blocks pass, so it gets its own contract. The existing block remains a strong implementation reference for the mobile-shaped pieces.

## Related

- Web source of truth (docked detail panel + actions/bags): [Develop/Rock.Blocks/Engagement/ConnectionsHub.cs](../Rock.Blocks/Engagement/ConnectionsHub.cs) (`GetConnectionRequestDetails`, `SaveConnectionRequest`, `ChangeRequestStatus`, `UpdateRequestStates`, `ReassignConnector`, `AddActivityForRequests`, `UpdateActivity`, `DeleteActivity`, `LaunchWorkflowForRequests`, `UpsertCelebrationText`); [connectionRequestDockedPanel.partial.obs](../Rock.JavaScript.Obsidian.Blocks/src/Engagement/ConnectionsHub/connectionRequestDockedPanel.partial.obs).
- Web detail bags: [Develop/Rock.ViewModels/Blocks/Engagement/ConnectionsHub/](../Rock.ViewModels/Blocks/Engagement/ConnectionsHub) (`ConnectionRequestDetailsBag`, `ConnectionRequestBag`, `ConnectionRequestDetailOptionsBag`, `ActivityEntryBag`, `ActivityBag`, `ConnectionWorkflowBag`, `ConnectionStatusBag`, `PlacementGroupDetailsBag`, `UpsertCelebrationBag`).
- Existing mobile server block (left intact; reference for mobile-shaped pieces): [Develop/Rock/Blocks/Types/Mobile/Connection/ConnectionRequestDetail.cs](../Rock/Blocks/Types/Mobile/Connection/ConnectionRequestDetail.cs).
- Services: `ConnectionRequestService`, `ConnectionRequestActivityService`, `ConnectionWorkflowService`, `NoteService` (celebration). Attribute helpers via `IHasAttributes` (`GetPublicAttributesForEdit` etc.) and `MobileHelper`.
- Enums (reused): `Rock.Enums/Connection/ConnectionState.cs`, `Rock.Enums/Connection/DueStatus.cs`.
- Sibling backend specs: [Connection Type List](260608-mobile-connection-type-list.md), [Connection Type Detail](260608-mobile-connection-type-detail.md), [Connection Opportunity Detail](260609-mobile-connection-opportunity-detail.md), [My Connection Requests](260609-mobile-my-connection-requests.md).
- Mobile shell spec: [../../RM/specs/260610-mobile-connection-request-detail-shell.md](../../RM/specs/260610-mobile-connection-request-detail-shell.md)
