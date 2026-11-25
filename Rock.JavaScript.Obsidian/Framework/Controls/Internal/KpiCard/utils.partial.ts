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

import { InjectionKey, Ref } from "vue";

/**
 * Lets the cards in a KPI container share one value size. A card that needs the
 * small size (set explicitly, or too narrow for the default size) registers that
 * need, and every card without an explicit size follows the container's answer.
 */
export type KpiContainerValueSize = {
    /** `true` when any card in the container needs the small value size. */
    isSmall: Ref<boolean>;

    /**
     * Registers whether a card needs the small value size.
     *
     * @param key A key unique to the card.
     * @param needsSmall `true` while the card needs the small value size.
     */
    register(key: string, needsSmall: Ref<boolean>): void;

    /**
     * Removes a card's registration.
     *
     * @param key The key the card registered with.
     */
    unregister(key: string): void;
};

/**
 * Provided by KpiContainer and injected by each KpiCard inside it. Symbol.for because the
 * two controls are separate bundles that each get their own copy of this file.
 */
export const KpiContainerValueSizeKey: InjectionKey<KpiContainerValueSize> = Symbol.for("kpi-container-value-size");

/** Compact cards narrower than this can't fit the default value size beside a chart. */
export const KpiCompactSmallValueWidth = 360;
