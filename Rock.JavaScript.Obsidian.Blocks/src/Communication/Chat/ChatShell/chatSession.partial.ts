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

// What the whole chat session shares, provided once by the shell, so no layer between the shell
// and a part carries these on its way down.

import { computed, inject, InjectionKey, provide, Ref, ref } from "vue";
import type { PickerSource } from "./components/emojiPicker.partial.obs";
import type { MentionPickerSource } from "./components/mentionPicker.partial.obs";
import { SidebarRow } from "./types.partial";

export type ChatMe = {
    personAliasGuid: string;
    name: string;
    photoUrl: string | null;
};

export type SessionContext = {
    isPhone: Readonly<Ref<boolean>>;

    /** Replaced when a birthdate answer lets a new session in. */
    me: Readonly<Ref<ChatMe>>;

    church: Readonly<{ name: string }>;

    /** Null for a part drawn alone, with no shell running. */
    emojiSource: Readonly<Ref<PickerSource>> | null;

    mentionSource: (row: SidebarRow | null) => MentionPickerSource | null;
};

/** Exported so a spec can mount a part under a session. */
export const chatSessionKey: InjectionKey<SessionContext> = Symbol("chat-session");

/** Called once, by the shell. */
export function provideChatSession(session: SessionContext): void {
    provide(chatSessionKey, session);
}

/** A part drawn alone reads a session with no one signed in, not at a phone's width, as it draws by itself. */
export function useChatSession(): SessionContext {
    return inject(chatSessionKey, null) ?? {
        isPhone: ref(false),
        me: computed((): ChatMe => ({ personAliasGuid: "", name: "", photoUrl: null })),
        church: { name: "" },
        emojiSource: null,
        mentionSource: () => null
    };
}
