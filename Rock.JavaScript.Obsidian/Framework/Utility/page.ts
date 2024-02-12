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

import { Guid } from "@Obsidian/Types";
import { CurrentPersonBag } from "@Obsidian/ViewModels/Crm/currentPersonBag";

export type PageConfig = {
    executionStartTime: number;
    pageId: number;
    pageGuid: Guid;
    pageParameters: Record<string, string>;
    currentPerson: CurrentPersonBag | null;
    isAnonymousVisitor: boolean;
    loginUrlWithReturnUrl: string;
};

export function smoothScrollToTop(): void {
    window.scrollTo({ top: 0, behavior: "smooth" });
}

export default {
    smoothScrollToTop
};

// eslint-disable-next-line @typescript-eslint/naming-convention, @typescript-eslint/no-explicit-any
declare const Obsidian: any;


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
 * @param fingerprint If set to false, then a fingerprint will not be added to the source URL. Default is true.
 *
 * @returns A Promise that indicates if the script was loaded or not.
 */
export async function loadJavaScriptAsync(source: string, isScriptLoaded?: () => boolean, attributes?: Record<string, string>, fingerprint?: boolean): Promise<boolean> {
    let src = source;

    // Add the cache busting fingerprint if we have one.
    if (fingerprint !== false && typeof Obsidian !== "undefined" && Obsidian?.options?.fingerprint) {
        if (src.indexOf("?") === -1) {
            src += `?${Obsidian.options.fingerprint}`;
        }
        else {
            src += `&${Obsidian.options.fingerprint}`;
        }
    }

    // Check if the script is already loaded. First see if we have a custom
    // function that will do the check. Otherwise fall back to looking for any
    // script tags that have the same source.
    if (isScriptLoaded) {
        if (isScriptLoaded()) {
            return true;
        }
    }

    // Make sure the script wasn't already added in some other way.
    const scripts = Array.from(document.getElementsByTagName("script"));
    const thisScript = scripts.filter(s => s.src === src);

    if (thisScript.length > 0) {
        const promise = scriptLoadedPromise(thisScript[0]);
        return promise;
    }

    // Build the script tag that will be dynamically loaded.
    const script = document.createElement("script");
    script.type = "text/javascript";
    script.src = src;
    if (attributes) {
        for (const key in attributes) {
            script.setAttribute(key, attributes[key]);
        }
    }

    // Load the script.
    const promise = scriptLoadedPromise(script);
    document.getElementsByTagName("head")[0].appendChild(script);

    return promise;

    async function scriptLoadedPromise(scriptElement: HTMLScriptElement): Promise<boolean> {
        try {
            await new Promise<void>((resolve, reject) => {
                scriptElement.addEventListener("load", () => resolve());
                scriptElement.addEventListener("error", () => {
                    reject();
                });
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
}

/**
 * Adds a new link to the quick return action menu. The URL in the address bar
 * will be used as the destination.
 *
 * @param title The title of the quick link that identifies the current page.
 * @param section The section title to place this link into.
 * @param sectionOrder The priority order to give the section if it doesn't already exist.
 */
export function addQuickReturn(title: string, section: string, sectionOrder?: number): void {
    interface IRock {
        personalLinks: {
            addQuickReturn: (type: string, typeOrder: number, itemName: string) => void
        }
    }

    (window["Rock"] as IRock).personalLinks.addQuickReturn(section, sectionOrder ?? 0, title);
}
