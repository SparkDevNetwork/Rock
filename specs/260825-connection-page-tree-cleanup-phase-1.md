---
author: Jason Hendee
date_created: 2026-08-25
summary: >-
  Phase 1 of the Connections page cleanup: collapse the duplicate Connections
  page tree into a single flat hierarchy under the Connections root and remove
  the Add Campaign Requests and Bulk Update Requests WebForms blocks. Page
  structure and block removal only; the Connections Hub add entry point and the
  link redirection sweep are deferred to later phases.
contributors: []
---

# Connection Page Tree Cleanup (Phase 1)

## Summary

A second generation of Connections pages shipped without the first being retired, leaving administrators with two pages both named "Connections," one nested inside the other, and a configuration branch sitting above the pages staff actually use. This spec flattens that tree into a single level under the Connections root and removes two WebForms blocks whose capability is now covered elsewhere.

Phase 1 is page structure and block removal only. It touches no C# block logic, no Obsidian components, and no link targets. Every page that survives keeps its Id, Guid, and routes, so partner configuration pointing at those pages stays valid.

## Motivation

The duplicate middle layer exists only as a container. It pushes Connection Types, Connection Request Detail, and Connection Requests Bulk Update to depth 2 for no functional reason, and it puts a second page named "Connections" directly under the first, which makes the admin page tree ambiguous to read. Two blocks parked in that subtree are dead weight: Add Campaign Requests duplicates the Connections Hub's add-from-campaign modal, and Bulk Update Requests was launched from the Connection Request Board, which was already deleted in `Rock.Migrations/Migrations/202605202132541_Rollup_20260520.cs:788`.

Splitting this off from the parent task keeps the migration reviewable on its own. The parent task bundles the page work with a behavior change to how Connection Requests are created, which carries open design questions that should not block a structural cleanup.

## Current State

Verified against `Rock_develop` on 2026-08-25. Pages are outdented, block instances indented beneath the page they sit on. Every guid is the full value; `order` is the `[Order]` column on the page.

```
Connections                                        2A0C135A-8421-4125-A484-83C8B4FB3D34  order 14
    Connection Type Navigation                     340FBA54-FC54-4EA1-8DD2-301536405034
├── Connections                                    530860ED-BC73-4A43-8E7C-69533EF2B6AD  order 7    [DELETE]
│       Add Campaign Requests                      BF39BE49-B4F6-4A5B-BDA2-EB343FC80CCA             [DELETE]
│       My Connection Opportunities                80710A2C-9B90-40AE-B887-B885AAA43538             [MOVE]
│   ├── Connection Request Detail                  50F04E77-8D3B-4268-80AB-BC15DD6CB262  order 0    [MOVE]
│   │       Connection Request Detail              94187C5A-7F6A-4D45-B5C2-C3C8673E8817
│   ├── Connection Types                           9CC19684-7AD2-4D4E-A7C4-10DAE56E7FA6  order 1    [MOVE]
│   │       Connection Type List                   C3333691-9476-4DF6-A07C-C985857EB976
│   │   ├── Connection Type Detail                 DEFF1AFE-2C33-4E56-B0F5-BE3B75224186
│   │   │       Connection Type Detail             0D66ADEF-07B2-4F23-8AF3-9D6B6420CEA4
│   │   │       Connection Opportunity List        5A078DC0-9E85-4429-BC72-29003B81D8B5
│   │   │   └── Connection Opportunity Detail      0E5797FF-A507-4E02-891F-B80AF353E585
│   │   │           Connection Opportunity Detail  D9C657FB-1426-44FA-9B1E-88CECDB52A7F
│   │   └── Connection Campaigns                   B252FAA6-0E9D-41CD-A00D-E7159E881714
│   │           Campaign List                      3A62AD36-5031-4C62-BCC1-7800AE43F78B
│   │       └── Campaign Configuration             A22133B5-B5C6-455A-A300-690F7926356D
│   │               Campaign Configuration         5DC2943E-EFBD-4F25-B1D7-738CB86AB628
│   └── Connection Requests Bulk Update            1F5D34CF-89C1-426C-A139-83D87905D669  order 2    [DELETE]
│           Connection Requests Bulk Update        86249F40-DAA9-46AC-901B-7460E2659C10             [DELETE]
├── Connections Hub                                8B5F2875-0D36-4625-8EE4-B738AE8E12F5  order 8
│       Connections Hub                            1422636F-548F-4F50-BF2A-D494FB936A5C
├── Operational Snapshot                           3421FD03-018F-457D-A0B6-9326C5D5A5F4  order 9
│       Connection Operational Snapshot            BAD25336-28FF-4012-8078-C9C34C62FE7F
├── Connections Opportunities                      F8B0E0CE-76A3-4449-B4EB-28DD9A42D71F  order 10
│       Connection Opportunity Navigation          D5130BD5-92A1-4904-ACEB-5CC6D9E8CDA5
├── My Connections                                 3E55BE64-C8E8-487B-9BD4-E94C7F99BE1B  order 11   [RENAME]
│       Connections Hub                            54628553-464A-478E-B86B-73B2CFDB29B2
└── Celebrations Report                            E59810B6-5225-4CF6-A239-F2757A4369B1  order 12
        Connection Celebrations Report             32AD2827-E829-4650-95C3-1085B7AFF54B
```

