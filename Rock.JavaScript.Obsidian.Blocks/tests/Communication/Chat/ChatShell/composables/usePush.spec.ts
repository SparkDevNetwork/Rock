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
// Web push on this browser. The browser asks for permission only from a click, and the church's
// setting decides when the button shows; a browser that already said yes registers again on every
// load without asking. The token goes to the platform under the person's token and into a cookie,
// so a sign-out anywhere in Rock can take this browser out of push.
import { createPushRegistration, PushDependencies } from "../../../../../src/Communication/Chat/ChatShell/composables/usePush.partial";
import { CallResult } from "../../../../../src/Communication/Chat/ChatShell/platformCall.partial";

const web = { apiKey: "k", projectId: "p", messagingSenderId: "1", appId: "1:1:web:1" };

/** A promise the test settles by hand. */
function deferred<T>(): { promise: Promise<T>, resolve: (value: T) => void, reject: (reason: unknown) => void } {
    let resolve!: (value: T) => void;
    let reject!: (reason: unknown) => void;
    const promise = new Promise<T>((res, rej) => {
        resolve = res;
        reject = rej;
    });
    return { promise, resolve, reject };
}

/** Lets every queued promise settle. */
async function settle(): Promise<void> {
    for (let i = 0; i < 10; i++) {
        await Promise.resolve();
    }
}

/**
 * A service worker as the browser hands it over: an event target with its state, which fires
 * statechange as it moves from installing to installed, activating and activated, or redundant.
 */
class FakeServiceWorker extends EventTarget {
    constructor(public state: ServiceWorkerState, public readonly scriptURL = "/Scripts/Rock/Chat/chatPushWorker.js?page=%2Fchat") {
        super();
    }
}

/** A registration as navigator.serviceWorker.register answers it, its one worker in the slot its state puts it in. */
type FakeRegistration = {
    scope: string;
    installing: FakeServiceWorker | null;
    waiting: FakeServiceWorker | null;
    active: FakeServiceWorker | null;
};

function fakeRegistration(state: ServiceWorkerState): { registration: FakeRegistration, worker: FakeServiceWorker, become: (next: ServiceWorkerState) => void } {
    const worker = new FakeServiceWorker(state);
    const registration: FakeRegistration = { scope: "/Scripts/Rock/Chat/", installing: null, waiting: null, active: null };

    /** Moves the worker on as the browser does: the registration's slots first, then the event. */
    const become = (next: ServiceWorkerState): void => {
        registration.installing = next === "installing" ? worker : null;
        registration.waiting = next === "installed" ? worker : null;
        registration.active = next === "activating" || next === "activated" ? worker : null;
        worker.state = next;
        worker.dispatchEvent(new Event("statechange"));
    };

    registration.installing = state === "installing" ? worker : null;
    registration.waiting = state === "installed" ? worker : null;
    registration.active = state === "activating" || state === "activated" ? worker : null;

    return { registration, worker, become };
}

/** The answer createPlatformCall gives for a register the platform accepted. */
const registered: CallResult = { ok: true, data: { token: "fcm-token-1", platform: "web", notifications_enabled: true } };

/** Dependencies with every outside call recorded, overridable per test. */
function dependencies(overrides: Partial<PushDependencies> & { prompt?: string, permission?: string } = {}) {
    const calls: Array<{ name: string, args: Record<string, unknown> }> = [];
    const cookies: Array<string | null> = [];
    let permission = overrides.permission ?? "default";

    const base: PushDependencies = {
        settings: () => ({ web, vapid_key: "vapid", prompt: overrides.prompt ?? "after_send" }),
        pagePath: "/chat",
        permission: () => permission,
        requestPermission: jest.fn(async () => {
            permission = "granted";
            return permission;
        }),
        registerWorker: jest.fn(async (_pagePath: string) => fakeRegistration("activated").registration),
        getToken: jest.fn(async () => "fcm-token-1"),
        call: jest.fn(async (name: string, args: Record<string, unknown>) => {
            calls.push({ name, args });
            return registered;
        }),
        writeCookie: (token: string | null) => {
            cookies.push(token);
        },
        // the cookie as the last write left it
        readCookie: () => cookies.length > 0 ? cookies[cookies.length - 1] : null
    };

    // prompt and permission seed the fakes above; only real dependencies replace the base ones
    const { prompt: _prompt, permission: _permission, ...replacements } = overrides;

    return { deps: { ...base, ...replacements } as PushDependencies, calls, cookies, setPermission: (value: string) => { permission = value; } };
}

