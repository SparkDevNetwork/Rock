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
// The details dialog, mounted, opened from the header's title: who the conversation is, Leave, and
// for a group its name, drawn as a grouped list of short rows. A channel shows its mark, name,
// meta and description, then its members and leaders as rows; a direct message the other person;
// a group its faces, its count and everyone in it as a row. Leave asks inside the dialog, in words chosen by the room's kind, and a refusal replaces
// the question with the door's own sentence. A room that cannot be left says why instead, and one
// the person is not in says how to join. Every sentence names a channel without a "#".
//
// The dialog draws a room's facts in the exact shape chat_get_channel answers, and the leave and
// rename states in the exact shape useRooms keeps; what it asks for comes out as its events.
/* eslint-disable @typescript-eslint/naming-convention */
import { readFileSync } from "fs";
import { join } from "path";
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import ConversationDetails from "../../../../../src/Communication/Chat/ChatShell/components/conversationDetails.partial.obs";
import Avatar from "../../../../../src/Communication/Chat/ChatShell/ui/avatar.partial.obs";
import Icon from "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs";
import { LeaveState, RenameState } from "../../../../../src/Communication/Chat/ChatShell/composables/useRooms.partial";
import { ChannelFacts, ChannelLeader, ChannelPerson } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

// #region Fixtures

const downtown = { guid: "d0000001-0000-4000-8000-000000000000", name: "Downtown Campus", shortCode: "Downtown" };

function leader(n: number, nickName: string, lastName: string): ChannelLeader {
    return { person_alias_guid: `a000000${n}-0000-4000-8000-000000000000`, nick_name: nickName, last_name: lastName, avatar_url: null };
}

function person(n: number, nickName: string, lastName: string, details: ChannelPerson["details"] = null): ChannelPerson {
    return { ...leader(n, nickName, lastName), details };
}

const tom = leader(1, "Tom", "Hale");
const grace = leader(2, "Grace", "Kim");
const ana = leader(3, "Ana", "Ruiz");

/** chat_get_channel's answer for Young Adults, a public channel the person is in and may leave. */
function channelFacts(overrides: Partial<ChannelFacts> = {}): ChannelFacts {
    return {
        channel_id: "c0000001-0000-4000-8000-000000000000",
        name: "young-adults",
        icon_url: null,
        channel_type: "shared",
        is_member: true,
        can_post: true,
        can_post_code: null,
        can_post_reason: null,
        max_length: 5000,
        can_leave: true,
        description: "Twenties and thirties at Cedar Hill",
        group_type_name: "Small Group",
        campus_id: downtown.guid,
        is_public: true,
        member_count: 42,
        leaders: [tom, grace],
        people: [],
        ...overrides
    };
}

/** chat_get_channel's answer for a direct message with Anna, who shows her details. */
function personFacts(overrides: Partial<ChannelFacts> = {}): ChannelFacts {
    return channelFacts({
        channel_id: "c0000005-0000-4000-8000-000000000000",
        name: null,
        channel_type: "dm",
        can_leave: false,
        description: null,
        group_type_name: null,
        campus_id: null,
        is_public: false,
        member_count: 2,
        leaders: [],
        faces: [],
        people: [person(4, "Anna", "Reyes", { campus_id: downtown.guid, badge_keys: [] })],
        ...overrides
    });
}

/** chat_get_channel's answer for a group of four the person may leave and rename. */
function groupFacts(overrides: Partial<ChannelFacts> = {}): ChannelFacts {
    return channelFacts({
        channel_id: "c0000006-0000-4000-8000-000000000000",
        name: null,
        channel_type: "dm",
        can_leave: true,
        description: null,
        group_type_name: null,
        campus_id: null,
        is_public: false,
        member_count: 4,
        leaders: [],
        faces: [],
        people: [person(5, "Caleb", "Brooks"), person(6, "Hannah", "Diaz"), person(7, "Luis", "Romero")],
        ...overrides
    });
}

const idle: LeaveState = { kind: "idle" };
const renameIdle: RenameState = { kind: "idle" };

// #endregion Fixtures

// #region Helpers

let wrapper: VueWrapper | null = null;

function mountDetails(props: Record<string, unknown> = {}): VueWrapper {
    wrapper = mount(ConversationDetails, {
        props: {
            kind: "channel",
            title: "young-adults",
            facts: channelFacts(),
            groupName: null,
            campuses: [downtown],
            organizationName: "Cedar Hill Church",
            personName: "Jordan Ellis",
            personPhotoUrl: null,
            leave: idle,
            rename: renameIdle,
            canRename: false,
            nameMaxLength: 100,
            ...props
        },
        attachTo: document.body
    });
    return wrapper;
}

/** A group the person is in, which Rock's members door says they may rename. */
function mountGroup(props: Record<string, unknown> = {}): VueWrapper {
    return mountDetails({ kind: "group", title: "Caleb, Hannah and Luis", facts: groupFacts(), canRename: true, ...props });
}

function dialogOf(w: VueWrapper): HTMLElement {
    return w.get("[role='dialog']").element as HTMLElement;
}

/** The dialog's accessible name, read from what it is labelled by. */
function nameOf(dialog: HTMLElement): string {
    const labelledBy = dialog.getAttribute("aria-labelledby");
    return labelledBy ? document.getElementById(labelledBy)?.textContent?.trim() ?? "" : dialog.getAttribute("aria-label") ?? "";
}

function plain(text: string): string {
    return text.replace(/\s+/g, " ").trim();
}

/** An element's words as a screen reader reads them: an avatar's letters are hidden decoration. */
function spoken(inside: { element: Element }): string {
    const copy = inside.element.cloneNode(true) as Element;
    copy.querySelectorAll("[aria-hidden='true']").forEach(hidden => hidden.remove());
    return plain(copy.textContent ?? "");
}

