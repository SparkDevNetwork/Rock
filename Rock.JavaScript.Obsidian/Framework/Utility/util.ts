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

import { CancellationTokenSource, ICancellationToken } from "./cancellation";

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

/**
 * Options for debounceAsync function
 */
type DebounceAsyncOptions = {
    /**
     * The period in milliseconds. If the returned function is called more than
     * once during this period, then `fn` will only be executed once for the
     * period.
     * @default 250
     */
    delay?: number;

    /**
     * If `true`, then the `fn` function will be called immediately on the first
     * call, and any subsequent calls will be debounced.
     * @default false
     */
    eager?: boolean;
};

/**
 * Debounces the function so it will only be called once during the specified
 * delay period. The returned function should be called to trigger the original
 * function that is to be debounced.
 *
 * **Note:** Due to the asynchronous nature of JavaScript and how promises work,
 * `fn` may be invoked multiple times before previous invocations have completed.
 * To ensure that only the latest invocation proceeds and to prevent stale data,
 * you should check `cancellationToken.isCancellationRequested` at appropriate
 * points within your `fn` implementation—ideally after you `await` data from the
 * server. If cancellation is requested, `fn` should promptly abort execution.
 *
 * @param fn The function to be called once per delay period.
 * @param options An optional object specifying debounce options.
 *
 * @returns A function to be called when `fn` should be executed. This function
 * accepts an optional `parentCancellationToken` that, when canceled, will also
 * cancel the execution of `fn`.
 */
export function debounceAsync(
    fn: ((cancellationToken?: ICancellationToken) => PromiseLike<void>),
    options?: DebounceAsyncOptions
): ((parentCancellationToken?: ICancellationToken) => Promise<void>) {
    const delay = options?.delay ?? 250;
    const eager = options?.eager ?? false;

    let timeout: NodeJS.Timeout | null = null;
    let source: CancellationTokenSource | null = null;
    let isEagerExecutionInProgress = false;

    return async (parentCancellationToken?: ICancellationToken): Promise<void> => {
        // Always cancel any ongoing execution of fn.
        source?.cancel();

        if (timeout) {
            clearTimeout(timeout);
            timeout = null;
        }
        else if (eager && !isEagerExecutionInProgress) {
            // Execute immediately on the first call.
            isEagerExecutionInProgress = true;
            source = new CancellationTokenSource(parentCancellationToken);

            // Set the timeout before awaiting fn.
            timeout = setTimeout(() => {
                timeout = null;
                isEagerExecutionInProgress = false;
            }, delay);

            try {
                await fn(source.token);
            }
            catch (e) {
                console.error(e || "Unknown error while debouncing async function call.");
                throw e;
            }

            return;
        }

        // Schedule the function to run after the delay.
        source = new CancellationTokenSource(parentCancellationToken);
        const cts = source;
        timeout = setTimeout(async () => {
            try {
                await fn(cts.token);
            }
            catch (e) {
                console.error(e || "Unknown error while debouncing async function call.");
                throw e;
            }

            timeout = null;
            isEagerExecutionInProgress = false;
        }, delay);
    };
}
