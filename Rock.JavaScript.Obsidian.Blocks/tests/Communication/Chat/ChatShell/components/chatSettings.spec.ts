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
// Chat settings, mounted: the person's own settings dialog, opened from the sidebar's foot or their
// own profile card. A grouped list: who they are and where their name and photo come from, then
// Privacy's two switches, then Notifications' browser row in each of push's states and the
// reaction switch, each row a short label with at most a short line under it. Each switch is told
// to the block at once, which saves it; a save that lands shows nothing more, and a refused one
// shows the server's sentence and Try again under its switch.
/* eslint-disable @typescript-eslint/naming-convention */
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import ChatSettings from "../../../../../src/Communication/Chat/ChatShell/components/chatSettings.partial.obs";
import Icon from "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs";

// #region Helpers

let wrapper: VueWrapper | null = null;

const me = { show_profile_details: true, is_open_dm_allowed: false, reaction_push: true };

function mountSettings(props: Record<string, unknown> = {}): VueWrapper {
    wrapper = mount(ChatSettings, {
        props: {
            isOpen: true,
            personName: "Jordan Lee",
            personPhotoUrl: null,
            campusName: "Downtown Campus",
            organizationName: "Cedar Hill",
            me,
            notes: {},
            announcement: null,
            push: "offered",
            ...props
        },
        attachTo: document.body
    });
    return wrapper;
}

function plain(text: string): string {
    return text.replace(/\s+/g, " ").trim();
}

function switchOf(w: VueWrapper, key: string): ReturnType<VueWrapper["get"]> {
    return w.get(`[data-testid='switch-${key}'] [role='switch']`);
}

function hasIcon(w: VueWrapper, inside: { element: Element }, name: string): boolean {
    return w.findAllComponents(Icon).some(icon => icon.props("name") === name && inside.element.contains(icon.element));
}

async function settle(): Promise<void> {
    await nextTick();
    await nextTick();
}

// #endregion Helpers

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
    document.body.innerHTML = "";
});

describe("the dialog", () => {
    test("is a settings dialog named Chat settings, closed from its head", async () => {
        const w = mountSettings();

        const dialog = w.get("[role='dialog']");
        expect(document.getElementById(dialog.attributes("aria-labelledby") ?? "")?.textContent?.trim()).toBe("Chat settings");
        expect(dialog.classes()).toContain("is-panel");

        await w.get("[data-testid='settings-close']").trigger("click");
        expect(w.emitted("close")).toHaveLength(1);
    });

    test("puts focus on Turn on as it opens while this browser's notifications can be turned on", async () => {
        const w = mountSettings({ push: "offered" });
        await settle();

        expect(document.activeElement).toBe(w.get("[data-testid='push-row'] button").element);
    });

    test("otherwise puts focus on its Close button, whose tooltip does not show unasked", async () => {
        const w = mountSettings({ push: "on" });
        await settle();

        expect(document.activeElement).toBe(w.get("[data-testid='settings-close']").element);
        await new Promise(resolve => setTimeout(resolve, 600));
        expect(document.querySelector("[role='tooltip']")).toBeNull();
    });

    test("names the person, their campus, and where their name and photo come from", () => {
        const w = mountSettings();

        expect(w.get("[data-testid='settings-name']").text()).toBe("Jordan Lee");
        expect(w.findAll("[data-testid='settings-meta-item']").map(item => item.text())).toEqual(["Downtown Campus", "From your Cedar Hill profile"]);
        // The dot between them is drawn for the eye, so a screen reader hears the two apart.
        expect(plain(w.get("[data-testid='settings-meta']").text())).toBe("Downtown Campus, From your Cedar Hill profile");
    });

    test("leaves the campus out for a person with none", () => {
        const w = mountSettings({ campusName: null });

        expect(w.findAll("[data-testid='settings-meta-item']").map(item => item.text())).toEqual(["From your Cedar Hill profile"]);
        expect(plain(w.get("[data-testid='settings-meta']").text())).toBe("From your Cedar Hill profile");
    });

    test("keeps its help to a few short lines", () => {
        const w = mountSettings({ push: "blocked" });
        const words = (text: string): number => plain(text).split(" ").filter(word => word !== "").length;

        // the Status group's line is her status as it stands, not help, so it is not counted
        const help = w.findAll(".chat-sg-sub, [data-testid='settings-foot']").filter(line => !line.element.closest("[data-testid='status-row']")).map(line => line.text());
        expect(help.length).toBeGreaterThan(0);
        // 32 words, and the ten of the line under "Show public profile details", which says what it now shares
        expect(help.reduce((sum, line) => sum + words(line), 0)).toBeLessThanOrEqual(42);
    });
});

