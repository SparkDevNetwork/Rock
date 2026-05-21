---
name: sql
description: >-
  Generate safe, correct SQL scripts for the Rock RMS database. Handles INSERT, UPDATE, DELETE,
  SELECT queries, data migrations, and seed scripts that respect Rock's schema conventions
  (PersonAlias, audit columns, FK constraints, DefinedType/DefinedValue lookups, Guid-based references).
  Use when the user says "create a sql script", "write sql", "insert data", "seed data",
  "data migration", "update records", "sql for Rock", "populate data", "add test data",
  "query Rock database", or any request involving direct SQL against the Rock RMS database.
  Also use when the user describes data they want to add, modify, or query in Rock — even if
  they don't explicitly say "SQL" — such as "add 100 attendance records", "create a new campus",
  "give Ted Decker some financial transactions", or "set up check-in data". If the task involves
  Rock database records and SQL is the right tool, use this skill.
argument-hint: "Describe what the script should do (e.g., 'add a new campus' or 'insert 50 attendance records')"
compatibility: Requires Claude Code with access to the Rock RMS codebase for reading model files and writing .sql output files.
metadata:
  version: "1.2"
  author: "Maxwell Eley"
---

# Rock RMS SQL Script Generator

You are generating SQL scripts that run against the Rock RMS database. Rock has specific schema conventions that, if ignored, cause FK constraint violations, missing audit trails, or broken data. This skill ensures you get it right.

**The user's request:** $ARGUMENTS

---

## Reference Routing Table

Load reference files progressively — only when needed.

| Reference File | Load When |
|---|---|
| `references/schema-patterns.md` | Before writing any INSERT or UPDATE (always) |
| `references/sample-data.md` | When the script references people, families, or groups |

**Do NOT read both files upfront.** Read `schema-patterns.md` for every script; read `sample-data.md` only when people are involved.

## Step 1: Understand the Request

Parse what the user wants:
- **What data** is being created, modified, queried, or deleted?
- **Which tables** are involved?
- **Are people involved?** If so, you'll need PersonAlias lookups.
- **Is this a one-time script or a migration?** Migrations need IF NOT EXISTS guards.

If the request is ambiguous, ask clarifying questions before writing SQL.

## Step 2: Research the Schema

Before writing any SQL, read the relevant Entity Framework model files to understand the actual columns, data types, required fields, and FK relationships. This is critical because Rock's schema evolves and assumptions lead to errors.

**How to find model files:**
- Entity models live in `Rock/Model/[Domain]/[Entity]/[Entity].cs`
- If that path doesn't exist, search: `Rock/Model/**/{EntityName}.cs`
- For enums referenced in models, check `Rock.Enums/[Domain]/`
- For SystemGuids, check `Rock/SystemGuid/{EntityType}.cs`

