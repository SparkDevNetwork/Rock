# Plugin Migration Methods and SQL Patterns

Methods, SQL formatting rules, and code patterns for writing plugin migration logic.

---

## Available Methods

These methods are available inside `Up()` and `Down()`:

- `Sql(string sql)` — Execute raw SQL
- `SqlScalar(string sql)` — Execute SQL and return a scalar value (useful for reading session properties)
- `RockMigrationHelper.*` — All MigrationHelper methods (AddPage, AddBlock, AddOrUpdateBlockTypeAttribute, AddPostUpdateServiceJob, etc.)
- EF wrapper methods: `AddColumn()`, `DropColumn()`, `CreateIndex()`, `AddForeignKey()`, etc.

---

## Block Type Method Families — Use the Right One

See `.claude/rules/data-model.md` § "Block Type Methods" for the full safety warning. Summary:
- **Obsidian/Mobile blocks** → `AddOrUpdateEntityBlockType()`
- **WebForms blocks** → `UpdateBlockType()` or `UpdateBlockTypeByGuid()`
- **DANGER:** Never use `UpdateBlockTypeByGuid()` for Obsidian/Mobile blocks — causes data loss.

---

## `using` Statements

- Always include `using System;`
- Add `using Rock.Model;` if referencing `typeof()` for job types or entity types

---

## SQL Formatting and String Escaping

See `.claude/rules/code-conventions.md` § "SQL Formatting" for UPPERCASE keywords, bracket-wrapped identifiers, JOIN syntax, IF NOT EXISTS guards, and string escaping rules for `$@"..."` interpolated verbatim strings.

---

## Stored Procedure Pattern

When creating or updating stored procedures, save and restore ANSI_NULLS and QUOTED_IDENTIFIER session settings:

```csharp
var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

Sql( "SET ANSI_NULLS ON;" );
Sql( "SET QUOTED_IDENTIFIER ON;" );

Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spYourProcedure]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spYourProcedure];" );

Sql( @"CREATE PROCEDURE [dbo].[spYourProcedure] ... " );

Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );
```

For large SQL scripts, consider using embedded resources via `HotFixMigrationResource._NNN_Description_ProcedureName` instead of inline strings.

---

## Code Organization

- Use descriptive private methods for logical groups of operations
- Name private methods with developer initials and date: `NA_FixHtmlContent_Up()`
- Add XML doc comments on private methods explaining what they fix and why
- Reference GitHub issues where applicable: `/// Fix for issue #6682`
