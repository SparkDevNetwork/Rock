---
author: Daniel Hazelbaker
date_created: 2026-08-27
summary: >-
  A new ReportingSkill exposing Rock's Reports and Data Views to an agent:
  list and get both, run a data view to get its results (with optional
  restriction to a set of IdKeys), and get a report's definition. Running a
  report to produce rows depends on System.Web, which the agent assembly must
  not reference, so report execution goes through an internal Rock-core shim
  that wraps the already-headless ReportOutputBuilder behind a System.Web-free
  signature. All six tools, data views and reports, ship with this skill.
contributors: []
---

# Reporting Agent Skill

## Summary

This spec defines `ReportingSkill`, an agent skill over Rock's `Report` and `DataView` entities. It is its own skill rather than part of Core Administration because reporting is a coherent user-facing feature area with two related entities and an execution path of its own, where defined types and categories are pure cross-cutting reference data.

Proposed tools:

- `ListReports`, `GetReport` — the report and its definition.
- `ListDataViews`, `GetDataView` — the data view and its definition.
- `GetDataViewItems` — run a data view and return its results, optionally restricted to a set of IdKeys.
- `GetReportItems` — run a report and return its rows, with sorting. Built on an `internal` Rock-core shim that wraps `ReportOutputBuilder` so `System.Web` stays out of the agent assembly.

Scale, from the numbers on the request: one partner instance has ~371 reports and ~3,300 data views, so both list tools must page and neither may cap.

## Motivation

An agent that can read a person or a group but not a data view is blind to exactly the saved questions a church has already decided are worth asking. Data views are where the useful segments live ("active adults", "gave last year, not this year"), and there are an order of magnitude more of them than reports. Being able to enumerate them, read what one does, and run it against a provided set of people is high leverage.

Running things is also where reporting stops being like the other skills. A list or a get is a read of metadata; running a report or a data view executes a query the church authored, which raises questions of cost, security, and, as it turns out, assembly dependencies that the metadata tools never touch.

## The System.Web constraint, and how the shim resolves it

Report execution touches `System.Web`, but not as deeply as it first appears. Reading the code separates two things: what leaks `System.Web` into a caller's *compile-time surface*, and what merely uses it *internally at runtime*.

**Leaks to the caller (unusable from the agent):**

- `ReportingHelper.BindGrid` takes a live `System.Web.UI.WebControls.Grid`. Web-only.
- `Report.GetQueryable( ReportGetQueryableArgs )` requires constructing `ReportGetQueryableArgs`, whose `SortProperty.Direction` is `System.Web.UI.WebControls.SortDirection`. Building the args from the agent would force a `System.Web` reference.

**Internal only (safe to call):**

- `ReportOutputBuilder.GetReportData( Person, Expression, ParameterExpression, RockContext, ReportOutputBuilderFieldContentSpecifier, int? pageIndex, int? pageSize )` returns a `TabularReportOutputResult` (a `DataTable` plus a column map). **Its public signature contains no `System.Web` type.** Its `System.Web.UI.WebControls` usage (`BoundField.FormatDataValue` for cell formatting, `BoundField`/`DataControlField` for column configuration) is entirely internal, and none of it requires an `HttpContext` (the single `HttpContext.Current` reference is a null-safe logging call in a catch block).

That last point is not a guess. `Rock.Tests.Integration/Reporting/ReportBuilder/ReportBuilderTests.cs` already calls this exact overload with `FormattedText`, from the integration-test host with no ASP.NET request, and asserts on both the formatted values and per-field VIEW masking. Report output already runs headless; it is proven by passing tests.

**The shim.** Because `GetReportData`'s runtime path needs `System.Web.dll` loaded and constructing a `SortProperty` needs the `System.Web` sort enum, those two things must live in an assembly that already references `System.Web`, which is `Rock.dll`. So report execution for the agent is delivered by an **`internal` shim in Rock core** that exposes a `System.Web`-free method, maps a `System.Web`-free sort specification onto the report, and calls `ReportOutputBuilder`. The agent calls the shim; `System.Web` never enters `Rock.AI.Agent`. Rock core already declares `[assembly: InternalsVisibleTo( "Rock.AI.Agent" )]`, so the shim can stay `internal`, which is deliberate: it is semi-temporary and carries no API-stability promise, so it can be deleted or reshaped when a fuller `System.Web`-free report runner is built. See the shim design under tool 6.

