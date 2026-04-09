---
name: entity-model
description: >-
  Scaffold a new Rock RMS entity model with all required files and conventions.
  Creates the entity class, EntityTypeConfiguration, SystemGuid entry, optional
  enum definitions, and optional service class. Use when the user says "create
  an entity", "new entity model", "scaffold entity", "add a new table",
  "new model class", or describes a new domain object that needs a database table.
  Also use when asked to "review entity model" or "check my entity".
  Do NOT use for ViewModels/Bags — use /bag-generator instead.
  Do NOT use for migrations — use /migration after scaffolding the entity.
argument-hint: "Describe the entity to create (e.g., 'a CampusSchedule entity in the Core domain that links a Campus to a Schedule with an IsActive flag'), or say 'review' to review an existing entity"
compatibility: Requires Claude Code with access to the Rock RMS codebase.
metadata:
  version: "1.0"
  author: "Maxwell Eley"
---

# Rock RMS Entity Model Scaffolder

You are scaffolding a new entity model in the Rock RMS codebase, or reviewing an existing one. Rock entities follow strict conventions — this skill enforces them.

**The user's request:** $ARGUMENTS

---

## Reference Routing Table

Load reference files progressively — only when needed.

| Reference File | Load When |
|---|---|
| `references/entity-patterns.md` | Before writing any entity code (always in write mode) |
| `references/common-pitfalls.md` | Before finalizing — both write and review modes |

**Do NOT read all files upfront.** Read `entity-patterns.md` always for write mode; read `common-pitfalls.md` before finalizing.

---

## Step 1 — Understand the Request and Determine Mode

**Write mode** — The user describes a new entity or says "create", "scaffold", "add". Proceed to **Step 2 (Write Mode)**.

**Review mode** — The user says "review", "check", "audit", or names an existing entity file. Proceed to **Step 2 (Review Mode)**.

For write mode, parse the user's intent:
- **Entity name** — PascalCase, singular (e.g., `CampusSchedule`, not `CampusSchedules`)
- **Domain** — Which Rock domain it belongs to (Core, CRM, Communication, Connection, Event, Finance, Group, Engagement, LMS, etc.)
- **Properties** — What data fields does it need?
- **Relationships** — What other entities does it relate to? Required or optional FKs?
- **Interfaces** — Does it need ordering (`IOrdered`), active flag (`IHasActiveFlag`), caching (`ICacheable`)?
- **Base class** — `Model<T>` (default — includes audit columns, security, attributes) or `Entity<T>` (simple entities without audit)

If the user doesn't specify all of these, make reasonable decisions based on the entity's purpose, but ask about the domain if unclear.

---

# Write Mode

## Step 2 — Research Existing Patterns

Before writing any code:

1. **Read `references/entity-patterns.md`** for the complete entity template and conventions
2. **Check if the entity already exists:** `Rock/Model/**/{EntityName}.cs`
3. **Read related entity models** that this entity will reference (FK targets) — verify their exact property names, types, and table names
4. **Check `Rock/SystemGuid/EntityType.cs`** for naming conventions and to ensure no GUID conflicts
5. **Check `Rock.Enums/`** if the entity needs new enum types — verify the domain folder exists

## Step 3 — Generate SystemGuid

Every entity needs a unique GUID. There are two approaches used in the codebase:

**Option A — SystemGuid constant (preferred for new entities):**
1. Generate a new GUID (uppercase, hyphenated format)
2. Add a constant to `Rock/SystemGuid/EntityType.cs` in alphabetical order
3. Reference it in the entity class attribute

```csharp
// In Rock/SystemGuid/EntityType.cs:
/// <summary>
/// The campus schedule entity type
/// </summary>
public const string CAMPUS_SCHEDULE = "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX";

// In the entity class:
[Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.CAMPUS_SCHEDULE )]
```

**Option B — Inline GUID string:** Many entities (especially newer ones) use an inline GUID string directly in the attribute instead of a SystemGuid constant. This is acceptable but less refactorable:
```csharp
[Rock.SystemGuid.EntityTypeGuid( "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX" )]
```

