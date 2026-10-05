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
// The live connection. The client library authorises only the first private join on a socket
// when it is handed a token directly, so the token is loaded with setAuth() and no argument, read
// back through the client's own token callback, before the first join; handing it a token string
// is the form that loses every join after the first.
import {
    channelTopic,
    createRealtimeHub,
    personalTopic,
    RealtimeChannelLike,
    RealtimeClientLike,
    realtimeOptions
} from "../../../../../src/Communication/Chat/ChatShell/composables/useRealtimeHub.partial";

const tenant = "10000000-0000-4000-8000-00000000000A";
const alias = "A0000001-0000-4000-8000-00000000000B";
const channelOne = "C0000001-0000-4000-8000-00000000000C";
const channelTwo = "c0000002-0000-4000-8000-00000000000d";

type FakeChannel = RealtimeChannelLike & {
    topic: string;
    isPrivate: boolean;
    handler: ((message: { event: string, payload: unknown }) => void) | null;
    // Realtime hands a system listener the message itself: its status, its words, the topic.
    system: ((message: { event: string, payload: unknown }) => void) | null;
    status: ((status: string, error?: unknown) => void) | null;
};

function fakeClient(): { client: RealtimeClientLike, log: string[], channels: FakeChannel[], removed: string[] } {
    const log: string[] = [];
    const channels: FakeChannel[] = [];
    const removed: string[] = [];

    const client: RealtimeClientLike = {
        realtime: {
            setAuth: async (token?: string | null): Promise<void> => {
                log.push(token === undefined ? "setAuth()" : `setAuth(${token})`);
            }
        },
        channel: (topic, options) => {
            log.push(`channel(${topic})`);
            const channel: FakeChannel = {
                topic,
                isPrivate: options.config.private,
                handler: null,
                system: null,
                status: null,
                on: (type, _filter, callback) => {
                    // Realtime's own messages about the channel arrive on their own listener.
                    if ((type as string) === "system") {
                        channel.system = callback;
                    }
                    else {
                        channel.handler = callback;
                    }
                    return channel;
                },
                subscribe: (callback) => {
                    channel.status = callback;
                    return channel;
                }
            };
            channels.push(channel);
            return channel;
        },
        removeChannel: async (channel) => {
            removed.push((channel as FakeChannel).topic);
            return "ok";
        }
    };

    return { client, log, channels, removed };
}

function hubWith(client: RealtimeClientLike): { hub: ReturnType<typeof createRealtimeHub>, seen: string[] } {
    const seen: string[] = [];
    const hub = createRealtimeHub({
        client,
        tenantId: tenant,
        personAliasGuid: alias,
        onChannelEvent: (channelId, event, payload) => seen.push(`channel ${channelId} ${event} ${JSON.stringify(payload)}`),
        onPersonalEvent: (event, payload) => seen.push(`personal ${event} ${JSON.stringify(payload)}`),
        onJoined: channelId => seen.push(`joined ${channelId}`),
        onStatus: error => seen.push(`status ${error ? error.code : "healthy"}`)
    });
    return { hub, seen };
}

describe("topic names", () => {
    test("are lower case, whatever case the identifiers arrive in", () => {
        expect(channelTopic(tenant, channelOne)).toBe("t:10000000-0000-4000-8000-00000000000a:c:c0000001-0000-4000-8000-00000000000c");
        expect(personalTopic(tenant, alias)).toBe("t:10000000-0000-4000-8000-00000000000a:p:a0000001-0000-4000-8000-00000000000b");
    });
});

