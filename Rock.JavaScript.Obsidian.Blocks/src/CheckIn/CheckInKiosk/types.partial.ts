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

import { CheckInItemBag } from "@Obsidian/ViewModels/CheckIn/checkInItemBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ConfigurationTemplateBag } from "@Obsidian/ViewModels/CheckIn/configurationTemplateBag";
import { KioskConfigurationBag } from "@Obsidian/ViewModels/Blocks/CheckIn/CheckInKiosk/kioskConfigurationBag";
import { WebKioskBag } from "@Obsidian/ViewModels/Blocks/CheckIn/CheckInKiosk/webKioskBag";
import { SavedCheckInConfigurationBag } from "@Obsidian/ViewModels/CheckIn/savedCheckInConfigurationBag";
import { AchievementBag } from "@Obsidian/ViewModels/CheckIn/achievementBag";
import { AttendanceBag } from "@Obsidian/ViewModels/CheckIn/attendanceBag";
import { PersonBag } from "@Obsidian/ViewModels/CheckIn/personBag";
import { GetCurrentAttendanceResponseBag } from "@Obsidian/ViewModels/Blocks/CheckIn/CheckInKiosk/getCurrentAttendanceResponseBag";
import { RegistrationFamilyBag } from "@Obsidian/ViewModels/CheckIn/registrationFamilyBag";
import { RegistrationPersonBag } from "@Obsidian/ViewModels/CheckIn/registrationPersonBag";
import { EditFamilyResponseBag } from "@Obsidian/ViewModels/Blocks/CheckIn/CheckInKiosk/editFamilyResponseBag";
import { Guid } from "@Obsidian/Types";
import { ValidPropertiesBox } from "@Obsidian/ViewModels/Utility/validPropertiesBox";

// #region Temporary Types

export type CampusBag = CheckInItemBag & {
    kiosks?: WebKioskBag[] | null;
};

export type CheckInKioskOptionsBag = {
    kioskConfiguration?: KioskConfigurationBag | null;

    isEditAllowed: boolean;

    isManualSetupAllowed: boolean;

    isConfigureByLocationEnabled: boolean;

    geoLocationCacheInMinutes: number;

    campuses?: CampusBag[] | null;

    currentTheme?: string | null;

    templates?: ConfigurationTemplateBag[] | null;

    themes?: ListItemBag[] | null;

    savedConfigurations?: SavedCheckInConfigurationBag[] | null;

    kioskPageRoute?: string | null;
};

// #endregion

/**
 * Identifies each and every screen that can possibly be displayed in the
 * check-in kiosk. This does not include the supervisor screens.
 */
export enum Screen {
    /**
     * No screen shown, this is used while initially loading.
     */
    None,

    /**
     * The main screen that will be visible when people want to start a new
     * check-in session.
     */
    Welcome,

    /**
     * Provides the UI to search for families in the system.
     */
    Search,

    /**
     * Displays the family search results and allows for selecting a single
     * family.
     */
    FamilySelect,

    /**
     * Displays an option to check-out somebody that is already checked in or
     * to start a new check-in session.
     */
    ActionSelect,

    /**
     * Displays the list of people currently checked in and allows selecting one
     * to checkout.
     */
    CheckoutSelect,

    /**
     * Displays the members and relations for a single family. This also handles
     * showing the current check-in options when using auto-select mode.
     * Depending on the mode either a single person can be selected or
     * multiple people may be selected.
     */
    PersonSelect,

    /**
     * Displays all the option combinations that are valid for the selected
     * attendee in a single list. This will then update all opportunity
     * selections at the same time.
     */
    AutoModeOpportunitySelect,

    /**
     * Displays the available ability levels for an attendee and requires that
     * one be selected.
     */
    AbilityLevelSelect,

    /**
     * Displays the available areas for an attendee and requires that one be
     * selected.
     */
    AreaSelect,

    /**
     * Displays the available groups for an attendee and requires that one be
     * selected.
     */
    GroupSelect,

    /**
     * Displays the available locations for an attendee and requires that one
     * be selected.
     */
    LocationSelect,

    /**
     * Displays the available schedules for an attendee and requires that one
     * be selected.
     */
    ScheduleSelect,

    /**
     * Displays the success message associated with a completed check-in. This
     * also shows any achievement information and handles passing print data
     * to the native apps.
     */
    Success,

    /**
     * Displays the success message associated with a completed check out.
     */
    CheckoutSuccess,
}

/**
 * Identifies each of the supervisor screens that can be displayed in the
 * check-in kiosk. This does not include the screens that are part of a normal
 * check-in flow.
 */
export enum SupervisorScreen {
    /**
     * The screen that will be displayed when a supervisor wants to login.
     */
    Login = 100,

