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
// setting decides when the button shows. A browser's permission belongs to whoever granted it, so
// only this person's own yes on this browser registers on load, and never for a sign-in that ends
// with the browser session; any other load lets go the token the browser holds, so the last
// person's notifications stop arriving here. The token goes to the platform under the person's
// token and into a cookie, so a sign-out anywhere in Rock can take this browser out of push.
import { createPushRegistration, PushChoice, PushDependencies } from "../../../../../src/Communication/Chat/ChatShell/composables/usePush.partial";
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

/** This person's choice on this browser, as local storage holds it, shared by every load given it. */
function choice(initial: PushChoice | null = null): { read: () => PushChoice | null, write: jest.Mock, set: (value: PushChoice | null) => void } {
    let current = initial;
    return {
        read: () => current,
        write: jest.fn((value: PushChoice) => {
            current = value;
        }),
        // another tab of this person's writes the same key
        set: (value: PushChoice | null) => {
            current = value;
        }
    };
}

/** Dependencies with every outside call recorded, overridable per test. */
function dependencies(overrides: Partial<PushDependencies> & { prompt?: string, permission?: string } = {}) {
    const calls: Array<{ name: string, args: Record<string, unknown> }> = [];
    const cookies: Array<string | null> = [];
    let permission = overrides.permission ?? "default";

    const base: PushDependencies = {
        settings: () => ({ web, vapid_key: "vapid", prompt: overrides.prompt ?? "after_send" }),
        isSignInKept: true,
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

    test("a browser this person said yes on registers on load without asking, through the one call path, and keeps the token in the cookie", async () => {
        const { deps, calls, cookies } = dependencies({ permission: "granted", prompt: "off", choice: choice("on") });
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
        const { deps, calls, cookies } = dependencies({ permission: "granted", choice: choice("on") });
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
            const { deps, calls, cookies } = dependencies({ permission: "granted", choice: choice("on") });
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
        const { deps, calls } = dependencies({ permission: "granted", choice: choice("on") });
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
        const { deps, calls, cookies } = dependencies({ permission: "granted", choice: choice("on") });
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
        const { deps, cookies } = dependencies({ permission: "granted", choice: choice("on") });
        deps.call = jest.fn(async () => refused);
        const push = createPushRegistration(deps);

        await push.start();

        expect(cookies).toEqual([]);
    });

    test("a stop while the token is on its way registers nothing once it arrives", async () => {
        const token = deferred<string>();
        const { deps, calls, cookies } = dependencies({ permission: "granted", choice: choice("on") });
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
        const { deps } = dependencies({ permission: "granted", choice: choice("on") });
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
        const { deps, cookies } = dependencies({ permission: "granted", choice: choice("on") });
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
// browser's own box is up, on, blocked once the browser said no, not set up where the platform
// sent no Firebase values, or not kept for a sign-in that ends with the browser session. The row
// offers Turn on whatever the church's prompt timing, since the person came to the setting
// themselves. Turn off takes this browser's device out of push and clears the sign-out cookie, and
// is remembered on this browser as this person's choice, so the next load does not register it
// again while the browser still allows notifications; Turn on then needs no ask.
describe("the browser row's state", () => {
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

    test.each(["startup", "Turn on"])("%s shows on only after its delayed register succeeds", async (mode) => {
        const answer = deferred<CallResult>();
        const { deps } = dependencies({ permission: "granted", choice: choice("on") });
        deps.call = jest.fn(() => answer.promise);
        const push = createPushRegistration(deps);

        const registering = mode === "startup" ? push.start() : push.turnOn();
        await settle();
        expect(deps.call).toHaveBeenCalledWith("chat_register_device", expect.anything());
        // a click is answered at once by its own busy state; a load's register shows nothing
        expect(push.state()).toBe(mode === "startup" ? "offered" : "turning_on");

        answer.resolve(registered);
        await registering;
        expect(push.state()).toBe("on");
    });

    test.each([
        ["startup", "Firebase"], ["startup", "platform"],
        ["Turn on", "Firebase"], ["Turn on", "platform"]
    ])("%s remains retryable after %s refuses registration", async (mode, failure) => {
        const token = deferred<string>();
        const answer = deferred<CallResult>();
        const { deps } = dependencies({ permission: "granted", choice: choice("on") });
        deps.getToken = jest.fn(() => token.promise);
        deps.call = jest.fn(() => answer.promise);
        const push = createPushRegistration(deps);

        const registering = mode === "startup" ? push.start() : push.turnOn();
        await settle();
        if (failure === "Firebase") {
            token.reject(new Error("messaging/permission-blocked"));
        }
        else {
            token.resolve("fcm-token-1");
            await settle();
            answer.resolve({ ok: false, status: 503, error: { code: "rpc.read_only", severity: "degraded", text: "Read only tonight." }, retryAfterSeconds: null });
        }
        const refusal = await registering;
        expect(push.state()).toBe("offered");
        // a click that fails says so, in the platform's words where it gave some; a load that
        // fails tries again on the next one, unsaid
        if (mode === "Turn on") {
            expect(refusal).toEqual({
                code: "push.register_failed",
                severity: "failed",
                text: failure === "platform" ? "Read only tonight." : "Notifications couldn't be turned on for this browser. Try again in a moment."
            });
        }
        else {
            expect(refusal).toBeUndefined();
        }

        deps.getToken = jest.fn(async () => "fcm-token-1");
        deps.call = jest.fn(async () => registered);
        await push.turnOn();
        expect(push.state()).toBe("on");
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
        const flag = choice("on");
        const { deps, calls, cookies } = dependencies({ permission: "granted", choice: flag });
        const push = createPushRegistration(deps);
        await push.start();
        expect(push.state()).toBe("on");

        await push.turnOff();

        expect(calls.map(c => [c.name, c.args])).toEqual([
            ["chat_register_device", { p_token: "fcm-token-1", p_platform: "web" }],
            ["chat_unregister_device", { p_token: "fcm-token-1" }]
        ]);
        expect(cookies).toEqual(["fcm-token-1", null]);
        expect(flag.write).toHaveBeenCalledWith("off");
        expect(push.state()).toBe("offered");
    });

    test("Turn off after a load that registered nothing has nothing left to take out", async () => {
        // The load itself let go the device the cookie kept, since this person never chose push here.
        const { deps, calls, cookies } = dependencies({ permission: "default" });
        cookies.push("fcm-earlier");
        const push = createPushRegistration(deps);
        await push.start();

        expect(await push.turnOff()).toBeNull();

        expect(calls).toEqual([{ name: "chat_unregister_device", args: { p_token: "fcm-earlier" } }]);
        expect(cookies).toEqual(["fcm-earlier", null]);
    });

    test("Turn off overtakes a startup token and leaves the earlier device removed", async () => {
        const token = deferred<string>();
        const { deps, calls, cookies } = dependencies({ permission: "granted", choice: choice("on") });
        cookies.push("fcm-earlier");
        deps.getToken = jest.fn(() => token.promise);
        const push = createPushRegistration(deps);

        const started = push.start();
        await settle();
        expect(deps.getToken).toHaveBeenCalledTimes(1);
        expect(await push.turnOff()).toBeNull();
        token.resolve("fcm-new");
        await started;

        expect(calls).toEqual([{ name: "chat_unregister_device", args: { p_token: "fcm-earlier" } }]);
        expect(cookies[cookies.length - 1]).toBeNull();
        expect(push.state()).toBe("offered");
    });

    test("Turn off waits for a sent register and removes its device after it succeeds", async () => {
        const answer = deferred<CallResult>();
        const enabled = new Set(["fcm-earlier"]);
        const { deps, calls, cookies } = dependencies({ permission: "granted", choice: choice("on") });
        cookies.push("fcm-earlier");
        deps.getToken = jest.fn(async () => "fcm-new");
        deps.call = jest.fn(async (name, args) => {
            calls.push({ name, args });
            if (name === "chat_register_device") {
                const result = await answer.promise;
                if (result.ok) {
                    enabled.add(args.p_token as string);
                }
                return result;
            }
            enabled.delete(args.p_token as string);
            return registered;
        });
        const push = createPushRegistration(deps);
        const started = push.start();
        await settle();
        expect(calls[0].name).toBe("chat_register_device");

        let hasTurnedOff = false;
        const turningOff = push.turnOff().then(result => {
            hasTurnedOff = true;
            return result;
        });
        await settle();
        expect(enabled.has("fcm-earlier")).toBe(false);
        expect(hasTurnedOff).toBe(false);

        answer.resolve(registered);
        await started;
        expect(await turningOff).toBeNull();
        expect([...enabled]).toEqual([]);
        expect(calls[calls.length - 1]).toEqual({ name: "chat_unregister_device", args: { p_token: "fcm-new" } });
        expect(cookies[cookies.length - 1]).toBeNull();
        expect(push.state()).toBe("offered");
    });

    test("Turn off with no cookie takes out the browser's own token", async () => {
        // allowed only after the load, so the load had no token of its own to let go
        const { deps, calls, setPermission } = dependencies({ permission: "default", choice: choice("off") });
        const push = createPushRegistration(deps);
        await push.start();
        expect(calls).toEqual([]);

        setPermission("granted");
        await push.turnOff();

        expect(deps.getToken).toHaveBeenCalledTimes(1);
        expect(calls).toEqual([{ name: "chat_unregister_device", args: { p_token: "fcm-token-1" } }]);
    });

    test("an unregister that fails keeps the cookie and the choice of off, answers a refusal, and the next load asks again", async () => {
        const flag = choice("on");
        let isRefused = true;
        const { deps, calls, cookies } = dependencies({ permission: "granted", choice: flag });
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
        expect(flag.read()).toBe("off");
        expect(push.state()).toBe("offered");

        isRefused = false;
        const nextLoad = createPushRegistration({ ...deps, readCookie: () => cookies[cookies.length - 1] });
        await nextLoad.start();

        expect(calls.map(c => c.name)).toEqual(["chat_register_device", "chat_unregister_device", "chat_unregister_device"]);
        expect(cookies[cookies.length - 1]).toBeNull();
    });

    test("a browser turned off here does not register on the next load, and Turn on registers it without asking", async () => {
        const flag = choice("off");
        const { deps, calls } = dependencies({ permission: "granted", choice: flag });
        const push = createPushRegistration(deps);

        // the load lets go the browser's own token, which an earlier person may have left registered
        await push.start();
        expect(calls.map(c => c.name)).toEqual(["chat_unregister_device"]);
        expect(push.state()).toBe("offered");

        await push.turnOn();

        expect(deps.requestPermission).not.toHaveBeenCalled();
        expect(flag.write).toHaveBeenCalledWith("on");
        expect(calls.map(c => c.name)).toEqual(["chat_unregister_device", "chat_register_device"]);
        expect(push.state()).toBe("on");
    });
});

// A browser's permission belongs to whoever granted it, so a load registers only on this person's
// own yes here, and never for a sign-in that ends with the browser session: a phone-number sign-in,
// an impersonation, or a password sign-in without Keep me logged in. Every other load lets go the
// token the browser holds, so whoever used this browser before stops getting notifications on it.
describe("whose push this browser carries", () => {
    test("a sign-in that is not kept never registers, offers nothing, and its Turn on does nothing", async () => {
        const { deps, calls } = dependencies({ permission: "granted", prompt: "offer", isSignInKept: false, choice: choice("on") });
        const push = createPushRegistration(deps);

        await push.start();
        push.noteSend();
        expect(calls.filter(c => c.name === "chat_register_device")).toEqual([]);
        expect(push.state()).toBe("not_kept");
        expect(push.isOffered()).toBe(false);

        await push.turnOn();

        expect(deps.requestPermission).not.toHaveBeenCalled();
        expect(calls.filter(c => c.name === "chat_register_device")).toEqual([]);
        expect(push.state()).toBe("not_kept");
    });

    test("a sign-in that is not kept lets go the token an earlier sign-in left in the cookie", async () => {
        const { deps, calls, cookies } = dependencies({ permission: "granted", isSignInKept: false });
        cookies.push("tok-a");
        const push = createPushRegistration(deps);

        await push.start();

        expect(calls).toEqual([{ name: "chat_unregister_device", args: { p_token: "tok-a" } }]);
        expect(cookies[cookies.length - 1]).toBeNull();
    });

    test("a browser someone else allowed, where this person never chose, registers nothing, lets go the browser's own token and offers Turn on", async () => {
        const { deps, calls } = dependencies({ permission: "granted", prompt: "offer", choice: choice() });
        const push = createPushRegistration(deps);

        await push.start();

        expect(calls).toEqual([{ name: "chat_unregister_device", args: { p_token: "fcm-token-1" } }]);
        expect(push.state()).toBe("offered");
        expect(push.isOffered()).toBe(true);
    });

    test("Turn on is this person's yes on this browser: it is remembered, and the next load registers without asking", async () => {
        const flag = choice();
        const first = dependencies({ permission: "granted", choice: flag });
        const push = createPushRegistration(first.deps);
        await push.start();

        await push.turnOn();

        expect(flag.read()).toBe("on");
        expect(push.state()).toBe("on");
        expect(push.isOffered()).toBe(false);

        const next = dependencies({ permission: "granted", choice: flag });
        const nextLoad = createPushRegistration(next.deps);
        await nextLoad.start();

        expect(next.deps.requestPermission).not.toHaveBeenCalled();
        expect(next.calls.map(c => c.name)).toEqual(["chat_register_device"]);
        expect(nextLoad.state()).toBe("on");
    });

    test("a Turn on during the load's release waits for it, so the device it registers stays and keeps its cookie", async () => {
        const released = deferred<CallResult>();
        const enabled = new Set(["fcm-token-1"]);
        const { deps, calls, cookies } = dependencies({ permission: "granted", prompt: "offer", choice: choice() });
        deps.call = jest.fn(async (name, args) => {
            calls.push({ name, args });
            if (name === "chat_unregister_device") {
                // the release lands on the platform only when its answer comes back
                const result = await released.promise;
                enabled.delete(args.p_token as string);
                return result;
            }
            enabled.add(args.p_token as string);
            return registered;
        });
        const push = createPushRegistration(deps);

        const started = push.start();
        await settle();
        expect(calls.map(c => c.name)).toEqual(["chat_unregister_device"]);

        const turning = push.turnOn();
        await settle();
        released.resolve({ ok: true, data: { token: "fcm-token-1", removed: true } });
        await started;
        await turning;

        expect(calls.map(c => c.name)).toEqual(["chat_unregister_device", "chat_register_device"]);
        expect(enabled.has("fcm-token-1")).toBe(true);
        expect(cookies[cookies.length - 1]).toBe("fcm-token-1");
        expect(push.state()).toBe("on");
    });

    test("a release reads the choice again before it sends, so a Turn on in another tab is not undone", async () => {
        const token = deferred<string>();
        const flag = choice();
        const { deps, calls } = dependencies({ permission: "granted", choice: flag });
        deps.getToken = jest.fn(() => token.promise);
        const push = createPushRegistration(deps);

        const started = push.start();
        await settle();
        flag.set("on");
        token.resolve("fcm-token-1");
        await started;

        expect(calls).toEqual([]);
    });

    test("a release that a Turn off overtakes keeps nothing of its own answer, so a failure arriving late puts no cookie back", async () => {
        const released = deferred<CallResult>();
        const { deps, calls, cookies } = dependencies({ permission: "granted", choice: choice() });
        let unregisters = 0;
        deps.call = jest.fn(async (name, args) => {
            calls.push({ name, args });
            // the load's release waits; Turn off's own unregister answers at once
            return ++unregisters === 1 ? released.promise : { ok: true, data: { token: args.p_token, removed: true } } as CallResult;
        });
        const push = createPushRegistration(deps);

        const started = push.start();
        await settle();
        expect(await push.turnOff()).toBeNull();
        expect(cookies[cookies.length - 1]).toBeNull();

        released.resolve({ ok: false, status: 503, error: { code: "rpc.unavailable", severity: "degraded", text: null }, retryAfterSeconds: null });
        await started;

        expect(calls.map(c => c.name)).toEqual(["chat_unregister_device", "chat_unregister_device"]);
        expect(cookies[cookies.length - 1]).toBeNull();
    });
});

// Turn on is one busy state from the click until the register settles: the header's offer goes
// at once, a second click does nothing, the browser's own box reads as asking only while it is
// open, and a failure is said rather than falling back to Off unsaid.
describe("a Turn on in progress", () => {
    test("a one-click Turn on is busy from the click, takes the header's offer away at once, and a second click does nothing", async () => {
        const answer = deferred<CallResult>();
        const { deps, calls } = dependencies({ permission: "granted", prompt: "offer", choice: choice() });
        const push = createPushRegistration(deps);
        await push.start();
        expect(push.isOffered()).toBe(true);
        deps.call = jest.fn(async (name: string, args: Record<string, unknown>) => {
            calls.push({ name, args });
            return answer.promise;
        });

        const turning = push.turnOn();
        expect(push.state()).toBe("turning_on");
        expect(push.isOffered()).toBe(false);

        const again = push.turnOn();
        await settle();
        expect(calls.filter(c => c.name === "chat_register_device")).toHaveLength(1);

        answer.resolve(registered);
        expect(await turning).toBeNull();
        await again;
        expect(push.state()).toBe("on");
    });

    test("reads as asking only while the browser's box is open, then busy until the register settles", async () => {
        const allowed = deferred<string>();
        const answer = deferred<CallResult>();
        const { deps, setPermission } = dependencies({ choice: choice() });
        deps.requestPermission = jest.fn(() => allowed.promise);
        const push = createPushRegistration(deps);
        await push.start();
        deps.call = jest.fn(() => answer.promise);

        const turning = push.turnOn();
        expect(push.state()).toBe("asking");

        setPermission("granted");
        allowed.resolve("granted");
        await settle();
        expect(push.state()).toBe("turning_on");

        answer.resolve(registered);
        await turning;
        expect(push.state()).toBe("on");
    });

    test("a box closed with no answer is Off again with nothing to say", async () => {
        const { deps } = dependencies({ prompt: "offer", choice: choice() });
        deps.requestPermission = jest.fn(async () => "default");
        const push = createPushRegistration(deps);
        await push.start();

        expect(await push.turnOn()).toBeNull();

        expect(push.state()).toBe("offered");
        expect(push.isOffered()).toBe(true);
    });
});

// A load that lets go the browser's token remembers it for this church on this browser, so the
// next load that will not register neither loads Firebase nor calls the platform again; any
// register here forgets it, so the load after a sign-out that failed to reach the platform still
// lets go what that person left.
describe("a release remembered", () => {
    function mark(initial = false): { read: () => boolean, write: jest.Mock } {
        let isReleased = initial;
        return {
            read: () => isReleased,
            write: jest.fn((value: boolean) => {
                isReleased = value;
            })
        };
    }

    test("a load that let go the browser's token is remembered, and the next such load asks nothing", async () => {
        const released = mark();
        const first = dependencies({ permission: "granted", choice: choice(), released });
        await createPushRegistration(first.deps).start();
        expect(first.calls.map(c => c.name)).toEqual(["chat_unregister_device"]);
        expect(released.read()).toBe(true);

        const next = dependencies({ permission: "granted", choice: choice(), released });
        await createPushRegistration(next.deps).start();

        expect(next.deps.getToken).not.toHaveBeenCalled();
        expect(next.calls).toEqual([]);
    });

    test("a register forgets it, so the next load that will not register lets go again", async () => {
        const released = mark(true);
        const flag = choice("on");
        const kept = dependencies({ permission: "granted", choice: flag, released });
        await createPushRegistration(kept.deps).start();
        expect(released.read()).toBe(false);

        // the next person's load, after a sign-out that expired the cookie but never reached the platform
        const next = dependencies({ permission: "granted", choice: choice(), released });
        await createPushRegistration(next.deps).start();

        expect(next.calls).toEqual([{ name: "chat_unregister_device", args: { p_token: "fcm-token-1" } }]);
    });

    test("a cookie still holding a token is let go whatever was remembered", async () => {
        const released = mark(true);
        const { deps, calls, cookies } = dependencies({ permission: "granted", choice: choice(), released });
        cookies.push("tok-left");

        await createPushRegistration(deps).start();

        expect(calls).toEqual([{ name: "chat_unregister_device", args: { p_token: "tok-left" } }]);
    });

    test("a release that fails is not remembered", async () => {
        const released = mark();
        const { deps } = dependencies({ permission: "granted", choice: choice(), released });
        deps.call = jest.fn(async () => ({ ok: false, status: 503, error: { code: "rpc.unavailable", severity: "degraded", text: null }, retryAfterSeconds: null }) as CallResult);

        await createPushRegistration(deps).start();

        expect(released.read()).toBe(false);
    });
});
