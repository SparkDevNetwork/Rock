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
// How a channel's messages are laid out before anything is drawn: which messages share a group,
// where each day starts and what it is called, where the New divider sits, and the words of the
// unread strip and Jump to latest. Pure rules, so both layouts draw the same groups and a test
// reads them without mounting anything. The clock and the locale are arguments for the same
// reason; the feed passes the browser's own.
import { PendingMessage, TimelineMessage } from "./types.partial";

/** Messages from one sender this close together share a group. */
export const groupWindowMs = 5 * 60 * 1000;

/** How long a just-confirmed send shows "Sent" before it settles to its time. */
export const sentHoldMs = 2200;

/** A first page faster than this never shows the skeleton, so a quick load does not flicker. */
export const skeletonDelayMs = 1000;

/** The two presentations of one timeline. */
export type FeedLayout = "conversational" | "community";

/** A row's place in its group, which decides its corners, its name line, its avatar and its meta. */
export type RowPosition = "single" | "first" | "middle" | "last";

/** One row of a group: a stored message, or the person's own send that is not stored yet. */
export type FeedRow = {
    key: string;
    message: TimelineMessage | null;
    pending: PendingMessage | null;
    position: RowPosition;
};

/** One thing the feed draws in a day: a group of rows, the New divider, a system line or a block post. */
export type FeedItem =
    | { kind: "divider", key: string }
    | { kind: "system", key: string, message: TimelineMessage }
    | { kind: "card", key: string, message: TimelineMessage }
    | { kind: "group", key: string, isOwn: boolean, senderGuid: string | null, rows: FeedRow[] };

/** One day of the feed, so only that day's pill sticks while its messages are on screen. */
export type FeedSection = {
    key: string;
    label: string;
    items: FeedItem[];
};

/** What the feed is laid out from. */
export type FeedLayoutOptions = {
    messages: TimelineMessage[];
    pending: PendingMessage[];
    personAliasGuid: string;
    newDividerBeforeId: number | null;
    layout: FeedLayout;
    now: Date;
    locale?: string;
};

/** A message or a pending send, with what grouping reads from it. */
type Entry = {
    key: string;
    message: TimelineMessage | null;
    pending: PendingMessage | null;
    sender: string;
    at: Date;
};

const dayMs = 24 * 60 * 60 * 1000;

/**
 * Lays the feed out in day sections of groups, system lines, block posts and the New divider.
 * The person's pending and failed sends go after the newest stored message, today, as their own
 * rows, so a send settles in place.
 *
 * @param options The messages, the pending sends, the reader and where the divider sits.
 *
 * @returns The sections, oldest first.
 */
export function layoutFeed(options: FeedLayoutOptions): FeedSection[] {
    const me = options.personAliasGuid.toLowerCase();
    const sections: FeedSection[] = [];
    let section: FeedSection | null = null;
    let group: Extract<FeedItem, { kind: "group" }> | null = null;
    let previous: Entry | null = null;
    let lastAt = new Date(0);

    const entries: Entry[] = options.messages.map(message => {
        const at = new Date(message.created_at);
        // A time that does not parse keeps its place beside the message before it.
        lastAt = isNaN(at.getTime()) ? lastAt : at;
        return { key: `m:${message.id}`, message, pending: null, sender: (message.person_alias_guid ?? "").toLowerCase(), at: lastAt };
    });

    for (const pending of options.pending) {
        entries.push({ key: `p:${pending.localId}`, message: null, pending, sender: me, at: options.now });
    }

    for (const entry of entries) {
        const dayKey = localDayKey(entry.at);
        if (!section || section.key !== dayKey) {
            section = { key: dayKey, label: dayLabel(entry.at, options.now, options.locale), items: [] };
            sections.push(section);
            group = null;
        }

        const message = entry.message;
        if (message && message.id === options.newDividerBeforeId) {
            section.items.push({ kind: "divider", key: "divider" });
            group = null;
        }

        if (message && hasBlocks(message)) {
            section.items.push({ kind: "card", key: entry.key, message });
            group = null;
        }
        else if (message && message.message_type === "system") {
            section.items.push({ kind: "system", key: entry.key, message });
            group = null;
        }
        else if (group && previous && joinsGroup(previous, entry, options.layout)) {
            group.rows.push({ key: entry.key, message, pending: entry.pending, position: "single" });
        }
        else {
            group = {
                kind: "group",
                key: entry.key,
                isOwn: entry.sender === me,
                senderGuid: message ? message.person_alias_guid : options.personAliasGuid,
                rows: [{ key: entry.key, message, pending: entry.pending, position: "single" }]
            };
            section.items.push(group);
        }

        previous = entry;
    }

    for (const item of sections.flatMap(s => s.items)) {
        if (item.kind === "group") {
            setPositions(item.rows);
        }
    }

    return sections;
}

/**
 * Where the New divider sits: above the first message past the read position that is news to
 * the reader. A system line is not news, and neither is the reader's own message, sent from
 * another device.
 *
 * @param messages The timeline, oldest first.
 * @param readCursor The reader's position when the channel opened.
 * @param unreadCount How many messages the platform counted above it.
 * @param personAliasGuid The reader.
 *
 * @returns The id of the message the divider sits above, or null for none.
 */
