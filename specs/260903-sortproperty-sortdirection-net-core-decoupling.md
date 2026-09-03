---
author: Daniel Hazelbaker
date_created: 2026-09-03
summary: >-
  Decouple Rock's sorting types from System.Web for .NET Core compatibility:
  replace System.Web.UI.WebControls.SortDirection on the small set of declaration
  sites, introduce a UI-free Rock.Data.SortProperty alongside the legacy
  Rock.Web.UI.Controls.SortProperty (bridged by implicit operators), and add a
  non-generic IService.Get overload so the reporting/data-view code can drop the
  reflection it currently uses to reach Service<T>.Get.
contributors: []
---

# SortProperty / SortDirection .NET Core Decoupling

## Summary

The next-gen (.NET Core) branch cannot reference `System.Web.UI.WebControls.SortDirection`, and today Rock's core sorting types depend on it. This spec captures the decoupling work in three connected parts: (1) replace the WebForms `SortDirection` enum on the handful of *declaration* sites with a cross-platform enum; (2) lift the `SortProperty` DTO out of the WebForms `Grid.cs` into a new `Rock.Data.SortProperty` while keeping the legacy `Rock.Web.UI.Controls.SortProperty` in place, bridged by implicit conversion operators so existing Grid/WebForms code needs no edits; and (3) add a non-generic `Get(...)` overload to `IService` so the reporting and data-view code can call it directly instead of via reflection.

This is likely to be split into two paired specs later (one for the `SortProperty` type work, one for the `IService` change, which is a deliberate breaking change). It is kept as a single document for now so the full analysis is captured in git.

## Motivation

`System.Web` is not available on .NET Core, and `System.Web.UI.WebControls.SortDirection` is baked into several core types that must compile cross-platform. The most consequential is `SortProperty`, the sort descriptor threaded through `Service<T>`, `LinqExtensions`, the reporting builders, and the data-view query pipeline. Its `Direction` property is the WebForms enum, and the type itself lives inside the WebForms-only `Grid.cs`, so it cannot exist on the next-gen branch as written.

A second, related problem surfaced during analysis: the data-view and reporting code reaches `Service<T>.Get(..., SortProperty)` through reflection (`GetType().GetMethod("Get", ...)`) rather than a direct call. This reflection is brittle (a `typeof(SortProperty)` argument that must be kept in exact sync with the method signature) and exists only because the sort overloads are not on the non-generic `IService` interface. Any change to the `SortProperty` type risks silently breaking those reflection lookups at runtime, so the two problems are best solved together.

## Background: the `SortDirection` declaration surface

A scan for `System.Web.UI.WebControls.SortDirection` (including short-form references via `using`) found that the type is *declared* (property/parameter/return/generic-arg) in a small number of places. The bulk of the ~99 raw hits are value reads/writes (`x.Direction == SortDirection.Ascending`) that compile unchanged against a same-shaped replacement enum. Unrelated types were excluded: `System.ComponentModel.ListSortDirection` (used by `NoteContainer`) and the Stream Chat SDK's own `SortDirection` (used by `StreamChatProvider`).

Declaration / signature sites:

| # | Location | Kind | Disposition |
|---|---|---|---|
| 1 | `Rock/Model/Reporting/ReportField/ReportField.cs:118` | EF entity property (mapped to `[SortDirection]` column) | Conditional type, same name/column (see Design) |
| 2 | `Rock/Bulk/BulkExport/ExportOptions.cs:45` | Public property | Deprecate old + add new-enum property |
| 3 | `Rock/Web/UI/Controls/Grid/Grid.cs:4828` | Public property `SortProperty.Direction` | Moves with the `SortProperty` split |
| 4 | `Rock.Rest/Controllers/PeopleController.Partial.cs:1451` | Method parameter (legacy `Export`) | Leave; excluded on next-gen |
| 5 | `Rock.Rest/Controllers/FinancialTransactionsController.Partial.cs:243` | Method parameter (legacy `Export`) | Leave; excluded on next-gen |
| 6 | `Rock/Reporting/ReportOutputBuilder.cs:151` | `Dictionary<string, SortDirection>` local | Update via conditional `using` alias |
| 7 | `Rock/Reporting/ReportOutputBuilder.cs:564` | `Dictionary<string, SortDirection>` local | Update via conditional `using` alias |
| 8 | `Rock/Reporting/ReportingHelper.cs:380` | `Dictionary<string, SortDirection>` local | Update via conditional `using` alias |
| 9 | `RockWeb/Blocks/Reporting/ReportDetail.ascx.cs:773` | `ConvertToEnum<SortDirection>` generic arg | WebForms-only; no next-gen impact |

