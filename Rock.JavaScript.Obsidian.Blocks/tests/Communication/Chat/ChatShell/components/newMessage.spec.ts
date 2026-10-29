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
// New message, mounted as the full view it is: a To field of chips over the platform's people
// search, which waits for a pause in typing and marks what it matched, and a body that follows
// the lookup of the conversation the chosen people already have. Nothing chosen asks the person
// to choose; a slow lookup says it is looking; a conversation they have already says so and takes
// what they send; new people get the start of their conversation, its first message drawn through
// its phases on the pending message, a failed one with Try again and Delete, and a refusal in a
// strip that offers to take the refused person out, the text kept. The keys are the To field's:
// Enter with text only ever picks someone, Enter on an empty field and Tab move to the message,
// Backspace takes the last chip back, Escape closes the list and then the view.
//
// The view draws the people, the lookup and the draft in the exact shapes useDirectMessage keeps,
// and reads what it asks for from its events.
/* eslint-disable @typescript-eslint/naming-convention */
import { DOMWrapper, mount, VueWrapper } from "@vue/test-utils";
import { nextTick, reactive } from "vue";
import NewMessage from "../../../../../src/Communication/Chat/ChatShell/components/newMessage.partial.obs";
import { DirectMessageDraft, DirectMessageLookup, PersonRow } from "../../../../../src/Communication/Chat/ChatShell/composables/useDirectMessage.partial";
import { TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";
import Avatar from "../../../../../src/Communication/Chat/ChatShell/ui/avatar.partial.obs";

// #region Fixtures

function person(n: number, nickName: string, lastName: string): PersonRow {
    return { person_alias_guid: `a00000${String(n).padStart(2, "0")}-0000-4000-8000-000000000000`, nick_name: nickName, last_name: lastName, avatar_url: null };
}

const anna = person(1, "Anna", "Reyes");
const andre = person(2, "Andre", "Lin");
const caleb = person(3, "Caleb", "Brooks");
const hannah = person(4, "Hannah", "Diaz");
const luis = person(5, "Luis", "Romero");
const eight = [anna, andre, caleb, hannah, luis, person(6, "Mia", "Soto"), person(7, "Noah", "Park"), person(8, "Omar", "Faris")];

const idle: DirectMessageLookup = { status: "idle", channelId: null, isSlow: false };
const waiting: DirectMessageLookup = { status: "waiting", channelId: null, isSlow: false };
const fresh: DirectMessageLookup = { status: "new", channelId: null, isSlow: false };
const existing = "c0000005-0000-4000-8000-000000000000";
const found: DirectMessageLookup = { status: "found", channelId: existing, isSlow: false };

function draftFor(people: PersonRow[], overrides: Partial<DirectMessageDraft> = {}): DirectMessageDraft {
    return { people, body: "", status: "draft", channelGuid: null, phase: null, ...overrides };
}

// #endregion Fixtures

// #region Helpers

let wrapper: VueWrapper | null = null;

function mountView(props: Record<string, unknown> = {}): VueWrapper {
    wrapper = mount(NewMessage, {
        props: {
            maxOthers: 8,
            chosen: [],
            search: jest.fn(async () => [anna, andre]),
            lookup: idle,
            foundTitle: null,
            draft: null,
            error: null,
            refused: null,
            isPhone: false,
            ...props
        },
        attachTo: document.body
    });
    return wrapper;
}

function field(w: VueWrapper): HTMLInputElement {
    return w.get("[data-testid='nd-to-input']").element as HTMLInputElement;
}

function editor(w: VueWrapper): HTMLElement {
    return w.get("[data-testid='new-message-view'] [data-testid='composer-input']").element as HTMLElement;
}

function plain(text: string | undefined): string {
    return (text ?? "").replace(/\s+/g, " ").trim();
}

/** The words beside the face an element draws: a face without a photo draws its initials as text. */
function besideFace(element: DOMWrapper<Element>): string {
    const face = element.findComponent(Avatar);
    return plain(face.exists() ? element.text().replace(face.text(), "") : element.text());
}

async function settle(): Promise<void> {
    for (let i = 0; i < 6; i++) {
        await Promise.resolve();
        await nextTick();
    }
}

/** Types in the To field, as a browser does, with focus in it. */
async function typeName(w: VueWrapper, text: string): Promise<void> {
    field(w).focus();
    await w.get("[data-testid='nd-to-input']").trigger("focus");
    await w.get("[data-testid='nd-to-input']").setValue(text);
}

/** Waits out the search's pause and lets its answer land. */
async function afterPause(): Promise<void> {
    jest.advanceTimersByTime(250);
    await settle();
}

async function press(w: VueWrapper, key: string, options: Record<string, unknown> = {}): Promise<KeyboardEvent> {
    const event = new KeyboardEvent("keydown", { key, bubbles: true, cancelable: true, ...options });
    field(w).dispatchEvent(event);
    await settle();
    return event;
}

/** Types into the message editor as a browser does, then fires input. */
async function typeMessage(w: VueWrapper, text: string): Promise<void> {
    const box = editor(w);
    box.replaceChildren(document.createTextNode(text));
    box.dispatchEvent(new Event("input", { bubbles: true }));
    await nextTick();
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
    jest.useRealTimers();
    document.body.innerHTML = "";
});

