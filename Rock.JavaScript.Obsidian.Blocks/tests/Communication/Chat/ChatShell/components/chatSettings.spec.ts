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
import { defineComponent, nextTick } from "vue";
import ChatSettings from "../../../../../src/Communication/Chat/ChatShell/components/chatSettings.partial.obs";
import { Politeness, useAnnouncer } from "../../../../../src/Communication/Chat/ChatShell/ui/announcer.partial";
import Button from "../../../../../src/Communication/Chat/ChatShell/ui/button.partial.obs";
import Icon from "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs";
import Avatar from "../../../../../src/Communication/Chat/ChatShell/ui/avatar.partial.obs";
import SettingGroup from "../../../../../src/Communication/Chat/ChatShell/ui/settingGroup.partial.obs";
import Skeleton from "../../../../../src/Communication/Chat/ChatShell/ui/skeleton.partial.obs";
import { readFileSync } from "fs";
import { join } from "path";
import { badgesIn, createStatusStore, provideStatus, setsBothRings, shellFolder, StatusStore, viewer } from "../statusStore";
import { providedSession } from "../sessionShapes";

// #region Helpers

let wrapper: VueWrapper | null = null;

const me = { show_profile_details: true, is_open_dm_allowed: false, reaction_push: true };

/**
 * Mounts the dialog; on a phone, as the session reads its width, it rises as a sheet. The blocked
 * list's rows leave as they do in a browser, by its own transition, which the empty page follows.
 */
