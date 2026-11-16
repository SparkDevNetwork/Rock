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

// The session's floating notices: one list that any part raises a notice on and the one host
// draws. Apart from the host because it lives as long as the session.

import { computed, ComputedRef, inject, InjectionKey, provide, ref } from "vue";

/** Info passes; a failure stays until it is let go, since the person may need to act on it. */
export type NoticeTone = "info" | "failure";

export type NoticeAction = {
    label: string;
    onPress: () => void;
};

/** Raised again under its key, a notice replaces itself rather than stacking. */
export type NoticeItem = {
    key: string;
    text: string;
    icon?: string;
    tone?: NoticeTone;

    /** A name tests and the page's own scripts find the pill by, such as an error's code. */
    code?: string;

    action?: NoticeAction;
    isDismissible?: boolean;
    isLasting?: boolean;

    /** Never told when its owner takes it back, which the owner already knows. */
    onDismiss?: () => void;

    /**
     * Mirrors its owner's state, as the shell's failures do: letting it go only tells the owner,
     * and it goes when the owner takes it back, so it never disagrees with what it shows.
     */
    isOwned?: boolean;
};

export type Notices = {
    readonly items: ComputedRef<readonly NoticeItem[]>;
    show(item: NoticeItem): void;
    dismiss(key: string): void;
    withdraw(key: string): void;
};

/** Info with nothing to act on passes after the toast life, unless asked to stay. */
export function isPassing(item: NoticeItem): boolean {
    return item.tone !== "failure" && !item.action && !item.isLasting;
}

export function isDismissible(item: NoticeItem): boolean {
    return item.tone === "failure" || !!item.isDismissible;
}

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

/** Called once, by the shell, so every part raises on the one list. */
export function provideNotices(): Notices {
    const notices = createNotices();
    provide(noticesKey, notices);
    return notices;
}

/** A part drawn alone provides a list of its own, so the host it draws reads the same list. */
export function useNotices(): Notices {
    return inject(noticesKey, null) ?? provideNotices();
}
