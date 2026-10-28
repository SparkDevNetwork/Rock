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

/**
 * What kind of room a row is, as the platform says by its live head count: a shared channel, a
 * conversation of two, or a group.
 */
export type RoomKind = "channel" | "person" | "group";

/**
 * A system line's kind, as Rock gave it when it posted the line; a line with none is told by the
 * info mark.
 */
export type SystemKind = "joined" | "left" | "added" | "removed" | "renamed";

/**
 * A person as every answer and live event names one: their primary alias, their name, and their
 * photo, null only when they have none, so a face draws initials only for someone with no photo.
 */
export type ChatPerson = {
    person_alias_guid: string;
    nick_name: string | null;
    last_name: string | null;
    avatar_url: string | null;
};

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
    last_sender_avatar_url?: string | null;
    last_sender_listed: boolean;
    last_message_preview: string | null;

    /** Whether the last message is the reader's own, under any of their aliases; false with none. */
    last_message_is_own: boolean;

    /**
     * Whether Rock posted the last message, under the reader's name or another's. One posted in
     * the reader's name reads "You: " and still makes the row unread; one they typed does not.
     */
    last_message_posted_by_rock: boolean;
    read_cursor: number | null;
    is_unread: boolean;
    mention_count: number;

    /**
     * Messages above the reader's place that name everyone with @all, not their own and not
     * already counted by name. The sidebar's badge adds it to mention_count; the app's icon does not.
     */
    all_mention_count: number;

    /** Whether the @ picker lists people here: the reader may see the roster, or leads the room. */
    can_mention: boolean;
    /** The same people by name only, which a platform older than the people sends alone. */
    dm_names?: string[] | null;

    /**
     * A direct message's other people without a name of its own, up to four, in its title's order;
     * absent from a platform older than this field.
     */
    dm_people?: ChatPerson[] | null;
    dm_other_count: number | null;

    /** What kind of room this is; every screen reads this rather than counting the names it is shown. */
    room_kind: RoomKind;

    /** The name the room shows: its own, or for a direct message without one its other people. */
    title: string;

    /** The whole line the row draws under its title, its empty state and its "You: " included. */
    preview_line: string;

    /**
     * Whether a send here would be taken, and if not its code and the server's sentence, as the
     * sidebar call reads them for each row; a row the client made itself carries none.
     */
    can_post?: boolean;
    can_post_code?: string | null;
    can_post_reason?: string | null;

    /** The longest message the send takes here, in characters. */
    max_length?: number;

    /**
     * Whether the person hid the room from this screen. The platform's sidebar leaves a hidden room
     * out, so only a row the person hid here, while it is open, carries it.
     */
    hidden?: boolean;
};

/** A leader of a channel, as chat_get_channel lists up to three of them by name. */
export type ChannelLeader = ChatPerson;

/** One of the other people in a direct message, as chat_get_channel lists up to eight of them. */
export type ChannelPerson = ChannelLeader & {
    /** Their badges, only when they show their details; null otherwise. */
    details: PersonDetails | null;
};

/** What chat_get_channel answers about one room, read when it opens. */
export type ChannelFacts = {
    channel_id: string;
    name: string | null;
    icon_url: string | null;
    channel_type: string;
    is_member: boolean;
    can_post: boolean;
    can_post_code: string | null;

    /** The server's sentence for why the reader cannot post, or null when they can. */
    can_post_reason: string | null;
    max_length: number;
    can_leave: boolean;
    description: string | null;
    group_type_name: string | null;
    campus_id: string | null;
    is_public: boolean;

    /** The room's listed members, each person once. */
    member_count: number;

    /** Up to three listed leaders by name; empty for a direct message. */
    leaders: ChannelLeader[];

    /**
     * Up to three of the room's people for the header's stack, its leaders first; empty for a
     * direct message and where the room hides its roster from the reader.
     */
    faces?: ChatPerson[];

    /** For a direct message, up to eight of the others; empty for a channel. */
    people: ChannelPerson[];
};

