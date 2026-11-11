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
// What the frame draws of rooms: the open conversation's row, facts, kind, title, look and
// subtitle, the open thread's room, and the conversation New message found. The template, the
// members panel, the message actions, the composers and the block's own watchers read these, so
// each is worked out once here and the same computed value handed to every reader, which keeps one
// fact waking everything that draws it.

import { computed, Ref } from "vue";
import { HeaderRoomKind } from "../components/chatHeader.partial.obs";
import { FeedLayout } from "../feedLayout.partial";
import type { ChatShell } from "../shell.partial";
import { joinNames } from "../stores/channelStore.partial";
import { ChannelFacts, ChatPerson, SidebarRow } from "../types.partial";
import { rowPeople } from "../ui/face.partial";
import { TimelineState } from "./useHistory.partial";
import { OpenThreadState } from "./useThreads.partial";

/** What the rooms view reads from the block. */
export type RoomsViewDependencies = {
    /** The block's shell, read on every use since a birthdate replaces it. */
    shell: Readonly<Ref<ChatShell>>;

    activeChannelId: Readonly<Ref<string | null>>;

    /** The thread the drawer draws, whose room the thread's composer and look follow. */
    drawnThread: Readonly<Ref<OpenThreadState | null>>;
};

/**
 * Works out what the frame draws of the open conversation, the open thread's room and the
 * conversation New message found.
 *
 * @param dependencies What the view reads from the block.
 *
 * @returns Each value, for the template and the composables that read it.
 */
