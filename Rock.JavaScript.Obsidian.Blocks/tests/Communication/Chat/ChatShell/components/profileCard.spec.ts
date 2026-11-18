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
// them. A glance that leaves the person's message readable: their face, name with no pill, the
// status words and campus line, and one row of actions, Message and Details at equal widths and a
// square More holding Block, or Unblock for someone blocked. The person's own card offers Chat
// settings in Message's place and no More. Their contact, badges and the church's template are
// the profile panel's, which Details opens. Opened from a message, it stands 8 px under the whole
// row with its left edge on the face, and above the row only where there is no room below. The
// card is a dialog named for the person, takes focus as it opens, and Escape gives focus back to
// what opened it.
/* eslint-disable @typescript-eslint/naming-convention */
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import ProfileCard from "../../../../../src/Communication/Chat/ChatShell/components/profileCard.partial.obs";
import { PersonProfile } from "../../../../../src/Communication/Chat/ChatShell/types.partial";
import { join } from "path";
import { anna, badgesIn, carl, createStatusStore, provideStatus, setsBothRings, shellFolder, StatusStore, styleRules, swaps, viewer } from "../statusStore";

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

/** The test ids of the action row's controls, in order: its buttons and the More menu's button. */
function actionIds(w: VueWrapper): string[] {
    return w.findAll("[data-testid='profile-actions'] button").map(button => button.attributes("data-testid") ?? "");
}

/** Opens the card's More and answers its items' keys, in order. */
async function openMore(w: VueWrapper): Promise<string[]> {
    await w.get("[data-testid='profile-more']").trigger("click");
    await settle();
    return Array.from(document.querySelectorAll<HTMLElement>("[role='menu'] [data-key]")).map(item => item.dataset.key ?? "");
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

    test("draws the person's photo through the image reveal, hidden until decoded", () => {
        const w = mountCard(profile({ avatar_url: "https://photos.example/anna.jpg" }));
        const photo = w.get("img.chat-av-photo");

        expect(photo.classes()).toContain("chat-img-hold");

        photo.element.dispatchEvent(new Event("load"));

        expect(photo.classes()).not.toContain("chat-img-hold");
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

        outside.dispatchEvent(new MouseEvent("pointerdown", { bubbles: true }));
        await settle();

        expect(w.emitted("close")).toHaveLength(1);
    });
});

