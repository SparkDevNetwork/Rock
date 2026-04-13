# Rock RMS Entity Model Patterns

Complete reference for entity class structure, property annotations, configuration, and service classes.

---

## File and Namespace Conventions

```
Rock/Model/[Domain]/[EntityName]/
├── [EntityName].cs              # Entity class + EntityTypeConfiguration (required)
├── [EntityName]Service.cs       # Custom service methods - partial class (optional)
├── [EntityName].SaveHook.cs     # Save lifecycle hooks (optional, ~95 entities have one)
├── [EntityName].Logic.cs        # Business logic methods (optional, for complex entities)
└── Options/                     # Query option POCOs (optional)
    └── [EntityName]QueryOptions.cs
```

**Namespace for entities, services, and enums:** `namespace Rock.Model`
**Namespace for Options POCOs:** `namespace Rock.Model.[Domain].[EntityName].Options`

---

## Copyright Header

Use the exact Rock copyright header defined in `.claude/rules/code-conventions.md`. Every new `.cs` file must include it.

---

## Base Class Choice: Model<T> vs Entity<T>

| Base Class | When to Use | What It Provides |
|---|---|---|
| `Model<T>` | **Default.** Standard entities with audit trails, security, and attributes. | `Id`, `Guid`, `ForeignId/Guid/Key`, audit columns (`CreatedDateTime`, `ModifiedDateTime`, `CreatedByPersonAliasId`, `ModifiedByPersonAliasId`), `ISecured`, `IHasAttributes` |
| `Entity<T>` | Simple/lightweight entities without audit trails (e.g., log tables, junction tables) | `Id`, `Guid`, `ForeignId/Guid/Key` only — no audit columns, no security, no attributes |

**Use `Model<T>` unless there's a specific reason not to.** Most entities use `Model<T>`.

---

## Class Declaration — Required Attributes

Every entity must have these five class-level attributes:

```csharp
[RockDomain( "DomainName" )]
[Table( "TableName" )]
[DataContract]
[CodeGenerateRest]
[Rock.SystemGuid.EntityTypeGuid( "GUID-HERE" )]
public partial class EntityName : Model<EntityName>
```

### Attribute Details

**`[RockDomain( "Domain" )]`** — Groups the entity by domain. Valid domains:
AI, Blocks, CheckIn, Cms, Communication, Connection, Controls, Core, Crm, Engagement, Event, Finance, Geography, Group, Lms, Mobile, Net, Observability, Reporting, Security, WebFarm, Workflow

**`[Table( "TableName" )]`** — Database table name. Use singular, PascalCase (e.g., `"Person"`, `"BinaryFile"`). Rock removes the pluralizing convention.

**`[DataContract]`** — Required for serialization.

**`[CodeGenerateRest]`** — Controls REST API endpoint generation. Options:
- `[CodeGenerateRest]` — Generate all endpoints
- `[CodeGenerateRest( Enums.CodeGenerateRestEndpoint.ReadOnly )]` — Read-only
- `[CodeGenerateRest( DisableEntitySecurity = true )]` — No entity security
- `[CodeGenerateRest( ~( Enums.CodeGenerateRestEndpoint.ReadAttributeValues | ... ) )]` — Exclude specific endpoints

**`[Rock.SystemGuid.EntityTypeGuid( "..." )]`** — Unique GUID for the entity type. Must also be registered in `Rock/SystemGuid/EntityType.cs`.

---

## Common Interfaces

Add these to the class declaration as needed:

| Interface | When to Use | Requires |
|---|---|---|
| `IOrdered` | Entity has a sort order | `int Order { get; set; }` property |
| `IHasActiveFlag` | Entity can be active/inactive | `bool IsActive { get; set; }` property |
| `ICacheable` | Entity has a corresponding cache object | `UpdateCache()` and `GetCacheObject()` methods |
| `IHasAdditionalSettings` | Entity stores JSON settings | `string AdditionalSettingsJson { get; set; }` property |

Example: `public partial class MyEntity : Model<MyEntity>, IOrdered, IHasActiveFlag`

