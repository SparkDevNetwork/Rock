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
// The conversation header, mounted: the room's title with the line under it, the offer to turn on
// notifications, and what the header says while the connection is down. The subtitle gives way to
// the connection's state while there is one, a sweeping bar and a banner say the same thing more
// loudly, and the service's own sentence sits under the header in the tone its kind calls for. On a
// phone the header leads with Back, and the offer moves into the overflow menu, which is left out
// when it would hold nothing.
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import ChatHeader from "../../../../../src/Communication/Chat/ChatShell/components/chatHeader.partial.obs";
import { ChannelFacts, ChannelPerson, ChatError } from "../../../../../src/Communication/Chat/ChatShell/types.partial";
import Icon from "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs";

const lostConnection: ChatError = { code: "rt.disconnected", severity: "degraded", text: null };

let wrapper: VueWrapper | null = null;

function mountHeader(props: Record<string, unknown> = {}): VueWrapper {
    wrapper = mount(ChatHeader, {
        props: { title: "worship-team", ...props },
        attachTo: document.body
    });
    return wrapper;
}

/** Whether an icon of the chat's set with this name is drawn inside the element. */
function hasIcon(w: VueWrapper, inside: { element: Element }, name: string): boolean {
    return w.findAllComponents(Icon).some(icon => icon.props("name") === name && inside.element.contains(icon.element));
}

async function settle(): Promise<void> {
    await nextTick();
    await nextTick();
}

