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
import { FamilyBag } from "@Obsidian/ViewModels/CheckIn/familyBag";
import { UnexpectedErrorMessage, clone, isGuidInList } from "./utils.partial";
import { Screen, SessionOpportunitySelectionBag } from "./types.partial";
import { KioskConfigurationBag } from "@Obsidian/ViewModels/Blocks/CheckIn/CheckInKiosk/kioskConfigurationBag";
import { SearchForFamiliesOptionsBag } from "@Obsidian/ViewModels/Rest/CheckIn/searchForFamiliesOptionsBag";
import { SearchForFamiliesResponseBag } from "@Obsidian/ViewModels/Rest/CheckIn/searchForFamiliesResponseBag";
import { FamilyMembersOptionsBag } from "@Obsidian/ViewModels/Rest/CheckIn/familyMembersOptionsBag";
import { FamilyMembersResponseBag } from "@Obsidian/ViewModels/Rest/CheckIn/familyMembersResponseBag";
import { AttendeeOpportunitiesOptionsBag } from "@Obsidian/ViewModels/Rest/CheckIn/attendeeOpportunitiesOptionsBag";
import { AttendeeOpportunitiesResponseBag } from "@Obsidian/ViewModels/Rest/CheckIn/attendeeOpportunitiesResponseBag";
import { HttpFunctions } from "@Obsidian/Types/Utility/http";
import { AttendeeBag } from "@Obsidian/ViewModels/CheckIn/attendeeBag";
import { AttendanceBag } from "@Obsidian/ViewModels/CheckIn/attendanceBag";
import { Guid } from "@Obsidian/Types";
import { areEqual } from "@Obsidian/Utility/guid";
import { OpportunityCollectionBag } from "@Obsidian/ViewModels/CheckIn/opportunityCollectionBag";
import { KioskCheckInMode } from "@Obsidian/Enums/CheckIn/kioskCheckInMode";
import { AbilityLevelOpportunityBag } from "@Obsidian/ViewModels/CheckIn/abilityLevelOpportunityBag";
import { DeepReadonly } from "vue";
import { AreaOpportunityBag } from "@Obsidian/ViewModels/CheckIn/areaOpportunityBag";
import { GroupOpportunityBag } from "@Obsidian/ViewModels/CheckIn/groupOpportunityBag";
import { LocationOpportunityBag } from "@Obsidian/ViewModels/CheckIn/locationOpportunityBag";
import { ScheduleOpportunityBag } from "@Obsidian/ViewModels/CheckIn/scheduleOpportunityBag";

/**
 * The error message to throw if the check-in state is not valid for the
 * requested action. This is displayed in the UI.
 */
const invalidCheckInStateMessage = "Invalid check-in state.";

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

    /* eslint-enable @typescript-eslint/naming-convention */

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

    // #endregion

    // #region Constructors

    /**
     * Creates a new check-in session object that can be used to process a
     * single individual check-in or a whole family check-in depending on the
     * kiosk configuration.
     *
     * @param configuration The kiosk configured that this session will conform to.
     * @param http The object that provides HTTP access to the server.
     */
    public constructor(configuration: KioskConfigurationBag, http: HttpFunctions) {
        this._currentScreen = Screen.Welcome;
        this.configuration = configuration;
        this.http = http;
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
        const copy = new CheckInSession(configuration ?? this.configuration, this.http);

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

    // #endregion

    // #region Public Selection Functions

    public withConfiguration(configuration: KioskConfigurationBag): CheckInSession {
        return this.clone(configuration);
    }

    public async withSearch(searchTerm: string, searchType: FamilySearchMode): Promise<CheckInSession> {
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

    public withSelectedAttendees(attendeeGuids: Guid[]): CheckInSession {
        const copy = this.clone();

        copy._selectedAttendeeGuids = attendeeGuids;

        return copy;
    }

    public async withAttendee(attendeeGuid: Guid): Promise<CheckInSession> {
        if (!this.configuration.template || !this.configuration.kiosk) {
            throw new Error(invalidCheckInStateMessage);
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

    // #endregion

    // #region Public Functions

    public getAvailableAbilityLevels(): AbilityLevelOpportunityBag[] {
        if (!this._attendeeOpportunities?.abilityLevels) {
            return [];
        }

        return this._attendeeOpportunities.abilityLevels;
    }

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

    // #endregion

    // #region Screen Switch Functions

    /**
     * Moves to the next screen to display based on the current selections.
     * This may trigger automatic logic, such as moving to the next attendee
     * in the family.
     *
     * @returns The new check-in session representing the next screen.
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

    private withNextScreenFromWelcome(): Promise<CheckInSession> {
        if (!this.families) {
            return Promise.resolve(this.withScreen(Screen.Search));
        }
        else if (this.families.length === 1) {
            return Promise.resolve(this.withScreen(Screen.PersonSelect));
        }

        return Promise.resolve(this.withScreen(Screen.FamilySelect));
    }

    private withNextScreenFromSearch(): Promise<CheckInSession> {
        if (!this.families) {
            return Promise.resolve(this.withScreen(Screen.Welcome));
        }

        // Always show family select even if only one family when
        // coming from the search screen.
        return Promise.resolve(this.withScreen(Screen.FamilySelect));
    }

    private withNextScreenFromFamilySelect(): Promise<CheckInSession> {
        if (!this.currentFamily || !this._attendees) {
            return Promise.resolve(this.withScreen(Screen.Welcome));
        }
        else if (this._currentlyCheckedIn && this._currentlyCheckedIn.length > 0) {
            return Promise.resolve(this.withScreen(Screen.ActionSelect));
        }

        return Promise.resolve(this.withScreen(Screen.PersonSelect));
    }

    private withNextScreenFromPersonSelect(): Promise<CheckInSession> {
        if (this.configuration.template?.kioskCheckInType == KioskCheckInMode.Family) {
            // If we don't have an attendee then we are done.
            if (!this.currentAttendeeGuid) {
                return Promise.resolve(this.withScreen(Screen.Success));
            }
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
            return Promise.resolve(newSession.withScreen(Screen.AbilityLevelSelect));
        }

        return newSession.withNextScreenFromAbilityLevelSelect();
    }

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

    private withNextScreenFromScheduleSelect(): Promise<CheckInSession> {
        if (this.configuration.template?.kioskCheckInType === KioskCheckInMode.Family) {
            if ((this._attendeeOpportunities?.abilityLevels?.length ?? 0) > 1) {
                return Promise.resolve(this.withScreen(Screen.ScheduleSelect));
            }

            return this.withNextScreenFromAbilityLevelSelect();
        }

        return Promise.resolve(this.withScreen(Screen.Success));
    }

    private withNextScreenFromSuccess(): Promise<CheckInSession> {
        return Promise.resolve(this.withScreen(Screen.Welcome));
    }

    // #endregion
}
