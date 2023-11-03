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

import { RockColor } from "@Obsidian/Core/Utilities/rockColor";

const valueDelimiter = "|";
const isValidColorRegex = /^#(([0-9a-fA-F]{3})|([0-9a-fA-F]{4})|([0-9a-fA-F]{6})|([0-9a-fA-F]{8}))$/;

/**
 * Tests if the provided string is a valid hexadecimal color.
 * Validate formats are:
 * 1. #RGB
 * 2. #RGBA
 * 3. #RRGGBB
 * 4. #RRGGBBAA
 * @param value The value to test.
 */
export function isValidHexColor(value: string): boolean {
    return isValidColorRegex.test(value);
}

export function deserializeColors(value: string): string[] {
    return value.split(valueDelimiter);
}

export function serializeColors(values: string[]): string {
    return values.join(valueDelimiter);
}

export function deserializeValue(value: string): string[] {
    return value.split(valueDelimiter);
}

export function serializeValue(values: string[]): string {
    return values.join(valueDelimiter);
}

export function deserializeAllowMultiple(value: string): boolean {
    return value?.toLowerCase() === "true";
}

export function serializeAllowMultiple(allowMultiple: boolean): string {
    return allowMultiple.toString();
}

export function getSelectedColors(colors: string[], values: string[]): string[] {
    return colors.filter(color => values.some(value => value.toLowerCase() === color.toLowerCase()));
}

/**
 * Sets the camouflaged class if the element color is similar to the target element (or parent if not set).
 * @param {Element} element The element to compare.
 * @param {Element} ancestorElement (Optional) The target element with which to compare colors. Defaults to the parent element.
 */
export function setCamouflagedClass(element: Element, ancestorElement?: Element | null): void {
    if (!ancestorElement) {
        ancestorElement = element.parentElement;

        if (!ancestorElement) {
            return;
        }
    }

    const elementColor = getBackgroundColor(element);
    const ancestorElementColor = getBackgroundColor(ancestorElement);

    if (elementColor.isSimilarTo(ancestorElementColor)) {
        element.classList.add(elementColor.luma > 0.5 ? "camouflaged-light" : "camouflaged-dark");
    }
    else {
        element.classList.remove("camouflaged-light");
        element.classList.remove("camouflaged-dark");
    }
}

function getBackgroundColor(element: Element): RockColor {
    function getComputedBackgroundColor(element: Element): string {
        return window.getComputedStyle(element).getPropertyValue("background-color");
    }

    // typically "rgba(0, 0, 0, 0)"
    const defaultColor = new RockColor(getComputedBackgroundColor(document.body));

    // Start with the supplied element.
    const elementsToProcess: Element[] = [
        element
    ];

    while (elementsToProcess.length) {
        // Process the first element in the array.
        const elementToProcess = elementsToProcess.shift();

        if (!elementToProcess) {
            // Skip to the next element if the current one is null or undefined.
            continue;
        }

        const backgroundColor = new RockColor(getComputedBackgroundColor(elementToProcess));

        // If we got a different value than the default, return it.
        if (backgroundColor.alpha !== 0 && backgroundColor.compareTo(defaultColor) !== 0) {
            return backgroundColor;
        }
        // Otherwise, add the parent element to the elements in process.
        else if (elementToProcess.parentElement) {
            elementsToProcess.push(elementToProcess.parentElement);
        }
    }

    // If we reached the top parent element and no unique color was found,
    // then return the default color.
    return defaultColor;
}