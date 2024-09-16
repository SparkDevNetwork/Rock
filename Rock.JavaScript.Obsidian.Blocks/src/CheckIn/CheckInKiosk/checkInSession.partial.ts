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

import { FamilySearchMode } from "@Obsidian/Enums/CheckIn/familySearchMode";
import { KioskCheckInMode } from "@Obsidian/Enums/CheckIn/kioskCheckInMode";
import { Guid } from "@Obsidian/Types";
import { HttpFunctions } from "@Obsidian/Types/Utility/http";
import { newGuid } from "@Obsidian/Utility/guid";
import { KioskConfigurationBag } from "@Obsidian/ViewModels/Blocks/CheckIn/CheckInKiosk/kioskConfigurationBag";
import { AbilityLevelOpportunityBag } from "@Obsidian/ViewModels/CheckIn/abilityLevelOpportunityBag";
import { AreaOpportunityBag } from "@Obsidian/ViewModels/CheckIn/areaOpportunityBag";
import { AttendanceBag } from "@Obsidian/ViewModels/CheckIn/attendanceBag";
import { AttendeeBag } from "@Obsidian/ViewModels/CheckIn/attendeeBag";
import { FamilyBag } from "@Obsidian/ViewModels/CheckIn/familyBag";
import { GroupOpportunityBag } from "@Obsidian/ViewModels/CheckIn/groupOpportunityBag";
import { LocationOpportunityBag } from "@Obsidian/ViewModels/CheckIn/locationOpportunityBag";
import { OpportunityCollectionBag } from "@Obsidian/ViewModels/CheckIn/opportunityCollectionBag";
import { ScheduleOpportunityBag } from "@Obsidian/ViewModels/CheckIn/scheduleOpportunityBag";
import { AttendeeOpportunitiesOptionsBag } from "@Obsidian/ViewModels/Rest/CheckIn/attendeeOpportunitiesOptionsBag";
import { AttendeeOpportunitiesResponseBag } from "@Obsidian/ViewModels/Rest/CheckIn/attendeeOpportunitiesResponseBag";
import { CheckoutOptionsBag } from "@Obsidian/ViewModels/Rest/CheckIn/checkoutOptionsBag";
import { CheckoutResponseBag } from "@Obsidian/ViewModels/Rest/CheckIn/checkoutResponseBag";
import { ConfirmAttendanceOptionsBag } from "@Obsidian/ViewModels/Rest/CheckIn/confirmAttendanceOptionsBag";
import { ConfirmAttendanceResponseBag } from "@Obsidian/ViewModels/Rest/CheckIn/confirmAttendanceResponseBag";
import { FamilyMembersOptionsBag } from "@Obsidian/ViewModels/Rest/CheckIn/familyMembersOptionsBag";
import { FamilyMembersResponseBag } from "@Obsidian/ViewModels/Rest/CheckIn/familyMembersResponseBag";
import { SaveAttendanceOptionsBag } from "@Obsidian/ViewModels/Rest/CheckIn/saveAttendanceOptionsBag";
import { SaveAttendanceResponseBag } from "@Obsidian/ViewModels/Rest/CheckIn/saveAttendanceResponseBag";
import { SearchForFamiliesOptionsBag } from "@Obsidian/ViewModels/Rest/CheckIn/searchForFamiliesOptionsBag";
import { SearchForFamiliesResponseBag } from "@Obsidian/ViewModels/Rest/CheckIn/searchForFamiliesResponseBag";
import { Screen } from "./types.partial";
import { InvalidCheckInStateError, UnexpectedErrorMessage, clone, isAnyIdInList } from "./utils.partial";
import { AttendanceRequestBag } from "@Obsidian/ViewModels/CheckIn/attendanceRequestBag";
import { RecordedAttendanceBag } from "@Obsidian/ViewModels/CheckIn/recordedAttendanceBag";
import { OpportunitySelectionBag } from "@Obsidian/ViewModels/CheckIn/opportunitySelectionBag";
import { CheckInItemBag } from "@Obsidian/ViewModels/CheckIn/checkInItemBag";
import { AbilityLevelDeterminationMode } from "@Obsidian/Enums/CheckIn/abilityLevelDeterminationMode";

type Mutable<T> = { -readonly [P in keyof T]: T[P] };

type FunctionPropertyNames<T> = {
    // eslint-disable-next-line @typescript-eslint/ban-types
    [K in keyof T]: T[K] extends Function ? K : never
}[keyof T];

type CheckInSessionProperties = Partial<Omit<CheckInSession, NonNullable<FunctionPropertyNames<CheckInSession>>>>;

/**
 * Handles the heavy logic for a single check-in session. A session begins when
 * a search operation is initiated and ends when the welcome screen is displayed.
 * So a session can handle a single person check-in or a whole family check-in.
 *
 * This is designed to be an immutable object. Changes are made by creating new
 * instances that are copies of the original with the changes overlayed on top.
 * This makes it easier to manage the state alongside Vue so that we don't have
 * to deal with deep watchers. It also helps deal with the fact that we have
 * lots of async functionality. If we changed one property at a time we would
 * then have Vue updating the UI to reflect those partial changes while we waited
 * for a response from the server on the second bit of data.
 */
export class CheckInSession {
    // #region Fields

    /** The current screen that should be displayed for this session. */
    public readonly currentScreen: Screen;

    /** The search term used to start this session. */
    public readonly searchTerm?: string;

    /** The search type used to start this session. */
    public readonly searchType?: FamilySearchMode;

    /** The families that were matched by the search term for this session. */
    public readonly families?: FamilyBag[];

    /** The currently selected family identifier. */
    public readonly currentFamilyId?: string | null;

    /** The currently checked in attendance records. */
    public readonly currentlyCheckedIn?: AttendanceBag[];

    /** The potential attendees that can be checked in. */
    public readonly attendees?: AttendeeBag[];

    /** The attendee identifiers that were selected to be checked in. */
    public readonly selectedAttendeeIds?: string[];

    /** The currently selected attendee identifier. */
    public readonly currentAttendeeId?: string | null;

    /** The available opportunities for the currently selected attendee. */
    public readonly attendeeOpportunities?: OpportunityCollectionBag;

    /** The currently selected ability level for the currently selected attendee. */
    public readonly selectedAbilityLevel?: CheckInItemBag;

    /** The currently area level for the currently selected attendee. */
    public readonly selectedArea?: CheckInItemBag;

    /** The currently group level for the currently selected attendee. */
    public readonly selectedGroup?: CheckInItemBag;

    /** The currently location level for the currently selected attendee. */
    public readonly selectedLocation?: CheckInItemBag;

    /**
     * The currently selected schedules for either the currently selected
     * attendee or a family of attendees depending on the kiosk mode.
     */
    public readonly selectedSchedules?: CheckInItemBag[];

    /**
     * When in non-auto select family mode, this indicates which schedule
     * we are currently processing for the attendee.
     */
    public readonly currentFamilyScheduleId?: string | null;

