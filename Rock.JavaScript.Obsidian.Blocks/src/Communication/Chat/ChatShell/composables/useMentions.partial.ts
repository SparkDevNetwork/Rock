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
// Mentions: the @ picker's search, the mentions inbox, the inbox's last visit on this device and
// the header's dot. The shape only, with no behaviour yet.
/* eslint-disable @typescript-eslint/naming-convention */
import { reactive } from "vue";
import { CallResult } from "../platformCall.partial";
import { ChatError, MentionPerson, MentionRow, SidebarRow, UnreadMention } from "../types.partial";
import { Timelines } from "./useHistory.partial";

/** How long the picker waits after the last keystroke before it searches. */
export const mentionSearchPause = 150;

/** How many rows a page of the inbox holds. */
export const mentionPageSize = 30;

/** What the picker draws for the newest query. */
export type MentionPickerState = {
    /** The query typed after the @, or null when the picker is closed. */
    query: string | null;
    people: MentionPerson[];

    /** Whether the @all row leads the list. */
    isAllOffered: boolean;
};

/** The inbox as the panel draws it. */
export type MentionInboxState = {
    rows: MentionRow[];
    hasMore: boolean;
    isLoading: boolean;
    isRefreshing: boolean;
    isLoadingOlder: boolean;
    error: ChatError | null;
    olderError: ChatError | null;

    /** The messages newer than the last visit and not opened here, marked New for this visit. */
    newIds: number[];

    /** The message whose row was just opened. */
    currentId: number | null;
};

/** What the mentions hold for the screen. */
export type MentionsState = {
    picker: MentionPickerState;
    inbox: MentionInboxState;

    /** Whether the header's Mentions button shows its dot. */
    hasDot: boolean;
};

/** A toast that is not a failure, in the shape the toast draws. */
export type MentionNotice = {
    code: string;
    text: string;
    icon: string;
};

/** What the mentions reach outside themselves. */
export type MentionsDependencies = {
    call: (name: string, args: Record<string, unknown>) => Promise<CallResult>;
    tenantId: string;
    personAliasGuid: string;

    /** The device's storage, which may be missing or refuse every read and write. */
    storage: () => Pick<Storage, "getItem" | "setItem">;

    /** The channel timelines a jump looks for its message in. */
    timelines: Pick<Timelines, "state" | "loadOlder">;

    /** Opens a conversation; resolves true once it is on screen. */
    openChannel: (channelId: string) => Promise<boolean>;

    /** Scrolls the open conversation to a message it holds and flashes it. */
    land: (messageId: number) => void;

    /** Opens a thread at a reply. */
    openThread: (channelId: string, rootId: number, messageId: number) => Promise<void>;

    toast: (notice: MentionNotice) => void;
};

/** The mentions a shell holds. */
export type Mentions = {
    state: MentionsState;
    search: (channel: Pick<SidebarRow, "channel_id" | "can_mention" | "can_mention_all">, query: string) => void;
    closeSearch: () => void;
    openInbox: () => Promise<void>;
    refreshInbox: () => Promise<void>;
    loadOlder: () => Promise<void>;
    closeInbox: () => void;
    jump: (row: MentionRow) => Promise<void>;
    setUnread: (entries: UnreadMention[]) => void;
    applyPersonalEvent: (event: string, payload: unknown) => void;
    channelRead: (channelId: string, readCursor: number) => void;
    threadOpened: (channelId: string, rootId: number) => void;
    stop: () => void;
};

/**
 * Creates the mentions.
 *
 * @param _dependencies What the mentions reach outside themselves.
 *
 * @returns The mentions.
 */
export function createMentions(_dependencies: MentionsDependencies): Mentions {
    const state = reactive<MentionsState>({
        picker: { query: null, people: [], isAllOffered: false },
        inbox: { rows: [], hasMore: false, isLoading: false, isRefreshing: false, isLoadingOlder: false, error: null, olderError: null, newIds: [], currentId: null },
        hasDot: false
    }) as MentionsState;

    return {
        state,
        search: () => undefined,
        closeSearch: () => undefined,
        openInbox: async () => undefined,
        refreshInbox: async () => undefined,
        loadOlder: async () => undefined,
        closeInbox: () => undefined,
        jump: async () => undefined,
        setUnread: () => undefined,
        applyPersonalEvent: () => undefined,
        channelRead: () => undefined,
        threadOpened: () => undefined,
        stop: () => undefined
    };
}
