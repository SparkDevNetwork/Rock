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
// The conversation header, mounted: the room's title with the line under it, the offer to turn on
// notifications, and what the header says while the connection is down. The subtitle gives way to
// the connection's state while there is one, a sweeping bar and a banner say the same thing more
// loudly, and the service's own sentence sits under the header in the tone its kind calls for. On a
// phone the header leads with Back, and the offer moves into the overflow menu, which is left out
// when it would hold nothing.
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import ChatHeader from "../../../../../src/Communication/Chat/ChatShell/components/chatHeader.partial.obs";
import { ChatError } from "../../../../../src/Communication/Chat/ChatShell/types.partial";
import Icon from "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs";

const lostConnection: ChatError = { code: "rt.disconnected", severity: "degraded", text: null };

let wrapper: VueWrapper | null = null;

function mountHeader(props: Record<string, unknown> = {}): VueWrapper {
    wrapper = mount(ChatHeader, {
        props: { title: "worship-team", ...props },
        attachTo: document.body
    });
    return wrapper;
}

/** Whether an icon of the chat's set with this name is drawn inside the element. */
function hasIcon(w: VueWrapper, inside: { element: Element }, name: string): boolean {
    return w.findAllComponents(Icon).some(icon => icon.props("name") === name && inside.element.contains(icon.element));
}

async function settle(): Promise<void> {
    await nextTick();
    await nextTick();
}