export function useRoomsView(dependencies: RoomsViewDependencies) {
    const { shell, activeChannelId, drawnThread } = dependencies;

    /** The sidebar row of the open conversation, which carries everything the header draws. */
    const activeRow = computed((): SidebarRow | null => shell.value.channels.rows.find(r => r.channel_id === shell.value.state.activeChannelId) ?? null);

    /** What chat_get_channel last answered about the open conversation, or null before it lands. */
    const activeFacts = computed((): ChannelFacts | null => activeChannelId.value ? shell.value.rooms.factsOf(activeChannelId.value) : null);

    const activeKind = computed((): HeaderRoomKind | null => activeRow.value?.room_kind ?? null);

    /**
     * Whether the open channel is one the person is not in, where the join bar stands in the
     * composer's place. A join stays here while pending, until the sidebar shows them a member.
     */
    const isJoinBarShown = computed((): boolean => activeKind.value === "channel" && !!activeRow.value && !activeRow.value.is_member);

    /** A conversation with one other person, where a name over each bubble would only repeat the header. */
    const isActiveDirectMessage = computed((): boolean => activeRow.value?.room_kind === "person");

    /** The other people of the open direct message, with their photos, as the sidebar sent them. */
    const activePeople = computed((): ChatPerson[] => activeRow.value ? rowPeople(activeRow.value) : []);

    /** The other person of the open two-person conversation, whose Busy its composer says; null in a group or a channel. */
    const activePair = computed((): ChatPerson | null => pairOf(activeRow.value));

    const activeTitle = computed((): string => activeRow.value?.title ?? "");

    /** A channel in Community's flat rows, a direct message in Conversational's bubbles, by kind alone. */
    const look = computed((): FeedLayout | null => {
        if (!activeKind.value) {
            return null;
        }

        return activeKind.value === "channel" ? "community" : "conversational";
    });

    /**
     * The header's line until the room's facts are read: a channel's description, or an unnamed
     * group's head count.
     */
    const activeSubtitle = computed((): string | null => {
        if (activeKind.value === "channel") {
            return activeRow.value?.description?.trim() || null;
        }

        if (activeKind.value !== "group") {
            return null;
        }

        // The platform counts a room's people only when it has no name, so a named group arrives
        // with 0 and has no count to show.
        const others = activeRow.value?.dm_other_count ?? activePeople.value.length;
        if (others < 1) {
            return null;
        }

        // A group always holds the reader and at least one other, so the count is never singular.
        return `${others + 1} people`;
    });

    /** Whether the open channel's catch-up failed and is trying again, so messages may be missing. */
    const isActiveBehind = computed((): boolean => !!activeChannelId.value && shell.value.timelines.state(activeChannelId.value).isBehind);

    /** Each conversation's title by channel id, as the header writes it, for the rooms the panels name. */
    const roomTitles = computed((): Record<string, string> => {
        const titles: Record<string, string> = {};
        for (const row of shell.value.channels.rows) {
            titles[row.channel_id] = row.title;
        }

        return titles;
    });

    /** Each conversation's kind by channel id, which picks the room's icon on the All tab. */
    const roomKinds = computed((): Record<string, HeaderRoomKind> => {
        const kinds: Record<string, HeaderRoomKind> = {};
        for (const row of shell.value.channels.rows) {
            kinds[row.channel_id] = row.room_kind;
        }

        // the open room may be one the list holds no row for, which only its facts place
        const open = shell.value.state.activeChannelId;
        const openKind = open && !kinds[open] ? shell.value.roomKindOf(open) : null;
        if (open && openKind) {
            kinds[open] = openKind;
        }

        return kinds;
    });

    /** Whether each channel is public, which draws its hash rather than its lock on the All tab. */
    const publicRooms = computed((): Record<string, boolean> => {
        const isPublic: Record<string, boolean> = {};
        for (const row of shell.value.channels.rows) {
            isPublic[row.channel_id] = row.is_public;
        }

        return isPublic;
    });

    /** The sidebar row of the open thread's conversation, which says whether its composer offers the @ picker. */
    const threadRow = computed((): SidebarRow | null => drawnThread.value ? shell.value.channels.rows.find(r => r.channel_id === drawnThread.value?.channelId) ?? null : null);

    /** The open thread's conversation kind, which picks its look and its composer's foot. */
    const threadKind = computed((): HeaderRoomKind | null => drawnThread.value ? roomKinds.value[drawnThread.value.channelId] ?? null : null);

    /** The other person of the open thread's two-person conversation, whose Busy the reply box says too. */
    const threadPair = computed((): ChatPerson | null => pairOf(threadRow.value));

    /** New message's found conversation by the sidebar's title, or by the people chosen when it is not listed. */
    const foundTitle = computed((): string | null => {
        const { lookup, chosen } = shell.value.directMessages;
        if (!lookup.channelId) {
            return null;
        }

        return roomTitles.value[lookup.channelId] ?? joinNames(chosen.map(person => `${person.nick_name} ${person.last_name}`), 0);
    });

    /** The sidebar row of the conversation New message found, which says how its feed is drawn. */
    const foundRow = computed((): SidebarRow | null => {
        const channelId = shell.value.directMessages.lookup.channelId;
        return channelId ? shell.value.channels.rows.find(r => r.channel_id === channelId) ?? null : null;
    });

    /** The found conversation's last few messages as a feed draws a timeline: all of it read, nothing unread. */
    const foundTimeline = computed((): TimelineState | null => {
        const messages = shell.value.directMessages.recent;
        if (!messages || messages.length === 0) {
            return null;
        }

        return { messages, isLoaded: true, pagesRead: 0, hasMore: false, readCursor: null, unreadCount: 0, firstUnreadId: null, isBehind: false, verifiedThrough: messages[messages.length - 1].id, rev: null };
    });

    /** Whether the person is reading a channel they have not joined, where they cannot act on its messages. */
    function isPreviewingRoom(channelId: string): boolean {
        const row = shell.value.channels.rows.find(r => r.channel_id === channelId);
        return !!row && !row.is_member;
    }

    return {
        activeRow, activeFacts, activeKind, isJoinBarShown, isActiveDirectMessage, activePeople, activePair, activeTitle, look, activeSubtitle, isActiveBehind,
        roomTitles, roomKinds, publicRooms, threadRow, threadKind, threadPair, foundTitle, foundRow, foundTimeline, isPreviewingRoom
    };
}

/**
 * The other person of a two-person conversation, by the kind the platform sent: a group showing a
 * single person, someone else in it unlisted, is still a group.
 *
 * @param row The conversation's sidebar row, or null for none.
 *
 * @returns That person, or null for a group, a channel or no row.
 */
function pairOf(row: SidebarRow | null): ChatPerson | null {
    return row?.room_kind === "person" ? row.dm_people?.[0] ?? null : null;
}
