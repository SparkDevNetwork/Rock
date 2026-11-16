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
// Marking messages seen, the one way: a row counts as seen once any of it is on screen in a page
// the reader is looking at, each row is watched once however often the list redraws, and a
// browser with no observer marks nothing rather than failing. The feed and the thread drawer each
// kept a copy, and only one of them survived a browser with no observer.
import { mount, VueWrapper } from "@vue/test-utils";
import { defineComponent, h, nextTick, ref } from "vue";
import { SeenObserver, useSeenObserver } from "../../../../../src/Communication/Chat/ChatShell/composables/useSeenObserver.partial";

const observers: FakeObserver[] = [];

class FakeObserver {
    public watched: Element[] = [];
    public isDisconnected = false;
    constructor(public callback: IntersectionObserverCallback, public options?: IntersectionObserverInit) {
        observers.push(this);
    }
    observe(element: Element): void {
        this.watched.push(element);
    }
    unobserve(): void { /* not used */ }
    disconnect(): void {
        this.isDisconnected = true;
    }
    see(target: Element, isIntersecting = true): void {
        this.callback([{ target, isIntersecting } as IntersectionObserverEntry], this as unknown as IntersectionObserver);
    }
}

async function settle(): Promise<void> {
    await nextTick();
    await nextTick();
}

describe("marking messages seen", () => {
    let wrapper: VueWrapper | null = null;
    let seen: SeenObserver;
    const original = window.IntersectionObserver;
    const onSeen = jest.fn();

    beforeEach(() => {
        observers.length = 0;
        onSeen.mockReset();
        window.IntersectionObserver = FakeObserver as unknown as typeof IntersectionObserver;
        Object.defineProperty(document, "visibilityState", { value: "visible", configurable: true });
    });

    afterEach(() => {
        wrapper?.unmount();
        wrapper = null;
        window.IntersectionObserver = original;
    });

    function feed(): HTMLElement {
        const Feed = defineComponent({
            setup() {
                const scroller = ref<HTMLElement>();
                seen = useSeenObserver(() => scroller.value ?? null, onSeen);
                return () => h("div", { ref: scroller, "data-testid": "scroller" }, [1, 2].map(id => h("div", { "data-message-id": id }, `message ${id}`)));
            }
        });
        wrapper = mount(Feed, { attachTo: document.body });
        return wrapper.element as HTMLElement;
    }

    it("watches within the list's own scroller", async () => {
        const scroller = feed();
        await settle();
        seen.observeAll(scroller, "[data-message-id]");
        expect(observers[0].options?.root).toBe(scroller);
    });

    it("says a row is seen once any of it is on screen", async () => {
        const scroller = feed();
        await settle();
        seen.observeAll(scroller, "[data-message-id]");
        const row = scroller.querySelector("[data-message-id='2']") as HTMLElement;
        observers[0].see(row);
        expect(onSeen).toHaveBeenCalledWith(row);
    });

    it("says nothing for a row on screen in a page the reader is not looking at", async () => {
        const scroller = feed();
        await settle();
        seen.observeAll(scroller, "[data-message-id]");
        Object.defineProperty(document, "visibilityState", { value: "hidden", configurable: true });
        observers[0].see(scroller.querySelector("[data-message-id='1']") as HTMLElement);
        expect(onSeen).not.toHaveBeenCalled();
    });

    it("says nothing for a row leaving the screen", async () => {
        const scroller = feed();
        await settle();
        seen.observeAll(scroller, "[data-message-id]");
        observers[0].see(scroller.querySelector("[data-message-id='1']") as HTMLElement, false);
        expect(onSeen).not.toHaveBeenCalled();
    });

    it("watches each row once, however often the list asks", async () => {
        const scroller = feed();
        await settle();
        seen.observeAll(scroller, "[data-message-id]");
        seen.observeAll(scroller, "[data-message-id]");
        expect(observers[0].watched).toHaveLength(2);
    });

    it("stops with the list", async () => {
        const scroller = feed();
        await settle();
        seen.observeAll(scroller, "[data-message-id]");
        wrapper?.unmount();
        wrapper = null;
        expect(observers[0].isDisconnected).toBe(true);
    });

    it("marks nothing, and throws nothing, in a browser with no observer", async () => {
        window.IntersectionObserver = undefined as unknown as typeof IntersectionObserver;
        const scroller = feed();
        await settle();
        expect(() => seen.observeAll(scroller, "[data-message-id]")).not.toThrow();
    });
});
