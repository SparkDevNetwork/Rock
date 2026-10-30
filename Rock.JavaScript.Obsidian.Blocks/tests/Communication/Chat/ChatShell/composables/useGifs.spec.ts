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

// GIF search: the session's one way to ask the platform's search, the switch that hides GIFs the
// moment the platform refuses them as off, and a picker's search, which waits for the person to
// stop typing, asks once for each query that is long enough, drops an answer a newer query
// overtook, and pages on. Laid out in two columns, each tile placed in the shorter.
import { computed, ref } from "vue";
import { createGifs, createGifSearch, gifSuggestions, GifSearchAnswer, Gifs, masonry, nextInMasonry } from "../../../../../src/Communication/Chat/ChatShell/composables/useGifs.partial";
import { CallResult } from "../../../../../src/Communication/Chat/ChatShell/platformCall.partial";
import { createSession, ExchangeResult } from "../../../../../src/Communication/Chat/ChatShell/composables/useSession.partial";
import { GifResult } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

function result(slug: string, width = 200, height = 112): GifResult {
    return {
        token: `token-${slug}`,
        slug,
        title: `Title ${slug}`,
        url: `https://static.klipy.com/ii/md/${slug}.mp4`,
        still: `https://static.klipy.com/ii/md/${slug}.jpg`,
        preview: { url: `https://static.klipy.com/ii/sm/${slug}.webp`, width, height },
        blur: "data:image/jpeg;base64,AAAA",
        width: 498,
        height: 280
    };
}

type Asked = { query: string, page: number, signal: AbortSignal | undefined, answer: (value: GifSearchAnswer) => void };

/** A search whose every ask the test answers by hand, in any order. */
function fakeGifs(): Gifs & { asked: Asked[] } {
    const asked: Asked[] = [];
    const on = ref(true);
    return {
        asked,
        isOn: () => on.value,
        search: (query, page, signal) => new Promise(resolve => asked.push({ query, page, signal, answer: resolve })),
        turnOff: () => {
            on.value = false;
        }
    };
}

async function settle(): Promise<void> {
    for (let i = 0; i < 5; i++) {
        await Promise.resolve();
    }
}

beforeEach(() => {
    jest.useFakeTimers();
});

afterEach(() => {
    jest.useRealTimers();
});

describe("createGifs", () => {
    function setup(answer: CallResult, isEnabled = true): { gifs: Gifs, calls: Array<{ path: string, body: unknown, signal: AbortSignal | undefined }> } {
        const calls: Array<{ path: string, body: unknown, signal: AbortSignal | undefined }> = [];
        const gifs = createGifs({
            callFunction: async (path, body, options) => {
                calls.push({ path, body, signal: options?.signal });
                return answer;
            },
            isEnabled: () => isEnabled
        });
        return { gifs, calls };
    }

    test("asks the platform's GIF search for a query's page, handing on the signal that ends it", async () => {
        const { gifs, calls } = setup({ ok: true, data: { items: [result("a")], has_next: true } });
        const controller = new AbortController();

        const answer = await gifs.search("thanks", 2, controller.signal);

        expect(calls).toEqual([{ path: "gif-search", body: { q: "thanks", page: 2 }, signal: controller.signal }]);
        expect(answer).toEqual({ ok: true, items: [result("a")], hasNext: true });
    });

    test("GIFs are on only while the church has them on and the platform has not refused them as off", async () => {
        expect(setup({ ok: true, data: {} }, false).gifs.isOn()).toBe(false);

        const { gifs } = setup({ ok: false, status: 403, error: { code: "rpc.gifs_off", severity: "failed" }, retryAfterSeconds: null });
        expect(gifs.isOn()).toBe(true);

        const answer = await gifs.search("thanks", 1);

        expect(answer).toMatchObject({ ok: false, error: { code: "rpc.gifs_off" } });
        expect(gifs.isOn()).toBe(false);
    });

    test("a refusal hands on its code and the wait it gave", async () => {
        const { gifs } = setup({ ok: false, status: 429, error: { code: "rpc.gif_search_busy", severity: "failed", text: "GIF search is busy. Try again in a moment." }, retryAfterSeconds: 9 });

        expect(await gifs.search("thanks", 1)).toEqual({ ok: false, error: { code: "rpc.gif_search_busy", severity: "failed", text: "GIF search is busy. Try again in a moment." }, retryAfterSeconds: 9 });
    });

    test("an answer of a shape this client cannot read is no results, never a failure", async () => {
        expect(await setup({ ok: true, data: null }).gifs.search("thanks", 1)).toEqual({ ok: true, items: [], hasNext: false });
    });
});

