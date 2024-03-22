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

import { parse } from "@Obsidian/Libs/marked";
import { Html5Qrcode } from "@Obsidian/Libs/html5-qrcode";
import { KioskType } from "@Obsidian/Enums/Core/kioskType";
import { CheckInItemBag } from "@Obsidian/ViewModels/CheckIn/checkInItemBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ConfigurationTemplateBag } from "@Obsidian/ViewModels/CheckIn/configurationTemplateBag";

// #region Temporary Types

export type CampusBag = CheckInItemBag & {
    kiosks?: KioskBag[] | null;
};

export type KioskBag = CheckInItemBag & {
    type?: KioskType | null;

    hasCamera: boolean;
};

export type CheckInKioskOptionsBag = {
    isManualSetupAllowed: boolean;

    isConfigureByLocationEnabled: boolean;

    geoLocationCacheInMinutes: number;

    campuses?: CampusBag[] | null;

    defaultTheme?: string | null;

    templates?: ConfigurationTemplateBag[] | null;

    themes?: ListItemBag[] | null;
};

export type SavedConfigurationBag = {
    name?: string | null;

    description?: string | null;
};

export type KioskConfigurationBag = {
    kiosk?: CheckInItemBag | null;

    template?: ConfigurationTemplateBag | null;

    theme?: ListItemBag | null;

    areas?: CheckInItemBag[] | null;
};

export enum Screen {
    Configuration = 0,

    Welcome = 1,

    Search = 2
}

// #endregion

export function markdown(content: string | undefined | null): string {
    if (!content) {
        return "";
    }

    return parse(content) as string;
}

/**
 * Gets the current geo position as an asynchronous operation.
 *
 * @returns The geolocation position from the browser.
 */
export function getCurrentPosition(): Promise<GeolocationPosition> {
    return new Promise<GeolocationPosition>((resolve, reject) => {
        navigator.geolocation.getCurrentPosition(position => {
            resolve(position);
        }, error => {
            reject(new Error(error.message));
        }, {
            enableHighAccuracy: true
        });
    });
}

/**
 * Determines if we are running on the iPad app with camera support.
 *
 * @param kiosk The kiosk device to be checked.
 *
 * @returns true if this kiosk is an iPad running the native application with
 * built in camera support.
 */
export function isIpadAppWithCamera(kiosk: KioskBag): boolean {
    // If kiosk Type is defined, honor KioskType setting, otherwise
    // use auto-detect logic.

    if (kiosk.type === undefined || kiosk.type === null) {
        // Type is not defined, so autodetect.
        return typeof window["RockCheckinNative"] !== "undefined" && typeof window["RockCheckinNative"].StartCamera !== "undefined";
    }
    else if (kiosk.type === KioskType.IPad) {
        // Type is defined as iPad, but double check if really is.
        return typeof window["RockCheckinNative"] !== "undefined" && typeof window["RockCheckinNative"].StartCamera !== "undefined";
    }
    else {
        // Type is set, but isn't set to IPad.
        return false;
    }
}

/**
 * Determines if the theme supports HTML5 camera operations.
 *
 * @returns true if the theme supports HTML5 camera operations.
 */
export function doesThemeSupportCamera(): boolean {
    return document.body.classList.contains("js-camera-supported");
}

/**
 * Gets the cameras currently available for use.
 *
 * @returns A list of items that represent the available cameras.
 */
export async function getCameraItems(): Promise<ListItemBag[]> {
    // Note that if this is running in the IPad App, but is using a Device that
    // is configured as KioskType.Browser with an HTML5 Camera, the HTML5
    // cameras won't get listed because of the IPad app's browser permissions.
    // This is OK because we don't really support using HTML5 Camera in the IPad.

    try {
        // This method will trigger user permissions
        const devices = await Html5Qrcode.getCameras();

        return devices.map(d => ({
            value: d.id,
            text: d.label
        }));
    }
    catch (err) {
        console.log(err);

        return [];
    }
}

/**
 * Determines if the HTML camera feature is available for use.
 *
 * @param kiosk The kiosk that will be using the camera.
 *
 * @returns true if the HTML camera feature is available.
 */
export function isHtmlCameraAvailable(kiosk: KioskBag | undefined): boolean {
    // Only show HTML5 Camera options if all the following are true
    // -- Kiosk HasCamera is true
    // -- Kiosk Type has been set (the HTML5 camera feature won't be enabled until they specifically set the KioskType)
    // -- The KioskType is not an IPad
    // -- The current Theme supports the HTML5 Camera feature
    // -- Not running on the iPad application

    return !!kiosk
        && kiosk.hasCamera
        && kiosk.type !== null
        && kiosk.type !== undefined
        && kiosk.type !== KioskType.IPad
        // TODO: Enable This && doesThemeSupportCamera()
        && !isIpadAppWithCamera(kiosk);
}
