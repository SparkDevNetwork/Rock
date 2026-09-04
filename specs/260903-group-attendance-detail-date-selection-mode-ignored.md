---
author: Jason Hendee
date_created: 2026-09-03
summary: >-
  The Group Attendance Detail block ignored its "Date Selection Mode" setting and
  locked the date to today whenever ScheduleId or LocationId arrived as a page
  parameter. Records the root cause, the fix, and the test plan.
contributors: []
---

# Group Attendance Detail Ignores "Date Selection Mode" When Schedule or Location Is Passed

## Summary

Passing `ScheduleId` or `LocationId` to the Group Attendance Detail block silently bypassed its "Date Selection Mode" setting and rendered the date field read-only, pinned to today. The fix separates the rule that locks the date from the rule that locks the location and schedule, so each page parameter pins only its own field as the legacy WebForms block did, with two deliberate divergences, noted below, where the block locks a field that legacy left editable.

## Problem Statement

Linking into Group Attendance Detail from a filtered Group Attendance List produces a URL carrying `ScheduleId` and `LocationId` but no `Date`. In that state the date field renders read-only and pinned to today, regardless of whether "Date Selection Mode" is set to Date Picker or Pick From Schedule. Removing the filter, so the link carries no schedule or location, restores the editable date picker.

Reported against Rock 19.4.

## Root Cause

`GetAttendanceOccurrenceSearchParameters` treated a location or schedule parameter as evidence that the individual was viewing one particular occurrence:

```csharp
occurrenceDataSearchParameters.IsSpecificSearch =
    ( _block.DatePageParameter ?? _block.OccurrencePageParameter ).HasValue
    || _block.LocationIdPageParameter.HasValue
    || _block.ScheduleIdPageParameter.HasValue
    || attendanceOccurrenceGuid.HasValue
    || _block.OccurrenceIdPageParameter.HasValue;
```

That flag became `IsSpecificOccurrence`, which forced the date, location, and schedule all to read-only in a single branch of `GetInitializationBox`, bypassing the `DateSelectionMode` switch entirely. Separately, with no `Date` parameter to search on, the occurrence lookup fell back to `RockDateTime.Today`, which is why the locked value was always today.

The single flag is the underlying defect. Five different parameters, each meaning something different, all collapsed into one "everything is read-only" decision.

The legacy block (`RockWeb/Blocks/Groups/GroupAttendanceDetail.ascx.cs`, removed in `278e1fb088`) decided each field on its own: the date went read-only on `OccurrenceId` alone, the location on `LocationId`, and the schedule on `ScheduleId`. A `Date` parameter pinned nothing.

## Fix

Four changes, all server-side.