describe("Privacy", () => {
    test("is a group of the two switches under its label, each as the person's own setting stands", () => {
        const w = mountSettings();

        const group = w.get("[data-testid='settings-privacy'] [role='group']");
        expect(document.getElementById(group.attributes("aria-labelledby") ?? "")?.textContent?.trim()).toBe("Privacy");
        expect(switchOf(w, "profile_details").attributes("aria-checked")).toBe("true");
        expect(switchOf(w, "open_dm").attributes("aria-checked")).toBe("false");
        expect(plain(w.get("[data-testid='switch-profile_details']").text())).toBe("Show public profile details Adds your email, mobile phone and badges to your card.");
        expect(plain(w.get("[data-testid='switch-open_dm']").text())).toBe("Anyone can message me Off: only people in a private channel with you");
        expect(hasIcon(w, w.get("[data-testid='switch-profile_details']"), "id-card")).toBe(true);
        expect(hasIcon(w, w.get("[data-testid='switch-open_dm']"), "message")).toBe(true);
    });

    test("names each switch by its label and describes it by the line under it", () => {
        const w = mountSettings();

        const openDm = switchOf(w, "open_dm");
        expect(document.getElementById(openDm.attributes("aria-labelledby") ?? "")?.textContent).toBe("Anyone can message me");
        expect(document.getElementById(openDm.attributes("aria-describedby") ?? "")?.textContent?.trim()).toBe("Off: only people in a private channel with you");
        const details = switchOf(w, "profile_details");
        expect(document.getElementById(details.attributes("aria-labelledby") ?? "")?.textContent).toBe("Show public profile details");
        expect(document.getElementById(details.attributes("aria-describedby") ?? "")?.textContent?.trim()).toBe("Adds your email, mobile phone and badges to your card.");
    });

    test("a press on a row's words toggles its switch, as a label does", async () => {
        const w = mountSettings();
        // after the dialog has put focus where it opens
        await settle();

        await w.get("[data-testid='switch-open_dm'] .chat-sg-txt").trigger("click");

        expect(w.emitted("toggle")).toEqual([["open_dm", true]]);
        expect(document.activeElement).toBe(switchOf(w, "open_dm").element);
    });

    test("tells the block the setting and its new value", async () => {
        const w = mountSettings();

        await switchOf(w, "profile_details").trigger("click");
        await switchOf(w, "open_dm").trigger("click");

        expect(w.emitted("toggle")).toEqual([["profile_details", false], ["open_dm", true]]);
    });

    test("a refused save's note carries the server's sentence and Try again, which asks again and gives focus back to its switch", async () => {
        const retry = jest.fn();
        const w = mountSettings({ notes: { open_dm: { kind: "error", text: "Chat is not available to you right now.", retry } } });

        const refused = w.get("[data-testid='switch-open_dm'] [data-testid='switch-note']");
        expect(refused.attributes("data-kind")).toBe("error");
        expect(plain(refused.text())).toBe("Chat is not available to you right now. Try again");
        expect(hasIcon(w, refused, "alert")).toBe(true);
        // The switch is described by its note, so a screen reader hears why with the switch.
        expect(document.getElementById(switchOf(w, "open_dm").attributes("aria-describedby")?.split(" ").pop() ?? "")?.textContent).toContain("Chat is not available to you right now.");

        // after the dialog has put focus where it opens
        await settle();
        await refused.get("[data-testid='note-retry']").trigger("click");

        expect(retry).toHaveBeenCalledTimes(1);
        // Try again is not a press on the row, which would toggle the switch too.
        expect(w.emitted("toggle")).toBeUndefined();
        expect(document.activeElement).toBe(switchOf(w, "open_dm").element);
    });
});

