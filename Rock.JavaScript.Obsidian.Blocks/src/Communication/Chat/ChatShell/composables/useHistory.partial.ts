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
// A channel's timeline on screen: history pages and live events merged into one list ordered by
// message id, the order the platform uses for history, the read position and the unread count.
//
// History and the live join start together, so either can finish first. Live events that arrive
// before the first page are held and applied after it. A message committed between the page's
// read and the join's confirmation reaches neither, and a live copy can be lost outright, so every
// confirmed join, a rejoin after a drop included, pages forward from the newest message held until
// nothing newer is left. A socket gone for hours can miss more than is worth paging through, so
// past a bound the channel is opened fresh at its newest page instead.
import { reactive } from "vue";
import { HistoryPage, MessageCreatedEvent, MessageDeletedEvent, MessageEditedEvent, MessageVisibilityEvent, TimelineMessage } from "../types.partial";

/** How many messages the first page of a channel holds. */
export const firstPageSize = 50;

/** How catch-up pages forward, when the settings name nothing else: estimates, not measurements. */
export const defaultCatchUp = { page: 100, max: 500 };

/** What a message row draws: what kind of row, and its text. */
export type MessageDisplay = {
    kind: "message" | "system" | "deleted" | "notice";
    text: string | null;
};

/**
 * What a row shows for a message. The server decides when a message is hidden or removed and
 * writes the sentence shown in its place, and a message type this client was not built with is
 * drawn by its text, so a released client still shows what later releases of the platform send.
 *
 * @param message The message.
 *
 * @returns The kind of row and its text.
 */
export function messageDisplay(message: TimelineMessage): MessageDisplay {
    if (message.deleted_at) {
        return { kind: "deleted", text: "This message was deleted." };
    }

    if (message.visibility === "hidden" || message.visibility === "removed") {
        return { kind: "notice", text: message.visibility_notice ?? "This message is not shown." };
    }

    return { kind: message.message_type === "system" ? "system" : "message", text: message.body };
}

/** A channel's timeline. */
export type TimelineState = {
    /** The messages, oldest first, by id. */
    messages: TimelineMessage[];

    /** Whether the first page has arrived. */
    isLoaded: boolean;

    /** Whether older messages exist than the oldest here. */
    hasMore: boolean;

    /** The person's read position when the first page was read. */
    readCursor: number | null;

    /** How many messages were above that position, for the unread divider. */
    unreadCount: number;
};

/** What the timeline reaches outside itself. */
export type TimelineDependencies = {
    /** Fetches a page: the newest, the page before an id, or the page after one. */
    fetchPage: (channelId: string, options: { limit: number, beforeId?: number, afterId?: number }) => Promise<HistoryPage>;

    /** How catch-up pages forward on a rejoin, read from the settings each time. */
    catchUp?: () => { page: number, max: number };
};

/** The timelines a shell holds. */
export type Timelines = {
    /** The timeline of a channel, created empty on first ask. */
    state: (channelId: string) => TimelineState;

    /** Fetches the first page and applies anything that arrived live while it was in flight. */
    loadNewest: (channelId: string) => Promise<void>;

    /** Fetches the page before the oldest message held. */
    loadOlder: (channelId: string) => Promise<void>;

    /** The channel's live topic was joined: pages forward from the newest held until caught up. */
    onJoined: (channelId: string) => Promise<void>;

    /** A live event on the channel's topic. */
    applyEvent: (channelId: string, event: string, payload: unknown) => void;

    /** Puts one message in place by id, a person's own confirmed send for one. */
    upsert: (channelId: string, message: TimelineMessage) => void;
};

/**
 * Whether a message belongs on the channel's timeline: a root, or a reply also shown in the
 * channel. A reply shown only in its thread is the thread panel's.
 */
function isTimelineMessage(message: Pick<TimelineMessage, "parent_id" | "shown_in_channel">): boolean {
    return message.parent_id === null || message.parent_id === undefined || message.shown_in_channel === true;
}

