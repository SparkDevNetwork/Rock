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

// The session's floating notices: one list, provided once, that any part raises a notice on and
// the one host draws. Its own file because it lives as long as the session, apart from the host
// that draws it, and a script holds no component.

import { computed, ComputedRef, inject, InjectionKey, provide, ref } from "vue";

/** Info passes; a failure stays until it is let go. Only the icon carries the colour. */
export type NoticeTone = "info" | "failure";

/** A button in the pill, such as Undo or Jump to latest. */
export type NoticeAction = {
    label: string;
    onPress: () => void;
};

/** One notice. Raised again under its key, it replaces itself rather than stacking. */
export type NoticeItem = {
    key: string;
    text: string;

    /** The icon's name; a failure's is the alert unless given another. */
    icon?: string;

    tone?: NoticeTone;

    /** A name tests and the page's own scripts find the pill by, such as an error's code. */
    code?: string;

    action?: NoticeAction;

    /** Offers its own dismiss button; a failure always does. */
    isDismissible?: boolean;

    /** Stays until it is let go or taken back, rather than passing after the toast life. */
    isLasting?: boolean;

    /** Told once when the person lets it go or its life ends, never when its owner takes it back. */
    onDismiss?: () => void;

    /**
     * Mirrors its owner's own state, as the shell's failures do: letting it go or its life ending
     * only tells the owner, and it goes when the owner takes it back, so it never disagrees with
     * the state it shows.
     */
    isOwned?: boolean;
};

export type Notices = {
    readonly items: ComputedRef<readonly NoticeItem[]>;

    /** Raises a notice, or replaces the one under its key where it stands. */
    show(item: NoticeItem): void;

    /** The person let it go, or its life ended: its owner is told, and it goes unless its owner holds it. */
    dismiss(key: string): void;

    /** Its owner took it back: it goes quietly. */
    withdraw(key: string): void;
};

/** Whether a notice passes after the toast life: info with nothing to act on, unless asked to stay. */
export function isPassing(item: NoticeItem): boolean {
    return item.tone !== "failure" && !item.action && !item.isLasting;
}

/** Whether a notice offers its own dismiss button. */
export function isDismissible(item: NoticeItem): boolean {
    return item.tone === "failure" || !!item.isDismissible;
}

/** A new, empty list of notices. */
export function createNotices(): Notices {
    const items = ref<NoticeItem[]>([]);

    function take(key: string): NoticeItem | undefined {
        const item = items.value.find(one => one.key === key);
        if (item) {
            items.value = items.value.filter(one => one.key !== key);
        }
        return item;
    }

    return {
        // Read through a computed, so only show, dismiss and withdraw change the list.
        items: computed((): readonly NoticeItem[] => items.value),

        show(item: NoticeItem): void {
            const at = items.value.findIndex(one => one.key === item.key);
            items.value = at < 0 ? [...items.value, item] : items.value.map((one, index) => index === at ? item : one);
        },

        dismiss(key: string): void {
            const item = items.value.find(one => one.key === key);
            if (item && !item.isOwned) {
                take(key);
            }
            item?.onDismiss?.();
        },

        withdraw(key: string): void {
            take(key);
        }
    };
}

const noticesKey: InjectionKey<Notices> = Symbol("chat-notices");

/** Makes the session's notices and provides them to every part below, once, from the shell. */
export function provideNotices(): Notices {
    const notices = createNotices();
    provide(noticesKey, notices);
    return notices;
}

/**
 * The session's notices. A part drawn alone, with no session above it, holds a list of its own,
 * provided to what it draws, so its own host reads the same list.
 */
export function useNotices(): Notices {
    return inject(noticesKey, null) ?? provideNotices();
}
