import { FamilySearchMode } from "@Obsidian/Enums/CheckIn/familySearchMode";
import { FamilyBag } from "@Obsidian/ViewModels/CheckIn/familyBag";
import { Screen, UnexpectedErrorMessage } from "./utils.partial";
import { KioskConfigurationBag } from "@Obsidian/ViewModels/Blocks/CheckIn/CheckInKiosk/kioskConfigurationBag";
import { SearchForFamiliesOptionsBag } from "@Obsidian/ViewModels/Rest/CheckIn/searchForFamiliesOptionsBag";
import { SearchForFamiliesResponseBag } from "@Obsidian/ViewModels/Rest/CheckIn/searchForFamiliesResponseBag";
import { FamilyMembersOptionsBag } from "@Obsidian/ViewModels/Rest/CheckIn/familyMembersOptionsBag";
import { FamilyMembersResponseBag } from "@Obsidian/ViewModels/Rest/CheckIn/familyMembersResponseBag";
import { HttpFunctions } from "@Obsidian/Types/Utility/http";
import { AttendeeBag } from "@Obsidian/ViewModels/CheckIn/attendeeBag";
import { AttendanceBag } from "@Obsidian/ViewModels/CheckIn/attendanceBag";

export class CheckInSession {
    // #region Fields

    /* eslint-disable @typescript-eslint/naming-convention */
    // This is against our naming conventions, but for the moment I can't find
    // a better way to make these read-only and also have them show up as
    // properties when used with console.log. -DSH

    private _searchTerm?: string;

    private _searchType?: FamilySearchMode;

    private _families?: FamilyBag[];

    private _family?: FamilyBag;

    private _attendees?: AttendeeBag[];

    private _currentlyCheckedIn?: AttendanceBag[];

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

    public get family(): FamilyBag | undefined {
        return this._family;
    }

    public get attendees(): AttendeeBag[] | undefined {
        return this._attendees;
    }

    public get currentlyCheckedIn(): AttendanceBag[] | undefined {
        return this._currentlyCheckedIn;
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
        copy._family = this._family;
        copy._attendees = this._attendees;
        copy._currentlyCheckedIn = this._currentlyCheckedIn;

        return copy;
    }

    public withConfiguration(configuration: KioskConfigurationBag): CheckInSession {
        return this.clone(configuration);
    }

    public async searchForFamily(searchTerm: string, searchType: FamilySearchMode): Promise<CheckInSession> {
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

    public async withFamily(family: FamilyBag): Promise<CheckInSession> {
        const request: FamilyMembersOptionsBag = {
            configurationTemplateGuid: this.configuration.template?.guid,
            kioskGuid: this.configuration.kiosk?.guid,
            areaGuids: this.configuration.areas?.filter(a => !!a.guid).map(a => a.guid as string),
            familyGuid: family.guid
        };

        const response = await this.http.post<FamilyMembersResponseBag>("/api/v2/checkin/FamilyMembers", undefined, request);

        if (!response.isSuccess || !response.data?.people) {
            throw new Error(response.errorMessage || UnexpectedErrorMessage);
        }

        const copy = this.clone();

        copy._family = family;
        copy._attendees = response.data.people;
        copy._currentlyCheckedIn = response.data.currentlyCheckedInAttendances ?? [];

        return copy;
    }

    public getNextScreen(currentScreen: Screen): Screen {
        if (currentScreen === Screen.Welcome) {
            if (!this.families) {
                return Screen.Search;
            }
            else if (this.families.length === 1) {
                // TODO: Search for family members first.
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
                // Always show family select even if only one family.
                return Screen.FamilySelect;
            }
        }
        else if (currentScreen === Screen.FamilySelect) {
            if (!this.family || !this._attendees) {
                return Screen.Welcome;
            }
            else if (this._currentlyCheckedIn && this._currentlyCheckedIn.length > 0) {
                return Screen.ActionSelect;
            }
            else {
                return Screen.PersonSelect;
            }
        }
        else {
            return Screen.Welcome;
        }
    }
}
