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
// The push worker is a plain script the browser loads by URL, outside every page, so it is read
// from disk and run here against a fake worker scope. It shows only what the push carries, asks
// a focused window whether that channel is already on screen before showing anything, keeps the
// app badge at the count a mention push carries, and opens the channel when tapped.
import { readFileSync } from "fs";
import { resolve } from "path";

const workerPath = resolve(__dirname, "../../../../../RockWeb/Scripts/Rock/Chat/chatPushWorker.js");

type Shown = { title: string, options: Record<string, unknown> };
type Listener = (event: Record<string, unknown>) => void;

/** A window of the site as the worker sees it through clients.matchAll. */
type FakeWindow = {
    url: string,
    focused: boolean,
    posted: unknown[],
    focus: jest.Mock,
    postMessage: (message: unknown, transfer?: FakePort[]) => void
};

/** One end of a fake MessageChannel; posting on one end calls the other's onmessage. */
type FakePort = { onmessage: ((event: { data: unknown }) => void) | null, other?: FakePort, postMessage: (data: unknown) => void };

class FakeMessageChannel {
    port1: FakePort;
    port2: FakePort;

    constructor() {
        const make = (): FakePort => {
            const port: FakePort = {
                onmessage: null,
                postMessage: (data: unknown) => port.other?.onmessage?.({ data })
            };
            return port;
        };
        this.port1 = make();
        this.port2 = make();
        this.port1.other = this.port2;
        this.port2.other = this.port1;
    }
}

/** A window that answers the worker's question the way the test tells it to, or never. */
function chatWindow(url: string, focused: boolean, answer: ((message: Record<string, unknown>) => unknown) | null): FakeWindow {
    const window: FakeWindow = {
        url,
        focused,
        posted: [],
        focus: jest.fn(async () => window),
        postMessage: (message: unknown, transfer?: FakePort[]) => {
            window.posted.push(message);
            const port = transfer?.[0];
            if (port && answer) {
                port.postMessage(answer(message as Record<string, unknown>));
            }
        }
    };
    return window;
}

/** Loads the worker into a fresh fake scope. */
function loadWorker(options: { windows?: FakeWindow[], hasBadge?: boolean, page?: string } = {}) {
    const listeners: Record<string, Listener> = {};
    const shown: Shown[] = [];
    const badges: number[] = [];
    const opened: string[] = [];
    const timers: Array<{ callback: () => void, milliseconds: number }> = [];
    const windows = options.windows ?? [];
    const page = options.page ?? "/chat";

    const navigator: Record<string, unknown> = {};
    if (options.hasBadge !== false) {
        navigator.setAppBadge = jest.fn(async (count: number) => {
            badges.push(count);
        });
    }

    const self = {
        location: { href: `https://church.example/Scripts/Rock/Chat/chatPushWorker.js?page=${encodeURIComponent(page)}` },
        navigator,
        addEventListener: (type: string, listener: Listener) => {
            listeners[type] = listener;
        },
        skipWaiting: jest.fn(),
        registration: {
            showNotification: jest.fn(async (title: string, notificationOptions: Record<string, unknown>) => {
                shown.push({ title, options: notificationOptions });
            })
        },
        clients: {
            matchAll: jest.fn(async () => windows),
            openWindow: jest.fn(async (url: string) => {
                opened.push(url);
                return null;
            }),
            claim: jest.fn(async () => undefined)
        }
    };

    const setTimeoutFake = (callback: () => void, milliseconds: number): number => {
        timers.push({ callback, milliseconds });
        return timers.length;
    };

    // eslint-disable-next-line @typescript-eslint/no-implied-eval
    new Function("self", "MessageChannel", "setTimeout", "clearTimeout", readFileSync(workerPath, "utf8"))(
        self, FakeMessageChannel, setTimeoutFake, () => undefined);

    /** Fires an event and returns the promise it handed to waitUntil. */
    const fire = (type: string, event: Record<string, unknown>): Promise<unknown> => {
        let work: Promise<unknown> = Promise.resolve();
        listeners[type]({ ...event, waitUntil: (promise: Promise<unknown>) => { work = promise; } });
        return work;
    };

    const push = (payload: unknown): Promise<unknown> => fire("push", { data: { json: () => payload } });

    return { self, listeners, shown, badges, opened, timers, push, fire };
}

/** A push as push-dispatch sends a web device one. */
function payload(data: Record<string, string>, notification = { title: "Youth Team", body: "Ted Decker: see you at 6" }) {
    return {
        notification,
        data: { message_id: "981", channel_id: "6f1c2a9e-0000-4000-8000-000000000001", parent_id: "", in_channel: "true", reason: "message", ...data }
    };
}

/** Lets every queued promise settle. */
async function settle(): Promise<void> {
    for (let i = 0; i < 10; i++) {
        await Promise.resolve();
    }
}