/** Whether a live payload carries the message id every event is keyed by. */
function hasId(payload: unknown): payload is { id: number } {
    return !!payload && typeof payload === "object" && typeof (payload as { id: unknown }).id === "number";
}

/**
 * Combines what is held for a message with a newer copy of it. A page can have been read
 * before an edit or a delete that has already arrived live, so an edit held is kept over an
 * older body and a delete held is never undone. A live copy lacks the sender's name, so a field
 * the newer copy does not carry keeps what is held.
 */
function combine(held: TimelineMessage, incoming: TimelineMessage): TimelineMessage {
    const result: TimelineMessage = { ...held };

    for (const [key, value] of Object.entries(incoming)) {
        if (value !== undefined) {
            (result as Record<string, unknown>)[key] = value;
        }
    }

    const isNewerEditHeld = !!held.edited_at && (!incoming.edited_at || incoming.edited_at < held.edited_at);
    if (isNewerEditHeld) {
        result.body = held.body;
        result.edited_at = held.edited_at;
    }

    if (held.deleted_at && !incoming.deleted_at) {
        result.deleted_at = held.deleted_at;
        result.body = null;
        result.metadata = null;
    }

    return result;
}

/**
 * Finds where a message id is, or would go, in a list ordered by id.
 *
 * @returns The index of the id, or of the first message with a greater id.
 */
function positionOf(messages: TimelineMessage[], id: number): number {
    let low = 0;
    let high = messages.length;

    while (low < high) {
        const middle = (low + high) >>> 1;
        if (messages[middle].id < id) {
            low = middle + 1;
        }
        else {
            high = middle;
        }
    }

    return low;
}

/**
 * Creates the timelines.
 *
 * @param dependencies What the timelines reach outside themselves.
 *
 * @returns The timelines.
 */