### `SortProperty` public API surface

`SortProperty` (currently `Rock.Web.UI.Controls.SortProperty`) is referenced as a type in ~46 places. The public API surface (the part that constrains a namespace/type change) is 11 declarations:

Public method parameters (8):
- `Rock/Utility/ExtensionMethods/LinqExtensions.cs:249` — `Where<T>(…, SortProperty)`
- `Rock/Utility/ExtensionMethods/LinqExtensions.cs:264` — `Where<T>(…, SortProperty, int?)`
- `Rock/Utility/ExtensionMethods/LinqExtensions.cs:394` — `Sort<T>(…, SortProperty)`
- `Rock/Data/Service.cs:493` — `Get(…, SortProperty)`
- `Rock/Data/Service.cs:507` — `GetNoTracking(…, SortProperty, int?)`
- `Rock/Data/Service.cs:520` — `Get(…, SortProperty, int?)`
- `Rock/Data/Service.cs:928` — `Transform(…, SortProperty = null)`
- `Rock.Blocks/Group/GroupMemberList.cs:55` — `GetGroupMemberList(…, SortProperty)` (public Obsidian block action)

Public properties (3):
- `Rock/Reporting/ReportGetQueryableArgs.cs:68`
- `Rock/Reporting/GetQueryableOptions.cs:41`
- `Rock/Model/Reporting/DataView/DataViewGetQueryArgs.cs:40`

WebForms-only (stays behind the WebForms build): `Grid.SortProperty` property (`Rock/Web/UI/Controls/Grid/Grid.cs:450`) and the `SortProperty(GridViewSortEventArgs)` ctor. Internal: `ExportOptions.SortProperty`. The remaining ~18 references are RockWeb block locals / grid member reads that define no API and keep working unchanged (see Design).

### Reflection sites reaching `Service<T>.Get`

Six sites resolve the `Get(ParameterExpression, Expression, SortProperty)` overload by reflection and would otherwise need their `typeof(SortProperty)` kept in lockstep with any type change:

- `Rock/Reporting/ReportOutputBuilder.cs:1224`
- `Rock/Reporting/DataViewQueryBuilder.cs:135`
- `Rock/Reporting/DataViewQueryBuilder.cs:211`
- `Rock/Model/Reporting/Report/Report.WebForms.cs:249`
- `Rock/Model/Reporting/DataView/DataView.WebForms.cs:112`
- `Rock/Lava/Blocks/RockEntityBlock.cs:256`

## Requirements

- The next-gen build MUST compile with no reference to `System.Web.UI.WebControls.SortDirection` from any cross-platform type.
- A cross-platform, UI-free sort descriptor (`Rock.Data.SortProperty`) MUST exist and be the forward-looking type used by cross-platform data/reporting code.
- Existing Grid/WebForms code that produces `Rock.Web.UI.Controls.SortProperty` MUST continue to compile with no call-site changes.
- Existing plugins MUST retain binary compatibility on the WebForms build for the current public API (old-typed overloads/properties kept until a major-version break).
- `ReportField.SortDirection` MUST remain queryable and map to the existing `[SortDirection]` column (no schema change, no runtime break for callers reading the property).
- The reflection-based invocation of `Service<T>.Get(..., SortProperty)` SHOULD be replaced with a direct call once `SortProperty` lives in `Rock.Data`.
- The replacement enum MUST preserve the underlying values `Ascending = 0`, `Descending = 1` so column data and `(int)` bridging stay identical.

## Design

### Part 1 — Replacement enum on the declaration sites

Define a single canonical cross-platform enum (see Open Questions for the type decision) with `Ascending = 0`, `Descending = 1`.

- **Item 1 (`ReportField.SortDirection`)** — keep the property name and column mapping; make only the *type* conditional so the property stays queryable and binary-compatible on WebForms while being System.Web-free on next-gen:

  ```csharp
  [DataMember]
  #if WEBFORMS
  public System.Web.UI.WebControls.SortDirection SortDirection { get; set; }
  #else
  public SortDirectionSpecifier SortDirection { get; set; }
  #endif
  ```

- **Items 6-8 (cross-platform locals)** — add a per-file conditional `using` alias so the existing lines compile unchanged on both builds:

  ```csharp
  #if WEBFORMS
  using SortDirection = System.Web.UI.WebControls.SortDirection;
  #else
  using SortDirection = Rock.Enums.Controls.SortDirection; // canonical safe enum
  #endif
  ```