beforeAll(() => {
    // jsdom has no ResizeObserver, and the floating menu and tooltip keep their place with one.
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

describe("the title and subtitle", () => {
    test("shows the title and a group's subtitle", () => {
        const w = mountHeader({ title: "Anna and Marcus", subtitle: "3 people" });

        expect(w.get("[data-testid='header-title']").text()).toBe("Anna and Marcus");
        expect(w.get("[data-testid='header-subtitle']").text()).toBe("3 people");
    });

    test("draws no subtitle line when the wire has none and the connection is well", () => {
        const w = mountHeader({ subtitle: null });

        expect(w.find("[data-testid='header-subtitle']").exists()).toBe(false);
    });
    test("names a channel plainly and marks it with the hash icon, or the lock when it is private", async () => {
        const w = mountHeader({ title: "Worship Team", kind: "channel", isPublic: true });
        expect(w.get("[data-testid='header-title']").text()).toBe("Worship Team");
        expect(hasIcon(w, w.get(".chat-header-ic"), "hash")).toBe(true);

        await w.setProps({ isPublic: false });
        expect(w.get("[data-testid='header-title']").text()).toBe("Worship Team");
        expect(hasIcon(w, w.get(".chat-header-ic"), "lock")).toBe(true);
        expect(hasIcon(w, w.get(".chat-header-ic"), "hash")).toBe(false);

        await w.setProps({ title: "Anna Kim", kind: "person", people: ["Anna Kim"] });
        expect(w.get("[data-testid='header-title']").text()).toBe("Anna Kim");
        expect(w.find(".chat-header-ic").exists()).toBe(false);
    });
});

describe("the connection's states", () => {
    test("say 'Reconnecting…' in the subtitle with the live dot while the connection is lost", () => {
        const w = mountHeader({ subtitle: "3 people", connection: lostConnection });
        const state = w.get("[data-testid='connection-badge']");

        expect(state.text()).toBe("Reconnecting…");
        expect(state.attributes("data-tone")).toBe("warning");
        expect(state.find("[data-testid='connection-dot']").exists()).toBe(true);
        // The subtitle's own words give way while the connection speaks.
        expect(w.find("[data-testid='header-subtitle']").exists()).toBe(false);
    });

    test("say 'Catching up on new messages…' once back and behind", () => {
        const w = mountHeader({ isBehind: true });
        const state = w.get("[data-testid='catching-up-badge']");

        expect(state.text()).toBe("Catching up on new messages…");
        expect(state.attributes("data-tone")).toBe("info");
    });

    test("sweep a bar across the top while offline only", async () => {
        const w = mountHeader({ connection: lostConnection });
        expect(w.find("[data-testid='connection-sweep']").exists()).toBe(true);

        await w.setProps({ connection: null, isBehind: true });
        expect(w.find("[data-testid='connection-sweep']").exists()).toBe(false);
    });

    test("open the offline banner, then the back-online banner, then nothing", async () => {
        const w = mountHeader({ connection: lostConnection });

        const offline = w.get("[data-testid='connection-banner']");
        expect(offline.attributes("data-tone")).toBe("warning");
        expect(offline.text()).toContain("You're offline.");
        expect(offline.text()).toContain("Reconnecting now. Messages you send will go out when you're back.");

        await w.setProps({ connection: null, isBehind: true });
        const back = w.get("[data-testid='connection-banner']");
        expect(back.attributes("data-tone")).toBe("info");
        expect(back.text()).toContain("Back online. Catching up on new messages…");

        await w.setProps({ isBehind: false });
        expect(w.find("[data-testid='connection-banner']").exists()).toBe(false);
    });
});

describe("the service banners", () => {
    test("show the server's read-only sentence in the warning tone", () => {
        const sentence = "Chat is read-only while we do some maintenance. You can read messages but not send them.";
        const w = mountHeader({ banner: sentence });
        const banner = w.get("[data-testid='service-banner']");

        expect(banner.text()).toContain(sentence);
        expect(banner.attributes("data-tone")).toBe("warning");
        expect(hasIcon(w, banner, "alert")).toBe(true);
    });

    test("show update required in the info tone with the refresh icon", () => {
        const w = mountHeader({ banner: "Chat needs a newer version of Rock.", isUpdateRequired: true });
        const banner = w.get("[data-testid='service-banner']");

        expect(banner.attributes("data-tone")).toBe("info");
        expect(hasIcon(w, banner, "refresh")).toBe(true);
    });
});

describe("the push offer on a desktop", () => {
    test("is a bell icon button labelled 'Turn on notifications'", async () => {
        const w = mountHeader({ isPushOffered: true });
        const offer = w.get("[data-testid='push-offer']");

        expect(offer.classes()).toContain("chat-icon-btn");
        expect(offer.attributes("aria-label")).toBe("Turn on notifications");
        expect(hasIcon(w, offer, "bell")).toBe(true);

        await offer.trigger("click");
        expect(w.emitted("turnOnPush")).toHaveLength(1);
    });

    test("is gone when not offered, and there is no overflow on a desktop", () => {
        const w = mountHeader({ isPushOffered: false });

        expect(w.find("[data-testid='push-offer']").exists()).toBe(false);
        expect(w.find("[data-testid='header-more']").exists()).toBe(false);
        expect(w.find("[data-testid='header-back']").exists()).toBe(false);
    });
});

describe("the header on a phone", () => {
    test("leads with a Back chevron that returns to the list", async () => {
        const w = mountHeader({ isPhone: true });
        const back = w.get("[data-testid='header-back']");

        expect(back.attributes("aria-label")).toBe("Back");
        expect(hasIcon(w, back, "left")).toBe(true);

        await back.trigger("click");
        expect(w.emitted("back")).toHaveLength(1);
    });

    test("moves the push offer into the overflow menu as its one item", async () => {
        const w = mountHeader({ isPhone: true, isPushOffered: true });

        expect(w.find("[data-testid='push-offer']").exists()).toBe(false);

        const more = w.get("[data-testid='header-more'] button");
        expect(more.attributes("aria-label")).toBe("More");
        await more.trigger("click");
        await settle();

        const items = Array.from(document.querySelectorAll<HTMLElement>("[role='menu'] [role='menuitem']"));
        expect(items.map(i => i.textContent?.trim())).toEqual(["Turn on notifications"]);

        items[0].click();
        await settle();
        expect(w.emitted("turnOnPush")).toHaveLength(1);
    });

    test("leaves the overflow out when it would hold nothing", () => {
        const w = mountHeader({ isPhone: true, isPushOffered: false });

        expect(w.find("[data-testid='header-more']").exists()).toBe(false);
    });
});

describe("the end of an outage", () => {
    test("is told once the connection is back and the catch-up after it is done, so the shell can say it", async () => {
        const w = mountHeader({ connection: lostConnection });

        await w.setProps({ connection: null, isBehind: true });
        expect(w.emitted("backOnline")).toBeUndefined();

        await w.setProps({ isBehind: false });
        expect(w.emitted("backOnline")).toEqual([[]]);
    });

    test("is never told when the connection was never lost", async () => {
        const w = mountHeader({ isBehind: true });

        await w.setProps({ isBehind: false });

        expect(w.emitted("backOnline")).toBeUndefined();
    });
});