Data views need no shim. `DataViewCache.GetQuery(...)` and `GetEntityIds(...)` have no `System.Web` involvement and run fully headless.

## Requirements

- The skill MUST NOT introduce a `System.Web` reference into `Rock.AI.Agent`.
- Report execution MUST go through an `internal` Rock core shim; the agent MUST NOT call `Report.GetQueryable`, `ReportingHelper`, or construct any `System.Web` type directly.
- The shim MUST be `internal` (no API-stability promise) and MUST NOT mutate the persisted report when applying a caller-supplied sort.
- Every parameter MUST be an IdKey or a simple value. No class names, no raw Ids in parameters. Sorting MUST be expressed in a `System.Web`-free form.
- Both `List` tools MUST page with a cursor (secured entities, database query) and MUST NOT cap.
- `Report` and `DataView` are `ISecured`; every tool MUST enforce `IsAuthorized( VIEW )` on the report or data view, whose `ParentAuthority` is its `Category`.
- `GetDataViewItems` MUST support restricting results to a caller-supplied set of IdKeys.
- Running a data view or a report MUST NOT be capped; results are paged.

## Design

### Skill declaration

```csharp
[Description( "Provides access to Rock's reports and data views: listing them, reading their configuration, and running data views to get results." )]
[AgentSkillGuid( "39BB9DB1-569A-44C1-9F1D-61E8B16D8846" )]
[EntityTypeGuid( "F8E8E905-6893-442F-8331-6DFC352C86C1" )]
internal sealed partial class ReportingSkill : AgentSkillComponent
```

### Tool inventory

| # | Tool | Kind | Paging | Status | Guid |
|---|---|---|---|---|---|
| 1 | `ListReports` | List | cursor | ready | `6729CFE6-C23F-4DD2-BBF9-DBBD5F5C190A` |
| 2 | `GetReport` | Get | none | ready | `A811A466-90B5-4ACF-B887-9DDDF8CB1145` |
| 3 | `ListDataViews` | List | cursor | ready | `B7EA3A42-8C2A-42AA-B244-FA14F6551DD6` |
| 4 | `GetDataView` | Get | none | ready | `61E0F59C-9885-4224-8D0D-7E24BD71E3D2` |
| 5 | `GetDataViewItems` | Get (runs) | page number | ready | `1D97B2CD-7B22-4673-9112-99BF74050D0B` |
| 6 | `GetReportItems` | Get (runs) | page number | ready (via internal shim) | `0481F45E-98DA-403E-8709-A132960F9107` |

### 1. ListReports

```csharp
public AgentToolResult ListReports( string entityTypeIdKey, string partialName = null, string categoryIdKey = null, string cursor = null )
```

`entityTypeIdKey` is required, as specified on the request. It is the strongest available narrowing on a set that runs into the hundreds, and an agent listing reports nearly always knows the subject entity it cares about. The tradeoff, that reports with a null `EntityTypeId` cannot be reached this way, is called out in Open Questions.

Source is `ReportService.Queryable()` filtered by `EntityTypeId`, optional `Name` contains, and optional `Category`. `Report` is `Model<T>` and therefore `ISecured`, and this is a database query, so it MUST use `CursorPaginator` with the person constructor, ordered by `Name` then `Id`.

**Output** per row: `IdKey, Name, Category { IdKey, Name }`. Description, entity type, data view, fetch top, and fields belong to `GetReport`.

### 2. GetReport

```csharp
public AgentToolResult GetReport( string reportIdKey )
```

**Output.** `IdKey, Guid, Name, Description, Category { IdKey, Name }, EntityType { IdKey, Name }, DataView { IdKey, Name }, FetchTop, Fields[]`. Each field: `ReportFieldType` (Property, Attribute, or DataSelectComponent), `Selection`, `ColumnHeaderText`, `ColumnOrder`, `SortOrder`, `IsSortable`, `ShowInGrid`. The fields describe the report's columns so a caller knows what `GetReportItems` will return and which columns it can sort on. `SortOrder` is included but not the sort *direction*, because `ReportField.SortDirection` is a `System.Web` type; see Result classes.

`IsSortable` tells the caller whether a field may appear in `GetReportItems`'s `sortBy`:

- Property and Attribute fields are always sortable (they resolve to a column or a correlated subquery, both of which SQL can order by).
- A DataSelectComponent field is sortable unless its component disables it. `DataSelectComponent.SortProperties( selection )` returns `null` to sort on the field itself, a property list to sort on those, or `string.Empty` to disable sorting. So `IsSortable` for such a field is `SortProperties( field.Selection ) != string.Empty`.

### 3. ListDataViews

```csharp
public AgentToolResult ListDataViews( string entityTypeIdKey, string partialName = null, string categoryIdKey = null, string cursor = null )
```

`DataView.EntityTypeId` is required on the model, so requiring it as a filter is natural here. Source `DataViewService.GetByEntityTypeId( entityTypeId )` (or `Queryable()` with the same filter), plus optional name and category. Cursor paging, same reasoning as reports; the ~3,300 count makes it mandatory.

**Output** per row: `IdKey, Name, Category { IdKey, Name }`.

### 4. GetDataView

```csharp
public AgentToolResult GetDataView( string dataViewIdKey )
```

**Output.** `IdKey, Guid, Name, Description, Category { IdKey, Name }, EntityType { IdKey, Name }, IsPersisted, PersistedScheduleIntervalMinutes, TransformEntityType { IdKey, Name }, IncludeDeceased, FilterDescription`. The raw filter tree is **not** returned; it is large and nested. In its place, `FilterDescription` carries the human-readable summary from `DataViewFilter.ToString( entityType )` (the same `DataFilterContainer` component formatting Rock's own screens use, which lives in `DataViewFilter.Logic.cs` and is `System.Web`-free), so a caller can see what the view does without the raw tree.

### 5. GetDataViewItems

```csharp
public AgentToolResult GetDataViewItems( string dataViewIdKey, List<string> entityIdKeys = null, int pageNumber = 1 )
```

Runs the data view headlessly and returns its results as identity rows.

**Execution.** Get the data view via `DataViewCache.Get( dataViewIdKey )`, enforce `IsAuthorized( VIEW )`, then run it with `GetQuery( new GetQueryableOptions { DatabaseTimeoutSeconds = 180 } )`, which returns a composable `IQueryable<IEntity>`.

**Restricting to a set of IdKeys.** There is no built-in "only these Ids" argument on `DataViewGetQueryArgs` or `DataViewFilterOverrides`; the latter only overrides existing filter *selections*. The supported mechanism is to intersect the returned queryable with the decoded Ids server-side, which is exactly what Rock does internally for persisted views:

```csharp
var ids = entityIdKeys.Select( k => IdHasher.Instance.GetId( k ) ).Where( id => id.HasValue ).Select( id => id.Value ).ToList();
var query = dataViewCache.GetQuery( options );
if ( entityIdKeys != null ) { var idSet = new HashSet<int>( ids ); query = query.Where( e => idSet.Contains( e.Id ) ); }
```

The `.Where( e => idSet.Contains( e.Id ) )` translates to SQL and ANDs with the data view's own filters. This is the answer to the "filter by array of IdKeys" question the request flagged as unresolved. Respect `DisablePredictableIds` when decoding.

**Output** per row: `IdKey, Name`, where `Name` is the entity's `ToString()`. Results are identity only; the point of the tool is "who is in this segment", and the fields of each entity are read through that entity's own skill.

**Paging.** Page number over the result. `GetEntityIds()` returns the full Id list cheaply; intersect with the filter, page the Id list, then load and name only the page's entities.

**Why not a cursor.** The results are entities of a type known only at runtime (`IEntity`), so `CursorPaginator<T>`, which is generic over a compile-time entity type, cannot be applied. The data view as a whole is `VIEW`-gated, matching Rock's own `DataViewResults` block, which applies no per-row security filter to results. Per-row result security is deliberately not applied, matching that core behavior.

**Never cap.** A data view can return a large set; it pages, it does not truncate.

### 6. GetReportItems

```csharp
public AgentToolResult GetReportItems( string reportIdKey, List<ReportSortSpecifier> sortBy = null, int pageNumber = 1 )
```

Runs a report and returns its rows, paged, honoring per-field VIEW masking. It is built on the internal shim described below rather than on the WebForms engine directly.

**Execution.** Resolve the report with `helper.GetRequiredEntity<Report>( reportIdKey )` (which enforces `IsAuthorized( VIEW )`), then call the shim. The shim returns a `TabularReportOutputResult` (a `DataTable` plus a `Guid -> column` map); the tool projects each `DataRow` into a result row keyed by column name, and reads the row's `Id` column (always present) as the entity IdKey.

**Sorting.** `sortBy` is an ordered, `System.Web`-free list; each entry names a report field (by its column key or report-field guid) and a direction, and the order of the list is the sort precedence. Each entry MUST name a field that `GetReport` reports as `IsSortable`; a non-sortable or unknown field is rejected with an error naming it and chaining to `GetReport`, rather than being silently dropped. The shim applies the list (see below). When `sortBy` is omitted, the report's own saved sort is used.

**Output** per row: `IdKey` plus the report's columns as name/value pairs, using the `ColumnHeaderText` from `GetReport`. Values are the formatted strings `ReportOutputBuilder` produces; a column the caller may not view is masked, exactly as in the core screens.

**Paging.** Page number. The shim passes `pageIndex`/`pageSize` straight through to `GetReportData`, which pages in SQL.

**Never cap.** A report can be large; it pages, it does not truncate.

#### The report-data shim (internal, Rock core)

Provisional name `Rock.Reporting.AgentReportRunner`, in `Rock.dll`, `internal` and reachable from the agent through the existing `InternalsVisibleTo`. Its whole purpose is to keep every `System.Web` type inside `Rock.dll` while giving the agent a clean seam.

```csharp
// Rock.dll — internal, System.Web-free signature
internal static class AgentReportRunner
{
    internal static AgentReportResult GetReportData(
        Report report,
        Person currentPerson,
        RockContext rockContext,
        IReadOnlyList<(Guid FieldGuid, bool IsDescending)> sortBy,
        int? pageIndex,
        int? pageSize );

    internal static bool IsReportFieldSortable( ReportField field );
}

// System.Web-free result so the agent never names ReportOutputBuilder
internal sealed class AgentReportResult
{
    internal DataTable Data { get; set; }
    internal Dictionary<Guid, string> ReportFieldToDataColumnMap { get; set; }
    internal int? ReportRowCount { get; set; }
}
```

The shim does **not** modify the report. Instead, `ReportOutputBuilder` gains an `internal SortOverride` property; the shim sets it and lets the builder apply the sort:

1. Sets `ReportOutputBuilder.SortOverride` to the requested `(fieldGuid, isDescending)` list. `GetReportData` uses that in place of the report's saved field sort, mapping `IsDescending` to `System.Web.UI.WebControls.SortDirection` inside `Rock.dll`, so the agent never touches the `System.Web` enum. When the override is null, the report's own saved sort is used.
2. Calls `GetReportData( currentPerson, null, null, rockContext, FormattedText, pageIndex, pageSize )` and maps the result to the `System.Web`-free `AgentReportResult`.

This mirrors how the grid path already works, where an ad-hoc sort arrives as `gReport.SortProperty` and the report's fields are read only for the default sort. **The report entity is never written to**, so no accidental `SaveChanges` can persist a transient sort, and there is no need to load a throwaway copy. The agent's `ReportSortSpecifier` (its `FieldIdKey` + `IsDescending`) is resolved to field guids by `GetReportItems` and passed to the shim as the tuple list.

**Why internal and semi-temporary.** The shim still runs `ReportOutputBuilder`, which formats through `BoundField`. That is proven to work headless (see the constraint section) but is WebForms-era code. A later, larger effort can give reporting a fully `System.Web`-free execution and formatting path; when it does, the shim's body is rewritten or deleted while its `System.Web`-free signature stays put, so `GetReportItems` never changes. Keeping it `internal` is what makes that freedom real: nothing outside Rock can bind to it.

### Result classes

Under `Agent/Rock.AI.Agent/Classes/Skills/ReportingSkill/`: `ReportResult`, `ReportDetailResult`, `ReportFieldResult`, `DataViewResult`, `DataViewDetailResult`, `DataViewItemResult` (IdKey, Name), and `ReportItemResult` (IdKey plus the report's columns as name/value pairs). No new attribute shapes.

`ReportFieldResult` carries `SortOrder` (int?) and `IsSortable` (bool) but **not** the field's sort *direction*: `ReportField.SortDirection` is a `System.Web.UI.WebControls` type, and reading it would pull `System.Web` into the agent assembly. The shim reads it internally; the agent does not.

The shim and its `ReportSortSpecifier` live in `Rock.dll` (`Rock.Reporting`), not in the agent project, because only the core assembly may reference `System.Web`.

### Cross-cutting conventions applied

- **Security everywhere.** Every tool enforces `IsAuthorized( VIEW )` on the report or data view; the parent authority is the category, so category permissions inherit.
- **Cursor for secured database lists**, page number where the runtime entity type defeats the cursor, each documented.
- **Chain forward.** A bad entity type chains to `ListEntityTypes`, a bad category to `ListCategories` with the Report or DataView entity type.
- **Categories are entity-scoped.** Report categories carry the Report entity type; data view categories carry the DataView entity type. `ListCategories` must be called with the matching entity type.

## Out of Scope

- **Creating or editing reports and data views.** No `AddOrUpdate`. Authoring a data view means authoring a filter tree, which is a large surface of its own; this skill reads and runs, it does not write.
- **The raw data view filter tree.** `GetDataView` returns a text summary of the filters, not the raw nested tree.
- **A fully `System.Web`-free report engine.** The shim still runs `ReportOutputBuilder`, which formats through `BoundField`. Replacing that with a `System.Web`-free execution and formatting path is a larger core effort left for later; the shim's signature is designed so that effort does not change the agent.
- **Per-row security filtering of data view results.** Not applied, matching core's `DataViewResults`, which filters no rows.

## Considered but Rejected

### Make `entityTypeIdKey` optional on the list tools

Rejected for v1, per the request. On sets of hundreds to thousands, the entity type is the one filter that reliably narrows to a workable page, and the agent almost always knows the subject entity. The cost, unreachable null-entity-type reports, is small and noted.

### Return full entity rows from `GetDataViewItems`

Rejected. A data view's entity type varies, its rows can be wide, and each entity already has a skill that returns its fields with the right shape and security. Identity plus `ToString()` is what "who is in this segment" needs; the caller drills in through the entity's own skill.

### Build `GetReportItems` on `ReportOutputBuilder` now

Rejected. It is the fastest path to rows and the wrong one: it imports `System.Web` into an assembly that must stay clear of it. Option B reaches the same rows for the common case without that cost; option C fixes it properly in core.

## Resolved Decisions

The open questions this spec was drafted with have been settled:

1. **Report execution: an internal core shim (a scoped take on Option C).** `GetReportItems` ships in this skill, built on an `internal` shim in `Rock.dll` that wraps `ReportOutputBuilder.GetReportData` behind a `System.Web`-free signature (see tool 6). The full Option C, a reporting engine with no `System.Web` at all, is not required to unblock the tool: `GetReportData` already runs headless (proven by existing integration tests), so the shim only has to keep the `System.Web` *types* on the Rock side of the call. The shim is `internal` and semi-temporary; a later full extraction can replace its body without touching the agent. Options A (defer) and B (data-view fallback) are not pursued.
2. **`GetDataView` returns a filter description.** It returns the human-readable summary produced by `DataViewFilter.ToString( entityType )`, the same `DataFilterContainer` component formatting Rock's own screens use, rather than the raw nested tree.
3. **No per-row security on data view results.** The data view is `VIEW`-gated as a whole and results are returned unfiltered, matching Rock's `DataViewResults` block, which applies no per-row filter.
4. **`entityTypeIdKey` is required on the list tools.** A report or data view without an entity type is not valid (the model leaves `Report.EntityTypeId` nullable, but the UI enforces it), so requiring it hides nothing legitimate.

## Related

- [260807-ai-agent-tool-conventions.md](260807-ai-agent-tool-conventions.md) — the shared conventions this spec assumes.
- `Rock/Reporting/ReportOutputBuilder.cs` — the headless report output path the shim wraps.
- `Rock.Tests.Integration/Reporting/ReportBuilder/ReportBuilderTests.cs` — existing tests proving `GetReportData` runs headless, including formatting and VIEW masking.
- `Rock/Web/Cache/Entities/DataViewCache.cs` — the headless data view execution path this skill builds on.
- `Rock/Reporting/ReportOutputBuilder.cs` — the report execution engine, and the source of the `System.Web` constraint.