---

## Property Patterns

**XML doc style note:** Newer entities (2024+) use `<summary>` tags only. Older entities also include `<value>` tags. For new entities, use the simpler `<summary>`-only style.

### Required String Property
```csharp
/// <summary>
/// The friendly name of this entity. This property is required.
/// </summary>
[Required]
[MaxLength( 255 )]
[DataMember( IsRequired = true )]
public string Name { get; set; }
```

### Optional String Property
```csharp
/// <summary>
/// The description of this entity.
/// </summary>
[DataMember]
public string Description { get; set; }
```

### Boolean Property
```csharp
/// <summary>
/// Indicates whether this entity is active.
/// </summary>
[Required]
[DataMember( IsRequired = true )]
public bool IsActive { get; set; } = true;
```

### Integer / Numeric Property
```csharp
/// <summary>
/// The order in which this entity should be displayed.
/// </summary>
[Required]
[DataMember( IsRequired = true )]
public int Order { get; set; }
```

### Enum Property
```csharp
/// <summary>
/// The classification of this notification.
/// </summary>
[Required]
[DataMember( IsRequired = true )]
public NotificationClassification Classification { get; set; }
```

### Nullable Foreign Key Property
```csharp
/// <summary>
/// The Id of the <see cref="Rock.Model.Campus"/> that this entity is associated with.
/// </summary>
[DataMember]
public int? CampusId { get; set; }
```

### Required Foreign Key Property
```csharp
/// <summary>
/// The Id of the <see cref="Rock.Model.Category"/> that this entity belongs to.
/// </summary>
[Required]
[DataMember( IsRequired = true )]
public int CategoryId { get; set; }
```

### DateTime Property
```csharp
/// <summary>
/// The date and time when this occurred.
/// </summary>
[DataMember]
public DateTime? OccurredDateTime { get; set; }
```

### Decimal Property
```csharp
/// <summary>
/// The monetary amount.
/// </summary>
[DataMember]
[DecimalPrecision( 18, 2 )]
public decimal Amount { get; set; }
```

### Indexed Property (Composite)
```csharp
/// <summary>
/// The Id of the entity type.
/// </summary>
[Index( "IX_EntityTypeId_EntityId", Order = 1 )]
[DataMember]
public int EntityTypeId { get; set; }

/// <summary>
/// The Id of the entity.
/// </summary>
[Index( "IX_EntityTypeId_EntityId", Order = 2 )]
[DataMember]
public int EntityId { get; set; }
```

---

## Navigation Properties

### Scalar Navigation (to parent entity)
```csharp
/// <summary>
/// The <see cref="Rock.Model.Campus"/> that this entity is associated with.
/// </summary>
[DataMember]
public virtual Campus Campus { get; set; }
```

**Note:** `[DataMember]` on navigation properties is common but not universal. Some newer entities omit it on navigation properties. Include it unless there's a reason to exclude the relationship from serialization.

### Collection Navigation — Newer inline style (preferred for new entities)
```csharp
/// <summary>
/// The child entities associated with this entity.
/// </summary>
public virtual ICollection<ChildEntity> ChildEntities { get; set; } = new Collection<ChildEntity>();
```

### Collection Navigation — Older lazy-load style (also acceptable)
```csharp
/// <summary>
/// Gets or sets a collection of <see cref="Rock.Model.ChildEntity"/> objects.
/// </summary>
[DataMember]
public virtual ICollection<ChildEntity> ChildEntities
{
    get { return _childEntities ?? ( _childEntities = new Collection<ChildEntity>() ); }
    set { _childEntities = value; }
}

private ICollection<ChildEntity> _childEntities;
```

---

## Entity Configuration (EF Fluent API)

The configuration class goes in the same file as the entity, in a `#region Entity Configuration` block at the bottom:

