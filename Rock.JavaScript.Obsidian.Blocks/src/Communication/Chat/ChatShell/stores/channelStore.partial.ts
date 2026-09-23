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
// The sidebar. Only this store writes it: the rows arrive whole from the platform's sidebar
// call, and after that they change only through the person's own signals and their own saves.
//
// The person's topic carries a short signal the first time a channel gets something they have
// not read, and never again until they read it. A signal for a channel the sidebar does not
// hold, a hidden channel coming back or one they were just added to, reloads the sidebar, since
// the signal carries no name to draw a row with.
import { MarkReadResult, SidebarRow } from "../types.partial";

/** What the store reaches outside itself. */
export type ChannelStoreDependencies = {
    /** Fetches the sidebar again and hands it to setSidebar. */
    reloadSidebar: () => void;
};

/** The sidebar a shell holds. */
export type ChannelStore = {
    /** The rows in the platform's order. */
    rows: SidebarRow[];

    /** The channel on screen, or null. */
    activeChannelId: string | null;

    /** Replaces every row with a fresh sidebar. */
    setSidebar: (rows: SidebarRow[]) => void;

    /** Sets the channel on screen. */
    setActive: (channelId: string | null) => void;

    /** An event on the person's own topic. */
    applyPersonalEvent: (event: string, payload: unknown) => void;

    /** A save of the read position came back. */
    applyMarkRead: (channelId: string, result: MarkReadResult) => void;
};

/**
 * Creates the sidebar store.
 *
 * @param dependencies What the store reaches outside itself.
 *
 * @returns The store.
 */
export function createChannelStore(_dependencies: ChannelStoreDependencies): ChannelStore {
    throw new Error("not implemented");
}
