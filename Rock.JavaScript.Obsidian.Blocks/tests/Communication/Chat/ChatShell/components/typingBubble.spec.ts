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
// Someone else typing, drawn as their next message would be: their face, or the group face of
// everyone typing, beside an incoming bubble of three dots, and in a group a name line saying who.
// A screen reader meets one hidden sentence, never announced. How it comes and goes is the tail's
// of the feed or the thread it is drawn in.
import { readFileSync } from "fs";
import { join } from "path";
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import TypingBubble from "../../../../../src/Communication/Chat/ChatShell/components/typingBubble.partial.obs";
import Avatar from "../../../../../src/Communication/Chat/ChatShell/ui/avatar.partial.obs";
import GroupFace from "../../../../../src/Communication/Chat/ChatShell/ui/groupFace.partial.obs";
import type { Typing } from "../../../../../src/Communication/Chat/ChatShell/composables/usePresence.partial";
import { FeedTail, feedTailKey } from "../../../../../src/Communication/Chat/ChatShell/composables/useFeedTail.partial";
import { ChatPerson } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

const anna = "a0000002-0000-4000-8000-000000000000";
const ben = "a0000003-0000-4000-8000-000000000000";
const cal = "a0000004-0000-4000-8000-000000000000";

function person(alias: string, nick: string, last: string, photo: string | null = null): ChatPerson {
    return { person_alias_guid: alias, nick_name: nick, last_name: last, avatar_url: photo };
}

const annaTyping: Typing = { typists: [{ alias: anna, person: person(anna, "Anna", "Reyes", "https://x/anna.jpg") }], names: "Anna", sentence: "Anna is typing…" };
const twoTyping: Typing = {
    typists: [{ alias: anna, person: person(anna, "Anna", "Reyes") }, { alias: ben, person: person(ben, "Ben", "Okafor") }],
    names: "Anna and Ben",
    sentence: "Anna and Ben are typing…"
};
const severalTyping: Typing = {
    typists: [{ alias: anna, person: person(anna, "Anna", "Reyes") }, { alias: ben, person: person(ben, "Ben", "Okafor") }, { alias: cal, person: null }],
    names: "Several people",
    sentence: "Several people are typing…"
};

/**
 * Mounts the bubble in a host of its own. A test about how it goes keeps Vue's real Transition,
 * which keeps a leaving row in the host while it plays, as a browser does.
 */
function draw(props: Record<string, unknown>, isTransitionKept = false): VueWrapper {
    const host = document.createElement("div");
    document.body.append(host);
    return mount(TypingBubble, { props, attachTo: host, global: isTransitionKept ? { stubs: { transition: false } } : {} });
}

/** The row as the page holds it, a leaving one included. */
function rowIn(wrapper: VueWrapper): Element | null {
    return (wrapper.element.parentElement ?? document.body).querySelector("[data-testid='typing-bubble']");
}

afterEach(() => {
    document.body.innerHTML = "";
});

