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
// "Turn on notifications"; the church's setting decides when that button shows. A browser that
// already said yes registers again on every load without asking, which keeps its token current.
//
// The token goes to the platform under the person's token, through the one call path, and into a
// cookie: the Chat block is not on the page where a person signs out of Rock, so Rock reads the
// cookie there and takes this browser out of push itself.
import { ChatSettings } from "./useSession.partial";
import { CallResult } from "../platformCall.partial";

/** What push reaches outside itself. */
export type PushDependencies = {
    /** The push settings the latest exchange answered. */
    settings: () => ChatSettings["push"];

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
};

/** The part of a ServiceWorkerRegistration push reads: the worker in each slot. */
export type WorkerRegistration = Pick<ServiceWorkerRegistration, "installing" | "waiting" | "active">;

/**
 * How long a new push worker may take to become active. A small script activates in well under a
 * second; past this the load gives up and the next one tries again.
 */
const workerActivationMs = 10000;

/** Push as the shell holds it. */
export type PushRegistration = {
    /** Registers without asking where the browser already allowed it. */
    start: () => Promise<void>;

    /** Whether the "Turn on notifications" button shows. */
    isOffered: () => boolean;

    /** The person sent a message, which is when an after_send prompt shows. */
    noteSend: () => void;

    /** The button was clicked: asks the browser, and registers on a yes. */
    turnOn: () => Promise<void>;

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

    /** Whether the platform sent the Firebase values push needs. */
    const isAvailable = (): boolean => dependencies.settings().web !== null && dependencies.settings().vapid_key !== null;

    /**
     * Registers the worker, gets the token and gives it to the platform. Every await is checked
     * against the stop, so a page closed or signed out halfway registers nothing afterwards.
     */
    async function register(): Promise<void> {
        const push = dependencies.settings();

        try {
            const registration = await dependencies.registerWorker(dependencies.pagePath);
            if (isStopped) {
                return;
            }

            await whenActive(registration);
            if (isStopped) {
                return;
            }

            const token = await dependencies.getToken(push.web as Record<string, unknown>, push.vapid_key as string, registration);
            if (isStopped) {
                return;
            }

            const result = await dependencies.call("chat_register_device", { p_token: token, p_platform: "web" });
            if (isStopped || !result.ok) {
                return;
            }

            dependencies.writeCookie(token);
        }
        catch {
            // Push is extra: a browser that blocks it, or a Firebase that will not answer, leaves
            // chat working as before, and the next load tries again.
        }
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
            if (isStopped || !isAvailable() || dependencies.permission() !== "granted") {
                return;
            }

            await register();
        },

        isOffered: (): boolean => {
            if (isStopped || !isAvailable() || dependencies.permission() !== "default") {
                return false;
            }

            const prompt = dependencies.settings().prompt;
            return prompt === "offer" || (prompt === "after_send" && hasSent);
        },

        noteSend: (): void => {
            hasSent = true;
        },

        turnOn: async (): Promise<void> => {
            if (isStopped || !isAvailable()) {
                return;
            }

            const answer = await dependencies.requestPermission();
            if (isStopped || answer !== "granted") {
                return;
            }

            await register();
        },

        stop: (): void => {
            isStopped = true;
        }
    };
}