export function newDividerBeforeId(messages: TimelineMessage[], readCursor: number | null, unreadCount: number, personAliasGuid: string): number | null {
    if (readCursor === null || unreadCount <= 0) {
        return null;
    }

    const me = personAliasGuid.toLowerCase();
    const first = messages.find(m => m.id > readCursor
        && m.message_type !== "system"
        && (m.person_alias_guid ?? "").toLowerCase() !== me);

    return first?.id ?? null;
}

/**
 * A day's label: Today, Yesterday, the weekday within the past six days, then the date, with the
 * year only when it is not this year.
 *
 * @param day Any moment of the day.
 * @param now The reader's clock.
 * @param locale The reader's locale, or the browser's when not given.
 *
 * @returns The label.
 */
export function dayLabel(day: Date, now: Date, locale?: string): string {
    // Calendar days rather than elapsed hours, rounded so a daylight saving change is not a day.
    const daysAgo = Math.round((startOfDay(now).getTime() - startOfDay(day).getTime()) / dayMs);

    if (daysAgo <= 0) {
        return "Today";
    }

    if (daysAgo === 1) {
        return "Yesterday";
    }

    if (daysAgo <= 6) {
        return day.toLocaleDateString(locale, { weekday: "long" });
    }

    if (day.getFullYear() === now.getFullYear()) {
        return day.toLocaleDateString(locale, { month: "long", day: "numeric" });
    }

    return day.toLocaleDateString(locale, { month: "long", day: "numeric", year: "numeric" });
}

/**
 * A message's time as a person reads it, such as 9:02 AM.
 *
 * @param at The moment.
 * @param locale The reader's locale, or the browser's when not given.
 *
 * @returns The time.
 */
export function timeLabel(at: Date, locale?: string): string {
    return at.toLocaleTimeString(locale, { hour: "numeric", minute: "2-digit" });
}

/**
 * A continuation row's time in Community's gutter: the time without AM or PM, which is narrow
 * enough for the gutter and plain from the group's head above it.
 *
 * @param at The moment.
 * @param locale The reader's locale, or the browser's when not given.
 *
 * @returns The time.
 */
export function gutterTimeLabel(at: Date, locale?: string): string {
    return timeLabel(at, locale).replace(/\s*[AP]\.?M\.?$/i, "");
}

/**
 * How many new messages, as every count in the chat reads: "99+" above 99.
 *
 * @param count The count.
 *
 * @returns "1 new message", "7 new messages", "99+ new messages".
 */
export function newMessagesText(count: number): string {
    const shown = count > 99 ? "99+" : String(count);
    return `${shown} new ${count === 1 ? "message" : "messages"}`;
}

/**
 * The unread strip's sentence.
 *
 * @param count How many messages are unread.
 * @param since When the first of them was sent.
 * @param locale The reader's locale, or the browser's when not given.
 *
 * @returns "7 new messages since 11:30 AM".
 */
export function unreadStripText(count: number, since: Date, locale?: string): string {
    return `${newMessagesText(count)} since ${timeLabel(since, locale)}`;
}

/**
 * Jump to latest's words: the count of what arrived while the reader was scrolled up, if any.
 *
 * @param newCount How many messages arrived meanwhile.
 *
 * @returns "Jump to latest" or "3 new messages".
 */
export function jumpLabel(newCount: number): string {
    return newCount > 0 ? newMessagesText(newCount) : "Jump to latest";
}

/** Whether a message is laid out in blocks, which draws it as a card of its own. */
function hasBlocks(message: TimelineMessage): boolean {
    return Array.isArray(message.blocks) && message.blocks.length > 0;
}

/**
 * Whether a message joins the group of the one before it: the same sender within five minutes.
 * Community also starts a group at a quote or an announcement, so its reply line or label sits
 * on a head row, and after an announcement, so the label stays with its own text.
 */
function joinsGroup(previous: Entry, entry: Entry, layout: FeedLayout): boolean {
    if (previous.sender !== entry.sender || entry.at.getTime() - previous.at.getTime() > groupWindowMs) {
        return false;
    }

    if (layout !== "community") {
        return true;
    }

    const isAnnouncement = (e: Entry): boolean => e.message?.message_type === "announcement";
    return !isAnnouncement(entry) && !isAnnouncement(previous) && !entry.message?.quoted_message_id;
}

/** Marks each row with its place in the group. */
function setPositions(rows: FeedRow[]): void {
    rows.forEach((row, index) => {
        row.position = rows.length === 1 ? "single"
            : index === 0 ? "first"
                : index === rows.length - 1 ? "last" : "middle";
    });
}

function startOfDay(at: Date): Date {
    return new Date(at.getFullYear(), at.getMonth(), at.getDate());
}

/** The reader's calendar day, which is what a day section is. */
function localDayKey(at: Date): string {
    return `${at.getFullYear()}-${at.getMonth() + 1}-${at.getDate()}`;
}
