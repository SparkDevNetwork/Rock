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

// What the whole chat session shares, provided once by the shell and read by any part that needs
// it: whether it is drawn at a phone's width, who is signed in, the church, and the emoji and
// mention sources. A part reads these here rather than taking them as inputs, so no layer between
// the shell and the part carries them on its way down.

import { computed, inject, InjectionKey, provide, Ref, ref } from "vue";
import type { PickerSource } from "./components/emojiPicker.partial.obs";
import type { MentionPickerSource } from "./components/mentionPicker.partial.obs";
import { SidebarRow } from "./types.partial";

/** The signed-in person, as every part that marks what is theirs reads them. */
export type ChatMe = {
    personAliasGuid: string;
    name: string;
    photoUrl: string | null;
};

/** What the session shares. */
export type SessionContext = {
    /** Whether the chat is drawn at a phone's width, so menus, pickers and cards open as sheets. */
    isPhone: Readonly<Ref<boolean>>;

    /** Who is signed in; replaced when a birthdate answer lets a new session in. */
    me: Readonly<Ref<ChatMe>>;

    /** The church, which names its own emoji and its notes. */
    church: Readonly<{ name: string }>;

    /** What every emoji picker draws; null where no shell is running, as for a part drawn alone. */
    emojiSource: Readonly<Ref<PickerSource>> | null;

    /** Whom a room's message box may mention, by its sidebar row; null for no room. */
    mentionSource: (row: SidebarRow | null) => MentionPickerSource | null;
};

const chatSessionKey: InjectionKey<SessionContext> = Symbol("chat-session");

/**
 * Provides the session to every part below the caller. Called once, by the shell.
 *
 * @param session What the session shares.
 */
export function provideChatSession(session: SessionContext): void {
    provide(chatSessionKey, session);
}

/**
 * The session, as provided by the shell. A part drawn alone, outside a shell, reads a session in
 * which no one is signed in and the width is not a phone's, which is how each part draws by itself.
 *
 * @returns The session.
 */
export function useChatSession(): SessionContext {
    return inject(chatSessionKey, null) ?? {
        isPhone: ref(false),
        me: computed((): ChatMe => ({ personAliasGuid: "", name: "", photoUrl: null })),
        church: { name: "" },
        emojiSource: null,
        mentionSource: () => null
    };
}
