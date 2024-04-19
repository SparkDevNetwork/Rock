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
import { areEqual, newGuid } from "@Obsidian/Utility/guid";
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
import { FamilyMembersOptionsBag } from "@Obsidian/ViewModels/Rest/CheckIn/familyMembersOptionsBag";
import { FamilyMembersResponseBag } from "@Obsidian/ViewModels/Rest/CheckIn/familyMembersResponseBag";
import { SaveAttendanceOptionsBag } from "@Obsidian/ViewModels/Rest/CheckIn/saveAttendanceOptionsBag";
import { SaveAttendanceResponseBag } from "@Obsidian/ViewModels/Rest/CheckIn/saveAttendanceResponseBag";
import { SearchForFamiliesOptionsBag } from "@Obsidian/ViewModels/Rest/CheckIn/searchForFamiliesOptionsBag";
import { SearchForFamiliesResponseBag } from "@Obsidian/ViewModels/Rest/CheckIn/searchForFamiliesResponseBag";
import { DeepReadonly } from "vue";
import { Screen, SessionOpportunitySelectionBag } from "./types.partial";
import { InvalidCheckInStateError, UnexpectedErrorMessage, clone, invalidCheckInStateMessage, isGuidInList } from "./utils.partial";
import { AttendanceRequestBag } from "@Obsidian/ViewModels/CheckIn/attendanceRequestBag";
import { RecordedAttendanceBag } from "@Obsidian/ViewModels/CheckIn/recordedAttendanceBag";
import { OpportunitySelectionBag } from "@Obsidian/ViewModels/CheckIn/opportunitySelectionBag";

/**
 * Handles the heavy logic for a single check-in session. A session begins when
 * a search operation is initiated and ends when the welcome screen is displayed.
 * So a session can handle a single person check-in or a whole family check-in.
 */
export class CheckInSession {
    // #region Fields

    /* eslint-disable @typescript-eslint/naming-convention */
    // This is against our naming conventions, but for the moment I can't find
    // a better way to make these read-only and also have them show up as
    // properties when used with console.log. -DSH

    /** The current screen that should be displayed for this session. */
    private _currentScreen: Screen;

    /** The search term used to start this session. */
    private _searchTerm?: string;

    /** The search type used to start this session. */
    private _searchType?: FamilySearchMode;

    /** The families that were matched by the search term for this session. */
    private _families?: FamilyBag[];

    /** The currently selected family unique identifier. */
    private _currentFamilyGuid?: Guid;

    /** The currently checked in attendance records. */
    private _currentlyCheckedIn?: AttendanceBag[];

    /** The potential attendees that can be checked in. */
    private _attendees?: AttendeeBag[];

    /** The attendee unique identifiers that were selected to be checked in. */
    private _selectedAttendeeGuids?: Guid[];

    /** The currently selected attendee unique identifier. */
    private _currentAttendeeGuid?: Guid;

    /** The available opportunities for the currently selected attendee. */
    private _attendeeOpportunities?: OpportunityCollectionBag;

    /** The currently selected opportunities for the current attendee. */
    private _currentOpportunitySelection?: SessionOpportunitySelectionBag;

    /**
     * All selections made for attendees. This is the set of selections that
     * will be converted to attendance records during save.
     */
    private _allAttendeeSelections: { attendeeGuid: Guid, selections: DeepReadonly<OpportunitySelectionBag[]> }[] = [];

    /** The attendance records that have been sent to the server and saved. */
    private _attendances: RecordedAttendanceBag[] = [];

    /* eslint-enable @typescript-eslint/naming-convention */

    /**
     * The unique identifer for this check-in session. This is used to link
     * all attendance records together since we might save them at different
     * times.
     */
    private readonly sessionGuid: Guid;

    /** The kiosk configuration that this session will conform to. */
    private readonly configuration: KioskConfigurationBag;

    /** The object to use when making HTTP requests to the server. */
    private readonly http: HttpFunctions;

    // #endregion

    // #region Field Getters

