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

import { inject, InjectionKey, provide } from "vue";

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
