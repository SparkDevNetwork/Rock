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
// The sidebar. Only this store writes it: the rows arrive whole from the platform's sidebar
// call, and after that they change only through the person's own signals and their own saves.
//
// The person's topic carries a short signal the first time a channel gets something they have
// not read, and never again until they read it. A signal for a channel the sidebar does not
// hold, a hidden channel coming back or one they were just added to, reloads the sidebar, since
// the signal carries no name to draw a row with.
import { reactive } from "vue";
import { isUnreadAfterSave } from "../composables/useMarkRead.partial";
import { ChannelUnreadEvent, MarkReadResult, SidebarRow } from "../types.partial";

/** What the store reaches outside itself. */
export type ChannelStoreDependencies = {
    /** Fetches the sidebar again and hands it to setSidebar. */
    reloadSidebar: () => void;
};

/** The sidebar a shell holds. */
export type ChannelStore = {
    /** The rows in the platform's order. */
    rows: SidebarRow[];

    /** The channel on screen, or null. */
    activeChannelId: string | null;

    /** Replaces every row with a fresh sidebar. */
    setSidebar: (rows: SidebarRow[]) => void;

    /** Sets the channel on screen. */
    setActive: (channelId: string | null) => void;

    /** An event on the person's own topic. */
    applyPersonalEvent: (event: string, payload: unknown) => void;

    /** A save of the read position came back. */
    applyMarkRead: (channelId: string, result: MarkReadResult) => void;
};

/**
 * Creates the sidebar store.
 *
 * @param dependencies What the store reaches outside itself.
 *
 * @returns The store.
 */
export function createChannelStore(dependencies: ChannelStoreDependencies): ChannelStore {
    const store = reactive({
        rows: [] as SidebarRow[],
        activeChannelId: null as string | null
    });

    /** The row of a channel, if the sidebar holds it. */
    function find(channelId: string): SidebarRow | undefined {
        return store.rows.find(row => row.channel_id === channelId);
    }

    /** A signal that a channel has something the person has not read. */
    function applyUnread(payload: unknown): void {
        const signal = payload as Partial<Record<keyof ChannelUnreadEvent, unknown>> | null;
        if (!signal || typeof signal.channel_id !== "string") {
            return;
        }

        // The person is looking at it; their save when they leave settles it.
        if (signal.channel_id === store.activeChannelId) {
            return;
        }

        const row = find(signal.channel_id);
        if (!row) {
            dependencies.reloadSidebar();
            return;
        }

        row.is_unread = true;
        if (typeof signal.message_id === "number" && (row.last_message_id === null || signal.message_id > row.last_message_id)) {
            row.last_message_id = signal.message_id;
        }
    }

    return Object.assign(store, {
        setSidebar: (rows: SidebarRow[]): void => {
            store.rows = rows;
        },

        setActive: (channelId: string | null): void => {
            store.activeChannelId = channelId;
        },

        applyPersonalEvent: (event: string, payload: unknown): void => {
            if (event === "channel.unread") {
                applyUnread(payload);
            }
            else if (event === "membership.changed") {
                dependencies.reloadSidebar();
            }
        },

        applyMarkRead: (channelId: string, result: MarkReadResult): void => {
            const row = find(channelId);
            if (!row) {
                return;
            }

            row.read_cursor = result.read_cursor;
            if (result.last_message_id !== null) {
                row.last_message_id = result.last_message_id;
            }
            // A save that comes back after the person has returned to the channel leaves it read:
            // they are looking at it, and their next save settles it.
            row.is_unread = channelId !== store.activeChannelId && isUnreadAfterSave(result);
        }
    });
}

/**
 * People's names as one title: "Anna", "Anna and Ben", "Anna, Ben and Cara", or "Anna, Ben, Cara,
 * Dee and 2 more" when the platform names only the first few of a group.
 *
 * @param names The names the platform sent.
 * @param unnamed How many more people there are than names.
 *
 * @returns The title.
 */
export function joinNames(names: string[], unnamed: number): string {
    if (unnamed > 0) {
        return `${names.join(", ")} and ${unnamed} more`;
    }

    if (names.length <= 1) {
        return names[0] ?? "";
    }

    return `${names.slice(0, -1).join(", ")} and ${names[names.length - 1]}`;
}

/**
 * Whether a direct message is between the person and one other. Only a group can be named, and
 * the platform counts a room's people only when it has no name, so a named one, sent with a count
 * of 0, is a group.
 *
 * @param row The sidebar row.
 *
 * @returns True for a two-person direct message.
 */
export function isTwoPersonDirectMessage(row: SidebarRow): boolean {
    return row.channel_type === "dm" && !row.name && (row.dm_other_count ?? 1) <= 1;
}

/**
 * A room's title as the sidebar and the header draw it: its own name, or for a direct message
 * without one the people in it. The platform's count of others includes the people it names, so
 * only the rest are "more".
 *
 * @param row The sidebar row.
 *
 * @returns The title.
 */
export function roomTitle(row: SidebarRow): string {
    if (row.name) {
        return row.name;
    }

    const names = row.dm_names ?? [];
    return joinNames(names, (row.dm_other_count ?? names.length) - names.length);
}
