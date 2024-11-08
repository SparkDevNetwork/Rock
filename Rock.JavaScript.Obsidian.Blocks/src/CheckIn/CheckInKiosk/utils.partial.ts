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
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { KioskBag } from "@Obsidian/ViewModels/CheckIn/kioskBag";
import { LegacyClientLabelBag } from "@Obsidian/ViewModels/CheckIn/Labels/legacyClientLabelBag";
import { inject, provide } from "vue";
import { showDialog } from "@Obsidian/Utility/dialogs";
import { zeroPad } from "@Obsidian/Utility/numberUtils";
import { IRockCheckInNative } from "./types.partial";
import { ICancellationToken } from "@Obsidian/Utility/cancellation";
import { ClientLabelBag } from "@Obsidian/ViewModels/CheckIn/Labels/clientLabelBag";

/** The unique key for the kiosk state in Vue. */
const kioskStateKey = Symbol("KioskState");

/**
 * The message to display when an error is returned by the server without an
 * error message included.
 */
export const UnexpectedErrorMessage = "Unexpected error encountered, please try again or ask for assistance.";

/**
 * The error message to throw if the check-in state is not valid for the
 * requested action. This is displayed in the UI.
 */
export const invalidCheckInStateMessage = "Invalid check-in state.";

/**
 * Makes a state object available to child components. This is essentially an
 * in-memory cache so they can quickly restore their state. It will be lost
 * if the page is reloaded.
 *
 * @param state The state object to make available to child components.
 */
export function provideKioskState(state: Record<string, unknown>): void {
    provide(kioskStateKey, state);
}

/**
 * Gets the state object for the kiosk. This is essentially an in-memory cache
 * so child components can quickly restore their state. It will be lost if the
 * page is reloaded. The state is not reactive.
 *
 * @returns The state object that can be used to store custom data.
 */
export function useKioskState(): Record<string, unknown> {
    return inject<Record<string, unknown>>(kioskStateKey, {});
}

/**
 * Converts a string of Markdown into HTML.
 *
 * @param content The markdown content to be converted to HTML.
 *
 * @returns A string that represents the HTML.
 */
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
    const native = window["RockCheckinNative"] as IRockCheckInNative | undefined;

    if (kiosk.type === undefined || kiosk.type === null) {
        // Type is not defined, so autodetect.
        return !!native && !!native.StartCamera;
    }
    else if (kiosk.type === KioskType.IPad) {
        // Type is defined as iPad, but double check if really is.
        return !!native && !!native.StartCamera;
    }
    else {
        // Type is set, but isn't set to IPad.
        return false;
    }
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
    // -- Kiosk IsCameraEnabled is true
    // -- Kiosk Type has been set (the HTML5 camera feature won't be enabled until they specifically set the KioskType)
    // -- The KioskType is not an IPad
    // -- Not running on the iPad application

    return !!kiosk
        && kiosk.isCameraEnabled
        && kiosk.type !== null
        && kiosk.type !== undefined
        && kiosk.type !== KioskType.IPad
        && !isIpadAppWithCamera(kiosk);
}

/**
 * Converts the seconds into an hour, minute, second countdown string.
 *
 * @param seconds The number of seconds.
 *
 * @returns A string in the format of "[hh:]mm:ss".
 */
export function secondsToCountdown(seconds: number): string {
    let timeString: string = "";

    if (seconds < 0) {
        seconds = 0;
    }

    const hours = Math.floor(seconds / 3600);
    seconds %= 3600;

    const minutes = Math.floor(seconds / 60);
    seconds %= 60;

    if (hours > 0) {
        timeString = `${hours}:`;
    }

    return `${timeString}${zeroPad(minutes, 2)}:${zeroPad(seconds, 2)}`;
}

/**
 * Clones an object by converting it to JSON and back. This will work with
 * nulls, undefined values, numbers, strings, arrays and objects.
 *
 * @param value The value to be cloned.
 *
 * @returns A new object with the same content as the original.
 */
export function clone<T>(value: T): T {
    if (value === undefined) {
        return undefined as unknown as T;
    }
    else if (value === null) {
        return null as unknown as T;
    }

    return JSON.parse(JSON.stringify(value));
}

