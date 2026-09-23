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
// The read position is saved on events only: leaving the channel, the page going to the
// background or losing focus, and close. Never on open, never on a send, never on a timer.
import {
    createKeepaliveSave,
    createReadTracker,
    isUnreadAfterSave,
    PageEventTargets
} from "../../../../../src/Communication/Chat/ChatShell/composables/useMarkRead.partial";
import { MarkReadResult } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

const channel = "c0000001-0000-4000-8000-000000000000";
const other = "c0000002-0000-4000-8000-000000000000";

function tracker(): { tracker: ReturnType<typeof createReadTracker>, saves: string[], saved: string[] } {
    const saves: string[] = [];
    const saved: string[] = [];
    const t = createReadTracker({
        save: async (channelId, messageId) => {
            saves.push(`${channelId}:${messageId}`);
            return { read_cursor: messageId, last_message_id: messageId };
        },
        onSaved: (channelId, result) => saved.push(`${channelId}:${result.read_cursor}`)
    });
    return { tracker: t, saves, saved };
}

type Listeners = Record<string, Array<() => void>>;

function page(): { targets: PageEventTargets, fire: (type: string) => void, listeners: () => number } {
    const onDocument: Listeners = {};
    const onWindow: Listeners = {};
    const add = (map: Listeners) => (type: string, listener: () => void): void => {
        (map[type] ??= []).push(listener);
    };
    const remove = (map: Listeners) => (type: string, listener: () => void): void => {
        map[type] = (map[type] ?? []).filter(l => l !== listener);
    };
    const targets: PageEventTargets = {
        document: { visibilityState: "visible", addEventListener: add(onDocument), removeEventListener: remove(onDocument) },
        window: { addEventListener: add(onWindow), removeEventListener: remove(onWindow) }
    };
    return {
        targets,
        fire: (type: string) => {
            if (type === "hidden") {
                targets.document.visibilityState = "hidden";
                (onDocument["visibilitychange"] ?? []).forEach(l => l());
            }
            else if (type === "visible") {
                targets.document.visibilityState = "visible";
                (onDocument["visibilitychange"] ?? []).forEach(l => l());
            }
            else {
                (onWindow[type] ?? []).forEach(l => l());
            }
        },
        listeners: () => [...Object.values(onDocument), ...Object.values(onWindow)].reduce((n, l) => n + l.length, 0)
    };
}

/** The jsdom environment has no fetch Response, so the save is handed the part of one it reads. */
function fakeResponse(status: number, body: unknown): Response {
    return { ok: status >= 200 && status < 300, status, json: async () => body } as unknown as Response;
}

async function settle(): Promise<void> {
    for (let i = 0; i < 10; i++) {
        await Promise.resolve();
    }
}

