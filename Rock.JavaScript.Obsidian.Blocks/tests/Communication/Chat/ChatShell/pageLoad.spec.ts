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
// The page load: the first channel's history, its live join and the sidebar start at once,
// never history after the sidebar. A link wins; else the channel this browser last opened for
// this person; else, only on a first visit, the sidebar's first row.
import { nextTick } from "vue";
import {
    createPageReveal,
    faceGraceMs,
    openChannel,
    OpenOutcome,
    readRememberedChannel,
    readRememberedKind,
    rememberedChannelKey,
    revealCeilingMs,
    skeletonWaitMs,
    startPageLoad,
    StorageLike,
    writeRememberedChannel,
    writeRememberedKind
} from "../../../../src/Communication/Chat/ChatShell/pageLoad.partial";
import { SidebarRow } from "../../../../src/Communication/Chat/ChatShell/types.partial";

const tenant = "10000000-0000-4000-8000-00000000000A";
const alias = "A0000001-0000-4000-8000-000000000000";
const linked = "c0000001-0000-4000-8000-000000000000";
const remembered = "c0000002-0000-4000-8000-000000000000";
const first = "c0000003-0000-4000-8000-000000000000";
const second = "c0000004-0000-4000-8000-000000000000";

type Deferred<T> = { promise: Promise<T>, resolve: (value: T) => void, reject: (error: unknown) => void };

function deferred<T>(): Deferred<T> {
    let resolve!: (value: T) => void;
    let reject!: (error: unknown) => void;
    const promise = new Promise<T>((a, b) => {
        resolve = a;
        reject = b;
    });
    return { promise, resolve, reject };
}

function rows(...ids: string[]): SidebarRow[] {
    return ids.map(id => ({ channel_id: id } as SidebarRow));
}

describe("the remembered channel", () => {
    test("is kept per church and per person, in lower case", () => {
        const key = rememberedChannelKey(tenant, alias);

        expect(key).toBe(key.toLowerCase());
        expect(key).toContain("10000000-0000-4000-8000-00000000000a");
        expect(key).toContain("a0000001-0000-4000-8000-000000000000");
        expect(rememberedChannelKey(tenant, "a0000009-0000-4000-8000-000000000000")).not.toBe(key);
    });

    test("round-trips through storage", () => {
        const values: Record<string, string> = {};
        const storage: StorageLike = { getItem: k => values[k] ?? null, setItem: (k, v) => values[k] = v };

        writeRememberedChannel(storage, "k", remembered);

        expect(readRememberedChannel(storage, "k")).toBe(remembered);
    });

    test("blocked or missing storage only loses the memory", () => {
        const blocked: StorageLike = {
            getItem: () => {
                throw new Error("SecurityError");
            },
            setItem: () => {
                throw new Error("QuotaExceededError");
            }
        };

        expect(() => writeRememberedChannel(blocked, "k", remembered)).not.toThrow();
        expect(readRememberedChannel(blocked, "k")).toBeNull();
        expect(readRememberedChannel(null, "k")).toBeNull();
        expect(() => writeRememberedChannel(null, "k", remembered)).not.toThrow();
    });
});

describe("openChannel", () => {
    test("joins and fetches history together, neither waiting on the other", async () => {
        const history = deferred<void>();
        const log: string[] = [];

        const opening = openChannel({
            join: id => log.push(`join ${id}`),
            loadNewest: id => {
                log.push(`history ${id}`);
                return history.promise;
            },
            isRefusal: () => false,
            remember: id => log.push(`remember ${id}`)
        }, linked);

        expect(log).toEqual([`join ${linked}`, `history ${linked}`]);
        history.resolve();
        expect(await opening).toBe("opened");
        expect(log).toEqual([`join ${linked}`, `history ${linked}`, `remember ${linked}`]);
    });

    test("a channel the platform refuses is not remembered", async () => {
        const log: string[] = [];

        const outcome = await openChannel({
            join: () => undefined,
            loadNewest: () => Promise.reject({ message: "authz.not_readable" }),
            isRefusal: error => (error as { message: string }).message === "authz.not_readable",
            remember: id => log.push(`remember ${id}`)
        }, remembered);

        expect(outcome).toBe("refused");
        expect(log).toEqual([]);
    });

    test("any other failure is a failure, not a refusal", async () => {
        const outcome = await openChannel({
            join: () => undefined,
            loadNewest: () => Promise.reject(new TypeError("Failed to fetch")),
            isRefusal: () => false,
            remember: () => undefined
        }, remembered);

        expect(outcome).toBe("failed");
    });
});