describe("the typing bubble", () => {
    test("in a direct message: the typist's face beside a bubble of three dots, with no name line", () => {
        const wrapper = draw({ typing: annaTyping });

        const avatar = wrapper.findComponent(Avatar);
        expect(avatar.props("people")).toEqual([{ name: "Anna Reyes", photoUrl: "https://x/anna.jpg" }]);
        expect(avatar.props("size")).toBe(34);
        expect(wrapper.findComponent(GroupFace).exists()).toBe(false);
        expect(wrapper.get("[data-testid='typing-dots']").findAll("i")).toHaveLength(3);
        expect(wrapper.find("[data-testid='typing-names']").exists()).toBe(false);
    });

    test("in a group the name line says who, and two typing draw the group face of both", () => {
        const one = draw({ typing: annaTyping, isGroup: true });
        expect(one.get("[data-testid='typing-names']").text()).toBe("Anna");
        expect(one.findComponent(GroupFace).exists()).toBe(false);

        const two = draw({ typing: twoTyping, isGroup: true });
        expect(two.get("[data-testid='typing-names']").text()).toBe("Anna and Ben");
        expect(two.findComponent(GroupFace).props("people")).toEqual([{ name: "Anna Reyes", photoUrl: null }, { name: "Ben Okafor", photoUrl: null }]);
        expect(two.findComponent(GroupFace).props("size")).toBe(34);
    });

    test("three or more read as several people, the group face showing each, someone not named as the people icon", () => {
        const wrapper = draw({ typing: severalTyping, isGroup: true });

        expect(wrapper.get("[data-testid='typing-names']").text()).toBe("Several people");
        expect(wrapper.findComponent(GroupFace).props("people")).toEqual([
            { name: "Anna Reyes", photoUrl: null },
            { name: "Ben Okafor", photoUrl: null },
            { name: "", photoUrl: null }
        ]);
    });

    test("with no one typing nothing is drawn", () => {
        const wrapper = draw({ typing: null, isGroup: true });

        expect(wrapper.find("[data-testid='typing-bubble']").exists()).toBe(false);
    });

    // Typing is no message: the message-to-message keys skip it, and a live region would speak
    // every pause, so a reader moving through the feed meets one sentence after the last message.
    test("a screen reader hears one hidden sentence, and the row is not a message, a live region nor inside one", () => {
        const wrapper = draw({ typing: twoTyping, isGroup: true });
        const row = wrapper.get("[data-testid='typing-bubble']").element as HTMLElement;
        const heard = row.querySelectorAll(".chat-sr-only");

        expect(heard).toHaveLength(1);
        expect(heard[0].textContent).toBe("Anna and Ben are typing…");
        expect(heard[0].closest("[aria-hidden='true']")).toBeNull();
        expect(row.querySelector("[data-testid='typing-names']")?.closest("[aria-hidden='true']")).not.toBeNull();
        expect(row.querySelector("[data-testid='typing-dots']")?.closest("[aria-hidden='true']")).not.toBeNull();
        for (let node: HTMLElement | null = row; node; node = node.parentElement) {
            expect(node.getAttribute("aria-live")).toBeNull();
            expect(node.getAttribute("role")).toBeNull();
            expect(node.tagName).not.toBe("ARTICLE");
        }
        expect(row.hasAttribute("tabindex")).toBe(false);
        expect(row.hasAttribute("data-message-id")).toBe(false);
    });

    // Her run's next row is drawn as her message there will be: no name over it, a row's gap above.
    test("continuing the typist's run in a group it draws no name line, and is marked as in her run", () => {
        const wrapper = draw({ typing: annaTyping, isGroup: true, isInRun: true });

        expect(wrapper.find("[data-testid='typing-names']").exists()).toBe(false);
        expect(wrapper.get("[data-testid='typing-bubble']").classes()).toContain("is-in-run");
        expect(wrapper.findComponent(Avatar).exists()).toBe(true);
    });

    test("the typists changing redraws the same row rather than a new one", async () => {
        const wrapper = draw({ typing: annaTyping, isGroup: true });
        const row = wrapper.get("[data-testid='typing-bubble']").element;

        await wrapper.setProps({ typing: twoTyping });

        expect(wrapper.get("[data-testid='typing-bubble']").element).toBe(row);
        expect(wrapper.get("[data-testid='typing-names']").text()).toBe("Anna and Ben");
    });
});

