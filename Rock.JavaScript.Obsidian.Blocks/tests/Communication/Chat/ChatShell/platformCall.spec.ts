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
// Every platform call goes one way: a POST the route decides. A released chat client cannot be
// changed for years, so the send can be moved behind the platform's own gate by a setting, and
// the client already knows how to reach it. A failure on the gate is reported, never retried on
// the direct route: the server decides the route, and a client that chose for itself would undo
// the reason for moving it.
import {
    computeBackoff,
    createPlatformCall,
    parseRetryAfter,
    PlatformCallOptions
} from "../../../../src/Communication/Chat/ChatShell/platformCall.partial";

const url = "http://platform";

type Sent = { url: string, init: RequestInit };

function response(status: number, body: unknown, headers: Record<string, string> = {}): Response {
    return {
        ok: status >= 200 && status < 300,
        status,
        headers: { get: (name: string) => headers[name.toLowerCase()] ?? null },
        json: async () => body
    } as unknown as Response;
}

function setup(answers: Array<Response | Error>, route: "direct" | "edge" = "direct"): { call: ReturnType<typeof createPlatformCall>, sent: Sent[] } {
    const sent: Sent[] = [];
    const options: PlatformCallOptions = {
        fetch: async (to, init) => {
            sent.push({ url: to, init });
            const answer = answers.shift();
            if (answer instanceof Error) {
                throw answer;
            }
            return answer as Response;
        },
        projectUrl: url,
        publishableKey: "k",
        currentToken: () => "platform-token",
        routes: () => ({ send: route })
    };

    return { call: createPlatformCall(options), sent };
}

function headers(sent: Sent): Record<string, string> {
    return sent.init.headers as Record<string, string>;
}

describe("createPlatformCall", () => {
    test("a call on the direct route posts the arguments to the database function with the token", async () => {
        const { call, sent } = setup([response(200, { messages: [], has_more: false })]);

        const result = await call.call("chat_get_history", { p_channel_id: "c1", p_limit: 50 });

        expect(result).toEqual({ ok: true, data: { messages: [], has_more: false } });
        expect(sent).toHaveLength(1);
        expect(sent[0].url).toBe(`${url}/rest/v1/rpc/chat_get_history`);
        expect(sent[0].init.method).toBe("POST");
        expect(headers(sent[0])["Authorization"]).toBe("Bearer platform-token");
        expect(headers(sent[0])["apikey"]).toBe("k");
        expect(JSON.parse(sent[0].init.body as string)).toEqual({ p_channel_id: "c1", p_limit: 50 });
    });

    test("the send goes to the gate when the settings route it there, with the same arguments", async () => {
        const { call, sent } = setup([response(200, { id: 7, created_at: "2026-10-05T10:00:00Z", notice: null })], "edge");

        const result = await call.call("chat_send_message", { p_channel_id: "c1", p_body: "hi", p_client_key: "k1" });

        expect(sent[0].url).toBe(`${url}/functions/v1/chat-send`);
        expect(headers(sent[0])["Authorization"]).toBe("Bearer platform-token");
        expect(JSON.parse(sent[0].init.body as string)).toEqual({ p_channel_id: "c1", p_body: "hi", p_client_key: "k1" });
        expect(result).toEqual({ ok: true, data: { id: 7, created_at: "2026-10-05T10:00:00Z", notice: null } });
    });

    test("the send goes direct when the settings say direct", async () => {
        const { call, sent } = setup([response(200, { id: 7 })], "direct");

        await call.call("chat_send_message", { p_channel_id: "c1", p_body: "hi" });

        expect(sent[0].url).toBe(`${url}/rest/v1/rpc/chat_send_message`);
    });

    test("only the send has a second route; every other call goes direct whatever the send's route", async () => {
        const { call, sent } = setup([response(200, { channels: [] })], "edge");

        await call.call("chat_get_bootstrap", {});

        expect(sent[0].url).toBe(`${url}/rest/v1/rpc/chat_get_bootstrap`);
    });

    test("the read position is saved through the same path, with keepalive so it survives the page closing", async () => {
        const { call, sent } = setup([response(200, { read_cursor: 4, last_message_id: 4 })]);

        const result = await call.call("chat_mark_read", { p_channel_id: "c1", p_message_id: 4 }, { keepalive: true });

        expect(sent[0].url).toBe(`${url}/rest/v1/rpc/chat_mark_read`);
        expect(sent[0].init.keepalive).toBe(true);
        expect(result).toEqual({ ok: true, data: { read_cursor: 4, last_message_id: 4 } });
    });

    test("a gate that fails is reported and never retried on the direct route", async () => {
        const { call, sent } = setup([response(503, { error: { code: "rpc.unavailable", message: "Chat is busy. Try again in a moment.", request_id: "r1" } })], "edge");

        const result = await call.call("chat_send_message", { p_channel_id: "c1", p_body: "hi" });

        expect(sent).toHaveLength(1);
        expect(result).toMatchObject({ ok: false, status: 503, error: { code: "rpc.unavailable", text: "Chat is busy. Try again in a moment." } });
    });

    test("a gate that cannot be reached is a transport failure, not a reason to go direct", async () => {
        const { call, sent } = setup([new TypeError("Failed to fetch")], "edge");

        const result = await call.call("chat_send_message", { p_channel_id: "c1", p_body: "hi" });

        expect(sent).toHaveLength(1);
        expect(result).toMatchObject({ ok: false, status: 0, error: { code: "rpc.transport" } });
    });

    test("a database refusal keeps our code and the server's sentence for it", async () => {
        const { call } = setup([response(409, { code: "PT409", message: "rpc.idempotency_mismatch", details: null, hint: "That message was already sent with different text." })]);

        const result = await call.call("chat_send_message", { p_channel_id: "c1", p_body: "hi" });

        expect(result).toMatchObject({
            ok: false,
            status: 409,
            error: { code: "rpc.idempotency_mismatch", text: "That message was already sent with different text." }
        });
    });

    test("a refusal that says when to come back is honoured in seconds", async () => {
        const { call } = setup([response(429, { code: "PT429", message: "rpc.rate_limited", hint: "Slow down." }, { "retry-after": "7" })]);

        const result = await call.call("chat_send_message", { p_channel_id: "c1", p_body: "hi" });

        expect(result).toMatchObject({ ok: false, status: 429, retryAfterSeconds: 7 });
    });

    test("an expired platform token is a session failure the caller refreshes for", async () => {
        const { call } = setup([response(401, { code: "PGRST303", message: "JWT expired" })]);

        const result = await call.call("chat_get_bootstrap", {});

        expect(result).toMatchObject({ ok: false, status: 401, error: { code: "auth.expired", severity: "session" } });
    });
});