// A save that lands shows nothing: the switch already shows it. Nothing in the head, nothing under
// the switch, and the switch never dims while its save is out.
describe("a save that lands", () => {
    test("shows no word in the head and no busy switch", () => {
        const w = mountSettings();

        expect(w.find("[data-testid='settings-save']").exists()).toBe(false);
        expect(w.text()).not.toMatch(/Saving|Saved/);
        expect(w.findAll("[role='switch'][aria-busy]")).toHaveLength(0);
        expect(w.findAll("[data-testid='switch-note']")).toHaveLength(0);
    });
});

describe("what is said aloud", () => {
    test("reads the settings' news in its polite live region, and a failed save in its assertive one", async () => {
        const w = mountSettings();

        await w.setProps({ announcement: { text: "Starred", isAssertive: false, count: 3 } });
        expect(w.get("[data-testid='settings-announcement']").text()).toBe("Starred");
        expect(w.get("[data-testid='settings-alert']").text()).toBe("");

        await w.setProps({ announcement: { text: "Not saved. Chat is read only tonight.", isAssertive: true, count: 4 } });
        expect(w.get("[data-testid='settings-alert']").text()).toBe("Not saved. Chat is read only tonight.");
        expect(w.get("[data-testid='settings-announcement']").text()).toBe("");
    });
});

describe("this browser's notifications", () => {
    // The state in words beside its dot, and a line under the label only where there is something to do.
    const rows: Array<[string, string | null, string | null, string | null]> = [
        ["offered", "Off", null, "Turn on"],
        ["asking", null, "Choose Allow in your browser", "Turn on"],
        ["on", "On", null, "Turn off"],
        ["blocked", "Blocked", "Allow it in browser settings, then reload", null],
        ["unavailable", "Not available", "Not set up for Cedar Hill yet", null]
    ];

    test.each(rows)("in its %s state says so and offers what it can", (state, status, description, action) => {
        const w = mountSettings({ push: state });

        const row = w.get("[data-testid='push-row']");
        expect(row.attributes("data-state")).toBe(state);
        expect(row.get("b").text()).toBe("This browser");
        expect(hasIcon(w, row, "monitor")).toBe(true);
        const statusElement = row.find("[data-testid='push-status']");
        expect(statusElement.exists() ? statusElement.text() : null).toBe(status);
        const desc = row.find(".chat-sg-sub");
        expect(desc.exists() ? desc.text() : null).toBe(description);
        const button = row.find("button");
        expect(button.exists() ? plain(button.text()) : null).toBe(action);
    });

    test("names the browser's Turn on and Turn off for this browser, for a screen reader", () => {
        const offered = mountSettings({ push: "offered" });
        expect(offered.get("[data-testid='push-row'] button").attributes("aria-label")).toBe("Turn on notifications for this browser");
        offered.unmount();

        const on = mountSettings({ push: "on" });
        expect(on.get("[data-testid='push-row'] button").attributes("aria-label")).toBe("Turn off notifications for this browser");
    });

    test("is in the Notifications group with the reaction switch", () => {
        const w = mountSettings();

        const group = w.get("[data-testid='settings-notifications'] [role='group']");
        expect(document.getElementById(group.attributes("aria-labelledby") ?? "")?.textContent?.trim()).toBe("Notifications");
        expect(group.find("[data-testid='push-row']").exists()).toBe(true);
        expect(group.find("[data-testid='switch-reaction_push']").exists()).toBe(true);
    });

    test("Turn on asks the block, and is held while the browser is asking", async () => {
        const w = mountSettings({ push: "offered" });

        await w.get("[data-testid='push-row'] button").trigger("click");
        expect(w.emitted("pushOn")).toHaveLength(1);

        await w.setProps({ push: "asking" });
        expect(w.get("[data-testid='push-row'] button").attributes("disabled")).toBeDefined();
    });

    test("Turn off asks the block", async () => {
        const w = mountSettings({ push: "on" });

        await w.get("[data-testid='push-row'] button").trigger("click");

        expect(w.emitted("pushOff")).toHaveLength(1);
    });

    test("the reaction switch, with a word for a browser that will not show them", async () => {
        const w = mountSettings({ push: "offered" });

        expect(switchOf(w, "reaction_push").attributes("aria-checked")).toBe("true");
        expect(plain(w.get("[data-testid='switch-reaction_push']").text())).toBe("Reactions to my messages Sent to your other devices");
        expect(hasIcon(w, w.get("[data-testid='switch-reaction_push']"), "smile")).toBe(true);

        await w.setProps({ push: "on" });
        expect(plain(w.get("[data-testid='switch-reaction_push']").text())).toBe("Reactions to my messages");

        await switchOf(w, "reaction_push").trigger("click");
        expect(w.emitted("toggle")).toEqual([["reaction_push", false]]);
    });

    test("ends by saying where each conversation's own setting is", () => {
        const w = mountSettings();

        expect(w.get("[data-testid='settings-foot']").text()).toBe("Each conversation sets its own, from its name at the top.");
    });
});

