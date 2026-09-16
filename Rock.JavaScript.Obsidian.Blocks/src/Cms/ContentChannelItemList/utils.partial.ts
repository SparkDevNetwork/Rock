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

import { inject, InjectionKey, provide, Ref } from "vue";

const mediaElementUrlInjectionKey: InjectionKey<Ref<string>> = Symbol("media-element-url");

/**
 * Sets the readonly, reactive media element URL.
 *
 * It can be injected as a dependency into child components with `useMediaElementUrl()`.
 */
export function provideMediaElementUrl(value: Ref<string>): void {
    provide(mediaElementUrlInjectionKey, value);
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
 * Gets the media element URL that can be used to provide reactive behavior.
 */
export function useMediaElementUrl(): Ref<string> {
    return use(mediaElementUrlInjectionKey);
}
