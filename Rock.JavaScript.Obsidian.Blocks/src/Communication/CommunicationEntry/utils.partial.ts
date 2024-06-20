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
export function use<T>(key: string | InjectionKey<T>): T {
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

// TODO JMH Remove this debug option.
export const KeepPersonPickerOpen: InjectionKey<Ref<boolean>> = Symbol("keep-person-picker-open");


export function getArrayDiff<Key, Value>(oldValues: Value[], keySelector: ((item: Value) => Key | undefined), newKeys: Key[]): { readonly removedKeys: Key[]; readonly addedKeys: Key[]; readonly values: Value[]; removeValues(keys: Key[]): void; addValues(values: Value[]): void; } {
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
    const keysToAdd: Key[] = [];
    const valueToRemoveDictionary = new Map<Key, Value>(oldValueDictionary);
    for (const newKey of newKeys) {
        if (!newValueDictionary.has(newKey)) {
            keysToAdd.push(newKey);
        }
        else {
            // oldValuesToRemove starts as a copy of the current values.
            // For each value that should be kept, it gets removed from oldValuesToRemove.
            // After processing all *new* values, oldValuesToRemove should be left
            // with only the values that should be removed.
            valueToRemoveDictionary.delete(newKey);
        }
    }

    return {
        get removedKeys(): Key[] {
            return Array.from(valueToRemoveDictionary.keys());
        },
        get addedKeys(): Key[] {
            return keysToAdd;
        },
        get values(): Value[] {
            return Array.from(newValueDictionary.values());
        },
        removeValues(keys: Key[]): void {
            if (valueToRemoveDictionary.size) {
                for (const key of keys) {
                    newValueDictionary.delete(key);
                }
            }
        },
        addValues(values: Value[]): void {
            for (const value of values) {
                const key = keySelector(value);
                if (key) {
                    newValueDictionary.set(key, value);
                }
            }
        }
    };
}

export function updateArray<Key, Value>(oldValues: Value[], keySelector: ((item: Value) => Key | undefined), newKeys: Key[], getNewValues: ((keys: Key[]) => Promise<Value[]>)): Promise<Value[]> {
    const arrayDiff = getArrayDiff(oldValues, keySelector, newKeys);
    arrayDiff.removeValues(arrayDiff.removedKeys);
    const keysToAdd = arrayDiff.addedKeys;

    // Add values.
    if (keysToAdd.length) {
        return getNewValues(keysToAdd)
            .then(valuesToAdd => {
                arrayDiff.addValues(valuesToAdd);
                return arrayDiff.values;
            });
    }
    else {
        return Promise.resolve(arrayDiff.values);
    }
}