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
// The channel intro, mounted: what a channel is, read from its facts, centered at the top of its
// history. The mark says whether the room is public or private, the name stands alone, one quiet
// line gives its visibility, size and campus, and the description follows when the room has one.
// Nothing the header already offers is repeated: no buttons, no leaders. A room the person cannot
// post in says why and how they hear of it, and an empty room says whether it waits for them or
// for someone else.
import { mount, VueWrapper } from "@vue/test-utils";
import ChannelIntro from "../../../../../src/Communication/Chat/ChatShell/components/channelIntro.partial.obs";
import { ChannelFacts } from "../../../../../src/Communication/Chat/ChatShell/types.partial";
import Avatar from "../../../../../src/Communication/Chat/ChatShell/ui/avatar.partial.obs";
import Icon from "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs";

const downtown = { guid: "d0000001-0000-4000-8000-000000000000", name: "Downtown Campus", shortCode: "Downtown" };

/** A room's facts in the exact shape chat_get_channel answers, a public member channel by default. */
function facts(overrides: Partial<ChannelFacts> = {}): ChannelFacts {
    return {
        channel_id: "c0000001-0000-4000-8000-000000000000",
        name: "Volunteers",
        icon_url: null,
        channel_type: "shared",
        is_member: true,
        can_post: true,
        can_post_code: null,
        can_post_reason: null,
        max_length: 5000,
        can_leave: true,
        description: "Serve teams across the church",
        group_type_name: "Serving Team",
        campus_id: downtown.guid,
        is_public: true,
        member_count: 42,
        leaders: [],
        faces: [],
        people: [],
        ...overrides
    };
}

function leader(nickName: string, lastName: string, index: number): ChannelFacts["leaders"][number] {
    return { person_alias_guid: `a000000${index}-0000-4000-8000-000000000000`, nick_name: nickName, last_name: lastName, avatar_url: null };
}

let wrapper: VueWrapper | null = null;

function mountIntro(props: Record<string, unknown> = {}): VueWrapper {
    wrapper = mount(ChannelIntro, {
        props: { facts: facts(), campuses: [downtown], notifyMode: "all", isEmpty: false, isPhone: false, ...props }
    });
    return wrapper;
}

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
});

describe("the mark and the title", () => {
    test("marks a public room with the hash and names it, with no '#' before the name", () => {
        const w = mountIntro();

        const mark = w.get("[data-testid='intro-mark']");
        expect(mark.attributes("aria-hidden")).toBe("true");
        expect(w.findAllComponents(Icon).some(icon => icon.props("name") === "hash" && mark.element.contains(icon.element))).toBe(true);
        expect(w.get("[data-testid='intro-title']").text()).toBe("Volunteers");
        expect(w.get("[data-testid='channel-intro']").attributes("aria-labelledby")).toBe(w.get("[data-testid='intro-title']").attributes("id"));
    });

    test("marks a private room with the lock", () => {
        const w = mountIntro({ facts: facts({ is_public: false }) });

        const mark = w.get("[data-testid='intro-mark']");
        expect(w.findAllComponents(Icon).some(icon => icon.props("name") === "lock" && mark.element.contains(icon.element))).toBe(true);
    });

    test("shows the channel's own image as its mark when it has one", () => {
        const w = mountIntro({ facts: facts({ icon_url: "https://church.example/GetImage.ashx?guid=team" }) });

        expect(w.get("[data-testid='intro-mark'] [data-testid='channel-mark-image']").attributes("src")).toBe("https://church.example/GetImage.ashx?guid=team");
        expect(w.get("[data-testid='intro-mark']").attributes("aria-hidden")).toBe("true");
    });

    test("the title is the room's name alone, never a sentence about the start of the room", () => {
        const w = mountIntro({ facts: facts({ description: null }) });

        expect(w.text()).not.toContain("This is the start of");
    });

    test("falls back to a plain name for a room with none", () => {
        const w = mountIntro({ facts: facts({ name: "  " }) });

        expect(w.get("[data-testid='intro-title']").text()).toBe("this channel");
    });
});

describe("the quiet line", () => {
    test("reads Public, the member count and the campus by name, and nothing else", () => {
        const w = mountIntro();

        expect(w.findAll("[data-testid='intro-meta'] > span").map(part => part.text())).toEqual(["Public", "42 members", "Downtown Campus"]);
    });

    test("says Private, and leaves out a campus the room does not carry", () => {
        const w = mountIntro({ facts: facts({ is_public: false, campus_id: null }) });

        expect(w.findAll("[data-testid='intro-meta'] > span").map(part => part.text())).toEqual(["Private", "42 members"]);
    });

    test("leaves out the group type, which the details dialog carries", () => {
        const w = mountIntro();

        expect(w.get("[data-testid='intro-meta']").text()).not.toContain("Serving Team");
    });

    test("counts one member in the singular and many with separators", () => {
        const one = mountIntro({ facts: facts({ member_count: 1 }) });
        expect(one.get("[data-testid='intro-members']").text()).toBe("1 member");
        one.unmount();

        const many = mountIntro({ facts: facts({ member_count: 13536 }) });
        expect(many.get("[data-testid='intro-members']").text()).toBe(`${(13536).toLocaleString()} members`);
    });

    test("leaves out a campus the church no longer lists", () => {
        const w = mountIntro({ campuses: [] });

        expect(w.findAll("[data-testid='intro-meta'] > span").map(part => part.text())).toEqual(["Public", "42 members"]);
    });
});

