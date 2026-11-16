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
// Web push on this browser. A browser that has been refused once can never be asked again, and
// Firefox and Safari ask only from a click, so the browser is asked only when the person clicks
// "Turn on notifications"; the church's setting decides when that button shows.
//
// The browser's permission belongs to whoever granted it, not to the person signed in now, so it
// never decides alone whose notifications this browser shows. A load registers only on this
// person's own yes on this browser, their Turn on, remembered here per church and person, which
// keeps the token current on every load after it. A sign-in that ends with the browser session (a
// phone-number sign-in, an impersonation, a password sign-in without Keep me logged in) never
// registers: closing the browser is no sign-out Rock sees, so the next person to open it would get
// this person's notifications. Every load that will not register lets go the token the browser
// holds, whoever registered it, so the last person's notifications stop arriving here even when
// their sign-out could not reach the platform.
//
// The token goes to the platform under the person's token, through the one call path, and into a
// cookie: the Chat block is not on the page where a person signs out of Rock, so Rock reads the
// cookie there and takes this browser out of push itself.
//
// The church's prompt timing decides only when the header offers push unasked, and only to a
// person who has not chosen here. Chat settings shows this browser's push as one state and offers
// Turn on whatever that timing, since the person came to the setting themselves. Turn off takes
// the device out and is remembered as their choice; an unregister that fails keeps the cookie, so
// the next load asks again.
import { ChatSettings } from "./useSession.partial";
import { CallResult } from "../platformCall.partial";
import { failedWords } from "../errors.partial";
import { ChatError } from "../types.partial";

/** What push reaches outside itself. */
export type PushDependencies = {
    /** The push settings the latest exchange answered. */
    settings: () => ChatSettings["push"];

    /** Whether the person's sign-in outlasts the browser session; one that does not never registers. */
    isSignInKept: boolean;

    /** The path of the page the Chat block is on, which a tap on a notification opens. */
    pagePath: string;

    /** The browser's notification permission: default, granted or denied. */
    permission: () => string;

    /** Asks the browser for permission. Only ever called from a click. */
    requestPermission: () => Promise<string>;

    /** Registers the push worker for this page, answering its registration. */
    registerWorker: (pagePath: string) => Promise<WorkerRegistration>;

    /** Loads Firebase and answers this browser's messaging token. */
    getToken: (web: Record<string, unknown>, vapidKey: string, registration: unknown) => Promise<string>;

    /** Calls a platform function through the shell's one call path. */
    call: (name: string, args: Record<string, unknown>) => Promise<CallResult>;

    /** Keeps this browser's token for Rock's sign-out, or clears it with null. */
    writeCookie: (token: string | null) => void;

    /** The token the cookie keeps from an earlier load, or null. */
    readCookie: () => string | null;

    /**
     * This person's choice of push on this browser, kept in its storage, or null where they never
     * chose; none keeps nothing, so only this load's Turn on registers.
     */
    choice?: { read: () => PushChoice | null, write: (choice: PushChoice) => void };
};

/** A person's own choice of push on one browser: their Turn on or their Turn off. */
export type PushChoice = "on" | "off";

/**
 * This browser's push as Chat settings draws it: offered, the browser's own box up, on, refused by
 * the browser, not set up where the platform sent no Firebase values, or not kept for a sign-in
 * that ends with the browser session.
 */
export type PushState = "offered" | "asking" | "on" | "blocked" | "unavailable" | "not_kept";

/** The part of a ServiceWorkerRegistration push reads: the worker in each slot. */
export type WorkerRegistration = Pick<ServiceWorkerRegistration, "installing" | "waiting" | "active">;

/**
 * How long a new push worker may take to become active. A small script activates in well under a
 * second; past this the load gives up and the next one tries again.
 */
const workerActivationMs = 10000;

/** Push as the shell holds it. */
export type PushRegistration = {
    /** Registers where this person said yes here, or else lets go the token the browser holds. */
    start: () => Promise<void>;

    /** Whether the "Turn on notifications" button shows. */
    isOffered: () => boolean;

    /** The person sent a message, which is when an after_send prompt shows. */
    noteSend: () => void;

    /** The button was clicked: asks the browser, and registers on a yes. */
    turnOn: () => Promise<void>;

    /** Takes this browser's device out of push and remembers it here; answers why it could not, or null. */
    turnOff: () => Promise<ChatError | null>;

    /** This browser's push as Chat settings draws it. */
    state: () => PushState;

    /** Stops: nothing on its way registers, and nothing new starts. */
    stop: () => void;
};