describe("the header", () => {
    test("names the view, a region labelled by its title, with a close X on a wide block", async () => {
        const w = mountView();

        const view = w.get("[data-testid='new-message-view']");
        expect(view.attributes("role")).toBe("region");
        expect(view.attributes("aria-labelledby")).toBe(w.get("[data-testid='nd-title']").attributes("id"));
        expect(w.get("[data-testid='nd-title']").text()).toBe("New message");
        expect(w.find("[data-testid='nd-back']").exists()).toBe(false);

        const close = w.get("[data-testid='nd-close']");
        expect(close.attributes("aria-label")).toBe("Close New message");
        await close.trigger("click");
        expect(w.emitted("close")).toHaveLength(1);
    });

    test("leads with Back on a phone and draws no X", async () => {
        const w = mountView({ isPhone: true });

        expect(w.find("[data-testid='nd-close']").exists()).toBe(false);
        const back = w.get("[data-testid='nd-back']");
        expect(back.attributes("aria-label")).toBe("Back to conversations");
        await back.trigger("click");
        expect(w.emitted("back")).toHaveLength(1);
    });

    test.each([
        ["nothing chosen", {}, "Choose up to 8 people"],
        ["nothing chosen where the platform's group size is 3", { maxOthers: 3 }, "Choose up to 3 people"],
        ["one person new", { chosen: [anna], lookup: fresh, draft: draftFor([anna]) }, "New conversation"],
        ["a new group", { chosen: [anna, caleb, hannah], lookup: fresh, draft: draftFor([anna, caleb, hannah]) }, "New group of 4 people"],
        ["a conversation they have", { chosen: [anna], lookup: found, foundTitle: "Anna Reyes" }, "You already have this conversation"]
    ])("says what the view holds under the title: %s", (_name, props, subtitle) => {
        const w = mountView(props);

        expect(w.get("[data-testid='nd-subtitle']").text()).toBe(subtitle);
    });
});

describe("the To field", () => {
    test("is labelled To: and asks for a name, focus landing on it as the view opens", () => {
        const w = mountView();

        expect(w.get("[data-testid='nd-to-label']").text()).toBe("To:");
        expect(field(w).getAttribute("role")).toBe("combobox");
        expect(field(w).placeholder).toBe("Type a name");
        expect(field(w).hasAttribute("data-autofocus")).toBe(true);
    });

    test("draws each person chosen as a chip with their face and a labelled remove", async () => {
        const w = mountView({ chosen: [anna, caleb], lookup: fresh, draft: draftFor([anna, caleb]) });

        const chips = w.findAll("[data-testid='nd-chip']");
        expect(chips.map(besideFace)).toEqual(["Anna Reyes", "Caleb Brooks"]);
        expect(chips[0].findComponent(Avatar).exists()).toBe(true);
        expect(field(w).placeholder).toBe("");

        const remove = chips[1].get("[data-testid='nd-chip-remove']");
        expect(remove.attributes("aria-label")).toBe("Remove Caleb Brooks");
        await remove.trigger("click");
        expect(w.emitted("unchoose")).toEqual([[caleb.person_alias_guid]]);
    });

    test("opens no list of suggestions on focus with nothing typed, and has no Recent heading", async () => {
        const w = mountView();

        field(w).focus();
        await w.get("[data-testid='nd-to-input']").trigger("focus");
        await settle();

        expect(w.find("[data-testid='nd-suggestions']").exists() && w.get("[data-testid='nd-suggestions']").isVisible()).toBe(false);
        expect(w.text()).not.toContain("Recent");
    });
});