    /** The current screen that should be displayed for this session. */
    public get currentScreen(): Screen {
        return this._currentScreen;
    }

    /** The search term used to start this session. */
    public get searchTerm(): string | undefined {
        return this._searchTerm;
    }

    /** The search type used to start this session. */
    public get searchType(): FamilySearchMode | undefined {
        return this._searchType;
    }

    /** The families that were matched by the search term for this session. */
    public get families(): DeepReadonly<FamilyBag[]> | undefined {
        return this._families;
    }

    /** The currently selected family unique identifier. */
    public get currentFamilyGuid(): Guid | undefined {
        return this._currentFamilyGuid;
    }

    /** The current family that is being worked with for this session. */
    public get currentFamily(): DeepReadonly<FamilyBag> | undefined {
        return this._families?.find(f => areEqual(f.guid, this._currentFamilyGuid));
    }

    /** The currently checked in attendance records. */
    public get currentlyCheckedIn(): DeepReadonly<AttendanceBag[]> | undefined {
        return this._currentlyCheckedIn;
    }

    /** The potential attendees that can be checked in. */
    public get attendees(): DeepReadonly<AttendeeBag[]> | undefined {
        return this._attendees;
    }

    /** The attendee unique identifiers that were selected to be checked in. */
    public get selectedAttendeeGuids(): Guid[] | undefined {
        return this._selectedAttendeeGuids;
    }

    /** The currently selected attendee unique identifier. */
    public get currentAttendeeGuid(): Guid | undefined {
        return this._currentAttendeeGuid;
    }

    /** The current attendee that is being worked with while making selections. */
    public get currentAttendee(): DeepReadonly<AttendeeBag> | undefined {
        return this._attendees?.find(f => areEqual(f.person?.guid, this._currentAttendeeGuid));
    }

    /** The available opportunities for the currently selected attendee. */
    public get attendeeOpportunities(): DeepReadonly<OpportunityCollectionBag> | undefined {
        return this._attendeeOpportunities;
    }

    /** The currently selected opportunities for the current attendee. */
    public get currentOpportunitySelection(): DeepReadonly<SessionOpportunitySelectionBag> | undefined {
        return this._currentOpportunitySelection;
    }

    /** The attendance records that have been saved during this session. */
    public get attendances(): DeepReadonly<RecordedAttendanceBag[]> | undefined {
        return this._attendances;
    }

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
    public constructor(configuration: KioskConfigurationBag, http: HttpFunctions, sessionGuid?: Guid) {
        this._currentScreen = Screen.Welcome;
        this.configuration = configuration;
        this.http = http;
        this.sessionGuid = sessionGuid ?? newGuid();
    }

    // #endregion

    // #region Private Support Functions

    /**
     * Creates a new session object that is identical to the original. This is
     * designed to make them identical but separate, meaning you can make any
     * change you want to the new one and it won't affect the old one. This means
     * all array and object properties are also cloned.
     *
     * @param configuration The optional configuration to use when creating the session.
     *
     * @returns A new session object that is identical to the original.
     */
    private clone(configuration?: KioskConfigurationBag): CheckInSession {
        const copy = new CheckInSession(configuration ?? this.configuration, this.http, this.sessionGuid);

        copy._currentScreen = this._currentScreen;
        copy._searchTerm = this._searchTerm;
        copy._searchType = this._searchType;
        copy._families = clone(this._families);
        copy._currentFamilyGuid = this._currentFamilyGuid;
        copy._attendees = clone(this._attendees);
        copy._currentlyCheckedIn = clone(this._currentlyCheckedIn);
        copy._selectedAttendeeGuids = clone(this._selectedAttendeeGuids);
        copy._currentAttendeeGuid = this._currentAttendeeGuid;
        copy._attendeeOpportunities = clone(this._attendeeOpportunities);
        copy._currentOpportunitySelection = clone(this._currentOpportunitySelection);
        copy._allAttendeeSelections = clone(this._allAttendeeSelections);
        copy._attendances = clone(this._attendances);

        return copy;
    }

