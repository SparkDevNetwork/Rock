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
// notifications, the room's notify menu and Star, and what the header says while the connection is
// down. The subtitle gives way to
// the connection's state while there is one, a sweeping bar and a banner say the same thing more
// loudly, and the service's own sentence sits under the header in the tone its kind calls for. On a
// phone the header leads with Back, keeps the notify bell, and moves the offer and Star into the
// overflow menu, which is left out when it would hold nothing.
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

        await w.setProps({ title: "Anna Kim", kind: "person", people: [{ person_alias_guid: "anna-kim", nick_name: "Anna", last_name: "Kim", avatar_url: null }] });
        expect(w.get("[data-testid='header-title']").text()).toBe("Anna Kim");
        expect(w.find(".chat-header-ic").exists()).toBe(false);
    });
});

describe("a channel's own image", () => {
    test("marks a channel with its own image in the icon box, the hash when it has none", async () => {
        const w = mountHeader({ title: "Greeters", kind: "channel", isPublic: true, iconUrl: "https://church.example/GetImage.ashx?guid=team" });
        expect(w.get(".chat-header-ic [data-testid='channel-mark-image']").attributes("src")).toBe("https://church.example/GetImage.ashx?guid=team");
        expect(hasIcon(w, w.get(".chat-header-ic"), "hash")).toBe(false);

        await w.setProps({ iconUrl: null });
        expect(w.find("[data-testid='channel-mark-image']").exists()).toBe(false);
        expect(hasIcon(w, w.get(".chat-header-ic"), "hash")).toBe(true);
    });

    test("never draws a direct message's mark from an image", () => {
        const w = mountHeader({ title: "Anna Kim", kind: "person", iconUrl: "https://church.example/GetImage.ashx?guid=team", people: [{ person_alias_guid: "anna-kim", nick_name: "Anna", last_name: "Kim", avatar_url: null }] });

        expect(w.find("[data-testid='channel-mark-image']").exists()).toBe(false);
    });
});

