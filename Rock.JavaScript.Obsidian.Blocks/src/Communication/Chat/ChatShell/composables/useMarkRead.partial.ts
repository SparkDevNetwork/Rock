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
// The read position. It is the busiest write chat has, so it is saved on events only and never
// on a timer: when the person leaves the channel, when the tab or app goes to the background or
// the window loses focus, and when the page closes. What is saved is the newest message that was
// actually on screen, and nothing is sent when nothing newer was seen since the last save. A
// person who sits in a busy channel writes nothing until they leave it.
//
// The save goes out with fetch and keepalive so that one made as the page closes still arrives;
// a beacon cannot carry the person's token.
import { MarkReadResult } from "../types.partial";

/** What the tracker reaches outside itself. */
export type ReadTrackerDependencies = {
    /** Saves the position. Resolves null when the save failed. */
    save: (channelId: string, messageId: number) => Promise<MarkReadResult | null>;

    /** A save came back with the position as stored and the channel's last message. */
    onSaved: (channelId: string, result: MarkReadResult) => void;
};

/** The parts of the page the tracker listens to. */
export type PageEventTargets = {
    document: {
        visibilityState: string;
        addEventListener: (type: string, listener: () => void) => void;
        removeEventListener: (type: string, listener: () => void) => void;
    };
    window: {
        addEventListener: (type: string, listener: () => void) => void;
        removeEventListener: (type: string, listener: () => void) => void;
    };
};

/** The read tracker a shell holds. */
export type ReadTracker = {
    /** A channel was opened, with the position the platform holds for it. */
    open: (channelId: string, storedCursor: number | null) => void;

    /** A message of the open channel was on screen. */
    seen: (messageId: number) => void;

    /** Saves the open channel's position if something newer was seen. */
    flush: () => Promise<void>;

    /** Saves, then stops tracking the channel that was open. */
    leave: () => Promise<void>;

    /** Saves on background, lost focus and close. Returns what removes the listeners. */
    attach: (targets: PageEventTargets) => () => void;
};

/**
 * Creates the read tracker.
 *
 * @param dependencies What the tracker reaches outside itself.
 *
 * @returns The tracker.
 */
export function createReadTracker(dependencies: ReadTrackerDependencies): ReadTracker {
    let activeChannelId: string | null = null;
    let newestSeen: number | null = null;
    let lastSaved: number | null = null;

    /** Saves the open channel's position if something newer than the last save was seen. */
    async function flush(): Promise<void> {
        const channelId = activeChannelId;
        const messageId = newestSeen;

        if (channelId === null || messageId === null || (lastSaved !== null && messageId <= lastSaved)) {
            return;
        }

        // Taken as saved before the answer, so a second event while this save is in flight,
        // a close right after losing focus for one, sends nothing more.
        const previous = lastSaved;
        lastSaved = messageId;

        const result = await dependencies.save(channelId, messageId);

        if (!result) {
            // The next event tries again, unless the person has moved on since.
            if (activeChannelId === channelId && lastSaved === messageId) {
                lastSaved = previous;
            }
            return;
        }

        dependencies.onSaved(channelId, result);
    }

    return {
        open: (channelId: string, storedCursor: number | null): void => {
            activeChannelId = channelId;
            lastSaved = storedCursor;
            newestSeen = storedCursor;
        },

        seen: (messageId: number): void => {
            if (activeChannelId !== null && (newestSeen === null || messageId > newestSeen)) {
                newestSeen = messageId;
            }
        },

        flush,

        leave: async (): Promise<void> => {
            const channelId = activeChannelId;
            const messageId = newestSeen;
            const saved = lastSaved;

            // Let go at once, so a channel opened while this save is in flight is tracked from
            // its own position and nothing seen in it is taken for the channel left.
            activeChannelId = null;
            newestSeen = null;
            lastSaved = null;

            if (channelId === null || messageId === null || (saved !== null && messageId <= saved)) {
                return;
            }

            const result = await dependencies.save(channelId, messageId);
            if (result) {
                dependencies.onSaved(channelId, result);
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
    const url = `${options.projectUrl.replace(/\/+$/, "")}/rest/v1/rpc/chat_mark_read`;

    return async (channelId: string, messageId: number): Promise<MarkReadResult | null> => {
        const token = options.currentToken();
        if (!token) {
            return null;
        }

        try {
            const response = await options.fetch(url, {
                method: "POST",
                keepalive: true,
                headers: {
                    "Authorization": `Bearer ${token}`,
                    "apikey": options.publishableKey,
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ p_channel_id: channelId, p_message_id: messageId })
            });

            if (!response.ok) {
                return null;
            }

            const body = await response.json() as Partial<MarkReadResult>;
            return { read_cursor: body.read_cursor ?? null, last_message_id: body.last_message_id ?? null };
        }
        catch {
            // A save that never arrived is tried again by the next event; nothing to show.
            return null;
        }
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
