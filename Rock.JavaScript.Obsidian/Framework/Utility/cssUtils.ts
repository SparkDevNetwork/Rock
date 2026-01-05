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

/**
 * Gets the CSS variable value.
 *
 * @param cssVariableName The name of the CSS variable.
 * @param fallbackValue The optional fallback value to use.
 * @param targetElement The optional target element to get the CSS variable
 * value from. Can be a CSS selector to query the element or a reference to
 * the HTML element itself. If not provided, defaults to the document root.
 *
 * @returns The value of the CSS variable, the fallback value if not found, or
 * an empty string if neither are defined.
 */
export function getCssVariableValue(cssVariableName: string, fallbackValue?: string, targetElement?: string | HTMLElement): string {
    const element = targetElement ?
        (
            typeof targetElement === "string" ?
                document.querySelector(targetElement)
                : targetElement
        )
        : document.documentElement;

    if (!element) {
        return fallbackValue || "";
    }

    const computedStyle = getComputedStyle(element)
        .getPropertyValue(cssVariableName)
        .trim();

    return computedStyle || fallbackValue || "";
}