14 pages, 16 block instances, nesting five levels deep, two pages named "Connections."

## Target State

```
Connections                                    2A0C135A-8421-4125-A484-83C8B4FB3D34
    Connection Type Navigation                 340FBA54-FC54-4EA1-8DD2-301536405034
├── Connections Hub                            8B5F2875-0D36-4625-8EE4-B738AE8E12F5  order 0
│       Connections Hub                        1422636F-548F-4F50-BF2A-D494FB936A5C
├── Operational Snapshot                       3421FD03-018F-457D-A0B6-9326C5D5A5F4  order 1
│       Connection Operational Snapshot        BAD25336-28FF-4012-8078-C9C34C62FE7F
├── Connections Opportunities                  F8B0E0CE-76A3-4449-B4EB-28DD9A42D71F  order 2
│       Connection Opportunity Navigation      D5130BD5-92A1-4904-ACEB-5CC6D9E8CDA5
├── My Connection Requests                     3E55BE64-C8E8-487B-9BD4-E94C7F99BE1B  order 3
│       Connections Hub                        54628553-464A-478E-B86B-73B2CFDB29B2
├── My Connection Opportunities                {NEW_PAGE_GUID}                       order 4
│       My Connection Opportunities            80710A2C-9B90-40AE-B887-B885AAA43538
├── Connection Types                           9CC19684-7AD2-4D4E-A7C4-10DAE56E7FA6  order 5
│       Connection Type List                   C3333691-9476-4DF6-A07C-C985857EB976
│   ├── Connection Type Detail                 DEFF1AFE-2C33-4E56-B0F5-BE3B75224186
│   │       Connection Type Detail             0D66ADEF-07B2-4F23-8AF3-9D6B6420CEA4
│   │       Connection Opportunity List        5A078DC0-9E85-4429-BC72-29003B81D8B5
│   │   └── Connection Opportunity Detail      0E5797FF-A507-4E02-891F-B80AF353E585
│   │           Connection Opportunity Detail  D9C657FB-1426-44FA-9B1E-88CECDB52A7F
│   └── Connection Campaigns                   B252FAA6-0E9D-41CD-A00D-E7159E881714
│           Campaign List                      3A62AD36-5031-4C62-BCC1-7800AE43F78B
│       └── Campaign Configuration             A22133B5-B5C6-455A-A300-690F7926356D
│               Campaign Configuration         5DC2943E-EFBD-4F25-B1D7-738CB86AB628
├── Celebrations Report                        E59810B6-5225-4CF6-A239-F2757A4369B1  order 6
│       Connection Celebrations Report         32AD2827-E829-4650-95C3-1085B7AFF54B
└── Connection Request Detail                  50F04E77-8D3B-4268-80AB-BC15DD6CB262  order 7   (hidden from nav)
        Connection Request Detail              94187C5A-7F6A-4D45-B5C2-C3C8673E8817
```

13 pages, 14 block instances, four levels of nesting, one page named "Connections."

## Requirements

