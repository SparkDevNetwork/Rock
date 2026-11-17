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
// The person's own status menu, mounted: Automatic, Busy with its ends, and Appear offline, one
// flat set of radio items opened from the sidebar foot's face or the Chat settings group's Change.
// The check sits on what her settings say, never on the item pressed last, so another tab's choice
// or a refused one shows as it stands. The ends are the viewer's own clock, worked out as the menu
// opens and again at the choice. While the church is read only both buttons still open it, every
// item drawn but never chosen and the banner's words as its note, since a choice could only be
// refused and a phone shows no tooltip to say why.
/* eslint-disable @typescript-eslint/naming-convention */
import { readFileSync } from "fs";
import { join } from "path";
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import StatusMenu from "../../../../../src/Communication/Chat/ChatShell/components/statusMenu.partial.obs";
import Button from "../../../../../src/Communication/Chat/ChatShell/ui/button.partial.obs";
import IconButton from "../../../../../src/Communication/Chat/ChatShell/ui/iconButton.partial.obs";
import Menu from "../../../../../src/Communication/Chat/ChatShell/ui/menu.partial.obs";
import { presenceStatusKey } from "../../../../../src/Communication/Chat/ChatShell/composables/usePresenceStatus.partial";
import { FaceStatus } from "../../../../../src/Communication/Chat/ChatShell/types.partial";
import { providedSession } from "../sessionShapes";

// #region Helpers

/** Tuesday, November 10, 2026, 9 p.m. in the viewer's own time. */
const evening = new Date(2026, 10, 10, 21, 0);

const automatic = { busy_until: null, appear_offline: false };

const reason = "Cedar Hill's chat is read only until 9.";

let wrapper: VueWrapper | null = null;

/** Mounts the menu for Jordan Lee, signed in; `isPhone` is the session's width, the rest the menu's inputs. */
function mountMenu(props: Record<string, unknown> = {}, statusOf: (guid: string) => FaceStatus | null = () => null, now: () => number = Date.now): VueWrapper {
    const { isPhone, ...own } = props;
    wrapper = mount(StatusMenu, {
        props: {
            choice: automatic,
            trigger: "change",
            ...own
        },
        global: {
            provide: {
                ...providedSession({ isPhone: isPhone === true, me: { personAliasGuid: "a0000001-0000-4000-8000-000000000000", name: "Jordan Lee" } }),
                [presenceStatusKey as symbol]: { statusOf, register: () => () => undefined, now }
            }
        },
        attachTo: document.body
    });
    return wrapper;
}

function trigger(w: VueWrapper): HTMLElement {
    return w.get("[data-testid='status-change'], [data-testid='status-face']").element as HTMLElement;
}

async function settle(): Promise<void> {
    await nextTick();
    await nextTick();
}

async function openMenu(w: VueWrapper): Promise<void> {
    trigger(w).focus();
    trigger(w).click();
    await settle();
}

function menu(): HTMLElement | null {
    return document.querySelector("[role='menu']");
}

function items(): HTMLElement[] {
    return Array.from(document.querySelectorAll<HTMLElement>("[role='menu'] [role='menuitemradio']"));
}

function keys(): string[] {
    return items().map(item => item.dataset.key ?? "");
}

function itemOf(key: string): HTMLElement {
    return items().find(item => item.dataset.key === key)!;
}

/** The words an item shows as its name, without what it says under them. */
function titleOf(item: HTMLElement): string {
    return plain((item.querySelector(".chat-menu-title") ?? item.querySelector(".chat-menu-label"))!.textContent!);
}

/** What a screen reader calls an item: its own name where it has one, else its words. */
function nameOf(item: HTMLElement): string {
    return plain(item.getAttribute("aria-label") ?? item.textContent ?? "");
}

function checked(): string[] {
    return items().filter(item => item.getAttribute("aria-checked") === "true").map(item => item.dataset.key ?? "");
}

/** Times as a person reads them; some ICU builds put a narrow no-break space before AM and PM. */
function plain(text: string): string {
    return text.replace(/\s+/g, " ").trim();
}