describe("the church switch through a real session", () => {
    /** A session whose every exchange answers the next of the given gifs flags. */
    function sessionAnswering(...flags: boolean[]): ReturnType<typeof createSession> {
        let calls = 0;
        return createSession({
            mintChurchToken: async () => ({ gate: "ok", churchToken: "church" }),
            exchange: async (): Promise<ExchangeResult> => ({
                ok: true,
                accessToken: "platform",
                expiresInSeconds: 300,
                settings: { flags: { gifs: flags[Math.min(calls++, flags.length - 1)] } }
            }),
            pushTokenToConnection: async () => undefined,
            random: () => 0.5,
            setTimer: () => 0,
            clearTimer: () => undefined
        });
    }

    test("a reader made while the church has GIFs off follows the switch on, and off again, at a refresh", async () => {
        const session = sessionAnswering(false, true, false);
        const gifs = createGifs({ callFunction: async () => ({ ok: true, data: {}, retryAfterSeconds: null }) as unknown as CallResult, isEnabled: () => session.settings().flags.gifs });
        await session.start();
        const isOffered = computed(() => gifs.isOn());
        expect(isOffered.value).toBe(false);

        await session.refresh();
        expect(isOffered.value).toBe(true);

        await session.refresh();
        expect(isOffered.value).toBe(false);
    });
});

describe("a picker's search", () => {
    test("before two characters it asks nothing and offers the suggestions", async () => {
        const gifs = fakeGifs();
        const search = createGifSearch(gifs);

        search.setQuery("t");
        jest.advanceTimersByTime(1000);

        expect(gifs.asked).toHaveLength(0);
        expect(search.status).toBe("idle");
        expect(gifSuggestions.length).toBeGreaterThanOrEqual(6);
    });

    test("asks once, 300ms after the typing stops, for the query as it then reads", () => {
        const gifs = fakeGifs();
        const search = createGifSearch(gifs);

        search.setQuery("th");
        jest.advanceTimersByTime(200);
        search.setQuery("tha");
        jest.advanceTimersByTime(299);
        expect(gifs.asked).toHaveLength(0);
        expect(search.status).toBe("loading");

        jest.advanceTimersByTime(1);
        expect(gifs.asked.map(a => [a.query, a.page])).toEqual([["tha", 1]]);
    });

    test("a suggestion is asked for at once", () => {
        const gifs = fakeGifs();
        const search = createGifSearch(gifs);

        search.setQuery("Amen", true);

        expect(gifs.asked.map(a => a.query)).toEqual(["Amen"]);
    });

    test("a newer query ends the ask before it, and an answer to the older one is dropped", async () => {
        const gifs = fakeGifs();
        const search = createGifSearch(gifs);

        search.setQuery("cats", true);
        search.setQuery("dogs", true);
        expect(gifs.asked[0].signal?.aborted).toBe(true);

        gifs.asked[1].answer({ ok: true, items: [result("dog")], hasNext: false });
        await settle();
        gifs.asked[0].answer({ ok: true, items: [result("cat")], hasNext: false });
        await settle();

        expect(search.items.map(item => item.slug)).toEqual(["dog"]);
        expect(search.status).toBe("results");
    });

    test("typing while an ask is out does not show the failure its end causes", async () => {
        const gifs = fakeGifs();
        const search = createGifSearch(gifs);

        search.setQuery("cats", true);
        search.setQuery("cat s");
        gifs.asked[0].answer({ ok: false, error: { code: "rpc.unavailable", severity: "failed" }, retryAfterSeconds: null });
        await settle();

        expect(search.status).toBe("loading");
    });

    test("a refusal as off from a search is final, not a failure to try again", async () => {
        const gifs = fakeGifs();
        const search = createGifSearch(gifs);

        search.setQuery("cats", true);
        gifs.asked[0].answer({ ok: false, error: { code: "rpc.gifs_off", severity: "failed" }, retryAfterSeconds: null });
        await settle();

        expect(search.status).not.toBe("error");
    });

    test("no results, a failure with Try again, and busy with the server's sentence each have their state", async () => {
        const gifs = fakeGifs();
        const search = createGifSearch(gifs);

        search.setQuery("qwzx", true);
        gifs.asked[0].answer({ ok: true, items: [], hasNext: false });
        await settle();
        expect(search.status).toBe("empty");

        search.setQuery("thanks", true);
        gifs.asked[1].answer({ ok: false, error: { code: "rpc.unavailable", severity: "failed" }, retryAfterSeconds: null });
        await settle();
        expect(search.status).toBe("error");

        search.retry();
        expect(gifs.asked.map(a => a.query)).toEqual(["qwzx", "thanks", "thanks"]);
        gifs.asked[2].answer({ ok: false, error: { code: "rpc.gif_search_busy", severity: "failed", text: "GIF search is busy. Try again in a moment." }, retryAfterSeconds: 5 });
        await settle();
        expect(search.status).toBe("busy");
        expect(search.busyText).toBe("GIF search is busy. Try again in a moment.");
    });

    test("busy with no sentence from the server says the client's own", async () => {
        const gifs = fakeGifs();
        const search = createGifSearch(gifs);

        search.setQuery("thanks", true);
        gifs.asked[0].answer({ ok: false, error: { code: "rpc.gif_search_busy", severity: "failed" }, retryAfterSeconds: null });
        await settle();

        expect(search.busyText).toBe("GIF search is busy. Try again in a moment.");
    });

    test("the next page is asked once while one is on its way, and adds to the results", async () => {
        const gifs = fakeGifs();
        const search = createGifSearch(gifs);
        search.setQuery("thanks", true);
        gifs.asked[0].answer({ ok: true, items: [result("a"), result("b")], hasNext: true });
        await settle();

        search.loadMore();
        search.loadMore();
        expect(gifs.asked.map(a => [a.query, a.page])).toEqual([["thanks", 1], ["thanks", 2]]);

        gifs.asked[1].answer({ ok: true, items: [result("c")], hasNext: false });
        await settle();
        search.loadMore();

        expect(search.items.map(item => item.slug)).toEqual(["a", "b", "c"]);
        expect(search.hasNext).toBe(false);
        expect(gifs.asked).toHaveLength(2);
    });

    test("a GIF a later page repeats is not added twice", async () => {
        const gifs = fakeGifs();
        const search = createGifSearch(gifs);
        search.setQuery("thanks", true);
        gifs.asked[0].answer({ ok: true, items: [result("a"), result("b")], hasNext: true });
        await settle();

        search.loadMore();
        gifs.asked[1].answer({ ok: true, items: [result("b"), result("c")], hasNext: false });
        await settle();

        expect(search.items.map(item => item.slug)).toEqual(["a", "b", "c"]);
    });

    test("stop ends the ask in flight and nothing is written after", async () => {
        const gifs = fakeGifs();
        const search = createGifSearch(gifs);
        search.setQuery("thanks", true);

        search.stop();
        gifs.asked[0].answer({ ok: true, items: [result("a")], hasNext: false });
        await settle();
        search.setQuery("more", true);

        expect(gifs.asked[0].signal?.aborted).toBe(true);
        expect(search.items).toEqual([]);
        expect(gifs.asked).toHaveLength(1);
    });
});

