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
// Signing in and keeping the platform token: the session with Rock's mint and the platform's
// exchange, the words a start says while it waits and the timers that move them on, and the page's
// back listeners that bring a waiting try forward. It has its own state and lifetime (the start's
// wait record, its stage timers, the listeners) and nothing to do with composing the shell's
// modules, so it lives beside the shell rather than inside it.
import { classifyActionFailure } from "./errors.partial";
import { exchangeChurchToken } from "./platformCall.partial";
import type { PresenceStatus } from "./composables/usePresenceStatus.partial";
import { ChatSession, ChurchTokenResult, createSession, ExchangeResult, RefreshReason } from "./composables/useSession.partial";
import type { ShellOptions, ShellState } from "./shell.partial";
import { ChatError } from "./types.partial";

/**
 * How long a start that has met a failure stays quiet before it says it is connecting, so a start
 * that only needs a second try says nothing. An estimate.
 */
export const startQuietMs = 1000;

/** How long a start whose first try has not answered at all stays quiet. An estimate. */
export const startSilentMs = 2500;

/**
 * When a start still waiting says so more gently: past the longest Retry-After the platform asks
 * for and about two backoffs after it. An estimate.
 */
export const startSlowMs = 20000;

/**
 * When a start still waiting points the person at their church: no outage of the platform's own
 * should run this long, and one that does is likely one that will not pass by itself. An estimate.
 */
export const startLongMs = 120000;

/** The words of a start that is still waiting, in the order it reaches them. */
const startWaitStages: Array<NonNullable<ShellState["startWait"]>> = ["connecting", "slow", "long"];

/** What the Rock token action answered. */
export type MintActionResult = {
    isSuccess: boolean;
    statusCode: number;
    data?: { gate?: string | null, churchToken?: string | null } | null;
};

/** What signing in reaches outside itself. */
export type ShellSessionDependencies = {
    state: ShellState;

    /** The shell's own options, of which signing in reads Rock's mint, the fetch and the page. */
    options: Pick<ShellOptions, "mintChurchToken" | "fetch" | "mark" | "pageTargets" | "reloadPage">;
    projectUrl: string;
    publishableKey: string;
    personAliasGuid: string;

    /** Tells the person about a failure. */
    report: (error: ChatError) => void;

    /** Puts the session's latest settings on the page. */
    showSettings: () => void;

    /** Hands the new token to the live connection, when there is one. */
    setAuth: () => Promise<void>;

    /** The session has ended: the shell leaves every topic and says why. */
    end: () => void;

    /**
     * Online status's side of each exchange: the body it carries, its answer, the server's clock
     * from the token it minted, and each refresh settling. Reached through closures, since the
     * status is made after signing in.
     */
    status: Pick<PresenceStatus, "exchangeBody" | "exchanged" | "clockFrom" | "settled">;
};

/** Signing in, as the shell holds it. */
export type ShellSession = {
    session: ChatSession;

    /**
     * The start's wait as the page tells it: when it began, how many of its tries failed, and
     * whether the first quiet second is over.
     */
    waited: Readonly<{ at: number, tries: number, isQuietOver: boolean }>;

    /** Signs in, saying on the page how long it has waited. Answers whether it signed in. */
    start: () => Promise<boolean>;

    /** Removes the page's listeners, so nothing heard afterwards asks for a token. */
    detach: () => void;
};

/**
 * A platform token's claims, or null where it holds none this client can read. The token's
 * signature is the platform's to check; this only reads what it says.
 */
export function tokenClaims(token: string | null): Record<string, unknown> | null {
    const payload = token?.split(".")[1];
    if (!payload) {
        return null;
    }

    try {
        const claims: unknown = JSON.parse(atob(payload.replace(/-/g, "+").replace(/_/g, "/")));
        return claims !== null && typeof claims === "object" && !Array.isArray(claims) ? claims as Record<string, unknown> : null;
    }
    catch {
        // Not a token this client can read, so it says nothing.
        return null;
    }
}

/** The person a platform token names, or null where it names none this client can read. */
export function tokenPerson(token: string | null): string | null {
    const sub = tokenClaims(token)?.sub;
    return typeof sub === "string" ? sub.toLowerCase() : null;
}

/**
 * The sentence shown for a gate that refused. The reason a person is refused is not always
 * theirs to know, so the sentences that could reveal a ban say no more than that chat is not
 * available to them.
 *
 * @param gate The gate code.
 *
 * @returns The sentence.
 */
