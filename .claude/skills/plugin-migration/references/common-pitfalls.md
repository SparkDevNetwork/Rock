# Plugin Migration Common Pitfalls

Known failure modes and danger areas for Rock plugin migrations. Review this checklist before finalizing any plugin migration — whether writing or reviewing.

---

## Pitfall 1: Wrong Block Type Method — DATA LOSS RISK

**The bug:** Using `UpdateBlockTypeByGuid()` for Obsidian or Mobile blocks. This method contains `DELETE FROM [BlockType] WHERE [Path] = '{path}'`. Entity-based block types (Obsidian/Mobile) have an empty `Path` value, so this DELETE can wipe out ALL entity-based block types with an empty path.

**Rule:**
- **Obsidian/Mobile blocks** (`Rock.Blocks.*`, `Rock.Blocks.Types.Mobile.*`) → `AddOrUpdateEntityBlockType()`
- **WebForms blocks** (`~/Blocks/*.ascx`) → `UpdateBlockType()` or `UpdateBlockTypeByGuid()`

This exact bug caused data loss in a production migration. Always verify the block type before choosing the method.

---

## Pitfall 2: String Escaping in SQL

### Single quotes
SQL string literals need doubled single quotes inside C# verbatim strings:
```csharp
// WRONG — C# compiles but SQL fails at runtime
Sql( @"UPDATE [Person] SET [LastName] = 'O'Brien'" );

// CORRECT
Sql( @"UPDATE [Person] SET [LastName] = 'O''Brien'" );
```

### Curly braces in interpolated verbatim strings
When using `$@"..."`, literal curly braces must be doubled:
```csharp
// WRONG — C# compilation error
Sql( $@"UPDATE [Attribute] SET [DefaultValue] = '{ ""key"": ""value"" }'" );

// CORRECT
Sql( $@"UPDATE [Attribute] SET [DefaultValue] = '{{""key"": ""value""}}'" );
```

---

## Pitfall 3: Missing `using Rock.Model;`

If the migration uses `typeof()` for job types or entity types (e.g., in `AddPostUpdateServiceJob()`), the `using Rock.Model;` statement is required. Without it, the code compiles but the type reference is wrong.

```csharp
// WRONG — missing using statement, typeof may not resolve correctly
RockMigrationHelper.AddPostUpdateServiceJob(
    jobType: typeof( Rock.Jobs.PostUpdateJobs.PostV183UpdateAchievementTypes ).FullName,
    ...
);

// CORRECT — add using at top of file
using Rock.Model;
```

---

## Pitfall 4: GUID Format Errors

Rock GUIDs must be:
- **Uppercase** letters (A-F, not a-f)
- **Hyphenated** format: `XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX`
- **Proper length**: 36 characters including hyphens

```csharp
// WRONG — lowercase
"18dbda15-5ed7-4fe8-bc30-da872f6a3c22"

// CORRECT — uppercase
"18DBDA15-5ED7-4FE8-BC30-DA872F6A3C22"
```

When generating new GUIDs, use `Guid.NewGuid().ToString().ToUpper()` format.

**Exception:** Some historical migrations use lowercase GUIDs. Match the casing convention of the entity you're referencing — check `Rock/SystemGuid/` files.

---

## Pitfall 5: Migration Number Conflicts

If two developers create plugin migrations at the same time, they may pick the same number. Before writing:

1. Pull latest from the branch
2. Re-scan `Rock/Plugin/HotFixes/` for the actual highest number
3. Use highest + 1

If a conflict is discovered after the fact, the second migration must be renumbered (both filename and `[MigrationNumber]` attribute).

---

## Pitfall 6: Hardcoded Order Values

When inserting records that have an `[Order]` column, do NOT hardcode `0` or any fixed value. Calculate from the existing maximum:

```sql
DECLARE @Order INT = (SELECT ISNULL(MAX([Order]), 0) + 1 FROM [YourTable] WHERE [ParentId] = @ParentId)
```

---

## Pre-Finalization Checklist

Run through this before presenting the migration:

1. Migration number doesn't conflict with any existing file in `Rock/Plugin/HotFixes/`
2. `[MigrationNumber]` attribute number matches the filename number
3. Class name matches filename (without the number prefix and underscore)
4. Minimum Rock version is valid and appropriate
5. All GUIDs are proper format (uppercase, hyphenated)
6. SQL follows Rock conventions (UPPERCASE keywords, brackets, JOIN syntax)
7. IF NOT EXISTS guards used where appropriate for idempotency
8. `using` statements are correct (System always, Rock.Model if using typeof())
9. Copyright header is complete
10. XML doc comments describe the migration's purpose
11. Block type methods match the block type (entity-based vs path-based)
12. No `UpdateBlockTypeByGuid()` used for Obsidian/Mobile blocks
