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
// read and the join's confirmation reaches neither, so every confirmed join, a rejoin after a
// drop included, fetches the newest page again and merges it by id.
import { HistoryPage, TimelineMessage } from "../types.partial";

/** How many messages the first page of a channel holds. */
export const firstPageSize = 50;

/** How many of the newest messages are fetched again when a join is confirmed. */
export const rejoinPageSize = 20;

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
    /** Fetches a page: the newest, or the page before an id. */
    fetchPage: (channelId: string, options: { limit: number, beforeId?: number }) => Promise<HistoryPage>;
};

/** The timelines a shell holds. */
export type Timelines = {
    /** The timeline of a channel, created empty on first ask. */
    state: (channelId: string) => TimelineState;

    /** Fetches the first page and applies anything that arrived live while it was in flight. */
    loadNewest: (channelId: string) => Promise<void>;

    /** Fetches the page before the oldest message held. */
    loadOlder: (channelId: string) => Promise<void>;

    /** The channel's live topic was joined: fetches the newest messages again and merges them. */
    onJoined: (channelId: string) => Promise<void>;

    /** A live event on the channel's topic. */
    applyEvent: (channelId: string, event: string, payload: unknown) => void;

    /** Puts one message in place by id, a person's own confirmed send for one. */
    upsert: (channelId: string, message: TimelineMessage) => void;
};

/**
 * Creates the timelines.
 *
 * @param dependencies What the timelines reach outside themselves.
 *
 * @returns The timelines.
 */
export function createTimelines(_dependencies: TimelineDependencies): Timelines {
    throw new Error("not implemented");
}
