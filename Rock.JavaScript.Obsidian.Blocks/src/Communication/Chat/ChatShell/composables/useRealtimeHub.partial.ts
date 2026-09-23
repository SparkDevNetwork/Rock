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
export function channelTopic(_tenantId: string, _channelId: string): string {
    throw new Error("not implemented");
}

/**
 * The topic of a person. The platform accepts lower-case identifiers only.
 *
 * @param tenantId The church.
 * @param personAliasGuid The person's primary alias.
 *
 * @returns The topic name.
 */
export function personalTopic(_tenantId: string, _personAliasGuid: string): string {
    throw new Error("not implemented");
}

/**
 * Creates the hub.
 *
 * @param dependencies The client and the shell's handlers.
 *
 * @returns The hub.
 */
export function createRealtimeHub(_dependencies: RealtimeHubDependencies): RealtimeHub {
    throw new Error("not implemented");
}