describe("the suggestions", () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });

    test("say Searching… at once, and search 250 ms after the last keystroke", async () => {
        const search = jest.fn(async () => [anna, andre]);
        const w = mountView({ search });

        await typeName(w, "a");
        await typeName(w, "an");
        expect(w.get("[data-testid='nd-searching']").text()).toBe("Searching…");
        jest.advanceTimersByTime(249);
        expect(search).not.toHaveBeenCalled();

        await afterPause();
        expect(search).toHaveBeenCalledTimes(1);
        expect(search).toHaveBeenCalledWith("an");
    });

    test("name each person's campus under their name, from the church's campuses, where they show it", async () => {
        const downtown = { guid: "d0000001-0000-4000-8000-000000000000", name: "Downtown Campus", shortCode: "Downtown" };
        const search = jest.fn(async () => [{ ...anna, campus_id: "D0000001-0000-4000-8000-000000000000" }, { ...andre, campus_id: null }, { ...caleb, campus_id: "d0000009-0000-4000-8000-000000000000" }]);
        const w = mountView({ search, campuses: [downtown] });

        await typeName(w, "a");
        await afterPause();

        const options = w.findAll("[data-testid='nd-option']");
        expect(options.map(o => o.find("[data-testid='nd-option-campus']").exists() ? o.get("[data-testid='nd-option-campus']").text() : null))
            .toEqual(["Downtown Campus", null, null]);
        expect(options[0].get("b").text()).toBe("Anna Reyes");
    });

    test("list the people found as options, the match marked, with nothing under the name", async () => {
        const w = mountView();

        await typeName(w, "an");
        await afterPause();

        const list = w.get("[data-testid='nd-suggestions']");
        expect(list.attributes("role")).toBe("listbox");
        expect(field(w).getAttribute("aria-expanded")).toBe("true");
        const options = w.findAll("[data-testid='nd-option']");
        expect(options.map(besideFace)).toEqual(["Anna Reyes", "Andre Lin"]);
        expect(options.every(o => o.attributes("role") === "option")).toBe(true);
        expect(options[0].get("mark").text()).toBe("An");
        expect(options[0].attributes("aria-selected")).toBe("true");
    });

    test("leave out the people already chosen", async () => {
        const w = mountView({ chosen: [anna], lookup: fresh, draft: draftFor([anna]) });

        await typeName(w, "an");
        await afterPause();

        expect(w.findAll("[data-testid='nd-option']").map(besideFace)).toEqual(["Andre Lin"]);
    });

    test("say no one was found, with the hint about who can be messaged", async () => {
        const w = mountView({ search: jest.fn(async () => []) });

        await typeName(w, "zz");
        await afterPause();

        expect(w.get("[data-testid='nd-none']").text())
            .toBe("No one found for “zz”. Can't find someone? They may not accept messages from everyone.");
    });

    test("an answer for text the person has since changed is dropped", async () => {
        let answerFirst!: (people: PersonRow[]) => void;
        const search = jest.fn()
            .mockImplementationOnce(() => new Promise<PersonRow[]>(resolve => answerFirst = resolve))
            .mockImplementationOnce(async () => [caleb]);
        const w = mountView({ search });

        await typeName(w, "an");
        await afterPause();
        await typeName(w, "ca");
        await afterPause();
        answerFirst([anna, andre]);
        await settle();

        expect(w.findAll("[data-testid='nd-option']").map(besideFace)).toEqual(["Caleb Brooks"]);
    });

    test("say how many were found to a screen reader", async () => {
        const w = mountView();

        await typeName(w, "an");
        await afterPause();

        const live = w.get("[data-testid='nd-announcement']");
        expect(live.attributes("aria-live")).toBe("polite");
        expect(live.text()).toBe("2 people found");
    });

    test("a click on an option chooses that person", async () => {
        const w = mountView();
        await typeName(w, "an");
        await afterPause();

        await w.findAll("[data-testid='nd-option']")[1].trigger("mousedown");

        expect(w.emitted("choose")).toEqual([[andre]]);
    });
});

describe("the notes under the field", () => {
    test("count the people from six, of the eight a conversation can hold", () => {
        const five = mountView({ chosen: eight.slice(0, 5), lookup: fresh, draft: draftFor(eight.slice(0, 5)) });
        expect(plain(five.find("[data-testid='nd-note']").exists() ? five.get("[data-testid='nd-note']").text() : "")).toBe("");
        five.unmount();

        const six = mountView({ chosen: eight.slice(0, 6), lookup: fresh, draft: draftFor(eight.slice(0, 6)) });
        expect(six.get("[data-testid='nd-note']").text()).toBe("6 of 8 people");
    });

    test("at eight, say it is the most and how to move on", () => {
        const w = mountView({ chosen: eight, lookup: fresh, draft: draftFor(eight) });

        expect(w.get("[data-testid='nd-note']").text()).toBe("8 people is the most for one conversation. Press Tab to write your message.");
    });
});