- The duplicate "Connections" page (`530860ED-BC73-4A43-8E7C-69533EF2B6AD`) MUST be deleted.
- The "Connection Requests Bulk Update" page (`1F5D34CF-89C1-426C-A139-83D87905D669`) and its block instance MUST be deleted.
- The "Add Campaign Requests" and "Connection Requests Bulk Update" block types MUST be deleted, along with the `PreviousPage` attribute owned by the latter.
- **Every** instance of those two block types MUST be deleted, not just the two Rock ships, since an organization may have placed their own. Each instance's `Auth` rows MUST go with it.
- The corresponding WebForms files MUST be removed from the repository.
- Connection Types and Connection Request Detail MUST be reparented to the Connections root, keeping their existing Id, Guid, routes, and child pages.
- The My Connection Opportunities block instance MUST move onto a new page rather than being recreated, so its configured attribute values survive.
- Connection Request Detail MUST be hidden from the navigation menu.
- "My Connections" MUST be renamed to "My Connection Requests" without changing its Id, Guid, route, or block.
- Sibling ordering under the Connections root MUST place the day-to-day staff pages first, then Connection Types, then Celebrations Report, with Connection Request Detail last.
- Exactly one page is created. Connection Request Detail already has its own page and MUST be reparented rather than recreated, so its Id and Guid survive.
- No page or block that partners may reference MUST change identity. Deletions are limited to the four records named above.

## Design

### Migration operations

An EF migration in `Rock.Migrations/Migrations/`, following the shape of `Rollup_20260520.cs`, which retired the Connection Request Board in the same domain. The migration file is scaffolded with `Add-Migration`; what follows describes the `Up()` and `Down()` bodies that go into it.

**1. Delete every instance of the two block types, then the block types.**

Deleting only the block instances Rock ships is not enough. An organization may have placed additional instances of either block on pages of their own, so the delete has to be driven off the block type, not off the two known block guids:

```sql
DECLARE @BlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '{blockTypeGuid}' OR [Path] = '{path}');
DECLARE @BlockEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block');

IF @BlockTypeId IS NOT NULL
BEGIN
    DELETE [Auth]
    WHERE [EntityTypeId] = @BlockEntityTypeId
        AND [EntityId] IN (SELECT [Id] FROM [Block] WHERE [BlockTypeId] = @BlockTypeId);

    DELETE [Block]
    WHERE [BlockTypeId] = @BlockTypeId;

    DELETE [BlockType]
    WHERE [Id] = @BlockTypeId;
END
```

`RemoveLegacyCheckInManagerLocationsBlockType.cs` and `Rollup_20260730.cs` both use this shape. The `Auth` delete is the addition: `FK_dbo.Block_dbo.BlockType_BlockTypeId` is `ON DELETE CASCADE`, so dropping the block type would silently take partner block instances with it, but `Auth.EntityId` is not a foreign key, so their security rows would be left orphaned pointing at dead block ids. `RockMigrationHelper.DeleteBlock` clears `Auth` for exactly this reason; a raw cascade does not.

The `PreviousPage` attribute is deleted **before** its block type, so `DeleteAttribute` takes the attribute's values with it. Deleting the block type first would cascade the blocks away and strand those values.

Both are WebForms block types with a populated `Path`, so `DeleteBlockType` would be the correct helper if a plain delete were enough. `UpdateBlockTypeByGuid` MUST NOT be used, per `.claude/rules/data-model.md`.

**2. Add the My Connection Opportunities page and move the existing block onto it.**

The page takes the Full Width layout (`D65F783D-87A9-4CC9-8110-E83466A0EADB`) to match its WebForms siblings. The block instance is moved by guid, which preserves its attribute values including the `DetailPage` setting that Phase 2 will rewrite.

A new constant is added to `Rock/SystemGuid/Page.cs` alongside the other pages in this subtree:

```csharp
/// <summary>
/// Gets the My Connection Opportunities page guid.
/// ParentPage: Connections
/// </summary>
public const string MY_CONNECTION_OPPORTUNITIES = "{NEW_PAGE_GUID}";
```

```csharp
RockMigrationHelper.AddPage( true, Rock.SystemGuid.Page.CONNECTION_OPPORTUNITY_SELECT, "D65F783D-87A9-4CC9-8110-E83466A0EADB", "My Connection Opportunities", "", Rock.SystemGuid.Page.MY_CONNECTION_OPPORTUNITIES, "" );
RockMigrationHelper.AddOrUpdatePageRoute( Rock.SystemGuid.Page.MY_CONNECTION_OPPORTUNITIES, "people/connections/my-opportunities", "{NEW_ROUTE_GUID}" );

Sql( $@"
    UPDATE  [Block]
    SET     [PageId] = ( SELECT [Id] FROM [Page] WHERE [Guid] = '{Rock.SystemGuid.Page.MY_CONNECTION_OPPORTUNITIES}' )
    WHERE   [Guid] = '80710A2C-9B90-40AE-B887-B885AAA43538';" );
```