describe("startPageLoad", () => {
    function harness(options: { linked?: string | null, remembered?: string | null, outcomes?: Record<string, OpenOutcome> }): {
        run: () => Promise<string | null>,
        sidebar: Deferred<SidebarRow[]>,
        log: string[]
    } {
        const sidebar = deferred<SidebarRow[]>();
        const log: string[] = [];
        const run = (): Promise<string | null> => startPageLoad({
            linkedChannelId: options.linked ?? null,
            rememberedChannelId: options.remembered ?? null,
            loadSidebar: () => {
                log.push("sidebar");
                return sidebar.promise;
            },
            open: async id => {
                log.push(`open ${id}`);
                return options.outcomes?.[id] ?? "opened";
            }
        });
        return { run, sidebar, log };
    }

    test("with a linked channel, the sidebar and the channel start before either returns", async () => {
        const h = harness({ linked, remembered });

        const loading = h.run();

        expect(h.log.sort()).toEqual([`open ${linked}`, "sidebar"].sort());
        h.sidebar.resolve(rows(first));
        expect(await loading).toBe(linked);
    });

    test("with no link, the remembered channel starts beside the sidebar", async () => {
        const h = harness({ remembered });

        const loading = h.run();

        expect(h.log.sort()).toEqual([`open ${remembered}`, "sidebar"].sort());
        h.sidebar.resolve(rows(first));
        expect(await loading).toBe(remembered);
    });

    test("on a first visit the sidebar's first row opens once it arrives", async () => {
        const h = harness({});

        const loading = h.run();
        expect(h.log).toEqual(["sidebar"]);
        h.sidebar.resolve(rows(first, second));

        expect(await loading).toBe(first);
        expect(h.log).toEqual(["sidebar", `open ${first}`]);
    });

    test("a remembered channel the person can no longer read falls back to the first row", async () => {
        const h = harness({ remembered, outcomes: { [remembered]: "refused" } });

        const loading = h.run();
        h.sidebar.resolve(rows(first, second));

        expect(await loading).toBe(first);
        expect(h.log).toContain(`open ${first}`);
    });

    test("a refused first row is not retried with the same channel", async () => {
        const h = harness({ remembered: first, outcomes: { [first]: "refused" } });

        const loading = h.run();
        h.sidebar.resolve(rows(first, second));

        expect(await loading).toBe(second);
    });

    test("a channel the person has already moved away from stops the page load", async () => {
        const h = harness({ linked, outcomes: { [linked]: "superseded" } });

        const loading = h.run();
        h.sidebar.resolve(rows(first, second));

        expect(await loading).toBeNull();
        expect(h.log).not.toContain(`open ${first}`);
    });

    test("an empty sidebar on a first visit opens nothing", async () => {
        const h = harness({});

        const loading = h.run();
        h.sidebar.resolve([]);

        expect(await loading).toBeNull();
    });
});

