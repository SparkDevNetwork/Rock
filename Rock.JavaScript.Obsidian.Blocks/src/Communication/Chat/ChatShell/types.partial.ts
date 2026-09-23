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
// names are the platform's own snake_case, because these are read straight off its JSON.

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
    metadata?: unknown;
    reply_count?: number;
    last_reply_at?: string | null;
    participant_count?: number;
    created_at: string;
    edited_at?: string | null;
    deleted_at?: string | null;
};

/** A page of history, as chat_get_history returns it. */
export type HistoryPage = {
    messages: TimelineMessage[];
    read_cursor: number | null;
    unread_count: number;
    has_more: boolean;
};

/** What chat_mark_read returns: the position as stored, and the channel's last message. */
export type MarkReadResult = {
    read_cursor: number | null;
    last_message_id: number | null;
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
    created_at: string;
};

/** An edit on a channel topic. */
export type MessageEditedEvent = {
    id: number;
    channel_id: string;
    body: string | null;
    edited_at: string | null;
};

/** A soft delete on a channel topic. It carries no body. */
export type MessageDeletedEvent = {
    id: number;
    channel_id: string;
    deleted_at: string;
};

/** Any event a channel topic carries, by its event name. */
export type ChannelTopicEvent =
    | { event: "message.created", payload: MessageCreatedEvent }
    | { event: "message.edited", payload: MessageEditedEvent }
    | { event: "message.deleted", payload: MessageDeletedEvent };

/** A person's own message that has not been confirmed yet, or whose send failed. */
export type PendingMessage = {
    localId: string;
    channelId: string;
    body: string;
    status: "sending" | "failed";
    errorCode: string | null;
};

/** How bad a failure is, which decides where it is shown. */
export type ChatErrorSeverity = "session" | "permission" | "failed" | "degraded" | "unknown";

/** A failure after classification: a stable code and how bad it is. */
export type ChatError = {
    code: string;
    severity: ChatErrorSeverity;
};
