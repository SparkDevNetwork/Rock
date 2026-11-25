---
name: migration
description: >-
  Create, write, review or convert Rock RMS SQL migrations in
  Rock.Data.Migrations/Migrations/. Creates the migration file itself (there is
  no Add-Migration), writes the Up() body as raw SQL and RockMigrationHelper
  calls, reviews an existing migration, and converts an EF6 migration or a
  HotFix that arrives on develop through a merge-up. On request, gives the SQL
  to forget a migration so it runs again, or tries to write SQL that undoes it.
  Use when the user says "write the migration", "create a migration", "new
  migration", "review the migration", "check my migration", "convert this
  migration", "convert the HotFix", "forget the migration", "undo the
  migration", or describes schema changes, data migrations, or platform entity
  changes. Do NOT use for HotFixes on release or hotfix branches, which still use
  Rock/Plugin/HotFixes; use /plugin-migration there. Do NOT use for standalone
  SQL scripts; use /sql instead.
argument-hint: "Describe what the migration should do, or say 'review', 'convert <file or HotFix number>', 'forget <migration>' or 'undo <migration>'."
compatibility: Requires Claude Code with access to the Rock RMS codebase.
metadata:
  version: "2.0"
  author: "Maxwell Eley"
---

# Rock RMS SQL Migration Builder, Reviewer and Converter

You are working with Rock's SQL migrations. Each migration is a C# class in the `Rock.Data.Migrations` project that only emits SQL. The SQL migration runner applies every migration not yet recorded in `[RockMigration]`, ordered by target Rock version and then ID, each in its own transaction. The design is in `specs/261027-sql-migrations-on-net-framework.md`; read the section you need when a rule here is not enough.

EF6 migrations are gone on this branch: there is no `Add-Migration`, no `Down()`, no `.Designer.cs` or `.resx`, and no EF schema methods (`AddColumn`, `CreateTable` and so on). Schema changes are raw SQL.

**The user's request:** $ARGUMENTS

---

## Reference Routing Table

Load reference files progressively, only when needed.

| Reference File | Load When |
|---|---|
| `references/sql-ddl-patterns.md` | Before writing or reviewing any schema change or data migration |
| `references/migration-helper-methods.md` | When using RockMigrationHelper for pages, blocks, attributes, jobs and so on |
| `references/common-pitfalls.md` | Before finalizing any migration, in every mode |
| `references/conversion-checklist.md` | Convert mode only |
| `references/development-undo.md` | Forget or undo mode, and for the backup reminder's SQL |

---

## Step 1: Determine the Mode

| Mode | When |
|---|---|
| **Write** | The user describes what a new migration should do, or says "create", "write", "new". |
| **Review** | The user says "review", "check", "audit", or names an existing migration without asking for changes. |
| **Convert** | The user names an EF6 migration (a `Rock.Migrations/...` file or a 15-digit ID) or a HotFix (`Rock/Plugin/HotFixes/...` or a HotFix number), or the stray-file guard (`StrayMigrationFilesTests` in `Rock.Tests`) reported files after a merge. |
| **Forget** | The user asks for the SQL to make a migration run again. Only on request. |
| **Undo** | The user asks for SQL to reverse a migration. Only on request. |

If the mode is unclear, ask.

---

# Write Mode

## Step 2: Create the Migration File

There is no `Add-Migration`; you create the file.

1. **ID.** The current UTC time as `yyyyMMddHHmmss`, for example `20261117153042`. Get it from the clock (`date -u +%Y%m%d%H%M%S`), never by incrementing another migration's ID. When writing several migrations at once, read the clock for each, waiting a second between them if needed, so no two share an ID. A unit test fails on duplicate or invalid IDs.
2. **Target version.** The version being developed: the major and minor of `<Version>` in `Directory.Build.props` (for example `21.0.2` gives `21.0`).
3. **Name.** A PascalCase description of what the migration does, unique among the migrations' class names.
4. **File.** `Rock.Data.Migrations/Migrations/Version {x.y}/{id}_{Name}.cs`. Create the `Version {x.y}` folder if it does not exist.
5. **Template:**