async function settle(): Promise<void> {
    await nextTick();
    await nextTick();
}

/** Opens the leave confirm from the Leave button. */
async function askToLeave(w: VueWrapper): Promise<void> {
    await w.get("[data-testid='details-leave']").trigger("click");
    await settle();
}

function keydown(element: Element, key: string): void {
    element.dispatchEvent(new KeyboardEvent("keydown", { key, bubbles: true, cancelable: true }));
}

// #endregion Helpers

beforeAll(() => {
    if (typeof window.ResizeObserver === "undefined") {
        window.ResizeObserver = class {
            observe(): void { /* jsdom lays nothing out, so there is nothing to watch. */ }
            unobserve(): void { /* As above. */ }
            disconnect(): void { /* As above. */ }
        } as unknown as typeof ResizeObserver;
    }
});

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
    document.body.innerHTML = "";
});

describe("the dialog", () => {
    test("is titled Channel details for a channel and Conversation details for a direct message or group", () => {
        const channel = mountDetails();
        expect(nameOf(dialogOf(channel))).toBe("Channel details");
        channel.unmount();

        const dm = mountDetails({ kind: "person", title: "Anna Reyes", facts: personFacts() });
        expect(nameOf(dialogOf(dm))).toBe("Conversation details");
        dm.unmount();

        const group = mountGroup();
        expect(nameOf(dialogOf(group))).toBe("Conversation details");
    });

    test("closes from its close button and on Escape", async () => {
        const w = mountDetails();

        const close = w.get("[data-testid='details-close']");
        expect(close.attributes("aria-label")).toBe("Close");
        await close.trigger("click");
        expect(w.emitted("close")).toHaveLength(1);

        keydown(dialogOf(w), "Escape");
        await settle();
        expect(w.emitted("close")).toHaveLength(2);
    });

    test("puts focus on the room's chosen mode as it opens, as a radio group is entered", async () => {
        const w = mountDetails({ notifyMode: "mentions" });
        await settle();

        const chosen = w.get("[data-testid='notify-option'][data-value='mentions']").element;
        expect(document.activeElement).toBe(chosen);
    });

    test("with no modes to choose puts focus on its Close button, whose tooltip does not show unasked", async () => {
        const w = mountDetails({ facts: channelFacts({ is_member: false }) });
        await settle();

        expect(document.activeElement).toBe(w.get("[data-testid='details-close']").element);
        await new Promise(resolve => setTimeout(resolve, 600));
        expect(document.querySelector("[role='tooltip']")).toBeNull();
    });

    test("reads the rooms' news said while it is open in its own live region, the assertive apart", async () => {
        const w = mountGroup({ announcement: { text: "You joined Volunteers", isAssertive: false, count: 3 } });
        expect(spoken(w.get("[data-testid='details-announcement']"))).toBe("");

        await w.setProps({ announcement: { text: "Renamed to Trail crew.", isAssertive: false, count: 4 } });
        expect(w.get("[data-testid='details-announcement']").attributes("aria-live")).toBe("polite");
        expect(spoken(w.get("[data-testid='details-announcement']"))).toBe("Renamed to Trail crew.");

        await w.setProps({ announcement: { text: "No.", isAssertive: true, count: 5 } });
        expect(spoken(w.get("[data-testid='details-alert']"))).toBe("No.");
        expect(spoken(w.get("[data-testid='details-announcement']"))).toBe("");
    });

    test("writes no sentence with a hash, whatever the room", async () => {
        const channel = mountDetails({ facts: channelFacts({ is_public: false }) });
        await askToLeave(channel);
        expect(channel.text()).not.toContain("#");
        channel.unmount();

        const notJoined = mountDetails({ title: "volunteers", facts: channelFacts({ name: "volunteers", is_member: false, can_leave: false }) });
        expect(notJoined.text()).not.toContain("#");
        notJoined.unmount();

        const cannot = mountDetails({ facts: channelFacts({ can_leave: false }) });
        expect(cannot.text()).not.toContain("#");
    });
});

