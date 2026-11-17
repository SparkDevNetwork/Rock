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
import SettingNote from "../../../../../src/Communication/Chat/ChatShell/ui/settingNote.partial.obs";

let wrapper: VueWrapper | null = null;

function mountBar(state: JoinState = { kind: "idle" }, props: Record<string, unknown> = {}): VueWrapper {
    wrapper = mount(JoinBar, { props: { name: "Volunteers", state, ...props }, attachTo: document.body });
    return wrapper;
}

/** The rules a component's styles give under reduced motion, selector and body, read from its source. */
function reducedMotionRules(relativePath: string): Array<{ selector: string, body: string }> {
    const source = readFileSync(join(__dirname, relativePath), "utf8");
    const style = source.slice(source.indexOf("<style"), source.indexOf("</style>")).replace(/\/\*[\s\S]*?\*\//g, "");
    const rules: Array<{ selector: string, body: string }> = [];
    for (const media of style.matchAll(/@media \(prefers-reduced-motion: reduce\)\s*\{/g)) {
        // the block runs to the brace that closes the one the query opened
        let depth = 1;
        let end = (media.index ?? 0) + media[0].length;
        for (; end < style.length && depth > 0; end++) {
            depth += style[end] === "{" ? 1 : style[end] === "}" ? -1 : 0;
        }
        const inner = style.slice((media.index ?? 0) + media[0].length, end - 1);
        rules.push(...Array.from(inner.matchAll(/([^{}]+)\{([^{}]*)\}/g)).map(m => ({ selector: m[1].trim(), body: m[2] })));
    }
    return rules;
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

    test("says why when the door could not be reached, in the inline notice over the bar, and offers Join channel again", () => {
        const w = mountBar({ kind: "failed", text: "You're offline. Try again when you're back." });

        const bar = w.get("[data-testid='join-bar']");
        expect(bar.attributes("data-state")).toBe("failed");
        expect(w.get("[data-testid='join-bar-title']").text()).toBe("You're previewing Volunteers");
        const failure = w.getComponent(SettingNote);
        expect(failure.attributes("data-testid")).toBe("join-bar-failure");
        expect(failure.props("note")).toEqual({ kind: "error", text: "You're offline. Try again when you're back." });
        // over the bar, as a failed send's notice stands over the box; Join channel is its way on
        expect(failure.element.compareDocumentPosition(bar.element) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy();
        expect(w.get("[data-testid='join-bar-text']").text()).toBe("Join this channel to post and get notifications.");
        const join = w.get("[data-testid='join-channel']");
        expect(join.text()).toBe("Join channel");
        expect(join.attributes("disabled")).toBeUndefined();
        expect(join.attributes("aria-disabled")).toBeUndefined();
    });
});

describe("joining", () => {
    test("turns Join into a busy Joining with a spinner that cannot be pressed again", async () => {
        const w = mountBar({ kind: "joining" });

        expect(w.get("[data-testid='join-bar']").attributes("data-state")).toBe("joining");
        const join = w.get("[data-testid='join-channel']");
        expect(join.text()).toBe("Joining");
        expect(join.find(".chat-spinner").exists()).toBe(true);
        // held by aria-disabled rather than disabled, since a browser drops focus from a disabled button
        expect(join.attributes("aria-disabled")).toBe("true");
        expect(join.attributes("aria-busy")).toBe("true");
        expect(w.get("[data-testid='join-bar-title']").text()).toBe("You're previewing Volunteers");

        await join.trigger("click");
        expect(w.emitted("join")).toBeUndefined();
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
    test("keeps focus on the bar when the button the person pressed goes, rather than dropping it to the page", async () => {
        const w = mountBar({ kind: "joining" });
        (w.get("[data-testid='join-channel']").element as HTMLElement).focus();

        await w.setProps({ state: { kind: "pending" } });
        await nextTick();

        expect(document.activeElement).toBe(w.get("[data-testid='join-bar']").element);
    });

});

describe("refused", () => {
    const refused: JoinState = { kind: "refused", text: "Tech Team is full right now. Ask Nathan Hughes about joining." };

    test("says the person can't join, with the door's own sentence and Browse channels", async () => {
        const w = mountBar(refused, { name: "Tech Team" });

        const bar = w.get("[data-testid='join-bar']");
        expect(bar.attributes("data-state")).toBe("refused");
        // the bar is the inline notice, a failure: no second red card of its own
        const notice = w.getComponent(SettingNote);
        expect(notice.element).toBe(bar.element);
        expect(notice.props("note").kind).toBe("error");
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

// A join's news is the rooms' to say, through the chat's one voice, whichever view is open: the
// rooms' cells hold a pending join said politely with Browse channels closed, and the same failure
// said twice; the announcer's, politeness and repeated words.
describe("what is spoken", () => {
    test.each([
        { kind: "pending" },
        { kind: "refused", text: "Volunteers is full right now." },
        { kind: "failed", text: "Couldn't join." }
    ] as JoinState[])("draws no live region of its own while $kind, so its news is said once, in the chat's one voice", state => {
        const w = mountBar(state);

        expect(w.findAll("[aria-live], [role='status'], [role='alert']")).toHaveLength(0);
    });
});

describe("the entrance", () => {
    test("rises in by the sheet's rise over the strip's time, as every strip does", () => {
        const w = mountBar();
        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/joinBar.partial.obs"), "utf8");
        const style = source.slice(source.indexOf("<style"), source.indexOf("</style>"));

        expect(w.get("[data-testid='join-bar']").classes()).toContain("chat-joinbar");
        expect(style).toMatch(/\.chat-joinbar\s*\{[^}]*animation:\s*chat-enter var\(--chat-t-base\) var\(--chat-ease-in\)/);
        expect(/\.chat-joinbar\s*\{([^}]*)\}/.exec(style)?.[1]).not.toMatch(/--chat-rise:/);
    });

    test("only fades in under reduced motion, as the quote strip does", () => {
        const sheet = readFileSync(join(__dirname, "../../../../../../RockWeb/Styles/Blocks/Communication/Chat/chat.css"), "utf8");
        const reduced = sheet.slice(sheet.indexOf("@media (prefers-reduced-motion: reduce)"));

        // The bar rises by the sheet's rise, which the token sheet takes to nothing under reduced motion.
        expect(reduced).toMatch(/--chat-rise:\s*0px;/);
        expect(reducedMotionRules("../../../../../src/Communication/Chat/ChatShell/components/joinBar.partial.obs")).toHaveLength(0);
    });
});

// In Browse channels the bar is the preview's footer: the channel is beside the list, not open, so
// a person in it is offered Open channel and Leave, Leave asks first in the footer's own place,
// and nothing changes shape as Join turns to Joining and then to Open channel.
describe("in the preview's footer", () => {
    function mountFooter(state: JoinState = { kind: "idle" }, props: Record<string, unknown> = {}): VueWrapper {
        return mountBar(state, { place: "preview", ...props });
    }

    test("offers Join channel beside how to post, for a channel the person is not in", async () => {
        const w = mountFooter();

        const bar = w.get("[data-testid='join-bar']");
        expect(bar.attributes("data-place")).toBe("preview");
        expect(bar.attributes("data-state")).toBe("preview");
        expect(w.get("[data-testid='join-bar-text']").text()).toBe("Join to post and get notifications");
        expect(w.find("[data-testid='join-bar-title']").exists()).toBe(false);
        expect(w.find("[data-testid='join-open']").exists()).toBe(false);
        expect(w.find("[data-testid='join-leave']").exists()).toBe(false);

        await w.get("[data-testid='join-channel']").trigger("click");
        expect(w.emitted("join")).toHaveLength(1);
    });

    test("keeps Join channel's button while the door is out, busy and holding its width, saying what is joining", async () => {
        const w = mountFooter();
        const join = w.get("[data-testid='join-channel']");
        (join.element as HTMLElement).focus();

        await w.setProps({ state: { kind: "joining" } });

        const joining = w.get("[data-testid='join-channel']");
        expect(joining.element).toBe(join.element);
        expect(document.activeElement).toBe(joining.element);
        expect(joining.attributes("aria-busy")).toBe("true");
        // the button holds its resting width while busy, so the footer never shifts
        expect(joining.attributes("style")).toMatch(/min-width/);
        expect(w.get("[data-testid='join-bar-text']").text()).toBe("Joining Volunteers");
        await joining.trigger("click");
        expect(w.emitted("join")).toBeUndefined();
    });

    test("offers Open channel and Leave for a channel the person is in, Open asking for the channel", async () => {
        const w = mountFooter({ kind: "idle" }, { isMember: true });

        expect(w.get("[data-testid='join-bar']").attributes("data-state")).toBe("member");
        expect(w.get("[data-testid='join-bar-text']").text()).toBe("You're a member");
        expect(w.find("[data-testid='join-channel']").exists()).toBe(false);
        const open = w.get("[data-testid='join-open']");
        expect(open.text()).toBe("Open channel");
        expect(w.get("[data-testid='join-leave']").text()).toBe("Leave");
        // Leave before Open, Open at the end where Join stood
        expect(w.get("[data-testid='join-leave']").element.compareDocumentPosition(open.element) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy();

        await open.trigger("click");
        expect(w.emitted("open")).toHaveLength(1);
    });

    test("swaps Join for Open channel and Leave once joined, saying the person joined", () => {
        const w = mountFooter({ kind: "joined", isFresh: true }, { isMember: true });

        expect(w.get("[data-testid='join-bar-text']").text()).toBe("You joined Volunteers");
        expect(w.find("[data-testid='join-open']").exists()).toBe(true);
        expect(w.find("[data-testid='join-leave']").exists()).toBe(true);
    });

    test("moves focus from Joining to Open channel, where it stood, once the join lands", async () => {
        const w = mountFooter();
        (w.get("[data-testid='join-channel']").element as HTMLElement).focus();
        await w.setProps({ state: { kind: "joining" } });

        await w.setProps({ state: { kind: "joined", isFresh: true }, isMember: true });
        await nextTick();

        expect(document.activeElement).toBe(w.get("[data-testid='join-open']").element);
    });

    test("says the room is setting up while the platform catches up, with no button", () => {
        const w = mountFooter({ kind: "pending" });

        expect(w.get("[data-testid='join-bar']").attributes("data-state")).toBe("pending");
        expect(w.get("[data-testid='join-bar-title']").text()).toBe("You've joined Volunteers");
        expect(w.get("[data-testid='join-bar-text']").find(".chat-spinner").exists()).toBe(true);
        expect(w.find("button").exists()).toBe(false);
    });

    test("says the door's own sentence when it refuses, with no way to Browse channels, which is already open", () => {
        const w = mountFooter({ kind: "refused", text: "Volunteers is full right now." });

        expect(w.get("[data-testid='join-bar-title']").text()).toBe("You can't join Volunteers");
        expect(w.get("[data-testid='join-bar-text']").text()).toBe("Volunteers is full right now.");
        expect(w.find("[data-testid='join-bar-browse']").exists()).toBe(false);
        expect(w.find("[data-testid='join-channel']").exists()).toBe(false);
    });

    test("says why when the door could not be reached, and offers Join channel again", () => {
        const w = mountFooter({ kind: "failed", text: "You're offline. Try again when you're back." });

        expect(w.get("[data-testid='join-bar-text']").text()).toBe("You're offline. Try again when you're back.");
        expect(w.get("[data-testid='join-channel']").attributes("aria-disabled")).toBeUndefined();
    });

    test("asks before leaving, in the footer's place, Cancel first so Enter never leaves by accident", async () => {
        const w = mountFooter({ kind: "idle" }, { isMember: true });

        await w.get("[data-testid='join-leave']").trigger("click");
        await nextTick();

        expect(w.get("[data-testid='join-bar']").attributes("data-state")).toBe("confirmLeave");
        expect(w.get("[data-testid='leave-question']").text()).toBe("Leave Volunteers?");
        expect(w.get("[data-testid='leave-body']").text()).toBe("You'll leave its Rock group too.");
        expect(document.activeElement).toBe(w.get("[data-testid='leave-cancel']").element);
        expect(w.find("[data-testid='join-open']").exists()).toBe(false);
        expect(w.emitted("leave")).toBeUndefined();

        await w.get("[data-testid='leave-go']").trigger("click");
        expect(w.emitted("leave")).toHaveLength(1);
    });

    test("goes back from the question to Leave with Cancel or Escape, the Escape kept from the view behind", async () => {
        const w = mountFooter({ kind: "idle" }, { isMember: true });
        const outside = jest.fn();
        document.body.addEventListener("keydown", outside);

        await w.get("[data-testid='join-leave']").trigger("click");
        await nextTick();
        await w.get("[data-testid='leave-cancel']").trigger("click");
        await nextTick();
        expect(w.find("[data-testid='leave-question']").exists()).toBe(false);
        expect(document.activeElement).toBe(w.get("[data-testid='join-leave']").element);

        await w.get("[data-testid='join-leave']").trigger("click");
        await nextTick();
        await w.get("[data-testid='leave-cancel']").trigger("keydown", { key: "Escape" });
        await nextTick();
        expect(w.find("[data-testid='leave-question']").exists()).toBe(false);
        expect(outside).not.toHaveBeenCalled();
        document.body.removeEventListener("keydown", outside);
    });

    test("holds Leave busy while the door is out, and says the door's own sentence when it refuses", async () => {
        const w = mountFooter({ kind: "idle" }, { isMember: true });

        await w.get("[data-testid='join-leave']").trigger("click");
        await w.get("[data-testid='leave-go']").trigger("click");
        await w.setProps({ leave: { kind: "leaving" } });
        const going = w.get("[data-testid='leave-go']");
        expect(going.attributes("aria-busy")).toBe("true");
        expect(going.text()).toBe("Leaving");

        await w.setProps({ leave: { kind: "refused", text: "Leaders can't leave Volunteers." } });
        expect(w.get("[data-testid='leave-error']").text()).toBe("Leaders can't leave Volunteers.");
    });

    test("is Join channel again once the person has left", async () => {
        const w = mountFooter({ kind: "idle" }, { isMember: true });

        await w.get("[data-testid='join-leave']").trigger("click");
        await w.get("[data-testid='leave-go']").trigger("click");
        await w.setProps({ leave: { kind: "left" }, isMember: false });

        expect(w.find("[data-testid='leave-question']").exists()).toBe(false);
        expect(w.get("[data-testid='join-channel']").text()).toBe("Join channel");
    });
});
