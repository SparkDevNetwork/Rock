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
        registerWorker: jest.fn(async (pagePath: string) => ({ scope: "/Scripts/Rock/Chat/", pagePath })),
        getToken: jest.fn(async () => "fcm-token-1"),
        call: jest.fn(async (name: string, args: Record<string, unknown>) => {
            calls.push({ name, args });
            return registered;
        }),
        writeCookie: (token: string | null) => {
            cookies.push(token);
        }
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