// The first screen is drawn once: the frame stands at mount, and every region crossfades in
// together when the first screen is ready, or at the ceiling for whatever has arrived.
describe("the page reveal", () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    /** Every gate has answered. */
    function answerAll(reveal: ReturnType<typeof createPageReveal>): void {
        reveal.sidebarAnswered();
        reveal.historyAnswered();
        reveal.factsAnswered();
    }

    /** The render the last answer caused, where the faces it brought mount and report themselves. */
    const rendered = (): Promise<void> => nextTick();

    test("shows nothing revealed before the sidebar, the first page and the facts have all answered", async () => {
        const reveal = createPageReveal({ kind: "channel" });

        reveal.historyAnswered();
        reveal.factsAnswered();
        await rendered();
        jest.advanceTimersByTime(revealCeilingMs - 1);

        expect(reveal.state.isSidebarRevealed).toBe(false);
        expect(reveal.state.isMainRevealed).toBe(false);

        reveal.sidebarAnswered();
        await rendered();

        expect(reveal.state.isSidebarRevealed).toBe(true);
        expect(reveal.state.isMainRevealed).toBe(true);
        expect(reveal.state.isReleased).toBe(true);
    });

    test("reveals every region in the same tick once the first screen is ready", async () => {
        const reveal = createPageReveal({ kind: "channel" });
        reveal.sidebarAnswered();
        reveal.historyAnswered();

        expect(reveal.state.isSidebarRevealed).toBe(false);
        expect(reveal.state.isMainRevealed).toBe(false);

        reveal.factsAnswered();
        await rendered();

        expect(reveal.state.isSidebarRevealed).toBe(true);
        expect(reveal.state.isMainRevealed).toBe(true);
    });

    test("holds the reveal for a first-screen face still decoding, but no longer than the grace", async () => {
        const reveal = createPageReveal({ kind: "channel" });
        reveal.holdFace(new Promise(() => undefined));
        answerAll(reveal);
        await rendered();

        jest.advanceTimersByTime(faceGraceMs - 1);
        expect(reveal.state.isMainRevealed).toBe(false);

        jest.advanceTimersByTime(1);
        expect(reveal.state.isMainRevealed).toBe(true);
        expect(reveal.state.isSidebarRevealed).toBe(true);
    });

    test("holds the reveal for a face that mounts after the last answer, which renders on the next tick", async () => {
        const reveal = createPageReveal({ kind: "channel" });

        // The data that arrived with the last answer queued its render first: the region's faces
        // mount in it and report themselves, after the answer was called.
        void nextTick(() => reveal.holdFace(new Promise(() => undefined)));
        answerAll(reveal);
        await rendered();

        expect(reveal.state.isReleased).toBe(false);
        jest.advanceTimersByTime(faceGraceMs - 1);
        expect(reveal.state.isMainRevealed).toBe(false);

        jest.advanceTimersByTime(1);
        expect(reveal.state.isMainRevealed).toBe(true);
    });

    test("releases on the next tick when no face is held, with no timer", async () => {
        const reveal = createPageReveal({ kind: "channel" });
        answerAll(reveal);

        await rendered();

        expect(reveal.state.isReleased).toBe(true);
        expect(jest.getTimerCount()).toBe(0);
    });

    test("reveals at once when the held faces decode inside the grace", async () => {
        const reveal = createPageReveal({ kind: "channel" });
        let decoded: () => void = () => undefined;
        reveal.holdFace(new Promise<void>(resolve => {
            decoded = resolve;
        }));
        answerAll(reveal);
        await rendered();
        expect(reveal.state.isMainRevealed).toBe(false);

        decoded();
        await Promise.resolve();
        await Promise.resolve();

        expect(reveal.state.isMainRevealed).toBe(true);
    });

    test("a face that decodes before the last answer does not release the page early", async () => {
        const reveal = createPageReveal({ kind: "channel" });
        let decoded: () => void = () => undefined;
        reveal.holdFace(new Promise<void>(resolve => {
            decoded = resolve;
        }));
        reveal.sidebarAnswered();
        reveal.historyAnswered();
        decoded();
        await Promise.resolve();
        await Promise.resolve();

        void nextTick(() => reveal.holdFace(new Promise(() => undefined)));
        reveal.factsAnswered();
        await rendered();

        expect(reveal.state.isReleased).toBe(false);
    });

    test("at the ceiling reveals every region, so a call that never answers cannot lock the column", async () => {
        const reveal = createPageReveal({ kind: "channel" });
        reveal.sidebarAnswered();

        jest.advanceTimersByTime(revealCeilingMs);

        expect(reveal.state.isReleased).toBe(true);
        expect(reveal.state.isSidebarRevealed).toBe(true);
        expect(reveal.state.isMainRevealed).toBe(true);
        expect(reveal.state.isSkeletonShown).toBe(false);
        expect(jest.getTimerCount()).toBe(0);

        // A late answer changes nothing and throws nothing.
        reveal.historyAnswered();
        reveal.factsAnswered();
        expect(reveal.state.isMainRevealed).toBe(true);
    });

    test("at the ceiling reveals even a sidebar that never answered, which draws its own loading rows", () => {
        const reveal = createPageReveal({ kind: "channel" });

        jest.advanceTimersByTime(revealCeilingMs);

        expect(reveal.state.isSidebarRevealed).toBe(true);
        expect(reveal.state.isMainRevealed).toBe(true);
    });

    test("a failure reveals what has arrived at once, without the face grace, and a region still waiting reveals when it lands", () => {
        const reveal = createPageReveal({ kind: "channel" });
        reveal.holdFace(new Promise(() => undefined));
        reveal.sidebarAnswered();

        reveal.failed();

        expect(reveal.state.isSidebarRevealed).toBe(true);
        expect(reveal.state.isMainRevealed).toBe(false);

        reveal.historyAnswered();
        reveal.factsAnswered();
        expect(reveal.state.isMainRevealed).toBe(true);
    });

    test("nothing to open answers the first page and the facts together", async () => {
        const reveal = createPageReveal({ kind: "channel" });
        reveal.sidebarAnswered();

        reveal.nothingToOpen();
        await rendered();

        expect(reveal.state.isMainRevealed).toBe(true);
    });

    test("draws no skeleton marks under the wait, and draws them after it", () => {
        const reveal = createPageReveal({ kind: "channel" });

        jest.advanceTimersByTime(skeletonWaitMs - 1);
        expect(reveal.state.isSkeletonShown).toBe(false);

        jest.advanceTimersByTime(1);
        expect(reveal.state.isSkeletonShown).toBe(true);
    });

    test("a load that is ready inside the wait never draws the marks", async () => {
        const reveal = createPageReveal({ kind: "channel" });
        answerAll(reveal);
        await rendered();

        jest.advanceTimersByTime(skeletonWaitMs * 2);

        expect(reveal.state.isSkeletonShown).toBe(false);
    });

    test("takes the remembered room's look, which the skeleton's shape follows", () => {
        expect(createPageReveal({ kind: "direct" }).state.kind).toBe("direct");
        expect(createPageReveal({ kind: "channel" }).state.kind).toBe("channel");
    });

    test("stopped mid-load leaves no timer and takes no late answer", async () => {
        const reveal = createPageReveal({ kind: "channel" });
        let decoded: () => void = () => undefined;
        reveal.holdFace(new Promise<void>(resolve => {
            decoded = resolve;
        }));
        reveal.sidebarAnswered();
        reveal.historyAnswered();
        reveal.factsAnswered();

        reveal.stop();

        expect(jest.getTimerCount()).toBe(0);

        decoded();
        await Promise.resolve();
        await Promise.resolve();
        reveal.sidebarAnswered();
        reveal.failed();
        jest.advanceTimersByTime(revealCeilingMs * 2);

        expect(reveal.state.isSidebarRevealed).toBe(false);
        expect(reveal.state.isMainRevealed).toBe(false);
        expect(reveal.state.isSkeletonShown).toBe(false);
        expect(jest.getTimerCount()).toBe(0);
    });

    test("stopped before any answer leaves no timer either", () => {
        const reveal = createPageReveal({ kind: "channel" });

        reveal.stop();

        expect(jest.getTimerCount()).toBe(0);
    });

    test("stopped while the render after the last answer is pending starts no grace and releases nothing", async () => {
        const reveal = createPageReveal({ kind: "channel" });
        answerAll(reveal);

        reveal.stop();
        await rendered();

        expect(reveal.state.isReleased).toBe(false);
        expect(jest.getTimerCount()).toBe(0);
    });
});

