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
// The icon button, mounted. The chat's chrome is icons only, so a button's words live in its label:
// a screen reader reads it, and a pointer that rests on the button sees it as a tooltip after half
// a second, late enough that passing over a row of buttons does not flicker tips.
import { mount, VueWrapper } from "@vue/test-utils";
import { readFileSync } from "fs";
import { resolve } from "path";
import { Component, h, nextTick } from "vue";
import Icon from "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs";
import IconButton from "../../../../../src/Communication/Chat/ChatShell/ui/iconButton.partial.obs";
import { partRuleBreaks, ruleOf, styleOf } from "./partStyle";

function tooltip(): Element | null {
    return document.querySelector("[role='tooltip']");
}

describe("the icon button", () => {
    let wrapper: VueWrapper | null = null;

    beforeEach(() => {
        jest.useFakeTimers();
    });

    afterEach(() => {
        wrapper?.unmount();
        wrapper = null;
        jest.useRealTimers();
    });

    function draw(): VueWrapper {
        // Attached, because a tooltip may be drawn outside the button so the chat's clipping cannot cut it.
        wrapper = mount(IconButton, { props: { icon: "bell", label: "Turn on notifications" }, attachTo: document.body });
        return wrapper;
    }

    test("names itself by its label and draws its icon", () => {
        const button = draw().get("button");

        expect(button.attributes("aria-label")).toBe("Turn on notifications");
        expect(button.find("svg").exists()).toBe(true);
    });

    // A holder places the button with its own scoped rules, as the composer puts a remove on a
    // staged GIF's corner; those rules reach the button only when it is the part's one root.
    test("takes a holder's class and the holder's style scope on its button", () => {
        const holder = {
            __scopeId: "data-v-holder",
            render: () => h("div", [h(IconButton, { icon: "x", label: "Remove GIF", class: "holder-remove" })])
        } as unknown as Component;
        wrapper = mount(holder, { attachTo: document.body });
        const button = wrapper.get("button");

        expect(button.classes()).toContain("holder-remove");
        expect(button.attributes()).toHaveProperty("data-v-holder");
    });

    test("cannot be drawn without a label", () => {
        const props = (IconButton as unknown as { props: Record<string, { required?: boolean }> }).props;

        expect(props.label.required).toBe(true);
    });

    test("shows its label as a tooltip once the pointer has rested 500ms, and not before", async () => {
        await draw().get("button").trigger("mouseenter");

        jest.advanceTimersByTime(499);
        await nextTick();
        expect(tooltip()).toBeNull();

        jest.advanceTimersByTime(1);
        await nextTick();
        expect(tooltip()?.textContent?.trim()).toBe("Turn on notifications");
    });

    // Every tip in the chat is drawn by the one Tip part, so they look and place alike.
    test("draws its tooltip with the shared tip part", async () => {
        const wrapper = draw();
        await wrapper.get("button").trigger("mouseenter");
        jest.advanceTimersByTime(500);
        await nextTick();

        expect(wrapper.findComponent({ name: "tip.partial" }).exists()).toBe(true);
        expect(tooltip()?.textContent?.trim()).toBe("Turn on notifications");
    });

    test("shows no tooltip when the pointer leaves before 500ms", async () => {
        const button = draw().get("button");

        await button.trigger("mouseenter");
        jest.advanceTimersByTime(300);
        await button.trigger("mouseleave");
        jest.advanceTimersByTime(500);
        await nextTick();

        expect(tooltip()).toBeNull();
    });

    test("hides the tooltip on a click and passes the click on", async () => {
        const wrapped = draw();
        const button = wrapped.get("button");

        await button.trigger("mouseenter");
        jest.advanceTimersByTime(500);
        await nextTick();
        expect(tooltip()).not.toBeNull();

        await button.trigger("click");
        await nextTick();

        expect(tooltip()).toBeNull();
        expect(wrapped.emitted("click")).toHaveLength(1);
    });

    describe("on focus", () => {
        // jsdom cannot match :focus-visible, so a browser's answer is stood in for.
        function focusVisibleIs(isVisible: boolean | "unsupported"): void {
            const real = Element.prototype.matches;
            jest.spyOn(Element.prototype, "matches").mockImplementation(function (this: Element, selector: string): boolean {
                if (selector === ":focus-visible") {
                    if (isVisible === "unsupported") {
                        throw new SyntaxError("not a valid selector");
                    }

                    return isVisible;
                }

                return real.call(this, selector);
            });
        }

        afterEach(() => {
            jest.restoreAllMocks();
        });

        test("shows no tooltip for a focus the page moved, as a closing panel returns to its opener", async () => {
            focusVisibleIs(false);
            const button = draw().get("button");

            await button.trigger("focus");
            jest.advanceTimersByTime(500);
            await nextTick();

            expect(tooltip()).toBeNull();
        });

        test("shows the tooltip for a keyboard focus", async () => {
            focusVisibleIs(true);
            const button = draw().get("button");

            await button.trigger("focus");
            jest.advanceTimersByTime(500);
            await nextTick();

            expect(tooltip()?.textContent?.trim()).toBe("Turn on notifications");
        });

        test("shows the tooltip where the browser cannot tell a keyboard focus", async () => {
            focusVisibleIs("unsupported");
            const button = draw().get("button");

            await button.trigger("focus");
            jest.advanceTimersByTime(500);
            await nextTick();

            expect(tooltip()).not.toBeNull();
        });
    });

    // A finger cannot rest on a button, and a tap that a phone reports as a pointer arriving would
    // leave a tip standing over the screen, as Back's did; the name is still read aloud.
    describe("on a screen that cannot hover", () => {
        const original = window.matchMedia;

        beforeEach(() => {
            window.matchMedia = ((query: string) => ({ media: query, matches: query === "(hover: none)" })) as unknown as typeof window.matchMedia;
        });

        afterEach(() => {
            window.matchMedia = original;
            jest.restoreAllMocks();
        });

        test("shows no tooltip for a tap, which a phone reports as the pointer arriving, and keeps its name", async () => {
            const button = draw().get("button");

            await button.trigger("pointerdown");
            await button.trigger("mouseenter");
            jest.advanceTimersByTime(1000);
            await nextTick();

            expect(tooltip()).toBeNull();
            expect(button.attributes("aria-label")).toBe("Turn on notifications");
        });

        test("shows no tooltip for a keyboard's focus either", async () => {
            jest.spyOn(Element.prototype, "matches").mockReturnValue(true);
            const button = draw().get("button");

            await button.trigger("focus");
            jest.advanceTimersByTime(1000);
            await nextTick();

            expect(tooltip()).toBeNull();
        });
    });
});

