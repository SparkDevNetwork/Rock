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

/** What a spec sets of the session; the rest reads as a wide window with no one signed in. */
export type SessionShape = {
    isPhone?: boolean;
    me?: Partial<ChatMe>;
    churchName?: string;
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
            mentionSource: () => null
        }
    };
}
