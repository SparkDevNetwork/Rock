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

import { AsyncComponentLoader, Component, ComponentPublicInstance, defineAsyncComponent as vueDefineAsyncComponent, ref, Ref, watch, WatchOptions } from "vue";
import { deepEqual } from "./util";
import { useSuspense } from "./suspense";
import { newGuid } from "./guid";

type Prop = { [key: string]: unknown };
type PropKey<T extends Prop> = Extract<keyof T, string>;
// eslint-disable-next-line @typescript-eslint/no-explicit-any
type EmitFn<E extends `update:${string}`> = E extends Array<infer EE> ? (event: EE, ...args: any[]) => void : (event: E, ...args: any[]) => void;

/**
 * Utility function for when you are using a component that takes a v-model
 * and uses that model as a v-model in that component's template. It creates
 * a new ref that keeps itself up-to-date with the given model and fires an
 * 'update:MODELNAME' event when it gets changed.
 *
 * Ensure the related `props` and `emits` are specified to ensure there are
 * no type issues.
 */
export function useVModelPassthrough<T extends Prop, K extends PropKey<T>, E extends `update:${K}`>(props: T, modelName: K, emit: EmitFn<E>, options?: WatchOptions): Ref<T[K]> {
    const internalValue = ref(props[modelName]) as Ref<T[K]>;

    watch(() => props[modelName], val => internalValue.value = val, options);
    watch(internalValue, val => emit(`update:${modelName}`, val), options);

    return internalValue;
}

/**
 * Updates the Ref value, but only if the new value is actually different than
 * the current value. A strict comparison is performed.
 * 
 * @param target The target Ref object to be updated.
 * @param value The new value to be assigned to the target.
 *
 * @returns True if the target was updated, otherwise false.
 */
export function updateRefValue<T>(target: Ref<T>, value: T): boolean {
    if (deepEqual(target.value, value, true)) {
        return false;
    }

    target.value = value;

    return true;
}

/**
 * Defines a component that will be loaded asynchronously. This contains logic
 * to properly work with the RockSuspense control.
 * 
 * @param source The function to call to load the component.
 *
 * @retunrs The component that was loaded.
 */
export function defineAsyncComponent<T extends Component = { new(): ComponentPublicInstance }>(source: AsyncComponentLoader<T>): T {
    return vueDefineAsyncComponent(async () => {
        const suspense = useSuspense();
        const operationKey = newGuid();

        suspense?.startAsyncOperation(operationKey);
        const component = await source();
        suspense?.completeAsyncOperation(operationKey);

        return component;
    });
}