    /**
     * All possible schedules available for check-in, this is used in family
     * mode when displaying the schedule list since we haven't loaded a person
     * yet.
     */
    public readonly possibleSchedules?: ScheduleOpportunityBag[];

    /**
     * All selections made for attendees. This is the set of selections that
     * will be converted to attendance records during save.
     */
    public readonly allAttendeeSelections: { attendeeId: string, selections: OpportunitySelectionBag[] }[] = [];

    /** The attendance records that have been sent to the server and saved. */
    public readonly attendances: RecordedAttendanceBag[] = [];

    /** `true` if the current operation is for checkout. */
    public readonly isCheckoutAction: boolean = false;

    /** The attendance records that were checked out. */
    public readonly checkedOutAttendances: AttendanceBag[] = [];

    /**
     * The unique identifer for this check-in session. This is used to link
     * all attendance records together since we might save them at different
     * times.
     */
    public readonly sessionGuid: Guid;

    /** The kiosk configuration that this session will conform to. */
    public readonly configuration: KioskConfigurationBag;

    /** The object to use when making HTTP requests to the server. */
    public readonly http: HttpFunctions;

    // #endregion

    // #region Constructors

    /**
     * Creates a new check-in session object that can be used to process a
     * single individual check-in or a whole family check-in depending on the
     * kiosk configuration.
     *
     * @param configuration The kiosk configured that this session will conform to.
     * @param http The object that provides HTTP access to the server.
     * @param sessionGuid The session unique identifier, if not specified then one will be generated.
     */
    public constructor(configuration: KioskConfigurationBag, http: HttpFunctions, sessionGuid?: Guid);

    /**
     * Clones an existing session and then updates any specific override values.
     *
     * @param session The existing session to clone.
     * @param overrides The properties to be set in the new instance.
     */
    public constructor(session: CheckInSession, overrides: CheckInSessionProperties);

    public constructor(configurationOrSession: KioskConfigurationBag | CheckInSession, httpOrOverrides: HttpFunctions | CheckInSessionProperties, sessionGuid?: Guid) {
        if (configurationOrSession instanceof CheckInSession) {
            this.sessionGuid = configurationOrSession.sessionGuid;
            this.configuration = configurationOrSession.configuration;
            this.http = configurationOrSession.http;
            this.currentScreen = configurationOrSession.currentScreen;
            this.searchTerm = configurationOrSession.searchTerm;
            this.searchType = configurationOrSession.searchType;
            this.families = clone(configurationOrSession.families);
            this.currentFamilyId = configurationOrSession.currentFamilyId;
            this.attendees = clone(configurationOrSession.attendees);
            this.currentlyCheckedIn = clone(configurationOrSession.currentlyCheckedIn);
            this.selectedAttendeeIds = clone(configurationOrSession.selectedAttendeeIds);
            this.currentAttendeeId = configurationOrSession.currentAttendeeId;
            this.attendeeOpportunities = clone(configurationOrSession.attendeeOpportunities);
            this.selectedAbilityLevel = clone(configurationOrSession.selectedAbilityLevel);
            this.selectedArea = clone(configurationOrSession.selectedArea);
            this.selectedGroup = clone(configurationOrSession.selectedGroup);
            this.selectedLocation = clone(configurationOrSession.selectedLocation);
            this.selectedSchedules = clone(configurationOrSession.selectedSchedules);
            this.currentFamilyScheduleId = configurationOrSession.currentFamilyScheduleId;
            this.possibleSchedules = configurationOrSession.possibleSchedules;
            this.allAttendeeSelections = clone(configurationOrSession.allAttendeeSelections);
            this.attendances = clone(configurationOrSession.attendances);
            this.isCheckoutAction = configurationOrSession.isCheckoutAction;
            this.checkedOutAttendances = configurationOrSession.checkedOutAttendances;

            for (const key of Object.keys(httpOrOverrides as CheckInSessionProperties)) {
                this[key] = httpOrOverrides[key];
            }
        }
        else {
            this.currentScreen = Screen.Welcome;
            this.configuration = configurationOrSession;
            this.http = httpOrOverrides as HttpFunctions;
            this.sessionGuid = sessionGuid ?? newGuid();
        }
    }

    // #endregion

    // #region Private Support Functions

    /**
     * Creates a new check-in session object that will display the specified
     * screen.
     *
     * @returns A cloned check-in session object.
     */
    private withScreen(screen: Screen): CheckInSession {
        return new CheckInSession(this, { currentScreen: screen });
    }

    /**
     * Saves the attendance record(s) for the current attendee and then returns
     * a new session object that contains the attendance results.
     *
     * @param isPending True if the attendance should be saved as pending.
     * @returns A new CheckInSession object.
     */
    private async withSaveAttendance(isPending: boolean): Promise<CheckInSession> {
        // Verify we have a valid configuration template.
        if (!this.configuration.template) {
            throw new InvalidCheckInStateError("Template has not been configured.");
        }

        // We need a valid search type.
        if (this.searchType === undefined) {
            throw new InvalidCheckInStateError("Search type was not defined.");
        }

        // Make sure we have selected attendees.
        if (!this.selectedAttendeeIds) {
            throw new InvalidCheckInStateError("No individuals were selected.");
        }

        const attendanceRequests: AttendanceRequestBag[] = [];

        for (const attendeeSelections of this.allAttendeeSelections) {
            // Skip this attendee's selections if they aren't selected for
            // check-in.
            if (!this.selectedAttendeeIds.some(a => a === attendeeSelections.attendeeId)) {
                continue;
            }

            for (const selection of attendeeSelections.selections) {
                const attendance: AttendanceRequestBag = {
                    personId: attendeeSelections.attendeeId,
                    selection: {
                        abilityLevel: selection.abilityLevel,
                        area: selection.area,
                        group: selection.group,
                        location: selection.location,
                        schedule: selection.schedule
                    }
                };

                attendanceRequests.push(attendance);
            }
        }

        // Make sure we have at least one thing to save.
        if (attendanceRequests.length === 0) {
            throw new InvalidCheckInStateError("No attendance records to create.");
        }

        // Build the API request options.
        const request: SaveAttendanceOptionsBag = {
            kioskId: this.configuration.kiosk?.id,
            templateId: this.configuration.template.id,
            session: {
                guid: this.sessionGuid,
                isPending: isPending,
                searchMode: this.searchType,
                searchTerm: this.searchTerm,
                familyId: this.currentFamilyId
            },
            requests: attendanceRequests
        };

        const response = await this.http.post<SaveAttendanceResponseBag>("/api/v2/checkin/SaveAttendance", undefined, request);

        if (!response.isSuccess || !response.data?.attendances) {
            throw new Error(response.errorMessage || UnexpectedErrorMessage);
        }

        return new CheckInSession(this, {
            attendances: [...this.attendances, ...response.data.attendances],
            allAttendeeSelections: []
        });
    }

