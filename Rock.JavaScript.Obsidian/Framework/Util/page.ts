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

import { IEntity, Person } from "../ViewModels";
import { Guid } from "./guid";

export type PageConfig = {
    executionStartTime: number;
    pageId: number;
    pageGuid: Guid;
    pageParameters: Record<string, unknown>;
    currentPerson: Person | null;
    contextEntities: Record<string, IEntity>;
    loginUrlWithReturnUrl: string;
};

export function smoothScrollToTop(): void {
    window.scrollTo({ top: 0, behavior: "smooth" });
}

export default {
    smoothScrollToTop
};


/*
 * Code to handle working with modals.
 */
let currentModalCount = 0;

/**
 * Track a modal being opened or closed. This is used to adjust the page in response
 * to any modals being visible.
 * 
 * @param state true if the modal is now open, false if it is now closed.
 */
export function trackModalState(state: boolean): void {
    const body = document.body;
    const cssClasses = ["modal-open"];

    if (state) {
        currentModalCount++;
    }
    else {
        currentModalCount = currentModalCount > 0 ? currentModalCount - 1 : 0;
    }

    if (currentModalCount > 0) {
        for (const cssClass of cssClasses) {
            body.classList.add(cssClass);
        }
    }
    else {
        for (const cssClass of cssClasses) {
            body.classList.remove(cssClass);
        }
    }
}

/**
 * Loads a JavaScript file asynchronously into the document and returns a
 * Promise that can be used to determine when the script has loaded. The
 * promise will return true if the script loaded successfully or false if it
 * failed to load.
 *
 * The function passed in isScriptLoaded will be called before the script is
 * inserted into the DOM as well as after to make sure it actually loaded.
 * 
 * @param source The source URL of the script to be loaded.
 * @param isScriptLoaded An optional function to call to determine if the script is loaded.
 * @param attributes An optional set of attributes to apply to the script tag.
 *
 * @returns A Promise that indicates if the script was loaded or not.
 */
export async function loadJavaScriptAsync(source: string, isScriptLoaded?: () => boolean, attributes?: Record<string, string>): Promise<boolean> {
    // Check if the script is already loaded. First see if we have a custom
    // function that will do the check. Otherwise fall back to looking for any
    // script tags that have the same source.
    if (isScriptLoaded) {
        if (isScriptLoaded()) {
            return true;
        }
    }
    else {
        const scripts = Array.from(document.getElementsByTagName("script"));

        if (scripts.filter(s => s.src === source).length > 0) {
            return true;
        }
    }

    // Build the script tag that will be dynamically loaded.
    const script = document.createElement("script");
    script.type = "text/javascript";
    script.src = source;
    if (attributes) {
        for (const key in attributes) {
            script.setAttribute(key, attributes[key]);
        }
    }

    // Load the script.
    try {
        await new Promise<void>((resolve, reject) => {
            script.addEventListener("load", () => resolve());
            script.addEventListener("error", () => reject());

            document.getElementsByTagName("head")[0].appendChild(script);
        });

        // If we have a custom function, call it to see if the script loaded correctly.
        if (isScriptLoaded) {
            return isScriptLoaded();
        }

        return true;
    }
    catch {
        return false;
    }
}
