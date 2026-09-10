---
author: Jason Hendee
date_created: 2026-08-25
summary: >-
  Phase 2 of the Connections page cleanup: make the Connections Hub the entry
  point for creating a Connection Request. Adds a Request=0 add indicator, an
  IsAddConnectionRequestRequested option, and add modal preselection from the
  Connection Type or Opportunity. Hub block changes only; the link redirection
  sweep is Phase 3.
contributors: []
---

# Connections Hub Add Entry Point (Phase 2 of Connection Page/Block Cleanup)

## Summary

DEV-15156 decided the Connections Hub becomes the single place staff enter a Connection Request, replacing the legacy Connection Request Detail page. Phase 3 repoints every core link at the Hub. The add link shape it will send is a silent no-op against the Hub today.

Phase 2 fixes that, entirely inside `Rock.Blocks/Engagement/ConnectionsHub.cs`, `Rock.ViewModels/Blocks/Engagement/ConnectionsHub/ConnectionsHubOptionsBag.cs`, and `Rock.JavaScript.Obsidian.Blocks/src/Engagement/connectionsHub.obs`. It changes no links, no pages, and no block attribute values, so nothing outside the Hub behaves differently until Phase 3 lands.

## Motivation

Phase 1 flattened the page tree and removed two dead blocks. It deliberately deferred the behavior change, because the Hub entry point carried unresolved questions that should not hold up a structural cleanup.

Those questions are now answerable from the code, and the ordering matters. Phase 3 rewrites the links; if it lands first, the add link breaks outright. `RockWeb/Blocks/Connection/MyConnectionOpportunities.ascx.cs:548` calls `NavigateToConnectionPage( 0 )`, which lands on the legacy detail page in add mode. Repointed at the Hub with `Request=0`, it renders the ordinary grid and no modal.

Doing the Hub work first also gives it its own QA pass, before every core link in Rock depends on it.

## Current State

The Hub already declares and resolves the three page parameters this needs. `ConnectionType` and `ConnectionOpportunity` are resolved through their services honoring the site's predictable-ids setting, and the resolved opportunity already reaches the client as `ConnectionOpportunityGuidFromPageParameter`, which seeds the add modal's opportunity at `connectionsHub.obs:1132`. Nothing new needs parsing, and the add modal itself is fully built at `Rock.JavaScript.Obsidian.Blocks/src/Engagement/ConnectionsHub/addConnectionRequestModal.partial.obs`.

One defect sits between that and a working entry point: `Request=0` is a silent no-op. `Rock.Blocks/Engagement/ConnectionsHub.cs:327`:

```csharp
if ( PageParameter( PageParameterKey.Request ).IsNotNullOrWhiteSpace() )
{
    options.ConnectionRequestIdKey = new ConnectionRequestService( RockContext )
        .Get( PageParameter( PageParameterKey.Request ), !PageCache.Layout.Site.DisablePredictableIds )?.IdKey
        ?? string.Empty;
}
```

`Get( "0" )` returns null, so `ConnectionRequestIdKey` lands as an empty string and the Hub renders its normal list. No modal, no error.

Outside add mode the Hub is already correct. A `Request` that resolves opens its docked panel at `connectionsHub.obs:4759`, and the Connection Type context comes from the `ConnectionType` or `ConnectionOpportunity` parameter as it does today.

## Requirements

Add mode:

- `Request=0` MUST be treated as an add indicator and MUST NOT be sent through the request lookup.
- `ConnectionsHubOptionsBag` MUST carry a new `IsAddConnectionRequestRequested` boolean defaulting to `false`.
- When the flag is set, the Hub MUST open the add modal on load, reusing `onAddItem` (`connectionsHub.obs:2186`).
- The trigger MUST respect the same authorization that gates the Add button, `canEditConnectionRequests` (`connectionsHub.obs:1269`). An individual who cannot add sees the ordinary grid, not the modal.
- Preselection MUST follow the parent task: both parameters passed opens the modal on that type and that opportunity; only the type passed opens it on that type with the opportunity list filtered to it; neither passed opens it unfiltered, which the Hub provides only in My Connections mode.
- Outside My Connections mode an add link MUST supply a `ConnectionType` or `ConnectionOpportunity`, since the Hub has no type context without one.

View mode:

- A view link MUST supply a `ConnectionType` or `ConnectionOpportunity` alongside `Request`. The Hub is unchanged on this path.