describe("a channel's identity", () => {
    test("draws its mark and its name with no hash", () => {
        const w = mountDetails();

        expect(w.find("[data-testid='details-mark']").exists()).toBe(true);
        expect(w.get("[data-testid='details-name']").text()).toBe("young-adults");
    });

    test("shows the channel's own image as its mark, the hash or lock when it has none", async () => {
        const w = mountDetails({ facts: channelFacts({ icon_url: "https://church.example/GetImage.ashx?guid=team" }) });
        expect(w.get("[data-testid='details-mark'] [data-testid='channel-mark-image']").attributes("src")).toBe("https://church.example/GetImage.ashx?guid=team");

        await w.setProps({ facts: channelFacts({ icon_url: null }) });
        expect(w.find("[data-testid='channel-mark-image']").exists()).toBe(false);
        expect(w.findAllComponents(Icon).some(icon => icon.props("name") === "hash" && w.get("[data-testid='details-mark']").element.contains(icon.element))).toBe(true);
    });

    test("reads Public, or Private for a private room, the campus by name and the group type", async () => {
        const w = mountDetails();
        expect(w.findAll("[data-testid='details-meta-item']").map(item => item.text())).toEqual(["Public", "Downtown Campus", "Small Group"]);
        // The dots between them are drawn for the eye, so a screen reader hears the parts apart.
        expect(spoken(w.get(".chat-rd-meta"))).toBe("Public, Downtown Campus, Small Group");

        await w.setProps({ facts: channelFacts({ is_public: false, group_type_name: null, campus_id: null }) });
        expect(w.findAll("[data-testid='details-meta-item']").map(item => item.text())).toEqual(["Private"]);
    });

    test("ends the description with a period added where it has none, and keeps its own end", async () => {
        const w = mountDetails();
        expect(w.get("[data-testid='details-description']").text()).toBe("Twenties and thirties at Cedar Hill.");

        await w.setProps({ facts: channelFacts({ description: "Who is coming Sunday?" }) });
        expect(w.get("[data-testid='details-description']").text()).toBe("Who is coming Sunday?");

        await w.setProps({ facts: channelFacts({ description: null }) });
        expect(w.find("[data-testid='details-description']").exists()).toBe(false);
    });

    test("names its leaders in a Led by row with their faces, joined by commas", async () => {
        const w = mountDetails();
        const leaders = w.get("[data-testid='details-leaders']");
        expect(spoken(leaders)).toBe("Led by Tom Hale, Grace Kim");
        expect(leaders.findComponent(Avatar).props("people")).toHaveLength(2);

        await w.setProps({ facts: channelFacts({ leaders: [tom, grace, ana] }) });
        expect(spoken(w.get("[data-testid='details-leaders']"))).toBe("Led by Tom Hale, Grace Kim, Ana Ruiz");

        await w.setProps({ facts: channelFacts({ leaders: [] }) });
        expect(w.find("[data-testid='details-leaders']").exists()).toBe(false);
    });

    test("counts its members in a Members row of plain text where the reader may not see who is here", async () => {
        const w = mountDetails({ facts: channelFacts({ member_count: 1204 }) });
        const members = w.get("[data-testid='details-members']");
        expect(plain(members.text())).toBe(`Members ${(1204).toLocaleString()}`);
        expect(["BUTTON", "A"]).not.toContain(members.element.tagName);
        expect(members.find("button, a").exists()).toBe(false);

        await w.setProps({ facts: channelFacts({ member_count: 1 }) });
        expect(plain(w.get("[data-testid='details-members']").text())).toBe("Members 1");
    });

    test("holds the Members and Led by rows in one group", () => {
        const w = mountDetails();

        const group = w.get("[data-testid='details-facts']");
        expect(group.find("[data-testid='details-members']").exists()).toBe(true);
        expect(group.find("[data-testid='details-leaders']").exists()).toBe(true);
    });

    test("makes the Members row a way to the members panel where the reader may see who is here", async () => {
        const w = mountDetails({ facts: channelFacts({ member_count: 42 }), canSeeRoster: true });

        const members = w.get("[data-testid='details-members']");
        expect(members.element.tagName).toBe("BUTTON");
        expect(plain(members.text())).toBe("Members 42");
        expect(w.findAllComponents(Icon).some(icon => icon.props("name") === "right" && members.element.contains(icon.element))).toBe(true);

        await members.trigger("click");
        expect(w.emitted("members")).toHaveLength(1);
    });
});

describe("a direct message's identity", () => {
    test("draws the other person's face and name, their campus when they show it, and Direct message", () => {
        const w = mountDetails({ kind: "person", title: "Anna Reyes", facts: personFacts() });

        expect(w.get("[data-testid='details-identity']").findComponent(Avatar).props("people")).toEqual([{ name: "Anna Reyes", photoUrl: null }]);
        expect(w.get("[data-testid='details-name']").text()).toBe("Anna Reyes");
        expect(w.findAll("[data-testid='details-meta-item']").map(item => item.text())).toEqual(["Downtown Campus", "Direct message"]);
    });

    test("leaves the campus out for a person who keeps their details private", () => {
        const w = mountDetails({ kind: "person", title: "Anna Reyes", facts: personFacts({ people: [person(4, "Anna", "Reyes", null)] }) });

        expect(w.findAll("[data-testid='details-meta-item']").map(item => item.text())).toEqual(["Direct message"]);
    });
});

describe("a group's identity", () => {
    test("draws the group's mark, the name, the head count and Group message", () => {
        const w = mountGroup();

        const mark = w.get("[data-testid='details-identity'] [data-testid='group-face']");
        expect(mark.attributes("data-shape")).toBe("trio");
        expect((mark.element as HTMLElement).style.getPropertyValue("--chat-gf-s")).toBe("44px");
        expect(mark.findAll(".chat-av").map(face => face.text())).toEqual(["C", "H", "L"]);
        expect(w.get("[data-testid='details-name']").text()).toBe("Caleb, Hannah and Luis");
        expect(w.findAll("[data-testid='details-meta-item']").map(item => item.text())).toEqual(["4 people", "Group message"]);
    });

    test("draws a named group's first two and a count of everyone else by its head count", () => {
        const people = [person(5, "Caleb", "Brooks"), person(6, "Hannah", "Diaz"), person(7, "Luis", "Romero"), person(8, "Mia", "Ford"), person(9, "Noah", "Kent")];
        const w = mountGroup({ title: "Trail crew", facts: groupFacts({ name: "Trail crew", member_count: 9, people }) });

        const mark = w.get("[data-testid='details-identity'] [data-testid='group-face']");
        expect(mark.attributes("data-shape")).toBe("trio");
        expect(mark.get("[data-testid='group-face-more']").text()).toBe("+6");
    });

    test("lists everyone in it as a row, the person included", () => {
        const w = mountGroup();

        const chips = w.findAll("[data-testid='details-person']").map(chip => spoken(chip));
        expect(chips).toEqual(["Jordan Ellis (you)", "Caleb Brooks", "Hannah Diaz", "Luis Romero"]);
        expect(w.get("[data-testid='details-people'] [role='list']").attributes("aria-label")).toBe("People in this conversation");
        expect(w.findAll("[data-testid='details-person']").map(row => row.attributes("role"))).toEqual(["listitem", "listitem", "listitem", "listitem"]);
    });
});

