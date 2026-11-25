# Plugin Migration Common Pitfalls

Known failure modes and danger areas for Rock plugin migrations. Review this checklist before finalizing any plugin migration, whether writing or reviewing.

---

## Pitfall 1: Wrong Block Type Method (DATA LOSS RISK)

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
// WRONG: C# compiles but SQL fails at runtime
Sql( @"UPDATE [Person] SET [LastName] = 'O'Brien'" );

// CORRECT
Sql( @"UPDATE [Person] SET [LastName] = 'O''Brien'" );
```

### Curly braces in interpolated verbatim strings
When using `$@"..."`, literal curly braces must be doubled:
```csharp
// WRONG: C# compilation error
Sql( $@"UPDATE [Attribute] SET [DefaultValue] = '{ ""key"": ""value"" }'" );

// CORRECT
Sql( $@"UPDATE [Attribute] SET [DefaultValue] = '{{""key"": ""value""}}'" );
```

---

## Pitfall 3: Job Type Names

Pass `typeof( <fully qualified type> ).FullName` as the `jobType` of `AddPostUpdateServiceJob()`, as migration 278 does. A fully qualified name resolves without any extra `using` statement.

`using Rock.Model;` is needed only when calling helpers that take the `Rock.Model.SpecialRole` enum (`AddSecurityAuthForBlock`, `BinaryFileType`, `GroupType`, `Calendar`, `RestController`, `RestAction` and `ContentChannel`). The EntityType, Page, Site, Attribute, Category, NoteType and AI helpers take an `int` instead.

---

## Pitfall 4: GUID Format Errors

Rock GUIDs must be:
- **Uppercase** letters (A-F, not a-f)
- **Hyphenated** format: `XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX`
- **Proper length**: 36 characters including hyphens

```csharp
// WRONG: lowercase
"18dbda15-5ed7-4fe8-bc30-da872f6a3c22"

// CORRECT: uppercase
"18DBDA15-5ED7-4FE8-BC30-DA872F6A3C22"
```

When generating new GUIDs, use `Guid.NewGuid().ToString().ToUpper()` format.

**Exception:** Some historical migrations use lowercase GUIDs. Match the casing convention of the entity you're referencing: check `Rock/SystemGuid/` files.

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

## Pitfall 7: Code That Is Not Safe to Run Twice

A HotFix's code MUST be safe to run twice. At each release its code is copied into a rollup, which runs it again on every database that already ran the HotFix; on develop, a merged-up rollup can also run code that develop already converted on its own. A HotFix that is not safe to rerun has broken real databases: HotFix 320's route swap moved Next-Gen check-in's routes to `checkin-classic` the second time it ran.

- Guard every `CREATE`, `ALTER` and `DROP` with `IF NOT EXISTS` / `IF EXISTS`.
- Write data changes so a second run changes nothing: a `WHERE` that no longer matches once the change is made.
- Be careful with swaps and renames (A to B, then C to A): on a second run the first step moves the new value. Guard the whole swap on the old state still being there.

---

## Pre-Finalization Checklist

Run through this before presenting the migration:

1. Migration number doesn't conflict with any existing file in `Rock/Plugin/HotFixes/`
2. `[MigrationNumber]` attribute number matches the filename number
3. Class name matches filename (without the number prefix and underscore)
4. Minimum Rock version is the release of the branch this HotFix targets
5. All GUIDs are proper format (uppercase, hyphenated)
6. SQL follows Rock conventions (UPPERCASE keywords, brackets, JOIN syntax)
7. IF NOT EXISTS guards used where appropriate for idempotency
8. `using` statements cover every type referenced by its short name
9. Copyright header is complete
10. XML doc comments describe the migration's purpose
11. Block type methods match the block type (entity-based vs path-based)
12. No `UpdateBlockTypeByGuid()` used for Obsidian/Mobile blocks
13. Every statement is safe to run twice (Pitfall 7)
14. The `[MigrationNumber]` version is the release of the branch, from the branch name
