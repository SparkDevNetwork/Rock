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
import { classifyRealtimeMessage, classifyRealtimeStatus } from "../errors.partial";
import { computeBackoff } from "../platformCall.partial";
import { ChatError } from "../types.partial";
import { PageEventTargets } from "./useMarkRead.partial";
import { ChatSettings } from "./useSession.partial";

/** The Realtime protocol this client speaks, pinned so a library update cannot change it. */
export const realtimeProtocolVersion = "2.0.0";

/**
 * The realtime client's options: the pinned protocol, and reconnect waits that are random and
 * grow under a cap, so every church does not reconnect in the same second after an outage.
 *
 * @param limits The reconnect base and cap from the settings.
 * @param random A number from 0 up to 1.
 *
 * @returns The options for the realtime client.
 */
export function realtimeOptions(limits: Pick<ChatSettings["limits"], "reconnect_base_ms" | "reconnect_cap_ms">, random: () => number): { vsn: string, reconnectAfterMs: (tries: number) => number } {
    return {
        vsn: realtimeProtocolVersion,

        // The library counts its first reconnect as try one.
        reconnectAfterMs: (tries: number): number => computeBackoff(Math.max(0, tries - 1), limits.reconnect_base_ms, limits.reconnect_cap_ms, random)
    };
}

/**
 * What a channel listener is handed: a broadcast's event and payload, or Realtime's own message
 * about the channel, its status and its words.
 */
export type RealtimeMessageLike = {
    event?: string;
    payload?: unknown;
    status?: string;
    message?: string;
};