    /**
     * Creates a new session object by confirming the pending attendance data
     * and then storing the final attendance data in the new session.
     *
     * @returns A new CheckInSession object.
     */
    private async withConfirmAttendance(): Promise<CheckInSession> {
        if (!this.configuration.template) {
            throw new InvalidCheckInStateError("Template configuration is missing.");
        }

        // Build the API request options.
        const request: ConfirmAttendanceOptionsBag = {
            templateId: this.configuration.template.id,
            kioskId: this.configuration.kiosk?.id,
            sessionGuid: this.sessionGuid
        };

        const response = await this.http.post<ConfirmAttendanceResponseBag>("/api/v2/checkin/ConfirmAttendance", undefined, request);

        if (!response.isSuccess || !response.data?.attendances) {
            throw new Error(response.errorMessage || UnexpectedErrorMessage);
        }

        // Take our existing attendance records and replace any that match
        // the records we just got. Add any new records that didn't previously
        // exist in the data.
        const attendances = [...this.attendances];
        for (let i = 0; i < response.data.attendances.length; i++) {
            const attendanceItem = response.data.attendances[i];
            const existingIndex = attendances
                .findIndex(a => a.attendance?.id === attendanceItem.attendance?.id);

            if (existingIndex !== -1) {
                attendances.splice(existingIndex, 1, attendanceItem);
            }
            else {
                attendances.push(attendanceItem);
            }
        }

        return new CheckInSession(this, {
            attendances,
            allAttendeeSelections: []
        });
    }

    // #endregion

    // #region Public Selection Functions

    /**
     * Creates a new session with the kiosk configuration options.
     *
     * @param configuration The new configuration that should be applied to the session.
     *
     * @returns A new CheckInSession object.
     */
    public withConfiguration(configuration: KioskConfigurationBag): CheckInSession {
        return new CheckInSession(this, {
            configuration
        });
    }

    /**
     * Creates a new session by performing a family search.
     *
     * @param searchTerm The term to use when searching for families.
     * @param searchType The type of the search term.
     *
     * @returns A new CheckInSession object.
     */
    public async withFamilySearch(searchTerm: string, searchType: FamilySearchMode): Promise<CheckInSession> {
        if (!this.configuration.template) {
            throw new InvalidCheckInStateError("No configuration template found.");
        }

        const request: SearchForFamiliesOptionsBag = {
            configurationTemplateId: this.configuration.template?.id,
            kioskId: this.configuration.kiosk?.id,
            prioritizeKioskCampus: false,
            searchTerm: searchTerm,
            searchType: searchType
        };

        const response = await this.http.post<SearchForFamiliesResponseBag>("/api/v2/checkin/SearchForFamilies", undefined, request);

        if (!response.isSuccess || !response.data?.families) {
            throw new Error(response.errorMessage || UnexpectedErrorMessage);
        }

        return new CheckInSession(this, {
            families: response.data.families,
            searchTerm,
            searchType
        });
    }

    /**
     * Creates a new session by selecting the specified family. This will load
     * the family members for the selected family.
     *
     * @param familyId The family identifier to be selected.
     *
     * @returns A new CheckInSession object.
     */
    public async withFamily(familyId: string): Promise<CheckInSession> {
        if (!this.configuration.template || !this.configuration.kiosk) {
            throw new InvalidCheckInStateError("No configuration template or kiosk.");
        }

        const request: FamilyMembersOptionsBag = {
            configurationTemplateId: this.configuration.template.id,
            kioskId: this.configuration.kiosk.id,
            areaIds: this.configuration.areas?.filter(a => !!a.id).map(a => a.id as string),
            familyId: familyId
        };

        const response = await this.http.post<FamilyMembersResponseBag>("/api/v2/checkin/FamilyMembers", undefined, request);

        if (!response.isSuccess || !response.data?.people) {
            throw new Error(response.errorMessage || UnexpectedErrorMessage);
        }

        return new CheckInSession(this, {
            currentFamilyId: familyId,
            attendees: response.data.people,
            possibleSchedules: response.data.possibleSchedules ?? [],
            currentlyCheckedIn: response.data.currentlyCheckedInAttendances ?? []
        });
    }

    /**
     * Creates a new session by configuring it for the checkout action.
     *
     * @returns A new CheckInSession object.
     */
    public async withCheckoutAction(): Promise<CheckInSession> {
        if (!this.configuration.template || !this.configuration.kiosk) {
            throw new InvalidCheckInStateError("No configuration template or kiosk.");
        }

        return new CheckInSession(this, {
            isCheckoutAction: true
        });
    }

    /**
     * Creates a new session by selecting the attendance that should be
     * processed for checkout.
     *
     * @param attendanceIds The attendance identifiers that should be selected.
     *
     * @returns A new CheckInSession object.
     */
    public async withCheckoutAttendances(attendanceIds: string[]): Promise<CheckInSession> {
        if (!this.configuration.template || !this.configuration.kiosk) {
            throw new InvalidCheckInStateError("No configuration template or kiosk.");
        }

        // We need a valid search type.
        if (this.searchType === undefined) {
            throw new InvalidCheckInStateError("Search type was not defined.");
        }

        // Build the API request options.
        const request: CheckoutOptionsBag = {
            kioskId: this.configuration.kiosk?.id,
            templateId: this.configuration.template.id,
            session: {
                guid: this.sessionGuid,
                isPending: false,
                searchMode: this.searchType,
                searchTerm: this.searchTerm,
                familyId: this.currentFamilyId
            },
            attendanceIds: attendanceIds
        };

        const response = await this.http.post<CheckoutResponseBag>("/api/v2/checkin/Checkout", undefined, request);

        if (!response.isSuccess || !response.data?.attendances) {
            throw new Error(response.errorMessage || UnexpectedErrorMessage);
        }

        return new CheckInSession(this, {
            checkedOutAttendances: [...this.checkedOutAttendances, ...response.data.attendances]
        });
    }

    /**
     * Creates a new session by selecting the attendees that should be processed
     * in family check-in mode.
     *
     * @param attendeeIds The attendee identifiers that should be selected.
     *
     * @returns A new CheckInSession object.
     */
    public withSelectedAttendees(attendeeIds: string[]): CheckInSession {
        return new CheckInSession(this, {
            selectedAttendeeIds: attendeeIds
        });
    }

    /**
     * Creates a new session by selecting the next attendee that should be
     * processed for opportunity selection screens. This will load the the
     * opporunity options from the server. If there are no more attendees then
     * the {@link currentAttendeeId} will be undefined.
     *
     * @returns A new CheckInSession object.
     */
    public withNextAttendee(): Promise<CheckInSession> {
        if (!this.selectedAttendeeIds || this.selectedAttendeeIds.length === 0) {
            throw new InvalidCheckInStateError("No selected attendees.");
        }

        if (!this.currentAttendeeId) {
            return this.withAttendee(this.selectedAttendeeIds[0]);
        }

        const currentIndex = this.selectedAttendeeIds
            .findIndex(a => a === this.currentAttendeeId);

        if (currentIndex === -1) {
            throw new InvalidCheckInStateError("Current selected attendee was not found.");
        }

        if (currentIndex + 1 < this.selectedAttendeeIds.length) {
            return this.withAttendee(this.selectedAttendeeIds[currentIndex + 1]);
        }
        else {
            return this.withAttendee(null);
        }
    }

