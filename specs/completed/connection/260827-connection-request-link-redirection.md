---
author: Jason Hendee
date_created: 2026-08-27
summary: >-
  Phase 3 of the Connections page cleanup: repoint every core link that targets
  the Connection Request Detail page at the Connections Hub, renaming the query
  string parameters and switching the identifiers to IdKeys. Covers five code
  sites, two Lava templates, four LinkedPage defaults, and a migration that
  updates the shipped block attribute values.
contributors: []
---

# Connection Request Link Redirection (Phase 3 of Connection Page/Block Cleanup)

## Summary

DEV-15156 decided the Connections Hub is the single front door for Connection Requests. Phase 1 flattened the page tree, Phase 2 taught the Hub to open its add modal from a link. Phase 3 rewrites the links themselves.

Every core site that targets the Connection Request Detail page moves to the Hub. The page itself is untouched: it keeps its Id, Guid, routes, and block, exactly as Phase 1 arranged.

## Motivation

The task states the consequence plainly, and it is worth restating here. Once this lands, nothing in core navigates to the Connection Request Detail page. It stays reachable through the per-type reference on `ConnectionType` and through any link a partner built themselves. The page becomes partner-only in practice, which is precisely why Phase 1 moved it rather than deleting it.

## Current State

Verified against the repository on 2026-08-27. Every site below currently targets the Connection Request Detail page.

| Site | Kind | Passes today |
|---|---|---|
| `RockWeb/Blocks/Connection/MyConnectionOpportunities.ascx.cs:548` | Add | `ConnectionRequestId=0`, `ConnectionOpportunityId={Id}` |
| `RockWeb/Blocks/Connection/MyConnectionOpportunities.ascx.cs:558` | View | `ConnectionRequestId={Id}`, `ConnectionOpportunityId={Id}` |
| `RockWeb/Themes/RockManager/Assets/Lava/MyConnectionOpportunitiesSortable.lava:53` | View | `ConnectionRequestId={{ connectionRequest.Id }}`, `ConnectionOpportunityId` |
| `Rock.Frontend.Styles/src/themes/RockNextGen/Assets/Lava/MyConnectionOpportunitiesSortable.lava:53` | View | Same as above |
| `Rock.Blocks/Connection/CelebrationsReport.cs:201` | View | `ConnectionRequestId=((Key))`, no opportunity |
| `Rock.Blocks/Crm/PersonDetail/ConnectionRequests.cs:225` | View | `ConnectionRequestId={IdKey}`, `ConnectionOpportunityId={IdKey}` |
| `RockWeb/Blocks/Connection/WebConnectionRequestListLava.ascx.cs:112` | View | `ConnectionRequestId={{ connectionRequest.Id }}`, `ConnectionOpportunityId` |

Four `LinkedPage` attributes default to `Rock.SystemGuid.Page.CONNECTION_REQUEST_DETAIL`: `CelebrationsReport.cs:51`, `MyConnectionOpportunitiesLava.ascx.cs:44`, `WebConnectionRequestListLava.ascx.cs:65`, and the WebView block type attribute seeded by `Rock.Migrations/Migrations/Version 13.0/Version 1.13.1/202201190043546_CodeGenerated_20220118.cs:575`. `MyConnectionOpportunities.ascx.cs:56` and `Crm/PersonDetail/ConnectionRequests.cs:53` declare theirs with no default.

Phase 1 verified against a live database that exactly two `AttributeValue` rows point at the Connection Request Detail page: the My Connection Opportunities block's `DetailPage`, and the Person Profile Connection Requests block's `ConnectionRequestDetail`.

Two of the four `MyConnectionOpportunitiesSortable.lava` copies on disk are build output and gitignored (`Rock.Frontend.Styles/dist/...` and `RockWeb/Themes/RockNextGen/...`). Only the two tracked sources in the table get edited.

## Requirements

- Every core link that targets the Connection Request Detail page MUST target `Rock.SystemGuid.Page.CONNECTIONS_HUB` instead.
- Each site MUST be confirmed as a view link or an add link before it is rewritten.
- A view link MUST pass `Request={IdKey}` plus a `ConnectionType` or `ConnectionOpportunity`, which the Hub requires for its type context.
- An add link MUST pass `Request=0` plus the type or opportunity the caller knows.
- Identifiers MUST be IdKeys, not integer Ids, so the links work on sites with predictable ids disabled.
- The query string names change with the target: `ConnectionRequestId` becomes `Request`, and `ConnectionOpportunityId` becomes `ConnectionOpportunity`.
- `LinkedPage` defaults MUST change from `CONNECTION_REQUEST_DETAIL` to `CONNECTIONS_HUB`.
- A migration MUST update the shipped block attribute values. It MUST rewrite an `AttributeValue` row only where the row still holds the shipped default, so a partner-customized value survives.
- The `UseConnectionRequestDetailPageFromConnectionType` branches MUST keep their current behavior, so a partner-set per-type page still wins.
- The Connection Request Detail block MUST accept `Request` and `ConnectionOpportunity` alongside its existing parameter names, so one link shape serves both pages and no caller has to know which page it is pointing at.
- The Connection Request Detail page, its block, its Id, its Guid, and its routes MUST NOT change.

