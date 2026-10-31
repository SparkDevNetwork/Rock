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
import { readFileSync } from "fs";
import { join } from "path";
import { mount, VueWrapper } from "@vue/test-utils";
import { defineComponent, nextTick, reactive } from "vue";
import MessageRow from "../../../../../src/Communication/Chat/ChatShell/components/messageRow.partial.obs";
import Avatar from "../../../../../src/Communication/Chat/ChatShell/ui/avatar.partial.obs";
import Icon from "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs";
import MessageQuote from "../../../../../src/Communication/Chat/ChatShell/components/messageQuote.partial.obs";
import { ActionBarDock, useActionBar } from "../../../../../src/Communication/Chat/ChatShell/composables/useActionBar.partial";
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
    isPersonLeader?: boolean;
    personAliasGuid?: string;
    isChannel?: boolean;
    position?: "single" | "first" | "middle" | "last";
    isDirectMessage?: boolean;
    isSendingSlow?: boolean;
    isJustSent?: boolean;
    isArriving?: boolean;
    senderNames?: ReadonlyMap<string, string>;
    isEditing?: boolean;
    editError?: { code: string, severity: string, text?: string | null } | null;
    isEditSaving?: boolean;
    isPhone?: boolean;
    isPreviewing?: boolean;
    isInThread?: boolean;
    reactionHost?: unknown;
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