    /**
     * Creates a new session by selecting the attendee that should be processed
     * for opportunity selection screens. This will load the the opporunity
     * options from the server.
     *
     * @param attendeeId The attendee identifier to be selected.
     *
     * @returns A new CheckInSession object.
     */
    public async withAttendee(attendeeId: string | null): Promise<CheckInSession> {
        if (!this.configuration.template || !this.configuration.kiosk) {
            throw new InvalidCheckInStateError("No configuration template or kiosk.");
        }

        if (!attendeeId) {
            return new CheckInSession(this, {
                currentAttendeeId: undefined,
                attendeeOpportunities: undefined
            });
        }

        const request: AttendeeOpportunitiesOptionsBag = {
            configurationTemplateId: this.configuration.template.id,
            kioskId: this.configuration.kiosk.id,
            areaIds: this.configuration.areas?.filter(a => !!a.id).map(a => a.id as string),
            familyId: this.currentFamilyId,
            personId: attendeeId
        };

        const response = await this.http.post<AttendeeOpportunitiesResponseBag>("/api/v2/checkin/AttendeeOpportunities", undefined, request);

        if (!response.isSuccess || !response.data?.opportunities) {
            throw new Error(response.errorMessage || UnexpectedErrorMessage);
        }

        if (this.configuration.template.kioskCheckInType === KioskCheckInMode.Family) {
            if (!this.possibleSchedules || this.possibleSchedules.length === 0) {
                throw new InvalidCheckInStateError("No schedules were available for check-in.");
            }

            return new CheckInSession(this, {
                currentAttendeeId: attendeeId,
                attendeeOpportunities: response.data.opportunities,
                selectedAbilityLevel: undefined,
                selectedArea: undefined,
                selectedGroup: undefined,
                selectedLocation: undefined,
                currentFamilyScheduleId: this.possibleSchedules[0].id
            });
        }

        const newPropertyValues: Mutable<CheckInSessionProperties> = {
            currentAttendeeId: attendeeId,
            attendeeOpportunities: response.data.opportunities,
            selectedAbilityLevel: undefined,
            selectedArea: undefined,
            selectedGroup: undefined,
            selectedLocation: undefined,
            selectedSchedules: undefined
        };

        // Set default selections if any items have only one choice.
        if (this.attendeeOpportunities) {
            if (this.attendeeOpportunities.abilityLevels?.length === 1) {
                newPropertyValues.selectedAbilityLevel = {
                    id: this.attendeeOpportunities.abilityLevels[0].id,
                    name: this.attendeeOpportunities.abilityLevels[0].name
                };
            }

            if (this.attendeeOpportunities.areas?.length === 1) {
                newPropertyValues.selectedArea = {
                    id: this.attendeeOpportunities.areas[0].id,
                    name: this.attendeeOpportunities.areas[0].name
                };
            }

            if (this.attendeeOpportunities.groups?.length === 1) {
                newPropertyValues.selectedGroup = {
                    id: this.attendeeOpportunities.groups[0].id,
                    name: this.attendeeOpportunities.groups[0].name
                };
            }

            if (this.attendeeOpportunities.locations?.length === 1) {
                newPropertyValues.selectedLocation = {
                    id: this.attendeeOpportunities.locations[0].id,
                    name: this.attendeeOpportunities.locations[0].name
                };
            }

            if (this.attendeeOpportunities.schedules?.length === 1) {
                newPropertyValues.selectedSchedules = [{
                    id: this.attendeeOpportunities.schedules[0].id,
                    name: this.attendeeOpportunities.schedules[0].name
                }];
            }
        }

        return new CheckInSession(this, newPropertyValues);
    }

    /**
     * Creates a new session by selecting the ability level for the currently
     * selected attendee.
     *
     * @param abilityLevelId The ability level identifier to be selected.
     *
     * @returns A new CheckInSession object.
     */
    public withSelectedAbilityLevel(abilityLevelId: string): CheckInSession {
        const abilityLevel = this.attendeeOpportunities
            ?.abilityLevels
            ?.find(a => a.id === abilityLevelId);

        if (!abilityLevel) {
            throw new Error("That ability level is not valid.");
        }

        return new CheckInSession(this, {
            selectedAbilityLevel: {
                id: abilityLevel.id,
                name: abilityLevel.name
            }
        });
    }

    /**
     * Creates a new session by selecting the area for the currently selected
     * attendee.
     *
     * @param areaId The area identifier to be selected.
     *
     * @returns A new CheckInSession object.
     */
    public withSelectedArea(areaId: string): CheckInSession {
        const area = this.attendeeOpportunities
            ?.areas
            ?.find(a => a.id === areaId);

        if (!area) {
            throw new Error("That area is not valid.");
        }

        return new CheckInSession(this, {
            selectedArea: {
                id: area.id,
                name: area.name
            }
        });
    }

    /**
     * Creates a new session by selecting the group for the currently
     * selected attendee.
     *
     * @param groupId The group identifier to be selected.
     *
     * @returns A new CheckInSession object.
     */
    public withSelectedGroup(groupId: string): CheckInSession {
        const group = this.attendeeOpportunities
            ?.groups
            ?.find(g => g.id === groupId);

        if (!group) {
            throw new Error("That group is not valid.");
        }

        return new CheckInSession(this, {
            selectedGroup: {
                id: group.id,
                name: group.name
            }
        });
    }

    /**
     * Creates a new session by selecting the location for the currently
     * selected attendee.
     *
     * @param locationId The location identifier to be selected.
     *
     * @returns A new CheckInSession object.
     */
    public withSelectedLocation(locationId: string): CheckInSession {
        const location = this.attendeeOpportunities
            ?.locations
            ?.find(g => g.id === locationId);

        if (!location) {
            throw new Error("That location is not valid.");
        }

        return new CheckInSession(this, {
            selectedLocation: {
                id: location.id,
                name: location.name
            }
        });
    }

    /**
     * Creates a new session by selecting the schedules for the currently
     * selected attendee.
     *
     * @param scheduleIds The schedule identifiers to be selected.
     *
     * @returns A new CheckInSession object.
     */
    public withSelectedSchedules(scheduleIds: string[]): CheckInSession {
        let schedules: ScheduleOpportunityBag[] | undefined;

        if (this.configuration.template?.kioskCheckInType === KioskCheckInMode.Individual) {
            schedules = this.attendeeOpportunities
                ?.schedules
                ?.filter(s => isAnyIdInList(s.id, scheduleIds));
        }
        else {
            schedules = this.possibleSchedules
                ?.filter(s => isAnyIdInList(s.id, scheduleIds));
        }

        if (!schedules || schedules.length === 0 || schedules.length !== scheduleIds.length) {
            throw new Error("Those times are not valid.");
        }

        return new CheckInSession(this, {
            selectedSchedules: schedules
                .map(s => ({
                    id: s.id,
                    name: s.name
                }))
        });
    }

