import { CheckInStatus } from "@Obsidian/Enums/Event/checkInStatus";
import { RockDateTime } from "@Obsidian/Utility/rockDateTime";
import { RosterAttendanceRecord, RosterSingleAttendanceRecord, RosterViewMode } from "./types.partial";
import { RosterAttendanceBag } from "@Obsidian/ViewModels/Blocks/CheckIn/Manager/Roster/rosterAttendanceBag";
import { inject, provide } from "vue";
import { RosterContext } from "./types.partial";

const contextSymbol = Symbol("RosterContext");

/**
 * Provides the RosterContext to child components.
 *
 * @param context The context to be provided to child components.
 */
export function provideRosterContext(context: RosterContext): void {
    provide(contextSymbol, context);
}

/**
 * Retrieves the RosterContext provided to child components.
 *
 * @returns The context to use with the roster block.
 */
export function useRosterContext(): RosterContext {
    const context = inject<RosterContext>(contextSymbol);

    if (!context) {
        throw new Error("RosterContext is not provided");
    }

    return context;
}

/**
 * Calculates the duration in minutes since the check-in time.
 *
 * @param checkInTime The time the check-in happened as an ISO string.
 *
 * @returns The number of minutes since the check-in happened.
 */
export function calculateDuration(checkInTime: string | undefined): number {
    if (!checkInTime) {
        return 0;
    }

    const date = RockDateTime.parseISO(checkInTime);

    if (!date) {
        return 0;
    }

    const localCheckInTime = date.localDateTime;
    const now = RockDateTime.now();
    const durationInSeconds = (now.toMilliseconds() - localCheckInTime.toMilliseconds()) / 1000;

    return Math.max(0, Math.floor(durationInSeconds / 60));
}

/**
 * Determines if the given status matches the selected roster view mode.
 *
 * @param status The status for the attendance record.
 * @param mode The roster view mode displayed in the UI.
 *
 * @returns true if the status matches the selected roster mode.
 */
export function statusMatchesMode(status: CheckInStatus, mode: RosterViewMode): boolean {
    switch (mode) {
        case RosterViewMode.CheckedIn:
            return status === CheckInStatus.NotPresent;
        case RosterViewMode.Present:
            return status === CheckInStatus.Present;
        case RosterViewMode.CheckedOut:
            return status === CheckInStatus.CheckedOut;
        default:
            return true;
    }
}

/**
 * Updates a single attendance record in the grid with new data.
 *
 * @param attendance The attendance record that contains the updated data.
 * @param record The record in the grid to update.
 */
export function updateSingleGridAttendanceRecord(attendance: RosterAttendanceBag, record: RosterSingleAttendanceRecord): void {
    record.attendee = attendance.attendee!;
    record.checkInTime = attendance.checkInTime!;
    record.presentTime = attendance.presentTime ?? undefined;
    record.checkoutTime = attendance.checkoutTime ?? undefined;
    record.code = attendance.code!;
    record.schedule = attendance.schedule!;
    record.group = attendance.group!;
    record.area = attendance.area!;
    record.status = attendance.status;
    record.isFirstTime = attendance.isFirstTime;
    record.isCheckoutSupported = attendance.isCheckoutSupported;
    record.isPresenceSupported = attendance.isPresenceSupported;

    record.checkInDuration = calculateDuration(record.checkInTime);
    record.presentDuration = calculateDuration(record.presentTime);
    record.checkoutDuration = calculateDuration(record.checkoutTime);
}

/**
 * Retrieves single attendance records from a roster attendance record. If the
 * attendance record is compound, all single records are returned; otherwise,
 * the original record is returned in an array of one.
 *
 * @param record The roster attendance record.
 *
 * @returns An array of single attendance records.
 */
export function getSingleAttendanceRecords(record: RosterAttendanceRecord): RosterSingleAttendanceRecord[] {
    if ("records" in record) {
        return record.records;
    }
    else {
        return [record];
    }
}

/**
 * Gets the CSS class names for a grid row based on its data.
 *
 * @param row The row about to be displayed.
 *
 * @returns A string that contains CSS class names separated by a space.
 */
export function getGridRowClass(row: Record<string, unknown>): string {
    const record = row as RosterAttendanceRecord;

    if (!("records" in record)) {
        if (record.isRemoved) {
            return "o-50";
        }
    }
    else {
        if (record.records.every(r => r.isRemoved)) {
            return "o-50";
        }
    }

    return "";
}

/**
 * Gets the HTML string for a status badge based on the check-in status.
 *
 * @param status The enum value of the check-in status.
 *
 * @returns A string that contains the HTML to render the badge.
 */
export function getStatusBadgeHtml(status: CheckInStatus): string {
    if (status === CheckInStatus.NotPresent) {
        return "<span class='badge badge-warning'>Checked-in</span>";
    }
    else if (status === CheckInStatus.Present) {
        return "<span class='badge badge-success'>Present</span>";
    }
    else {
        return "<span class='badge badge-checked-out'>Checked-out</span>";
    }
}

/**
 * Formats a duration in seconds into a string representing the duration in
 * hours and minutes.
 *
 * @param duration The duration in seconds.
 *
 * @returns A formatted string representing the duration in hours and minutes.
 */
export function getDurationText(duration: number): string {
    if (duration >= 60) {
        const hours = Math.floor(duration / 60);
        const minutes = duration % 60;
        return `${hours}h ${minutes}m`;
    }
    else {
        return `${duration}m`;
    }
}