```csharp
// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Data.Migrations;

/// <summary>
/// [What this migration does.]
/// </summary>
[SqlMigration( {id}, "{x.y}" )]
internal class {Name} : Migration
{
    /// <summary>
    /// Operations to be performed during the upgrade process.
    /// </summary>
    public override void Up()
    {
    }
}
```

The class is `internal` and inherits `Rock.Data.Migration`. Do not add `using Rock.Plugin;`: `Migration` would then be ambiguous with `Rock.Plugin.Migration`.

## Step 3: Understand the Intent

Classify what the migration must do:

- **Schema changes:** tables, columns, keys, indexes.
- **Data migrations:** UPDATE or INSERT existing records, seed data.
- **Platform entity changes:** pages, blocks, block types, attributes, defined values, security, through `RockMigrationHelper`.
- **Work that needs C# against the database** (loading entities, calling services, anything using `RockContext`): this does NOT go in a migration. The runner throws if a migration creates a `RockContext` or uses `DbService`. Write a run-once post-update job instead, created by the migration with `RockMigrationHelper.AddPostUpdateServiceJob` and listed in `Rock/Jobs/PostUpdateJobs/DataMigrationsStartup.cs` (see `references/sql-ddl-patterns.md`).

If an entity model changes, its migration MUST land in the same change.

## Step 4: Research the Schema

Do not guess column names, types or relationships.

- **Models:** `Rock/Model/[Domain]/[Entity]/[Entity].cs`, or search `Rock/Model/**/{EntityName}.cs`. Look for `[Required]` (NOT NULL), `[MaxLength(N)]`, `[DecimalPrecision]`, navigation properties (foreign keys) and enum-typed properties. The schema must support exactly what the model maps; the mapping check verifies it.
- **Standard columns:** every `Model<T>` entity has `Id`, `Guid`, `CreatedDateTime`, `ModifiedDateTime`, `CreatedByPersonAliasId`, `ModifiedByPersonAliasId`, `ForeignId`, `ForeignGuid` and `ForeignKey`. `IsSystem` is declared per entity, not on `Model<T>`; check the model before using it. Audit columns reference `[PersonAlias]`, never `[Person]`, with no cascade. Campus foreign keys use `ON DELETE SET NULL`.
- **Existing names:** when altering or dropping an existing object, read its real name from the database or from the baseline (`Rock.Data.Migrations/Migrations/Baseline/20131125173405-01_Schema.sql` and `-03_Constraints.sql`), never assume it.
- **Examples:** the converted migrations under `Rock.Data.Migrations/Migrations/Version 20.0` show the exact SQL EF6 generated, for example `20260526172639_AddGroupPhotoId.cs`. They were converted as they were, so many lack the guards new migrations need.
- **SystemGuid** constants: `Rock/SystemGuid/`. **Enums:** `Rock.Enums/[Domain]/`.

Then read `references/sql-ddl-patterns.md`, and `references/migration-helper-methods.md` if the migration uses `RockMigrationHelper`.

## Step 5: Write Up()

Order of operations:

1. Schema changes (tables, columns, primary keys).
2. Indexes and foreign keys, after the columns and referenced tables exist.
3. Data changes.
4. Platform entity changes through `RockMigrationHelper`.

Rules (details and examples in `references/sql-ddl-patterns.md`):