export function gateMessage(gate: string | null): string {
    switch (gate) {
        case "sign_in_required":
            return "Sign in to use chat.";
        case "not_configured":
            return "Chat is not set up for this organization yet.";
        case "age_verification_required":
            return "Chat needs your birthdate before it can open.";
        case "age_restricted":
            return "Chat is not available at your age.";
        case "invalid_key":
        case "gate_unavailable":
            return "Chat could not start. Try again later, and let your church know if this keeps happening.";
        default:
            return "Chat is not available for your account. If you think this is wrong, let your church know.";
    }
}

/**
 * The sentence shown when chat does not open.
 *
 * @param phase Where the shell stopped.
 * @param gate The gate that refused, when one did.
 *
 * @returns The sentence.
 */
export function refusalMessage(phase: ShellState["phase"], gate: string | null): string {
    // Failed means Rock's gates passed and the platform did not finish signing in, so the
    // sentence is about chat not starting rather than about the person.
    return gateMessage(phase === "failed" ? "gate_unavailable" : gate);
}

/**
 * What a start still waiting says, on the first load's veil and once to a screen reader. No words
 * offer to try again: the session's wait is what honours the platform's Retry-After, and reload is
 * always the person's own.
 *
 * @param stage How long the start has waited.
 *
 * @returns The sentence.
 */
export function startWaitMessage(stage: NonNullable<ShellState["startWait"]>): string {
    switch (stage) {
        case "connecting":
            return "Connecting to chat…";
        case "slow":
            return "Still connecting. Chat will open as soon as it can.";
        default:
            return "Chat is still trying to connect. If this keeps happening, let your church know.";
    }
}

/**
 * What the Rock token action's answer means to the session.
 *
 * @param result What the action answered.
 *
 * @returns The church token, or the reason there is none.
 */
export function churchTokenFromAction(result: MintActionResult): ChurchTokenResult {
    if (result.isSuccess && result.data) {
        return { gate: result.data.gate ?? "gate_unavailable", churchToken: result.data.churchToken ?? null };
    }

    // Rock answers 401 when the person's Rock sign-in has ended.
    if (result.statusCode === 401) {
        return { gate: "sign_in_required", churchToken: null };
    }

    // No answer, too many requests, or a server fault may pass; anything else is Rock refusing.
    const isPassing = result.statusCode === 0 || result.statusCode === 429 || result.statusCode >= 500;

    return isPassing
        ? { gate: "gate_unavailable", churchToken: null, isUnreachable: true }
        : { gate: "gate_unavailable", churchToken: null };
}

/**
 * Creates the shell's sign-in.
 *
 * @param dependencies What signing in reaches outside itself.
 *
 * @returns The session and the start's wait.
 */
