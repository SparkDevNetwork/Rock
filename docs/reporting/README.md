# Reporting Documentation

Reporting is Rock's data-querying surface for non-developers: DataViews (population queries), Reports (column projection), Metrics (time-series), Dynamic Data blocks (admin SQL), and Analytics Source tables (denormalized snapshots). DataViews are the foundation; almost every other domain integrates with them.

If you are new, start with [reporting-overview.md](reporting-overview.md). Sub-topics worth their own docs (DataView Filter Components, Custom Filters, Metric Sources, Analytics Source Tables, Dynamic Data Block) will be added as separate files.

## Files in this directory

| Doc | Summary |
|---|---|
| [Analytics Source Tables](analytics-source-tables.md) | Denormalized snapshots for analytics, population jobs, SCD-2 historical entities, AnalyticsSourcePostalCode and Census data. |
| [Custom Metrics](custom-metrics.md) | Time-series metric definitions, partition-driven multi-dimensional values, source types (SQL/DataView/Lava/Manual). |
| [DataView Filter Components](dataview-filter-components.md) | Pluggable filter components, LINQ expression generation, configuration UI per filter, custom filter authoring. |
| [Dynamic Data Block](dynamic-data-block.md) | Admin-authored SQL + Lava reports, page-parameter filtering, workflow / communication launches on selected rows. |
| [Reporting Domain Overview](reporting-overview.md) | DataView/Report/Metric layering, the cross-domain DataView consumer story, and the analytics-source denormalization pattern. |
