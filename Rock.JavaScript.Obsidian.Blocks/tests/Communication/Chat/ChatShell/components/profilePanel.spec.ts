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
// The profile panel, mounted: a person's full profile in the side panel, which the card's Details
// opens. One column: a centred head (their 88 px face, name with no pill, the status words and
// campus line), then the actions, Message, View profile to Rock's person profile page in a new tab
// only where Rock found this viewer may view it and gave its link, and a More holding Block, or
// Unblock for someone blocked; the person's own panel has Chat settings alone. Below, only when they
// show their public profile details: Rock's contact rows, reserved while Rock answers, the badges
// chat holds, and the church's template with no height cap; a note when someone keeps them
// private. Last, the channels and group conversations the two share, each opening its room.
/* eslint-disable @typescript-eslint/naming-convention */
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import ProfilePanel from "../../../../../src/Communication/Chat/ChatShell/components/profilePanel.partial.obs";
import { ChatBadge, PersonProfile, SharedConversation } from "../../../../../src/Communication/Chat/ChatShell/types.partial";
import { ProfileRockState } from "../../../../../src/Communication/Chat/ChatShell/composables/useRooms.partial";
import { join } from "path";
import { anna as annaAlias, badgesIn, createStatusStore, provideStatus, shellFolder, styleRules } from "../statusStore";

// #region Fixtures

const downtown = { guid: "d0000001-0000-4000-8000-000000000000", name: "Downtown Campus", shortCode: "Downtown" };

/** A person in the exact shape chat_get_person answers: Anna, who shows her details, in Downtown. */
function profile(overrides: Partial<PersonProfile> = {}): PersonProfile {
    return {
        person_alias_guid: "a0000002-0000-4000-8000-000000000000",
        nick_name: "Anna",
        last_name: "Reyes",
        avatar_url: null,
        campus_id: downtown.guid,
        is_self: false,
        is_leader: false,
        details: { badge_keys: ["member", "baptized"] },
        can_message: true,
        dm_channel_id: null,
        ...overrides
    };
}

/** Rock's answer for Anna, who shows her details. */
function answered(extra: Record<string, unknown> = {}): ProfileRockState {
    return {
        kind: "answered",
        details: { isShown: true, email: "anna.reyes@gmail.com", mobilePhone: "(602) 555-0142", mobilePhoneUri: "tel:+16025550142", lavaHtml: null, ...extra }
    };
}

/** A conversation the two share, as the profile lists it. */
function shared(name: string, kind: "channel" | "group" = "channel"): SharedConversation {
    return { channel_id: `c-${name}`, name, kind, icon_url: null };
}

// #endregion Fixtures

// #region Helpers

let wrapper: VueWrapper | null = null;

function mountPanel(person: PersonProfile = profile(), props: Record<string, unknown> = {}): VueWrapper {
    wrapper = mount(ProfilePanel, {
        props: {
            person,
            campuses: [downtown],
            ...props
        },
        attachTo: document.body
    });
    return wrapper;
}

/** Aliased for the cells moved here from the card, which mount a person's profile the same way. */
const mountCard = mountPanel;

async function settle(): Promise<void> {
    await nextTick();
    await nextTick();
}

/** The test ids of the action row's controls, in order. */
function actionIds(w: VueWrapper): string[] {
    return w.findAll("[data-testid='profile-actions'] :is(button, a)").map(control => control.attributes("data-testid") ?? "");
}

/** Opens the panel's More and answers its items' keys, in order. */
async function openMore(w: VueWrapper): Promise<string[]> {
    await w.get("[data-testid='profile-more']").trigger("click");
    await settle();
    return Array.from(document.querySelectorAll<HTMLElement>("[role='menu'] [data-key]")).map(item => item.dataset.key ?? "");
}

// #endregion Helpers

