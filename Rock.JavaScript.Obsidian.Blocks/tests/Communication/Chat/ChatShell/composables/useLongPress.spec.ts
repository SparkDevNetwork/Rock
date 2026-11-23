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
// A long press, the one owner of every hold on a touch screen: the message row and the reaction
// chip. A finger held shows the press from the fast duration and fires at the long-press duration;
// moving, the browser taking the touch, letting go early, a second finger, a scroll or the page
// going away each end it. The click that ends a fired hold is swallowed wherever it lands, once,
// and the next press anywhere disarms that, so a hold that ends with no click never eats a tap.
import { mount, VueWrapper } from "@vue/test-utils";
import { defineComponent, h, nextTick, ref } from "vue";
import { useLongPress } from "../../../../../src/Communication/Chat/ChatShell/composables/useLongPress.partial";

/** A pointer event as jsdom lacks one: a mouse event carrying the pointer's fields. */
function pointer(type: string, init: { x?: number, y?: number, pointerType?: string, isPrimary?: boolean, pointerId?: number, button?: number } = {}): Event {
    const event = new MouseEvent(type, { bubbles: true, cancelable: true, clientX: init.x ?? 10, clientY: init.y ?? 10, button: init.button ?? 0 });
    Object.defineProperty(event, "pointerType", { value: init.pointerType ?? "touch" });
    Object.defineProperty(event, "isPrimary", { value: init.isPrimary ?? true });
    Object.defineProperty(event, "pointerId", { value: init.pointerId ?? 1 });
    return event;
}

