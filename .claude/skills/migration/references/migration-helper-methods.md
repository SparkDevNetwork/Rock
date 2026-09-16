# RockMigrationHelper Quick Reference

Key method signatures from `Rock/Data/MigrationHelper.cs` (~9,500 lines). When in doubt about parameter order, read the actual source file.

---

## Entity Type

```csharp
UpdateEntityType( string name, string guid, bool isEntity, bool isSecured )
UpdateEntityType( string name, string friendlyName, string assemblyName, bool isEntity, bool isSecured, string guid )
RenameEntityType( string guid, string name, string friendlyName, string assemblyName, bool isEntity, bool isSecured )
DeleteEntityType( string guid )
```

## Block Type

There are two families of block type methods — one for **WebForms** (path-based) and one for **Obsidian/Mobile** (entity-based). Using the wrong family is a serious bug that has caused data loss in production.

### Obsidian / Mobile blocks (entity-based — NO path)

Use these for any block whose class lives in `Rock.Blocks.*` or `Rock.Blocks.Types.Mobile.*`:

```csharp
// Primary method for Obsidian and Mobile blocks
AddOrUpdateEntityBlockType( string name, string description, string entityTypeName, string category, string guid )

// Legacy alias — same as above, do not use for new code
UpdateMobileBlockType( string name, string description, string entityName, string category, string guid )
```

These methods look up the `EntityTypeId` from the entity name and set it on the BlockType. They do NOT set a Path.

### WebForms blocks (path-based — `~/Blocks/...`)

Use these ONLY for blocks that have a physical `.ascx` file:

```csharp
// WebForms only — takes a path like "~/Blocks/Core/SomeBlock.ascx"
UpdateBlockType( string name, string description, string path, string category, string guid )
UpdateBlockTypeByGuid( string name, string description, string path, string category, string guid )
AddBlockType( string name, string description, string path, string category, string guid )
RenameBlockType( string oldPath, string newPath, string newCategory = null, string newDescription = null )
```

**WARNING:** `UpdateBlockTypeByGuid()` contains SQL that does `DELETE FROM [BlockType] WHERE [Path] = '{path}'`. If you accidentally pass a path for an Obsidian block (which has no path), this can delete unrelated block types with empty Path values. This exact bug occurred in a production migration.

### How to tell which family to use

- Block class is in `Rock.Blocks.*` or `Rock.Blocks.Types.Mobile.*` → **Entity-based** → use `AddOrUpdateEntityBlockType()`
- Block class has a `.ascx` file in `RockWeb/Blocks/` → **Path-based** → use `UpdateBlockType()` or `UpdateBlockTypeByGuid()`
- If unsure, check the block's registration: does it have a `[Path]` or an `[EntityTypeId]` in the BlockType table?

### Shared methods (work for both types)

```csharp
DeleteBlockType( string guid )  // Safe for both — deletes by GUID only
```

## Page

```csharp
AddPage( bool isSystem, string parentPageGuid, string layoutGuid, string name, string description, string guid, string iconCssClass = "" )
MovePage( string pageGuid, string newParentPageGuid )
DeletePage( string guid )
RenamePage( string pageGuid, string newName )
UpdatePageIcon( string pageGuid, string iconCssClass )
UpdatePageLayout( string pageGuid, string layoutGuid )
```

## Page Route

```csharp
AddPageRoute( string pageGuid, string route, string guid )
AddOrUpdatePageRoute( string pageGuid, string route, string guid )
DeletePageRoute( string guid )
```

## Block

```csharp
AddBlock( bool isSystem, Guid pageGuid, Guid? layoutGuid, Guid siteGuid, Guid blockTypeGuid, string name, string zone, string preHtml, string postHtml, int order, string guid )
DeleteBlock( string guid )
```

Note: `pageGuid` and `blockTypeGuid` use `.AsGuid()` when passing string GUIDs.

## Block Attributes

```csharp
AddOrUpdateBlockTypeAttribute( string blockTypeGuid, string fieldTypeGuid, string name, string key, string category, string description, int order, string defaultValue, string guid )
AddBlockTypeAttribute( string blockTypeGuid, string fieldTypeGuid, string name, string key, string category, string description, int order, string defaultValue, string guid )
AddBlockAttributeValue( string blockGuid, string attributeGuid, string value )
```