// The bubble has no motion of its own: the feed or the thread it is drawn in owns its tail, and
// the bubble hands that tail its element as it goes, which says when it is gone. Drawn alone, as a
// test or a view with no tail draws it, it comes and goes at once.
describe("the typing bubble going", () => {
    /** A tail standing in for the view's, which keeps each bubble it is handed until told it is gone. */
    function fakeTail(): { tail: FeedTail, handed: Array<{ element: Element, done: () => void }> } {
        const handed: Array<{ element: Element, done: () => void }> = [];
        return { tail: { leave: (element, done) => void handed.push({ element, done }) }, handed };
    }

    test("drawn alone it goes at once, whatever ended the typing", async () => {
        const wrapper = draw({ typing: annaTyping }, true);

        await wrapper.setProps({ typing: null });

        expect(rowIn(wrapper)).toBeNull();
    });

    test("in a view its going is the tail's: the tail is handed the row, which stays until the tail is done", async () => {
        const { tail, handed } = fakeTail();
        const host = document.createElement("div");
        document.body.append(host);
        const wrapper = mount(TypingBubble, { props: { typing: annaTyping }, attachTo: host, global: { stubs: { transition: false }, provide: { [feedTailKey as symbol]: tail } } });
        const row = rowIn(wrapper);

        await wrapper.setProps({ typing: null });
        expect(handed.map(going => going.element)).toEqual([row]);
        expect(rowIn(wrapper)).toBe(row);

        handed[0].done();
        await nextTick();
        expect(rowIn(wrapper)).toBeNull();
    });

    // The built block runs Vue's production runtime, which keeps no element for a comment in a
    // Transition's slot, so a second going would find stale DOM: mounted here in that runtime.
    test("comes and goes again and again in the production runtime the block ships with, handed to the tail each time", async () => {
        const previousEnvironment = process.env.NODE_ENV;
        process.env.NODE_ENV = "production";
        let production!: { mount: typeof mount, nextTick: typeof nextTick, bubble: typeof TypingBubble, key: typeof feedTailKey };
        jest.isolateModules(() => {
            production = {
                mount: jest.requireActual("@vue/test-utils").mount,
                nextTick: jest.requireActual("vue").nextTick,
                bubble: jest.requireActual("../../../../../src/Communication/Chat/ChatShell/components/typingBubble.partial.obs").default,
                key: jest.requireActual("../../../../../src/Communication/Chat/ChatShell/composables/useFeedTail.partial").feedTailKey
            };
        });
        process.env.NODE_ENV = previousEnvironment;

        const logged = jest.spyOn(console, "error");
        const { tail, handed } = fakeTail();
        const host = document.createElement("div");
        document.body.append(host);
        const wrapper = production.mount(production.bubble, { props: { typing: annaTyping }, attachTo: host, global: { provide: { [production.key as symbol]: tail } } });

        for (let turn = 0; turn < 3; turn++) {
            await wrapper.setProps({ typing: null });
            handed[handed.length - 1]?.done();
            await production.nextTick();
            await wrapper.setProps({ typing: twoTyping });
            await production.nextTick();
        }

        expect(handed).toHaveLength(3);
        expect(host.querySelectorAll("[data-testid='typing-bubble']")).toHaveLength(1);
        expect(logged).not.toHaveBeenCalled();
        logged.mockRestore();
        wrapper.unmount();
    });

    test("marks its row, its dots, and its face and name line by the group it is drawn in, for the tail to read where they stand", () => {
        const one = draw({ typing: annaTyping });
        expect(one.get("[data-testid='typing-bubble']").attributes("data-tail")).toBe("typing:typing");
        expect(one.get("[data-testid='typing-dots']").attributes("data-tail")).toBe("dots:typing");
        expect(one.get(".chat-typing-face").attributes("data-tail")).toBe("face:typing");
        expect(draw({ typing: twoTyping, isGroup: true }).get(".chat-typing-face").attributes("data-tail")).toBe("face:typing");
        expect(draw({ typing: twoTyping, isGroup: true }).get("[data-testid='typing-names']").attributes("data-tail")).toBe("name:typing");
        // continuing her run, its face is her run's, so it moves from her last row and back
        expect(draw({ typing: annaTyping, isInRun: true, groupKey: "k:7" }).get(".chat-typing-face").attributes("data-tail")).toBe("face:k:7");
    });

    test("a change of who types hands the face and the name line going to the tail, the new ones drawn beside them, in the production runtime too", async () => {
        const previousEnvironment = process.env.NODE_ENV;
        process.env.NODE_ENV = "production";
        let production!: { mount: typeof mount, nextTick: typeof nextTick, bubble: typeof TypingBubble, key: typeof feedTailKey };
        jest.isolateModules(() => {
            production = {
                mount: jest.requireActual("@vue/test-utils").mount,
                nextTick: jest.requireActual("vue").nextTick,
                bubble: jest.requireActual("../../../../../src/Communication/Chat/ChatShell/components/typingBubble.partial.obs").default,
                key: jest.requireActual("../../../../../src/Communication/Chat/ChatShell/composables/useFeedTail.partial").feedTailKey
            };
        });
        process.env.NODE_ENV = previousEnvironment;

        const logged = jest.spyOn(console, "error");
        const { tail, handed } = fakeTail();
        const host = document.createElement("div");
        document.body.append(host);
        const wrapper = production.mount(production.bubble, { props: { typing: twoTyping, isGroup: true }, attachTo: host, global: { provide: { [production.key as symbol]: tail } } });

        for (const typing of [annaTyping, twoTyping, annaTyping]) {
            await wrapper.setProps({ typing });
            await production.nextTick();
            expect(host.querySelectorAll(".chat-typing-face")).toHaveLength(2);
            handed.splice(0).forEach(going => going.done());
            await production.nextTick();
            expect(host.querySelectorAll(".chat-typing-face")).toHaveLength(1);
        }

        expect(host.querySelectorAll("[data-testid='typing-bubble']")).toHaveLength(1);
        expect(logged).not.toHaveBeenCalled();
        logged.mockRestore();
        wrapper.unmount();
    });
});

