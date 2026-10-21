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
// own profile card. Who they are, where their name and photo come from, the two privacy switches,
// this browser's notifications in each of push's states, and the reaction switch with a word for
// a browser that will not show them. Each switch is told to the block at once, which saves it;
// the head's indicator and the note under a switch show what the save came to, a pending save in
// its own words and a refusal in the server's.
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
            save: null,
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

    test("names the person, their campus, and where their name and photo come from", () => {
        const w = mountSettings();

        expect(w.get("[data-testid='settings-name']").text()).toBe("Jordan Lee");
        expect(plain(w.get("[data-testid='settings-meta']").text())).toBe("Downtown Campus · Your name and photo come from your Cedar Hill profile.");
    });

    test("leaves the campus out for a person with none", () => {
        const w = mountSettings({ campusName: null });

        expect(plain(w.get("[data-testid='settings-meta']").text())).toBe("Your name and photo come from your Cedar Hill profile.");
    });
});

describe("Privacy", () => {
    test("is the two switches, each as the person's own setting stands", () => {
        const w = mountSettings();

        expect(w.get("[data-testid='settings-privacy'] h4").text()).toBe("Privacy");
        expect(switchOf(w, "profile_details").attributes("aria-checked")).toBe("true");
        expect(switchOf(w, "open_dm").attributes("aria-checked")).toBe("false");
        expect(plain(w.get("[data-testid='switch-profile_details']").text())).toBe("Show my details People who open your profile card see your campus and badges. Your name and photo always show.");
        expect(plain(w.get("[data-testid='switch-open_dm']").text())).toBe("Open direct messages Anyone at Cedar Hill can start a conversation with you. When off, only people in a private channel with you can.");
    });

    test("tells the block the setting and its new value", async () => {
        const w = mountSettings();

        await switchOf(w, "profile_details").trigger("click");
        await switchOf(w, "open_dm").trigger("click");

        expect(w.emitted("toggle")).toEqual([["profile_details", false], ["open_dm", true]]);
    });

    test("a pending save's note says it can take a minute, and a refusal's carries the server's sentence", () => {
        const w = mountSettings({
            notes: {
                profile_details: { kind: "pending", text: "Saved. It can take a minute to reach everyone." },
                open_dm: { kind: "error", text: "Chat is not available to you right now." }
            }
        });

        const pending = w.get("[data-testid='switch-profile_details'] [data-testid='switch-note']");
        expect(pending.attributes("data-kind")).toBe("pending");
        expect(pending.text()).toBe("Saved. It can take a minute to reach everyone.");
        expect(hasIcon(w, pending, "clock")).toBe(true);

        const refused = w.get("[data-testid='switch-open_dm'] [data-testid='switch-note']");
        expect(refused.attributes("data-kind")).toBe("error");
        expect(hasIcon(w, refused, "alert")).toBe(true);
    });
});

describe("each switch's Saving", () => {
    test("marks busy every switch whose save is out, not only the one pressed last", () => {
        const w = mountSettings({ save: { state: "saving", count: 2 }, saving: { reaction_push: true, open_dm: true } });
        const busy = (name: string): string | undefined => w.get(`[data-testid='switch-${name}'] [role='switch']`).attributes("aria-busy");

        expect(busy("reaction_push")).toBe("true");
        expect(busy("open_dm")).toBe("true");
        expect(busy("profile_details")).not.toBe("true");
    });
});

describe("the save indicator", () => {
    test("says Saving, Saved, Saved, syncing and Not saved", async () => {
        const w = mountSettings({ save: { state: "saving", count: 1 } });
        const indicator = (): ReturnType<VueWrapper["get"]> => w.get("[data-testid='settings-save']");

        expect(plain(indicator().text())).toBe("Saving");
        await w.setProps({ save: { state: "saved", count: 1 } });
        expect(plain(indicator().text())).toBe("Saved");
        expect(indicator().attributes("data-state")).toBe("saved");
        await w.setProps({ save: { state: "pending", count: 2 } });
        expect(plain(indicator().text())).toBe("Saved, syncing");
        await w.setProps({ save: { state: "refused", count: 3 } });
        expect(plain(indicator().text())).toBe("Not saved");
    });

    test("fades Saved after a moment", async () => {
        jest.useFakeTimers();
        try {
            const w = mountSettings({ save: { state: "saving", count: 1 } });
            await w.setProps({ save: { state: "saved", count: 1 } });

            jest.advanceTimersByTime(1600);
            await settle();

            expect(w.get("[data-testid='settings-save']").attributes("data-state")).toBe("fade");
        }
        finally {
            jest.useRealTimers();
        }
    });

    test("reads the settings' news in its live region", async () => {
        const w = mountSettings();

        await w.setProps({ announcement: { text: "Open direct messages on. Saved.", isAssertive: false, count: 3 } });

        expect(w.get("[data-testid='settings-announcement']").text()).toBe("Open direct messages on. Saved.");
    });
});

describe("this browser's notifications", () => {
    const rows: Array<[string, string, string, string | null]> = [
        ["offered", "Off for this browser", "Turn them on to hear about new messages when chat isn't open.", "Turn on"],
        ["asking", "Waiting for your browser", "Choose Allow in the box your browser shows.", "Turn on"],
        ["on", "On for this browser", "You'll get notifications here, following each conversation's setting.", "Turn off"],
        ["blocked", "Blocked by your browser", "Allow notifications for this site in your browser's settings, then reload the page.", null],
        ["unavailable", "Not available here", "Browser notifications aren't set up for Cedar Hill yet.", null]
    ];

    test.each(rows)("in its %s state says so and offers what it can", (state, title, description, action) => {
        const w = mountSettings({ push: state });

        const row = w.get("[data-testid='push-row']");
        expect(row.attributes("data-state")).toBe(state);
        expect(row.get("b").text()).toBe(title);
        expect(row.get("[data-testid='push-desc']").text()).toBe(description);
        const button = row.find("button");
        expect(button.exists() ? plain(button.text()) : null).toBe(action);
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
        expect(plain(w.get("[data-testid='switch-reaction_push']").text())).toBe(
            "Reactions to my messages A notification when someone reacts to a message you sent, grouped per message. This browser won't show them, but it applies to your other devices with notifications on."
        );

        await w.setProps({ push: "on" });
        expect(w.get("[data-testid='switch-reaction_push']").text()).not.toContain("This browser won't show them");

        await switchOf(w, "reaction_push").trigger("click");
        expect(w.emitted("toggle")).toEqual([["reaction_push", false]]);
    });

    test("ends by saying where each conversation's own setting is", () => {
        const w = mountSettings();

        expect(w.get("[data-testid='settings-foot']").text()).toBe("Each conversation has its own setting. Choose it from the conversation's name at the top.");
    });
});
