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
import { ChatTooltip, isCutShort, useTooltip } from "../../../../../src/Communication/Chat/ChatShell/ui/tooltip.partial";
import { usePopover } from "../../../../../src/Communication/Chat/ChatShell/ui/popover.partial";

// The real placing, watched, so a cell can tell whether a tip has made one yet.
jest.mock("../../../../../src/Communication/Chat/ChatShell/ui/popover.partial", () => {
    const actual = jest.requireActual("../../../../../src/Communication/Chat/ChatShell/ui/popover.partial");
    return { ...actual, usePopover: jest.fn(actual.usePopover) };
});

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

// Where nothing can hover, a finger's tap arrives as the pointer entering, and a tip shown then
// stands over the screen until something else is tapped; every tip in the chat goes through here.
describe("on a screen that cannot hover", () => {
    const original = window.matchMedia;

    afterEach(() => {
        window.matchMedia = original;
    });

    it("never shows a tip, for the pointer arriving or for a focus", async () => {
        window.matchMedia = ((query: string) => ({ media: query, matches: query === "(hover: none)" })) as unknown as typeof window.matchMedia;
        const tip = (wrapper?.vm as unknown as { tip: ChatTooltip }).tip;

        tip.onPointerEnter();
        jest.advanceTimersByTime(1000);
        tip.onFocus();
        jest.advanceTimersByTime(1000);
        await nextTick();

        expect(tip.isTipShown.value).toBe(false);
    });
});

// A name or a file's words that fit say everything already, so their tip is kept away; the
// sidebar's names and the tray's chips ask the same question of their text.
describe("whether words were cut short", () => {
    /** An element whose words take the given width inside the given room. */
    function wordsIn(wordsWidth: number, roomWidth: number): HTMLElement {
        const element = document.createElement("span");
        Object.defineProperty(element, "scrollWidth", { value: wordsWidth });
        Object.defineProperty(element, "clientWidth", { value: roomWidth });

        return element;
    }

    it("says yes only where the words are wider than their room", () => {
        expect(isCutShort(wordsIn(180, 120))).toBe(true);
        expect(isCutShort(wordsIn(120, 120))).toBe(false);
        expect(isCutShort(wordsIn(80, 120))).toBe(false);
    });

    it("says no while nothing is drawn yet", () => {
        expect(isCutShort(undefined)).toBe(false);
    });
});

// A long channel draws a tip anchor on every message's time and body, hundreds of them, and the
// pointer rests on a handful. Each tip makes its placing only when one may show.
describe("when a tip makes its placing", () => {
    const placings = usePopover as jest.Mock;
    const original = window.matchMedia;

    beforeEach(() => {
        placings.mockClear();
        wrapper?.unmount();
        wrapper = mount(Host, { attachTo: document.body });
    });

    afterEach(() => {
        window.matchMedia = original;
    });

    it("makes none at mount", () => {
        expect(placings).not.toHaveBeenCalled();
    });

    it("makes one on the pointer's first arrival, once, and still waits the delay", async () => {
        const tip = (wrapper?.vm as unknown as { tip: ChatTooltip }).tip;
        const anchor = placeAnchor(300);

        anchor.dispatchEvent(new MouseEvent("mouseenter"));
        expect(placings).toHaveBeenCalledTimes(1);
        jest.advanceTimersByTime(100);
        await nextTick();
        expect(tip.isTipShown.value).toBe(false);

        const shown = await showTip(anchor);
        expect(shown.isTipShown.value).toBe(true);
        expect(placings).toHaveBeenCalledTimes(1);
    });

    it("makes one for a keyboard's focus", () => {
        const tip = (wrapper?.vm as unknown as { tip: ChatTooltip }).tip;

        placeAnchor(300).focus();
        tip.onFocus();

        expect(placings).toHaveBeenCalledTimes(1);
    });

    it("makes none where nothing can hover", () => {
        window.matchMedia = ((query: string) => ({ media: query, matches: query === "(hover: none)" })) as unknown as typeof window.matchMedia;
        const tip = (wrapper?.vm as unknown as { tip: ChatTooltip }).tip;

        tip.onPointerEnter();
        tip.onFocus();

        expect(placings).not.toHaveBeenCalled();
    });
});
