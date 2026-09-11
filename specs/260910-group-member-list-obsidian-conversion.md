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
- MUST show the legacy "No roles!" warning above the grid when the group type has no roles. The grid stays and binds normally, listing any memberships that already exist, with no Add button, as legacy did.
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

- Name cell: photo, name, and connection status beneath the name when `GroupType.ShowConnectionStatus` is on, plus these indicators after the name: the person's signal icon, untooltipped: legacy's bare "signal" title says nothing, and naming the person's signals the way the Top Person Signal badge does ([TopPersonSignal.cs:60](Rock/Badge/Component/TopPersonSignal.cs:60)) would cost a query for `PersonSignal` and `SignalType`, so it waits until someone asks for it; a danger triangle ("does not meet requirements") or warning triangle ("has requirement warnings"); a warning circle with the legacy multiple-roles tooltip when `GroupType.IsSchedulingEnabled` and the person holds more than one active role; a note icon with the note as tooltip when Show Note Column is off and the member has a note; `ti ti-edit text-danger`, untooltipped, when the group requires a signature document the person has not signed. Rock has no existing wording for this icon, so it stays unexplained until someone writes one.
- Row classes: `is-inactive` (member status Inactive), `is-inactive-person` (record status Inactive), `is-deceased`.
- Attribute columns: GroupMember attributes flagged IsGridColumn that are qualified to the group (`GroupId`) or inherited through the group type chain (`GroupTypeId`), active, and VIEW-authorized. Column filters come from the framework.
- Export-only columns (hidden on screen): Name (as "Last, First"), Nick Name, Last Name, Birth Date, Age, Email, RecordStatusValueId, Record Status, Gender, Is Deceased, Home Phone, Cell Phone, Home Address, Latitude, Longitude. The export file is named after the group.
- **What legacy exported.** The block sets `ExcelExportSource.DataSource` ([GroupMemberList.ascx.cs:290](RockWeb/Blocks/Groups/GroupMemberList.ascx.cs:290)). In that mode every `BoundField` is exported regardless of its `Visible` flag, and a `RockLiteralField` is exported only when it is marked `AlwaysInclude` ([Grid.cs:2513](Rock/Web/UI/Controls/Grid/Grid.cs:2513)). So:
  - Always exported, display setting or not: Marital Status, Connection Status, Date Added, Note, and every export-only person field. A column whose setting is off therefore hides on screen rather than disappearing.
  - Never exported: the on-screen name (`NeverInclude`), Registration (`NeverInclude`), and First Attended / Last Attended (no behavior set, so `IncludeIfVisible`, which `DataSource` mode skips). Those three carry `excludeFromExport`.
  - The export's Name column is `Person.FullNameReversed` ("Last, First"), never the displayed name.
  - A boolean field needs `BooleanColumn`, not `Column`. A plain `Column` falls back to rendering its cell and scraping the text, and a Vue render function returning a raw boolean produces no output, so the cell exports blank. `BooleanColumn` supplies its own `exportValue`. Strings and numbers are unaffected.
  - The action columns are not exported either: `PersonProfileLinkField` is a `HyperLinkField` and both the delete and Place Elsewhere columns are `RockTemplateField`s left at the default `IncludeIfVisible`, so `DataSource` mode skips all three. They carry `excludeFromExport` in slice 6.
  - Attribute columns are `AttributeField : RockBoundField`, so they always export. Legacy appends them to the end of `Columns` ([GroupMemberList.ascx.cs:1542](RockWeb/Blocks/Groups/GroupMemberList.ascx.cs:1542)), which puts them last in the export, after Longitude, while on screen they still read directly after Note because everything between is hidden. Obsidian derives both orders from one declaration, so slice 2c had to pick. Decided: `<AttributeColumns />` sits after Note and before Status, matching the on-screen order this section specifies. The cost is that the attribute columns land earlier in the export than legacy put them, right after Note instead of after Longitude.
- Column order follows legacy, with the single Figma reorder that renames Member Status to Status and moves it last. Gender is new, so legacy pins only its export position (between Record Status and Is Deceased); the on-screen column sits with the other demographic columns after Name.

**Filters**