Two guids need minting when the migration is authored: the page (`MY_CONNECTION_OPPORTUNITIES`) and its route. Both uppercase and hyphenated per `.claude/rules/data-model.md`.

`80710A2C-9B90-40AE-B887-B885AAA43538` is seeded in `Rock.Migrations/Migrations/CreateDatabase/201311251734059_CreateDatabase.sql`, so it is stable across every Rock install and safe to target by guid. There is no `MoveBlock` helper, so the reparent is raw SQL.

**3. Reparent Connection Types and Connection Request Detail, then delete the duplicate page.**

```csharp
RockMigrationHelper.MovePage( Rock.SystemGuid.Page.CONNECTION_TYPES, Rock.SystemGuid.Page.CONNECTION_OPPORTUNITY_SELECT );
RockMigrationHelper.MovePage( Rock.SystemGuid.Page.CONNECTION_REQUEST_DETAIL, Rock.SystemGuid.Page.CONNECTION_OPPORTUNITY_SELECT );

RockMigrationHelper.DeletePage( Rock.SystemGuid.Page.CONNECTION_REQUESTS_BULK_UPDATE );
RockMigrationHelper.DeletePage( Rock.SystemGuid.Page.CONNECTIONS );
```

Order matters. Both moves and the bulk update delete MUST run before the duplicate page is deleted, or the duplicate's remaining children are orphaned.

**4. Rename My Connections and hide Connection Request Detail from nav.**

```csharp
RockMigrationHelper.RenamePage( Rock.SystemGuid.Page.MY_CONNECTIONS, "My Connection Requests" );
```

`RenamePage` already updates `InternalName`, `PageTitle`, and `BrowserTitle` in one statement, so the breadcrumb and browser tab follow automatically and no extra SQL is needed.

`DisplayInNavWhen` on Connection Request Detail is currently `0` (WhenAllowed). It MUST become `2` (Never), since the page is a landing target rather than something staff browse to.

**5. Set sibling order.** Assign `[Order]` 0 through 7 under the Connections root per the target tree.

### Down migration

`Down()` reverses every group in reverse order: restore the "My Connections" name, re-add the duplicate Connections page and the Bulk Update page, move Connection Types and Connection Request Detail back beneath the duplicate, restore the original sibling orders, move the My Connection Opportunities block back, drop the new page, and re-add both legacy block types with their block instances.

The two block types **are** recreated, via `AddBlockType` plus `AddBlock`, matching the pattern `MoveCheckInConfigPagesAndBlocks.cs` used to restore the legacy Check-in Types block. A `Down()` is only ever run alongside reverting the corresponding repository changes, which puts the `.ascx` files back, so the restored paths resolve.

The `PreviousPage` attribute is recreated explicitly rather than left to block type registration. `BlockTypeService.RegisterBlockTypes` does re-register a missing WebForms block type and honors its `[Rock.SystemGuid.BlockTypeGuid]`, so the block types themselves would come back with the right identifiers on their own. But `PreviousPage` is declared as a `[LinkedPage]` with no explicit Guid, so the scan would mint a fresh one and strand `80783AF9-3C03-4DC7-BDFF-9940E6338DB8`, which a later re-run of `Up()` deletes by identifier. Block instances are never restored by the scan under any circumstances.

Not recoverable either way: any block attribute values an organization had set on the two deleted instances. Neither has any in a stock database.

### Files removed

- `RockWeb/Blocks/Connection/AddCampaignRequests.ascx`
- `RockWeb/Blocks/Connection/AddCampaignRequests.ascx.cs`
- `RockWeb/Blocks/Connection/BulkUpdateRequests.ascx`
- `RockWeb/Blocks/Connection/BulkUpdateRequests.ascx.cs`

Neither block has a `.ascx.designer.cs`, so that is four files, not six. RockWeb is an ASP.NET website project rather than a csproj, so there are no project file entries to remove; the files are compiled by their presence on disk.