/**
 * Determines if the needle is in the haystack of ids. Only one of the needle
 * values must exist in the haystack.
 *
 * @param needle The ids to be searched for.
 * @param haystack The array of ids to be searched in.
 *
 * @returns true if needle was found in the haystack.
 */
export function isAnyIdInList(needle: string | string[] | null | undefined, haystack: string[] | null | undefined): boolean {
    if (!needle || !haystack) {
        return false;
    }

    if (Array.isArray(needle)) {
        return haystack.some(h => needle.some(n => h === n));
    }

    return haystack.some(h => h === needle);
}

/**
 * A custom error that indicates something is wrong with the check-in state.
 * A standard error message will be used to show the user but the provided
 * message will be logged to the console for debugging.
 */
export class InvalidCheckInStateError extends Error {
    public readonly stateMessage: string;

    /**
     * Creates a new instance of InvalidCheckInStateError.
     *
     * @param stateMessage The message describing how the state is invalid.
     */
    constructor(stateMessage: string) {
        super(invalidCheckInStateMessage);
        this.name = "InvalidCheckInStateError";
        this.stateMessage = stateMessage;

        console.error(stateMessage);
    }
}

/**
* Prints the labels through the native bridge if available.
*
* @param labels The labels to be printed.
*
* @returns An array of error messages that should be displayed.
*/
export async function printLabels(labels: ClientLabelBag[]): Promise<string[]> {
    if (labels.length === 0) {
        return [];
    }

    const native = window["RockCheckinNative"] as IRockCheckInNative | undefined;

    if (native?.PrintV2Labels) {
        try {
            return await native.PrintV2Labels(JSON.stringify(labels));
        }
        catch (error) {
            if (error instanceof Error) {
                return [error.message];
            }
            else if (typeof error === "string") {
                return [error];
            }
            else if (typeof error === "object" && error && "Error" in error) {
                return [error["Error"] as string];
            }
            else {
                return ["Unknown error printing label."];
            }
        }
    }
    else {
        return ["Device does not support printing."];
    }
}

/**
 * Prints the legacy labels by invoking the injected native handler code in
 * the browser from the client.
 *
 * @param legacyLabels The legacy labels to be printed.
 *
 * @returns An array of error messages.
 */
export async function printLegacyLabels(legacyLabels: LegacyClientLabelBag[]): Promise<string[]> {
    if (legacyLabels.length === 0) {
        return [];
    }

    for (const label of legacyLabels) {
        if (label.printerAddress === null) {
            label.printerAddress = "";
        }
    }

    if (typeof window["RockCheckinNative"] !== "undefined") {
        // The iOS app needs PascalCase keys, so convert.
        const labels = legacyLabels.map(l => {
            const label: Record<string, unknown> = {};

            for (const key in l) {
                if (Object.hasOwnProperty.call(l, key)) {
                    label[key.charAt(0).toUpperCase() + key.substring(1)] = l[key];
                }
            }

            return label;
        });

        try {
            await window["RockCheckinNative"].PrintLabels(JSON.stringify(labels));
        }
        catch (error) {
            if (error && typeof error === "object" && typeof error["Error"] === "string") {
                return [error["Error"]];
            }
            else {
                return ["Unknown error printing labels."];
            }
        }
    }
    else if (window["chrome"] && window["chrome"].webview && typeof window["chrome"].webview.postMessage) {
        const cmd = {
            eventName: "PRINT_LABELS",
            eventData: JSON.stringify(legacyLabels)
        };

        window["chrome"].webview.postMessage(cmd);
    }

    return [];
}

/**
 * Shows an alert message that requires the user to acknowledge.
 *
 * @param message The message text to be displayed.
 * @param cancellationToken Can be used to automatically dismiss the dialog.
 *
 * @returns A promise that indicates when the dialog has been dismissed.
 */
export async function alert(message: string, cancellationToken?: ICancellationToken): Promise<void> {
    await showDialog({
        message,
        buttons: [
            {
                key: "ok",
                label: "OK",
                className: "btn btn-primary"
            }
        ],
        cancellationToken
    });
}
