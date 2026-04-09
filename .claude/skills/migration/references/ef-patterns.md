# EF Migration Patterns for Rock RMS

Common patterns with complete examples from actual Rock migrations.

## Quick Reference: Available EF DbMigration Methods

These are inherited from `RockMigration` (which extends `DbMigration`):

```csharp
// Columns
AddColumn("dbo.TableName", "ColumnName", c => c.Int(nullable: false));
AddColumn("dbo.TableName", "ColumnName", c => c.String(maxLength: 100));
AddColumn("dbo.TableName", "ColumnName", c => c.Boolean(nullable: false));
AddColumn("dbo.TableName", "ColumnName", c => c.DateTime());  // nullable by default
DropColumn("dbo.TableName", "ColumnName");
AlterColumn("dbo.TableName", "ColumnName", c => c.Int());  // e.g. making nullable
RenameColumn("dbo.TableName", "OldName", "NewName");

// Indexes
CreateIndex("dbo.TableName", "ColumnName");
CreateIndex("dbo.TableName", new[] { "Col1", "Col2" });  // composite
DropIndex("dbo.TableName", new[] { "ColumnName" });

// Foreign Keys
AddForeignKey("dbo.TableName", "FKColumnId", "dbo.ReferencedTable", "Id");
AddForeignKey("dbo.TableName", "FKColumnId", "dbo.ReferencedTable", "Id", cascadeDelete: true);
DropForeignKey("dbo.TableName", "FKColumnId", "dbo.ReferencedTable");

// Tables
CreateTable("dbo.TableName", c => new { ... });
DropTable("dbo.TableName");
RenameTable("dbo.OldName", "NewName");
```

---