- **Names follow EF6 conventions:** `PK_dbo.Table`, `FK_dbo.Table_dbo.Target_Column`, `IX_Column` (`IX_Column1_Column2` for several columns). The name ratchet fails on any new name that does not.
- **Guard every DDL statement** with `IF NOT EXISTS` / `IF EXISTS` against the `sys.*` views (or `COL_LENGTH`, `OBJECT_ID`), so a migration that is run again after a fix does not fail.
- **Everything runs in one transaction**, and statements SQL Server does not allow in a transaction (`ALTER DATABASE`, full-text index changes) are not supported.
- **`Sql()` and `SqlScalar()` run immediately, in order,** on the migration's own connection and transaction, so a `SqlScalar` sees every earlier statement of the same migration.
- **Large SQL** (stored procedures, views, long scripts) goes in a script file next to the migration, `{id}-{Suffix}.sql`, run with `SqlResource( "{Suffix}" )`. The build compresses and embeds it; never commit a `.gz`.
- **No file system, no `System.Web`:** no `System.IO`, `HostingEnvironment`, `HttpContext` or `MapPath`. Files an upgrade removes go in the update package's `deletefile.lst`.
- **Values that must differ on every install** (a `Guid` or key generated with `NEWID()` that should not be shared between installs): list the migration in the table in `Rock.Data.Migrations/README.md` in the same change.
- Break a migration with several logical operations into private methods, so `Up()` reads like a table of contents. There is no `Down()`, so there are no `_Up` / `_Down` pairs.

## Step 6: Validate

Read `references/common-pitfalls.md` and work through its checklist.

## Step 7: Output and Backup Reminder

Write the file, show it in the conversation, and end with this reminder:

> Before you run this migration, back up your database or take a SQL Server snapshot, so you can go back if it needs changing. Ask me for the snapshot SQL, or later for the SQL to forget or undo the migration.

Also remind the developer that the migration and schema check integration tests (in `Rock.Tests.Integration`: `MigratedDatabaseSchemaChecksTests`, `MigrationOrderCommuteTests`, `BaselineSqlServerVersionTests`) must pass locally before the migration merges, because CI does not run integration tests yet.

---

# Review Mode

## Step 2: Find and Read the Migration

1. If `$ARGUMENTS` names a migration (class name, ID or file), find it under `Rock.Data.Migrations/Migrations/`.
2. Otherwise, take the migration with the highest ID among files changed on the current branch (`git diff --name-only` against the merge base with `develop`), or the highest ID overall.
3. Read the whole file and any `{id}-*.sql` scripts it runs.

## Step 3: Research

Read the models of every entity it touches, the `SystemGuid` and enum files it references, `references/sql-ddl-patterns.md` and `references/common-pitfalls.md`.

## Step 4: Audit

Run the checklist in `references/common-pitfalls.md`, plus:

- **Structure:** copyright header, file-scoped `Rock.Data.Migrations` namespace, `internal` class inheriting `Migration`, `[SqlMigration( id, "x.y" )]`, file name `{id}_{Name}.cs` in the `Version x.y` folder matching the target version, XML doc comments.
- **Cross-references:** columns match the model; `SystemGuid` constants exist; other migrations on the branch that touch the same objects do not depend on running in a particular order (the commute check builds the database in both orders).

## Step 5: Report

| Severity | Meaning | Examples |
|---|---|---|
| **CRITICAL** | Must fix: errors, data loss, or a failed check | Non-EF6 name, `RockContext` in a migration, wrong column type, wrong block type method, unguarded DDL that fails on rerun |
| **WARNING** | Should fix | Magic GUIDs instead of `SystemGuid`, hard-coded `[Order]`, missing README entry for a per-install value |
| **NOTE** | Consider | Missing XML doc comment, repeated GUID that could be a `const` |

Format each finding as:

```
[SEVERITY-N] Short title
Description of the issue and what to change.
Line/area: [where in the file]
```

End with totals by severity and a go/no-go. If the migration is clean, say so; do not invent issues.

---

# Convert Mode

Used when an EF6 migration or a HotFix reaches develop from a branch that still writes them: a merge-up from a release or hotfix branch, or a feature branch that started before the switch. Git usually reports these as "file location" conflicts and moves the files into `Rock.Data.Migrations/Migrations/Version 20.0/`, or adds them under `Rock.Migrations/` or `Rock/Plugin/HotFixes/`; the stray-file guard fails on them wherever they land.