    /**
     * Creates a new check-in session object that will display the specified
     * screen.
     *
     * @returns A cloned check-in session object.
     */
    private withScreen(screen: Screen): CheckInSession {
        const copy = this.clone();

        copy._currentScreen = screen;

        return copy;
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
        if (!this.selectedAttendeeGuids) {
            throw new InvalidCheckInStateError("No individuals were selected.");
        }

        const attendanceRequests: AttendanceRequestBag[] = [];

        for (const attendeeSelections of this._allAttendeeSelections) {
            // Skip this attendee's selections if they aren't selected for
            // check-in.
            if (!this.selectedAttendeeGuids.some(a => areEqual(a, attendeeSelections.attendeeGuid))) {
                continue;
            }

            for (const selection of attendeeSelections.selections) {
                const attendance: AttendanceRequestBag = {
                    personGuid: attendeeSelections.attendeeGuid,
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
            kioskGuid: this.configuration.kiosk?.guid,
            templateGuid: this.configuration.template.guid,
            session: {
                guid: this.sessionGuid,
                isPending: isPending,
                searchMode: this.searchType,
                searchTerm: this.searchTerm,
                familyGuid: this.currentFamilyGuid
            },
            requests: attendanceRequests
        };

        const response = await this.http.post<SaveAttendanceResponseBag>("/api/v2/checkin/SaveAttendance", undefined, request);

        if (!response.isSuccess || !response.data?.attendances) {
            throw new Error(response.errorMessage || UnexpectedErrorMessage);
        }

        const clone = this.clone();

        clone._attendances.push(...response.data.attendances);
        console.log("success", response.data.attendances);

        return clone;
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
        return this.clone(configuration);
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
            throw new Error(invalidCheckInStateMessage);
        }

        const request: SearchForFamiliesOptionsBag = {
            configurationTemplateGuid: this.configuration.template?.guid,
            kioskGuid: this.configuration.kiosk?.guid,
            prioritizeKioskCampus: false,
            searchTerm: searchTerm,
            searchType: searchType
        };

        const response = await this.http.post<SearchForFamiliesResponseBag>("/api/v2/checkin/SearchForFamilies", undefined, request);

        if (!response.isSuccess || !response.data?.families) {
            throw new Error(response.errorMessage || UnexpectedErrorMessage);
        }

        const copy = this.clone();

        copy._families = response.data.families;
        copy._searchTerm = searchTerm;
        copy._searchType = searchType;

        return copy;
    }

    /**
     * Creates a new session by selecting the specified family. This will load
     * the family members for the selected family.
     *
     * @param familyGuid The family identifier to be selected.
     *
     * @returns A new CheckInSession object.
     */
    public async withFamily(familyGuid: Guid): Promise<CheckInSession> {
        if (!this.configuration.template || !this.configuration.kiosk) {
            throw new Error(invalidCheckInStateMessage);
        }

        const request: FamilyMembersOptionsBag = {
            configurationTemplateGuid: this.configuration.template.guid,
            kioskGuid: this.configuration.kiosk.guid,
            areaGuids: this.configuration.areas?.filter(a => !!a.guid).map(a => a.guid as string),
            familyGuid: familyGuid
        };

        const response = await this.http.post<FamilyMembersResponseBag>("/api/v2/checkin/FamilyMembers", undefined, request);

        if (!response.isSuccess || !response.data?.people) {
            throw new Error(response.errorMessage || UnexpectedErrorMessage);
        }

        const copy = this.clone();

        copy._currentFamilyGuid = familyGuid;
        copy._attendees = response.data.people;
        copy._currentlyCheckedIn = response.data.currentlyCheckedInAttendances ?? [];

        return copy;
    }

    /**
     * Creates a new session by selecting the attendees that should be processed
     * in family check-in mode.
     *
     * @param attendeeGuids The attendee identifiers that should be selected.
     *
     * @returns A new CheckInSession object.
     */
    public withSelectedAttendees(attendeeGuids: Guid[]): CheckInSession {
        const copy = this.clone();

        copy._selectedAttendeeGuids = attendeeGuids;

        return copy;
    }

    /**
     * Creates a new session by selecting the attendee that should be processed
     * for opportunity selection screens. This will load the the opporunity
     * options from the server.
     *
     * @param attendeeGuid The attendee identifier to be selected.
     *
     * @returns A new CheckInSession object.
     */
    public async withAttendee(attendeeGuid: Guid | null): Promise<CheckInSession> {
        if (!this.configuration.template || !this.configuration.kiosk) {
            throw new Error(invalidCheckInStateMessage);
        }

        if (attendeeGuid === null) {
            const emptyAttendeeSession = this.clone();

            emptyAttendeeSession._currentAttendeeGuid = undefined;
            emptyAttendeeSession._attendeeOpportunities = undefined;

            return emptyAttendeeSession;
        }

        const request: AttendeeOpportunitiesOptionsBag = {
            configurationTemplateGuid: this.configuration.template.guid,
            kioskGuid: this.configuration.kiosk.guid,
            areaGuids: this.configuration.areas?.filter(a => !!a.guid).map(a => a.guid as string),
            familyGuid: this.currentFamilyGuid,
            personGuid: attendeeGuid
        };

        const response = await this.http.post<AttendeeOpportunitiesResponseBag>("/api/v2/checkin/AttendeeOpportunities", undefined, request);

        if (!response.isSuccess || !response.data?.opportunities) {
            throw new Error(response.errorMessage || UnexpectedErrorMessage);
        }

        const copy = this.clone();

        copy._currentAttendeeGuid = attendeeGuid;
        copy._attendeeOpportunities = response.data.opportunities;

        if (this.configuration.template.kioskCheckInType === KioskCheckInMode.Individual) {
            copy._currentOpportunitySelection = {};

            // Set default selections if any items have only one choice.
            if (this.attendeeOpportunities) {
                if (this.attendeeOpportunities.abilityLevels?.length === 1) {
                    copy._currentOpportunitySelection.abilityLevel = {
                        guid: this.attendeeOpportunities.abilityLevels[0].guid,
                        name: this.attendeeOpportunities.abilityLevels[0].name
                    };
                }

                if (this.attendeeOpportunities.areas?.length === 1) {
                    copy._currentOpportunitySelection.area = {
                        guid: this.attendeeOpportunities.areas[0].guid,
                        name: this.attendeeOpportunities.areas[0].name
                    };
                }

                if (this.attendeeOpportunities.groups?.length === 1) {
                    copy._currentOpportunitySelection.group = {
                        guid: this.attendeeOpportunities.groups[0].guid,
                        name: this.attendeeOpportunities.groups[0].name
                    };
                }

                if (this.attendeeOpportunities.locations?.length === 1) {
                    copy._currentOpportunitySelection.location = {
                        guid: this.attendeeOpportunities.locations[0].guid,
                        name: this.attendeeOpportunities.locations[0].name
                    };
                }

                if (this.attendeeOpportunities.schedules?.length === 1) {
                    copy._currentOpportunitySelection.schedules = [{
                        guid: this.attendeeOpportunities.schedules[0].guid,
                        name: this.attendeeOpportunities.schedules[0].name
                    }];
                }
            }
        }
        else {
            // TODO: Need to probably do something different in family mode.
            copy._currentOpportunitySelection = {};
        }

        return copy;
    }

    /**
     * Creates a new session by selecting the ability level for the currently
     * selected attendee.
     *
     * @param abilityLevelGuid The ability level identifier to be selected.
     *
     * @returns A new CheckInSession object.
     */
    public withSelectedAbilityLevel(abilityLevelGuid: Guid): CheckInSession {
        const abilityLevel = this._attendeeOpportunities
            ?.abilityLevels
            ?.find(a => areEqual(a.guid, abilityLevelGuid));

        if (!abilityLevel) {
            throw new Error("That ability level is not valid.");
        }

        const copy = this.clone();

        if (!copy._currentOpportunitySelection) {
            throw new Error(invalidCheckInStateMessage);
        }

        copy._currentOpportunitySelection.abilityLevel = {
            guid: abilityLevel.guid,
            name: abilityLevel.name
        };

        return copy;
    }

    /**
     * Creates a new session by selecting the area for the currently selected
     * attendee.
     *
     * @param areaGuid The area identifier to be selected.
     *
     * @returns A new CheckInSession object.
     */
    public withSelectedArea(areaGuid: Guid): CheckInSession {
        const area = this._attendeeOpportunities
            ?.areas
            ?.find(a => areEqual(a.guid, areaGuid));

        if (!area) {
            throw new Error("That area is not valid.");
        }

        const copy = this.clone();

        if (!copy._currentOpportunitySelection) {
            throw new Error(invalidCheckInStateMessage);
        }

        copy._currentOpportunitySelection.area = {
            guid: area.guid,
            name: area.name
        };

        return copy;
    }

    /**
     * Creates a new session by selecting the group for the currently
     * selected attendee.
     *
     * @param groupGuid The group identifier to be selected.
     *
     * @returns A new CheckInSession object.
     */
    public withSelectedGroup(groupGuid: Guid): CheckInSession {
        const group = this._attendeeOpportunities
            ?.groups
            ?.find(g => areEqual(g.guid, groupGuid));

        if (!group) {
            throw new Error("That group is not valid.");
        }

        const copy = this.clone();

        if (!copy._currentOpportunitySelection) {
            throw new Error(invalidCheckInStateMessage);
        }

        copy._currentOpportunitySelection.group = {
            guid: group.guid,
            name: group.name
        };

        return copy;
    }

    /**
     * Creates a new session by selecting the location for the currently
     * selected attendee.
     *
     * @param locationGuid The location identifier to be selected.
     *
     * @returns A new CheckInSession object.
     */
    public withSelectedLocation(locationGuid: Guid): CheckInSession {
        const location = this._attendeeOpportunities
            ?.locations
            ?.find(g => areEqual(g.guid, locationGuid));

        if (!location) {
            throw new Error("That location is not valid.");
        }

        const copy = this.clone();

        if (!copy._currentOpportunitySelection) {
            throw new Error(invalidCheckInStateMessage);
        }

        copy._currentOpportunitySelection.location = {
            guid: location.guid,
            name: location.name
        };

        return copy;
    }

    /**
     * Creates a new session by selecting the schedules for the currently
     * selected attendee.
     *
     * @param scheduleGuids The schedule identifiers to be selected.
     *
     * @returns A new CheckInSession object.
     */
    public withSelectedSchedules(scheduleGuids: Guid[]): CheckInSession {
        const schedules = this._attendeeOpportunities
            ?.schedules
            ?.filter(g => isGuidInList(g.guid, scheduleGuids));

        if (!schedules || schedules.length === 0 || schedules.length !== scheduleGuids.length) {
            throw new Error("Those times are not valid.");
        }

        const copy = this.clone();

        if (!copy._currentOpportunitySelection) {
            throw new Error(invalidCheckInStateMessage);
        }

        copy._currentOpportunitySelection.schedules = schedules
            .map(s => ({
                guid: s.guid,
                name: s.name
            }));

        return copy;
    }

    /**
     * Creates a new session by selecting the opporunities for the currently
     * selected attendee.
     *
     * @param selections The selections for the current attendee.
     *
     * @returns A new CheckInSession object.
     */
    public withSelections(selections: OpportunitySelectionBag[]): CheckInSession {
        const copy = this.clone();

        if (!copy.currentAttendeeGuid) {
            throw new Error(invalidCheckInStateMessage);
        }

        const otherAttendeeSelections = copy._allAttendeeSelections
            .filter(s => !areEqual(s.attendeeGuid, copy.currentAttendeeGuid));

        copy._allAttendeeSelections = [...otherAttendeeSelections, {
            attendeeGuid: copy.currentAttendeeGuid,
            selections: selections
        }];

        return copy;
    }

    // #endregion

    // #region Public Functions

    /**
     * Gets the ability level opportunities that are available for selection
     * on the currently selected attendee.
     *
     * @returns An array of ability level opportunities.
     */
    public getAvailableAbilityLevels(): AbilityLevelOpportunityBag[] {
        if (!this._attendeeOpportunities?.abilityLevels) {
            return [];
        }

        return this._attendeeOpportunities.abilityLevels;
    }

    /**
     * Gets the area opportunities that are available for selection on the
     * currently selected attendee.
     *
     * @returns An array of area opportunities.
     */
    public getAvailableAreas(): AreaOpportunityBag[] {
        const selection = this._currentOpportunitySelection;

        if (!this._attendeeOpportunities?.areas || !selection) {
            return [];
        }

        if (this.configuration.template?.kioskCheckInType === KioskCheckInMode.Individual) {
            return this._attendeeOpportunities.areas;
        }

        // In family mode we need to filter the areas by the selected schedules
        // which means we need to start by getting the locations that are
        // valid for that schedule.
        const selectedScheduleGuids = selection.schedules?.map(s => s.guid);
        const validLocationGuids = this._attendeeOpportunities
            .locations
            ?.filter(l => isGuidInList(selectedScheduleGuids, l.scheduleGuids))
            .map(l => l.guid) ?? [];

        // Now find all groups for those locations.
        const validAreaGuids = this._attendeeOpportunities
            .groups
            ?.filter(g => isGuidInList(validLocationGuids, g.locationGuids))
            .map(g => g.areaGuid) ?? [];

        // Now we can find the areas
        return this._attendeeOpportunities
            .areas
            .filter(a => isGuidInList(a.guid, validAreaGuids));
    }

    /**
     * Gets the group opportunities that are available for selection on the
     * currently selected attendee.
     *
     * @returns An array of group opportunities.
     */
    public getAvailableGroups(): GroupOpportunityBag[] {
        const selection = this._currentOpportunitySelection;

        if (!this._attendeeOpportunities?.groups || !selection) {
            return [];
        }

        if (this.configuration.template?.kioskCheckInType === KioskCheckInMode.Individual) {
            // In individual mode we need to filter the groups by the selected
            // area.
            return this._attendeeOpportunities
                .groups
                .filter(g => areEqual(g.areaGuid, selection.area?.guid));
        }

        // In family mode we need to filter the areas by the selected schedule
        // and the selected area. Which means we need to start by getting the
        // locations that are valid for that schedule.
        const selectedScheduleGuids = selection.schedules?.map(s => s.guid);
        const validLocationGuids = this._attendeeOpportunities
            .locations
            ?.filter(l => isGuidInList(selectedScheduleGuids, l.scheduleGuids))
            .map(l => l.guid) ?? [];

        // Now find all groups for those locations and the selected area.
        return this._attendeeOpportunities
            .groups
            .filter(g => isGuidInList(validLocationGuids, g.locationGuids))
            .filter(g => areEqual(g.areaGuid, selection.area?.guid));
    }

    /**
     * Gets the location opportunities that are available for selection on the
     * currently selected attendee.
     *
     * @returns An array of location opportunities.
     */
    public getAvailableLocations(): LocationOpportunityBag[] {
        const selection = this._currentOpportunitySelection;

        if (!this._attendeeOpportunities?.locations || !selection) {
            return [];
        }

        const group = this._attendeeOpportunities
            .groups
            ?.find(g => areEqual(g.guid, selection.group?.guid));

        if (!group) {
            return [];
        }

        if (this.configuration.template?.kioskCheckInType === KioskCheckInMode.Individual) {
            // In individual mode we need to filter the locations by the selected
            // group.
            return this._attendeeOpportunities
                .locations
                .filter(l => isGuidInList(l.guid, group.locationGuids));
        }

        // In family mode we need to filter the locations by the selected schedules
        // and the selected group.
        const selectedScheduleGuids = selection.schedules?.map(s => s.guid);

        return this._attendeeOpportunities
            .locations
            .filter(l => isGuidInList(l.guid, group.locationGuids))
            .filter(l => isGuidInList(selectedScheduleGuids, l.scheduleGuids));
    }

    /**
     * Gets the schedule opportunities that are available for selection on the
     * currently selected attendee.
     *
     * @returns An array of schedule opportunities.
     */
    public getAvailableSchedules(): ScheduleOpportunityBag[] {
        const selection = this._currentOpportunitySelection;

        if (!this._attendeeOpportunities?.schedules || !selection) {
            return [];
        }

        if (this.configuration.template?.kioskCheckInType === KioskCheckInMode.Individual) {
            // In individual mode we need to filter the schedules by the selected
            // location.
            const location = this._attendeeOpportunities
                .locations
                ?.find(l => areEqual(l.guid, selection.location?.guid));

            if (!location) {
                return [];
            }

            return this._attendeeOpportunities
                .schedules
                .filter(s => isGuidInList(s.guid, location.scheduleGuids));
        }

        // In family mode we are the first step so we just show everything.
        return this._attendeeOpportunities.schedules;
    }

    /**
     * Gets the selections that have previously been made for the specified
     * attendee.
     *
     * @param attendeeGuid The attendee unique identifier.
     *
     * @returns The collection of selections made for this attendee.
     */
    public getAttendeeSelections(attendeeGuid: Guid): OpportunitySelectionBag[] {
        const selections = this._allAttendeeSelections
            .find(s => areEqual(s.attendeeGuid, attendeeGuid));

        if (!selections) {
            return [];
        }

        return clone(selections.selections) as OpportunitySelectionBag[];
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
    private withNextScreenFromFamilySelect(): Promise<CheckInSession> {
        if (!this.currentFamily || !this._attendees) {
            return Promise.resolve(this.withScreen(Screen.Welcome));
        }
        else if (this._currentlyCheckedIn && this._currentlyCheckedIn.length > 0) {
            return Promise.resolve(this.withScreen(Screen.ActionSelect));
        }

        const isFamilyAutoMode = this.configuration.template?.kioskCheckInType === KioskCheckInMode.Family
            && this.configuration.template?.isAutoSelect;

        if (isFamilyAutoMode && this.attendees) {
            const copy = this.clone();

            for (const attendee of this.attendees) {
                if (!attendee.selectedOpportunities || !attendee.person) {
                    continue;
                }

                copy._allAttendeeSelections.push({
                    attendeeGuid: attendee.person.guid,
                    selections: attendee.selectedOpportunities
                });
            }

            return Promise.resolve(copy.withScreen(Screen.PersonSelect));
        }

        return Promise.resolve(this.withScreen(Screen.PersonSelect));
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
                if (this.currentAttendeeGuid) {
                    return Promise.resolve(this.withScreen(Screen.AutoModeOpportunitySelect));
                }
                else {
                    // Family check-in in auto-select mode saves everybody at
                    // once. Perform a full save of the attendance.
                    const newSessionFamily = await this.withSaveAttendance(false);

                    return newSessionFamily.withScreen(Screen.Success);
                }
            }

            throw new Error("Non-auto mode family check-in is not implemented yet.");
        }

        const newSession = this.clone();

        // We should be either family mode that is updating an attendee or
        // individual mode with an attendee.
        if (!newSession.currentAttendeeGuid || !newSession._currentOpportunitySelection) {
            throw new Error(invalidCheckInStateMessage);
        }

        const abilityLevels = this.getAvailableAbilityLevels();

        // If an ability level is not already selected then try to select
        // one if there is a single option to pick from.
        if (!newSession._currentOpportunitySelection.abilityLevel) {
            if (abilityLevels.length === 1) {
                newSession._currentOpportunitySelection.abilityLevel = {
                    guid: abilityLevels[0].guid,
                    name: abilityLevels[0].name
                };
            }
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
        const newSession = this.clone();

        // We should be either family mode that is updating an attendee or
        // individual mode with an attendee.
        if (!newSession.currentAttendeeGuid || !newSession._currentOpportunitySelection) {
            throw new Error(invalidCheckInStateMessage);
        }

        const areas = this.getAvailableAreas();

        // If an area is not already selected then try to select
        // one if there is a single option to pick from.
        if (!newSession._currentOpportunitySelection.area) {
            if (areas.length === 1) {
                newSession._currentOpportunitySelection.area = {
                    guid: areas[0].guid,
                    name: areas[0].name
                };
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
        const newSession = this.clone();

        // We should be either family mode that is updating an attendee or
        // individual mode with an attendee.
        if (!newSession.currentAttendeeGuid || !newSession._currentOpportunitySelection) {
            throw new Error(invalidCheckInStateMessage);
        }

        const groups = this.getAvailableGroups();

        // If a group is not already selected then try to select
        // one if there is a single option to pick from.
        if (!newSession._currentOpportunitySelection.group) {
            if (groups.length === 1) {
                newSession._currentOpportunitySelection.group = {
                    guid: groups[0].guid,
                    name: groups[0].name
                };
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
        const newSession = this.clone();

        // We should be either family mode that is updating an attendee or
        // individual mode with an attendee.
        if (!newSession.currentAttendeeGuid || !newSession._currentOpportunitySelection) {
            throw new Error(invalidCheckInStateMessage);
        }

        const locations = this.getAvailableLocations();

        // If a location is not already selected then try to select
        // one if there is a single option to pick from.
        if (!newSession._currentOpportunitySelection.location) {
            if (locations.length === 1) {
                newSession._currentOpportunitySelection.location = {
                    guid: locations[0].guid,
                    name: locations[0].name
                };
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
    private withNextScreenFromLocationSelect(): Promise<CheckInSession> {
        if (this.configuration.template?.kioskCheckInType === KioskCheckInMode.Family) {
            // TODO: Check if another person to process. This might require some logic.

            return Promise.resolve(this.withScreen(Screen.Success));
        }

        if ((this._attendeeOpportunities?.schedules?.length ?? 0) > 1) {
            return Promise.resolve(this.withScreen(Screen.ScheduleSelect));
        }

        return this.withNextScreenFromScheduleSelect();
    }

    /**
     * Creates a new session that has been updated to reflect the next screen
     * after the schedule select screen.
     *
     * @returns A new CheckInSession object.
     */
    private async withNextScreenFromScheduleSelect(): Promise<CheckInSession> {
        if (this.configuration.template?.kioskCheckInType === KioskCheckInMode.Family) {
            if ((this._attendeeOpportunities?.abilityLevels?.length ?? 0) > 1) {
                return this.withScreen(Screen.AbilityLevelSelect);
            }

            return await this.withNextScreenFromAbilityLevelSelect();
        }

        const currentSelection = this.currentOpportunitySelection;
        const selections: OpportunitySelectionBag[] = [];

        if (!currentSelection) {
            throw new Error(invalidCheckInStateMessage);
        }

        if (!currentSelection.area) {
            throw new Error(invalidCheckInStateMessage);
        }

        if (!currentSelection.group) {
            throw new Error(invalidCheckInStateMessage);
        }

        if (!currentSelection.location) {
            throw new Error(invalidCheckInStateMessage);
        }

        if (!currentSelection.schedules) {
            throw new Error(invalidCheckInStateMessage);
        }

        for (const schedule of currentSelection.schedules) {
            selections.push({
                abilityLevel: currentSelection.abilityLevel,
                area: currentSelection.area,
                group: currentSelection.group,
                location: currentSelection.location,
                schedule: schedule
            });
        }

        if (selections.length === 0) {
            throw new Error(invalidCheckInStateMessage);
        }

        let clone = this.withSelections(selections);

        // Individual check-in. Perform a full save of the attendance.
        clone = await clone.withSaveAttendance(false);

        return clone.withScreen(Screen.Success);
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
