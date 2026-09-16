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

import { KioskButton } from "../CheckInKiosk/types.partial";

/**
 * A page footer button, with the layout hint the shared button type lacks.
 */
export type LauncherAction = KioskButton & {
    /**
     * Whether the button may stretch to the full footer width when it is the
     * only action shown. An escape action like Cancel keeps its natural width
     * so it never reads as the step's primary action.
     */
    isFullWidthAllowed?: boolean;
};

export const enum NavigationUrlKey {
    LoginPage = "LoginPage",
    PhoneIdentificationPage = "PhoneIdentificationPage"
}

/**
 * The outcome of asking the browser for the individual's position.
 */
export type PositionResult = {
    /**
     * The position, or null when the browser would not report one.
     */
    position: GeolocationPosition | null;

    /**
     * Whether location access is blocked for this site. The browser will not
     * prompt again, so asking a second time cannot succeed.
     */
    isPermissionDenied: boolean;
};

/**
 * The screen the launcher is currently showing.
 */
export const enum LauncherState {
    /**
     * Asks the individual to identify themselves before check-in can start.
     */
    Identify = "identify",

    /**
     * Warns that the browser is about to ask for location permission.
     */
    LocationPrompt = "locationPrompt",

    /**
     * Waits while the browser reports the individual's location and the kiosk
     * is matched.
     */
    LocationProgress = "locationProgress",

    /**
     * Asks the individual which campus they are at.
     */
    CampusSelect = "campusSelect",

    /**
     * A kiosk has been matched and check-in is open there, so check-in can
     * start.
     */
    WelcomeBack = "welcomeBack",

    /**
     * A kiosk has been matched but check-in is not open there right now.
     */
    NoServices = "noServices",

    /**
     * Runs the template-driven selection steps and saves the check-in.
     */
    CheckInFlow = "checkInFlow",

    /**
     * Explains why check-in cannot continue, with the option to start over.
     */
    Message = "message"
}