Use Option A when the entity type GUID will be referenced in migrations, seed data, or other code. Use Option B for entities that are only referenced by type name.

## Step 4 — Write the Entity File

**File path:** `Rock/Model/[Domain]/[EntityName]/[EntityName].cs`
**Namespace:** `Rock.Model`

The entity file contains both the entity class and its EntityTypeConfiguration in the same file. Read `references/entity-patterns.md` for the complete template and all annotation requirements.

Key structural requirements:
- Copyright header (exact format from reference)
- Class-level attributes: `[RockDomain]`, `[Table]`, `[DataContract]`, `[CodeGenerateRest]`, `[Rock.SystemGuid.EntityTypeGuid]`
- Inherit from `Model<T>` (or `Entity<T>` for non-audited entities)
- Properties organized in `#region Entity Properties` / `#region Navigation Properties` / `#region Public Methods`
- `EntityTypeConfiguration` class in `#region Entity Configuration` at the bottom of the file
- Every FK relationship configured in the EntityTypeConfiguration constructor

## Step 5 — Write Supporting Files (if needed)

### Enum definitions
If the entity has enum-typed properties, create the enum file:
- **File:** `Rock.Enums/[Domain]/[EnumName].cs`
- **Namespace:** `Rock.Model`
- **Required attribute:** `[Enums.EnumDomain( "Domain" )]`
- XML doc comment on every enum value

### Custom service methods
Only create `Rock/Model/[Domain]/[EntityName]/[EntityName]Service.cs` if the entity needs custom query methods beyond standard CRUD. The base `Service<T>` class (auto-generated via CodeGeneration) provides standard operations.

If created:
- It's a `partial class` extending the auto-generated service
- Constructor takes `RockContext`
- Contains domain-specific query methods (e.g., `GetByParentId()`)

### Options POCOs
If a service method needs multiple behavior-modifying parameters, create an Options POCO:
- **File:** `Rock/Model/[Domain]/[EntityName]/Options/[PocoName].cs`
- **Namespace:** `Rock.Model.[Domain].[EntityName].Options`

## Step 6 — Validate

Read `references/common-pitfalls.md` and run through the checklist before presenting.

**Output:** Write the files and show their contents in conversation. Remind the user of next steps:
1. Run `Rock.CodeGeneration` (WPF app) to generate the `[Entity]Service.CodeGenerated.cs` file
2. Use `/migration` to scaffold and write the EF migration that creates the table
3. Use `/bag-generator` (if available) to create ViewModels/Bags for Obsidian blocks

---

# Review Mode

## Step 2 (Review Mode) — Find and Read the Entity

1. If `$ARGUMENTS` names a specific entity, find it: `Rock/Model/**/{EntityName}.cs`
2. Read the full entity file — class, properties, navigation, configuration
3. If a custom service file exists, read that too

## Step 3 (Review Mode) — Research for Comparison

- Read `references/entity-patterns.md` for expected patterns
- Read related entity models referenced by FKs — verify relationship correctness
- Check `Rock/SystemGuid/EntityType.cs` — verify the GUID exists and matches
- If enums are used, verify they exist in `Rock.Enums/[Domain]/` with correct attributes

## Step 4 (Review Mode) — Audit

Read `references/common-pitfalls.md` and run through the checklist, plus:

### Structural checks
- Inherits `Model<T>` or `Entity<T>` (not raw `DbContext` entity)
- Copyright header present
- All five class-level attributes present
- `partial class` keyword used
- EntityTypeConfiguration class present in `#region Entity Configuration`

### Property checks
- All properties have XML doc comments (at minimum `<summary>` tags; older entities also have `<value>` tags)
- `[DataMember]` on entity properties (navigation properties may omit it — check if entity follows newer or older conventions)
- `[DataMember( IsRequired = true )]` on required properties
- `[Required]` on non-nullable value-type properties and required string properties
- `[MaxLength]` on string properties that need length constraints
- FK properties are nullable `int?` for optional relationships, non-nullable `int` for required

### Relationship checks
- Every FK property has a matching navigation property
- Every FK has a configuration entry in EntityTypeConfiguration
- Cascade delete rules are appropriate (PersonAlias FKs: always `false`; parent-child: depends)