describe("the keys in the To field", () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });

    test("Enter with text picks the person marked, and never sends", async () => {
        const w = mountView();
        await typeName(w, "an");
        await afterPause();

        await press(w, "ArrowDown");
        expect(w.findAll("[data-testid='nd-option']")[1].attributes("aria-selected")).toBe("true");
        await press(w, "Enter");

        expect(w.emitted("choose")).toEqual([[andre]]);
        expect(w.emitted("send")).toBeUndefined();
    });

    test("Enter before the search has answered picks the first person it finds", async () => {
        const w = mountView();
        await typeName(w, "an");

        await press(w, "Enter");
        expect(w.emitted("choose")).toBeUndefined();
        await afterPause();

        expect(w.emitted("choose")).toEqual([[anna]]);
    });

    test("Enter with text and no one found says there is no one to message", async () => {
        const w = mountView({ search: jest.fn(async () => []) });
        await typeName(w, "zz");
        await afterPause();

        await press(w, "Enter");

        expect(w.emitted("choose")).toBeUndefined();
        expect(w.get("[data-testid='nd-note']").text()).toBe("No one to message for “zz”");
    });

    test("Enter on an empty field with people chosen moves to the message", async () => {
        const w = mountView({ chosen: [anna], lookup: fresh, draft: draftFor([anna]) });
        field(w).focus();

        await press(w, "Enter");

        expect(document.activeElement).toBe(editor(w));
        expect(w.emitted("send")).toBeUndefined();
    });

    test("Enter on an empty field at eight people keeps focus in the field", async () => {
        const w = mountView({ chosen: eight, lookup: fresh, draft: draftFor(eight) });
        field(w).focus();

        await press(w, "Enter");

        expect(document.activeElement).toBe(field(w));
    });

    test("Tab moves to the message editor, past the emoji button", async () => {
        const w = mountView({ chosen: [anna], lookup: fresh, draft: draftFor([anna]) });
        field(w).focus();

        const event = await press(w, "Tab");

        expect(event.defaultPrevented).toBe(true);
        expect(document.activeElement).toBe(editor(w));
    });

    test("Backspace on an empty field takes the last chip back", async () => {
        const w = mountView({ chosen: [anna, caleb], lookup: fresh, draft: draftFor([anna, caleb]) });
        field(w).focus();

        await press(w, "Backspace");

        expect(w.emitted("unchoose")).toEqual([[caleb.person_alias_guid]]);
    });

    test("Escape closes the list first, then the view when no one is chosen", async () => {
        const w = mountView();
        await typeName(w, "an");
        await afterPause();
        expect(field(w).getAttribute("aria-expanded")).toBe("true");

        await press(w, "Escape");
        expect(field(w).getAttribute("aria-expanded")).toBe("false");
        expect(w.emitted("close")).toBeUndefined();

        await w.get("[data-testid='nd-to-input']").setValue("");
        await press(w, "Escape");
        expect(w.emitted("close")).toHaveLength(1);
    });

    test("Escape keeps the view while people are chosen", async () => {
        const w = mountView({ chosen: [anna], lookup: fresh, draft: draftFor([anna]) });
        field(w).focus();

        await press(w, "Escape");

        expect(w.emitted("close")).toBeUndefined();
    });
});

