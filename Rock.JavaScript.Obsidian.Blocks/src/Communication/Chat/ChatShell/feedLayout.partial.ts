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
// reason; the feed passes the browser's own, and lays out again when a moment the layout names passes.
import { onScopeDispose, Ref, ref, watchPostEffect } from "vue";
import { blockedSentence, rowKey, sendKey } from "./heldMessage.partial";
import { PendingMessage, TimelineMessage } from "./types.partial";

/** Messages from one sender this close together share a group. */
export const groupWindowMs = 5 * 60 * 1000;

/** How long a just-confirmed send shows "Sent" before it settles to its time. */
export const sentHoldMs = 2200;

/**
 * How long a send is pending before its row says "Sending…". Most sends are answered well inside
 * it, and a label that flashed for a few frames before turning to "Sent" read as flicker.
 */
export const sendingQuietMs = 1000;

/** A first page faster than this never shows the skeleton, so a quick load does not flicker. */
export const skeletonDelayMs = 1000;

/**
 * How far from the bottom a reader may be and still count as at the newest message, in the
 * conversation and the thread drawer alike: such a reader follows what arrives, anyone further up
 * stays put and is offered Jump to latest. It covers a group's typing bubble, 70px with its gap
 * and name line, so a reader nearer the end than the bubble follows it, and its settling away
 * never clamps the scroll of a reader counted as scrolled up.
 */
export const nearBottomPx = 72;

/**
 * How far from the whole-pixel end a feed still counts as at it. The sizes the browser reports are
 * rounded whole numbers while the scroll is fractional at page zoom and on dense screens, so a
 * reader the browser has clamped to the true end can read up to a pixel or so short of it.
 */
export const endSlackPx = 2;

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

    /**
     * The group's sender is typing her next row after this one. That row has the run's face and
     * corners, as her message will, but no time, so the time stays under this row.
     */
    isTypingNext: boolean;
};

/**
 * One thing the feed draws in a day: a group of rows, the New divider, a system line, a block post,
 * or a run of messages by people the reader blocked, drawn as one quiet line.
 */
export type FeedItem =
    | { kind: "divider", key: string }
    | { kind: "blocked", key: string, messages: TimelineMessage[] }
    | { kind: "system", key: string, message: TimelineMessage }
    | { kind: "card", key: string, message: TimelineMessage }
    | { kind: "group", key: string, isOwn: boolean, senderGuid: string | null, rows: FeedRow[] };

/** One day of the feed, its pill where the day starts. */
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

    /** Who else is typing here, by lower-case alias, in the order first heard. */
    typists?: readonly string[];
};

/**
 * Where someone else typing is drawn: the newest row, either the next row of the typist's own run
 * or the first of a group of its own. It never starts a day of its own; her message brings its
 * day's pill when it lands.
 */
export type TypingRow = {
    isInRun: boolean;

    /** The key of the group it is drawn in: her run's when it continues it, else its own, "typing". */
    groupKey: string;

    /**
     * In her run, the first moment it no longer holds the typing row, in epoch milliseconds: past
     * her newest message's five minutes, or the day's end if sooner. The layout reads the clock
     * only as its inputs change, so its owner lays out again then. Null outside her run.
     */
    endsAt: number | null;
};

