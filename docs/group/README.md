# Group Documentation

The Groups domain is Rock's universal "people in a relationship" container. Families, small groups, volunteer teams, security roles, communication lists, and check-in classrooms are all Groups. Configuration is templated through `GroupType`; runtime state lives on `Group` and `GroupMember`. Soft-delete via `IsArchived` and a global query filter is the default "remove" path. Save hooks own derived state.

These docs lead with the **mental model** and the **practical things you need to know**, then drop into a **technical reference** at the bottom for active contributors. If you only have ten minutes, read the Overview, Mental Model, and What You Need to Know sections of whatever doc covers your area; the dense reference content is for when you are actually changing code.

Code entry points: [Rock/Model/Group/](../../Rock/Model/Group/) for entities, [Rock.Blocks/Group/](../../Rock.Blocks/Group/) for C# blocks, [Rock.JavaScript.Obsidian.Blocks/src/Group/](../../Rock.JavaScript.Obsidian.Blocks/src/Group/) for Obsidian Vue blocks, [Rock/Web/Cache/Entities/Group*.cs](../../Rock/Web/Cache/Entities/) for cache classes. Background work runs through [Rock/Jobs/CalculateGroupRequirements.cs](../../Rock/Jobs/CalculateGroupRequirements.cs), [Rock/Jobs/GroupSync.cs](../../Rock/Jobs/GroupSync.cs), [Rock/Jobs/SendSignUpReminders.cs](../../Rock/Jobs/SendSignUpReminders.cs), and [Rock/Jobs/ProcessGroupHistory.cs](../../Rock/Jobs/ProcessGroupHistory.cs).

If you are new to the domain, start with [group-overview.md](group-overview.md). It lays out the entity map, the soft-delete vs archive distinction, and the save-hook pipeline.

## Files in this directory

| Doc | Summary |
|---|---|
| [Groups Domain Overview](group-overview.md) | Top-level mental model: entity map, soft-delete and archive semantics, save-hook pipeline. Read this first. |
| [Group Types](group-types.md) | `GroupType` configuration, the many-to-many child-type hierarchy, check-in area templating, what does and doesn't inherit through `InheritedGroupTypeId`. |
| [Group Members and Roles](group-members-and-roles.md) | `GroupMember` lifecycle, `GroupTypeRole` permissions, validation, archive vs delete vs inactive. |
| [Group Requirements](group-requirements.md) | Three-tier requirement system, the Meets/Warning/NotMet state machine, the calculation job. |
| [Group Locations](group-locations.md) | `GroupLocation`, capacity configs, location selection modes, picker-input resolution patterns. |
| [Group Sync](group-sync.md) | DataView-driven membership. The "DataView is law" rule and unarchive-on-rejoin. |
| [Group Scheduling](group-scheduling.md) | Volunteer scheduling: assignments, templates, exclusions, capacity, the Scheduler and Toolbox blocks. Plus a Group's inline Schedule lifecycle. |
| [Group Attendance](group-attendance.md) | `AttendanceOccurrence` vs `Attendance`, `DidNotOccur` vs `DidAttend`. |
| [Group Chat](group-chat.md) | Tri-state per-Group chat overrides, `GetIsChatEnabled()` resolution, channel avatar lifecycle, the `ChatHelper` integration boundary. |
| [Peer Network](group-peer-network.md) | Inferred relationship-strength scoring from shared memberships. Six per-Group override columns, the four-way multiplier matrix, the `PeerNetwork` entity. |
| [Group Historical Entities](group-history.md) | SCD-2 historical entities, the `ProcessGroupHistory` job, opt-in per GroupType. |
| [Group Caching](group-caching.md) | Four cache classes, the cache-mirrors-model security rule, invalidation, lifetime. |