describe("the subtitle, from the room's facts", () => {
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
            faces: [],
            people: [],
            ...overrides
        };
    }

    function anna(details: ChannelPerson["details"]): ChannelPerson {
        return { person_alias_guid: "a0000002-0000-4000-8000-000000000000", nick_name: "Anna", last_name: "Reyes", avatar_url: null, details };
    }

    test("is a channel's description", () => {
        const w = mountHeader({ kind: "channel", facts: facts({ description: "Serve teams across the church" }) });

        expect(w.get("[data-testid='header-subtitle']").text()).toBe("Serve teams across the church");
    });

    test("is left out for a channel with no description", () => {
        const w = mountHeader({ kind: "channel", facts: facts({ description: "  " }) });

        expect(w.find("[data-testid='header-subtitle']").exists()).toBe(false);
    });

    test("is left out in a conversation of two, whose facts carry no campus: only a person's card names it", () => {
        const shown = mountHeader({ title: "Anna Reyes", kind: "person", people: [{ person_alias_guid: "anna-reyes", nick_name: "Anna", last_name: "Reyes", avatar_url: null }], facts: facts({ channel_type: "dm", member_count: 2, people: [anna({ badge_keys: [] })] }) });
        expect(shown.find("[data-testid='header-subtitle']").exists()).toBe(false);
        shown.unmount();

        const hidden = mountHeader({ kind: "person", facts: facts({ channel_type: "dm", member_count: 2, people: [anna(null)] }) });
        expect(hidden.find("[data-testid='header-subtitle']").exists()).toBe(false);
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

// The church's prompt timing decides when the header offers push; Chat settings offers it always.
describe("the push offer on a desktop", () => {
    test("is a small pill in the church's color reading 'Turn on notifications', with no glyph", async () => {
        const w = mountHeader({ isPushOffered: true });
        const offer = w.get("[data-testid='push-offer']");

        expect(offer.element.tagName).toBe("BUTTON");
        expect(offer.classes()).toEqual(expect.arrayContaining(["chat-header-push", "is-brand", "is-sm"]));
        expect(offer.classes()).not.toContain("chat-icon-btn");
        expect(offer.text()).toBe("Turn on notifications");
        expect(offer.findAllComponents(Icon)).toHaveLength(0);

        await offer.trigger("click");
        expect(w.emitted("turnOnPush")).toHaveLength(1);
    });

    test("leaves the room's notify menu as the header's only bell", () => {
        const w = mountHeader({ kind: "channel", isPushOffered: true, isPrefsOffered: true, notifyMode: "all", isThreadsOffered: true, isMentionsOffered: true });
        const header = w.get("[data-testid='chat-header']");

        const bells = w.findAllComponents(Icon).filter(icon => ["bell", "bell-at", "bell-off"].includes(icon.props("name")) && header.element.contains(icon.element));
        expect(bells).toHaveLength(1);
        expect(w.get("[data-testid='header-notify']").element.contains(bells[0].element)).toBe(true);
    });

    test("is gone when not offered, and there is no overflow on a desktop", () => {
        const w = mountHeader({ isPushOffered: false });

        expect(w.find("[data-testid='push-offer']").exists()).toBe(false);
        expect(w.find("[data-testid='header-more']").exists()).toBe(false);
        expect(w.find("[data-testid='header-back']").exists()).toBe(false);
    });

    test("sits after Mentions and before the room's notify menu", () => {
        const w = mountHeader({ kind: "channel", isPushOffered: true, isPrefsOffered: true, notifyMode: "all", isThreadsOffered: true, isMentionsOffered: true });

        const order = w.findAll(".chat-header-actions > [data-testid]").map(b => b.attributes("data-testid"));
        expect(order).toEqual(["header-threads", "header-mentions", "push-offer", "header-notify", "header-star"]);
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

    test("opening More, which holds Members, says the person is reaching for it, so the panel's rights are read first", async () => {
        const w = mountHeader({ isPhone: true, kind: "channel", isMembersOffered: true });

        await w.get("[data-testid='header-more'] button").trigger("click");
        await settle();

        expect(w.emitted("membersIntent")).toHaveLength(1);
        expect(w.emitted("members")).toBeUndefined();
    });

    test("opening a More that holds no Members says nothing of it", async () => {
        const w = mountHeader({ isPhone: true, isPushOffered: true });

        await w.get("[data-testid='header-more'] button").trigger("click");
        await settle();

        expect(w.emitted("membersIntent")).toBeUndefined();
    });

    test("keeps the notify bell and moves Star into the overflow menu", async () => {
        const w = mountHeader({ isPhone: true, kind: "channel", isPrefsOffered: true, notifyMode: "all", isStarred: false });

        expect(w.find("[data-testid='header-notify']").exists()).toBe(true);
        expect(w.find("[data-testid='header-star']").exists()).toBe(false);

        await w.get("[data-testid='header-more'] button").trigger("click");
        await settle();

        const items = Array.from(document.querySelectorAll<HTMLElement>("[role='menu'] [role='menuitem']"));
        expect(items.map(i => i.textContent?.trim())).toEqual(["Star", "Channel details"]);

        items[0].click();
        await settle();
        expect(w.emitted("star")).toHaveLength(1);
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
        ["a direct message", { title: "Anna Kim", kind: "person", people: [{ person_alias_guid: "anna-kim", nick_name: "Anna", last_name: "Kim", avatar_url: null }] }, "Anna Kim, conversation details"],
        ["a group", { title: "Hike planning", kind: "group", people: [{ person_alias_guid: "caleb-brooks", nick_name: "Caleb", last_name: "Brooks", avatar_url: null }, { person_alias_guid: "hannah-diaz", nick_name: "Hannah", last_name: "Diaz", avatar_url: null }] }, "Hike planning, conversation details"]
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
        const w = mountHeader({ isPhone: true, kind, isThreadsOffered: true, people: [{ person_alias_guid: "anna-kim", nick_name: "Anna", last_name: "Kim", avatar_url: null }] });

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

// One members button, for a channel and a group alike, opens the members panel, before Threads: up
// to three of the room's other people, a channel's leaders first, then "+N" for everyone not
// drawn, or where no face can be shown (a hidden roster, someone not in the room) the people icon
// and the head count. A count past 999 is shortened and the label keeps it whole. A conversation of
// two has none. On a phone it folds into the menu.
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
            faces: [],
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

    /** The disc after the faces: "+N" for the people not drawn, or the head count after the people icon. */
    function countOf(button: ReturnType<VueWrapper["get"]>): string | null {
        const disc = button.find("[data-testid='header-members-count']");
        return disc.exists() ? disc.text() : null;
    }

    function person(n: number, nick: string, last: string): ChannelFacts["people"][number] {
        return { person_alias_guid: `b000000${n}-0000-4000-8000-000000000000`, nick_name: nick, last_name: last, avatar_url: null };
    }

    test("is a channel's three faces then +N for the people not drawn, before Threads", async () => {
        const w = mountHeader({ kind: "channel", isMembersOffered: true, isThreadsOffered: true, facts: facts({ leaders: [tom], faces: [tom, grace, jordan] }) });

        const pile = w.get("[data-testid='header-members']");
        expect(pile.element.tagName).toBe("BUTTON");
        expect(pile.attributes("aria-label")).toBe("42 members, open member list");
        expect(pile.findAll(".chat-av")).toHaveLength(3);
        expect(pile.findAll(".chat-av").every(face => face.attributes("style")?.includes("width: 26px"))).toBe(true);
        // 42 members less the reader and the three drawn
        expect(countOf(pile)).toBe("+38");
        expect(hasIcon(w, pile, "users")).toBe(false);

        const buttons = w.findAll(".chat-header-actions button");
        expect(buttons.findIndex(b => b.element === pile.element)).toBe(buttons.findIndex(b => b.attributes("data-testid") === "header-threads") - 1);

        await pile.trigger("click");
        expect(w.emitted("members")).toHaveLength(1);
    });

    test("draws no +N when every other person is a face", () => {
        const w = mountHeader({ kind: "channel", isMembersOffered: true, facts: facts({ member_count: 3, faces: [tom, grace] }) });

        const pile = w.get("[data-testid='header-members']");
        expect(pile.findAll(".chat-av")).toHaveLength(2);
        expect(countOf(pile)).toBeNull();
        expect(pile.attributes("aria-label")).toBe("3 members, open member list");
    });

    test("draws the people icon and the count where no face can be shown, and says one member", () => {
        const w = mountHeader({ kind: "channel", isMembersOffered: true, facts: facts({ member_count: 1 }) });

        const pile = w.get("[data-testid='header-members']");
        expect(pile.attributes("aria-label")).toBe("1 member, open member list");
        expect(pile.findAll(".chat-av")).toHaveLength(0);
        expect(hasIcon(w, pile, "users")).toBe(true);
        expect(countOf(pile)).toBe("1");
    });

    test("shows no faces to someone not in the room, only the icon and the whole count", () => {
        const w = mountHeader({ kind: "channel", isMembersOffered: true, facts: facts({ is_member: false, leaders: [tom], faces: [tom] }) });

        const pile = w.get("[data-testid='header-members']");
        expect(pile.findAll(".chat-av")).toHaveLength(0);
        expect(hasIcon(w, pile, "users")).toBe(true);
        expect(countOf(pile)).toBe("42");
    });

    test("shortens a count past 999 and keeps it whole in the label", async () => {
        const w = mountHeader({ kind: "channel", isMembersOffered: true, facts: facts({ member_count: 1000, faces: [tom, grace, jordan] }) });
        const pile = (): ReturnType<VueWrapper["get"]> => w.get("[data-testid='header-members']");
        expect(countOf(pile())).toBe("+996");

        await w.setProps({ facts: facts({ member_count: 1004, faces: [tom, grace, jordan] }) });
        expect(countOf(pile())).toBe("+1k");
        expect(pile().attributes("aria-label")).toBe("1,004 members, open member list");

        await w.setProps({ facts: facts({ is_member: false, member_count: 1250 }) });
        expect(countOf(pile())).toBe("1.2k");

        await w.setProps({ facts: facts({ is_member: false, member_count: 13536 }) });
        expect(countOf(pile())).toBe("13k");
        expect(pile().attributes("aria-label")).toBe("13,536 members, open member list");
    });

    test("is the same button for a group, its people's faces then +N, and nothing for a conversation of two", async () => {
        const people = [person(1, "Caleb", "Brooks"), person(2, "Hannah", "Diaz"), person(3, "Iris", "Moore"), person(4, "Jon", "Park")];
        const group = mountHeader({ title: "Hike planning", kind: "group", people, otherCount: 5, isMembersOffered: true });

        const members = group.get("[data-testid='header-members']");
        expect(members.classes()).toContain("chat-header-pile");
        expect(members.attributes("aria-label")).toBe("6 members, open member list");
        expect(members.findAll(".chat-av")).toHaveLength(3);
        expect(countOf(members)).toBe("+2");
        expect(hasIcon(group, members, "users")).toBe(false);
        await members.trigger("click");
        expect(group.emitted("members")).toHaveLength(1);

        // once read, the room's facts give the head count
        await group.setProps({ people: people.slice(0, 2), otherCount: null, facts: facts({ channel_type: "dm", member_count: 4 }) });
        expect(members.attributes("aria-label")).toBe("4 members, open member list");
        expect(members.findAll(".chat-av")).toHaveLength(2);
        expect(countOf(members)).toBe("+1");
        group.unmount();

        const pair = mountHeader({ title: "Anna Kim", kind: "person", people: [person(5, "Anna", "Kim")], isMembersOffered: true, facts: facts({ channel_type: "dm", member_count: 2 }) });
        expect(pair.find("[data-testid='header-members']").exists()).toBe(false);
    });

    test("says the person is reaching for it, on hover, focus or press, so the panel's rights are read before the click", async () => {
        const channel = mountHeader({ kind: "channel", isMembersOffered: true, facts: facts() });
        const pile = channel.get("[data-testid='header-members']");
        await pile.trigger("pointerenter");
        await pile.trigger("focus");
        await pile.trigger("pointerdown");
        expect(channel.emitted("membersIntent")).toHaveLength(3);
        expect(channel.emitted("members")).toBeUndefined();
        channel.unmount();

        const group = mountHeader({ title: "Hike planning", kind: "group", people: [], isMembersOffered: true, facts: facts({ channel_type: "dm", member_count: 4 }) });
        await group.get("[data-testid='header-members']").trigger("pointerenter");
        expect(group.emitted("membersIntent")).toHaveLength(1);
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

// The bell is the room's notify menu: "Notify me about" over the three modes, each with what it
// means, the chosen one checked, and the room's details last. Its icon is the room's mode. Star
// sits beside it, pressed while the room is starred. Both are offered only in a room the person
// is a member of; a choice is told to the block, which saves it.
describe("the notify menu and Star", () => {
    function menuItems(): HTMLElement[] {
        return Array.from(document.querySelectorAll<HTMLElement>("[role='menu'] [role='menuitemradio'], [role='menu'] [role='menuitem']"));
    }

    test("draws the room's mode as the bell's icon", async () => {
        const w = mountHeader({ kind: "channel", isPrefsOffered: true, notifyMode: "all" });
        const bell = (): ReturnType<VueWrapper["get"]> => w.get("[data-testid='header-notify'] button");

        expect(bell().attributes("aria-label")).toBe("Notifications");
        expect(bell().attributes("aria-haspopup")).toBe("menu");
        expect(hasIcon(w, bell(), "bell")).toBe(true);

        await w.setProps({ notifyMode: "mentions" });
        expect(hasIcon(w, bell(), "bell-at")).toBe(true);

        await w.setProps({ notifyMode: "silent" });
        expect(hasIcon(w, bell(), "bell-off")).toBe(true);
    });

    test("opens Notify me about with the three modes and what each means, the chosen one checked", async () => {
        const w = mountHeader({ kind: "channel", isPrefsOffered: true, notifyMode: "mentions" });

        await w.get("[data-testid='header-notify'] button").trigger("click");
        await settle();

        expect(document.querySelector("[role='menu'] .chat-menu-head")?.textContent?.trim()).toBe("Notify me about");
        const radios = Array.from(document.querySelectorAll<HTMLElement>("[role='menu'] [role='menuitemradio']"));
        expect(radios.map(r => r.querySelector(".chat-menu-title")?.textContent?.trim())).toEqual(["All messages", "Mentions only", "Off"]);
        expect(radios.map(r => r.querySelector(".chat-menu-desc")?.textContent?.trim())).toEqual([
            "Every new message in this channel.",
            "When someone mentions you by name or uses @all.",
            "Nothing, unless someone mentions you by name."
        ]);
        expect(radios.map(r => r.getAttribute("aria-checked"))).toEqual(["false", "true", "false"]);
        expect(radios[1].querySelector("[data-testid='menu-check']")).not.toBeNull();
        expect(radios[0].querySelector("[data-testid='menu-check']")).toBeNull();
        expect(menuItems().map(i => i.textContent?.trim()).slice(-1)).toEqual(["Channel details"]);
    });

    test("says conversation outside a channel", async () => {
        const w = mountHeader({ kind: "group", isPrefsOffered: true, notifyMode: "all" });

        await w.get("[data-testid='header-notify'] button").trigger("click");
        await settle();

        expect(document.querySelector("[role='menu'] .chat-menu-desc")?.textContent?.trim()).toBe("Every new message in this conversation.");
        expect(menuItems().map(i => i.textContent?.trim()).slice(-1)).toEqual(["Conversation details"]);
    });

    test("tells the block the mode chosen, and nothing for the one already chosen", async () => {
        const w = mountHeader({ kind: "channel", isPrefsOffered: true, notifyMode: "all" });

        await w.get("[data-testid='header-notify'] button").trigger("click");
        await settle();
        menuItems()[2].click();
        await settle();

        await w.get("[data-testid='header-notify'] button").trigger("click");
        await settle();
        menuItems()[0].click();
        await settle();

        expect(w.emitted("notify")).toEqual([["silent"]]);
    });

    test("opens the details from its last item", async () => {
        const w = mountHeader({ kind: "channel", isPrefsOffered: true, notifyMode: "all" });

        await w.get("[data-testid='header-notify'] button").trigger("click");
        await settle();
        menuItems()[3].click();
        await settle();

        expect(w.emitted("details")).toHaveLength(1);
    });

    test("Star is pressed while the room is starred, and asks to change it", async () => {
        const w = mountHeader({ kind: "channel", isPrefsOffered: true, notifyMode: "all", isStarred: false });
        const star = (): ReturnType<VueWrapper["get"]> => w.get("[data-testid='header-star']");

        expect(star().attributes("aria-pressed")).toBe("false");
        expect(star().attributes("aria-label")).toBe("Star");
        expect(hasIcon(w, star(), "star")).toBe(true);

        await w.setProps({ isStarred: true });
        expect(star().attributes("aria-pressed")).toBe("true");
        expect(star().attributes("aria-label")).toBe("Unstar");
        expect(hasIcon(w, star(), "star-f")).toBe(true);

        await star().trigger("click");
        expect(w.emitted("star")).toHaveLength(1);
    });

    test("are not offered in a room the person is not a member of", () => {
        const w = mountHeader({ kind: "channel", isPrefsOffered: false, notifyMode: "all" });

        expect(w.find("[data-testid='header-notify']").exists()).toBe(false);
        expect(w.find("[data-testid='header-star']").exists()).toBe(false);
    });

    test("sit after Mentions, the bell then Star", () => {
        const w = mountHeader({ kind: "channel", isPrefsOffered: true, notifyMode: "all", isThreadsOffered: true, isMentionsOffered: true });

        const order = w.findAll(".chat-header-actions > [data-testid]").map(b => b.attributes("data-testid"));
        expect(order).toEqual(["header-threads", "header-mentions", "header-notify", "header-star"]);
    });
});

// The header draws people's own faces: a direct message's other person, a group's mark, and
// a channel's stack of up to three of its people, leaders first, as the room's facts send them.
describe("the faces in the header", () => {
    const anna = { person_alias_guid: "anna", nick_name: "Anna", last_name: "Reyes", avatar_url: "https://photos.example/anna.jpg" };
    const ben = { person_alias_guid: "ben", nick_name: "Ben", last_name: "Okafor", avatar_url: null };

    test("is a direct message's other person's photo", () => {
        const w = mountHeader({ title: "Anna Reyes", kind: "person", people: [anna] });

        const faces = w.findAll(".chat-av");
        expect(faces).toHaveLength(1);
        expect(faces[0].get("img").attributes("src")).toBe("https://photos.example/anna.jpg");
    });

    test("is a group's diagonal pair in a person's 36px slot, photos and one initial for one with none", () => {
        const w = mountHeader({ title: "Anna and Ben", kind: "group", people: [anna, ben] });

        const mark = w.get("[data-testid='group-face']");
        expect(mark.attributes("data-shape")).toBe("pair");
        expect((mark.element as HTMLElement).style.getPropertyValue("--chat-gf-s")).toBe("36px");
        const faces = mark.findAll(".chat-av");
        expect(faces).toHaveLength(2);
        expect(faces[0].get("img").attributes("src")).toBe("https://photos.example/anna.jpg");
        expect(faces[1].find("img").exists()).toBe(false);
        expect(faces[1].text()).toBe("B");
    });

    test("is a larger group's triangle, counting everyone past the first two by the row's head count", () => {
        const cara = { ...ben, person_alias_guid: "cara", nick_name: "Cara" };
        const dee = { ...ben, person_alias_guid: "dee", nick_name: "Dee" };
        const w = mountHeader({ title: "Anna, Ben, Cara, Dee and 2 more", kind: "group", people: [anna, ben, cara, dee], otherCount: 6 });

        const mark = w.get("[data-testid='group-face']");
        expect(mark.attributes("data-shape")).toBe("trio");
        expect(mark.get("[data-testid='group-face-more']").text()).toBe("+4");
    });

    test("is a named group's people from the room's facts, which the sidebar's row does not carry", () => {
        const cara = { ...ben, person_alias_guid: "cara", nick_name: "Cara", details: null };
        const named = {
            channel_id: "c0000001-0000-4000-8000-000000000000", name: "Trail crew", icon_url: null, channel_type: "dm",
            is_member: true, can_post: true, can_post_code: null, can_post_reason: null, max_length: 5000, can_leave: true,
            description: null, group_type_name: null, campus_id: null, is_public: false, member_count: 6,
            leaders: [], faces: [], people: [{ ...anna, details: null }, { ...ben, details: null }, cara]
        };
        const before = mountHeader({ title: "Trail crew", kind: "group" });
        expect(before.get("[data-testid='group-face']").attributes("data-shape")).toBe("none");
        before.unmount();

        const w = mountHeader({ title: "Trail crew", kind: "group", facts: named });
        const mark = w.get("[data-testid='group-face']");
        expect(mark.attributes("data-shape")).toBe("trio");
        expect(mark.findAll(".chat-av")[0].get("img").attributes("src")).toBe("https://photos.example/anna.jpg");
        expect(mark.get("[data-testid='group-face-more']").text()).toBe("+3");
    });

    test("is a channel's faces, which need not be leaders, with their photos", () => {
        const facts = {
            channel_id: "c0000001-0000-4000-8000-000000000000", name: "young-adults", icon_url: null, channel_type: "shared",
            is_member: true, can_post: true, can_post_code: null, can_post_reason: null, max_length: 5000, can_leave: true,
            description: null, group_type_name: null, campus_id: null, is_public: true, member_count: 42,
            leaders: [anna], faces: [anna, ben], people: []
        };
        const w = mountHeader({ kind: "channel", isMembersOffered: true, facts });

        const faces = w.get("[data-testid='header-members']").findAll(".chat-av");
        expect(faces).toHaveLength(2);
        expect(faces[0].get("img").attributes("src")).toBe("https://photos.example/anna.jpg");
        expect(faces[1].text()).toBe("BO");
    });
});

// A platform older than this client sends a room's facts without faces; the header still draws.
describe("facts from an older platform", () => {
    test("without faces draw the people icon and the count", () => {
        const facts = {
            channel_id: "c0000001-0000-4000-8000-000000000000", name: "young-adults", icon_url: null, channel_type: "shared",
            is_member: true, can_post: true, can_post_code: null, can_post_reason: null, max_length: 5000, can_leave: true,
            description: null, group_type_name: null, campus_id: null, is_public: true, member_count: 42,
            leaders: [], people: []
        };
        const w = mountHeader({ kind: "channel", isMembersOffered: true, facts });

        const pile = w.get("[data-testid='header-members']");
        expect(pile.findAll(".chat-av")).toHaveLength(0);
        expect(pile.text()).toContain("42");
    });
});