- **Items 4-5, 9** — legacy REST `Export` endpoints and a WebForms block; left as-is (excluded from / irrelevant to the next-gen build).
- **Item 2 (`ExportOptions`)** — plain POCO; handled by the deprecation strategy below.

### Part 2 — `SortProperty` two-class split with implicit operators

`SortProperty` is extracted from `Grid.cs` and split into two separate classes (not a subclass relationship). `Rock.Data.SortProperty` always uses the new enum; the legacy type keeps the WebForms enum and the grid convenience ctor. Two `public static implicit operator` conversions bridge them, both declared on the legacy (UI) side so `Rock.Data.SortProperty` never references a UI type.

Note: C# requires conversion operators to be `public static` (CS0558 forbids `internal`). The exposure is benign (interconversion of two identically-shaped DTOs).

New — `Rock/Data/SortProperty.cs`:

```csharp
[Serializable]
public class SortProperty
{
    public SortDirection Direction { get; set; }               // new enum, unconditional
    public string DirectionString => Direction == SortDirection.Descending ? "DESC" : "ASC";
    public string Property { get; set; }
    public SortProperty() { }
    public override string ToString() => string.Format( "{0} [{1}]", this.Property, this.Direction );
}
```

Legacy — `Rock/Web/UI/Controls/SortProperty.cs` (extracted from `Grid.cs:4816-4888`), keeps the WebForms enum, the `GridViewSortEventArgs` ctor, and gains:

```csharp
public static implicit operator Rock.Data.SortProperty( SortProperty source )
{
    if ( source == null ) { return null; }
    return new Rock.Data.SortProperty
    {
        Property = source.Property,
        Direction = ( Rock.Enums.Controls.SortDirection ) ( int ) source.Direction
    };
}

public static implicit operator SortProperty( Rock.Data.SortProperty source )
{
    if ( source == null ) { return null; }
    return new SortProperty
    {
        Property = source.Property,
        Direction = ( System.Web.UI.WebControls.SortDirection ) ( int ) source.Direction
    };
}
```

Because the operators are implicit, legacy code (e.g. `service.Get( ..., gGrid.SortProperty )`) auto-converts the legacy type into the new-typed APIs with no edits. Conversions copy by value (no reference identity across a round-trip), which is correct for a value-like sort descriptor. Both operators are null-guarded because C# invokes the operator with a `null` source on assignment from a null reference. The legacy file joins the WebForms-only compilation set (the status `Grid.cs` has today), so no internal `#if WEBFORMS` is required inside it; `Rock.Data.SortProperty` is what survives on the next-gen branch.

### Part 3 — Remove the reflection via a non-generic `IService.Get`

`Reflection.GetServiceForEntityType()` (`Rock/Utility/Reflection.cs:915`) returns the non-generic `Rock.Data.IService`, which today declares only `ParameterExpression` and `GetIds(...)`. The sort-aware `Get(...)` overloads return `IQueryable<T>` and therefore live only on `Service<T>`, so an `IService` reference cannot reach them at compile time — hence the reflection. The one conceptual reason not to hoist them onto `IService` is that `SortProperty` is currently a `Rock.Web.UI.Controls` (UI) type; moving it to `Rock.Data` removes that objection.

Add a non-generic overload to `IService`:

```csharp
// IService (Rock.Data)
IQueryable Get( ParameterExpression parameterExpression, Expression whereExpression, SortProperty sortProperty );
```

Implement explicitly on `Service<T>` (delegates to the strongly-typed overload; `IQueryable<T>` is-a `IQueryable`, no cast needed):

```csharp
IQueryable IService.Get( ParameterExpression p, Expression w, SortProperty s )
    => this.Get( p, w, s );
```

Each reflection site then collapses. For example `DataViewQueryBuilder.cs:135`/`:211`:

```csharp
// before
var getMethod = service.GetType().GetMethod( "Get", new Type[] { typeof( ParameterExpression ), typeof( Expression ), typeof( SortProperty ) } );
if ( getMethod == null ) { throw ...; }
var query = getMethod.Invoke( service, new object[] { paramExpression, whereExpression, sortProperty } ) as IQueryable<IEntity>;

// after
var query = service.Get( paramExpression, whereExpression, sortProperty ) as IQueryable<IEntity>;
```

