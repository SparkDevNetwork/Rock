# SQL Migration Patterns for Rock RMS

Schema changes are raw SQL, run with `Sql()`. There are no EF schema methods (`AddColumn`, `CreateTable` and so on) and no DDL helpers. Every name MUST be the name EF6 would have generated, and every DDL statement MUST be guarded so the migration can run again.

## Table of Contents

1. [Naming Conventions](#naming-conventions)
2. [Type Mapping](#type-mapping)
3. [Add a Column](#add-a-column)
4. [Add a Foreign Key Column](#add-a-foreign-key-column)
5. [Create a Table](#create-a-table)
6. [Alter a Column](#alter-a-column)
7. [Drop a Column, Index or Foreign Key](#drop-a-column-index-or-foreign-key)
8. [Indexes](#indexes)
9. [Data Migrations](#data-migrations)
10. [Stored Procedures, Views and Large Scripts](#stored-procedures-views-and-large-scripts)
11. [Reading Values with SqlScalar](#reading-values-with-sqlscalar)
12. [Post-Update Jobs](#post-update-jobs)
13. [Code Organization](#code-organization)

---

## Naming Conventions

Every name in Rock's schema came from EF6, and later migrations drop and alter objects by name. The name ratchet (`Names_MigratedDatabase_EveryNewNameFollowsEf6Conventions`) fails on a new name that does not follow these.

| Object | Name | Example |
|---|---|---|
| Primary key | `PK_dbo.{Table}` | `PK_dbo.FlexibleDocument` |
| Foreign key | `FK_dbo.{Table}_dbo.{TargetTable}_{Column}` | `FK_dbo.Group_dbo.BinaryFile_PhotoId` |
| Index | `IX_{Column}` | `IX_PhotoId` |
| Index on several columns | `IX_{Column1}_{Column2}` | `IX_EntityTypeId_EntityId` |
| Unique index on `Guid` | `IX_Guid` (unique) | `CREATE UNIQUE INDEX [IX_Guid] ...` |
| Default constraint | Leave unnamed; SQL Server names it | `... NOT NULL DEFAULT 0` |

Index names are unique per table, not per database, so `IX_CategoryId` exists on many tables.

## Type Mapping

| C# property | SQL type |
|---|---|
| `int` / `int?` | `[int] NOT NULL` / `[int]` |
| `long` | `[bigint]` |
| `bool` / `bool?` | `[bit] NOT NULL` / `[bit]` |
| `decimal` (default precision) | `[decimal](18, 2)`; check `[DecimalPrecision]` on the model |
| `DateTime` / `DateTime?` | `[datetime] NOT NULL` / `[datetime]` |
| `DateTimeOffset` | `[datetimeoffset](7)` |
| `TimeSpan` | `[time](7)` |
| `Guid` / `Guid?` | `[uniqueidentifier] NOT NULL` / `[uniqueidentifier]` |
| `string` with `[MaxLength(N)]` | `[nvarchar](N)`; `NOT NULL` only with `[Required]` |
| `string` without `[MaxLength]` | `[nvarchar](max)` |
| `byte[]` | `[varbinary](max)` |
| enum | `[int]`, nullable to match the property |

The mapping check compares the database with the EF model, so type, length, precision, scale and nullability must match the model exactly.

## Add a Column

Nullable column:

```csharp
Sql( @"
IF COL_LENGTH( 'dbo.Group', 'PhotoId' ) IS NULL
BEGIN
    ALTER TABLE [dbo].[Group] ADD [PhotoId] [int];
END" );
```

`NOT NULL` column on an existing table needs a default, or the `ALTER` fails on tables with rows:

```csharp
Sql( @"
IF COL_LENGTH( 'dbo.RegistrationTemplate', 'AreDuplicateRegistrantsPrevented' ) IS NULL
BEGIN
    ALTER TABLE [dbo].[RegistrationTemplate] ADD [AreDuplicateRegistrantsPrevented] [bit] NOT NULL DEFAULT 0;
END" );
```

## Add a Foreign Key Column

Column, then index, then foreign key, each guarded. EF6 creates the `IX_` index for every foreign key column.

```csharp
Sql( @"
IF COL_LENGTH( 'dbo.Group', 'PhotoId' ) IS NULL
BEGIN
    ALTER TABLE [dbo].[Group] ADD [PhotoId] [int];
END

IF NOT EXISTS ( SELECT 1 FROM [sys].[indexes] WHERE [name] = 'IX_PhotoId' AND [object_id] = OBJECT_ID( 'dbo.Group' ) )
BEGIN
    CREATE INDEX [IX_PhotoId] ON [dbo].[Group]([PhotoId]);
END

IF NOT EXISTS ( SELECT 1 FROM [sys].[foreign_keys] WHERE [name] = 'FK_dbo.Group_dbo.BinaryFile_PhotoId' )
BEGIN
    ALTER TABLE [dbo].[Group] ADD CONSTRAINT [FK_dbo.Group_dbo.BinaryFile_PhotoId] FOREIGN KEY ([PhotoId]) REFERENCES [dbo].[BinaryFile] ([Id]);
END" );
```

Cascade rules (see `.claude/rules/data-model.md`):

- `PersonAlias` (audit or custom), `DefinedValue`: no cascade.
- `Campus`: `ON DELETE SET NULL`.
- A child that has no meaning without its parent: `ON DELETE CASCADE` (rare).
- Everything else: no cascade.

## Create a Table

The standard `Model<T>` columns, the primary key, the audit foreign keys and indexes, and the unique `Guid` index. Compare with `20260831161511_AddFlexibleDocument.cs`.

```csharp
Sql( @"
IF OBJECT_ID( 'dbo.ExampleThing', 'U' ) IS NULL
BEGIN
    CREATE TABLE [dbo].[ExampleThing] (
        [Id] [int] NOT NULL IDENTITY,
        [Name] [nvarchar](100) NOT NULL,
        [CategoryId] [int],
        [CreatedDateTime] [datetime],
        [ModifiedDateTime] [datetime],
        [CreatedByPersonAliasId] [int],
        [ModifiedByPersonAliasId] [int],
        [Guid] [uniqueidentifier] NOT NULL,
        [ForeignId] [int],
        [ForeignGuid] [uniqueidentifier],
        [ForeignKey] [nvarchar](100),
        CONSTRAINT [PK_dbo.ExampleThing] PRIMARY KEY ([Id])
    );

    ALTER TABLE [dbo].[ExampleThing] ADD CONSTRAINT [FK_dbo.ExampleThing_dbo.Category_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Category] ([Id]);
    ALTER TABLE [dbo].[ExampleThing] ADD CONSTRAINT [FK_dbo.ExampleThing_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY ([CreatedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id]);
    ALTER TABLE [dbo].[ExampleThing] ADD CONSTRAINT [FK_dbo.ExampleThing_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY ([ModifiedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id]);

    CREATE INDEX [IX_CategoryId] ON [dbo].[ExampleThing]([CategoryId]);
    CREATE INDEX [IX_CreatedByPersonAliasId] ON [dbo].[ExampleThing]([CreatedByPersonAliasId]);
    CREATE INDEX [IX_ModifiedByPersonAliasId] ON [dbo].[ExampleThing]([ModifiedByPersonAliasId]);
    CREATE UNIQUE INDEX [IX_Guid] ON [dbo].[ExampleThing]([Guid]);
END" );
```

An entity that inherits `Entity<T>` instead of `Model<T>` has only `Id`, `Guid` and the three `Foreign*` columns. A new entity also needs its `EntityType` row (`RockMigrationHelper.UpdateEntityType`), which startup registration otherwise adds.

## Alter a Column

Indexes and foreign keys on the column block `ALTER COLUMN`; drop them first and recreate them after.

```csharp
Sql( @"
IF EXISTS ( SELECT 1 FROM [sys].[columns] WHERE [object_id] = OBJECT_ID( 'dbo.PersonViewed' ) AND [name] = 'IpAddress' AND [max_length] <> 90 )
BEGIN
    ALTER TABLE [dbo].[PersonViewed] ALTER COLUMN [IpAddress] [nvarchar](45);
END" );
```

`sys.columns.max_length` is in bytes, so `nvarchar(45)` is 90. To make a nullable column `NOT NULL`, first `UPDATE` every `NULL` to a safe value.

## Drop a Column, Index or Foreign Key

Drop in reverse: foreign key, then index, then any default constraint, then the column. Read the real names from the database or the baseline (`Migrations/Baseline/20131125173405-01_Schema.sql` and `-03_Constraints.sql`).

```csharp
Sql( @"
IF EXISTS ( SELECT 1 FROM [sys].[foreign_keys] WHERE [name] = 'FK_dbo.Group_dbo.BinaryFile_PhotoId' )
BEGIN
    ALTER TABLE [dbo].[Group] DROP CONSTRAINT [FK_dbo.Group_dbo.BinaryFile_PhotoId];
END

IF EXISTS ( SELECT 1 FROM [sys].[indexes] WHERE [name] = 'IX_PhotoId' AND [object_id] = OBJECT_ID( 'dbo.Group' ) )
BEGIN
    DROP INDEX [IX_PhotoId] ON [dbo].[Group];
END

DECLARE @DefaultConstraint NVARCHAR(256) = (
    SELECT [dc].[name]
    FROM [sys].[default_constraints] AS [dc]
    INNER JOIN [sys].[columns] AS [c] ON [c].[object_id] = [dc].[parent_object_id] AND [c].[column_id] = [dc].[parent_column_id]
    WHERE [dc].[parent_object_id] = OBJECT_ID( 'dbo.Group' ) AND [c].[name] = 'PhotoId' );

IF @DefaultConstraint IS NOT NULL
BEGIN
    EXEC( 'ALTER TABLE [dbo].[Group] DROP CONSTRAINT [' + @DefaultConstraint + ']' );
END

IF COL_LENGTH( 'dbo.Group', 'PhotoId' ) IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[Group] DROP COLUMN [PhotoId];
END" );
```

## Indexes

```csharp
Sql( @"
IF NOT EXISTS ( SELECT 1 FROM [sys].[indexes] WHERE [name] = 'IX_EntityTypeId_EntityId' AND [object_id] = OBJECT_ID( 'dbo.ExampleThing' ) )
BEGIN
    CREATE INDEX [IX_EntityTypeId_EntityId] ON [dbo].[ExampleThing]([EntityTypeId], [EntityId]);
END" );
```

An index on a large table (`Interaction`, `AttributeValue`, `History`, `CommunicationRecipient`) can take minutes and blocks startup while it builds. Put it in a run-once post-update job instead (see [Post-Update Jobs](#post-update-jobs)); Rock has many examples under `Rock/Jobs/PostUpdateJobs`.

## Data Migrations

Find rows by `Guid` or another stable key, never by `Id` (it differs between databases), and guard so a second run changes nothing:

```csharp
Sql( @"
UPDATE [HtmlContent]
SET [Content] = REPLACE( [Content], 'class=""alert""', 'class=""alert alert-warning""' )
WHERE [Guid] = '6D8C5D5E-4D44-4F3B-A3B2-ACF1C8D2E3B7'
    AND [Content] LIKE '%class=""alert""%';" );
```

`[Order]` values come from the current maximum, not a hard-coded number:

```sql
DECLARE @Order INT = ( SELECT ISNULL( MAX( [Order] ), -1 ) + 1 FROM [DefinedValue] WHERE [DefinedTypeId] = @DefinedTypeId );
```

## Stored Procedures, Views and Large Scripts

Put the script in a file next to the migration, named `{id}-{Suffix}.sql` (letters, digits and underscores in the suffix), and run it with `SqlResource( "{Suffix}" )`. Use `SqlResource()` for a single script named `{id}.sql`. The build compresses and embeds every `.sql`; commit the plain `.sql`, never a `.gz`. `GO` lines split the script into batches, so a data value inside a script must not have `GO` alone on a line. `Sql()` is the opposite: it runs its text as one batch and never splits, so `GO` in a `Sql()` string is a syntax error; use one `Sql()` call per batch or a script.

```csharp
SqlResource( "spCrm_PersonMerge" );   // runs {id}-spCrm_PersonMerge.sql
```

```sql
-- {id}-spCrm_PersonMerge.sql
CREATE OR ALTER PROCEDURE [dbo].[spCrm_PersonMerge]
    @OldId INT,
    @NewId INT
AS
BEGIN
    ...
END
```

`CREATE OR ALTER` is safe to run twice. Compare with `20260708174028_Rollup_20260708.cs` and its `20260708174028-295_...sql` scripts.

## Reading Values with SqlScalar

`Sql()` and `SqlScalar()` run immediately, in order, on the migration's own connection and transaction. A `SqlScalar` sees every earlier statement of the same migration.

```csharp
var blockTypeId = SqlScalar( "SELECT [Id] FROM [BlockType] WHERE [Guid] = 'A1B2C3D4-...'" ).ToStringSafe().AsIntegerOrNull();

if ( blockTypeId == null )
{
    return;
}
```

## Post-Update Jobs

Work that needs C# against the database (loading entities, calling services, a `RockContext`, cache lookups) or that takes too long for startup goes in a run-once job, not in the migration. The runner throws if a migration creates a `RockContext` or uses `DbService`.

1. Write the job in `Rock/Jobs/PostUpdateJobs/`, ending with `ServiceJobService.DeleteJob( GetJobId() )`.
2. Add a `SystemGuid.ServiceJob` constant.
3. In the migration, create the job:

```csharp
RockMigrationHelper.AddPostUpdateServiceJob(
    name: "Rock Update Helper v21.0 - Example",
    description: "What the job does.",
    jobType: "Rock.Jobs.PostV21Example",
    cronExpression: "0 0 21 1/1 * ? *",
    guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_210_EXAMPLE );
```

4. Add the `Guid` to `startupRunOnceJobGuids` (runs at startup) or `scheduledRunOnceJobGuids` (runs at 2 AM, for jobs that slow the database) in `Rock/Jobs/PostUpdateJobs/DataMigrationsStartup.cs`.

## Code Organization

Break a migration with several logical operations into private methods, so `Up()` reads like a table of contents:

```csharp
public override void Up()
{
    AddConnectionsPages();
    AddConnectionNavigationViewBlocks();
    UpdateConnectionProperties();
}
```

- Prefix with the developer's initials when several developers share a rollup (`JPH_AddConnectionsPages()`).
- Use `private const string` for a `Guid` used more than once.
- There is no `Down()`, so there are no `_Up` / `_Down` method pairs.
