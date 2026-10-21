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
// The shell as the Chat block builds it in a real page: Rock's block actions for the church token
// and the doors, the platform's client, the browser's fetch and storage, and its web push with
// the one Firebase app the block keeps. The shell itself takes all of these as options so it runs
// anywhere; this is the one place that hands it the page's own.
import { createClient } from "@Obsidian/Libs/supabase";
import { InvokeBlockActionFunc } from "@Obsidian/Types/Utility/block";
import { ChatChurchTokenBag } from "@Obsidian/ViewModels/Blocks/Communication/Chat/ChatShell/chatChurchTokenBag";
import { ChatMemberRightsBag } from "@Obsidian/ViewModels/Blocks/Communication/Chat/ChatShell/chatMemberRightsBag";
import { ChatMembershipResultBag } from "@Obsidian/ViewModels/Blocks/Communication/Chat/ChatShell/chatMembershipResultBag";
import { ChatPersonSettingResultBag } from "@Obsidian/ViewModels/Blocks/Communication/Chat/ChatShell/chatPersonSettingResultBag";
import { DoorResult } from "./composables/useDirectMessage.partial";
import { ChatShell, createChatShell, PlatformClientLike, ShellOptions } from "./shell.partial";

/** What the block hands in once, for every shell it builds. */
export type BrowserShellOptions = {
    /** The channel a link to the page names, if any. */
    linkedChannelId: string | null;

    areSharedChannelsFilteredByCampus: boolean;

    /** Shows a toast that is not a failure. */
    notify: NonNullable<ShellOptions["notify"]>;

    /** Calls one of the Chat block's actions in Rock. */
    invokeBlockAction: InvokeBlockActionFunc;

    /** The frame's views a jump from the mentions inbox lands in. */
    mentionViews: NonNullable<ShellOptions["mentionViews"]>;
};

/** The cookie Rock reads at sign-out to take this browser out of push. */
const pushCookieName = "rock_chat_push";

/** Where this browser remembers that the person turned its notifications off in Chat settings. */
const pushOffKey = "rock-chat-push-off";

/**
 * Makes the block's shell builder. A birthdate save replaces the shell with one for the session
 * it answers with, so the builder is kept for the block's life and every shell it builds shares
 * the one Firebase app, which Firebase refuses to make twice under its name.
 *
 * @param options What the block hands in once.
 *
 * @returns A function that builds the shell for a session.
 */