describe("a long press", () => {
    let wrapper: VueWrapper | null = null;
    const onPress = jest.fn();
    const isOn = ref(true);
    let pressing: { value: boolean } = { value: false };
    let listened: string[] = [];

    beforeEach(() => {
        jest.useFakeTimers();
        isOn.value = true;
        listened = [];
    });

    afterEach(() => {
        wrapper?.unmount();
        wrapper = null;
        onPress.mockReset();
        jest.useRealTimers();
        document.body.innerHTML = "";
    });

    /** A held element with a link and a plain word inside it, and an inner element that holds its own presses. */
    function held(options: { isPointerTaken?: (event: PointerEvent) => boolean } = {}): VueWrapper {
        const Held = defineComponent({
            setup() {
                const element = ref<HTMLElement>();
                const { isPressing } = useLongPress(element, { isOn: () => isOn.value, onPress, ...options });
                pressing = isPressing;
                return () => h("div", { ref: element, "data-testid": "held" }, [
                    h("a", { href: "#x", "data-testid": "link" }, "a link"),
                    h("span", { "data-testid": "word" }, "a word"),
                    h("span", { "data-long-press": "", "data-testid": "inner" }, "inner")
                ]);
            }
        });
        wrapper = mount(Held, { attachTo: document.body });
        const element = wrapper.element as HTMLElement;
        const add = element.addEventListener.bind(element);
        element.addEventListener = ((type: string, ...rest: never[]) => {
            listened.push(type);
            return add(type, ...rest);
        }) as typeof element.addEventListener;
        return wrapper;
    }

    function at(testId: string): HTMLElement {
        return document.querySelector<HTMLElement>(`[data-testid='${testId}']`) as HTMLElement;
    }

    test("shows the press from the fast duration and fires at the long-press duration, with what was under the finger", async () => {
        held();
        await nextTick();
        at("link").dispatchEvent(pointer("pointerdown"));

        jest.advanceTimersByTime(119);
        expect(pressing.value).toBe(false);
        jest.advanceTimersByTime(1);
        expect(pressing.value).toBe(true);
        jest.advanceTimersByTime(329);
        expect(onPress).not.toHaveBeenCalled();
        jest.advanceTimersByTime(1);
        expect(onPress).toHaveBeenCalledWith(at("link"));
    });

    test.each([
        ["a move past 8px", (): void => {
            window.dispatchEvent(pointer("pointermove", { x: 19, y: 10 }));
        }],
        ["the browser taking the touch to scroll", (): void => {
            window.dispatchEvent(pointer("pointercancel"));
        }],
        ["an early release", (): void => {
            window.dispatchEvent(pointer("pointerup"));
        }],
        ["a second finger", (): void => {
            at("word").dispatchEvent(pointer("pointerdown", { isPrimary: false, pointerId: 2 }));
        }],
        ["a scroll", (): void => {
            document.dispatchEvent(new Event("scroll"));
        }],
        ["the page going away", (): void => {
            document.dispatchEvent(new Event("visibilitychange"));
        }]
    ])("is ended by %s", async (_what, end) => {
        held();
        await nextTick();
        at("word").dispatchEvent(pointer("pointerdown"));
        jest.advanceTimersByTime(200);
        expect(pressing.value).toBe(true);

        end();
        jest.advanceTimersByTime(1000);

        expect(pressing.value).toBe(false);
        expect(onPress).not.toHaveBeenCalled();
    });

    test("a move inside 8px keeps it", async () => {
        held();
        await nextTick();
        at("word").dispatchEvent(pointer("pointerdown"));
        window.dispatchEvent(pointer("pointermove", { x: 15, y: 15 }));
        jest.advanceTimersByTime(450);
        expect(onPress).toHaveBeenCalledTimes(1);
    });

    test("swallows the click that ends a fired hold, wherever it lands, and no later tap", async () => {
        held();
        await nextTick();
        at("link").dispatchEvent(pointer("pointerdown"));
        jest.advanceTimersByTime(450);
        window.dispatchEvent(pointer("pointerup"));

        const release = new MouseEvent("click", { bubbles: true, cancelable: true });
        const reached = jest.fn();
        at("link").addEventListener("click", reached);
        at("link").dispatchEvent(release);
        expect(release.defaultPrevented).toBe(true);
        expect(reached).not.toHaveBeenCalled();

        const tap = new MouseEvent("click", { bubbles: true, cancelable: true });
        at("link").dispatchEvent(tap);
        expect(tap.defaultPrevented).toBe(false);
        expect(reached).toHaveBeenCalledTimes(1);
    });

    // A hold the browser ends with no click, then a press somewhere else, in a sheet drawn
    // outside the held element: that press disarms the swallow, so its own click goes through.
    test("a hold that ends with no click does not eat the next tap, even outside the element", async () => {
        held();
        await nextTick();
        at("word").dispatchEvent(pointer("pointerdown"));
        jest.advanceTimersByTime(450);
        window.dispatchEvent(pointer("pointercancel"));

        const elsewhere = document.createElement("button");
        document.body.appendChild(elsewhere);
        elsewhere.dispatchEvent(pointer("pointerdown"));
        const tap = new MouseEvent("click", { bubbles: true, cancelable: true });
        elsewhere.dispatchEvent(tap);

        expect(tap.defaultPrevented).toBe(false);
    });

    test("prevents the phone's own menu for the whole press, after the fire too, and not after it ends", async () => {
        held();
        await nextTick();
        const idle = new MouseEvent("contextmenu", { bubbles: true, cancelable: true });
        at("word").dispatchEvent(idle);
        expect(idle.defaultPrevented).toBe(false);

        at("word").dispatchEvent(pointer("pointerdown"));
        const during = new MouseEvent("contextmenu", { bubbles: true, cancelable: true });
        at("word").dispatchEvent(during);
        expect(during.defaultPrevented).toBe(true);

        jest.advanceTimersByTime(450);
        const afterFire = new MouseEvent("contextmenu", { bubbles: true, cancelable: true });
        at("word").dispatchEvent(afterFire);
        expect(afterFire.defaultPrevented).toBe(true);

        window.dispatchEvent(pointer("pointerup"));
        const ended = new MouseEvent("contextmenu", { bubbles: true, cancelable: true });
        at("word").dispatchEvent(ended);
        expect(ended.defaultPrevented).toBe(false);
    });

    test("leaves a press on an inner element that holds its own presses to that element", async () => {
        held();
        await nextTick();
        at("inner").dispatchEvent(pointer("pointerdown"));
        jest.advanceTimersByTime(1000);
        expect(onPress).not.toHaveBeenCalled();
        expect(pressing.value).toBe(false);
    });

    test("binds nothing while off, and only the press's start while idle", async () => {
        isOn.value = false;
        held();
        await nextTick();
        at("word").dispatchEvent(pointer("pointerdown"));
        jest.advanceTimersByTime(1000);
        expect(onPress).not.toHaveBeenCalled();
        expect((wrapper?.element as HTMLElement).hasAttribute("data-long-press")).toBe(false);

        isOn.value = true;
        await nextTick();
        expect(listened).toEqual(["pointerdown"]);
        expect((wrapper?.element as HTMLElement).hasAttribute("data-long-press")).toBe(true);
    });

    test("takes any primary pointer by default, and only what the owner takes when it says", async () => {
        held({ isPointerTaken: event => event.pointerType !== "mouse" });
        await nextTick();
        at("word").dispatchEvent(pointer("pointerdown", { pointerType: "mouse" }));
        jest.advanceTimersByTime(450);
        expect(onPress).not.toHaveBeenCalled();

        at("word").dispatchEvent(pointer("pointerdown", { pointerType: "pen" }));
        jest.advanceTimersByTime(450);
        expect(onPress).toHaveBeenCalledTimes(1);
    });
});
