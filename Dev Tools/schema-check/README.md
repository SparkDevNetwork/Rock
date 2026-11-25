# Schema Check

A console tool that checks a Rock database's schema. These checks are the gate
for SQL migrations; see `specs/261027-sql-migrations-on-net-framework.md`,
*Schema checks (required)*.

The comparison core is internal to `Rock.dll` (`Rock/Data/SchemaChecks/`) and
reads the database through the `sys.*` views with no EF, so integration tests
can call it directly. This project adds the command line and the EF6 model
adapter (`Ef6ModelAdapter.cs`), which is the only code that reads EF metadata.

`MigratedDatabaseSchemaChecksTests` in `Rock.Tests.Integration` runs `names`
and `mapping` against the integration test database, which is built from the
baseline and every migration, using the same allow list. A migration that
adds a misnamed key or index, or leaves the schema short of the model, fails
those tests.

## Commands

```bash
dotnet run --project "Dev Tools/schema-check" -- mapping
dotnet run --project "Dev Tools/schema-check" -- names
dotnet run --project "Dev Tools/schema-check" -- equivalence --left RockBefore --right RockAfter
dotnet run --project "Dev Tools/schema-check" -- commute --left DevelopmentOrder --right ProductionOrder
```

`--left` and `--right` name any two databases, for example one built before a
change and one after.

| Command | What it checks |
|---|---|
| `mapping` | The database supports what the EF6 model maps. Fails on a missing table or column, a different store type, length, precision, scale or nullability, a different primary key, and an unmapped `NOT NULL` column with no default. Reports missing foreign keys and cascade differences without failing. Entities mapped to views are only checked for column presence. |
| `names` | Every primary key, foreign key, index and unique constraint has its EF6 name: `PK_dbo.Table`, `FK_dbo.Table_dbo.Target_Column`, `IX_Column1_Column2`. |
| `equivalence` | Two databases have the same catalog (tables, columns, defaults, keys, foreign keys, indexes and their names, check constraints, module definitions) and the same table data. Date, time, GUID, `timestamp`, spatial, `xml` and `sql_variant` columns are left out of the data comparison. `--catalog-only` skips the data. |
| `commute` | The catalog part of `equivalence`, for two databases built in different migration orders. `MigrationOrderCommuteTests` in `Rock.Tests.Integration` builds the two databases with the SQL migration runner and runs this comparison. |

### What the checks do not cover

- `equivalence` and `commute` do not compare user-defined types, sequences,
  synonyms, full-text indexes, partitioning or column order. Spatial and XML
  indexes are compared by their key columns only. None of these mattered for
  the switch to SQL migrations; add them to the catalog reader if one starts
  to.
- `mapping` reports these on develop's database on every run, and they are
  expected, so they are informational rather than failures:
  - About 28 `MissingForeignKey`: mostly the `*DateKey` relationships to
    `AnalyticsSourceDate`, plus a few on hot tables such as `Interaction` and
    `PersonAlias.AliasPersonId`. Rock leaves out these foreign keys on purpose.
  - 3 `CascadeMismatch` where the model cascades and the database uses
    `SET NULL` (`Attendance.AttendanceCheckInSessionId`,
    `BackgroundCheck.ConnectionRequestId`,
    `FinancialPaymentDetail.FinancialPersonSavedAccountId`). The database
    follows the rule in `.claude/rules/data-model.md`.

`--database`, `--left` and `--right` take a connection string, or a database
name on the server in `RockWeb/web.ConnectionStrings.config`. `--database`
defaults to that file's database.

## Allow list

`schema-check-allow-list.json` lists accepted findings, each with a reason. A
finding matches an entry on check, code and object. An entry that no longer
matches anything fails the check (`StaleAllowListEntry`) until it is removed,
so the list only shrinks. Only entries for the checks that ran are checked:
`equivalence --catalog-only` does not check `data` entries.

`names --freeze` rewrites the `names` entries from the current database. It
was run once to freeze the names that existed when the ratchet was added. Do
not run it to make a new name pass; fix the name instead.

## Output

One line per finding (`FAIL`, `INFO`, or `ALLOW` with `--verbose`), the time
each step took, and a summary line. The exit code is 0 when the checks pass,
1 when a finding fails that is not allowed, and 2 when the tool could not run.
