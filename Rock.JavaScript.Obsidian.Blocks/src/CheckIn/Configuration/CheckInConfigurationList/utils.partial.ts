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

import { ComputedRef, inject, InjectionKey, provide } from "vue";
import { BreakpointHelper } from "./types.partial";

const breakpointHelperInjectionKey: InjectionKey<ComputedRef<BreakpointHelper>> = Symbol("breakpoint-helper");

/**
 * Sets the readonly, reactive breakpoint helper.
 *
 * It can be injected as a dependency into child components with `useBreakpointHelper()`.
 */
export function provideBreakpointHelper(value: ComputedRef<BreakpointHelper>): void {
    provide(breakpointHelperInjectionKey, value);
}

/**
 * Injects a provided value.
 * Throws an exception if the value is undefined or not yet provided.
 */
function use<T>(key: string | InjectionKey<T>): T {
    const result = inject<T>(key);

    if (result === undefined) {
        throw `Attempted to access ${key.toString()} before a value was provided.`;
    }

    return result;
}

/**
 * Gets the breakpoint helper that can be used to provide responsive behavior.
 */
export function useBreakpointHelper(): ComputedRef<BreakpointHelper> {
    return use(breakpointHelperInjectionKey);
}
