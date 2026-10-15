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
// A message's quote, mounted: the message it quotes, drawn above it. In a direct message it is a
// quiet ghost of the original under a label saying who quoted whom; in a channel it is one reply
// line joined to the avatar by a spine. Either way a click jumps to the original. An original
// that was deleted or hidden since is said so, and offers no jump.
//
// The component these tests are written to, components/messageQuote.partial.obs:
//
//   props: {
//       message: TimelineMessage,    the quoting message, carrying quoted_message_id and quoted as
//                                    every message object does: { id, person_alias_guid,
//                                    sender_nick_name, sender_last_name, sender_avatar_url, body,
//                                    pieces, visibility, visibility_notice, created_at, deleted_at }
//       isOwn: boolean,              the quoting message is the reader's
//       personAliasGuid: string,     the reader, to know a quote of their own message
//       isCommunity: boolean         a channel's reply line rather than the ghost
//   }
//   emits: jump(quotedMessageId)
//
// The quote is the element marked data-testid="message-quote", its label data-testid="quote-label"
// and its preview or gone sentence data-testid="quote-body".
import { mount, VueWrapper } from "@vue/test-utils";
import MessageQuote from "../../../../../src/Communication/Chat/ChatShell/components/messageQuote.partial.obs";
import { TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

const me = "a0000001-0000-4000-8000-000000000000";
const anna = "a0000002-0000-4000-8000-000000000000";
const ben = "a0000003-0000-4000-8000-000000000000";
const lee = "a0000007-0000-4000-8000-000000000000";

/** The quoted message as the platform sends it inside the quoting message. */
function quoted(extra: Record<string, unknown> = {}): Record<string, unknown> {
    return {
        id: 40,
        person_alias_guid: anna,
        sender_nick_name: "Anna",
        sender_last_name: "Reyes",
        sender_avatar_url: null,
        body: "Morning all",
        pieces: null,
        visibility: "visible",
        visibility_notice: null,
        created_at: "2026-10-15T09:00:00Z",
        deleted_at: null,
        ...extra
    };
}

const nobodyQuoted = Symbol("no quote");

/** The quoting message, from Ben unless told otherwise. */
function reply(from: string, quote: Record<string, unknown> | null | typeof nobodyQuoted = quoted()): TimelineMessage {
    return {
        id: 50,
        person_alias_guid: from,
        sender_nick_name: from === me ? "Jordan" : "Ben",
        sender_last_name: from === me ? "Ellis" : "Okafor",
        message_type: "text",
        body: "Count me in",
        pieces: null,
        blocks: null,
        created_at: "2026-10-15T09:05:00Z",
        quoted_message_id: 40,
        quoted: quote === nobodyQuoted ? null : quote
    } as unknown as TimelineMessage;
}

function draw(message: TimelineMessage, extra: { isOwn?: boolean, isCommunity?: boolean } = {}): VueWrapper {
    return mount(MessageQuote, { props: { message, personAliasGuid: me, isOwn: message.person_alias_guid === me, isCommunity: false, ...extra } });
}

function quote(w: VueWrapper): ReturnType<VueWrapper["get"]> {
    return w.get("[data-testid='message-quote']");
}

function plain(text: string): string {
    return text.replace(/\s+/g, " ").trim();
}

describe("the label, by who quoted whom", () => {
    test.each([
        ["someone quoting another person", reply(ben, quoted()), "Replied to Anna"],
        ["the reader quoting another person", reply(me, quoted()), "You replied to Anna"],
        ["someone quoting the reader", reply(ben, quoted({ person_alias_guid: me, sender_nick_name: "Jordan", sender_last_name: "Ellis" })), "Replied to you"],
        ["the reader quoting themselves", reply(me, quoted({ person_alias_guid: me, sender_nick_name: "Jordan", sender_last_name: "Ellis" })), "You replied to yourself"],
        ["someone quoting their own message", reply(ben, quoted({ person_alias_guid: ben, sender_nick_name: "Ben", sender_last_name: "Okafor" })), "Replied to their message"],
        ["a quote the platform no longer sends", reply(ben, nobodyQuoted), "Replied to a message"]
    ])("%s", (_who, message, label) => {
        const w = draw(message);

        expect(plain(w.get("[data-testid='quote-label']").text())).toBe(label);
    });

    test("the reader's alias matches in any letter case", () => {
        const w = draw(reply(ben, quoted({ person_alias_guid: me.toUpperCase(), sender_nick_name: "Jordan", sender_last_name: "Ellis" })));

        expect(plain(w.get("[data-testid='quote-label']").text())).toBe("Replied to you");
    });
});

describe("the ghost quote, in a direct message", () => {
    test("previews the original's text, a mention as the person's name, never the token", () => {
        const w = draw(reply(ben, quoted({ body: `Ask <@${lee}>`, pieces: [{ type: "text", text: "Ask " }, { type: "mention", text: "@Lee Leader", person_alias_guid: lee }] })));

        expect(plain(w.get("[data-testid='quote-body']").text())).toBe("Ask @Lee Leader");
        expect(w.text()).not.toContain("<@");
    });

    test("takes the own tint when it quotes the reader's message, and not otherwise", () => {
        const mine = draw(reply(ben, quoted({ person_alias_guid: me, sender_nick_name: "Jordan", sender_last_name: "Ellis" })));
        const annas = draw(reply(ben, quoted()));

        expect(quote(mine).classes()).toContain("is-self");
        expect(quote(annas).classes()).not.toContain("is-self");
    });

    test("is a button that asks to jump to the original, named for it", async () => {
        const w = draw(reply(ben, quoted()));

        expect(quote(w).element.tagName).toBe("BUTTON");
        expect(plain(quote(w).attributes("aria-label") ?? "")).toBe("Jump to Anna Reyes's message: Morning all");

        await quote(w).trigger("click");

        expect(w.emitted("jump")).toEqual([[40]]);
    });

    test("draws no reply line", () => {
        const w = draw(reply(ben, quoted()));

        expect(quote(w).classes()).not.toContain("is-community");
        expect(w.find("[data-testid='quote-spine']").exists()).toBe(false);
    });
});

describe("an original that is gone", () => {
    test.each([
        ["deleted", quoted({ deleted_at: "2026-10-15T09:03:00Z", body: null }), "Original message was deleted."],
        ["hidden", quoted({ visibility: "hidden", visibility_notice: "Hidden by a moderator.", body: null }), "Original message is unavailable."],
        ["removed", quoted({ visibility: "removed", visibility_notice: "Removed by staff.", body: null }), "Original message is unavailable."],
        ["no longer sent", nobodyQuoted, "Original message was deleted."]
    ] as Array<[string, Record<string, unknown> | typeof nobodyQuoted, string]>)("%s: says so, and offers no jump", async (_how, original, sentence) => {
        const w = draw(reply(ben, original));

        expect(plain(w.get("[data-testid='quote-body']").text())).toBe(sentence);
        expect(quote(w).classes()).toContain("is-gone");
        expect(quote(w).element.tagName).not.toBe("BUTTON");

        await quote(w).trigger("click");
        expect(w.emitted("jump")).toBeUndefined();
    });

    test("in a channel's reply line, the same sentence, with no jump", async () => {
        const w = draw(reply(ben, quoted({ deleted_at: "2026-10-15T09:03:00Z", body: null })), { isCommunity: true });

        expect(plain(w.get("[data-testid='quote-body']").text())).toBe("Original message was deleted.");
        expect(w.find("button").exists()).toBe(false);
    });
});

describe("the reply line, in a channel", () => {
    test("is joined to the avatar by a spine and names the quoted person over a one-line preview", () => {
        const w = draw(reply(ben, quoted()), { isCommunity: true });

        expect(quote(w).classes()).toContain("is-community");
        expect(w.find("[data-testid='quote-spine']").exists()).toBe(true);
        expect(w.get("[data-testid='quote-name']").text()).toBe("Anna Reyes");
        expect(plain(w.get("[data-testid='quote-body']").text())).toBe("Morning all");
    });

    test("names the reader as You", () => {
        const w = draw(reply(ben, quoted({ person_alias_guid: me, sender_nick_name: "Jordan", sender_last_name: "Ellis" })), { isCommunity: true });

        expect(w.get("[data-testid='quote-name']").text()).toBe("You");
    });

    test("a click asks to jump to the original", async () => {
        const w = draw(reply(ben, quoted()), { isCommunity: true });

        await w.get("button").trigger("click");

        expect(w.emitted("jump")).toEqual([[40]]);
    });
});