    /**
     * Creates a new session that has removed all the stashed selections for
     * for the specified attendee.
     *
     * @param attendeeId The attendee identifier the selections to to be removed from.
     *
     * @returns A new CheckInSession object.
     */
    public withUnstashedAttendeeSelections(attendeeId: string): CheckInSession {
        const allSelections = clone(this.allAttendeeSelections);
        const attendeeIndex = allSelections.findIndex(s => s.attendeeId === attendeeId);

        if (attendeeIndex !== undefined) {
            allSelections.splice(attendeeIndex, 1);
        }

        return new CheckInSession(this, {
            allAttendeeSelections: allSelections
        });
    }

    /**
     * Creates a new session by append the selections for the current attendee.
     *
     * @param selections The selections for the current attendee.
     *
     * @returns A new CheckInSession object.
     */
    public withStashedCurrentAttendeeSelections(selections: OpportunitySelectionBag[]): CheckInSession {
        if (!this.currentAttendeeId) {
            throw new InvalidCheckInStateError("No attendee currently selected.");
        }

        return this.withStashedAttendeeSelections(this.currentAttendeeId, selections);
    }

    /**
     * Creates a new session by append the selections for the specified attendee.
     *
     * @param attendeeId The attendee identifier these selections are for.
     * @param selections The selections for the current attendee.
     *
     * @returns A new CheckInSession object.
     */
    public withStashedAttendeeSelections(attendeeId: string, selections: OpportunitySelectionBag[]): CheckInSession {
        const allSelections = clone(this.allAttendeeSelections);
        let attendeeSelection = allSelections.find(s => s.attendeeId === attendeeId);

        if (!attendeeSelection) {
            attendeeSelection = {
                attendeeId,
                selections
            };

            allSelections.push(attendeeSelection);
        }
        else {
            attendeeSelection.selections.push(...selections);
        }

        return new CheckInSession(this, {
            allAttendeeSelections: allSelections
        });
    }

    /**
     * Creats a new session that is ready to start accepting selections for
     * the current attendee at the next selected family schedule. If the new
     * session {@link currentFamilyScheduleId} property is undefined then this was
     * the last schedule.
     *
     * @returns A new CheckInSession object.
     */
    public withNextFamilySchedule(): CheckInSession {
        if (!this.selectedSchedules) {
            throw new InvalidCheckInStateError("No selected schedules.");
        }

        let nextScheduleId: string | undefined | null;

        if (!this.currentFamilyScheduleId) {
            nextScheduleId = this.selectedSchedules[0].id;
        }
        else {
            const currentScheduleIndex = this.selectedSchedules
                .findIndex(s => s.id === this.currentFamilyScheduleId);

            if (currentScheduleIndex === -1) {
                throw new InvalidCheckInStateError("Current schedule was not found.");
            }

            if (currentScheduleIndex + 1 < this.selectedSchedules.length) {
                nextScheduleId = this.selectedSchedules[currentScheduleIndex + 1].id;
            }
            else {
                nextScheduleId = undefined;
            }
        }
        return new CheckInSession(this, {
            selectedArea: undefined,
            selectedGroup: undefined,
            selectedLocation: undefined,
            currentFamilyScheduleId: nextScheduleId
        });
    }

    // #endregion

    // #region Public Functions

    /**
     * If the check-in session is in family mode and a family schedule is being
     * processed then append the schedule name to the text.
     *
     * @param text The text that will have the schedule name appended.
     *
     * @returns A new formatted string.
     */
    public appendScheduleName(text: string): string {
        if (this.configuration.template?.kioskCheckInType === KioskCheckInMode.Individual) {
            return text;
        }

        if (!this.possibleSchedules || !this.currentFamilyScheduleId) {
            return text;
        }

        const schedule = this.possibleSchedules.find(s => s.id === this.currentFamilyScheduleId);

        if (!schedule) {
            return text;
        }

        return `${text} @ ${schedule.name}`;
    }

    /** The current family that is being worked with for this session. */
    public getCurrentFamily(): FamilyBag | undefined {
        return this.families?.find(f => f.id === this.currentFamilyId);
    }

    /** The current attendee that is being worked with while making selections. */
    public getCurrentAttendee(): AttendeeBag | undefined {
        return this.attendees?.find(f => f.person?.id === this.currentAttendeeId);
    }

    /**
     * Gets the ability level opportunities that are available for selection
     * on the currently selected attendee.
     *
     * @returns An array of ability level opportunities.
     */
    public getAvailableAbilityLevels(): AbilityLevelOpportunityBag[] {
        if (!this.attendeeOpportunities?.abilityLevels) {
            return [];
        }

        return this.attendeeOpportunities.abilityLevels;
    }

    /**
     * Gets the area opportunities that are available for selection on the
     * currently selected attendee.
     *
     * @returns An array of area opportunities.
     */
    public getAvailableAreas(): AreaOpportunityBag[] {
        if (!this.attendeeOpportunities?.areas) {
            return [];
        }

        if (this.configuration.template?.kioskCheckInType === KioskCheckInMode.Individual) {
            return this.attendeeOpportunities.areas;
        }

        // In family mode we need to filter the areas by the selected schedules
        // which means we need to start by getting the locations that are
        // valid for that schedule.
        const selectedScheduleIds = this.selectedSchedules?.map(s => s.id as string);
        const validLocationIds = this.attendeeOpportunities
            .locations
            ?.filter(l => isAnyIdInList(selectedScheduleIds, l.scheduleIds))
            .map(l => l.id as string) ?? [];

        // Now find all groups for those locations.
        const validAreaIds = this.attendeeOpportunities
            .groups
            ?.filter(g => isAnyIdInList(validLocationIds, g.locationIds))
            .map(g => g.areaId as string) ?? [];

        // Now we can find the areas
        return this.attendeeOpportunities
            .areas
            .filter(a => a.id && validAreaIds.includes(a.id));
    }

    /**
     * Gets the group opportunities that are available for selection on the
     * currently selected attendee.
     *
     * @returns An array of group opportunities.
     */
    public getAvailableGroups(): GroupOpportunityBag[] {
        if (!this.attendeeOpportunities?.groups) {
            return [];
        }

        if (this.configuration.template?.kioskCheckInType === KioskCheckInMode.Individual) {
            // In individual mode we need to filter the groups by the selected
            // area.
            return this.attendeeOpportunities
                .groups
                .filter(g => g.areaId === this.selectedArea?.id);
        }

        // In family mode we need to filter the areas by the selected schedule
        // and the selected area. Which means we need to start by getting the
        // locations that are valid for that schedule.
        const selectedScheduleIds = this.selectedSchedules?.map(s => s.id as string);
        const validLocationIds = this.attendeeOpportunities
            .locations
            ?.filter(l => isAnyIdInList(selectedScheduleIds, l.scheduleIds))
            .map(l => l.id as string) ?? [];

        // Now find all groups for those locations and the selected area.
        return this.attendeeOpportunities
            .groups
            .filter(g => isAnyIdInList(validLocationIds, g.locationIds))
            .filter(g => g.areaId === this.selectedArea?.id);
    }

