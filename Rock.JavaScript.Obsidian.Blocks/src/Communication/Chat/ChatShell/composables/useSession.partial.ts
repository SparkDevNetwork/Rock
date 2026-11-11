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
// The person's session on the chat platform. Rock signs a short church token after running its
// gates; the platform exchanges it for its own token; the platform token is refreshed the same
// way before it expires, so every refresh passes through Rock's gates again.
//
// Every exchange also answers the platform's settings for this church, which is how a client
// released years ago still learns the service state, the route of the send and its limits: it is
// the one call every client already makes at load and every few minutes.
import { shallowRef } from "vue";
import { maintenanceSentence, readOnlySentence } from "../errors.partial";
import { computeBackoff } from "../platformCall.partial";
import { ChatError } from "../types.partial";

/* eslint-disable @typescript-eslint/naming-convention */
/**
 * The limits the platform enforces or its pages ask for, the same for every church, which every
 * client and Rock read from the platform rather than holding their own.
 */
export type PlatformLimits = {
    /** The most people a conversation holds besides the one starting it. */
    dm_max_others: number;

    /** The longest name a group conversation carries. */
    dm_name_max_length: number;
    body_max_length: number;
    history_page_size: number;
    member_page_size: number;
    mention_page_size: number;

    /** A page of the threads list. */
    thread_page_size: number;
    directory_page_size: number;
    people_search_size: number;

    /** The largest file a message carries, in bytes. */
    attachment_max_bytes: number;

    /** The most files one message carries. */
    attachment_max_files: number;

    /** How long, in seconds, a person may do nothing before online status reads her Away. */
    presence_idle_seconds: number;
};

/** The file types a message may carry, bare, as the platform lists them; empty takes no files. */
export type AttachmentTypes = {
    attachment_types: string[];
};

/** The settings the token exchange answers, every key filled. Names are the platform's own. */
export type ChatSettings = {
    service: { state: string, banner: string | null };
    client: { update_required: boolean };
    /** The send's route, and the database's region, which calls that can be pinned to it are; null for none. */
    routes: { send: string, region: string | null };
    limits: {
        reconnect_base_ms: number, reconnect_cap_ms: number, catch_up_page: number, catch_up_max: number, problem_reports_per_load: number,
        tally_reports_per_load: number, gap_reorder_ms: number, gap_repair_wait_ms: number, stale_reload_ms: number,
        typing_interval_ms: number, typing_expiry_ms: number, typing_first_after_ms: number, typing_min_show_ms: number
    } & PlatformLimits & AttachmentTypes;
    /**
     * Switches by name; gifs is whether the church may search for and send GIFs, typing whether
     * its conversations show who is typing, and online whether people's online status is shared,
     * each off unless true.
     */
    flags: Record<string, unknown> & { gifs: boolean, typing: boolean, online: boolean };
    push: { web: Record<string, unknown> | null, vapid_key: string | null, prompt: string };
};
/* eslint-enable @typescript-eslint/naming-convention */

/** The service states this client knows; any other is read as normal. */
const serviceStates = ["normal", "degraded", "read_only", "maintenance"];

/** The routes this client can take; any other is read as direct, the route every release has. */
const sendRoutes = ["direct", "edge"];

/** An AWS region's name, as Supabase's x-region header takes it and the exchange checks it, such as us-east-1. */
const regionPattern = /^[a-z]{2}(-[a-z]+)+-\d$/;

/**
 * When the "Turn on notifications" button shows; any other value is read as after a send, the
 * moment a person has shown they mean to use chat.
 */
const pushPrompts = ["off", "offer", "after_send"];

/**
 * The defaults when the platform sends nothing for a key. The reconnect and catch-up figures are
 * estimates, not measurements; the platform can change them without a release.
 */