describe("the description", () => {
    test("ends with a period added where it has none", () => {
        const w = mountIntro();

        expect(w.get("[data-testid='intro-description']").text()).toBe("Serve teams across the church.");
    });

    test("keeps its own end where it has one", () => {
        const w = mountIntro({ facts: facts({ description: "Who's serving this Sunday?" }) });

        expect(w.get("[data-testid='intro-description']").text()).toBe("Who's serving this Sunday?");
    });

    // The two-line cut is only visual, so a screen reader still reads every word of a long one.
    test("keeps every word of a long one in the page, however few lines it shows", () => {
        const long = `${"Service times, volunteer needs, parking updates and what is coming up this month ".repeat(6).trim()}.`;
        const w = mountIntro({ facts: facts({ description: long }) });

        expect(w.get("[data-testid='intro-description']").text()).toBe(long);
    });

    test.each([
        ["a public room", true],
        ["a private room", false]
    ])("is left out for %s with none, or only spaces", (_name, isPublic) => {
        const w = mountIntro({ facts: facts({ is_public: isPublic, description: "   " }) });

        expect(w.find("[data-testid='intro-description']").exists()).toBe(false);
    });
});

// The header carries the room's actions and its leaders, so the intro repeats none of them.
describe("what the header already says", () => {
    test("no leaders line, however many leaders the room lists", () => {
        const w = mountIntro({ facts: facts({ leaders: [leader("Tom", "Hale", 1), leader("Grace", "Kim", 2)] }) });

        expect(w.find("[data-testid='intro-leaders']").exists()).toBe(false);
        expect(w.text()).not.toContain("Led by");
        expect(w.findComponent(Avatar).exists()).toBe(false);
    });

    test("no cover behind the mark", () => {
        const w = mountIntro();

        expect(w.find("[data-testid='intro-cover']").exists()).toBe(false);
    });

    test.each([
        ["a member who can post", facts()],
        ["a member who cannot post", facts({ can_post: false, can_post_code: "authz.leaders_only", can_post_reason: "Only leaders can post." })],
        ["a person previewing the room", facts({ is_member: false, can_post: false, can_post_code: "authz.join_required", can_post_reason: "Join this channel to post." })]
    ])("no buttons for %s", (_name, roomFacts) => {
        const w = mountIntro({ facts: roomFacts });

        expect(w.find("button").exists()).toBe(false);
        expect(w.find("[data-testid='intro-actions']").exists()).toBe(false);
    });
});

describe("the posting note", () => {
    test("is left out where the person can post", () => {
        const w = mountIntro();

        expect(w.find("[data-testid='intro-note']").exists()).toBe(false);
    });

    test("is left out where joining is the only thing between the person and posting, which the join bar says", () => {
        const w = mountIntro({ facts: facts({ is_member: false, can_post: false, can_post_code: "authz.join_required", can_post_reason: "Join this channel to post." }) });

        expect(w.find("[data-testid='intro-note']").exists()).toBe(false);
    });

    test("says the server's reason, then how the person hears of every post", () => {
        const w = mountIntro({ facts: facts({ can_post: false, can_post_code: "authz.leaders_only", can_post_reason: "Only leaders can post in Announcements." }) });

        expect(w.get("[data-testid='intro-note']").text()).toBe("Only leaders can post in Announcements. You're notified of every post.");
    });

    test("says the person hears only of mentions, or nothing, by their setting for the room", () => {
        const reason = { can_post: false, can_post_code: "authz.leaders_only", can_post_reason: "Only leaders can post in Announcements." };
        const mentions = mountIntro({ facts: facts(reason), notifyMode: "mentions" });
        expect(mentions.get("[data-testid='intro-note']").text()).toBe("Only leaders can post in Announcements. You're notified only when you're mentioned.");
        mentions.unmount();

        const off = mountIntro({ facts: facts(reason), notifyMode: "silent" });
        expect(off.get("[data-testid='intro-note']").text()).toBe("Only leaders can post in Announcements. Notifications are off for this channel.");
    });
});

describe("an empty room's foot", () => {
    test("is left out once the room has messages", () => {
        const w = mountIntro();

        expect(w.find("[data-testid='intro-foot']").exists()).toBe(false);
    });

    test("asks the person to say hello where they can post", () => {
        const w = mountIntro({ isEmpty: true });

        expect(w.get("[data-testid='intro-foot']").text()).toBe("No messages yet. Be the first to say hello.");
    });

    test("asks a person previewing the room to say hello, since they need only join to post", () => {
        const w = mountIntro({ isEmpty: true, facts: facts({ is_member: false, can_post: false, can_post_code: "authz.join_required", can_post_reason: "Join this channel to post." }) });

        expect(w.get("[data-testid='intro-foot']").text()).toBe("No messages yet. Be the first to say hello.");
    });

    test("says nothing has been posted to a person previewing the room whose join was refused", () => {
        const w = mountIntro({ isEmpty: true, isJoinRefused: true, facts: facts({ is_member: false, can_post: false, can_post_code: "authz.join_required", can_post_reason: "Join this channel to post." }) });

        expect(w.get("[data-testid='intro-foot']").text()).toBe("Nothing has been posted yet.");
        // the join bar says why they cannot join, so no note repeats it
        expect(w.find("[data-testid='intro-note']").exists()).toBe(false);
    });

    test("says nothing has been posted where they cannot", () => {
        const w = mountIntro({ isEmpty: true, facts: facts({ can_post: false, can_post_code: "authz.leaders_only", can_post_reason: "Only leaders can post." }) });

        expect(w.get("[data-testid='intro-foot']").text()).toBe("Nothing has been posted yet.");
    });
});

describe("on a phone", () => {
    test("is marked as the phone's, for its tighter spacing", () => {
        const w = mountIntro({ isPhone: true });

        expect(w.get("[data-testid='channel-intro']").classes()).toContain("is-phone");
    });
});