describe("the body", () => {
    test("with nothing chosen, asks the person to start a conversation", () => {
        const w = mountView();

        const empty = w.get("[data-testid='nd-empty']");
        expect(empty.get("b").text()).toBe("Start a conversation");
        expect(plain(empty.get("p").text())).toBe("Choose one person for a private conversation, or up to 8 for a group. You can message people who accept messages from everyone, and people in your private groups.");
        expect(w.find("[data-testid='new-message-view'] [data-testid='composer-input']").exists()).toBe(false);
    });

    test("while the lookup is out, says nothing until it is slow, then that it is looking", async () => {
        const w = mountView({ chosen: [anna], lookup: waiting });
        expect(w.find("[data-testid='nd-looking']").exists()).toBe(false);
        expect(w.find("[data-testid='nd-start']").exists()).toBe(false);

        await w.setProps({ lookup: { ...waiting, isSlow: true } });
        expect(plain(w.get("[data-testid='nd-looking']").text())).toBe("Looking for your conversation…");
    });

    test("for a conversation they already have, says so above it, with Open and where Enter sends", async () => {
        const w = mountView({ chosen: [anna], lookup: found, foundTitle: "Anna Reyes" });

        const strip = w.get("[data-testid='nd-found']");
        expect(strip.attributes("role")).toBe("status");
        expect(strip.get("b").text()).toBe("You already have this conversation");
        expect(plain(strip.text())).toContain("Anna Reyes. What you send goes there.");
        expect(plain(w.get("[data-testid='nd-hint']").text())).toBe("Enter sends to your conversation with Anna Reyes");

        await w.get("[data-testid='nd-open']").trigger("click");
        expect(w.emitted("open")).toEqual([[existing]]);
    });

    test("for a group they already have, draws its mark in a person's 36px slot", () => {
        const w = mountView({ chosen: [anna, caleb, hannah], lookup: found, foundTitle: "Anna, Caleb and Hannah" });

        const mark = w.get("[data-testid='nd-found'] [data-testid='group-face']");
        expect(mark.attributes("data-shape")).toBe("trio");
        expect((mark.element as HTMLElement).style.getPropertyValue("--chat-gf-s")).toBe("36px");
        expect(mark.findAll(".chat-av")).toHaveLength(3);
    });

    test("for one new person, starts their conversation by full name and says only the two can see it", () => {
        const w = mountView({ chosen: [caleb], lookup: fresh, draft: draftFor([caleb]) });

        const start = w.get("[data-testid='nd-start']");
        expect(start.findComponent(Avatar).props("people")).toEqual([{ name: "Caleb Brooks", photoUrl: null }]);
        expect(w.get("[data-testid='nd-start-title']").text()).toBe("Caleb Brooks");
        expect(plain(start.get("p").text())).toBe("This is the start of your conversation with Caleb. Only the two of you can see it.");
        expect(start.get("p strong").text()).toBe("Caleb");
        expect(w.get("[data-testid='nd-hint']").text()).toBe("Your first message starts the conversation.");
    });

    test("for a new group, draws the group's mark, the title by the sidebar's rule, and how many can see it", () => {
        const w = mountView({ chosen: [caleb, hannah], lookup: fresh, draft: draftFor([caleb, hannah]) });

        const start = w.get("[data-testid='nd-start']");
        const mark = start.get("[data-testid='group-face']");
        expect(mark.attributes("data-shape")).toBe("pair");
        expect((mark.element as HTMLElement).style.getPropertyValue("--chat-gf-s")).toBe("56px");
        expect(w.get("[data-testid='nd-start-title']").text()).toBe("Caleb and Hannah");
        expect(plain(start.get("p").text())).toBe("This is the start of a group conversation. Only the 3 people here can see it.");
    });

    test("sends what is typed to the view's owner, which says where it goes", async () => {
        const w = mountView({ chosen: [caleb], lookup: fresh, draft: draftFor([caleb]) });

        await typeMessage(w, "hello");
        await w.get("[data-testid='new-message-view'] form").trigger("submit");

        expect(w.emitted("send")).toEqual([["hello"]]);
        expect(w.emitted("edit")?.slice(-1)).toEqual([["hello"]]);
    });
});