describe("Leave", () => {
    test("is a row of its own group, worded by the room's kind", () => {
        const channel = mountDetails();
        expect(channel.get("[data-testid='details-leave']").text()).toBe("Leave channel");
        expect(channel.get("[data-testid='details-leave']").element.closest("[data-testid='details-leave-group']")).not.toBeNull();
        // A group with no label is no named group, so a screen reader announces no empty one around the row.
        expect(channel.findAll("[data-testid='details-leave-group'] [role='group']")).toHaveLength(0);
        channel.unmount();

        const group = mountGroup();
        expect(group.get("[data-testid='details-leave']").text()).toBe("Leave conversation");
    });

    test("asks inside the dialog first, naming the room with no hash", async () => {
        const w = mountDetails();
        await askToLeave(w);

        const box = w.get("[data-testid='leave-confirm']");
        expect(box.attributes("role")).toBe("group");
        expect(w.get("[data-testid='leave-question']").text()).toBe("Leave young-adults?");
        expect(w.emitted("leave")).toBeUndefined();
    });

    test("says what leaving a public channel, a private channel and a group means", async () => {
        const open = mountDetails();
        await askToLeave(open);
        expect(open.get("[data-testid='leave-body']").text())
            .toBe("You'll stop getting its messages and leave the group at Cedar Hill Church. You can join again from Browse channels.");
        open.unmount();

        const closed = mountDetails({ facts: channelFacts({ is_public: false }) });
        await askToLeave(closed);
        expect(closed.get("[data-testid='leave-body']").text())
            .toBe("You'll stop getting its messages and leave the group at Cedar Hill Church. To come back, a leader will need to add you.");
        closed.unmount();

        const group = mountGroup();
        await askToLeave(group);
        expect(group.get("[data-testid='leave-question']").text()).toBe("Leave Caleb, Hannah and Luis?");
        expect(group.get("[data-testid='leave-body']").text())
            .toBe("The others will see that you left. To come back, someone in the conversation will need to add you.");
    });

    test("Cancel puts the Leave button back and asks for nothing", async () => {
        const w = mountDetails();
        await askToLeave(w);

        await w.get("[data-testid='leave-cancel']").trigger("click");
        await settle();

        expect(w.find("[data-testid='leave-confirm']").exists()).toBe(false);
        expect(w.find("[data-testid='details-leave']").exists()).toBe(true);
        expect(w.emitted("leave")).toBeUndefined();
    });

    test("Leave asks the door, and reads Leaving with both buttons held while it is out, focus kept on Leave", async () => {
        const w = mountDetails();
        await askToLeave(w);
        const go = w.get("[data-testid='leave-go']");
        (go.element as HTMLElement).focus();

        await go.trigger("click");
        expect(w.emitted("leave")).toHaveLength(1);

        await w.setProps({ leave: { kind: "leaving" } });
        expect(plain(go.text())).toBe("Leaving");
        // Held, not disabled, since a disabled button drops the focus it holds to the page.
        expect(go.attributes("disabled")).toBeUndefined();
        expect(go.attributes("aria-disabled")).toBe("true");
        expect(w.get("[data-testid='leave-cancel']").attributes("aria-disabled")).toBe("true");
        expect(document.activeElement).toBe(go.element);

        await go.trigger("click");
        await w.get("[data-testid='leave-cancel']").trigger("click");
        expect(w.emitted("leave")).toHaveLength(1);
        expect(w.find("[data-testid='leave-confirm']").exists()).toBe(true);
    });

    test("Escape in the confirm cancels the confirm only, and the dialog stays", async () => {
        const w = mountDetails();
        await askToLeave(w);

        keydown(w.get("[data-testid='leave-cancel']").element, "Escape");
        await settle();

        expect(w.find("[data-testid='leave-confirm']").exists()).toBe(false);
        expect(w.emitted("close")).toBeUndefined();
    });

    test("a refusal replaces the question with the door's own sentence and an OK, which puts Leave back", async () => {
        const w = mountDetails();
        await askToLeave(w);
        await w.get("[data-testid='leave-go']").trigger("click");
        await w.setProps({ leave: { kind: "leaving" } });

        await w.setProps({ leave: { kind: "refused", text: "Leaving prayer-requests is turned off. A leader can remove you if you need to step away." } });
        await settle();

        expect(w.find("[data-testid='leave-question']").exists()).toBe(false);
        expect(w.get("[data-testid='leave-error']").text()).toBe("Leaving prayer-requests is turned off. A leader can remove you if you need to step away.");
        expect(w.find("[data-testid='leave-go']").exists()).toBe(false);
        const ok = w.get("[data-testid='leave-cancel']");
        expect(ok.text()).toBe("OK");
        expect(document.activeElement).toBe(ok.element);

        await ok.trigger("click");
        await settle();
        expect(w.find("[data-testid='leave-confirm']").exists()).toBe(false);
        expect(w.find("[data-testid='details-leave']").exists()).toBe(true);
    });

    test("offline says so in place of the question, and OK lets the person ask again", async () => {
        const w = mountDetails();
        await askToLeave(w);
        await w.get("[data-testid='leave-go']").trigger("click");
        await w.setProps({ leave: { kind: "offline" } });
        await settle();

        expect(w.get("[data-testid='leave-error']").text()).toBe("You're offline. Try again when you're back.");

        await w.get("[data-testid='leave-cancel']").trigger("click");
        await askToLeave(w);
        await w.get("[data-testid='leave-go']").trigger("click");
        expect(w.emitted("leave")).toHaveLength(2);
    });

    test("a leave that failed while online says to try again, not that the person is offline", async () => {
        const w = mountDetails();
        await askToLeave(w);
        await w.get("[data-testid='leave-go']").trigger("click");
        await w.setProps({ leave: { kind: "failed" } });
        await settle();

        expect(w.get("[data-testid='leave-error']").text()).toBe("Couldn't leave. Try again in a moment.");
    });

    test("a refusal from an earlier ask is not shown when the confirm opens again", async () => {
        const w = mountDetails({ leave: { kind: "refused", text: "Not now." } });
        await askToLeave(w);

        expect(w.get("[data-testid='leave-question']").text()).toBe("Leave young-adults?");
        expect(w.find("[data-testid='leave-error']").exists()).toBe(false);
    });
});