beforeAll(() => {
    // jsdom has no ResizeObserver, and the floating menu and tooltip keep their place with one.
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

describe("the title and subtitle", () => {
    test("shows the title and a group's subtitle", () => {
        const w = mountHeader({ title: "Anna and Marcus", subtitle: "3 people" });

        expect(w.get("[data-testid='header-title']").text()).toBe("Anna and Marcus");
        expect(w.get("[data-testid='header-subtitle']").text()).toBe("3 people");
    });

    test("draws no subtitle line when the wire has none and the connection is well", () => {
        const w = mountHeader({ subtitle: null });

        expect(w.find("[data-testid='header-subtitle']").exists()).toBe(false);
    });
    test("names a channel plainly and marks it with the hash icon, or the lock when it is private", async () => {
        const w = mountHeader({ title: "Worship Team", kind: "channel", isPublic: true });
        expect(w.get("[data-testid='header-title']").text()).toBe("Worship Team");
        expect(hasIcon(w, w.get(".chat-header-ic"), "hash")).toBe(true);

        await w.setProps({ isPublic: false });
        expect(w.get("[data-testid='header-title']").text()).toBe("Worship Team");
        expect(hasIcon(w, w.get(".chat-header-ic"), "lock")).toBe(true);
        expect(hasIcon(w, w.get(".chat-header-ic"), "hash")).toBe(false);

        await w.setProps({ title: "Anna Kim", kind: "person", people: ["Anna Kim"] });
        expect(w.get("[data-testid='header-title']").text()).toBe("Anna Kim");
        expect(w.find(".chat-header-ic").exists()).toBe(false);
    });
});

describe("the subtitle, from the room's facts", () => {
    const downtown = { guid: "d0000001-0000-4000-8000-000000000000", name: "Downtown Campus", shortCode: "Downtown" };

    /** A room's facts in the exact shape chat_get_channel answers. */
    function facts(overrides: Partial<ChannelFacts> = {}): ChannelFacts {
        return {
            channel_id: "c0000001-0000-4000-8000-000000000000",
            name: null,
            icon_url: null,
            channel_type: "shared",
            is_member: true,
            can_post: true,
            can_post_code: null,
            can_post_reason: null,
            max_length: 5000,
            can_leave: true,
            description: null,
            group_type_name: null,
            campus_id: null,
            is_public: true,
            member_count: 0,
            leaders: [],
            people: [],
            ...overrides
        };
    }

    function anna(details: ChannelPerson["details"]): ChannelPerson {
        return { person_alias_guid: "a0000002-0000-4000-8000-000000000000", nick_name: "Anna", last_name: "Reyes", avatar_url: null, details };
    }

    test("is a channel's description", () => {
        const w = mountHeader({ kind: "channel", facts: facts({ description: "Serve teams across the church" }), campuses: [downtown] });

        expect(w.get("[data-testid='header-subtitle']").text()).toBe("Serve teams across the church");
    });

    test("is left out for a channel with no description", () => {
        const w = mountHeader({ kind: "channel", facts: facts({ description: "  " }) });

        expect(w.find("[data-testid='header-subtitle']").exists()).toBe(false);
    });

    test("is the other person's campus by name in a conversation of two, when they show their details", () => {
        const w = mountHeader({ title: "Anna Reyes", kind: "person", people: ["Anna Reyes"], facts: facts({ channel_type: "dm", member_count: 2, people: [anna({ campus_id: downtown.guid, badge_keys: [] })] }), campuses: [downtown] });

        expect(w.get("[data-testid='header-subtitle']").text()).toBe("Downtown Campus");
    });

    test("is left out for a person who does not show their details, or has no campus", () => {
        const hidden = mountHeader({ kind: "person", facts: facts({ channel_type: "dm", member_count: 2, people: [anna(null)] }), campuses: [downtown] });
        expect(hidden.find("[data-testid='header-subtitle']").exists()).toBe(false);
        hidden.unmount();

        const none = mountHeader({ kind: "person", facts: facts({ channel_type: "dm", member_count: 2, people: [anna({ campus_id: null, badge_keys: [] })] }), campuses: [downtown] });
        expect(none.find("[data-testid='header-subtitle']").exists()).toBe(false);
    });

    test("is a group's head count from the room's members, a named group's included", () => {
        const w = mountHeader({ title: "Hike planning", kind: "group", facts: facts({ channel_type: "dm", name: "Hike planning", member_count: 4 }) });

        expect(w.get("[data-testid='header-subtitle']").text()).toBe("4 people");
    });

    test("counts one person as one person, as a group left with only the reader listed is", () => {
        const w = mountHeader({ title: "Hike planning", kind: "group", facts: facts({ channel_type: "dm", name: "Hike planning", member_count: 1 }) });

        expect(w.get("[data-testid='header-subtitle']").text()).toBe("1 person");
    });

    test("stands in for the line the shell worked out once the facts land", async () => {
        const w = mountHeader({ kind: "group", subtitle: "3 people" });
        expect(w.get("[data-testid='header-subtitle']").text()).toBe("3 people");

        await w.setProps({ facts: facts({ channel_type: "dm", member_count: 5 }) });

        expect(w.get("[data-testid='header-subtitle']").text()).toBe("5 people");
    });

    test("still gives way to the connection's state", () => {
        const w = mountHeader({ kind: "channel", facts: facts({ description: "Serve teams" }), connection: lostConnection });

        expect(w.find("[data-testid='header-subtitle']").exists()).toBe(false);
        expect(w.get("[data-testid='connection-badge']").text()).toContain("Reconnecting");
    });
});

describe("the connection's states", () => {
    test("say 'Reconnecting…' in the subtitle with the live dot while the connection is lost", () => {
        const w = mountHeader({ subtitle: "3 people", connection: lostConnection });
        const state = w.get("[data-testid='connection-badge']");

        expect(state.text()).toBe("Reconnecting…");
        expect(state.attributes("data-tone")).toBe("warning");
        expect(state.find("[data-testid='connection-dot']").exists()).toBe(true);
        // The subtitle's own words give way while the connection speaks.
        expect(w.find("[data-testid='header-subtitle']").exists()).toBe(false);
    });

    test("say 'Catching up on new messages…' once back and behind", () => {
        const w = mountHeader({ isBehind: true });
        const state = w.get("[data-testid='catching-up-badge']");

        expect(state.text()).toBe("Catching up on new messages…");
        expect(state.attributes("data-tone")).toBe("info");
    });

    test("sweep a bar across the top while offline only", async () => {
        const w = mountHeader({ connection: lostConnection });
        expect(w.find("[data-testid='connection-sweep']").exists()).toBe(true);

        await w.setProps({ connection: null, isBehind: true });
        expect(w.find("[data-testid='connection-sweep']").exists()).toBe(false);
    });

    test("open the offline banner, then the back-online banner, then nothing", async () => {
        const w = mountHeader({ connection: lostConnection });

        const offline = w.get("[data-testid='connection-banner']");
        expect(offline.attributes("data-tone")).toBe("warning");
        expect(offline.text()).toContain("You're offline.");
        expect(offline.text()).toContain("Reconnecting now. You can send again once you're back.");

        await w.setProps({ connection: null, isBehind: true });
        const back = w.get("[data-testid='connection-banner']");
        expect(back.attributes("data-tone")).toBe("info");
        expect(back.text()).toContain("Back online. Catching up on new messages…");

        await w.setProps({ isBehind: false });
        expect(w.find("[data-testid='connection-banner']").exists()).toBe(false);
    });
});

describe("the service banners", () => {
    test("show the server's read-only sentence in the warning tone", () => {
        const sentence = "Chat is read-only while we do some maintenance. You can read messages but not send them.";
        const w = mountHeader({ banner: sentence });
        const banner = w.get("[data-testid='service-banner']");

        expect(banner.text()).toContain(sentence);
        expect(banner.attributes("data-tone")).toBe("warning");
        expect(hasIcon(w, banner, "alert")).toBe(true);
    });

    test("show update required in the info tone with the refresh icon", () => {
        const w = mountHeader({ banner: "Chat needs a newer version of Rock.", isUpdateRequired: true });
        const banner = w.get("[data-testid='service-banner']");

        expect(banner.attributes("data-tone")).toBe("info");
        expect(hasIcon(w, banner, "refresh")).toBe(true);
    });
});

describe("the push offer on a desktop", () => {
    test("is a bell icon button labelled 'Turn on notifications'", async () => {
        const w = mountHeader({ isPushOffered: true });
        const offer = w.get("[data-testid='push-offer']");

        expect(offer.classes()).toContain("chat-icon-btn");
        expect(offer.attributes("aria-label")).toBe("Turn on notifications");
        expect(hasIcon(w, offer, "bell")).toBe(true);

        await offer.trigger("click");
        expect(w.emitted("turnOnPush")).toHaveLength(1);
    });

    test("is gone when not offered, and there is no overflow on a desktop", () => {
        const w = mountHeader({ isPushOffered: false });

        expect(w.find("[data-testid='push-offer']").exists()).toBe(false);
        expect(w.find("[data-testid='header-more']").exists()).toBe(false);
        expect(w.find("[data-testid='header-back']").exists()).toBe(false);
    });
});

describe("the header on a phone", () => {
    test("leads with a Back chevron that returns to the list", async () => {
        const w = mountHeader({ isPhone: true });
        const back = w.get("[data-testid='header-back']");

        expect(back.attributes("aria-label")).toBe("Back");
        expect(hasIcon(w, back, "left")).toBe(true);

        await back.trigger("click");
        expect(w.emitted("back")).toHaveLength(1);
    });

    test("moves the push offer into the overflow menu as its one item", async () => {
        const w = mountHeader({ isPhone: true, isPushOffered: true });

        expect(w.find("[data-testid='push-offer']").exists()).toBe(false);

        const more = w.get("[data-testid='header-more'] button");
        expect(more.attributes("aria-label")).toBe("More");
        await more.trigger("click");
        await settle();

        const items = Array.from(document.querySelectorAll<HTMLElement>("[role='menu'] [role='menuitem']"));
        expect(items.map(i => i.textContent?.trim())).toEqual(["Turn on notifications"]);

        items[0].click();
        await settle();
        expect(w.emitted("turnOnPush")).toHaveLength(1);
    });

    test("leaves the overflow out when it would hold nothing", () => {
        const w = mountHeader({ isPhone: true, isPushOffered: false });

        expect(w.find("[data-testid='header-more']").exists()).toBe(false);
    });
});

describe("the Mentions button", () => {
    test("sits beside Threads on a desktop, labelled Mentions, and opens the inbox", async () => {
        const w = mountHeader({ isThreadsOffered: true, isMentionsOffered: true });

        const buttons = w.findAll(".chat-header-actions button");
        const threadsAt = buttons.findIndex(b => b.attributes("data-testid") === "header-threads");
        const mentions = w.get("[data-testid='header-mentions']");
        expect(buttons.findIndex(b => b.element === mentions.element)).toBe(threadsAt + 1);
        expect(mentions.attributes("aria-label")).toBe("Mentions");
        expect(hasIcon(w, mentions, "at")).toBe(true);
        expect(mentions.attributes("aria-expanded")).toBe("false");
        expect(w.find("[data-testid='header-mentions-dot']").exists()).toBe(false);

        await mentions.trigger("click");
        expect(w.emitted("mentions")).toHaveLength(1);
    });

    test("shows as expanded while the inbox is open", () => {
        const w = mountHeader({ isMentionsOffered: true, isMentionsOpen: true });

        expect(w.get("[data-testid='header-mentions']").attributes("aria-expanded")).toBe("true");
    });

    test("carries the dot and says so while a mention is new", () => {
        const w = mountHeader({ isMentionsOffered: true, hasMentionDot: true });

        const mentions = w.get("[data-testid='header-mentions']");
        expect(mentions.attributes("aria-label")).toBe("Mentions, new mentions");
        const dot = mentions.get("[data-testid='header-mentions-dot']");
        expect(dot.attributes("aria-hidden")).toBe("true");
    });

    test("is not drawn unless offered", async () => {
        const w = mountHeader({ isThreadsOffered: true });
        expect(w.find("[data-testid='header-mentions']").exists()).toBe(false);

        await w.setProps({ isMentionsOffered: true });
        expect(w.find("[data-testid='header-mentions']").exists()).toBe(true);
    });

    test("moves into the phone's overflow menu, first, the menu button carrying the dot", async () => {
        const w = mountHeader({ isPhone: true, isThreadsOffered: true, isMentionsOffered: true, hasMentionDot: true });

        expect(w.find("[data-testid='header-mentions']").exists()).toBe(false);

        const more = w.get("[data-testid='header-more'] button");
        expect(more.attributes("aria-label")).toBe("More, new mentions");
        expect(w.find("[data-testid='header-more-dot']").exists()).toBe(true);

        await more.trigger("click");
        await settle();
        const items = Array.from(document.querySelectorAll<HTMLElement>("[role='menu'] [role='menuitem']"));
        // the label each item shows; the Mentions item's dot also reads "new" to a screen reader
        expect(items.map(i => i.querySelector(".chat-menu-label")?.textContent?.trim())).toEqual(["Mentions", "Threads"]);
        expect(items[0].textContent?.trim()).toBe("Mentionsnew");

        items[0].click();
        await settle();
        expect(w.emitted("mentions")).toHaveLength(1);
    });

    test("leaves the phone's menu button plain with no new mention", () => {
        const w = mountHeader({ isPhone: true, isMentionsOffered: true });

        expect(w.get("[data-testid='header-more'] button").attributes("aria-label")).toBe("More");
        expect(w.find("[data-testid='header-more-dot']").exists()).toBe(false);
    });
});

// The room's mark and title are one button that opens the conversation's details, named for the
// room and its kind with no "#"; on a phone the overflow menu offers the same details last.
describe("the details", () => {
    test.each([
        ["a channel", { title: "Worship Team", kind: "channel", isPublic: true }, "Worship Team, channel details"],
        ["a direct message", { title: "Anna Kim", kind: "person", people: ["Anna Kim"] }, "Anna Kim, conversation details"],
        ["a group", { title: "Hike planning", kind: "group", people: ["Caleb Brooks", "Hannah Diaz"] }, "Hike planning, conversation details"]
    ])("open from the title, a button naming %s's details", async (_kind, props, label) => {
        const w = mountHeader(props);

        const details = w.get("[data-testid='header-details']");
        expect(details.element.tagName).toBe("BUTTON");
        expect(details.attributes("aria-haspopup")).toBe("dialog");
        expect(details.attributes("aria-label")).toBe(label);
        expect(details.get("[data-testid='header-title']").text()).toBe(props.title);

        await details.trigger("click");
        expect(w.emitted("details")).toHaveLength(1);
    });

    test("holds nothing focusable inside the button, which is what takes focus", () => {
        const w = mountHeader({ title: "Worship Team", kind: "channel", isPublic: true });

        const details = w.get("[data-testid='header-details']");
        expect(details.findAll("[tabindex], button, a, input").length).toBe(0);
        expect(details.attributes("data-header-focus")).toBeDefined();
        expect(w.findAll("[data-header-focus]")).toHaveLength(1);
    });

    test("with no room to open, the title itself takes focus", () => {
        const w = mountHeader({ title: "Opening chat", kind: null });

        expect(w.find("[data-testid='header-details']").exists()).toBe(false);
        expect(w.get("[data-testid='header-title']").attributes("tabindex")).toBe("-1");
        expect(w.get("[data-testid='header-title']").attributes("data-header-focus")).toBeDefined();
    });

    test.each([
        ["channel", "Channel details"],
        ["person", "Conversation details"],
        ["group", "Conversation details"]
    ])("are the phone's last overflow item for a %s", async (kind, label) => {
        const w = mountHeader({ isPhone: true, kind, isThreadsOffered: true, people: ["Anna Kim"] });

        await w.get("[data-testid='header-more'] button").trigger("click");
        await settle();
        const items = Array.from(document.querySelectorAll<HTMLElement>("[role='menu'] [role='menuitem']"));
        expect(items.map(i => i.querySelector(".chat-menu-label")?.textContent?.trim())).toEqual(["Threads", label]);

        items[items.length - 1].click();
        await settle();
        expect(w.emitted("details")).toHaveLength(1);
    });
});

describe("the end of an outage", () => {
    test("is told once the connection is back and the catch-up after it is done, so the shell can say it", async () => {
        const w = mountHeader({ connection: lostConnection });

        await w.setProps({ connection: null, isBehind: true });
        expect(w.emitted("backOnline")).toBeUndefined();

        await w.setProps({ isBehind: false });
        expect(w.emitted("backOnline")).toEqual([[]]);
    });

    test("is never told when the connection was never lost", async () => {
        const w = mountHeader({ isBehind: true });

        await w.setProps({ isBehind: false });

        expect(w.emitted("backOnline")).toBeUndefined();
    });
});

// A channel's facepile and a group's Members button open the members panel, before Threads. The
// facepile draws up to three of the room's leaders over the head count, or the people icon where
// there are none to show; a conversation of two has neither. On a phone they fold into the menu.
describe("the way into the members panel", () => {
    /** A room's facts in the exact shape chat_get_channel answers. */
    function facts(overrides: Partial<ChannelFacts> = {}): ChannelFacts {
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
            description: null,
            group_type_name: null,
            campus_id: null,
            is_public: true,
            member_count: 42,
            leaders: [],
            people: [],
            ...overrides
        };
    }

    function leader(n: number, nick: string, last: string): ChannelFacts["leaders"][number] {
        return { person_alias_guid: `a000000${n}-0000-4000-8000-000000000000`, nick_name: nick, last_name: last, avatar_url: null };
    }

    const tom = leader(1, "Tom", "Hale");
    const grace = leader(2, "Grace", "Kim");
    const jordan = leader(3, "Jordan", "Lee");

    test("is a channel's facepile of its leaders' faces then the count, before Threads", async () => {
        const w = mountHeader({ kind: "channel", isMembersOffered: true, isThreadsOffered: true, facts: facts({ leaders: [tom, grace, jordan] }) });

        const pile = w.get("[data-testid='header-members']");
        expect(pile.element.tagName).toBe("BUTTON");
        expect(pile.attributes("aria-label")).toBe("42 members");
        expect(pile.text()).toContain("42");
        expect(pile.findAll(".chat-av").map(face => face.attributes("style"))).toHaveLength(3);
        expect(pile.findAll(".chat-av").every(face => face.attributes("style")?.includes("width: 26px"))).toBe(true);
        expect(hasIcon(w, pile, "users")).toBe(false);

        const buttons = w.findAll(".chat-header-actions button");
        expect(buttons.findIndex(b => b.element === pile.element)).toBe(buttons.findIndex(b => b.attributes("data-testid") === "header-threads") - 1);

        await pile.trigger("click");
        expect(w.emitted("members")).toHaveLength(1);
    });

    test("draws the people icon and the count where no leader can be shown, and says one member", () => {
        const w = mountHeader({ kind: "channel", isMembersOffered: true, facts: facts({ member_count: 1 }) });

        const pile = w.get("[data-testid='header-members']");
        expect(pile.attributes("aria-label")).toBe("1 member");
        expect(pile.findAll(".chat-av")).toHaveLength(0);
        expect(hasIcon(w, pile, "users")).toBe(true);
    });

    test("shows no faces to someone not in the room, only the icon and the count", () => {
        const w = mountHeader({ kind: "channel", isMembersOffered: true, facts: facts({ is_member: false, leaders: [tom] }) });

        const pile = w.get("[data-testid='header-members']");
        expect(pile.findAll(".chat-av")).toHaveLength(0);
        expect(hasIcon(w, pile, "users")).toBe(true);
    });

    test("is a group's Members button naming its head count, and nothing for a conversation of two", async () => {
        const group = mountHeader({ title: "Hike planning", kind: "group", people: ["Caleb Brooks", "Hannah Diaz"], isMembersOffered: true, facts: facts({ channel_type: "dm", member_count: 4 }) });

        const members = group.get("[data-testid='header-members']");
        expect(members.classes()).toContain("chat-icon-btn");
        expect(members.attributes("aria-label")).toBe("Members, 4 people");
        expect(hasIcon(group, members, "users")).toBe(true);
        await members.trigger("click");
        expect(group.emitted("members")).toHaveLength(1);
        group.unmount();

        const pair = mountHeader({ title: "Anna Kim", kind: "person", people: ["Anna Kim"], isMembersOffered: true, facts: facts({ channel_type: "dm", member_count: 2 }) });
        expect(pair.find("[data-testid='header-members']").exists()).toBe(false);
    });

    test("shows as expanded while the members panel is open", async () => {
        const w = mountHeader({ kind: "channel", isMembersOffered: true, facts: facts() });
        expect(w.get("[data-testid='header-members']").attributes("aria-expanded")).toBe("false");

        await w.setProps({ isMembersOpen: true });
        expect(w.get("[data-testid='header-members']").attributes("aria-expanded")).toBe("true");
    });

    test("is not drawn unless offered", () => {
        const w = mountHeader({ kind: "channel", facts: facts() });

        expect(w.find("[data-testid='header-members']").exists()).toBe(false);
    });

    test("moves into the phone's overflow menu as Members, before Threads", async () => {
        const w = mountHeader({ isPhone: true, kind: "channel", isMembersOffered: true, isThreadsOffered: true, facts: facts() });

        expect(w.find("[data-testid='header-members']").exists()).toBe(false);
        await w.get("[data-testid='header-more'] button").trigger("click");
        await settle();
        const items = Array.from(document.querySelectorAll<HTMLElement>("[role='menu'] [role='menuitem']"));
        expect(items.map(i => i.querySelector(".chat-menu-label")?.textContent?.trim())).toEqual(["Members", "Threads", "Channel details"]);

        items[0].click();
        await settle();
        expect(w.emitted("members")).toHaveLength(1);
    });
});
