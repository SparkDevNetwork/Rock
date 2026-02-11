import { CheckInStatus } from "@Obsidian/Enums/Event/checkInStatus";
import { RosterAttendeeBag } from "@Obsidian/ViewModels/Blocks/CheckIn/Manager/Roster/rosterAttendeeBag";
import { RosterOptionsBag } from "@Obsidian/ViewModels/Blocks/CheckIn/Manager/Roster/rosterOptionsBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { Ref } from "vue";

export const enum RosterViewMode {
    All = 0,
    CheckedIn = 1,
    Present = 2,
    CheckedOut = 3,
}

export type RosterSingleAttendanceRecord = {
    idKey: string;

    attendee: RosterAttendeeBag;

    checkInTime: string;

    checkInDuration: number;

    presentTime?: string;

    presentDuration: number;

    checkoutTime?: string;

    checkoutDuration: number;

    code: string;

    schedule: ListItemBag;

    group: ListItemBag;

    area: ListItemBag;

    status: CheckInStatus;

    isFirstTime: boolean;

    isCheckoutSupported: boolean;

    isPresenceSupported: boolean;

    isRemoved?: boolean;
};

export type RosterCompoundAttendanceRecord = {
    /** The identifier of the attendee. */
    idKey?: string | null;

    attendee: RosterAttendeeBag;

    records: RosterSingleAttendanceRecord[];
};

export type RosterAttendanceRecord = RosterSingleAttendanceRecord | RosterCompoundAttendanceRecord;

export type RosterContext = {
    config: RosterOptionsBag;

    loadingRowCount: Ref<number | undefined>;

    mode: Ref<RosterViewMode>;

    schedule: Ref<ListItemBag | undefined>;

    onSelectItem: (key: string) => void;

    onAttendancePresentClick: (rows: RosterAttendanceRecord[]) => Promise<void>;

    onAttendanceCheckoutClick: (row: RosterAttendanceRecord) => Promise<void>;

    onAttendanceCheckoutAllClick: (records: RosterSingleAttendanceRecord[]) => Promise<void>;

    onAttendanceDeleteClick: (row: RosterAttendanceRecord) => Promise<void>;

    onAttendanceStayingClick: (row: RosterAttendanceRecord) => Promise<void>;

    onAttendanceNotPresentClick: (row: RosterAttendanceRecord) => Promise<void>;
};
