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
// One new message in each of the three shapes it reaches the client in, with the platform's own
// key sets: the send's answer as the sender builds it from what the platform answers, the live
// message.created as the platform's broadcast states it, and a row of a history page as the
// platform's page builder states it. A page states every field of a message's state; the answer
// and the live copy state none of them, which is what a client that compares copies must survive.
import { createSender } from "../../../../src/Communication/Chat/ChatShell/composables/useSend.partial";
import { TimelineMessage } from "../../../../src/Communication/Chat/ChatShell/types.partial";

export const channelId = "c0000001-0000-4000-8000-000000000000";
export const me = "a0000001-0000-4000-8000-000000000000";
export const anna = "a0000002-0000-4000-8000-000000000000";

/** What one message is: its id, sender, words and time, and the send's key when one was used. */
export type Facts = {
    id: number;
    sender?: string;
    body?: string;
    createdAt?: string;
    key?: string | null;
    parentId?: number | null;
    isShownInChannel?: boolean;
};

const nine = "2026-10-12T14:00:00.000Z";

/**
 * The person's own message as the sender puts it in the timeline once the platform answers the
 * send, built by the real sender from the answer's own keys (id, created_at, notice, pieces,
 * blocks, attachments).
 */
export async function sendAnswer(facts: Facts): Promise<TimelineMessage> {
    let confirmed: TimelineMessage | null = null;
    const capture = (_channelId: string, message: TimelineMessage): void => {
        confirmed = message;
    };
    const sender = createSender({
        send: async () => ({ ok: true, id: facts.id, createdAt: facts.createdAt ?? nine, notice: null, pieces: null, blocks: null }),
        timelines: { upsert: capture },
        onReplyConfirmed: capture,
        personAliasGuid: facts.sender ?? me,
        newLocalId: () => facts.key ?? "k-answer"
    });
    const reply = typeof facts.parentId === "number" ? { parentId: facts.parentId, isShownInChannel: facts.isShownInChannel === true } : undefined;
    await sender.send(channelId, facts.body ?? `m${facts.id}`, undefined, reply);
    sender.stop();
    if (!confirmed) {
        throw new Error("the sender put nothing in place");
    }
    return confirmed;
}

/** The live message.created payload, with exactly the keys the platform's broadcast states, and the room's revision. */
export function liveCreated(facts: Facts, rev: number | null = null): Record<string, unknown> & TimelineMessage {
    return {
        id: facts.id,
        channel_id: channelId,
        parent_id: facts.parentId ?? null,
        shown_in_channel: facts.isShownInChannel ?? false,
        person_alias_guid: facts.sender ?? me,
        client_key: facts.key ?? null,
        sender_nick_name: facts.sender === anna ? "Anna" : "Jordan",
        sender_last_name: facts.sender === anna ? "Reyes" : "Lee",
        sender_avatar_url: null,
        sender_listed: true,
        message_type: "text",
        system_kind: null,
        sender_is_leader: false,
        body: facts.body ?? `m${facts.id}`,
        attachments: null,
        pieces: null,
        blocks: null,
        actions: null,
        quoted_message_id: null,
        quoted: null,
        created_at: facts.createdAt ?? nine,
        ...(rev === null ? {} : { rev })
    };
}

/**
 * A row of a history page for a message never edited, deleted, hidden, reacted to or replied to,
 * with exactly the keys the platform's page builder states.
 */
export function pageRow(facts: Facts, extra: Partial<TimelineMessage> = {}): TimelineMessage {
    return {
        id: facts.id,
        parent_id: facts.parentId ?? null,
        quoted_message_id: null,
        quoted: null,
        shown_in_channel: facts.isShownInChannel ?? false,
        person_alias_guid: facts.sender ?? me,
        client_key: facts.key ?? null,
        is_own: (facts.sender ?? me) === me,
        sender_nick_name: facts.sender === anna ? "Anna" : "Jordan",
        sender_last_name: facts.sender === anna ? "Reyes" : "Lee",
        sender_avatar_url: null,
        sender_listed: true,
        sender_is_leader: false,
        message_type: "text",
        system_kind: null,
        body: facts.body ?? `m${facts.id}`,
        attachments: null,
        pieces: null,
        blocks: null,
        metadata: null,
        visibility: "visible",
        visibility_notice: null,
        reactions: [],
        actions: null,
        reply_count: 0,
        last_reply_at: null,
        participant_count: 0,
        repliers: null,
        thread_following: null,
        thread_unread: null,
        created_at: facts.createdAt ?? nine,
        edited_at: null,
        deleted_at: null,
        ...extra
    };
}
