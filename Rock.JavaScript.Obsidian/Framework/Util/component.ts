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

import { ref, Ref, watch } from "vue";

type Prop = { [key: string]: unknown };
type PropKey<T extends Prop> = Extract<keyof T, string>;
type EmitFn<T extends string> = (event: `update:${T}`, ...args: any[]) => void

/**
 * Utility function for when you are using a component that takes a v-model
 * and uses that model as a v-model in that component's template. It creates
 * a new ref that keeps itself up-to-date with the given model and fires an
 * 'update:MODELNAME' event when it gets changed.
 *
 * Ensure the related `props` and `emits` are specified to ensure there are
 * no type issues.
 */
export function useVModelPassthrough<T extends Prop, K extends PropKey<T>>(props: T, modelName: K, emit: EmitFn<K>) : Ref<T[K]> {
    let internalValue = ref(props[modelName]) as Ref<T[K]>;

    watch(() => props[modelName], val => internalValue.value = val);
    watch(internalValue, val => emit(`update:${modelName}`, val));

    return internalValue;
}
