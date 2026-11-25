# Rock.Data.Migrations

Rock's SQL migrations: the baseline, which builds a new database at a released version, and every migration after it. See `specs/261027-sql-migrations-on-net-framework.md` for the design.

## Values that must differ on every install

Some values are meant to be different on every Rock install, such as the built-in Admin person's `Guid`. A migration that creates one with `NEWID()` works until the next squash, which copies the value from the database the baseline is built from into every new install. Two places keep this from happening.

### Migrations to handle at the next squash

When you write a migration that creates or changes a value that must differ on every install, list it here in the same change. You do not need to target the value yet; at the squash its rows are frozen in the baseline and their `Id` values never change again, so the step can be written then.

At each squash, work through this table: add a step to `Migrations/Baseline/Baseline.Reseed.cs` for each entry (or a deletion to the baseline tool's `cleanup` for a value Rock creates again at runtime), then remove the entry.

| Migration ID | What must differ per install, and why |
|---|---|
| | |

### Handled at every squash

These need no entry above. They are handled in code, which is kept across squashes:

- **`Migrations/Baseline/Baseline.Reseed.cs`** regenerates values after the baseline loads, on every new install. Each step finds its rows by an `Id` or `Key`, never by the frozen value, and checks its row counts, so a later migration that changes or deletes the row makes every new install fail, test runs included, until the step is fixed.
- **`Dev Tools/baseline-tool/CleanupCommand.cs`** (`PerSiteSettings`) deletes system settings Rock creates at runtime that hold the build site's values, so they are never in the baseline's data files.
- **`generateGuid`** in a baseline data file (the `Meta*` lookups) gives every loaded row a new `Guid`.

## Migrations that share code

A squash deletes the migrations the new baseline includes. A migration that uses another migration's code must go in the same squash as the one it uses, or the code must move first; otherwise `Rock.Data.Migrations` stops building.

| Migration that holds the code | Migrations that use it |
|---|---|
| `20261116204133_EscapeLavaChartShortcodeData` (`EscapeChartMarkupSql`) | `20261116214405_ReapplyEscapeLavaChartShortcodeData`, `20261118233201_Rollup_20261118` |
