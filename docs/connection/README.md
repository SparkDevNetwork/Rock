# Connection Documentation

Connection is Rock's process-management system for tracking the lifecycle of an "intent to connect": volunteer signups, baptism interest, membership steps. `ConnectionType` -> `ConnectionOpportunity` -> `ConnectionRequest` is the three-tier model.

If you are new, start with [connection-overview.md](connection-overview.md). Sub-topics worth their own docs (Connection Request Board, Status Automation, Workflow Integration, Health Snapshots, Mobile Connection blocks) will be added as separate files.

## Files in this directory

| Doc | Summary |
|---|---|
| [Connection Domain Overview](connection-overview.md) | Three-tier type/opportunity/request model, status lifecycle, automation vs workflow split, and the Connections Hub. |
| [Connection Request Board](request-board.md) | Kanban-style operator surface, drag-and-drop transitions, campus / connector filters, performance under concurrent creation. |
| [Connection Request Entry](request-entry.md) | Public-facing multi-select intake form: configurable fields, match-or-create Person, and one connection request per selected opportunity. |
| [Connection Status Automation](status-automation.md) | Job-driven rule evaluation, time-based and DataView-based conditions, status history tracking. |
| [Connection Workflows and Triggers](workflows-and-triggers.md) | Type / Opportunity / Status workflow placement, trigger event types, conditional application via Age Classification and DataView. |
