/* eslint-disable @typescript-eslint/naming-convention */

// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

import { InjectionKey, Ref } from "vue";
import { ResourceListSourceType } from "@Obsidian/Enums/Blocks/Group/Scheduling/resourceListSourceType";

/**
 * Information about a scheduler resource assignment for the group scheduler.
 * Represenation of: https://github.com/SparkDevNetwork/Rock/blob/8dfb45edcbf4f166d483f6e96ed39806f3ca6a1b/Rock/Model/Event/Attendance/AttendanceService.cs#L3107
 */
export interface ISchedulerResourceAssignment {
    /** The group identifier. */
    GroupId: number,

    /** The name of the group. */
    GroupName?: string | null,

    /** The schedule identifier. */
    ScheduleId: number,

    /** The naem of the schedule. */
    ScheduleName?: string | null,

    /** The location identifier. */
    LocationId?: number | null,

    /** The name of the location. */
    LocationName?: string | null,

    /** The occurrence date. */
    OccurrenceDate?: string | null
}

/**
 * Information about a group [type] role.
 */
export interface IGroupRole {
    Name?: string | null;
}

/**
 * Information about a potential scheduler resource (Person) for the group scheduler.
 * Represenation of: https://github.com/SparkDevNetwork/Rock/blob/8dfb45edcbf4f166d483f6e96ed39806f3ca6a1b/Rock/Model/Event/Attendance/AttendanceService.cs#L3169
 */
export interface ISchedulerResource {
    /** The person identifier. */
    PersonId: number,

    /** The scheduled attendance confirmation status of the resource. */
    ConfirmationStatus: string,

    /** The group member ID. NOTE: This will be NULL if the resource list has manually added personIds and/or comes from a Person DataView. */
    GroupMemberId?: number | null,

    // LPC CODE
    /** The name of the group member's ScheduleTemplate. NOTE: This will be NULL if the resource list has manually added personIds and/or comes from a Person DataView. */
    ScheduleTemplateName?: string | null,
    // END LPC CODE

    /** The nickname of the person. */
    PersonNickName?: string | null,

    /** The last name of the person. */
    PersonLastName?: string | null,

    /** The name of the person. */
    PersonName?: string | null,

    /** The photo URL for the person. */
    PersonPhotoUrl?: string | null,

    /** The last attendance date time. */
    LastAttendanceDateTime?: string | null,

    /** The last attendance date time, formattted. */
    LastAttendanceDateTimeFormatted?: string | null,

    /** The note. */
    Note?: string | null,

    /** Whether this person has a conflict. */
    HasConflict: boolean,

    /** The conflict note. */
    ConflictNote?: string | null,

    /** Whether this Person has blackout conflict for all the occurrences. */
    HasBlackoutConflict: boolean,

    /** Whether this Person has partial blackout conflict (blackout for some of the occurrences, but not all of them). */
    HasPartialBlackoutConflict: boolean,

    /** The number of days shown in the Group Scheduler. */
    DisplayedDaysCount?: number | null,

    /** Obsolete: Use DisplayedDaysCount instead */
    OccurrenceDateCount: number,

    /** The displayed time slot count. */
    DisplayedTimeSlotCount?: number | null,

    /** The blackout dates */
    BlackoutDates?: string[] | null,

    /** Whether this Person has group requirements conflict. */
    HasGroupRequirementsConflict: boolean,

    /** Whether this Person has scheduling conflict with some other group for this schedule+date. */
    HasSchedulingConflict: boolean,

    /** The scheduling conflicts. */
    SchedulingConflicts?: ISchedulerResourceAssignment[] | null,

    /** Whether this Person is already scheduled for this group+schedule+date. */
    IsAlreadyScheduledForGroup?: boolean | null,

    /** The group role, if the person is a member of the occurrence group. */
    GroupRole: IGroupRole,

    /** The name of the group role. */
    GroupRoleName?: string | null,

    /** The resource's preferences. */
    ResourcePreferenceList?: ISchedulerResourceAssignment[] | null,

    /** Teh resource's scheduled list. */
    ResourceScheduledList?: ISchedulerResourceAssignment[] | null
}

/**
 * A scheduler resource (Person) that has been associated with an attendance occurrence in some sort of scheduled state (Pending, Confirmed or Declined).
 * Representation of: https://github.com/SparkDevNetwork/Rock/blob/8dfb45edcbf4f166d483f6e96ed39806f3ca6a1b/Rock/Model/Event/Attendance/AttendanceService.cs#L3045
 */
export interface ISchedulerResourceAttend extends ISchedulerResource {
    /** The attendance identifier. */
    AttendanceId: number,

