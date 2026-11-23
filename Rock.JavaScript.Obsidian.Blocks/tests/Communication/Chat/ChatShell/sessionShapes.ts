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
// A session context for a spec to mount a part under, as the shell provides one: a part reads the
// phone flag and the signed-in person from it rather than from its own inputs.
import { computed, ref } from "vue";
import { ChatMe, chatSessionKey, SessionContext } from "../../../../src/Communication/Chat/ChatShell/chatSession.partial";
import { BackStack, noBackStack } from "../../../../src/Communication/Chat/ChatShell/composables/useBackStack.partial";

/** What a spec sets of the session; the rest reads as a wide window with no one signed in. */
export type SessionShape = {
    isPhone?: boolean;
    me?: Partial<ChatMe>;
    churchName?: string;
    backStack?: BackStack;
};

/**
 * The session as a spec's mount provides it: `mount(Part, { global: { provide: providedSession({ isPhone: true }) } })`.
 *
 * @param shape What the spec sets.
 *
 * @returns The provide map.
 */
export function providedSession(shape: SessionShape = {}): Record<symbol, SessionContext> {
    const me: ChatMe = { personAliasGuid: "", name: "", photoUrl: null, ...shape.me };
    return {
        [chatSessionKey as symbol]: {
            isPhone: ref(shape.isPhone ?? false),
            me: computed(() => me),
            church: { name: shape.churchName ?? "" },
            emojiSource: null,
            mentionSource: () => null,
            backStack: shape.backStack ?? noBackStack
        }
    };
}

/** A back stack that records what is held, for a part's cells; Back closes the newest held layer. */
export type RecordedBackStack = {
    stack: BackStack;

    /** Every layer held now, oldest first. */
    held: Array<{ back: () => void }>;

    /** The system Back: asks the newest held layer's owner to close it. */
    back: () => void;
};

/**
 * A back stack that records holds instead of writing history.
 *
 * @returns The stack and what it holds.
 */
export function recordingBackStack(): RecordedBackStack {
    const held: Array<{ back: () => void }> = [];
    const stack: BackStack = {
        screens: () => undefined,
        hold: back => {
            const layer = { back };
            held.push(layer);
            return () => {
                const at = held.indexOf(layer);
                if (at >= 0) {
                    held.splice(at, 1);
                }
            };
        }
    };
    return { stack, held, back: () => held[held.length - 1]?.back() };
}