beforeAll(() => {
    // jsdom has no ResizeObserver, and a menu keeps its place beside its button with one.
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

describe("the panel", () => {
    test("is the side panel titled Profile, with a back chevron only when it was opened from somewhere", async () => {
        const w = mountPanel();
        expect(w.get("[data-testid='panel-title']").text()).toBe("Profile");
        expect(w.find("[data-testid='panel-back']").exists()).toBe(false);
        w.unmount();

        const back = mountPanel(profile(), { backLabel: "Back to Members" });
        await back.get("[data-testid='panel-back']").trigger("click");
        expect(back.emitted("back")).toHaveLength(1);
    });

    test("heads the column with the 88 px face, the full name with no pill, and the status and campus line", () => {
        const status = createStatusStore();
        const unprovide = provideStatus(status);
        try {
            const w = mountPanel(profile({ person_alias_guid: annaAlias, is_leader: true }));

            const head = w.get("[data-testid='profile-identity']");
            expect(head.get(".chat-av").attributes("style")).toContain("width: 88px");
            expect(head.get("[data-testid='profile-name']").text()).toBe("Anna Reyes");
            expect(head.find("[data-testid='profile-leader']").exists()).toBe(false);
            expect(badgesIn(head.element)).toEqual([{ state: "online", isMoving: false }]);
            expect(head.get("[data-testid='profile-line']").text().replace(/\s+/g, " ").trim()).toBe("Online, Downtown Campus");
        }
        finally {
            unprovide();
        }
    });

    test("puts the details after the actions", () => {
        const w = mountPanel(profile(), { rock: answered() });

        const actions = w.get("[data-testid='profile-actions']").element;
        const details = w.get("[data-testid='profile-details']").element;
        expect(actions.compareDocumentPosition(details) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy();
    });

    test("draws no badges without the church's list", () => {
        const own = mountPanel(profile({ is_self: true }));

        expect(own.find("[data-testid='profile-badges']").exists()).toBe(false);
        own.unmount();

        const other = mountPanel();
        expect(other.find("[data-testid='profile-badges']").exists()).toBe(false);
        expect(other.text()).not.toContain("Member");
    });

    test("has no switch for the person's own details: Chat settings holds it", () => {
        const w = mountPanel(profile({ is_self: true, can_message: false }));

        expect(w.find("[data-testid='switch-profile_details']").exists()).toBe(false);
    });
});

describe("the actions", () => {
    test("are Message, View profile and More where Rock let this viewer view the person's page", () => {
        const w = mountPanel(profile(), { mayViewProfile: true, rock: answered({ personProfileUrl: "/Person/12" }) });

        expect(actionIds(w)).toEqual(["profile-message", "profile-view", "profile-more"]);
    });

    test("View profile opens Rock's person profile page in a new tab", () => {
        const w = mountPanel(profile(), { mayViewProfile: true, rock: answered({ personProfileUrl: "/Person/12" }) });

        const view = w.get("[data-testid='profile-view']");
        expect(view.element.tagName).toBe("A");
        expect(view.text()).toBe("View profile");
        expect(view.attributes("href")).toBe("/Person/12");
        expect(view.attributes("target")).toBe("_blank");
        expect(view.attributes("rel")).toContain("noopener");
    });

    test.each([
        ["Rock found the viewer may not view the page", false, "/Person/12"],
        ["Rock gave no link", true, null],
        ["neither", false, null]
    ])("leave out View profile where %s", (_case, mayViewProfile, personProfileUrl) => {
        const w = mountPanel(profile(), { mayViewProfile, rock: answered({ personProfileUrl }) });

        expect(w.find("[data-testid='profile-view']").exists()).toBe(false);
        expect(actionIds(w)).toEqual(["profile-message", "profile-more"]);
    });

    test("Message asks for the person", async () => {
        const anna = profile();
        const w = mountPanel(anna);

        await w.get("[data-testid='profile-message']").trigger("click");

        expect(w.emitted("message")).toEqual([[anna]]);
    });

    test("More holds Block, which asks for the confirmation", async () => {
        const w = mountPanel();

        expect(await openMore(w)).toEqual(["block"]);
        document.querySelector<HTMLElement>("[data-key='block']")!.click();
        await settle();

        expect(w.emitted("block")?.[0]?.[0]).toMatchObject({ person_alias_guid: "a0000002-0000-4000-8000-000000000000" });
    });

    test("for someone blocked say so in words, with a More that holds Unblock", async () => {
        const w = mountPanel(profile({ is_blocked: true, can_message: false }));

        expect(w.get("[data-testid='profile-blocked']").text()).toContain("You blocked Anna.");
        expect(actionIds(w)).toEqual(["profile-more"]);
        expect(await openMore(w)).toEqual(["unblock"]);
        document.querySelector<HTMLElement>("[data-key='unblock']")!.click();
        await settle();

        expect(w.emitted("unblock")).toHaveLength(1);
    });

    test("on the person's own panel are Chat settings alone", async () => {
        const w = mountPanel(profile({ is_self: true, can_message: false }), { mayViewProfile: true, rock: answered({ personProfileUrl: "/Person/1" }) });

        expect(actionIds(w)).toEqual(["profile-settings"]);
        await w.get("[data-testid='profile-settings']").trigger("click");
        expect(w.emitted("settings")).toHaveLength(1);
    });

    test("for someone who cannot be messaged say so in words in Message's place", () => {
        const w = mountPanel(profile({ nick_name: "Luis", can_message: false }));

        expect(w.find("[data-testid='profile-message']").exists()).toBe(false);
        expect(w.get("[data-testid='profile-cannot-message']").text()).toContain("Direct messages aren't available with Luis.");
        expect(actionIds(w)).toEqual(["profile-more"]);
    });
});

describe("shared conversations", () => {
    test("lists the channels and group conversations the two share, each opening its room", async () => {
        const w = mountPanel(profile({ shared_conversations: [shared("Young adults"), shared("Hike planning", "group")], shared_conversations_total: 2 }));

        const section = w.get("[data-testid='profile-shared']");
        expect(section.get("h3").text()).toBe("Shared conversations");
        const rows = section.findAll("[data-testid='profile-shared-row']");
        expect(rows.map(row => row.text())).toEqual(["Young adults", "Hike planning"]);
        await rows[1].trigger("click");

        expect(w.emitted("openRoom")).toEqual([["c-Hike planning"]]);
        expect(section.find("[data-testid='profile-shared-more']").exists()).toBe(false);
    });

    test("says how many more in words past the ones listed", () => {
        const w = mountPanel(profile({ shared_conversations: [shared("Young adults")], shared_conversations_total: 24 }));

        expect(w.get("[data-testid='profile-shared-more']").text()).toBe("and 23 more");
    });

    test("is left out where they share none, or the platform has not said", () => {
        const none = mountPanel(profile({ shared_conversations: [], shared_conversations_total: 0 }));
        expect(none.find("[data-testid='profile-shared']").exists()).toBe(false);
        none.unmount();

        const unsaid = mountPanel(profile());
        expect(unsaid.find("[data-testid='profile-shared']").exists()).toBe(false);
    });

    test("is left out of the person's own panel", () => {
        const w = mountPanel(profile({ is_self: true, shared_conversations: [shared("Young adults")], shared_conversations_total: 1 }));

        expect(w.find("[data-testid='profile-shared']").exists()).toBe(false);
    });
});

describe("the private note", () => {
    test("says a person who keeps their details private does, by their first name, with no details block", () => {
        const w = mountCard(profile({ details: null }), { rock: { kind: "answered", details: { isShown: false } } });

        expect(w.get("[data-testid='profile-note']").text()).toBe("Anna keeps profile details private.");
        expect(w.find("[data-testid='profile-details']").exists()).toBe(false);
    });

    test("is left out for someone who shows their details", () => {
        const w = mountCard(profile(), { rock: answered() });

        expect(w.find("[data-testid='profile-note']").exists()).toBe(false);
    });

    test("is left out of the person's own panel, whose setting is theirs in Chat settings", () => {
        const w = mountCard(profile({ is_self: true, details: null }));

        expect(w.find("[data-testid='profile-note']").exists()).toBe(false);
    });
});

// Rock's half: the contact rows and the church's template, read as the profile opens.

describe("Rock's details", () => {
    test("draw the email as a mail link and the mobile phone as a call link", () => {
        const w = mountCard(profile(), { rock: answered() });

        const email = w.get("[data-testid='profile-email']");
        expect(email.element.tagName).toBe("A");
        expect(email.attributes("href")).toBe("mailto:anna.reyes@gmail.com");
        expect(email.text()).toBe("anna.reyes@gmail.com");

        const phone = w.get("[data-testid='profile-phone']");
        expect(phone.attributes("href")).toBe("tel:+16025550142");
        expect(phone.text()).toBe("(602) 555-0142");
    });

    test("leave out a row Rock has nothing for, and the whole contact list when it has neither", () => {
        const noPhone = mountCard(profile(), { rock: answered({ mobilePhone: null, mobilePhoneUri: null }) });
        expect(noPhone.find("[data-testid='profile-phone']").exists()).toBe(false);
        expect(noPhone.find("[data-testid='profile-email']").exists()).toBe(true);
        noPhone.unmount();

        const neither = mountCard(profile(), { rock: answered({ email: null, mobilePhone: null, mobilePhoneUri: null }) });
        expect(neither.find("[data-testid='profile-contact']").exists()).toBe(false);
    });

    test("an email address is never made into a link Rock did not give", () => {
        const w = mountCard(profile(), { rock: answered({ email: "a@b.org?bcc=x@y.org" }) });

        expect(w.get("[data-testid='profile-email']").attributes("href")).toBe("mailto:a@b.org%3Fbcc%3Dx@y.org");
    });

    test("are reserved while Rock answers: two contact rows, and the template's lines only when the church has one", () => {
        const plain = mountCard(profile(), { rock: { kind: "loading" } });
        expect(plain.get("[data-testid='profile-details']").attributes("aria-busy")).toBe("true");
        expect(plain.findAll("[data-testid='profile-contact-skeleton']")).toHaveLength(2);
        expect(plain.find("[data-testid='profile-lava-skeleton']").exists()).toBe(false);
        plain.unmount();

        const templated = mountCard(profile(), { rock: { kind: "loading" }, isLavaConfigured: true });
        expect(templated.find("[data-testid='profile-lava-skeleton']").exists()).toBe(true);
    });

    test("reserve nothing for someone keeping details private", () => {
        const w = mountCard(profile({ details: null }), { rock: { kind: "loading" }, isLavaConfigured: true });

        expect(w.find("[data-testid='profile-contact-skeleton']").exists()).toBe(false);
        expect(w.find("[data-testid='profile-lava-skeleton']").exists()).toBe(false);
    });

    test("Rock answering before the panel is drawn draws it once, whole, with nothing reserved", () => {
        const w = mountCard(profile(), { rock: answered({ lavaHtml: "<p>Serving in Kids</p>" }), isLavaConfigured: true });

        expect(w.find("[data-testid='profile-contact-skeleton']").exists()).toBe(false);
        expect(w.find("[data-testid='profile-lava-skeleton']").exists()).toBe(false);
        expect(w.find(".is-arriving").exists()).toBe(false);
        expect(w.get("[data-testid='profile-details']").attributes("aria-busy")).toBeUndefined();
    });

    test("Rock answering after the panel is drawn fills the reserved rows in place", async () => {
        const w = mountCard(profile(), { rock: { kind: "loading" }, isLavaConfigured: true });

        await w.setProps({ rock: answered({ lavaHtml: "<p>Serving in Kids</p>" }) });

        expect(w.find("[data-testid='profile-contact-skeleton']").exists()).toBe(false);
        expect(w.get("[data-testid='profile-email']").classes()).toContain("is-arriving");
        expect(w.get("[data-testid='profile-lava']").classes()).toContain("is-arriving");
    });

    test("Rock failing collapses its rows and keeps the badges chat holds", () => {
        const badges = [{ badge_key: "member", name: "Member", icon_css: null, bg_color: null, fg_color: null, sort_order: 1 }];
        const w = mountCard(profile(), { rock: { kind: "failed" }, isLavaConfigured: true, badges });

        expect(w.find("[data-testid='profile-contact']").exists()).toBe(false);
        expect(w.find("[data-testid='profile-contact-skeleton']").exists()).toBe(false);
        expect(w.find("[data-testid='profile-lava']").exists()).toBe(false);
        expect(w.findAll("[data-testid='profile-badge']").map(b => b.text())).toEqual(["Member"]);
    });

    test("Rock saying the person is not shown, while chat still says they are, collapses its rows", () => {
        const badges = [{ badge_key: "member", name: "Member", icon_css: null, bg_color: null, fg_color: null, sort_order: 1 }];
        const w = mountCard(profile(), { rock: { kind: "answered", details: { isShown: false } }, badges });

        expect(w.find("[data-testid='profile-contact']").exists()).toBe(false);
        expect(w.findAll("[data-testid='profile-badge']")).toHaveLength(1);
    });
});


describe("the church's template", () => {
    test("is drawn from Rock's safe output whole, with no height cap, in a region named for the person", () => {
        const w = mountCard(profile(), { rock: answered({ lavaHtml: "<div class=\"lh\">Serving in</div><ul><li><b>Kids</b></li></ul>" }) });

        const lava = w.get("[data-testid='profile-lava']");
        expect(lava.find("ul li b").text()).toBe("Kids");
        expect(lava.attributes("aria-label")).toBe("More about Anna");
        expect(lava.attributes("role")).toBe("region");
        const rule = styleRules(join(shellFolder, "components/profilePanel.partial.obs"), /^\.chat-pp-lava$/).join(";");
        expect(rule).not.toMatch(/max-height/);
        expect(rule).not.toMatch(/overflow-y/);
    });

    test("is left out when Rock rendered nothing", () => {
        const w = mountCard(profile(), { rock: answered({ lavaHtml: null }), isLavaConfigured: true });

        expect(w.find("[data-testid='profile-lava']").exists()).toBe(false);
    });
});


describe("badges", () => {
    /** One row of the bootstrap's badges, exactly its keys. */
    function badge(key: string, name: string, sortOrder: number, extra: Partial<ChatBadge> = {}): ChatBadge {
        return { badge_key: key, name, icon_css: null, bg_color: null, fg_color: null, sort_order: sortOrder, ...extra };
    }

    const member = badge("member", "Member", 2, { icon_css: "fa fa-home", bg_color: "#fdf1d8", fg_color: "#6b4a07" });
    const baptized = badge("baptized", "Baptized", 1, { icon_css: "fa fa-tint", bg_color: "#dceefb", fg_color: "#0b4f7a" });
    const serving = badge("serving", "Serving", 3);

    /** The pills' names, in the order drawn. */
    function pillNames(w: VueWrapper): string[] {
        return w.findAll("[data-testid='profile-badge']").map(pill => pill.text().trim());
    }

    test("lists the person's badges under a rule in the church's order", () => {
        const w = mountCard(profile({ details: { badge_keys: ["member", "serving", "baptized"] } }), { badges: [member, baptized, serving] });

        const list = w.get("[data-testid='profile-badges']");
        expect(list.element.tagName).toBe("UL");
        expect(list.attributes("aria-label")).toBe("Badges");
        expect(pillNames(w)).toEqual(["Baptized", "Member", "Serving"]);
    });

    test("drops a key the church's list does not know, and draws no list when none is left", async () => {
        const w = mountCard(profile({ details: { badge_keys: ["retired", "member"] } }), { badges: [member] });
        expect(pillNames(w)).toEqual(["Member"]);

        await w.setProps({ person: profile({ details: { badge_keys: ["retired"] } }) });
        expect(w.find("[data-testid='profile-badges']").exists()).toBe(false);
    });

    test("draws none for someone who keeps their details private", () => {
        const w = mountCard(profile({ details: null }), { badges: [member, baptized] });

        expect(w.find("[data-testid='profile-badges']").exists()).toBe(false);
    });

    test("tints each pill by its badge's colors, and leaves a badge with none plain", () => {
        const w = mountCard(profile({ details: { badge_keys: ["member", "serving"] } }), { badges: [member, serving] });

        const [tinted, plain] = w.findAll("[data-testid='profile-badge']").map(pill => pill.element as HTMLElement);
        expect(tinted.style.getPropertyValue("--chat-badge-bg")).toBe("#fdf1d8");
        expect(tinted.style.getPropertyValue("--chat-badge-fg")).toBe("#6b4a07");
        expect(tinted.classList.contains("is-toned")).toBe(true);
        expect(plain.style.getPropertyValue("--chat-badge-bg")).toBe("");
        expect(plain.style.getPropertyValue("--chat-badge-fg")).toBe("");
        expect(plain.classList.contains("is-toned")).toBe(false);
    });

    test("draws the badge's own icon class, and no icon where it has none", () => {
        const w = mountCard(profile({ details: { badge_keys: ["member", "serving"] } }), { badges: [member, { ...serving, icon_css: "  " }] });

        const [withIcon, without] = w.findAll("[data-testid='profile-badge']");
        const icon = withIcon.get("i");
        expect(icon.classes()).toEqual(expect.arrayContaining(["fa", "fa-home"]));
        expect(icon.attributes("aria-hidden")).toBe("true");
        expect(without.find("i").exists()).toBe(false);
    });

    test("never draws a badge or a pill for leading the room", () => {
        const w = mountCard(profile({ is_leader: true, details: { badge_keys: [] } }), { badges: [member] });

        expect(w.find("[data-testid='profile-badges']").exists()).toBe(false);
        expect(w.find("[data-testid='profile-leader']").exists()).toBe(false);
    });

    test("take the badge corner, square-ish rather than a full pill", () => {
        const rule = styleRules(join(shellFolder, "components/profilePanel.partial.obs"), /^\.chat-pp-badges li$/).join(";");

        expect(rule).toMatch(/border-radius:\s*var\(--chat-r-badge\)/);
    });
});