describe("the push worker", () => {
    test("shows exactly the title and body the push carries, tagged by the message so an edit's push replaces it", async () => {
        const worker = loadWorker();

        await worker.push(payload({ face: "https://church.example/photo.jpg" }));

        expect(worker.shown).toEqual([{
            title: "Youth Team",
            options: expect.objectContaining({
                body: "Ted Decker: see you at 6",
                tag: "981",
                icon: "https://church.example/photo.jpg"
            })
        }]);
    });

    test("with no window of the site in front, shows at once without asking anything", async () => {
        const background = chatWindow("https://church.example/chat", false, () => ({ open: true }));
        const worker = loadWorker({ windows: [background] });

        await worker.push(payload({}));

        expect(background.posted).toEqual([]);
        expect(worker.timers).toEqual([]);
        expect(worker.shown).toHaveLength(1);
    });

    test("hides the banner when a focused window says that channel is on screen", async () => {
        const front = chatWindow("https://church.example/chat", true, message => ({ open: message.channel_id === "6f1c2a9e-0000-4000-8000-000000000001" }));
        const worker = loadWorker({ windows: [front] });

        await worker.push(payload({}));

        expect(front.posted).toEqual([{ type: "chat.push.is-open", channel_id: "6f1c2a9e-0000-4000-8000-000000000001" }]);
        expect(worker.shown).toEqual([]);
    });

    test("shows the banner when the focused window has another channel open", async () => {
        const front = chatWindow("https://church.example/chat", true, () => ({ open: false }));
        const worker = loadWorker({ windows: [front] });

        await worker.push(payload({}));

        expect(worker.shown).toHaveLength(1);
    });

    test("always shows a thread-only reply, since no thread can be open on screen yet", async () => {
        const front = chatWindow("https://church.example/chat", true, () => ({ open: true }));
        const worker = loadWorker({ windows: [front] });

        await worker.push(payload({ parent_id: "970", in_channel: "false", reason: "thread" }));

        expect(front.posted).toEqual([]);
        expect(worker.shown).toHaveLength(1);
    });

    test("hides a reply shown in the channel when that channel is on screen, as it hides a root", async () => {
        const front = chatWindow("https://church.example/chat", true, () => ({ open: true }));
        const worker = loadWorker({ windows: [front] });

        await worker.push(payload({ parent_id: "970", in_channel: "true", reason: "message" }));

        expect(worker.shown).toEqual([]);
    });

    test("a focused window that never answers costs the wait, then the banner shows", async () => {
        const silent = chatWindow("https://church.example/somewhere-else", true, null);
        const worker = loadWorker({ windows: [silent] });

        const work = worker.push(payload({}));
        await settle();

        expect(worker.shown).toEqual([]);
        expect(worker.timers.map(timer => timer.milliseconds)).toEqual([250]);

        worker.timers[0].callback();
        await work;

        expect(worker.shown).toHaveLength(1);
    });

    test("an answer that arrives after the wait changes nothing", async () => {
        let reply: ((value: unknown) => void) | undefined;
        const late: FakeWindow = chatWindow("https://church.example/chat", true, null);
        late.postMessage = (_message: unknown, transfer?: FakePort[]) => {
            reply = (value: unknown) => transfer?.[0].postMessage(value);
        };
        const worker = loadWorker({ windows: [late] });

        const work = worker.push(payload({}));
        await settle();
        worker.timers[0].callback();
        await work;
        reply?.({ open: true });
        await settle();

        expect(worker.shown).toHaveLength(1);
    });

    test("a mention push sets the badge to the count it carries; any other push leaves it", async () => {
        const worker = loadWorker();

        await worker.push(payload({ reason: "mention", badge: "3" }));
        await worker.push(payload({ message_id: "982" }));

        expect(worker.badges).toEqual([3]);
    });

    test("a browser with no badge support still shows the push", async () => {
        const worker = loadWorker({ hasBadge: false });

        await worker.push(payload({ reason: "mention", badge: "3" }));

        expect(worker.shown).toHaveLength(1);
    });

    test("a tap focuses a window already on the chat page and tells it which channel to open", async () => {
        const chat = chatWindow("https://church.example/chat?ChannelGuid=aaaa", false, null);
        const other = chatWindow("https://church.example/give", false, null);
        const worker = loadWorker({ windows: [other, chat] });
        const close = jest.fn();

        await worker.fire("notificationclick", { notification: { close, data: payload({}).data } });

        expect(close).toHaveBeenCalled();
        expect(chat.focus).toHaveBeenCalled();
        expect(chat.posted).toEqual([{ type: "chat.push.open", channel_id: "6f1c2a9e-0000-4000-8000-000000000001" }]);
        expect(other.focus).not.toHaveBeenCalled();
        expect(worker.opened).toEqual([]);
    });

    test("a tap with no chat window open opens the chat page on that channel", async () => {
        const worker = loadWorker({ windows: [chatWindow("https://church.example/give", true, null)], page: "/page/412" });

        await worker.fire("notificationclick", { notification: { close: jest.fn(), data: payload({}).data } });

        expect(worker.opened).toEqual(["/page/412?ChannelGuid=6f1c2a9e-0000-4000-8000-000000000001"]);
    });
});
