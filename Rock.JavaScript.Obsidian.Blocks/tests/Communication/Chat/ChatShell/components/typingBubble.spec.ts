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
// A screen reader meets one hidden sentence, never announced. When a typist's message arrives the
// bubble goes at once and the message stands in its place; when they stop without sending it
// settles away.
import { readFileSync } from "fs";
import { join } from "path";
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import TypingBubble from "../../../../../src/Communication/Chat/ChatShell/components/typingBubble.partial.obs";
import Avatar from "../../../../../src/Communication/Chat/ChatShell/ui/avatar.partial.obs";
import GroupFace from "../../../../../src/Communication/Chat/ChatShell/ui/groupFace.partial.obs";
import type { Typing } from "../../../../../src/Communication/Chat/ChatShell/composables/usePresence.partial";
import { ChatPerson, TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

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

function message(id: number, sender: string): TimelineMessage {
    return { id, person_alias_guid: sender, body: `m${id}`, created_at: new Date(2026, 10, 9, 9, id % 60).toISOString() } as unknown as TimelineMessage;
}

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

    test("the typists changing redraws the same row rather than a new one", async () => {
        const wrapper = draw({ typing: annaTyping, isGroup: true });
        const row = wrapper.get("[data-testid='typing-bubble']").element;

        await wrapper.setProps({ typing: twoTyping });

        expect(wrapper.get("[data-testid='typing-bubble']").element).toBe(row);
        expect(wrapper.get("[data-testid='typing-names']").text()).toBe("Anna and Ben");
    });
});

describe("the typing bubble going", () => {
    test("a typist's message arriving takes its place at once", async () => {
        const wrapper = draw({ typing: annaTyping, newestMessage: message(10, ben) }, true);

        await wrapper.setProps({ typing: null, newestMessage: message(11, anna.toUpperCase()) });

        expect(rowIn(wrapper)).toBeNull();
    });

    test("one of several typists' message takes the place of the whole bubble when it ends their typing too", async () => {
        const wrapper = draw({ typing: twoTyping, isGroup: true, newestMessage: message(10, cal) }, true);

        await wrapper.setProps({ typing: null, newestMessage: message(11, ben) });

        expect(rowIn(wrapper)).toBeNull();
    });

    test("a typist who stops without sending settles away rather than vanishing", async () => {
        const wrapper = draw({ typing: annaTyping, newestMessage: message(10, ben) }, true);

        await wrapper.setProps({ typing: null });
        await nextTick();

        expect(rowIn(wrapper)?.classList.contains("is-leaving")).toBe(true);
    });

    test("someone else's message as the typist stops is no message of theirs, so the bubble still settles away", async () => {
        const wrapper = draw({ typing: annaTyping, newestMessage: message(10, ben) }, true);

        await wrapper.setProps({ typing: null, newestMessage: message(11, ben) });
        await nextTick();

        expect(rowIn(wrapper)?.classList.contains("is-leaving")).toBe(true);
    });

    // A message of hers drawn while she still types (her signal named it as followed) is older
    // than the bubble she then stops, so it is not what took the bubble's place.
    test("a typist's message that came while she kept typing does not count as taking the bubble's place when she stops", async () => {
        const wrapper = draw({ typing: annaTyping, newestMessage: message(10, ben) }, true);
        await wrapper.setProps({ newestMessage: message(11, anna) });

        await wrapper.setProps({ typing: null });
        await nextTick();

        expect(rowIn(wrapper)?.classList.contains("is-leaving")).toBe(true);
    });
});

describe("the typing bubble's motion", () => {
    const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/typingBubble.partial.obs"), "utf8");
    const style = source.slice(source.indexOf("<style"), source.indexOf("</style>"));
    const reduced = style.slice(style.indexOf("@media (prefers-reduced-motion: reduce)"));

    test("comes in as a message arrives, and settles away over the base time with its gap above", () => {
        expect(style).toMatch(/\.chat-typing\.is-arriving\s*\{[^}]*animation: chat-typing-in var\(--chat-t-msg\) var\(--chat-ease-in\)/);
        expect(style).toMatch(/@keyframes chat-typing-in\s*\{\s*from\s*\{[^}]*transform: translateY\(var\(--chat-rise\)\) scale\(var\(--chat-msg-scale\)\)/);
        expect(style).toMatch(/\.chat-typing\.is-leaving\s*\{[^}]*grid-template-rows var\(--chat-t-base\)/);
        expect(style).toMatch(/\.chat-typing\.is-gone\s*\{[^}]*grid-template-rows: 0fr;[^}]*margin-top: 0;/);
    });

    test("its dots bob, and under reduced motion stand still at full strength and a stop is not animated", () => {
        expect(style).toMatch(/\.chat-typing-bubble i\s*\{[^}]*animation: chat-typing-bob var\(--chat-d-pulse\)/);
        expect(reduced).toMatch(/\.chat-typing-bubble i\s*\{[^}]*opacity: 1;[^}]*animation: none;/);
        expect(reduced).toMatch(/\.chat-typing\.is-leaving\s*\{[^}]*transition: none;/);
    });
});