/**
 * Creates push for this browser.
 *
 * @param dependencies What push reaches outside itself.
 *
 * @returns The registration.
 */
export function createPushRegistration(dependencies: PushDependencies): PushRegistration {
    let isStopped = false;
    let hasSent = false;

    /** Turn off invalidates every registration that has not yet reached the platform. */
    let operationGeneration = 0;

    /** Sent registrations must settle before Turn off can remove the devices they may enable. */
    const registering = new Set<{ token: string, result: Promise<CallResult> }>();

    /** Set while the browser's own permission box is up. */
    let isAsking = false;

    /** The token this browser registered under on this load, which Turn off takes out. */
    let registeredToken: string | null = null;

    /** The load's letting go of the browser's token while it is on its way, which a Turn on waits for. */
    let releasing: Promise<void> = Promise.resolve();

    const readChoice = (): PushChoice | null => {
        try {
            return dependencies.choice?.read() ?? null;
        }
        catch {
            // a browser that blocks its storage remembers nothing, so only this load's Turn on registers
            return null;
        }
    };

    const writeChoice = (choice: PushChoice): void => {
        try {
            dependencies.choice?.write(choice);
        }
        catch {
            // as above
        }
    };

    /**
     * Whether a load registers: only on this person's own yes here, under a sign-in that outlasts
     * the browser session, while the browser still allows notifications.
     */
    const mayRegisterOnLoad = (): boolean => dependencies.isSignInKept && readChoice() === "on" && dependencies.permission() === "granted";

    const readCookie = (): string | null => {
        try {
            return dependencies.readCookie() || null;
        }
        catch {
            return null;
        }
    };

    /** Whether the platform sent the Firebase values push needs. */
    const isAvailable = (): boolean => dependencies.settings().web !== null && dependencies.settings().vapid_key !== null;

    /**
     * Registers the worker, gets the token and gives it to the platform. Every await is checked
     * against the stop, so a page closed or signed out halfway registers nothing afterwards.
     */
    async function register(generation: number): Promise<void> {
        const push = dependencies.settings();
        const isCurrent = (): boolean => !isStopped && generation === operationGeneration;

        try {
            const registration = await dependencies.registerWorker(dependencies.pagePath);
            if (!isCurrent()) {
                return;
            }

            await whenActive(registration);
            if (!isCurrent()) {
                return;
            }

            const token = await dependencies.getToken(push.web as Record<string, unknown>, push.vapid_key as string, registration);
            if (!isCurrent()) {
                return;
            }

            const pending = { token, result: dependencies.call("chat_register_device", { p_token: token, p_platform: "web" }) };
            registering.add(pending);
            try {
                const result = await pending.result;
                if (!isCurrent() || !result.ok) {
                    return;
                }

                registeredToken = token;
                dependencies.writeCookie(token);
            }
            finally {
                registering.delete(pending);
            }
        }
        catch {
            // Push is extra: a browser that blocks it, or a Firebase that will not answer, leaves
            // chat working as before, and the next load tries again.
        }
    }

    /**
     * The token the platform holds for this browser: the one this load registered, else the one
     * an earlier load kept in the cookie, else the browser's own, which a load that never
     * registered still has from an earlier one. Null where none can be had.
     */
    async function heldToken(): Promise<string | null> {
        const kept = registeredToken ?? readCookie();
        if (kept || !isAvailable() || dependencies.permission() !== "granted") {
            return kept;
        }

        try {
            const registration = await dependencies.registerWorker(dependencies.pagePath);
            await whenActive(registration);
            const push = dependencies.settings();
            return await dependencies.getToken(push.web as Record<string, unknown>, push.vapid_key as string, registration);
        }
        catch {
            return null;
        }
    }

    /**
     * Takes the held device out, whoever registered it, since the platform lets the holder of a
     * token remove it. A failure keeps the token in the cookie, so the next load, which will not
     * register, asks again, and Rock's sign-out still reaches it.
     *
     * @param registered The token to take out, or none for the one this browser holds.
     * @param isCurrent Whether the caller still wants the answer kept, read after every await.
     */
    async function unregister(registered?: string, isCurrent: () => boolean = () => !isStopped): Promise<ChatError | null> {
        const token = registered ?? await heldToken();
        if (!token || !isCurrent()) {
            return null;
        }

        const result = await dependencies.call("chat_unregister_device", { p_token: token }).catch(() => null);
        if (!isCurrent()) {
            return null;
        }

        if (result?.ok) {
            registeredToken = null;
            dependencies.writeCookie(null);
            return null;
        }

        dependencies.writeCookie(token);
        return {
            code: "push.unregister_failed",
            severity: "failed",
            text: (result && !result.ok ? result.error.text : null) ?? failedWords("Notifications couldn't be turned off on this browser.").alone
        };
    }

    /**
     * Lets go the token this browser holds on a load that will not register, whoever registered
     * it: the cookie's, else the browser's own, since a sign-out that failed to reach the platform
     * still expires the cookie. A Turn on in this tab, or this person's yes from another tab, wins:
     * the choice is read again once the token is had, and an answer arriving after either is not
     * kept. Nothing is said of a failure; the cookie keeps the token and the next load tries again.
     */
    async function release(generation: number): Promise<void> {
        const isCurrent = (): boolean => !isStopped && generation === operationGeneration && !mayRegisterOnLoad();
        await unregister(undefined, isCurrent);
    }

    /**
     * Settles once the registration has an active worker. On a browser's first visit the worker
     * is still installing, and a token asked for then waits on a push subscription that never
     * settles. navigator.serviceWorker.ready cannot be used: it waits for a worker whose scope
     * holds this page, and the push worker's scope is its own folder.
     */
    function whenActive(registration: WorkerRegistration): Promise<void> {
        // An update installs beside a worker that is already active, which push can use at once.
        const worker = registration.active ?? registration.waiting ?? registration.installing;
        if (worker?.state === "activated") {
            return Promise.resolve();
        }

        if (!worker) {
            return Promise.reject(new Error("The push registration holds no worker."));
        }

        return new Promise<void>((resolve, reject) => {
            const timer = window.setTimeout(() => finish(new Error("The push worker did not activate.")), workerActivationMs);

            function finish(error?: Error): void {
                window.clearTimeout(timer);
                worker?.removeEventListener("statechange", onStateChange);
                if (error) {
                    reject(error);
                }
                else {
                    resolve();
                }
            }

            function onStateChange(): void {
                if (worker?.state === "activated") {
                    finish();
                }
                else if (worker?.state === "redundant") {
                    finish(new Error("The push worker was discarded."));
                }
            }

            worker.addEventListener("statechange", onStateChange);
        });
    }

    return {
        start: async (): Promise<void> => {
            if (isStopped || !isAvailable()) {
                return;
            }

            if (mayRegisterOnLoad()) {
                await register(operationGeneration);
                return;
            }

            releasing = release(operationGeneration);
            await releasing;
        },

        // Unasked only to a person who has not chosen here, under a kept sign-in, while the
        // browser can still ask or already allows, and not once this load registered.
        isOffered: (): boolean => {
            if (isStopped || !isAvailable() || !dependencies.isSignInKept || dependencies.permission() === "denied"
                || readChoice() !== null || registeredToken !== null) {
                return false;
            }

            const prompt = dependencies.settings().prompt;
            return prompt === "offer" || (prompt === "after_send" && hasSent);
        },

        noteSend: (): void => {
            hasSent = true;
        },

        turnOn: async (): Promise<void> => {
            if (isStopped || !isAvailable() || !dependencies.isSignInKept) {
                return;
            }

            const generation = ++operationGeneration;
            // A browser that already allows notifications, by anyone's yes, needs no asking.
            if (dependencies.permission() !== "granted") {
                isAsking = true;
                const answer = await dependencies.requestPermission().finally(() => {
                    isAsking = false;
                });
                if (isStopped || generation !== operationGeneration || answer !== "granted") {
                    return;
                }
            }

            writeChoice("on");
            // A release this load sent may still land on the platform; the register goes after it,
            // so it can never take out the device this registers.
            await releasing;
            if (isStopped || generation !== operationGeneration) {
                return;
            }

            await register(generation);
        },

        turnOff: async (): Promise<ChatError | null> => {
            if (isStopped) {
                return null;
            }

            operationGeneration++;
            writeChoice("off");
            const pending = [...registering];
            let failure = await unregister();
            for (const registration of pending) {
                // Even a lost answer may have enabled the device. Removal must follow the RPC.
                await registration.result.catch(() => null);
                failure = await unregister(registration.token) ?? failure;
            }
            return failure;
        },

        state: (): PushState => {
            if (!isAvailable()) {
                return "unavailable";
            }

            if (!dependencies.isSignInKept) {
                return "not_kept";
            }

            if (isAsking) {
                return "asking";
            }

            const permission = dependencies.permission();
            if (permission === "denied") {
                return "blocked";
            }

            // a Turn off whose unregister failed keeps its token here, and still reads as off
            return permission === "granted" && registeredToken !== null && readChoice() !== "off" ? "on" : "offered";
        },

        stop: (): void => {
            isStopped = true;
        }
    };
}
