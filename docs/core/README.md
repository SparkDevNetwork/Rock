# Core Documentation

This folder covers cross-cutting Rock concepts that recur across many domains and do not belong to any single feature area. If a concept would otherwise be re-explained in three or more domain docs (entity reference resolution, cache invalidation, RockContext lifecycle, PersonAlias semantics, Lava data object conventions, etc.), it lives here once and the domain docs link to it. Engineers writing or auditing core platform code, and authors of domain docs that touch a cross-cutting pattern, are the primary readers.

## Files in this directory

| Doc | Summary |
|---|---|
| [Cache Invalidation](cache-invalidation.md) | How `ItemCache` / `EntityCache` / `ModelCache` work, save-hook-driven eviction, web-farm propagation, and the cache-mirrors-model security rule. |
| [Cascade Picker Pattern](cascade-picker-pattern.md) | Parent dropdown plus v-if-gated children with pre-shipped option maps; clear-on-parent-change semantics and the phantom-emit guard for wrapping field-type editors. |
| [Composite Field Type Pattern](composite-field-type-pattern.md) | One attribute, several related values: pipe-delimited storage with a JSON edit shape, pre-shipped cascade data via `GetPublicConfigurationValues`, and `IEntityReferenceFieldType` for indexing. |
| [Defined Type and Defined Value](defined-type-and-value.md) | Configurable lookup-list pattern, Guid-based references, the cache pair, and the `Info` suffix convention for custom defined types. |
| [Entity Reference Resolution](entity-reference-resolution.md) | How Rock resolves entity references from raw int Id, Guid, or IdKey across blocks and services, and how the per-site Disable Predictable Ids setting governs which forms are accepted. |
| [Lava Data Object](lava-data-object.md) | `LavaDataObject` vs `RockDynamic`, the three usage patterns (derive, proxy, dictionary), and the `Info` suffix convention. |
| [Obsidian Block Lifecycle](obsidian-block-lifecycle.md) | Three-folder layout (C# / Vue / bags), `BlockAction` round-trips, `RockDetailBlockType`, configuration attributes, security gating. |
| [PersonAlias Semantics](person-alias-semantics.md) | Why audit columns reference PersonAlias, what merges actually do, and when direct `PersonId` is acceptable. |
| [RockContext Lifecycle](rock-context-lifecycle.md) | Unit-of-work model, the don't-dispose-prematurely rule, the `RockApp.Current.CreateRockContext()` factory pattern, and lazy-load constraints. |
| [Save Hook Pattern](save-hook-pattern.md) | `EntitySaveHook<TEntity>`, Pre/PostSave/SaveFailed lifecycle, when each runs, what belongs in a hook vs a service method. |
