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

// Define the browser-specific versions of these functions that older browsers
// implemented before using the standard API.
declare global {
    // eslint-disable-next-line @typescript-eslint/naming-convention
    interface Document {
        mozCancelFullScreen?: () => Promise<void>;
        webkitExitFullscreen?: () => Promise<void>;
        mozFullScreenElement?: Element;
        webkitFullscreenElement?: Element;
    }

    // eslint-disable-next-line @typescript-eslint/naming-convention
    interface HTMLElement {
        mozRequestFullscreen?: () => Promise<void>;
        webkitRequestFullscreen?: () => Promise<void>;
    }
}

/**
 * Request that the window enter true fullscreen mode for the given element.
 *
 * @param element The element that will be the root of the fullscreen view.
 * @param exitCallback The function to call when leaving fullscreen mode.
 *
 * @returns A promise that indicates when the operation has completed.
 */
export async function enterFullscreen(element: HTMLElement, exitCallback?: (() => void)): Promise<boolean> {
    try {
        if (element.requestFullscreen) {
            await element.requestFullscreen();
        }
        else if (element.mozRequestFullscreen) {
            await element.mozRequestFullscreen();
        }
        else if (element.webkitRequestFullscreen) {
            await element.webkitRequestFullscreen();
        }
        else {
            return false;
        }

        element.classList.add("is-fullscreen");

        const onFullscreenChange = (): void => {
            element.classList.remove("is-fullscreen");

            document.removeEventListener("fullscreenchange", onFullscreenChange);
            document.removeEventListener("mozfullscreenchange", onFullscreenChange);
            document.removeEventListener("webkitfullscreenchange", onFullscreenChange);

            if (exitCallback) {
                exitCallback();
            }
        };

        document.addEventListener("fullscreenchange", onFullscreenChange);
        document.addEventListener("mozfullscreenchange", onFullscreenChange);
        document.addEventListener("webkitfullscreenchange", onFullscreenChange);

        return true;
    }
    catch (ex) {
        console.error(ex);
        return false;
    }
}

/**
 * Checks if any element is currently in fullscreen mode.
 *
 * @returns True if an element is currently in fullscreen mode in the window; otherwise false.
 */
export function isFullscreen(): boolean {
    return !!getFullscreenElement();
}

/**
 * Gets the element that is currently the root of fullscreen mode.
 */
export function getFullscreenElement(): Element | undefined {
    return document.fullscreenElement || document.mozFullScreenElement || document.webkitFullscreenElement;
}

/**
 * Manually exits fullscreen mode.
 *
 * @returns True if fullscreen mode was exited; otherwise false.
 */
export async function exitFullscreen(): Promise<boolean> {
    try {
        if (document.exitFullscreen) {
            await document.exitFullscreen();
        }
        else if (document.mozCancelFullScreen) {
            await document.mozCancelFullScreen();
        }
        else if (document.webkitExitFullscreen) {
            document.webkitExitFullscreen();
        }
        else {
            return false;
        }

        return true;
    }
    catch (ex) {
        console.error(ex);
        return false;
    }
}