export function createBrowserShellBuilder(options: BrowserShellOptions): (session: ShellOptions["session"]) => ChatShell {
    const { invokeBlockAction } = options;
    let firebaseAppInstance: unknown = null;

    /** The block's one Firebase app, made on the first token and reused for every later one. */
    function firebaseApp(firebase: typeof import("@Obsidian/Libs/firebase"), web: Record<string, unknown>): ReturnType<typeof firebase.initializeApp> {
        firebaseAppInstance ??= firebase.initializeApp(web, "rock-chat");
        return firebaseAppInstance as ReturnType<typeof firebase.initializeApp>;
    }

    /** This browser's web push, or undefined where it has none. */
    function browserPush(): ShellOptions["push"] {
        if (!("serviceWorker" in navigator) || !("Notification" in window) || !("PushManager" in window)) {
            return undefined;
        }

        return {
            pagePath: window.location.pathname,
            permission: () => Notification.permission,
            requestPermission: () => Notification.requestPermission(),
            // Its own folder as its scope, so it never takes the site root from a worker a
            // church's theme registers there.
            registerWorker: pagePath => navigator.serviceWorker.register(
                `/Scripts/Rock/Chat/chatPushWorker.js?page=${encodeURIComponent(pagePath)}`,
                { scope: "/Scripts/Rock/Chat/" }),
            getToken: async (web, vapidKey, registration) => {
                const firebase = await import("@Obsidian/Libs/firebase");
                return firebase.getToken(firebase.getMessaging(firebaseApp(firebase, web)), {
                    vapidKey,
                    serviceWorkerRegistration: registration as ServiceWorkerRegistration
                });
            },
            offFlag: {
                read: () => window.localStorage.getItem(pushOffKey) === "true",
                write: isOff => isOff ? window.localStorage.setItem(pushOffKey, "true") : window.localStorage.removeItem(pushOffKey)
            },
            readCookie: () => {
                const pair = document.cookie.split("; ").find(part => part.startsWith(`${pushCookieName}=`));
                return pair ? decodeURIComponent(pair.slice(pushCookieName.length + 1)) : null;
            },
            writeCookie: token => {
                const secure = window.location.protocol === "https:" ? "; Secure" : "";
                // a year, a choice: the block writes it again on every load while push is on
                document.cookie = token
                    ? `${pushCookieName}=${encodeURIComponent(token)}; path=/; max-age=31536000; SameSite=Lax${secure}`
                    : `${pushCookieName}=; path=/; max-age=0; SameSite=Lax${secure}`;
            }
        };
    }

    /**
     * Asks one of Rock's membership doors. Rock answering without the door's result decided
     * nothing, so it is answered as the door unreached, which the person may ask again: offline
     * where the connection is lost, otherwise a failure to try again in a moment.
     */
    async function membershipDoor(action: string, args: Record<string, unknown>): Promise<ChatMembershipResultBag> {
        const result = await invokeBlockAction<ChatMembershipResultBag>(action, args);
        if (result.isSuccess && result.data) {
            return result.data;
        }

        return { code: "door.unavailable", message: result.errorMessage ?? null, isPending: false };
    }

    return session => createChatShell({
        session,
        linkedChannelId: options.linkedChannelId,
        notify: options.notify,
        mentionViews: options.mentionViews,
        areSharedChannelsFilteredByCampus: options.areSharedChannelsFilteredByCampus,
        mintChurchToken: async () => {
            const result = await invokeBlockAction<ChatChurchTokenBag>("MintChurchToken");
            return { isSuccess: result.isSuccess, statusCode: result.statusCode, data: result.data };
        },
        startDirectMessage: async personAliasGuids => {
            const result = await invokeBlockAction<DoorResult>("StartDirectMessage", { personAliasGuids });
            if (result.isSuccess && result.data) {
                return result.data;
            }

            // Rock did not answer with a door result at all, so there is no sentence of its
            // own to show and the draft stays for the person to try again.
            return {
                code: "door.unavailable",
                channelGuid: null,
                isPending: false,
                message: result.errorMessage ?? "The conversation could not be started. Try again.",
                personAliasGuid: null
            };
        },
        joinChannel: async channelId => {
            const result = await invokeBlockAction<ChatMembershipResultBag>("JoinChannel", { channelGuid: channelId });
            if (result.isSuccess && result.data) {
                return result.data;
            }

            // Rock did not answer with a door result, so the message fails and can be sent again.
            return { code: "door.unavailable", message: result.errorMessage ?? null, isPending: false };
        },
        leaveChannel: channelId => membershipDoor("LeaveChannel", { channelGuid: channelId }),
        renameConversation: (channelId, name) => membershipDoor("RenameConversation", { channelGuid: channelId, name }),
        getMemberRights: async channelId => {
            const result = await invokeBlockAction<ChatMemberRightsBag>("GetMemberRights", { channelGuid: channelId });
            // Without Rock's answer nothing is offered; the doors still decide every change.
            return { canManageMembers: result.isSuccess && result.data?.canManageMembers === true };
        },
        addMembers: (channelId, personAliasGuids) => membershipDoor("AddMembers", { channelGuid: channelId, personAliasGuids }),
        removeMember: (channelId, personAliasGuid, replacementPersonAliasGuid) => membershipDoor("RemoveMember", { channelGuid: channelId, personAliasGuid, replacementPersonAliasGuid }),
        savePersonSetting: async (setting, value) => {
            const result = await invokeBlockAction<ChatPersonSettingResultBag>("SavePersonSetting", { setting, value });
            if (result.isSuccess && result.data) {
                return { code: result.data.code ?? "door.unavailable", isPending: result.data.isPending, message: result.data.message ?? null };
            }

            // Rock answered without the door's result, which decided nothing; the switch goes back.
            return { code: "door.unavailable", isPending: false, message: result.errorMessage ?? null };
        },
        createPlatformClient: (url, key, accessToken, realtime) => createClient(url, key, { accessToken, realtime }) as unknown as PlatformClientLike,
        fetch: (url, init) => window.fetch(url, init),
        storage: safeLocalStorage(),
        pageTargets: { document, window },
        push: browserPush(),
        mark: name => {
            try {
                performance.mark(name);
            }
            catch {
                // Intentionally ignored: timing marks are for measurement only.
            }
        }
    });
}

/** The browser's local storage, or null where the browser blocks it. */
function safeLocalStorage(): Storage | null {
    try {
        return window.localStorage;
    }
    catch {
        return null;
    }
}
