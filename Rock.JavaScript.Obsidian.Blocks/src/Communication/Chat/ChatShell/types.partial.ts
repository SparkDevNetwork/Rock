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
// The shapes the chat platform sends back, as its calls and live events define them. Property
// names are the platform's own snake_case, because these are read straight off its JSON; mapping
// every message to camelCase on arrival would cost a copy of each for no reader's benefit.
/* eslint-disable @typescript-eslint/naming-convention */

/** One row of the sidebar, as chat_get_bootstrap returns it. */
export type SidebarRow = {
    channel_id: string;
    name: string | null;
    icon_url: string | null;
    channel_type: string;
    is_public: boolean;
    always_shown: boolean;
    is_member: boolean;
    is_favorite: boolean;
    notify_mode: string | null;
    is_leader: boolean;
    can_post_announcements: boolean;
    can_mention_all: boolean;
    last_message_id: number | null;
    last_message_at: string | null;
    last_message_type: string | null;
    last_sender_nick_name: string | null;
    last_sender_last_name: string | null;
    last_sender_listed: boolean;
    last_message_preview: string | null;
    read_cursor: number | null;
    is_unread: boolean;
    mention_count: number;
    dm_names: string[] | null;
    dm_other_count: number | null;
};

/**
 * One piece of a message body as the server built it. Every piece has a text, which is what a
 * client that does not know its type shows; the other keys depend on the type.
 */
export type DisplayPiece = {
    type: string;
    text: string;
    [key: string]: unknown;
};

/**
 * One block of a laid-out message. Every block carries a fallback text block, drawn when this
 * client does not know the block's type.
 */
export type MessageBlock = {
    type: string;
    fallback?: DisplayPiece;
    [key: string]: unknown;
};

/** One message of a channel's timeline, as chat_get_history returns it. */
export type TimelineMessage = {
    id: number;
    parent_id?: number | null;
    quoted_message_id?: number | null;
    shown_in_channel?: boolean;
    person_alias_guid: string | null;
    sender_nick_name?: string | null;
    sender_last_name?: string | null;
    sender_avatar_url?: string | null;
    sender_listed?: boolean;
    message_type: string;
    body: string | null;

    /** The body's pieces as the server built them; null when the body is plain text. */
    pieces?: DisplayPiece[] | null;

    /** The message laid out in blocks, drawn in place of the body; null for an ordinary message. */
    blocks?: MessageBlock[] | null;
    metadata?: unknown;
    reply_count?: number;
    last_reply_at?: string | null;
    participant_count?: number;

    /** The newest people to reply, newest first, at most four; null when there is no live reply. */
    repliers?: Replier[] | null;

    /** Whether the reader follows this root's thread; null when it is not a root with live replies. */
    thread_following?: boolean | null;

    /**
     * Whether the reader follows this root and someone replied past their place in the thread;
     * null when it is not a root with live replies.
     */
    thread_unread?: boolean | null;
    created_at: string;
    edited_at?: string | null;
    deleted_at?: string | null;

    /** visible, hidden or removed; the platform decides, and a hidden or removed body is withheld. */
    visibility?: string;

    /** The server's sentence shown in place of a hidden or removed message. */
    visibility_notice?: string | null;

    /** The server's sentence about the person's own send, shown under it. */
    notice?: string | null;

    /**
     * The key the message was sent under, the sender's retry key; null for a message sent without
     * one. Every copy of a message carries it, so the sender's row and each copy are one message.
     */
    client_key?: string | null;
};

/** One person who replied in a thread, under their primary alias. */
export type Replier = {
    person_alias_guid: string;
    nick_name: string | null;
    last_name: string | null;
    avatar_url: string | null;
};

/** A page of history, as chat_get_history returns it. */
export type HistoryPage = {
    messages: TimelineMessage[];
    read_cursor: number | null;
    unread_count: number;
    has_more: boolean;
};

/** A page of a thread, as chat_get_thread_history returns it: the replies newest first. */
export type ThreadPage = {
    root: TimelineMessage | null;
    messages: TimelineMessage[];
    read_cursor: number | null;
    unread_count: number;
    following: boolean;
    has_more: boolean;
};

/** One row of the threads list, as chat_list_threads returns it. */
export type ThreadListRow = {
    channel_id: string;

    /** The root, carrying its repliers as every message does. */
    root: TimelineMessage;
    reply_count: number;
    last_reply_at: string | null;
    last_reply_id: number | null;
    participant_count: number;
    following: boolean;
    has_unread: boolean;
    mention_count: number;
};

/** What chat_list_threads returns. */
export type ThreadListPage = {
    threads: ThreadListRow[];
    has_more: boolean;
};