async function press(key: string): Promise<void> {
    (document.activeElement as HTMLElement).dispatchEvent(new KeyboardEvent("keydown", { key, bubbles: true }));
    await settle();
}

/** The choice the menu told its host, the last one. */
function chosen(w: VueWrapper): { busy_until: string | null, appear_offline: boolean } | undefined {
    const all = w.emitted("choose") as Array<[{ busy_until: string | null, appear_offline: boolean }]> | undefined;
    return all?.[all.length - 1]?.[0];
}

// #endregion Helpers

beforeAll(() => {
    // jsdom has no ResizeObserver, and floating placement keeps its menu beside the anchor with one.
    if (typeof window.ResizeObserver === "undefined") {
        window.ResizeObserver = class {
            observe(): void { /* jsdom lays nothing out, so there is nothing to watch. */ }
            unobserve(): void { /* As above. */ }
            disconnect(): void { /* As above. */ }
        } as unknown as typeof ResizeObserver;
    }
});

beforeEach(() => {
    // only the clock: Vue's ticks and the menu's frames run as they do in a browser
    jest.useFakeTimers({ now: evening, doNotFake: ["nextTick", "queueMicrotask", "setImmediate", "setTimeout", "clearTimeout", "setInterval", "clearInterval", "requestAnimationFrame", "cancelAnimationFrame"] });
});

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
    document.body.innerHTML = "";
    jest.useRealTimers();
});