/** One public channel of Browse channels, as chat_list_directory lists them by name. */
export type DirectoryRow = {
    channel_id: string;
    name: string | null;
    icon_url: string | null;
    campus_id: string | null;
    is_member: boolean;
    can_post_code: string | null;
    description: string | null;
    group_type_name: string | null;
};

/** What chat_list_directory answers: a page of rows, and every room matching, counted past the page. */
export type DirectoryPage = {
    channels: DirectoryRow[];
    total: number;
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

/**
 * An action the server lists on a message, the same for every viewer. Its call names the
 * function it runs, which the client calls only under the chat's action prefix.
 */
export type MessageAction = {
    id: string;
    label: string;
    call: string;
};

/**
 * The message a message quotes, as the platform previews it. Its body is withheld once it is
 * hidden or removed, and it is deleted or gone when the original went.
 */
export type QuotedMessage = {
    id: number;
    person_alias_guid: string | null;
    sender_nick_name?: string | null;
    sender_last_name?: string | null;
    sender_avatar_url?: string | null;
    body: string | null;
    pieces?: DisplayPiece[] | null;
    visibility?: string;
    visibility_notice?: string | null;
    created_at: string;
    deleted_at?: string | null;
};

/** One message of a channel's timeline, as chat_get_history returns it. */
export type TimelineMessage = {
    id: number;
    parent_id?: number | null;
    quoted_message_id?: number | null;

    /** The quoted message's preview; null when it quotes nothing, or the original is gone. */
    quoted?: QuotedMessage | null;

    /**
     * Whether the reader sent it, under any of their aliases. Only a page carries it, since a
     * live copy goes to every viewer alike.
     */
    is_own?: boolean;

    /** The actions the server lists on it; empty when it lists none. */
    actions?: MessageAction[];

    /** Its reactions as the server counts them, in the catalog's picker order; empty when none. */
    reactions?: Reaction[];
    shown_in_channel?: boolean;
    person_alias_guid: string | null;
    sender_nick_name?: string | null;
    sender_last_name?: string | null;
    sender_avatar_url?: string | null;
    sender_listed?: boolean;

    /**
     * Whether the sender leads this channel now, under any of their merged records; false in a
     * direct message and on a system line. Kept live, as a renamed sender's name is.
     */
    sender_is_leader?: boolean;
    message_type: string;

    /** What a system line says happened, as Rock gave it; null on any other message, and on a line Rock gave none. */
    system_kind?: SystemKind | null;
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

/**
 * One emoji's reactions on a message, as a page and the add's and remove's answer carry them. The
 * glyph comes from the server, so an emoji this client has never seen still draws.
 */
export type Reaction = {
    code: string;
    count: number;

    /** Whether the reader is among those who reacted with it, under any of their aliases. */
    mine: boolean;
    emoji: string | null;
    image_url: string | null;
    label: string;

    /**
     * The newest three who reacted with it, newest first, the reader among them as anyone else; the
     * first three of the full list. Absent on a chip drawn before the server answered, read as none.
     */
    reactors?: ChipReactor[];
};

/**
 * One person who reacted, under their primary alias, with no name or photo for a person the chat
 * no longer lists. The time and the alias it was made under place them in the full list.
 */
export type ChipReactor = ChatPerson & {
    reacted_at: string;

    /** The alias the reaction was made under, which pages the list. */
    reactor_person_alias_guid: string;
};

/** The glyph of an emoji, which is all a toggle or a new chip needs of it. */
export type EmojiGlyph = {
    code: string;
    emoji: string | null;
    image_url: string | null;
    label: string;
};

/** One of the church's favorite reactions, as the bootstrap's reactions key lists them in order. */
export type ReactionFavorite = EmojiGlyph;

/** One emoji the picker offers, as chat_list_emojis returns them in picker order. */
export type CatalogEmoji = EmojiGlyph & {
    category: string;
    keywords: string[];
};

/** What chat_add_reaction and chat_remove_reaction return: the message's whole list. */
export type ReactionAnswer = {
    message_id: number;
    reactions: Reaction[];
};

/** One person who reacted, as chat_list_reactors returns them, newest first, with the emoji they reacted with. */
export type Reactor = ChipReactor & {
    code: string;
};

/** Where the next page of the full list starts: after this row. */
export type ReactorsCursor = Pick<Reactor, "reacted_at" | "reactor_person_alias_guid" | "code">;

/** What chat_list_reactors returns. */
export type ReactorsPage = {
    reactors: Reactor[];
    has_more: boolean;
};

/**
 * A reaction added or removed, on a channel topic. Its alias is the one the reaction was made
 * under, which tells the reader's own from everyone else's.
 */
export type ReactionEvent = EmojiGlyph & {
    message_id: number;
    channel_id: string;
    person_alias_guid: string;

    /** The chip's newest three as the server read them after the change, none once it is empty. */
    reactors: ChipReactor[];
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

    /** How many messages past the read position are news to the reader, up to 100. */
    unread_count: number;

    /**
     * The first of them, which the New divider stands above; null with nothing unread or no read
     * position. Every page the platform answers carries it; a stand-in page may leave it out.
     */
    first_unread_id?: number | null;
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

    /** Their campus, by the guid the init box names it under; null when they have none. */
    campus_id?: string | null;

    /** Whether they are in the room the search was asked for; sent only when one was named. */
    in_room?: boolean;
};

/** One person as chat_get_person answers for the profile card, in the room the card was opened from. */
export type PersonProfile = PersonRow & {
    /** Their campus, shown whatever their setting; null when they have none. */
    campus_id: string | null;

    is_self: boolean;

    /** Leads that room; false in a direct message and wherever the room hides its roster. */
    is_leader: boolean;

    /** Their badges, only when they show their details; null otherwise. */
    details: PersonDetails | null;

    /** Whether the caller may message them: never themselves. */
    can_message: boolean;

    /** The two-person conversation the caller already has with them, or null. */
    dm_channel_id: string | null;
};

/** What chat_search_people returns. */
export type PeopleSearchAnswer = {
    people: PersonRow[];
};

/** What chat_find_dm returns: the conversation those people already share, or null. */
export type FindDmAnswer = {
    channel_id: string | null;
};

/**
 * What chat itself holds of a person's public profile details, sent only when they show them.
 * Their email, mobile phone and the church's template are Rock's, read as their card opens.
 */
export type PersonDetails = {
    /** The church's chat badges they hold, by key, in no particular order. */
    badge_keys: string[];
};

/** One person of a room's roster, as chat_list_members answers it. */
export type RosterMember = {
    person_alias_guid: string;
    nick_name: string | null;
    last_name: string | null;
    avatar_url: string | null;

    /** Whether they lead this room. */
    is_leader: boolean;

    /** Their badges, only when they show their profile details; else null. */
    details: PersonDetails | null;

    /** Whether the reader may message them from here. */
    can_message: boolean;
};

/** What chat_list_members returns: one page, leaders first, then by name. */
export type RosterPage = {
    members: RosterMember[];
};

/**
 * One of the church's chat badges, as the bootstrap lists them: a label a person carries across
 * the whole church, drawn on their profile card in the church's order.
 */
export type ChatBadge = {
    badge_key: string;
    name: string;

    /** A Font Awesome class Rock pages load, or null for none. */
    icon_css: string | null;

    /** The badge's own colors, or null where the church set none. */
    bg_color: string | null;
    fg_color: string | null;
    sort_order: number;
};

/** One person chat_search_mentionable answers. */
export type MentionPerson = ChatPerson;

/** What chat_search_mentionable returns. */
export type MentionSearchAnswer = {
    people: MentionPerson[];
};

/**
 * One row of the mentions inbox, as chat_list_mentions returns it: the message in the shape a
 * page of history carries, its conversation, and for a reply the root it answers.
 */
export type MentionRow = {
    channel_id: string;
    parent_id: number | null;
    message: TimelineMessage;
};

/** What chat_list_mentions returns, newest first. */
export type MentionListPage = {
    mentions: MentionRow[];
    has_more: boolean;
};

/**
 * One of the newest mentions of the reader, by name or with @all, still unread where it lives, as
 * the bootstrap's me.unread_mentions lists them, newest first.
 */
export type UnreadMention = {
    channel_id: string;
    parent_id: number | null;
    message_id: number;
    created_at: string;

    /** Whether the message is shown in the conversation, rather than only in its thread. */
    in_channel: boolean;
};

/** The person was named in a message, on their personal topic: by name, or with @all. */
export type ChannelMentionEvent = {
    channel_id: string;
    message_id: number;
    parent_id: number | null;
    in_channel: boolean;
    all: boolean;
};

/** A new message on a channel topic. */
export type MessageCreatedEvent = {
    id: number;
    channel_id: string;
    parent_id: number | null;
    shown_in_channel: boolean;
    person_alias_guid: string | null;
    message_type: string;

    /** What a system line says happened, as Rock gave it; null on any other message, and on a line Rock gave none. */
    system_kind?: SystemKind | null;
    body: string | null;
    pieces?: DisplayPiece[] | null;
    blocks?: MessageBlock[] | null;
    quoted_message_id?: number | null;
    quoted?: QuotedMessage | null;

    /** The actions the server lists on it, the same for every viewer. */
    actions?: MessageAction[];

    /** Whether the sender leads this channel now; false in a direct message and on a system line. */
    sender_is_leader?: boolean;

    /** The sender under their primary alias, as a page names them; none on a system line. */
    sender_nick_name?: string | null;
    sender_last_name?: string | null;
    sender_avatar_url?: string | null;
    sender_listed?: boolean;
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

/**
 * A direct message's last timeline message changed, on the personal topic of every member, the
 * sender's included: a send, an edit of the newest, or a delete or hide walking back to the one
 * before. Everything but the room is null once no message is left.
 */
export type DmPreviewEvent = {
    channel_id: string;
    message_id: number | null;

    /** The message as the sidebar draws it, as text. */
    preview: string | null;
    sent_at: string | null;

    /** The sender, under their current name and photo. */
    sender: ChatPerson | null;

    /** Whether the message is this reader's, true only on the sender's own topic. */
    is_own: boolean;

    /** Whether Rock posted it; a post in the reader's own name still makes the row unread. */
    posted_by_rock: boolean;

    /** The whole line this reader's row draws, by the rule the sidebar reads. */
    preview_line: string;
};

/**
 * A read saved on another of the person's devices that moved their place, on their personal
 * topic: the save's own answer, with the channel it was for.
 */
export type ChannelReadEvent = MarkReadResult & {
    channel_id: string;
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
    | { event: "thread.updated", payload: ThreadUpdatedEvent }
    | { event: "reaction.added" | "reaction.removed", payload: ReactionEvent };

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

    /** The message it quotes, sent on every try under the row's key. */
    quotedMessageId?: number;

    /**
     * The refusal that ended this send for good, set only on such a failure: the row shows its
     * sentence and offers Delete alone, since the same text under the same key can only be
     * refused again.
     */
    refusal?: ChatError;

    /**
     * Why the last try failed, in words the person can read, where anything was said: a refusal's
     * sentence, being offline when nothing could be reached, or the server's own sentence.
     */
    reason?: FailureWords;

    /**
     * Set on a direct message's first message, whose conversation the platform may not hold yet,
     * so a refusal as not a member is not taken as final.
     */
    isFirstMessage?: boolean;

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

    /** The kind of room a lost read was from, so the sentence calls a conversation a conversation. */
    roomKind?: RoomKind | null;
};

/**
 * A failure in words for the two places it is told: alone, where the sentence asks the person to
 * try again in its own words where that can help, and beside a Try again, where the button asks
 * and the sentence only says what happened, so the two never say it twice.
 */
export type FailureWords = {
    /** The sentence where nothing beside it offers to try again. */
    alone: string;

    /** The sentence beside a Try again. */
    besideRetry: string;

    /** Whether asking the same again can pass, which is when a Try again is offered at all. */
    canRetry: boolean;
};
