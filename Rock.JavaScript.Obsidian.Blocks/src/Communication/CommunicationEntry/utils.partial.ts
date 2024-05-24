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

import { AlertType } from "@Obsidian/Enums/Controls/alertType";
import { nextTick } from "vue";

/** Converts a string to an alert type. */
export function getAlertType(type: string | null | undefined): AlertType {
    return type as AlertType ?? AlertType.Default;
}

/**
 * Scrolls an element into view.
 */
export function scrollIntoView(elementGetter: () => (Element | undefined)): void {
    console.log("trying to scroll to element");
    if (!elementGetter) {
        console.log("scroll to element failed: no element getter defined");
        // Nothing to scroll to.
        return;
    }

    // Need to wait until next tick to get the element as it may not exist yet.
    nextTick(() => {
        const element = elementGetter();
        console.log("about to scroll to ", element);
        if (element && typeof element["scrollIntoView"] === "function") {
            console.log("scrolling to element", element);
            element.scrollIntoView();
            console.log("scrolled to element", element);
        }
        else {
            console.log("scroll to element failed: element doesn't support 'scrollIntoView'", element);
        }
    });
}

/**
 * Scrolls to the top of the window without scrolling horizontally.
 */
export function scrollToTopOfWindow(): void {
    window.scrollTo(window.screenX, 0);
}