describe("web push on this browser", () => {
    test("with no Firebase values from the platform nothing loads and nothing is offered", async () => {
        const { deps, calls } = dependencies({ permission: "granted", settings: () => ({ web: null, vapid_key: null, prompt: "offer" }) });
        const push = createPushRegistration(deps);

        await push.start();
        push.noteSend();

        expect(push.isOffered()).toBe(false);
        expect(deps.registerWorker).not.toHaveBeenCalled();
        expect(deps.getToken).not.toHaveBeenCalled();
        expect(calls).toEqual([]);
    });

    test("prompt off never offers the button", async () => {
        const push = createPushRegistration(dependencies({ prompt: "off" }).deps);

        await push.start();
        push.noteSend();

        expect(push.isOffered()).toBe(false);
    });

    test("prompt offer shows the button from the first load", async () => {
        const push = createPushRegistration(dependencies({ prompt: "offer" }).deps);

        await push.start();

        expect(push.isOffered()).toBe(true);
    });

    test("prompt after_send shows the button only once the person has sent", async () => {
        const push = createPushRegistration(dependencies({ prompt: "after_send" }).deps);

        await push.start();
        expect(push.isOffered()).toBe(false);

        push.noteSend();
        expect(push.isOffered()).toBe(true);
    });

    test("a browser that already said yes registers on load without asking, through the one call path, and keeps the token in the cookie", async () => {
        const { deps, calls, cookies } = dependencies({ permission: "granted", prompt: "off" });
        const push = createPushRegistration(deps);

        await push.start();

        expect(deps.requestPermission).not.toHaveBeenCalled();
        expect(deps.registerWorker).toHaveBeenCalledWith("/chat");
        expect(deps.getToken).toHaveBeenCalledWith(web, "vapid", expect.objectContaining({ scope: "/Scripts/Rock/Chat/" }));
        expect(calls).toEqual([{ name: "chat_register_device", args: { p_token: "fcm-token-1", p_platform: "web" } }]);
        expect(cookies).toEqual(["fcm-token-1"]);
        expect(push.isOffered()).toBe(false);
    });

    test("a browser that said no is never offered the button and never registers", async () => {
        const { deps, calls } = dependencies({ permission: "denied", prompt: "offer" });
        const push = createPushRegistration(deps);

        await push.start();

        expect(push.isOffered()).toBe(false);
        expect(calls).toEqual([]);
    });

    test("the click asks the browser, and a yes registers and hides the button", async () => {
        const { deps, calls } = dependencies({ prompt: "offer" });
        const push = createPushRegistration(deps);
        await push.start();

        await push.turnOn();

        expect(deps.requestPermission).toHaveBeenCalledTimes(1);
        expect(calls.map(c => c.name)).toEqual(["chat_register_device"]);
        expect(push.isOffered()).toBe(false);
    });

    test("a no at the browser's prompt hides the button and registers nothing", async () => {
        const { deps, calls, setPermission } = dependencies({ prompt: "offer" });
        deps.requestPermission = jest.fn(async () => {
            setPermission("denied");
            return "denied";
        });
        const push = createPushRegistration(deps);
        await push.start();

        await push.turnOn();

        expect(push.isOffered()).toBe(false);
        expect(calls).toEqual([]);
    });

    test("a worker still installing, as in a browser's first visit, is waited for: no token is asked for until it is active", async () => {
        const { registration, become } = fakeRegistration("installing");
        const { deps, calls, cookies } = dependencies({ permission: "granted" });
        deps.registerWorker = jest.fn(async () => registration);
        const push = createPushRegistration(deps);

        const started = push.start();
        await settle();
        expect(deps.getToken).not.toHaveBeenCalled();

        become("installed");
        await settle();
        become("activating");
        await settle();
        expect(deps.getToken).not.toHaveBeenCalled();

        become("activated");
        await started;

        expect(deps.getToken).toHaveBeenCalledWith(web, "vapid", registration);
        expect(calls.map(c => c.name)).toEqual(["chat_register_device"]);
        expect(cookies).toEqual(["fcm-token-1"]);
    });

    test("a worker that never activates ends this load with nothing registered and throws nothing", async () => {
        jest.useFakeTimers();
        try {
            const { registration } = fakeRegistration("installing");
            const { deps, calls, cookies } = dependencies({ permission: "granted" });
            deps.registerWorker = jest.fn(async () => registration);
            const push = createPushRegistration(deps);

            const started = push.start();
            await jest.advanceTimersByTimeAsync(60000);

            await expect(started).resolves.toBeUndefined();
            expect(deps.getToken).not.toHaveBeenCalled();
            expect(calls).toEqual([]);
            expect(cookies).toEqual([]);
        }
        finally {
            jest.useRealTimers();
        }
    });

    test("a worker the browser throws away while installing ends this load with nothing registered", async () => {
        const { registration, become } = fakeRegistration("installing");
        const { deps, calls } = dependencies({ permission: "granted" });
        deps.registerWorker = jest.fn(async () => registration);
        const push = createPushRegistration(deps);

        const started = push.start();
        await settle();
        become("redundant");

        await expect(started).resolves.toBeUndefined();
        expect(deps.getToken).not.toHaveBeenCalled();
        expect(calls).toEqual([]);
    });

    test("a token that cannot be had registers nothing and throws nothing", async () => {
        const { deps, calls, cookies } = dependencies({ permission: "granted" });
        deps.getToken = jest.fn(async () => {
            throw new Error("messaging/permission-blocked");
        });
        const push = createPushRegistration(deps);

        await expect(push.start()).resolves.toBeUndefined();

        expect(calls).toEqual([]);
        expect(cookies).toEqual([]);
    });

    test("a register the platform refuses leaves no cookie", async () => {
        const refused: CallResult = { ok: false, status: 503, error: { code: "rpc.read_only", severity: "degraded", text: "Read only tonight." }, retryAfterSeconds: null };
        const { deps, cookies } = dependencies({ permission: "granted" });
        deps.call = jest.fn(async () => refused);
        const push = createPushRegistration(deps);

        await push.start();

        expect(cookies).toEqual([]);
    });

    test("a stop while the token is on its way registers nothing once it arrives", async () => {
        const token = deferred<string>();
        const { deps, calls, cookies } = dependencies({ permission: "granted" });
        deps.getToken = jest.fn(() => token.promise);
        const push = createPushRegistration(deps);

        const started = push.start();
        await settle();
        push.stop();
        token.resolve("fcm-token-1");
        await started;

        expect(calls).toEqual([]);
        expect(cookies).toEqual([]);
    });

    test("a stop while the worker is registering asks for no token", async () => {
        const worker = deferred<unknown>();
        const { deps } = dependencies({ permission: "granted" });
        deps.registerWorker = jest.fn(() => worker.promise);
        const push = createPushRegistration(deps);

        const started = push.start();
        await settle();
        push.stop();
        worker.resolve({ scope: "/Scripts/Rock/Chat/" });
        await started;

        expect(deps.getToken).not.toHaveBeenCalled();
    });

    test("a stop while the register is on its way writes no cookie when it answers", async () => {
        const answer = deferred<CallResult>();
        const { deps, cookies } = dependencies({ permission: "granted" });
        deps.call = jest.fn(() => answer.promise);
        const push = createPushRegistration(deps);

        const started = push.start();
        await settle();
        push.stop();
        answer.resolve(registered);
        await started;

        expect(cookies).toEqual([]);
    });

    test("a stopped registration offers nothing and a click does nothing", async () => {
        const { deps, calls } = dependencies({ prompt: "offer" });
        const push = createPushRegistration(deps);
        await push.start();

        push.stop();
        await push.turnOn();

        expect(push.isOffered()).toBe(false);
        expect(deps.requestPermission).not.toHaveBeenCalled();
        expect(calls).toEqual([]);
    });
});