describe("a room that cannot be left", () => {
    test("a channel says so and points to its notifications, with no Leave button", () => {
        const w = mountDetails({ facts: channelFacts({ can_leave: false }) });

        expect(w.find("[data-testid='details-leave']").exists()).toBe(false);
        expect(w.get("[data-testid='details-callout']").text()).toBe("This channel can't be left. Turn notifications off instead.");
    });

    test("a conversation of two says so and points to Hide", () => {
        const w = mountDetails({ kind: "person", title: "Anna Reyes", facts: personFacts() });

        expect(w.find("[data-testid='details-leave']").exists()).toBe(false);
        expect(w.get("[data-testid='details-callout']").text()).toBe("A conversation between two people can't be left. Hide it instead.");
    });
});

describe("a channel the person has not joined", () => {
    test("says how to join it, by name with no hash, and offers no Leave", () => {
        const w = mountDetails({ title: "volunteers", facts: channelFacts({ name: "volunteers", is_member: false, can_leave: false }) });

        expect(w.get("[data-testid='details-callout']").text()).toBe("Join volunteers to choose its notifications and keep it in your sidebar.");
        expect(w.find("[data-testid='details-leave']").exists()).toBe(false);
        expect(w.findAll("[data-testid='details-callout']")).toHaveLength(1);
    });
});

