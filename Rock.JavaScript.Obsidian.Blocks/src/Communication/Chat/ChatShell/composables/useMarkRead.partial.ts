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
export function createReadTracker(_dependencies: ReadTrackerDependencies): ReadTracker {
    throw new Error("not implemented");
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
export function createKeepaliveSave(_options: KeepaliveSaveOptions): ReadTrackerDependencies["save"] {
    throw new Error("not implemented");
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
export function isUnreadAfterSave(_result: MarkReadResult): boolean {
    throw new Error("not implemented");
}
