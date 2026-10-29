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
import { nextTick } from "vue";
import IconButton from "../../../../../src/Communication/Chat/ChatShell/ui/iconButton.partial.obs";

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
});
