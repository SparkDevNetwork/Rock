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
// The browser's history on a phone mirrors one number, how many steps deep the chat is. Each step
// is one entry on the same address; Back, the edge swipe or the on-screen arrow takes one step.
// A fake history stands in for the browser's, so every call and every popstate is seen.
import { readdirSync, readFileSync, statSync } from "fs";
import { join } from "path";
import { effectScope, EffectScope, nextTick, ref, Ref } from "vue";
import { BackStack, BackStackHost, createBackStack } from "../../../../../src/Communication/Chat/ChatShell/composables/useBackStack.partial";

type Entry = { state: unknown, url: string };

/** A history as a browser keeps it: entries, where the page is among them, and popstates arriving as tasks. */
class FakeHistory {
    entries: Entry[] = [{ state: null, url: "/page/1" }];
    index = 0;
    calls: string[] = [];
    pending: Array<() => void> = [];
    listeners: Array<(event: PopStateEvent) => void> = [];

    /** Set to make the next push throw, as WebKit's rate limit does. */
    isPushRefused = false;

    /** Set to make the next push do nothing, as Chrome's throttle does. */
    isPushDropped = false;

    /** Set to make a go bring no popstate, as one past the entries a browser kept. */
    isGoLost = false;

    get state(): unknown {
        return this.entries[this.index].state;
    }

    get length(): number {
        return this.entries.length;
    }

    pushState(state: unknown, _title: string, url?: string): void {
        this.calls.push("push");
        if (this.isPushRefused) {
            this.isPushRefused = false;
            throw new Error("SecurityError: too many calls");
        }
        if (this.isPushDropped) {
            this.isPushDropped = false;
            return;
        }
        this.entries.splice(this.index + 1);
        this.entries.push({ state, url: url ?? this.entries[this.index].url });
        this.index++;
    }

    replaceState(state: unknown, _title: string, url?: string): void {
        this.calls.push("replace");
        this.entries[this.index] = { state, url: url ?? this.entries[this.index].url };
    }

    go(delta: number): void {
        this.calls.push(`go(${delta})`);
        if (this.isGoLost) {
            this.isGoLost = false;
            return;
        }
        this.pending.push(() => this.travel(delta));
    }

    /** The person's own Back or Forward, through the browser. */
    userGo(delta: number, hasUAVisualTransition = false): void {
        this.pending.push(() => this.travel(delta, hasUAVisualTransition));
    }

    private travel(delta: number, hasUAVisualTransition = false): void {
        const index = Math.max(0, Math.min(this.entries.length - 1, this.index + delta));
        this.index = index;
        const event = Object.assign(new Event("popstate"), { state: this.state, hasUAVisualTransition }) as PopStateEvent;
        this.listeners.forEach(listener => listener(event));
    }
}

let history: FakeHistory;
let timers: Array<{ id: number, run: () => void }>;
let scope: EffectScope | null = null;

function host(): BackStackHost {
    return {
        history: history as unknown as History,
        addEventListener: (_type: "popstate", listener: (event: PopStateEvent) => void) => history.listeners.push(listener),
        removeEventListener: (_type: "popstate", listener: (event: PopStateEvent) => void) => {
            history.listeners = history.listeners.filter(l => l !== listener);
        },
        setTimeout: (run: () => void) => {
            const id = timers.length + 1;
            timers.push({ id, run });
            return id;
        },
        clearTimeout: (id: number | undefined) => {
            timers = timers.filter(t => t.id !== id);
        }
    };
}

/** Delivers every popstate waiting, each followed by the patch and flush it brings. */
async function settle(): Promise<void> {
    await nextTick();
    await nextTick();
    while (history.pending.length) {
        history.pending.shift()?.();
        await nextTick();
        await nextTick();
    }
}

function make(isPhone: Ref<boolean> = ref(true)): BackStack {
    scope = effectScope();
    return scope.run(() => createBackStack(() => isPhone.value, host())) as BackStack;
}

/** A layer whose owner closes it when asked, as a sheet does. */
function layer(stack: BackStack, isRefusing = false): { isOpen: Ref<boolean>, release: () => void } {
    const isOpen = ref(true);
    const release = stack.hold(() => {
        if (!isRefusing) {
            isOpen.value = false;
            release();
        }
    });
    return { isOpen, release };
}