One stale comment names a deleted block: `Rock/Utility/CampaignConnectionHelper.cs:493` explains an empty campus list in terms of what "the AddCampaignRequests block" allowed. Reword it to describe the condition rather than the caller.

## Capability Parity for the Removed Blocks

**Add Campaign Requests** is covered by the Connections Hub. `addFromCampaignModal.partial.obs` is reachable from the Hub's action menu ("Add From Campaign", `Rock.JavaScript.Obsidian.Blocks/src/Engagement/connectionsHub.obs:1291`), backed by `FetchConnectionCampaigns` (`Rock.Blocks/Engagement/ConnectionsHub.cs:5917`) and `AssignConnectionRequestsFromCampaign` (`Rock.Blocks/Engagement/ConnectionsHub.cs:5981`). Both paths call the same `CampaignConnectionHelper.AddConnectionRequestsForPerson`, apply the same `GetConnectorCampusIds` authorization check, and compute the same pending count and default daily limit. Parity holds.

**Bulk Update Requests** has no surviving caller. Its only entry point was the `BulkUpdateRequestsPage` attribute on the WebForms Connection Request Board block type (`28DBE708-E99B-4879-A64D-656C030D25B5`), and that block type was deleted in `Rock.Migrations/Migrations/202605202132541_Rollup_20260520.cs:788`. Its `PreviousPage` attribute default pointed at the Connection Board page, deleted in the same migration. The block is already unreachable in a current database.

## Blast Radius

Verified by query against `Rock_develop`:

- **No `AttributeValue` rows reference either deleted page** (`530860ED`, `1F5D34CF`), either deleted block type, or either deleted block instance. Nothing points at what is being removed.
- **Two `AttributeValue` rows reference the Connection Request Detail page** (`50f04e77-8d3b-4268-80ab-bc15dd6cb262`): the WebForms My Connection Opportunities block's `DetailPage`, and the Obsidian Person Profile Connection Requests block's `ConnectionRequestDetail`. `MovePage` preserves the page's Id and Guid, so both stay valid. Rewriting them is Phase 2 work.
- **Neither deleted page has a page route.** The `people/connections` route belongs to the Connections root (`2A0C135A`), not the duplicate (`530860ED`), so it is unaffected.
- **`ConnectionType.ConnectionRequestDetailPageId`** is null for every row in the local database. The per-type override exists in the schema and partners may use it; it survives the move for the same reason.
- **The duplicate Connections page and the My Connection Opportunities block are base seed data**, not local artifacts. `Rock.Migrations/Migrations/CreateDatabase/201311251734059_CreateDatabase.sql` seeds the block row with guid `80710a2c-9b90-40ae-b887-b885aaa43538` on the duplicate Connections page. Every Rock install therefore has this page and this block instance, so the migration behaves the same everywhere and can safely address both by guid. Note the file is UTF-16LE; a plain `grep` will not find the row.

## Discrepancies with the Parent Task

The Asana description states 13 pages and 15 block instances today, dropping to 12 and 13. The verified counts are 14 and 16, dropping to 13 and 14.

The difference is the **Celebrations Report** page (`E59810B6-5225-4CF6-A239-F2757A4369B1`) and its Connection Celebrations Report block, which the parent task's inventory and target tree both omit. That page was added by `Rock.Migrations/Migrations/202607280003172_AddCelebrationReportPage.cs` in July 2026, four months after the task was drafted. It stays, and this spec places it at order 6, below Connection Types and above Connection Request Detail.

The parent task also describes the Bulk Update Requests block as "already marked obsolete." That is not true at the code level: `RockWeb/Blocks/Connection/BulkUpdateRequests.ascx.cs` carries no `[Obsolete]` or `[RockObsolete]` attribute. It is obsolete in practice because its launcher was deleted, which is the argument this spec makes instead.

**Only one page is created, not two.** The parent task's Changes items 5 and 6 both say the block "gets its own page as a direct child of the Connections root page," and item 6 adds "Same operation as item 5." That reads as two new pages. It is not:

- **My Connection Opportunities** has no page of its own. Its block sits directly on the duplicate Connections page, which is being deleted, so a new page is genuinely needed.
- **Connection Request Detail** already has its own dedicated page (`50F04E77-8D3B-4268-80AB-BC15DD6CB262`), currently a child of the duplicate. It needs reparenting, not creating.

