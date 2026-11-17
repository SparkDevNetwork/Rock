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
// Loading more as the reader scrolls, the one way: a sentinel at the list's end asks for the next
// page as it comes into view, never twice for one arrival, never while a page is out or none is
// left, and again at once when a short page leaves the sentinel still in view. Lists used to page
// three ways (a button, a scroll distance and a sentinel); these cells hold the sentinel.
import { mount, VueWrapper } from "@vue/test-utils";
import { defineComponent, h, nextTick, ref } from "vue";
import { usePagingTrigger } from "../../../../../src/Communication/Chat/ChatShell/composables/usePagingTrigger.partial";

/** The observers made, so a test can say what came into view. */
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
    unobserve(element: Element): void {
        this.watched = this.watched.filter(one => one !== element);
    }
    disconnect(): void {
        this.isDisconnected = true;
        this.watched = [];
    }
    see(isIntersecting: boolean): void {
        this.callback(this.watched.map(target => ({ target, isIntersecting }) as IntersectionObserverEntry), this as unknown as IntersectionObserver);
    }
}

async function settle(): Promise<void> {
    await nextTick();
    await nextTick();
}

describe("loading more as the reader scrolls", () => {
    let wrapper: VueWrapper | null = null;
    const original = window.IntersectionObserver;

    beforeEach(() => {
        observers.length = 0;
        window.IntersectionObserver = FakeObserver as unknown as typeof IntersectionObserver;
    });

    afterEach(() => {
        wrapper?.unmount();
        wrapper = null;
        window.IntersectionObserver = original;
    });

    function list(loadMore: () => unknown, enabled = ref(true)): { enabled: typeof enabled } {
        const List = defineComponent({
            setup() {
                const sentinel = ref<HTMLElement>();
                usePagingTrigger(sentinel, loadMore, { enabled: () => enabled.value });
                return () => h("ul", [h("li", "row"), h("li", { ref: sentinel, "data-testid": "sentinel" })]);
            }
        });
        wrapper = mount(List, { attachTo: document.body });
        return { enabled };
    }

    it("asks for the next page as the sentinel comes into view", async () => {
        const loadMore = jest.fn();
        list(loadMore);
        await settle();
        observers[0].see(true);
        expect(loadMore).toHaveBeenCalledTimes(1);
    });

    it("asks once for one arrival, though the observer reports it twice before the page is out", async () => {
        const loadMore = jest.fn();
        list(loadMore);
        await settle();
        observers[0].see(true);
        observers[0].see(true);
        expect(loadMore).toHaveBeenCalledTimes(1);
    });

    it("asks nothing while a page is out or none is left", async () => {
        const loadMore = jest.fn();
        list(loadMore, ref(false));
        await settle();
        observers[0].see(true);
        expect(loadMore).not.toHaveBeenCalled();
    });

    it("asks again once a page lands and leaves the sentinel still in view, as a short page does", async () => {
        const loadMore = jest.fn();
        const { enabled } = list(loadMore);
        await settle();
        observers[0].see(true);
        enabled.value = false;
        await settle();
        enabled.value = true;
        await settle();
        expect(loadMore).toHaveBeenCalledTimes(2);
    });

    it("asks again for a new arrival after the sentinel left view", async () => {
        const loadMore = jest.fn();
        list(loadMore);
        await settle();
        observers[0].see(true);
        observers[0].see(false);
        observers[0].see(true);
        expect(loadMore).toHaveBeenCalledTimes(2);
    });

    it("waits for a page asked as a promise to settle before asking again", async () => {
        let land: () => void = () => undefined;
        const loadMore = jest.fn(() => new Promise<void>(resolve => {
            land = resolve;
        }));
        list(loadMore);
        await settle();
        observers[0].see(true);
        observers[0].see(false);
        observers[0].see(true);
        expect(loadMore).toHaveBeenCalledTimes(1);
        land();
        await settle();
        expect(loadMore).toHaveBeenCalledTimes(2);
    });

    // A failed page is the list's to show, with a control that asks again; asking again at once
    // would only repeat the failure, over and over, while the sentinel stays in view.
    it("lets a page that failed go without asking again at once, and asks on the sentinel's next arrival", async () => {
        // fails once; an ask after it waits, so an ask repeated at once is counted, not run forever
        const loadMore = jest.fn((): Promise<void> => new Promise(() => undefined));
        loadMore.mockImplementationOnce(() => Promise.reject(new Error("offline")));
        list(loadMore);
        await settle();
        observers[0].see(true);
        await settle();
        expect(loadMore).toHaveBeenCalledTimes(1);

        observers[0].see(false);
        observers[0].see(true);
        await settle();
        expect(loadMore).toHaveBeenCalledTimes(2);
    });

    it("stops watching when the list goes", async () => {
        list(jest.fn());
        await settle();
        wrapper?.unmount();
        wrapper = null;
        expect(observers[0].isDisconnected).toBe(true);
    });

    // A list that went, as a conversation's feed does when the person opens another, asks nothing
    // once its last page settles: the loader reads the conversation open now, not the one the
    // page was for.
    it("asks nothing more once the list has gone, when its last page settles", async () => {
        let land: () => void = () => undefined;
        const loadMore = jest.fn(() => new Promise<void>(resolve => {
            land = resolve;
        }));
        list(loadMore);
        await settle();
        observers[0].see(true);
        wrapper?.unmount();
        wrapper = null;

        land();
        await settle();

        expect(loadMore).toHaveBeenCalledTimes(1);
    });

    it("does nothing in a browser with no observer", async () => {
        window.IntersectionObserver = undefined as unknown as typeof IntersectionObserver;
        const loadMore = jest.fn();
        list(loadMore);
        await settle();
        expect(loadMore).not.toHaveBeenCalled();
    });
});
