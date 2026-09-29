---
author: Kyle Henning
date_created: 2026-09-14
summary: >-
  Add a Workflows list to the Connections Hub docked panel showing every persisted
  workflow launched from the selected Connection Request (type, trigger, current
  activity, start date, status), with row click opening the workflow's entry form or
  detail page. Restores the Workflows grid the legacy Connection Request Board modal had.
contributors: []
related_docs:
  - docs/connection/request-board.md
  - docs/connection/workflows-and-triggers.md
---

# Connections Hub: Workflows List in the Docked Panel

## Summary

The legacy WebForms Connection Request Board showed a collapsible "Workflows" grid inside its request modal, listing every `ConnectionRequestWorkflow` row for the request with Workflow Type, Trigger, Current Activity, Start Date, and a Running/Completed status pill. Clicking a row opened the workflow's entry form (if the viewer had an active form) or the Workflow Detail page. The Obsidian Connections Hub replaced that block but only carried over the "launch a manual workflow" buttons, not the list of workflows that have already been launched. This spec adds the list to the docked panel's view mode, backed by a new bag on `ConnectionRequestDetailsBag` and a refresh block action so a workflow launched from the panel shows up immediately.

## Motivation

[Issue #7036](https://github.com/SparkDevNetwork/Rock/issues/7036) reports that after launching a manual workflow from a request in v20, "there is no visual representation of that workflow on the connection request." Staff can only confirm the launch through the Workflow Type's instance list or SQL. This also affects workflows fired automatically by triggers (Status Changed, Connected, and so on), which are the common case for follow-up tasks such as background checks. Connectors need to see, from the request itself, that a workflow exists, what it is waiting on, and whether it finished.

The data is already persisted. `LaunchWorkflowForRequests` writes a `ConnectionRequestWorkflow` row for a single request at `Rock.Blocks/Engagement/ConnectionsHub.cs:5619`, the bulk background path does the same at `:1740`, and the automatic triggers have always written them from `Rock/Tasks/ProcessConnectionRequestChange.cs`, `Rock/Transactions/ConnectionRequestActivityChangeTransaction.cs:169`, and the `Rock/Jobs/ConnectionRequestWorkflowTriggers.cs:231` job. Only the display is missing.

## Current State

The docked panel view body (`Rock.JavaScript.Obsidian.Blocks/src/Engagement/ConnectionsHub/viewDockedPanelBody.partial.obs`) renders three regions: the Additional Requests strip, a "Connection Request" `ContentSection` (`:96`), and an "Activity Feed" `ContentSection` (`:315`). Manual workflows appear as action buttons fed by `ConnectionRequestDetailsBag.ActionItems`, built at `ConnectionsHub.cs:3455-3476`, and a click emits `launchWorkflow` (`viewDockedPanelBody.partial.obs:904`), which the hub handles in `launchWorkflowForRequests` (`connectionsHub.obs:3945`). That handler shows a status alert and opens the entry page in a new tab when the result carries `WorkflowEntryPageUrl`. It does not reload the docked panel.

`ConnectionRequestDetailsBag` (`Rock.ViewModels/Blocks/Engagement/ConnectionsHub/ConnectionRequestDetailsBag.cs`) has no collection of launched workflows, and `GetConnectionRequestDetailsBag` (`ConnectionsHub.cs:3307`) never queries `ConnectionRequestWorkflowService`.

The block already declares both linked-page settings the legacy grid depended on: `WorkflowDetailPage` (`ConnectionsHub.cs:67`, default `Rock.SystemGuid.Page.WORKFLOW_DETAIL`) and `WorkflowEntryPage` (`:73`). `WorkflowEntryPage` is used by the launch action. `WorkflowDetailPage` is declared but referenced nowhere else in the block, so it is currently dead configuration.

Legacy reference (from `git show 3f99e195ac^:RockWeb/Blocks/Connection/ConnectionRequestBoard.ascx.cs`):

- `BindRequestModalViewModeWorkflowsGrid()` (`:2928`) queried `ConnectionRequestWorkflowService` for the request, included `Workflow.WorkflowType`, kept rows whose Workflow Type the viewer could VIEW, projected Workflow Type name, `TriggerType.ConvertToString()`, `Workflow.ActiveActivityNames`, `ActivatedDateTime` short date, and a status pill (`label-success` when `CompletedDateTime` is set, otherwise `label-info`; the text "Active" was rewritten to "Running"). Rows sorted by activation date descending. The panel widget was hidden when no rows survived and otherwise titled `Workflows <badge>{count}</badge>`.
- `gRequestModalViewModeWorkflows_RowSelected` (`:2992`) opened the entry page in a new window when `Workflow.HasActiveEntryForm( CurrentPerson )` was true, otherwise navigated to the Workflow Detail page with `WorkflowId`.

## Requirements

### Display

- The docked panel view mode MUST show a "Workflows" section listing every `ConnectionRequestWorkflow` for the selected request whose `Workflow` and `Workflow.WorkflowType` still exist and whose Workflow Type the current person is authorized to VIEW.
- Each row MUST show: Workflow Type name, trigger type description, current activity name(s), activation date, and a status pill. The pill MUST use success styling when `Workflow.CompletedDateTime` has a value and info styling with the text "Running" otherwise, mirroring the legacy rewrite of the raw `Status` value "Active".
- Rows MUST be ordered by `Workflow.ActivatedDateTime` descending.
- The section header MUST display the row count as a badge.
- The section MUST be collapsible and MUST start collapsed, so the count is visible at a glance without the grid pushing the Activity Feed down. The legacy modal's Workflows panel widget was likewise collapsible.
- The section MUST render after the Activity Feed, as the last section in the panel.
- The section MUST be hidden entirely when there are no visible rows, matching the legacy behavior and keeping the panel compact for requests that never launched a workflow.
- A failure to load workflows MUST NOT block the rest of the request details from rendering.

### Navigation

- Clicking a row MUST open the workflow in a new tab, consistent with `navigateToPersonProfile` and `navigateToGroup` in `connectionRequestDockedPanel.partial.obs:527-544`.
- The target MUST be the Workflow Entry page (with `WorkflowTypeId` and `WorkflowGuid`) when `Workflow.HasActiveEntryForm( CurrentPerson )` is true, otherwise the Workflow Detail page (with `WorkflowId`). Both come from the existing `WorkflowEntryPage` and `WorkflowDetailPage` block settings.
- If the relevant linked page setting is blank, the row MUST render without a link rather than producing a broken URL.

### Refresh

- After a workflow is launched from the docked panel and the launch action returns, the Workflows section MUST refresh without a full details reload so the new row appears immediately.
- Bulk launches from the list view (background path) are out of scope for live refresh; the row appears the next time the request is opened.

### Security and compatibility

- Authorization MUST be enforced server-side per row against the Workflow Type, exactly as the legacy block did. Rows the viewer cannot see MUST NOT be sent to the client.
- No changes to `Rock.Model`, no migration, and no change to existing block action signatures.

## Design

### Server: new bag

Add `Rock.ViewModels/Blocks/Engagement/ConnectionsHub/ConnectionRequestWorkflowBag.cs`:

```csharp
public class ConnectionRequestWorkflowBag
{
    /// <summary>The IdKey of the ConnectionRequestWorkflow row. Used as the client row key.</summary>
    public string IdKey { get; set; }

    /// <summary>The name of the workflow type that was launched.</summary>
    public string WorkflowTypeName { get; set; }

    /// <summary>The trigger that launched the workflow (Manual, Status Changed, etc.).</summary>
    public ConnectionWorkflowTriggerType TriggerType { get; set; }

    /// <summary>The names of the workflow's currently active activities. Empty when completed.</summary>
    public List<string> ActiveActivityNames { get; set; }

    /// <summary>When the workflow was activated.</summary>
    public DateTimeOffset? ActivatedDateTime { get; set; }

    /// <summary>True when Workflow.CompletedDateTime has a value.</summary>
    public bool IsCompleted { get; set; }

    /// <summary>The raw Workflow.Status value, shown when a completed workflow has a custom terminal status.</summary>
    public string Status { get; set; }

    /// <summary>
    /// The URL to open when the row is clicked: the Workflow Entry page when the current person has an
    /// active entry form, otherwise the Workflow Detail page. Null when the relevant page setting is blank.
    /// </summary>
    public string WorkflowUrl { get; set; }
}
```

The trigger is sent as the enum so the client can use the generated `ConnectionWorkflowTriggerTypeDescription` map (`Rock.JavaScript.Obsidian/Framework/Enums/Connection/connectionWorkflowTriggerType.ts:79`) instead of a server-formatted string. Active activity names are a list rather than the `<br/>`-delimited string that `Workflow.ActiveActivityNames` produces (`Rock/Model/Workflow/Workflow/Workflow.Logic.cs:119`), so the template controls layout and nothing is rendered as raw HTML.

Add `public List<ConnectionRequestWorkflowBag> Workflows { get; set; }` to `ConnectionRequestDetailsBag`. Regenerate `connectionRequestDetailsBag.d.ts` and add `connectionRequestWorkflowBag.d.ts` under `Rock.JavaScript.Obsidian/Framework/ViewModels/Blocks/Engagement/ConnectionsHub/`.

### Server: query helper

Add a private helper in `ConnectionsHub.cs` next to the private `GetActivityEntries( ConnectionRequest, mergeFields )`:

```csharp
/// <summary>
/// Gets the persisted workflows launched from the Connection Request that the current person
/// is authorized to view, newest first.
/// </summary>
private List<ConnectionRequestWorkflowBag> GetConnectionRequestWorkflowBags( int connectionRequestId )
```

Behavior:

1. Query `new ConnectionRequestWorkflowService( RockContext ).Queryable().AsNoTracking()` filtered to `ConnectionRequestId == connectionRequestId && Workflow != null && Workflow.WorkflowType != null`, including `Workflow.Activities.Select( a => a.ActivityType )`, `Workflow.Activities.Select( a => a.AssignedPersonAlias )`, and `Workflow.Activities.Select( a => a.AssignedGroup.Members )`. Those are the navigation properties `ActiveActivities` and `HasActiveEntryForm` (`Workflow.Logic.cs:314`) read; loading them up front avoids N+1 lazy loads per row.
2. For each row, check `WorkflowTypeCache.Get( workflow.WorkflowTypeId ).IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson )`. Skip unauthorized rows. The cache is preferred over the entity because the type's security is already cached and the hub already resolves workflow types through the cache in the launch action.
3. Compute `WorkflowUrl`:
   - If `workflow.HasActiveEntryForm( RequestContext.CurrentPerson )`: `this.GetLinkedPageUrl( AttributeKey.WorkflowEntryPage, new Dictionary<string, string> { ["WorkflowTypeId"] = workflow.WorkflowTypeId.ToString(), ["WorkflowGuid"] = workflow.Guid.ToString() } )`. This is the same parameter set the launch action builds at `:5650-5655`.
   - Otherwise: `this.GetLinkedPageUrl( AttributeKey.WorkflowDetailPage, "WorkflowId", workflow.IdKey )`.
   - `GetLinkedPageUrl` returns an empty string when the setting is blank; normalize to null.
4. Order by `ActivatedDateTime` descending and return.

Call it from `GetConnectionRequestDetailsBag` (`:3307`) after `ActivityEntries` is set, so the initial docked panel load includes the list.

### Server: refresh block action

Add alongside `GetActivityEntries` (`:7127`), following its shape exactly (resolve by IdKey honoring `DisablePredictableIds`, bad request on not found, bad request on missing VIEW authorization on the request):

```csharp
[BlockAction]
public BlockActionResult GetConnectionRequestWorkflows( string connectionRequestIdKey )
```

Returns `List<ConnectionRequestWorkflowBag>`.

### Client: docked panel body

In `viewDockedPanelBody.partial.obs`, add a third `ContentSection` after the "Activity Feed" section (ends `:395`) as the last section in the panel, containing an Obsidian `Grid` in light mode. It is collapsible and starts collapsed so the count badge is visible at a glance without the grid pushing the feed around. Light mode is the established pattern for read-only sub-lists inside detail panels (for example `Rock.JavaScript.Obsidian.Blocks/src/Cms/MediaElementDetail/viewPanel.partial.obs:7`); it drops the filter header, footer, paging, and striping (`Rock.JavaScript.Obsidian/Framework/Controls/Grid/grid.partial.obs:358`), and the five legacy columns map one-to-one onto grid columns.

```html
<ContentSection v-if="workflowRows.length > 0"
                icon="ti ti-settings-cog"
                :isCollapsed="true">
    <template #title>
        <span class="pr-spacing-xs">Workflows</span>
        <Badge badgeType="info">{{ workflowRows.length }}</Badge>
    </template>
    <Grid :data="workflowGridData"
          keyField="idKey"
          itemTerm="Workflow"
          light
          :onSelectItem="onWorkflowSelected">
        <TextColumn name="workflowTypeName"
                    title="Workflow Type"
                    field="workflowTypeName"
                    visiblePriority="xs" />
        <TextColumn name="trigger"
                    title="Trigger"
                    field="trigger"
                    visiblePriority="sm" />
        <TextColumn name="currentActivity"
                    title="Current Activity"
                    field="currentActivity"
                    visiblePriority="sm" />
        <DateColumn name="activatedDateTime"
                    title="Start Date"
                    field="activatedDateTime"
                    visiblePriority="xs" />
        <LabelColumn name="status"
                     title="Status"
                     field="status"
                     :classSource="workflowStatusClassSource"
                     defaultLabelClass="success"
                     visiblePriority="xs" />
    </Grid>
</ContentSection>
```

`ContentSection` exposes a `title` slot (`Rock.JavaScript.Obsidian/Framework/Controls/contentSection.obs:16`), which is how the count gets into the header without adding a prop to the shared control. The count uses the existing `Badge` control (already imported in this partial) rather than raw badge markup, and the `pr-spacing-xs` utility keeps a gap between the label and the badge.

The docked panel is a narrow side pane, so the `visiblePriority` values let the grid's responsive column hiding drop Trigger and Current Activity first while Workflow Type, Start Date, and Status stay visible at the smallest width.

Script additions, in the existing regions:

- Imports: `import Grid, { TextColumn, DateColumn, LabelColumn } from "@Obsidian/Controls/grid";` and `ConnectionWorkflowTriggerTypeDescription` from the generated enum file.
- Values: `const workflows = ref<ConnectionRequestWorkflowBag[]>(props.requestDetails?.workflows ?? []);` and `const workflowStatusClassSource = { Running: "info" };` (any other value falls through to the `success` default, so custom terminal statuses render as completed).
- Computed Values: `workflowRows` maps each bag onto a flat row: `idKey`, `workflowTypeName`, `trigger` (`ConnectionWorkflowTriggerTypeDescription[bag.triggerType]`), `currentActivity` (active activity names joined with ", "), `activatedDateTime` (passed through; `DateColumn` formats it), and `status` (`getWorkflowStatusText(bag)`). `workflowGridData` wraps that as `{ rows: workflowRows.value }`, the same client-built `GridDataBag` shape `viewPanel.partial.obs:106` uses.
- Functions: `getWorkflowStatusText` returns "Running" when not completed; when completed it returns the raw status unless that status is "Active", "Completed", or empty, in which case it returns "Completed". `onWorkflowSelected(key)` looks up the bag by `idKey` and calls `window.open(bag.workflowUrl, "_blank")` when the URL is present, matching the docked panel's other outbound links; rows without a URL no-op.
- Watchers: mirror the `activityEntries` watcher at `:1093` so `workflows` follows `props.requestDetails?.workflows`.

Because `onSelectItem` enables selection for every row, the grid shows the pointer cursor on rows that have no URL. That is acceptable; the alternative (a per-row `ButtonColumn`) adds a column to an already tight table.

### Client: refresh after launch

Add `refreshConnectionRequestWorkflows(requestIdKey)` to `connectionsHub.obs` next to `refreshActivityEntries` (`:4526`), with the same guards (docked panel visible, details box has an entity, key present). It invokes `GetConnectionRequestWorkflows` and assigns the result to `dockedPanelDetailsBox.value.entity.workflows`; on failure it calls `setErrorMessage("view", ...)` so the message surfaces in the panel's existing error slot.

In `launchWorkflowForRequests` (`:3945`), inside the success branch and before the early `return`, call `refreshConnectionRequestWorkflows(dockedPanelDetailsBox.value?.entity?.connectionRequestIdKey)` when `isDockedPanelVisible.value` is true. The single-request path in `LaunchWorkflowForRequests` adds the `ConnectionRequestWorkflow` row synchronously before returning (`:5619`), so the refreshed list already contains it. The bulk path defers to `LaunchWorkflowsInBackground` and is not refreshed (see Requirements).

### Migration

None. Both linked-page attributes already exist on the block type with defaults pointing at the core Workflow Entry and Workflow Detail pages. The new bag property and block action are additive.

## Considered but Rejected

### Reuse `Workflow.ActiveActivityNames` as a string
Rejected. It joins names with `<br/>` (`Workflow.Logic.cs:123`), which would force `v-html` in the template. A list of names lets the template decide separators and avoids rendering server strings as HTML.

### Send URL placeholders through `NavigationUrls` like Person Profile and Group Detail
Rejected. The person and group links need one key each, so a `((Key))` placeholder works (`ConnectionsHub.cs:312-313`). The workflow link needs two parameters for the entry page and a different page altogether when there is no active form, and the choice depends on `HasActiveEntryForm( CurrentPerson )`, which is a server-side authorization decision. Precomputing one URL per row keeps that logic on the server, the same way `LaunchWorkflowResultBag.WorkflowEntryPageUrl` already does.

### Reload the whole details box after a launch
Rejected. `getConnectionRequestDetails` (`connectionsHub.obs:4507`) clears the box and shows the loading skeleton, which would flash the entire panel just to add one row. A targeted refresh action follows the `GetActivityEntries` precedent.

### Hand-rolled two-line rows instead of a grid
Rejected. Compact name-plus-meta rows (the Additional Requests pattern at `viewDockedPanelBody.partial.obs:59`) would fit the narrow pane, but they lose the column alignment that makes five fields scannable across many workflows, and they require bespoke styles and a custom status pill. The light grid already provides column hiding by `visiblePriority`, a `DateColumn`, and a `LabelColumn`, and it matches how other Obsidian detail panels render read-only sub-lists.

### Show all workflows regardless of Workflow Type security
Rejected. The legacy block filtered per row on VIEW for the Workflow Type, and `ActionItems` already filters manual workflows the same way (`:3455`). Exposing type names or activity names for workflows the viewer cannot open would be a regression in information disclosure.

### Put the list inside the Activity Feed as another entry type
Rejected. Activity entries are chronological events with notes and connectors; a workflow is a live object with a changing current activity and status. Mixing them would hide the status pill behind the activity filter dropdown and confuse the "Show ..." filter semantics.

## Open Questions

- Should the section also appear in the panel's edit mode (`editDockedPanelBody.partial.obs`)? The legacy modal only showed the grid in view mode. Recommendation: view mode only.

## Verification Steps

1. Configure a Connection Type with a manual-trigger workflow and a Status Changed trigger workflow, both with auto-persist enabled on their workflow types.
2. Open the Connections Hub, select a request, and confirm no "Workflows" section renders when no workflows have been launched.
3. Click the manual workflow action button. After the status alert closes, confirm a collapsed "Workflows" section appears at the bottom of the panel with a count badge of 1, without the panel reloading. Expand it and confirm the row shows the type name, "Manual", the current activity, today's date, and a "Running" info pill.
4. Change the request's status to fire the trigger workflow. Reopen the request and confirm two rows, newest first, with the second showing "Status Changed".
5. Complete one workflow. Reopen the request and confirm its pill turns success and reads "Completed" (or its custom terminal status).
6. As a user with an active entry form on a running workflow, click the row and confirm the Workflow Entry page opens in a new tab with `WorkflowTypeId` and `WorkflowGuid`.
7. As a user without an active form, click a row and confirm the Workflow Detail page opens in a new tab with `WorkflowId`.
8. Remove VIEW from the Workflow Type for a test role and confirm that role no longer sees the row and the count decrements or the section hides.
9. Blank the block's Workflow Detail Page setting and confirm clicking a completed row does nothing and logs no console error.
10. Narrow the browser until the docked panel is at its minimum width and confirm the Trigger and Current Activity columns hide while Workflow Type, Start Date, and Status remain.
11. Confirm `GetConnectionRequestWorkflows` returns a bad request for an IdKey the current person cannot VIEW.
12. Run `npm run lint` in `Rock.JavaScript.Obsidian.Blocks` and build `Rock.sln`.

## Out of Scope

- Live refresh after bulk launches from the list view (background path).
- A Workflows column or badge on the request cards or grid rows.
- Deleting or cancelling workflows from the panel.
- Showing workflows on the Mobile Connection Request Detail block.

## Related

- [Issue #7036](https://github.com/SparkDevNetwork/Rock/issues/7036), Workflows Panel Missing in Docked View of Connection Request Detail. Reported against 20.0.4. Read 2026-09-14; this spec covers the four fields the issue asks for (trigger type, current activity, start date, status) plus row navigation from the legacy grid.
- Legacy implementation: `git show 3f99e195ac^:RockWeb/Blocks/Connection/ConnectionRequestBoard.ascx.cs` (`BindRequestModalViewModeWorkflowsGrid` at `:2928`, `gRequestModalViewModeWorkflows_RowSelected` at `:2992`, `RegisterWorkflowDetailPageScript` at `:3025`) and `.ascx` (`:571-582`). Removed in `3f99e195ac`.
- Prior Hub parity spec: `specs/completed/connection/260909-connections-hub-connection-type-and-assigned-settings.md`.
- `docs/connection/request-board.md` and `docs/connection/workflows-and-triggers.md` describe the surfaces this spec touches.