describe("the remembered room's look", () => {
    function memory(): StorageLike {
        const values = new Map<string, string>();
        return { getItem: key => values.get(key) ?? null, setItem: (key, value) => void values.set(key, value) };
    }

    const key = rememberedChannelKey(tenant, alias);

    test("is a channel's until a direct message is remembered", () => {
        const storage = memory();
        expect(readRememberedKind(storage, key)).toBe("channel");

        writeRememberedKind(storage, key, "direct");
        expect(readRememberedKind(storage, key)).toBe("direct");

        writeRememberedKind(storage, key, "channel");
        expect(readRememberedKind(storage, key)).toBe("channel");
    });

    test("is kept beside the channel's id, which still reads as before", () => {
        const storage = memory();
        writeRememberedChannel(storage, key, remembered);
        writeRememberedKind(storage, key, "direct");

        expect(readRememberedChannel(storage, key)).toBe(remembered);
    });

    test("reads as a channel's when storage is blocked or holds something else", () => {
        const blocked: StorageLike = {
            getItem: () => {
                throw new Error("blocked");
            },
            setItem: () => {
                throw new Error("blocked");
            }
        };
        expect(readRememberedKind(blocked, key)).toBe("channel");
        expect(readRememberedKind(null, key)).toBe("channel");
        expect(() => writeRememberedKind(blocked, key, "direct")).not.toThrow();

        const odd = memory();
        odd.setItem(`${key}:kind`, "nonsense");
        expect(readRememberedKind(odd, key)).toBe("channel");
    });
});
