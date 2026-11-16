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
// The focus trap, the one way a modal surface keeps the keyboard inside it: Tab wraps from its last
// control to its first and Shift+Tab back, focus starts inside as it opens and goes back to what
// opened it as it closes. Five surfaces kept their own copy, and they disagreed on what can take
// focus; these cells hold the one answer.
import { mount, VueWrapper } from "@vue/test-utils";
import { defineComponent, h, nextTick, PropType, ref } from "vue";
import { focusableSelector, isFocusQuiet, useFocusTrap } from "../../../../../src/Communication/Chat/ChatShell/ui/focusTrap.partial";

async function settle(): Promise<void> {
    await nextTick();
    await nextTick();
}

function find(id: string): HTMLElement {
    return document.querySelector<HTMLElement>(`[data-testid='${id}']`) as HTMLElement;
}

function tab(shiftKey = false): KeyboardEvent {
    const event = new KeyboardEvent("keydown", { key: "Tab", shiftKey, bubbles: true, cancelable: true });
    (document.activeElement ?? document.body).dispatchEvent(event);
    return event;
}

/** A modal holding what it is given, opened and closed by its prop. */
const Trap = defineComponent({
    props: {
        isOpen: { type: Boolean, default: true },
        children: { type: Function as PropType<() => ReturnType<typeof h>[]>, required: true },
        returnTo: { type: Function as PropType<() => HTMLElement | null>, default: undefined }
    },
    setup(props) {
        const element = ref<HTMLElement>();
        useFocusTrap(element, { returnTo: props.returnTo });
        return () => props.isOpen ? h("div", { ref: element, "data-testid": "trap" }, props.children()) : null;
    }
});

describe("the focus trap", () => {
    let wrapper: VueWrapper | null = null;

    afterEach(() => {
        wrapper?.unmount();
        wrapper = null;
        document.body.innerHTML = "";
    });

    function trap(children: () => ReturnType<typeof h>[], props: Record<string, unknown> = {}): VueWrapper {
        wrapper = mount(Trap, { props: { children, ...props }, attachTo: document.body });
        return wrapper;
    }

    const twoButtons = (): ReturnType<typeof h>[] => [h("button", { "data-testid": "first" }, "1"), h("button", { "data-testid": "last" }, "2")];

    it("starts on the first control as it opens", async () => {
        trap(twoButtons);
        await settle();
        expect(document.activeElement).toBe(find("first"));
    });

    it("starts quietly, so the first control's tip does not rise for a focus the person did not ask for", async () => {
        let wasQuiet: boolean | null = null;
        trap(() => [h("button", { "data-testid": "first", onFocus: () => {
            wasQuiet = isFocusQuiet();
        } }, "1")]);
        await settle();
        expect(wasQuiet).toBe(true);
    });

    it("starts on the control marked to start on, where there is one", async () => {
        trap(() => [h("button", "1"), h("button", { "data-testid": "marked", "data-autofocus": "" }, "2")]);
        await settle();
        expect(document.activeElement).toBe(find("marked"));
    });

    it("wraps Tab from the last control to the first, and Shift+Tab from the first to the last", async () => {
        trap(twoButtons);
        await settle();
        find("last").focus();
        expect(tab().defaultPrevented).toBe(true);
        expect(document.activeElement).toBe(find("first"));
        tab(true);
        expect(document.activeElement).toBe(find("last"));
    });

    it("leaves a Tab between two of its controls to the browser", async () => {
        trap(twoButtons);
        await settle();
        expect(tab().defaultPrevented).toBe(false);
    });

    it("counts a link as a control, so Tab from a link at the end still wraps", async () => {
        trap(() => [h("button", { "data-testid": "first" }, "1"), h("a", { href: "#", "data-testid": "link" }, "a link")]);
        await settle();
        find("link").focus();
        tab();
        expect(document.activeElement).toBe(find("first"));
    });

    it("passes over a control that is hidden or disabled", async () => {
        trap(() => [
            h("button", { "data-testid": "first" }, "1"),
            h("button", { "data-testid": "last" }, "2"),
            h("button", { disabled: true }, "off"),
            h("button", { style: "display: none" }, "hidden")
        ]);
        await settle();
        find("last").focus();
        tab();
        expect(document.activeElement).toBe(find("first"));
    });

    it("moves one by one among rows that are all out of the tab order, as a sheet's menu rows are", async () => {
        trap(() => ["a", "b", "c"].map(name => h("button", { tabindex: -1, "data-testid": name }, name)));
        await settle();
        find("a").focus();
        tab();
        expect(document.activeElement).toBe(find("b"));
        find("c").focus();
        tab();
        expect(document.activeElement).toBe(find("a"));
        tab(true);
        expect(document.activeElement).toBe(find("c"));
    });

    it("holds Tab where there is nothing to move to", async () => {
        trap(() => [h("p", { tabindex: -1, "data-testid": "words" }, "nothing to press")]);
        await settle();
        find("words").focus();
        expect(tab().defaultPrevented).toBe(true);
        expect(document.activeElement).toBe(find("words"));
    });

    it("gives focus back to what had it when the trap opened, as it closes", async () => {
        const opener = document.createElement("button");
        document.body.appendChild(opener);
        opener.focus();
        const drawn = trap(twoButtons);
        await settle();
        await drawn.setProps({ isOpen: false });
        await settle();
        expect(document.activeElement).toBe(opener);
    });

    it("gives focus back to the element it was told to, over what had it", async () => {
        const opener = document.createElement("button");
        const other = document.createElement("button");
        document.body.append(opener, other);
        opener.focus();
        const drawn = trap(twoButtons, { returnTo: () => other });
        await settle();
        await drawn.setProps({ isOpen: false });
        await settle();
        expect(document.activeElement).toBe(other);
    });

    it("has one selector for what can take focus, links and editable text included", () => {
        expect(focusableSelector).toMatch(/\[href\]/);
        expect(focusableSelector).toMatch(/contenteditable/);
        expect(focusableSelector).toMatch(/button:not\(\[disabled\]\)/);
    });
});