**What to look for in model files:**
- `[Required]` attributes — these columns cannot be NULL
- `[MaxLength(N)]` — string length limits
- FK navigation properties (e.g., `public virtual PersonAlias CreatedByPersonAlias`)
- Enum-typed properties — you'll need the int value, not the enum name
- Default values in constructors or property initializers
- **`public bool IsSystem`** — verify the model declares this before including `[IsSystem]` in an INSERT (it's per-entity, not on `Model<T>`). See `schema-patterns.md` for the list of common offenders. **#1 first-run failure mode.**

Read `references/schema-patterns.md` for the standard column patterns that apply to every Rock entity.

## Step 3: Write the SQL

Follow these rules strictly:

### SQL Formatting (from Rock conventions)
- SQL keywords in **UPPERCASE**: `SELECT`, `FROM`, `WHERE`, `JOIN`, `INSERT`, `UPDATE`, `DELETE`, `DECLARE`, `SET`, `BEGIN`, `END`
- Wrap all table and column names in **brackets**: `[Person].[FirstName]`
- Use `JOIN` syntax, not `WHERE` clauses for joins
- Use aliases: `[Person] AS [p]`, `[GroupMember] AS [gm]`

### Required Patterns

**Every INSERT must include:**
1. `[Guid]` — use `NEWID()` unless a specific Guid is required
2. Audit columns when the entity inherits from `Model<T>` (most entities, but not all — `Entity<T>` derivatives like `MetricCategory` and the various join tables have no audit columns):
   - `[CreatedDateTime]` = `GETDATE()`
   - `[ModifiedDateTime]` = `GETDATE()`
   - `[CreatedByPersonAliasId]` = looked up from a known person (use admin or sample data person)
   - `[ModifiedByPersonAliasId]` = same as above

**Conditional columns — verify before including:**
- `[IsSystem]` — only when the entity model declares `public bool IsSystem`.
- `[Order]` — only when the entity implements `IOrdered` or declares `public int Order` directly.
- `[ForeignId]` / `[ForeignGuid]` / `[ForeignKey]` — present on `IModel`/`Model<T>`; usually safe to omit (default to NULL).

**PersonAlias vs Person — this is the most common mistake:**
- Audit fields (`CreatedByPersonAliasId`, `ModifiedByPersonAliasId`) reference `[PersonAlias]`, NOT `[Person]`
- To get a PersonAliasId from a Person, query: `SELECT [Id] FROM [PersonAlias] WHERE [PersonId] = @PersonId AND [AliasPersonId] = @PersonId`
- Many FK columns throughout Rock reference PersonAliasId, not PersonId. Always check the model.

**Lookups by Guid (preferred over hardcoded Ids):**
```sql
DECLARE @CampusId INT = (SELECT TOP 1 [Id] FROM [Campus] WHERE [Guid] = 'some-guid-here')
DECLARE @DefinedValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = 'some-guid-here')
```

**Safety guards:**
```sql
-- For idempotent scripts (can be re-run safely)
IF NOT EXISTS (SELECT 1 FROM [TableName] WHERE [Guid] = 'your-guid')
BEGIN
    INSERT INTO [TableName] (...) VALUES (...)
END
```

**Transactions for multi-statement scripts:**
```sql
BEGIN TRANSACTION

-- Your statements here

IF @@ERROR <> 0
BEGIN
    ROLLBACK TRANSACTION
    RETURN
END

COMMIT TRANSACTION
```

### Referencing People

Whether to use existing sample data people or create new ones depends on context. Use your judgment:

**Use existing sample data people when:**
- The script is about generating related records (transactions, attendance, notes, group memberships) and the user hasn't specified particular people
- The user says "test data", "sample transactions", or similar — they want quick, realistic data tied to people who already exist
- Creating new people would be unnecessary overhead for the actual goal

**Create new people when:**
- The user explicitly asks to add a person, family, or member
- The scenario requires it (e.g., "add a new family and their giving history")
- The user describes specific people who don't match anyone in sample data

When creating new people, remember to also create the PersonAlias and add them to a Family group — an orphaned Person record causes problems in Rock.

Read `references/sample-data.md` for the full list of available sample people, their GUIDs, and family structures.

**Looking up existing people — prefer Guid over hardcoded Ids:**
```sql
-- By Guid from sample data
DECLARE @TedDeckerPersonId INT = (SELECT TOP 1 [Id] FROM [Person] WHERE [Guid] = '8FEDC6EE-8630-41ED-9FC5-C7157FD1EAA4')
DECLARE @TedDeckerPersonAliasId INT = (SELECT TOP 1 [Id] FROM [PersonAlias] WHERE [PersonId] = @TedDeckerPersonId AND [AliasPersonId] = @TedDeckerPersonId)

-- By name if Guid isn't critical
DECLARE @PersonId INT = (SELECT TOP 1 [Id] FROM [Person] WHERE [FirstName] = 'Ted' AND [LastName] = 'Decker')

-- Grab a set of existing people for bulk data generation
SELECT TOP 10 [Id] FROM [Person] WHERE [IsDeceased] = 0 AND [RecordStatusValueId] = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E') ORDER BY [Id]
```

### Common Script Patterns

**Insert a DefinedValue:**
```sql
DECLARE @DefinedTypeId INT = (SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = '{guid}')
DECLARE @MaxOrder INT = (SELECT ISNULL(MAX([Order]), 0) FROM [DefinedValue] WHERE [DefinedTypeId] = @DefinedTypeId)

INSERT INTO [DefinedValue] ([IsSystem], [DefinedTypeId], [Order], [Value], [Description], [IsActive], [Guid], [CreatedDateTime], [ModifiedDateTime])
VALUES (0, @DefinedTypeId, @MaxOrder + 1, 'New Value', 'Description here', 1, NEWID(), GETDATE(), GETDATE())
```

**Add a person to a group:**
```sql
DECLARE @GroupId INT = (SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '{group-guid}')
DECLARE @GroupTypeId INT = (SELECT TOP 1 [GroupTypeId] FROM [Group] WHERE [Id] = @GroupId)
DECLARE @DefaultRoleId INT = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [GroupTypeId] = @GroupTypeId AND [IsDefaultGroupTypeRole] = 1)
DECLARE @PersonId INT = (SELECT TOP 1 [Id] FROM [Person] WHERE [Guid] = '{person-guid}')

IF NOT EXISTS (SELECT 1 FROM [GroupMember] WHERE [GroupId] = @GroupId AND [PersonId] = @PersonId AND [IsArchived] = 0)
BEGIN
    INSERT INTO [GroupMember] ([IsSystem], [GroupId], [PersonId], [GroupRoleId], [GroupMemberStatus], [Guid], [CreatedDateTime], [ModifiedDateTime], [DateTimeAdded], [IsNotified], [IsArchived], [CommunicationPreference], [GroupTypeId])
    VALUES (0, @GroupId, @PersonId, @DefaultRoleId, 1, NEWID(), GETDATE(), GETDATE(), GETDATE(), 0, 0, 0, @GroupTypeId)
END
```

**Set an attribute value:**
```sql
DECLARE @AttributeId INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Key] = 'AttributeKey' AND [EntityTypeId] = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Person'))
DECLARE @EntityId INT = (SELECT TOP 1 [Id] FROM [Person] WHERE [Guid] = '{person-guid}')

IF EXISTS (SELECT 1 FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @EntityId)
BEGIN
    UPDATE [AttributeValue]
    SET [Value] = 'new value', [ModifiedDateTime] = GETDATE()
    WHERE [AttributeId] = @AttributeId AND [EntityId] = @EntityId
END
ELSE
BEGIN
    INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid], [CreatedDateTime], [ModifiedDateTime])
    VALUES (0, @AttributeId, @EntityId, 'new value', NEWID(), GETDATE(), GETDATE())
END
```

## Step 4: Validate Before Presenting

Run through this checklist before giving the script to the user:

1. Every INSERT has `[Guid]` with `NEWID()` (or a specific Guid if required)
2. Every INSERT to a `Model<T>` entity has `[CreatedDateTime]` and `[ModifiedDateTime]` (skip for `Entity<T>` derivatives that have no audit columns)
3. `[IsSystem]` is included **only** when the model declares it (verified by grep). When in doubt, omit it.
4. All FK columns reference valid lookups (no hardcoded Ids without a DECLARE)
5. PersonAlias is used for audit columns, not Person
6. String values don't exceed `[MaxLength]` from the model
7. Required (`NOT NULL`) columns are populated
8. `[Order]` columns are calculated, not hardcoded to 0
9. Enum values use the correct integer (check the enum definition)
10. The script is idempotent where appropriate (IF NOT EXISTS guards)

## Output Format

Write the SQL script to a `.sql` file in the `Dev Tools/Sql/` directory. Follow the existing naming conventions:

| Prefix | Use When |
|---|---|
| `Populate_` | Seeding/generating test data (e.g., `Populate_FinancialTransactions_Contributions.sql`) |
| `CodeGen_` | Generating migration code or system data (e.g., `CodeGen_SystemGuid_DefinedValue.sql`) |
| `Tool_` | Utility/maintenance scripts (e.g., `Tool_DeletePeople.sql`) |
| `View_` | Creating or querying views (e.g., `View_PersonAttributeValues.sql`) |
| `Report_` | Reporting queries (e.g., `Report_OrphanedAttributes.sql`) |
| `Enable_` | Enabling features or settings (e.g., `Enable_FacebookLogin.sql`) |

Use PascalCase with underscores between words in the filename.

**File contents should include:**
- A brief comment header explaining what the script does
- `DECLARE` statements for all lookups at the top
- The main operations
- Any cleanup or verification queries at the end (e.g., a SELECT to confirm the data was inserted)

```sql
SET NOCOUNT ON

-- =============================================
-- Description: [What this script does]
-- Date: [Current date]
-- =============================================

-- Lookups
DECLARE @SomeId INT = (...)

-- Main operations
BEGIN TRANSACTION
...
COMMIT TRANSACTION

-- Verify
SELECT * FROM [TableName] WHERE [Guid] = '...'
```

Also show the script contents in the conversation so the user can review it without opening the file.

## Examples

### Example 1: Populate test data for existing people

User says: "Create some financial transactions for the sample data people"

Actions:
1. Read `references/schema-patterns.md` and `references/sample-data.md`
2. Read `Rock/Model/Finance/FinancialTransaction/FinancialTransaction.cs` and `Rock/Model/Finance/FinancialTransactionDetail/FinancialTransactionDetail.cs` for required columns
3. Look up FinancialAccount, TransactionType, CurrencyType DefinedValues via SystemGuid
4. Use existing sample data people (Ted Decker, Bill Marble, etc.) — no new Person records needed
5. Write script to `Dev Tools/Sql/Populate_FinancialTransactions_SampleData.sql`

Result: A runnable `.sql` file that creates realistic transactions tied to existing sample people.

### Example 2: Add a new entity

User says: "Add a new campus called North Campus"

Actions:
1. Read `references/schema-patterns.md`
2. Read `Rock/Model/Core/Campus/Campus.cs` for required columns
3. Look up CampusStatus and CampusType DefinedValues
4. Write script with IF NOT EXISTS guard
5. Write script to `Dev Tools/Sql/Populate_Campus_NorthCampus.sql`

Result: An idempotent script that adds a new Campus record with all required fields.

### Example 3: Add new people with related data

User says: "Add a new family — the Johnsons — with two adults and a child, and give them some attendance history"

Actions:
1. Read `references/schema-patterns.md`
2. Read Person, PersonAlias, Group, GroupMember, Attendance model files
3. Create new Person records with PersonAlias and Family GroupMember records
4. Generate attendance records tied to the new people
5. Write script to `Dev Tools/Sql/Populate_JohnsonFamily_WithAttendance.sql`

Result: A transactional script that creates a complete family with attendance data, no orphaned records.

---

## Troubleshooting

**`Invalid column name 'IsSystem'`:** The entity doesn't declare `IsSystem` (it's per-entity, not on `Model<T>`). Remove `[IsSystem]` from the INSERT column list and the corresponding `0` from the VALUES/SELECT. See `schema-patterns.md` "IsSystem — Per-Entity, Not Base" for the offender list.

**FK constraint violation:** You're referencing an Id that doesn't exist. Check your DECLARE lookups — did the subquery return NULL? Add a NULL check: `IF @SomeId IS NOT NULL BEGIN ... END`

**Cannot insert NULL:** You missed a required column. Read the model file and look for `[Required]` attributes.

**String truncation:** Your string value exceeds the column's `[MaxLength]`. Check the model.

**Duplicate key:** Your INSERT conflicts with a unique constraint (usually on `[Guid]` or a composite key). Use IF NOT EXISTS.