describe("the first message", () => {
    test.each([
        ["asking", "Starting your conversation…"],
        ["waiting", "Still setting it up…"],
        ["trying", "Sending…"]
    ] as const)("draws its phase on the pending message: %s", (phase, words) => {
        // started long enough ago that Sending… is no longer held back
        const w = mountView({ chosen: [caleb], lookup: fresh, draft: draftFor([caleb], { body: "hello", status: "starting", phase, startedAt: Date.now() - 5000 }) });

        const pending = w.get("[data-testid='nd-pending']");
        expect(pending.text()).toContain("hello");
        expect(plain(pending.get("[data-testid='nd-phase']").text())).toBe(words);
    });

    // As in a conversation, timed from the send's start and not from the view's mount.
    describe("Sending… waits a quiet second from the send's start", () => {
        beforeEach(() => jest.useFakeTimers());
        afterEach(() => jest.useRealTimers());

        test("a send that began 600 ms ago says nothing until 1000 ms after it began", async () => {
            const w = mountView({ chosen: [caleb], lookup: fresh, draft: draftFor([caleb], { body: "hello", status: "starting", phase: "trying", startedAt: Date.now() - 600 }) });

            expect(w.find("[data-testid='nd-phase']").exists()).toBe(false);
            expect(w.get("[data-testid='nd-pending']").text()).toContain("hello");

            jest.advanceTimersByTime(399);
            await nextTick();
            expect(w.find("[data-testid='nd-phase']").exists()).toBe(false);

            jest.advanceTimersByTime(1);
            await nextTick();
            expect(plain(w.get("[data-testid='nd-phase']").text())).toBe("Sending…");
        });

        test("a send older than a second says Sending… as the view opens", () => {
            const w = mountView({ chosen: [caleb], lookup: fresh, draft: draftFor([caleb], { body: "hello", status: "starting", phase: "trying", startedAt: Date.now() - 2500 }) });

            expect(plain(w.get("[data-testid='nd-phase']").text())).toBe("Sending…");
        });

        test.each(["asking", "waiting", "trying"] as const)("%s shows nothing and announces nothing until 1000 ms after the send began", async phase => {
            const w = mountView({ chosen: [caleb], lookup: fresh, draft: draftFor([caleb], { body: "hello", status: "starting", phase, startedAt: Date.now() }) });

            jest.advanceTimersByTime(999);
            await settle();
            expect(w.find("[data-testid='nd-phase']").exists()).toBe(false);
            expect(w.get("[data-testid='nd-announcement']").text()).toBe("");

            jest.advanceTimersByTime(1);
            await settle();
            const words = { asking: "Starting your conversation…", waiting: "Still setting it up…", trying: "Sending…" }[phase];
            expect(plain(w.get("[data-testid='nd-phase']").text())).toBe(words);
            expect(w.get("[data-testid='nd-announcement']").text()).toBe(words);
        });

        test("a phase change after the quiet second shows and is announced at once", async () => {
            const w = mountView({ chosen: [caleb], lookup: fresh, draft: draftFor([caleb], { body: "hello", status: "starting", phase: "asking", startedAt: Date.now() }) });
            jest.advanceTimersByTime(1000);
            await settle();

            await w.setProps({ draft: draftFor([caleb], { body: "hello", status: "starting", phase: "waiting", startedAt: Date.now() - 1000 }) });
            await settle();

            expect(plain(w.get("[data-testid='nd-phase']").text())).toBe("Still setting it up…");
            expect(w.get("[data-testid='nd-announcement']").text()).toBe("Still setting it up…");
        });

        test("a phase change inside the quiet second stays quiet, then says the current phase", async () => {
            const startedAt = Date.now();
            const w = mountView({ chosen: [caleb], lookup: fresh, draft: draftFor([caleb], { body: "hello", status: "starting", phase: "asking", startedAt }) });
            jest.advanceTimersByTime(500);
            await w.setProps({ draft: draftFor([caleb], { body: "hello", status: "starting", phase: "trying", startedAt }) });
            await settle();
            expect(w.find("[data-testid='nd-phase']").exists()).toBe(false);
            expect(w.get("[data-testid='nd-announcement']").text()).toBe("");

            jest.advanceTimersByTime(500);
            await settle();
            expect(plain(w.get("[data-testid='nd-phase']").text())).toBe("Sending…");
            expect(w.get("[data-testid='nd-announcement']").text()).toBe("Sending…");
        });

        test("a failure inside the quiet second shows at once", async () => {
            const w = mountView({ chosen: [caleb], lookup: fresh, draft: draftFor([caleb], { body: "hello", status: "starting", phase: "trying", startedAt: Date.now() }) });
            jest.advanceTimersByTime(200);
            await w.setProps({ draft: draftFor([caleb], { body: "hello", status: "draft", phase: "failed" }) });
            await settle();

            expect(w.get("[data-testid='nd-phase']").text()).toBe("Not sent");
        });
    });

    test("locks the To field and its chips while it is on its way, and says why", () => {
        const w = mountView({ chosen: [caleb], lookup: fresh, draft: draftFor([caleb], { body: "hello", status: "starting", phase: "asking" }) });

        expect(field(w).disabled).toBe(true);
        expect(w.get("[data-testid='nd-chip-remove']").attributes("disabled")).toBeDefined();
        expect(w.get("[data-testid='nd-hint']").text()).toBe("You can send more once the conversation starts.");
    });

    // As in a conversation: the message is marked where Sending… was, and the choices wait in the
    // strip over the composer.
    test("failed: Not sent under the message, with Try again and Delete over the composer", async () => {
        const w = mountView({ chosen: [caleb], lookup: fresh, draft: draftFor([caleb], { body: "hello", status: "draft", phase: "failed" }) });

        const pending = w.get("[data-testid='nd-pending']");
        expect(pending.get("[data-testid='nd-phase']").text()).toBe("Not sent");
        expect(pending.find(".chat-fail-mark").exists()).toBe(true);
        expect(pending.findAll("button")).toHaveLength(0);

        const strip = w.get("[data-testid='composer-failed']");
        expect(strip.get("[data-testid='composer-failed-lead']").text()).toBe("Your message wasn't sent");
        await strip.get("[data-testid='composer-failed-retry']").trigger("click");
        await strip.get("[data-testid='composer-failed-discard']").trigger("click");

        expect(w.emitted("retry")).toHaveLength(1);
        expect(w.emitted("discard")).toHaveLength(1);
        expect(field(w).disabled).toBe(false);
    });

    test("refused for a person: a strip with the door's sentence offers to take them out by first name, the text kept", async () => {
        const w = mountView({
            chosen: [caleb, luis],
            lookup: fresh,
            draft: draftFor([caleb, luis], { body: "hello", status: "draft" }),
            error: "Luis Romero isn't taking new conversations right now.",
            refused: { person: luis, text: "Luis Romero isn't taking new conversations right now." }
        });
        await settle();

        const strip = w.get("[data-testid='nd-refused']");
        expect(strip.attributes("role")).toBe("alert");
        expect(strip.text()).toContain("Luis Romero isn't taking new conversations right now.");
        expect(editor(w).textContent).toBe("hello");

        const remove = strip.get("[data-testid='nd-refused-remove']");
        expect(remove.text()).toBe("Remove Luis");
        await remove.trigger("click");
        expect(w.emitted("unchoose")).toEqual([[luis.person_alias_guid]]);
    });

    test("refused with no person named: the strip carries the sentence alone", () => {
        const w = mountView({
            chosen: [caleb],
            lookup: fresh,
            draft: draftFor([caleb], { body: "hello", status: "draft" }),
            error: "The conversation could not be started. Try again."
        });

        expect(w.get("[data-testid='nd-refused']").text()).toContain("The conversation could not be started. Try again.");
        expect(w.find("[data-testid='nd-refused-remove']").exists()).toBe(false);
    });
});