describe("createReadTracker", () => {
    test("opening a channel saves nothing", async () => {
        const { tracker: t, saves } = tracker();

        t.open(channel, 10);
        t.seen(12);
        await settle();

        expect(saves).toEqual([]);
    });

    test("leaving saves the newest message that was on screen", async () => {
        const { tracker: t, saves, saved } = tracker();
        t.open(channel, 10);

        t.seen(11);
        t.seen(14);
        t.seen(12);
        await t.leave();

        expect(saves).toEqual([`${channel}:14`]);
        expect(saved).toEqual([`${channel}:14`]);
    });

    test("nothing is sent when nothing newer than the stored position was seen", async () => {
        const { tracker: t, saves } = tracker();
        t.open(channel, 20);

        t.seen(18);
        t.seen(20);
        await t.leave();

        expect(saves).toEqual([]);
    });

    test("a second event with nothing new since the first save sends nothing", async () => {
        const { tracker: t, saves } = tracker();
        t.open(channel, 1);
        t.seen(5);

        await t.flush();
        await t.flush();

        expect(saves).toEqual([`${channel}:5`]);
    });

    test("background, lost focus and close each save, and becoming visible does not", async () => {
        const { tracker: t, saves } = tracker();
        const p = page();
        t.attach(p.targets);
        t.open(channel, 1);

        t.seen(2);
        p.fire("hidden");
        await settle();
        p.fire("visible");
        await settle();
        t.seen(3);
        p.fire("blur");
        await settle();
        t.seen(4);
        p.fire("pagehide");
        await settle();

        expect(saves).toEqual([`${channel}:2`, `${channel}:3`, `${channel}:4`]);
    });

    test("after leaving, events save nothing for the channel that was left", async () => {
        const { tracker: t, saves } = tracker();
        const p = page();
        t.attach(p.targets);
        t.open(channel, 1);
        t.seen(2);
        await t.leave();

        p.fire("blur");
        await settle();

        expect(saves).toEqual([`${channel}:2`]);
    });

    test("opening another channel starts from that channel's own stored position", async () => {
        const { tracker: t, saves } = tracker();
        t.open(channel, 1);
        t.seen(2);
        await t.leave();

        t.open(other, 50);
        t.seen(40);
        await t.leave();

        expect(saves).toEqual([`${channel}:2`]);
    });

    test("leaving lets go of the channel at once, so one opened while the save is in flight keeps its own position", async () => {
        const saves: string[] = [];
        let finish!: () => void;
        const t = createReadTracker({
            save: (channelId, messageId) => {
                saves.push(`${channelId}:${messageId}`);
                return new Promise(resolve => finish = () => resolve({ read_cursor: messageId, last_message_id: messageId }));
            },
            onSaved: () => undefined
        });
        t.open(channel, 1);
        t.seen(2);

        const leaving = t.leave();
        t.open(other, 10);
        t.seen(11);
        finish();
        await leaving;
        await t.flush();

        expect(saves).toEqual([`${channel}:2`, `${other}:11`]);
    });

    test("the listeners come off", () => {
        const { tracker: t } = tracker();
        const p = page();

        const detach = t.attach(p.targets);
        expect(p.listeners()).toBeGreaterThan(0);
        detach();

        expect(p.listeners()).toBe(0);
    });

    test("a failed save is tried again on the next event", async () => {
        const saves: number[] = [];
        let failing = true;
        const t = createReadTracker({
            save: async (_c, messageId) => {
                saves.push(messageId);
                return failing ? null : { read_cursor: messageId, last_message_id: messageId };
            },
            onSaved: () => undefined
        });
        t.open(channel, 1);
        t.seen(3);

        await t.flush();
        failing = false;
        await t.flush();

        expect(saves).toEqual([3, 3]);
    });
});

describe("createKeepaliveSave", () => {
    test("posts to the mark-read call with keepalive, the person's token and the publishable key", async () => {
        const calls: { url: string, init: RequestInit }[] = [];
        const save = createKeepaliveSave({
            fetch: async (url, init) => {
                calls.push({ url, init });
                return fakeResponse(200, { read_cursor: 7, last_message_id: 9 });
            },
            projectUrl: "http://127.0.0.1:54321",
            publishableKey: "sb_publishable_test",
            currentToken: () => "platform-token"
        });

        const result = await save(channel, 7);

        expect(result).toEqual({ read_cursor: 7, last_message_id: 9 });
        expect(calls).toHaveLength(1);
        expect(calls[0].url).toBe("http://127.0.0.1:54321/rest/v1/rpc/chat_mark_read");
        expect(calls[0].init.method).toBe("POST");
        expect(calls[0].init.keepalive).toBe(true);
        const headers = calls[0].init.headers as Record<string, string>;
        expect(headers["Authorization"]).toBe("Bearer platform-token");
        expect(headers["apikey"]).toBe("sb_publishable_test");
        expect(headers["Content-Type"]).toBe("application/json");
        expect(JSON.parse(calls[0].init.body as string)).toEqual({ p_channel_id: channel, p_message_id: 7 });
    });

    test("a refused save resolves null rather than throwing", async () => {
        const save = createKeepaliveSave({
            fetch: async () => fakeResponse(403, { message: "authz.not_readable" }),
            projectUrl: "http://127.0.0.1:54321",
            publishableKey: "k",
            currentToken: () => "t"
        });

        expect(await save(channel, 7)).toBeNull();
    });

    test("a save with no token sends nothing", async () => {
        let called = false;
        const save = createKeepaliveSave({
            fetch: async () => {
                called = true;
                return fakeResponse(200, {});
            },
            projectUrl: "http://127.0.0.1:54321",
            publishableKey: "k",
            currentToken: () => null
        });

        expect(await save(channel, 7)).toBeNull();
        expect(called).toBe(false);
    });
});

describe("isUnreadAfterSave", () => {
    test.each<[MarkReadResult, boolean]>([
        [{ read_cursor: 10, last_message_id: 10 }, false],
        [{ read_cursor: 10, last_message_id: 11 }, true],
        [{ read_cursor: 12, last_message_id: 11 }, false],
        [{ read_cursor: 10, last_message_id: null }, false],
        [{ read_cursor: null, last_message_id: 3 }, true]
    ])("%p is unread: %p", (result, expected) => {
        expect(isUnreadAfterSave(result)).toBe(expected);
    });
});
