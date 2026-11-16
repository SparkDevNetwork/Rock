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
// The read position, a conversation's and an open thread's alike. It is the busiest write chat
// has, so it is saved on events only and never on a timer: when the person leaves the
// conversation or thread, when the tab or app goes to the background or the window loses focus,
// and when the page closes. What is saved is the newest message that was actually on screen, and
// nothing is sent when nothing newer was seen since the last save. A person who sits in a busy
// channel writes nothing until they leave it.
//
// The save goes out with fetch and keepalive so that one made as the page closes still arrives;
// a beacon cannot carry the person's token.
import { createPlatformCall } from "../platformCall.partial";
import { MarkReadResult } from "../types.partial";

/**
 * What the tracker reaches outside itself, for what it tracks by its key (a conversation by its
 * channel, a thread by its root) and what a save answers.
 */
export type ReadTrackerDependencies<K = string, A = MarkReadResult> = {
    /** Saves the position. Resolves null when the save failed. */
    save: (key: K, messageId: number) => Promise<A | null>;

    /** A save came back with its answer: the position as stored, and what it says of the unread. */
    onSaved: (key: K, answer: A) => void;
};

/** The parts of the page the tracker listens to. */
export type PageEventTargets = {
    document: {
        visibilityState: string;
        addEventListener: (type: string, listener: () => void) => void;
        removeEventListener: (type: string, listener: () => void) => void;
    };
    // The window hands each listener its event; the drafts read a storage event's key from it.
    // Online status listens for input on it in the capture phase, passively, so it hears every
    // input on the page, inside any part of it, without slowing any.
    window: {
        addEventListener: (type: string, listener: (event: unknown) => void, options?: { capture?: boolean, passive?: boolean }) => void;
        removeEventListener: (type: string, listener: (event: unknown) => void, options?: { capture?: boolean }) => void;
    };
};

/** A read tracker, for a conversation or a thread. */
export type ReadTracker<K = string> = {
    /** It was put on screen, before the position the platform holds for it is known. */
    show: (key: K) => void;

    /** It was opened, or its stored position learned, with the position the platform holds for it. */
    open: (key: K, storedCursor: number | null) => void;

    /** A message of what is open was on screen. */
    seen: (messageId: number) => void;

    /** Saves the open position if something newer was seen. */
    flush: () => Promise<void>;

    /** Saves, then stops tracking what was open. */
    leave: () => Promise<void>;

    /** Saves on background, lost focus and close. Returns what removes the listeners. */
    attach: (targets: PageEventTargets) => () => void;
};

/**
 * Creates a read tracker.
 *
 * @param dependencies What the tracker reaches outside itself.
 *
 * @returns The tracker.
 */
export function createReadTracker<K = string, A = MarkReadResult>(dependencies: ReadTrackerDependencies<K, A>): ReadTracker<K> {
    let activeKey: K | null = null;
    let newestSeen: number | null = null;
    let lastSaved: number | null = null;

    /** Saves the open position if something newer than the last save was seen. */
    async function flush(): Promise<void> {
        const key = activeKey;
        const messageId = newestSeen;

        if (key === null || messageId === null || (lastSaved !== null && messageId <= lastSaved)) {
            return;
        }

        // Taken as saved before the answer, so a second event while this save is in flight,
        // a close right after losing focus for one, sends nothing more.
        const previous = lastSaved;
        lastSaved = messageId;

        const result = await dependencies.save(key, messageId);

        if (!result) {
            // The next event tries again, unless the person has moved on since.
            if (activeKey === key && lastSaved === messageId) {
                lastSaved = previous;
            }
            return;
        }

        dependencies.onSaved(key, result);
    }

    /** The larger of two positions, either of which may be unknown. */
    const newer = (a: number | null, b: number | null): number | null => a === null ? b : b === null ? a : Math.max(a, b);

    return {
        // Tracked from the moment it is on screen: held messages, or a page that lands before the
        // open has finished catching up, are seen before the stored position is known, and the
        // feed reports a message once, when it comes into view. A save made meanwhile is safe,
        // since the platform only ever moves a position forward.
        show: (key: K): void => {
            activeKey = key;
            newestSeen = null;
            lastSaved = null;
        },

        open: (key: K, storedCursor: number | null): void => {
            const isShown = activeKey === key;
            activeKey = key;
            lastSaved = isShown ? newer(lastSaved, storedCursor) : storedCursor;
            newestSeen = isShown ? newer(newestSeen, storedCursor) : storedCursor;
        },

        seen: (messageId: number): void => {
            if (activeKey !== null && (newestSeen === null || messageId > newestSeen)) {
                newestSeen = messageId;
            }
        },

        flush,

        leave: async (): Promise<void> => {
            const key = activeKey;
            const messageId = newestSeen;
            const saved = lastSaved;

            // Let go at once, so what is opened while this save is in flight is tracked from its
            // own position and nothing seen in it is taken for what was left.
            activeKey = null;
            newestSeen = null;
            lastSaved = null;

            if (key === null || messageId === null || (saved !== null && messageId <= saved)) {
                return;
            }

            const result = await dependencies.save(key, messageId);
            if (result) {
                dependencies.onSaved(key, result);
            }
        },

        attach: (targets: PageEventTargets): (() => void) => {
            const onVisibility = (): void => {
                if (targets.document.visibilityState === "hidden") {
                    void flush();
                }
            };
            const onLeave = (): void => {
                void flush();
            };

            targets.document.addEventListener("visibilitychange", onVisibility);
            targets.window.addEventListener("blur", onLeave);
            targets.window.addEventListener("pagehide", onLeave);

            return () => {
                targets.document.removeEventListener("visibilitychange", onVisibility);
                targets.window.removeEventListener("blur", onLeave);
                targets.window.removeEventListener("pagehide", onLeave);
            };
        }
    };
}

/** What the keepalive save needs. */
export type KeepaliveSaveOptions = {
    fetch: (url: string, init: RequestInit) => Promise<Response>;
    projectUrl: string;
    publishableKey: string;

    /** The current platform token, read at the moment of the save. */
    currentToken: () => string | null;
};

/**
 * Creates a save that posts the position straight to the platform with keepalive.
 *
 * @param options What the save needs.
 *
 * @returns The save.
 */
export function createKeepaliveSave(options: KeepaliveSaveOptions): ReadTrackerDependencies["save"] {
    // The same call path as every other platform call; the save has only the one route.
    const platform = createPlatformCall({ ...options, routes: () => ({ send: "direct" }) });

    return async (channelId: string, messageId: number): Promise<MarkReadResult | null> => {
        if (!options.currentToken()) {
            return null;
        }

        const result = await platform.call("chat_mark_read", { p_channel_id: channelId, p_message_id: messageId }, { keepalive: true });

        // A save that never arrived, or was refused, is tried again by the next event; nothing
        // to show.
        if (!result.ok || !result.data || typeof result.data !== "object") {
            return null;
        }

        const body = result.data as Partial<MarkReadResult>;
        return { read_cursor: body.read_cursor ?? null, last_message_id: body.last_message_id ?? null };
    };
}

/**
 * Whether a channel still has something unread after a save: the platform's last message is
 * above the position it stored. This keeps the bold in step with the platform when a message
 * landed as the person left.
 *
 * @param result What the save returned.
 *
 * @returns True when the channel should show as unread.
 */
export function isUnreadAfterSave(result: MarkReadResult): boolean {
    if (result.last_message_id === null) {
        return false;
    }

    return result.read_cursor === null || result.last_message_id > result.read_cursor;
}