## Defined Type / Defined Value

```csharp
AddDefinedType( string category, string name, string description, string guid, string helpText = "" )
UpdateDefinedType( string guid, string category, string name, string description, string helpText = "" )
DeleteDefinedType( string guid )

AddDefinedValue( string definedTypeGuid, string value, string description, string guid, bool isSystem = true )
UpdateDefinedValue( string guid, string value, string description, int order )
DeleteDefinedValue( string guid )
```

## Attribute (general)

```csharp
UpdatePersonAttribute( string fieldTypeGuid, string categoryGuid, string name, string abbreviatedName, string key, string iconCssClass, string description, int order, string defaultValue, string guid )
AddOrUpdatePersonAttributeByGuid( string fieldTypeGuid, string categoryGuid, string name, string abbreviatedName, string key, string iconCssClass, string description, int order, string defaultValue, string guid )
```

## Category

```csharp
UpdateCategory( string entityTypeGuid, string name, string iconCssClass, string description, string guid, int order = 0, string parentCategoryGuid = "" )
DeleteCategory( string guid )
```

## Security / Auth

```csharp
AddSecurityAuth( string entityTypeName, string action, string groupGuid, string guid )
AddSecurityAuthForPage( string pageGuid, int order, string action, bool allow, string groupGuid, int specialRole, string authGuid )
AddSecurityAuthForBlockType( string blockTypeGuid, int order, string action, bool allow, string groupGuid, int specialRole, string authGuid )
AddSecurityAuthForEntityType( string entityTypeName, int order, string action, bool allow, string groupGuid, Rock.Model.SpecialRole specialRole, string authGuid )
```

## Group Type

```csharp
AddGroupType( string name, string description, string groupTerm, string groupMemberTerm, bool allowMultipleLocations, bool showInGroupList, bool showInNavigation, string iconCssClass, int order, string inheritedGroupTypeGuid, string guid )
UpdateGroupType( string guid, string name, string description, string groupTerm, string groupMemberTerm )
AddGroupTypeRole( string groupTypeGuid, string name, string description, int order, bool isLeader, bool canView, bool canEdit, string guid, bool canManageMembers = false )
DeleteGroupType( string guid )
```

## Service Jobs

```csharp
AddPostUpdateServiceJob( string name, string description, string jobType, string cronExpression, string guid )
```

## Misc

```csharp
DeleteByGuid( string guid, string tableName )  // Generic delete any record by GUID
AddOrUpdateLavaShortcode( string name, string description, string documentation, string markup, string guid, ... )
DeleteLavaShortcode( string guid )
```

---

## Common SystemGuid Constants

Access via `Rock.SystemGuid.*` for compile-time safety:

| Constant | Value | Used For |
|---|---|---|
| `Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY` | `790E3215-3B10-442B-AF69-616C0DCB998E` | Family group type |
| `Rock.SystemGuid.Page.CONNECTION_OPPORTUNITY_SELECT` | (check file) | Connection pages |
| `Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON` | `36CF10D6-C695-413D-8E7C-4546EFEF385E` | Person record type |
| `Rock.SystemGuid.Person.SYSTEM_SENDER` | (check file) | System sender person |

For the full list, browse `Rock/SystemGuid/*.cs`.

---

## Common Field Type GUIDs (for attributes)

| Field Type | GUID |
|---|---|
| Text | `9C204CD0-1233-41C5-818A-C5DA439445AA` |
| Boolean | `1EDAFDED-DFE6-4334-B019-6EECBA89E05A` |
| Integer | `A75DFC58-7A1B-4799-BF31-451B2BBE38FF` |
| Date | `6B6AA175-4758-453F-8D83-FCD8044B5F36` |
| Person | `E4EAB7B2-0B76-429B-AFE4-AD86D7428C70` |
| Defined Value | `59D5A94C-94A0-4630-B80A-BB21FAF1E9E9` |
| Page Reference | `BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108` |
| Single Select (DropDown) | `7525C4CB-EE6B-41D4-9B64-A08048D5A5C0` |