    /**
     * Gets the location opportunities that are available for selection on the
     * currently selected attendee.
     *
     * @returns An array of location opportunities.
     */
    public getAvailableLocations(): LocationOpportunityBag[] {
        if (!this.attendeeOpportunities?.locations) {
            return [];
        }

        const group = this.attendeeOpportunities
            .groups
            ?.find(g => g.id === this.selectedGroup?.id);

        if (!group) {
            return [];
        }

        if (this.configuration.template?.kioskCheckInType === KioskCheckInMode.Individual) {
            // In individual mode we need to filter the locations by the selected
            // group.
            return this.attendeeOpportunities
                .locations
                .filter(l => isAnyIdInList(l.id, group.locationIds));
        }

        // In family mode we need to filter the locations by the selected schedules
        // and the selected group.
        const selectedScheduleIds = this.selectedSchedules?.map(s => s.id as string);

        return this.attendeeOpportunities
            .locations
            .filter(l => isAnyIdInList(l.id, group.locationIds))
            .filter(l => isAnyIdInList(selectedScheduleIds, l.scheduleIds));
    }

    /**
     * Gets the schedule opportunities that are available for selection on the
     * currently selected attendee.
     *
     * @returns An array of schedule opportunities.
     */
    public getAvailableSchedules(): ScheduleOpportunityBag[] {
        if (this.configuration.template?.kioskCheckInType === KioskCheckInMode.Individual) {
            if (!this.attendeeOpportunities?.schedules) {
                return [];
            }

            // In individual mode we need to filter the schedules by the selected
            // location.
            const location = this.attendeeOpportunities
                .locations
                ?.find(l => l.id === this.selectedLocation?.id);

            if (!location) {
                return [];
            }

            return this.attendeeOpportunities
                .schedules
                .filter(s => isAnyIdInList(s.id, location.scheduleIds));
        }

        // In family mode we are the first step so we just show everything.
        return this.possibleSchedules ?? [];
    }

    /**
     * Gets the selections that have previously been made for the specified
     * attendee.
     *
     * @param attendeeId The attendee identifier.
     *
     * @returns The collection of selections made for this attendee.
     */
    public getAttendeeSelections(attendeeId: string): OpportunitySelectionBag[] {
        const selections = this.allAttendeeSelections
            .find(s => s.attendeeId === attendeeId);

        if (!selections) {
            return [];
        }

        return clone(selections.selections) as OpportunitySelectionBag[];
    }

    /**
     * Gets the current selections represented by this session. In individual
     * check-in mode this will be the selections for the current attendee and
     * will include multiple items if multiple schedules were selected. In
     * family check-in mode this will be the selections for the current attendee
     * but will only contain one item for the schedule currently being processed.
     *
     * @returns An array of selection bags.
     */
    public getCurrentSelections(): OpportunitySelectionBag[] {
        const selections: OpportunitySelectionBag[] = [];

        if (!this.selectedArea) {
            throw new InvalidCheckInStateError("No area selected.");
        }

        if (!this.selectedGroup) {
            throw new InvalidCheckInStateError("No group selected.");
        }

        if (!this.selectedLocation) {
            throw new InvalidCheckInStateError("No location selected.");
        }

        if (this.configuration.template?.kioskCheckInType === KioskCheckInMode.Family) {
            const schedule = this.possibleSchedules
                ?.find(s => s.id === this.currentFamilyScheduleId);

            if (!schedule) {
                throw new InvalidCheckInStateError("No schedule currently selected.");
            }

            selections.push({
                abilityLevel: this.selectedAbilityLevel,
                area: this.selectedArea,
                group: this.selectedGroup,
                location: this.selectedLocation,
                schedule: schedule
            });
        }
        else {
            if (!this.selectedSchedules || this.selectedSchedules.length === 0) {
                throw new InvalidCheckInStateError("No schedules selected.");
            }

            for (const schedule of this.selectedSchedules) {
                selections.push({
                    abilityLevel: this.selectedAbilityLevel,
                    area: this.selectedArea,
                    group: this.selectedGroup,
                    location: this.selectedLocation,
                    schedule: schedule
                });
            }
        }

        return selections;
    }

    /**
     * Determines if our state reflects that we are processing the first
     * selected family schedule.
     *
     * @returns true if we are currently processing the first family schedule.
     */
    public isProcessingFirstFamilySchedule(): boolean {
        const scheduleIndex = this.possibleSchedules
            ?.findIndex(s => s.id === this.currentFamilyScheduleId);

        return scheduleIndex === 0;
    }

    // #endregion

    // #region Screen Switch Functions

    /**
     * Creates a new session that has been updated to reflect the next screen
     * after the current screen.
     *
     * @returns A new CheckInSession object.
     */
    public withNextScreen(): Promise<CheckInSession> {
        if (this.currentScreen === Screen.Welcome) {
            return this.withNextScreenFromWelcome();
        }
        else if (this.currentScreen === Screen.Search) {
            return this.withNextScreenFromSearch();
        }
        else if (this.currentScreen === Screen.FamilySelect) {
            return this.withNextScreenFromFamilySelect();
        }
        else if (this.currentScreen === Screen.ActionSelect) {
            return this.withNextScreenFromActionSelect();
        }
        else if (this.currentScreen === Screen.CheckoutSelect) {
            return this.withNextScreenFromCheckoutSelect();
        }
        else if (this.currentScreen === Screen.PersonSelect) {
            return this.withNextScreenFromPersonSelect();
        }
        else if (this.currentScreen === Screen.AutoModeOpportunitySelect) {
            return this.withNextScreenFromAutoModeOpportunitySelect();
        }
        else if (this.currentScreen === Screen.AbilityLevelSelect) {
            return this.withNextScreenFromAbilityLevelSelect();
        }
        else if (this.currentScreen === Screen.AreaSelect) {
            return this.withNextScreenFromAreaSelect();
        }
        else if (this.currentScreen === Screen.GroupSelect) {
            return this.withNextScreenFromGroupSelect();
        }
        else if (this.currentScreen === Screen.LocationSelect) {
            return this.withNextScreenFromLocationSelect();
        }
        else if (this.currentScreen === Screen.ScheduleSelect) {
            return this.withNextScreenFromScheduleSelect();
        }
        else if (this.currentScreen === Screen.Success) {
            return this.withNextScreenFromSuccess();
        }
        else {
            return Promise.resolve(this.withScreen(Screen.Welcome));
        }
    }

