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
// Online status in every shape the platform sends it, with the platform's own keys and formats, so
// a status spec tests against what really arrives rather than against what the client wishes did.
// A status is decided at an instant, written as epoch microseconds in a JSON integer, so two
// decisions inside one millisecond still order; a time it holds to is a timestamptz as Postgres
// writes one into JSON, to the microsecond with its offset. A person the rule says nothing about
// for the reader (offline, hidden, blocked, unrelated) is answered with the decision time alone,
// except by the token exchange, which sends one decision time for its whole answer and leaves
// such a person out.
// Her automatic state rides only on her own entry, and only while she is Busy.

/** The server's clock in these fixtures: 3:00 PM UTC on the day. */
export const serverNow = Date.parse("2026-11-10T15:00:00.000Z");

/** The same instant as the token's iat writes it, whole seconds. */
export const serverNowSeconds = serverNow / 1000;

/**
 * A decision time as the platform writes it: epoch microseconds, a JSON integer of 16 digits,
 * which a double holds exactly.
 *
 * @param msAfter Milliseconds after the fixtures' server now; negative for before.
 * @param micros Microseconds more, inside that millisecond.
 */
export function asOf(msAfter = 0, micros = 0): number {
    return (serverNow + msAfter) * 1000 + micros;
}

/**
 * A server time as Postgres writes a timestamptz into JSON: six digits of fraction and +00:00, not
 * the Z and three digits a browser writes.
 *
 * @param msAfter Milliseconds after the fixtures' server now.
 * @param micros The microseconds written past the millisecond, which a browser's clock drops.
 */
export function pgTime(msAfter: number, micros = 123): string {
    const iso = new Date(serverNow + msAfter).toISOString();
    return `${iso.slice(0, 23)}${String(micros).padStart(3, "0")}+00:00`;
}

/** How long a row counts after a heartbeat: the token's 300 s and the 270 s slack. */
export const rowLifeMs = 570_000;

/** How long the store keeps a decision that draws nothing, past any older decision's row: twenty minutes. */
export const heldNothingMs = 1_200_000;

/** Online, decided msAfter, holding to its row's end. */
export function online(msAfter = 0, untilAfter = rowLifeMs, micros = 0): Record<string, unknown> {
    return { state: "online", until: pgTime(untilAfter), as_of: asOf(msAfter, micros) };
}

/** Away: a live row with no input inside the idle line. */
export function away(msAfter = 0, untilAfter = rowLifeMs, micros = 0): Record<string, unknown> {
    return { state: "away", until: pgTime(untilAfter), as_of: asOf(msAfter, micros) };
}

/** Busy with an end, or with none when busyUntilAfter is null: the key is then left out, as the platform does for no end. */
export function busy(busyUntilAfter: number | null, msAfter = 0, untilAfter = rowLifeMs): Record<string, unknown> {
    return {
        state: "busy",
        until: pgTime(untilAfter),
        ...(busyUntilAfter !== null ? { busy_until: pgTime(busyUntilAfter) } : {}),
        as_of: asOf(msAfter)
    };
}

/** The viewer's own entry while Busy: her automatic state underneath rides with it, and only here. */
export function selfBusy(busyUntilAfter: number, auto: "online" | "away", msAfter = 0): Record<string, unknown> {
    return { ...busy(busyUntilAfter, msAfter), auto };
}

/** What the rule answers for someone it says nothing about to this reader: the decision time alone. */
export function nothing(msAfter = 0, micros = 0): Record<string, unknown> {
    return { as_of: asOf(msAfter, micros) };
}

/** One person as chat_get_channel, chat_list_members, chat_get_person and chat_search_people answer them, covering: status is always there. */
export function readerPerson(alias: string, status: Record<string, unknown>, nick = "Anna", last = "Reyes"): Record<string, unknown> {
    return { person_alias_guid: alias, nick_name: nick, last_name: last, avatar_url: null, details: null, status };
}