describe("the conversation name", () => {
    test("is offered only where Rock's door says the person may rename the room", () => {
        const channel = mountDetails();
        expect(channel.find("[data-testid='rename-input']").exists()).toBe(false);
        channel.unmount();

        const dm = mountDetails({ kind: "person", title: "Anna Reyes", facts: personFacts() });
        expect(dm.find("[data-testid='rename-input']").exists()).toBe(false);
        dm.unmount();

        const group = mountGroup();
        expect(group.find("[data-testid='rename-input']").exists()).toBe(true);
        group.unmount();

        // A group the person is in, which Rock still will not let them rename.
        const refused = mountGroup({ canRename: false });
        expect(refused.find("[data-testid='rename-input']").exists()).toBe(false);
    });

    test("holds a name of up to the length the platform sets, and counts toward that length", async () => {
        const w = mountGroup({ nameMaxLength: 60 });
        const input = w.get("[data-testid='rename-input']");

        expect((input.element as HTMLInputElement).maxLength).toBe(60);
        await input.setValue("x".repeat(49));
        expect(w.find("[data-testid='rename-count']").exists()).toBe(false);
        await input.setValue("x".repeat(55));
        expect(w.get("[data-testid='rename-count']").text()).toBe("55 of 60 characters");
    });

    test("is a labelled field of up to 100 characters holding the name, the members' first names as its placeholder", () => {
        const w = mountGroup({ groupName: "Hike planning", title: "Hike planning" });

        const input = w.get("[data-testid='rename-input']").element as HTMLInputElement;
        expect(input.maxLength).toBe(100);
        expect(input.value).toBe("Hike planning");
        expect(input.placeholder).toBe("Caleb, Hannah, Luis");
        expect(input.labels?.[0]?.textContent?.trim()).toBe("Conversation name");
        expect(w.get("[data-testid='rename-help']").text()).toBe("Everyone here sees it. Blank shows everyone's names.");
    });

    test("holds Save until the name changes", async () => {
        const w = mountGroup({ groupName: "Hike planning", title: "Hike planning" });
        const save = w.get("[data-testid='rename-save']");
        expect(save.text()).toBe("Save");
        expect(save.attributes("disabled")).toBeDefined();

        await w.get("[data-testid='rename-input']").setValue("Hike planning  ");
        expect(w.get("[data-testid='rename-save']").attributes("disabled")).toBeDefined();

        await w.get("[data-testid='rename-input']").setValue("Trail crew");
        expect(w.get("[data-testid='rename-save']").attributes("disabled")).toBeUndefined();
    });

    test("Save and Enter each ask for the trimmed name", async () => {
        const w = mountGroup();
        const input = w.get("[data-testid='rename-input']");

        await input.setValue("  Trail crew ");
        await w.get("[data-testid='rename-save']").trigger("click");
        expect(w.emitted("rename")).toEqual([["Trail crew"]]);

        await input.setValue("Hike crew");
        await input.trigger("keydown", { key: "Enter" });
        expect(w.emitted("rename")).toEqual([["Trail crew"], ["Hike crew"]]);
    });

    test("Enter asks nothing while the name is unchanged", async () => {
        const w = mountGroup({ groupName: "Hike planning", title: "Hike planning" });

        await w.get("[data-testid='rename-input']").trigger("keydown", { key: "Enter" });

        expect(w.emitted("rename")).toBeUndefined();
    });

    test("an emptied name asks to go back to everyone's names", async () => {
        const w = mountGroup({ groupName: "Hike planning", title: "Hike planning" });

        await w.get("[data-testid='rename-input']").setValue("");
        await w.get("[data-testid='rename-save']").trigger("click");

        expect(w.emitted("rename")).toEqual([[""]]);
    });

    test("reads Saving with Save held and the field read only while the door is out, focus kept in the field", async () => {
        const w = mountGroup();
        const input = w.get("[data-testid='rename-input']");
        await input.setValue("Trail crew");
        (input.element as HTMLElement).focus();

        await w.setProps({ rename: { kind: "saving" } });

        expect(plain(w.get("[data-testid='rename-save']").text())).toBe("Saving");
        expect(w.get("[data-testid='rename-save']").attributes("disabled")).toBeDefined();
        expect(input.attributes("disabled")).toBeUndefined();
        expect(input.attributes("readonly")).toBeDefined();
        expect(document.activeElement).toBe(input.element);
    });

    test("leaves focus in the field once saved, where Escape still closes the dialog", async () => {
        const w = mountGroup();
        const input = w.get("[data-testid='rename-input']");
        await input.setValue("Trail crew");
        (input.element as HTMLElement).focus();
        await w.setProps({ rename: { kind: "saving" } });

        await w.setProps({ rename: { kind: "saved" }, groupName: "Trail crew" });
        await settle();

        expect(document.activeElement).toBe(input.element);
        keydown(input.element, "Escape");
        await settle();
        expect(w.emitted("close")).toHaveLength(1);
    });

    test.each([
        ["saving", { kind: "saving" }, "Saving"],
        ["saved", { kind: "saved" }, "Saved"],
        ["pending", { kind: "pending" }, "Saved, syncing"],
        ["refused", { kind: "refused", text: "No." }, "Not saved"],
        ["offline", { kind: "offline" }, "Not saved"],
        ["failed", { kind: "failed" }, "Not saved"]
    ])("the head's save indicator reads the rename %s", async (_kind, rename, words) => {
        const w = mountGroup();

        await w.setProps({ rename });

        const indicator = w.get("[data-testid='details-save']");
        expect(plain(indicator.text())).toBe(words);
        expect(indicator.attributes("data-state")).toBe(_kind === "offline" || _kind === "failed" ? "refused" : _kind);
    });

    test("the head's Saved fades after 1.6 seconds", async () => {
        jest.useFakeTimers();
        try {
            const w = mountGroup();
            await w.setProps({ rename: { kind: "saved" } });
            expect(w.get("[data-testid='details-save']").attributes("data-state")).toBe("saved");

            jest.advanceTimersByTime(1600);
            await settle();

            expect(w.get("[data-testid='details-save']").attributes("data-state")).toBe("fade");
        }
        finally {
            jest.useRealTimers();
        }
    });

    test("the head shows nothing before a save", () => {
        const w = mountGroup();

        expect(plain(w.get("[data-testid='details-save']").text())).toBe("");
    });

    test("counts the characters from 90", async () => {
        const w = mountGroup();
        const input = w.get("[data-testid='rename-input']");

        await input.setValue("x".repeat(89));
        expect(w.find("[data-testid='rename-count']").exists()).toBe(false);

        await input.setValue("x".repeat(95));
        expect(w.get("[data-testid='rename-count']").text()).toBe("95 of 100 characters");
    });

    test("shows a refusal's sentence under the field, with Save offered again", async () => {
        const w = mountGroup();
        await w.get("[data-testid='rename-input']").setValue("Trail crew");
        await w.setProps({ rename: { kind: "saving" } });

        await w.setProps({ rename: { kind: "refused", text: "Chat is read only for a few minutes while Cedar Hill updates it. Try again soon." } });

        expect(w.get("[data-testid='rename-note']").text()).toBe("Chat is read only for a few minutes while Cedar Hill updates it. Try again soon.");
        expect(w.get("[data-testid='rename-save']").attributes("disabled")).toBeUndefined();
        expect((w.get("[data-testid='rename-input']").element as HTMLInputElement).value).toBe("Trail crew");
    });

    test("a save that failed while online says to try again, not that the person is offline, with Save offered again", async () => {
        const w = mountGroup();
        await w.get("[data-testid='rename-input']").setValue("Trail crew");
        await w.setProps({ rename: { kind: "saving" } });

        await w.setProps({ rename: { kind: "failed" } });

        expect(w.get("[data-testid='rename-note']").text()).toBe("Couldn't save the name. Try again in a moment.");
        expect(w.get("[data-testid='rename-note']").attributes("data-kind")).toBe("error");
        expect(w.get("[data-testid='rename-save']").attributes("disabled")).toBeUndefined();
    });

    test("says a pending save can take a minute to reach everyone", async () => {
        const w = mountGroup();

        await w.setProps({ rename: { kind: "pending" } });

        expect(w.get("[data-testid='rename-note']").text()).toBe("Saved. It can take a minute to reach everyone.");
    });

    test("is not offered in a group the person is not in, which Rock's door says", () => {
        const w = mountGroup({ facts: groupFacts({ is_member: false }), canRename: false });

        expect(w.find("[data-testid='rename-input']").exists()).toBe(false);
    });
});

