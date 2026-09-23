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
// What happens between sign-in and the first message on screen. The open channel's history, its
// live join and the sidebar all start at once: history needs nothing from the sidebar, so the
// time to the first message is sign-in plus one history call. The channel that opens is the one
// a link names, else the last one this browser opened for this person, else, only on a first
// visit, the sidebar's first row once it arrives.
import { SidebarRow } from "./types.partial";

/** The part of browser storage the shell uses. */
export type StorageLike = {
    getItem: (key: string) => string | null;
    setItem: (key: string, value: string) => void;
};

/**
 * The storage key for the last channel a person opened in a church, so that one person on a
 * shared computer never opens on another person's channel.
 *
 * @param tenantId The church.
 * @param personAliasGuid The person.
 *
 * @returns The key.
 */
export function rememberedChannelKey(_tenantId: string, _personAliasGuid: string): string {
    throw new Error("not implemented");
}

/**
 * Reads the remembered channel. Storage that is blocked or missing only loses the memory.
 *
 * @param storage The browser storage, or null when there is none.
 * @param key The key.
 *
 * @returns The channel, or null.
 */
export function readRememberedChannel(_storage: StorageLike | null, _key: string): string | null {
    throw new Error("not implemented");
}

/**
 * Remembers the channel. Storage that is blocked or missing only loses the memory.
 *
 * @param storage The browser storage, or null when there is none.
 * @param key The key.
 * @param channelId The channel.
 */
export function writeRememberedChannel(_storage: StorageLike | null, _key: string, _channelId: string): void {
    throw new Error("not implemented");
}

/** How opening a channel went. */
export type OpenOutcome = "opened" | "refused" | "failed";

/** What opening a channel reaches. */
export type ChannelOpenerDependencies = {
    /** Joins the channel's live topic. Returns at once; the join confirms later. */
    join: (channelId: string) => void;

    /** Fetches the channel's first page. Rejects with the platform's refusal when it refuses. */
    loadNewest: (channelId: string) => Promise<void>;

    /** Whether a failure is the platform refusing the channel to this person. */
    isRefusal: (error: unknown) => boolean;

    /** Remembers the channel once its history has loaded. */
    remember: (channelId: string) => void;
};

/**
 * Opens a channel: its live join and its history start together, neither waiting on the other.
 *
 * @param dependencies What opening a channel reaches.
 * @param channelId The channel.
 *
 * @returns How it went.
 */
export function openChannel(_dependencies: ChannelOpenerDependencies, _channelId: string): Promise<OpenOutcome> {
    throw new Error("not implemented");
}

/** What the page load reaches. */
export type PageLoadDependencies = {
    /** The channel a link named, or null. */
    linkedChannelId: string | null;

    /** The channel this browser opened last for this person, or null. */
    rememberedChannelId: string | null;

    /** Fetches the sidebar. */
    loadSidebar: () => Promise<SidebarRow[]>;

    /** Opens a channel. */
    open: (channelId: string) => Promise<OpenOutcome>;
};

/**
 * Runs the page load. The sidebar and the first channel start together when the channel is
 * known; otherwise the sidebar names it.
 *
 * @param dependencies What the page load reaches.
 *
 * @returns The channel that opened, or null when none could.
 */
export function startPageLoad(_dependencies: PageLoadDependencies): Promise<string | null> {
    throw new Error("not implemented");
}