export const defaultLimits: Omit<ChatSettings["limits"], keyof PlatformLimits | keyof AttachmentTypes> = {
    reconnect_base_ms: 1000,
    reconnect_cap_ms: 30000,
    catch_up_page: 100,
    catch_up_max: 500,
    // an estimate: enough to name what went wrong on a load without a page that keeps failing flooding the log
    problem_reports_per_load: 10,
    // An estimate: how many tallies (counts of something gone wrong, a gap in what was heard live)
    // a load may send, one for each kind and code counted each time the page is hidden or left.
    // A cap of its own, so a load hidden often while it misses changes never spends the one-off
    // reports' cap.
    tally_reports_per_load: 10,
    // A missed live change: how long a hole in a room's revisions may stay open before it is a gap
    // (Telegram's figure), the longest random wait before its repair (the rejoin's wait), and how
    // long a room may go with nothing confirmed before a gap reloads it rather than repairs it.
    gap_reorder_ms: 500,
    gap_repair_wait_ms: 5000,
    stale_reload_ms: 900000,
    // A platform without typing's pace sends no typing flag either; these only keep a missing key
    // from becoming a signal on every key.
    typing_interval_ms: 5000,
    typing_expiry_ms: 7500,
    // A message sent within the first wait never shows typing; a typist drawn stays at least the
    // minimum, so a word typed and deleted never blinks.
    typing_first_after_ms: 700,
    typing_min_show_ms: 1000
};

/** The platform's limits by name, read from every exchange's answer. */
const platformLimitKeys: Array<keyof PlatformLimits> = [
    "dm_max_others",
    "dm_name_max_length",
    "body_max_length",
    "history_page_size",
    "member_page_size",
    "mention_page_size",
    "thread_page_size",
    "directory_page_size",
    "people_search_size",
    "attachment_max_bytes",
    "attachment_max_files",
    "presence_idle_seconds"
];

/** An object's own value for a key, or undefined when the value is not an object. */
function section(value: unknown, key: string): Record<string, unknown> {
    const outer = value && typeof value === "object" ? (value as Record<string, unknown>)[key] : undefined;

    return outer && typeof outer === "object" && !Array.isArray(outer) ? outer as Record<string, unknown> : {};
}

/**
 * Reads the settings the exchange answered. Every key may be missing or of a shape this client
 * does not know, since the platform keeps changing after a release; each falls back to its
 * default on its own, so one bad key never costs the rest.
 *
 * @param raw The settings as the exchange answered them.
 *
 * @returns The settings with every key filled.
 */
export function readSettings(raw: unknown): ChatSettings {
    const service = section(raw, "service");
    const client = section(raw, "client");
    const routes = section(raw, "routes");
    const limits = section(raw, "limits");
    const flags = section(raw, "flags");
    const push = section(raw, "push");
    const web = push.web && typeof push.web === "object" && !Array.isArray(push.web) ? push.web as Record<string, unknown> : null;
    // Firebase needs both, so one without the other is push off
    const hasPush = web !== null && typeof push.vapid_key === "string" && push.vapid_key !== "";

    const limit = (key: keyof typeof defaultLimits): number => {
        const value = limits[key];
        return typeof value === "number" && value > 0 ? value : defaultLimits[key];
    };

    // The platform answers its limits on every exchange and the client keeps no copy of its own,
    // so one missing reads as 0 and what needs it is refused rather than guessed at.
    const platformLimits = Object.fromEntries(platformLimitKeys.map(key => {
        const value = limits[key];
        return [key, typeof value === "number" && Number.isInteger(value) && value > 0 ? value : 0];
    })) as PlatformLimits;

    return {
        service: {
            state: typeof service.state === "string" && serviceStates.includes(service.state) ? service.state : "normal",
            banner: typeof service.banner === "string" ? service.banner : null
        },
        client: { update_required: client.update_required === true },
        routes: {
            send: typeof routes.send === "string" && sendRoutes.includes(routes.send) ? routes.send : "direct",
            region: typeof routes.region === "string" && regionPattern.test(routes.region) ? routes.region : null
        },
        limits: {
            reconnect_base_ms: limit("reconnect_base_ms"),
            reconnect_cap_ms: limit("reconnect_cap_ms"),
            // history serves 1 to 100 messages a page and refuses anything else, so a setting
            // outside that would fail every catch-up
            catch_up_page: Math.min(100, Math.max(1, Math.round(limit("catch_up_page")))),
            catch_up_max: limit("catch_up_max"),
            problem_reports_per_load: limit("problem_reports_per_load"),
            tally_reports_per_load: limit("tally_reports_per_load"),
            gap_reorder_ms: limit("gap_reorder_ms"),
            gap_repair_wait_ms: limit("gap_repair_wait_ms"),
            stale_reload_ms: limit("stale_reload_ms"),
            typing_interval_ms: limit("typing_interval_ms"),
            typing_expiry_ms: limit("typing_expiry_ms"),
            typing_first_after_ms: limit("typing_first_after_ms"),
            typing_min_show_ms: limit("typing_min_show_ms"),
            ...platformLimits,
            // a list of anything but strings is no list, so files are refused rather than guessed at
            attachment_types: Array.isArray(limits.attachment_types) && limits.attachment_types.every(type => typeof type === "string")
                ? [...limits.attachment_types as string[]]
                : []
        },
        // only the JSON true turns a switch on, so a value this client cannot read leaves it off
        flags: { ...flags, gifs: flags.gifs === true, typing: flags.typing === true, online: flags.online === true },
        push: {
            web: hasPush ? web : null,
            vapid_key: hasPush ? push.vapid_key as string : null,
            prompt: typeof push.prompt === "string" && pushPrompts.includes(push.prompt) ? push.prompt : "after_send"
        }
    };
}

