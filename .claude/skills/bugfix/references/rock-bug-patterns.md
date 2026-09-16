# Rock RMS Common Bug Patterns

Organized by symptom. When investigating a bug, scan this list for patterns that match the reported behavior — they'll point you toward the root cause faster than reading code blind.

---

## Data Appears Stale or Wrong After Save

**Cache not invalidated.** Rock caches aggressively — `EntityTypeCache`, `DefinedTypeCache`, `GroupTypeCache`, `CategoryCache`, `BlockTypeCache`, and many more. If data is saved to the database but the UI still shows old values, the cache layer is likely serving stale data. Look for:
- Direct database updates that bypass the service layer (service methods trigger cache invalidation; raw SQL or `DbContext.SaveChanges()` without the service may not)
- Missing `FlushItem()` or `FlushAll()` calls after modifying cached entities
- Cache read happening before the save transaction commits

**RockContext disposed too early.** If `RockContext` is disposed (via `using` block or explicit `.Dispose()`) before all lazy-loaded navigation properties are accessed, those properties silently return null/empty instead of throwing. The data *looks* wrong but it's actually just unloaded. Check whether the context lifetime covers all property access.

**PersonAlias vs Person confusion.** Audit columns and custom FK columns should reference `PersonAlias`, not `Person`. If a person merge happened, the old `PersonId` becomes invalid but the `PersonAliasId` still resolves. If the code uses `PersonId` where it should use `PersonAliasId`, data will appear to "disappear" after a merge.

---

## NullReferenceException / Object Not Set

**Navigation property not loaded.** Entity Framework lazy loading requires the context to still be alive. If you see a null reference on a navigation property (e.g., `person.PrimaryAlias.Id`), check that the query includes `.Include()` or that the context hasn't been disposed.

**Cache miss returning null.** `EntityTypeCache.Get( id )` returns null if the ID doesn't exist. Code that chains `.Get().Name` without a null check will NRE. This often happens with stale IDs from user input or configuration.

**Rock block not initialized.** Obsidian blocks receive configuration via `InitializationBox`. If the box isn't fully populated on the C# side, the Vue component may dereference undefined properties. Check that all required bag properties are set in `GetInitializationBox()` or `GetEntityBagForView()`.

---

## Security / Permission Bugs

**Missing `IsAuthorized` check.** Rock blocks should verify `IsAuthorized( Authorization.VIEW )` or `Authorization.EDIT` before returning data. If a block action returns data without checking, it's a security bypass. The check typically goes in `BlockCustomActionBag` methods or early in `GetInitializationBox`.

**Wrong authorization entity.** The authorization check might be on the block type when it should be on the entity, or vice versa. Check what object `IsAuthorized` is called on.

---

## UI Doesn't Update / Reactivity Broken (Obsidian)

**Bag property name mismatch.** The C# ViewBag/Bag property names are auto-generated to camelCase in TypeScript. If the C# property is `IsActive` but the TypeScript reads `isActive`, it works. But if someone manually typed `is_active` or `active`, it won't bind. Check exact property name alignment between the C# bag and the `.obs` component.

**Watch vs computed not triggering.** Vue 3 reactivity requires refs or reactive objects. If a value is destructured out of a reactive object (e.g., `const { name } = props.modelValue`), `name` loses reactivity. The fix is to use `computed()` or `toRef()`.

**BlockAction response not applied.** If a block action returns updated data but the Vue component doesn't update the reactive state with the response, the UI stays stale. Check that the `invokeBlockAction` result is actually assigned back to the ref.

---

## Query Returns Wrong Results

**Missing filter.** The most common query bug in Rock is a forgotten `.Where()` clause. Check that queries filter by:
- `IsActive` (most entities have this)
- `IsSystem` (system records often should be excluded from user-facing queries)
- `IsAuthorized` (for security-sensitive queries — though this is often post-query)

**Wrong join or wrong FK.** If the query joins on `PersonId` instead of `PersonAliasId`, or uses the wrong navigation property, results will be wrong. Cross-reference the entity model to verify FK relationships.

**LINQ `Guid` vs `Id` in Where clause.** Using `Guid` in a `.Where()` is slower and can produce unexpected results if there are Guid format mismatches. Prefer `Id` when the value is available (often from a cache).

**Large `Contains()` list.** `.Where( x => list.Contains( x.Id ) )` generates a SQL `IN` clause. If `list` has >2,000 items, it can exceed batch size limits or timeout. Prefer an `IQueryable` subquery so EF generates a subquery instead.

---

## Singleton / Thread-Safety Bugs

**Class variable on a singleton.** Rock has many singletons: Workflow Actions, FieldTypes, Cache types, etc. Declaring instance variables (non-static fields) on these means all requests share the same state, causing race conditions. The symptom is intermittent, hard-to-reproduce wrong data or exceptions. Look for instance fields on classes that inherit from common singleton bases.

**In-process `lock()` in web farm.** `lock()` only works within a single process. In a clustered/web-farm environment, multiple servers run independently. If the code uses `lock()` to prevent concurrent writes, it won't work across servers. The fix is usually a database-level unique constraint or advisory lock.

---

## Data Loss / Destructive Bugs

**`UpdateBlockTypeByGuid()` with empty path.** This method contains `DELETE FROM [BlockType] WHERE [Path] = '{path}'`. Obsidian/Mobile block types have an empty `Path`, so calling this with an empty path deletes ALL entity-based block types. This has caused production data loss. Always use `AddOrUpdateEntityBlockType()` for Obsidian/Mobile blocks.

**Cascade delete surprises.** Check FK cascade settings. Rock defaults to `WillCascadeOnDelete( false )`, but if someone set it to true, deleting a parent entity will silently delete children. Verify cascade behavior in the `EntityTypeConfiguration`.

---

## Cross-Layer Bugs (C# ↔ TypeScript ↔ SQL)

These bugs are the hardest to find because each layer looks correct in isolation.

**Type mismatch across layers.** C# `int?` serializes as `null` in JSON, but TypeScript may treat `0` and `null` differently. C# `decimal` may lose precision in JSON round-trip. C# `DateTime` vs TypeScript string date handling. Check that the type conversion at each boundary is correct.

**Enum value mismatch.** C# enums serialize as integers by default. If the TypeScript side expects the string name, or vice versa, the value will be wrong. Check `[EnumMember]` attributes and any custom JSON converters.

**Bag not updated in both directions.** If a save operation reads from the bag but the bag was populated from a different code path than the one that validates/transforms the data, the save may write stale or invalid values. Trace the full round-trip: C# populates bag → TypeScript displays/edits → TypeScript sends back → C# reads and saves.