## Step 5 (Review Mode) — Report Findings

Present findings using severity levels:

| Severity | Meaning | Examples |
|---|---|---|
| **CRITICAL** | Must fix — will cause build errors, runtime failures, or data issues | Missing EntityTypeConfiguration, wrong base class, missing SystemGuid |
| **WARNING** | Should fix — violates conventions or could cause subtle bugs | Missing `[DataMember]`, wrong cascade rule, missing XML docs |
| **NOTE** | Consider — minor improvement | Could implement `IOrdered`, property naming suggestion |

End with a summary: total findings by severity and a go/no-go recommendation.

**If the entity is clean:** Say so. Don't invent issues to fill a report.

---

## Examples

### Example 1: Simple entity with FK

User says: "Create a CampusSchedule entity in Core that links a Campus to a Schedule with an IsActive flag"

Actions:
1. Generate GUID, add to `Rock/SystemGuid/EntityType.cs`
2. Create `Rock/Model/Core/CampusSchedule/CampusSchedule.cs` with:
   - `CampusId` (required FK), `ScheduleId` (required FK), `IsActive` (bool)
   - Navigation properties for Campus and Schedule
   - EntityTypeConfiguration with `HasRequired` for both FKs, cascade delete false
3. No enum or custom service needed

Result: Two files written — SystemGuid entry and entity file. User reminded to run CodeGeneration and `/migration`.

### Example 2: Entity with enum and ordering

User says: "Create a NotificationPreference entity in Communication with a PreferenceType enum (Email, SMS, Push) and an Order property"

Actions:
1. Create enum file: `Rock.Enums/Communication/NotificationPreferenceType.cs`
2. Generate GUID, add to `Rock/SystemGuid/EntityType.cs`
3. Create entity implementing `IOrdered` with `PreferenceType` enum property and `Order` int property

Result: Three files written — enum definition, SystemGuid entry, and entity file with `IOrdered` interface.

### Example 3: Entity with custom service methods

User says: "Create a MembershipCard entity in Group with a PersonAliasId, GroupId, ExpirationDate, and a service method to get active cards for a person"

Actions:
1. Generate GUID, add to SystemGuid
2. Create entity with FK properties and ExpirationDate
3. Create `MembershipCardService.cs` partial class with `GetActiveCardsForPerson(int personAliasId)` method

Result: Three files written — SystemGuid entry, entity file, and custom service file.

### Example 4: Review an existing entity

User says: "review the AIAgent entity"

Actions:
1. Find and read `Rock/Model/AI/AIAgent/AIAgent.cs`
2. Check against entity patterns checklist
3. Verify SystemGuid exists, FK configurations are correct, properties are properly annotated

Result: Severity-based findings report with go/no-go recommendation.

---

## Troubleshooting

**"Entity not discovered by EF":** Rock uses reflection to auto-discover `IEntity` implementations. Ensure the class inherits from `Model<T>` or `Entity<T>`, is `public`, is not `abstract`, and doesn't have `[NotMapped]`. No DbSet registration is needed.

**"CodeGeneration didn't produce a service file":** The CodeGeneration WPF app (`Rock.CodeGeneration/`) generates `[Entity]Service.CodeGenerated.cs` files. It must be run manually after creating a new entity. The generated file goes in `Rock/Model/CodeGenerated/`.

**"Table name conflicts":** The `[Table]` attribute value should match the entity name (singular, no prefix). Rock removes the pluralizing convention, so `[Table("CampusSchedule")]` maps to table `[CampusSchedule]`.

**"FK cascade delete causing issues":** Default to `WillCascadeOnDelete( false )` for most relationships. Only use `true` for true parent-child ownership where deleting the parent should delete all children. PersonAlias audit FKs must always be `false`.

**"Enum not recognized in entity":** Ensure the enum is in `Rock.Enums/[Domain]/` with namespace `Rock.Model` and has the `[Enums.EnumDomain("Domain")]` attribute. The enum project must be referenced by the Rock project.

**"Build error: 'EntityTypeConfiguration' not found":** Add `using System.Data.Entity.ModelConfiguration;` to the entity file's using statements.
