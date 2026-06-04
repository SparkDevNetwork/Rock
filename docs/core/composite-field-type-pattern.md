---
title: Composite Field Type Pattern
last_updated: 2026-05-28
related_specs:
  - specs/completed/communication/260506-sms-action-create-connection-request.md
related_files:
  - Rock/Field/Types/ConnectionTypeSettingsFieldType.cs
  - Rock.JavaScript.Obsidian/Framework/FieldTypes/connectionTypeSettingsField.partial.ts
  - Rock.JavaScript.Obsidian/Framework/FieldTypes/connectionTypeSettingsFieldComponents.ts
---

# Composite Field Type Pattern

## Overview

A composite field type stores several related values in a single `AttributeValue.Value` and presents them in the editor as a structured shape. The persisted form on disk is one string (typically pipe-delimited guids); the editor exchanges a richer JSON object so the Vue component can drive multiple sub-pickers. `ConnectionTypeSettingsFieldType` is the canonical example: it stores Connection Type plus Opportunity, Status, and Type Source as a single attribute value used by SMS pipeline actions and Form Builder.

## Why It Exists

A single attribute is the right unit when multiple selections only make sense together. The four Connection slots are a single configuration decision ("which Connection Opportunity should this action create requests under, and which Status and Source should those requests use"), not four independent attributes. Modeling them as one attribute keeps the configuration UX coherent, the migration story simple (one attribute value to seed, one to upgrade), and the indexing logic centralised (`IEntityReferenceFieldType` returns the full set of references in one call). The alternative, four independent attributes, would split the cascade across four editor surfaces, force consumers to re-stitch them at runtime, and create a window where one slot is updated and the others are stale.

## Mental Model

Think of a composite field type as having two faces:

- A **private storage form** that lives in `AttributeValue.Value`. Short, stable, indexable. A pipe-delimited list of guids is the default shape because it sorts, hashes, and trims cheaply.
- A **public edit form** that the Obsidian editor sees. A JSON object with one property per slot, each shaped as a `ListItemBag` (`{ value, text }`) so the editor can render selected labels without re-fetching them.

The field type converts between the two forms. The C# class owns the conversion; the Obsidian editor never sees the pipe-delimited form, and the database never sees the JSON.

```mermaid
flowchart LR
    DB[(AttributeValue.Value<br/>"type|opp|status|source")] -->|GetPublicEditValue| EditForm[Public Edit JSON<br/>4 ListItemBag props]
    EditForm -->|GetPrivateEditValue| DB
    DB -->|GetTextValue| Display["Display text<br/>'Career > Career Coaching'"]
```

The slot order is fixed and positional. Empty slots are written as empty strings between pipes (`|opp||source`), not omitted, so the parser can split by index without keyword lookups. Any slot may be null independently of the others; the field type enforces nothing about combinations. Downstream consumers (the SMS action runtime, validation hooks) layer their own "Opportunity must be set" rule on top.

## What You Need to Know

**Two value shapes, one source of truth.** `GetPublicEditValue` reads the private string, hydrates the four guids into entity models for labels, and emits JSON. `GetPrivateEditValue` reads JSON, pulls the guid out of each `ListItemBag`, and re-joins the pipe-delimited string. The two methods are inverses; if you change one, change the other in the same edit.

**Pre-ship the option lists; do not round-trip per pick.** `GetPublicConfigurationValues` queries all upstream lookups (every Connection Type, every Opportunity grouped by Type, every Status grouped by Type, every Source grouped by Type) and ships them as JSON in the configuration values dictionary. The Obsidian editor consumes those four keys directly. No `[BlockAction]` is involved; the cascade is fully client-side once the editor mounts. This avoids per-pick latency and keeps the field type usable in places (Form Builder's per-field attribute editor) that do not host a block action surface.

**Limit the pre-shipped data to what an editor needs.** `GetPublicConfigurationValues` checks `ConfigurationValueUsage.Edit` or `Configure` before querying. Read-only views skip the heavy queries and get only the values they need to render the formatted text.

**Implement `IEntityReferenceFieldType` if your slots are entity guids.** Each non-null guid in the persisted value should be reported as a `ReferencedEntity` so Rock's attribute-value indexer knows the AttributeValue depends on those entities. `GetReferencedProperties` also tells the indexer which properties to watch (the slot's display name, almost always `Name`), so when a Connection Opportunity gets renamed the cached display text for every AttributeValue referencing it gets invalidated. Skipping `IEntityReferenceFieldType` means stale display text and broken cascade deletes on attribute values.