export function createShellSession(dependencies: ShellSessionDependencies): ShellSession {
    const { state, options, projectUrl, publishableKey, personAliasGuid, report, showSettings, setAuth, end, status } = dependencies;

    const session: ChatSession = createSession({
        mintChurchToken: async (): Promise<ChurchTokenResult> => {
            const result = await options.mintChurchToken();
            options.mark("chat:mint");

            if (!result.isSuccess && !churchTokenFromAction(result).isUnreachable) {
                report(classifyActionFailure(result.statusCode));
            }

            return churchTokenFromAction(result);
        },
        exchange,
        pushTokenToConnection: async (): Promise<void> => {
            showSettings();

            // A token for someone other than the person the page loaded for means a merge made
            // them part of another person. The page listens on a personal topic that is no longer
            // theirs, so it loads again rather than move every topic under a live connection.
            const person = tokenPerson(session.currentToken());
            if (person !== null && personAliasGuid !== "" && person !== personAliasGuid.toLowerCase()) {
                (options.reloadPage ?? (() => globalThis.location?.reload()))();
                return;
            }

            await setAuth();
        },
        random: Math.random,
        setTimer: (callback, milliseconds) => setTimeout(callback, milliseconds),
        clearTimer: handle => clearTimeout(handle as ReturnType<typeof setTimeout>),
        onEnded: () => end(),
        onWaiting: () => onStartWaiting(),
        settled: (reason, ok) => status.settled(reason, ok),
        // Away is the page hidden or the browser offline: what coming back, the one reason to try
        // sooner, is coming back from.
        isAway: () => options.pageTargets.document.visibilityState !== "visible" || globalThis.navigator?.onLine === false
    });

    /**
     * The start's wait as the page tells it: when it began, how many of its tries failed, and
     * whether the first quiet second is over.
     */
    const startWaited = { at: 0, tries: 0, isQuietOver: false };

    /** A try of the start failed in a way that may pass: past the quiet second, the page says it is connecting. */
    function onStartWaiting(): void {
        startWaited.tries++;
        if (startWaited.isQuietOver) {
            showStartWait("connecting");
        }
    }

    /** Moves the start's words on to a stage, never back, and only while it is still starting. */
    function showStartWait(stage: NonNullable<ShellState["startWait"]>): void {
        const reached = state.startWait === null ? -1 : startWaitStages.indexOf(state.startWait);
        if (state.phase === "starting" && startWaitStages.indexOf(stage) > reached) {
            state.startWait = stage;
        }
    }

    /**
     * The network coming back or the page shown is a reason to try again soon, whoever is waiting:
     * a start, or a refresh after one that failed. The session decides what that moves.
     */
    const onPageBack = (): void => {
        if (options.pageTargets.document.visibilityState === "visible") {
            session.nudge();
        }
    };

    /**
     * A page restored from the back-forward cache kept its token and its topics while it ran
     * nothing, so it asks again what the person may read; Realtime closes whatever she has lost
     * once the new token reaches it.
     */
    const onPageRestored = (event: unknown): void => {
        if ((event as { persisted?: unknown } | null)?.persisted === true) {
            void session.refresh("recheck");
        }
    };

    /**
     * Exchanges a church token for a platform token and the settings beside it, carrying online
     * status's body for the reason it was asked. The session's signal reaches the request, so a try
     * given up at its deadline ends the request with it.
     */
    async function exchange(churchToken: string, exchangeOptions: { signal: AbortSignal, reason: RefreshReason }): Promise<ExchangeResult> {
        const { signal, reason } = exchangeOptions;
        const body = status.exchangeBody(reason);
        const answer = await exchangeChurchToken(options.fetch, projectUrl, publishableKey, churchToken, {
            signal,
            region: session.settings().routes.region,
            ...(body ? { body } : {})
        });

        // A try given up is over, whatever it answered, even a body read in full just before the
        // abort: the session has moved on, so nothing it said is kept or counted.
        if (signal.aborted) {
            return answer;
        }
        options.mark("chat:exchange");

        // the server's clock from the token this answer minted, before its status is drawn by it
        const issuedAt = answer.ok ? tokenClaims(answer.accessToken)?.iat : undefined;
        if (typeof issuedAt === "number") {
            status.clockFrom(issuedAt);
        }
        // only whether the answer carried status, and what it said, reaches the store; never the token
        status.exchanged(body?.watched ?? [], answer.ok ? ("presence" in answer ? { presence: answer.presence } : {}) : null);

        return answer;
    }

    return {
        session,
        waited: startWaited,

        start: async (): Promise<boolean> => {
            options.pageTargets.window.addEventListener("online", onPageBack);
            options.pageTargets.document.addEventListener("visibilitychange", onPageBack);
            options.pageTargets.window.addEventListener("pageshow", onPageRestored);

            // The start's words: quiet for its first second, so a start that only needs a second
            // try says nothing; then connecting, once a try has failed, or once the first has gone
            // unanswered for longer; then gentler words the longer it waits.
            startWaited.at = Date.now();
            const stageTimers = [
                setTimeout(() => {
                    startWaited.isQuietOver = true;
                    if (startWaited.tries > 0) {
                        showStartWait("connecting");
                    }
                }, startQuietMs),
                setTimeout(() => showStartWait("connecting"), startSilentMs),
                setTimeout(() => showStartWait("slow"), startSlowMs),
                setTimeout(() => showStartWait("long"), startLongMs)
            ];

            const isSignedIn = await session.start();
            stageTimers.forEach(timer => clearTimeout(timer));

            return isSignedIn;
        },

        detach: (): void => {
            options.pageTargets.window.removeEventListener("online", onPageBack);
            options.pageTargets.document.removeEventListener("visibilitychange", onPageBack);
            options.pageTargets.window.removeEventListener("pageshow", onPageRestored);
        }
    };
}