/**
 * The banner the page shows, or null for none. The platform's own sentence wins; this client's
 * sentence is only for a state the platform sent without one.
 *
 * @param settings The settings.
 *
 * @returns The sentence, or null.
 */
export function serviceBanner(settings: ChatSettings): string | null {
    if (settings.client.update_required) {
        return "Chat needs a newer version of Rock. Ask your church's Rock administrator to update.";
    }

    if (settings.service.state === "normal") {
        return null;
    }

    if (settings.service.banner) {
        return settings.service.banner;
    }

    switch (settings.service.state) {
        case "read_only":
            return readOnlySentence;
        case "maintenance":
            return maintenanceSentence;
        default:
            return "Chat is running slowly right now.";
    }
}

/** What the Rock token action answers. */
export type ChurchTokenResult = {
    gate: string;
    churchToken: string | null;

    /**
     * True when Rock could not be asked at all, so the gate is unknown rather than refused. A
     * session that still holds a token keeps it and asks again.
     */
    isUnreachable?: boolean;
};

/**
 * Why a token is being asked for: the start's tries, the timer's refresh, a recheck of what the
 * person may still read, a call refused as expired, the try after a wait, or online status asking
 * before the timer would. The exchange's body follows from it.
 */
export type RefreshReason = "start" | "scheduled" | "recheck" | "expired" | "retry" | "early";

/** What the platform's token exchange answers. */
export type ExchangeResult =
    | { ok: true, accessToken: string, expiresInSeconds: number, settings?: unknown }
    | { ok: false, status: number, code: string, retryAfterSeconds?: number | null };

/** Everything the session reaches outside itself, so a test can stand in for each. */
export type SessionDependencies = {
    /** Asks Rock for a church token. Every ask re-runs the gates. */
    mintChurchToken: () => Promise<ChurchTokenResult>;

    /**
     * Exchanges a church token for a platform token; the signal gives up a try that took too long,
     * and the reason is why the token was asked for.
     */
    exchange: (churchToken: string, options: { signal: AbortSignal, reason: RefreshReason }) => Promise<ExchangeResult>;

    /**
     * Hands the current token to the live connection that is already open. Called after every
     * refresh; the connection reads the token back through the client's own token callback.
     */
    pushTokenToConnection: () => Promise<void>;

    /** A number from 0 up to 1, where to refresh within the permitted window. */
    random: () => number;

    /** Starts a timer. */
    setTimer: (callback: () => void, milliseconds: number) => unknown;

    /** Stops a timer. */
    clearTimer: (handle: unknown) => void;

    /**
     * Starts the deadline of one try at a token, which lives only as long as the try and is
     * stopped by clearTimer; setTimer when not given.
     */
    setDeadline?: (callback: () => void, milliseconds: number) => unknown;

    /**
     * Whether the person is away from the page now: it is hidden, or the browser is offline. Only
     * a wait that began while they were away is brought forward when they come back; never away
     * when not given.
     */
    isAway?: () => boolean;

    /** The time now in milliseconds; the browser's clock unless a test stands in. */
    now?: () => number;

    /**
     * Called when a session that held a token no longer does: Rock refused a refresh, or the
     * token ran out before one succeeded. Whatever the token opened should be closed.
     */
    onEnded?: () => void;

    /**
     * Called when a try of the start failed in a way that may pass, with the time its next try is
     * due, so the page can say it is still connecting.
     */
    onWaiting?: (untilMs: number) => void;

    /**
     * Called once a refresh has settled, and once after each try of the start: after the next
     * refresh is scheduled and the refresh is no longer out, so a refresh asked from here starts an
     * exchange of its own rather than joining the one that just ended.
     */
    settled?: (reason: RefreshReason, ok: boolean) => void;
};

