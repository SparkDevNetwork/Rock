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
// The real status store for a part's spec, with a page that fires nothing and a session that never
// refreshes, so a part drawing status is tested against what the store itself draws: its switch,
// its blocks, its revisions and its words, rather than against a stand-in that answers whatever
// the spec wishes. A spec tells it what a reader answered, as the shell does.
import { ref, Ref } from "vue";
import { createPresenceStatus, PresenceStatus, presenceStatusKey } from "../../../../src/Communication/Chat/ChatShell/composables/usePresenceStatus.partial";
import type { ChatSettings } from "../../../../src/Communication/Chat/ChatShell/composables/useSession.partial";

/** The viewer's own alias in these stores. */
export const viewer = "a0000007-0000-4000-8000-000000000000";

export type StatusStore = {
    status: PresenceStatus;

    /** The church's switch for online status, which the store watches. */
    isOn: Ref<boolean>;

    /** The people the viewer blocked, by lower-case alias. */
    blocked: Set<string>;

    /** What a mount provides, so a part's faces read this store. */
    provide: Record<symbol, unknown>;
};

/** A status store with the switch on, the viewer's settings read and nothing told yet. */
export function statusStore(): StatusStore {
    const isOn = ref(true);
    const blocked = new Set<string>();
    const listeners = {
        addEventListener: () => undefined,
        removeEventListener: () => undefined
    };
    const status = createPresenceStatus({
        settings: () => ({ flags: { gifs: false, typing: true, online: isOn.value }, limits: { presence_idle_seconds: 600 } }) as unknown as ChatSettings,
        personAliasGuid: viewer,
        me: () => ({ busy_until: null, appear_offline: false }),
        isBlocked: alias => blocked.has(alias.toLowerCase()),
        isStopped: () => false,
        targets: { document: { visibilityState: "visible", ...listeners }, window: listeners },
        leave: () => undefined,
        session: {
            refresh: async () => true,
            nextRefreshAt: () => null,
            tokenLifeSeconds: () => 300,
            inFlight: () => null
        }
    });

    return { status, isOn, blocked, provide: { [presenceStatusKey as symbol]: status } };
}

/**
 * A status decided now by the page's clock, as a reader carries it: Busy until an end, or with no
 * end when it is null, its row live for ten minutes.
 *
 * @param end When the Busy ends, on the viewer's clock, or null for none.
 * @param msAfter How long after now it was decided, so a later one is newer.
 */
export function busyNow(end: Date | null, msAfter = 0): Record<string, unknown> {
    const now = Date.now() + msAfter;
    return {
        state: "busy",
        until: new Date(now + 600_000).toISOString(),
        ...(end ? { busy_until: end.toISOString() } : {}),
        as_of: now * 1000
    };
}

/** Online, decided now by the page's clock, its row live for ten minutes. */
export function onlineNow(msAfter = 0): Record<string, unknown> {
    const now = Date.now() + msAfter;
    return { state: "online", until: new Date(now + 600_000).toISOString(), as_of: now * 1000 };
}

/** A reader's person, carrying a status. */
export function told(alias: string, status: Record<string, unknown>, nick = "Cindy"): Record<string, unknown> {
    return { person_alias_guid: alias, nick_name: nick, last_name: "Ames", avatar_url: null, status };
}