// The person's settings for the room, for a member: Notifications as one segmented All, Mentions,
// Off control, a radio group with the room's mode chosen and one line under it saying what only
// the chosen mode means; Sidebar as the Star and Hide switches, the words beside a switch
// toggling it too. Each change is told to the block at once, which saves it; the head's indicator
// and the notes under a switch or the choices show what the save came to, a refusal in the
// server's own words.
describe("Notifications", () => {
    function radios(w: VueWrapper): HTMLElement[] {
        return w.findAll("[data-testid='notify-option']").map(option => option.element as HTMLElement);
    }

    test("is a segmented radio group of three under its label, the room's mode chosen", () => {
        const w = mountDetails({ notifyMode: "mentions" });

        const group = w.get("[data-testid='details-notify'] [role='radiogroup']");
        expect(document.getElementById(group.attributes("aria-labelledby") ?? "")?.textContent?.trim()).toBe("Notifications");
        expect(radios(w).map(r => r.getAttribute("role"))).toEqual(["radio", "radio", "radio"]);
        expect(radios(w).map(r => plain(r.textContent ?? ""))).toEqual(["All", "Mentions", "Off"]);
        expect(radios(w).map(r => w.findAllComponents(Icon).find(icon => r.contains(icon.element))?.props("name"))).toEqual(["bell", "at-bell", "bell-off"]);
        expect(radios(w).map(r => r.getAttribute("aria-checked"))).toEqual(["false", "true", "false"]);
        // one stop for Tab, on the choice made
        expect(radios(w).map(r => r.getAttribute("tabindex"))).toEqual(["-1", "0", "-1"]);
    });

    test.each([
        ["all", "Every new message."],
        ["mentions", "Your name or @all."],
        ["silent", "Only mentions of your name."]
    ])("says in one line what only the chosen mode, %s, means", (mode, words) => {
        const w = mountDetails({ notifyMode: mode });

        const caption = w.get("[data-testid='notify-caption']");
        expect(caption.text()).toBe(words);
        expect(w.findAll("[data-testid='notify-caption']")).toHaveLength(1);
    });

    test("describes each mode by its own line, so a screen reader arrowing between them hears what each means", () => {
        const w = mountDetails({ notifyMode: "all" });

        expect(radios(w).map(r => document.getElementById(r.getAttribute("aria-describedby") ?? "")?.textContent?.trim())).toEqual([
            "Every new message.",
            "Your name or @all.",
            "Only mentions of your name."
        ]);
        expect(w.get("[data-testid='details-notify'] [role='radiogroup']").attributes("aria-describedby")).toBeUndefined();
    });

    test("with no mode of its own, says what All means, the mode Tab lands on", () => {
        const w = mountDetails({ notifyMode: null });

        expect(w.get("[data-testid='notify-caption']").text()).toBe("Every new message.");
    });

    test("moves its line to the newly chosen mode", async () => {
        const w = mountDetails({ notifyMode: "all" });

        await w.setProps({ notifyMode: "silent" });

        expect(w.get("[data-testid='notify-caption']").text()).toBe("Only mentions of your name.");
    });

    test("no longer carries the foot about threads and badges", () => {
        const w = mountDetails({ notifyMode: "all" });

        expect(w.find("[data-testid='notify-foot']").exists()).toBe(false);
        expect(w.text()).not.toContain("Threads you follow");
        expect(w.text()).not.toContain("always show a badge");
    });

    test("tells the block the mode chosen", async () => {
        const w = mountDetails({ notifyMode: "all" });

        radios(w)[2].click();
        await settle();
        radios(w)[0].click();
        await settle();

        expect(w.emitted("notify")).toEqual([["silent"]]);
    });

    test("moves and chooses with the arrow keys, wrapping round", async () => {
        const w = mountDetails({ notifyMode: "all" });
        // after the dialog has put focus on the chosen mode as it opened
        await settle();

        radios(w)[0].focus();
        keydown(radios(w)[0], "ArrowDown");
        await settle();
        expect(document.activeElement).toBe(radios(w)[1]);
        expect(w.emitted("notify")).toEqual([["mentions"]]);

        await w.setProps({ notifyMode: "mentions" });
        keydown(radios(w)[1], "ArrowUp");
        await settle();
        keydown(radios(w)[0], "ArrowUp");
        await settle();
        expect(document.activeElement).toBe(radios(w)[2]);
        expect(w.emitted("notify")?.slice(-2)).toEqual([["all"], ["silent"]]);
    });

    test("moves with Left and Right too, as a row of segments is read", async () => {
        const w = mountDetails({ notifyMode: "all" });
        await settle();

        radios(w)[0].focus();
        keydown(radios(w)[0], "ArrowRight");
        await settle();
        expect(document.activeElement).toBe(radios(w)[1]);

        await w.setProps({ notifyMode: "mentions" });
        keydown(radios(w)[1], "ArrowLeft");
        await settle();
        expect(document.activeElement).toBe(radios(w)[0]);
        expect(w.emitted("notify")).toEqual([["mentions"], ["all"]]);
    });

    test("shows a refusal's sentence under the choices", () => {
        const w = mountDetails({ notifyMode: "all", prefsNotes: { notify: { kind: "error", text: "Chat is read only tonight." } } });

        expect(w.get("[data-testid='notify-error']").text()).toBe("Chat is read only tonight.");
        expect(w.get("[data-testid='details-notify'] [role='radiogroup']").attributes("aria-describedby")).toContain(w.get("[data-testid='notify-error']").attributes("id"));
    });

    test("is offered in a direct message too, and not in a room the person is not in", () => {
        const dm = mountDetails({ kind: "person", title: "Anna Reyes", facts: personFacts(), notifyMode: "all" });
        expect(dm.find("[data-testid='details-notify']").exists()).toBe(true);
        expect(dm.find("[data-testid='details-sidebar']").exists()).toBe(true);
        dm.unmount();

        const out = mountDetails({ facts: channelFacts({ is_member: false, can_leave: false }) });
        expect(out.find("[data-testid='details-notify']").exists()).toBe(false);
        expect(out.find("[data-testid='details-sidebar']").exists()).toBe(false);
    });
});