// Chat settings' browser row reads push's state for this browser: offered, asking while the
// browser's own box is up, on, blocked once the browser said no, or not set up where the platform
// sent no Firebase values. The row offers Turn on whatever the church's prompt timing, since the
// person came to the setting themselves. Turn off takes this browser's device out of push and
// clears the sign-out cookie, and is remembered on this browser, so the next load does not
// register it again while the browser still allows notifications; Turn on then needs no ask.
describe("the browser row's state", () => {
    /** The browser's remembered Turn off, as local storage holds it. */
    function offFlag(initial = false): { read: () => boolean, write: jest.Mock } {
        let isOff = initial;
        return {
            read: () => isOff,
            write: jest.fn((value: boolean) => {
                isOff = value;
            })
        };
    }

    test("is not set up with no Firebase values, and blocked once the browser said no", async () => {
        const none = createPushRegistration(dependencies({ settings: () => ({ web: null, vapid_key: null, prompt: "offer" }) }).deps);
        await none.start();
        expect(none.state()).toBe("unavailable");

        const blocked = createPushRegistration(dependencies({ permission: "denied" }).deps);
        await blocked.start();
        expect(blocked.state()).toBe("blocked");
    });

    test("is offered while the browser has not been asked, whatever the church's prompt timing", async () => {
        const push = createPushRegistration(dependencies({ prompt: "off" }).deps);
        await push.start();

        expect(push.state()).toBe("offered");
    });

    test("is asking while the browser's own box is up, then on once allowed and registered", async () => {
        const answer = deferred<string>();
        const { deps, calls, setPermission } = dependencies();
        deps.requestPermission = jest.fn(() => answer.promise);
        const push = createPushRegistration(deps);
        await push.start();

        const turning = push.turnOn();
        expect(push.state()).toBe("asking");

        setPermission("granted");
        answer.resolve("granted");
        await turning;

        expect(push.state()).toBe("on");
        expect(calls.map(c => c.name)).toEqual(["chat_register_device"]);
    });

    test("is blocked when the browser's box is answered no", async () => {
        const { deps, setPermission } = dependencies();
        deps.requestPermission = jest.fn(async () => {
            setPermission("denied");
            return "denied";
        });
        const push = createPushRegistration(deps);
        await push.start();

        await push.turnOn();

        expect(push.state()).toBe("blocked");
    });

    test("Turn off takes the device out, clears the cookie and is remembered", async () => {
        const flag = offFlag();
        const { deps, calls, cookies } = dependencies({ permission: "granted", offFlag: flag });
        const push = createPushRegistration(deps);
        await push.start();
        expect(push.state()).toBe("on");

        await push.turnOff();

        expect(calls.map(c => [c.name, c.args])).toEqual([
            ["chat_register_device", { p_token: "fcm-token-1", p_platform: "web" }],
            ["chat_unregister_device", { p_token: "fcm-token-1" }]
        ]);
        expect(cookies).toEqual(["fcm-token-1", null]);
        expect(flag.write).toHaveBeenCalledWith(true);
        expect(push.state()).toBe("offered");
    });

    test("Turn off on a load that registered nothing takes out the device the cookie keeps", async () => {
        const { deps, calls, cookies } = dependencies({ permission: "default", readCookie: () => "fcm-earlier" });
        const push = createPushRegistration(deps);
        await push.start();

        expect(await push.turnOff()).toBeNull();

        expect(calls).toEqual([{ name: "chat_unregister_device", args: { p_token: "fcm-earlier" } }]);
        expect(cookies).toEqual([null]);
    });

    test("Turn off with no cookie takes out the browser's own token", async () => {
        const { deps, calls } = dependencies({ permission: "granted", offFlag: offFlag(true) });
        const push = createPushRegistration(deps);
        await push.start();
        expect(calls).toEqual([]);

        await push.turnOff();

        expect(deps.getToken).toHaveBeenCalledTimes(1);
        expect(calls).toEqual([{ name: "chat_unregister_device", args: { p_token: "fcm-token-1" } }]);
    });

    test("an unregister that fails keeps the cookie and the off flag, answers a refusal, and the next load asks again", async () => {
        const flag = offFlag();
        let isRefused = true;
        const { deps, calls, cookies } = dependencies({ permission: "granted", offFlag: flag });
        deps.call = jest.fn(async (name: string, args: Record<string, unknown>) => {
            calls.push({ name, args });
            return name === "chat_unregister_device" && isRefused
                ? { ok: false, status: 503, error: { code: "rpc.unavailable", severity: "degraded", text: null }, retryAfterSeconds: null } as CallResult
                : registered;
        });
        const push = createPushRegistration(deps);
        await push.start();

        const refusal = await push.turnOff();

        expect(refusal?.code).toBe("push.unregister_failed");
        expect(refusal?.text).toBe("Notifications couldn't be turned off on this browser. Try again in a moment.");
        expect(cookies[cookies.length - 1]).toBe("fcm-token-1");
        expect(flag.write).not.toHaveBeenCalledWith(false);
        expect(push.state()).toBe("offered");

        isRefused = false;
        const nextLoad = createPushRegistration({ ...deps, readCookie: () => cookies[cookies.length - 1] });
        await nextLoad.start();

        expect(calls.map(c => c.name)).toEqual(["chat_register_device", "chat_unregister_device", "chat_unregister_device"]);
        expect(cookies[cookies.length - 1]).toBeNull();
    });

    test("a browser turned off here does not register on the next load, and Turn on registers it without asking", async () => {
        const flag = offFlag(true);
        const { deps, calls } = dependencies({ permission: "granted", offFlag: flag });
        const push = createPushRegistration(deps);

        await push.start();
        expect(calls).toEqual([]);
        expect(push.state()).toBe("offered");

        await push.turnOn();

        expect(deps.requestPermission).not.toHaveBeenCalled();
        expect(flag.write).toHaveBeenCalledWith(false);
        expect(calls.map(c => c.name)).toEqual(["chat_register_device"]);
        expect(push.state()).toBe("on");
    });
});