describe("what a screen reader is told", () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });

    /** The live regions in the view that hold these words right now. */
    function regionsSaying(w: VueWrapper, words: string): Element[] {
        return Array.from(w.element.querySelectorAll("[aria-live]")).filter(region => plain(region.textContent ?? "").includes(words));
    }

    test("the first person chosen is one person chosen, though the list already holds them as the choice is told", async () => {
        const chosen = reactive<PersonRow[]>([]);
        const w = mountView({ chosen, onChoose: (picked: PersonRow) => chosen.push(picked) });
        await typeName(w, "an");
        await afterPause();

        await press(w, "Enter");

        expect(w.get("[data-testid='nd-announcement']").text()).toBe("Added Anna Reyes. 1 person chosen.");
    });

    test("Enter with no one found is said once, in one live region", async () => {
        const w = mountView({ search: jest.fn(async () => []) });
        await typeName(w, "zz");
        await afterPause();

        await press(w, "Enter");

        expect(regionsSaying(w, "No one to message for")).toHaveLength(1);
        expect(w.get("[data-testid='nd-note']").attributes("aria-live")).toBeUndefined();
    });

    test("the eighth person chosen also says how to move on, since the note under the field is not read by itself", async () => {
        const chosen = reactive<PersonRow[]>(eight.slice(0, 7));
        const w = mountView({ chosen, search: jest.fn(async () => [eight[7]]), onChoose: (picked: PersonRow) => chosen.push(picked) });
        await typeName(w, "om");
        await afterPause();

        await press(w, "Enter");

        expect(w.get("[data-testid='nd-announcement']").text()).toBe("Added Omar Faris. 8 people chosen. 8 people is the most for one conversation. Press Tab to write your message.");
    });

    test("a chip taken out says who was removed", async () => {
        const w = mountView({ chosen: [anna, caleb], lookup: fresh, draft: draftFor([anna, caleb]) });

        await w.findAll("[data-testid='nd-chip-remove']")[0].trigger("click");

        expect(w.emitted("unchoose")).toEqual([[anna.person_alias_guid]]);
        expect(w.get("[data-testid='nd-announcement']").text()).toBe("Removed Anna Reyes.");
    });

    test("Backspace on an empty field says who it took out", async () => {
        const w = mountView({ chosen: [anna, caleb], lookup: fresh, draft: draftFor([anna, caleb]) });
        field(w).focus();

        await press(w, "Backspace");

        expect(w.emitted("unchoose")).toEqual([[caleb.person_alias_guid]]);
        expect(w.get("[data-testid='nd-announcement']").text()).toBe("Removed Caleb Brooks.");
    });

    test.each([
        ["asking", "Starting your conversation…"],
        ["waiting", "Still setting it up…"],
        ["trying", "Sending…"]
    ] as const)("each phase of the first message is told as it comes: %s", async (phase, words) => {
        const w = mountView({ chosen: [caleb], lookup: fresh, draft: draftFor([caleb], { body: "hello" }) });

        // The phase is told once it is shown, a quiet second after the send began.
        await w.setProps({ draft: draftFor([caleb], { body: "hello", status: "starting", phase, startedAt: Date.now() - 5000 }) });
        await settle();

        expect(w.get("[data-testid='nd-announcement']").text()).toBe(words);
    });
});