// Two sizes: the 32px button of every bar and panel, and the 22px close that sits inside a pill, a
// notice or a field. A failure is the Not sent marker: the red mark and its words on the line of
// the text around it, with no button look of its own, owned here rather than restyled from outside.
describe("the icon button's sizes and tones", () => {
    let wrapper: VueWrapper | null = null;

    afterEach(() => {
        wrapper?.unmount();
        wrapper = null;
    });

    test("is the middle size and the default tone unless told otherwise", () => {
        wrapper = mount(IconButton, { props: { icon: "bell", label: "Notifications" } });

        expect(wrapper.get("button").classes()).toEqual(expect.arrayContaining(["chat-icon-btn", "is-md"]));
        expect(wrapper.get("button").classes()).not.toContain("is-failure");
        expect(wrapper.findComponent(Icon).props("size")).toBe("md");
    });

    test("small is the close of a pill or a field, drawing the smallest icon unless given one", () => {
        wrapper = mount(IconButton, { props: { icon: "x", label: "Dismiss", size: "sm" } });

        expect(wrapper.get("button").classes()).toContain("is-sm");
        expect(wrapper.findComponent(Icon).props("size")).toBe("xs");
    });

    // A button whose menu has closed on a choice that opened something else, as the composer's "+"
    // whose GIF picker is up, keeps the look of a button whose menu is open.
    test("draws the open look while what it opened is up, as it does while its own menu is open", () => {
        wrapper = mount(IconButton, { props: { icon: "plus", label: "Add a GIF or file", isOpen: true } });

        expect(wrapper.get("button").classes()).toContain("is-open");
        expect(ruleOf(styleOf("iconButton.partial.obs"), ".chat-icon-btn.is-open")).toMatch(/background:\s*var\(--chat-brand-wash\)/);
    });

    // A close over a photo, as on a staged GIF, is drawn on the photo's own scrim in the media ink,
    // as the GIF badge beside it is; a hover keeps that look, since the chat's wash would grey the photo.
    test("media is the close over a photo: the media ink on the photo's scrim, kept on hover", () => {
        wrapper = mount(IconButton, { props: { icon: "x", label: "Remove GIF", size: "sm", tone: "media" } });
        const style = styleOf("iconButton.partial.obs");

        expect(wrapper.get("button").classes()).toContain("is-media");
        for (const selector of [".chat-icon-btn.is-media", ".chat-icon-btn.is-media:hover"]) {
            expect(ruleOf(style, selector)).toMatch(/color:\s*var\(--chat-media-fg\)/);
            expect(ruleOf(style, selector)).toMatch(/background:\s*var\(--chat-media-scrim\)/);
        }
    });

    test("a failure is marked as one, and keeps its name and its words for a screen reader", () => {
        wrapper = mount(IconButton, { props: { icon: "bang", label: "Not sent. Try again or delete", tone: "failure" }, slots: { default: "<span>Not sent</span>" } });
        const button = wrapper.get("button");

        expect(button.classes()).toContain("is-failure");
        expect(button.attributes("aria-label")).toBe("Not sent. Try again or delete");
        expect(button.text()).toBe("Not sent");
    });
});

