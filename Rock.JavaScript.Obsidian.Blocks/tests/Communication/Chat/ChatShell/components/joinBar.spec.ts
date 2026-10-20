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
// The join bar, mounted: what stands in the composer's place while a person previews a channel
// they are not in. It offers Join, says Joining while the door is out, says the person is in and
// the room is still setting up while the platform catches up, and when the door refuses, shows its
// own sentence with a way back to Browse channels, focus following the person's place.
import { readFileSync } from "fs";
import { join } from "path";
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import JoinBar from "../../../../../src/Communication/Chat/ChatShell/components/joinBar.partial.obs";
import { JoinState } from "../../../../../src/Communication/Chat/ChatShell/composables/useRooms.partial";
import Icon from "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs";

let wrapper: VueWrapper | null = null;

function mountBar(state: JoinState = { kind: "idle" }, props: Record<string, unknown> = {}): VueWrapper {
    wrapper = mount(JoinBar, { props: { name: "Volunteers", state, ...props }, attachTo: document.body });
    return wrapper;
}

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
    document.body.innerHTML = "";
});

describe("previewing", () => {
    test("says the person is previewing the room by name, with no '#', and how to post", () => {
        const w = mountBar();

        const bar = w.get("[data-testid='join-bar']");
        expect(bar.attributes("data-state")).toBe("preview");
        expect(w.get("[data-testid='join-bar-title']").text()).toBe("You're previewing Volunteers");
        expect(w.get("[data-testid='join-bar-text']").text()).toBe("Join this channel to post and get notifications.");
        expect(w.findAllComponents(Icon).some(icon => icon.props("name") === "door" && bar.element.contains(icon.element))).toBe(true);
    });

    test("offers Join channel, which asks the door", async () => {
        const w = mountBar();

        const join = w.get("[data-testid='join-channel']");
        expect(join.text()).toBe("Join channel");
        await join.trigger("click");

        expect(w.emitted("join")).toHaveLength(1);
    });

    test("offers Join channel again when the door could not be reached", () => {
        const w = mountBar({ kind: "offline" });

        expect(w.get("[data-testid='join-bar']").attributes("data-state")).toBe("preview");
        expect(w.get("[data-testid='join-channel']").attributes("disabled")).toBeUndefined();
    });
});

describe("joining", () => {
    test("turns Join into a busy Joining with a spinner that cannot be pressed again", async () => {
        const w = mountBar({ kind: "joining" });

        expect(w.get("[data-testid='join-bar']").attributes("data-state")).toBe("joining");
        const join = w.get("[data-testid='join-channel']");
        expect(join.text()).toBe("Joining");
        expect(join.find(".chat-spinner").exists()).toBe(true);
        expect(join.attributes("disabled")).toBeDefined();
        expect(join.attributes("aria-busy")).toBe("true");
        expect(w.get("[data-testid='join-bar-title']").text()).toBe("You're previewing Volunteers");
    });

    test("keeps focus on the button while it turns to Joining", async () => {
        const w = mountBar();
        (w.get("[data-testid='join-channel']").element as HTMLElement).focus();

        await w.setProps({ state: { kind: "joining" } });

        expect(document.activeElement).toBe(w.get("[data-testid='join-channel']").element);
    });
});

describe("pending", () => {
    test("says the person has joined and the room is setting up, with no button", () => {
        const w = mountBar({ kind: "pending" });

        expect(w.get("[data-testid='join-bar']").attributes("data-state")).toBe("pending");
        expect(w.get("[data-testid='join-bar-title']").text()).toBe("You've joined Volunteers");
        const text = w.get("[data-testid='join-bar-text']");
        expect(text.text()).toBe("Setting up, a few seconds");
        expect(text.find(".chat-spinner").exists()).toBe(true);
        expect(w.find("button").exists()).toBe(false);
    });
});

describe("refused", () => {
    const refused: JoinState = { kind: "refused", text: "Tech Team is full right now. Ask Nathan Hughes about joining." };

    test("says the person can't join, with the door's own sentence and Browse channels", async () => {
        const w = mountBar(refused, { name: "Tech Team" });

        const bar = w.get("[data-testid='join-bar']");
        expect(bar.attributes("data-state")).toBe("refused");
        expect(bar.classes()).toContain("is-refused");
        expect(w.get("[data-testid='join-bar-title']").text()).toBe("You can't join Tech Team");
        expect(w.get("[data-testid='join-bar-text']").text()).toBe("Tech Team is full right now. Ask Nathan Hughes about joining.");
        expect(w.find("[data-testid='join-channel']").exists()).toBe(false);

        const browse = w.get("[data-testid='join-bar-browse']");
        expect(browse.text()).toBe("Browse channels");
        await browse.trigger("click");
        expect(w.emitted("browse")).toHaveLength(1);
    });

    test("moves focus from the button the person pressed to Browse channels", async () => {
        const w = mountBar({ kind: "joining" });
        (w.get("[data-testid='join-channel']").element as HTMLElement).focus();

        await w.setProps({ state: refused });
        await nextTick();

        expect(document.activeElement).toBe(w.get("[data-testid='join-bar-browse']").element);
    });

    test("leaves focus alone when the person was elsewhere", async () => {
        const elsewhere = document.createElement("button");
        document.body.appendChild(elsewhere);
        const w = mountBar({ kind: "joining" });
        elsewhere.focus();

        await w.setProps({ state: refused });
        await nextTick();

        expect(document.activeElement).toBe(elsewhere);
    });
});

describe("the entrance", () => {
    test("rises in from 6px below over the strip's time, as every strip does", () => {
        const w = mountBar();
        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/joinBar.partial.obs"), "utf8");
        const style = source.slice(source.indexOf("<style"), source.indexOf("</style>"));

        expect(w.get("[data-testid='join-bar']").classes()).toContain("chat-joinbar");
        expect(style).toMatch(/\.chat-joinbar\s*\{[^}]*animation:\s*chat-strip-in var\(--chat-t-base\) var\(--chat-ease-in\)/);
        expect(style).toMatch(/@keyframes chat-strip-in\s*\{\s*from\s*\{\s*opacity:\s*0;\s*transform:\s*translateY\(6px\);/);
    });
});