/** What chat_follow_thread returns. */
export type FollowThreadAnswer = {
    following: boolean;
    read_cursor: number | null;
};

/** What chat_mark_thread_read returns: the position as stored, and the thread's last reply. */
export type MarkThreadReadAnswer = {
    read_cursor: number | null;
    last_reply_id: number | null;
};

/** What chat_send_message returns: the stored message, and the server's sentence about it. */
export type SendAnswer = {
    id: number;
    created_at: string;
    notice: string | null;

    /** The pieces the stored row draws, so the sender's own message shows them at once. */
    pieces?: DisplayPiece[] | null;
    blocks?: MessageBlock[] | null;
};

/** What chat_mark_read returns: the position as stored, and the channel's last message. */
export type MarkReadResult = {
    read_cursor: number | null;
    last_message_id: number | null;
};

/** One person chat_search_people answers. */
export type PersonRow = {
    person_alias_guid: string;
    nick_name: string;
    last_name: string;
    avatar_url: string | null;
};

/** What chat_search_people returns. */
export type PeopleSearchAnswer = {
    people: PersonRow[];
};

/** What chat_find_dm returns: the conversation those people already share, or null. */
export type FindDmAnswer = {
    channel_id: string | null;
};

/** A new message on a channel topic. */
export type MessageCreatedEvent = {
    id: number;
    channel_id: string;
    parent_id: number | null;
    shown_in_channel: boolean;
    person_alias_guid: string | null;
    message_type: string;
    body: string | null;
    pieces?: DisplayPiece[] | null;
    blocks?: MessageBlock[] | null;
    created_at: string;

    /** The key the message was sent under; null for a message sent without one. */
    client_key?: string | null;
};

/** An edit on a channel topic. */
export type MessageEditedEvent = {
    id: number;
    channel_id: string;
    body: string | null;
    pieces?: DisplayPiece[] | null;
    edited_at: string | null;
};

/** A soft delete on a channel topic. It carries no body. */
export type MessageDeletedEvent = {
    id: number;
    channel_id: string;
    deleted_at: string;
};

/**
 * A message hidden, removed or shown again, on a channel topic. It carries the body only when the
 * message is shown again, so a hidden body never travels.
 */
export type MessageVisibilityEvent = {
    id: number;
    channel_id: string;
    state: string;
    notice: string | null;
    body?: string;
    pieces?: DisplayPiece[] | null;
    blocks?: MessageBlock[] | null;
};

/**
 * A thread's root changed its counts, on the channel topic: once per reply sent and per reply
 * deleted. repliers is null once no live reply is left.
 */
export type ThreadUpdatedEvent = {
    channel_id: string;
    root_id: number;
    reply_count: number;
    last_reply_at: string | null;
    participant_count: number;
    repliers: Replier[] | null;
};

/** The person's own "something new here" signal, on their personal topic. */
export type ChannelUnreadEvent = {
    channel_id: string;
    message_id: number;
};

/** What the platform's token exchange answers. */
export type TokenExchangeResponse = {
    access_token?: string;
    expires_in?: number;

    /** The settings beside the token; any key may be missing, and the client has a default. */
    settings?: unknown;
    error?: { code?: string, message?: string };
};

/** Any event a channel topic carries, by its event name. */
export type ChannelTopicEvent =
    | { event: "message.created", payload: MessageCreatedEvent }
    | { event: "message.edited", payload: MessageEditedEvent }
    | { event: "message.deleted", payload: MessageDeletedEvent }
    | { event: "message.visibility", payload: MessageVisibilityEvent }
    | { event: "thread.updated", payload: ThreadUpdatedEvent };

/** A person's own message that has not been confirmed yet, or whose send failed. */
export type PendingMessage = {
    localId: string;
    channelId: string;
    body: string;
    status: "sending" | "failed";
    errorCode: string | null;

    /** The root a reply answers, so the thread draws the row and the conversation does not. */
    parentId?: number | null;

    /** Whether a reply is also sent to the conversation. */
    isShownInChannel?: boolean;

    /**
     * The stored message's id once a copy of it reached the page, set as the row goes, so the
     * feed knows the send went rather than was discarded.
     */
    confirmedId?: number;
};

/**
 * How bad a failure is, which decides where it is shown. A quota refusal is Realtime turning the
 * whole connection away because the project is over one of its limits.
 */
export type ChatErrorSeverity = "session" | "permission" | "failed" | "degraded" | "quota" | "unknown";

/** A failure after classification: a stable code, how bad it is, and the server's sentence. */
export type ChatError = {
    code: string;
    severity: ChatErrorSeverity;
    text?: string | null;
};