/** Why the session is not running, when it is not. */
export type SessionState = {
    gate: string | null;
    errorCode: string | null;
};

/** The session a shell holds. */
export type ChatSession = {
    /**
     * Mints, exchanges and schedules the first refresh, asking again on the session's own wait for
     * as long as Rock or the platform cannot answer. Resolves true with a token, and false only when
     * a gate or the platform refused, or the session was stopped.
     */
    start: () => Promise<boolean>;

    /** The current platform token, for the client's token callback and a save on close. */
    currentToken: () => string | null;

    /**
     * Mints and exchanges again and hands the new token to the open connection. A refresh asked
     * while one is out joins it, and keeps its reason.
     */
    refresh: (reason: RefreshReason) => Promise<boolean>;

    /** When the next refresh is due, by the session's clock; null while starting, waiting after a failure, or stopped. */
    nextRefreshAt: () => number | null;

    /** How long, in seconds, the latest token the exchange answered lives; 0 before the first. */
    tokenLifeSeconds: () => number;

    /** Why the token being asked for now is asked, from a refresh's mint to its settle and through the whole start; null when none is out. */
    inFlight: () => RefreshReason | null;

    /**
     * Runs a platform call, and if the platform refuses the token as expired, refreshes once
     * and runs it again.
     */
    withFreshToken: <T>(call: () => Promise<T>, isExpired: (result: T) => boolean) => Promise<T>;

    /**
     * The network came back or the page was shown: a wait after a failed try that began while the
     * person was away, and that the platform no longer holds the session to, ends soon, once per
     * wait. A wait that began while they watched is left as computed, so flicking between tabs
     * never shortens the backoff. Nothing else is moved.
     */
    nudge: () => void;

    /** Stops the session for good: no refresh runs, is scheduled or is handed over after this. */
    stop: () => void;

    /** The settings the latest exchange answered. */
    settings: () => ChatSettings;

    /**
     * A refusal for read only or maintenance means the service state changed since the last
     * exchange, and its sentence is the banner, so the page shows it without waiting for the
     * next refresh.
     */
    applyServiceRefusal: (error: ChatError) => void;

    /** Why the session is not running. */
    state: SessionState;
};

/** The earliest point in a token's life at which it is refreshed. */
export const refreshWindowStart = 0.5;

/** The latest point in a token's life at which it is refreshed. */
export const refreshWindowEnd = 0.9;

/**
 * The longest one try at a token may take, Rock's mint and the platform's exchange together,
 * before it counts as no answer. An estimate: a socket a laptop's sleep or a change of network
 * left dead can hold a request for minutes, and while one is out nothing else is asked.
 */
export const tryTimeoutMs = 10000;

/**
 * How long a church token Rock signed is exchanged again by a later try rather than minted anew.
 * An estimate: Rock signs one for five minutes and the exchange grants from it until two are
 * left, so it is good there for about three; this keeps half a minute in hand for Rock's clock
 * running ahead of the platform's, and for a mint's answer up to a try's deadline late, since its
 * age is counted from when the answer arrived. It spares Rock a mint on every try of a long outage.
 */
export const churchTokenReuseMs = 150000;

/** What a try that ran out of time, or was given up by a stop, answered. */
const noAnswer = Symbol("no answer");

/** One try at a token: its signal, and what settles once it is given up. */
type TokenTry = {
    signal: AbortSignal;
    abandoned: Promise<typeof noAnswer>;
    abandon: () => void;
    timer: unknown;
};


/**
 * Creates the session.
 *
 * @param dependencies What the session reaches outside itself.
 *
 * @returns The session.
 */
