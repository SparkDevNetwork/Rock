---
name: plugin-migration
description: >-
  Create or review Rock RMS plugin migrations (hotfixes) in Rock/Plugin/HotFixes/.
  Plugin migrations are single .cs files with sequential numbering and a [MigrationNumber] attribute.
  Use when the user says "create a plugin migration", "plugin migration", "hotfix migration",
  "new hotfix", "write a hotfix", "review plugin migration", "check this hotfix",
  "audit hotfix migration", or needs to create a data fix, configuration change,
  attribute update, or stored procedure update that ships as a plugin migration.
  Also use when the user describes a fix or change that targets Rock/Plugin/HotFixes/.
  Do NOT use for EF migrations in Rock.Migrations/ — use /migration instead.
  Do NOT use for standalone SQL scripts — use /sql instead.
argument-hint: "Describe what the plugin migration should do, or say 'review' to review an existing one (e.g., 'fix the HTML content for the SQL runner warning block')"
compatibility: Requires Claude Code with access to the Rock RMS codebase.
metadata:
  version: "1.3"
  author: "Maxwell Eley"
---

# Rock RMS Plugin Migration Creator & Reviewer

You are creating or reviewing a plugin migration (hotfix) in the Rock RMS codebase. Plugin migrations are single .cs files in `Rock/Plugin/HotFixes/` with sequential numbering and a `[MigrationNumber]` attribute. They inherit from `Rock.Plugin.Migration` and provide `Up()` and `Down()` methods.

**The user's request:** $ARGUMENTS

---

## Reference Routing Table

Load reference files progressively — only when needed.

| Reference File | Load When |
|---|---|
| `references/hotfix-patterns.md` | Before writing migration logic (always in write mode) |
| `references/methods-and-sql.md` | When choosing methods, writing SQL, or working with stored procedures |
| `references/common-pitfalls.md` | Before finalizing any migration — both write and review modes |

**Do NOT read all files upfront.** Read `hotfix-patterns.md` always for write mode; read others only when their trigger condition is met.

---

## Step 1 — Understand the Request and Determine Mode

Parse what the migration needs to do and determine which mode to use:

**Write mode** — The user describes what the migration should accomplish, or says "create", "write", "new". Proceed to **Step 2 (Write Mode)**.

**Review mode** — The user says "review", "check", "audit", or names an existing migration file. Proceed to **Step 2 (Review Mode)**.

Migration categories (for write mode):
- **Data fix** — UPDATE/INSERT/DELETE records to correct bad data
- **Configuration change** — update page layouts, block settings, HTML content
- **Attribute update** — add/modify block type attributes, person attributes, defined values
- **Stored procedure update** — create or alter a stored procedure
- **Post-update job** — add a run-once service job
- **Block type change** — add/update/remove block types and entity types

If ambiguous, ask clarifying questions before proceeding.

---

# Write Mode

## Step 2 — Determine Migration Number and Version

1. Scan `Rock/Plugin/HotFixes/` for the highest existing migration number:
   ```
   Glob: Rock/Plugin/HotFixes/*.cs
   ```
   Parse the leading number from each filename (e.g., `279_MigrationRollupsForV18_3_0_1.cs` → 279).

2. Next number = highest + 1

3. Determine minimum Rock version:
   - If the user specifies a version, use it
   - If working on a specific release branch, use that version
   - If unclear, ask the user

4. Generate filename: `{number}_{PascalCaseDescription}.cs`
   - Class name = the PascalCase description (without the number prefix)

## Step 3 — Research the Schema

Before writing the migration, you must understand the actual database schema. Do not guess column names, types, or relationships.

**Read the model files for every entity you're touching:**
- Entity models: `Rock/Model/[Domain]/[Entity]/[Entity].cs`
- If not found, search: `Rock/Model/**/{EntityName}.cs`
- Look for: `[Required]` (NOT NULL), `[MaxLength(N)]` (string limits), FK navigation properties, enum-typed properties

**Understand Rock's standard column patterns:**
- Every entity inheriting `Model<T>` has: `Id` (identity), `Guid` (uniqueidentifier), `IsSystem` (bit), `CreatedDateTime`, `ModifiedDateTime`, `CreatedByPersonAliasId`, `ModifiedByPersonAliasId`
- Audit FK columns (`CreatedByPersonAliasId`, `ModifiedByPersonAliasId`) reference `[PersonAlias]`, NOT `[Person]`
- Campus FK columns typically use `ON DELETE SET NULL`
- `[Order]` columns should be calculated from existing max, not hardcoded

**Check SystemGuid files:** `Rock/SystemGuid/{EntityType}.cs` for well-known GUIDs

**Check enum definitions:** `Rock.Enums/[Domain]/` for integer values of enum-typed columns

**Load references:**
- Read `references/hotfix-patterns.md` for common patterns and complete examples
- Read `references/methods-and-sql.md` if using RockMigrationHelper, stored procedures, or complex SQL

## Step 4 — Write the Migration File

Use the following template. The copyright block, namespace, class structure, and XML doc comments must match exactly:

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