Everywhere:

- Behavior MUST be unchanged when `Request` is absent, and unchanged for any `Request` value other than the add indicator.

## Design

### 1. Short-circuit the add indicator

The existing block at `ConnectionsHub.cs:327` gains an indicator check ahead of the lookup:

```csharp
var requestParameter = PageParameter( PageParameterKey.Request );

if ( requestParameter == AddConnectionRequestIndicator )
{
    options.IsAddConnectionRequestRequested = true;
}
else if ( requestParameter.IsNotNullOrWhiteSpace() )
{
    options.ConnectionRequestIdKey = new ConnectionRequestService( RockContext )
        .Get( requestParameter, !PageCache.Layout.Site.DisablePredictableIds )?.IdKey ?? string.Empty;
}
```

Nothing moves. The Connection Type and Opportunity resolve where they do today, `SetSingleConnectionTypeHubOptions` runs unchanged, and no preference write is touched.

### 2. Options bag

One new property on `Rock.ViewModels/Blocks/Engagement/ConnectionsHub/ConnectionsHubOptionsBag.cs`, alongside the existing `ConnectionRequestIdKey`:

```csharp
/// <summary>
/// Gets or sets a value indicating whether the block should open the
/// Add Connection Request modal on load.
/// </summary>
public bool IsAddConnectionRequestRequested { get; set; }
```

### 3. Client trigger

The deep-link handling at the bottom of `connectionsHub.obs` gains a parallel branch next to the existing one at line 4759:

```ts
if (!isNullOrWhiteSpace(box.options?.connectionRequestIdKey)) {
    onSelectItem(box.options!.connectionRequestIdKey!);
}
else if (box.options?.isAddConnectionRequestRequested && canEditConnectionRequests.value) {
    onAddItem();
}
```

The two are mutually exclusive by construction: the server never sets both.

### 4. Preselection needs no new plumbing

`internalSelectedOpportunity` (`connectionsHub.obs:1129`) already initializes from `connectionOpportunityGuidFromPageParameter`, and `newConnectionRequest` (`connectionsHub.obs:1132`) already seeds its `connectionOpportunityGuid` and `campusGuid` from it. A supplied Connection Type already puts the Hub in single-type mode, which drives `selectedConnectionTypeOptions` and filters the modal's opportunity list. Both preselection cases fall out of state that exists; only the trigger is new.

## Capability Parity with Legacy Add Mode

This closes the parent task's first open verification item.

The legacy add path is `ConnectionRequestDetail.ascx.cs:2038`. Given `ConnectionRequestId=0` and a `ConnectionOpportunityId`, it builds an unsaved request seeded with the opportunity, the type's default status, `ConnectionState.Active`, and the campus from the block's own person preference, then auto-expands the person picker.

The Hub modal collects requester, opportunity, request source, state, follow-up date, campus, connector, status, placement group with role, member status and member attributes, comments, and the request's own attributes. Status defaults through `getDefaultStatusGuid`, state and campus are seeded the same way the Add button seeds them, and the opportunity arrives from the page parameter. Every field the legacy add form wrote has a home, so parity holds. The one cosmetic difference is the auto-expanding person picker, which the modal does not need since the requester field is the first control in it.

## Test Plan

A parent row is checked only once every nested row under it passes.

### Add indicator

- [x] `{hub route}?Request=0&ConnectionType={IdKey}`, with a saved opportunity filter for that type, opens the modal with that opportunity preselected.
- [x] `{hub route}?Request=0&ConnectionType={IdKey}`, with no saved opportunity filter, opens the modal with the Opportunity dropdown empty and listing only that type's opportunities.
- [x] `{hub route}?Request=0&ConnectionOpportunity={IdKey}` opens the modal on that opportunity.
- [x] Both parameters together open the modal on that type and that opportunity.
- [x] A request saved from a deep-linked modal lands with the same values as one saved from the Add button.
- [x] An individual without edit rights on the Connection Type sees the ordinary grid and no modal.
- [x] `Request=0` from the My Connections page, where `IsMyConnectionsView=true` and a `Connector` is supplied, opens the modal unfiltered.

### View deep link

- [x] `Request={IdKey}` together with a `ConnectionType` or `ConnectionOpportunity` behaves exactly as it does today, on a site with predictable ids enabled and on one with them disabled.

