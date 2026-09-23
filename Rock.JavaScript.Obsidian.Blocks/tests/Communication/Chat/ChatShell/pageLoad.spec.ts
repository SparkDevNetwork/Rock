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
import {
    openChannel,
    OpenOutcome,
    readRememberedChannel,
    rememberedChannelKey,
    startPageLoad,
    StorageLike,
    writeRememberedChannel
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