## Table of Contents
1. [Simple Column Addition](#simple-column-addition)
2. [Column with Foreign Key](#column-with-foreign-key)
3. [Making a Column Nullable](#making-a-column-nullable)
4. [Custom FK with ON DELETE SET NULL](#custom-fk-with-on-delete-set-null)
5. [Adding a Page + Block + Block Type](#adding-a-page--block--block-type)
6. [Data Migration with Raw SQL](#data-migration-with-raw-sql)
7. [Rollup Migration Structure](#rollup-migration-structure)
8. [Stored Procedure Update](#stored-procedure-update)
9. [Down() Patterns](#down-patterns)

---

## Simple Column Addition

From `AddRegistrationTemplateAreDuplicateRegistrantsPreventedColumn`:

```csharp
public override void Up()
{
    AddColumn( "dbo.RegistrationTemplate", "AreDuplicateRegistrantsPrevented", c => c.Boolean( nullable: false ) );
}

public override void Down()
{
    DropColumn( "dbo.RegistrationTemplate", "AreDuplicateRegistrantsPrevented" );
}
```

**Column type mappings:**
| C# Type | EF Migration Syntax |
|---|---|
| `int` (required) | `c => c.Int( nullable: false )` |
| `int?` (nullable) | `c => c.Int()` |
| `string` (required, max 100) | `c => c.String( nullable: false, maxLength: 100 )` |
| `string` (nullable, max length) | `c => c.String( maxLength: 250 )` |
| `string` (nullable, no max) | `c => c.String()` |
| `bool` (required) | `c => c.Boolean( nullable: false )` |
| `DateTime?` (nullable) | `c => c.DateTime()` |
| `Guid` (required) | `c => c.Guid( nullable: false )` |
| `decimal` (required) | `c => c.Decimal( nullable: false, precision: 18, scale: 2 )` |

---

## Column with Foreign Key

```csharp
public override void Up()
{
    AddColumn( "dbo.PersistedDataset", "CreatedByPersonAliasId", c => c.Int() );
    AddColumn( "dbo.PersistedDataset", "ModifiedByPersonAliasId", c => c.Int() );
    
    CreateIndex( "dbo.PersistedDataset", "CreatedByPersonAliasId" );
    CreateIndex( "dbo.PersistedDataset", "ModifiedByPersonAliasId" );
    
    AddForeignKey( "dbo.PersistedDataset", "CreatedByPersonAliasId", "dbo.PersonAlias", "Id" );
    AddForeignKey( "dbo.PersistedDataset", "ModifiedByPersonAliasId", "dbo.PersonAlias", "Id" );
}

public override void Down()
{
    DropForeignKey( "dbo.PersistedDataset", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
    DropForeignKey( "dbo.PersistedDataset", "CreatedByPersonAliasId", "dbo.PersonAlias" );
    
    DropIndex( "dbo.PersistedDataset", new[] { "ModifiedByPersonAliasId" } );
    DropIndex( "dbo.PersistedDataset", new[] { "CreatedByPersonAliasId" } );
    
    DropColumn( "dbo.PersistedDataset", "ModifiedByPersonAliasId" );
    DropColumn( "dbo.PersistedDataset", "CreatedByPersonAliasId" );
}
```

**Order matters:** Up() = column → index → FK. Down() = FK → index → column (exact reverse).

**FK cascade rules:**
- PersonAlias audit columns: **no cascade** (default — don't pass `cascadeDelete`)
- Campus references: typically `ON DELETE SET NULL` (requires custom SQL, see below)
- GroupMember → Group: `cascadeDelete: true`
- Most other FKs: no cascade (default)

---

## Making a Column Nullable

```csharp
public override void Up()
{
    AlterColumn( "dbo.HtmlContent", "BlockId", c => c.Int() );  // now nullable
}

public override void Down()
{
    // Must handle NULLs before making NOT NULL again
    Sql( @"UPDATE [dbo].[HtmlContent]
           SET [BlockId] = (SELECT TOP 1 [Id] FROM [dbo].[Block] ORDER BY [Id])
           WHERE [BlockId] IS NULL" );
    
    AlterColumn( "dbo.HtmlContent", "BlockId", c => c.Int( nullable: false ) );
}
```

---

## Custom FK with ON DELETE SET NULL

EF doesn't support ON DELETE SET NULL natively, so use raw SQL:

```csharp
public override void Up()
{
    DropForeignKey( "dbo.HtmlContent", "BlockId", "dbo.Block" );
    AlterColumn( "dbo.HtmlContent", "BlockId", c => c.Int() );  // nullable
    
    Sql( @"ALTER TABLE [dbo].[HtmlContent]
           ADD CONSTRAINT [FK_dbo.HtmlContent_dbo.Block_BlockId] FOREIGN KEY ([BlockId])
           REFERENCES [dbo].[Block] ([Id])
           ON DELETE SET NULL;" );
}

public override void Down()
{
    Sql( @"UPDATE [dbo].[HtmlContent]
           SET [BlockId] = (SELECT TOP 1 [Id] FROM [dbo].[Block] ORDER BY [Id])
           WHERE [BlockId] IS NULL" );
    
    DropForeignKey( "dbo.HtmlContent", "BlockId", "dbo.Block" );
    DropIndex( "dbo.HtmlContent", new[] { "BlockId" } );
    AlterColumn( "dbo.HtmlContent", "BlockId", c => c.Int( nullable: false ) );
    CreateIndex( "dbo.HtmlContent", "BlockId" );
    AddForeignKey( "dbo.HtmlContent", "BlockId", "dbo.Block", "Id", cascadeDelete: true );
}
```

---

## Adding a Page + Block + Block Type

From `AddMcpServerBlock`:

```csharp
public override void Up()
{
    // Schema changes first
    AddColumn( "dbo.UserLogin", "ApiKeyPurpose", c => c.Int() );
    AddColumn( "dbo.UserLogin", "Description", c => c.String( maxLength: 250 ) );

    // Data migration
    Sql( "UPDATE dbo.UserLogin SET ApiKeyPurpose = 1 WHERE [ApiKey] IS NOT NULL AND RTRIM([ApiKey]) <> ''" );

    // Add Page 
    //  Internal Name: MCP Servers
    //  Site: Rock RMS
    RockMigrationHelper.AddPage( true, "CF54E680-2E02-4F16-B54B-A2F2D29CD932", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "MCP Servers", "", "87BD0803-9532-49DA-B584-D9568A2AD796", "ti ti-robot" );

    // Add Page Route
    //   Page:MCP Servers
    //   Route:my/mcp-servers
    RockMigrationHelper.AddOrUpdatePageRoute( "87BD0803-9532-49DA-B584-D9568A2AD796", "my/mcp-servers", "1566561F-A051-48D9-805B-D099C535F145" );

    // Add/Update Obsidian Block Entity Type
    //   EntityType:Rock.Blocks.Core.McpServerList
    RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.McpServerList", "Mcp Server List", "Rock.Blocks.Core.McpServerList, Rock.Blocks, Version=19.0.6.0, Culture=neutral, PublicKeyToken=null", false, false, "F0B14291-8035-4986-A4D8-DC1AE08E4F7B" );

    // Add/Update Obsidian Block Type
    //   Name:MCP Server List
    //   Category:Core
    //   EntityType:Rock.Blocks.Core.McpServerList
    RockMigrationHelper.AddOrUpdateEntityBlockType( "MCP Server List", "Displays a list of MCP Servers.", "Rock.Blocks.Core.McpServerList", "Core", "54B23A63-87C0-4955-B915-C91F23C36D48" );

    // Add Block 
    //  Block Name: MCP Server List
    //  Page Name: MCP Server List
    RockMigrationHelper.AddBlock( true, "87BD0803-9532-49DA-B584-D9568A2AD796".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "54B23A63-87C0-4955-B915-C91F23C36D48".AsGuid(), "MCP Server List", "Main", @"", @"", 0, "30A085FD-A7FD-4B8C-933D-5DA6B13881F2" );
}

public override void Down()
{
    // Remove Block
    //  Name: MCP Server List, from Page: MCP Servers, Site: Rock RMS
    RockMigrationHelper.DeleteBlock( "30A085FD-A7FD-4B8C-933D-5DA6B13881F2" );

    // Delete BlockType 
    //   Name: MCP Server List
    RockMigrationHelper.DeleteBlockType( "54B23A63-87C0-4955-B915-C91F23C36D48" );

    // Delete Page 
    //  Internal Name: MCP Servers
    RockMigrationHelper.DeletePage( "87BD0803-9532-49DA-B584-D9568A2AD796" );

    DropColumn( "dbo.UserLogin", "Description" );
    DropColumn( "dbo.UserLogin", "ApiKeyPurpose" );
}
```

**Key takeaway:** Down() deletes in reverse order: Block → BlockType → Page → schema columns.

---

## Data Migration with Raw SQL

```csharp
public override void Up()
{
    // Seed default values for existing rows
    Sql( @"
UPDATE [ConnectionType]
SET [EnabledFeatures] = 1 | 2 | 4
    , [EnabledViews] = 1 | 2 | 4 | 8 | 16;" );
}

public override void Down()
{
    // Reverse to original state
    Sql( @"
UPDATE [ConnectionType]
SET [EnabledFeatures] = 0
    , [EnabledViews] = 0;" );
}
```

---

## Private Methods for Logical Operations

Every non-trivial migration should break logic into private methods — one per logical operation, with matching Up/Down pairs. Up() and Down() should read like a table of contents:

```csharp
public override void Up()
{
    JPH_SeedEnabledViewsAndFeaturesForExistingConnectionType_Up();
    JPH_AddConnectionsPages_Up();
    JPH_AddConnectionNavigationViewBlocks_Up();
    KH_AddConnectionsListBlockUp();
    KH_AddConnectionRequestNoteType_Up();
}

public override void Down()
{
    JPH_AddConnectionNavigationViewBlocks_Down();
    JPH_AddConnectionsPages_Down();
    JPH_SeedEnabledViewsAndFeaturesForExistingConnectionType_Down();
}
```

**This pattern applies to ALL multi-operation migrations, not just rollups.** Even single-feature migrations benefit from named methods when the logic is complex.

**Naming:** Developer initials + descriptive name + `_Up()`/`_Down()`:
- `JPH_AddConnectionsPages_Up()` / `JPH_AddConnectionsPages_Down()`
- `NA_RemoveThreeUnneededBlocks_Up()`
- `KH_AddConnectionRequestNoteType_Up()`

## Rollup Migration Structure

Rollup migrations are the most common use of the private method pattern. They batch 5-20+ operations from different developers:

```csharp
public override void Up()
{
    JE_IconTransitionTableUpdate_Up();      // v18.3
    NA_RemoveThreeUnneededBlocks_Up();      // v19.0.5
    PS_RenameBeaconDashboard_Up();
}

public override void Down()
{
    // Reverse order
    PS_RenameBeaconDashboard_Down();
    NA_RemoveThreeUnneededBlocks_Down();
    JE_IconTransitionTableUpdate_Down();
}
```

---

## Stored Procedure Update

Save and restore session settings:

```csharp
private void UpdateMyStoredProcedure()
{
    var isAnsiNullsOn = Convert.ToBoolean( SqlScalar(
        "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
    var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar(
        "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );
    
    Sql( "SET ANSI_NULLS ON;" );
    Sql( "SET QUOTED_IDENTIFIER ON;" );
    
    Sql( @"IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spMyProcedure]') AND TYPE IN (N'P', N'PC'))
        DROP PROCEDURE [dbo].[spMyProcedure];" );
    
    // For large procedures, use embedded resources:
    // Sql( RockMigrationSQL._{MigrationName}_{ProcedureName} );
    
    Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
    Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );
}
```

---

## Down() Patterns

### Full reversal
Most common — every Up() operation reversed in reverse order.

### Empty Down() (data repair)
When the migration fixes bad data and rollback doesn't make sense:
```csharp
public override void Down()
{
    //
}
```

### Down() with data fixup before schema change
When reverting a nullable column back to NOT NULL:
```csharp
public override void Down()
{
    // Fix NULLs before tightening constraint
    Sql( @"UPDATE [dbo].[MyTable] SET [MyColumn] = 0 WHERE [MyColumn] IS NULL" );
    AlterColumn( "dbo.MyTable", "MyColumn", c => c.Int( nullable: false ) );
}
```
