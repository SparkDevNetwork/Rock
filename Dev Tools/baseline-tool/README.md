# Baseline Tool

Builds the SQL migrations baseline: the scripts that create a new database at
Rock version B. See `specs/261027-sql-migrations-on-net-framework.md`,
*Baseline tool and periodic squash*.

The tool does not reference Rock, so one build of it works for any B. It reads
B's source tree (the run-once job lists and HotFix numbers) and B's `RockWeb`
folder (its version, settings and connection string), and works on the
database with SqlClient and SMO.

## Workflow

1. Build B's release (for example the `19.7.0` tag) in a separate worktree.
2. Create an empty database on the minimum SQL Server version B supports
   (SQL Server 2022 for v19) and point B's site at it.
3. Run `verify`.
4. Start B's site from Visual Studio and run `monitor`. Run from Rock any 2 AM
   run-once jobs it lists. When it reports that startup settled, stop the site.
   - Start it from Visual Studio, not by running `iisexpress.exe` from the
     command line. Rock runs the startup run-once jobs only when
     `RunJobsInIISContext` is on (which `verify` requires to be off) or
     `HostingEnvironment.IsDevelopmentEnvironment` is true, and IIS Express
     started from the command line is not a development environment. The
     site then installs, but `monitor` never settles because no run-once job
     runs (seen 2026-11-17 while building the 19.8.1 baseline).
   - Do not turn on `RunJobsInIISContext` to work around it: that also starts
     the scheduler, whose regular jobs change data that would end up in the
     baseline.
5. Back up the database if you need a copy at this point.
6. Run `cleanup`. It stops if startup has not settled or logged exceptions;
   review them and rerun with `--accept-exceptions` when they are expected.
7. Run `export`, then `package`, and paste the line `package` prints into
   `Rock/Data/Migration.cs`.
8. Check the packaged files in `Rock.Data.Migrations/Migrations/Baseline` for
   run state before committing them. This check found `ScheduleDate` rows
   anchored to the build day in the first v19 baseline.
   - No data file for `ExceptionLog`, `History`, `ScheduleDate`,
     `Interaction`, `InteractionSession`, `InteractionComponent`,
     `InteractionDeviceType`, `AttendanceOccurrence`, `ServiceJobHistory`,
     `WebFarmNode`, `AnalyticsSourceDate`, `AnalyticsSourcePostalCode` or
     `IdentityVerificationCode`, and none for `__MigrationHistory` or
     `RockMigration`.
   - A `PluginMigration` data file, and the `Meta*` data files with
     `generateGuid`.
   - The `PostInstallDataMigrations` job row present, never run.
   - The `ServiceJob` run-state columns null, and the `RockInstanceId`
     attribute's `CreatedDateTime` null.
   - Triggers in `03_Constraints` and the reseed statements in `04_Reseed`.
   - Anything else that looks like the state of the build site (dates near
     the build day, users, sessions, keys).

```bash
dotnet run --project "Dev Tools/baseline-tool" -- verify --rockweb D:/source/Rock-19.7/RockWeb
dotnet run --project "Dev Tools/baseline-tool" -- monitor --source D:/source/Rock-19.7 --rockweb D:/source/Rock-19.7/RockWeb
dotnet run --project "Dev Tools/baseline-tool" -- cleanup --source D:/source/Rock-19.7 --rockweb D:/source/Rock-19.7/RockWeb
dotnet run --project "Dev Tools/baseline-tool" -- export --rockweb D:/source/Rock-19.7/RockWeb
dotnet run --project "Dev Tools/baseline-tool" -- package
```

## Commands

| Command | What it does |
|---|---|
| `verify` | The database is empty, the server is at least the minimum SQL Server major version B supports (`--sql-major`, default 16), or newer, and `RunJobsInIISContext` is off. |
| `monitor` | Only reads. Polls until nothing is outstanding (the same run-once job and HotFix checks as `cleanup`) and the registration tables (`EntityType`, `FieldType`, `BlockType`, `Block`, `Attribute`, `AttributeQualifier`, `AttributeValue`) have not changed for `--quiet` seconds (default 60). The quiet period stands in for the block type thread, which writes nothing that marks it finished. It polls every `--poll` seconds (default 10) and gives up after `--timeout` minutes (default 120). |
| `cleanup` | Checks that no job from either `DataMigrationsStartup` list is left and every HotFix B runs is recorded, then stops on startup exceptions unless accepted. In one transaction it clears the `RockInstanceId` creation date and job run state, undoes the post-install job (deletes `AnalyticsSourceDate`, `AnalyticsSourcePostalCode` and `IdentityVerificationCode`, restores its job row with `Id` 64), empties run data (exception log, history, login history, schedule dates, interactions, attendance occurrences, job history, web farm nodes), and marks users offline. Missing tables are skipped. It records the identity values, instance ID and last included migration in the work folder. |
| `export` | Writes the database into `Rock.Data.Migrations/Migrations/Baseline`: `01_Schema` with SMO (types, tables with keys and indexes, views, functions, procedures), one JSON data file per table with rows (`{Table}.json`, one row per line in primary key order, bulk loaded by the baseline), and `03_Constraints` with SMO (foreign keys and triggers, after the data). The `Meta*` lookups leave out `Guid` and set `generateGuid`, so each install gets its own. `geography`, `geometry` and `hierarchyid` are written as SQL Server's serialized bytes. The migration history tables are left out. Files are UTF-8 with no headers. |
| `package` | Writes `04_Reseed`, which first gives the `RockInstanceId` attribute a new `Guid` with `NEWID()` and then restores every table's identity value; generates `Baseline.cs` (the schema script, a `BulkResource` call per data file, the constraints and the reseed); and prints the `BaselineIncludesThroughMigrationId` line. |

`--database` takes a connection string, or a database name on the server in
the RockWeb folder's `web.ConnectionStrings.config`. The work folder
(`Dev Tools/baseline-tool/work`, ignored by git) holds what `cleanup` records
for `package`.

The exit code is 0 on success, 1 when a check stopped the command, and 2 when
the tool could not run.