    /** The occurrence date. */
    OccurrenceDate?: string | null,

    /** How the scheduled attendance instance matches the preference of the individual. */
    MatchesPreference: string,

    /** Whether this scheduled resource has a blackout conflict for the occurrence date. */
    HasBlackoutConflict: boolean,

    /** The declined reason. */
    DeclinedReason?: string | null
}

/**
 * Request parameters indicating the resources that should be retrieved for a group scheduler occurrence.
 * Representation of: https://github.com/SparkDevNetwork/Rock/blob/8dfb45edcbf4f166d483f6e96ed39806f3ca6a1b/Rock/Model/Event/Attendance/AttendanceService.cs#L3486
 */
export interface ISchedulerResourceParameters {
    /** The attendance occurrence group identifier. */
    AttendanceOccurrenceGroupId: number,

    /** The attendance occurrence schedule IDs. */
    AttendanceOccurrenceScheduleIds: number[],

    /** The attendance occurrence location IDs */
    AttendanceOccurrenceLocationIds: number[],

    /** The attendance occurrence Sunday date. */
    AttendanceOccurrenceSundayDate: string,

    /** The resource group identifier. */
    ResourceGroupId?: number | null,

    /** The resource group guid. */
    ResourceGroupGuid?: string | null,

    /** If we just need the data for a specific person. */
    LimitToPersonId?: number | null,

    /** The group member filter type that should be used when retrieving available resources for the group scheduler. */
    GroupMemberFilterType?: number | null,

    /** The resource list source type that should be used when retrieving available resources for the group scheduler. */
    ResourceListSourceType?: ResourceListSourceType | null,

    /** The resource data view identifier. */
    ResourceDataViewId?: number | null,

    /** The resource data view guid. */
    ResourceDataViewGuid?: string | null,

    /** The resource additional person IDs. */
    ResourceAdditionalPersonIds?: number[] | null
}

/**
 * Information about a group scheduler occurrence's progress (towards filling the specified min, desired and max capcacities).
 */
export interface IScheduleProgress {
    /** The minimum capacity for this occurrence. */
    minimumCapacity?: number | null,

    /** The desired capacity for this occurrence. */
    desiredCapacity?: number | null,

    /** The maximum capacity for this occurrence. */
    maximumCapacity?: number | null,

    /** The count of confirmed resources for this occurrence. */
    confirmedCount: number,

    /** The count of pending resources for this occurrence. */
    pendingCount: number
}

/**
 * Information about the remaining resource spots to be filled for a group scheduler occurrence.
 */
export interface IRemainingResourceSpots {
    /** A brief explanation of the remaning spots. */
    label: string;

    /** The CSS class(es) that should be applied to the remaining spots element. */
    cssClass: string;
}

/**
 * An injection key to provide a computed occurrence date title to descendent components.
 */
export const OccurrenceDateTitle: InjectionKey<Ref<string>> = Symbol("occurrence-date-title");

/**
 * An injection key to instruct all schedule occurrences to reload themselves.
 */
export const ReloadAllOccurrences: InjectionKey<Ref<boolean>> = Symbol("reload-all-occurrences");

/**
 * An injection key to instruct occurrences belonging to a specific schedule to reload themselves.
 * The number injected will be the attendance occurrence ID that triggered the update, so it knows
 * that it doesn't need to update itself, while all of its siblings will know to update themselves.
 */
export const ReloadScheduleOccurrences: InjectionKey<Ref<number | null>> = Symbol("reload-schedule-occurrences");

/**
 * An injection key to instruct occurrences containing a specific resource to reload themselves.
 * The number injected will be the person ID representing the targeted resource.
 */
export const ReloadOccurrencesContainingResource: InjectionKey<Ref<number | null>> = Symbol("reload-occurrences-containing-resource");

/**
 * The available progress states for a group scheduler occurrence.
 */
export const ProgressState = {
    danger: "danger",
    critical: "critical",
    warning: "warning",
    success: "success"
};

/**
 * The classification of how a scheduled attendance instance matches the preference of the individual.
 */
export const Preference = {
    matches: "matches-preference",
    notMatches: "not-matches-preference",
    none: "no-preference"
};

/**
 * The actions that can be taken for a given, scheduled resource.
 */
export const enum ResourceAction {
    MarkConfirmed = 0,
    MarkPending = 1,
    MarkDeclined = 2,
    ResendConfirmation = 3,
    UpdatePreference = 4,
    Remove = 5
}

/**
 * The navigation url keys for linked pages.
 */
export const enum NavigationUrlKey {
    CopyLink = "CopyLink",
    RosterPage = "RosterPage"
}
