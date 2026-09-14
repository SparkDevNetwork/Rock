---
author: Kyle Henning
date_created: 2026-09-14
summary: >-
  The Connections Hub docked panel's Additional Requests list comes back in arbitrary
  (in practice oldest-first) order because the server unions the per-setting queries
  and never applies an OrderBy. Fix by ordering the combined query by CreatedDateTime
  descending, then Id descending, before projection.
contributors: []
related_docs:
  - docs/connection/request-board.md
---

# Connections Hub: Additional Requests List Is Unordered

## Summary

The docked panel in the Connections Hub shows an Additional Requests strip above the request details, listing other Connection Requests for the same person (and optionally their family) according to the Connection Type's "Additional Requests to Show" settings. Only the first five render; the rest sit behind a "+N" expander. The server builds the list by unioning one query per configured setting and projecting the result with no `OrderBy`, so SQL Server returns rows in whatever order the plan produces, which in practice is oldest first. A request from 2015 shows above requests created minutes ago and the newest one is hidden behind the expander. The fix is a two-key sort on the combined query, newest first.

## Problem Statement

`GetAdditionalConnectionRequests` in `Rock.Blocks/Engagement/ConnectionsHub.cs:3536` never orders its result. Because the five-row cutoff is applied on the client, an unordered list is not just cosmetic: it decides which requests are visible at a glance and which are hidden.

## Reproduction

From [Issue #7018](https://github.com/SparkDevNetwork/Rock/issues/7018), confirmed on `develop` as of 2026-09-03 and reported against 19.3.4:

1. Create a second Connection Type with a status, an activity type, and an opportunity.
2. On the first Connection Type, edit Additional Settings and add an "Additional Requests to Show" entry that points at the second type, with Inactive states enabled.
3. For one person, create one request in the second type and six requests in the first type, spaced a few minutes apart.
4. Open one of the person's requests in the Connections Hub.
5. The Additional Requests strip lists the oldest request first and the most recent request is behind the "+1" expander.

Affected in every version that ships the Connections Hub docked panel. The method arrived in `2e02956397` (2026-03-09), first tagged in the 19.0 pre-releases.

## Root Cause

`GetAdditionalConnectionRequests` (`ConnectionsHub.cs:3536-3606`):

1. Loops over `settingsList`, building one `IQueryable<ConnectionRequest>` per setting (`:3551-3576`).
2. Combines them with `combinedQuery.Union( query )` (`:3578-3580`).
3. Projects `combinedQuery.Select( r => new { ... } ).ToList()` (`:3583-3595`) and maps to `AdditionalRequestBag`.

No `OrderBy` appears anywhere in the chain. `UNION` carries no ordering guarantee, and EF6 emits no `ORDER BY` unless asked, so the row order is whatever SQL Server's plan yields. The client (`Rock.JavaScript.Obsidian.Blocks/src/Engagement/ConnectionsHub/viewDockedPanelBody.partial.obs:59` and `:77`) renders `additionalRequests.slice(0, 5)` and `slice(5)` verbatim, with no sort of its own.

## Affected Code Paths

Primary:

- `Rock.Blocks/Engagement/ConnectionsHub.cs:3536` `GetAdditionalConnectionRequests`, the only place the list is built.

Secondary (consumers, no change needed):

- `ConnectionsHub.cs:3512` in `GetConnectionRequestDetailsBag`, which assigns the result to `ConnectionRequestDetailsBag.AdditionalRequests`.
- `viewDockedPanelBody.partial.obs:41-92`, the Additional Requests strip, which slices the array at five.

## Workarounds

None practical. Expanding the "+N" control shows every row, but still unordered. Tightening "Limit to Recent Requests (Days)" on the setting reduces the list so nothing is hidden, at the cost of dropping older requests the setting was meant to show.

## Proposed Fix

Order the combined query before projection, newest first with `Id` as a tiebreaker so batch-created requests sharing a timestamp still order deterministically:

```csharp
var additionalRequestsProjection = combinedQuery
    .OrderByDescending( r => r.CreatedDateTime )
    .ThenByDescending( r => r.Id )
    .Select( r => new
    {
        RequestId = r.Id,
        // existing projection unchanged
    } )
    .ToList();
```

EF6 wraps the `UNION` in a derived table and applies the `ORDER BY` to the outer select, so the sort is done in SQL and the in-memory projection that follows preserves it. The bag already carries `RequestCreatedDateTime`, so no view model or client change is needed.

Update the method's `<remarks>` (`:3529-3532`) and `<returns>` (`:3535`) to state that results are ordered by created date descending, then Id descending, matching how the sibling `GetActivityEntries` documents its ordering.

## Fix Risks

- **Query shape.** Adding `ORDER BY` over a `UNION` of up to N per-setting queries is cheap relative to the joins already present (`ConnectionOpportunity.ConnectionType`, `PersonAlias.Person`, `ConnectorPersonAlias.Person`). No new indexes are warranted; `CreatedDateTime` is not indexed on `ConnectionRequest`, but the filtered row set per person is small.
- **Null `CreatedDateTime`.** Legacy or imported rows with a null created date sort last under `DESC` in SQL Server. That is the desired behavior for "newest first".
- **Behavior change visible to users.** Sites that have grown used to the current oldest-first order will see it flip. This is the reported expectation and matches the Activity Feed, so no toggle is proposed.
- **Blast radius.** Single private-ish method (it is declared `public`, see Out of Scope), one call site, no signature change.

## Verification Steps

1. Follow the Reproduction steps and open the person's request in the Connections Hub.
2. Confirm the Additional Requests strip lists the six visible requests newest first and the oldest request (not the newest) is behind the "+1" expander.
3. Expand the strip and confirm the hidden rows continue the descending order.
4. Create two requests through a bulk path so they share a `CreatedDateTime` (for example Bulk Update or a SQL insert with identical timestamps) and confirm the one with the higher `Id` lists first.
5. Add a second "Additional Requests to Show" setting for a third Connection Type and confirm requests from both settings interleave by date rather than grouping by setting.
6. Enable "Include Family Member Requests" on a setting and confirm family members' requests interleave by date with the requester's own.
7. Build `Rock.sln`; no client build is required.

## Out of Scope

- Making the five-row cutoff configurable or moving the slice to the server.
- Ordering options (oldest first, by status) on the "Additional Requests to Show" setting.
- Changing the accessibility of `GetAdditionalConnectionRequests`. It is `public` on the block type today, which is wider than needed, but narrowing it is an unrelated cleanup that could affect plugins.
- The client-side sort of the Activity Feed, which is already ordered.

## Related

- [Issue #7018](https://github.com/SparkDevNetwork/Rock/issues/7018), Connections Hub: additional requests list is returned unordered. Read 2026-09-14; the proposed fix here matches the issue's expected behavior (CreatedDateTime descending, Id descending).
- Introducing commit `2e02956397` (2026-03-09), "WIP: Polish and additional request logic."
- `docs/connection/request-board.md` describes the docked panel this list belongs to.
