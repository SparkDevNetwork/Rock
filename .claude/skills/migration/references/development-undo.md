# Undoing a Migration During Development

SQL migrations have no `Down()`. While developing a migration, there are three ways back, in order of reliability. The skill always gives the first as a reminder; the other two only on request.

## 1. Back up or snapshot first (always remind)

Before running a new migration, back up the database or take a SQL Server database snapshot. Reverting to it is the exact undo, data included.

```sql
-- Take a snapshot (the logical file name is usually the database name; check sys.database_files).
SELECT [name] FROM [sys].[database_files] WHERE [type] = 0;

CREATE DATABASE [RockDev_BeforeMigration] ON
    ( NAME = N'RockDev', FILENAME = N'C:\SQLData\RockDev_BeforeMigration.ss' )
AS SNAPSHOT OF [RockDev];
```

```sql
-- Revert to it. Stop Rock first: the revert needs no other connections.
USE [master];
ALTER DATABASE [RockDev] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE [RockDev] FROM DATABASE_SNAPSHOT = 'RockDev_BeforeMigration';
ALTER DATABASE [RockDev] SET MULTI_USER;

-- Drop the snapshot when you no longer need it.
DROP DATABASE [RockDev_BeforeMigration];
```

Database snapshots are in every SQL Server edition since 2016 SP1, but not in Azure SQL Database. A database with more than one snapshot cannot be reverted until the others are dropped. Replace the database name, logical file name and path with the developer's own.

## 2. Forget the migration (on request)

Deletes the migration's history row, so the runner runs it again at the next start:

```sql
DELETE FROM [RockMigration] WHERE [MigrationId] = 20261117153042;
```

This does NOT undo anything the first run did. It is for rerunning a corrected migration whose statements are guarded (`IF NOT EXISTS` and the like), so the parts that already ran are skipped. If the first run created something wrong (a column with the wrong type, say), the guarded rerun skips it too: fix that by hand, or restore the backup.

## 3. Undo script (on request, best effort)

Write SQL that reverses the migration's statements in reverse order, then deletes its history row. Reverse each statement:

| Up() did | Undo |
|---|---|
| Added a column | Drop its foreign key, index and default constraint, then the column |
| Created a table | Drop its foreign keys, then the table |
| Added an index or foreign key | Drop it |
| Altered a column | Alter it back (a type change may lose data) |
| Created or altered a procedure or view | Restore the previous definition from the baseline or the previous migration that defined it, or drop it if it is new |
| `RockMigrationHelper.Add*` | The matching `RockMigrationHelper.Delete*`, written as SQL, or a `DELETE` by `Guid` |
| Updated or deleted data | Usually not reversible: the old values are gone |

```sql
-- Undo of 20261117153042_AddGroupPhotoId (best effort; review before running).
IF EXISTS ( SELECT 1 FROM [sys].[foreign_keys] WHERE [name] = 'FK_dbo.Group_dbo.BinaryFile_PhotoId' )
    ALTER TABLE [dbo].[Group] DROP CONSTRAINT [FK_dbo.Group_dbo.BinaryFile_PhotoId];

IF EXISTS ( SELECT 1 FROM [sys].[indexes] WHERE [name] = 'IX_PhotoId' AND [object_id] = OBJECT_ID( 'dbo.Group' ) )
    DROP INDEX [IX_PhotoId] ON [dbo].[Group];

IF COL_LENGTH( 'dbo.Group', 'PhotoId' ) IS NOT NULL
    ALTER TABLE [dbo].[Group] DROP COLUMN [PhotoId];

DELETE FROM [RockMigration] WHERE [MigrationId] = 20261117153042;
```

Always say:

- It is a best effort that nothing tests.
- A data migration often cannot be reversed, because the original values are gone.
- Restoring the backup or snapshot is the reliable way back.

Give the script in the conversation or as a scratch file outside the repository. Never commit it, and never put it in `Rock.Data.Migrations`: a `.sql` file there without a migration fails the orphan-script check in `SqlMigrationFileTests`.
