# Common Migration Pitfalls

These cause real failures, many of them only on a later upgrade or squash. Check every one before finalizing a migration, in every mode.

---

## 1. Names that are not EF6's

A primary key, foreign key or index whose name is not the EF6 name passes at first, then breaks a later migration that drops or alters it by name. The name ratchet fails on it. See `sql-ddl-patterns.md`, *Naming Conventions*.

```sql
-- WRONG
ALTER TABLE [dbo].[Group] ADD CONSTRAINT [FK_Group_BinaryFile] FOREIGN KEY ([PhotoId]) REFERENCES [dbo].[BinaryFile] ([Id]);
-- CORRECT
ALTER TABLE [dbo].[Group] ADD CONSTRAINT [FK_dbo.Group_dbo.BinaryFile_PhotoId] FOREIGN KEY ([PhotoId]) REFERENCES [dbo].[BinaryFile] ([Id]);
```

## 2. Unguarded DDL

A migration may run again: after a developer forgets it to rerun a fix, or when a converted HotFix's code runs on a database that already has it. Every `CREATE`, `ALTER` and `DROP` needs an `IF NOT EXISTS` / `IF EXISTS` guard (`sys.indexes`, `sys.foreign_keys`, `sys.columns`, `COL_LENGTH`, `OBJECT_ID`), and data changes need a `WHERE` that makes a second run change nothing.

## 3. Schema that does not match the model

The mapping check fails when the schema does not support what the EF model maps. Match the model exactly:

- `[Required]` string: `NOT NULL`; otherwise nullable.
- `[MaxLength(N)]`: `[nvarchar](N)`; no `[MaxLength]`: `[nvarchar](max)`.
- `[DecimalPrecision( p, s )]`: `[decimal](p, s)`; none: `[decimal](18, 2)`.
- Non-nullable value types (`int`, `bool`, `DateTime`, enums): `NOT NULL`.

An entity change and its migration MUST land in the same change.

## 4. NOT NULL column on a table with rows

`ALTER TABLE ... ADD [Col] [bit] NOT NULL` fails when the table has rows. Add a default (`NOT NULL DEFAULT 0`), and leave the default constraint unnamed, as EF6 did.

## 5. C# work against the database

A migration only emits SQL. The runner throws, naming the migration, if `Up()` creates a `RockContext` (directly, or through a service, an attribute load or a cache miss) or opens a `DbService` connection. Move that work to a run-once post-update job (`sql-ddl-patterns.md`, *Post-Update Jobs*).

## 6. File system or System.Web

No `System.IO`, `File`, `Directory`, `HostingEnvironment`, `HttpContext` or `MapPath`. A migration runs in a transaction and a file change cannot roll back with it. Files an upgrade removes go in the update package's `deletefile.lst`; other one-time file work goes in startup code.

## 7. Statements that cannot run in a transaction

Every migration runs in a transaction, with no opt-out. `ALTER DATABASE`, full-text index changes and other statements SQL Server does not allow in a transaction are not supported.

## 8. Migration ID

- Exactly 14 digits, a valid `yyyyMMddHHmmss` time, from the UTC clock.
- Unique across the assembly. An agent writing several migrations must read the clock for each, not increment the last ID.
- Every script the migration runs starts with the same ID: `{id}.sql` or `{id}-{Suffix}.sql`.

`SqlMigrationFileTests` in `Rock.Tests` fails on a bad or duplicate ID and on an orphan or unembedded script.

## 9. Target version and folder

The target version is the version being developed (major.minor of `<Version>` in `Directory.Build.props`), and the file is in the matching `Version x.y` folder. Production applies migrations by target version, then ID, so a wrong version changes where the migration runs on a new install. For a converted HotFix, the target version is its `MigrationNumber` version as written (see `conversion-checklist.md`, step 11).

## 10. Order dependence between migrations

Production order (target version, then ID) and a developer's order (ID) differ when a fix with a lower target version is merged up after newer work. Two migrations that change the same object must not depend on which runs first; the commute check builds the database in both orders and fails if the schemas differ.

## 11. `using Rock.Plugin;`

Makes `Migration` ambiguous between `Rock.Data.Migration` and `Rock.Plugin.Migration`. Do not import it.

## 12. String escaping in interpolated SQL

In `$@"..."` strings, double single quotes and literal curly braces:

```csharp
// WRONG
Sql( $@"UPDATE [Person] SET [LastName] = 'O'Brien' WHERE [Guid] = '{personGuid}'" );
// CORRECT
Sql( $@"UPDATE [Person] SET [LastName] = 'O''Brien' WHERE [Guid] = '{personGuid}'" );
// Literal braces, for example JSON:
Sql( $@"UPDATE [Block] SET [AdditionalSettingsJson] = '{{""key"": ""{value}""}}' WHERE [Guid] = '{blockGuid}'" );
```

## 13. Block type method family

- **Obsidian and Mobile blocks** (`Rock.Blocks.*`, `Rock.Blocks.Types.Mobile.*`): `AddOrUpdateEntityBlockType()`. Entity-based, no path.
- **WebForms blocks** (`~/Blocks/*.ascx`): `UpdateBlockType()` or `UpdateBlockTypeByGuid()`. Path-based.

`UpdateBlockTypeByGuid()` contains `DELETE FROM [BlockType] WHERE [Path] = '{path}'`, which deletes every entity-based block type (their `Path` is empty). This caused data loss in production.

## 14. Assembly-qualified name versions

`UpdateEntityType()` with an assembly name needs the current version. Copy it from a recent migration on the same branch rather than guessing:

```csharp
"Rock.Blocks.Core.McpServerList, Rock.Blocks, Version=21.0.2.0, Culture=neutral, PublicKeyToken=null"
```

## 15. Values that must differ on every install

A `Guid` or key generated with `NEWID()` that should be different on each install is copied into every new install at the next squash. List the migration in the table in `Rock.Data.Migrations/README.md` in the same change.

## 16. Hard-coded `Id` or `[Order]`

`Id` values differ between databases: find rows by `Guid` or another stable key. Compute `[Order]` from the current maximum.

---

## Pre-Finalization Checklist

1. File `Version x.y/{id}_{Name}.cs`; `[SqlMigration( id, "x.y" )]`; `internal class {Name} : Migration`; file-scoped `Rock.Data.Migrations` namespace; copyright header and XML doc comments.
2. ID is 14 digits, from the clock, unique; any scripts are `{id}-{Suffix}.sql` next to the migration and run with `SqlResource`.
3. Every new key and index has its EF6 name.
4. Every DDL statement is guarded; data changes are safe to run twice.
5. Column types, lengths and nullability match the model.
6. `NOT NULL` columns on existing tables have a default.
7. Foreign key cascade rules: no cascade to `PersonAlias` or `DefinedValue`; `SET NULL` for `Campus`.
8. No `RockContext`, `DbService`, services or cache lookups; that work is in a post-update job.
9. No file system, `System.Web` or non-transactional statements.
10. `SystemGuid` constants instead of literal GUIDs where they exist; GUIDs uppercase.
11. Block type methods match the block type.
12. String escaping is correct in `$@"..."` SQL.
13. Per-install values are listed in `Rock.Data.Migrations/README.md`.
14. The developer is reminded to back up first and to run the migration and schema check integration tests locally.
