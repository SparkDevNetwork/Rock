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

import { ComputedRef, inject, InjectionKey, provide, Ref } from "vue";
import { BreakpointHelper } from "./types.partial";
import { Guid } from "@Obsidian/Types";

/**
 * The `idKey` used for an unsaved placeholder node. Matches Rock's
 * `?Foo=0` convention for "create new" routes.
 */
export const placeholderNodeKey = "0";

/** Returns true when `key` belongs to an unsaved placeholder node. */
export function isPlaceholderKey(key: string | null | undefined): boolean {
    return key === placeholderNodeKey;
}

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

const navigationGuardInjectionKey: InjectionKey<() => Promise<boolean>> = Symbol("navigation-guard");

/**
 * Provides an async navigation guard for descendant components. The guard
 * returns true when navigation may proceed, or false when the individual
 * opted to stay (e.g., declined to discard unsaved edits).
 */
export function provideNavigationGuard(guard: () => Promise<boolean>): void {
    provide(navigationGuardInjectionKey, guard);
}

/**
 * Gets the async navigation guard. Call this before initiating any action
 * that would navigate away from the currently-open editor; only proceed
 * when the resolved value is true.
 */
export function useNavigationGuard(): () => Promise<boolean> {
    return use(navigationGuardInjectionKey);
}

const updateEditorIsDirtyInjectionKey: InjectionKey<(isDirty: boolean) => void> = Symbol("update-editor-is-dirty");

/**
 * Provides the function descendant editors call whenever their isDirty value
 * changes. The provider stores the latest value so the navigation guard can
 * consult it without a v-model chain through intermediate partials.
 */
export function provideUpdateEditorIsDirty(updater: (isDirty: boolean) => void): void {
    provide(updateEditorIsDirtyInjectionKey, updater);
}

/**
 * Gets the function used to update the editor's isDirty value. The editor calls
 * the returned function whenever its dirty state flips (becoming dirty or clean).
 */
export function useUpdateEditorIsDirty(): (isDirty: boolean) => void {
    return use(updateEditorIsDirtyInjectionKey);
}

const selectedCampusInjectionKey: InjectionKey<Ref<Guid | null>> = Symbol("selected-campus");

/**
 * Provides the campus the slicer is set to (null for "All Campuses") so
 * descendant editors can filter campus-scoped data to it. Reactive: the block
 * updates the ref when the campus context changes and owns the single
 * browser-bus subscription that drives it.
 */
export function provideSelectedCampus(value: Ref<Guid | null>): void {
    provide(selectedCampusInjectionKey, value);
}

/**
 * Gets the campus the slicer is set to. Null means no campus context
 * ("All Campuses"), so descendants show every campus's data.
 */
export function useSelectedCampus(): Ref<Guid | null> {
    return use(selectedCampusInjectionKey);
}

const campusRootLocationGuidInjectionKey: InjectionKey<Ref<Guid | null>> = Symbol("campus-root-location-guid");

/**
 * Provides the selected campus's root named-location Guid (null for "All
 * Campuses") so descendant location pickers can scope themselves to the campus
 * the slicer is set to. Reactive: the block updates the ref when the campus
 * context changes and owns the single browser-bus subscription that drives it.
 */
export function provideCampusRootLocationGuid(value: Ref<Guid | null>): void {
    provide(campusRootLocationGuidInjectionKey, value);
}

/**
 * Gets the selected campus's root named-location Guid for scoping a location
 * picker. Null means no campus context ("All Campuses"), so the picker shows
 * the full location tree.
 */
export function useCampusRootLocationGuid(): Ref<Guid | null> {
    return use(campusRootLocationGuidInjectionKey);
}