describe("the masonry", () => {
    test("places each tile in the shorter column, in KLIPY's order", () => {
        // heights as a share of the width: 1, 0.5, 0.5, 1
        const items = [result("a", 100, 100), result("b", 100, 50), result("c", 100, 50), result("d", 100, 100)];

        const columns = masonry(items);

        expect(columns.map(column => column.map(tile => tile.item.slug))).toEqual([["a", "d"], ["b", "c"]]);
        expect(columns[1].map(tile => tile.index)).toEqual([1, 2]);
    });

    test("Up and Down move within a column; Left and Right go to the tile beside it nearest by height", () => {
        // column one: a (0 to 1), d (1 to 2); column two: b (0 to .5), c (.5 to 1), e (1 to 1.5)
        const items = [result("a", 100, 100), result("b", 100, 50), result("c", 100, 50), result("d", 100, 100), result("e", 100, 50)];
        const columns = masonry(items);

        expect(nextInMasonry(columns, 0, "ArrowDown")).toBe(3);
        expect(nextInMasonry(columns, 3, "ArrowUp")).toBe(0);
        expect(nextInMasonry(columns, 0, "ArrowUp")).toBeNull();
        expect(nextInMasonry(columns, 3, "ArrowRight")).toBe(4);
        expect(nextInMasonry(columns, 2, "ArrowLeft")).toBe(0);
        expect(nextInMasonry(columns, 1, "ArrowLeft")).toBe(0);
        expect(nextInMasonry(columns, 0, "ArrowLeft")).toBeNull();
        expect(nextInMasonry(columns, 4, "ArrowRight")).toBeNull();
    });
});