describe("the items", () => {
    test("are six radio items in one set: Automatic, Busy's four ends short to long, Appear offline", async () => {
        const w = mountMenu();
        await openMenu(w);

        expect(keys()).toEqual(["auto", "busy-1h", "busy-2h", "busy-morning", "busy-off", "appear_offline"]);
        expect(menu()!.querySelectorAll("[role='menuitem']")).toHaveLength(0);
        // one set: nothing groups a part of them, so the separators and the heading make no sets of their own
        expect(menu()!.querySelector("[role='group']")).toBeNull();
    });

    // Automatic's word and its green badge say it; Appear offline keeps its line, since without it people fear missing messages.
    test("say what Appear offline means under its name, and nothing under Automatic", async () => {
        const w = mountMenu();
        await openMenu(w);

        expect(titleOf(itemOf("auto"))).toBe("Automatic");
        expect(itemOf("auto").querySelector(".chat-menu-desc")).toBeNull();
        expect(titleOf(itemOf("appear_offline"))).toBe("Appear offline");
        expect(plain(itemOf("appear_offline").querySelector(".chat-menu-desc")!.textContent!)).toBe("You'll look offline. Messages still arrive.");
        expect(itemOf("busy-1h").querySelector(".chat-menu-desc")).toBeNull();
    });

    test("head Busy's ends with a heading a screen reader passes over, each end named as a Busy", async () => {
        const w = mountMenu();
        await openMenu(w);

        const heading = menu()!.querySelector("[role='presentation']")!;
        expect(plain(heading.textContent!)).toBe("Busy, notifications paused");
        // between Automatic and the first end
        expect(heading.compareDocumentPosition(itemOf("auto")) & Node.DOCUMENT_POSITION_PRECEDING).toBeTruthy();
        expect(heading.compareDocumentPosition(itemOf("busy-1h")) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy();

        expect(items().map(titleOf)).toEqual([
            "Automatic", "For 1 hour", "For 2 hours", "Until tomorrow, 8:00 AM", "Until I turn it off", "Appear offline"
        ]);
        expect(["busy-1h", "busy-2h", "busy-morning", "busy-off"].map(key => nameOf(itemOf(key)))).toEqual([
            "Busy for 1 hour", "Busy for 2 hours", "Busy until tomorrow, 8:00 AM", "Busy until I turn it off"
        ]);
    });

    test("name the next 8:00 as today's before the morning comes", async () => {
        jest.setSystemTime(new Date(2026, 10, 11, 1, 0));
        const w = mountMenu();
        await openMenu(w);

        expect(titleOf(itemOf("busy-morning"))).toBe("Until 8:00 AM");
        expect(nameOf(itemOf("busy-morning"))).toBe("Busy until 8:00 AM");
    });

    test("work the ends out again as the menu opens", async () => {
        const w = mountMenu();
        await openMenu(w);
        expect(titleOf(itemOf("busy-morning"))).toBe("Until tomorrow, 8:00 AM");
        await press("Escape");

        // past midnight, the next 8:00 is the same day's
        jest.setSystemTime(new Date(2026, 10, 11, 0, 30));
        await openMenu(w);

        expect(titleOf(itemOf("busy-morning"))).toBe("Until 8:00 AM");
    });
});

describe("the check", () => {
    test("sits on Automatic while she is neither Busy nor hidden", async () => {
        const w = mountMenu();
        await openMenu(w);

        expect(checked()).toEqual(["auto"]);
    });

    test("sits on Appear offline while she has chosen it", async () => {
        const w = mountMenu({ choice: { busy_until: null, appear_offline: true } });
        await openMenu(w);

        expect(checked()).toEqual(["appear_offline"]);
    });

    test("sits on Until I turn it off for a Busy with no end", async () => {
        const w = mountMenu({ choice: { busy_until: "infinity", appear_offline: false } });
        await openMenu(w);

        expect(checked()).toEqual(["busy-off"]);
    });

    test("sits on the morning item for a Busy that ends at the next 8:00", async () => {
        const w = mountMenu({ choice: { busy_until: new Date(2026, 10, 11, 8, 0).toISOString(), appear_offline: false } });
        await openMenu(w);

        expect(checked()).toEqual(["busy-morning"]);
        expect(keys()).toHaveLength(6);
    });

    test("is a row of its own, first among the ends, for any other end, since an end cannot be read back as a length", async () => {
        jest.setSystemTime(new Date(2026, 10, 10, 12, 0));
        const end = new Date(2026, 10, 10, 15, 0).toISOString();
        const w = mountMenu({ choice: { busy_until: end, appear_offline: false } });
        await openMenu(w);

        expect(keys()).toEqual(["auto", "busy-current", "busy-1h", "busy-2h", "busy-morning", "busy-off", "appear_offline"]);
        expect(checked()).toEqual(["busy-current"]);
        expect(titleOf(itemOf("busy-current"))).toBe("Until 3:00 PM");
        expect(nameOf(itemOf("busy-current"))).toBe("Busy until 3:00 PM");

        // choosing it keeps the end as it stands
        itemOf("busy-current").click();
        await settle();
        expect(chosen(w)).toEqual({ busy_until: end, appear_offline: false });
    });

    test("sits on Automatic once a Busy's end has passed", async () => {
        const w = mountMenu({ choice: { busy_until: new Date(2026, 10, 10, 20, 0).toISOString(), appear_offline: false } });
        await openMenu(w);

        expect(checked()).toEqual(["auto"]);
        expect(keys()).toHaveLength(6);
    });

    test("follows the choice it is given, not the item pressed last, so a choice not yet shown leaves no check behind", async () => {
        const w = mountMenu();
        await openMenu(w);
        itemOf("busy-1h").click();
        await settle();

        // her settings still say Automatic, as they do after a refusal put it back
        await openMenu(w);
        expect(checked()).toEqual(["auto"]);

        // another tab's choice reaches her settings: the check moves to it
        await w.setProps({ choice: { busy_until: "infinity", appear_offline: false } });
        await press("Escape");
        await openMenu(w);
        expect(checked()).toEqual(["busy-off"]);
    });
});

describe("a choice", () => {
    test.each([
        ["auto", { busy_until: null, appear_offline: false }],
        ["busy-1h", { busy_until: new Date(2026, 10, 10, 22, 0).toISOString(), appear_offline: false }],
        ["busy-2h", { busy_until: new Date(2026, 10, 10, 23, 0).toISOString(), appear_offline: false }],
        ["busy-morning", { busy_until: new Date(2026, 10, 11, 8, 0).toISOString(), appear_offline: false }],
        ["busy-off", { busy_until: "infinity", appear_offline: false }],
        ["appear_offline", { busy_until: null, appear_offline: true }]
    ])("%s is told to the host with its end as a time, and closes the menu", async (key, value) => {
        const w = mountMenu({ choice: key === "auto" ? { busy_until: "infinity", appear_offline: false } : automatic });
        await openMenu(w);

        itemOf(key).click();
        await settle();

        expect(chosen(w)).toEqual(value);
        expect(menu()).toBeNull();
    });

    test("works its end out again at the choice", async () => {
        const w = mountMenu();
        await openMenu(w);

        jest.setSystemTime(new Date(2026, 10, 10, 21, 10));
        itemOf("busy-1h").click();
        await settle();

        expect(chosen(w)).toEqual({ busy_until: new Date(2026, 10, 10, 22, 10).toISOString(), appear_offline: false });
    });

    // Her Busy's end is an instant the server holds her to, and her face is drawn by the server's
    // clock, so the menu reads that clock too: a page clock ten minutes slow names no end the
    // server has passed, and a length is counted from the server's now, never refused as passed.
    test("reads her Busy and counts its lengths by the server's clock the status store reads, not the page's", async () => {
        const serverAhead = 600_000;
        const w = mountMenu({ choice: { busy_until: new Date(evening.getTime() + 300_000).toISOString(), appear_offline: false } }, () => null, () => Date.now() + serverAhead);
        await openMenu(w);

        expect(checked()).toEqual(["auto"]);
        expect(keys()).not.toContain("busy-current");

        itemOf("busy-1h").click();
        await settle();
        expect(chosen(w)).toEqual({ busy_until: new Date(evening.getTime() + serverAhead + 3_600_000).toISOString(), appear_offline: false });
    });

    test("of Busy while Busy takes the new end", async () => {
        const w = mountMenu({ choice: { busy_until: "infinity", appear_offline: false } });
        await openMenu(w);

        itemOf("busy-2h").click();
        await settle();

        expect(chosen(w)).toEqual({ busy_until: new Date(2026, 10, 10, 23, 0).toISOString(), appear_offline: false });
    });
});

describe("the keyboard", () => {
    test("opens on the Busy chosen", async () => {
        const w = mountMenu({ choice: { busy_until: "infinity", appear_offline: false } });
        await openMenu(w);

        expect(document.activeElement).toBe(itemOf("busy-off"));
    });

    // Opened on Busy, Enter, Enter from the face set Busy unasked; on what is checked, Enter keeps it.
    test("opens on what is checked: Appear offline while she appears offline, Automatic while neither", async () => {
        const w = mountMenu({ choice: { busy_until: null, appear_offline: true } });
        await openMenu(w);

        expect(document.activeElement).toBe(itemOf("appear_offline"));

        await press("Escape");
        await w.setProps({ choice: { busy_until: null, appear_offline: false } });
        await openMenu(w);

        expect(document.activeElement).toBe(itemOf("auto"));
    });

    test("moves with the arrows, wraps, and Escape returns focus to the button", async () => {
        const w = mountMenu();
        await openMenu(w);

        // open on Automatic, checked; the shortest Busy is next
        await press("ArrowDown");
        expect(document.activeElement).toBe(itemOf("busy-1h"));
        await press("ArrowUp");
        expect(document.activeElement).toBe(itemOf("auto"));
        await press("ArrowUp");
        expect(document.activeElement).toBe(itemOf("appear_offline"));

        await press("Escape");
        expect(menu()).toBeNull();
        expect(document.activeElement).toBe(trigger(w));
    });

    test("chooses with Enter", async () => {
        const w = mountMenu();
        await openMenu(w);

        // from Automatic, checked, down past the hours to the morning end
        await press("ArrowDown");
        await press("ArrowDown");
        await press("ArrowDown");
        await press("Enter");

        expect(chosen(w)).toEqual({ busy_until: new Date(2026, 10, 11, 8, 0).toISOString(), appear_offline: false });
    });
});

// A choice whose save keeps failing shows on her faces as though saved, so the menu she chose it
// from says it is not, its items still open: a newer choice waits the same retry.
describe("while her choice keeps failing to save", () => {
    const retrying = "Not saved yet. Trying again.";

    test.each(["change", "face"])("the %s button's menu says so as its note, every item still open", async (kind) => {
        const w = mountMenu({ trigger: kind, isRetrying: true });
        await openMenu(w);

        expect(items().some(item => item.hasAttribute("aria-disabled"))).toBe(false);
        const note = menu()!.querySelector("[data-testid='menu-note']")!;
        expect(plain(note.textContent!)).toBe(retrying);
        expect(document.getElementById(menu()!.getAttribute("aria-describedby") ?? "")).toBe(note);
        // an alert, not the lock that says nothing can be chosen
        expect(note.querySelector("svg circle")).not.toBeNull();
        expect(note.querySelector("svg rect")).toBeNull();
    });

    test("an open menu says it where it stands and drops it once the save lands or is refused", async () => {
        const w = mountMenu();
        await openMenu(w);

        await w.setProps({ isRetrying: true });
        await settle();
        expect(plain(menu()!.querySelector("[data-testid='menu-note']")!.textContent!)).toBe(retrying);

        await w.setProps({ isRetrying: false });
        await settle();
        expect(menu()!.querySelector("[data-testid='menu-note']")).toBeNull();
    });

    test("read only's words win, since nothing in the menu can be chosen", async () => {
        const w = mountMenu({ isRetrying: true, closedReason: "Cedar Hill's chat is read only until 9." });
        await openMenu(w);

        expect(plain(menu()!.querySelector("[data-testid='menu-note']")!.textContent!)).toBe("Cedar Hill's chat is read only until 9.");
    });
});

describe("while the church is read only", () => {
    test.each(["change", "face"])("the %s button opens the menu as ever, its items drawn but never chosen and the banner's words as its note", async (kind) => {
        const w = mountMenu({ trigger: kind, closedReason: reason });

        const button = trigger(w);
        // a live button, on a phone too, where no tooltip is ever shown
        expect(button.hasAttribute("aria-disabled")).toBe(false);
        // the face names itself in its tooltip; Change says its words and is named in full
        expect(kind === "face" ? w.findComponent(IconButton).props("tooltip") : button.getAttribute("aria-label")).toBe(kind === "face" ? "Set status" : "Change your status");
        await openMenu(w);

        expect(items().map(item => item.getAttribute("aria-disabled"))).toEqual(Array(6).fill("true"));
        expect(checked()).toEqual(["auto"]);
        const note = menu()!.querySelector("[data-testid='menu-note']")!;
        expect(plain(note.textContent!)).toBe(reason);
        // said as the menu opens, not only seen at its foot
        expect(document.getElementById(menu()!.getAttribute("aria-describedby") ?? "")).toBe(note);

        itemOf("busy-1h").click();
        await settle();
        await press("Enter");
        expect(w.emitted("choose")).toBeUndefined();
        expect(menu()).not.toBeNull();
    });

    test("an open menu turns read only where it stands, and back", async () => {
        const w = mountMenu();
        await openMenu(w);

        await w.setProps({ closedReason: reason });
        await settle();
        expect(menu()).not.toBeNull();
        expect(items().every(item => item.getAttribute("aria-disabled") === "true")).toBe(true);
        expect(plain(menu()!.querySelector("[data-testid='menu-note']")!.textContent!)).toBe(reason);

        await w.setProps({ closedReason: null });
        await settle();
        expect(items().some(item => item.hasAttribute("aria-disabled"))).toBe(false);
        expect(menu()!.querySelector("[data-testid='menu-note']")).toBeNull();
        expect(menu()!.hasAttribute("aria-describedby")).toBe(false);
        itemOf("busy-off").click();
        await settle();
        expect(chosen(w)).toEqual({ busy_until: "infinity", appear_offline: false });
    });

    test("leaves her face as it is, never faded", () => {
        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/statusMenu.partial.obs"), "utf8");
        const style = source.slice(source.indexOf("<style"), source.indexOf("</style>"));

        expect(style).not.toMatch(/opacity/);
        expect(style).not.toMatch(/aria-disabled/);
    });
});

describe("its triggers", () => {
    // Change was an icon button restyled from outside into a pill; it is the one small line
    // button every other row's words use, and the face a trigger the menu draws as its own.
    test("draws Change as the shared small line button with its word", () => {
        const w = mountMenu({ trigger: "change" });
        const change = w.findComponent(Button);

        expect(change.exists()).toBe(true);
        expect(change.props("size")).toBe("sm");
        expect(change.props("kind")).toBe("line");
        expect(change.text()).toBe("Change");
        expect(change.element).toBe(trigger(w));
        expect(w.findComponent(IconButton).exists()).toBe(false);
    });

    test("styles its face and its Change itself, reaching into no other part", () => {
        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/statusMenu.partial.obs"), "utf8");
        const style = source.slice(source.indexOf("<style"), source.indexOf("</style>"));

        expect(style).not.toMatch(/:deep\(/);
    });

    // The face's 40px disc and its ring over each fill are the menu's own scoped rules, which
    // reach the button only when the button carries the menu's scope. The test build compiles no
    // scope, so the menu is given one here.
    test("sizes and rings its face by its own rules, which reach the face's button", () => {
        const scoped = StatusMenu as unknown as { __scopeId?: string };
        scoped.__scopeId = "data-v-status";
        try {
            const face = trigger(mountMenu({ trigger: "face" }));

            expect(face.tagName).toBe("BUTTON");
            expect(face.classList).toContain("chat-status-face");
            expect(face.hasAttribute("data-v-status")).toBe(true);
        }
        finally {
            delete scoped.__scopeId;
        }
    });
});

// On a phone Chat settings is a sheet, so its Change turns that sheet into a Status page rather
// than raising a second sheet over it; the page draws the same items in place.
describe("on a phone, inside Chat settings", () => {
    test("Change asks its sheet to turn to Status, opening no menu or sheet of its own", async () => {
        const w = mountMenu({ trigger: "change", isPhone: true });

        expect(trigger(w).hasAttribute("aria-haspopup")).toBe(false);
        expect(trigger(w).getAttribute("aria-label")).toBe("Change your status");
        await openMenu(w);

        expect(w.emitted("turn")).toHaveLength(1);
        expect(menu()).toBeNull();
        expect(document.querySelector("[data-testid='sheet']")).toBeNull();
    });

    test("Change on a desktop opens the menu beside itself and asks for no page", async () => {
        const w = mountMenu({ trigger: "change" });
        await openMenu(w);

        expect(w.emitted("turn")).toBeUndefined();
        expect(keys()).toHaveLength(6);
    });

    test("as the page, draws the same six items in place, checked by her settings, starting on the one checked", () => {
        const w = mountMenu({ trigger: "page", isPhone: true, choice: { busy_until: null, appear_offline: true } });

        expect(w.findComponent(Menu).props("isPage")).toBe(true);
        expect(w.find("[data-testid='status-change']").exists()).toBe(false);
        expect(keys()).toEqual(["auto", "busy-1h", "busy-2h", "busy-morning", "busy-off", "appear_offline"]);
        expect(checked()).toEqual(["appear_offline"]);
        expect(itemOf("appear_offline").hasAttribute("data-autofocus")).toBe(true);
        expect(document.querySelector("[data-testid='sheet']")).toBeNull();
    });

    test("as the page, a choice is told with its end worked out at the choice", async () => {
        const w = mountMenu({ trigger: "page", isPhone: true });

        itemOf("busy-1h").click();
        await settle();

        expect(chosen(w)).toEqual({ busy_until: new Date(evening.getTime() + 3_600_000).toISOString(), appear_offline: false });
    });

    test("as the page while the church is read only, every item is drawn but never chosen, the reason its note", async () => {
        const w = mountMenu({ trigger: "page", isPhone: true, closedReason: reason });

        expect(plain(document.querySelector("[role='menu'] [data-testid='menu-note']")!.textContent!)).toBe(reason);
        itemOf("busy-1h").click();
        await settle();
        expect(w.emitted("choose")).toBeUndefined();
    });
});

describe("the sidebar foot's face", () => {
    const online: FaceStatus = { state: "online", label: ", online", tip: "Online", busyUntil: null, revision: 1 };

    test("is a round face button named by her status, carrying her own badge", async () => {
        const w = mountMenu({ trigger: "face" }, guid => guid === "a0000001-0000-4000-8000-000000000000" ? online : null);

        const button = trigger(w);
        expect(button.getAttribute("aria-label")).toBe("Your status: Online. Set status");
        expect(button.getAttribute("aria-haspopup")).toBe("menu");
        expect(button.querySelectorAll(".chat-av")).toHaveLength(1);
        expect(button.querySelector("[data-testid='status-badge']")?.getAttribute("data-state")).toBe("online");

        await openMenu(w);
        expect(keys()).toHaveLength(6);
    });

    // The button's tip and name already carry it, so resting on the badge shows one tip, not two.
    test("its badge holds no tip of its own inside the button", async () => {
        jest.useFakeTimers();
        try {
            const w = mountMenu({ trigger: "face" }, guid => guid === "a0000001-0000-4000-8000-000000000000" ? online : null);

            trigger(w).querySelector("[data-testid='status-badge']")!.dispatchEvent(new MouseEvent("mouseenter"));
            jest.advanceTimersByTime(1000);
            await nextTick();

            expect(Array.from(document.querySelectorAll("[role='tooltip']")).map(tip => tip.textContent?.trim())).not.toContain("Online");
        }
        finally {
            jest.useRealTimers();
        }
    });

    test("draws her Appear offline as the hollow ring, her own face being the only one that shows it", () => {
        const hidden: FaceStatus = { state: "hidden", label: ", appear offline", tip: "Appear offline", busyUntil: null, revision: 2 };
        const w = mountMenu({ trigger: "face" }, () => hidden);

        expect(trigger(w).querySelector("[data-testid='status-badge']")?.getAttribute("data-state")).toBe("hidden");
        expect(trigger(w).getAttribute("aria-label")).toBe("Your status: Appear offline. Set status");
    });

    test("carries the menu's not saved words in her face's name while a save keeps failing", async () => {
        const hidden: FaceStatus = { state: "hidden", label: ", appear offline", tip: "Appear offline", busyUntil: null, revision: 2 };
        const w = mountMenu({ trigger: "face", isRetrying: true }, () => hidden);

        expect(trigger(w).getAttribute("aria-label")).toBe("Your status: Appear offline, not saved yet. Trying again. Set status");

        await w.setProps({ isRetrying: false });

        expect(trigger(w).getAttribute("aria-label")).toBe("Your status: Appear offline. Set status");
    });

    test("is named Set status alone while her status is not yet known", () => {
        const w = mountMenu({ trigger: "face" });

        expect(trigger(w).getAttribute("aria-label")).toBe("Set status");
        expect(trigger(w).querySelector("[data-testid='status-badge']")).toBeNull();
    });

    test("opens above itself, at the foot of the chat, and the settings group's Change below itself", () => {
        const w = mountMenu({ trigger: "face" });
        expect(w.findComponent(Menu).props("placement")).toBe("top-start");

        wrapper!.unmount();
        const change = mountMenu({ trigger: "change" });
        expect(change.findComponent(Menu).props("placement")).toBe("bottom-end");
    });

    test("opens as a sheet on a phone, as the chat's other menus do", async () => {
        const w = mountMenu({ trigger: "face", isPhone: true });
        await openMenu(w);

        expect(menu()?.closest("[data-testid='sheet']")).not.toBeNull();
    });
});