## Design

### 1. My Connection Opportunities (WebForms)

`NavigateToConnectionPage` at `MyConnectionOpportunities.ascx.cs:1326` serves both the Add button (line 548, passing `0`) and the row Edit (line 558). It builds the query string itself, so this is a code edit rather than an attribute change.

The integer parameters become IdKeys through `IdHasher.Instance.GetHash`, and the add case keeps passing `0` verbatim since that is the indicator rather than an identifier. `AsIdKey()` is the shorter form elsewhere, but it is `internal` to the Rock assembly (`IntegerExtensions.cs:258`) and RockWeb is not in the `InternalsVisibleTo` list, so this site calls the hasher directly:

```csharp
var requestParameter = connectionRequestId > 0
    ? IdHasher.Instance.GetHash( connectionRequestId )
    : "0";

var pageParameters = new Dictionary<string, string>
{
    { "Request", requestParameter },
    { "ConnectionOpportunity", IdHasher.Instance.GetHash( SelectedOpportunityId.Value ) }
};
```

The `UseConnectionRequestDetailPageFromConnectionType` branch at line 1329 keeps its existing shape and continues to win when a partner has set a per-type page. Both branches emit the same parameters.

### 2. The two Lava templates

Line 53 in each becomes:

```liquid
<a href="{{ LinkedPages.DetailPage }}?Request={{ connectionRequest.IdKey }}&ConnectionOpportunity={{ connectionRequest.ConnectionOpportunity.IdKey }}">
```

`IdKey` is a `[DataMember]` property on `Entity<T>` and is not Lava-hidden, so it renders like any other entity property. The dist and theme build-output copies are regenerated from these.

### 3. Celebrations Report

`GetBoxNavigationUrls` at `CelebrationsReport.cs:201` builds a template URL with a single `((Key))` placeholder and passes no opportunity, so it cannot satisfy the second-identifier requirement as written. Its grid rows already carry `connectionRequestIdKey` (line 296) and its query already includes `cr.ConnectionOpportunity` (line 265), so the row builder gains an opportunity IdKey field and the client substitutes both values.

### 4. Person Profile Connection Requests

`GetDetailUrl` at `Crm/PersonDetail/ConnectionRequests.cs:221` already passes both identifiers as IdKeys through the entities' own `IdKey` properties, so no conversion is needed. Only the key names and the linked page change. Its `UseConnectionRequestDetailPageFromConnectionType` branch at line 229 follows section 5. `GetDetailUrl` runs per row, so the block type lookup is resolved once and reused.

### 5. The legacy detail page accepts both parameter names

Every rewritten link emits `Request` and `ConnectionOpportunity`, with no branching on the target. The Connection Request Detail block reads the new names first and falls back to its own, so a link built for the Hub resolves there too:

```csharp
private string GetConnectionRequestPageParameterValue()
{
    var value = PageParameter( PageParameterKey.Request );

    return value.IsNotNullOrWhiteSpace() ? value : PageParameter( PageParameterKey.ConnectionRequestId );
}
```

`GetConnectionRequestIdPageParameter` and `GetConnectionOpportunityIdPageParameter` read through those helpers, as do the three places the block re-navigates to itself carrying its own parameters forward. Both names keep working, so a partner whose per-type page or block setting still points at the detail page is unaffected, and callers need no knowledge of where they are pointing.

The block already accepted an IdKey in either parameter, so switching the callers to IdKeys needs nothing further here.

### 6. Connection Request List (WebView)

`WebConnectionRequestListLava.ascx.cs` needs the same rewrite in its default Lava template at line 112 and its `LinkedPage` default at line 65. This block runs on external sites. See Open Questions before rewriting it.

### 7. Migration

An EF migration in `Rock.Migrations/Migrations/` that:

- Moves the detail page `AttributeValue` rows for My Connection Opportunities (`DetailPage`), My Connection Opportunities Lava (`DetailPage`), Person Profile Connection Requests (`ConnectionRequestDetail`), and Celebrations Report (`ConnectionRequestDetailPage`) to the Connections Hub page Guid, but only where the row still holds the Connection Request Detail page Guid. A row a partner has pointed somewhere else is left alone.
- Reverses that in `Down()`, under the same equality guard.

The last two blocks change their default in code as well, but saving block settings writes a value even when it matches the default, so an explicit row can still be pinning the old page. No `Attribute` default is touched: `Rock/Attribute/Helper.cs:349` syncs `DefaultValue` from the code declaration on the next block type scan.

## Known Consequences

