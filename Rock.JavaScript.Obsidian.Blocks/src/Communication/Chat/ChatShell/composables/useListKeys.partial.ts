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

import { computed, inject, InjectionKey, provide, Ref, ref, shallowReactive, watch } from "vue";

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

/** How a list's keys behave beyond the arrows. */
export type ListKeysOptions<K> = {
    /** What Enter does on a row, for rows that are not buttons; a button's own click otherwise. */
    onPick?: (key: K) => void;

    /** The row holding the stop while none has been focused; the first row when not said. */
    defaultKey?: () => K | null;

    /**
     * Keys act only on the row itself, never bubbling up from a control inside it, for rows that
     * hold controls of their own whose arrows are theirs: a message's links, chips and bar.
     */
    isRowOnly?: boolean;
};

const rowKeysKey: InjectionKey<ListKeys<string>> = Symbol("chat-row-keys");

/**
 * A list's keys. The list is one tab stop: the row last focused holds it while it is drawn, the
 * default row while none has been. Up and Down move by one and stop at the ends, Home and End go to
 * them, and the row moved to is kept in view; every other key is the row's own.
 *
 * The stop is held as the one key in a set each row asks about its own key alone, so the stop
 * moving wakes the row it leaves and the row it reaches, and the list changing wakes no row: a
 * feed of hundreds of messages redraws none of them as one arrives.
 *
 * @param items The rows' keys, in order.
 * @param options What Enter does, which row holds the stop at first, and whether keys from inside a row are its own.
 *
 * @returns What each row binds.
 */
export function useListKeys<K>(items: () => readonly K[], options: ListKeysOptions<K> = {}): ListKeys<K> {
    const current = ref(null) as Ref<K | null>;
    const elements = new Map<K, HTMLElement>();
    const stop = shallowReactive(new Set<K>());

    /** The row holding the stop: the one last focused while it is drawn, else the default. */
    const stopKey = computed((): K | null => {
        const keys = items();
        if (current.value !== null && keys.includes(current.value)) {
            return current.value;
        }

        return options.defaultKey ? options.defaultKey() : keys[0] ?? null;
    });

    // Moved key by key, never cleared, since clearing the set would wake every row that asks it;
    // before the rows draw, never in the middle of a change to the list the stop is read from.
    watch(stopKey, (next, before) => {
        if (before !== null && before !== undefined) {
            stop.delete(before);
        }
        if (next !== null) {
            stop.add(next);
        }
    }, { immediate: true });

    function onKeydown(key: K, event: KeyboardEvent): void {
        if (options.isRowOnly && event.target !== event.currentTarget) {
            return;
        }

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
            tabindex: stop.has(key) ? 0 : -1,
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

/**
 * Hands a column's rows their list, by each row's key, as the feed and the thread drawer do for
 * their messages.
 *
 * @param keys The column's list.
 *
 * @returns The same list.
 */
export function provideRowKeys(keys: ListKeys<string>): ListKeys<string> {
    provide(rowKeysKey, keys);
    return keys;
}

/**
 * The list of the column a row is drawn in.
 *
 * @returns The list, or null for a row drawn alone, which is then focusable but no tab stop.
 */
export function useRowKeys(): ListKeys<string> | null {
    return inject(rowKeysKey, null);
}
