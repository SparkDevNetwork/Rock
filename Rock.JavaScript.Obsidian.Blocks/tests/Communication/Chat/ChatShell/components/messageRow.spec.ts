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
// One message row, mounted, in both looks: a channel's Community rows and a direct message's
// Conversational bubbles, picked by the conversation's kind alone. A stored message and the person's own message that
// is still sending or failed draw through the same row, so a send settles in place rather than
// jumping from a list below the timeline. Every state draws its words and a marker the styles and
// the screenshots key on. A mention the server admitted is a chip; anything the server has not
// admitted, a pending body above all, is plain text and never the raw token.
import { mount, VueWrapper } from "@vue/test-utils";
import MessageRow from "../../../../../src/Communication/Chat/ChatShell/components/messageRow.partial.obs";
import Avatar from "../../../../../src/Communication/Chat/ChatShell/ui/avatar.partial.obs";
import Icon from "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs";
import { PendingMessage, TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

const me = "a0000001-0000-4000-8000-000000000000";
const anna = "a0000002-0000-4000-8000-000000000000";
const lee = "a0000007-0000-4000-8000-000000000000";
const stranger = "a0000009-0000-4000-8000-000000000000";

/** 9:02 AM on the day the tests are about, in local time. */
const nineOhTwo = new Date(2026, 9, 12, 9, 2).toISOString();

/** A stored message, in the shape chat_get_history returns, from Anna unless told otherwise. */
function message(extra: Partial<TimelineMessage> = {}): TimelineMessage {
    return {
        id: 42,
        person_alias_guid: anna,
        sender_nick_name: "Anna",
        sender_last_name: "Reyes",
        sender_avatar_url: null,
        sender_listed: true,
        message_type: "text",
        body: "Morning all",
        pieces: null,
        blocks: null,
        created_at: nineOhTwo,
        edited_at: null,
        deleted_at: null,
        visibility: "visible",
        visibility_notice: null,
        ...extra
    };
}

/** The person's own message as the send confirms it: no sender names, the server's pieces. */
function ownConfirmed(extra: Partial<TimelineMessage> = {}): TimelineMessage {
    return message({ person_alias_guid: me, sender_nick_name: undefined, sender_last_name: undefined, ...extra });
}

function pendingRow(extra: Partial<PendingMessage> = {}): PendingMessage {
    return { localId: "local-1", channelId: "c1", body: "On my way", status: "sending", errorCode: null, ...extra };
}

type RowProps = {
    message?: TimelineMessage | null;
    pending?: PendingMessage | null;
    isOwn?: boolean;
    personAliasGuid?: string;
    isChannel?: boolean;
    position?: "single" | "first" | "middle" | "last";
    isDirectMessage?: boolean;
    isJustSent?: boolean;
    senderNames?: ReadonlyMap<string, string>;
};

function draw(props: RowProps): VueWrapper {
    return mount(MessageRow, { props: { personAliasGuid: me, ...props } });
}

/** Text as a person reads it; some ICU builds put a narrow no-break space before AM and PM. */
function plain(text: string | undefined): string {
    return (text ?? "").replace(/\s+/g, " ").trim();
}

/** The row's outermost element, which carries its role, label and markers. */
function root(wrapper: VueWrapper): HTMLElement {
    return wrapper.element as HTMLElement;
}

const mentionLee = { type: "mention", text: "@Lee Leader", person_alias_guid: lee };
const mentionMe = { type: "mention", text: "@Jordan Lee", person_alias_guid: me };
const mentionAll = { type: "mention", text: "@all", all: true };

describe("every message row", () => {
    test("is an article labelled by the sender and the time, focusable by a jump", () => {
        const row = root(draw({ message: message() }));

        expect(row.getAttribute("role")).toBe("article");
        expect(plain(row.getAttribute("aria-label") ?? "")).toBe("Anna Reyes, 9:02 AM");
        expect(row.getAttribute("tabindex")).toBe("-1");
        expect(row.getAttribute("data-testid")).toBe("message-row");
    });

    test("the reader's own message is labelled You", () => {
        const row = root(draw({ message: ownConfirmed(), isOwn: true }));

        expect(plain(row.getAttribute("aria-label") ?? "")).toBe("You, 9:02 AM");
        expect(row.classList.contains("is-own")).toBe(true);
    });

    test.each([["a direct message", false], ["a channel", true]] as const)("is the same article in %s", (_kind, isChannel) => {
        const row = root(draw({ message: message(), isChannel }));

        expect(row.getAttribute("role")).toBe("article");
        expect(plain(row.getAttribute("aria-label") ?? "")).toBe("Anna Reyes, 9:02 AM");
    });
});

describe("the look, by the conversation's kind", () => {
    test("a channel draws Community's head row with the name on every group, with no layout input", () => {
        const channel = draw({ message: message(), isChannel: true, position: "first" });

        expect(root(channel).classList.contains("is-community")).toBe(true);
        expect(channel.get("[data-testid='message-head'] [data-testid='sender-name']").text()).toBe("Anna Reyes");
        expect(channel.findComponent(Avatar).props("size")).toBe(36);
    });

    test("a direct message draws Conversational bubbles, the name hidden in a two-person one", () => {
        const direct = draw({ message: message(), isDirectMessage: true, position: "single" });
        const group = draw({ message: message(), position: "first" });

        expect(root(direct).classList.contains("is-conversational")).toBe(true);
        expect(direct.find("[data-testid='message-head']").exists()).toBe(false);
        expect(direct.find("[data-testid='sender-name']").exists()).toBe(false);
        expect(direct.findComponent(Avatar).props("size")).toBe(34);

        expect(root(group).classList.contains("is-conversational")).toBe(true);
        expect(group.find("[data-testid='message-head']").exists()).toBe(false);
        expect(group.get("[data-testid='sender-name']").text()).toBe("Anna Reyes");
    });
});

describe("the states, in Conversational", () => {
    test("sending: the row says Sending… and is marked sending", () => {
        const wrapper = draw({ pending: pendingRow(), isOwn: true });

        expect(root(wrapper).getAttribute("data-state")).toBe("sending");
        expect(wrapper.get("[data-testid='message-state']").text()).toBe("Sending…");
        expect(wrapper.text()).toContain("On my way");
    });

    test("a pending row keeps the test ids the end-to-end runs and older tests read", () => {
        const row = root(draw({ pending: pendingRow(), isOwn: true }));

        expect(row.getAttribute("data-testid")).toBe("pending-row");
        expect(row.getAttribute("data-status")).toBe("sending");
        expect(row.getAttribute("role")).toBe("article");
    });

    test("sent: the time, then Sent with its check, while the row is just sent", () => {
        const wrapper = draw({ message: ownConfirmed(), isOwn: true, isJustSent: true });

        expect(root(wrapper).getAttribute("data-state")).toBe("sent");
        expect(wrapper.get("[data-testid='message-state']").text()).toBe("Sent");
        expect(plain(wrapper.get("[data-testid='message-meta']").text())).toContain("9:02 AM");
    });

    test("once the hold ends, the time alone", () => {
        const wrapper = draw({ message: ownConfirmed(), isOwn: true });

        expect(wrapper.find("[data-testid='message-state']").exists()).toBe(false);
        expect(plain(wrapper.get("[data-testid='message-meta']").text())).toBe("9:02 AM");
    });

    test("failed: Not sent., with Try again and Delete, and a spoken alert", async () => {
        const wrapper = draw({ pending: pendingRow({ status: "failed", errorCode: "network" }), isOwn: true });

        expect(root(wrapper).getAttribute("data-state")).toBe("failed");
        expect(root(wrapper).getAttribute("data-status")).toBe("failed");
        expect(wrapper.get("[data-testid='message-state']").text()).toBe("Not sent.");
        expect(wrapper.get("[data-testid='retry-send']").text()).toBe("Try again");
        expect(wrapper.get("[data-testid='discard-send']").text()).toBe("Delete");
        expect(wrapper.get("[aria-live='assertive']").text()).toBe("Your message was not sent. Try again.");

        await wrapper.get("[data-testid='retry-send']").trigger("click");
        await wrapper.get("[data-testid='discard-send']").trigger("click");

        expect(wrapper.emitted("retry")).toEqual([["local-1"]]);
        expect(wrapper.emitted("discard")).toEqual([["local-1"]]);
    });

    test("deleted: the sentence in place of the body, and nothing else of it", () => {
        const wrapper = draw({ message: message({ deleted_at: nineOhTwo, body: "secret", edited_at: nineOhTwo }) });

        expect(root(wrapper).getAttribute("data-state")).toBe("deleted");
        expect(wrapper.get("[data-testid='message-body']").text()).toBe("This message was deleted.");
        expect(wrapper.text()).not.toContain("secret");
        expect(wrapper.find("[data-testid='message-edited']").exists()).toBe(false);
    });

    test.each(["hidden", "removed"])("%s: the server's sentence in place of the body", visibility => {
        const notice = visibility === "hidden" ? "Hidden by a moderator." : "Removed by Cedar Hill staff: this broke the community guidelines.";
        const wrapper = draw({ message: message({ visibility, visibility_notice: notice, body: null }) });

        expect(root(wrapper).getAttribute("data-state")).toBe(visibility);
        expect(wrapper.get("[data-testid='message-body']").text()).toBe(notice);
    });

    test("announcement: the Announcement pill on its own line above the text", () => {
        const wrapper = draw({ message: message({ message_type: "announcement", body: "Doors open at 6" }) });

        expect(root(wrapper).classList.contains("is-announcement")).toBe(true);
        expect(wrapper.get("[data-testid='announcement-pill']").text()).toBe("Announcement");
        expect(wrapper.get("[data-testid='message-body']").text()).toBe("Doors open at 6");
    });

    test("a type this client does not know draws the text the server sent, marked unknown", () => {
        const body = "This message can't be shown in this version of Rock. Open it on the Cedar Hill app.";
        const wrapper = draw({ message: message({ message_type: "poll", body }) });

        expect(root(wrapper).getAttribute("data-state")).toBe("unknown");
        expect(wrapper.get("[data-testid='message-body']").text()).toBe(body);
    });

    test.each([
        ["one emoji", "🎉", true],
        ["six emoji", "🎉🎉🎉🎉🎉🎉", true],
        ["a joined emoji", "👩‍👩‍👧", true],
        ["seven emoji", "🎉🎉🎉🎉🎉🎉🎉", false],
        ["an emoji with words", "🎉 yes", false],
        ["words", "yes", false]
    ])("%s is jumbo: %s", (_what, body, isJumbo) => {
        const wrapper = draw({ message: message({ body }) });

        expect(root(wrapper).classList.contains("is-jumbo")).toBe(isJumbo);
    });

    test("emoji the server built from shortcodes count toward jumbo", () => {
        const pieces = [
            { type: "emoji", text: ":tada:", code: "tada", emoji: "🎉", label: "Party popper" },
            { type: "emoji", text: ":tada:", code: "tada", emoji: "🎉", label: "Party popper" }
        ];
        const wrapper = draw({ message: message({ body: ":tada::tada:", pieces }) });

        expect(root(wrapper).classList.contains("is-jumbo")).toBe(true);
    });

    test("edited, at the group's last message: · Edited in the meta under it", () => {
        const wrapper = draw({ message: message({ edited_at: nineOhTwo }), position: "last" });

        expect(wrapper.get("[data-testid='message-meta'] [data-testid='message-edited']").text()).toBe("Edited");
    });

    test("edited, mid-group: Edited under that bubble, with no meta", () => {
        const wrapper = draw({ message: message({ edited_at: nineOhTwo }), position: "middle" });

        expect(wrapper.find("[data-testid='message-meta']").exists()).toBe(false);
        expect(wrapper.get("[data-testid='message-edited']").text()).toBe("Edited");
    });

    test("the server's sentence about the person's own send stays under it", () => {
        const wrapper = draw({ message: ownConfirmed({ notice: "Only Anna will see this." }), isOwn: true });

        expect(wrapper.get("[data-testid='message-notice']").text()).toBe("Only Anna will see this.");
    });
});

describe("the group's furniture, in Conversational", () => {
    test("another person's group head shows their name; later rows do not", () => {
        expect(draw({ message: message(), position: "first" }).get("[data-testid='sender-name']").text()).toBe("Anna Reyes");
        expect(draw({ message: message(), position: "single" }).get("[data-testid='sender-name']").text()).toBe("Anna Reyes");
        expect(draw({ message: message(), position: "middle" }).find("[data-testid='sender-name']").exists()).toBe(false);
        expect(draw({ message: message(), position: "last" }).find("[data-testid='sender-name']").exists()).toBe(false);
    });

    test("the name line is hidden in a direct message and on the reader's own messages", () => {
        expect(draw({ message: message(), position: "first", isDirectMessage: true }).find("[data-testid='sender-name']").exists()).toBe(false);
        expect(draw({ message: ownConfirmed(), isOwn: true, position: "first" }).find("[data-testid='sender-name']").exists()).toBe(false);
    });

    test("the 34px avatar sits at another person's group's last message only", () => {
        const last = draw({ message: message(), position: "last" });
        const avatar = last.findComponent(Avatar);
        expect(avatar.exists()).toBe(true);
        expect(avatar.props("size")).toBe(34);

        expect(draw({ message: message(), position: "single" }).findComponent(Avatar).exists()).toBe(true);
        expect(draw({ message: message(), position: "first" }).findComponent(Avatar).exists()).toBe(false);
        expect(draw({ message: message(), position: "middle" }).findComponent(Avatar).exists()).toBe(false);
    });

    test("the reader's own messages draw no avatar", () => {
        expect(draw({ message: ownConfirmed(), isOwn: true, position: "last" }).findComponent(Avatar).exists()).toBe(false);
    });

    test("the meta sits under the group's last message only", () => {
        expect(draw({ message: message(), position: "first" }).find("[data-testid='message-meta']").exists()).toBe(false);
        expect(plain(draw({ message: message(), position: "last" }).get("[data-testid='message-meta']").text())).toBe("9:02 AM");
    });

    test("a failed send mid-group still shows its own Not sent. line", () => {
        const wrapper = draw({ pending: pendingRow({ status: "failed" }), isOwn: true, position: "first" });

        expect(wrapper.get("[data-testid='message-state']").text()).toBe("Not sent.");
        expect(wrapper.find("[data-testid='retry-send']").exists()).toBe(true);
    });

    test("the row says which place in its group it holds, for its corners", () => {
        expect(root(draw({ message: message(), position: "middle" })).getAttribute("data-position")).toBe("middle");
    });
});

describe("Community, in a channel", () => {
    test("a group's head row draws a 36px avatar, the bold name and the time", () => {
        const wrapper = draw({ message: message(), isChannel: true, position: "first" });

        const head = wrapper.get("[data-testid='message-head']");
        expect(head.get("[data-testid='sender-name']").text()).toBe("Anna Reyes");
        expect(plain(head.get("[data-testid='message-time']").text())).toBe("9:02 AM");
        expect(wrapper.findComponent(Avatar).props("size")).toBe(36);
    });

    test("the head names every group, the reader's own and another person's", () => {
        const own = draw({ message: ownConfirmed({ sender_nick_name: "Jordan", sender_last_name: "Lee" }), isOwn: true, isChannel: true, position: "single" });
        const other = draw({ message: message(), isChannel: true, position: "single" });

        expect(own.get("[data-testid='sender-name']").text()).toBe("Jordan Lee");
        expect(own.findComponent(Avatar).exists()).toBe(true);
        expect(other.get("[data-testid='sender-name']").text()).toBe("Anna Reyes");
    });

    test.each(["sending", "failed"] as const)("the reader's own %s row that starts a group is headed with their name and photo", status => {
        const wrapper = mount(MessageRow, {
            props: {
                personAliasGuid: me,
                pending: pendingRow({ status }),
                isOwn: true,
                isChannel: true,
                position: "single",
                personName: "Jordan Lee",
                personPhotoUrl: "/GetImage.ashx?id=7",
                organizationName: "Cedar Hill Church"
            }
        });

        expect(wrapper.get("[data-testid='sender-name']").text()).toBe("Jordan Lee");
        expect(wrapper.findComponent(Avatar).props("people")).toEqual([{ name: "Jordan Lee", photoUrl: "/GetImage.ashx?id=7" }]);
    });

    test("an own row with no name from the session is still the reader's, never the church's", () => {
        const wrapper = mount(MessageRow, {
            props: { personAliasGuid: me, pending: pendingRow(), isOwn: true, isChannel: true, position: "single", organizationName: "Cedar Hill Church" }
        });

        expect(wrapper.get("[data-testid='sender-name']").text()).toBe("You");
        expect(wrapper.findComponent(Avatar).props("people")).toEqual([{ name: "You", photoUrl: null }]);
    });

    test("a continuation row has no head, and its time in the gutter without AM or PM", () => {
        const wrapper = draw({ message: message(), isChannel: true, position: "middle" });

        expect(wrapper.find("[data-testid='message-head']").exists()).toBe(false);
        expect(wrapper.findComponent(Avatar).exists()).toBe(false);
        expect(plain(wrapper.get("[data-testid='gutter-time']").text())).toBe("9:02");
    });

    test("edited reads (edited) inline after the text", () => {
        const wrapper = draw({ message: message({ edited_at: nineOhTwo }), isChannel: true });

        expect(wrapper.get("[data-testid='message-edited']").text()).toBe("(edited)");
    });

    test("Sending… and Sent ride inline after the text", () => {
        const sending = draw({ pending: pendingRow(), isOwn: true, isChannel: true });
        const sent = draw({ message: ownConfirmed(), isOwn: true, isJustSent: true, isChannel: true });

        expect(sending.get("[data-testid='message-state']").text()).toBe("Sending…");
        expect(sent.get("[data-testid='message-state']").text()).toBe("Sent");
    });

    test("a failed send keeps Not sent., Try again and Delete under the text", () => {
        const wrapper = draw({ pending: pendingRow({ status: "failed" }), isOwn: true, isChannel: true });

        expect(root(wrapper).getAttribute("data-state")).toBe("failed");
        expect(wrapper.get("[data-testid='message-state']").text()).toBe("Not sent.");
        expect(wrapper.get("[data-testid='retry-send']").text()).toBe("Try again");
        expect(wrapper.get("[data-testid='discard-send']").text()).toBe("Delete");
    });

    test("tombstones and the announcement pill draw as in Conversational", () => {
        const deleted = draw({ message: message({ deleted_at: nineOhTwo }), isChannel: true });
        const announcement = draw({ message: message({ message_type: "announcement" }), isChannel: true, position: "single" });

        expect(deleted.get("[data-testid='message-body']").text()).toBe("This message was deleted.");
        expect(announcement.get("[data-testid='announcement-pill']").text()).toBe("Announcement");
    });

    test("a mention of the reader by name tints the row and says so; @all does not", () => {
        const byName = draw({ message: message({ body: `hi <@${me}>`, pieces: [{ type: "text", text: "hi " }, mentionMe] }), isChannel: true });
        const toAll = draw({ message: message({ body: "hi <@all>", pieces: [{ type: "text", text: "hi " }, mentionAll] }), isChannel: true });

        expect(root(byName).classList.contains("is-mention-me")).toBe(true);
        expect(plain(root(byName).getAttribute("aria-label") ?? "")).toBe("Anna Reyes, 9:02 AM, mentions you");
        expect(root(toAll).classList.contains("is-mention-me")).toBe(false);
    });
});

describe("mentions", () => {
    test("a confirmed own send draws the server's mention as @Name, never the token", () => {
        const wrapper = draw({
            message: ownConfirmed({ body: `Hi <@${lee}>`, pieces: [{ type: "text", text: "Hi " }, mentionLee] }),
            isOwn: true
        });

        expect(wrapper.get("[data-testid='piece-mention']").text()).toContain("Lee Leader");
        expect(wrapper.text()).not.toContain("<@");
    });

    test("a mention of someone else is a person chip; of the reader, the you chip; of the room, the @all chip", () => {
        const wrapper = draw({
            message: message({ body: "x", pieces: [mentionLee, { type: "text", text: " " }, mentionMe, { type: "text", text: " " }, mentionAll] })
        });

        const chips = wrapper.findAll("[data-testid='piece-mention']");
        expect(chips.map(chip => chip.attributes("data-mention"))).toEqual(["person", "you", "all"]);
        expect(chips[1].attributes("aria-label")).toBe("@Jordan Lee, you");
        expect(chips[2].attributes("aria-label")).toBe("@all, everyone in this channel");
    });

    test("the reader's alias guid matches a mention in any letter case", () => {
        const upper = { ...mentionMe, person_alias_guid: me.toUpperCase() };
        const wrapper = draw({ message: message({ body: "x", pieces: [upper] }) });

        expect(wrapper.get("[data-testid='piece-mention']").attributes("data-mention")).toBe("you");
    });

    test("a token the server left as text, naming no one mentionable, is not a chip", () => {
        const wrapper = draw({ message: message({ body: `ask <@${stranger}>`, pieces: [{ type: "text", text: "ask " }, { type: "text", text: `<@${stranger}>` }] }) });

        expect(wrapper.findAll("[data-testid='piece-mention']")).toHaveLength(0);
        expect(wrapper.text()).toContain("ask");
    });

    test("a pending row draws a held name as @Name and an unknown one as @someone, never a chip or the token", () => {
        const wrapper = draw({
            pending: pendingRow({ body: `Hi <@${lee}> and <@${stranger}>` }),
            isOwn: true,
            senderNames: new Map([[lee, "Lee Leader"]])
        });

        expect(wrapper.get("[data-testid='message-body']").text()).toBe("Hi @Lee Leader and @someone");
        expect(wrapper.findAll("[data-testid='piece-mention']")).toHaveLength(0);
        expect(wrapper.text()).not.toContain("<@");
    });

    test("a failed row draws its mentions the same way", () => {
        const wrapper = draw({
            pending: pendingRow({ status: "failed", body: `<@${lee}> <@all>` }),
            isOwn: true,
            senderNames: new Map([[lee, "Lee Leader"]])
        });

        expect(wrapper.get("[data-testid='message-body']").text()).toBe("@Lee Leader @all");
        expect(wrapper.text()).not.toContain("<@");
    });
});

describe("system lines and block posts", () => {
    test.each([["a direct message", false], ["a channel", true]] as const)("a system line in %s draws its text with no name and no avatar", (_kind, isChannel) => {
        const wrapper = draw({ message: message({ message_type: "system", person_alias_guid: null, body: "Grace Kim added Sam Ortiz to the conversation." }), isChannel });

        expect(wrapper.text()).toContain("Grace Kim added Sam Ortiz to the conversation.");
        expect(root(wrapper).getAttribute("data-state")).toBe("system");
        expect(wrapper.find("[data-testid='sender-name']").exists()).toBe(false);
        expect(wrapper.findComponent(Avatar).exists()).toBe(false);
    });

    test.each([["a direct message", false], ["a channel", true]] as const)("a block post in %s is a card with its sender, its blocks and an unknown block's fallback", (_kind, isChannel) => {
        const blocks = [
            { type: "text", text: "Easter signups are open", pieces: null, fallback: { type: "text", text: "Easter signups are open" } },
            { type: "poll", options: ["a", "b"], fallback: { type: "text", text: "Vote: a or b" } }
        ];
        const wrapper = draw({ message: message({ message_type: "system", body: "Easter signups are open", blocks }), isChannel });

        const card = wrapper.get("[data-testid='block-card']");
        expect(card.get("[data-testid='block-text']").text()).toBe("Easter signups are open");
        expect(card.get("[data-testid='block-fallback']").text()).toBe("Vote: a or b");
        expect(wrapper.get("[data-testid='sender-name']").text()).toBe("Anna Reyes");
    });
});

describe("a reply also sent to the conversation", () => {
    // Drawn as an ordinary message under a label in the quote label's recipe: the thread icon and
    // "Replied to a thread", a button that opens the thread at its root, emitted as
    // openThread(rootId). Marked data-testid="thread-reply-label".
    function alsoSent(extra: Partial<TimelineMessage> = {}): TimelineMessage {
        return message({ id: 51, parent_id: 40, shown_in_channel: true, body: "Count me in", ...extra });
    }

    test.each([["a channel", true], ["a direct message", false]] as const)("draws the label as a button in %s", (_kind, isChannel) => {
        const wrapper = draw({ message: alsoSent(), isChannel, position: "single" });

        const label = wrapper.get("[data-testid='thread-reply-label']");
        expect(label.element.tagName).toBe("BUTTON");
        expect(plain(label.text())).toBe("Replied to a thread");
        const icons = wrapper.findAllComponents(Icon).filter(icon => label.element.contains(icon.element));
        expect(icons.map(icon => icon.props("name"))).toContain("thread");
        expect(wrapper.get("[data-testid='message-body']").text()).toBe("Count me in");
    });

    test("opens the thread at its root", async () => {
        const wrapper = draw({ message: alsoSent(), isChannel: true, position: "single" });

        await wrapper.get("[data-testid='thread-reply-label']").trigger("click");

        expect(wrapper.emitted("openThread")).toEqual([[40]]);
    });

    test("is not drawn on a root", () => {
        const wrapper = draw({ message: message({ parent_id: null, shown_in_channel: false }), isChannel: true, position: "single" });

        expect(wrapper.find("[data-testid='thread-reply-label']").exists()).toBe(false);
    });
});
