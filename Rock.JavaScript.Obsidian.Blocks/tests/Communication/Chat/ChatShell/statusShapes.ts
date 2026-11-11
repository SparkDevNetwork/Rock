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
// for the reader (offline, hidden, blocked, unrelated) is answered with the decision time alone.
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
 * The token exchange's answer once its status read ran in time: a top-level presence map by
 * primary alias, beside the token and settings, never inside them. Left out when the read did not
 * answer in time, which is no failure.
 */
export function exchangeAnswer(presence?: Record<string, Record<string, unknown>>): Record<string, unknown> {
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