/** A person from a reader that carries no status at all, such as a channel's leaders. */
export function personWithoutStatus(alias: string): Record<string, unknown> {
    return { person_alias_guid: alias, nick_name: "Lee", last_name: "Park", avatar_url: null };
}

/**
 * A watched person's entry in the token exchange's answer: her status without a decision time,
 * since the answer carries one time for every entry.
 *
 * @param status The status as a reader would carry it, its decision time dropped.
 */
export function toldEntry(status: Record<string, unknown>): Record<string, unknown> {
    const { as_of: _, ...entry } = status;
    return entry;
}

/**
 * The token exchange's presence: one decision time for the whole answer, and told naming only the
 * watched people it tells something of; a watched person it leaves out is told nothing as of then.
 *
 * @param msAfter Milliseconds after the fixtures' server now that the answer was decided.
 * @param told The entries, each without a time of its own.
 * @param micros Microseconds more, inside that millisecond.
 */
export function watchAnswer(msAfter = 0, told: Record<string, Record<string, unknown>> = {}, micros = 0): Record<string, unknown> {
    return { as_of: asOf(msAfter, micros), told };
}

/**
 * The token exchange's answer once its status read ran in time: a top-level presence, beside the
 * token and settings, never inside them. Left out when the read did not answer in time, which is
 * no failure.
 */
export function exchangeAnswer(presence?: Record<string, unknown>): Record<string, unknown> {
    return {
        access_token: platformToken({ iat: serverNowSeconds, exp: serverNowSeconds + 300, sub: "a0000007-0000-4000-8000-000000000000", online: true }),
        expires_in: 300,
        settings: { flags: { online: true, typing: true }, limits: { presence_idle_seconds: 600 } },
        ...(presence ? { presence } : {})
    };
}

/**
 * A typing row as the typing call writes it: the status sits at the top level beside the typist's
 * alias (the token's alias), on starts and stops alike, only where the call proved its readers.
 */
export function typingRow(channelId: string, alias: string, status?: Record<string, unknown>, isStop = false): Record<string, unknown> {
    return {
        channel_id: channelId,
        parent_id: null,
        person_alias_guid: alias,
        after: 10,
        interval_ms: 5000,
        ...(isStop ? { stopped: true } : {}),
        id: "4f9c1a2e-0000-4000-8000-000000000001",
        ...(status ? { status } : {})
    };
}

/**
 * A dm.preview row on a reader's personal topic: the author is the sender's primary alias, which
 * Postgres may hand over in whatever case it was stored in, and the status is at the top level.
 */
export function dmPreview(channelId: string, sender: string, status?: Record<string, unknown>): Record<string, unknown> {
    return {
        channel_id: channelId,
        message_id: 41,
        preview: "On my way",
        sent_at: pgTime(0),
        sender: { person_alias_guid: sender.toUpperCase(), nick_name: "Anna", last_name: "Reyes", avatar_url: null },
        is_own: false,
        posted_by_rock: false,
        preview_line: "On my way",
        id: "4f9c1a2e-0000-4000-8000-000000000002",
        ...(status ? { status } : {})
    };
}

/** The bootstrap's status map: the other person of each two-person row, the rule's decision-time-alone answers mixed in. */
export function bootstrapStatus(entries: Record<string, Record<string, unknown>>): Record<string, Record<string, unknown>> {
    return { ...entries };
}

/** The three shapes chat_set_status answers. */
export const setStatusAnswers = {
    /** A choice saved: Busy with its end. */
    busy: { status: "busy", busy_until: pgTime(3_600_000) },

    /** A choice equal to what was saved. */
    unchanged: { status: "auto", changed: false },

    /** A page's goodbye. */
    leave: { client: "leave" }
};

/** A platform token as the exchange mints it: three base64url parts, the claims in seconds. */
export function platformToken(claims: { iat: number, exp: number, sub: string, online?: boolean }): string {
    const part = (value: unknown): string => Buffer.from(JSON.stringify(value)).toString("base64url");
    return `${part({ alg: "HS256", typ: "JWT" })}.${part({ role: "authenticated", ...claims })}.c2lnbmF0dXJl`;
}