describe("the name line", () => {
    test("carries no Leader pill, even for a leader of the room the card was opened from", () => {
        const w = mountCard(profile({ is_leader: true }));

        expect(w.find("[data-testid='profile-leader']").exists()).toBe(false);
        expect(w.get("[data-testid='profile-name']").text()).toBe("Anna Reyes");
    });

    test("carries no pill for anyone else", () => {
        const w = mountCard();

        expect(w.find("[data-testid='profile-leader']").exists()).toBe(false);
        expect(w.find("[data-testid='profile-you']").exists()).toBe(false);
    });

    test("carries no You pill on the person's own card", () => {
        const w = mountCard(profile({ is_self: true }));

        expect(w.find("[data-testid='profile-you']").exists()).toBe(false);
        expect(w.get("[data-testid='profile-name']").text()).toBe("Anna Reyes");
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

    test("holds no details: Rock's rows, the badges and the church's template are the panel's", () => {
        const w = mountCard(profile());

        expect(w.find("[data-testid='profile-details']").exists()).toBe(false);
        expect(w.find("[data-testid='profile-note']").exists()).toBe(false);
        expect(w.find(".chat-skeleton").exists()).toBe(false);
    });
});

// Opened from a message, the card leaves the message readable: under the whole row, its left edge
// on the face column, whether the face or the name was pressed; above the row only without room.
describe("where it is placed", () => {
    type Box = { left: number, top: number, width: number, height: number };

    function boxed(element: HTMLElement, box: Box): void {
        element.getBoundingClientRect = () => ({ ...box, x: box.left, y: box.top, right: box.left + box.width, bottom: box.top + box.height, toJSON: () => box }) as DOMRect;
    }

    /** Opens the card from a name inside a message row, in a window 1000 by 800; the card is 300 by 180. */
    async function openFrom(row: Box, layout: "bubble" | "community"): Promise<HTMLElement> {
        Object.defineProperty(document.documentElement, "clientWidth", { configurable: true, value: 1000 });
        Object.defineProperty(document.documentElement, "clientHeight", { configurable: true, value: 800 });
        const message = document.createElement("div");
        message.dataset.messageId = "42";
        const face = document.createElement("span");
        face.className = layout === "bubble" ? "chat-msg-face" : "chat-cm-gutter";
        const name = document.createElement("button");
        message.append(face, name);
        document.body.append(message);
        boxed(message, row);
        boxed(face, { left: row.left + 16, top: row.top + 4, width: 36, height: 36 });
        boxed(name, { left: row.left + 64, top: row.top + 4, width: 80, height: 18 });

        const sized = jest.spyOn(HTMLElement.prototype, "offsetWidth", "get").mockImplementation(function (this: HTMLElement) {
            return this.classList.contains("chat-pc") ? 300 : 0;
        });
        const tall = jest.spyOn(HTMLElement.prototype, "offsetHeight", "get").mockImplementation(function (this: HTMLElement) {
            return this.classList.contains("chat-pc") ? 180 : 0;
        });

        const w = mountCard(profile(), { anchor: name });
        await settle();
        await new Promise(resolve => setTimeout(resolve, 0));
        await settle();
        sized.mockRestore();
        tall.mockRestore();

        return card(w);
    }

    test.each(["bubble", "community"] as const)("in the %s layout it stands 8 px under the whole row, its left edge on the face", async layout => {
        const placed = await openFrom({ left: 300, top: 200, width: 600, height: 60 }, layout);

        expect(placed.style.left).toBe("316px");
        expect(placed.style.top).toBe(`${200 + 60 + 8}px`);
    });

    // From a Members row the card stands beside the panel, its own face level with the row's: the
    // two faces' middles on one line, whatever the card's name makes of its height.
    test("from a Members row it stands 8 px left of the panel, its face's middle on the row's face's middle", async () => {
        Object.defineProperty(document.documentElement, "clientWidth", { configurable: true, value: 1000 });
        Object.defineProperty(document.documentElement, "clientHeight", { configurable: true, value: 800 });
        const panel = document.createElement("div");
        panel.className = "chat-panel";
        const who = document.createElement("button");
        const face = document.createElement("span");
        panel.append(who, face);
        document.body.append(panel);
        boxed(panel, { left: 700, top: 0, width: 300, height: 800 });
        boxed(who, { left: 700, top: 300, width: 300, height: 48 });
        // the row's face stands above the button's middle, as beside a name on two lines, so the
        // card follows the face and not the button it was pressed on
        boxed(face, { left: 710, top: 296, width: 36, height: 36 });

        // the card's face, 56 px, sits 20 px down a card whose name runs to two lines
        const sized = jest.spyOn(HTMLElement.prototype, "offsetWidth", "get").mockImplementation(function (this: HTMLElement) {
            return this.classList.contains("chat-pc") ? 300 : 0;
        });
        const tall = jest.spyOn(HTMLElement.prototype, "offsetHeight", "get").mockImplementation(function (this: HTMLElement) {
            return this.classList.contains("chat-pc") ? 180 : this.classList.contains("chat-pile") && !!this.closest(".chat-pc-id") ? 56 : 0;
        });
        const down = jest.spyOn(HTMLElement.prototype, "offsetTop", "get").mockImplementation(function (this: HTMLElement) {
            return this.classList.contains("chat-pile") && !!this.closest(".chat-pc-id") ? 20 : 0;
        });

        const w = mountCard(profile(), { anchor: who, face });
        await settle();
        await new Promise(resolve => setTimeout(resolve, 0));
        await settle();
        sized.mockRestore();
        tall.mockRestore();
        down.mockRestore();

        const placed = card(w);
        expect(placed.style.left).toBe(`${700 - 8 - 300}px`);
        expect(placed.style.top).toBe(`${296 + 18 - (20 + 28)}px`);
        panel.remove();
    });

    test("with no room below the row it stands 8 px above it, its left edge still on the face", async () => {
        const placed = await openFrom({ left: 300, top: 560, width: 600, height: 60 }, "bubble");

        expect(placed.style.left).toBe("316px");
        expect(placed.style.top).toBe(`${560 - 8 - 180}px`);
    });
});

// The switch for a person's own details is Chat settings' alone; their own card is a way there.
describe("the person's own switch", () => {
    test("is not on their own card, whichever way it is set", () => {
        const on = mountCard(profile({ is_self: true, can_message: false }));
        expect(on.find("[data-testid='switch-profile_details']").exists()).toBe(false);
        on.unmount();

        const off = mountCard(profile({ is_self: true, can_message: false, details: null }));
        expect(off.find("[data-testid='switch-profile_details']").exists()).toBe(false);
    });

    test("leaves their own card Chat settings and Details at equal widths, with no More", () => {
        const w = mountCard(profile({ is_self: true, can_message: false }));

        expect(actionIds(w)).toEqual(["profile-settings", "profile-details-open"]);
        expect(w.findAll("[data-testid='profile-actions'] .chat-pc-wide")).toHaveLength(2);
        expect(w.find("[data-testid='profile-more']").exists()).toBe(false);
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

describe("the action row", () => {
    test("is Message and Details at equal widths, then a square More", () => {
        const w = mountCard();

        expect(actionIds(w)).toEqual(["profile-message", "profile-details-open", "profile-more"]);
        expect(w.findAll("[data-testid='profile-actions'] .chat-pc-wide")).toHaveLength(2);
        expect(w.find("[data-testid='profile-actions'] .chat-pc-wide [data-testid='profile-more']").exists()).toBe(false);
        expect(w.get("[data-testid='profile-more']").attributes("aria-label")).toBe("More");
    });

    test("shares the row's width equally between its wide buttons, the More keeping its square", () => {
        const sheet = join(shellFolder, "components/profileCard.partial.obs");
        const wide = styleRules(sheet, /^\.chat-pc-wide$/).join(";");

        expect(styleRules(sheet, /^\.chat-pc-row$/).join(";")).toMatch(/display:\s*flex/);
        expect(wide).toMatch(/flex:\s*1 1 0/);
        expect(wide).toMatch(/min-width:\s*0/);
    });

    test("Details asks for the person's full profile", async () => {
        const anna = profile();
        const w = mountCard(anna);

        const details = w.get("[data-testid='profile-details-open']");
        expect(details.text()).toBe("Details");
        await details.trigger("click");

        expect(w.emitted("details")).toEqual([[anna]]);
    });

    test("for someone who cannot be messaged is Details and More under the words", () => {
        const w = mountCard(profile({ can_message: false }));

        expect(actionIds(w)).toEqual(["profile-details-open", "profile-more"]);
        const words = w.get("[data-testid='profile-cannot-message']").element;
        expect(words.compareDocumentPosition(w.get("[data-testid='profile-details-open']").element) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy();
    });
});

describe("blocking from the card", () => {
    test("someone else's card holds Block in its More, and asks for it", async () => {
        const w = mountCard(profile());

        expect(w.find("[data-testid='profile-block']").exists()).toBe(false);
        expect(await openMore(w)).toEqual(["block"]);
        document.querySelector<HTMLElement>("[data-key='block']")!.click();
        await settle();

        expect(w.emitted("block")?.[0]?.[0]).toMatchObject({ person_alias_guid: "a0000002-0000-4000-8000-000000000000" });
    });

    test("a card of someone the person blocked says so in words, with only a More that holds Unblock", async () => {
        const w = mountCard(profile({ is_blocked: true, can_message: false }));

        expect(w.get("[data-testid='profile-blocked']").text()).toContain("You blocked Anna.");
        expect(actionIds(w)).toEqual(["profile-more"]);
        expect(w.find("[data-testid='profile-cannot-message']").exists()).toBe(false);
        expect(await openMore(w)).toEqual(["unblock"]);
        document.querySelector<HTMLElement>("[data-key='unblock']")!.click();
        await settle();

        expect(w.emitted("unblock")).toHaveLength(1);
    });

    test("the person's own card offers neither", () => {
        const w = mountCard(profile({ is_self: true }));

        expect(w.find("[data-testid='profile-more']").exists()).toBe(false);
        expect(w.find("[data-testid='profile-block']").exists()).toBe(false);
        expect(w.find("[data-testid='profile-unblock']").exists()).toBe(false);
    });

    test("a block or unblock still out holds its item, and choosing it asks nothing", async () => {
        const w = mountCard(profile({ is_blocked: true, can_message: false }), { isBlockBusy: true });

        await openMore(w);
        const unblock = document.querySelector<HTMLElement>("[data-key='unblock']")!;
        expect(unblock.getAttribute("aria-disabled")).toBe("true");
        unblock.click();
        await settle();

        expect(w.emitted("unblock")).toBeUndefined();
    });

    test("stays open while focus is in its own More menu", async () => {
        const w = mountCard(profile());
        await settle();
        await openMore(w);
        const item = document.querySelector<HTMLElement>("[data-key='block']")!;

        card(w).dispatchEvent(new FocusEvent("focusout", { bubbles: true, relatedTarget: item }));
        await settle();

        expect(w.emitted("close")).toBeUndefined();
    });
});

describe("online status", () => {
    let status: StatusStore;
    let unprovide: () => void;

    beforeEach(() => {
        status = createStatusStore();
        unprovide = provideStatus(status);
    });

    afterEach(() => {
        unprovide();
    });

    const identity = (w: VueWrapper): Element => w.get("[data-testid='profile-identity']").element;
    const line = (w: VueWrapper): string | null => {
        const found = w.find("[data-testid='profile-line']");
        return found.exists() ? found.text().replace(/\s+/g, " ").trim() : null;
    };

    test("the card draws the person's badge and their words on the campus line", () => {
        const w = mountCard(profile({ person_alias_guid: anna.toUpperCase() }));

        expect(badgesIn(identity(w))).toEqual([{ state: "online", isMoving: false }]);
        expect(line(w)).toBe("Online, Downtown Campus");
        expect(w.get("[data-testid='profile-status']").text()).toBe("Online");
        expect(w.get("[data-testid='profile-campus']").text()).toBe("Downtown Campus");
    });

    test("the words stand alone with no campus, and the line holds only the campus when the store says nothing", () => {
        expect(line(mountCard(profile({ person_alias_guid: anna, campus_id: null })))).toBe("Online");
        wrapper?.unmount();
        expect(line(mountCard(profile({ person_alias_guid: carl })))).toBe("Downtown Campus");
        wrapper?.unmount();
        expect(line(mountCard(profile({ person_alias_guid: carl, campus_id: null })))).toBeNull();
    });

    test("her own card draws her own face in her own words, and her hollow ring under Appear offline", async () => {
        const w = mountCard(profile({ person_alias_guid: viewer, is_self: true, campus_id: null }));
        expect(line(w)).toBe("Online");

        status.mine.value = { busy_until: null, appear_offline: true };
        await nextTick();

        expect(badgesIn(identity(w)).map(badge => badge.state)).toEqual(["hidden"]);
        expect(line(w)).toBe("Appear offline");
    });

    // The card says who she is to the people who open it, so her own Busy reads as theirs does;
    // what a Busy does to her is said where she acts on it, her face and the Status group.
    test.each([
        ["Busy until 3:00 PM", new Date(2026, 10, 10, 15, 0).toISOString()],
        ["Busy", "infinity"]
    ])("her own card says her Busy in the words others' cards use: %s", async (words, busyUntil) => {
        jest.useFakeTimers({ now: new Date(2026, 10, 10, 12, 0), doNotFake: ["nextTick", "queueMicrotask", "setImmediate", "setTimeout", "clearTimeout", "setInterval", "clearInterval", "requestAnimationFrame", "cancelAnimationFrame"] });
        unprovide();
        status = createStatusStore({ clock: "page" });
        unprovide = provideStatus(status);
        status.mine.value = { busy_until: busyUntil, appear_offline: false };

        try {
            const w = mountCard(profile({ person_alias_guid: viewer, is_self: true, campus_id: null }));

            expect(badgesIn(identity(w)).map(badge => badge.state)).toEqual(["busy"]);
            expect(line(w)).toBe(words);
        }
        finally {
            jest.useRealTimers();
        }
    });

    // The line beside the face already says the words, so the badge holds no tip that says them
    // again, and on her own card none in her own longer words.
    test.each([
        ["her own card, Busy", viewer, true],
        ["another person's card", anna, false]
    ])("resting on the badge of %s shows no tip", async (_case, alias, isSelf) => {
        jest.useFakeTimers({ now: new Date(2026, 10, 10, 12, 0) });
        if (isSelf) {
            status.mine.value = { busy_until: new Date(2026, 10, 10, 15, 0).toISOString(), appear_offline: false };
        }

        try {
            const w = mountCard(profile({ person_alias_guid: alias, is_self: isSelf, campus_id: null }));
            expect(badgesIn(identity(w))).toHaveLength(1);

            identity(w).querySelector("[data-testid='status-badge']")!.dispatchEvent(new MouseEvent("mouseenter"));
            jest.advanceTimersByTime(1000);
            await nextTick();

            expect(document.querySelectorAll("[role='tooltip']")).toHaveLength(0);
        }
        finally {
            jest.useRealTimers();
        }
    });

    test("the words line is one line that never splits into columns: the words keep their width up to the line's, the campus gives way first with an ellipsis", () => {
        const sheet = join(shellFolder, "components/profileCard.partial.obs");
        const rule = (selector: RegExp): string => styleRules(sheet, selector).join(";");
        const w = mountCard(profile({ person_alias_guid: anna }));

        // the line never wraps, and holds what does not fit inside itself
        expect(rule(/^\.chat-pc-line$/)).toMatch(/white-space:\s*nowrap/);
        expect(rule(/^\.chat-pc-line$/)).toMatch(/overflow:\s*hidden/);
        expect(rule(/^\.chat-pc-line$/)).not.toMatch(/flex-wrap:\s*wrap/);
        // the words never shrink for the campus, and cut with an ellipsis only past the whole line
        expect(rule(/^\.chat-pc-status$/)).toMatch(/flex:\s*none/);
        expect(rule(/^\.chat-pc-status$/)).toMatch(/max-width:\s*100%/);
        expect(rule(/^\.chat-pc-status$/)).toMatch(/text-overflow:\s*ellipsis/);
        // the campus shrinks, its name cut with an ellipsis, its pin kept
        expect(rule(/^\.chat-pc-campus$/)).toMatch(/min-width:\s*0/);
        expect(rule(/^\.chat-pc-place$/)).toMatch(/text-overflow:\s*ellipsis/);
        expect(rule(/^\.chat-pc-place$/)).toMatch(/overflow:\s*hidden/);
        expect(w.get("[data-testid='profile-campus'] .chat-pc-place").text()).toBe("Downtown Campus");
    });

    test("rings its badge in the card's fill", () => {
        expect(setsBothRings(join(shellFolder, "components/profileCard.partial.obs"), /^\.chat-pc-id(\[|:|$)/)).toBe(true);
    });

    test.each(swaps)("swapped in place to another person, %s, draws the new person's face as state", async (_case, from, to) => {
        const w = mountCard(profile({ person_alias_guid: from }));
        const face = w.get("[data-testid='profile-identity'] .chat-av").element;

        await w.setProps({ person: profile({ person_alias_guid: to }) });
        await nextTick();

        expect(w.get("[data-testid='profile-identity'] .chat-av").element).not.toBe(face);
        expect(badgesIn(card(w)).filter(badge => badge.isMoving)).toEqual([]);
        expect(badgesIn(card(w))).toHaveLength(to === carl ? 0 : 1);
    });
});