    /**
     * The screen that displays the list of available actions to the supervisor.
     */
    Actions,

    /**
     * The screen that allows a reprint of existing check-in labels.
     */
    Reprint,

    /**
     * The screen that allows configuring which locations are scheduled to
     * be open.
     */
    ScheduleLocations,
}

/**
 * Identifies each of the registration screens that are available when the
 * kiosk has registration mode enabled.
 */
export enum RegistrationScreen {
    /**
     * The edit family screen displays a list of individuals in the family
     * and allows editing family information as well as removing existing
     * family members.
     */
    EditFamily = 200,

    /**
     * The edit individual screen allows for both adding and editing family
     * members.
     */
    EditIndividual = 201
}

/**
 * Defines a button that will be displayed by the main kiosk page at the
 * request of child screens.
 */
export type KioskButton = {
    /** The text to display in the button. */
    title: string;

    /**
     * A unique key that identifies this button. This allows a button such as
     * "Next" to stay on screen between transitions if both screens use the
     * button. It also allows a single button to remain when the title changes.
     */
    key: string;

    /** True if the button is currently in a disabled state. */
    disabled: boolean;

    /** The type of button to render. */
    type: "default" | "primary" | "success" | "info" | "warning" | "danger"

    /** Additional CSS classes to put on the button element. */
    class?: string;

    /** The function to call when the button is clicked. */
    handler?: () => void | Promise<void>;
};

/**
 * Defines the structure of aggregate attendance data used on the success
 * screen. The server returns multiple recorded attendance records for a single
 * individual, but we need those aggregated by person.
 */
export type AggregateAttendance = {
    person: PersonBag;
    attendances: AttendanceBag[],
    justCompletedAchievements: AchievementBag[],
    inProgressAchievements: AchievementBag[]
};

/* eslint-disable @typescript-eslint/naming-convention */
/**
 * The interface that the native application provides when the web page is
 * running in the iOS or Windows native app.
 */
export interface IRockCheckInNative {
    /**
     * Prints the legacy labels from check-in v1.
     *
     * @param tagJson The JSON data that contains the label details.
     */
    PrintLabels?(tagJson: string): Promise<void>;

    /**
     * Prints the labels from check-in v2.
     *
     * @param tagJson The JSON data that contains the label details.
     */
    PrintV2Labels?(tagJson: string): Promise<string[]>;

    /**
     * Starts the native camera scanning feature of the application.
     *
     * @param isPassive True if the camera should be in passive mode.
     */
    StartCamera?(isPassive: boolean): Promise<void>;

    /**
     * Stops the native camera scanning feature of the application.
     */
    StopCamera?(): Promise<void>;

    /**
     * Sets the kiosk identifier for the native application.
     *
     * @param kioskId The kiosk integer identifier.
     */
    SetKioskId?(kioskId: number): Promise<void>;
}
/* eslint-enable @typescript-eslint/naming-convention */

/**
 * Makes all properties on the type not nullable and not undefined.
 */
type NoNullishField<T> = { [P in keyof T]-?: NonNullable<T[P]> };

export type SupervisorScreenData = {
    pinCode: string;

    counts?: GetCurrentAttendanceResponseBag;
};

export type RegistrationScreenData = Omit<Omit<EditFamilyResponseBag, "people">, "family"> & {
    editPersonGuid?: string | null;

    family: NoNullishField<ValidPropertiesBox<RegistrationFamilyBag>>;

    people: NoNullishField<ValidPropertiesBox<AugmentedRegistrationPersonBag>>[];
};

export type AugmentedRegistrationPersonBag = RegistrationPersonBag & {
    guid: Guid;
};

export type AttendanceCountGroup = {
    id: string;

    name: string;

    count: number;

    children: AttendanceCountGroup[];
};

/**
 * Contains count adjustments for locations. These are tracking during a
 * check-in session only.
 */
export type LocationCountAdjustment = {
    /** The encrypted identifier of the attendance record. */
    id?: string | null;

    /** The timestamp that this adjustment was received. */
    timestamp: number;

    /** The encrypted identifier of the location. */
    locationId: string;

    /** The adjustment value, may be negative. */
    count: number;
};

export type KioskConfiguration = KioskConfigurationBag & {
    /** This maps location Guid values to IdKey values. */
    locationIdMap: Record<string, string>;

    /** This maps group Guid values to IdKey values. */
    groupIdMap: Record<string, string>;

    /**
     * This contains any location count adjustments that will be used during
     * the check-in process. It is reset whenever we navigate away from the
     * welcome screen.
     */
    locationCountAdjustments: LocationCountAdjustment[];
};
