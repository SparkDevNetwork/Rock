# Check-in Documentation

Check-in is the kiosk and mobile self-service flow for recording attendance at services, classrooms, and events. Two engines exist: the legacy `Rock.CheckIn` engine and the next-generation `Rock.CheckIn.v2` (the default for new deployments).

If you are new, start with [check-in-overview.md](check-in-overview.md). Sub-topics worth their own docs (Opportunity Filters, Label Designer, Cloud-Print, v2 vs Legacy, Family Edit, KioskDevice configuration) will be added as separate files.

## Files in this directory

| Doc | Summary |
|---|---|
| [Check-in Domain Overview](check-in-overview.md) | Session phases, the v2 provider/filter pattern, and the integration with the shared Attendance entity. |
| [Kiosk Configuration](kiosk-configuration.md) | `KioskDevice` / `CheckinType` / `LocalDeviceConfiguration` three-layer model, default Person settings, address display. |
| [Label Designer and Printing](label-designer-and-printing.md) | Visual label designer, field-data-source pattern, formatters, cloud-print serialization for shared printers. |
| [Mobile Check-in](mobile-check-in.md) | Same v2 engine on mobile, cloud-print label routing, `SourceTypeValueId` for entry-channel distinction. |
| [Next-Gen Check-in (v2) vs Legacy](v2-vs-legacy.md) | Two engines comparison, why v2 exists, migration considerations. |
| [Opportunity Filters](opportunity-filters.md) | The filter chain, built-in filters (age/grade/gender/etc.), authoring custom filters, ordering decisions. |