Read `references/conversion-checklist.md` and follow it exactly. It covers the ID, target version, shell, schema calls, scripts, `SqlScalar` timing, the HotFix-specific steps, and the two reports this mode MUST make:

- **HotFixes already converted on their own** (checklist item 10): list each HotFix number whose code is in a converted rollup and also already has its own migration, and ask for a review that its code is safe to run twice.
- **HotFix target version** (checklist item 11): report any HotFix whose `MigrationNumber` version differs from the version it was introduced in by major version, or by more than 1 minor version. Convert with the written version anyway.

After converting, delete the original files, run the `Rock.Tests` migration tests (`Rock.Tests.Data.SqlMigrations`), and give the backup reminder from Write Mode Step 7.

---

# Forget Mode (on request)

Give the SQL that deletes the migration's `[RockMigration]` row, so the runner runs it again at the next start. Use `references/development-undo.md`. Say plainly that this does not undo anything the first run did: it is for rerunning a corrected migration whose statements are guarded.

# Undo Mode (on request)

Try to write SQL that reverses the migration and then deletes its `[RockMigration]` row, using `references/development-undo.md`. Say plainly that it is a best effort nothing tests, that a data migration often cannot be reversed because the original values are gone, and that restoring the backup or snapshot is the reliable way back. Give the SQL in the conversation or as a scratch file outside the repository; never commit it, and never put a `.sql` file in `Rock.Data.Migrations` without a migration (the orphan-script check fails on it).

---

## Examples

### Example 1: Add a column

User says: "I added an AreDuplicateRegistrantsPrevented bool to RegistrationTemplate."

1. Create `Version 21.0/{id}_AddRegistrationTemplateAreDuplicateRegistrantsPrevented.cs`.
2. Read the model: `bool`, not nullable.
3. `Up()`: guarded `ALTER TABLE [dbo].[RegistrationTemplate] ADD [AreDuplicateRegistrantsPrevented] [bit] NOT NULL DEFAULT 0`.

### Example 2: New page and Obsidian block

User says: "Add a page for MCP Server configuration with an Obsidian block."

1. Read `references/migration-helper-methods.md`.
2. `Up()`: `UpdateEntityType`, `AddOrUpdateEntityBlockType`, `AddPage`, `AddOrUpdatePageRoute`, `AddBlock`, each as its own private method.

### Example 3: Convert a merged-up HotFix

User says: "The guard failed on 349_SecureEventAndLmsAISkills.cs after merging develop."

1. Read `references/conversion-checklist.md`.
2. ID from the HotFix file's first commit, target version from its `MigrationNumber` (report it if it is far from where it was introduced), `ReplacesHotFixNumber = 349`.
3. Write the migration, delete the HotFix file, run the `Rock.Tests` migration tests.

### Example 4: Forget a migration

User says: "Forget 20261117153042 so it runs again."

Give the `DELETE` from `[RockMigration]` in `references/development-undo.md`, with the warning that it does not undo the first run.

---

## Troubleshooting

**The stray-file guard fails after a merge:** a merged branch brought an EF6 migration or HotFix. Use Convert Mode on each file it lists.

**Duplicate or invalid migration ID:** `SqlMigrationFileTests` in `Rock.Tests` reports it. Give the newer migration a new ID from the clock.

**"could not find its script":** the script's file name must be `{id}-{Suffix}.sql` with the migration's own ID, next to the migration, and `SqlResource` must be passed the suffix only.

**Name ratchet fails:** a new primary key, foreign key or index does not have its EF6 name. Rename it in the migration; do not add it to the allow list.

**Mapping check fails:** the schema does not support what the model maps (missing column, wrong type, length or nullability). Fix the migration or the model.

**Commute check fails:** two migrations change the same object and the result depends on their order. Make the later one independent of the earlier one.

**RockMigrationHelper signature:** check `Rock/Data/MigrationHelper.cs` for the exact overload.
