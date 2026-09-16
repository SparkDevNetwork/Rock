---
name: migration
description: >-
  Write or review Up() and Down() method bodies for Rock RMS Entity Framework
  migrations in Rock.Migrations/Migrations/. The developer must first scaffold
  files via 'Add-Migration'. Use when the user says "write the migration",
  "EF migration", "review the migration", "check my migration", or describes
  schema changes, data migrations, or platform entity changes for an EF migration.
  Do NOT use for plugin/hotfix migrations — use /plugin-migration instead.
  Do NOT use for standalone SQL scripts — use /sql instead.
argument-hint: "Describe what the migration should do, or say 'review' to review an existing one. You must first run 'Add-Migration YourName' in Package Manager Console to generate the .cs/.Designer.cs/.resx files."
compatibility: Requires Claude Code with access to the Rock RMS codebase for reading model files and editing migration files.
metadata:
  version: "1.3"
  author: "Maxwell Eley"
---

# Rock RMS EF Migration Builder & Reviewer

You are writing or reviewing the Up() and Down() method bodies for an Entity Framework migration in the Rock RMS codebase. The developer has already scaffolded the migration files via `Add-Migration` in Package Manager Console — your job is to either write the migration logic or review what's already been written.

**The user's request:** $ARGUMENTS

---

## Reference Routing Table

Load reference files progressively — only when needed.

| Reference File | Load When |
|---|---|
| `references/ef-patterns.md` | Before writing any migration logic (always in write mode) |
| `references/migration-helper-methods.md` | When using RockMigrationHelper for pages, blocks, attributes, etc. |
| `references/common-pitfalls.md` | Before finalizing any migration — both write and review modes |

**Do NOT read all files upfront.** Read `ef-patterns.md` always for write mode; read others only when their trigger condition is met.

---

## Step 1 — Verify Migration Files Exist

This is a hard gate. Before doing anything else:

1. If `$ARGUMENTS` includes a migration name, search for it in `Rock.Migrations/Migrations/`
2. Otherwise, find the most recently created .cs migration file (highest timestamp prefix):
   ```
   Glob: Rock.Migrations/Migrations/*.cs (exclude *.Designer.cs)
   ```
3. Confirm the main .cs, .Designer.cs, and .resx files all exist for that migration

**If files are NOT found, stop immediately and tell the user:**

> I couldn't find scaffolded migration files. Before using this skill, you need to run `Add-Migration YourMigrationName` in the Package Manager Console (targeting the Rock.Migrations project). This generates the .cs, .Designer.cs, and .resx files. Once that's done, invoke `/migration` again.

Do NOT proceed past this step without confirmed migration files.

## Step 2 — Read the Migration and Determine Mode

- Read the main .cs file fully to get the class name and current contents of Up() and Down()
- Note the class structure: `public partial class {Name} : Rock.Migrations.RockMigration`

**Now determine which mode to use:**

- **Write mode** — Up() and Down() are empty, minimal (only auto-scaffolded schema changes), or the user explicitly asked to "write" or "build" the migration. Proceed to **Step 3 (Write Mode)**.
- **Review mode** — Up() and Down() already contain substantial logic (developer has written it) and the user said "review", "check", "audit", or just invoked `/migration` without describing what to write. Proceed to **Step 3 (Review Mode)**.

If you can't tell which mode, look at the user's `$ARGUMENTS`. If they describe what the migration *should do*, it's write mode. If they ask to review, check, or validate, it's review mode. When in doubt, ask.

---

# Write Mode

## Step 3 (Write Mode) — Understand the User's Intent

Parse what the migration needs to accomplish from `$ARGUMENTS` and conversation context:

- **Schema changes** — add/modify/drop columns, tables, indexes, FKs
- **Data migrations** — UPDATE existing records, seed new data
- **Platform entity changes** — add pages, blocks, block types, attributes via MigrationHelper
- **Mixed** — schema + data + platform (common in rollup migrations)

If the intent is unclear, ask clarifying questions before proceeding.

## Step 4 — Research the Schema

Before writing migration logic, you must understand the actual database schema. Do not guess column names, types, or relationships.

**Read the model files:**
- Entity models: `Rock/Model/[Domain]/[Entity]/[Entity].cs`
- If not found at that path, search: `Rock/Model/**/{EntityName}.cs`
- Look for: `[Required]` (NOT NULL), `[MaxLength(N)]` (string limits), FK navigation properties, enum-typed properties, default values

