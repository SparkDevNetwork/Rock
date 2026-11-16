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

// A list's keys, the one way every list of rows moves: one tab stop for the whole list, held by
// the row last focused, and Up, Down, Home and End between the rows.

import { Ref, ref } from "vue";

/** What a row binds on the element that takes its focus. */
export type ListItemAttrs = {
    tabindex: number;
    onFocus: () => void;
    onKeydown: (event: KeyboardEvent) => void;
};

/** What a list binds on each row. */
export type ListKeys<K> = {
    /** The row's tab stop and handlers. */
    itemAttrs: (key: K) => ListItemAttrs;

    /** The row's element, bound as its ref, which the arrows move focus to. */
    itemRef: (key: K) => (element: unknown) => void;

    /** The row that holds the tab stop, once one has been focused. */
    current: Ref<K | null>;
};

/**
 * A list's keys. The list is one tab stop: the row last focused holds it while it is drawn, the
 * first row while none has been. Up and Down move by one and stop at the ends, Home and End go to
 * them, and the row moved to is kept in view; every other key is the row's own.
 *
 * @param items The rows' keys, in order.
 * @param options What Enter does on a row, for rows that are not buttons; a button's own click otherwise.
 *
 * @returns What each row binds.
 */
export function useListKeys<K>(items: () => readonly K[], options: { onPick?: (key: K) => void } = {}): ListKeys<K> {
    const current = ref(null) as Ref<K | null>;
    const elements = new Map<K, HTMLElement>();

    function isTabStop(key: K): boolean {
        const keys = items();
        const isCurrentShown = current.value !== null && keys.includes(current.value);
        return isCurrentShown ? current.value === key : keys[0] === key;
    }

    function onKeydown(key: K, event: KeyboardEvent): void {
        if (event.key === "Enter" && options.onPick) {
            event.preventDefault();
            options.onPick(key);
            return;
        }

        if (!["ArrowDown", "ArrowUp", "Home", "End"].includes(event.key)) {
            return;
        }

        const keys = items();
        const at = keys.indexOf(key);
        if (at < 0) {
            return;
        }

        event.preventDefault();
        const wanted = event.key === "Home" ? 0 : event.key === "End" ? keys.length - 1 : at + (event.key === "ArrowDown" ? 1 : -1);
        const next = elements.get(keys[Math.max(0, Math.min(keys.length - 1, wanted))]);
        next?.focus();
        next?.scrollIntoView({ block: "nearest" });
    }

    return {
        current,
        itemAttrs: key => ({
            tabindex: isTabStop(key) ? 0 : -1,
            onFocus: (): void => {
                current.value = key;
            },
            onKeydown: (event: KeyboardEvent): void => onKeydown(key, event)
        }),
        itemRef: key => (element: unknown): void => {
            if (element instanceof HTMLElement) {
                elements.set(key, element);
            }
            else {
                elements.delete(key);
            }
        }
    };
}
