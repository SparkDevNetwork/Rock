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
 * Compares two values for equality by performing deep nested comparisons
 * if the values are objects or arrays.
 * 
 * @param a The first value to compare.
 * @param b The second value to compare.
 * @param strict True if strict comparision is required (meaning 1 would not equal "1").
 *
 * @returns True if the two values are equal to each other.
 */
export function deepEqual(a: unknown, b: unknown, strict: boolean): boolean {
    // Catches everything but objects.
    if (strict && a === b) {
        return true;
    }
    else if (!strict && a == b) {
        return true;
    }

    // NaN never equals another NaN, but functionally they are the same.
    if (typeof a === "number" && typeof b === "number" && isNaN(a) && isNaN(b)) {
        return true;
    }

    // Remaining value types must both be of type object
    if (a && b && typeof a === "object" && typeof b === "object") {
        // Array status must match.
        if (Array.isArray(a) !== Array.isArray(b)) {
            return false;
        }

        if (Array.isArray(a) && Array.isArray(b)) {
            // Array lengths must match.
            if (a.length !== b.length) {
                return false;
            }

            // Each element in the array must match.
            for (let i = 0; i < a.length; i++) {
                if (!deepEqual(a[i], b[i], strict)) {
                    return false;
                }
            }

            return true;
        }
        else {
            // NOTE: There are a few edge cases not accounted for here, but they
            // are rare and unusual:
            // Map, Set, ArrayBuffer, RegExp

            // The objects must be of the same "object type".
            if (a.constructor !== b.constructor) {
                return false;
            }

            // Get all the key/value pairs of each object and sort them so they
            // are in the same order as each other.
            const aEntries = Object.entries(a).sort((a, b) => a[0] < b[0] ? -1 : (a[0] > b[0] ? 1 : 0));
            const bEntries = Object.entries(b).sort((a, b) => a[0] < b[0] ? -1 : (a[0] > b[0] ? 1 : 0));

            // Key/value count must be identical.
            if (aEntries.length !== bEntries.length) {
                return false;
            }

            for (let i = 0; i < aEntries.length; i++) {
                const aEntry = aEntries[i];
                const bEntry = bEntries[i];

                // Ensure the keys are equal, must always be strict.
                if (!deepEqual(aEntry[0], bEntry[0], true)) {
                    return false;
                }

                // Ensure the values are equal.
                if (!deepEqual(aEntry[1], bEntry[1], strict)) {
                    return false;
                }
            }

            return true;
        }
    }

    return false;
}


/**
 * Debounces the function so it will only be called once during the specified
 * delay period. The returned function should be called to trigger the original
 * function that is to be debounced.
 * 
 * @param fn The function to be called once per delay period.
 * @param delay The period in milliseconds. If the returned function is called
 * more than once during this period then fn will only be executed once for
 * the period. If not specified then it defaults to 250ms.
 * @param eager If true then the fn function will be called immediately and
 * then any subsequent calls will be debounced.
 *
 * @returns A function to be called when fn should be executed.
 */
export function debounce(fn: (() => void), delay: number = 250, eager: boolean = false): (() => void) {
    let timeout: NodeJS.Timeout | null = null;

    return (): void => {
        if (timeout) {
            clearTimeout(timeout);
        }
        else if (eager) {
            // If there was no previous timeout and we are configured for
            // eager calls, then execute now.
            fn();

            // An eager call should not result in a final debounce call.
            timeout = setTimeout(() => timeout = null, delay);

            return;
        }

        // If we had a previous timeout or we are not set for eager calls
        // then set a timeout to initiate the function after the delay.
        timeout = setTimeout(() => {
            timeout = null;
            fn();
        }, delay);
    };
}