**Understand Rock's standard column patterns:**
- Every entity inheriting `Model<T>` has: `Id`, `Guid`, `IsSystem`, `CreatedDateTime`, `ModifiedDateTime`, `CreatedByPersonAliasId`, `ModifiedByPersonAliasId`
- Audit FK columns reference `[PersonAlias]`, NOT `[Person]` — no cascade delete
- Campus FK columns typically use `ON DELETE SET NULL`
- `[Order]` columns should be calculated from existing max, not hardcoded to 0

**Check SystemGuid files** if referencing well-known entities: `Rock/SystemGuid/{EntityType}.cs`

**Check enum definitions** if a column uses an enum type: `Rock.Enums/[Domain]/`

**Load references:**
- Read `references/ef-patterns.md` for EF methods and correct migration patterns
- Read `references/migration-helper-methods.md` if using RockMigrationHelper

## Step 5 — Write the Up() Method

Build the Up() body following this order of operations:

1. **Schema changes first** — AddColumn, CreateTable, AlterColumn, CreateIndex
2. **Foreign key additions** — AddForeignKey (after the column and referenced table exist)
3. **Data migrations** — Sql() calls for UPDATE/INSERT
4. **Platform entity changes** — RockMigrationHelper calls for pages, blocks, attributes

For the full list of available EF methods and their signatures, consult `references/ef-patterns.md`. For RockMigrationHelper method signatures, consult `references/migration-helper-methods.md`.

### Raw SQL (via Sql() method)

- Follow same formatting rules as the `/sql` skill (UPPERCASE keywords, bracketed identifiers)
- Use IF EXISTS / IF NOT EXISTS guards for idempotency
- Use string interpolation `$@"..."` for multi-line SQL with embedded constants

### Code organization — private methods for each logical operation

Every non-trivial migration should break its logic into private methods — one per logical operation, with matching Up/Down pairs. The Up() and Down() methods themselves should read like a table of contents:

```csharp
public override void Up()
{
    AddConnectionsPages_Up();
    AddConnectionNavigationViewBlocks_Up();
    AddConnectionRequestNoteType_Up();
    UpdateConnectionProperties();
}

public override void Down()
{
    AddConnectionNavigationViewBlocks_Down();
    AddConnectionsPages_Down();
}
```

**Naming convention:** Developer initials prefix (optional) + descriptive name + `_Up()`/`_Down()` suffix.

**When to use:** Any migration with more than one logical operation, always for rollups, or when logic is complex enough to benefit from a named method.

**Additional organization:**
- Use `private const string` at class level for GUIDs referenced multiple times
- Use `#region Private Methods` / `#endregion` in complex migrations

## Step 6 — Write the Down() Method

Build the Down() body as the **exact reverse** of Up():