    /**
     * Creates a new session that has been updated to reflect the next screen
     * after the welcome screen.
     *
     * @returns A new CheckInSession object.
     */
    private withNextScreenFromWelcome(): Promise<CheckInSession> {
        if (!this.families) {
            return Promise.resolve(this.withScreen(Screen.Search));
        }
        else if (this.families.length === 1) {
            return Promise.resolve(this.withScreen(Screen.PersonSelect));
        }

        return Promise.resolve(this.withScreen(Screen.FamilySelect));
    }

    /**
     * Creates a new session that has been updated to reflect the next screen
     * after the search screen.
     *
     * @returns A new CheckInSession object.
     */
    private withNextScreenFromSearch(): Promise<CheckInSession> {
        if (!this.families) {
            return Promise.resolve(this.withScreen(Screen.Welcome));
        }

        // Always show family select even if only one family when
        // coming from the search screen.
        return Promise.resolve(this.withScreen(Screen.FamilySelect));
    }

    /**
     * Creates a new session that has been updated to reflect the next screen
     * after the family select screen.
     *
     * @returns A new CheckInSession object.
     */
    private withNextScreenFromFamilySelect(forceCheckin: boolean = false): Promise<CheckInSession> {
        if (!this.getCurrentFamily() || !this.attendees) {
            return Promise.resolve(this.withScreen(Screen.Welcome));
        }
        else if (!forceCheckin && this.currentlyCheckedIn && this.currentlyCheckedIn.length > 0) {
            return Promise.resolve(this.withScreen(Screen.ActionSelect));
        }

        const isFamilyAutoMode = this.configuration.template?.kioskCheckInType === KioskCheckInMode.Family
            && this.configuration.template?.isAutoSelect;

        if (isFamilyAutoMode && this.attendees) {
            const newAttendeeSelections = clone(this.allAttendeeSelections);

            for (const attendee of this.attendees) {
                if (!attendee.selectedOpportunities || !attendee.person?.id) {
                    continue;
                }

                newAttendeeSelections.push({
                    attendeeId: attendee.person.id,
                    selections: attendee.selectedOpportunities
                });
            }

            const copy = new CheckInSession(this, {
                allAttendeeSelections: newAttendeeSelections
            });

            return Promise.resolve(copy.withScreen(Screen.PersonSelect));
        }

        return Promise.resolve(this.withScreen(Screen.PersonSelect));
    }

    /**
     * Creates a new session that has been updated to reflect the next screen
     * after the action select screen.
     *
     * @returns A new CheckInSession object.
     */
    private withNextScreenFromActionSelect(): Promise<CheckInSession> {
        if (this.isCheckoutAction) {
            if (this.currentlyCheckedIn && this.currentlyCheckedIn.length > 0) {
                return Promise.resolve(this.withScreen(Screen.CheckoutSelect));
            }
            else {
                return Promise.resolve(this.withScreen(Screen.Welcome));
            }
        }

        return this.withNextScreenFromFamilySelect(true);
    }

    /**
     * Creates a new session that has been updated to reflect the next screen
     * after the checkout select screen.
     *
     * @returns A new CheckInSession object.
     */
    private withNextScreenFromCheckoutSelect(): Promise<CheckInSession> {
        return Promise.resolve(this.withScreen(Screen.CheckoutSuccess));
    }

    /**
     * Creates a new session that has been updated to reflect the next screen
     * after the person select screen.
     *
     * @returns A new CheckInSession object.
     */
    private async withNextScreenFromPersonSelect(): Promise<CheckInSession> {
        if (this.configuration.template?.kioskCheckInType == KioskCheckInMode.Family) {
            if (this.configuration.template?.isAutoSelect) {
                if (this.currentAttendeeId) {
                    return this.withScreen(Screen.AutoModeOpportunitySelect);
                }
                else {
                    // Family check-in in auto-select mode saves everybody at
                    // once. Perform a full save of the attendance.
                    const newSessionFamily = await this.withSaveAttendance(false);

                    return newSessionFamily.withScreen(Screen.Success);
                }
            }

            if (!this.possibleSchedules || this.possibleSchedules.length === 0) {
                throw new Error("Nothing available to check-in to.");
            }

            if (this.possibleSchedules.length > 1) {
                return this.withScreen(Screen.ScheduleSelect);
            }

            return await this.withSelectedSchedules(this.possibleSchedules.map(s => s.id as string))
                .withNextScreenFromScheduleSelect();
        }

        const abilityLevels = this.getAvailableAbilityLevels();
        let newSession = new CheckInSession(this, {});

        // We should be either family mode that is updating an attendee or
        // individual mode with an attendee.
        if (!newSession.currentAttendeeId) {
            throw new InvalidCheckInStateError("No current attendee selected.");
        }

        // If an ability level is not already selected then try to select
        // one if there is a single option to pick from.
        if (!newSession.selectedAbilityLevel) {
            if (abilityLevels.length === 1) {
                newSession = new CheckInSession(newSession, {
                    selectedAbilityLevel: {
                        id: abilityLevels[0].id,
                        name: abilityLevels[0].name
                    }
                });
            }
        }

        if (this.configuration.template?.abilityLevelDetermination === AbilityLevelDeterminationMode.DoNotAsk) {
            return newSession.withNextScreenFromAbilityLevelSelect();
        }

        // If we have more than 1 ability level to pick from then show the
        // ability level screen.
        if (abilityLevels.length > 1) {
            return newSession.withScreen(Screen.AbilityLevelSelect);
        }

        return await newSession.withNextScreenFromAbilityLevelSelect();
    }

    /**
     * Creates a new session that has been updated to reflect the next screen
     * after the auto mode opportunity select screen.
     *
     * @returns A new CheckInSession object.
     */
    private withNextScreenFromAutoModeOpportunitySelect(): Promise<CheckInSession> {
        return Promise.resolve(this.withScreen(Screen.PersonSelect));
    }

    /**
     * Creates a new session that has been updated to reflect the next screen
     * after the ability level select screen.
     *
     * @returns A new CheckInSession object.
     */
    private withNextScreenFromAbilityLevelSelect(): Promise<CheckInSession> {
        let newSession = new CheckInSession(this, {});

        // We should be either family mode that is updating an attendee or
        // individual mode with an attendee.
        if (!newSession.currentAttendeeId) {
            throw new InvalidCheckInStateError("No current attendee selected.");
        }

        const areas = this.getAvailableAreas();

        // If an area is not already selected then try to select
        // one if there is a single option to pick from.
        if (!newSession.selectedArea) {
            if (areas.length === 1) {
                newSession = new CheckInSession(newSession, {
                    selectedArea: {
                        id: areas[0].id,
                        name: areas[0].name
                    }
                });
            }
        }

        // If we have more than 1 area to pick from then show the area screen.
        if (areas.length > 1) {
            return Promise.resolve(newSession.withScreen(Screen.AreaSelect));
        }

        return newSession.withNextScreenFromAreaSelect();
    }