/**
 * One answer a status save can meet, as the wire carries it: the HTTP status, the body (null for
 * a page a proxy wrote, which is not JSON) and the headers; or a request that reached nothing,
 * because the network failed, the save's deadline ended it, or the deadline ended it while its
 * answer was being read.
 */
export type StatusSaveAnswer =
    | { status: number, body: Record<string, unknown> | null, headers?: Record<string, string> }
    | { unreached: "network" | "deadline" | "deadlineWhileReading" };

/** The service refusing a change while the church is read only, the banner, or the code's own sentence, in hint. */
function readOnly(hint: string | null): StatusSaveAnswer {
    return { status: 503, body: { code: "PT503", message: "rpc.read_only", details: "this church's chat is in read_only", hint } };
}

/**
 * Every answer a status save can meet that is not a landing, in the platform's, PostgREST's and a
 * proxy's own shapes. Every refusal of ours carries its code in message and a sentence in hint,
 * the code's own unless the church set a banner.
 */
export const statusSaveFailures = {
    network: { unreached: "network" },
    deadline: { unreached: "deadline" },
    deadlineWhileReading: { unreached: "deadlineWhileReading" },

    /** The gateway refusing a token that has expired, before our code runs. */
    expired: { status: 401, body: { code: "PGRST303", message: "JWT expired", details: null, hint: null } },

    /** A proxy that gave up waiting, about the path, not the choice. */
    proxyTimeout: { status: 408, body: null },

    serverError: { status: 500, body: { code: "57014", message: "canceling statement due to statement timeout", details: null, hint: null } },
    maintenance: { status: 503, body: { code: "PT503", message: "rpc.maintenance", details: "this church's chat is in maintenance", hint: "Chat is down for maintenance. Try again soon." } },
    readOnlyBanner: readOnly("Chat is read only tonight while Cedar Hill moves to its new server."),
    readOnlyServerSentence: readOnly("Chat is read only right now. You can read messages but not send them."),
    readOnlyNoHint: readOnly(null),

    /** The status rule's own pace, which names no Retry-After. */
    rateLimited: { status: 429, body: { code: "PT429", message: "rpc.rate_limited", details: "too many status changes in a minute", hint: "You're sending messages too quickly. Wait a moment and try again." } },

    /** A gateway's pace, with none of our codes and a Retry-After of its own. */
    gatewayRateLimited: { status: 429, body: { message: "API rate limit exceeded" }, headers: { "retry-after": "90" } },

    /** A Busy end that has already passed. */
    badRequest: { status: 400, body: { code: "PT400", message: "rpc.bad_request", details: "busy_until has already passed", hint: "Chat could not understand that request." } },

    notLivePerson: { status: 403, body: { code: "42501", message: "authz.not_live_person", details: "only a live person may set their status", hint: "Chat isn't available for your account." } },

    /** A deploy whose function PostgREST's schema cache has not loaded yet. */
    notInSchemaCache: {
        status: 404,
        body: {
            code: "PGRST202",
            message: "Could not find the function public.chat_set_status(p_change, p_client) in the schema cache",
            details: "Searched for the function public.chat_set_status with parameters p_change, p_client or with a single unnamed json/jsonb parameter, but no matches were found in the schema cache.",
            hint: null
        }
    },

    proxyForbidden: { status: 403, body: null },
    proxyTooLarge: { status: 413, body: null }
} satisfies Record<string, StatusSaveAnswer>;

/** One tab's Web Locks, as a browser hands them to each document, with the tab's closing. */
export type TabLocks = {
    request: (name: string, options: { signal?: AbortSignal }, callback: () => Promise<unknown>) => Promise<unknown>;

    /** The tab closes: the browser lets its locks go without settling their callbacks, and drops what it waits for. */
    close: () => void;
};

/** One tab's BroadcastChannel, as the browser hands one out. */
export type TabChannel = {
    postMessage: (message: unknown) => void;
    close: () => void;
    onmessage: ((event: { data: unknown }) => void) | null;
};