function depthOf(state: unknown): number | undefined {
    return (state as { rockChat?: { depth: number } } | null)?.rockChat?.depth;
}

beforeEach(() => {
    history = new FakeHistory();
    timers = [];
});

afterEach(() => {
    scope?.stop();
    scope = null;
});

describe("the back stack", () => {
    test("a layer held adds one entry on the same address, marked with its depth", async () => {
        const stack = make();
        layer(stack);
        await settle();

        expect(history.length).toBe(2);
        expect(history.entries[1].url).toBe("/page/1");
        expect(depthOf(history.state)).toBe(1);
        expect(depthOf(history.entries[0].state)).toBe(0);
    });

    test("Back closes the top layer only", async () => {
        const stack = make();
        const under = layer(stack);
        const top = layer(stack);
        await settle();
        expect(history.length).toBe(3);

        history.userGo(-1);
        await settle();

        expect(top.isOpen.value).toBe(false);
        expect(under.isOpen.value).toBe(true);
        expect(history.index).toBe(1);
        expect(history.calls.filter(c => c.startsWith("go"))).toEqual([]);
    });

    test("a layer closed by code goes back once, and its popstate closes nothing more", async () => {
        const stack = make();
        const under = layer(stack);
        const top = layer(stack);
        await settle();

        top.isOpen.value = false;
        top.release();
        await settle();

        expect(history.calls.filter(c => c.startsWith("go"))).toEqual(["go(-1)"]);
        expect(under.isOpen.value).toBe(true);
        expect(history.index).toBe(1);
    });

    test("a layer let go and another held in one tick is a replacement: no call", async () => {
        const stack = make();
        const first = layer(stack);
        await settle();
        history.calls = [];

        first.release();
        layer(stack);
        await settle();

        expect(history.calls).toEqual([]);
        expect(history.index).toBe(1);
    });

    test("a sheet closing with two turned pages goes back once, by three", async () => {
        const stack = make();
        const sheet = layer(stack);
        const page1 = layer(stack);
        const page2 = layer(stack);
        await settle();

        page2.release();
        page1.release();
        sheet.release();
        await settle();

        expect(history.calls.filter(c => c.startsWith("go"))).toEqual(["go(-3)"]);
        expect(history.index).toBe(0);
    });

    test("a refused close bounces: the entry comes back", async () => {
        const stack = make();
        const stubborn = layer(stack, true);
        await settle();

        history.userGo(-1);
        await settle();

        expect(stubborn.isOpen.value).toBe(true);
        expect(history.index).toBe(1);
        expect(depthOf(history.state)).toBe(1);
    });

    test("a push waits for a trip back still on its way", async () => {
        const stack = make();
        const first = layer(stack);
        await settle();

        first.release();
        await nextTick();
        await nextTick();
        // the go is out; a new layer is held before its popstate
        layer(stack);
        await nextTick();
        await nextTick();
        expect(history.calls.filter(c => c === "push")).toHaveLength(1);

        await settle();
        expect(history.calls.filter(c => c === "push")).toHaveLength(2);
        expect(history.index).toBe(1);
    });

    test("a page loaded at a depth from its last life trips back to its base first", async () => {
        history.entries = [0, 1, 2, 3].map(depth => ({ state: { rockChat: { depth } }, url: "/page/1" }));
        history.index = 3;
        const stack = make();
        layer(stack);
        await settle();

        expect(history.calls[0]).toBe("go(-3)");
        expect(history.index).toBe(1);
        expect(history.length).toBe(2);
    });

    test("Forward onto a chat entry goes back again", async () => {
        const stack = make();
        const top = layer(stack);
        await settle();
        top.isOpen.value = false;
        top.release();
        await settle();
        expect(history.index).toBe(0);

        history.userGo(1);
        await settle();

        expect(history.index).toBe(0);
    });

    test("off a phone nothing is written", async () => {
        const stack = make(ref(false));
        layer(stack);
        await settle();

        expect(history.calls).toEqual([]);
        expect(history.length).toBe(1);
    });

    test("widening past the phone trips back to the base; narrowing pushes again", async () => {
        const isPhone = ref(true);
        const stack = make(isPhone);
        const held = layer(stack);
        await settle();

        isPhone.value = false;
        await settle();
        expect(history.index).toBe(0);
        expect(held.isOpen.value).toBe(true);

        isPhone.value = true;
        await settle();
        expect(history.index).toBe(1);
    });

    test("the base entry keeps the state it carried", async () => {
        history.entries[0].state = { a: 1 };
        const stack = make();
        layer(stack);
        await settle();

        expect(history.entries[0].state).toEqual({ a: 1, rockChat: { depth: 0 } });
    });

    test("the frame's screens step back after every layer above them is closed", async () => {
        const stack = make();
        const count = ref(2);
        const steps: Array<{ steps: number, isDrawnByBrowser: boolean }> = [];
        stack.screens(() => count.value, (n, options) => {
            steps.push({ steps: n, ...options });
            count.value -= n;
        });
        const sheet = layer(stack);
        await settle();
        expect(history.index).toBe(3);

        history.userGo(-2, true);
        await settle();

        expect(sheet.isOpen.value).toBe(false);
        expect(steps).toEqual([{ steps: 1, isDrawnByBrowser: true }]);
        expect(history.index).toBe(1);
    });

    test("an owner that closes a tick late costs at most one entry and never leaves chat", async () => {
        const stack = make();
        const isOpen = ref(true);
        const release = stack.hold(() => {
            void nextTick().then(() => {
                isOpen.value = false;
                release();
            });
        });
        await settle();
        const most = history.length;

        history.userGo(-1);
        await settle();

        expect(isOpen.value).toBe(false);
        expect(history.length).toBeLessThanOrEqual(most + 1);
        expect(history.index).toBe(0);
    });
});