export function createSession(dependencies: SessionDependencies): ChatSession {
    const state: SessionState = { gate: null, errorCode: null };
    let token: string | null = null;
    let timer: unknown = null;
    let expiresAt = 0;
    let lifeSeconds = 0;
    // When the scheduled refresh is due, while one is scheduled, and why the refresh out now was
    // asked, from its mint to its settle.
    let refreshAt: number | null = null;
    let refreshReason: RefreshReason | null = null;
    // Read by components (a flag in it decides what the message box offers), so it is a ref: a
    // refresh that changes a flag must reach a reader made before it.
    const settings = shallowRef(readSettings(undefined));

    // Failed tries in a row, which sets how long the next wait is, and how long the platform asked
    // to be left alone after the last one, when it said.
    let failures = 0;
    let retryAfterSeconds: number | null = null;
    // The wait after a failed try, while one is pending: when its try is due, before when the
    // platform asked not to be asked, what runs when it ends, whether it began while the person
    // was away, and whether a nudge has moved it. Nothing asks before it is due but its own
    // timer; a platform that answers 503 while it struggles is just when every call refused as
    // expired would otherwise start an exchange.
    let waiting: { due: number, floor: number, run: () => void, isAway: boolean, isNudged: boolean } | null = null;
    // The try out now, which a stop gives up, and the church token a failed try left, which the
    // next exchanges again while it is young instead of asking Rock for another.
    let current: TokenTry | null = null;
    let held: { churchToken: string, mintedAt: number } | null = null;
    let refreshing: Promise<boolean> | null = null;
    // The start is the one asker until it settles, so a call made with no token yet waits for it
    // rather than starting a sign-in of its own.
    let isStarting = false;
    let isStopped = false;
    const now = dependencies.now ?? ((): number => Date.now());

    /** Starts a try, which is given up as no answer when it runs past its time or the session stops. */
    function beginTry(): TokenTry {
        const controller = new AbortController();
        let abandon = (): void => undefined;
        const abandoned = new Promise<typeof noAnswer>(resolve => {
            abandon = (): void => {
                controller.abort();
                resolve(noAnswer);
            };
        });
        const timer = (dependencies.setDeadline ?? dependencies.setTimer)(() => abandon(), tryTimeoutMs);

        return { signal: controller.signal, abandoned, abandon, timer };
    }

    /**
     * Mints (or takes the church token a failed try left while it is young) and exchanges,
     * reminting at once when the platform calls the church token stale. A gate that refused, a
     * platform that refused the church token, or a stale answer twice in a row is final: asking
     * again cannot mend Rock's clock or key. A Rock or a platform that could not answer this time,
     * or a try that ran past its time, is not, and says nothing about the token already held.
     */
    async function acquire(reason: RefreshReason): Promise<"ok" | "refused" | "unavailable"> {
        // A Retry-After answers the one exchange that carried it, so each try starts without one:
        // a try that fails at Rock's mint waits the backoff alone.
        retryAfterSeconds = null;
        const attempt = current = beginTry();
        // An answer that lands after the try was given up is never read: the race has settled.
        const within = <T>(work: Promise<T>): Promise<T | typeof noAnswer> => Promise.race([work, attempt.abandoned]);

        try {
            let isStaleOnce = false;
            for (;;) {
                if (held === null || now() - held.mintedAt >= churchTokenReuseMs) {
                    held = null;
                    const minted = await within(dependencies.mintChurchToken());

                    // Stopped while Rock answered: nothing more is asked, and nothing it said is kept.
                    if (isStopped || minted === noAnswer) {
                        return "unavailable";
                    }

                    if (minted.isUnreachable) {
                        // Only a session with nothing yet shows it; one that holds a token carries on.
                        if (token === null) {
                            state.gate = minted.gate;
                        }
                        return "unavailable";
                    }

                    state.gate = minted.gate;
                    if (minted.gate !== "ok" || !minted.churchToken) {
                        return "refused";
                    }
                    held = { churchToken: minted.churchToken, mintedAt: now() };
                }

                const exchanged = await within(dependencies.exchange(held.churchToken, { signal: attempt.signal, reason }));
                if (isStopped || exchanged === noAnswer) {
                    return "unavailable";
                }
                if (exchanged.ok) {
                    // Every scheduled refresh mints, so it passes Rock's gates again.
                    held = null;
                    token = exchanged.accessToken;
                    expiresAt = now() + exchanged.expiresInSeconds * 1000;
                    lifeSeconds = exchanged.expiresInSeconds;
                    settings.value = readSettings(exchanged.settings);
                    state.errorCode = null;
                    failures = 0;
                    schedule(exchanged.expiresInSeconds);
                    return "ok";
                }

                state.errorCode = exchanged.code;
                retryAfterSeconds = exchanged.retryAfterSeconds ?? null;

                // A stale church token only means Rock signed it too long ago, so a fresh one is
                // worth one more try at once; a second in a row means Rock signs them wrongly.
                if (exchanged.code === "auth.stale_token") {
                    held = null;
                    if (isStaleOnce) {
                        return "refused";
                    }
                    isStaleOnce = true;
                    continue;
                }

                // No answer, a gateway that gave up, too many requests, or a server fault may pass;
                // any other refusal is the platform saying no to this church token, and asking
                // again would only repeat it.
                const status = exchanged.status;
                if (status === 0 || status === 408 || status === 429 || status >= 500) {
                    return "unavailable";
                }
                held = null;
                return "refused";
            }
        }
        finally {
            dependencies.clearTimer(attempt.timer);
            current = null;
        }
    }

    /**
     * Mints and exchanges, and settles what the session holds by the outcome: a refusal ends the
     * session; a failure that may pass keeps a token that is still alive and asks again soon,
     * since the token has at least a tenth of its life left when a refresh runs. Once the token
     * has expired there is nothing left to keep.
     */
    async function renew(reason: RefreshReason): Promise<boolean> {
        const outcome = await acquire(reason);

        // A stopped session has already been closed by whoever stopped it; it is not ended again.
        if (isStopped) {
            return false;
        }

        if (outcome === "ok") {
            return true;
        }

        if (outcome === "refused" || token === null || now() >= expiresAt) {
            const wasHeld = token !== null;
            token = null;
            held = null;
            clear();
            if (wasHeld) {
                dependencies.onEnded?.();
            }
            return false;
        }

        waitThen(() => void refresh("retry"));
        return false;
    }

    /**
     * Signs in, and while Rock or the platform cannot answer, waits and asks again, for as long as
     * it takes: a short outage of the platform is not a reason chat never opens. Each failed try
     * is told, so the page can say it is still connecting.
     */
    async function start(): Promise<boolean> {
        isStarting = true;
        try {
            for (;;) {
                const outcome = await acquire("start");
                if (isStopped) {
                    return false;
                }
                if (outcome !== "unavailable") {
                    // A refusal is final until the page loads again: the session ends here, so no
                    // later wake, restore or call can sign her in behind a page that says chat is
                    // refused.
                    if (outcome === "refused") {
                        isStopped = true;
                    }
                    // no longer starting before it is told, so what it asks is a refresh of its own
                    isStarting = false;
                    dependencies.settled?.("start", outcome === "ok");
                    return outcome === "ok";
                }
                dependencies.settled?.("start", false);

                // A stop lets the wait go at once, and the loop finds the session stopped.
                await new Promise<void>(resolve => {
                    waitThen(resolve);
                    dependencies.onWaiting?.(waiting?.due ?? now());
                });
                if (isStopped) {
                    return false;
                }
            }
        }
        finally {
            isStarting = false;
        }
    }

    /**
     * Schedules the next refresh at a random point in the permitted window of the token's
     * life, so that everyone who opened chat at the same moment does not refresh together.
     */
    function schedule(expiresInSeconds: number): void {
        clear();
        if (isStopped) {
            return;
        }
        const fraction = refreshWindowStart + (refreshWindowEnd - refreshWindowStart) * dependencies.random();
        const wait = expiresInSeconds * 1000 * fraction;
        refreshAt = now() + wait;
        timer = dependencies.setTimer(() => void refresh("scheduled"), wait);
    }

    /** The settings' backoff for a number of failures in a row, at a random point of its range. */
    function backoff(attempt: number): number {
        const { reconnect_base_ms: baseMs, reconnect_cap_ms: capMs } = settings.value.limits;
        return computeBackoff(attempt, baseMs, capMs, dependencies.random);
    }

    /**
     * Waits after a failed try, then runs what comes next: the longer of when the platform said to
     * come back and a random wait that grows with each failure in a row, so the clients of every
     * church that failed together do not all ask again in the same second, and a short Retry-After
     * cannot keep the wait from growing. The cap bounds a proxy's huge value. A start and a
     * refresh wait the same way; before a start the limits are the defaults, as nothing else is
     * known yet.
     */
    function waitThen(run: () => void): void {
        clear();
        const capMs = settings.value.limits.reconnect_cap_ms;
        const floorMs = Math.min(capMs, (retryAfterSeconds ?? 0) * 1000);
        const wait = Math.min(capMs, Math.max(floorMs, backoff(failures)));
        failures++;
        waiting = { due: now() + wait, floor: now() + floorMs, run, isAway: dependencies.isAway?.() ?? false, isNudged: false };
        timer = dependencies.setTimer(endWait, wait);
    }

    /** The wait is over: its try runs. */
    function endWait(): void {
        const run = waiting?.run;
        waiting = null;
        run?.();
    }

    /** Stops the pending refresh or wait, if there is one. */
    function clear(): void {
        if (timer !== null) {
            dependencies.clearTimer(timer);
            timer = null;
        }
        refreshAt = null;
        waiting = null;
    }

    /**
     * Mints and exchanges again and hands the new token to the open connection. A refresh asked
     * for while one is running shares it, so the timer and an expired call never race each other
     * to decide what the session holds.
     */
    function refresh(reason: RefreshReason): Promise<boolean> {
        // A stopped session asks Rock for nothing more, whatever still calls it: an expired call
        // in flight, or a timer that fired late. Nothing is asked while the start is still
        // signing in, or inside the wait after a failure, whoever asks; the start and the wait's
        // own timer ask.
        if (isStopped || isStarting || (waiting !== null && now() < waiting.due)) {
            return Promise.resolve(false);
        }

        if (refreshing === null) {
            refreshReason = reason;
            refreshing = (async (): Promise<boolean> => {
                let isOk = false;
                try {
                    isOk = await renew(reason) && !isStopped;
                    if (isOk) {
                        await dependencies.pushTokenToConnection();
                    }
                    return isOk;
                }
                finally {
                    // No longer out before it is told, so a refresh asked from there is a new one.
                    refreshing = null;
                    refreshReason = null;
                    dependencies.settled?.(reason, isOk);
                }
            })();
        }

        return refreshing;
    }

    return {
        start,
        currentToken: () => token,
        refresh,
        nextRefreshAt: () => isStopped || isStarting || waiting !== null ? null : refreshAt,
        tokenLifeSeconds: () => lifeSeconds,
        inFlight: () => isStarting ? "start" : refreshReason,
        withFreshToken: async <T>(call: () => Promise<T>, isExpired: (result: T) => boolean): Promise<T> => {
            const result = await call();
            if (!isExpired(result) || !await refresh("expired")) {
                return result;
            }

            return call();
        },
        nudge: (): void => {
            // A try out now, a wait already moved, or one that began while the person watched is
            // left as it is: coming back is the reason, so a person flicking between tabs while
            // every try fails in front of them never turns the backoff into a try a second.
            if (isStopped || waiting === null || !waiting.isAway || waiting.isNudged) {
                return;
            }
            waiting.isNudged = true;

            // Past the platform's own floor the next try comes within a second, at a random point
            // so a church's devices coming back together do not all ask in one moment; inside it,
            // the floor is kept. A nudge never moves a try later.
            const at = now() >= waiting.floor ? now() + backoff(0) : waiting.floor;
            if (at < waiting.due) {
                waiting.due = at;
                if (timer !== null) {
                    dependencies.clearTimer(timer);
                }
                timer = dependencies.setTimer(endWait, at - now());
            }
        },
        stop: (): void => {
            isStopped = true;
            // Whatever awaits the try or the wait is let go now, and finds the session stopped.
            current?.abandon();
            const run = waiting?.run;
            clear();
            run?.();
        },
        settings: () => settings.value,
        applyServiceRefusal: (error: ChatError): void => {
            if (error.code === "rpc.read_only" || error.code === "rpc.maintenance") {
                settings.value = { ...settings.value, service: { state: error.code.slice("rpc.".length), banner: error.text ?? null } };
            }
        },
        state
    };
}
