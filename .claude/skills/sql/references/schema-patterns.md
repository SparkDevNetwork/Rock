# Rock RMS Database Schema Patterns

This reference covers the standard column patterns, FK conventions, and data types used across all Rock RMS entities. Consult this before writing any INSERT or UPDATE statement.

## Table of Contents
1. [Base Entity Columns](#base-entity-columns)
2. [PersonAlias Pattern](#personalias-pattern)
3. [DefinedType and DefinedValue](#definedtype-and-definedvalue)
4. [AttributeValue Pattern](#attributevalue-pattern)
5. [Group and GroupMember](#group-and-groupmember)
6. [Person Table](#person-table)
7. [Common FK Patterns](#common-fk-patterns)
8. [Enum Values](#enum-values)
9. [SystemGuid Locations](#systemguid-locations)
10. [SQL Formatting Rules](#sql-formatting-rules)

---

## Base Entity Columns

Every entity inheriting from `Model<T>` has these columns (from `Rock/Data/Model.cs` and `Rock/Data/Entity.cs`):

| Column | Type | Required | Notes |
|---|---|---|---|
| `Id` | int | Yes (identity) | Auto-generated, never set manually |
| `Guid` | uniqueidentifier | Yes | Use `NEWID()` for new records. Unique index. |
| `CreatedDateTime` | datetime | No (nullable) | Set to `GETDATE()` on insert |
| `ModifiedDateTime` | datetime | No (nullable) | Set to `GETDATE()` on insert and update |
| `CreatedByPersonAliasId` | int | No (nullable) | FK to `[PersonAlias].[Id]` — NOT `[Person].[Id]` |
| `ModifiedByPersonAliasId` | int | No (nullable) | FK to `[PersonAlias].[Id]` — NOT `[Person].[Id]` |
| `ForeignId` | int | No (nullable) | For external system sync |
| `ForeignGuid` | uniqueidentifier | No (nullable) | For external system sync |
| `ForeignKey` | nvarchar(100) | No (nullable) | For external system sync |

Entities inheriting from `Entity<T>` (a smaller subset, mostly join tables like `MetricCategory`) get only `Id`, `Guid`, and the `Foreign*` columns — no audit columns. Always verify by reading the model file.

---

## IsSystem — Per-Entity, Not Base

**`IsSystem` is NOT on the `Model<T>` or `Entity<T>` base classes.** Rock declares `IsSystem` individually on each entity that needs it. Including `[IsSystem]` in an INSERT for an entity that doesn't declare it produces `Msg 207, Invalid column name 'IsSystem'` on first run — by far the most common error in this skill's history.

**Always verify before including `[IsSystem]`:** grep the entity's `.cs` file for `public bool IsSystem`. If the property is not declared on that entity (or inherited from a partial class in the same project), omit `[IsSystem]` from the INSERT.

| Entities that **have** `IsSystem` (partial list) | Entities that **lack** `IsSystem` (partial list) |
|---|---|
| `Person`, `Group`, `GroupMember`, `GroupType`, `GroupTypeRole` | `MetricValue`, `MetricPartition`, `MetricValuePartition` |
| `Campus`, `Schedule`, `Location`, `Category` | `MetricCategory` (Entity<T> derivative) |
| `Metric`, `BlockType`, `Block`, `Page`, `PageRoute`, `Site`, `Layout` | `Attendance`, `AttendanceOccurrence` |
| `Attribute`, `AttributeValue`, `DefinedType`, `DefinedValue`, `EntityType`, `FieldType` | `FinancialTransaction`, `FinancialTransactionDetail`, `FinancialBatch` |
| `WorkflowType`, `WorkflowAction`, `WorkflowActionType`, `BinaryFileType` | `Note`, `History`, `AuditLog`, `Following`, `Tag` |

The lists above are not exhaustive — verify per script with a grep on the actual model file you're targeting.

---

## PersonAlias Pattern

This is the most critical pattern to understand. Rock uses `PersonAlias` to track person merges and historical identity.

**Key rules:**
- A Person's "primary" alias has `AliasPersonId = PersonId` (self-reference)
- Audit FK columns (`CreatedByPersonAliasId`, `ModifiedByPersonAliasId`) always reference PersonAlias, never Person
- Many other FK columns (like `RequestedByPersonAliasId`, `AuthorizedPersonAliasId`) also reference PersonAlias

**To get a PersonAliasId from a known Person:**
```sql
-- By PersonId
DECLARE @PersonAliasId INT = (
    SELECT TOP 1 [Id]
    FROM [PersonAlias]
    WHERE [PersonId] = @PersonId
    AND [AliasPersonId] = @PersonId
)

-- By Person Guid
DECLARE @PersonAliasId INT = (
    SELECT TOP 1 [pa].[Id]
    FROM [PersonAlias] AS [pa]
    INNER JOIN [Person] AS [p] ON [p].[Id] = [pa].[PersonId]
    WHERE [p].[Guid] = '8FEDC6EE-8630-41ED-9FC5-C7157FD1EAA4'
    AND [pa].[AliasPersonId] = [pa].[PersonId]
)
```

**PersonAlias table columns:**
| Column | Type | Required |
|---|---|---|
| `Id` | int (identity) | Yes |
| `PersonId` | int | Yes (FK to Person) |
| `AliasPersonId` | int | No (nullable) |
| `AliasPersonGuid` | uniqueidentifier | No (nullable) |
| `Guid` | uniqueidentifier | Yes |

---

## DefinedType and DefinedValue

Rock uses DefinedType/DefinedValue as a flexible enum system. Many columns reference DefinedValue IDs.

**DefinedType columns:**
| Column | Type | Required | Notes |
|---|---|---|---|
| `Id` | int (identity) | Yes | |
| `IsSystem` | bit | Yes | |
| `FieldTypeId` | int | No | FK to FieldType |
| `Order` | int | Yes | For sort order |
| `Name` | nvarchar(100) | Yes | Unique |
| `Description` | nvarchar(max) | No | |
| `IsActive` | bit | Yes | Default `1` |
| `Guid` | uniqueidentifier | Yes | |

**DefinedValue columns:**
| Column | Type | Required | Notes |
|---|---|---|---|
| `Id` | int (identity) | Yes | |
| `IsSystem` | bit | Yes | |
| `DefinedTypeId` | int | Yes | FK to DefinedType |
| `Order` | int | Yes | Calculate from existing max |
| `Value` | nvarchar(250) | Yes | The display value |
| `Description` | nvarchar(max) | No | |
| `IsActive` | bit | Yes | Default `1` |
| `CategoryId` | int | No | FK to Category |
| `Guid` | uniqueidentifier | Yes | |

**Common DefinedTypes referenced throughout Rock:**
- Record Type (Person vs Business)
- Record Status (Active, Inactive, Pending)
- Connection Status (Member, Attendee, Visitor, Web Prospect)
- Marital Status
- Phone Number Type (Mobile, Home, Work)
- Group Location Type (Home, Work, Previous)
- Currency Type
- Transaction Type
- Campus Status
- Campus Type

**To look up a DefinedValue by its well-known Guid, check `Rock/SystemGuid/DefinedValue.cs`.**

---

## AttributeValue Pattern

Rock's EAV (Entity-Attribute-Value) system for extensible properties.

**AttributeValue columns:**
| Column | Type | Required | Notes |
|---|---|---|---|
| `Id` | int (identity) | Yes | |
| `IsSystem` | bit | Yes | |
| `AttributeId` | int | Yes | FK to Attribute |
| `EntityId` | int | No | The Id of the entity this value belongs to |
| `Value` | nvarchar(max) | No | The stored value (format depends on FieldType) |
| `Guid` | uniqueidentifier | Yes | |

**Unique constraint:** `(EntityId, AttributeId)` — one value per attribute per entity.

**To find an Attribute's Id:**
```sql
DECLARE @AttributeId INT = (
    SELECT TOP 1 [Id]
    FROM [Attribute]
    WHERE [Key] = 'YourAttributeKey'
    AND [EntityTypeId] = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Person')
)
```

**Common value formats by FieldType:**
- Text: plain string
- Boolean: `"True"` or `"False"` (string, not bit)
- Date: `"2024-03-15T00:00:00.0000000"` (ISO 8601)
- Integer: `"42"` (string representation)
- DefinedValue: the Guid of the DefinedValue (not the Id)
- Person: the Guid of the Person

---

## Group and GroupMember

**Group columns (key fields):**
| Column | Type | Required | Notes |
|---|---|---|---|
| `IsSystem` | bit | Yes | |
| `GroupTypeId` | int | Yes | FK to GroupType |
| `CampusId` | int | No | FK to Campus |
| `Name` | nvarchar(100) | Yes | |
| `Description` | nvarchar(max) | No | |
| `IsSecurityRole` | bit | Yes | |
| `IsActive` | bit | Yes | |
| `Order` | int | Yes | |
| `IsPublic` | bit | Yes | |
| `IsArchived` | bit | Yes | Soft delete |
| `Guid` | uniqueidentifier | Yes | |

**GroupMember columns (key fields):**
| Column | Type | Required | Notes |
|---|---|---|---|
| `IsSystem` | bit | Yes | |
| `GroupId` | int | Yes | FK to Group (CASCADE DELETE) |
| `PersonId` | int | Yes | FK to Person |
| `GroupRoleId` | int | Yes | FK to GroupTypeRole |
| `GroupMemberStatus` | int | Yes | 1=Active, 2=Inactive, 3=Pending |
| `GroupTypeId` | int | Yes | FK to GroupType |
| `DateTimeAdded` | datetime | No | |
| `IsNotified` | bit | Yes | |
| `IsArchived` | bit | Yes | Soft delete |
| `CommunicationPreference` | int | Yes | |
| `Guid` | uniqueidentifier | Yes | |

**Important:** `GroupTypeId` on GroupMember is denormalized — it must match the Group's GroupTypeId.

**Family group pattern:**
- GroupType "Family" Guid: `790E3215-3B10-442B-AF69-616C0DCB998E`
- Adult Role Guid: `2639F9A5-2AAE-4E48-A8C3-4FFE86681670`
- Child Role Guid: `C8B1814F-6AA7-4055-B2D7-48FE20429CB9`

---

## Person Table

The Person table has many columns. Key required/important ones:

| Column | Type | Required | Notes |
|---|---|---|---|
| `IsSystem` | bit | Yes | Usually `0` |
| `RecordTypeValueId` | int | No | FK to DefinedValue (Person vs Business) |
| `RecordStatusValueId` | int | No | FK to DefinedValue (Active/Inactive/Pending) |
| `ConnectionStatusValueId` | int | No | FK to DefinedValue |
| `IsDeceased` | bit | Yes | |
| `FirstName` | nvarchar(50) | Yes | |
| `NickName` | nvarchar(50) | No | |
| `LastName` | nvarchar(50) | Yes | |
| `Gender` | int | Yes | 0=Unknown, 1=Male, 2=Female |
| `Email` | nvarchar(75) | No | |
| `IsEmailActive` | bit | Yes | |
| `EmailPreference` | int | Yes | 0=EmailAllowed, 1=NoMassEmails, 2=DoNotEmail |
| `CommunicationPreference` | int | Yes | |
| `AgeClassification` | int | Yes | 0=Unknown, 1=Adult, 2=Child |
| `IsLockedAsChild` | bit | Yes | |
| `GivingLeaderId` | int | Yes | Usually set to own Id, or family head |
| `AccountProtectionProfile` | int | Yes | |
| `AgeBracket` | int | Yes | |
| `Guid` | uniqueidentifier | Yes | |

**After inserting a Person, always create a matching PersonAlias:**
```sql
SET @NewPersonId = SCOPE_IDENTITY()

INSERT INTO [PersonAlias] ([PersonId], [AliasPersonId], [AliasPersonGuid], [Guid])
VALUES (@NewPersonId, @NewPersonId, (SELECT [Guid] FROM [Person] WHERE [Id] = @NewPersonId), NEWID())
```

**And add them to a Family group** (GroupType = Family), otherwise they'll appear orphaned in Rock.

---

## Common FK Patterns

| FK Column Pattern | References | Notes |
|---|---|---|
| `*PersonAliasId` | `PersonAlias.Id` | Audit fields, requestor fields |
| `*PersonId` | `Person.Id` | Direct person reference (GroupMember, etc.) |
| `*CampusId` | `Campus.Id` | Often ON DELETE SET NULL |
| `*GroupTypeId` | `GroupType.Id` | |
| `*DefinedValueId` or `*ValueId` | `DefinedValue.Id` | Check SystemGuid for well-known values |
| `*EntityTypeId` | `EntityType.Id` | Lookup by `[Name] = 'Rock.Model.X'` |
| `*CategoryId` | `Category.Id` | |
| `*FieldTypeId` | `FieldType.Id` | |

---

## Enum Values

Common enum integer values used in SQL:

**Gender:** 0=Unknown, 1=Male, 2=Female

**GroupMemberStatus:** 1=Active, 2=Inactive, 3=Pending

**RecordStatus (DefinedValue Guids):**
- Active: `618F906C-C33D-4FA3-8AEF-E58CB7B63F1E`
- Inactive: `1DAD99D5-41A9-4865-8366-F269902B80A4`
- Pending: `283999EC-7346-42E3-B807-BCE9B2BABB49`

**ConnectionStatus (DefinedValue Guids):**
- Member: `41540783-D081-4C63-962A-FC2BEDD817C0`
- Attendee: `39F491C5-D6AC-4A9B-8AC0-C431CB17D588`
- Visitor: `B91BA046-BC1E-400C-B85D-638C1F4E0CE2`
- Web Prospect: `368DD475-242C-49C4-A42C-7278BE690CC2`

**RecordType (DefinedValue Guids):**
- Person: `36CF10D6-C695-413D-8E7C-4546EFEF385E`
- Business: `BF64ADD3-E70A-44CE-9C4B-E76BBED37550`

**EmailPreference:** 0=EmailAllowed, 1=NoMassEmails, 2=DoNotEmail

**CommunicationPreference:** 0=Email, 1=SMS, 2=PushNotification

For enums not listed here, check `Rock.Enums/[Domain]/` or the model file's property type.

---

## SystemGuid Locations

When you need well-known Guids for system entities, these files contain them:

| File | Contains |
|---|---|
| `Rock/SystemGuid/DefinedType.cs` | DefinedType Guids |
| `Rock/SystemGuid/DefinedValue.cs` | DefinedValue Guids |
| `Rock/SystemGuid/EntityType.cs` | EntityType Guids |
| `Rock/SystemGuid/GroupType.cs` | GroupType Guids (Family, Security Role, etc.) |
| `Rock/SystemGuid/Person.cs` | System Person Guids (Admin, Anonymous) |
| `Rock/SystemGuid/BlockType.cs` | BlockType Guids |
| `Rock/SystemGuid/Category.cs` | Category Guids |
| `Rock/SystemGuid/Attribute.cs` | Well-known Attribute Guids |
| `Rock/SystemGuid/Page.cs` | Page Guids |
| `Rock/SystemGuid/ServiceJob.cs` | ServiceJob Guids |

**To use a SystemGuid in SQL:**
```sql
-- Look up the constant value in the .cs file, then use it directly
DECLARE @FamilyGroupTypeId INT = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E')
```

---

## SQL Formatting Rules

From the project's CLAUDE.md:

1. SQL keywords in **UPPERCASE**: `SELECT`, `FROM`, `WHERE`, `JOIN`, `INSERT INTO`, `VALUES`, `UPDATE`, `SET`, `DELETE`, `DECLARE`, `BEGIN`, `END`, `IF`, `ELSE`, `AND`, `OR`, `NOT`, `NULL`, `AS`, `ON`, `IN`, `EXISTS`, `TOP`, `ORDER BY`, `GROUP BY`, `HAVING`, `INNER`, `LEFT`, `RIGHT`, `OUTER`
2. Wrap all table and column names in **brackets**: `[Person].[FirstName]`
3. Use **`JOIN` syntax**, not `WHERE` clauses for joins
4. Use **table aliases**: `[Person] AS [p]`
5. **ISO 8601 dates**: `'2024-03-15T00:00:00'` not `'03/15/2024'`

**Example of properly formatted Rock SQL:**
```sql
SELECT [p].[FirstName], [p].[LastName], [g].[Name] AS [GroupName]
FROM [Person] AS [p]
INNER JOIN [GroupMember] AS [gm] ON [gm].[PersonId] = [p].[Id]
INNER JOIN [Group] AS [g] ON [g].[Id] = [gm].[GroupId]
WHERE [p].[IsDeceased] = 0
    AND [gm].[IsArchived] = 0
    AND [gm].[GroupMemberStatus] = 1
ORDER BY [p].[LastName], [p].[FirstName]
```