1. **One lock per field.** `IsSpecificSearch` and `IsSpecificOccurrence` are replaced by `IsDatePinned`, `IsLocationPinned`, and `IsSchedulePinned` on `AttendanceOccurrenceSearchParameters`; `OccurrenceData` carries a reference to the search parameters it was resolved from rather than copying the flags. Each page parameter pins only its own field: `Date` (or its legacy alias `Occurrence`) pins the date, `LocationId` the location, `ScheduleId` the schedule. A request for a particular occurrence by guid or `OccurrenceId` pins all three, since its date, location, and schedule identify it. The flags describe the request, not whether the occurrence was found, which matches legacy.
2. **Split the branch.** `GetInitializationBox` now decides the date, the location, and the schedule independently, in three gates instead of one. The `DateSelectionMode` switch itself is unchanged.
3. **Keep a pinned schedule pinned in the scheduled date picker.** Applies only when a `ScheduleId` page parameter is present.

   - **The setup.** The schedule renders read-only. Depending on Date Selection Mode, though, the *date* control may be `GroupLocationScheduleDatePicker`, a dropdown of date-and-schedule pairs:
     - `PickFromSchedule`: when the pinned schedule is scheduled to meet at the location within "Number of Previous Days To Show". The dropdown lists those meeting dates. If it is not scheduled to meet in that window there is no list to filter, and item 4 below governs what shows instead.
     - `CurrentDate`: never, once the schedule is pinned, regardless of which location is selected. In this mode the date is today, read-only, no matter what; the dropdown only ever chose which of today's schedules to record against. With the schedule pinned there is one choice (Rock records one occurrence per schedule per day; `OccurrenceDate` is a `date`), so the picker is skipped and the schedule renders as a read-only label, which is what this scenario showed before the fix.
     - `DatePicker`: never. A plain date picker is used, so this change does not apply.
   - **The problem.** Choosing an entry sets both the date and the schedule from one `"date|scheduleGuid"` value, and the control renders in the schedule's slot. The individual could pick a date and silently swap out the schedule that was supposed to be pinned.
   - **The fix.** `GetGroupLocationScheduleDateBags` filters its list to the pinned schedule. It reads the page parameter itself, so the two places that build the list (the initial render and the picker's own fetch) apply the same filter, and the client is never trusted to send it.
   - **The result.** In `PickFromSchedule`, every entry in the dropdown carries the pinned schedule, so choosing a date can no longer change it. In `CurrentDate`, the dropdown is not shown at all when the schedule is pinned.
4. **Show the date when Pick From Schedule has nothing to offer.** In `PickFromSchedule` mode, a selected location whose schedule has no meetings within "Number of Previous Days To Show" used to hide the date entirely (`None`) while attendance still recorded against today, invisibly. It now shows today's date read-only. It deliberately does not fall back to a free date picker: the admin chose this mode to keep dates on the schedule, and the lever for a group that meets less often is "Number of Previous Days To Show". `None` still applies while no location has been selected yet.

Resulting behavior:

| Page parameters | Date | Location | Schedule |
|---|---|---|---|
| none | per Date Selection Mode | picker | picker |
| `Date` or `Occurrence` | read-only | picker | picker |
| `LocationId` | per mode | read-only | picker |
| `ScheduleId` | per mode | picker | read-only |
| `LocationId` + `ScheduleId` | per mode | read-only | read-only |
| `Date` + `LocationId` | read-only | read-only | picker |
| `OccurrenceId` or occurrence guid | read-only | read-only | read-only |

Only the first and last rows match the pre-fix behavior. Every other row previously rendered all three fields read-only (and, absent a `Date`, the date pinned to today), because any one of these parameters set the single `IsSpecificSearch` flag.

"Picker" in the Schedule column means the schedule picker is available, not that it is always visible. It appears only once a location is selected, and it lists only that location's schedules that meet on the selected date. Both gates are pre-existing.

Two deliberate divergences from legacy. In both, the block locks a field that legacy left editable:

- **`Date` (or `Occurrence`) pins the date.** Legacy pre-filled the picker and left it editable. Every real link that passes a date names a specific meeting: the attendance reminder communication passes `GroupId` + `Occurrence`, and clicking a scheduled-but-not-yet-created row in Group Attendance List passes `Date` + `LocationId` + `ScheduleId`. Holding the date is the intent of those links, and it is how the Obsidian block has treated the date since conversion. A leader whose group met on a different day marks the scheduled occurrence "Did Not Meet" and uses the list's Add button, which offers an editable date.
- **`ScheduleId` alone pins the schedule.** Legacy pinned it only when `LocationId` was also present, an artifact of where the lookup sat in the code rather than a design choice.

## Affected Code Paths

- `Rock.Blocks/Group/GroupAttendanceDetail.cs`: `GetAttendanceOccurrenceSearchParameters`, `GetOccurrenceData`, `GetInitializationBox`, `GetGroupLocationScheduleDateBags`.

Nothing outside this file changed: no ViewModel, generated type, or `.obs` edits.

## Test Plan

A parent group is only complete when every case under it passes.

### 1. Reported issue

- [x] 1.1 Date Picker mode, link carrying `ScheduleId` and `LocationId` with no `Date`, no occurrence yet at that combination: date renders as an editable date picker defaulted to today; location and schedule render read-only with the passed values.
- [x] 1.2 Same link, but an occurrence already exists for today at that location and schedule: date is still editable.
- [x] 1.3 Pick From Schedule mode, same link: date renders as the scheduled date picker and offers only the pinned schedule's dates.
- [x] 1.4 Clear the Group Attendance List filter so the link carries no schedule or location: editable date picker, as before the fix.
- [x] 1.5 Pick From Schedule mode, same link as 1.3, but the pinned schedule has no start times inside "Number of Previous Days To Show": today's date shows read-only alongside the read-only location and schedule. No free date picker, and the date must not disappear.

### 2. Existing occurrence stays locked

- [x] 2.1 `OccurrenceId` parameter: date, location, and schedule all read-only. Confirm in each of the three Date Selection Modes.
- [x] 2.2 Click an existing occurrence row in Group Attendance List: same as 2.1.

### 3. No parameters (unchanged)

- [x] 3.1 Date Picker: editable date picker and location picker. Pick a location, then a date that location's schedule meets on: the schedule picker appears listing that location's schedules for that date.
- [x] 3.2 Pick From Schedule: scheduled date picker listing every schedule at the location.
- [x] 3.3 Current Date: unchanged from pre-fix behavior.
- [x] 3.4 Pick From Schedule: choose a location that has no scheduled dates. Today's date shows read-only instead of vanishing. Choose a location that does have scheduled dates and the scheduled date picker returns.

### 4. Per-field pinning

Each parameter must pin only its own field. Every case here previously rendered all three fields read-only.

- [x] 4.1 `Date` only: date read-only, showing that date; location picker; schedule picker once a location is chosen that has a schedule meeting on that date.
- [x] 4.2 `Occurrence` only (the legacy date alias): same as 4.1.
- [x] 4.3 `LocationId` only: location read-only; schedule renders as a picker listing that location's schedules that meet on the selected date; date per mode.
- [x] 4.4 `ScheduleId` only: schedule read-only; location renders as a picker; date per mode.
- [x] 4.5 `Date` + `LocationId`: date read-only; location read-only; schedule picker.
- [x] 4.6 `LocationId` + `ScheduleId`: location and schedule both read-only; date per mode.
- [x] 4.7 A group whose GroupType requires a location or schedule: the required field still resolves to a usable control (picker or read-only value) in each combination above. Note that neither block has ever blocked saving when a required location or schedule is missing; see Out of Scope.
- [x] 4.8 Click a scheduled-but-not-yet-created row in Group Attendance List (link carries `Date` + `LocationId` + `ScheduleId`): date, location, and schedule all read-only, the same as clicking an existing row.
- [x] 4.9 Follow an attendance reminder link (`GroupId` + `Occurrence`): date read-only showing the reminded date; location and schedule render as pickers.

### 5. Saving after the date changes

- [x] 5.1 With `ScheduleId` pinned, change the date and mark attendance: attendance lands on an occurrence for the new date carrying the pinned schedule, and the original occurrence is untouched.
- [x] 5.2 Pick From Schedule with `ScheduleId` pinned: choosing any date in the list leaves the schedule unchanged.
- [x] 5.3 "Allow Add" off: changing the date to one with no existing occurrence does not create one.

### 6. Schedule pin filter

- [x] 6.1 Location with two or more schedules, `ScheduleId` pinning one, Pick From Schedule: the dropdown lists only the pinned schedule's dates.
- [x] 6.2 Same location with no `ScheduleId`: the dropdown lists dates for all of the location's schedules.
- [x] 6.3 Current Date with `ScheduleId` pinned and that schedule meeting today: date, location, and schedule all render as read-only labels. No one-option picker.

## Out of Scope

- **"Current Date" mode can show a schedule picker.** By default it renders today's date read-only. It swaps in `GroupLocationScheduleDatePicker` only when a location is selected (pinned by `LocationId` or picked by the individual), the schedule is not pinned, and at least one of that location's active schedules meets today. The list is limited to today, so the date itself stays locked; what the control chooses is which of the location's schedules today's attendance is recorded against. Whether that is what admins expect from a mode named "Current Date" is a separate question and is not changed here. (The pinned-schedule case is in scope; see Fix item 3.)
- **A pinned schedule does not constrain the date in "Date Picker" mode.** Intentional: that setting's contract is "individual can pick any date," and an admin who wants the constraint selects Pick From Schedule. Legacy behaved the same way.
- **No save-time validation that an occurrence date falls on its schedule.** Neither block has ever had this. Validity is enforced only by what the pickers offer, never by what the save accepts, and page parameters or Date Picker mode route around the pickers. Making the safety real means validating the group, location, schedule, and date together on save; that is a separate change.
- **"Requires location" and "requires schedule" on the GroupType are not enforced on save.** The block reads both flags onto the box, but the client uses `isLocationRequired` only to hide the schedule column until a location is chosen, never reads `isScheduleRequired`, and the server never checks either. Attendance saves with a null location or schedule regardless. Legacy did not reference these flags at all. Pre-existing, and the fix does not change it.

## Considered but Rejected

### Narrow the single `IsSpecificSearch` flag to occurrence identity only

Rejected. A one-line change fixes the date, but because the same flag gated all three fields it would also turn location and schedule into editable pre-selections, which legacy kept read-only when they arrived as parameters. One flag per field is a slightly larger change that matches legacy in every combination.

### Stop propagating the flag onto newly created occurrences only

Rejected. The flag is recomputed from page parameters on every refetch and `ScheduleId` stays in the URL, so the date would unlock on first load and re-lock the moment a date was picked. It also misses the case where an occurrence already exists for today at that location and schedule.

### Fall back to Date Picker when the schedule is pinned

Rejected in favor of filtering the scheduled date picker's list. Falling back would let the individual choose a date the pinned schedule does not occur on, and would discard the schedule-aware date list entirely.

### Fall back to a free date picker when Pick From Schedule has no dates

Rejected after being tried. It offers an unrestricted date in the one mode an admin chose specifically to restrict dates to the schedule. Showing today read-only keeps the date visible without adding a new way to record the wrong one.

### Hide the date (`None`) when Pick From Schedule has no dates

Rejected. `None` hides only the date control; the roster still works and attendance still lands on today. The date can still be wrong; it just is not shown. That is not a safety.

### Leave the date editable when `Date` or `Occurrence` is passed (legacy behavior)

Rejected. Every real link that passes a date names a specific meeting: the attendance reminder communication and a scheduled row in Group Attendance List. Editable would let a leader drift attendance onto a different meeting silently. The "met on a different day" case has a clean path through "Did Not Meet" plus the list's Add button, which offers an editable date after this fix.

## Related

- [GitHub issue #7009](https://github.com/SparkDevNetwork/Rock/issues/7009)
- Asana DEV-15369
- Legacy block removed in `278e1fb088`