```csharp
#region Entity Configuration

/// <summary>
/// EntityName Configuration class.
/// </summary>
public partial class EntityNameConfiguration : EntityTypeConfiguration<EntityName>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNameConfiguration"/> class.
    /// </summary>
    public EntityNameConfiguration()
    {
        // Required FK — no cascade
        this.HasRequired( e => e.Category )
            .WithMany()
            .HasForeignKey( e => e.CategoryId )
            .WillCascadeOnDelete( false );

        // Optional FK — no cascade
        this.HasOptional( e => e.Campus )
            .WithMany()
            .HasForeignKey( e => e.CampusId )
            .WillCascadeOnDelete( false );
    }
}

#endregion
```

### FK Cascade Rules

See `.claude/rules/data-model.md` for the full FK cascade convention table. Summary: **default to `WillCascadeOnDelete( false )`**. PersonAlias FKs always false. Only use `true` for strict parent-child ownership.

---

## Entity File Structure (Region Order)

```csharp
namespace Rock.Model
{
    /// <summary> ... </summary>
    [RockDomain( "Domain" )]
    [Table( "TableName" )]
    [DataContract]
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( "GUID" )]
    public partial class EntityName : Model<EntityName>
    {
        #region Entity Properties
        // Scalar properties (Name, Description, FKs, etc.)
        #endregion

        #region Navigation Properties
        // Virtual navigation properties
        #endregion

        #region Public Methods
        // ToString() override, any public helpers
        #endregion
    }

    #region Entity Configuration
    // EntityTypeConfiguration<EntityName>
    #endregion
}
```

---

## SystemGuid Registration

Add a constant to `Rock/SystemGuid/EntityType.cs`:

```csharp
/// <summary>
/// The entity name description
/// </summary>
public const string ENTITY_NAME = "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX";
```

Constants use SCREAMING_SNAKE_CASE. GUIDs must be uppercase and hyphenated.

Then reference it in the entity class:
```csharp
[Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.ENTITY_NAME )]
```

Or inline the GUID string directly (both patterns exist in the codebase).

---

## Enum Definition

If the entity uses a new enum, create it in `Rock.Enums/[Domain]/EnumName.cs`:

```csharp
// <copyright>...</copyright>

namespace Rock.Model
{
    /// <summary>
    /// Description of the enum's purpose.
    /// </summary>
    [Enums.EnumDomain( "Domain" )]
    public enum EnumName
    {
        /// <summary>
        /// Description of first value.
        /// </summary>
        FirstValue = 0,

        /// <summary>
        /// Description of second value.
        /// </summary>
        SecondValue = 1
    }
}
```

**Requirements:**
- Namespace must be `Rock.Model`
- Must have `[Enums.EnumDomain( "Domain" )]` attribute
- XML doc comments on every value
- Explicit integer values recommended
- `[Description("Display Text")]` on values is common (requires `using System.ComponentModel;`)
- `[Flags]` attribute for combinable enums — use power-of-2 hex values (0x01, 0x02, 0x04...)

---

## Custom Service Class (Optional)

Only needed when the entity has custom query methods. Create as a partial class:

```csharp
// <copyright>...</copyright>

using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.EntityName"/> entities.
    /// </summary>
    public partial class EntityNameService
    {
        /// <summary>
        /// Gets entities by their category identifier.
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <returns>A queryable of matching entities.</returns>
        public IQueryable<EntityName> GetByCategoryId( int categoryId )
        {
            return Queryable().Where( e => e.CategoryId == categoryId );
        }
    }
}
```

The base `EntityNameService : Service<EntityName>` is auto-generated by Rock.CodeGeneration. The custom partial class extends it.

---

## Options POCO (When Needed)

For methods with multiple behavior-modifying parameters:

**File:** `Rock/Model/[Domain]/[Entity]/Options/[PocoName].cs`

```csharp
// <copyright>...</copyright>

using System.Collections.Generic;

namespace Rock.Model.[Domain].[EntityName].Options
{
    /// <summary>
    /// Options that define the behavior when querying entities.
    /// </summary>
    public class EntityNameQueryOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to include inactive entities.
        /// If <c>false</c>, only active entities are returned.
        /// </summary>
        /// <value><c>true</c> to include inactive entities; otherwise <c>false</c>.</value>
        public bool IncludeInactive { get; set; }
    }
}
```