describe("guarded history calls", () => {
    test("a push that throws keeps the depth, and the next flush pushes", async () => {
        const stack = make();
        history.isPushRefused = true;
        layer(stack);
        await settle();
        expect(history.index).toBe(0);

        layer(stack);
        await settle();
        expect(history.index).toBe(2);
    });

    test("a push dropped without a word keeps the depth, and the next flush pushes", async () => {
        const stack = make();
        history.isPushDropped = true;
        layer(stack);
        await settle();
        expect(history.index).toBe(0);

        layer(stack);
        await settle();
        expect(history.index).toBe(2);
    });

    test("a trip whose popstate never comes frees pushes after its bound", async () => {
        const stack = make();
        const first = layer(stack);
        await settle();
        history.isGoLost = true;
        first.release();
        await settle();
        layer(stack);
        await settle();
        const pushes = history.calls.filter(c => c === "push").length;

        timers.splice(0).forEach(t => t.run());
        await settle();

        expect(history.calls.filter(c => c === "push").length).toBe(pushes);
        expect(depthOf(history.state)).toBe(1);
    });

    test("150 opens and closes end with history at the depth drawn", async () => {
        const stack = make();
        for (let i = 0; i < 150; i++) {
            const held = layer(stack);
            if (i % 3 === 0) {
                await nextTick();
            }
            held.release();
            if (i % 7 === 0) {
                await settle();
            }
        }
        layer(stack);
        await settle();

        expect(depthOf(history.state)).toBe(1);
    });
});

describe("one owner", () => {
    test("no chat file but the back stack writes history or listens for popstate", () => {
        const root = join(__dirname, "../../../../../src/Communication/Chat");
        const files: string[] = [];
        const walk = (folder: string): void => {
            for (const name of readdirSync(folder)) {
                const path = join(folder, name);
                if (statSync(path).isDirectory()) {
                    walk(path);
                }
                else if (/\.(ts|obs)$/.test(name) && !name.startsWith("useBackStack.")) {
                    files.push(path);
                }
            }
        };
        walk(root);

        const writers = files.filter(path => /pushState|replaceState|history\.(go|back|forward)\b|["']popstate["']/.test(readFileSync(path, "utf8")));
        expect(writers).toEqual([]);
    });
});