1. Every operation in Up() must have a matching reverse in Down()
2. Operations must be in **REVERSE order** of Up()
3. Reversal mapping:
   - AddColumn / DropColumn
   - CreateIndex / DropIndex
   - AddForeignKey / DropForeignKey
   - CreateTable / DropTable
   - AlterColumn (nullable) / AlterColumn (not nullable) with data fixup SQL first
   - RockMigrationHelper.Add* / RockMigrationHelper.Delete*
   - Data INSERT / Data DELETE (or document why rollback isn't needed)

**Edge cases:**
- If Up() makes a column nullable, Down() must first UPDATE any NULL values to a safe default before making it NOT NULL again
- If the migration is a data repair/fix, Down() can be empty with a comment explaining why rollback isn't applicable

## Step 7 — Validate

Read `references/common-pitfalls.md` and run through this checklist before presenting:

1. Up() and Down() are both non-empty (or Down() has an intentional comment explaining why it's empty)
2. Down() reverses Up() in correct reverse order
3. All `AddColumn` types match the model's `[Required]`, `[MaxLength]`, nullable attributes
4. NOT NULL columns on existing tables have `defaultValue` or `defaultValueSql`
5. FK cascade behavior is correct (PersonAlias audit FKs: no cascade; Campus FKs: SET NULL)
6. `Rock.SystemGuid` constants used instead of magic string GUIDs where available
7. Raw SQL follows Rock formatting conventions — string escaping correct in `$@"..."` strings
8. MigrationHelper method parameters match expected signatures
9. No schema operations reference tables/columns that don't exist yet in that migration's context
10. Index names are consistent (EF convention: `IX_TableName_ColumnName`)
11. Copyright header, XML doc comments, and `partial class` keyword are present
12. Block type methods match the block type: `AddOrUpdateEntityBlockType()` for Obsidian/Mobile blocks, `UpdateBlockType()`/`UpdateBlockTypeByGuid()` for WebForms only
13. All EF method table names include the `dbo.` prefix
14. Assembly-qualified names in `UpdateEntityType()` use the correct version number for the current branch

**Output:** Edit the existing .cs migration file in place with the completed Up() and Down() bodies. Also show the complete file in conversation for review.

---

# Review Mode

## Step 3 (Review Mode) — Research for Comparison

Before reviewing, gather the context needed to judge correctness:

- Read the actual model files (`Rock/Model/[Domain]/[Entity]/[Entity].cs`) for every entity the migration touches — verify column types, `[Required]`, `[MaxLength]`, nullable, FK navigation properties
- Check `Rock/SystemGuid/` files if the migration references GUIDs
- Check `Rock.Enums/[Domain]/` if the migration uses enum integer values
- Read `references/ef-patterns.md` for the expected patterns
- Read `references/common-pitfalls.md` for known failure modes

## Step 4 (Review Mode) — Audit the Migration

Run through the **same 14-point validation checklist from Step 7 above**, plus these additional review-specific checks:

### Structural checks
- Class inherits `Rock.Migrations.RockMigration`
- Copyright header and XML doc comments present

### Cross-reference checks
- If migration adds columns, verify the model has matching properties
- If migration references SystemGuid constants, verify they exist
- Check for potential conflicts with other recent migrations on the same tables

## Step 5 (Review Mode) — Report Findings

Present findings using severity levels:

| Severity | Meaning | Examples |
|---|---|---|
| **CRITICAL** | Must fix before merging — will cause errors or data loss | Missing Down() reversal for schema change, wrong FK cascade, wrong column type |
| **WARNING** | Should fix — could cause issues or violates conventions | Missing IF NOT EXISTS guard, magic string GUIDs instead of SystemGuid, inconsistent ordering |
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

### Example 1: Simple column addition

User says: "I added an AreDuplicateRegistrantsPrevented bool to RegistrationTemplate"

Actions:
1. Read RegistrationTemplate model for column definition
2. Write `AddColumn("dbo.RegistrationTemplate", "AreDuplicateRegistrantsPrevented", c => c.Boolean(nullable: false));` in Up()
3. Write `DropColumn("dbo.RegistrationTemplate", "AreDuplicateRegistrantsPrevented");` in Down()

### Example 2: Platform entity changes (new page + block)

User says: "Add a new page for MCP Server configuration with an Obsidian block"

Actions:
1. Read references/migration-helper-methods.md
2. Write Up() with: UpdateEntityType, AddOrUpdateEntityBlockType, AddPage, AddOrUpdatePageRoute, AddBlock
3. Write Down() in reverse: DeleteBlock, DeleteBlockType, DeletePage

### Example 3: Review an existing migration

User says: "review my migration" or `/migration review`

Actions:
1. Find the most recent migration in Rock.Migrations/Migrations/
2. Read the .cs file — Up() and Down() already have logic
3. Enter Review Mode: research models, audit against validation checklist + common pitfalls
4. Report findings by severity (CRITICAL/WARNING/NOTE) with go/no-go recommendation

### Example 4: Nullable FK change

User says: "Make BlockId nullable on HtmlContent with ON DELETE SET NULL"

Actions:
1. Up(): DropForeignKey, AlterColumn (nullable), custom Sql() for new FK constraint with ON DELETE SET NULL
2. Down(): UPDATE NULLs to safe default, DropForeignKey, DropIndex, AlterColumn (not nullable), CreateIndex, AddForeignKey with cascadeDelete

---

## Troubleshooting

**"Migration files not found":** User needs to run `Add-Migration YourName` in Package Manager Console first.

**Auto-scaffolded content looks wrong:** EF sometimes misdetects model changes. Read the actual model file to verify what columns/types changed.

**MigrationHelper method signature mismatch:** Check `Rock/Data/MigrationHelper.cs` (~9,500 lines) for the exact overload and parameter order.

**Down() data loss concern:** If Down() can't safely reverse a data migration, add a comment explaining why and leave that portion empty.

**Conflict with existing migration:** If another migration in the same release touches the same table, coordinate the order of operations carefully.