- Column filters: Name (text), Role and Status (pick from existing), Gender when its column is shown, and attribute columns.
- Filter modal, remembered per person. Every filter in it narrows the query, because none of them can be answered from a row, and every one of them is conditional: Gender (shown when Display Gender Column is off, since with the column on its own filter does the job), Family Campus (Show Campus Filter, which keeps its legacy default of **true** rather than the go-forward "Obsidian booleans default false" rule; matches people in any family at that campus), Registration (shown when the group is linked to at least one registration instance; keeps members whose **person** registered for the chosen instance, which is what legacy matched, so it carries help text distinguishing it from the Registration column's own filter), and Signed Document Yes or No (shown when the group has a required signature document template). When none of the four applies, the settings button is not offered at all.
- The filters a row can answer are column filters instead: Registration on the Registration column (matches only the registration a member was added through, narrower than the modal filter above), Date Added (date range, present when Show Date Added is on), and Requirement Type and Requirement State on the Requirements column (shown when the group or group type has requirements).

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
- Custom grid columns through `[CustomizedGrid]` (legacy `ICustomGridColumns`). Legacy bound its grid to `GroupMember`, so a custom column's Lava `Row` means the group member. The grid is built from a projected row, so `WithBlock` is passed `GridBuilderGridOptions.LavaObject = row => row.GroupMember` to keep `Row` pointing at the same object ([StepParticipantList.cs:325](Rock.Blocks/Engagement/StepParticipantList.cs:325) is the pattern). Without it every custom column renders blank.

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
- Filter preferences are block person preferences prefixed with the group IdKey, so they follow the person, the block, and the group, the way legacy's grid filter preferences did. Legacy's own `{GroupId}-` preferences are not migrated.
- The export no longer carries the raw `GroupMember` entity columns. Legacy's `ExcelExportSource.DataSource` mode reflects over the bound type and appends every non-virtual property ([Grid.cs:2583](Rock/Web/UI/Controls/Grid/Grid.cs:2583)), so the file ended with roughly 28 extra columns: `IsSystem`, `GroupId`, `GroupTypeId`, `PersonId`, `GroupRoleId`, the archive and schedule fields, the audit fields, and `Id`, `IdKey`, `Guid` and the `Foreign*` set. The Obsidian grid exports declared columns only. Nothing in the block requested those columns and none are user-facing, but a church scripting against the export file would see them vanish.
- An attribute key defined at two levels of the group type chain produces one column instead of two. Legacy added a column per attribute but every one of them resolved through `dataItem.Attributes[DataField]` ([AttributeField.cs:86](Rock/Web/UI/Controls/Grid/AttributeField.cs:86)), which holds a single entry per key, so the extra columns rendered the same value under a different header. Confirmed on a test group. The surviving column is the group-qualified attribute, per [settled decision 8](#settled-decisions).
- There are now two ways to filter by registration, where legacy had one of each and they disagreed. The modal's Registration filter keeps legacy's meaning: it lists the instances the group is linked to and keeps members whose **person** registered for the chosen instance, whether or not their membership came from that registration ([GroupMemberList.ascx.cs:1920](RockWeb/Blocks/Groups/GroupMemberList.ascx.cs:1920)). The Registration column's filter reads the column, which shows only the registration a member was added through (`RegistrationRegistrant.GroupMemberId`, as legacy's column did at [GroupMemberList.ascx.cs:2067](RockWeb/Blocks/Groups/GroupMemberList.ascx.cs:2067)). So the modal filter can keep a row whose Registration cell is empty and the column filter cannot, which is why the modal filter carries help text saying so.
- An inactive group-qualified grid attribute no longer produces a column. Legacy passed `includeInactive: true` to `GetByEntityTypeQualifier` for group-qualified attributes while filtering `IsActive` on the inherited ones, so the two paths disagreed. Both are active-only now.

## Proposed Approach

### File layout

- `Rock.Blocks/Group/GroupMemberList.cs`: delete the stub and any related files; delete the existing stub `[Auth]` + `[BlockType]` + `[Block]`, `[EntityType]`, and block settings `[Attribute]` records. We'll start fresh, so we can perform a proper chop. `RestructureConnectionsPages.DeleteLegacyBlockTypeAndAllInstances()` has an example of how we've done this for legacy block types.
- We'll then re-add new `Rock.Blocks/Group/GroupMemberList.cs` and related files + new migration helper invocations as needed. It will ultimately look like editing in place; just spelling out the details here.
- `Rock.ViewModels/Blocks/Group/GroupMemberList/`: `GroupMemberListOptionsBag`, `GroupMemberListSyncBag`, `GroupMemberListPlaceElsewhereTriggerBag`, `GroupMemberListPlaceElsewhereRequestBag`.
- `Rock.JavaScript.Obsidian.Blocks/src/Group/groupMemberList.obs`, with partials under `src/Group/GroupMemberList/`: `personNameCell.partial.obs`, `gridSettingsModal.partial.obs`, `deleteOrArchiveCell.partial.obs` (pattern: [GroupList/deleteOrArchiveCell.partial.obs](Rock.JavaScript.Obsidian.Blocks/src/Group/GroupList/deleteOrArchiveCell.partial.obs)), `placeElsewhereModal.partial.obs`, `types.partial.ts`.
- A new set-based requirement status helper in the model layer next to [GroupService.GroupMembersNotMeetingRequirements](Rock/Model/Group/Group/GroupService.cs:882).

### Server shape

- `GetObsidianBlockInitialization` resolves the group once (group syncs with data view and role, requirements, member workflow triggers), builds the options bag (title, column flags, sync entries, Place Elsewhere triggers, filter lists, navigation URLs, `IsAddEnabled`, `IsDeleteEnabled`), and returns the grid definition.
- `GetListQueryable` starts from `GroupMemberService.Queryable( true )` filtered by `GroupId`, applies the family campus and gender modal filters, family campus as a subquery, and projects each member into a `GroupMemberRow`: the `GroupMember` entity, its role name and order, and a `PersonProjection` carrying only the person columns the grid uses (names and suffix, photo id, age, birth date, email, gender, record type and status, connection and marital status ids, age classification, top signal, deceased flag).
- `GetOrderedListQueryable` orders by role order, then last name, then first name.
- `GetListItems` materializes once, loads grid attribute values with `GridAttributeLoader.LoadFor( rows, r => r.GroupMember, GetGridAttributes(), rockContext )`, computes each person's IdKey, full name, reversed full name, and photo URL with the static `Person` helpers, then hydrates lookups keyed by member or person id, one query each: registrations, requirement statuses (bulk helper), members with history (when `EnableGroupHistory`), signed person ids (when a template is required), first and last attendance (when enabled), phone numbers and home addresses (the unexecuted person-id queryable as the `Contains` source, as in [SignUpOpportunityAttendeeList.cs:640](Rock.Blocks/Engagement/SignUp/SignUpOpportunityAttendeeList.cs:640)), and the multiple-roles person set from the rows themselves.
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
- `PersonColumn` for Name at a fixed `width="250px"`, not a percentage. `.grid-cell` carries `min-width: 120px` ([_grid-obsidian.scss:358](Rock.Frontend.Styles/src/styles/styles-v2/rock-components/_grid-obsidian.scss:358)), so once this block's visible columns pass roughly fourteen the row is wider than the viewport, nothing flexes, and every column including Name sits at 120px. That leaves about 40px for the name after padding and the avatar, and `word-break: break-word` on the cell then splits names mid-word. A percentage is `flex: 1 1 N%`, which that floor makes moot; a px value is `flex: 0 0 Npx` with `min-width: unset` ([getColumnStyles](Rock.JavaScript.Obsidian/Framework/Core/Controls/grid.ts:1346)), so it holds. The two most column-heavy Obsidian grids, the Registration Instance Registrant List and the Transaction List, pin their person column the same way.
- `personNameCell.partial.obs` is the `PersonColumn`'s `formatComponent`, rendering the framework person markup plus the indicator icons. It reads `hideAvatar`, `alwaysShowAvatar` and `enableHoverInfo` off the column the way the framework's own `personCell` does, so swapping it back out changes only the icons. The Requirements tab's name cell reuses it, matching legacy, which repeated every indicator but the triangle there. `AttributeColumns`; `hideOnScreen` columns for the export-only fields; `ButtonColumn` for the profile and Place Elsewhere buttons; `DeleteColumn` with the delete-or-archive cell.
- Panel header slot carries the sync label and its `PopOver`.

### Settled decisions

1. **Start fresh, not in place.** The stub's block type, block, auth, entity type, and attribute records are removed by migration (the `DeleteLegacyBlockTypeAndAllInstances` pattern in [RestructureConnectionsPages.cs:83](Rock.Migrations/Migrations/Version 20.0/Version 20.0/202608251721086_RestructureConnectionsPages.cs:83)), and the new block declares new guids, so the chop swaps one block type for another with nothing left behind.
2. **Projected rows, not the entity list base.** `RockListBlockType<GroupMemberRow>` selects the person columns the grid uses instead of materializing every `Person` column per member, and `GridAttributeLoader.LoadFor` gives projected rows the same bulk attribute load the entity base provides. The two closest siblings, the Sign-Up attendee list and the Step Participant List, project the same way.
3. **Two tabs over one data load.** The legacy Members and Requirements tabs are kept, so the block mimics what people use today. The tab selects which grid renders; both read the same `GetListItems` result, so the second tab costs no extra query. The tab bar goes in the grid's own `#gridHeaderPrepend` slot and writes `?tab=` to the URL, following [semesterGrid.partial.obs:33](Rock.JavaScript.Obsidian.Blocks/src/Lms/LearningProgramSecondaryLists/semesterGrid.partial.obs:33). Note that every Obsidian tabbed grid today switches between different entity types; ours is the first to tab between two column sets over the same rows.
4. **Bulk requirement statuses live in the model layer and are the single source.** The rules stay in one place next to the per-member method, a test compares both outputs on the same group, and the name-cell flags and the Requirements column read the same statuses so they cannot disagree.
5. **Inactive prompt is client-only.** The rows already carry `isInactive`, so no server round trip decides whether to prompt.
6. **The modal is only for filters that have to narrow the query.** A filter a row can answer is a column filter, so the grid's own filter row handles it. Registration is in both places because legacy's filter and its column never read the same data, and dropping either one would lose a behavior somebody uses; the modal one carries help text so the difference is visible where the choice is made.
7. **Preference keys are per group.** `MakeKeyUniqueToGroup` prefixes each preference key with the group's IdKey, and `makeKeyUniqueToGroup` mirrors it on the client off `GroupIdKey` in the options bag, following [GroupAttendanceList](Rock.Blocks/Group/GroupAttendanceList.cs:622), which scopes the same entity under the same method name. Twelve blocks scope preferences this way and the IdKey prefix is the majority of them. The same IdKey is passed as the grid's `preferencePrefix` so the column filters are scoped too, which only one other block does but which legacy required: `rFilter.PreferenceKeyPrefix` was set to the group id ([GroupMemberList.ascx.cs:267](RockWeb/Blocks/Groups/GroupMemberList.ascx.cs:267)) and covered the fields that are column filters now. Without it the Registration filter is the one that visibly breaks, since its values only exist on one group, but every filter would carry from group to group.
8. **A shared attribute key resolves to the most specific attribute.** `GridBuilder.AddField` throws on a duplicate field name, and neither `GetInheritedAttributesForQualifier` nor the group-qualified lookup deduplicates, so a key defined at two levels of a group type chain would fail the block outright. The candidates are gathered in the order [Helper.LoadAttributes](Rock/Attribute/Helper.cs:1163) uses, group-qualified then inherited from the most distant group type down, and the **first** one wins, matching the `TryAdd` that builds `entity.Attributes`. Note that Rock is not self-consistent here: `entity.AttributeValues` is built by plain assignment over the same list, so it is last-wins and can hold the inherited attribute's value under the group-qualified attribute's definition. The grid avoids that split because `GridAttributeLoader` only loads the attribute ids this method returns, so definition and value always come from the same attribute.

## Implementation Plan

Each row is a discrete implementation slice. **Implemented** means the code is written and in the working tree. **Tested** means every checkbox that belongs to this slice, in every linked test section, passes. Where a section is shared by several slices it is split by slice, and a row only owns its own sub-list. Mark ✅ when done, 🔄️ when started but incomplete, and leave blank when not begun.

| # | Slice | Verified by | Implemented | Tested |
|---|---|---|---|---|
| 1 | Block scaffold: rewrite the stub, options bag, group resolution, title, "no roles" warning, core columns (Name, Role, Status, Date Added, Note), row and Add navigation | [T1](#t1-block-load-and-group-resolution), [T2](#t2-header-and-title), [T12](#t12-authorization) | ✅ | ✅ |
| 2a | Projection-only columns: Marital Status, Connection Status, Gender, the export-only person fields, export file name, `[CustomizedGrid]` | [T3](#t3-columns-and-settings) | ✅ | ✅ |
| 2b | Support lookups: Registration, First and Last Attended, phones and home address | [T3](#t3-columns-and-settings) | ✅ | ✅ |
| 2c | Attribute columns | [T3](#t3-columns-and-settings) | ✅ | ✅ |
| 3 | Name indicators and row classes | [T4](#t4-name-indicators-and-row-styling) | ✅ | ✅ |
| 4 | Filters: column filters and the filter modal | [T5](#t5-filters) | ✅ | |
| 5 | Requirements tab: tab bar, Requirements grid, bulk status helper, requirement filters, name-cell requirement triangle | [T6](#t6-requirements) | | |
| 6 | Row actions: delete or archive, profile button, Place Elsewhere modal | [T7](#t7-row-actions), [T12](#t12-authorization) | | |
| 7 | Grid actions: Add visibility, Communicate with inactive prompt and merge field, standard actions | [T8](#t8-grid-actions-and-communication), [T12](#t12-authorization) | | |
| 8 | Group sync label, popover, Sync Now | [T9](#t9-group-sync), [T12](#t12-authorization) | | |
| 9 | Efficiency pass against the query budget | [T10](#t10-query-budget) | | |
| 10 | Chop: remove the WebForms files; block type swap migration authored separately | [T11](#t11-placements-and-chop-regression) | | |
| 11 | Query reduction: replace queries with existing caches first, then consolidate what is left | [T13](#t13-query-reduction) | | |

## Test Plan

Per-slice checklists backing the **Tested** column. A row is Tested only when every item in its linked sections passes.

**Environment setup.** One group per feature is enough: a small group with an inactive member, a member whose person record is inactive, and a deceased member; a group type with requirements (one role-specific, one with a warning state) and history enabled, with one member edited so a history snapshot exists; a group sync (any data view to a role); a required signature document template on the group; an event registration instance linked to the group with a registrant placed in it; a scheduling-enabled type with one person in two active roles; a MemberPlacedElsewhere trigger on the group type with Show Note on; a campus whose Team Group is set (Campus Detail); a group type with no roles.

### T1. Block load and group resolution

- [x] Group block setting wins over `GroupId`; with the setting blank, `GroupId` resolves as Id and as IdKey.
- [x] No `GroupId` but `CampusId` (Id and IdKey) resolves the campus team group.
- [x] No group, or a group the person cannot VIEW, renders nothing at all: no panel, no empty grid, no message.
- [x] A group type with no roles shows the "No roles!" warning above the grid, with no Add button; any memberships that already exist are still listed.
- [x] Row click opens the Detail Page with `GroupMemberId`; Add opens it with `GroupMemberId=0` and `GroupId`; both carry `CampusId` when the page had one.
- [x] Default sort is role order, then last name, then first name.
- [x] A deceased member is listed.
- [x] The Date Added and Note columns each appear only when their setting is on.

### T2. Header and title

- [x] Block Title shows as entered; blank falls back to "{GroupTerm} {GroupMemberTerm}s" using the group type's terms.
- [x] Icon is `ti ti-users`; the block sits in the secondary zone as before.

### T3. Columns and settings

**Slice 2a**

- [x] The Marital Status column appears on screen only when the group type's Show Marital Status is on.
- [x] Connection Status appears under the name only when the group type's Show Connection Status is on.
- [x] The Gender column appears on screen only when Display Gender Column is on, and sits after Marital Status rather than after Status.
- [x] Nick Name, Last Name, Birth Date, Age, Email, RecordStatusValueId, Record Status, Gender, and Is Deceased are absent on screen and present with values in the export.
- [x] The export carries Marital Status, Connection Status, Date Added, and Note with values even when their display settings are off, matching legacy's `AlwaysInclude` and `DataSource` behavior.
- [x] The export's Name column reads "Last, First" and the displayed name does not appear as a second Name column.
- [x] The export file is named after the group.
- [x] A custom grid column renders, with `Row` resolving to the group member: `{{ Row.Id }}`, `{{ Row.Note }}`, and `{{ Row.GroupMemberStatus }}` all produce values.
- [x] A custom grid column reaching a navigation property (`{{ Row.Person.NickName }}`, `{{ Row.GroupRole.Name }}`) either resolves or is confirmed blank, since the projected query does not eager-load those and runs `AsNoTracking`.

**Slice 2b**

- [x] The Registration column is hidden when no listed member has a registration.
- [x] The Registration column appears when at least one listed member has one, with one link per registration, each opening the Registration Page with `RegistrationId`.
- [x] First Attended and Last Attended appear only when Show First/Last Attendance is on and the group type takes attendance; with the setting on and attendance off, both stay hidden.
- [x] First and Last Attended show the earliest and latest attendance dates for a member with attendance, and are blank for a member with none.
- [x] Home Phone, Cell Phone, Home Address, Latitude, and Longitude are absent on screen and present with values in the export.
- [x] A member with no home address exports blank address, latitude, and longitude rather than failing.
- [x] Registration, First Attended, and Last Attended are absent from the export, as they were in legacy.

**Slice 2c**

- [x] A group-qualified grid attribute appears as a column.
- [x] An attribute inherited through the group type chain appears as a column.
- [x] An attribute not flagged Show in Grid does not appear.
- [x] An inactive attribute does not appear.
- [x] An attribute the person cannot VIEW does not appear, run as a person denied VIEW on that attribute.
- [x] An inherited attribute and a group-qualified attribute that share a key produce one column, not an error, and the surviving column is the group-qualified one. Legacy showed two columns here, but both rendered the same value, so the second was never a distinct attribute.
- [x] Attribute values are correct per row, including a member with no value for an attribute.
- [x] Export column order matches legacy end to end, accounting for the two deliberate moves: Status, which the Figma moves last among the on-screen columns, and the attribute columns, whose position follows whichever way the `<AttributeColumns />` placement above is decided.

### T4. Name indicators and row styling

The requirement triangle is a name indicator too, but it reads the bulk status helper that slice 5 builds, so it is checked in [T6](#t6-requirements) rather than here.

- [x] Multiple-roles warning appears only on a scheduling-enabled type and only for the person holding two active roles.
- [x] Note icon with tooltip when Show Note Column is off; no icon when it is on.
- [x] Unsigned-document icon appears only for people without a signed document when a template is required.
- [x] Signal icon renders for a person with a top signal, and hovering it shows no tooltip.
- [x] Icons are spaced away from the name and from each other, and wrap onto a second line rather than overflowing when the column is narrow.
- [x] Rows carry `is-inactive`, `is-inactive-person`, and `is-deceased` in the matching cases.

### T5. Filters

- [x] The Name column filter offers one option per listed member.
- [x] The Name column filter offers two separate options for two members who share a name.
- [x] Each Name column filter option reads as the member's name, not an identifier.
- [x] Checking one name in the Name column filter leaves only that member listed.
- [x] The Name column filter's search finds a member by nick name.
- [x] The Name column filter's search finds a member by last name.
- [x] The Name column filter's search finds a member by a legal first name that differs from their nick name.
- [x] The Name column filter offers fifty options at a time on a group with more than fifty members.
- [x] The Name column filter's search reaches a member outside the first fifty options.
- [x] The quick search finds a member by nick name.
- [x] The quick search finds a member by last name.
- [x] The quick search finds a member by a legal first name that differs from their nick name.
- [x] No First Name column appears on screen.
- [x] No First Name column appears in the export.
- [x] The Role column filter picks from the roles present in the list.
- [x] The Status column filter picks from the statuses present in the list.
- [x] The Gender column filter picks from existing values when Display Gender Column is on.
- [x] The Date Added column filter keeps a member whose date added falls on the lower bound.
- [x] The Date Added column filter keeps a member whose date added falls on the upper bound.
- [x] The Date Added column and its filter are absent when Show Date Added is off.
- [x] The Registration column filter is absent when no listed member has a registration.
- [x] The Registration column filter offers each registration on the listed rows as its own option, rather than one option per combination of them.
- [x] The Registration column filter keeps only the members whose own registration belongs to the chosen one.
- [x] The modal offers the Gender filter when Display Gender Column is off and omits it when that setting is on.
- [x] The modal offers the Family Campus filter only when Show Campus Filter is on.
- [x] The modal offers the Registration filter only when the group is linked to at least one registration instance.
- [x] The modal offers the Signed Document filter only when the group requires a signature document.
- [x] The modal holds nothing beyond those four.
- [x] The settings button is absent when all four modal filters are hidden: gender column on, campus filter off, no linked instance, no required template.
- [x] The Gender modal filter narrows the list when Display Gender Column is off.
- [x] Turning Display Gender Column on moves gender filtering to the column and produces the same rows.
- [x] The Family Campus filter keeps a person whose family at that campus is not their primary family.
- [x] The Signed Document filter set to Yes keeps only people who have signed.
- [x] The Signed Document filter set to No keeps only people who have not signed.
- [x] The modal's Registration filter lists the instances the group is linked to, including one that no listed member registered through.
- [x] The modal's Registration filter keeps a member whose person registered for the chosen instance even though their Registration cell is empty.
- [x] The modal's Registration filter carries help text explaining how it differs from the Registration column's filter.
- [x] The settings icon reads as active while a modal filter is set.
- [x] The settings icon reads as inactive once every modal filter is cleared.
- [x] A preference left behind by a modal filter that is now hidden does not light the settings icon.
- [x] A preference left behind by a modal filter that is now hidden does not narrow the list.
- [x] A modal filter value survives a page reload.
- [x] Saving the modal reloads the grid.
- [x] A modal filter set on one group is not in effect after switching the page to another group.
- [x] A column filter set on one group is not in effect after switching the page to another group.
- [x] Switching to another group and back restores the first group's filters.

### T6. Requirements

- [ ] The tab bar and the Requirements tab appear only when the group or group type has requirements; with none, the Members grid renders alone with no tab bar.
- [ ] Switching tabs issues no second `GetGridData` call; reloading on `?tab=Requirements` lands on that tab.
- [ ] Requirements column and its filters appear only when the group or group type has requirements.
- [ ] Labels: one per applicable type with the right color; a role-specific requirement shows only for that role; a type the person cannot VIEW is absent.
- [ ] Requirement Type and Requirement State filters narrow the list alone and combined.
- [ ] Bulk helper matches `GetGroupRequirementsStatuses` per member on the test group (states, warning dates, due-date fallback), including a data-view-scoped and an age-classification-scoped requirement.
- [ ] Requirement triangle in the name cell: danger for Not Met, warning for Meets With Warning, none when all requirements are met, with the legacy tooltips.
- [ ] Name-cell flags and the Requirements column agree for every member.

### T7. Row actions

- [ ] Delete removes a member with no history; Archive (icon, label, and confirm text) is offered when the type has history and the member has a snapshot.
- [ ] A member added through a registration gets the second registration warning before removal.
- [ ] A sync-managed role shows the disabled sync button with its tooltip.
- [ ] A refused delete (`CanDelete` false) shows the server message and keeps the row.
- [ ] Profile button appears only when Person Profile Page is set and opens the profile.
- [ ] Place Elsewhere: button appears only when a trigger exists; a single trigger shows its name, several show the chooser; Note visibility and required-ness follow the qualifier; Place removes the member, unlinks registrants, and the workflow launches.
- [ ] Without `canEdit`, no Add, delete, or Place Elsewhere buttons appear, and rows still open the detail page.
- [ ] The profile, Place Elsewhere, and delete columns are excluded from the export.

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

**Slice 1**

- [x] A person without group VIEW: the block renders nothing and `GetGridData` called directly returns no rows.
- [x] View-only: no Add button, and rows still open the detail page.
- [x] Group MANAGE_MEMBERS alone enables Add, the same as block EDIT and as group EDIT.
- [x] A person without group VIEW: `CreateGridEntitySet` and `CreateGridCommunication` called directly answer 403.

Per-attribute and per-requirement-type VIEW are checked where those features are tested, in [T3](#t3-columns-and-settings) and [T6](#t6-requirements).

**Slice 6**

- [ ] View-only: no delete or Place Elsewhere buttons.
- [ ] View-only: `Delete` and `PlaceElsewhere` called directly answer 400 and change nothing.
- [ ] Editor: `Delete` called directly for a sync-managed member answers 400 and the member remains.
- [ ] Editor: `Delete` and `PlaceElsewhere` called directly with the key of a member of another group answer 400 and change nothing.
- [ ] Editor: `PlaceElsewhere` called directly with a trigger id from another group type answers 400.
- [ ] Group MANAGE_MEMBERS alone enables delete, the same as block EDIT and as group EDIT.

**Slice 7**

- [ ] Communicate called directly with a recipient key that is not a member of the group creates the communication without that recipient.

**Slice 8**

- [ ] View-only: Sync Now is offered, and `SyncGroup` called directly runs the sync.

### T13. Query reduction

Slice 9 proves the block meets the query budget. This slice asks whether the budget itself can come down, once every lookup exists and can be judged together.

**First pass: cache before consolidation.** Before reshaping a single query, enumerate every query the block issues and ask of each one whether Rock already caches that data. Consolidating two queries into one is worth less than deleting one outright, and it is the more invasive change. Do this pass first and re-count before touching anything below.

- [ ] List every query the block issues, from `GetObsidianBlockInitialization` through `GetGridData`, including the ones inside helpers rather than only the obvious `Queryable()` calls. Capture it against a group with every feature enabled.
- [ ] For each one, search for an existing cache API before concluding it needs a query. `AttributeCache`, `GroupTypeCache`, `DefinedValueCache`, `EntityTypeCache`, and `FieldTypeCache` all carry lookup helpers that mirror their service equivalents. `AttributeCache.GetOrderedGridAttributes` replacing `AttributeService.GetByEntityTypeQualifier` in slice 2c is the worked example.
- [ ] When several blocks reach the same data different ways, prefer the way the most blocks use, not the way the nearest sibling block uses. A single sibling is one precedent, not the convention.
- [ ] Check whether any remaining query is asking for something a cached object already holds, one property at a time, rather than needing the row at all.

**Second pass: consolidation.** Only for what genuinely has to hit the database after the pass above.

- [ ] Extract the filtered `GroupMember` queryable so the `Contains` subqueries compose over entities rather than over the row projection. The subquery SQL should read as a plain `SELECT [Id] FROM [GroupMember] WHERE ...`. This also guarantees the subquery and the grid stay filtered identically once slice 4 puts the modal filters in `GetListQueryable`.
- [ ] Fold the phone number and home address lookups into the member projection, replacing two queries with none. Nested collection projections are established in Rock: [PersonSearch.cs:361](Rock.Blocks/Crm/PersonSearch.cs:361) projects two sibling collections in one `Select`, and [NamelessPersonList.cs:278](Rock.Blocks/Crm/NamelessPersonList.cs:278) does it inside a `RockListBlockType.GetListQueryable`. Home address needs no collection at all, just an ordered `FirstOrDefault()`.
- [ ] Consider folding registrations in as well. `GroupMember` has no inverse navigation to `RegistrationRegistrant`, so this needs a correlated subquery over a second `IQueryable` inside the projection, which has no precedent in Rock's blocks. Confirm the Registration column and its data-driven visibility rule still behave.
- [ ] Leave first and last attendance as its own `GROUP BY` unless measurement says otherwise. Folding it turns one scan over the group's attendance into a correlated aggregate per row.
- [ ] Re-run [T10](#t10-query-budget) after each change, and confirm row counts and every column value, on screen and in the export, are identical to before.

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