describe("Blocked people", () => {
    const sam = { person_alias_guid: "a0000005-0000-4000-8000-000000000000", nick_name: "Sam", last_name: "Ortiz", avatar_url: null };
    const gone = { person_alias_guid: "a0000006-0000-4000-8000-000000000000", nick_name: null, last_name: null, avatar_url: null };
    const list = (extra: Record<string, unknown> = {}): Record<string, unknown> => ({ people: [sam, gone], listState: "loaded", busy: {}, notes: {}, ...extra });

    test("lists each person blocked, one the church no longer lists as someone no longer in chat", () => {
        const w = mountSettings({ blocked: list() });

        const rows = w.findAll("[data-testid='blocked-row']").map(r => plain(r.get(".chat-sg-title").text()));
        expect(rows).toEqual(["Sam Ortiz", "Someone no longer in chat"]);
        expect(w.findAll("[data-testid='blocked-unblock']").map(b => b.attributes("aria-label"))).toEqual(["Unblock Sam Ortiz", "Unblock Someone no longer in chat"]);
    });

    test("Unblock is one tap, with no confirmation, since everything comes back as it was", async () => {
        const w = mountSettings({ blocked: list() });

        await w.findAll("[data-testid='blocked-unblock']")[0].trigger("click");

        expect(w.emitted("unblock")).toEqual([[sam]]);
        expect(document.querySelector("[role='alertdialog']")).toBeNull();
    });

    test("an unblock that lands says so and moves focus to the next row's Unblock, else the one before, else the empty line", async () => {
        const ben = { person_alias_guid: "a0000008-0000-4000-8000-000000000000", nick_name: "Ben", last_name: "Okafor", avatar_url: null };
        const w = mountSettings({ blocked: list({ people: [sam, ben, gone] }) });
        // the dialog's own first focus, after its open
        await nextTick();
        const said = (): string => plain(w.get("[data-testid='settings-announcement']").text());
        const unblock = async (index: number): Promise<void> => {
            const button = w.findAll<HTMLButtonElement>("[data-testid='blocked-unblock']")[index];
            button.element.focus();
            await button.trigger("click");
        };

        await unblock(0);
        await w.setProps({ blocked: list({ people: [ben, gone] }) });
        expect(said()).toBe("Unblocked Sam Ortiz");
        expect(document.activeElement?.getAttribute("aria-label")).toBe("Unblock Ben Okafor");

        await unblock(1);
        await w.setProps({ blocked: list({ people: [ben] }) });
        expect(said()).toBe("Unblocked Someone no longer in chat");
        expect(document.activeElement?.getAttribute("aria-label")).toBe("Unblock Ben Okafor");

        await unblock(0);
        await w.setProps({ blocked: list({ people: [] }) });
        expect(said()).toBe("Unblocked Ben Okafor");
        expect(document.activeElement).toBe(w.get("[data-testid='blocked-empty']").element);
    });

    test("two unblocks out at once are each said as they land, and focus moves only once its own row has gone", async () => {
        const ben = { person_alias_guid: "a0000008-0000-4000-8000-000000000000", nick_name: "Ben", last_name: "Okafor", avatar_url: null };
        const w = mountSettings({ blocked: list({ people: [sam, ben, gone] }) });
        await nextTick();
        const said = (): string => plain(w.get("[data-testid='settings-announcement']").text());
        const buttons = w.findAll<HTMLButtonElement>("[data-testid='blocked-unblock']");
        buttons[0].element.focus();
        await buttons[0].trigger("click");
        buttons[1].element.focus();
        await buttons[1].trigger("click");

        await w.setProps({ blocked: list({ people: [ben, gone] }) });
        expect(said()).toBe("Unblocked Sam Ortiz");
        expect(document.activeElement?.getAttribute("aria-label")).toBe("Unblock Ben Okafor");

        await w.setProps({ blocked: list({ people: [gone] }) });
        expect(said()).toBe("Unblocked Ben Okafor");
        expect(document.activeElement?.getAttribute("aria-label")).toBe("Unblock Someone no longer in chat");
    });

    test("two unblocks landing together are both said", async () => {
        const ben = { person_alias_guid: "a0000008-0000-4000-8000-000000000000", nick_name: "Ben", last_name: "Okafor", avatar_url: null };
        const w = mountSettings({ blocked: list({ people: [sam, ben, gone] }) });
        await nextTick();
        const buttons = w.findAll<HTMLButtonElement>("[data-testid='blocked-unblock']");
        await buttons[0].trigger("click");
        buttons[1].element.focus();
        await buttons[1].trigger("click");

        await w.setProps({ blocked: list({ people: [gone] }) });

        expect(plain(w.get("[data-testid='settings-announcement']").text())).toBe("Unblocked Sam Ortiz. Unblocked Ben Okafor");
        expect(document.activeElement?.getAttribute("aria-label")).toBe("Unblock Someone no longer in chat");
    });

    test("an unblock refused leaves the row, says nothing and keeps focus on its button", async () => {
        const w = mountSettings({ blocked: list() });
        await nextTick();
        const button = w.findAll<HTMLButtonElement>("[data-testid='blocked-unblock']")[0];
        button.element.focus();
        await button.trigger("click");

        await w.setProps({ blocked: list({ notes: { [sam.person_alias_guid]: { kind: "error", text: "Couldn't save." } } }) });

        expect(plain(w.get("[data-testid='settings-announcement']").text())).toBe("");
        expect(document.activeElement).toBe(button.element);
    });

    test("an unblock still out holds its button, and a refused one shows its note on the row", () => {
        const w = mountSettings({ blocked: list({ busy: { [sam.person_alias_guid]: true }, notes: { [gone.person_alias_guid]: { kind: "error", text: "Couldn't save." } } }) });

        expect(w.findAll("[data-testid='blocked-unblock']")[0].attributes("disabled")).toBeDefined();
        expect(w.findAll("[data-testid='blocked-row']")[1].text()).toContain("Couldn't save.");
    });

    test("says so when nobody is blocked, while the list loads, and when it failed, with Try again", async () => {
        expect(plain(mountSettings({ blocked: list({ people: [] }) }).get("[data-testid='blocked-empty']").text())).toBe("You haven't blocked anyone.");
        wrapper?.unmount();
        expect(mountSettings({ blocked: list({ listState: "loading" }) }).find("[data-testid='blocked-loading']").exists()).toBe(true);
        wrapper?.unmount();

        const failed = mountSettings({ blocked: list({ listState: "failed" }) });
        await failed.get("[data-testid='blocked-failed'] button").trigger("click");
        expect(failed.emitted("retryBlocked")).toHaveLength(1);
    });
});