describe("the typing bubble's look", () => {
    const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/typingBubble.partial.obs"), "utf8");
    const style = source.slice(source.indexOf("<style"), source.indexOf("</style>"));
    const reduced = style.slice(style.indexOf("@media (prefers-reduced-motion: reduce)"));

    test("draws no motion of its own: its Transition plays no class, and no keyframe but its dots' bob is its own", () => {
        expect(source).toMatch(/<Transition :css="false" @leave="onLeave">/);
        expect(style).not.toMatch(/is-arriving|is-gone|is-replaced|chat-typing-in|transition:/);
        expect(style.match(/@keyframes [\w-]+/g)).toEqual(["@keyframes chat-typing-bob"]);
    });

    // Going in her run, her last message takes the run's face back in the same draw; the tail hides
    // the going bubble's face because a part drawn after takes its mark, so no rule of its own does.
    test("keeps no rule of its own for a face going: which face shows is the tail's", () => {
        expect(draw({ typing: annaTyping, isInRun: true }).find(".chat-typing-line .chat-typing-face").exists()).toBe(true);
        expect(style).not.toMatch(/is-leaving/);
    });

    // The bubble's layout is its own: nothing the feed or the thread draws is on it. It reads the
    // group measures they state for their groups, so it stands where a group or a row of her run would.
    test("is inset and spaced by the group measures, a row's gap when in her run", () => {
        expect(style).toMatch(/\.chat-typing\s*\{[^}]*position: relative;[^}]*margin-top: var\(--chat-grp-space\);[^}]*padding: 0 var\(--chat-grp-room\) 0 var\(--chat-grp-inset\);/);
        expect(style).toMatch(/\.chat-typing\.is-in-run\s*\{[^}]*margin-top: var\(--chat-grp-gap\);/);
        expect(style).toMatch(/\.chat-typing\.is-in-run \.chat-typing-bubble\s*\{[^}]*border-top-left-radius: var\(--chat-r-tail\);/);
        expect(source).not.toMatch(/inheritAttrs|\$attrs/);
    });

    // The feed and the thread state another person's group measures once, on their frame, and
    // their groups read them as the bubble does; neither puts a rule of its own on the bubble.
    test("the feed and the thread state the group measures once, read by their groups, and style nothing of the bubble's", () => {
        for (const [file, frame, inset, room] of [["messageFeed", ".chat-feed-frame", "20px", "64px"], ["threadDrawer", ".chat-th-body", "16px", "40px"]]) {
            const host = readFileSync(join(__dirname, `../../../../../src/Communication/Chat/ChatShell/components/${file}.partial.obs`), "utf8");
            const sheet = host.slice(host.indexOf("<style"), host.indexOf("</style>"));
            const escaped = frame.replace(".", "\\.");

            expect(sheet).toMatch(new RegExp(`${escaped}\\s*\\{[^}]*--chat-grp-space: 14px;[^}]*--chat-grp-gap: 2px;[^}]*--chat-grp-inset: ${inset};[^}]*--chat-grp-room: ${room};`));
            expect(sheet).toMatch(new RegExp(`${escaped}\\.is-phone:not\\(\\.is-community\\)\\s*\\{[^}]*--chat-grp-inset: 12px;[^}]*--chat-grp-room: 36px;`));
            expect(sheet).toMatch(/\n\.chat-grp\s*\{[^}]*gap: var\(--chat-grp-gap\);[^}]*margin-top: var\(--chat-grp-space\);[^}]*padding: 0 var\(--chat-grp-room\) 0 var\(--chat-grp-inset\);/);
            expect(sheet).not.toMatch(/chat-typing/);
            expect(host).not.toMatch(/<TypingBubble[^>]*class=/);
        }
    });

    test("its dots bob, and under reduced motion stand still at full strength", () => {
        expect(style).toMatch(/\.chat-typing-bubble i\s*\{[^}]*animation: chat-typing-bob var\(--chat-d-pulse\)/);
        expect(reduced).toMatch(/\.chat-typing-bubble i\s*\{[^}]*opacity: 1;[^}]*animation: none;/);
    });
});
