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
// for a group its name. A channel shows its mark, name, meta, description, leaders and member
// count; a direct message the other person; a group its faces, its count and everyone in it as a
// chip. Leave asks inside the dialog, in words chosen by the room's kind, and a refusal replaces
// the question with the door's own sentence. A room that cannot be left says why instead, and one
// the person is not in says how to join. Every sentence names a channel without a "#".
//
// The dialog draws a room's facts in the exact shape chat_get_channel answers, and the leave and
// rename states in the exact shape useRooms keeps; what it asks for comes out as its events.
/* eslint-disable @typescript-eslint/naming-convention */
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
            ...props
        },
        attachTo: document.body
    });
    return wrapper;
}

function mountGroup(props: Record<string, unknown> = {}): VueWrapper {
    return mountDetails({ kind: "group", title: "Caleb, Hannah and Luis", facts: groupFacts(), ...props });
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

    test("takes focus itself as it opens, so its Close button's tooltip does not show unasked", async () => {
        const w = mountDetails();
        await settle();

        expect(document.activeElement).toBe(dialogOf(w));
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

    test("reads the group type, the campus by name and Public, or Private for a private room", async () => {
        const w = mountDetails();
        expect(w.findAll("[data-testid='details-meta-item']").map(item => item.text())).toEqual(["Small Group", "Downtown Campus", "Public"]);

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

    test("names its leaders with their faces, joined A and B, or A, B and C", async () => {
        const w = mountDetails();
        const leaders = w.get("[data-testid='details-leaders']");
        expect(spoken(leaders)).toBe("Led by Tom Hale and Grace Kim");
        expect(leaders.findComponent(Avatar).props("people")).toHaveLength(2);

        await w.setProps({ facts: channelFacts({ leaders: [tom, grace, ana] }) });
        expect(spoken(w.get("[data-testid='details-leaders']"))).toBe("Led by Tom Hale, Grace Kim and Ana Ruiz");

        await w.setProps({ facts: channelFacts({ leaders: [] }) });
        expect(w.find("[data-testid='details-leaders']").exists()).toBe(false);
    });

    test("counts its members as plain text where the reader may not see who is here, the singular for one", async () => {
        const w = mountDetails({ facts: channelFacts({ member_count: 1204 }) });
        const members = w.get("[data-testid='details-members']");
        expect(plain(members.text())).toBe(`${(1204).toLocaleString()} members`);
        expect(["BUTTON", "A"]).not.toContain(members.element.tagName);
        expect(members.find("button, a").exists()).toBe(false);

        await w.setProps({ facts: channelFacts({ member_count: 1 }) });
        expect(plain(w.get("[data-testid='details-members']").text())).toBe("1 member");
    });

    test("makes the count a link to the members panel where the reader may see who is here", async () => {
        const w = mountDetails({ facts: channelFacts({ member_count: 42 }), canSeeRoster: true });

        const members = w.get("[data-testid='details-members']");
        expect(members.element.tagName).toBe("BUTTON");
        expect(plain(members.text())).toBe("42 members");
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
    test("draws a pile of faces, the name, the head count and Group message", () => {
        const w = mountGroup();

        const identity = w.get("[data-testid='details-identity']");
        expect(identity.findComponent(Avatar).props("people")).toHaveLength(3);
        expect(w.get("[data-testid='details-name']").text()).toBe("Caleb, Hannah and Luis");
        expect(w.findAll("[data-testid='details-meta-item']").map(item => item.text())).toEqual(["4 people", "Group message"]);
    });

    test("lists everyone in it as a chip, the person included", () => {
        const w = mountGroup();

        const chips = w.findAll("[data-testid='details-person']").map(chip => spoken(chip));
        expect(chips).toEqual(["Jordan Ellis (you)", "Caleb Brooks", "Hannah Diaz", "Luis Romero"]);
        expect(w.get("[data-testid='details-people']").attributes("aria-label")).toBe("People in this conversation");
    });
});

describe("Leave", () => {
    test("is worded by the room's kind", () => {
        const channel = mountDetails();
        expect(channel.get("[data-testid='details-leave']").text()).toBe("Leave channel");
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
        expect(w.get("[data-testid='details-callout']").text()).toBe("This channel can't be left. You can turn its notifications off instead.");
    });

    test("a conversation of two says so and points to Hide", () => {
        const w = mountDetails({ kind: "person", title: "Anna Reyes", facts: personFacts() });

        expect(w.find("[data-testid='details-leave']").exists()).toBe(false);
        expect(w.get("[data-testid='details-callout']").text()).toBe("A conversation between two people can't be left. Hide it instead; it comes back when there's a new message.");
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
    test("is offered for a group only", () => {
        const channel = mountDetails();
        expect(channel.find("[data-testid='rename-input']").exists()).toBe(false);
        channel.unmount();

        const dm = mountDetails({ kind: "person", title: "Anna Reyes", facts: personFacts() });
        expect(dm.find("[data-testid='rename-input']").exists()).toBe(false);
        dm.unmount();

        const group = mountGroup();
        expect(group.find("[data-testid='rename-input']").exists()).toBe(true);
    });

    test("is a labelled field of up to 100 characters holding the name, the members' first names as its placeholder", () => {
        const w = mountGroup({ groupName: "Hike planning", title: "Hike planning" });

        const input = w.get("[data-testid='rename-input']").element as HTMLInputElement;
        expect(input.maxLength).toBe(100);
        expect(input.value).toBe("Hike planning");
        expect(input.placeholder).toBe("Caleb, Hannah, Luis");
        expect(input.labels?.[0]?.textContent?.trim()).toBe("Conversation name");
        expect(w.get("[data-testid='rename-help']").text()).toBe("Everyone here sees the new name. Leave it blank to show everyone's names.");
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

    test("is not offered in a group the person is not in", () => {
        const w = mountGroup({ facts: groupFacts({ is_member: false }) });

        expect(w.find("[data-testid='rename-input']").exists()).toBe(false);
    });
});
