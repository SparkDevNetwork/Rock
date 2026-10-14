# Locating the UI for a Change

How to get from a changed file to the place a tester clicks. Prefer stock pages from the sample database; give the menu path a human would use, plus the page id or route for agents.

## Database access

Read-only queries against the developer's database:

```bash
sqlcmd -S localhost -d <Database> -E -W -I -Q "SET NOCOUNT ON; <query>"
```

Get `<Database>` from the `Initial Catalog` in `RockWeb/web.ConnectionStrings.config`. Never write to the database while planning.

`../verify-run/references/ui-recipes.md` has a stock page map (login, Page Properties, Zone Blocks, and about 50 common admin pages) that saves most lookups.

Page ids and block ids can differ between databases. In the plan, name pages by menu path and route first, and give the id as "page 163 on the stock database". Tests that need exact ids should include the query that finds them.

## Obsidian block (`Rock.Blocks/<Domain>/<Name>.cs`)

The block type is keyed by entity type name `Rock.Blocks.<Domain>.<Name>`:

```sql
SELECT p.[Id] AS [PageId], p.[InternalName], pr.[Route], b.[Id] AS [BlockId], b.[Guid] AS [BlockGuid], p.[Guid] AS [PageGuid]
FROM [Block] AS [b]
JOIN [BlockType] AS [bt] ON bt.[Id] = b.[BlockTypeId]
JOIN [EntityType] AS [et] ON et.[Id] = bt.[EntityTypeId]
JOIN [Page] AS [p] ON p.[Id] = b.[PageId]
LEFT JOIN [PageRoute] AS [pr] ON pr.[PageId] = p.[Id]
WHERE et.[Name] = 'Rock.Blocks.Finance.TransactionList'
```

Blocks placed on a layout or site instead of a page have `PageId` NULL; check `b.[LayoutId]` and `b.[SiteId]`.

## WebForms block (`RockWeb/Blocks/<Path>.ascx`)

```sql
SELECT p.[Id], p.[InternalName], pr.[Route]
FROM [Block] AS [b]
JOIN [BlockType] AS [bt] ON bt.[Id] = b.[BlockTypeId]
JOIN [Page] AS [p] ON p.[Id] = b.[PageId]
LEFT JOIN [PageRoute] AS [pr] ON pr.[PageId] = p.[Id]
WHERE bt.[Path] = '~/Blocks/Finance/TransactionList.ascx'
```

## Block not on any stock page

Some block types exist but are not placed anywhere on a stock database (or a new block was added in this change). The test's setup then adds it: create a child page under **Admin Tools > Installed Plugins**, add the block in the Main zone through the zone editor, and set the block settings the test needs. Cleanup removes the page.

A new Obsidian block also needs its block type registered (by a migration, or automatically on startup for entity-based block types). If the block type is missing, the test's setup says so and the runner should report it.

## Service methods and other non-UI code

Find callers to choose an entry point:

- Grep the method name in `Rock.Blocks`, `RockWeb/Blocks`, `Rock.Rest`, `Rock/Jobs`, `Rock/Workflow`, and `Rock/Lava`.
- For a changed entity property or `PreSave` logic, any detail block for that entity is an entry point (`<Entity>Detail`).
- Lava access to an entity (`{% <entity> where:... %}`) is a fallback entry point for read-side changes.

## Jobs, workflow actions, field types

- **Jobs:** the class in `Rock/Jobs/` maps to a `ServiceJob` row by `Class` = `Rock.Jobs.<Name>`. Query `SELECT [Id], [Name], [IsActive] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.<Name>'`. Jobs Administration is under Admin Tools > System Settings.
- **Workflow actions:** search stock workflow types that already use the action: `SELECT wt.[Name] FROM [WorkflowActionType] AS [wat] JOIN [EntityType] AS [et] ON et.[Id] = wat.[EntityTypeId] JOIN [WorkflowActivityType] AS [wact] ON wact.[Id] = wat.[ActivityTypeId] JOIN [WorkflowType] AS [wt] ON wt.[Id] = wact.[WorkflowTypeId] WHERE et.[Name] = 'Rock.Workflow.Action.<Name>'`. If none, the test builds a small workflow type.
- **Field types:** `SELECT a.[Name], a.[EntityTypeId] FROM [Attribute] AS [a] JOIN [FieldType] AS [ft] ON ft.[Id] = a.[FieldTypeId] WHERE ft.[Class] = 'Rock.Field.Types.<Name>'` finds existing attributes to test against.

## Menu paths

Find the menu path by walking `ParentPageId` up from the page, or open the page in the browser and read the breadcrumbs. Use the names as they appear in the menu (for example "Finance > Functions > Batches").
