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
// The profile card, mounted: the person a name, a face or a chip names, as chat_get_person answers
// them. Their face, name, the Leader pill in the room it was opened from or "You" on the person's
// own card, and their campus by its name from the init box, whatever their setting. Message for
// someone the person may message, a quiet box for someone they may not. Below the action, only when
// they show their public profile details, a block under a rule: Rock's contact rows, the badges
// chat holds, and the church's template, with Rock's rows reserved while Rock answers; a note when
// someone keeps them private. The person's own card has the switch for them and Chat settings.
// The card is a dialog named for the person, takes focus as it opens, and Escape gives focus back
// to what opened it.
/* eslint-disable @typescript-eslint/naming-convention */
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import ProfileCard from "../../../../../src/Communication/Chat/ChatShell/components/profileCard.partial.obs";
import { ChatBadge, PersonProfile } from "../../../../../src/Communication/Chat/ChatShell/types.partial";
import { ProfileRockState } from "../../../../../src/Communication/Chat/ChatShell/composables/useRooms.partial";

// #region Fixtures

const downtown = { guid: "d0000001-0000-4000-8000-000000000000", name: "Downtown Campus", shortCode: "Downtown" };
const north = { guid: "d0000002-0000-4000-8000-000000000000", name: "North Campus", shortCode: null };

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

// #endregion Fixtures

// #region Helpers

let wrapper: VueWrapper | null = null;

function mountCard(person: PersonProfile = profile(), props: Record<string, unknown> = {}): VueWrapper {
    wrapper = mount(ProfileCard, {
        props: {
            person,
            campuses: [downtown, north],
            ...props
        },
        attachTo: document.body
    });
    return wrapper;
}

/** The card's own element, the dialog. */
function card(w: VueWrapper): HTMLElement {
    return w.get("[data-testid='profile-card']").element as HTMLElement;
}

async function settle(): Promise<void> {
    await nextTick();
    await nextTick();
}

// #endregion Helpers

