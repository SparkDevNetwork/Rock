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
    RealtimeClientLike
} from "../../../../../src/Communication/Chat/ChatShell/composables/useRealtimeHub.partial";

const tenant = "10000000-0000-4000-8000-00000000000A";
const alias = "A0000001-0000-4000-8000-00000000000B";
const channelOne = "C0000001-0000-4000-8000-00000000000C";
const channelTwo = "c0000002-0000-4000-8000-00000000000d";

type FakeChannel = RealtimeChannelLike & {
    topic: string;
    isPrivate: boolean;
    handler: ((message: { event: string, payload: unknown }) => void) | null;
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
                status: null,
                on: (_type, _filter, callback) => {
                    channel.handler = callback;
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
});