describe("parseRetryAfter", () => {
    const now = Date.parse("2026-10-05T10:00:00Z");

    test("a number of seconds is read as seconds", () => {
        expect(parseRetryAfter("12", now)).toBe(12);
    });

    test("an HTTP date is read as the seconds until it", () => {
        expect(parseRetryAfter("Mon, 05 Oct 2026 10:00:30 GMT", now)).toBe(30);
    });

    test("nothing, nonsense, or a time already past says nothing", () => {
        expect(parseRetryAfter(null, now)).toBeNull();
        expect(parseRetryAfter("soon", now)).toBeNull();
        expect(parseRetryAfter("Mon, 05 Oct 2026 09:00:00 GMT", now)).toBeNull();
    });
});

describe("computeBackoff", () => {
    // Every church reconnects at once after an outage, so each wait is random, and it grows so a
    // platform that is still down is asked less and less often.
    test("each wait is between half and all of a ceiling that doubles from the base", () => {
        expect(computeBackoff(0, 1000, 30000, () => 0)).toBe(500);
        expect(computeBackoff(0, 1000, 30000, () => 0.5)).toBe(750);
        expect(computeBackoff(1, 1000, 30000, () => 0.5)).toBe(1500);
        expect(computeBackoff(2, 1000, 30000, () => 0.5)).toBe(3000);
        expect(computeBackoff(3, 1000, 30000, () => 0)).toBe(4000);
    });

    test("the wait never passes the cap however many tries have failed", () => {
        expect(computeBackoff(5, 1000, 30000, () => 0.999)).toBeLessThanOrEqual(30000);
        expect(computeBackoff(40, 1000, 30000, () => 0.999)).toBeLessThanOrEqual(30000);
        expect(computeBackoff(40, 1000, 30000, () => 0)).toBe(15000);
    });

    test("two clients with different random draws wait different times", () => {
        expect(computeBackoff(3, 1000, 30000, () => 0.1)).not.toBe(computeBackoff(3, 1000, 30000, () => 0.9));
    });

    test("base and cap are the caller's, so the server can change them", () => {
        expect(computeBackoff(0, 2000, 60000, () => 0)).toBe(1000);
        expect(computeBackoff(10, 2000, 60000, () => 0)).toBe(30000);
    });
});
