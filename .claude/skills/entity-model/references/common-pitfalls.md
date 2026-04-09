# Entity Model Common Pitfalls

Known failure modes and mistakes when creating Rock RMS entity models. Review before finalizing.

---

## Pitfall 1: Wrong Base Class

Using `Entity<T>` when you need `Model<T>`:
- `Entity<T>` has NO audit columns, NO security, NO attributes
- `Model<T>` has all of the above
- **Use `Model<T>` by default.** Only use `Entity<T>` for lightweight log/junction entities.

If you inherit from `Entity<T>`, the entity will not support `CreatedDateTime`, `ModifiedDateTime`, attribute values, or security.

---

## Pitfall 2: Missing Class-Level Attributes

Every entity requires ALL FIVE of these:
1. `[RockDomain( "Domain" )]`
2. `[Table( "TableName" )]`
3. `[DataContract]`
4. `[CodeGenerateRest]` (or a specific variant)
5. `[Rock.SystemGuid.EntityTypeGuid( "GUID" )]`

Missing any of these causes code generation failures, missing REST endpoints, or entity type registration issues.

---

## Pitfall 3: FK Cascade Behavior

**Rock convention: almost always `WillCascadeOnDelete( false )`.**

Common mistakes:
- Forgetting to specify cascade behavior (EF defaults to cascade for required FKs)
- Using cascade delete on PersonAlias FKs (never cascade)
- Using cascade delete on DefinedValue FKs (never cascade)

Only use `WillCascadeOnDelete( true )` when the child entity has no meaning without the parent AND deletion should propagate (rare).

---

## Pitfall 4: Enum Not in Rock.Enums Project

Enums must be defined in `Rock.Enums/[Domain]/EnumName.cs`, NOT in the entity file or the Rock project directly.

Requirements:
- **Namespace:** `Rock.Model` (NOT `Rock.Enums.Domain`)
- **Attribute:** `[Enums.EnumDomain( "Domain" )]`
- XML doc comments on every value

If the enum is in the wrong location or namespace, code generation and Obsidian type generation will fail.

---

## Pitfall 5: Missing DataMember Attributes

- Every property exposed via REST API or serialization needs `[DataMember]`
- Required properties need `[DataMember( IsRequired = true )]`
- Navigation properties need `[DataMember]`

Missing `[DataMember]` means the property won't serialize to REST responses or Lava.

---

## Pitfall 6: Forgetting SystemGuid Registration

The GUID in `[Rock.SystemGuid.EntityTypeGuid]` must also be added as a constant to `Rock/SystemGuid/EntityType.cs`. Without this:
- Other code can't reference the entity type by GUID constant
- Migration code using `SystemGuid.EntityType.X` won't compile

---

## Pitfall 7: Wrong Namespace for Options POCO

Options POCOs use a domain-qualified namespace, NOT the standard `Rock.Model`:
- **Correct:** `namespace Rock.Model.Core.Category.Options`
- **Wrong:** `namespace Rock.Model`

---

## Pitfall 8: Pluralized Table Name

Rock removes the EF pluralizing convention. Table names should be **singular PascalCase**:
- **Correct:** `[Table( "GroupMember" )]`
- **Wrong:** `[Table( "GroupMembers" )]`

---

## Pitfall 9: Not Using `partial` Keyword

Entity classes and configuration classes must be `partial`:
- `public partial class EntityName : Model<EntityName>`
- `public partial class EntityNameConfiguration : EntityTypeConfiguration<EntityName>`

This allows the code generator to extend the class with service methods.

---

## Pitfall 10: Hardcoded Order Values in Seed Data

If the migration seeds data for this entity and the entity implements `IOrdered`, calculate `Order` from the existing max — don't hardcode 0.

---

## Pre-Finalization Checklist

Run through this before presenting the entity:

1. File path matches `Rock/Model/[Domain]/[EntityName]/[EntityName].cs`
2. Namespace is `Rock.Model`
3. Class is `partial`
4. All five class-level attributes present (`[RockDomain]`, `[Table]`, `[DataContract]`, `[CodeGenerateRest]`, `[EntityTypeGuid]`)
5. Inherits from `Model<T>` (or `Entity<T>` with justification)
6. Every property has XML doc comments with `<summary>` and `<value>` tags
7. Every property has `[DataMember]` (with `IsRequired = true` if `[Required]`)
8. FK properties are nullable (`int?`) unless the relationship is truly required
9. Navigation properties are `virtual`
10. EntityTypeConfiguration is in the same file in `#region Entity Configuration`
11. All FK relationships configured with explicit `WillCascadeOnDelete()` (usually `false`)
12. SystemGuid constant added to `Rock/SystemGuid/EntityType.cs`
13. GUID is uppercase, hyphenated, proper format
14. Copyright header is complete
15. `ToString()` override returns meaningful value (usually `Name`)
16. Any new enums are in `Rock.Enums/[Domain]/` with `[EnumDomain]` attribute
17. `using` statements are minimal and correct