Every link now carries the Hub's parameter names. The Connections Hub and the Connection Request Detail page both read them, so the only target that cannot is a page built by an organization around their own block. Such a link renders empty rather than erroring, and that organization can add the new names to their block or their theme's copy of the Lava template. Accepted rather than solved.

## Open Questions

**Question for PO: the external Connection Request List block.** `WebConnectionRequestListLava` is categorized "Connection > WebView" and runs on external site pages, where a visitor cannot reach an internal admin page. The task says the sweep has no exceptions, so this needs an answer before the block is rewritten.

The recommendation is to leave it pointed at the Connection Request Detail page. Rock registers the block type but seeds no page for it, so every organization running it placed it deliberately. Any of them that never overrode the Detail Page setting has no `AttributeValue` row at all and resolves the `Attribute` default instead, which means changing that default silently repoints their live external-site links at an internal page. The shipped-default guard on the migration cannot protect them, because there is no row to guard.

## Test Plan

A parent row is checked only once every nested row under it passes.

### My Connection Opportunities

- [x] The grid's Add button opens the Connections Hub with the add modal open, on the selected opportunity.
- [x] Clicking a request row opens the Hub with that request's docked panel open.
- [x] Both work on a site with predictable ids disabled.
- [x] With a per-type detail page set on the Connection Type, both still route to that page and the request opens there.

### Lava templates

- [x] The RockNextGen theme's sortable template links to the Hub with both IdKeys present in the query string, and opens the docked panel for that request.

The RockManager copy is not verifiable. That theme belongs to the Check-in Manager site, which moved to RockManagerNextGen in v19, and RockManagerNextGen does not carry this file. It is edited to match the task's instruction, not because a page renders it.

### Celebrations Report

- [x] A celebration row links to the Hub and opens the right request.
- [x] The link carries the opportunity, so the Hub renders its grid rather than an error.

### Person Profile

- [x] The Connection Requests badge or list links through to the Hub and opens the right request.
- [x] With a per-type detail page set, it still routes to that page and the request opens there.

### Migration

- [x] `Up()` runs against a database at the current develop schema and completes without error.
- [x] All four detail page attribute values point at the Connections Hub afterward.
- [x] `Down()` restores the Connection Request Detail page in all relevant Attributes (default values) and AttributeValues (values).
- [x] An `AttributeValue` row pointed at some other page before the migration still points there afterward.

### Nothing else moved

- [x] The Connection Request Detail page still exists with Guid `50F04E77-8D3B-4268-80AB-BC15DD6CB262` and still loads by url.
- [x] `ConnectionType.ConnectionRequestDetailPageId`, where set, still resolves.

## Out of Scope

- Deleting the Connection Request Detail page or its block. Phase 1 kept both deliberately.
- Any Obsidian conversion of My Connection Opportunities or Connection Request Detail.
- Mobile blocks under `Rock.Blocks/Mobile/Connection/`, which have their own detail block.
- Reparenting Connection Campaigns out from under Connection Types.

## Considered but Rejected

### Derive the Connection Type from the request inside the Hub
Rejected in Phase 2. It would let a view link be one parameter, but it moves work into `GetOptions`, which every existing entry point runs through. Requiring the second identifier keeps the Hub untouched and costs one grid field in Celebrations Report.

### Edit all four `MyConnectionOpportunitiesSortable.lava` copies
Rejected. Two of them are gitignored build output regenerated from the tracked sources. Editing them would be overwritten on the next build.

### Detect whether the target page hosts a Connections Hub block and pick the parameter names from that
Rejected. It works, and Rock has the pattern (`FormBuilderDetail.cs:774`), but it puts a page lookup behind every link, only covers the two C# callers since a Lava template cannot run the check, and leaves each caller reasoning about where it points. Teaching the one other expected page to read both names is less code and covers the Lava path for free.

### Pass ConnectionType instead of ConnectionOpportunity on view links
Rejected. The Hub persists a supplied `ConnectionOpportunity` as the individual's saved opportunity filter, so a view deep link updates that preference as a side effect. Passing the type would avoid the write, but the task does not ask for it and the impact is minor. If it ever matters, the lesser change is Hub-side, skipping preference writes whenever `Request` is supplied, which is a question for the PO.

### Keep passing integer Ids and rely on the site's predictable-ids setting
Rejected. The task specifies IdKeys, and an integer link silently fails to resolve on any site with predictable ids disabled.

## Related

- [Asana DEV-15156 (parent task)](https://app.asana.com/1/20866866924293/project/1208321217019996/task/1213724908502707), canonical for the decisions above. Its "Known reference sites for the sweep" list is the basis for Current State, expanded with the sites confirmed in the repository.
- `specs/completed/connection/260825-connection-page-tree-cleanup.md` is Phase 1, shipped in `7d5c5bd4e9`.
- `specs/260825-connections-hub-add-entry-point.md` is Phase 2, which this phase depends on for the `Request=0` indicator.