/**
 * The browser's Web Locks for every tab of one origin, as the spec defines them: a name is held by
 * one callback at a time and granted to its waiters in the order they asked, the next the moment
 * the holder's promise settles; a request whose signal aborts before its grant is rejected with an
 * AbortError; a tab that closes lets its locks go without its callbacks settling, as a document
 * that is gone runs nothing more.
 */
export function createLockFake(): { tab: () => TabLocks, holder: (name: string) => string | null } {
    type Waiter = { owner: number, callback: () => Promise<unknown>, resolve: (value: unknown) => void, reject: (error: unknown) => void };
    const held = new Map<string, Waiter>();
    const queues = new Map<string, Waiter[]>();
    let tabs = 0;

    function grant(name: string): void {
        if (held.has(name)) {
            return;
        }
        const next = queues.get(name)?.shift();
        if (!next) {
            return;
        }
        held.set(name, next);
        void Promise.resolve().then(next.callback).then(
            value => release(name, next, () => next.resolve(value)),
            error => release(name, next, () => next.reject(error))
        );
    }

    function release(name: string, waiter: Waiter, settle: () => void): void {
        if (held.get(name) !== waiter) {
            // its tab closed, which already let the lock go; nothing of it runs again
            return;
        }
        held.delete(name);
        settle();
        grant(name);
    }

    return {
        tab: (): TabLocks => {
            const owner = ++tabs;
            let isClosed = false;
            return {
                request: (name, options, callback) => new Promise((resolve, reject) => {
                    if (isClosed) {
                        return;
                    }
                    const waiter: Waiter = { owner, callback, resolve, reject };
                    const queue = queues.get(name) ?? [];
                    queues.set(name, queue);
                    queue.push(waiter);
                    options.signal?.addEventListener("abort", () => {
                        const at = queue.indexOf(waiter);
                        if (at >= 0) {
                            queue.splice(at, 1);
                            reject(new DOMException("The request was aborted.", "AbortError"));
                        }
                    });
                    grant(name);
                }),
                close: (): void => {
                    isClosed = true;
                    for (const [name, queue] of queues) {
                        queues.set(name, queue.filter(waiter => waiter.owner !== owner));
                    }
                    for (const [name, waiter] of [...held]) {
                        if (waiter.owner === owner) {
                            held.delete(name);
                            grant(name);
                        }
                    }
                }
            };
        },
        holder: name => held.has(name) ? `tab ${held.get(name)?.owner}` : null
    };
}

/**
 * The browser's BroadcastChannel for every tab of one origin: a post reaches every other open
 * channel of its name, never its own, on a later task and as a structured clone; posting on a
 * closed channel throws, as the browser's does. Every post is kept, for a spec to read.
 */
export function createChannelFake(): { open: (name: string) => TabChannel, posts: Array<{ name: string, data: unknown }>, openCount: () => number } {
    const open = new Set<{ name: string, channel: TabChannel }>();
    const posts: Array<{ name: string, data: unknown }> = [];
    return {
        open: (name: string): TabChannel => {
            let isClosed = false;
            const channel: TabChannel = {
                onmessage: null,
                postMessage: message => {
                    if (isClosed) {
                        throw new DOMException("The channel is closed.", "InvalidStateError");
                    }
                    const data = JSON.stringify(message);
                    posts.push({ name, data: JSON.parse(data) });
                    for (const other of open) {
                        if (other.name === name && other.channel !== channel) {
                            setTimeout(() => {
                                if (open.has(other)) {
                                    other.channel.onmessage?.({ data: JSON.parse(data) });
                                }
                            }, 0);
                        }
                    }
                },
                close: () => {
                    isClosed = true;
                    for (const entry of open) {
                        if (entry.channel === channel) {
                            open.delete(entry);
                        }
                    }
                }
            };
            open.add({ name, channel });
            return channel;
        },
        posts,
        openCount: () => open.size
    };
}