export function createTimelines(dependencies: TimelineDependencies): Timelines {
    const states = new Map<string, TimelineState>();
    const held = new Map<string, Array<{ event: string, payload: unknown }>>();

    /** The timeline of a channel, created empty on first ask. */
    function state(channelId: string): TimelineState {
        let timeline = states.get(channelId);

        if (!timeline) {
            timeline = reactive<TimelineState>({ messages: [], isLoaded: false, hasMore: false, readCursor: null, unreadCount: 0 }) as TimelineState;
            states.set(channelId, timeline);
        }

        return timeline;
    }

    /** Puts one message in place by id. */
    function upsert(channelId: string, message: TimelineMessage): void {
        const messages = state(channelId).messages;
        const index = positionOf(messages, message.id);

        if (index < messages.length && messages[index].id === message.id) {
            messages[index] = combine(messages[index], message);
        }
        else {
            messages.splice(index, 0, message);
        }
    }

    /** Merges every message of a page. */
    function mergePage(channelId: string, page: HistoryPage): void {
        for (const message of page.messages) {
            upsert(channelId, message);
        }
    }

    /** The message held with this id, if any. */
    function find(channelId: string, id: number): TimelineMessage | undefined {
        const messages = state(channelId).messages;
        const index = positionOf(messages, id);

        return index < messages.length && messages[index].id === id ? messages[index] : undefined;
    }

    /** Applies one live event to a loaded timeline. */
    function apply(channelId: string, event: string, payload: unknown): void {
        if (!hasId(payload)) {
            return;
        }

        if (event === "message.created") {
            const created = payload as MessageCreatedEvent;
            if (isTimelineMessage(created)) {
                // The live copy carries only the message row, so the sender's name and photo
                // come from their newest message already held, until a page brings the rest.
                const messages = state(channelId).messages;
                let sender: TimelineMessage | undefined;
                for (let i = messages.length - 1; i >= 0 && !sender; i--) {
                    if (messages[i].person_alias_guid === created.person_alias_guid && messages[i].sender_nick_name !== undefined) {
                        sender = messages[i];
                    }
                }

                // Every field the payload carries is kept, not a list of the ones known today, so
                // whatever the platform adds to a message later reaches the row unchanged.
                upsert(channelId, {
                    ...created,
                    sender_nick_name: sender?.sender_nick_name,
                    sender_last_name: sender?.sender_last_name,
                    sender_avatar_url: sender?.sender_avatar_url,
                    sender_listed: sender?.sender_listed
                } as TimelineMessage);
            }
        }
        else if (event === "message.edited") {
            const edited = payload as MessageEditedEvent;
            const message = find(channelId, edited.id);
            if (message && !message.deleted_at) {
                message.body = edited.body;
                message.edited_at = edited.edited_at;
            }
        }
        else if (event === "message.deleted") {
            const deleted = payload as MessageDeletedEvent;
            const message = find(channelId, deleted.id);
            if (message) {
                message.deleted_at = deleted.deleted_at;
                message.body = null;
                message.metadata = null;
            }
        }
        else if (event === "message.visibility") {
            const changed = payload as MessageVisibilityEvent;
            const message = find(channelId, changed.id);
            if (message) {
                message.visibility = changed.state;
                message.visibility_notice = changed.notice;

                // A hidden or removed body must not stay on screen. One shown again comes back with
                // the next page, since the event carries no body.
                if (changed.state !== "visible") {
                    message.body = null;
                    message.metadata = null;
                }
            }
        }
    }

    /** Sets a timeline from its newest page, as opening the channel does. */
    function takeNewest(channelId: string, page: HistoryPage): void {
        const timeline = state(channelId);

        mergePage(channelId, page);
        timeline.hasMore = page.has_more;
        timeline.readCursor = page.read_cursor;
        timeline.unreadCount = page.unread_count;
        timeline.isLoaded = true;
    }

    return {
        state,

        loadNewest: async (channelId: string): Promise<void> => {
            takeNewest(channelId, await dependencies.fetchPage(channelId, { limit: firstPageSize }));

            const waiting = held.get(channelId) ?? [];
            held.delete(channelId);
            for (const { event, payload } of waiting) {
                apply(channelId, event, payload);
            }
        },

        loadOlder: async (channelId: string): Promise<void> => {
            const timeline = state(channelId);
            const oldest = timeline.messages[0];
            if (!oldest) {
                return;
            }

            const page = await dependencies.fetchPage(channelId, { limit: firstPageSize, beforeId: oldest.id });
            mergePage(channelId, page);
            timeline.hasMore = page.has_more;
        },

        onJoined: async (channelId: string): Promise<void> => {
            const { page: pageSize, max } = dependencies.catchUp?.() ?? defaultCatchUp;
            const messages = state(channelId).messages;
            let afterId = messages.length > 0 ? messages[messages.length - 1].id : undefined;

            // Nothing held yet, because the first page is still on its way: there is no newest to
            // page from, so the newest messages are read and merged, which catches one that
            // landed between the first page's read and this join.
            if (afterId === undefined) {
                mergePage(channelId, await dependencies.fetchPage(channelId, { limit: pageSize }));
                return;
            }

            for (let fetched = 0; ;) {
                const page = await dependencies.fetchPage(channelId, { limit: pageSize, afterId });
                mergePage(channelId, page);
                fetched += page.messages.length;

                if (!page.has_more || page.messages.length === 0) {
                    return;
                }

                // Paging through more than this would keep the person waiting for history they
                // will mostly scroll past, so the channel opens fresh at its newest instead.
                if (fetched >= max) {
                    messages.splice(0, messages.length);
                    takeNewest(channelId, await dependencies.fetchPage(channelId, { limit: firstPageSize }));
                    return;
                }

                afterId = page.messages[page.messages.length - 1].id;
            }
        },

        applyEvent: (channelId: string, event: string, payload: unknown): void => {
            if (!state(channelId).isLoaded) {
                const waiting = held.get(channelId) ?? [];
                waiting.push({ event, payload });
                held.set(channelId, waiting);
                return;
            }

            apply(channelId, event, payload);
        },

        upsert
    };
}