**Empty-everything maps to null.** `GetPrivateEditValue` returns `null` when every slot is empty, not an empty string and not `|||`. Rock's block-level "attribute is empty" checks treat null and empty as the same thing; non-null wire values with all-empty slots look filled when they are not, and downstream validation gates (the "this attribute is required" red rim) silently misfire. See `ConnectionTypeSettingsFieldType.cs:117` for the canonical null-collapse.

**Platform support is editor-bound.** Composite editors are Obsidian-only by default. `[RockPlatformSupport(Utility.RockPlatform.Obsidian)]` on the field type tells Rock not to offer the field in WebForms attribute editors. Adding WebForms support means writing parallel server-side cascading pickers; almost always not worth it.

**Field-type editors trip phantom dirty signals on mount.** The Vue editor mounts, parses `modelValue` into refs, and the four-ref watcher fires once with the rebuilt-from-scratch value. The wrapper guards against this by serialising the new value and short-circuiting the emit when it matches `props.modelValue`. See `connectionTypeSettingsFieldComponents.ts:120`. Composite editors that skip this guard make every parent's dirty tracking spuriously fire on initial render.

## Common Scenarios

**"I want a single attribute that captures Foo plus optional Bar and Baz."**

1. Subclass `FieldType` in `Rock/Field/Types/{Name}FieldType.cs`. Implement `GetPublicEditValue` and `GetPrivateEditValue` as the JSON/pipe-delimited inverse pair.
2. Implement `GetPublicConfigurationValues` to pre-ship the option lists for Foo/Bar/Baz, gated on `ConfigurationValueUsage.Edit/Configure`.
3. Implement `GetTextValue` for the formatted display (used by reports, grid columns, and Lava field renderings).
4. Implement `IEntityReferenceFieldType` if the slots reference entities by guid.
5. Add a `[Rock.SystemGuid.FieldTypeGuid("...")]` and a migration that inserts a row into `[FieldType]` with that guid.
6. Add a partial in `Rock.JavaScript.Obsidian/Framework/FieldTypes/{name}Field.partial.ts` declaring the wire-shape type and config keys.
7. Add an editor in `{name}FieldComponents.ts` that parses the modelValue into refs, renders the picker, and re-serialises on change (with the phantom-emit guard).

**"My composite needs server-side filtering of child lists by parent."**

You have two options. Pre-ship `{ ParentGuid: Children[] }` maps in `GetPublicConfigurationValues` (the approach used here) when the lists are small enough to fit in initialization payload. Otherwise expose a separate block action that the consuming block (not the field type editor) calls, and accept that the field type will only work inside that block. Pre-shipping is preferred for reusable field types.

## Key Architectural Decisions

### Pipe-delimited storage, not JSON

Attribute values are queried, indexed, and compared as strings throughout Rock. A pipe-delimited form sorts and hashes predictably and stays trivially regex-able from SQL when needed. JSON in `AttributeValue.Value` would force every consumer that touches the raw string to parse before comparing.

### Client-side cascade, not per-pick REST

Pre-shipping the option maps eliminates a round-trip per pick and keeps the field type usable in attribute editors that do not host block actions (Form Builder per-field attributes, Workflow attribute matrix). The cost is initial-load payload size; the wins are responsiveness and reusability.

### `ListItemBag` as the public edit shape, not raw guid strings