---

## Auto-Discovery: No DbSet Registration Needed

Rock uses reflection-based auto-discovery in `RockContext.OnModelCreating()`:
- All `IEntity` implementations in the Rock assembly are automatically registered
- All `EntityTypeConfiguration` classes are loaded via `AddFromAssembly()`
- The pluralizing table name convention is removed (tables use singular names)

**You do NOT need to add DbSet<> properties or register the entity anywhere.** Just create the entity class with the correct base class and configuration, and EF discovers it automatically.

---

## Rock-Specific Property Attributes

| Attribute | Purpose |
|---|---|
| `[LavaVisible]` | Property accessible in Lava templates |
| `[LavaHidden]` | Property hidden from Lava templates |
| `[Previewable]` | Property shown in entity previews |
| `[IncludeForReporting]` | Property available in reporting |
| `[HideFromReporting]` | Property hidden from reporting |
| `[RockInternal( "X.Y" )]` | Internal API — not for plugin use |
| `[DecimalPrecision( 18, 2 )]` | Sets SQL decimal precision |
| `[DefinedValue( SystemGuid.DefinedType.X )]` | Links to a DefinedValue picker |
| `[TypeScriptType( "string" )]` | TypeScript type override for code generation |
| `[NotMapped]` | Excludes property from database — use for computed/derived properties |
| `[Analytics( true, true )]` | Entity supports analytics tables (params: supportsHistory, supportsAttributes) |
| `[CodeGenExclude( CodeGenFeature.DefaultRestController )]` | Exclude from specific code generation |
| `[Index( "IX_Name", Order = N )]` | Named composite index (use `Order` param for multi-column) |
| `[Index( IsUnique = true )]` | Unique index on a single column |

---

## ToString() Override

**Optional.** Not all entities have it — some newer entities skip it entirely. Include it when the entity has a meaningful string representation (typically `Name`):

```csharp
public override string ToString()
{
    return this.Name;
}
```

---

## SaveHook Pattern (Optional)

For entities that need custom logic during save operations (~95 entities have one). Create as a nested class in a separate partial file:

**File:** `Rock/Model/[Domain]/[EntityName]/[EntityName].SaveHook.cs`

```csharp
// <copyright>...</copyright>

using Rock.Data;

namespace Rock.Model
{
    public partial class EntityName
    {
        /// <summary>
        /// Save hook implementation for <see cref="EntityName"/>.
        /// </summary>
        internal class SaveHook : EntitySaveHook<EntityName>
        {
            /// <summary>
            /// Called before the save is executed.
            /// </summary>
            protected override void PreSave()
            {
                if ( PreSaveState == EntityContextState.Added )
                {
                    // Auto-populate fields on creation
                }
            }
        }
    }
}
```

**When to create:** When you need auto-population of fields, validation before save, cascading updates, or audit tracking beyond the standard audit columns.

---

## Flags Enum Pattern

For enums that represent combinable options:

```csharp
[Flags]
[Enums.EnumDomain( "Domain" )]
public enum DaysOfWeekFlags
{
    /// <summary>
    /// No days selected.
    /// </summary>
    None = 0x00,

    /// <summary>
    /// Sunday.
    /// </summary>
    Sunday = 0x01,

    /// <summary>
    /// Monday.
    /// </summary>
    Monday = 0x02,

    /// <summary>
    /// Tuesday.
    /// </summary>
    Tuesday = 0x04,

    // ... powers of 2 using hex notation

    /// <summary>
    /// All days.
    /// </summary>
    All = Sunday | Monday | Tuesday
}
```

---

## Enum Values with Description Attribute

Many enums use `[Description]` for display text:

```csharp
[Enums.EnumDomain( "AI" )]
public enum AgentType
{
    /// <summary>
    /// A standard agent.
    /// </summary>
    [Description( "Standard" )]
    Standard = 0,

    /// <summary>
    /// An MCP-based agent.
    /// </summary>
    [Description( "MCP" )]
    Mcp = 1
}
```

Requires `using System.ComponentModel;`