One consumer detail: `DataViewQueryBuilder.cs:218` derives the entity `Type` from `getMethod.ReturnType.GetGenericArguments()`. Replacing the reflection means sourcing that type another way (a small `Type IService.GetEntityType()` member, or from the factory, which already knows the entity type).

**This is a public-interface change and therefore a deliberate breaking change** for any external implementer of `IService`. That is the primary reason this part may be carved into its own paired spec.

### Deprecation strategy

Leave the legacy `Rock.Web.UI.Controls.SortProperty` *type* un-obsoleted for now (obsoleting it would light up the entire WebForms surface, including `Grid.SortProperty` and ~18 block consumers, with warnings and no migration path yet). Instead, deprecate the **non-grid public methods/properties** that take the legacy type and add `Rock.Data.SortProperty` counterparts:

- Obsolete old + add new overload: `Service<T>.Get` (x2), `GetNoTracking`, `Transform`, `LinqExtensions.Where` (x2), `LinqExtensions.Sort`, `GroupMemberList.GetGroupMemberList`.
- Obsolete old + add new property: `ReportGetQueryableArgs.SortProperty`, `GetQueryableOptions.SortProperty`, `DataViewGetQueryArgs.SortProperty`.
- Left alone: `Grid.SortProperty` and the `GridViewSortEventArgs` ctor (retired later with the Grid).

With the implicit operator present, retaining the old overloads is purely for plugin binary compatibility on the WebForms build; internal callers move to the new type freely, and legacy grid callers auto-convert. Obsolete the legacy *type* itself only in the later wave that retires the WebForms Grid.

## Open Questions

- **Canonical enum home/type.** New `Rock.Enums.Controls.SortDirection` (clean ownership, matches the explicit-domain-enum convention) vs. reusing `System.ComponentModel.ListSortDirection` (already cross-platform in the BCL, same `Ascending = 0 / Descending = 1` values, zero new types). Leaning toward a new Rock enum for clarity; not yet decided. Watch the name collision with the `Rock.Model` namespace (a `*Specifier` suffix may be warranted).
- **Split into paired specs.** Whether to carve Part 3 (`IService` breaking change) into its own spec from Parts 1-2 (`SortProperty`/enum). Keeping it single for now to capture everything.
- **EF model hash for item 1.** Confirm whether the next-gen branch's startup runs the EF model-compatibility check; the conditional-type approach keeps the same column and store type, so no DDL, but verify no model-hash friction.

## Considered but Rejected

### `[NotMapped]` + column-aliased rename on `ReportField` (item 1)
Rejected. Marking `SortDirection` `[NotMapped]` and adding a new mapped `Direction` property (via EF6 `[Column("SortDirection")]` aliasing) would make the old property non-queryable. That is a runtime break (LINQ against `SortDirection` would fail) even though it avoids a binary break. Keeping the property name/column and making only the type conditional avoids both.

### Subclass the legacy `SortProperty` from `Rock.Data.SortProperty`
Rejected in favor of two separate classes bridged by implicit operators. Subclassing works (Liskov lets the legacy type flow into new-typed APIs) but keeps the two types entangled and forces the base enum type onto the subclass. Separate classes with implicit conversions keep `Rock.Data.SortProperty` completely clean and still require no call-site changes in legacy code.

### Change the public method signatures to the new type and rely on implicit conversion (no retained overloads)
Rejected for the public API. Source-compatible via the implicit operator, but a binary-breaking change for pre-compiled plugins (the method token changes). Retained obsolete overloads preserve binary compatibility on the WebForms build. (The implicit conversion is still what keeps *internal/grid source* edit-free.)

### Keep the reflection in the data-view/reporting code
Rejected. The reflection exists only because the sort overloads are absent from `IService`; once `SortProperty` is a `Rock.Data` type there is no layering reason to keep it, and the `typeof(SortProperty)` coupling is exactly the fragile part that a type change would otherwise threaten.

## Out of Scope

- Retiring or converting the WebForms `Grid` control itself.
- Obsoleting the legacy `Rock.Web.UI.Controls.SortProperty` type (deferred to the Grid-retirement wave).
- The legacy REST `Export` endpoints (items 4-5) and other WebForms-only consumers.
- Broader `System.Web` decoupling beyond the sorting types.

## Related

- Scan performed on branch `feature-dh-develop-sort-property`.
- `WEBFORMS` / `REVIEW_WEBFORMS` build constants defined in `Directory.Build.props:35-36` (net472 only).
- Reflection helper: `Rock/Utility/Reflection.cs:915` (`GetServiceForEntityType` returns `Rock.Data.IService`).