The editor needs to render a label for the currently selected option without re-querying. Shipping `{ value, text }` lets the picker show the label immediately on load. The guid alone would force the editor to look up the label in the pre-shipped lists on every render, which is workable but slower and fragile when persisted values reference items no longer in the list.

### Implement `IEntityReferenceFieldType` from day one

Adding it later means backfilling reference rows for every existing AttributeValue. Adding it up-front means the indexer is correct from the first AttributeValue ever written.

## Considered but Rejected

### Four separate attributes (Type, Opportunity, Status, Source)

Rejected. The four slots are a single decision; splitting them produces four editor surfaces, four independent validation states, and four migration steps to keep in sync.

### Stored procedure or table-backed configuration instead of attribute value

Rejected. Composite field types are still attributes; reusing the attribute framework gets Rock-Lava merge, attribute-matrix support, attribute-import/export, and cache invalidation for free.

### JSON in `AttributeValue.Value`

Rejected. Every existing consumer that touches raw attribute strings would have to parse before comparing. Pipe-delimited stays grep-friendly and sortable.

## Technical Reference

### Field type class skeleton

```csharp
[FieldTypeUsage(FieldTypeUsage.System)]
[RockPlatformSupport(Utility.RockPlatform.Obsidian)]
[Rock.SystemGuid.FieldTypeGuid("...")]
public class ConnectionTypeSettingsFieldType : FieldType, IEntityReferenceFieldType
{
    private static class ConfigKey
    {
        public const string ConnectionTypes = "connectionTypes";
        public const string ConnectionOpportunitiesByType = "connectionOpportunitiesByType";
        public const string ConnectionStatusesByType = "connectionStatusesByType";
        public const string ConnectionTypeSourcesByType = "connectionTypeSourcesByType";
    }

    public override string GetPublicEditValue(string privateValue, Dictionary<string, string> privateConfigurationValues) { ... }
    public override string GetPrivateEditValue(string publicValue, Dictionary<string, string> privateConfigurationValues) { ... }
    public override Dictionary<string, string> GetPublicConfigurationValues(Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string privateValue) { ... }
    public override string GetTextValue(string privateValue, Dictionary<string, string> privateConfigurationValues) { ... }
}
```

See `Rock/Field/Types/ConnectionTypeSettingsFieldType.cs` for the full implementation.

### Parse helper

A static `ParseDelimitedGuids` method on the field type lets consumers (the SMS action runtime, validators, integration tests) split the persisted value without re-implementing the slot order. See `ConnectionTypeSettingsFieldType.cs:213`.

### Public edit shape

The wire shape is declared in `Rock.JavaScript.Obsidian/Framework/FieldTypes/connectionTypeSettingsField.partial.ts`:

```ts
export type ConnectionTypeSettings = {
    connectionType: ListItemBag | null;
    connectionOpportunity: ListItemBag | null;
    connectionStatus: ListItemBag | null;
    connectionTypeSource: ListItemBag | null;
};
```

The editor parses `props.modelValue` (JSON string) into four refs, re-serialises on change, and emits `update:modelValue`. See `connectionTypeSettingsFieldComponents.ts:87`.

### Migration to register the field type

```csharp
public override void Up()
{
    // Insert into [FieldType] with the field type guid + assembly-qualified type name.
    // FieldTypeService.RegisterFieldTypes() picks it up at app startup.
}
```

`FieldTypeService.RegisterFieldTypes()` runs at startup and reads `[Rock.SystemGuid.FieldTypeGuid(...)]` on every type that derives from `FieldType`. A migration that inserts the row is still required so existing databases pick up the new type.

### Related cross-cutting docs

- [Obsidian Block Lifecycle](obsidian-block-lifecycle.md) for the C#/Vue/bag triad that hosts these editors.
- [Entity Reference Resolution](entity-reference-resolution.md) for how the slot guids resolve when read back at runtime.

## Related Specs

- [Create Connection Request SMS Action](../../specs/completed/communication/260506-sms-action-create-connection-request.md) — 2026-05-06 (Josh Henninger)