/** The feed as laid out: its day sections, and the typing row after them while someone types. */
export type FeedLayoutResult = {
    sections: FeedSection[];
    typing: TypingRow | null;
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
 * Whether a message is the reader's own. A page says so itself, under any of the person's aliases,
 * a merged one included; a live copy cannot, since one broadcast reaches every viewer, so it is
 * the reader's when its sender is the session's alias.
 *
 * @param message The message.
 * @param personAliasGuid The reader, as the session knows them.
 *
 * @returns True when the reader sent it.
 */
export function isOwnMessage(message: Pick<TimelineMessage, "is_own" | "person_alias_guid">, personAliasGuid: string): boolean {
    if (typeof message.is_own === "boolean") {
        return message.is_own;
    }

    return (message.person_alias_guid ?? "").toLowerCase() === personAliasGuid.toLowerCase();
}

/**
 * Lays the feed out in day sections of groups, system lines, block posts and the New divider.
 * The person's pending and failed sends go after the newest stored message, today, as their own
 * rows, so a send settles in place.
 *
 * A row is keyed by the send's key wherever the message has one, else by its id, and a pending
 * row by its own key, which is that send's key. The answer, the live copy and every page of a
 * message the person sent therefore draw the element its pending row was, in whatever order they
 * arrive; a new element would cut its rise short and play it again. A pending row whose message is
 * already here is not drawn, since the message stands for it.
 *
 * Someone else typing is laid out as the typist's next row by the same grouping rule, so her
 * message lands where it stood: one typist who sent the newest group today, within the window,
 * continues that run, and anyone else typing starts a group after it.
 *
 * @param options The messages, the pending sends, the reader, where the divider sits and who is typing.
 *
 * @returns The sections, oldest first, and the typing row.
 */
export function layoutFeed(options: FeedLayoutOptions): FeedLayoutResult {
    const me = options.personAliasGuid.toLowerCase();
    const sections: FeedSection[] = [];
    let section: FeedSection | null = null;
    let group: Extract<FeedItem, { kind: "group" }> | null = null;
    let previous: Entry | null = null;
    let lastAt = new Date(0);

    const sentKeys = new Set<string>();

    const entries: Entry[] = options.messages.map(message => {
        const at = new Date(message.created_at);
        // A time that does not parse keeps its place beside the message before it.
        lastAt = isNaN(at.getTime()) ? lastAt : at;
        // keys compared in one case, so a message read back in another is still its row's
        const key = sendKey(message.client_key);
        if (key) {
            sentKeys.add(key);
        }
        // The reader's own messages group as one sender, whichever of their aliases sent them.
        const sender = isOwnMessage(message, me) ? me : (message.person_alias_guid ?? "").toLowerCase();
        return { key: rowKey(message), message, pending: null, sender, at: lastAt };
    });

    for (const pending of options.pending) {
        const key = sendKey(pending.localId);
        if (!sentKeys.has(key ?? "")) {
            entries.push({ key: `k:${key}`, message: null, pending, sender: me, at: options.now });
        }
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

        const last = section.items[section.items.length - 1];
        if (message?.blocked) {
            // A run folds into one line, keyed by its first message so the line stays as the run
            // grows. A root with replies stands alone, so the thread under it can still be opened.
            const runEnd = last?.kind === "blocked" ? last.messages[last.messages.length - 1] : null;
            if (last?.kind === "blocked" && runEnd && !hasReplies(runEnd) && !hasReplies(message)) {
                last.messages.push(message);
            }
            else {
                section.items.push({ kind: "blocked", key: entry.key, messages: [message] });
            }
            group = null;
        }
        else if (message && hasBlocks(message)) {
            section.items.push({ kind: "card", key: entry.key, message });
            group = null;
        }
        else if (message && message.message_type === "system") {
            section.items.push({ kind: "system", key: entry.key, message });
            group = null;
        }
        else if (group && previous && joinsGroup(previous, entry, options.layout)) {
            group.rows.push({ key: entry.key, message, pending: entry.pending, position: "single", isTypingNext: false });
        }
        else {
            group = {
                kind: "group",
                key: entry.key,
                isOwn: entry.sender === me,
                senderGuid: message ? message.person_alias_guid : options.personAliasGuid,
                rows: [{ key: entry.key, message, pending: entry.pending, position: "single", isTypingNext: false }]
            };
            section.items.push(group);
        }

        previous = entry;
    }

    // The typing row is one more row after the newest, laid out by the rule a message there would
    // be: it continues the newest group when that is the one typist's, today and within the window.
    const typists = options.typists ?? [];
    let typing: TypingRow | null = null;
    if (typists.length > 0) {
        const next: Entry = { key: "typing", message: null, pending: null, sender: typists.length === 1 ? typists[0].toLowerCase() : "", at: options.now };
        const isInRun = !!group && !!previous && section?.key === localDayKey(options.now) && next.sender !== "" && next.sender !== me
            && joinsGroup(previous, next, options.layout);
        let endsAt: number | null = null;
        if (isInRun && group && previous) {
            group.rows[group.rows.length - 1].isTypingNext = true;
            const midnight = new Date(options.now.getFullYear(), options.now.getMonth(), options.now.getDate() + 1).getTime();
            endsAt = Math.min(previous.at.getTime() + groupWindowMs + 1, midnight);
        }
        typing = { isInRun, groupKey: isInRun && group ? group.key : "typing", endsAt };
    }

    for (const item of sections.flatMap(s => s.items)) {
        if (item.kind === "group") {
            setPositions(item.rows);
        }
    }

    return { sections, typing };
}

/**
 * A count a feed's layout reads, moved when a moment the last layout named passes, such as the
 * typing row's run ending: a layout reads the clock only as its inputs change, so without this it
 * would keep a moment's answer past it. One timer, for the moment named last; stopped with its owner.
 *
 * @param at The moment the layout drawn last names, in epoch milliseconds, or null for none.
 *
 * @returns The count, for the layout to read.
 */
export function useLayoutAgain(at: () => number | null): Ref<number> {
    const count = ref(0);
    let timer: ReturnType<typeof setTimeout> | null = null;

    function stop(): void {
        if (timer !== null) {
            clearTimeout(timer);
            timer = null;
        }
    }

    // after the draw, so a layout that reads the count can name the moment from the first
    watchPostEffect(() => {
        stop();
        const moment = at();
        if (moment !== null) {
            timer = setTimeout(() => count.value++, Math.max(0, moment - Date.now()));
        }
    });

    onScopeDispose(stop);

    return count;
}

/**
 * What a folded run of messages by people the reader blocked says: how many, and whether they
 * came from one person or several, never who.
 *
 * @param messages The run, oldest first.
 *
 * @returns The line's words.
 */
export function blockedRunText(messages: Pick<TimelineMessage, "person_alias_guid">[]): string {
    if (messages.length <= 1) {
        return blockedSentence;
    }

    const senders = new Set(messages.map(m => (m.person_alias_guid ?? "").toLowerCase()));
    return `${messages.length.toLocaleString()} messages from ${senders.size > 1 ? "people" : "someone"} you blocked`;
}

/** Whether a message is a root with replies, whose thread is opened from under it. */
function hasReplies(message: TimelineMessage): boolean {
    return !message.parent_id && (message.reply_count ?? 0) > 0;
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
 * How many replies a thread has, as every count in the chat reads: "99+" above 99.
 *
 * @param count The count.
 *
 * @returns "1 reply", "3 replies", "99+ replies".
 */
export function replyCountText(count: number): string {
    const shown = count > 99 ? "99+" : String(count);
    return `${shown} ${count === 1 ? "reply" : "replies"}`;
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

/** Marks each row with its place in the group, counting a typing row after the last as one more. */
function setPositions(rows: FeedRow[]): void {
    const count = rows.length + (rows[rows.length - 1]?.isTypingNext ? 1 : 0);
    rows.forEach((row, index) => {
        row.position = count === 1 ? "single"
            : index === 0 ? "first"
                : index === count - 1 ? "last" : "middle";
    });
}

function startOfDay(at: Date): Date {
    return new Date(at.getFullYear(), at.getMonth(), at.getDate());
}

/** The reader's calendar day, which is what a day section is. */
function localDayKey(at: Date): string {
    return `${at.getFullYear()}-${at.getMonth() + 1}-${at.getDate()}`;
}