using System;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// [Description of what this migration does]
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( {number}, "{minRockVersion}" )]
    public class {ClassName} : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Migration logic here
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}
```

For available methods (`Sql()`, `SqlScalar()`, `RockMigrationHelper.*`, EF wrappers), SQL formatting rules, string escaping, stored procedure patterns, and code organization conventions, consult `references/methods-and-sql.md`.

## Step 5 — Validate

Read `references/common-pitfalls.md` and run through the pre-finalization checklist before presenting.

**Output:** Write the .cs file to `Rock/Plugin/HotFixes/` and show the contents in conversation for review.

---

# Review Mode

## Step 2 (Review Mode) — Find the Migration

1. If `$ARGUMENTS` names a specific file or migration number, find it in `Rock/Plugin/HotFixes/`
2. Otherwise, find the most recently created .cs file (highest number prefix):
   ```
   Glob: Rock/Plugin/HotFixes/*.cs
   ```
3. Read the full .cs file

## Step 3 (Review Mode) — Research for Comparison

Before reviewing, gather the context needed to judge correctness:

- Read the actual model files (`Rock/Model/[Domain]/[Entity]/[Entity].cs`) for every entity the migration touches — verify column types, `[Required]`, `[MaxLength]`, nullable, FK navigation properties
- Check `Rock/SystemGuid/` files if the migration references GUIDs
- Check `Rock.Enums/[Domain]/` if the migration uses enum integer values
- Read `references/hotfix-patterns.md` for expected patterns
- Read `references/common-pitfalls.md` for known failure modes

## Step 4 (Review Mode) — Audit the Migration

Run through the **pre-finalization checklist from `references/common-pitfalls.md`**, plus these additional review-specific checks:

### Structural checks
- Class inherits `Rock.Plugin.Migration`
- Copyright header and XML doc comments present
- `[MigrationNumber]` attribute present with valid number and version

### Cross-reference checks
- If migration references SystemGuid constants, verify they exist
- If migration uses RockMigrationHelper methods, verify parameter order matches method signatures
- Check for potential conflicts with other recent migrations

## Step 5 (Review Mode) — Report Findings

Present findings using severity levels:

| Severity | Meaning | Examples |
|---|---|---|
| **CRITICAL** | Must fix — will cause errors or data loss | Wrong block type method, missing string escaping, SQL syntax error |
| **WARNING** | Should fix — could cause issues or violates conventions | Missing IF NOT EXISTS guard, magic string GUIDs, wrong GUID casing |
| **NOTE** | Consider — minor improvement or style suggestion | Missing XML doc comment, could use `private const string` for repeated GUIDs |

Format each finding as:

```
[SEVERITY-N] Short title
Description of the issue and what to change.
Line/area: [where in the file]
```

End with a summary: total findings by severity and a go/no-go recommendation.

**If the migration is clean:** Say so. Don't invent issues to fill a report.

---

## Examples

### Example 1: Fix HTML content

User says: "Fix the SQL runner warning block HTML to use proper Bootstrap classes"

Actions:
1. Determine next migration number
2. Research the HtmlContent entity model
3. Write Sql() with UPDATE on [HtmlContent] by known Guid

Result: File written to `Rock/Plugin/HotFixes/280_FixSqlRunnerWarningHtml.cs` with a Sql() UPDATE on [HtmlContent] targeting the known Guid.

### Example 2: Add a block type attribute

User says: "Add a 'ShowHeader' boolean attribute to the PersonBio block type"

Actions:
1. Look up PersonBio block type Guid in SystemGuid
2. Use RockMigrationHelper.AddOrUpdateBlockTypeAttribute()

Result: File written with `RockMigrationHelper.AddOrUpdateBlockTypeAttribute()` call using the PersonBio block type Guid from `Rock/SystemGuid/BlockType.cs`.

### Example 3: Add a post-update service job

User says: "Add a run-once job to fix broken achievement types from the v18.0 bug"

Actions:
1. Add `using Rock.Model;` for typeof() reference
2. Use RockMigrationHelper.AddPostUpdateServiceJob() with job type, cron expression, and SystemGuid

Result: File written with `AddPostUpdateServiceJob()` using `typeof(Rock.Jobs.PostUpdateJobs.PostV183UpdateAchievementTypes).FullName` and the correct SystemGuid.

### Example 4: Update a stored procedure

User says: "Update the SynchronizeListRecipients proc to exclude archived members"

Actions:
1. Read `references/methods-and-sql.md` for stored procedure pattern
2. Save/restore ANSI_NULLS and QUOTED_IDENTIFIER
3. Drop existing procedure if exists
4. Create updated procedure (inline or via HotFixMigrationResource)

Result: File written with the full stored procedure pattern including session setting save/restore and IF EXISTS guard.

### Example 5: Review an existing plugin migration

User says: "review my plugin migration" or `/plugin-migration review`

Actions:
1. Find the most recent migration in `Rock/Plugin/HotFixes/`
2. Read the .cs file
3. Enter Review Mode: research models, audit against checklist + common pitfalls
4. Report findings by severity (CRITICAL/WARNING/NOTE) with go/no-go recommendation

Result: Severity-based findings report with actionable fix suggestions and a go/no-go recommendation.

---

## Troubleshooting

**Migration number conflict:** Another developer may have used that number. Re-scan the directory for the latest number after pulling latest.

**"Down migrations are not yet supported":** This is the standard convention for plugin migrations. Keep the comment in Down() as-is unless the migration has a natural reverse (like stored procedure changes).

**Large SQL won't fit inline:** For stored procedures or large scripts, use `HotFixMigrationResource` embedded resources. Add the SQL to the .resx file and reference it as `HotFixMigrationResource._NNN_Description_ProcedureName`.

**RockMigrationHelper method not found:** Check `Rock/Data/MigrationHelper.cs` for the exact method signature and overloads.

**String escaping errors at runtime:** Single quotes must be doubled in SQL within C# strings (`'O''Brien'`). Curly braces must be doubled in `$@"..."` strings (`{{`, `}}`). See `references/common-pitfalls.md` for full examples.

**Wrong block type method (data loss risk):** Never use `UpdateBlockTypeByGuid()` for Obsidian or Mobile blocks — it can delete entity-based block types. Use `AddOrUpdateEntityBlockType()` instead. See `references/common-pitfalls.md` for details.

**Missing `using Rock.Model;`:** Required when using `typeof()` for job types or entity types. Without it, the type reference may not resolve correctly.
