---
author: Jason Hendee
date_created: 2026-09-10
summary: >-
  Convert the WebForms Group Member List block to Obsidian with feature parity,
  the Figma refresh (person column with connection status, Status column moved
  last, optional Gender column, column filters plus a filter modal), and a fixed
  query budget that removes the legacy block's per-member queries.
contributors: []
related_docs:
  - docs/group/group-members-and-roles.md
---

# Group Member List Obsidian Conversion

## Summary

Rebuild the WebForms Group Member List block ([GroupMemberList.ascx.cs](RockWeb/Blocks/Groups/GroupMemberList.ascx.cs)) as an Obsidian list block. The block lists the members of one group, resolved from a block setting, the `GroupId` page parameter, or a campus team via `CampusId`, with settings-driven columns, group member attribute columns, filters, requirement and sync indicators, delete or archive, Place Elsewhere, and grid communication. The conversion keeps every legacy capability that still has a home in the Obsidian grid, applies the Figma refresh where it is not stale, and replaces the legacy data access (more than eleven queries for an empty group, plus queries per member) with a fixed set of set-based queries.

## Motivation

- The block is the members panel on Group Viewer, Security Roles, and campus team pages, so it is one of the most visited WebForms blocks left in the Group domain ([Asana task](https://app.asana.com/1/20866866924293/project/1208321217019996/task/1208355616356788)).
- The task's Note 1: Ben W's 2021 stub at [GroupMemberList.cs](Rock.Blocks/Group/GroupMemberList.cs) (a paging block action with two filters and no component) is to be replaced, not extended.
- The task's Note 2: the legacy block issues more than eleven SQL queries for a group with zero members. Requirement statuses, registrations, history, and the inactive-selection checks each run per member or per page load, and `_group.Members` loads every member entity a second time. The conversion must fix this, not port it.
- Kyle Henning (2026-09-04, on the task): the Figma is a little dated; use judgement on UI deviations and ask when unsure. The Figma is therefore treated as directional.

## Requirements

### Part 1: Parity

**Group and access**

- MUST resolve the group in this order: the Group block setting; the `GroupId` page parameter (Id, IdKey, or Guid, honoring the site's predictable-ids setting); the `CampusId` page parameter (Id or IdKey) resolved through `Campus.TeamGroupId`. Render nothing when no group resolves or the current person lacks VIEW on it.
- MUST show the legacy "no roles" warning and hide the grid when the group type has no roles.
- MUST include memberships of deceased people (legacy `Queryable( true )`).

**Authorization**

`canEdit` means block EDIT, or group EDIT, or group MANAGE_MEMBERS ([GroupMemberList.ascx.cs:294](RockWeb/Blocks/Groups/GroupMemberList.ascx.cs:294)). Legacy enforced most rules only by not rendering a button; a WebForms postback needs that button. Obsidian block actions are reachable over REST with page VIEW and block VIEW alone ([BlockActionsController.cs:221](Rock.Rest/v2/BlockActionsController.cs:221)), so every mutating action below MUST re-check its rule on the server.

| Action | Legacy rule | Obsidian rule |
|---|---|---|
| Render the block and load grid data | Group VIEW for the current person; page and block VIEW come from the framework | Same. `GetGridData` returns no rows when the group does not resolve or fails VIEW. |
| Open a member (row click) | Any viewer; the detail page enforces its own rules | Same |
| Add | Button only when `canEdit` and at least one role is not managed by a group sync | Same. Add only navigates; the Obsidian Group Member Detail block owns the save check ([GroupMemberDetail.cs:378](Rock.Blocks/Group/GroupMemberDetail.cs:378)), with the same rule plus group SCHEDULE for sign-up groups. |
| Delete or Archive | Button only when `canEdit`. Sync-managed roles disabled in the UI only. Archive when the group type has history enabled and the member has snapshots, otherwise `CanDelete` then delete with registrants unlinked. No server re-check. | Button as legacy. The `Delete` action refuses without `canEdit`, refuses a member outside the resolved group, refuses a sync-managed role, then follows the same archive-or-delete path ([GroupList.DeleteMember](Rock.Blocks/Group/GroupList.cs:982) is the pattern). |
| Place Elsewhere | Button for every viewer when a trigger exists; Place checks only `CanDelete` | Button and action require `canEdit`. The action also refuses a member outside the resolved group and a trigger that is not one of the group's or group type's MemberPlacedElsewhere triggers. |
| Sync Now | Every viewer | Same; the `SyncGroup` action needs only the group gate above |
| Communicate | Every viewer; recipients come from the grid's own rows; the communication page enforces its own rules | Same; `IsAllowedToCreateCommunication` keeps its default, and `CreateGridCommunication` keeps only recipient keys that resolve to members of the group ([SignUpOpportunityAttendeeList.cs:1054](Rock.Blocks/Engagement/SignUp/SignUpOpportunityAttendeeList.cs:1054)) |
| Bulk Update, Person Merge, Merge Template, Launch Workflow | Grid actions for grids with a person id field, subject to each target page's security | Framework: `WithBlock` adds an action URL only when the person is authorized for its route |
| Attribute columns and filters | Per attribute VIEW, group-qualified and inherited alike | Same |
| Requirement labels, name-cell flags, Requirement Type filter | Only requirement types the person can VIEW | Same, applied once when statuses are computed |
| Profile, registration, and data view links | Every viewer; target pages enforce their own rules | Same |

One legacy defect is not carried: the Requirements tab re-assigned its Add button without `canEdit` ([GroupMemberList.ascx.cs:333](RockWeb/Blocks/Groups/GroupMemberList.ascx.cs:333)), so any viewer saw Add there. Add follows `canEdit` everywhere.

**Header**

- Title from the Block Title setting; when blank, "{GroupTerm} {GroupMemberTerm, pluralized}". Icon `ti ti-users`.
- When the group has group syncs, show a sync label in the panel header whose popover lists each "{Data View} as {Role}", linking the data view to the Data View Detail Page when that setting is set, with a Sync Now action that queues `GroupSyncService.SyncGroup` and confirms that the sync is scheduled.

**Columns**, in this default order: Name; Marital Status (`GroupType.ShowMaritalStatus`); Registration (only when a listed member has one; links to the Registration Page with `RegistrationId`, one link per registration); Role; Date Added (Show Date Added); First Attended and Last Attended (Show First/Last Attendance and `GroupType.TakesAttendance`); Note (Show Note Column); attribute columns; Requirements (see below); Status; then the action columns.

- Name cell: photo, name, and connection status beneath the name when `GroupType.ShowConnectionStatus` is on, plus these indicators after the name: the person's signal icon; a danger triangle ("does not meet requirements") or warning triangle ("has requirement warnings"); a warning circle with the legacy multiple-roles tooltip when `GroupType.IsSchedulingEnabled` and the person holds more than one active role; a note icon with the note as tooltip when Show Note Column is off and the member has a note; `ti ti-edit text-danger` when the group requires a signature document the person has not signed.
- Row classes: `is-inactive` (member status Inactive), `is-inactive-person` (record status Inactive), `is-deceased`.
- Attribute columns: GroupMember attributes flagged IsGridColumn that are qualified to the group (`GroupId`) or inherited through the group type chain (`GroupTypeId`), active, and VIEW-authorized. Column filters come from the framework.
- Export-only columns (hidden on screen): Nick Name, Last Name, Birth Date, Age, Email, Record Status, Gender, Is Deceased, Home Phone, Cell Phone, Home Address, Latitude, Longitude. The export file is named after the group.

**Filters**

- Column filters: Name (text), Role and Status (pick from existing), Gender when its column is shown, and attribute columns.
- Filter modal, applied server-side and remembered per person and per group: Family Campus (Show Campus Filter; matches people in any family at that campus), Gender, Registration (shown when the group has linked registration instances; limits to registrants of the chosen instance), Signed Document Yes or No (shown when the group has a required signature document template), Date Added range (shown when Show Date Added), Requirement Type and Requirement State (shown when the group or group type has requirements).

**Requirements**

- When the group or group type has requirements, show a Requirements column with one label per applicable requirement type the viewer can VIEW: success for Meets, danger for Not Met, warning for Meets With Warning, info otherwise. Statuses follow the rules of [GroupMember.GetGroupRequirementsStatuses](Rock/Model/Group/GroupMember/GroupMember.Logic.cs:476) (role match, age classification, applies-to data view, due-date fallback), computed for the whole list at once (Part 3).

**Row actions**

- Row click opens the Detail Page with `GroupMemberId`; Add opens it with `GroupMemberId=0` and `GroupId`. Both pass `CampusId` through when the page received one.
- Add is shown only when `canEdit` and at least one role is not managed by a group sync.
- A person profile button when Person Profile Page is set (`PersonId`).
- A Place Elsewhere button when `canEdit` and the group or group type has a MemberPlacedElsewhere trigger. The modal shows the trigger name (or a chooser ordered by workflow name when there are several), the member, the workflow type, and a Note box when the trigger qualifier asks for one, required when it says so. Place requires `canEdit`, accepts only the group's or group type's MemberPlacedElsewhere triggers, checks `CanDelete`, unlinks registrants, deletes the member, and enqueues `GroupMemberPlacedElsewhereTransaction`.
- Delete or Archive: disabled with a sync icon and the tooltip "Managed by group sync for role "{Role}"." for sync-managed roles; archive icon and label when the group type has history enabled and the member has history snapshots; confirmation "Are you sure you want to {delete|archive} this group member?", followed by the registration warning when the member came from a registration; the server `CanDelete` message is surfaced on refusal.

**Grid actions**

- Communicate keeps the inactive prompt: when the recipients (selected rows, or every row when none is selected) include inactive members, ask "The selection contains inactive records. Do you want to include the inactive records?" with Yes and No, No being primary and sending active members only. Each recipient carries a GroupMember entity merge field qualified by `GroupTypeId` and `GroupId`.
- The framework's export, merge template, bulk update, person merge, launch workflow, and entity set actions, with GroupMember as the entity type and the person as the recipient key. Item term is "{GroupTerm} {GroupMemberTerm}".
- Custom grid columns through `[CustomizedGrid]` (legacy `ICustomGridColumns`).

### Part 2: UI refresh (Figma, directional)

- Name renders as a `PersonColumn` (avatar, name, connection status detail).
- "Member Status" is renamed "Status" and moved to the last data column.
- New block setting **Display Gender Column** (default false) adds a Gender column with a pick-from-existing filter.
- Role and Status filter by picking from existing values; Name filters as text. The legacy First Name and Last Name filters are dropped.
- Grid actions move into the Obsidian action bar (search, filter modal, add, overflow).
- The Figma's "Hours Serving" column is an example of a member attribute defined on the parent group type; we won't add it explicitly to this block's column definitions.

### Part 3: Efficiency

- No query may run per member at any group size, including attribute loading.
- Every lookup runs once per page load and only when its feature applies (see the [query budget](#query-budget)). A group with zero members and default settings MUST load with at most three SQL queries from this block.
- Requirement statuses are computed by a new set-based helper in the model layer, not by calling the per-member method in a loop.
- The inactive-recipient check and the multiple-roles check use the rows already loaded; neither queries the database.

### Accepted differences from legacy

- First Name and Last Name server filters are replaced by the Name column filter and quick search.
- The Requirements tab is kept, but both tabs share one data load instead of binding their grids separately.
- `ISecondaryBlock.SetVisible` has no Obsidian equivalent. `PageState.setAreSecondaryBlocksShown` is a deprecated no-op since v18, so the block declares `BlockRole.Secondary` and nothing else.
- Phone and home address data for the export-only columns loads on every request, because exports run client-side. [RegistrationInstanceRegistrantList](Rock.Blocks/Event/RegistrationInstanceRegistrantList.cs:438) made the same call.
- First Attended and Last Attended are sortable (client-side); legacy disabled that for performance.
- Filter preferences are block person preferences keyed per group. Legacy `{GroupId}-` grid filter preferences are not migrated.

## Proposed Approach

### File layout

- `Rock.Blocks/Group/GroupMemberList.cs`: delete the stub and any related files; delete the existing stub `[Auth]` + `[BlockType]` + `[Block]`, `[EntityType]`, and block settings `[Attribute]` records. We'll start fresh, so we can perform a proper chop. `RestructureConnectionsPages.DeleteLegacyBlockTypeAndAllInstances()` has an example of how we've done this for legacy block types.
- We'll then re-add new `Rock.Blocks/Group/GroupMemberList.cs` and related files + new migration helper invocations as needed. It will ultimately look like editing in place; just spelling out the details here.
- `Rock.ViewModels/Blocks/Group/GroupMemberList/`: `GroupMemberListOptionsBag`, `GroupMemberListSyncBag`, `GroupMemberListPlaceElsewhereTriggerBag`, `GroupMemberListPlaceElsewhereRequestBag`.
- `Rock.JavaScript.Obsidian.Blocks/src/Group/groupMemberList.obs`, with partials under `src/Group/GroupMemberList/`: `gridSettingsModal.partial.obs`, `deleteOrArchiveCell.partial.obs` (pattern: [GroupList/deleteOrArchiveCell.partial.obs](Rock.JavaScript.Obsidian.Blocks/src/Group/GroupList/deleteOrArchiveCell.partial.obs)), `placeElsewhereModal.partial.obs`, `types.partial.ts`.
- A new set-based requirement status helper in the model layer next to [GroupService.GroupMembersNotMeetingRequirements](Rock/Model/Group/Group/GroupService.cs:882).

### Server shape

- `GetObsidianBlockInitialization` resolves the group once (group syncs with data view and role, requirements, member workflow triggers), builds the options bag (title, column flags, sync entries, Place Elsewhere triggers, filter lists, navigation URLs, `IsAddEnabled`, `IsDeleteEnabled`), and returns the grid definition.
- `GetListQueryable` starts from `GroupMemberService.Queryable( true )` filtered by `GroupId`, applies the modal filters as subqueries (family campus, gender, registration instance registrants, signed-document person set, date added range), and projects each member into a `GroupMemberRow`: the `GroupMember` entity, its role name and order, and a `PersonProjection` carrying only the person columns the grid uses (names and suffix, photo id, age, birth date, email, gender, record type and status, connection and marital status ids, age classification, top signal, deceased flag).
- `GetOrderedListQueryable` orders by role order, then last name, then first name.
- `GetListItems` materializes once, loads grid attribute values with `GridAttributeLoader.LoadFor( rows, r => r.GroupMember, GetGridAttributes(), rockContext )`, computes each person's IdKey, full name, reversed full name, and photo URL with the static `Person` helpers, then hydrates lookups keyed by member or person id, one query each: registrations, requirement statuses (bulk helper), members with history (when `EnableGroupHistory`), signed person ids (when a template is required), first and last attendance (when enabled), phone numbers and home addresses (the unexecuted person-id queryable as the `Contains` source, as in [SignUpOpportunityAttendeeList.cs:640](Rock.Blocks/Engagement/SignUp/SignUpOpportunityAttendeeList.cs:640)), and the multiple-roles person set from the rows themselves. Requirement Type and Requirement State filters are applied here, after statuses exist.
- `GetGridAttributes` returns GroupMember attributes that are IsGridColumn, active, VIEW-authorized, and either qualified to this group or inherited through the group type chain, deduplicated by key, matching the legacy `BindAttributes` and the Sign-Up attendee list's `EnsureGridAttributes`.
- `GetGridBuilder` emits: `idKey`, `person` (a `PersonFieldBag` built from the projection, with the connection status only when the group type shows it), `personIdKey`, `role`, `status`, `dateAdded`, `note`, `maritalStatus`, `registrations`, `firstAttended`, `lastAttended`, `gender`, `requirements` (name plus state per applicable type), the indicator flags (`hasRequirementNotMet`, `hasRequirementWarning`, `hasMultipleRoles`, `isUnsigned`, `signalIconCssClass`, `signalColor`, `isInactive`, `isPersonInactive`, `isDeceased`, `isSyncManaged`, `needsArchive`, `canDelete`, `hasRegistration`), the export-only fields, and attribute fields through `AddAttributeFieldsFrom( r => r.GroupMember, GetGridAttributes() )`.
- Block actions: `Delete( key )` (refuses without `canEdit`, for a member outside the resolved group, or for a sync-managed role; archives when history requires it; returns the `CanDelete` message on refusal), `PlaceElsewhere( bag )` (refuses without `canEdit`, for a member outside the resolved group, or for a trigger that is not the group's), and `SyncGroup()` (open to every viewer that passes the group gate). `CreateGridCommunication` is overridden to drop recipient keys that are not members of the group and to stamp the GroupMember entity merge field on each recipient, following [SignUpOpportunityAttendeeList.cs:1054](Rock.Blocks/Engagement/SignUp/SignUpOpportunityAttendeeList.cs:1054).

### Query budget

| Data | Legacy | Obsidian | Runs when |
|---|---|---|---|
| Group, type, roles, syncs, requirements, triggers | 1 plus lazy loads, plus `_group.Members` (every member entity) | 1 plus cache | always |
| Members with role and person columns | 1, every `Person` column | 1, the projection's columns only | always |
| Attribute values | per row | 1 | members exist and grid attributes exist |
| Requirement statuses | 1 or more per member | 2, plus 1 per data-view-scoped requirement | group or type has requirements |
| Registrations | 1 | 1 | members exist |
| Members with history | 1, always | 1 | `EnableGroupHistory` |
| Signed documents | 1 | 1 | required template set |
| First and last attendance | 1 | 1 | setting on and `TakesAttendance` |
| Phones and home addresses | 2, export only | 2 | members exist |
| Inactive-selection checks | 1 to 3 | 0 | never (client) |
| Multiple active roles | `_group.Members` | 0 | never (loaded rows) |

Zero members with default settings: the group and the members queries only.

### Client shape

- A `TabbedBar` in the grid's `#gridHeaderPrepend` slot selects the Members or Requirements tab, shown only when the group or group type has requirements. The selection rides the `tab` query string so a reload lands back on the same tab. Both tabs render from the one `GetGridData` result.
- One `Grid`: `personKeyField` on the person's IdKey, `entityTypeGuid` for GroupMember, `itemTerm` and `exportTitle` from the options, `rowClass` from the three row flags, `gridSettings` bound to the modal, `showCommunicate` off, and a `customActions` Communicate entry that runs the inactive prompt and then calls the grid's exposed `onCommunicate`, using `selectedKeysOverride` to drop inactive rows when the individual answers No.
- `PersonColumn` for Name with a format component that renders the framework person markup plus the indicator icons; `AttributeColumns`; `hideOnScreen` columns for the export-only fields; `ButtonColumn` for the profile and Place Elsewhere buttons; `DeleteColumn` with the delete-or-archive cell.
- Panel header slot carries the sync label and its `PopOver`.

### Settled decisions

1. **Start fresh, not in place.** The stub's block type, block, auth, entity type, and attribute records are removed by migration (the `DeleteLegacyBlockTypeAndAllInstances` pattern in [RestructureConnectionsPages.cs:83](Rock.Migrations/Migrations/Version 20.0/Version 20.0/202608251721086_RestructureConnectionsPages.cs:83)), and the new block declares new guids, so the chop swaps one block type for another with nothing left behind.
2. **Projected rows, not the entity list base.** `RockListBlockType<GroupMemberRow>` selects the person columns the grid uses instead of materializing every `Person` column per member, and `GridAttributeLoader.LoadFor` gives projected rows the same bulk attribute load the entity base provides. The two closest siblings, the Sign-Up attendee list and the Step Participant List, project the same way.
3. **Two tabs over one data load.** The legacy Members and Requirements tabs are kept, so the block mimics what people use today. The tab selects which grid renders; both read the same `GetListItems` result, so the second tab costs no extra query. The tab bar goes in the grid's own `#gridHeaderPrepend` slot and writes `?tab=` to the URL, following [semesterGrid.partial.obs:33](Rock.JavaScript.Obsidian.Blocks/src/Lms/LearningProgramSecondaryLists/semesterGrid.partial.obs:33). Note that every Obsidian tabbed grid today switches between different entity types; ours is the first to tab between two column sets over the same rows.
4. **Bulk requirement statuses live in the model layer and are the single source.** The rules stay in one place next to the per-member method, a test compares both outputs on the same group, and the name-cell flags and the Requirements column read the same statuses so they cannot disagree.
5. **Inactive prompt is client-only.** The rows already carry `isInactive`, so no server round trip decides whether to prompt.
6. **Preference keys are per group.** The block prefixes each preference key with the group guid, the way [RegistrationInstanceRegistrantList](Rock.Blocks/Event/RegistrationInstanceRegistrantList.cs:1443) scopes by template.

## Implementation Plan

Each row is a discrete implementation slice. **Implemented** means the code is written and in the working tree. **Tested** means every checkbox in every linked test section passes. Mark ✅ when done, 🔄️ when started but incomplete, and leave blank when not begun.

| # | Slice | Verified by | Implemented | Tested |
|---|---|---|---|---|
| 1 | Block scaffold: rewrite the stub, options bag, group resolution, title, "no roles" warning, core columns (Name, Role, Status, Date Added, Note), row and Add navigation | [T1](#t1-block-load-and-group-resolution), [T2](#t2-header-and-title), [T12](#t12-authorization) | ✅ | |
| 2 | Settings-driven and export-only columns: Marital Status, Registration, First and Last Attended, Gender, attribute columns | [T3](#t3-columns-and-settings) | | |
| 3 | Name indicators and row classes | [T4](#t4-name-indicators-and-row-styling) | | |
| 4 | Filters: column filters and the filter modal with per-group preferences | [T5](#t5-filters) | | |
| 5 | Requirements tab: tab bar, Requirements grid, bulk status helper, requirement filters | [T6](#t6-requirements) | | |
| 6 | Row actions: delete or archive, profile button, Place Elsewhere modal | [T7](#t7-row-actions), [T12](#t12-authorization) | | |
| 7 | Grid actions: Add visibility, Communicate with inactive prompt and merge field, standard actions | [T8](#t8-grid-actions-and-communication), [T12](#t12-authorization) | | |
| 8 | Group sync label, popover, Sync Now | [T9](#t9-group-sync), [T12](#t12-authorization) | | |
| 9 | Efficiency pass against the query budget | [T10](#t10-query-budget) | | |
| 10 | Chop: remove the WebForms files; block type swap migration authored separately | [T11](#t11-placements-and-chop-regression) | | |

## Test Plan

Per-slice checklists backing the **Tested** column. A row is Tested only when every item in its linked sections passes.

**Environment setup.** One group per feature is enough: a small group with an inactive member, a member whose person record is inactive, and a deceased member; a group type with requirements (one role-specific, one with a warning state) and history enabled, with one member edited so a history snapshot exists; a group sync (any data view to a role); a required signature document template on the group; an event registration instance linked to the group with a registrant placed in it; a scheduling-enabled type with one person in two active roles; a MemberPlacedElsewhere trigger on the group type with Show Note on; a campus whose Team Group is set (Campus Detail); a group type with no roles.

### T1. Block load and group resolution

- [ ] Group block setting wins over `GroupId`; with the setting blank, `GroupId` resolves as Id and as IdKey.
- [ ] No `GroupId` but `CampusId` (Id and IdKey) resolves the campus team group.
- [ ] No group, or a group the person cannot VIEW, renders nothing and logs no error.
- [ ] A group type with no roles shows the legacy warning text and no grid.
- [ ] Row click opens the Detail Page with `GroupMemberId`; Add opens it with `GroupMemberId=0` and `GroupId`; both carry `CampusId` when the page had one.
- [ ] Default sort is role order, then last name, then first name.
- [ ] A deceased member is listed.

### T2. Header and title

- [ ] Block Title shows as entered; blank falls back to "{GroupTerm} {GroupMemberTerm}s" using the group type's terms.
- [ ] Icon is `ti ti-users`; the block sits in the secondary zone as before.

### T3. Columns and settings

- [ ] Marital Status appears only when the group type shows marital status; Connection Status appears under the name only when the group type shows connection status.
- [ ] Registration column appears only when a listed member has a registration, one link per registration, each opening the Registration Page.
- [ ] Date Added, Note, First and Last Attended, and Gender each follow their setting; First and Last Attended stay hidden when the group type does not take attendance.
- [ ] Attribute columns: a group-qualified grid attribute and an inherited group-type grid attribute both appear; a non-grid attribute and one the person cannot VIEW do not.
- [ ] Export includes every export-only column with values (phones, home address, latitude and longitude), is named after the group, and excludes the action columns.
- [ ] Custom grid columns configured through block settings render.

### T4. Name indicators and row styling

- [ ] Requirement triangle: danger for Not Met, warning for Meets With Warning, none when all requirements are met, with the legacy tooltips.
- [ ] Multiple-roles warning appears only on a scheduling-enabled type and only for the person holding two active roles.
- [ ] Note icon with tooltip when Show Note Column is off; no icon when it is on.
- [ ] Unsigned-document icon appears only for people without a signed document when a template is required.
- [ ] Signal icon renders for a person with a top signal.
- [ ] Rows carry `is-inactive`, `is-inactive-person`, and `is-deceased` in the matching cases.

### T5. Filters

- [ ] Name text filter and quick search find by first, nick, and last name; Role and Status pick from existing values.
- [ ] Family Campus hides when Show Campus Filter is off; when on, it limits to people in a family at that campus.
- [ ] Registration filter appears only when the group has linked instances and limits to that instance's registrants.
- [ ] Signed Document Yes and No split the list correctly; the filter hides without a template.
- [ ] Date Added range filters inclusively on both ends; the filter hides when Show Date Added is off.
- [ ] Gender modal filter works with the Gender column off.
- [ ] Filter values persist across reloads for the same group and do not leak to another group on the same page.

### T6. Requirements

- [ ] The tab bar and the Requirements tab appear only when the group or group type has requirements; with none, the Members grid renders alone with no tab bar.
- [ ] Switching tabs issues no second `GetGridData` call; reloading on `?tab=Requirements` lands on that tab.
- [ ] Requirements column and its filters appear only when the group or group type has requirements.
- [ ] Labels: one per applicable type with the right color; a role-specific requirement shows only for that role; a type the person cannot VIEW is absent.
- [ ] Requirement Type and Requirement State filters narrow the list alone and combined.
- [ ] Bulk helper matches `GetGroupRequirementsStatuses` per member on the test group (states, warning dates, due-date fallback), including a data-view-scoped and an age-classification-scoped requirement.
- [ ] Name-cell flags and the Requirements column agree for every member.

### T7. Row actions

- [ ] Delete removes a member with no history; Archive (icon, label, and confirm text) is offered when the type has history and the member has a snapshot.
- [ ] A member added through a registration gets the second registration warning before removal.
- [ ] A sync-managed role shows the disabled sync button with its tooltip.
- [ ] A refused delete (`CanDelete` false) shows the server message and keeps the row.
- [ ] Profile button appears only when Person Profile Page is set and opens the profile.
- [ ] Place Elsewhere: button appears only when a trigger exists; a single trigger shows its name, several show the chooser; Note visibility and required-ness follow the qualifier; Place removes the member, unlinks registrants, and the workflow launches.
- [ ] Without `canEdit`, no Add, delete, or Place Elsewhere buttons appear, and rows still open the detail page.

### T8. Grid actions and communication

- [ ] Add hides when every role is sync-managed and shows when at least one is not.
- [ ] Communicate with no selection and an inactive member present prompts; No sends active members only, Yes sends everyone.
- [ ] Communicate with only active rows selected does not prompt.
- [ ] The created communication resolves `{{ GroupMember.GroupRole.Name }}` per recipient.
- [ ] Export, merge template, bulk update, person merge, and launch workflow operate on the selection.

### T9. Group sync

- [ ] Sync label appears only for a synced group; popover lists "{Data View} as {Role}" per sync, linking the data view when Data View Detail Page is set.
- [ ] Sync Now confirms scheduling and the sync runs (member added or removed to match the data view).

### T10. Query budget

Capture the page load with SQL Server Profiler (or Extended Events) filtered to the request.

- [ ] Zero members, default settings: at most three queries from this block.
- [ ] Fifty members with every feature on (requirements, history, signature, attendance, registration, attributes): query count is the budget table's total and is identical at five members.
- [ ] No query text contains a single member's id in a filter.

### T11. Placements and chop regression

- [ ] Group Viewer, Security Roles Detail, and Campus Detail team pages show the Obsidian block with their existing settings intact after the swap.
- [ ] Legacy `.ascx` and `.ascx.cs` are gone and no page references the legacy path.
- [ ] Block settings from the WebForms block survive by key (`ShowAttendance` in particular).

### T12. Authorization

Run each row as an administrator and again as a person with group VIEW only (no block EDIT, no group EDIT or MANAGE_MEMBERS). To call an action directly, copy the block action request from the Network tab as fetch, edit the body in the Console, and read the response status.

- [ ] View-only: the block renders with no Add, delete, or Place Elsewhere buttons; Sync Now is offered; rows still open the detail page.
- [ ] View-only: `Delete` and `PlaceElsewhere` called directly answer 400 and change nothing; `SyncGroup` called directly runs the sync.
- [ ] Editor: `Delete` called directly for a sync-managed member answers 400 and the member remains.
- [ ] Editor: `Delete` and `PlaceElsewhere` called directly with the key of a member of another group answer 400 and change nothing.
- [ ] Editor: `PlaceElsewhere` called directly with a trigger id from another group type answers 400.
- [ ] Communicate called directly with a recipient key that is not a member of the group creates the communication without that recipient.
- [ ] A person without group VIEW: the block renders nothing and `GetGridData` called directly returns no rows.
- [ ] Group MANAGE_MEMBERS alone enables Add and delete, the same as block EDIT and as group EDIT.
- [ ] An attribute and a requirement type the person cannot VIEW are absent from columns, filters, labels, and name-cell flags.

## Open Questions

None open.

## Considered but Rejected

### Fold the Requirements tab into the main grid
Rejected. One grid with a Requirements column plus Requirement Type and Requirement State filters would carry the same data in less UI, and the Obsidian grid's `visiblePriority` removes the column-density problem that justified the tab in WebForms. Dropped anyway: the two tabs are what people use today, and mimicking legacy costs one tab bar over a data load we already make.

### Use `RockEntityListBlockType<GroupMember>` with `Include( Person )`
Rejected. Materializes all 74 mapped `Person` properties for every member when the grid uses about a dozen, and its one advantage, automatic attribute loading, is available to projected rows through `GridAttributeLoader.LoadFor`. The closest sibling lists project.

### Reuse `GroupService.GroupMembersNotMeetingRequirements` for requirement statuses
Rejected. It returns only members with issues, so it cannot feed a column that also shows Meets; once a member has any issue it reports requirements scoped to other roles as Not Met; and it ignores applies-to data views and the due-date fallback. Adequate for the Sign-Up attendee list's warning flag, not for a column that must match the per-member method.

### Call `GetGroupRequirementsStatuses` per member
Rejected. It is the largest source of the per-member queries the task asks to remove.

## Related

- [Asana task 1208355616356788](https://app.asana.com/1/20866866924293/project/1208321217019996/task/1208355616356788) (requirements live in this spec) and the original [task 1202447508641462](https://app.asana.com/1/20866866924293/project/1204726868141988/task/1202447508641462).
- [Figma: Obsidian Group Member List Block](https://www.figma.com/design/wxzjmZwW0Zq2PFg3WRRobw/v17-Misc?node-id=3401-45263) (directional; dated per Kyle Henning's 2026-09-04 comment). The frame with the "additional filter settings" the design notes point to could not be retrieved through the Figma MCP, so the modal contents above come from the legacy filters. Local copy: [figma-obsidian-group-member-list.png](artifacts/260910-group-member-list-obsidian-conversion/figma-obsidian-group-member-list.png).
- Legacy block on Group Viewer today: [legacy-block-on-group-viewer.png](artifacts/260910-group-member-list-obsidian-conversion/legacy-block-on-group-viewer.png).
- Legacy source: [GroupMemberList.ascx](RockWeb/Blocks/Groups/GroupMemberList.ascx), [GroupMemberList.ascx.cs](RockWeb/Blocks/Groups/GroupMemberList.ascx.cs).
- Ben W's stub: commit `0171d36fb5` (2021-06-22), now [Rock.Blocks/Group/GroupMemberList.cs](Rock.Blocks/Group/GroupMemberList.cs).
- Reference blocks: [StepParticipantList](Rock.Blocks/Engagement/StepParticipantList.cs) (profile button, attribute columns), [RegistrationInstanceRegistrantList](Rock.Blocks/Event/RegistrationInstanceRegistrantList.cs) (per-scope preferences, TVP lookups, export-only columns), [SignUpOpportunityAttendeeList](Rock.Blocks/Engagement/SignUp/SignUpOpportunityAttendeeList.cs) (converted by Maxwell Eley on 2026-06-11; the closest sibling: projected row, `GridAttributeLoader`, one-query support lookups, entity merge field on communications), [GroupList](Rock.Blocks/Group/GroupList.cs) (delete or archive cell), [CommunicationList/gridSettingsModal.partial.obs](Rock.JavaScript.Obsidian.Blocks/src/Communication/CommunicationList/gridSettingsModal.partial.obs).