describe("createRealtimeHub", () => {
    test("loads the token with setAuth() and no argument before the first join", async () => {
        const f = fakeClient();
        const { hub } = hubWith(f.client);

        await hub.start();

        expect(f.log[0]).toBe("setAuth()");
        expect(f.log[1]).toBe(`channel(${personalTopic(tenant, alias)})`);
        expect(f.channels[0].isPrivate).toBe(true);
    });

    test("an open channel joins its own private topic beside the personal one", async () => {
        const f = fakeClient();
        const { hub } = hubWith(f.client);
        await hub.start();

        hub.openChannel(channelOne);

        expect(f.channels.map(c => c.topic)).toEqual([personalTopic(tenant, alias), channelTopic(tenant, channelOne)]);
        expect(f.channels[1].isPrivate).toBe(true);
        expect(f.log.filter(l => l.startsWith("setAuth"))).toEqual(["setAuth()"]);
    });

    test("opening another channel leaves the first channel's topic and keeps the personal one", async () => {
        const f = fakeClient();
        const { hub } = hubWith(f.client);
        await hub.start();

        hub.openChannel(channelOne);
        hub.openChannel(channelTwo);

        expect(f.removed).toEqual([channelTopic(tenant, channelOne)]);
    });

    test("every confirmed join of the open channel is reported, a rejoin after a drop included", async () => {
        const f = fakeClient();
        const { hub, seen } = hubWith(f.client);
        await hub.start();
        hub.openChannel(channelOne);
        const topic = f.channels[1];

        topic.status?.("SUBSCRIBED");
        topic.status?.("CHANNEL_ERROR");
        topic.status?.("SUBSCRIBED");

        expect(seen).toEqual([
            `joined ${channelOne}`,
            "status rt.channel_error",
            "status healthy",
            `joined ${channelOne}`
        ]);
    });

    test("events are routed by topic, with their name and payload", async () => {
        const f = fakeClient();
        const { hub, seen } = hubWith(f.client);
        await hub.start();
        hub.openChannel(channelOne);

        f.channels[0].handler?.({ event: "channel.unread", payload: { channel_id: "x", message_id: 5 } });
        f.channels[1].handler?.({ event: "message.created", payload: { id: 7 } });

        expect(seen).toEqual([
            "personal channel.unread {\"channel_id\":\"x\",\"message_id\":5}",
            `channel ${channelOne} message.created {"id":7}`
        ]);
    });

    test("an event from a channel that has been left is dropped", async () => {
        const f = fakeClient();
        const { hub, seen } = hubWith(f.client);
        await hub.start();
        hub.openChannel(channelOne);
        const left = f.channels[1];
        hub.openChannel(channelTwo);

        left.handler?.({ event: "message.created", payload: { id: 8 } });
        left.status?.("SUBSCRIBED");

        expect(seen).toEqual([]);
    });

    test("stop leaves every topic", async () => {
        const f = fakeClient();
        const { hub } = hubWith(f.client);
        await hub.start();
        hub.openChannel(channelOne);

        await hub.stop();

        expect(f.removed.sort()).toEqual([channelTopic(tenant, channelOne), personalTopic(tenant, alias)].sort());
    });

    test("an error Realtime sends about the open channel is reported by its code", async () => {
        const f = fakeClient();
        const { hub, seen } = hubWith(f.client);
        await hub.start();
        hub.openChannel(channelOne);
        f.channels[1].status?.("SUBSCRIBED");

        f.channels[1].system?.({ status: "error", message: "Token has expired 1 seconds ago" } as never);

        expect(seen).toEqual([`joined ${channelOne}`, "status rt.token_expired"]);
    });

    test("a join refused with Realtime's words is reported by their code", async () => {
        const f = fakeClient();
        const { hub, seen } = hubWith(f.client);
        await hub.start();
        hub.openChannel(channelOne);

        f.channels[1].status?.("CHANNEL_ERROR", new Error("Unauthorized: You do not have permissions to read from this Channel topic: x"));

        expect(seen).toEqual(["status rt.not_readable"]);
    });

    test("a channel whose read was revoked is left and not joined again, and the personal topic stays", async () => {
        const f = fakeClient();
        const { hub, seen } = hubWith(f.client);
        await hub.start();
        hub.openChannel(channelOne);
        const open = f.channels[1];
        open.status?.("SUBSCRIBED");

        open.system?.({ status: "error", message: "You do not have permissions to read from this Channel topic: x" } as never);
        open.status?.("CLOSED");
        open.status?.("SUBSCRIBED");
        open.handler?.({ event: "message.created", payload: { id: 9 } });

        expect(seen).toEqual([`joined ${channelOne}`, "status rt.read_revoked"]);
        expect(f.removed).toEqual([channelTopic(tenant, channelOne)]);
    });

    test("the refusal words on the personal topic are a refused read, never a revoked channel, and the topic is kept", async () => {
        const f = fakeClient();
        const { hub, seen } = hubWith(f.client);
        await hub.start();
        hub.openChannel(channelOne);

        f.channels[0].system?.({ status: "error", message: "You do not have permissions to read from this Channel topic: x" } as never);

        expect(seen).toEqual(["status rt.not_readable"]);
        expect(f.removed).toEqual([]);
    });

    test("a hub stopped while it loads the token subscribes to nothing", async () => {
        const f = fakeClient();
        let loaded!: () => void;
        f.client.realtime.setAuth = () => new Promise<void>(resolve => loaded = resolve);
        const { hub } = hubWith(f.client);

        const starting = hub.start();
        await hub.stop();
        loaded();
        await starting;

        expect(f.channels).toEqual([]);
    });

    test("a stopped hub joins no channel", async () => {
        const f = fakeClient();
        const { hub } = hubWith(f.client);
        await hub.start();
        await hub.stop();

        hub.openChannel(channelOne);

        expect(f.channels.map(c => c.topic)).toEqual([personalTopic(tenant, alias)]);
    });
});

