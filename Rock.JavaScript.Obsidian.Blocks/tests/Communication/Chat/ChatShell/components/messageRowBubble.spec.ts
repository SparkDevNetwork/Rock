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
// The person's own send, from Enter to the platform's answer, in both looks: what the row draws
// while the message is not stored is what it draws once it is, so the answer moves nothing. The
// row is drawn whole, as the feed draws it, and given its stored copy in place of its pending one,
// as the feed does when the send settles under the same key.
import { mount, VueWrapper } from "@vue/test-utils";
import MessageRow from "../../../../../src/Communication/Chat/ChatShell/components/messageRow.partial.obs";
import { providedSession } from "../sessionShapes";
import { DisplayPiece, EmojiGlyph, PendingMessage, QuotedMessage, TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

const me = "a0000001-0000-4000-8000-000000000000";
const anna = "a0000002-0000-4000-8000-000000000000";
const sentAt = new Date(2026, 9, 12, 9, 2).toISOString();
const catalog: EmojiGlyph[] = [{ code: "tada", emoji: "🎉", image_url: null, label: "party popper" }];

const original: QuotedMessage = {
    id: 40,
    person_alias_guid: anna,
    sender_nick_name: "Anna",
    sender_last_name: "Reyes",
    sender_avatar_url: null,
    body: "Who is coming?",
    pieces: null,
    attachments: null,
    created_at: sentAt
};

function pending(extra: Partial<PendingMessage> = {}): PendingMessage {
    return { localId: "6c2f3d4e-5a61-4b7c-9d8e-0f1a2b3c4d5e", channelId: "c1", body: "On my way", status: "sending", errorCode: null, ...extra };
}

/** The send's confirmed copy, as the sender puts it in the timeline. */
function stored(extra: Partial<TimelineMessage> = {}): TimelineMessage {
    return {
        id: 72,
        person_alias_guid: me,
        sender_listed: true,
        message_type: "text",
        body: "On my way",
        pieces: null,
        blocks: null,
        created_at: sentAt,
        client_key: "6c2f3d4e-5a61-4b7c-9d8e-0f1a2b3c4d5e",
        ...extra
    };
}

function draw(row: { pending?: PendingMessage | null, message?: TimelineMessage | null }, isChannel: boolean): VueWrapper {
    return mount(MessageRow, {
        props: { personAliasGuid: me, isOwn: true, isChannel, position: "single", ...row },
        global: { provide: providedSession({ me: { personAliasGuid: me, name: "Maxwell Eley" }, emoji: catalog }) }
    });
}

describe.each([["Conversational", false], ["Community", true]])("a send in %s", (_look, isChannel) => {
    test("draws its body the same before and after the answer, for a link and a known shortcode", async () => {
        const body = "see https://cedarhill.church/hike. :tada:";
        const pieces: DisplayPiece[] = [
            { type: "text", text: "see " },
            { type: "link", text: "https://cedarhill.church/hike", url: "https://cedarhill.church/hike" },
            { type: "text", text: ". " },
            { type: "emoji", text: ":tada:", code: "tada", emoji: "🎉", label: "party popper" }
        ];
        const row = draw({ pending: pending({ body }) }, isChannel);
        const before = row.get("[data-testid='message-body']").html();

        await row.setProps({ pending: null, message: stored({ body, pieces }) });

        expect(row.get("[data-testid='message-body']").html()).toBe(before);
    });

    test("draws a picked mention in the chip's box before the answer, inert until the platform admits it", () => {
        const body = `hi <@${anna}>`;
        const row = draw({ pending: pending({ body, mentions: [{ type: "mention", text: "@Anna Reyes", person_alias_guid: anna }] }) }, isChannel);

        const chip = row.get(".chat-mn");
        expect(chip.element.tagName).toBe("SPAN");
        expect(chip.get(".chat-mn-name").text()).toBe("Anna Reyes");
        expect(chip.find(".chat-mn-av").exists()).toBe(true);
        expect(row.find("[data-testid='piece-mention']").exists()).toBe(false);
    });

    test("draws its quote from the first frame, never as deleted", async () => {
        const row = draw({ pending: pending({ quotedMessageId: 40, quoted: original }) }, isChannel);

        expect(row.get("[data-testid='quote-body']").text()).toBe("Who is coming?");

        await row.setProps({ pending: null, message: stored({ quoted_message_id: 40, quoted: original }) });

        expect(row.get("[data-testid='quote-body']").text()).toBe("Who is coming?");
    });
});

test("a reply also sent to the conversation says so before the answer", () => {
    const row = draw({ pending: pending({ parentId: 30, isShownInChannel: true }) }, true);

    expect(row.find("[data-testid='thread-reply-label']").exists()).toBe(true);
});