describe("the icon button's look", () => {
    const style = styleOf("iconButton.partial.obs");
    const sheet = readFileSync(resolve(__dirname, "../../../../../../RockWeb/Styles/Blocks/Communication/Chat/chat.css"), "utf8");

    // 32px unless what holds it sets the size hook, as the sidebar's head does beside its field.
    test("is a 32px square with 8px corners, and its small size a 22px round close", () => {
        expect(ruleOf(style, ".chat-icon-btn")).toMatch(/width:\s*var\(--chat-icon-btn-s, 32px\)/);
        expect(ruleOf(style, ".chat-icon-btn")).toMatch(/height:\s*var\(--chat-icon-btn-s, 32px\)/);
        expect(ruleOf(style, ".chat-icon-btn")).toMatch(/border-radius:\s*var\(--chat-r-control\)/);
        expect(ruleOf(style, ".chat-icon-btn.is-sm")).toMatch(/width:\s*22px/);
        expect(ruleOf(style, ".chat-icon-btn.is-sm")).toMatch(/height:\s*22px/);
        expect(ruleOf(style, ".chat-icon-btn.is-sm")).toMatch(/border-radius:\s*var\(--chat-r-full\)/);
    });

    // A finger or a pointer needs 24 by 24 CSS pixels, and the words are one short line, so the
    // button takes the height and a matching negative margin keeps the line from growing.
    test("a failure is at least 24px tall on the text's baseline, its mark the danger disc, its words setting the baseline", () => {
        const failure = ruleOf(style, ".chat-icon-btn.is-failure");

        expect(failure).toMatch(/min-height:\s*24px/);
        expect(failure).toMatch(/align-self:\s*baseline/);
        expect(failure).toMatch(/margin-block:\s*-4px/);
        expect(failure).toMatch(/flex-wrap:\s*wrap/);
        expect(failure).toMatch(/align-content:\s*center/);
        expect(ruleOf(style, ".chat-icon-btn-mark")).toMatch(/background:\s*var\(--chat-danger\)/);
        expect(ruleOf(style, ".chat-icon-btn.is-failure :slotted(*)")).toMatch(/align-self:\s*baseline/);
        expect(ruleOf(style, ".chat-icon-btn.is-failure :slotted(*)")).toMatch(/line-height:\s*var\(--chat-fail-mark-s, 14px\)/);
    });

    test("the failure's look lives in the part alone: the token sheet no longer restyles the button", () => {
        expect(sheet).not.toMatch(/chat-fail-btn/);
    });

    test("holds no motion, priority, reach or capitals of its own", () => {
        expect(partRuleBreaks("iconButton.partial.obs")).toEqual([]);
    });
});
