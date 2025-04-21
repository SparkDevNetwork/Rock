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
import { CommunicationEntryCommunicationBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationEntry/communicationEntryCommunicationBag";

/** Converts a string to an alert type. */
export function getAlertType(type: string | null | undefined): AlertType {
    return type as AlertType ?? AlertType.Default;
}

/**
 * Scrolls an element into view.
 */
export function scrollIntoView(elementGetter: () => (Element | undefined)): void {
    if (!elementGetter) {
        // Nothing to scroll to.
        return;
    }

    // Need to wait until next tick to get the element as it may not exist yet.
    nextTick(() => {
        const element = elementGetter();
        if (element && typeof element["scrollIntoView"] === "function") {
            element.scrollIntoView();
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
export function use<T>(key: string | InjectionKey<T>): T {
    const result = inject<T>(key);

    if (result === undefined) {
        throw `Attempted to access ${key.toString()} before a value was provided.`;
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

export function getArrayDiff<Key, Value>(oldValues: Iterable<Value>, keySelector: ((item: Value) => Key | undefined), newKeys: Iterable<Key>): {
    readonly removedKeys: Set<Key>;
    readonly addedKeys: Set<Key>;
    readonly values: Map<Key, Value>;
    removeValues(keys: Iterable<Key>): void;
    addValues(values: Iterable<Value>): void;
} {
    // Copy the old values to a dictionary for faster processing.
    const oldValueDictionary = new Map<Key, Value>();
    for (const oldValue of oldValues) {
        const key = keySelector(oldValue);
        if (key) {
            oldValueDictionary.set(key, oldValue);
        }
    }

    // Copy the old values. Values will be added/removed accordingly.
    const newValueDictionary = new Map<Key, Value>(oldValueDictionary);

    // Figure out which values were added/removed.
    const keysToAdd = new Set<Key>();
    const valueToRemoveDictionary = new Set<Key>(oldValueDictionary.keys());
    for (const newKey of newKeys) {
        if (!newValueDictionary.has(newKey)) {
            keysToAdd.add(newKey);
        }
        else {
            // valueToRemoveDictionary starts as a copy of the current values.
            // For each value that should be kept, it gets removed from valueToRemoveDictionary.
            // After processing all *new* values, valueToRemoveDictionary should be left
            // with only the values that should be removed.
            valueToRemoveDictionary.delete(newKey);
        }
    }

    return {
        get removedKeys(): Set<Key> {
            return valueToRemoveDictionary;
        },
        get addedKeys(): Set<Key> {
            return keysToAdd;
        },
        get values(): Map<Key, Value> {
            return newValueDictionary;
        },
        removeValues(keys: Iterable<Key>): void {
            for (const key of keys) {
                newValueDictionary.delete(key);
            }
        },
        addValues(values: Iterable<Value>): void {
            for (const value of values) {
                const key = keySelector(value);
                if (key) {
                    newValueDictionary.set(key, value);
                }
            }
        }
    };
}

export async function updateArray<Key, Value>(oldValues: Iterable<Value>, keySelector: ((item: Value) => Key | undefined), newKeys: Iterable<Key>, getNewValues: ((keys: Iterable<Key>) => Promise<Value[]>)): Promise<Map<Key, Value>> {
    const arrayDiff = getArrayDiff(oldValues, keySelector, newKeys);
    arrayDiff.removeValues(arrayDiff.removedKeys.values());
    const keysToAdd = arrayDiff.addedKeys;

    // Add values.
    if (keysToAdd.size) {
        const valuesToAdd = await getNewValues(keysToAdd.keys());
        arrayDiff.addValues(valuesToAdd.values());
        return arrayDiff.values;
    }
    else {
        return Promise.resolve(arrayDiff.values);
    }
}

export function removeQueryParams(url: URL, ...queryParamKeys: string[]): void {
    const queryParams = url.searchParams;

    for (let i = 0; i < queryParamKeys.length; i++) {
        const queryParamKey = queryParamKeys[i];
        queryParams.delete(queryParamKey);
    }
}

const templateInjectionKey: InjectionKey<Ref<CommunicationEntryCommunicationBag | null | undefined>> = Symbol("communication-entry-template");

export function provideTemplate(template: Ref<CommunicationEntryCommunicationBag | null | undefined>): void {
    provide(templateInjectionKey, template);
}

export function useTemplate(): Ref<CommunicationEntryCommunicationBag | null | undefined> {
    return use(templateInjectionKey);
}
