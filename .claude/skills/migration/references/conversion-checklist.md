# Converting an EF6 Migration or a HotFix

Release and hotfix branches still write EF6 migrations (`Rock.Migrations/Migrations/...`) and HotFixes (`Rock/Plugin/HotFixes/...`). When one reaches develop through a merge, it MUST be converted in that merge. This is the spec's conversion checklist (`specs/261027-sql-migrations-on-net-framework.md`, *Conversion checklist*) as steps.

## How they arrive

- **A new file merged in:** git's rename detection usually reports a "file location" conflict and moves it into `Rock.Data.Migrations/Migrations/Version 20.0/`, or it lands under `Rock.Migrations/` or `Rock/Plugin/HotFixes/`. Wherever it lands, `StrayMigrationFilesTests` in `Rock.Tests` fails and lists it.
- **A change to an existing EF6 migration or HotFix:** a modify/delete conflict, because develop deleted the file. Find the converted copy (same 14-digit ID for an EF6 migration, or `ReplacesHotFixNumber` for a HotFix, or the rollup that absorbed it) and apply the same change there. Then resolve the conflict by keeping the file deleted.

Get the original from the branch it came from (`git show <branch>:<path>`) when the merge moved or deleted it.

## Steps

### 1. Identity

| Source | ID | Target version |
|---|---|---|
| EF6 migration | The first 14 digits of its 15-digit ID (`202611201200000_Name` becomes `20261120120000`) | Its version folder (`Migrations/Version 20.0/Version 20.0/` gives `20.0`) |
| HotFix | The UTC time the HotFix file was first committed: `TZ=UTC git log <branch> --diff-filter=A --format=%ad --date=format-local:%Y%m%d%H%M%S -- Rock/Plugin/HotFixes/<file> \| tail -1` (`format-local` with `TZ=UTC` gives UTC, where `format:` gives the author's own time zone; `tail -1` takes the oldest when the file was added more than once; no `--follow`, which picks up the wrong date) | Its `MigrationNumber` version, unchanged (see step 11) |

The file goes in `Rock.Data.Migrations/Migrations/Version {target}/{id}_{Name}.cs`. The class name stays unless it collides with an existing migration's.

### 2. Shell

- File-scoped `namespace Rock.Data.Migrations;`, `internal class {Name} : Migration`, `[SqlMigration( {id}, "{target}" )]`.
- For a HotFix, add `ReplacesHotFixNumber = {number}`: `[SqlMigration( {id}, "{target}", ReplacesHotFixNumber = {number} )]`. Databases that already ran the HotFix then record the migration instead of running it again.
- Delete `Down()` and the methods only it used.
- Drop the EF6 metadata: the `.Designer.cs` file, `IMigrationMetadata`, and the snapshot `.resx`.
- Keep the copyright header, the XML doc comments and every engineering note.

### 3. Categorize

A migration can fall under more than one:

- **Straight copy:** only `Sql()`, `SqlScalar()` and `RockMigrationHelper` calls. The body moves unchanged, with one exception: `Sql()` runs its text as one batch and does not split on lines that contain only `GO`, as EF6's did (HotFix `Sql()` never split, so HotFixes need nothing here). If a converted EF6 `Sql()` string has `GO` lines, split it into one `Sql()` call per batch, or move it into a `{id}-{Suffix}.sql` script run by `SqlResource()`, which does split. EF6 also removed a backslash at the end of a line inside `Sql()` text (sqlcmd's line continuation) and the new `Sql()` does not: if a converted EF6 `Sql()` string has a line ending in `\`, remove the backslash and the line break by hand.
- **Schema calls:** EF6 methods (`AddColumn`, `CreateTable`, `AddForeignKey`, `CreateIndex`, `AlterColumn`, `DropColumn`, `RenameColumn` and so on). Step 4.
- **Resources:** `RockMigrationSQL.*` or `HotFixMigrationResource.*`. Step 5.
- **Rework:** file system calls, `System.Web`, C# database work. Steps 6 to 8.

### 4. Schema calls

Replace each with the exact SQL EF6 would have run, with EF6 names (see `sql-ddl-patterns.md`, *Naming Conventions*). Either:

- generate it with EF6's `SqlServerMigrationSqlGenerator` by building the migration on the branch it came from, where the EF6 project still exists, or
- write it, following the patterns in `sql-ddl-patterns.md`.

The mapping check, name ratchet and commute check verify the result. Converted EF6 SQL was kept exactly as generated, without guards, so earlier conversions do not show guards; that is acceptable for a conversion, since the SQL already ran on databases that ran the EF6 migration and those databases record it as applied.

### 5. Resources

Replace each `RockMigrationSQL.X` or `HotFixMigrationResource.X` reference with a script file `{id}-{Suffix}.sql` next to the migration, run with `SqlResource( "{Suffix}" )`. Copy the script content unchanged (decode it from UTF-16 or `.gz` if needed; commit plain UTF-8 `.sql`).

### 6. No `System.Web`, no file system

`HostingEnvironment`, `HttpContext`, `MapPath`, `File` and `Directory` calls MUST NOT carry over. Drop them when they duplicate Rock Update's work (`deletefile.lst`), otherwise move them to startup code, and say which you did.

### 7. `SqlScalar` timing

EF6 queued `Sql()` statements and ran them after `Up()` returned, while its `SqlScalar()` ran at once on a separate connection, so an EF6 `SqlScalar` saw the database before any of the migration's own statements. Now every call runs at once, in order, on one connection and transaction. For each `SqlScalar` (and each `DbService` read rewritten as `SqlScalar`), check whether an earlier statement in the same `Up()` changes what it reads; if one does, move the read before it or rewrite the logic.

### 8. C# database work

Entity loading, service calls, `RockContext`, `DbService` and cache lookups MUST NOT carry over. Move them to a run-once post-update job (see `sql-ddl-patterns.md`, *Post-Update Jobs*).

### 9. HotFixes being absorbed

Delete the HotFix file. Develop has no `Rock/Plugin/HotFixes` folder, and the stray-file guard fails until the file is gone. Set `ReplacesHotFixNumber` (step 2).

### 10. HotFixes already converted on their own (MUST report)

A release branch can absorb into an EF6 rollup a HotFix that develop already converted as its own migration. For an EF6 rollup being converted:

1. Find each HotFix whose code the rollup contains (the rollup's engineering notes and method names usually name them; the commit that disabled the HotFix on the release branch shows it too).
2. For each, search `Rock.Data.Migrations` for `ReplacesHotFixNumber = {number}`.
3. Convert the rollup as it is: do not strip the HotFix code.
4. **Report** each such HotFix number and ask for a review that its code is safe to run twice, because the converted rollup runs it again on databases where the HotFix already ran. A HotFix that is not safe to rerun is fixed in a new migration on every branch that has it, not removed from the rollup.

### 11. HotFix target version (MUST report)

Use the `MigrationNumber` version as the target version, unchanged. The conversion trusts the source and never picks its own version. Then check it:

1. Find the version the HotFix was introduced in: the lowest-versioned `hotfix-X.Y` or `release-X.Y` branch whose history adds that HotFix file.
   ```bash
   for b in $(git branch -r --format='%(refname:short)' | grep -E 'origin/(hotfix|release)-[0-9]'); do git cat-file -e "$b:Rock/Plugin/HotFixes/<file>" 2>/dev/null && echo "$b"; done
   ```
2. Accept it silently when the major version matches and the minor version is within 1 (a HotFix is often written before the branch's version is bumped).
3. Otherwise **report** the HotFix number, the written version and the introduced version, and say the fix belongs in the HotFix on its own branch. Convert with the written version anyway.

## After converting

1. Delete the originals: the EF6 `.cs`, `.Designer.cs` and `.resx`, or the HotFix `.cs` and any of its scripts.
2. Build `Rock.Data.Migrations`.
3. Run `Rock.Tests` → `Rock.Tests.Data.SqlMigrations` (stray-file guard, script and ID checks).
4. Remind the developer to run the migration and schema check integration tests locally (`MigratedDatabaseSchemaChecksTests`, `MigrationOrderCommuteTests`, `BaselineSqlServerVersionTests`), since CI does not run them.
5. Report: each file converted (original, new ID, target version, `ReplacesHotFixNumber`), the step 10 and step 11 reports, and anything moved to startup code or a job.
