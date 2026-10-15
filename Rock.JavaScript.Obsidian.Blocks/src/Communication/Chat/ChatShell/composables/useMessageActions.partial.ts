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
// Acting on a message. An empty stand-in with the planned shape and no behaviour yet, so the
// tests written against it fail on what they assert.
import { reactive } from "vue";
import { CallResult } from "../platformCall.partial";
import { ChatError, TimelineMessage } from "../types.partial";
import { Timelines } from "./useHistory.partial";

/** An action the server lists on a message. */
export type MessageAction = {
    id: string;
    label: string;
    call: string;
};

/** A toast the actions ask the shell to show. */
export type ActionNotice = {
    code: string;
    text: string;
    icon: string;
};

/** What the actions reach outside themselves. */
export type MessageActionsDependencies = {
    call: (name: string, args: Record<string, unknown>) => Promise<CallResult>;
    timelines: Pick<Timelines, "state" | "applyEvent">;
    personAliasGuid: string;
    notify: (notice: ActionNotice) => void;
    writeText?: (text: string) => Promise<void>;
};

/** What the actions hold. */
export type MessageActionsState = {
    editing: { channelId: string, messageId: number, isSaving: boolean, error: ChatError | null } | null;
    confirmingDelete: { channelId: string, message: TimelineMessage } | null;
    quotes: Record<string, TimelineMessage>;
};

/** The actions a shell holds. */
export type MessageActions = {
    state: MessageActionsState;
    startEdit: (channelId: string, messageId: number) => void;
    startEditNewest: (channelId: string) => boolean;
    cancelEdit: () => void;
    saveEdit: (body: string) => Promise<boolean>;
    askDelete: (channelId: string, message: TimelineMessage) => void;
    cancelDelete: () => void;
    confirmDelete: () => Promise<boolean>;
    runAction: (channelId: string, messageId: number, action: MessageAction) => Promise<void>;
    copy: (message: TimelineMessage) => Promise<void>;
    startQuote: (key: string, message: TimelineMessage) => void;
    cancelQuote: (key: string) => void;
    switchConversation: (channelId: string | null) => void;
    stop: () => void;
};

/**
 * Creates the actions.
 *
 * @param _dependencies What the actions reach outside themselves.
 *
 * @returns The actions.
 */
export function createMessageActions(_dependencies: MessageActionsDependencies): MessageActions {
    const state = reactive<MessageActionsState>({ editing: null, confirmingDelete: null, quotes: {} }) as MessageActionsState;

    return {
        state,
        startEdit: () => undefined,
        startEditNewest: () => false,
        cancelEdit: () => undefined,
        saveEdit: async () => false,
        askDelete: () => undefined,
        cancelDelete: () => undefined,
        confirmDelete: async () => false,
        runAction: async () => undefined,
        copy: async () => undefined,
        startQuote: () => undefined,
        cancelQuote: () => undefined,
        switchConversation: () => undefined,
        stop: () => undefined
    };
}