describe("the realtime client's options", () => {
    // The protocol is pinned so a library update cannot change it under a released client, and
    // reconnects are random and grow so every church does not come back in the same second.

    test("pins the protocol version", () => {
        expect(realtimeOptions({ reconnect_base_ms: 1000, reconnect_cap_ms: 30000 }, () => 0.5).vsn).toBe("2.0.0");
    });

    test("reconnects after a random, growing wait from the settings' base, never past their cap", () => {
        const options = realtimeOptions({ reconnect_base_ms: 1000, reconnect_cap_ms: 30000 }, () => 0);

        expect(options.reconnectAfterMs(1)).toBe(500);
        expect(options.reconnectAfterMs(2)).toBe(1000);
        expect(options.reconnectAfterMs(3)).toBe(2000);
        expect(options.reconnectAfterMs(40)).toBe(15000);
        expect(realtimeOptions({ reconnect_base_ms: 1000, reconnect_cap_ms: 30000 }, () => 0.999).reconnectAfterMs(40)).toBeLessThanOrEqual(30000);
        expect(realtimeOptions({ reconnect_base_ms: 4000, reconnect_cap_ms: 30000 }, () => 0).reconnectAfterMs(1)).toBe(2000);
    });
});

describe("a Realtime error the client cannot classify", () => {
    // Realtime can reword its errors in any upgrade. Words the client does not know are treated
    // as possibly a lost read, so the shell is told to check access again.
    function hubWithUnknown(client: RealtimeClientLike): { hub: ReturnType<typeof createRealtimeHub>, unknown: number[] } {
        const unknown: number[] = [];
        const dependencies = {
            client,
            tenantId: tenant,
            personAliasGuid: alias,
            onChannelEvent: () => undefined,
            onPersonalEvent: () => undefined,
            onJoined: () => undefined,
            onStatus: () => undefined,
            onUnknownError: () => unknown.push(1)
        };
        return { hub: createRealtimeHub(dependencies), unknown };
    }

    test("words it does not know about the open channel ask the shell to check access again", async () => {
        const f = fakeClient();
        const { hub, unknown } = hubWithUnknown(f.client);
        await hub.start();
        hub.openChannel(channelOne);
        f.channels[1].status?.("SUBSCRIBED");

        f.channels[1].system?.({ status: "error", message: "PolicyChanged: you may not read this any more" } as never);

        expect(unknown).toHaveLength(1);
    });

    test("a join refused with words it does not know asks the same", async () => {
        const f = fakeClient();
        const { hub, unknown } = hubWithUnknown(f.client);
        await hub.start();
        hub.openChannel(channelOne);

        f.channels[1].status?.("CHANNEL_ERROR", new Error("Unauthorized: a sentence never seen before"));

        expect(unknown).toHaveLength(1);
    });

    test("words it knows do not", async () => {
        const f = fakeClient();
        const { hub, unknown } = hubWithUnknown(f.client);
        await hub.start();
        hub.openChannel(channelOne);
        f.channels[1].status?.("SUBSCRIBED");

        f.channels[1].system?.({ status: "error", message: "Token has expired 1 seconds ago" } as never);

        expect(unknown).toHaveLength(0);
    });
});
