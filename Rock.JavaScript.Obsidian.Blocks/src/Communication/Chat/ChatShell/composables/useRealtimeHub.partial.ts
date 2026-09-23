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
// The live connection. A person holds two private topics on one socket: their personal topic
// for the whole session, which carries short signals and never a message body, and the topic
// of the channel they have open, which carries that channel's messages.
import { classifyRealtimeStatus } from "../errors.partial";
import { ChatError } from "../types.partial";

/** The part of a realtime channel the hub uses. */
export type RealtimeChannelLike = {
    on: (type: "broadcast", filter: { event: string }, callback: (message: { event: string, payload: unknown }) => void) => RealtimeChannelLike;
    subscribe: (callback: (status: string, error?: unknown) => void) => RealtimeChannelLike;
};

/** The part of the platform client the hub uses. */
export type RealtimeClientLike = {
    realtime: { setAuth: (token?: string | null) => Promise<void> };
    channel: (topic: string, options: { config: { private: boolean } }) => RealtimeChannelLike;
    removeChannel: (channel: RealtimeChannelLike) => Promise<unknown>;
};

/** What the hub tells the shell. */
export type RealtimeHubDependencies = {
    client: RealtimeClientLike;
    tenantId: string;
    personAliasGuid: string;

    /** An event on the open channel's topic. */
    onChannelEvent: (channelId: string, event: string, payload: unknown) => void;

    /** An event on the person's own topic. */
    onPersonalEvent: (event: string, payload: unknown) => void;

    /** The open channel's topic has been joined, the first time or again after a drop. */
    onJoined: (channelId: string) => void;

    /** The live connection's health changed; null when it is healthy again. */
    onStatus: (error: ChatError | null) => void;
};

/** The live connection a shell holds. */
export type RealtimeHub = {
    /** Loads the token onto the socket, then joins the person's topic. */
    start: () => Promise<void>;

    /** Leaves the topic of the channel that was open, if any, and joins this one's. */
    openChannel: (channelId: string) => void;

    /** Leaves every topic. */
    stop: () => Promise<void>;
};

/**
 * The topic of a channel. The platform accepts lower-case identifiers only.
 *
 * @param tenantId The church.
 * @param channelId The channel.
 *
 * @returns The topic name.
 */
export function channelTopic(tenantId: string, channelId: string): string {
    return `t:${tenantId.toLowerCase()}:c:${channelId.toLowerCase()}`;
}

/**
 * The topic of a person. The platform accepts lower-case identifiers only.
 *
 * @param tenantId The church.
 * @param personAliasGuid The person's primary alias.
 *
 * @returns The topic name.
 */
export function personalTopic(tenantId: string, personAliasGuid: string): string {
    return `t:${tenantId.toLowerCase()}:p:${personAliasGuid.toLowerCase()}`;
}

/**
 * Creates the hub.
 *
 * @param dependencies The client and the shell's handlers.
 *
 * @returns The hub.
 */
export function createRealtimeHub(dependencies: RealtimeHubDependencies): RealtimeHub {
    const { client } = dependencies;
    let personal: RealtimeChannelLike | null = null;
    let open: { channelId: string, channel: RealtimeChannelLike } | null = null;
    let isDegraded = false;

    /** Reports the connection's health, only when it changes. */
    function report(status: string): void {
        if (status === "SUBSCRIBED") {
            if (isDegraded) {
                isDegraded = false;
                dependencies.onStatus(null);
            }
            return;
        }

        isDegraded = true;
        dependencies.onStatus(classifyRealtimeStatus(status));
    }

    return {
        start: async (): Promise<void> => {
            // With no argument, setAuth reads the token back through the client's own token
            // callback, which is the form that authorises every private join on the socket and
            // not only the first.
            await client.realtime.setAuth();

            personal = client.channel(personalTopic(dependencies.tenantId, dependencies.personAliasGuid), { config: { private: true } })
                .on("broadcast", { event: "*" }, message => dependencies.onPersonalEvent(message.event, message.payload))
                .subscribe(status => report(status));
        },

        openChannel: (channelId: string): void => {
            if (open) {
                void client.removeChannel(open.channel);
            }

            // A channel that has been left can still deliver what was already in flight; only
            // the one open now is listened to. It is made the open one before it subscribes, so
            // a status that arrives at once is not taken for a stale channel's.
            const channel = client.channel(channelTopic(dependencies.tenantId, channelId), { config: { private: true } })
                .on("broadcast", { event: "*" }, message => {
                    if (open?.channel === channel) {
                        dependencies.onChannelEvent(channelId, message.event, message.payload);
                    }
                });

            open = { channelId, channel };

            channel.subscribe(status => {
                if (open?.channel !== channel) {
                    return;
                }

                report(status);
                if (status === "SUBSCRIBED") {
                    dependencies.onJoined(channelId);
                }
            });
        },

        stop: async (): Promise<void> => {
            const channels = [open?.channel, personal].filter((c): c is RealtimeChannelLike => !!c);
            open = null;
            personal = null;
            await Promise.all(channels.map(c => client.removeChannel(c)));
        }
    };
}
