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
// Sending. A message shows at once as the person's own pending row and becomes a timeline
// message when the platform confirms it with its id. A failed send keeps its text on the row so
// the person can send it again; nothing typed is lost to a failure. The platform does not
// deduplicate a retried send, so a retry after a lost confirmation can post twice, and the
// person deletes the copy; the live echo of a confirmed send is matched by its id.
import { ChatError, PendingMessage } from "../types.partial";
import { Timelines } from "./useHistory.partial";

/** What the platform's send answers. */
export type SendResult =
    | { ok: true, id: number, createdAt: string }
    | { ok: false, error: ChatError };

/** What the sender reaches outside itself. */
export type SenderDependencies = {
    /** Sends a text message to a channel. */
    send: (channelId: string, body: string) => Promise<SendResult>;

    /** The timelines a confirmed message is put into. */
    timelines: Pick<Timelines, "upsert">;

    /** The person sending, as the platform knows them. */
    personAliasGuid: string;

    /** A fresh identifier for a pending row. */
    newLocalId: () => string;
};

/** The sender a shell holds. */
export type Sender = {
    /** The person's pending and failed rows in a channel, oldest first. */
    pending: (channelId: string) => PendingMessage[];

    /** Sends a message. Resolves true when the platform confirmed it. Blank text sends nothing. */
    send: (channelId: string, body: string) => Promise<boolean>;

    /** Sends a failed row's text again. */
    retry: (localId: string) => Promise<boolean>;

    /** Drops a failed row. */
    discard: (localId: string) => void;
};

/**
 * Creates the sender.
 *
 * @param dependencies What the sender reaches outside itself.
 *
 * @returns The sender.
 */
export function createSender(_dependencies: SenderDependencies): Sender {
    throw new Error("not implemented");
}