    /**
     * Creates a new session that has been updated to reflect the next screen
     * after the area select screen.
     *
     * @returns A new CheckInSession object.
     */
    private withNextScreenFromAreaSelect(): Promise<CheckInSession> {
        let newSession = new CheckInSession(this, {});

        // We should be either family mode that is updating an attendee or
        // individual mode with an attendee.
        if (!newSession.currentAttendeeId) {
            throw new InvalidCheckInStateError("No current attendee selected.");
        }

        const groups = this.getAvailableGroups();

        // If a group is not already selected then try to select
        // one if there is a single option to pick from.
        if (!newSession.selectedGroup) {
            if (groups.length === 1) {
                newSession = new CheckInSession(newSession, {
                    selectedGroup: {
                        id: groups[0].id,
                        name: groups[0].name
                    }
                });
            }
        }

        // If we have more than 1 group to pick from then show the group screen.
        if (groups.length > 1) {
            return Promise.resolve(newSession.withScreen(Screen.GroupSelect));
        }

        return newSession.withNextScreenFromGroupSelect();
    }

    /**
     * Creates a new session that has been updated to reflect the next screen
     * after the groups elect screen.
     *
     * @returns A new CheckInSession object.
     */
    private withNextScreenFromGroupSelect(): Promise<CheckInSession> {
        let newSession = new CheckInSession(this, {});

        // We should be either family mode that is updating an attendee or
        // individual mode with an attendee.
        if (!newSession.currentAttendeeId) {
            throw new InvalidCheckInStateError("No current attendee selected.");
        }

        const locations = this.getAvailableLocations();

        // If a location is not already selected then try to select
        // one if there is a single option to pick from.
        if (!newSession.selectedLocation) {
            if (locations.length === 1) {
                newSession = new CheckInSession(newSession, {
                    selectedLocation: {
                        id: locations[0].id,
                        name: locations[0].name
                    }
                });
            }
        }

        // If we have more than 1 location to pick from then show the location screen.
        if (locations.length > 1) {
            return Promise.resolve(newSession.withScreen(Screen.LocationSelect));
        }

        return newSession.withNextScreenFromLocationSelect();
    }

    /**
     * Creates a new session that has been updated to reflect the next screen
     * after the location select screen.
     *
     * @returns A new CheckInSession object.
     */
    private async withNextScreenFromLocationSelect(): Promise<CheckInSession> {
        if (this.configuration.template?.kioskCheckInType === KioskCheckInMode.Family) {
            // Get all the selections made and stash them so they can be saved
            // later.
            const selections = this.getCurrentSelections();
            let familySession = this.withStashedCurrentAttendeeSelections(selections);

            familySession = familySession.withNextFamilySchedule();

            // Was this the last schedule for this attendee?
            if (!familySession.currentFamilyScheduleId) {
                // Save the stashed attendance as pending and move to the
                // next attendee.
                familySession = await familySession.withSaveAttendance(true);
                familySession = await familySession.withNextAttendee();
            }

            // Was this the last attendee?
            if (!familySession.currentAttendeeId) {
                familySession = await familySession.withConfirmAttendance();

                return familySession.withScreen(Screen.Success);
            }

            // If this is not the first family schedule then skip the ability
            // level screen as we only need to ask once per attendee.
            if (!familySession.isProcessingFirstFamilySchedule()) {
                return familySession.withNextScreenFromAbilityLevelSelect();
            }

            // If there are no ability levels configured then skip that screen.
            if (familySession.getAvailableAbilityLevels().length === 0) {
                return familySession.withNextScreenFromAbilityLevelSelect();
            }

            // If there are no groups that filter by ability level then we
            // can also skip the ability levle screen.
            const groupsWithAbilityLevels = familySession.attendeeOpportunities
                ?.groups
                ?.filter(g => !!g.abilityLevelId) ?? [];

            if (groupsWithAbilityLevels.length === 0) {
                return familySession.withNextScreenFromAbilityLevelSelect();
            }

            if (this.configuration.template.abilityLevelDetermination === AbilityLevelDeterminationMode.DoNotAsk) {
                return familySession.withNextScreenFromAbilityLevelSelect();
            }

            return familySession.withScreen(Screen.AbilityLevelSelect);
        }

        if ((this.attendeeOpportunities?.schedules?.length ?? 0) > 1) {
            return this.withScreen(Screen.ScheduleSelect);
        }

        if (!this.attendeeOpportunities?.schedules || this.attendeeOpportunities.schedules.length === 0) {
            throw new InvalidCheckInStateError("No schedules available.");
        }

        if (!this.attendeeOpportunities.schedules[0].id) {
            throw new InvalidCheckInStateError("Invalid schedule.");
        }

        const scheduleIds = [this.attendeeOpportunities.schedules[0].id];
        const individualSession = await this.withSelectedSchedules(scheduleIds);

        return await individualSession.withNextScreenFromScheduleSelect();
    }

    /**
     * Creates a new session that has been updated to reflect the next screen
     * after the schedule select screen.
     *
     * @returns A new CheckInSession object.
     */
    private async withNextScreenFromScheduleSelect(): Promise<CheckInSession> {
        if (this.configuration.template?.kioskCheckInType === KioskCheckInMode.Family) {
            if (!this.selectedAttendeeIds || this.selectedAttendeeIds.length === 0) {
                throw new InvalidCheckInStateError("Nobody has been selected for check-in.");
            }

            const familySession = await this.withAttendee(this.selectedAttendeeIds[0]);

            // If there are no ability levels configured then skip that screen.
            if (familySession.getAvailableAbilityLevels().length === 0) {
                return familySession.withNextScreenFromAbilityLevelSelect();
            }

            // If there are no groups that filter by ability level then we
            // can also skip the ability levle screen.
            const groupsWithAbilityLevels = familySession.attendeeOpportunities
                ?.groups
                ?.filter(g => !!g.abilityLevelId) ?? [];

            if (groupsWithAbilityLevels.length === 0) {
                return familySession.withNextScreenFromAbilityLevelSelect();
            }

            if (this.configuration.template.abilityLevelDetermination === AbilityLevelDeterminationMode.DoNotAsk) {
                return familySession.withNextScreenFromAbilityLevelSelect();
            }

            return familySession.withScreen(Screen.AbilityLevelSelect);
        }

        const selections = this.getCurrentSelections();

        let newSession = this.withStashedCurrentAttendeeSelections(selections);

        // Individual check-in. Perform a full save of the attendance.
        newSession = await newSession.withSaveAttendance(false);

        return newSession.withScreen(Screen.Success);
    }

    /**
     * Creates a new session that has been updated to reflect the next screen
     * after the success screen.
     *
     * @returns A new CheckInSession object.
     */
    private withNextScreenFromSuccess(): Promise<CheckInSession> {
        return Promise.resolve(this.withScreen(Screen.Welcome));
    }

    // #endregion
}