// Her status is a group of its own, first under who she is, since it changes the badge on the face
// above it and is not one conversation's setting. One row says it as her own words say it, and its
// Change opens the same menu the sidebar foot's face opens. The group stays with the online switch
// off, every choice in it, since a Busy still pauses her notifications and Appear offline still
// hides her typing. While the church is read only, Change stays, marked disabled with the banner's
// words, since a choice could only be refused.
describe("Status", () => {
    const reason = "Cedar Hill's chat is read only until 9.";

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

    afterEach(() => {
        jest.useRealTimers();
    });

    function groups(w: VueWrapper): string[] {
        return w.findAll("[data-testid^='settings-']").map(group => group.attributes("data-testid") ?? "").filter(id => ["settings-status", "settings-privacy", "settings-notifications", "settings-blocked"].includes(id));
    }

    function statusLine(w: VueWrapper): string {
        return plain(w.get("[data-testid='status-row'] .chat-sg-sub").text());
    }

    async function openChange(w: VueWrapper): Promise<void> {
        (w.get("[data-testid='status-change']").element as HTMLElement).click();
        await settle();
    }

    function menuKeys(): string[] {
        return Array.from(document.querySelectorAll<HTMLElement>("[role='menu'] [role='menuitemradio']")).map(item => item.dataset.key ?? "");
    }

    test("is the first group, right under who she is", () => {
        const w = mountSettings();

        expect(groups(w)).toEqual(["settings-status", "settings-privacy", "settings-notifications", "settings-blocked"]);
        const id = w.get(".chat-ms-id").element;
        expect(id.nextElementSibling).toBe(w.get("[data-testid='settings-status']").element);
    });

    test.each([
        ["Automatic", { busy_until: null, appear_offline: false }],
        ["Busy, notifications paused until 3:00 PM", { busy_until: new Date(2026, 10, 10, 15, 0).toISOString(), appear_offline: false }],
        ["Busy, notifications paused", { busy_until: "infinity", appear_offline: false }],
        ["Appear offline", { busy_until: null, appear_offline: true }],
        ["Automatic", { busy_until: new Date(2026, 10, 10, 11, 0).toISOString(), appear_offline: false }]
    ])("says %s as her own words say it", (words, statusChoice) => {
        jest.useFakeTimers({ now: new Date(2026, 10, 10, 12, 0), doNotFake: ["nextTick", "queueMicrotask", "setImmediate", "setTimeout", "clearTimeout", "setInterval", "clearInterval", "requestAnimationFrame", "cancelAnimationFrame"] });
        const w = mountSettings({ statusChoice });

        expect(statusLine(w)).toBe(words);
    });

    test("Change opens the status menu, and a choice is told to the block", async () => {
        const w = mountSettings({ statusChoice: { busy_until: null, appear_offline: false } });

        const change = w.get("[data-testid='status-change']");
        expect(plain(change.text())).toBe("Change");
        expect(change.attributes("aria-haspopup")).toBe("menu");
        await openChange(w);
        expect(menuKeys()).toEqual(["auto", "busy-morning", "busy-1h", "busy-2h", "busy-off", "appear_offline"]);

        (document.querySelector<HTMLElement>("[role='menu'] [data-key='appear_offline']"))!.click();
        await settle();
        expect(w.emitted("status")).toEqual([[{ busy_until: null, appear_offline: true }]]);
    });

    test("reads no online switch, so with it off the group stays with every choice in it", async () => {
        const w = mountSettings({ statusChoice: { busy_until: null, appear_offline: false } });

        expect(Object.keys(w.props())).not.toContain("isStatusShown");

        expect(w.find("[data-testid='settings-status']").exists()).toBe(true);
        await openChange(w);
        expect(menuKeys()).toEqual(["auto", "busy-morning", "busy-1h", "busy-2h", "busy-off", "appear_offline"]);
    });

    test("while the church is read only, Change stays, marked disabled with the banner's words, and opens nothing", async () => {
        const w = mountSettings({ statusChoice: { busy_until: null, appear_offline: false }, statusClosedReason: reason });

        const change = w.get("[data-testid='status-change']").element as HTMLElement;
        expect(change.getAttribute("aria-disabled")).toBe("true");
        expect(document.getElementById(change.getAttribute("aria-describedby") ?? "")?.textContent?.trim()).toBe(reason);
        await openChange(w);
        expect(document.querySelector("[role='menu']")).toBeNull();

        await w.setProps({ statusClosedReason: null });
        await openChange(w);
        expect(document.querySelector("[role='menu']")).not.toBeNull();
    });
});