// A failure is said once, assertively, by the strip that offers its choices, as in a conversation;
// the view's own polite line says nothing over it.
test("a first message that fails is said by the composer's strip, not the view's line", async () => {
    const w = mountView({ chosen: [caleb], lookup: fresh, draft: draftFor([caleb], { body: "hello", status: "starting", phase: "trying" }) });
    await settle();

    await w.setProps({ draft: draftFor([caleb], { body: "hello", status: "draft", phase: "failed" }) });
    await settle();

    expect(w.get("[data-testid='composer-failed-announcement']").text()).toBe("Your message wasn't sent.");
    expect(w.get("[data-testid='nd-announcement']").text()).toBe("");
});

describe("the conversation found", () => {
    function said(id: number): TimelineMessage {
        return { id, person_alias_guid: anna.person_alias_guid, body: `Message ${id}`, message_type: "text", created_at: "2026-10-20T10:00:00Z" } as TimelineMessage;
    }

    test("shows its last messages under the strip, out of reach, as the history slot draws them", () => {
        const w = mount(NewMessage, {
            props: { maxOthers: 8, chosen: [anna], search: jest.fn(async () => []), lookup: found, foundTitle: "Anna Reyes", recent: [said(1), said(2)] },
            slots: { history: "<p data-testid='drawn-history'>the feed</p>" },
            attachTo: document.body
        });
        wrapper = w;

        const history = w.get("[data-testid='nd-history']");
        expect(history.attributes("inert")).toBeDefined();
        expect(history.find("[data-testid='drawn-history']").exists()).toBe(true);
        expect(w.find("[data-testid='nd-history-empty']").exists()).toBe(false);
    });

    test("says No messages yet for a conversation with none", () => {
        const w = mountView({ chosen: [anna], lookup: found, foundTitle: "Anna Reyes", recent: [] });

        expect(w.get("[data-testid='nd-history-empty']").text()).toBe("No messages yet.");
    });

    test("draws nothing in place while its messages are being read, or when the read failed", () => {
        const w = mountView({ chosen: [anna], lookup: found, foundTitle: "Anna Reyes", recent: null });

        expect(w.find("[data-testid='nd-history']").exists()).toBe(false);
        expect(w.find("[data-testid='nd-history-empty']").exists()).toBe(false);
        expect(w.find("[data-testid='nd-found']").exists()).toBe(true);
    });
});

describe("a people search that fails", () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });

    test("says so in the suggestions, not the refused strip, and the next answer takes its place", async () => {
        const search = jest.fn()
            .mockImplementationOnce(async () => {
                throw new Error("People could not be searched. Try again.");
            })
            .mockImplementationOnce(async () => [anna]);
        const w = mountView({ search, chosen: [caleb], lookup: fresh, draft: draftFor([caleb]) });

        await typeName(w, "an");
        await afterPause();

        expect(w.get("[data-testid='nd-suggestions'] [data-testid='nd-search-error']").text()).toBe("People could not be searched. Try again.");
        expect(w.find("[data-testid='nd-refused']").exists()).toBe(false);
        expect(w.findAll("[data-testid='nd-option']")).toHaveLength(0);

        await typeName(w, "ann");
        await afterPause();

        expect(w.find("[data-testid='nd-search-error']").exists()).toBe(false);
        expect(w.findAll("[data-testid='nd-option']").map(besideFace)).toEqual(["Anna Reyes"]);
    });
});

describe("a refused start", () => {
    test("puts the typed text back in the box, as the box handed it over and the door said no", async () => {
        const draft = reactive(draftFor([luis]));
        const w = mountView({ chosen: [luis], lookup: fresh, draft, onEdit: (body: string) => draft.body = body });
        await settle();

        await typeMessage(w, "Want to help Saturday?");
        await w.get("[data-testid='new-message-view'] form").trigger("submit");
        // as the shell's sendFirst does: the draft takes the text and starts, then Rock refuses
        Object.assign(draft, { body: "Want to help Saturday?", status: "starting", phase: "asking" });
        await settle();
        Object.assign(draft, { status: "draft", phase: null });
        await settle();

        expect(editor(w).textContent).toBe("Want to help Saturday?");
    });
});