The task contradicts item 6 in two other places, both matching this spec. Its "Why the two legacy blocks are kept" section argues the page must be *moved* precisely so its Id and Guid survive for the `ConnectionType` per-type references, and its "Removal method" paragraph says "add the new My Connection Opportunities page ... reparent Connection Types and Connection Request Detail." Its own target tree lists Connection Request Detail once as an existing page, and its count of 12 pages only reconciles if exactly one page is new.

Taking item 6 literally would mint a new Id and Guid and orphan the `ConnectionType.ConnectionRequestDetailPageId` references the task keeps the page in order to protect. Item 6 is a wording error; the reparent is correct.

## Test Plan

A parent row is checked only once every nested row under it passes.

### Migration

- [x] `Up()` runs against a database at the current develop schema and completes without error.
- [x] `Up()` is idempotent enough to survive a re-run, or fails loudly rather than corrupting the tree.
- [x] `Down()` runs without error and returns the tree to a working five-level shape with no orphaned pages.
- [x] After `Down()`, both legacy block types exist again with their original guids, the `PreviousPage` attribute is back as `80783AF9-3C03-4DC7-BDFF-9940E6338DB8`, and both block instances sit on their original pages.
- [x] `Up()` runs cleanly a second time after a `Down()`, with no leftover orphan attribute.

### Page tree

- [x] In Admin Tools > CMS Configuration > Pages, the Connections subtree matches the Target State tree exactly: 13 pages, four levels deep, one page named "Connections."
- [x] The duplicate Connections page is gone.
- [x] The Connection Requests Bulk Update page is gone.
- [x] No page under Connections is orphaned.
- [x] Sibling order under the Connections root reads: Connections Hub, Operational Snapshot, Connections Opportunities, My Connection Requests, My Connection Opportunities, Connection Types, Celebrations Report, Connection Request Detail.
- [x] Connection Request Detail kept its original Guid `50F04E77-8D3B-4268-80AB-BC15DD6CB262`, confirming it was moved rather than recreated.
- [x] The renamed page shows "My Connection Requests" in the page tree, in the breadcrumb, and in the browser tab title.

### Pages still render

- [x] **All 12 surviving pages load with their block rendering.**
  - [x] Connections (root, Connection Type Navigation)
  - [x] Connections Hub
  - [x] Operational Snapshot
  - [x] Connections Opportunities
  - [x] My Connection Requests
  - [x] My Connection Opportunities (new page)
  - [x] Celebrations Report
  - [x] Connection Types
  - [x] Connection Type Detail
  - [x] Connection Opportunity Detail
  - [x] Connection Campaigns
  - [x] Campaign Configuration
  - [x] Connection Request Detail

### Routes still resolve

- [x] **Every route resolves to the right page.**
  - [x] `people/connections`
  - [x] `people/connections/hub`
  - [x] `people/connections/snapshot`
  - [x] `people/connections/opportunities`
  - [x] `people/connections/my-connections`
  - [x] `people/connections/my-opportunities` (new)
  - [x] `people/connections/celebrations`
  - [x] `people/connections/types`
  - [x] `people/connections/types/{ConnectionTypeId}`
  - [x] `people/connections/types/{ConnectionTypeId}/opportunity/{ConnectionOpportunityId}`
  - [x] `people/connections/campaigns`
  - [x] `people/connections/campaigns/{ConnectionCampaignGuid}`

### Moved block kept its configuration

- [x] The My Connection Opportunities block sits on its new page, not recreated: its Detail Page attribute still points at Connection Request Detail.
- [x] Clicking a request from My Connection Opportunities still lands on Connection Request Detail.
- [x] Any other configured attribute values on that block instance survived the move.

### Navigation

- [x] The Connections navigation menu does not list Connection Request Detail.
- [x] Connection Request Detail still loads when reached directly by url.
- [x] The Person Profile Connection Requests block still links through to Connection Request Detail.

### Removed blocks

- [x] The block type list contains no "Add Campaign Requests."
- [x] The block type list contains no "Connection Requests Bulk Update."
- [x] The solution builds with the four WebForms files removed.

## Out of Scope

Deferred to later phases of DEV-15156:

