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

import { inject, provide } from "vue";
import { ValueSources } from "./types";

// Unique key used to track the sources for the FormBuilderDetail block.
const sourcesKey = Symbol();

/**
 * Make the list of value sources available to child components.
 * 
 * @param sources The value sources to make available.
 */
export function provideSources(sources: ValueSources): void {
    provide(sourcesKey, sources);
}

/**
 * Uses the value sources previously made available by the parent component.
 *
 * @returns The value sources that were provided by the parent component.
 */
export function useSources(): ValueSources {
    return inject<ValueSources>(sourcesKey) ?? {};
}
