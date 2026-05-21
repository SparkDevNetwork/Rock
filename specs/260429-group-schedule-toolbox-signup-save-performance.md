---
author: Jon Edmiston
date_created: 2026-04-29
summary: >-
  The Obsidian Group Schedule Toolbox's "Sign Up for Additional Times" Save
  action is slow because the server re-runs the full availability query for
  validation, performs N+1 attendance lookups, commits the attendance record
  twice, and re-queries the saved record to send the coordinator email. This
  spec proposes targeted fixes that preserve all current behavior (Save button,
  coordinator emails, validation rules) while making the action feel instant.
contributors: []
related_docs:
  - docs/group/group-scheduling.md
---

# Group Schedule Toolbox Sign-Up Save Performance

## Summary

Volunteers using the Obsidian Group Schedule Toolbox's "Sign Up for Additional Times" feature report that clicking Save takes a noticeably long time. Reading the server-side flow confirms the issue and identifies four specific causes. This spec proposes bounded fixes for each. There is no UX trade-off: the Save button stays, the coordinator email behavior stays, the validation rules stay; only the server work shrinks.

## Motivation

A church partner reported the slowness in [Asana task 1209283639731704](https://app.asana.com/1/20866866924293/project/1208364266328691/task/1209283639731704) (created 2025-01-30 by Colleen Head, assigned to Vidhya, project: Core Development). The complaint is that Save is too slow.

The original developer who wrote the block already flagged this in code. At [Rock.Blocks/Group/Scheduling/GroupScheduleToolbox.cs:2616-2618](../Rock.Blocks/Group/Scheduling/GroupScheduleToolbox.cs):

> "But that call is quite slow, and until we can improve its performance, this fallback behavior will suffice."

The Save button itself is intentional and is documented in [docs/group/group-scheduling.md](../docs/group/group-scheduling.md) under "Sign-up commits on explicit Save". It protects the integrity of the Group Schedule Coordinator email feature added in v17 (commit `99b56405cd`, originating from Eagle Brook). This spec does not propose changing the UX. It proposes making the underlying save fast enough that the explicit Save button feels instantaneous.

## Problem Statement

Calling the `SaveSignUp` block action takes longer than necessary. On a Group with several locations, several schedules, and a 6-week sign-up window, the server work per save scales with the size of the available-sign-up grid rather than with the single occurrence the volunteer is actually committing to. The save also commits the attendance row twice and re-queries it from scratch to send the coordinator email.

## Reproduction

1. Configure a Group with `IsSchedulingEnabled` and three or more `GroupLocation`s, each with multiple `Schedule`s.
2. Set the block's `AdditionalTimeSignUpDateRange` to its default ("Next | 6 | Week").
3. Open the Group Schedule Toolbox in Obsidian as a volunteer.
4. Tick a sign-up checkbox, pick a location, click Save.
5. Capture the server time of the `SaveSignUp` block action via SQL profiler or the Rock log.

Larger location-times-schedule grids amplify the effect. Single-location, single-schedule groups are not noticeably slow.

## Root Cause

Four contributing causes, in order of impact:

### 1. Full re-validation on save

[`SaveSignUp` at Rock.Blocks/Group/Scheduling/GroupScheduleToolbox.cs:2504](../Rock.Blocks/Group/Scheduling/GroupScheduleToolbox.cs) calls [`GetSignUps()` at line 2530](../Rock.Blocks/Group/Scheduling/GroupScheduleToolbox.cs). `GetSignUps` is the same expensive method that initially loads the page. It returns every available occurrence across every location and schedule for the entire date range. The save uses one row from that result to validate the request, then discards everything else.

For a single-occurrence save, only the matching `(GroupId, ScheduleId, OccurrenceDate, LocationId)` needs to be re-validated, plus the existing-attendance collision check at [line 2576-2583](../Rock.Blocks/Group/Scheduling/GroupScheduleToolbox.cs).

### 2. N+1 queries inside `GetSignUpOccurrences`

[`GetSignUpOccurrences` at line 2185](../Rock.Blocks/Group/Scheduling/GroupScheduleToolbox.cs) iterates every (GroupLocation, Schedule) pair and inside the loop runs:

- [`AttendanceService.IsScheduled(occurrenceDate, scheduleId, personId)` at line 2339](../Rock.Blocks/Group/Scheduling/GroupScheduleToolbox.cs) — once per candidate occurrence.
- A `Count()` query for RSVP=Yes attendances at the schedule level (no location filter) [at line 2371-2373](../Rock.Blocks/Group/Scheduling/GroupScheduleToolbox.cs).
- A `Count()` query for RSVP=Yes attendances at the location level [at line 2394-2396](../Rock.Blocks/Group/Scheduling/GroupScheduleToolbox.cs).

For a Group with N locations × M schedules × K occurrence dates, this is on the order of `2*N*M*K + N*M*K` round-trips. Even modestly sized groups hit hundreds. Each Count is a sub-millisecond query in isolation, but the count of round-trips dominates total wall time.

### 3. Two `SaveChanges()` calls

[`SaveSignUp` calls `SaveChanges()` twice](../Rock.Blocks/Group/Scheduling/GroupScheduleToolbox.cs):

- Line 2695, after `ScheduledPersonAddPending` creates the attendance row.
- Line 2706, after `ScheduledPersonConfirm` flips the row to confirmed.

Both writes touch the same row in the same logical operation. They could be a single transaction.

### 4. Full re-query of the attendance after save

[Line 2710-2712](../Rock.Blocks/Group/Scheduling/GroupScheduleToolbox.cs) re-loads the attendance with `GetWithScheduledPersonResponseData()` to gather the joined data (`Group`, `Schedule`, `Location`, `PersonAlias.Person`, `Group.ScheduleCoordinatorPersonAlias.Person`) needed to render the coordinator email. The data is largely already known to the calling context — the block already loaded the Group via `GetCommonToolboxData`, the schedule and location IDs were just looked up in the dictionaries at line 2660-2672, and the person is the selected toolbox person. Only the `ScheduleCoordinatorPersonAlias.Person` and the freshly-attached `AttendanceOccurrence` reference need to be loaded.

## Affected Code Paths

Primary (where the fix lands):
- [Rock.Blocks/Group/Scheduling/GroupScheduleToolbox.cs](../Rock.Blocks/Group/Scheduling/GroupScheduleToolbox.cs)
  - `SaveSignUp` (line 2504)
  - `GetSignUps` (line 2099)
  - `GetSignUpOccurrences` (line 2185)
  - `TrySendScheduledPersonResponseEmails` (line 1261) — read-only consumer of the attendance graph

Secondary (callers / supporting services):
- [Rock/Model/Attendance/AttendanceService.cs](../Rock/Model/Attendance/AttendanceService.cs)
  - `ScheduledPersonAddPending` (whether it can be made to not auto-save)
  - `ScheduledPersonConfirm`
  - `IsScheduled`
  - `GetWithScheduledPersonResponseData`
- [Rock/Model/Attendance/AttendanceOccurrenceService.cs](../Rock/Model/Attendance/AttendanceOccurrenceService.cs)
  - `GetOrAdd`
- [Rock/Model/Group/Group/Group.cs](../Rock/Model/Group/Group/Group.cs) — read-only (`ScheduleCoordinatorPersonAlias` navigation property)

No client-side changes. The block action signature stays the same; the bag shape stays the same.

## Workarounds

User-side workarounds are limited. Administrators can reduce the impact by:

- Shortening `AdditionalTimeSignUpDateRange` (block setting) so fewer occurrences are considered. This shrinks the N+1 surface.
- Reducing the number of `GroupLocation`s or `Schedule`s on the affected Group.

Both of those are configuration tradeoffs, not real fixes. Most installations cannot meaningfully change either.

## Proposed Fix

Four changes, each independently shippable:

### Fix 1: Targeted single-occurrence validation

Replace the `GetSignUps` re-execution at [SaveSignUp line 2530](../Rock.Blocks/Group/Scheduling/GroupScheduleToolbox.cs) with a focused validation that only checks the specific occurrence the volunteer is committing to. The validation needs to confirm:

- The selected Group is still in the volunteer's schedulable groups (cheap — already loaded by `GetCommonToolboxData`).
- The selected `(Schedule, OccurrenceDate, Location)` combination is still valid for the Group, not at maximum capacity, not excluded by `PersonScheduleExclusion` or `GroupScheduleExclusion`, and not already taken by an existing attendance for the same person at a different location (the existing collision logic at line 2576-2583).
- The `SchedulingMustMeetRequirements` group requirement check (line 2195-2204) still passes.

Add a private method `ValidateSingleSignUpOccurrence(...)` that runs only those checks and returns the validated `(scheduleId, locationId, group, occurrenceDate)` tuple. The SaveSignUp method calls this in place of the full GetSignUps.

The full `SignUpsBag` returned to the client at line 2532 ("the updated list of available sign-ups") still needs to come from somewhere. Options:

- Run the full `GetSignUps` **after** the save commits, so it does not block the email and the user's perceived response time. Return the updated bag in the response as the last step.
- Return only the just-saved occurrence's removal instruction from the server and have the client remove it from the local list, deferring a full refresh until the next page interaction.

The first option is simpler and does not change the response shape; recommend that.

### Fix 2: Batch the attendance lookups in `GetSignUpOccurrences`

Even though the full `GetSignUps` will no longer run on save (Fix 1), it still runs on initial page load and on the post-save refresh. The N+1 pattern is worth fixing on its own merits.

Before the loop at [line 2284](../Rock.Blocks/Group/Scheduling/GroupScheduleToolbox.cs), pre-load:

- All `Attendance` rows for the selected person across the relevant date range, group, and schedules. One query, one round-trip. Used to replace the per-iteration `IsScheduled` call.
- All RSVP=Yes attendance counts grouped by `(ScheduleId, OccurrenceDate, LocationId)` and by `(ScheduleId, OccurrenceDate)` for the relevant Group. Two grouped queries; the per-iteration `Count()` calls become dictionary lookups.

The loop body keeps its existing logic but reads from the pre-loaded dictionaries instead of issuing new queries.

### Fix 3: Combine the two `SaveChanges()` calls

Refactor `ScheduledPersonAddPending` (or call it via a path that does not auto-save) so the pending attendance is added to the context but not committed, then `ScheduledPersonConfirm` runs against the in-memory entity, then a single `SaveChanges()` at the end commits both states.

`ScheduledPersonAddPending` currently calls `Add` and `SaveChanges` internally; the cleanest path is a new internal overload `ScheduledPersonAddPending(personId, occurrenceId, scheduledByPersonAlias, autoSave: false)` (or similar) that defers the save. The existing public signature is preserved for plugin callers. After both pending + confirm are done in memory, the block's own single `SaveChanges()` commits.

### Fix 4: Avoid the post-save re-query

The data needed by `TrySendScheduledPersonResponseEmails` is:

- `attendance.ScheduledByPersonAlias.Person` — already known (the current person).
- `attendance.PersonAlias.Person` — already known (the selected toolbox person).
- `attendance.Occurrence.Group` — already known (loaded by `GetCommonToolboxData`).
- `attendance.Occurrence.Group.ScheduleCoordinatorPersonAlias.Person` — load this once at the top of `SaveSignUp` if not already eager-loaded.
- `attendance.Occurrence.Schedule` and `attendance.Occurrence.Location` — IDs already in scope; load names with the existing `scheduleIdsByGuid` / `locationIdsByGuid` dictionaries or eagerly include them in the `GetOrAdd` call.

Replace the `GetWithScheduledPersonResponseData()` re-query at [line 2710-2712](../Rock.Blocks/Group/Scheduling/GroupScheduleToolbox.cs) with explicit attachment of the in-memory navigation properties before calling `TrySendScheduledPersonResponseEmails`.

### Combined target

After all four fixes, the save flow is:

1. Validate the single occurrence (cheap, bounded query count).
2. Get-or-add the `AttendanceOccurrence`.
3. Add the pending attendance and confirm it (one `SaveChanges`).
4. Send the coordinator email using in-memory data.
5. Asynchronously refresh the `SignUpsBag` for the response.

Target server time: under 500ms on a representative Group (5 locations × 3 schedules × 6 weeks).

## Fix Risks

- **Behavior parity in single-occurrence validation.** The full `GetSignUps` does subtle filtering (capacity at-max removal, exclusion checks, requirement evaluation, the existing-attendance collision dance with auto-fallback to a real location). The targeted validation must reproduce all of those checks faithfully or risk admitting sign-ups the full validation would have rejected. **Mitigation:** keep the existing helper methods (`IsExclusionDate`, `IsScheduled`, `GroupMembersNotMeetingRequirements`, the collision logic at line 2576-2655) and call them directly in the targeted path. Cover with unit tests that pin the rejection cases.
- **`ScheduledPersonAddPending` overload.** Adding an internal overload that defers `SaveChanges` is a behavior change in a service method many things depend on. **Mitigation:** the new behavior is gated behind a new optional parameter that defaults to the current auto-save behavior. Existing callers see no change.
- **Coordinator email regression.** If the in-memory navigation properties are not all attached correctly, the email body could render with empty fields (no group name, no location, etc.). **Mitigation:** add verification step that compares email rendered output before and after the change for representative scenarios.
- **Capacity race conditions.** Reducing the validation surface means a sign-up the user saw as available could become unavailable between page load and save. The targeted validation still catches this (the collision logic and capacity check remain), but the failure mode might surface differently. **Mitigation:** preserve the existing error messaging from the full validation.
- **Plugins that subclass the block.** None of the affected methods are publicly overridable today, but verify before changing signatures.

## Verification Steps

1. **Unit / functional tests for `ValidateSingleSignUpOccurrence`** (Fix 1): exclusion match, group-requirement failure, occurrence-not-in-window, location at max capacity, location-collision with existing attendance, all-locations-maxed scenario. Each must produce the same `SaveError` text the full path produces today.
2. **Save timing benchmark**: against a seed dataset with 5 locations × 3 schedules × 6 weeks of occurrences, measure server time for `SaveSignUp` before and after each fix. Target after all four: under 500ms p50, under 1s p99.
3. **Email parity**: trigger sign-up saves on a Group with `ScheduleCoordinator` configured for `SelfSchedule` notifications. Confirm the rendered email body (subject, group name, schedule name, location name, occurrence date, volunteer name) is byte-for-byte identical to current production.
4. **Bag-refresh smoke test**: after a save, confirm the next render of the toolbox sign-up list reflects the just-saved occurrence's removal and any capacity changes affecting other rows.
5. **Existing-attendance collision flow**: with a person already signed up at Location A, attempt sign-up at Location B for the same Group/Schedule/Date. Confirm the friendly error message at line 2619-2627 still fires unchanged.
6. **Plugin compatibility check**: grep public consumers of `AttendanceService.ScheduledPersonAddPending` and `ScheduledPersonConfirm` outside the toolbox to verify the deferred-save overload is opt-in.
7. **Load test under contention**: simulate 10 concurrent volunteers signing up for overlapping Group/Schedule/Date combinations. Confirm capacity enforcement holds and no duplicate attendances are created.

## Out of Scope

- **Conditional auto-save.** A separate work item is being considered to detect the case where no coordinator notifications would fire (no `ScheduleCoordinator` configured for any visible group, or the block's `SchedulingResponseEmail` is unset) and switch the sign-up UI to auto-save behavior in those cases. That is a UX change with its own design considerations and is intentionally not bundled here. See "Future Direction" in [docs/group/group-scheduling.md](../docs/group/group-scheduling.md).
- **Save flow for the Current Schedule actions** (Accept / Decline / Cancel). Those go through `PerformScheduleRowAction`, not `SaveSignUp`. They have their own performance profile and are not in this spec.
- **Initial page load performance of the toolbox.** Fix 2 (batched lookups) will incidentally help page load, but a comprehensive page-load optimization is not the goal here.
- **Mobile Group Schedule Toolbox** ([Blocks/Types/Mobile/Groups/GroupScheduleToolbox.cs](../Blocks/Types/Mobile/Groups/GroupScheduleToolbox.cs)). Different code path, not affected by this spec.

## Related

- Asana task: [Obsidian Group Schedule Toolbox >Sign Up for Additional Times - selections should automatically be saved and not require a person to select Save](https://app.asana.com/1/20866866924293/project/1208364266328691/task/1209283639731704)
- Originating commit for the Save button design: `99b56405cd` (Group Schedule Coordinator notifications, Eagle Brook)
- Originating Asana task for the Coordinator feature: [\[Eagle Brook\] Add Person to Notify When Additional Schedule Selected in Group Schedule Toolbox](https://app.asana.com/1/20866866924293/project/1174768427585341/task/1208174036357814)
- Current "as built" doc: [docs/group/group-scheduling.md](../docs/group/group-scheduling.md)
