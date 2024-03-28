import { FamilySearchMode } from "@Obsidian/Enums/CheckIn/familySearchMode";
import { FamilyBag } from "@Obsidian/ViewModels/CheckIn/familyBag";
import { Screen, UnexpectedErrorMessage } from "./utils.partial";
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

export class CheckInSession {
    // #region Fields

    /* eslint-disable @typescript-eslint/naming-convention */
    // This is against our naming conventions, but for the moment I can't find
    // a better way to make these read-only and also have them show up as
    // properties when used with console.log. -DSH

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

    /* eslint-enable @typescript-eslint/naming-convention */

    private readonly configuration: KioskConfigurationBag;

    private readonly http: HttpFunctions;

    // #endregion

    // #region Field Getters

    public get searchTerm(): string | undefined {
        return this._searchTerm;
    }

    public get searchType(): FamilySearchMode | undefined {
        return this._searchType;
    }

    public get families(): FamilyBag[] | undefined {
        return this._families;
    }

    public get currentFamilyGuid(): Guid | undefined {
        return this._currentFamilyGuid;
    }

    public get currentFamily(): FamilyBag | undefined {
        return this._families?.find(f => areEqual(f.guid, this._currentFamilyGuid));
    }

    public get attendees(): AttendeeBag[] | undefined {
        return this._attendees;
    }

    public get currentlyCheckedIn(): AttendanceBag[] | undefined {
        return this._currentlyCheckedIn;
    }

    public get selectedAttendeeGuids(): Guid[] | undefined {
        return this._selectedAttendeeGuids;
    }

    public get currentAttendeeGuid(): Guid | undefined {
        return this._currentAttendeeGuid;
    }

    public get currentAttendee(): AttendeeBag | undefined {
        return this._attendees?.find(f => areEqual(f.person?.guid, this._currentAttendeeGuid));
    }

    public get attendeeOpportunities(): OpportunityCollectionBag | undefined {
        return this._attendeeOpportunities;
    }

    // #endregion

    public constructor(configuration: KioskConfigurationBag, http: HttpFunctions) {
        this.configuration = configuration;
        this.http = http;
    }

    private clone(configuration?: KioskConfigurationBag): CheckInSession {
        const copy = new CheckInSession(configuration ?? this.configuration, this.http);

        copy._searchTerm = this._searchTerm;
        copy._searchType = this._searchType;
        copy._families = this._families;
        copy._currentFamilyGuid = this._currentFamilyGuid;
        copy._attendees = this._attendees;
        copy._currentlyCheckedIn = this._currentlyCheckedIn;

        return copy;
    }

    public withConfiguration(configuration: KioskConfigurationBag): CheckInSession {
        return this.clone(configuration);
    }

    public async withSearch(searchTerm: string, searchType: FamilySearchMode): Promise<CheckInSession> {
        if (!this.configuration.template) {
            throw new Error("Invalid session state.");
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
            throw new Error("Invalid session state.");
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
            throw new Error("Invalid session state.");
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

        return copy;
    }

    /**
     * Determines the next screen to display based on the current selections.
     * This method may be called multiple times on each screen so it should
     * not perform any logic that would change the session data.
     *
     * @param currentScreen The current screen that is displayed.
     *
     * @returns The next screen that should be displayed.
     */
    public getNextScreen(currentScreen: Screen): Screen {
        if (currentScreen === Screen.Welcome) {
            if (!this.families) {
                return Screen.Search;
            }
            else if (this.families.length === 1) {
                return Screen.PersonSelect;
            }
            else {
                return Screen.FamilySelect;
            }
        }
        else if (currentScreen === Screen.Search) {
            if (!this.families) {
                return Screen.Welcome;
            }
            else {
                // Always show family select even if only one family when
                // coming from the search screen.
                return Screen.FamilySelect;
            }
        }
        else if (currentScreen === Screen.FamilySelect) {
            if (!this.currentFamily || !this._attendees) {
                return Screen.Welcome;
            }
            else if (this._currentlyCheckedIn && this._currentlyCheckedIn.length > 0) {
                return Screen.ActionSelect;
            }
            else {
                return Screen.PersonSelect;
            }
        }
        else if (currentScreen === Screen.PersonSelect) {
            if (this.configuration.template?.kioskCheckInType == KioskCheckInMode.Family) {
                if (this.currentAttendeeGuid) {
                    // TODO: Go to time select (or whatever next is).
                    return Screen.Welcome;
                }
                else {
                    // TODO: Go to success screen.
                    return Screen.Welcome;
                }
            }
            else {
                // TODO: Go to ability select (or whatever is next).
                return Screen.Welcome;
            }
        }
        else {
            return Screen.Welcome;
        }
    }
}