describe("Sidebar", () => {
    function switchOf(w: VueWrapper, key: string): ReturnType<VueWrapper["get"]> {
        return w.get(`[data-testid='switch-${key}'] [role='switch']`);
    }

    test("is a group of the Star and Hide switches, Hide saying until when", () => {
        const w = mountDetails({ isStarred: true, isHidden: false });

        const group = w.get("[data-testid='details-sidebar'] [role='group']");
        expect(document.getElementById(group.attributes("aria-labelledby") ?? "")?.textContent?.trim()).toBe("Sidebar");
        expect(switchOf(w, "star").attributes("aria-checked")).toBe("true");
        expect(switchOf(w, "hide").attributes("aria-checked")).toBe("false");
        expect(plain(w.get("[data-testid='switch-star']").text())).toBe("Star");
        expect(plain(w.get("[data-testid='switch-hide']").text())).toBe("Hide Until there's a new message");
        expect(document.getElementById(switchOf(w, "star").attributes("aria-labelledby") ?? "")?.textContent).toBe("Star");
    });

    test("keeps its help, with the notify line, Hide's and a group's name, to a few short lines", () => {
        const w = mountGroup({ notifyMode: "all" });
        const words = (text: string): number => plain(text).split(" ").filter(word => word !== "").length;

        const help = w.findAll(".chat-sg-sub, [data-testid='notify-caption'], [data-testid='rename-help']").map(line => line.text());
        expect(help.reduce((sum, line) => sum + words(line), 0)).toBeLessThanOrEqual(20);
    });

    test("tells the block each switch pressed, the words beside a switch toggling it too", async () => {
        const w = mountDetails({ isStarred: false, isHidden: false });

        await switchOf(w, "star").trigger("click");
        await w.get("[data-testid='switch-hide'] .chat-sg-txt").trigger("click");

        expect(w.emitted("star")).toEqual([[true]]);
        expect(w.emitted("hide")).toEqual([[true]]);
    });

    test("carries each note under its own switch", () => {
        const w = mountDetails({
            isStarred: false,
            isHidden: true,
            prefsNotes: {
                hide: { kind: "info", text: "Hidden, and taken out of Starred. It leaves your list when you open another conversation." },
                star: { kind: "error", text: "Chat is read only tonight." }
            }
        });

        expect(w.get("[data-testid='switch-hide'] [data-testid='switch-note']").text()).toBe("Hidden, and taken out of Starred. It leaves your list when you open another conversation.");
        expect(w.get("[data-testid='switch-star'] [data-testid='switch-note']").attributes("data-kind")).toBe("error");
        expect(w.get("[data-testid='switch-star'] [data-testid='switch-note']").text()).toBe("Chat is read only tonight.");
    });
});

describe("the save indicator for the room's settings", () => {
    test("says Saving, Saved and Not saved as the newest save goes", async () => {
        const w = mountDetails({ prefsSave: { state: "saving", count: 1 } });
        const indicator = (): ReturnType<VueWrapper["get"]> => w.get("[data-testid='details-save']");

        expect(indicator().attributes("data-state")).toBe("saving");
        expect(plain(indicator().text())).toBe("Saving");

        await w.setProps({ prefsSave: { state: "saved", count: 1 } });
        expect(plain(indicator().text())).toBe("Saved");

        await w.setProps({ prefsSave: { state: "refused", count: 2 } });
        expect(plain(indicator().text())).toBe("Not saved");
    });

    test("reads the room's settings' news in the dialog's live region", async () => {
        const w = mountDetails({ prefsAnnouncement: null });

        await w.setProps({ prefsAnnouncement: { text: "Mentions only. Saved.", isAssertive: false, count: 4 } });

        expect(w.get("[data-testid='details-announcement']").text()).toBe("Mentions only. Saved.");
    });
});

// The grouped list's measures, read from the components' own styles, since the test renderer lays
// nothing out: a row's icon centred on its words, 48px rows and 40px segments on a phone, and the
// dialog's buttons as pills.
describe("the grouped list's measures", () => {
    /** A component's style rules, selector and body, read from its source with comments left out. */
    function rulesOf(relativePath: string): Array<{ selector: string, body: string }> {
        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell", relativePath), "utf8");
        const style = source.slice(source.indexOf(">", source.indexOf("<style")) + 1, source.indexOf("</style>")).replace(/\/\*[\s\S]*?\*\//g, "");
        return Array.from(style.matchAll(/([^{}]+)\{([^{}]*)\}/g)).map(m => ({ selector: m[1].trim(), body: m[2] }));
    }

    function bodyOf(relativePath: string, selector: string): string {
        return rulesOf(relativePath).filter(rule => rule.selector === selector).map(rule => rule.body).join("");
    }

    test("centres a row's icon on its words, never lifting it to the top", () => {
        const icon = bodyOf("ui/settingRow.partial.obs", ".chat-sg-ic");

        expect(bodyOf("ui/settingRow.partial.obs", ".chat-sg-row")).toMatch(/align-items:\s*center;/);
        expect(icon).not.toMatch(/align-self/);
        expect(icon).not.toMatch(/margin-top/);
    });

    test("draws a group's rows 44px high at least, and 48px on a phone, the name field's row too", () => {
        // The group owns its rows' height, so a row that is not a setting row, the name field's, is held to it too.
        expect(bodyOf("ui/settingGroup.partial.obs", ".chat-sg")).toMatch(/--chat-sg-row-min:\s*44px;/);
        expect(bodyOf("ui/settingGroup.partial.obs", ".chat-modal.is-sheet .chat-sg")).toMatch(/--chat-sg-row-min:\s*48px;/);
        expect(bodyOf("ui/settingRow.partial.obs", ".chat-sg-row")).toMatch(/min-height:\s*var\(--chat-sg-row-min\);/);
        expect(bodyOf("components/conversationDetails.partial.obs", ".chat-rn")).toMatch(/min-height:\s*var\(--chat-sg-row-min\);/);
    });

    test("draws the notify segments 32px high, and 40px on a phone", () => {
        expect(bodyOf("components/conversationDetails.partial.obs", ".chat-seg-opt")).toMatch(/height:\s*32px;/);
        expect(bodyOf("components/conversationDetails.partial.obs", ".chat-modal.is-sheet .chat-seg-opt")).toMatch(/height:\s*40px;/);
    });

    test("rounds the dialog's buttons, Leave's Cancel and Leave and the name's Save, as pills", () => {
        expect(bodyOf("components/conversationDetails.partial.obs", ".chat-rd-btn")).toMatch(/border-radius:\s*var\(--chat-r-full\);/);
    });

    test("rounds Chat settings' Turn on and Turn off as pills too", () => {
        expect(bodyOf("components/chatSettings.partial.obs", ".chat-ms-btn")).toMatch(/border-radius:\s*var\(--chat-r-full\);/);
    });
});
