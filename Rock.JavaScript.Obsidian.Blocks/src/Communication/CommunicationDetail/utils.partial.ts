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

import { ComputedRef, InjectionKey, inject, provide, Ref } from "vue";
import { ChartStyles } from "./types.partial";
import { CommunicationType } from "@Obsidian/Enums/Communication/communicationType";
import { CommunicationDetailBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationDetail/communicationDetailBag";

/**
 * Gets the CSS variable value.
 *
 * @param name The name of the CSS variable.
 * @param fallback The fallback value to use.
 */
export function getCssVar(name: string, fallback: string): string {
    const computedStyle = getComputedStyle(document.documentElement)
        .getPropertyValue(name)
        .trim();

    return computedStyle || fallback;
}

/**
 * Injects a provided value.
 * Throws an exception if the value is undefined or not yet provided.
 *
 * @param key The key to use for the injection.
 */
function use<T>(key: string | InjectionKey<T>): T {
    const result = inject<T>(key);

    if (result === undefined) {
        throw `Attempted to access ${key.toString()} before a value was provided.`;
    }

    return result;
}

const communicationDetailInjectionKey: InjectionKey<ComputedRef<CommunicationDetailBag | null | undefined>> = Symbol("communication-detail");

/**
 * Sets the readonly, reactive communication detail.
 *
 * It can be injected as a dependency into child components with `useCommunicationDetail()`.
 */
export function provideCommunicationDetail(value: ComputedRef<CommunicationDetailBag | null | undefined>): void {
    provide(communicationDetailInjectionKey, value);
}

/**
 * Gets the communication detail.
 */
export function useCommunicationDetail(): ComputedRef<CommunicationDetailBag | null | undefined> {
    return use(communicationDetailInjectionKey);
}

const mediumFilterInjectionKey: InjectionKey<ComputedRef<CommunicationType | null>> = Symbol("medium-filter");

/**
 * Sets the readonly, reactive medium filter.
 *
 * It can be injected as a dependency into child components with `useMediumFilter()`.
 */
export function provideMediumFilter(value: ComputedRef<CommunicationType | null>): void {
    provide(mediumFilterInjectionKey, value);
}

/**
 * Gets the medium filter.
 */
export function useMediumFilter(): ComputedRef<CommunicationType | null> {
    return use(mediumFilterInjectionKey);
}

const selectedTabInjectionKey: InjectionKey<Ref<string>> = Symbol("selected-tab");

/**
 * Sets the reactive selected tab.
 *
 * It can be injected as a dependency into child components with `useSelectedTab()`.
 */
export function provideSelectedTab(value: Ref<string>): void {
    provide(selectedTabInjectionKey, value);
}

/**
 * Gets the selected tab.
 */
export function useSelectedTab(): Ref<string> {
    return use(selectedTabInjectionKey);
}

const chartStylesInjectionKey: InjectionKey<ComputedRef<ChartStyles>> = Symbol("chart-styles");

/**
 * Sets the readonly, reactive chart styles.
 *
 * It can be injected as a dependency into child components with `useChartStyles()`.
 */
export function provideChartStyles(value: ComputedRef<ChartStyles>): void {
    provide(chartStylesInjectionKey, value);
}

/**
 * Gets the chart styles.
 */
export function useChartStyles(): ComputedRef<ChartStyles> {
    return use(chartStylesInjectionKey);
}
