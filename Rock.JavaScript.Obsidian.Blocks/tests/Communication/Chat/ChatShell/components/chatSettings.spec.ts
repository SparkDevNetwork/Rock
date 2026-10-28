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

        const help = w.findAll(".chat-sg-sub, [data-testid='settings-foot']").map(line => line.text());
        expect(help.length).toBeGreaterThan(0);
        expect(help.reduce((sum, line) => sum + words(line), 0)).toBeLessThanOrEqual(32);
    });
});

describe("Privacy", () => {
    test("is a group of the two switches under its label, each as the person's own setting stands", () => {
        const w = mountSettings();

        const group = w.get("[data-testid='settings-privacy'] [role='group']");
        expect(document.getElementById(group.attributes("aria-labelledby") ?? "")?.textContent?.trim()).toBe("Privacy");
        expect(switchOf(w, "profile_details").attributes("aria-checked")).toBe("true");
        expect(switchOf(w, "open_dm").attributes("aria-checked")).toBe("false");
        expect(plain(w.get("[data-testid='switch-profile_details']").text())).toBe("Show my campus and badges");
        expect(plain(w.get("[data-testid='switch-open_dm']").text())).toBe("Anyone can message me Off: only people in a private channel with you");
        expect(hasIcon(w, w.get("[data-testid='switch-profile_details']"), "id-card")).toBe(true);
        expect(hasIcon(w, w.get("[data-testid='switch-open_dm']"), "message")).toBe(true);
    });

    test("names each switch by its label and describes it by the line under it", () => {
        const w = mountSettings();

        const openDm = switchOf(w, "open_dm");
        expect(document.getElementById(openDm.attributes("aria-labelledby") ?? "")?.textContent).toBe("Anyone can message me");
        expect(document.getElementById(openDm.attributes("aria-describedby") ?? "")?.textContent?.trim()).toBe("Off: only people in a private channel with you");
        expect(switchOf(w, "profile_details").attributes("aria-describedby")).toBeUndefined();
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
