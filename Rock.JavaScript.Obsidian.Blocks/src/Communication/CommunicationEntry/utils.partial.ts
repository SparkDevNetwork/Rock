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
import { ComputedRef, InjectionKey, Ref, inject, nextTick, provide } from "vue";
import { Breakpoint, BreakpointHelper, MediumType } from "./types.partial";

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

/**
 * Injects a provided value.
 * Throws an exception if the value is undefined or not yet provided.
 */
function use<T>(key: string | InjectionKey<T>): T {
    const result = inject<T>(key);

    if (result === undefined) {
        throw `Attempted to access ${key} before a value was provided.`;
    }

    return result;
}

const selectedMediumTypeInjectionKey: InjectionKey<ComputedRef<MediumType>> = Symbol("selected-medium-type");

/**
 * Sets the readonly, reactive selected medium type.
 *
 * It can be injected as a dependency into child components with `useSelectedMediumType()`.
 */
export function provideSelectedMediumType(value: ComputedRef<MediumType>): void {
    provide(selectedMediumTypeInjectionKey, value);
}

/**
 * Gets the selected medium type.
 */
export function useSelectedMediumType(): ComputedRef<MediumType> {
    return use(selectedMediumTypeInjectionKey);
}

const breakpointInjectionKey: InjectionKey<ComputedRef<Breakpoint>> = Symbol("breakpoint");

const breakpointHelperInjectionKey: InjectionKey<ComputedRef<BreakpointHelper>> = Symbol("breakpoint-helper");

/**
 * Sets the readonly, reactive breakpoint.
 *
 * It can be injected as a dependency into child components with `useBreakpoint()`.
 */
export function provideBreakpoint(value: ComputedRef<Breakpoint>): void {
    provide(breakpointInjectionKey, value);
}

/**
 * Sets the readonly, reactive breakpoint helper.
 *
 * It can be injected as a dependency into child components with `useBreakpointHelper()`.
 */
export function provideBreakpointHelper(value: ComputedRef<BreakpointHelper>): void {
    provide(breakpointHelperInjectionKey, value);
}

/**
 * Gets the breakpoint that can be used to provide responsive behavior.
 */
export function useBreakpoint(): ComputedRef<Breakpoint> {
    return use(breakpointInjectionKey);
}

/**
 * Gets the breakpoint helper that can be used to provide responsive behavior.
 */
export function useBreakpointHelper(): ComputedRef<BreakpointHelper> {
    return use(breakpointHelperInjectionKey);
}