/** The part of a realtime channel the hub uses. */
export type RealtimeChannelLike = {
    on: (type: "broadcast" | "system", filter: { event?: string }, callback: (message: RealtimeMessageLike) => void) => RealtimeChannelLike;
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

    /**
     * The open channel's topic has been joined, the first time or again; a rejoin is a join after
     * a drop, which may have missed messages.
     */
    onJoined: (channelId: string, isRejoin: boolean) => void;

    /** The person's topic was joined again after a drop; never called for its first join. */
    onPersonalRejoined?: () => void;

    /**
     * The live connection's health changed; null when it is healthy again. A read Realtime has
     * revoked from the open channel arrives here as rt.read_revoked, after the hub has left it.
     */
    onStatus: (error: ChatError | null) => void;

    /**
     * Realtime refused or closed a channel in words this client does not know. They may mean a
     * lost read the client was not built to recognise, so the shell checks access again.
     */
    onUnknownError?: () => void;

    /**
     * The page, whose offline and online events say the network went and came back. Chrome can
     * keep a socket open on a dead network until a heartbeat goes unanswered, half a minute or
     * more, so the browser's own word is the first a person can be told.
     */
    page?: Pick<PageEventTargets, "window">;
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
    let open: { channelId: string, channel: RealtimeChannelLike, isJoined: boolean, isDropped: boolean } | null = null;
    let isDegraded = false;

    // What the connection is still waiting on: each topic whose last word was a failure, and the
    // network while the browser says it is offline. The connection is well again only once all
    // have come back, since until then something sent meanwhile is still missed.
    const failing = new Set<RealtimeChannelLike | "network">();

    // Set by stop; a hub stopped while it waits on the token, or asked to open a channel after,
    // joins nothing.
    let isStopped = false;

    /** Something the connection needs failed; the person is told each time, in its words. */
    function fail(source: RealtimeChannelLike | "network", error: ChatError): void {
        failing.add(source);
        isDegraded = true;
        dependencies.onStatus(error);
    }

    /** Something came back; the connection is well again once nothing is still failing. */
    function recover(source: RealtimeChannelLike | "network"): void {
        failing.delete(source);
        if (isDegraded && failing.size === 0) {
            isDegraded = false;
            dependencies.onStatus(null);
        }
    }

    /**
     * Reports a topic's status. A refused join carries Realtime's words, which say more than the
     * status does.
     */
    function report(channel: RealtimeChannelLike, status: string, error?: unknown): void {
        if (status === "SUBSCRIBED") {
            recover(channel);
            return;
        }

        const words = error instanceof Error ? classifyRealtimeMessage(error.message, false) : null;
        if (words?.code === "rt.unknown") {
            dependencies.onUnknownError?.();
        }
        fail(channel, words && words.code !== "rt.unknown" ? words : classifyRealtimeStatus(status));
    }

    // The browser's word on the network, which a socket on a dead network may not give for a
    // while; going offline is a closed connection, as a dropped socket says.
    const onOffline = (): void => fail("network", classifyRealtimeStatus("CLOSED"));
    const onOnline = (): void => recover("network");

    /**
     * Handles Realtime's own error about a channel. A read revoked from the open channel means
     * the person may no longer read it at all, so the hub leaves it rather than let the client
     * library join it again; anything else is reported and the channel's status says the rest.
     * Only the open channel can be revoked: the same words on the personal topic are a refused
     * read, and that topic is kept, since the session's end is what closes it.
     */
    function onSystem(channel: RealtimeChannelLike, message: RealtimeMessageLike): void {
        const isOpen = open?.channel === channel;

        // A topic that has been left says nothing about the connection now.
        if (message.status !== "error" || (!isOpen && channel !== personal)) {
            return;
        }

        const error = classifyRealtimeMessage(message.message ?? "", isOpen && open!.isJoined);

        if (error.code === "rt.unknown") {
            dependencies.onUnknownError?.();
        }

        // A revoked channel is left, so it is nothing the connection waits on.
        if (isOpen && error.code === "rt.read_revoked") {
            open = null;
            failing.delete(channel);
            void client.removeChannel(channel);
            isDegraded = true;
            dependencies.onStatus(error);
            return;
        }

        fail(channel, error);
    }

    return {
        start: async (): Promise<void> => {
            // With no argument, setAuth reads the token back through the client's own token
            // callback, which is the form that authorises every private join on the socket and
            // not only the first.
            await client.realtime.setAuth();
            if (isStopped) {
                return;
            }

            dependencies.page?.window.addEventListener("offline", onOffline);
            dependencies.page?.window.addEventListener("online", onOnline);

            const channel = client.channel(personalTopic(dependencies.tenantId, dependencies.personAliasGuid), { config: { private: true } });
            // Signals sent while the topic was down are lost, so a join after a drop is told to the
            // shell, which reads the sidebar again. The first join is the page load's, which has
            // just read it, and a second confirmation of the same join is not a rejoin.
            let hasJoined = false;
            let isDropped = false;
            personal = channel
                .on("broadcast", { event: "*" }, message => dependencies.onPersonalEvent(message.event ?? "", message.payload))
                .on("system", {}, message => onSystem(channel, message))
                .subscribe((status, error) => {
                    report(channel, status, error);
                    if (status !== "SUBSCRIBED") {
                        isDropped = hasJoined;
                        return;
                    }

                    const isRejoin = isDropped && !isStopped;
                    hasJoined = true;
                    isDropped = false;
                    if (isRejoin) {
                        dependencies.onPersonalRejoined?.();
                    }
                });
        },

        openChannel: (channelId: string): void => {
            if (isStopped) {
                return;
            }

            // The open topic is let go before it is removed: the client library closes a topic the
            // moment it starts to leave, before the leave is acknowledged, and a close heard while
            // the topic still counted as open would be taken for a lost connection on every switch.
            const left = open;
            open = null;
            if (left) {
                // a topic let go is nothing the connection waits on
                failing.delete(left.channel);
                void client.removeChannel(left.channel);
            }

            // A channel that has been left can still deliver what was already in flight; only
            // the one open now is listened to. It is made the open one before it subscribes, so
            // a status that arrives at once is not taken for a stale channel's.
            const channel = client.channel(channelTopic(dependencies.tenantId, channelId), { config: { private: true } })
                .on("broadcast", { event: "*" }, message => {
                    if (open?.channel === channel) {
                        dependencies.onChannelEvent(channelId, message.event ?? "", message.payload);
                    }
                })
                .on("system", {}, message => onSystem(channel, message));

            open = { channelId, channel, isJoined: false, isDropped: false };

            channel.subscribe((status, error) => {
                if (open?.channel !== channel) {
                    return;
                }

                report(channel, status, error);
                // A join after a drop may have missed messages, which its catch-up reads; a second
                // confirmation of the same join has missed nothing.
                if (status !== "SUBSCRIBED") {
                    open.isDropped = open.isJoined;
                    return;
                }
                const isRejoin = open.isDropped;
                open.isJoined = true;
                open.isDropped = false;
                dependencies.onJoined(channelId, isRejoin);
            });
        },

        stop: async (): Promise<void> => {
            isStopped = true;
            dependencies.page?.window.removeEventListener("offline", onOffline);
            dependencies.page?.window.removeEventListener("online", onOnline);
            const channels = [open?.channel, personal].filter((c): c is RealtimeChannelLike => !!c);
            open = null;
            personal = null;
            await Promise.all(channels.map(c => client.removeChannel(c)));
        }
    };
}
