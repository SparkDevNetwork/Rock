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
// Search while typing, the one way: every search box waits the same pause after the last
// keystroke, asks once, cancels the ask a newer query replaces, and draws only the newest answer,
// however late an older one arrives. Six boxes each did this with their own pause and their own
// counter, and only one could cancel; these cells hold the one way.
import { effectScope } from "vue";
import { searchPause, useLatestSearch } from "../../../../../src/Communication/Chat/ChatShell/composables/useLatestSearch.partial";

/** An ask the test answers by hand, in any order. */
type Ask = { query: string, signal: AbortSignal, answer: (value: string[]) => void, fail: (error: unknown) => void };

function fetcher(): { asks: Ask[], fetch: (query: string, signal: AbortSignal) => Promise<string[]> } {
    const asks: Ask[] = [];
    return {
        asks,
        fetch: (query, signal) => new Promise<string[]>((resolve, reject) => {
            asks.push({ query, signal, answer: resolve, fail: reject });
        })
    };
}

async function flush(): Promise<void> {
    for (let i = 0; i < 5; i++) {
        await Promise.resolve();
    }
}

describe("search while typing", () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    it("waits one shared pause of 200 ms after the last keystroke, and asks once", () => {
        expect(searchPause).toBe(200);
        const { asks, fetch } = fetcher();
        const search = useLatestSearch(fetch);
        search.set("a");
        jest.advanceTimersByTime(150);
        search.set("an");
        jest.advanceTimersByTime(150);
        search.set("ann");
        jest.advanceTimersByTime(199);
        expect(asks).toHaveLength(0);
        jest.advanceTimersByTime(1);
        expect(asks.map(ask => ask.query)).toEqual(["ann"]);
    });

    it("is busy from the keystroke until the answer, and holds the answer and the query it answered", async () => {
        const { asks, fetch } = fetcher();
        const search = useLatestSearch(fetch);
        search.set("ann");
        expect(search.busy).toBe(true);
        jest.advanceTimersByTime(searchPause);
        asks[0].answer(["Ann"]);
        await flush();
        expect(search.busy).toBe(false);
        expect(search.results).toEqual(["Ann"]);
        expect(search.query).toBe("ann");
        expect(search.error).toBeNull();
    });

    it("draws only the newest answer, dropping an older one that arrives after it", async () => {
        const { asks, fetch } = fetcher();
        const onAnswer = jest.fn();
        const search = useLatestSearch(fetch, { onAnswer });
        search.set("an");
        jest.advanceTimersByTime(searchPause);
        search.set("ann");
        jest.advanceTimersByTime(searchPause);
        asks[1].answer(["Ann"]);
        await flush();
        asks[0].answer(["Andrew", "Ann"]);
        await flush();
        expect(search.results).toEqual(["Ann"]);
        expect(onAnswer).toHaveBeenCalledTimes(1);
        expect(onAnswer).toHaveBeenCalledWith(["Ann"], "ann", 1);
    });

    it("cancels the ask a newer query replaces", () => {
        const { asks, fetch } = fetcher();
        const search = useLatestSearch(fetch);
        search.set("an");
        jest.advanceTimersByTime(searchPause);
        search.set("ann");
        expect(asks[0].signal.aborted).toBe(true);
    });

    it("drops the answer to a cancelled ask, though the call answers as a failure", async () => {
        const { asks, fetch } = fetcher();
        const search = useLatestSearch(fetch);
        search.set("an");
        jest.advanceTimersByTime(searchPause);
        search.set("ann");
        asks[0].fail(new DOMException("aborted", "AbortError"));
        await flush();
        expect(search.error).toBeNull();
        expect(search.busy).toBe(true);
    });

    it("asks at once when told to, as an @ with nothing typed after it", () => {
        const { asks, fetch } = fetcher();
        const search = useLatestSearch(fetch);
        search.set("", { isAtOnce: true });
        expect(asks).toHaveLength(1);
    });

    it("holds a failure as its error, and the next keystroke clears it", async () => {
        const { asks, fetch } = fetcher();
        const search = useLatestSearch(fetch);
        search.set("ann");
        jest.advanceTimersByTime(searchPause);
        asks[0].fail(new Error("offline"));
        await flush();
        expect(search.busy).toBe(false);
        expect((search.error as Error).message).toBe("offline");
        search.set("anne");
        expect(search.error).toBeNull();
    });

    it("asks nothing for a query it is told is no search, and forgets the last answer", async () => {
        const { asks, fetch } = fetcher();
        const search = useLatestSearch(fetch, { isSearchable: query => query.trim().length >= 2 });
        search.set("ann");
        jest.advanceTimersByTime(searchPause);
        asks[0].answer(["Ann"]);
        await flush();
        search.set("a");
        jest.advanceTimersByTime(searchPause);
        expect(asks).toHaveLength(1);
        expect(search.results).toBeNull();
        expect(search.busy).toBe(false);
    });

    it("takes another pause where a box is told to", () => {
        const { asks, fetch } = fetcher();
        const search = useLatestSearch(fetch, { pause: 0 });
        search.set("ann");
        jest.advanceTimersByTime(0);
        expect(asks).toHaveLength(1);
    });

    it("stops with the part that made it: the pause and the ask out are both dropped", async () => {
        const { asks, fetch } = fetcher();
        const scope = effectScope();
        const search = scope.run(() => useLatestSearch(fetch))!;
        search.set("an");
        jest.advanceTimersByTime(searchPause);
        search.set("ann");
        scope.stop();
        jest.advanceTimersByTime(searchPause);
        expect(asks).toHaveLength(1);
        expect(asks[0].signal.aborted).toBe(true);
    });

    it("cancel drops the pause and the ask out, and is no longer busy", () => {
        const { asks, fetch } = fetcher();
        const search = useLatestSearch(fetch);
        search.set("ann");
        search.cancel();
        jest.advanceTimersByTime(searchPause);
        expect(asks).toHaveLength(0);
        expect(search.busy).toBe(false);
    });

    it("asks the next page of the query it holds, told which page each answer is", async () => {
        const pages: number[] = [];
        const onAnswer = jest.fn();
        const asks: Array<(value: string[]) => void> = [];
        const search = useLatestSearch((_query: string, _signal: AbortSignal, page: number) => {
            pages.push(page);
            return new Promise<string[]>(resolve => asks.push(resolve));
        }, { onAnswer });
        search.set("cats", { isAtOnce: true });
        asks[0](["a"]);
        await flush();
        search.more();
        expect(pages).toEqual([1, 2]);
        asks[1](["b"]);
        await flush();
        expect(onAnswer).toHaveBeenLastCalledWith(["b"], "cats", 2);
    });

    it("asks no next page while an ask is out, and drops a next page a newer query replaced", async () => {
        const onAnswer = jest.fn();
        const asks: Array<{ page: number, signal: AbortSignal, answer: (value: string[]) => void }> = [];
        const search = useLatestSearch((_query: string, signal: AbortSignal, page: number) => new Promise<string[]>(resolve => asks.push({ page, signal, answer: resolve })), { onAnswer });
        search.set("cats", { isAtOnce: true });
        search.more();
        expect(asks).toHaveLength(1);
        asks[0].answer(["a"]);
        await flush();
        search.more();
        search.set("dogs", { isAtOnce: true });
        expect(asks[1].signal.aborted).toBe(true);
        asks[1].answer(["late"]);
        await flush();
        expect(onAnswer).not.toHaveBeenCalledWith(["late"], "cats", 2);
    });
});