- Making the Connections Hub an entry point for creating a Connection Request (the `Request=0` indicator, the `IsAddConnectionRequestRequested` option, and seeding the add modal from a type or opportunity).
- Repointing every core link that targets Connection Request Detail at the Connections Hub, including `MyConnectionOpportunities.ascx.cs`, the four `MyConnectionOpportunitiesSortable.lava` copies, `CelebrationsReport.cs`, `Crm/PersonDetail/ConnectionRequests.cs`, and `WebConnectionRequestListLava.ascx.cs`.
- Deciding what happens to the `UseConnectionRequestDetailPageFromConnectionType` branches.
- Any Obsidian conversion of the My Connection Opportunities or Connection Request Detail blocks. Both stay WebForms and stay functional.
- Reparenting Connection Campaigns out from under Connection Types. Deliberately left alone: optional cleanup, no functional payoff, one fewer page move.
- Mobile blocks under `Rock.Blocks/Mobile/Connection/`, which have their own detail block.

## Decisions

Settled during authoring; recorded so the migration does not relitigate them.

- **Celebrations Report sorts at order 6**, below Connection Types and above Connection Request Detail. It is a report rather than a page staff work out of every day, so it sits below the configuration branch. Connection Request Detail stays last per the parent task, which makes Celebrations Report the last entry that appears in the navigation menu.
- **The new page's route is `people/connections/my-opportunities`**, for symmetry with the existing `people/connections/my-connections`.
- **The My Connections route is not renamed.** The parent task asks only for a page rename and never mentions routes. `people/connections/my-connections` keeps working and keeps pointing at My Connection Requests. Renaming it would break existing links for cosmetic gain.
- **A `Rock.SystemGuid.Page.MY_CONNECTION_OPPORTUNITIES` constant is added.** Every other page in this subtree has one, so the new page gets one too.

## Considered but Rejected

### Delete and recreate the My Connection Opportunities block instance
Rejected. The block instance carries configured attribute values, including the Detail Page setting that Phase 2 depends on. Recreating it would reset those to block type defaults and silently drop any partner customization. A `PageId` update preserves everything.

### Delete the Connection Request Detail and My Connection Opportunities pages outright
Rejected, and this was the parent task's earlier draft. Neither block is marked obsolete in code, partners are still running both, and `ConnectionType` carries a per-type detail page reference that a delete would orphan. Moving keeps the page Id and Guid, so those references stay valid.

### Use `UpdateBlockTypeByGuid` to remove the two block types
Rejected. That helper runs `DELETE FROM [BlockType] WHERE [Path] = '{path}'`, and entity-based block types have an empty `Path`, so it can wipe out every Obsidian and Mobile block type. This has caused production data loss before. `DeleteBlockType` by guid is the correct call.

### Ship the page migration together with the Hub entry-point change
Rejected. The Hub change carries unresolved design questions (whether a bare `?Request=0` can work on a page that requires a Connection Type, and whether preselecting a type should write the individual's saved filter preferences). Those should not hold up a structural cleanup that is independently verifiable.

### Leave the two WebForms files in the repo after deleting their database records
Rejected. A block type with no `BlockType` row is unreachable, so the files would be dead code that still shows up in searches and still has to compile. `Rollup_20260520.cs` removed the Connection Request Board files in the same pass.

## Related

- [Asana DEV-15156 (parent task)](https://app.asana.com/1/20866866924293/project/1208321217019996/task/1213724908502707) referenced for context; the verified page and block inventory in this spec supersedes the counts in that description.
- Asana DEV-15159, "DA: Connection Request Add Behavior Change" (subtask), covers the Hub entry-point change deferred out of this phase.
- `Rock.Migrations/Migrations/202605202132541_Rollup_20260520.cs` is the precedent migration: same domain, same shape, removed the legacy Connection Request Board block type and its pages.
- `Rock.Migrations/Migrations/Version 19.0/Version 19.0/202603152106213_AddConnectionsFeatures.cs` added the second-generation pages this spec finishes cleaning up after.
- `Rock.Migrations/Migrations/202607280003172_AddCelebrationReportPage.cs` added the Celebrations Report page missing from the parent task's inventory.
- The parent task links a private artifact page with a visual before and after tree. Not verifiable from here; the Current State and Target State sections above are built from a direct database query instead.