// A row rises in once, as it first appears. Whether it does is read when it mounts: a class that
// came and went later would start the rise over on a row already on screen.
describe("the arrival", () => {
    test("a row drawn arriving keeps its rise when the feed later stops calling it arriving", async () => {
        const wrapper = draw({ pending: pendingRow(), isOwn: true, isArriving: true });
        expect(root(wrapper).classList.contains("is-arriving")).toBe(true);

        await wrapper.setProps({ pending: null, message: ownConfirmed(), isJustSent: true, isArriving: false });
        expect(root(wrapper).classList.contains("is-arriving")).toBe(true);

        await wrapper.setProps({ isJustSent: false, isArriving: true });
        expect(root(wrapper).classList.contains("is-arriving")).toBe(true);
    });

    test("a row drawn settled never rises later", async () => {
        const wrapper = draw({ message: message(), isArriving: false });

        await wrapper.setProps({ isArriving: true });

        expect(root(wrapper).classList.contains("is-arriving")).toBe(false);
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
    test("sending: a send gone quiet says Sending… and is marked sending", () => {
        const wrapper = draw({ pending: pendingRow(), isOwn: true, isSendingSlow: true });

        expect(root(wrapper).getAttribute("data-state")).toBe("sending");
        expect(wrapper.get("[data-testid='message-state']").text()).toBe("Sending…");
        expect(wrapper.text()).toContain("On my way");
    });

    test("a fresh send says nothing yet, in either look, but is still marked sending", () => {
        const direct = draw({ pending: pendingRow(), isOwn: true });
        const channel = draw({ pending: pendingRow(), isOwn: true, isChannel: true });

        expect(root(direct).getAttribute("data-state")).toBe("sending");
        expect(direct.find("[data-testid='message-state']").exists()).toBe(false);
        // the time sits where Sent will join it, so the row does not grow when Sending… or Sent comes
        expect(plain(direct.get("[data-testid='message-meta']").text())).not.toContain("Sending");
        expect(direct.text()).not.toContain("Sending");
        expect(channel.find("[data-testid='message-state']").exists()).toBe(false);
        expect(channel.text()).not.toContain("Sending");
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

    // The strip over the composer says it aloud and acts on every failed send; the message says it
    // was not sent, where Sending… was, and its marker is the one control for this message alone.
    test("failed: Not sent under the bubble, where Sending… was, as the one thing to press", () => {
        const wrapper = draw({ pending: pendingRow({ status: "failed", errorCode: "network" }), isOwn: true });
        const meta = wrapper.get("[data-testid='message-meta']");

        expect(root(wrapper).getAttribute("data-state")).toBe("failed");
        expect(root(wrapper).getAttribute("data-status")).toBe("failed");
        expect(wrapper.get("[data-testid='message-state']").text()).toBe("Not sent");
        expect(meta.find(".chat-fail-mark").exists()).toBe(true);
        expect(meta.findAll("button")).toHaveLength(1);
        expect(wrapper.find("[aria-live]").exists()).toBe(false);
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

    test("a failed send mid-group still shows its own Not sent line", () => {
        const wrapper = draw({ pending: pendingRow({ status: "failed" }), isOwn: true, position: "first" });

        expect(wrapper.get("[data-testid='message-state']").text()).toBe("Not sent");
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
        const sending = draw({ pending: pendingRow(), isOwn: true, isChannel: true, isSendingSlow: true });
        const sent = draw({ message: ownConfirmed(), isOwn: true, isJustSent: true, isChannel: true });

        expect(sending.get("[data-testid='message-state']").text()).toBe("Sending…");
        expect(sent.get("[data-testid='message-state']").text()).toBe("Sent");
    });

    test("a failed send's Not sent rides inline after the text, where Sending… was, as the one thing to press", () => {
        const wrapper = draw({ pending: pendingRow({ status: "failed" }), isOwn: true, isChannel: true });
        const state = wrapper.get(".chat-cm-text .chat-cm-state");

        expect(root(wrapper).getAttribute("data-state")).toBe("failed");
        expect(state.classes()).toContain("is-failed");
        expect(state.find(".chat-fail-mark").exists()).toBe(true);
        expect(wrapper.get("[data-testid='message-state']").text()).toBe("Not sent");
        expect(wrapper.find("[data-testid='message-meta']").exists()).toBe(false);
        expect(wrapper.findAll(".chat-cm-main button")).toHaveLength(1);
        expect(wrapper.find("[aria-live]").exists()).toBe(false);
    });

    test("tombstones and the announcement pill draw as in Conversational", () => {
        const deleted = draw({ message: message({ deleted_at: nineOhTwo }), isChannel: true });
        const announcement = draw({ message: message({ message_type: "announcement" }), isChannel: true, position: "single" });

        expect(deleted.get("[data-testid='message-body']").text()).toBe("This message was deleted.");
        expect(announcement.get("[data-testid='announcement-pill']").text()).toBe("Announcement");
    });

    test("a mention of the reader by name washes the row and says so", () => {
        const byName = draw({ message: message({ body: `hi <@${me}>`, pieces: [{ type: "text", text: "hi " }, mentionMe] }), isChannel: true });

        expect(root(byName).classList.contains("is-mention-me")).toBe(true);
        expect(plain(root(byName).getAttribute("aria-label") ?? "")).toBe("Anna Reyes, 9:02 AM, mentions you");
    });

    // @all reaches the reader as surely as their name, so it takes the same wash; the words and
    // the label say everyone rather than you, so the reader knows which it was.
    test("a mention of everyone washes the row and says Mentioned everyone, after the time", () => {
        const w = draw({ message: message({ body: "hi <@all>", pieces: [{ type: "text", text: "hi " }, mentionAll] }), isChannel: true, position: "first" });

        expect(root(w).classList.contains("is-mention-me")).toBe(true);
        const head = w.get("[data-testid='message-head']");
        const parts = Array.from(head.element.children).map(c => (c as HTMLElement).dataset.testid);
        expect(head.get("[data-testid='mention-tag']").text()).toBe("Mentioned everyone");
        expect(parts.indexOf("mention-tag")).toBe(parts.indexOf("message-time") + 1);
        expect(head.get("[data-testid='mention-tag']").attributes("aria-hidden")).toBe("true");
        expect(plain(root(w).getAttribute("aria-label") ?? "")).toBe("Anna Reyes, 9:02 AM, mentions everyone");
    });

    test("a row without a head says Mentioned everyone at the end of its text", () => {
        const w = draw({ message: message({ body: "hi <@all>", pieces: [{ type: "text", text: "hi " }, mentionAll] }), isChannel: true, position: "middle" });

        const tag = w.get(".chat-cm-text").get("[data-testid='mention-tag']");
        expect(tag.text()).toBe("Mentioned everyone");
        expect(root(w).classList.contains("is-mention-me")).toBe(true);
    });

    // Naming the reader is the nearer of the two, so a row that does both says you.
    test("a row naming the reader and everyone says Mentioned you", () => {
        const w = draw({ message: message({ body: "x", pieces: [mentionMe, { type: "text", text: " " }, mentionAll] }), isChannel: true, position: "first" });

        expect(w.get("[data-testid='mention-tag']").text()).toBe("Mentioned you");
        expect(plain(root(w).getAttribute("aria-label") ?? "")).toBe("Anna Reyes, 9:02 AM, mentions you");
    });

    test("the reader's own @all is not a mention of them", () => {
        const w = draw({ message: ownConfirmed({ body: "hi <@all>", pieces: [{ type: "text", text: "hi " }, mentionAll] }), isOwn: true, isChannel: true, position: "first" });

        expect(root(w).classList.contains("is-mention-me")).toBe(false);
        expect(w.find("[data-testid='mention-tag']").exists()).toBe(false);
        expect(root(w).getAttribute("aria-label") ?? "").not.toContain("mentions");
    });

    // A person previewing a room they have not joined is not in "everyone" yet.
    test("a room the reader is only previewing does not mention them by @all", () => {
        const w = draw({ message: message({ body: "hi <@all>", pieces: [{ type: "text", text: "hi " }, mentionAll] }), isChannel: true, position: "first", isPreviewing: true });

        expect(root(w).classList.contains("is-mention-me")).toBe(false);
        expect(w.find("[data-testid='mention-tag']").exists()).toBe(false);
        expect(root(w).getAttribute("aria-label") ?? "").not.toContain("mentions");
        expect(w.find("[data-mention='all']").exists()).toBe(true);
    });

    // The platform marks the room's mention as such; a piece without that mark names no room,
    // whatever its text says, so the wash and the chip read the one mark alike.
    test("a mention piece only says all, without the room's mark, is not a mention of everyone", () => {
        const w = draw({ message: message({ body: "hi", pieces: [{ type: "text", text: "hi " }, { type: "mention", text: "@all" }] }), isChannel: true, position: "first" });

        expect(root(w).classList.contains("is-mention-me")).toBe(false);
        expect(w.find("[data-mention='all']").exists()).toBe(false);
    });

    test("a deleted @all leaves no mention treatment", () => {
        const w = draw({ message: message({ body: "hi <@all>", pieces: [mentionAll], deleted_at: nineOhTwo }), isChannel: true, position: "first" });

        expect(root(w).classList.contains("is-mention-me")).toBe(false);
        expect(w.find("[data-testid='mention-tag']").exists()).toBe(false);
    });

    // The wash is color alone, so the words carry the same signal for anyone who cannot see it.
    test("a head row says Mentioned you in its head, after the time", () => {
        const w = draw({ message: message({ body: `hi <@${me}>`, pieces: [{ type: "text", text: "hi " }, mentionMe] }), isChannel: true, position: "first" });

        const head = w.get("[data-testid='message-head']");
        const parts = Array.from(head.element.children).map(c => (c as HTMLElement).dataset.testid);
        expect(head.get("[data-testid='mention-tag']").text()).toBe("Mentioned you");
        expect(parts.indexOf("mention-tag")).toBe(parts.indexOf("message-time") + 1);
        expect(head.get("[data-testid='mention-tag']").findComponent(Icon).props("name")).toBe("at");
        // The row's label already says ", mentions you", so the words are not read a second time.
        expect(head.get("[data-testid='mention-tag']").attributes("aria-hidden")).toBe("true");
    });

    // A row under its group's head has no name line, so the words ride at the end of the text,
    // where (edited) rides, and never add a line.
    test("a row without a head says Mentioned you at the end of its text", () => {
        const w = draw({ message: message({ body: `hi <@${me}>`, pieces: [{ type: "text", text: "hi " }, mentionMe] }), isChannel: true, position: "middle" });

        expect(w.find("[data-testid='message-head']").exists()).toBe(false);
        const text = w.get(".chat-cm-text");
        const tag = text.get("[data-testid='mention-tag']");
        expect(tag.text()).toBe("Mentioned you");
        expect(text.element.lastElementChild).toBe(tag.element);
        expect(tag.attributes("aria-hidden")).toBe("true");
        expect(root(w).classList.contains("is-mention-me")).toBe(true);
        expect(plain(root(w).getAttribute("aria-label") ?? "")).toBe("Anna Reyes, 9:02 AM, mentions you");
    });

    // An announcement owns the brand tint and its edge; one that names the reader keeps them,
    // and still says so in words.
    test("an announcement that names the reader keeps its own look and still says Mentioned you", () => {
        const w = draw({
            message: message({ message_type: "announcement", body: `hi <@${me}>`, pieces: [{ type: "text", text: "hi " }, mentionMe] }),
            isChannel: true,
            position: "first"
        });

        expect(root(w).classList.contains("is-announcement")).toBe(true);
        expect(root(w).classList.contains("is-mention-me")).toBe(false);
        expect(w.get("[data-testid='mention-tag']").text()).toBe("Mentioned you");
    });

    // A system line is a quiet note of what happened and a card a post from the church; neither is
    // a message to the reader, so neither takes the wash, the words or the label's suffix.
    test.each([
        ["a system line", { message_type: "system", person_alias_guid: null, system_kind: "added" }],
        ["a block card", { message_type: "system", blocks: [{ type: "text", text: "hi", pieces: null, fallback: { type: "text", text: "hi" } }] }]
    ] as const)("%s that names the reader or everyone has no mention treatment", (_kind, extra) => {
        const w = draw({
            message: message({ body: `Grace Kim added <@${me}> <@all>`, pieces: [{ type: "text", text: "Grace Kim added " }, mentionMe, { type: "text", text: " " }, mentionAll], ...extra } as Partial<TimelineMessage>),
            isChannel: true,
            position: "first"
        });

        expect(["system", "card"]).toContain(root(w).getAttribute("data-state"));
        expect(root(w).classList.contains("is-mention-me")).toBe(false);
        expect(w.find("[data-testid='mention-tag']").exists()).toBe(false);
        expect(root(w).getAttribute("aria-label") ?? "").not.toContain("mentions you");
    });

    test("the reader's own message naming themselves is not a mention of them", () => {
        const w = draw({ message: ownConfirmed({ body: `me <@${me}>`, pieces: [{ type: "text", text: "me " }, mentionMe] }), isOwn: true, isChannel: true, position: "first" });

        expect(root(w).classList.contains("is-mention-me")).toBe(false);
        expect(w.find("[data-testid='mention-tag']").exists()).toBe(false);
    });

    test("the wash is a rounded card inset from the row's edges, with no edge bar", () => {
        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/messageRow.partial.obs"), "utf8");
        const style = source.slice(source.indexOf("<style"), source.indexOf("</style>"));
        const rule = /\.chat-msg\.is-community\.is-mention-me\s*\{([^}]*)\}/.exec(style)?.[1] ?? "";

        expect(rule).toMatch(/background-color:\s*var\(--chat-row-mention\)/);
        expect(rule).toMatch(/--chat-cm-radius:\s*10px/);
        expect(rule).toMatch(/border-radius:\s*var\(--chat-cm-radius\)/);
        expect(rule).toMatch(/margin-inline:\s*var\(--chat-cm-inset\)/);
        expect(rule).not.toMatch(/(^|[\s;])margin(-top|-bottom|-block)?:/);
        // The text column stays where every other row has it: the inset comes out of the padding.
        expect(rule).toMatch(/padding-inline:\s*calc\(var\(--chat-cm-pad\)\s*-\s*var\(--chat-cm-inset\)\)/);
        expect(rule).not.toMatch(/box-shadow/);
    });

    // Rows sit in a flex group, where margins never collapse, so a margin around the wash would add
    // space. A clear 1px edge above and below, taken from the padding, keeps every row's height and
    // still parts two washed rows in a row; the wash is drawn inside it.
    test("a washed row is exactly as tall as an unwashed one, and two washed rows stand apart", () => {
        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/messageRow.partial.obs"), "utf8");
        const style = source.slice(source.indexOf("<style"), source.indexOf("</style>"));
        /** The declarations of the rule written for exactly this selector. */
        const block = (selector: string): string => {
            const at = style.search(new RegExp(selector.split(".").join("\\.") + "\\s*\\{"));
            return at < 0 ? "" : style.slice(style.indexOf("{", at) + 1, style.indexOf("}", at));
        };
        /** A declaration's pixel value, read from the start of a line so a longer property never matches. */
        const px = (rule: string, property: string): number => parseFloat(new RegExp("(?:^|[\\s;])" + property + ":\\s*([\\d.]+)px").exec(rule)?.[1] ?? "NaN");

        const washed = block(".chat-msg.is-community.is-mention-me");
        expect(washed).toMatch(/border-block:\s*1px solid transparent/);
        expect(washed).toMatch(/background-clip:\s*padding-box/);

        const row = block(".chat-msg.is-community");
        const rowPad = parseFloat(/padding:\s*([\d.]+)px/.exec(row)?.[1] ?? "NaN");
        expect(1 + px(washed, "padding-block")).toBe(rowPad);
        expect(1 + px(block(".chat-msg.is-community.is-mention-me.is-head"), "padding-top")).toBe(px(block(".chat-msg.is-community.is-head"), "padding-top"));
    });

    // The chip that reached the reader, theirs or @all, takes a thin amber ring in a washed row, so
    // it stands off the wash.
    test("the reader's chip and the @all chip in a washed row are ringed in the warning color", () => {
        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/messageRow.partial.obs"), "utf8");
        const style = source.slice(source.indexOf("<style"), source.indexOf("</style>"));
        const rule = /\.chat-msg\.is-community\.is-mention-me :deep\(\.chat-mn\.is-you\),\s*\.chat-msg\.is-community\.is-mention-me :deep\(\.chat-mn\.is-all\)\s*\{([^}]*)\}/.exec(style)?.[1] ?? "";

        expect(rule).toMatch(/box-shadow:\s*0 0 0 1px var\(--chat-mention-ring\)/);
    });

    // The row stands in from the feed's edge, and its bar, docked to the row, would follow it in;
    // it is pushed back out by the same inset, so the bar holds its place against the feed.
    test("a washed row's action bar keeps its place against the feed", () => {
        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/messageRow.partial.obs"), "utf8");
        const style = source.slice(source.indexOf("<style"), source.indexOf("</style>"));
        const rule = /\.chat-msg\.is-community\.is-mention-me :deep\(\.chat-tools\)\s*\{([^}]*)\}/.exec(style)?.[1] ?? "";

        expect(rule).toMatch(/margin-right:\s*calc\(-1 \* var\(--chat-cm-inset\)\)/);
        // The bar's top is measured from inside the row's clear 1px top edge; it is lifted back by it.
        expect(rule).toMatch(/margin-top:\s*-1px/);
    });

    // A click's focus drops the ring and squares an ordinary row; a washed row keeps its corners.
    test("a click's focus keeps the wash's corners", () => {
        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/messageRow.partial.obs"), "utf8");
        const style = source.slice(source.indexOf("<style"), source.indexOf("</style>"));
        const rule = /\.chat-msg\.is-community:focus:not\(:focus-visible\)\s*\{([^}]*)\}/.exec(style)?.[1] ?? "";

        expect(rule).toMatch(/border-radius:\s*var\(--chat-cm-radius,\s*0\)/);
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
            message: message({ body: "x", pieces: [mentionLee, { type: "text", text: " " }, mentionMe, { type: "text", text: " " }, mentionAll] }),
            isChannel: true
        });

        const chips = wrapper.findAll("[data-testid='piece-mention']");
        expect(chips.map(chip => chip.attributes("data-mention"))).toEqual(["person", "you", "all"]);
        expect(chips[1].attributes("aria-label")).toBe("@Jordan Lee, you");
        expect(chips[2].attributes("aria-label")).toBe("@all, everyone in this channel");
    });

    // A direct message reaches the reader whole, so a bubble naming them needs no wash or words:
    // the you chip says it, and its label says "you" to a screen reader.
    test("a bubble naming the reader is neither washed nor tagged; its you chip carries it", () => {
        const w = draw({ message: message({ body: `hi <@${me}>`, pieces: [{ type: "text", text: "hi " }, mentionMe] }), position: "single" });

        expect(root(w).classList.contains("is-conversational")).toBe(true);
        expect(root(w).classList.contains("is-mention-me")).toBe(false);
        expect(w.find("[data-testid='mention-tag']").exists()).toBe(false);
        expect(w.get("[data-testid='piece-mention']").attributes("aria-label")).toBe("@Jordan Lee, you");
    });

    test("a bubble naming everyone is neither washed nor tagged", () => {
        const w = draw({ message: message({ body: "hi <@all>", pieces: [{ type: "text", text: "hi " }, mentionAll] }), position: "single" });

        expect(root(w).classList.contains("is-conversational")).toBe(true);
        expect(root(w).classList.contains("is-mention-me")).toBe(false);
        expect(w.find("[data-testid='mention-tag']").exists()).toBe(false);
        expect(root(w).getAttribute("aria-label") ?? "").not.toContain("mentions");
    });

    test("the @all chip in a direct message or group says conversation, not channel", () => {
        const wrapper = draw({ message: message({ body: "x", pieces: [mentionAll] }) });

        expect(wrapper.get("[data-testid='piece-mention']").attributes("aria-label")).toBe("@all, everyone in this conversation");
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

    /** The icon a system line draws beside its text. */
    function systemIconOf(wrapper: VueWrapper): string | undefined {
        const slot = wrapper.get(".chat-sys-icon");
        return wrapper.findAllComponents(Icon).find(icon => slot.element.contains(icon.element))?.props("name") as string | undefined;
    }

    // The icon follows the kind Rock gave the line, never its words: "removed the conversation's
    // name" says removed, and is a rename.
    test.each([
        ["renamed", "Grace Kim removed the conversation's name.", "pencil"],
        ["renamed", "Grace Kim named the conversation Trail crew.", "pencil"],
        ["joined", "Grace Kim joined the channel.", "door"],
        ["left", "Grace Kim left the conversation.", "door"],
        ["added", "Grace Kim added Sam Ortiz.", "users"],
        ["removed", "Grace Kim removed Sam Ortiz.", "users"]
    ] as const)("a channel's %s line, \"%s\", draws the %s icon", (systemKind, body, icon) => {
        const wrapper = draw({ message: message({ message_type: "system", person_alias_guid: null, body, system_kind: systemKind }), isChannel: true });

        expect(systemIconOf(wrapper)).toBe(icon);
    });

    test("a channel's line with no kind draws the info mark, whatever its words say", () => {
        const wrapper = draw({ message: message({ message_type: "system", person_alias_guid: null, body: "Grace Kim joined the channel.", system_kind: null }), isChannel: true });

        expect(systemIconOf(wrapper)).toBe("info");
    });

    test("a direct message's system line always draws the quiet info mark", () => {
        const wrapper = draw({ message: message({ message_type: "system", person_alias_guid: null, body: "Grace Kim named the conversation Trail crew.", system_kind: "renamed" }), isChannel: false });

        expect(systemIconOf(wrapper)).toBe("info");
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

    test.each([["a direct message", false, "conversation"], ["a channel", true, "channel"]] as const)("a system line's @all chip in %s says %s", (_kind, isChannel, place) => {
        const wrapper = draw({ message: message({ message_type: "system", person_alias_guid: null, body: "hi <@all>", pieces: [{ type: "text", text: "hi " }, mentionAll] }), isChannel });

        expect(root(wrapper).getAttribute("data-state")).toBe("system");
        expect(wrapper.get("[data-testid='piece-mention']").attributes("aria-label")).toBe(`@all, everyone in this ${place}`);
    });

    test.each([["a direct message", false, "conversation"], ["a channel", true, "channel"]] as const)("a block post's @all chip in %s says %s", (_kind, isChannel, place) => {
        const blocks = [{ type: "text", text: "hi @all", pieces: [{ type: "text", text: "hi " }, mentionAll], fallback: { type: "text", text: "hi @all" } }];
        const wrapper = draw({ message: message({ message_type: "system", body: "hi <@all>", blocks }), isChannel });

        expect(wrapper.get("[data-testid='block-card'] [data-testid='piece-mention']").attributes("aria-label")).toBe(`@all, everyone in this ${place}`);
    });
});

describe("a reply also sent to the conversation", () => {
    // Drawn as an ordinary message under a label in the quote label's recipe: the thread icon and
    // "Replied to a thread", a button that opens the thread it belongs to, landed on the reply,
    // emitted as openThread({ rootId, messageId }). Marked data-testid="thread-reply-label".
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

    test("opens its thread at the reply", async () => {
        const wrapper = draw({ message: alsoSent(), isChannel: true, position: "single" });

        await wrapper.get("[data-testid='thread-reply-label']").trigger("click");

        expect(wrapper.emitted("openThread")).toEqual([[{ rootId: 40, messageId: 51 }]]);
    });

    test("is not drawn on a root", () => {
        const wrapper = draw({ message: message({ parent_id: null, shown_in_channel: false }), isChannel: true, position: "single" });

        expect(wrapper.find("[data-testid='thread-reply-label']").exists()).toBe(false);
    });
});

// A failed send is marked, never painted: no tinted row, no red edge, no ring on the bubble, no
// links on the row. A send refused for good is marked the same; its sentence is the strip's.
describe("a failed send's row", () => {
    const refusal = { code: "authz.no_mention_all_right", severity: "permission" as const, text: "You can't mention everyone in this channel." };
    const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/messageRow.partial.obs"), "utf8");
    const style = source.slice(source.indexOf("<style"), source.indexOf("</style>"));

    /** The declarations of every rule written for exactly this selector, alone. */
    function rule(selector: string): string {
        return [...style.replace(/\/\*[\s\S]*?\*\//g, "").matchAll(/([^{}]+)\{([^}]*)\}/g)]
            .filter(match => match[1].trim() === selector)
            .map(match => match[2])
            .join("");
    }

    test.each([["a direct message", false], ["a channel", true]] as const)("a send refused for good is marked Not sent like any other in %s", (_kind, isChannel) => {
        const wrapper = draw({ pending: pendingRow({ status: "failed", errorCode: refusal.code, refusal, reason: { alone: refusal.text, besideRetry: refusal.text, canRetry: false } }), isOwn: true, isChannel, position: "single" });

        expect(wrapper.get("[data-testid='message-state']").text()).toBe("Not sent");
        // the strip shows the sentence; the row keeps it only for a screen reader and a hover
        const shown = wrapper.element.cloneNode(true) as HTMLElement;
        shown.querySelectorAll(".chat-sr-only").forEach(hidden => hidden.remove());
        expect(shown.textContent).not.toContain(refusal.text);
        expect(wrapper.findAll("button").filter(button => /Try again|Delete/.test(button.text()))).toHaveLength(0);
    });

    // The strip says one reason for all of a place's failed sends, so each message keeps its own
    // where a screen reader reads the marker and a pointer rests on it: no reason is shown nowhere.
    test.each([["a direct message", false], ["a channel", true]] as const)("a failed send's own reason rides its Not sent marker in %s", (_kind, isChannel) => {
        const reason = "You're offline. Try again when you're back.";
        const wrapper = draw({ pending: pendingRow({ status: "failed", errorCode: "rpc.transport", reason: { alone: reason, besideRetry: "You're offline.", canRetry: true } }), isOwn: true, isChannel, position: "single" });
        const marker = wrapper.get("[data-testid='message-state']").element.closest(".chat-meta, .chat-cm-state") as HTMLElement;

        expect(marker.getAttribute("title")).toBe(reason);
        expect(marker.querySelector(".chat-sr-only")?.textContent).toBe(`. ${reason}`);
    });

    test("a failed send that said nothing has no reason on its marker", () => {
        const wrapper = draw({ pending: pendingRow({ status: "failed", errorCode: "rpc.unknown" }), isOwn: true, position: "single" });
        const marker = wrapper.get("[data-testid='message-state']").element.closest(".chat-meta, .chat-cm-state") as HTMLElement;

        expect(marker.hasAttribute("title")).toBe(false);
        expect(marker.querySelector(".chat-sr-only")).toBeNull();
    });

    test("its look has no tint, edge, ring or links of its own", () => {
        expect(style).not.toMatch(/\.is-community\.is-failed\s*\{/);
        expect(style).not.toMatch(/\.is-failed \.chat-bubble\s*\{/);
        expect(style).not.toMatch(/chat-link-button|has-refusal/);
        expect(source).not.toMatch(/retry-send|discard-send|message-refusal/);
    });

    // The state rides the text's line by the mention chip's rule: a flex box whose words keep the
    // text's baseline and whose mark is centred on them by the box, never sunk or lifted a set
    // number of pixels, which held for one font only and left the words low on the line.
    test("Sending…, Sent and Not sent keep the text's baseline, their mark centred on the words", () => {
        expect(rule(".chat-cm-state")).toMatch(/display:\s*inline-flex/);
        expect(rule(".chat-cm-state")).toMatch(/align-items:\s*center/);
        expect(rule(".chat-cm-state")).not.toMatch(/vertical-align/);
        expect(rule(".chat-cm-state-words")).toMatch(/align-self:\s*baseline/);

        const states = [
            draw({ pending: pendingRow(), isOwn: true, isChannel: true, isSendingSlow: true }),
            draw({ message: ownConfirmed(), isOwn: true, isJustSent: true, isChannel: true }),
            draw({ pending: pendingRow({ status: "failed" }), isOwn: true, isChannel: true })
        ];
        for (const wrapper of states) {
            expect(wrapper.get(".chat-cm-state [data-testid='message-state']").classes()).toContain("chat-cm-state-words");
        }
    });
});

// A message that quotes another draws the quote above it, in the conversation's look, and a jump
// asked from the quote is the row's to pass on as jump(quotedMessageId).
describe("a message that quotes another", () => {
    const quoted = {
        id: 40,
        person_alias_guid: lee,
        sender_nick_name: "Lee",
        sender_last_name: "Leader",
        sender_avatar_url: null,
        body: "Doors open at 6",
        pieces: null,
        visibility: "visible",
        visibility_notice: null,
        created_at: nineOhTwo,
        deleted_at: null
    };

    test.each([["a direct message", false], ["a channel", true]] as const)("draws its quote above it in %s, in that look", (_kind, isChannel) => {
        const plainRow = draw({ message: message(), isChannel, position: "single" });
        expect(plainRow.findComponent(MessageQuote).exists()).toBe(false);

        const wrapper = draw({ message: message({ quoted_message_id: 40, quoted } as Partial<TimelineMessage>), isChannel, position: "single" });

        const quote = wrapper.findComponent(MessageQuote);
        expect(quote.exists()).toBe(true);
        expect(quote.props("isCommunity")).toBe(isChannel);
        expect(quote.props("personAliasGuid")).toBe(me);
    });

    test.each([["its head row", "single"], ["a grouped continuation row", "middle"]] as const)("in a channel, on %s, the quote is a reply line with an arrow and no connector to the face", (_row, position) => {
        const wrapper = draw({ message: message({ quoted_message_id: 40, quoted } as Partial<TimelineMessage>), isChannel: true, position });

        expect(wrapper.find("[data-testid='quote-arrow']").exists()).toBe(true);
        expect(wrapper.find("[data-testid='quote-spine']").exists()).toBe(false);
        expect(wrapper.find(".chat-cm-gutter [data-testid='message-quote']").exists()).toBe(false);
    });

    test("passes on a jump to the original", async () => {
        const wrapper = draw({ message: message({ quoted_message_id: 40, quoted } as Partial<TimelineMessage>), position: "single" });
        expect(wrapper.findComponent(MessageQuote).exists()).toBe(true);

        wrapper.findComponent(MessageQuote).vm.$emit("jump", 40);

        expect(wrapper.emitted("jump")).toEqual([[40]]);
    });
});

// Edit in place, in both looks: the editor takes the text's place, loaded with the body and each
// admitted mention as a chip, and hands the row's owner a save with the wire text or a cancel.
// Enter saves and Escape cancels, an Escape kept from the panel behind; a save of the same text
// is only a cancel. A refusal's sentence stays under the editor with what the person typed.
describe("edit in place", () => {
    let mounted: VueWrapper | null = null;

    afterEach(() => {
        mounted?.unmount();
        mounted = null;
        document.body.innerHTML = "";
    });

    const lineup = ownConfirmed({ body: `Thanks <@${lee}>`, pieces: [{ type: "text", text: "Thanks " }, mentionLee] as TimelineMessage["pieces"] });

    function editing(props: RowProps): VueWrapper {
        mounted = mount(MessageRow, { props: { personAliasGuid: me, isOwn: true, isEditing: true, position: "single", ...props }, attachTo: document.body });
        return mounted;
    }

    function input(w: VueWrapper): HTMLElement {
        return w.get("[data-testid='edit-input']").element as HTMLElement;
    }

    async function key(w: VueWrapper, name: string): Promise<KeyboardEvent> {
        const event = new KeyboardEvent("keydown", { key: name, bubbles: true, cancelable: true });
        input(w).dispatchEvent(event);
        await nextTick();
        return event;
    }

    /** Adds text at the box's end, as typing there would. */
    async function typeAtEnd(w: VueWrapper, text: string): Promise<void> {
        const box = input(w);
        box.append(document.createTextNode(text));
        box.dispatchEvent(new InputEvent("input", { bubbles: true }));
        await nextTick();
    }

    function requests(w: VueWrapper): Array<Record<string, unknown>> {
        return (w.emitted("messageAction") ?? []).map(([request]) => request as Record<string, unknown>);
    }

    test.each([["a direct message", false], ["a channel", true]] as const)("takes the text's place in %s, loaded with the body and its mention as a chip", async (_kind, isChannel) => {
        const w = editing({ message: lineup, isChannel });
        await nextTick();

        expect(w.find("[data-testid='message-editor']").exists()).toBe(true);
        expect(w.find(".chat-bubble").exists()).toBe(false);
        expect(w.find(".chat-cm-text").exists()).toBe(false);
        expect(w.findAll("[data-testid='edit-input'] [data-testid='editor-chip'] .chat-mn-name").map(chip => chip.text())).toEqual(["Lee Leader"]);
        expect(plain(w.get("[data-testid='message-editor-foot']").text())).toContain("Esc to cancel, Enter to save");
        expect(w.get("[data-testid='edit-cancel']").text()).toBe("Cancel");
        expect(w.get("[data-testid='edit-save']").text()).toBe("Save");
    });

    test("takes focus as it opens", async () => {
        const w = editing({ message: lineup });
        await nextTick();
        await nextTick();

        expect(document.activeElement).toBe(input(w));
    });

    test("Enter asks to save the wire text, the mention kept as its token", async () => {
        const w = editing({ message: lineup });
        await nextTick();
        await typeAtEnd(w, " again");

        await key(w, "Enter");

        expect(requests(w)).toEqual([{ kind: "saveEdit", message: lineup, body: `Thanks <@${lee}> again` }]);
    });

    test("Save asks to save the same way", async () => {
        const w = editing({ message: lineup, isChannel: true });
        await nextTick();
        await typeAtEnd(w, "!");

        await w.get("[data-testid='edit-save']").trigger("click");

        expect(requests(w)).toEqual([{ kind: "saveEdit", message: lineup, body: `Thanks <@${lee}>!` }]);
    });

    test("Escape asks to cancel, and is not heard by the panel behind", async () => {
        const w = editing({ message: lineup });
        await nextTick();
        const behind = jest.fn();
        document.addEventListener("keydown", behind);

        await key(w, "Escape");
        document.removeEventListener("keydown", behind);

        expect(requests(w)).toEqual([{ kind: "cancelEdit", message: lineup }]);
        expect(behind).not.toHaveBeenCalled();
    });

    test("Cancel asks to cancel", async () => {
        const w = editing({ message: lineup });
        await nextTick();

        await w.get("[data-testid='edit-cancel']").trigger("click");

        expect(requests(w)).toEqual([{ kind: "cancelEdit", message: lineup }]);
    });

    test("saving the text unchanged, or emptied, only cancels", async () => {
        const w = editing({ message: lineup });
        await nextTick();
        await key(w, "Enter");

        input(w).replaceChildren();
        input(w).dispatchEvent(new InputEvent("input", { bubbles: true }));
        await nextTick();
        await key(w, "Enter");

        expect(requests(w)).toEqual([{ kind: "cancelEdit", message: lineup }, { kind: "cancelEdit", message: lineup }]);
    });

    test("a refusal shows the server's sentence under the editor, which keeps what was typed", async () => {
        const w = editing({ message: lineup });
        await nextTick();
        await typeAtEnd(w, " again");
        await key(w, "Enter");

        await w.setProps({ editError: { code: "rpc.not_editable", severity: "failed", text: "This message can't be edited." } });

        expect(w.get("[data-testid='edit-error']").text()).toBe("This message can't be edited.");
        expect(w.get("[data-testid='edit-error']").attributes("role")).toBe("alert");
        expect(w.find("[data-testid='message-editor']").exists()).toBe(true);
        expect(input(w).querySelectorAll("[data-testid='editor-chip']")).toHaveLength(1);
        expect(input(w).textContent?.endsWith(" again")).toBe(true);
    });

    test("Save is held while the save is out", async () => {
        const w = editing({ message: lineup, isEditSaving: true });
        await nextTick();

        expect(w.get("[data-testid='edit-save']").attributes("disabled")).toBeDefined();
    });

    test.each([["a direct message", false], ["a channel", true]] as const)("Edited slides in once the edit is saved in %s, and not on a message edited before it was drawn", async (_kind, isChannel) => {
        const before = draw({ message: message({ edited_at: nineOhTwo }), isChannel, position: "single" });
        expect(before.get("[data-testid='message-edited']").classes()).not.toContain("is-swapping");

        const w = draw({ message: message(), isChannel, position: "single" });
        await w.setProps({ message: message({ edited_at: nineOhTwo, body: "Morning everyone" }) });

        expect(w.get("[data-testid='message-edited']").classes()).toContain("is-swapping");
    });
});

// A phone, or a screen with no hover, acts on a message from a sheet a tap on the message opens,
// in place of the bar. Its items are the bar's and menu's, and each runs the same request.
describe("the phone's action sheet", () => {
    let mounted: VueWrapper | null = null;

    afterEach(() => {
        mounted?.unmount();
        mounted = null;
        document.body.innerHTML = "";
    });

    function onPhone(props: RowProps): VueWrapper {
        mounted = mount(MessageRow, { props: { personAliasGuid: me, isPhone: true, position: "single", ...props }, attachTo: document.body });
        return mounted;
    }

    function sheet(): HTMLElement | null {
        return document.querySelector<HTMLElement>("[data-testid='action-sheet']");
    }

    function sheetItem(label: string): HTMLElement {
        const item = Array.from(sheet()?.querySelectorAll<HTMLElement>("button") ?? []).find(button => button.textContent?.trim() === label);
        if (!item) {
            throw new Error(`No sheet item ${label}`);
        }
        return item;
    }

    test.each([["a direct message", false], ["a channel", true]] as const)("opens from a tap on the message in %s", async (_kind, isChannel) => {
        const w = onPhone({ message: message(), isChannel });
        expect(sheet()).toBeNull();

        await w.get(".chat-message-body").trigger("click");

        expect(sheet()).not.toBeNull();
        expect(w.find("[data-testid='action-bar']").exists()).toBe(false);
    });

    test("a tap on a button in the message does not open it", async () => {
        const w = onPhone({ message: message({ parent_id: 40, shown_in_channel: true }) });

        await w.get("[data-testid='thread-reply-label']").trigger("click");

        expect(sheet()).toBeNull();
    });

    test.each([
        ["on a wide screen", { isPhone: false }],
        ["on a deleted message", { message: message({ deleted_at: nineOhTwo, body: null }) }],
        ["on a send still going", { message: null, pending: pendingRow(), isOwn: true }],
        ["in a room the person is previewing", { isPreviewing: true }],
        ["on a message open in the editor", { isEditing: true, isOwn: true }]
    ] as const)("does not open %s", async (_where, extra) => {
        const w = onPhone({ message: message(), ...extra } as RowProps);

        await w.trigger("click");

        expect(sheet()).toBeNull();
    });

    test("its Quote asks for the same quote the bar would, and its Reply in thread opens the thread", async () => {
        const target = message();
        const w = onPhone({ message: target });
        await w.trigger("click");

        sheetItem("Quote").click();
        await nextTick();
        expect(w.emitted("messageAction")).toEqual([[{ kind: "quote", message: target }]]);

        await w.trigger("click");
        sheetItem("Reply in thread").click();
        await nextTick();
        expect(w.emitted("openThread")).toEqual([[{ rootId: 42, messageId: null }]]);
    });

    // A reply also sent is a message of its thread: its Reply in thread opens that thread at the
    // reply, never a thread rooted at the reply itself, which has none.
    test("its Reply in thread on a reply also sent opens the reply's own thread at the reply", async () => {
        const w = onPhone({ message: message({ id: 51, parent_id: 40, shown_in_channel: true }) });
        await w.trigger("click");

        sheetItem("Reply in thread").click();
        await nextTick();
        expect(w.emitted("openThread")).toEqual([[{ rootId: 40, messageId: 51 }]]);
    });

    // A bubble is the message in a direct message; a channel's message has no bubble, so the
    // picker is placed on its whole row.
    test.each([["a direct message", false], ["a channel", true]] as const)("its plus asks for the picker placed on the message in %s, since the sheet closes under it", async (_kind, isChannel) => {
        const target = message();
        const w = onPhone({ message: target, isChannel, reactionHost: host([]) });
        await w.trigger("click");

        sheet()?.querySelector<HTMLElement>("[data-testid='action-sheet-reactions'] button[aria-label='More reactions']")?.click();
        await nextTick();

        const [request] = (w.emitted("messageAction") ?? [])[0] as [{ kind: string, anchor: HTMLElement, opening?: { startsOn?: string } }];
        expect(request.kind).toBe("openPicker");
        expect(request.anchor).toBe(isChannel ? root(w) : w.get(".chat-bubble").element);
        expect(request.opening).toEqual({ startsOn: "full" });
    });
});

/** What the shell hands every row of the reactions: the favorites, the full list of who reacted, the picker's message. */
function host(favorites: unknown[], listReactors: jest.Mock = jest.fn(async () => null), pickerAnchor: HTMLElement | null = null): Record<string, unknown> {
    return { favorites, listReactors, pickerAnchorOf: () => pickerAnchor };
}

// The bar holding the open picker stays drawn; every other row asks only about its own message,
// so opening the picker on one message touches no other row. A row lends the column's one bar
// its props, so the bar gets the anchor of whichever message it is on.
describe("the open picker", () => {
    test("is handed to the bar of the message it is open on, and to no other row's", async () => {
        const anchor = document.createElement("button");
        const target = message();
        const other = message({ id: target.id + 1 });
        const reactionHost = { ...host([]), pickerAnchorOf: (messageId: number) => messageId === target.id ? anchor : null };

        const lent = new Map<number, ActionBarDock>();
        const column = defineComponent({
            components: { MessageRow },
            setup() {
                const bar = useActionBar(() => null);
                const offer = bar.offer;
                bar.offer = (messageId, dock): void => {
                    lent.set(messageId, dock);
                    offer(messageId, dock);
                };
                return { personAliasGuid: me, rows: [target, other], reactionHost };
            },
            template: `<div><MessageRow v-for="row in rows" :key="row.id" :message="row" :personAliasGuid="personAliasGuid" :reactionHost="reactionHost" /></div>`
        });
        mount(column);
        await nextTick();

        expect(lent.get(target.id)?.props.value.pickerAnchor).toBe(anchor);
        expect(lent.get(other.id)?.props.value.pickerAnchor).toBeNull();
    });
});

const heart = { code: "heart", count: 3, mine: true, emoji: "❤️", image_url: null, label: "red heart" };

// The reactions row under a message, in both looks: the chips the message carries, a press asking
// the shell to toggle the reader's reaction, and who reacted behind each chip's card.
describe("the reactions row", () => {
    test.each([["a direct message", false], ["a channel", true]] as const)("draws the message's chips in %s, and a press asks to react", async (_kind, isChannel) => {
        const target = message({ reactions: [heart] } as Partial<TimelineMessage>);
        const w = draw({ message: target, isChannel, reactionHost: host([]) });

        const chip = w.get("[data-testid='reaction-chip']");
        expect(chip.attributes("aria-label")).toBe("red heart, 3, including you. Remove yours");

        await chip.trigger("click");
        expect(w.emitted("messageAction")).toEqual([[{ kind: "react", message: target, code: "heart" }]]);
    });

    // The add chip goes with the last chip a choice in the picker takes away, so the picker is
    // placed on what stays, the bubble or a channel's whole row, and the add chip is only where
    // focus comes back to.
    test.each([["a direct message", false, ".chat-bubble"], ["a channel", true, null]] as const)("its add chip asks for the picker placed on the message in %s, focus coming back to the add chip", async (_kind, isChannel, bubble) => {
        const w = draw({ message: message({ reactions: [heart] } as Partial<TimelineMessage>), isChannel, reactionHost: host([]) });

        await w.get("[data-testid='reaction-add']").trigger("click");

        const [request] = (w.emitted("messageAction") ?? [])[0] as [{ kind: string, anchor: HTMLElement, opener?: HTMLElement, opening?: unknown }];
        expect(request.kind).toBe("openPicker");
        expect(request.anchor).toBe(bubble ? w.get(bubble).element : root(w));
        expect(request.opener).toBe(w.get("[data-testid='reaction-add']").element);
        expect(request.opening).toBeUndefined();
    });

    test("tucks into the bubble, which grows its foot to make room", () => {
        const w = draw({ message: message({ reactions: [heart] } as Partial<TimelineMessage>), reactionHost: host([]) });

        expect(root(w).classList.contains("has-reactions")).toBe(true);
    });

    test("draws nothing for a message with no reactions and no thread", () => {
        const w = draw({ message: message(), reactionHost: host([]) });

        expect(w.find("[data-testid='message-reactions']").exists()).toBe(false);
        expect(root(w).classList.contains("has-reactions")).toBe(false);
    });

    test("is drawn but not pressable for a person previewing the room", () => {
        const w = draw({ message: message({ reactions: [heart] } as Partial<TimelineMessage>), isPreviewing: true, reactionHost: host([]) });

        expect(w.get("[data-testid='reaction-chip']").attributes("aria-disabled")).toBe("true");
    });

    test("asks the shell for the full list of who reacted to this message", async () => {
        const page = { reactors: [], has_more: false };
        const listReactors = jest.fn(async () => page);
        const w = draw({ message: message({ reactions: [heart] } as Partial<TimelineMessage>), reactionHost: host([], listReactors) });
        const list = (w.findComponent({ name: "messageReactions.partial" }).props() as { listReactors: (code: string | null, after: unknown) => Promise<unknown> }).listReactors;

        await expect(list("heart", null)).resolves.toBe(page);
        expect(listReactors).toHaveBeenCalledWith(42, "heart", null);
    });

    test("names the reader for the chips' cards, and keeps the cards away while the picker is open", () => {
        const w = draw({
            message: message({ reactions: [heart] } as Partial<TimelineMessage>),
            personAliasGuid: "a0000001-0000-4000-8000-000000000000",
            personName: "Jordan Ellis",
            personPhotoUrl: "https://rock.example.org/jordan.jpg",
            reactionHost: host([], undefined, document.createElement("div"))
        });
        const props = w.findComponent({ name: "messageReactions.partial" }).props() as { reader: unknown, isCardDisabled: boolean };

        expect(props.reader).toEqual({ personAliasGuid: "a0000001-0000-4000-8000-000000000000", name: "Jordan Ellis", photoUrl: "https://rock.example.org/jordan.jpg" });
        expect(props.isCardDisabled).toBe(true);
    });

    // The page adds a message's first reaction in place, so the row must already hold its
    // reactions part to see the reader's chip arrive, rather than mount with it as a page draws one.
    test.each([["a direct message", false], ["a channel", true]] as const)("pops the reader's first reaction on a message in %s", async (_kind, isChannel) => {
        const target = reactive(message({ reactions: [] } as Partial<TimelineMessage>));
        const w = draw({ message: target, isChannel, reactionHost: host([]) });

        target.reactions!.push({ ...heart, count: 1 });
        await nextTick();
        await nextTick();

        expect(w.get("[data-testid='reaction-chip']").classes()).toContain("is-popping");
    });
});

// A message's author, by name or face, and a person's chip in its body each open that person's
// profile card: the row asks for it as one more message action, naming the person and the element
// the card opens beside. The room's @all chip names no one, so it opens nothing.
describe("the profile card's openers", () => {
    type PersonRequest = { kind: string, personAliasGuid: string, anchor: HTMLElement };

    function personRequests(wrapper: VueWrapper): PersonRequest[] {
        return ((wrapper.emitted("messageAction") ?? []) as Array<[PersonRequest]>).map(([request]) => request).filter(request => request.kind === "person");
    }

    test("in Community, the head's name and face are buttons asking for the author's card", async () => {
        const wrapper = draw({ message: message(), isChannel: true, position: "first" });

        const name = wrapper.get("[data-testid='sender-name']");
        expect(name.element.tagName).toBe("BUTTON");
        expect(name.text()).toBe("Anna Reyes");
        await name.trigger("click");

        const face = wrapper.get("[data-testid='sender-avatar']");
        expect(face.element.tagName).toBe("BUTTON");
        expect(face.attributes("aria-label")).toBe("Anna Reyes");
        expect(face.findComponent(Avatar).exists()).toBe(true);
        await face.trigger("click");

        const requests = personRequests(wrapper);
        expect(requests.map(r => r.personAliasGuid)).toEqual([anna, anna]);
        expect(requests[0].anchor).toBe(name.element);
        expect(requests[1].anchor).toBe(face.element);

        // a row still sending is not stored yet, so it names no one to open
        const sending = mount(MessageRow, {
            props: { personAliasGuid: me, pending: pendingRow(), isOwn: true, isChannel: true, position: "single", personName: "Jordan Lee" }
        });
        expect(sending.get("[data-testid='sender-name']").element.tagName).not.toBe("BUTTON");
        expect(sending.find("[data-testid='sender-avatar']").exists()).toBe(false);
    });

    test("in Conversational, the name over a group and the face at its end are the openers", async () => {
        const first = draw({ message: message(), position: "first" });
        const name = first.get("[data-testid='sender-name']");
        expect(name.element.tagName).toBe("BUTTON");
        await name.trigger("click");
        expect(personRequests(first).map(r => r.personAliasGuid)).toEqual([anna]);

        const last = draw({ message: message(), position: "last", isDirectMessage: true });
        const face = last.get("[data-testid='sender-avatar']");
        expect(face.attributes("aria-label")).toBe("Anna Reyes");
        await face.trigger("click");
        expect(personRequests(last).map(r => r.personAliasGuid)).toEqual([anna]);
    });

    test("a tap on the author on a phone asks for the card and never opens the action sheet", async () => {
        const wrapper = mount(MessageRow, { props: { personAliasGuid: me, message: message(), isChannel: true, position: "first", isPhone: true }, attachTo: document.body });

        await wrapper.get("[data-testid='sender-name']").trigger("click");

        expect(personRequests(wrapper)).toHaveLength(1);
        expect(document.querySelector("[data-testid='action-sheet']")).toBeNull();
        wrapper.unmount();
        document.body.innerHTML = "";
    });

    test("a person's chip in the body is a button asking for that person's card; the @all chip is not", async () => {
        const wrapper = draw({
            message: message({ body: "x", pieces: [mentionLee, { type: "text", text: " and " }, mentionAll] }),
            isChannel: true,
            position: "first"
        });

        const [lee, all] = wrapper.findAll("[data-testid='piece-mention']");
        expect(lee.element.tagName).toBe("BUTTON");
        expect(all.element.tagName).not.toBe("BUTTON");
        await lee.trigger("click");
        await all.trigger("click");

        const requests = personRequests(wrapper);
        expect(requests.map(r => r.personAliasGuid)).toEqual([mentionLee.person_alias_guid]);
        expect(requests[0].anchor).toBe(lee.element);
    });
});

describe("the Leader pill", () => {
    test("a channel's row marks a sender who leads the room, beside the name, in the one pill recipe", () => {
        const w = draw({ message: message({ sender_is_leader: true }), isChannel: true, position: "first" });

        const pill = w.get("[data-testid='message-head'] [data-testid='leader-pill']");
        expect(pill.text()).toBe("Leader");
        expect(pill.classes()).toContain("chat-pill");
        expect(pill.element.previousElementSibling?.getAttribute("data-testid")).toBe("sender-name");
    });

    test("the pill sits before an announcement's own pill", () => {
        const w = draw({ message: message({ sender_is_leader: true, message_type: "announcement" }), isChannel: true });

        const pills = w.findAll("[data-testid='message-head'] .chat-pill").map(p => p.text());
        expect(pills).toEqual(["Leader", "Announcement"]);
    });

    test("a block card from a leader in a channel carries the pill too", () => {
        const blocks = [{ type: "text", text: "Sign up", fallback: { type: "text", text: "Sign up" } }] as unknown as TimelineMessage["blocks"];
        const w = draw({ message: message({ sender_is_leader: true, blocks }), isChannel: true });

        expect(w.get("[data-testid='message-head'] [data-testid='leader-pill']").text()).toBe("Leader");
    });

    test("a sender who does not lead the room, or a message that does not say, draws none", () => {
        expect(draw({ message: message({ sender_is_leader: false }), isChannel: true }).find("[data-testid='leader-pill']").exists()).toBe(false);
        expect(draw({ message: message(), isChannel: true }).find("[data-testid='leader-pill']").exists()).toBe(false);
    });

    test("a sender who is not listed draws none, even one who leads the room", () => {
        const w = draw({ message: message({ sender_is_leader: true, sender_listed: false }), isChannel: true, position: "first" });

        expect(w.find("[data-testid='leader-pill']").exists()).toBe(false);
    });

    test("a direct message never draws it, in its bubbles or its card", () => {
        const blocks = [{ type: "text", text: "Sign up", fallback: { type: "text", text: "Sign up" } }] as unknown as TimelineMessage["blocks"];

        expect(draw({ message: message({ sender_is_leader: true }), isChannel: false }).find("[data-testid='leader-pill']").exists()).toBe(false);
        expect(draw({ message: message({ sender_is_leader: true, blocks }), isChannel: false }).find("[data-testid='leader-pill']").exists()).toBe(false);
    });

    test("a system line never draws it", () => {
        const w = draw({ message: message({ sender_is_leader: true, message_type: "system", body: "Tom Hale joined." }), isChannel: true });

        expect(w.find("[data-testid='leader-pill']").exists()).toBe(false);
    });

    test("a pending send by a leader of the channel carries the pill from its first frame", () => {
        const w = draw({ pending: pendingRow(), isOwn: true, isChannel: true, position: "first", isPersonLeader: true });

        expect(w.get("[data-testid='message-head'] [data-testid='leader-pill']").text()).toBe("Leader");
    });

    test("the reader's own message reads the reader's own fact, whatever its copy says", () => {
        // A room that hides its roster sends its live copy saying no for everyone, its leader too.
        const kept = draw({ message: ownConfirmed({ sender_is_leader: false }), isOwn: true, isChannel: true, position: "first", isPersonLeader: true });
        expect(kept.find("[data-testid='leader-pill']").exists()).toBe(true);

        const dropped = draw({ message: ownConfirmed({ sender_is_leader: true }), isOwn: true, isChannel: true, position: "first", isPersonLeader: false });
        expect(dropped.find("[data-testid='leader-pill']").exists()).toBe(false);
    });

    test("another person's message reads its own fact, never the reader's", () => {
        const w = draw({ message: message({ sender_is_leader: false }), isChannel: true, position: "first", isPersonLeader: true });

        expect(w.find("[data-testid='leader-pill']").exists()).toBe(false);
    });

    test("the reader's own message in a direct message draws none, however they lead elsewhere", () => {
        const w = draw({ message: ownConfirmed(), isOwn: true, isChannel: false, position: "first", isPersonLeader: true });

        expect(w.find("[data-testid='leader-pill']").exists()).toBe(false);
    });

    test("a continuation row has no head and so no pill", () => {
        const w = draw({ message: message({ sender_is_leader: true }), isChannel: true, position: "middle" });

        expect(w.find("[data-testid='leader-pill']").exists()).toBe(false);
    });
});

describe("the author's face, with no quote connector to paint over", () => {
    test("the face takes no stacking position, since nothing is drawn under it any more", () => {
        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/messageRow.partial.obs"), "utf8");
        const style = source.slice(source.indexOf("<style"), source.indexOf("</style>"));

        expect(style).toMatch(/\.chat-cm-av\s*\{/);
        expect(style).not.toMatch(/\.chat-cm-av\s*\{[^}]*position:/);
    });
});

// A message's files are drawn under its text in both looks; a message of files alone draws no
// empty bubble, and a message whose text is withheld shows none of its files.
describe("a message's files", () => {
    const photo = { id: "d0000000-0000-4000-8000-000000000001", key: "t/d0000000-0000-4000-8000-000000000001", kind: "image" as const, name: "p.jpg", size: 900, type: "image/jpeg", width: 400, height: 300 };

    test.each([false, true])("files alone draw the files and no empty text (Community: %s)", isChannel => {
        const wrapper = draw({ message: message({ body: null, attachments: [photo] }), isChannel });

        expect(wrapper.find("[data-testid='attachment-grid']").exists()).toBe(true);
        expect(wrapper.find(".chat-bubble").exists()).toBe(false);
        expect(wrapper.find(".chat-cm-text").exists()).toBe(isChannel);
    });

    test("text and files draw both, the files after the text", () => {
        const wrapper = draw({ message: message({ attachments: [photo] }) });

        expect(wrapper.find(".chat-bubble").text()).toContain("Morning all");
        expect(wrapper.find("[data-testid='attachment-grid']").exists()).toBe(true);
    });

    test("a deleted or hidden message draws none of its files", () => {
        expect(draw({ message: message({ body: null, attachments: [photo], deleted_at: nineOhTwo }) }).find("[data-testid='attachment-grid']").exists()).toBe(false);
        expect(draw({ message: message({ body: null, attachments: [photo], visibility: "hidden", visibility_notice: "Hidden." }) }).find("[data-testid='attachment-grid']").exists()).toBe(false);
    });

    test("a send still going draws its files from what was picked", () => {
        const wrapper = draw({ pending: pendingRow({ body: "", attachments: [photo] }), isOwn: true, isSendingSlow: true });

        expect(wrapper.find("[data-testid='attachment-grid']").exists()).toBe(true);
        expect(wrapper.find(".chat-bubble").exists()).toBe(false);
        expect(wrapper.get("[data-testid='message-state']").text()).toBe("Sending…");
    });
});

// Each failed send is its own to mend: the Not sent marker is the one thing to press on the row,
// and opens Try again and Delete for that message alone, so one failure among several is dealt
// with where it stands. A send refused for good offers Delete alone, as the strip does. The strip
// over the composer keeps acting on all of a place's failures.
describe("a failed send's own choices", () => {
    const refusal = { code: "authz.no_mention_all_right", severity: "permission" as const, text: "You can't mention everyone in this channel." };
    const failed = (extra: Partial<PendingMessage> = {}): PendingMessage => pendingRow({ status: "failed", errorCode: "rpc.transport", ...extra });

    function drawAttached(props: RowProps): VueWrapper {
        return mount(MessageRow, { props: { personAliasGuid: me, ...props }, attachTo: document.body });
    }

    async function press(wrapper: VueWrapper): Promise<void> {
        await wrapper.get("[data-testid='message-failed-actions']").trigger("click");
        await nextTick();
    }

    const labels = (wrapper: VueWrapper): string[] => wrapper.findAll("[role='menuitem']").map(item => item.text());

    test.each([["a direct message", false], ["a channel", true]] as const)("the Not sent marker is a button on a failed row in %s, and on no other row", (_kind, isChannel) => {
        const wrapper = draw({ pending: failed(), isOwn: true, isChannel, position: "single" });
        const marker = wrapper.get("[data-testid='message-failed-actions']");

        expect(marker.element.tagName).toBe("BUTTON");
        expect(marker.text()).toContain("Not sent");
        expect(marker.attributes("aria-haspopup")).toBe("menu");

        expect(draw({ pending: pendingRow(), isOwn: true, isChannel }).find("[data-testid='message-failed-actions']").exists()).toBe(false);
        expect(draw({ message: ownConfirmed(), isOwn: true, isChannel }).find("[data-testid='message-failed-actions']").exists()).toBe(false);
        expect(draw({ message: message(), isChannel }).find("[data-testid='message-failed-actions']").exists()).toBe(false);
    });

    test.each([["a direct message", false], ["a channel", true]] as const)("pressing it offers Try again and Delete for this message in %s", async (_kind, isChannel) => {
        const wrapper = draw({ pending: failed(), isOwn: true, isChannel, position: "single" });

        expect(wrapper.find("[role='menu']").exists()).toBe(false);
        await press(wrapper);

        expect(labels(wrapper)).toEqual(["Try again", "Delete"]);
    });

    test("Try again asks the owner to send this one message, by its key", async () => {
        const wrapper = draw({ pending: failed({ localId: "b" }), isOwn: true, position: "single" });

        await press(wrapper);
        await wrapper.findAll("[role='menuitem']")[0].trigger("click");

        expect(wrapper.emitted("retry")).toEqual([[["b"]]]);
        expect(wrapper.emitted("discard")).toBeUndefined();
        expect(wrapper.find("[role='menu']").exists()).toBe(false);
    });

    test("Delete asks the owner to drop this one message, by its key", async () => {
        const wrapper = draw({ pending: failed({ localId: "b" }), isOwn: true, isChannel: true, position: "single" });

        await press(wrapper);
        await wrapper.findAll("[role='menuitem']")[1].trigger("click");

        expect(wrapper.emitted("discard")).toEqual([["b"]]);
        expect(wrapper.emitted("retry")).toBeUndefined();
    });

    test("a send refused for good offers Delete alone", async () => {
        const wrapper = draw({ pending: failed({ errorCode: refusal.code, refusal, reason: { alone: refusal.text, besideRetry: refusal.text, canRetry: false } }), isOwn: true, position: "single" });

        await press(wrapper);

        expect(labels(wrapper)).toEqual(["Delete"]);
    });

    test("Enter on the marker opens the choices and Escape closes them, with focus back on the marker", async () => {
        const wrapper = drawAttached({ pending: failed(), isOwn: true, position: "single" });
        const marker = wrapper.get("[data-testid='message-failed-actions']");

        (marker.element as HTMLElement).focus();
        await marker.trigger("click");
        await nextTick();
        expect(wrapper.find("[role='menu']").exists()).toBe(true);
        expect(document.activeElement?.textContent?.trim()).toBe("Try again");

        await wrapper.get("[role='menu']").trigger("keydown", { key: "Escape" });
        await nextTick();

        expect(wrapper.find("[role='menu']").exists()).toBe(false);
        expect(document.activeElement).toBe(marker.element);
        wrapper.unmount();
    });

    test("the marker keeps its reason for a screen reader and a hover, now on the control that names it", () => {
        const reason = "You're offline. Try again when you're back.";
        const wrapper = draw({ pending: failed({ reason: { alone: reason, besideRetry: "You're offline.", canRetry: true } }), isOwn: true, isChannel: true, position: "single" });
        const state = wrapper.get(".chat-cm-state.is-failed");

        expect(state.attributes("title")).toBe(reason);
        expect(state.find(".chat-sr-only").text()).toBe(`. ${reason}`);
    });
});
