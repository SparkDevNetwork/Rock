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
// The chat's tooltip, placed beside its anchor. A tip hangs below its button, so the header's and
// a panel header's tips never cover the page header Rock draws above the chat; it goes above only
// where there is no room below, as every popover flips.
import { mount, VueWrapper } from "@vue/test-utils";
import { defineComponent, h, nextTick, ref } from "vue";
import { ChatTooltip, useTooltip } from "../../../../../src/Communication/Chat/ChatShell/ui/tooltip.partial";

/** A button with a plain tip, drawn where the tooltip places it. */
const Host = defineComponent({
    setup(_props, { expose }) {
        const anchor = ref<HTMLElement>();
        const tip = useTooltip(anchor);
        expose({ tip });

        return () => [
            h("button", { ref: anchor, "data-testid": "anchor", onMouseenter: tip.onPointerEnter }, "star"),
            tip.isTipShown.value
                ? h("div", { ref: tip.tipElement, class: "chat-tip", role: "tooltip", style: tip.floatingStyles.value }, "Star")
                : null
        ];
    }
});

let wrapper: VueWrapper | null = null;

/** Places the anchor at a spot in a 1000 by 800 window, which jsdom does not lay out. */
function placeAnchor(top: number): HTMLElement {
    Object.defineProperty(document.documentElement, "clientWidth", { configurable: true, value: 1000 });
    Object.defineProperty(document.documentElement, "clientHeight", { configurable: true, value: 800 });
    const anchor = document.querySelector<HTMLElement>("[data-testid='anchor']");
    if (!anchor) {
        throw new Error("no anchor");
    }

    anchor.getBoundingClientRect = (): DOMRect => ({ x: 400, y: top, left: 400, top, right: 432, bottom: top + 32, width: 32, height: 32, toJSON: () => ({}) }) as DOMRect;
    return anchor;
}

/** Rests the pointer on the anchor until the tip shows, then lets its placement land. */
async function showTip(anchor: HTMLElement): Promise<ChatTooltip> {
    const tip = (wrapper?.vm as unknown as { tip: ChatTooltip }).tip;
    anchor.dispatchEvent(new MouseEvent("mouseenter"));
    jest.advanceTimersByTime(600);
    await nextTick();
    jest.useRealTimers();
    await new Promise(resolve => setTimeout(resolve, 0));
    await nextTick();
    return tip;
}

beforeAll(() => {
    if (typeof window.ResizeObserver === "undefined") {
        window.ResizeObserver = class {
            observe(): void { /* jsdom lays nothing out. */ }
            unobserve(): void { /* As above. */ }
            disconnect(): void { /* As above. */ }
        } as unknown as typeof ResizeObserver;
    }
});

beforeEach(() => {
    jest.useFakeTimers();
    wrapper = mount(Host, { attachTo: document.body });
});

afterEach(() => {
    jest.useRealTimers();
    wrapper?.unmount();
    wrapper = null;
    document.body.innerHTML = "";
});

describe("where a tip is drawn", () => {
    it("hangs below its anchor, 8px under it", async () => {
        const tip = await showTip(placeAnchor(300));

        expect(tip.isTipShown.value).toBe(true);
        // the anchor's bottom is 332; a tip above would have its top under 300
        expect(tip.floatingStyles.value.top).toBe("340px");
    });

    it("goes above only where there is no room below", async () => {
        const tip = await showTip(placeAnchor(790));

        expect(parseFloat(tip.floatingStyles.value.top)).toBeLessThan(790);
    });
});
