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
// The one row that loads more at a list's end: it asks for the next page as it comes into view
// near the end of its scroller, and is also a button the keyboard can press. It is busy while a
// page is out, says Try again after a failed page and asks again only when pressed or when it
// arrives anew, and asks again at once after a short page that leaves it in view. Members,
// Mentions and Threads each built this their own way.
import { mount, VueWrapper } from "@vue/test-utils";
import { h, nextTick } from "vue";
import LoadMoreRow from "../../../../../src/Communication/Chat/ChatShell/ui/loadMoreRow.partial.obs";

/** The observers made, so a test can say what came into view. */
const observers: FakeObserver[] = [];

class FakeObserver {
    public watched: Element[] = [];
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
        this.watched = [];
    }
    see(isIntersecting: boolean): void {
        this.callback(this.watched.map(target => ({ target, isIntersecting }) as IntersectionObserverEntry), this as unknown as IntersectionObserver);
    }
}

/** A page the test lands by hand. */
type Deferred = { promise: Promise<void>, resolve: () => void };

function deferred(): Deferred {
    let resolve = (): void => { /* Set below. */ };
    const promise = new Promise<void>(done => {
        resolve = done;
    });
    return { promise, resolve };
}

let wrapper: VueWrapper | null = null;
const original = window.IntersectionObserver;

function mountRow(props: Record<string, unknown>): VueWrapper {
    // inside a scroller, as a panel's body is
    wrapper = mount({
        render: () => h("div", { "data-chat-scroller": "" }, [h(LoadMoreRow, { name: "members", label: "Load more", ...props })])
    }, { attachTo: document.body });
    return wrapper;
}

async function settle(): Promise<void> {
    await nextTick();
    await nextTick();
    await new Promise(resolve => setTimeout(resolve, 0));
    await nextTick();
}

function button(): HTMLButtonElement {
    const found = document.querySelector<HTMLButtonElement>("[data-testid='members-more']");
    if (!found) {
        throw new Error("no button");
    }

    return found;
}

beforeEach(() => {
    observers.length = 0;
    window.IntersectionObserver = FakeObserver as unknown as typeof IntersectionObserver;
});

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
    window.IntersectionObserver = original;
    document.body.innerHTML = "";
});

describe("the load-more row", () => {
    it("asks for the next page as it comes into view, watched against its own scroller", async () => {
        const load = jest.fn(() => Promise.resolve());
        mountRow({ load });
        await settle();

        expect(observers[0].options?.root).toBe(document.querySelector("[data-chat-scroller]"));
        observers[0].see(true);

        expect(load).toHaveBeenCalledTimes(1);
    });

    it("is a button the keyboard can press, busy until the page lands", async () => {
        const page = deferred();
        const load = jest.fn(() => page.promise);
        mountRow({ load });
        await settle();

        expect(button().textContent?.trim()).toBe("Load more");
        button().click();
        await nextTick();

        expect(load).toHaveBeenCalledTimes(1);
        expect(button().getAttribute("aria-busy")).toBe("true");
        expect(button().textContent?.trim()).toBe("Loading");

        page.resolve();
        await settle();
        expect(button().getAttribute("aria-busy")).toBeNull();
    });

    it("asks again at once after a short page that leaves it in view", async () => {
        const first = deferred();
        const load = jest.fn().mockReturnValueOnce(first.promise).mockReturnValue(new Promise(() => { /* Never lands. */ }));
        mountRow({ load });
        await settle();

        observers[0].see(true);
        first.resolve();
        await settle();

        expect(load).toHaveBeenCalledTimes(2);
    });

    it("after a failed page says Try again with its words, and asks again only when pressed", async () => {
        // the list says its last page failed, as its state holds it
        const load = jest.fn(() => Promise.resolve());
        const w = mountRow({ load, isFailed: true, failure: "Couldn't load more." });
        await settle();

        expect(w.get("[data-testid='members-more-error']").text()).toBe("Couldn't load more.");
        expect(button().textContent?.trim()).toBe("Try again");
        observers[0].see(true);
        await settle();
        expect(load).not.toHaveBeenCalled();

        button().click();
        await nextTick();
        expect(load).toHaveBeenCalledTimes(1);
    });

    it("re-arms once the failure clears: arriving in view asks again", async () => {
        const load = jest.fn(() => Promise.resolve());
        const w = mount(LoadMoreRow, { props: { name: "members", label: "Load more", load, isFailed: true, failure: "Couldn't load more." }, attachTo: document.body });
        wrapper = w;
        await settle();
        observers[0].see(true);
        expect(load).not.toHaveBeenCalled();

        await w.setProps({ isFailed: false, failure: "" });
        await settle();

        expect(load).toHaveBeenCalledTimes(1);
    });

    it("holds its own ask while the list says a page is already out", async () => {
        const load = jest.fn(() => Promise.resolve());
        mountRow({ load, isLoading: true });
        await settle();
        observers[0].see(true);

        expect(load).not.toHaveBeenCalled();
        expect(button().getAttribute("aria-busy")).toBe("true");
    });

    it("says whether focus is on it, for the list to move focus to the rows it brought", async () => {
        const w = mountRow({ load: jest.fn(() => Promise.resolve()) });
        await settle();
        const row = w.findComponent(LoadMoreRow).vm as unknown as { hasFocus: () => boolean };

        expect(row.hasFocus()).toBe(false);
        button().focus();
        expect(row.hasFocus()).toBe(true);
    });
});