### No regressions

- [x] The Hub with no `Request` parameter behaves exactly as before, from every existing entry point.
- [x] The Add button still opens the modal unfiltered.
- [x] Add From Campaign is unaffected.
- [x] The Connection Opportunity Navigation and Connection Type Navigation blocks still land on the Hub correctly.

## Out of Scope

Phase 3, the link redirection sweep:

- `RockWeb/Blocks/Connection/MyConnectionOpportunities.ascx.cs:1326`, both the add link at 548 and the view link at 558, plus the `UseConnectionRequestDetailPageFromConnectionType` branch.
- Both tracked `MyConnectionOpportunitiesSortable.lava` copies, line 53 in each: `RockWeb/Themes/RockManager` and `Rock.Frontend.Styles/src/themes/RockNextGen`. The `dist` and `RockWeb/Themes/RockNextGen` copies are build output and are gitignored.
- `Rock.Blocks/Connection/CelebrationsReport.cs:201` and its `LinkedPage` default at line 51. Its rows carry only `connectionRequestIdKey`, so the grid gains an opportunity IdKey field to satisfy the second-identifier requirement.
- `Rock.Blocks/Crm/PersonDetail/ConnectionRequests.cs:221` and its per-type branch.
- `RockWeb/Blocks/Connection/MyConnectionOpportunitiesLava.ascx.cs:44` and `RockWeb/Blocks/Connection/WebConnectionRequestListLava.ascx.cs:65`, both defaulted to `Rock.SystemGuid.Page.CONNECTION_REQUEST_DETAIL`.
- The query string rename that goes with all of the above: `ConnectionRequestId` becomes `Request`, and `ConnectionOpportunityId` becomes `ConnectionOpportunity`.
- A migration updating the two existing `AttributeValue` rows that point at the Connection Request Detail page.

Two Phase 3 questions worth raising early, since neither is settled by the parent task:

- `WebConnectionRequestListLava` is categorized "Connection > WebView" and runs on external sites. Repointing it at the internal Hub would send external visitors to a page they cannot reach. The parent task says the sweep has no exceptions; this one needs a decision before it is rewritten.
- What happens to the `UseConnectionRequestDetailPageFromConnectionType` branches once core links point at the Hub. Partner-set per-type pages still win there, so those partners keep landing on Connection Request Detail.

Also out of scope, unchanged from Phase 1: any Obsidian conversion of My Connection Opportunities or Connection Request Detail, and the mobile blocks under `Rock.Blocks/Mobile/Connection/`, which have their own detail block.

## Considered but Rejected

### Invent a separate add parameter, such as `IsAddRequested=true`
Rejected. The legacy detail page has meant "0 is add" since it shipped, and the Hub already owns a `Request` parameter. Reusing `Request=0` keeps one convention instead of two, which is what the parent task decided.

### Derive the Connection Type from the request when a link carries only `Request`
Rejected. It would let a view link be one parameter, but it moves work into `GetOptions`, which every existing entry point runs through. Requiring the second identifier keeps the Hub untouched on that path and costs one grid field in Celebrations Report, the only caller that does not already have an opportunity in hand.

### Parse the query string in `connectionsHub.obs` instead of the block
Rejected. Rock resolves page parameters server-side so they honor the site's predictable-ids setting and the individual's authorization. The Hub already does this for every other parameter; a client-side read would bypass both.

### Ship the Hub change and the link sweep together
Rejected. Phase 3 is a repo-wide rewrite plus a migration, and it depends on this work being correct. Splitting gives the entry point its own QA pass first, and keeps the migration reviewable on its own.

## Related

- [Asana DEV-15156 (parent task)](https://app.asana.com/1/20866866924293/project/1208321217019996/task/1213724908502707), read 2026-08-25 and treated as canonical for the decisions above. Its "Open verification items" section is answered in Capability Parity and Out of Scope.
- Asana DEV-15159, "DA: Connection Request Add Behavior Change", is the subtask this phase implements.
- The parent task links a private artifact page with the before and after tree. Not verifiable from here; every code fact above is taken from the repository directly.
- `specs/completed/connection/260825-connection-page-tree-cleanup.md` is Phase 1, shipped in `7d5c5bd4e9`.
- `specs/completed/connection/260622-connection-request-entry.md` is the public-facing Connection Request Entry block. Different front door, different audience; it is not affected by this phase.