function mountSettings(props: Record<string, unknown> = {}, isPhone = false): VueWrapper {
    wrapper = mount(ChatSettings, {
        global: { stubs: { "transition-group": false }, ...(isPhone ? { provide: providedSession({ isPhone }) } : {}) },
        props: {
            isOpen: true,
            personName: "Jordan Lee",
            personPhotoUrl: null,
            campusName: "Downtown Campus",
            organizationName: "Cedar Hill",
            me,
            notes: {},
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

/** What the chat's one voice last said, of a politeness, from inside the open dialog. */
function said(politeness: Politeness = "polite"): string {
    return plain(document.querySelector(`[role='dialog'] [data-testid='announcer-${politeness}']`)?.textContent ?? "");
}

/** Says a sentence through the chat's one voice, as the settings' saves do from the shell. */
function speak(text: string, politeness: Politeness = "polite"): void {
    mount(defineComponent({
        setup() {
            useAnnouncer().announce(text, politeness);
            return () => null;
        }
    })).unmount();
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

/** Waits out a list row's Leave motion: Vue ends a leave a frame after its last class goes on. */
async function leaveEnds(): Promise<void> {
    for (let frame = 0; frame < 3; frame++) {
        await new Promise<void>(resolve => requestAnimationFrame(() => resolve()));
    }
    await settle();
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

    test("stands centred in the frame, as every other dialog does", () => {
        const w = mountSettings();

        expect(w.get("[data-testid='dialog-scrim']").classes()).not.toContain("is-placed-top");
    });

    test("names the person and their campus, and not where the name comes from, since nothing here can change it", () => {
        const w = mountSettings();

        expect(w.get("[data-testid='settings-name']").text()).toBe("Jordan Lee");
        expect(plain(w.get("[data-testid='settings-meta']").text())).toBe("Downtown Campus");
        expect(w.text()).not.toContain("From your Cedar Hill profile");
    });

    test("draws no line under the name for a person with no campus", () => {
        const w = mountSettings({ campusName: null });

        expect(w.find("[data-testid='settings-meta']").exists()).toBe(false);
    });

    test("keeps a helper line only where a person would otherwise be surprised, and no other", () => {
        // Blocked is the one browser state whose line is the only way out, so every kept line shows.
        const w = mountSettings({ push: "blocked" });

        // the Status group's line is her status as it stands, not help, so it is not counted
        const help = w.findAll(".chat-sg-sub").filter(line => !line.element.closest("[data-testid='status-row']")).map(line => plain(line.text()));
        expect(help).toEqual([
            "Shows your email, mobile phone and badges on your card",
            "When off, only people in a private channel with you",
            "Allow it in browser settings, then reload"
        ]);
        expect(w.find("[data-testid='settings-foot']").exists()).toBe(false);
    });

    test("draws every button with the shared button part, small as a row's are, and none of its own", () => {
        const pushButton = (w: VueWrapper): ReturnType<VueWrapper["getComponent"]> | undefined => w.findAllComponents(Button).find(button => !!button.element.closest("[data-testid='push-row']"));
        const offered = mountSettings({ push: "offered" });
        expect(pushButton(offered)?.props("kind")).toBe("brand");
        expect(pushButton(offered)?.props("size")).toBe("sm");
        expect(offered.find(".chat-ms-btn").exists()).toBe(false);
        offered.unmount();

        const on = mountSettings({ push: "on" });
        expect(pushButton(on)?.props("kind")).toBe("line");
        expect(pushButton(on)?.props("size")).toBe("sm");
        // every button is a part's: the button, the icon button, a switch, or a whole settings row
        expect(on.findAll("button").every(button => ["chat-btn", "chat-icon-btn", "chat-switch", "chat-sg-row"].some(part => button.classes(part)))).toBe(true);
    });
});

describe("Privacy", () => {
    test("is a group of the two switches under its label, each as the person's own setting stands", () => {
        const w = mountSettings();

        const group = w.get("[data-testid='settings-privacy'] [role='group']");
        expect(document.getElementById(group.attributes("aria-labelledby") ?? "")?.textContent?.trim()).toBe("Privacy");
        expect(switchOf(w, "profile_details").attributes("aria-checked")).toBe("true");
        expect(switchOf(w, "open_dm").attributes("aria-checked")).toBe("false");
        expect(plain(w.get("[data-testid='switch-profile_details']").text())).toBe("Show public profile details Shows your email, mobile phone and badges on your card");
        expect(plain(w.get("[data-testid='switch-open_dm']").text())).toBe("Anyone can message me When off, only people in a private channel with you");
        expect(hasIcon(w, w.get("[data-testid='switch-profile_details']"), "id-card")).toBe(true);
        expect(hasIcon(w, w.get("[data-testid='switch-open_dm']"), "message")).toBe(true);
    });

    test("names each switch by its label and describes it by the line under it", () => {
        const w = mountSettings();

        const openDm = switchOf(w, "open_dm");
        expect(document.getElementById(openDm.attributes("aria-labelledby") ?? "")?.textContent).toBe("Anyone can message me");
        expect(document.getElementById(openDm.attributes("aria-describedby") ?? "")?.textContent?.trim()).toBe("When off, only people in a private channel with you");
        const details = switchOf(w, "profile_details");
        expect(document.getElementById(details.attributes("aria-labelledby") ?? "")?.textContent).toBe("Show public profile details");
        expect(document.getElementById(details.attributes("aria-describedby") ?? "")?.textContent?.trim()).toBe("Shows your email, mobile phone and badges on your card");
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
    // The settings' news is the shell's to say; the dialog draws no live region of its own and is
    // where the chat's one voice speaks from while it is open.
    test("reads the settings' news said while it is open politely, and a failed save assertively, from inside it", async () => {
        const w = mountSettings();
        await settle();
        expect(w.findAll("[aria-live]:not([data-testid^='announcer-'])")).toHaveLength(0);

        speak("Starred");
        expect(said()).toBe("Starred");
        expect(said("assertive")).toBe("");

        speak("Not saved. Chat is read only tonight.", "assertive");
        expect(said("assertive")).toBe("Not saved. Chat is read only tonight.");
        expect(said()).not.toBe("Not saved. Chat is read only tonight.");
    });
});

describe("this browser's notifications", () => {
    // The state in words beside its dot, and a line under the label only where there is something to do.
    const rows: Array<[string, string | null, string | null, string | null]> = [
        ["offered", "Off", null, "Turn on"],
        ["asking", null, "Choose Allow in your browser", "Turn on"],
        // from the click until the register settles, with the browser's box closed or never shown
        ["turning_on", null, null, "Turn on"],
        ["on", "On", null, "Turn off"],
        ["blocked", "Blocked", "Allow it in browser settings, then reload", null],
        // A sign-in that ends with the browser session gets none, and nothing to click: a phone-number
        // sign-in has no Keep me logged in box to point at.
        ["not_kept", "Off", "Your sign-in here ends when the browser closes", null],
        // the reason it is not set up is nothing a person can act on
        ["unavailable", "Not available", null, null]
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

        // Held as the button part holds a busy press: it keeps its focus, says it is busy, and passes no click on.
        await w.setProps({ push: "asking" });
        expect(w.get("[data-testid='push-row'] button").attributes("aria-busy")).toBe("true");
        expect(w.find("[data-testid='push-row'] .chat-spinner").exists()).toBe(true);
        await w.get("[data-testid='push-row'] button").trigger("click");
        expect(w.emitted("pushOn")).toHaveLength(1);

        // and held, with its spinner, until the register settles, whether or not the browser asked
        await w.setProps({ push: "turning_on" });
        expect(w.get("[data-testid='push-row'] button").attributes("aria-busy")).toBe("true");
        expect(w.find("[data-testid='push-row'] .chat-spinner").exists()).toBe(true);
        await w.get("[data-testid='push-row'] button").trigger("click");
        expect(w.emitted("pushOn")).toHaveLength(1);
    });

    test("Turn off asks the block", async () => {
        const w = mountSettings({ push: "on" });

        await w.get("[data-testid='push-row'] button").trigger("click");

        expect(w.emitted("pushOff")).toHaveLength(1);
    });

    test("the reaction switch is its title alone, with this browser off or on, since a line beside Off read as a contradiction", async () => {
        const w = mountSettings({ push: "offered" });

        expect(switchOf(w, "reaction_push").attributes("aria-checked")).toBe("true");
        expect(plain(w.get("[data-testid='switch-reaction_push']").text())).toBe("Reactions to my messages");
        expect(hasIcon(w, w.get("[data-testid='switch-reaction_push']"), "smile")).toBe(true);

        await w.setProps({ push: "on" });
        expect(plain(w.get("[data-testid='switch-reaction_push']").text())).toBe("Reactions to my messages");

        await switchOf(w, "reaction_push").trigger("click");
        expect(w.emitted("toggle")).toEqual([["reaction_push", false]]);
    });

    test("says nothing of where each conversation's own setting lives, which is not here", () => {
        const w = mountSettings();

        expect(w.text()).not.toContain("Each conversation sets its own");
    });
});

describe("Blocked people", () => {
    const sam = { person_alias_guid: "a0000005-0000-4000-8000-000000000000", nick_name: "Sam", last_name: "Ortiz", avatar_url: null };
    const gone = { person_alias_guid: "a0000006-0000-4000-8000-000000000000", nick_name: null, last_name: null, avatar_url: null };
    const ben = { person_alias_guid: "a0000008-0000-4000-8000-000000000000", nick_name: "Ben", last_name: "Okafor", avatar_url: null };
    const list = (extra: Record<string, unknown> = {}): Record<string, unknown> => ({ people: [sam, gone], listState: "loaded", busy: {}, notes: {}, ...extra });

    /** The Privacy row that turns the dialog into the page. */
    function privacyRow(w: VueWrapper): ReturnType<VueWrapper["get"]> {
        return w.get("[data-testid='settings-privacy'] [data-testid='blocked-people']");
    }

    /** Turns the dialog to the Blocked people page, from its Privacy row, focused first as a keyboard would be. */
    async function openPage(w: VueWrapper): Promise<void> {
        await settle();
        (privacyRow(w).element as HTMLElement).focus();
        await privacyRow(w).trigger("click");
        await settle();
    }

    /** Presses a row's Unblock, which asks in the row, then the row's own Unblock, which is the yes. */
    async function unblockRow(w: VueWrapper, index: number): Promise<void> {
        const button = w.findAll<HTMLButtonElement>("[data-testid='blocked-unblock']")[index];
        button.element.focus();
        await button.trigger("click");
        await settle();
        const yes = w.get<HTMLButtonElement>("[data-testid='blocked-ask-yes']");
        yes.element.focus();
        await yes.trigger("click");
        await settle();
    }

    test("is a row under Privacy with how many are blocked and a chevron, not a group of its own", () => {
        const w = mountSettings({ blocked: list() });

        expect(w.find("[data-testid='settings-blocked']").exists()).toBe(false);
        const row = privacyRow(w);
        expect(row.element.tagName).toBe("BUTTON");
        expect(row.get(".chat-sg-title").text()).toBe("Blocked people");
        expect(plain(row.get("[data-testid='setting-value']").text())).toBe("2");
        expect(hasIcon(w, row, "ban")).toBe(true);
        expect(hasIcon(w, row, "right")).toBe(true);
        expect(w.find("[data-testid='blocked-row']").exists()).toBe(false);
    });

    test("says None on the row when nobody is blocked, and no count before the list is read", () => {
        expect(plain(privacyRow(mountSettings({ blocked: list({ people: [] }) })).get("[data-testid='setting-value']").text())).toBe("None");
        wrapper?.unmount();

        expect(plain(privacyRow(mountSettings({ blocked: list({ listState: "loading" }) })).get("[data-testid='setting-value']").text())).toBe("");
    });

    test("keeps its chevron while the list loads and after it failed, since it still leads to the page", () => {
        for (const listState of ["loading", "failed"]) {
            const w = mountSettings({ blocked: list({ listState }) });
            expect(hasIcon(w, privacyRow(w), "right")).toBe(true);
            w.unmount();
        }
    });

    test("turns the one dialog into the Blocked people page, with Back and its title, focus on the page itself; Escape returns to the row", async () => {
        const w = mountSettings({ blocked: list() });
        await openPage(w);

        expect(document.querySelectorAll("[role='dialog']")).toHaveLength(1);
        expect(w.get("[data-testid='page-title']").text()).toBe("Blocked people");
        // focus is on the page itself, named by its title, so a screen reader says where the person is
        expect(document.activeElement).toBe(document.querySelector("[role='group'][aria-label='Blocked people']"));
        expect(w.findAll("[data-testid='blocked-row']").map(r => plain(r.get(".chat-sg-title").text()))).toEqual(["Sam Ortiz", "Someone no longer in chat"]);

        await w.get("[role='dialog']").trigger("keydown", { key: "Escape" });
        await settle();
        expect(w.find("[data-testid='page-title']").exists()).toBe(false);
        expect(w.emitted("close")).toBeUndefined();
        expect(document.activeElement).toBe(privacyRow(w).element);
    });

    test("draws each person with their face and Unblock, one the church no longer lists as someone no longer in chat", async () => {
        const w = mountSettings({ blocked: list() });
        await openPage(w);

        expect(w.findAll("[data-testid='blocked-row']").every(row => row.findComponent(Avatar).exists())).toBe(true);
        expect(w.findAll("[data-testid='blocked-unblock']").map(b => b.attributes("aria-label"))).toEqual(["Unblock Sam Ortiz", "Unblock Someone no longer in chat"]);
        expect(w.findAllComponents(Button).filter(b => !!b.element.closest("[data-testid='blocked-row']")).every(b => b.props("size") === "sm")).toBe(true);
    });

    test("Unblock asks inside the row first, naming what changes, and tells the block nothing until its yes", async () => {
        const w = mountSettings({ blocked: list() });
        await openPage(w);

        await w.findAll("[data-testid='blocked-unblock']")[0].trigger("click");
        await settle();

        expect(w.emitted("unblock")).toBeUndefined();
        const asking = w.findAll("[data-testid='blocked-row']")[0];
        expect(plain(asking.get(".chat-sg-title").text())).toBe("Unblock Sam?");
        expect(plain(asking.get(".chat-sg-sub").text())).toBe("They can message you, and you'll see their messages.");
        expect(plain(asking.get("[data-testid='blocked-ask-no']").text())).toBe("Cancel");
        expect(plain(asking.get("[data-testid='blocked-ask-yes']").text())).toBe("Unblock");
        // a stray Enter keeps them blocked
        expect(document.activeElement).toBe(asking.get("[data-testid='blocked-ask-no']").element);
        // no dialog over the dialog
        expect(document.querySelectorAll("[role='dialog'], [role='alertdialog']")).toHaveLength(1);

        await asking.get("[data-testid='blocked-ask-yes']").trigger("click");
        expect(w.emitted("unblock")).toEqual([[sam]]);
    });

    test("Cancel, or Escape in the row, keeps them blocked and gives focus back to the row's Unblock", async () => {
        const w = mountSettings({ blocked: list() });
        await openPage(w);

        await w.findAll("[data-testid='blocked-unblock']")[0].trigger("click");
        await settle();
        await w.get("[data-testid='blocked-ask-no']").trigger("click");
        await settle();
        expect(w.find("[data-testid='blocked-ask-yes']").exists()).toBe(false);
        expect(document.activeElement).toBe(w.findAll("[data-testid='blocked-unblock']")[0].element);

        await w.findAll("[data-testid='blocked-unblock']")[1].trigger("click");
        await settle();
        await w.get("[data-testid='blocked-ask-no']").trigger("keydown", { key: "Escape" });
        await settle();
        expect(w.find("[data-testid='blocked-ask-yes']").exists()).toBe(false);
        // the page stays: Escape was the row's
        expect(w.get("[data-testid='page-title']").text()).toBe("Blocked people");
        expect(document.activeElement).toBe(w.findAll("[data-testid='blocked-unblock']")[1].element);
        expect(w.emitted("unblock")).toBeUndefined();
    });

    test("one row asks at a time, and leaving the page drops the question", async () => {
        const w = mountSettings({ blocked: list() });
        await openPage(w);

        await w.findAll("[data-testid='blocked-unblock']")[0].trigger("click");
        await settle();
        await w.findAll("[data-testid='blocked-unblock']")[0].trigger("click");
        await settle();
        expect(w.findAll("[data-testid='blocked-ask-yes']")).toHaveLength(1);
        expect(plain(w.findAll("[data-testid='blocked-row']")[1].get(".chat-sg-title").text())).toBe("Unblock someone no longer in chat?");

        await w.get("[data-testid='page-back']").trigger("click");
        await settle();
        await openPage(w);
        expect(w.find("[data-testid='blocked-ask-yes']").exists()).toBe(false);
    });

    test("after its yes the row's Unblock is held while the unblock is out, with focus on it", async () => {
        const w = mountSettings({ blocked: list() });
        await openPage(w);
        await unblockRow(w, 0);

        expect(w.find("[data-testid='blocked-ask-yes']").exists()).toBe(false);
        expect(document.activeElement).toBe(w.findAll("[data-testid='blocked-unblock']")[0].element);
        await w.setProps({ blocked: list({ busy: { [sam.person_alias_guid]: true } }) });
        const held = w.findAll("[data-testid='blocked-unblock']")[0];
        expect(held.attributes("aria-busy")).toBe("true");
        await held.trigger("click");
        await settle();
        expect(w.find("[data-testid='blocked-ask-yes']").exists()).toBe(false);
        expect(w.emitted("unblock")).toHaveLength(1);
    });

    test("an unblock that lands says so and moves focus to the next row's Unblock, else the one before, else the empty page", async () => {
        const w = mountSettings({ blocked: list({ people: [sam, ben, gone] }) });
        await openPage(w);

        await unblockRow(w, 0);
        await w.setProps({ blocked: list({ people: [ben, gone] }) });
        await settle();
        expect(said()).toBe("Unblocked Sam Ortiz");
        expect(document.activeElement?.getAttribute("aria-label")).toBe("Unblock Ben Okafor");
        await leaveEnds();

        await unblockRow(w, 1);
        await w.setProps({ blocked: list({ people: [ben] }) });
        await settle();
        expect(said()).toBe("Unblocked Someone no longer in chat");
        expect(document.activeElement?.getAttribute("aria-label")).toBe("Unblock Ben Okafor");
        await leaveEnds();

        await unblockRow(w, 0);
        await w.setProps({ blocked: list({ people: [] }) });
        await settle();
        expect(said()).toBe("Unblocked Ben Okafor");
        await leaveEnds();
        expect(document.activeElement).toBe(w.get("[data-testid='blocked-empty']").element);
    });

    // The empty page waits for the last row's Leave motion, so unblocking the only person is seen
    // to go before No one blocked takes its place.
    test("unblocking the only person: the row fades in its list first, then the empty page shows and takes focus", async () => {
        const w = mountSettings({ blocked: list({ people: [sam] }) });
        await openPage(w);
        await unblockRow(w, 0);

        await w.setProps({ blocked: list({ people: [] }) });
        await settle();
        expect(said()).toBe("Unblocked Sam Ortiz");
        expect(w.find("[data-testid='blocked-empty']").exists()).toBe(false);
        expect(w.get("[data-testid='blocked-row']").classes()).toContain("is-leaving");

        await leaveEnds();
        expect(w.find("[data-testid='blocked-row']").exists()).toBe(false);
        expect(document.activeElement).toBe(w.get("[data-testid='blocked-empty']").element);
    });

    // Try again under a refused unblock asks it again through the blocks; the row's unblock is then
    // out again, so the page holds its Unblock, says it once it lands and moves focus as it does the first time.
    test("Try again on a refused unblock is held, said and followed by focus as its first try was", async () => {
        const w = mountSettings({ blocked: list() });
        await openPage(w);
        await unblockRow(w, 0);
        // the blocks drop the note and hold the row as they ask again, before Try again is told on
        const retry = jest.fn(() => void w.setProps({ blocked: list({ busy: { [sam.person_alias_guid]: true } }) }));
        await w.setProps({ blocked: list({ notes: { [sam.person_alias_guid]: { kind: "error", text: "Couldn't save.", retry } } }) });
        await settle();

        const again = w.get<HTMLButtonElement>("[data-testid='blocked-row'] [data-testid='note-retry']");
        again.element.focus();
        await again.trigger("click");
        await settle();
        expect(retry).toHaveBeenCalledTimes(1);
        expect(document.activeElement).toBe(w.findAll("[data-testid='blocked-unblock']")[0].element);

        await w.setProps({ blocked: list({ people: [gone] }) });
        await settle();
        expect(said()).toBe("Unblocked Sam Ortiz");
        expect(document.activeElement?.getAttribute("aria-label")).toBe("Unblock Someone no longer in chat");
    });

    test("two unblocks landing together are both said", async () => {
        const w = mountSettings({ blocked: list({ people: [sam, ben, gone] }) });
        await openPage(w);
        await unblockRow(w, 0);
        await unblockRow(w, 1);

        await w.setProps({ blocked: list({ people: [gone] }) });
        await settle();

        expect(said()).toBe("Unblocked Sam Ortiz. Unblocked Ben Okafor");
        expect(document.activeElement?.getAttribute("aria-label")).toBe("Unblock Someone no longer in chat");
    });

    test("an unblock refused keeps the row with its note, says nothing and puts focus on its Unblock", async () => {
        const w = mountSettings({ blocked: list() });
        await openPage(w);
        await unblockRow(w, 0);

        await w.setProps({ blocked: list({ notes: { [sam.person_alias_guid]: { kind: "error", text: "Couldn't save." } } }) });
        await settle();

        expect(said()).toBe("");
        expect(w.find("[data-testid='blocked-ask-yes']").exists()).toBe(false);
        expect(w.findAll("[data-testid='blocked-row']")[0].text()).toContain("Couldn't save.");
        expect(document.activeElement).toBe(w.findAll("[data-testid='blocked-unblock']")[0].element);
    });

    test("the leaving row leaves by the list's Leave motion, the rows below gliding by the one move", async () => {
        const w = mountSettings({ blocked: list({ people: [sam, ben, gone] }) });
        await openPage(w);

        const list_ = w.get("[data-testid='blocked-list'] [role='list']");
        const group = w.findAllComponents(SettingGroup).find(g => g.element.contains(list_.element));
        expect(group?.props("isList")).toBe(true);
    });

    test("says No one blocked when nobody is, a skeleton of rows while the list loads, and the one voice with Try again when it failed", async () => {
        const empty = mountSettings({ blocked: list({ people: [] }) });
        await openPage(empty);
        expect(plain(empty.get("[data-testid='blocked-empty']").text())).toBe("No one blocked");
        empty.unmount();

        const loading = mountSettings({ blocked: list({ listState: "loading" }) });
        await openPage(loading);
        expect(loading.get("[data-testid='blocked-loading']").attributes("aria-busy")).toBe("true");
        expect(loading.findAllComponents(Skeleton).length).toBeGreaterThan(0);
        expect(loading.find(".chat-skeleton").exists()).toBe(false);
        loading.unmount();

        const failed = mountSettings({ blocked: list({ listState: "failed" }) });
        await openPage(failed);
        expect(plain(failed.get("[data-testid='blocked-failed'] .chat-empty-title").text())).toBe("Couldn't load the people you blocked.");
        await failed.get("[data-testid='blocked-failed'] button").trigger("click");
        expect(failed.emitted("retryBlocked")).toHaveLength(1);
    });

    test("finds where focus goes by the rows' focus marks, never by a test id", () => {
        const source = readFileSync(join(shellFolder, "components", "chatSettings.partial.obs"), "utf8");

        expect(source).not.toMatch(/querySelector(?:All)?\s*(?:<[^>]*>)?\(\s*["'`][^"'`]*data-testid/);
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

    function identity(w: VueWrapper): Element {
        return w.get(".chat-ms-id").element;
    }

    test("the identity draws her own face, its words said by the Status group's line rather than after her name", () => {
        const w = mountSettings({ personAliasGuid: viewer });

        expect(badgesIn(identity(w))).toEqual([{ state: "online", isMoving: false }]);
        expect(w.get("[data-testid='settings-name']").text()).toBe("Jordan Lee");
    });

    test("the identity draws her hollow ring under Appear offline, since it is her own face", async () => {
        const w = mountSettings({ personAliasGuid: viewer });

        status.mine.value = { busy_until: null, appear_offline: true };
        await nextTick();

        expect(badgesIn(identity(w)).map(badge => badge.state)).toEqual(["hidden"]);
    });

    // The line reads her Busy by the store's clock, the server's, as her face does: with the page's
    // clock ten minutes slow an end five minutes ahead of it has passed, and with it ten minutes
    // fast the line moves on at the server's end, ten minutes after the page's.
    test("the Status line reads her Busy by the server's clock the store reads, as her face does", async () => {
        jest.useFakeTimers({ now: new Date(2026, 10, 10, 14, 0) });
        try {
            const end = new Date(2026, 10, 10, 14, 5).toISOString();
            status.store.clockFrom(Date.now() / 1000 + 600);
            const ahead = mountSettings({ personAliasGuid: viewer, statusChoice: { busy_until: end, appear_offline: false } });
            expect(plain(ahead.get("[data-testid='status-row'] .chat-sg-title").text())).toBe("Automatic");
            ahead.unmount();

            status.store.clockFrom(Date.now() / 1000 - 600);
            const behind = mountSettings({ personAliasGuid: viewer, statusChoice: { busy_until: end, appear_offline: false } });
            expect(plain(behind.get("[data-testid='status-row'] .chat-sg-title").text())).toMatch(/^Busy, notifications paused until 2:05\sPM$/u);
            jest.advanceTimersByTime(5 * 60_000 + 1000);
            await nextTick();
            expect(plain(behind.get("[data-testid='status-row'] .chat-sg-title").text())).toMatch(/^Busy/u);
            jest.advanceTimersByTime(10 * 60_000);
            await nextTick();
            expect(plain(behind.get("[data-testid='status-row'] .chat-sg-title").text())).toBe("Automatic");
        }
        finally {
            jest.useRealTimers();
        }
    });

    test("rings its badge in the dialog's fill", () => {
        expect(setsBothRings(join(shellFolder, "components/chatSettings.partial.obs"), /^\.chat-ms-id(\[|:|$)/)).toBe(true);
    });
});

// Her status is a group of its own, first under who she is, since it changes the badge on the face
// above it and is not one conversation's setting. One row says it as her own words say it, and its
// Change opens the same menu the sidebar foot's face opens. The group stays with the online switch
// off, every choice in it, since a Busy still pauses her notifications and Appear offline still
// hides her typing. While the church is read only, Change still opens the menu, every item drawn but
// never chosen and the banner's words as its note, since a choice could only be refused.
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
        return w.findAll("[data-testid^='settings-']").map(group => group.attributes("data-testid") ?? "").filter(id => ["settings-status", "settings-privacy", "settings-notifications"].includes(id));
    }

    function statusLine(w: VueWrapper): string {
        // the line is the row's title: the label above it said only what the line does
        return plain(w.get("[data-testid='status-row'] .chat-sg-title").text());
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

        expect(groups(w)).toEqual(["settings-status", "settings-privacy", "settings-notifications"]);
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
        expect(menuKeys()).toEqual(["auto", "busy-1h", "busy-2h", "busy-morning", "busy-off", "appear_offline"]);

        (document.querySelector<HTMLElement>("[role='menu'] [data-key='appear_offline']"))!.click();
        await settle();
        expect(w.emitted("status")).toEqual([[{ busy_until: null, appear_offline: true }]]);
    });

    // On a phone the dialog is a sheet, and a second sheet over it would hide where she came from:
    // Change turns the one sheet into Status, as the Privacy row turns it into Blocked people.
    test("on a phone, Change turns the one sheet into Status, starting on what is checked; Back returns to Change", async () => {
        const w = mountSettings({ statusChoice: { busy_until: null, appear_offline: true } }, true);
        await settle();
        (w.get("[data-testid='status-change']").element as HTMLElement).focus();
        await openChange(w);

        expect(w.get("[data-testid='dialog-scrim']").classes()).toContain("is-sheet");
        expect(document.querySelectorAll("[role='dialog']")).toHaveLength(1);
        expect(document.querySelector("[data-testid='sheet']")).toBeNull();
        expect(w.get("[data-testid='page-title']").text()).toBe("Status");
        expect(document.querySelector("[role='menu']")?.closest("[role='dialog']")).not.toBeNull();
        expect(menuKeys()).toEqual(["auto", "busy-1h", "busy-2h", "busy-morning", "busy-off", "appear_offline"]);
        expect(document.activeElement).toBe(document.querySelector("[role='menu'] [data-key='appear_offline']"));

        await w.get("[data-testid='page-back']").trigger("click");
        await settle();
        expect(w.find("[data-testid='page-title']").exists()).toBe(false);
        expect(document.querySelector("[role='menu']")).toBeNull();
        expect(w.emitted("close")).toBeUndefined();
        expect(document.activeElement).toBe(w.get("[data-testid='status-change']").element);
    });

    test("on a phone, a choice on the Status page is told to the block and turns back to Change", async () => {
        const w = mountSettings({ statusChoice: { busy_until: null, appear_offline: false } }, true);
        await settle();
        (w.get("[data-testid='status-change']").element as HTMLElement).focus();
        await openChange(w);

        document.querySelector<HTMLElement>("[role='menu'] [data-key='appear_offline']")!.click();
        await settle();

        expect(w.emitted("status")).toEqual([[{ busy_until: null, appear_offline: true }]]);
        expect(w.find("[data-testid='page-title']").exists()).toBe(false);
        expect(document.activeElement).toBe(w.get("[data-testid='status-change']").element);
    });

    test("on a phone, Escape on the Status page goes back to settings, not out of them", async () => {
        const w = mountSettings({}, true);
        await settle();
        (w.get("[data-testid='status-change']").element as HTMLElement).focus();
        await openChange(w);

        (document.activeElement as HTMLElement).dispatchEvent(new KeyboardEvent("keydown", { key: "Escape", bubbles: true }));
        await settle();

        expect(w.find("[data-testid='page-title']").exists()).toBe(false);
        expect(w.emitted("close")).toBeUndefined();
        expect(document.activeElement).toBe(w.get("[data-testid='status-change']").element);
    });

    test("reads no online switch, so with it off the group stays with every choice in it", async () => {
        const w = mountSettings({ statusChoice: { busy_until: null, appear_offline: false } });

        expect(Object.keys(w.props())).not.toContain("isStatusShown");

        expect(w.find("[data-testid='settings-status']").exists()).toBe(true);
        await openChange(w);
        expect(menuKeys()).toEqual(["auto", "busy-1h", "busy-2h", "busy-morning", "busy-off", "appear_offline"]);
    });

    test("while the church is read only, Change still opens the menu, which says why nothing in it can be chosen", async () => {
        const w = mountSettings({ statusChoice: { busy_until: null, appear_offline: false }, statusClosedReason: reason });

        const change = w.get("[data-testid='status-change']").element as HTMLElement;
        expect(change.hasAttribute("aria-disabled")).toBe(false);
        await openChange(w);
        expect(plain(document.querySelector("[role='menu'] [data-testid='menu-note']")!.textContent!)).toBe(reason);
        expect(Array.from(document.querySelectorAll("[role='menu'] [role='menuitemradio']")).every(item => item.getAttribute("aria-disabled") === "true")).toBe(true);

        await w.setProps({ statusClosedReason: null });
        await settle();
        expect(document.querySelector("[role='menu'] [data-testid='menu-note']")).toBeNull();
    });

    test("while her choice keeps failing to save, the Status line and its menu say it is not saved yet, until it lands", async () => {
        const w = mountSettings({ statusChoice: { busy_until: null, appear_offline: true }, isStatusRetrying: true });

        // the line still says what she chose, and the note under it that it is not saved
        expect(statusLine(w)).toBe("Appear offline");
        expect(plain(w.get("[data-testid='status-row'] [data-testid='switch-note']").text())).toBe("Not saved yet. Trying again.");
        await openChange(w);
        expect(plain(document.querySelector("[role='menu'] [data-testid='menu-note']")!.textContent!)).toBe("Not saved yet. Trying again.");

        await w.setProps({ isStatusRetrying: false });
        await settle();
        expect(w.find("[data-testid='status-row'] [data-testid='switch-note']").exists()).toBe(false);
        expect(document.querySelector("[role='menu'] [data-testid='menu-note']")).toBeNull();
    });

    // The line keeps time while the dialog is open: a Busy reads Automatic at its end, and an end on
    // the next day loses its "tomorrow" at midnight.
    describe("as the clock moves", () => {
        beforeEach(() => {
            jest.useFakeTimers({ doNotFake: ["nextTick", "queueMicrotask", "setImmediate", "requestAnimationFrame", "cancelAnimationFrame"] });
        });

        test("reads Automatic once her Busy ends", async () => {
            jest.setSystemTime(new Date(2026, 10, 10, 14, 59));
            const w = mountSettings({ statusChoice: { busy_until: new Date(2026, 10, 10, 15, 0).toISOString(), appear_offline: false } });
            expect(statusLine(w)).toBe("Busy, notifications paused until 3:00 PM");

            jest.advanceTimersByTime(60_000 - 1);
            await nextTick();
            expect(statusLine(w)).toBe("Busy, notifications paused until 3:00 PM");
            jest.advanceTimersByTime(1);
            await nextTick();
            expect(statusLine(w)).toBe("Automatic");
        });

        test("says an end on the next day as today's from midnight", async () => {
            jest.setSystemTime(new Date(2026, 10, 10, 23, 59));
            const w = mountSettings({ statusChoice: { busy_until: new Date(2026, 10, 11, 8, 0).toISOString(), appear_offline: false } });
            expect(statusLine(w)).toBe("Busy, notifications paused until tomorrow, 8:00 AM");

            jest.advanceTimersByTime(60_000);
            await nextTick();
            expect(statusLine(w)).toBe("Busy, notifications paused until 8:00 AM");
        });

        test("keeps no clock while the dialog is closed or for a choice with no end", async () => {
            jest.setSystemTime(new Date(2026, 10, 10, 14, 0));
            const w = mountSettings({ statusChoice: { busy_until: "infinity", appear_offline: false } });
            expect(jest.getTimerCount()).toBe(0);

            await w.setProps({ statusChoice: { busy_until: new Date(2026, 10, 10, 15, 0).toISOString(), appear_offline: false } });
            expect(jest.getTimerCount()).toBe(1);
            await w.setProps({ isOpen: false });
            expect(jest.getTimerCount()).toBe(0);
        });
    });
});