beforeAll(() => {
    // jsdom has no ResizeObserver, and a popover keeps its place beside its opener with one.
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

describe("the card", () => {
    test("is a dialog named for the person, by their full name", () => {
        const w = mountCard();

        expect(card(w).getAttribute("role")).toBe("dialog");
        expect(card(w).getAttribute("aria-label")).toBe("Profile, Anna Reyes");
        expect(w.get("[data-testid='profile-name']").text()).toBe("Anna Reyes");
    });

    test("takes focus as it opens", async () => {
        const opener = document.createElement("button");
        document.body.appendChild(opener);
        opener.focus();

        const w = mountCard();
        await settle();

        expect(document.activeElement).toBe(card(w));
    });

    test("closes on Escape and gives focus back to what opened it", async () => {
        const opener = document.createElement("button");
        document.body.appendChild(opener);
        opener.focus();
        const w = mountCard();
        await settle();

        card(w).dispatchEvent(new KeyboardEvent("keydown", { key: "Escape", bubbles: true }));
        await settle();

        expect(w.emitted("close")).toHaveLength(1);
        expect(document.activeElement).toBe(opener);
    });

    test("closes when Tab takes focus out of it, leaving focus where Tab put it", async () => {
        const before = document.createElement("button");
        const after = document.createElement("button");
        document.body.append(before, after);
        before.focus();
        const w = mountCard();
        await settle();
        const message = w.get("[data-testid='profile-message']").element as HTMLElement;
        message.focus();

        // as a browser moves focus on Tab past the card's last control
        message.dispatchEvent(new FocusEvent("focusout", { bubbles: true, relatedTarget: after }));
        after.focus();
        await settle();

        expect(w.emitted("close")).toHaveLength(1);
        expect(document.activeElement).toBe(after);
    });

    test("stays open as focus moves between its own parts", async () => {
        const w = mountCard();
        await settle();
        const message = w.get("[data-testid='profile-message']").element as HTMLElement;

        card(w).dispatchEvent(new FocusEvent("focusout", { bubbles: true, relatedTarget: message }));
        await settle();

        expect(w.emitted("close")).toBeUndefined();
    });

    test("closes on Escape wherever focus is, giving focus back to what opened it", async () => {
        const opener = document.createElement("button");
        document.body.appendChild(opener);
        opener.focus();
        const w = mountCard();
        await settle();

        // a key that reaches the page outside the card, as one with focus nowhere does
        document.body.dispatchEvent(new KeyboardEvent("keydown", { key: "Escape", bubbles: true }));
        await settle();

        expect(w.emitted("close")).toHaveLength(1);
        expect(document.activeElement).toBe(opener);
    });

    test("marks what opened it as expanded while it is open, so a member row keeps its actions beside it", async () => {
        const row = document.createElement("li");
        const opener = document.createElement("button");
        row.appendChild(opener);
        document.body.appendChild(row);
        const w = mountCard(profile(), { anchor: opener });
        await settle();

        expect(opener.getAttribute("aria-expanded")).toBe("true");

        card(w).dispatchEvent(new KeyboardEvent("keydown", { key: "Escape", bubbles: true }));
        await settle();
        expect(opener.hasAttribute("aria-expanded")).toBe(false);
    });

    test("closes on a press outside it", async () => {
        const outside = document.createElement("div");
        document.body.appendChild(outside);
        const w = mountCard();
        await settle();

        outside.dispatchEvent(new MouseEvent("mousedown", { bubbles: true }));
        await settle();

        expect(w.emitted("close")).toHaveLength(1);
    });

    test("draws no badges without the church's list", () => {
        const own = mountCard(profile({ is_self: true }));

        expect(own.find("[data-testid='profile-badges']").exists()).toBe(false);
        own.unmount();

        const other = mountCard();
        expect(other.find("[data-testid='profile-badges']").exists()).toBe(false);
        expect(other.text()).not.toContain("Member");
    });
});

describe("the name line", () => {
    test("carries the Leader pill for a leader of the room the card was opened from", () => {
        const w = mountCard(profile({ is_leader: true }));

        expect(w.get("[data-testid='profile-leader']").text()).toBe("Leader");
    });

    test("carries no pill for anyone else", () => {
        const w = mountCard();

        expect(w.find("[data-testid='profile-leader']").exists()).toBe(false);
        expect(w.find("[data-testid='profile-you']").exists()).toBe(false);
    });

    test("says You on the person's own card", () => {
        const w = mountCard(profile({ is_self: true }));

        expect(w.get("[data-testid='profile-you']").text()).toBe("You");
    });
});

describe("the campus line", () => {
    test("names the campus from the init box's campuses, whatever the person's setting", () => {
        const shown = mountCard();
        expect(shown.get("[data-testid='profile-campus']").text()).toBe("Downtown Campus");
        shown.unmount();

        const kept = mountCard(profile({ details: null }));
        expect(kept.get("[data-testid='profile-campus']").text()).toBe("Downtown Campus");
    });

    test("matches the campus in any letter case", () => {
        const w = mountCard(profile({ campus_id: downtown.guid.toUpperCase() }));

        expect(w.get("[data-testid='profile-campus']").text()).toBe("Downtown Campus");
    });

    test("is left out for a person with no campus, or one the church no longer lists", () => {
        const none = mountCard(profile({ campus_id: null }));
        expect(none.find("[data-testid='profile-campus']").exists()).toBe(false);
        none.unmount();

        const gone = mountCard(profile({ campus_id: "d0000009-0000-4000-8000-000000000000" }));
        expect(gone.find("[data-testid='profile-campus']").exists()).toBe(false);
    });
});

describe("the look", () => {
    test("has no band: the face sits in a row with the name and campus", () => {
        const w = mountCard();

        expect(w.find(".chat-pc-band").exists()).toBe(false);
        expect(card(w).style.getPropertyValue("--chat-av-hue")).toBe("");
        const identity = w.get("[data-testid='profile-identity']");
        expect(identity.find("[data-testid='profile-name']").exists()).toBe(true);
        expect(identity.find("[data-testid='profile-campus']").exists()).toBe(true);
    });

    test("puts the details after the action, so Rock's answer only lengthens the card's foot", () => {
        const w = mountCard(profile(), { rock: answered() });

        const actions = w.get("[data-testid='profile-actions']").element;
        const details = w.get("[data-testid='profile-details']").element;
        expect(actions.compareDocumentPosition(details) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy();
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

    test("is left out of the person's own card, whose switch says it instead", () => {
        const w = mountCard(profile({ is_self: true, details: null }), { showsOwnDetails: false });

        expect(w.find("[data-testid='profile-note']").exists()).toBe(false);
    });
});

// Rock's half: the contact rows and the church's template, read as the card opens.
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

    test("Rock answering before the card is drawn draws it once, whole, with nothing reserved", () => {
        const w = mountCard(profile(), { rock: answered({ lavaHtml: "<p>Serving in Kids</p>" }), isLavaConfigured: true });

        expect(w.find("[data-testid='profile-contact-skeleton']").exists()).toBe(false);
        expect(w.find("[data-testid='profile-lava-skeleton']").exists()).toBe(false);
        expect(w.find(".is-arriving").exists()).toBe(false);
        expect(w.get("[data-testid='profile-details']").attributes("aria-busy")).toBeUndefined();
    });

    test("Rock answering after the card is drawn fills the reserved rows in place", async () => {
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
    test("is drawn from Rock's safe output in a box capped in height that scrolls and can be reached by keyboard", () => {
        const w = mountCard(profile(), { rock: answered({ lavaHtml: "<div class=\"lh\">Serving in</div><ul><li><b>Kids</b></li></ul>" }) });

        const lava = w.get("[data-testid='profile-lava']");
        expect(lava.find("ul li b").text()).toBe("Kids");
        expect(lava.attributes("tabindex")).toBe("0");
        expect(lava.attributes("aria-label")).toBe("More about Anna");
        expect(lava.attributes("role")).toBe("region");
    });

    test("is left out when Rock rendered nothing", () => {
        const w = mountCard(profile(), { rock: answered({ lavaHtml: null }), isLavaConfigured: true });

        expect(w.find("[data-testid='profile-lava']").exists()).toBe(false);
    });
});

describe("the person's own switch", () => {
    test("is on their own card, by its name, saying what others see either way", () => {
        const on = mountCard(profile({ is_self: true, can_message: false }), { showsOwnDetails: true, rock: answered() });
        const onRow = on.get("[data-testid='switch-profile_details']");
        expect(onRow.text()).toContain("Show public profile details");
        expect(onRow.text()).toContain("Others see what's below");
        expect(onRow.get("[role='switch']").attributes("aria-checked")).toBe("true");
        expect(on.find("[data-testid='profile-details']").exists()).toBe(true);
        on.unmount();

        const off = mountCard(profile({ is_self: true, can_message: false, details: null }), { showsOwnDetails: false });
        const offRow = off.get("[data-testid='switch-profile_details']");
        expect(offRow.text()).toContain("Name, photo and campus only");
        expect(offRow.get("[role='switch']").attributes("aria-checked")).toBe("false");
        expect(off.find("[data-testid='profile-details']").exists()).toBe(false);
    });

    test("asks for the change, and the details follow the person's own setting at once", async () => {
        const w = mountCard(profile({ is_self: true, can_message: false, details: null }), { showsOwnDetails: false });

        await w.get("[data-testid='switch-profile_details'] [role='switch']").trigger("click");
        expect(w.emitted("toggleDetails")).toEqual([[true]]);

        await w.setProps({ showsOwnDetails: true, rock: { kind: "loading" } });
        expect(w.findAll("[data-testid='profile-contact-skeleton']")).toHaveLength(2);
    });

    test("shows a failed save's sentence under it", () => {
        const w = mountCard(profile({ is_self: true, can_message: false }), { showsOwnDetails: true, ownDetailsNote: { kind: "error", text: "Couldn't save. Try again in a moment.", retry: () => undefined } });

        expect(w.get("[data-testid='switch-profile_details']").text()).toContain("Couldn't save. Try again in a moment.");
    });

    test("is not on anyone else's card", () => {
        const w = mountCard();

        expect(w.find("[data-testid='switch-profile_details']").exists()).toBe(false);
    });
});

describe("Message", () => {
    test("is offered for someone the person may message, and asks for that person", async () => {
        const anna = profile({ dm_channel_id: "c0000005-0000-4000-8000-000000000000" });
        const w = mountCard(anna);

        const message = w.get("[data-testid='profile-message']");
        expect(message.element.tagName).toBe("BUTTON");
        expect(message.text()).toBe("Message");
        await message.trigger("click");

        expect(w.emitted("message")).toEqual([[anna]]);
        expect(w.find("[data-testid='profile-cannot-message']").exists()).toBe(false);
    });

    test("gives way to a quiet box for someone who cannot be messaged, naming them by first name", () => {
        const w = mountCard(profile({ nick_name: "Luis", last_name: "Romero", can_message: false }));

        expect(w.find("[data-testid='profile-message']").exists()).toBe(false);
        expect(w.get("[data-testid='profile-cannot-message']").text().replace(/\s+/g, " ").trim())
            .toBe("Direct messages aren't available with Luis. You can still mention them in a channel you share.");
    });

    test("is left out of the person's own card, with no box in its place", () => {
        const w = mountCard(profile({ is_self: true, can_message: false }));

        expect(w.find("[data-testid='profile-message']").exists()).toBe(false);
        expect(w.find("[data-testid='profile-cannot-message']").exists()).toBe(false);
    });
});

// The church's badges as the bootstrap lists them; a person's details name theirs by key.
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

    test("never draws a badge for leading the room", () => {
        const w = mountCard(profile({ is_leader: true, details: { badge_keys: [] } }), { badges: [member] });

        expect(w.find("[data-testid='profile-badges']").exists()).toBe(false);
        expect(w.get("[data-testid='profile-leader']").text()).toBe("Leader");
    });
});

describe("Chat settings", () => {
    test("is offered on the person's own card, and asks for the settings", async () => {
        const w = mountCard(profile({ is_self: true, can_message: false }));

        const settings = w.get("[data-testid='profile-settings']");
        expect(settings.element.tagName).toBe("BUTTON");
        expect(settings.text()).toBe("Chat settings");
        await settings.trigger("click");

        expect(w.emitted("settings")).toHaveLength(1);
    });

    test("is not offered on anyone else's card", () => {
        const w = mountCard();

        expect(w.find("[data-testid='profile-settings']").exists()).toBe(false);
    });
});
