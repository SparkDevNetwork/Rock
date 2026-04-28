# Common Migration Pitfalls

These cause real failures in production. Check for all of them before finalizing any migration.

---

## 1. Adding NOT NULL Columns to Tables with Existing Data

`AddColumn("dbo.Table", "Col", c => c.Boolean(nullable: false))` will FAIL if the table already has rows. You must provide a default:

```csharp
AddColumn("dbo.Table", "Col", c => c.Boolean(nullable: false, defaultValue: false));
// Or for strings:
AddColumn("dbo.Table", "Col", c => c.String(nullable: false, maxLength: 100, defaultValue: ""));
```

---

## 2. String Escaping in Interpolated SQL

When using `$@"..."` (interpolated verbatim strings), curly braces must be doubled and single quotes must be doubled:

```csharp
// WRONG - will fail
Sql($@"UPDATE [Table] SET [Col] = '{someVar}' WHERE [Name] = 'O'Brien'");
// CORRECT
Sql($@"UPDATE [Table] SET [Col] = '{someVar}' WHERE [Name] = 'O''Brien'");
// For literal curly braces in JSON etc:
Sql($@"UPDATE [Table] SET [Json] = '{{""key"": ""{value}""}}'");
```

---

## 3. Assembly-Qualified Name Version Numbers

`UpdateEntityType()` takes a full assembly name. The version must match the current build:

```csharp
// Check what version the current branch is targeting, don't hardcode
"Rock.Blocks.Core.McpServerList, Rock.Blocks, Version=19.0.6.0, Culture=neutral, PublicKeyToken=null"
```

Search existing recent migrations for the current version string rather than guessing.

---

## 4. Missing `partial class` Keyword

EF migration classes MUST be declared `partial` because the Designer.cs also declares the same class. Missing this breaks the build:

```csharp
// CORRECT
public partial class MyMigration : Rock.Migrations.RockMigration
// WRONG - build error
public class MyMigration : Rock.Migrations.RockMigration
```

---

## 5. Missing `dbo.` Prefix

All EF migration methods require the `dbo.` schema prefix for table names:

```csharp
// CORRECT
AddColumn("dbo.Person", "NewCol", c => c.Int());
// WRONG - may fail or target wrong schema
AddColumn("Person", "NewCol", c => c.Int());
```

Note: Raw SQL inside `Sql()` calls does NOT require `dbo.` (it defaults to dbo), but EF methods do.

---

## 6. Block Type Method Family Mismatch

Using the wrong block type method family is a CRITICAL bug:

- **Obsidian/Mobile blocks** (`Rock.Blocks.*`, `Rock.Blocks.Types.Mobile.*`): Use `AddOrUpdateEntityBlockType()` — entity-based, NO path
- **WebForms blocks** (`~/Blocks/*.ascx`): Use `UpdateBlockType()` or `UpdateBlockTypeByGuid()` — path-based

`UpdateBlockTypeByGuid()` contains `DELETE FROM [BlockType] WHERE [Path] = '{path}'` which can destroy entity-based block types that have an empty Path value. This exact bug caused data loss in a production migration.

---

## 7. Schema Operation Ordering

Operations must reference things that exist at that point in the migration:

- Cannot `AddForeignKey` before the column it references has been added
- Cannot `CreateIndex` on a column that hasn't been created yet
- In Down(), reverse the order: FK → Index → Column

---

## 8. Down() NULL Handling

If Up() makes a column nullable, Down() must handle existing NULL values before reverting to NOT NULL:

```csharp
public override void Down()
{
    // Fix NULLs before tightening constraint
    Sql( @"UPDATE [dbo].[MyTable] SET [MyColumn] = 0 WHERE [MyColumn] IS NULL" );
    AlterColumn( "dbo.MyTable", "MyColumn", c => c.Int( nullable: false ) );
}
```
