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
// The person's settings: each room's notify mode, star and hide, and their own three switches.
//
// Every change shows at once and writes at once, one save per change. A room's settings and the
// reaction switch go to chat_set_prefs, the reaction switch with no room; the two privacy switches
// go to Rock's SavePersonSetting door, one setting a call, since Rock owns them and pushes them on.
// Star and hide pull against each other, so hiding unstars and starring unhides in the same patch.
// A save marks its place Saving, then Saved, Saved, syncing where the door answers pending, or Not
// saved, when the control goes back and the server's sentence stands under it. Each save has a
// number; only the newest save of a setting may move it, so an answer to a change the person has
// since made again moves nothing. Another device sees a change at its next load. Once stopped,
// nothing more is asked and no answer reaches the screen.
/* eslint-disable @typescript-eslint/naming-convention */
import { reactive } from "vue";
import { messageForError } from "../errors.partial";
import { ChannelStore, RowPrefs } from "../stores/channelStore.partial";
import { ChatError } from "../types.partial";
import { ActionNotice } from "./useMessageActions.partial";

/** What chat_set_prefs answered, through the shell's one call path. */
export type PrefsCallResult = { ok: true, data: unknown } | { ok: false, error: ChatError };

/** One of the two switches Rock owns, as its SavePersonSetting door names them. */
export type PersonSettingKey = "profile_details" | "open_dm";

/** What Rock's SavePersonSetting door answered, as ChatPersonSettingResultBag reaches the browser. */
export type PersonSettingResult = { code: string, isPending: boolean, message: string | null };

/** The person's own switches, as the bootstrap's me and chat_set_prefs's me carry them. */
export type MeSettings = {
    show_profile_details: boolean;
    is_open_dm_allowed: boolean;
    reaction_push: boolean;
};

/** A room's notify mode, as the platform names them. */
export type NotifyMode = "all" | "mentions" | "silent";

/** Where the newest save of a place stands; the count moves with each save. */
export type SaveMark = { state: "saving" | "saved" | "pending" | "refused", count: number };

/** A line under a control: a word about the save, a pending save's, or a refusal's sentence. */
export type SettingNote = { kind: "info" | "pending" | "error", text: string };

/** A room's place in its details: its newest save, and the note under each of its controls. */
export type RoomPrefs = {
    save: SaveMark | null;
    notes: Partial<Record<"notify" | "star" | "hide", SettingNote>>;
};

/** One of the person's own switches, by the key the dialog names it. */
export type MeKey = PersonSettingKey | "reaction_push";

/** What the settings hold for the screen. */
export type PrefsState = {
    me: MeSettings;

    /** Chat settings' newest save; Saving while any switch's save is out. */
    meSave: SaveMark | null;

    /** The switches whose save is out, each its own Saving. */
    meSaving: Partial<Record<MeKey, boolean>>;
    meNotes: Partial<Record<MeKey, SettingNote>>;

    /** Each room's place, by its id; a room not here has saved nothing this visit. */
    rooms: Record<string, RoomPrefs>;

    /** A sentence for screen readers; the count moves on with each, so the same words twice are read twice. */
    announcement: { text: string, isAssertive: boolean, count: number } | null;
};

/**
 * A choice from a room's controls: its details, or the header's menus, which a missing
 * isFromMenu means. Star with no value turns the room's star over.
 */
export type RoomChoice =
    | { kind: "notify", mode: NotifyMode, isFromMenu?: boolean }
    | { kind: "star", isStarred?: boolean, isFromMenu?: boolean }
    | { kind: "hide", isHidden: boolean };

/** What the settings reach outside themselves. */
export type PrefsDependencies = {
    /** chat_set_prefs through the shell's call path: a room, or null for the reaction switch, and the patch. */
    setPrefs: (channelId: string | null, changes: Record<string, boolean | string | null>) => Promise<PrefsCallResult>;

    /** Rock's SavePersonSetting door; without it, a privacy switch is refused. */
    savePersonSetting?: (setting: PersonSettingKey, value: boolean) => Promise<PersonSettingResult>;

    /** The sidebar, whose rows show a room's settings. */
    channels: Pick<ChannelStore, "rows" | "applyPrefs">;

    /** Whether the connection is lost, when the door would not be reached. */
    isOffline: () => boolean;

    /** Shows a toast that is not a failure, for a change made from the header. */
    notify: (notice: ActionNotice) => void;
};

/** The settings a shell holds. */
export type Prefs = {
    state: PrefsState;

    /** The bootstrap's me, read at every sidebar load. */
    setMe: (me: Partial<MeSettings> | null | undefined) => void;

    /** A room's place, idle when it has saved nothing. */
    roomOf: (channelId: string) => RoomPrefs;

    /** Writes a room's notify mode; from the header's menu it is told in a toast. */
    setNotifyMode: (channelId: string, mode: NotifyMode, isFromMenu: boolean) => Promise<void>;

    /** Stars or unstars a room, unhiding it as it is starred; from the header it is told in a toast. */
    setStarred: (channelId: string, isStarred: boolean, isFromMenu: boolean) => Promise<void>;

    /** Hides or shows a room, unstarring it as it is hidden. */
    setHidden: (channelId: string, isHidden: boolean) => Promise<void>;

    /** Writes one of the two switches Rock owns, through its door. */
    setPersonSetting: (setting: PersonSettingKey, value: boolean) => Promise<void>;

    /** Writes whether reactions to the person's messages notify them. */
    setReactionPush: (value: boolean) => Promise<void>;

    /** Saves a choice for a room; with no room, nothing. */
    choose: (channelId: string | null, choice: RoomChoice) => Promise<void>;

    /** A Chat settings switch: the reaction switch is the platform's, the other two Rock's. */
    setSwitch: (key: MeKey, value: boolean) => Promise<void>;

    stop: () => void;
};

/** Each mode's name and the toast a change from the header's menu shows. */
const notifyWords: Record<NotifyMode, { title: string, toast: string, icon: string }> = {
    all: { title: "All messages", toast: "You'll be notified about every message", icon: "bell" },
    mentions: { title: "Mentions only", toast: "You'll be notified only when mentioned", icon: "at-bell" },
    silent: { title: "Off", toast: "Muted. A mention by name still reaches you.", icon: "bell-off" }
};

/** One notify mode as the details' cards and the header's menu draw it. */
export type NotifyOption = { value: NotifyMode, title: string, description: string, icon: string };

/**
 * The three notify modes in the one wording the details and the header's menu share, so the two
 * never disagree: a name breaks Off and @all does not, as the push filter decides. A channel is a
 * channel; a direct message or group is a conversation.
 *
 * @param kind The room's kind.
 *
 * @returns The modes, in order.
 */
export function notifyOptionsFor(kind: string | null): NotifyOption[] {
    const room = kind === "channel" ? "channel" : "conversation";
    return [
        { value: "all", title: notifyWords.all.title, description: `Every new message in this ${room}.`, icon: notifyWords.all.icon },
        { value: "mentions", title: notifyWords.mentions.title, description: "When someone mentions you by name or uses @all.", icon: notifyWords.mentions.icon },
        { value: "silent", title: notifyWords.silent.title, description: "Nothing, unless someone mentions you by name.", icon: notifyWords.silent.icon }
    ];
}

/** Each switch's title, which its saved sentence names. */
const meTitles: Record<MeKey, string> = {
    profile_details: "Show my details",
    open_dm: "Open direct messages",
    reaction_push: "Reactions to my messages"
};

/** The field of me each switch writes. */
const meFields: Record<MeKey, keyof MeSettings> = {
    profile_details: "show_profile_details",
    open_dm: "is_open_dm_allowed",
    reaction_push: "reaction_push"
};

const pendingSentence = "Saved. It can take a minute to reach everyone.";
const offlineSentence = "You're offline. Try again when you're back.";
const failedSentence = "Couldn't save. Try again in a moment.";
const noDoorSentence = "That setting can't be changed right now.";
const leavesListSentence = "It leaves your list when you open another conversation.";

/**
 * Creates the settings.
 *
 * @param dependencies What the settings reach outside themselves.
 *
 * @returns The settings.
 */
export function createPrefs(dependencies: PrefsDependencies): Prefs {
    const state = reactive<PrefsState>({
        me: { show_profile_details: false, is_open_dm_allowed: false, reaction_push: true },
        meSave: null,
        meSaving: {},
        meNotes: {},
        rooms: {},
        announcement: null
    }) as PrefsState;

    let isStopped = false;
    let saveCount = 0;
    let announcementCount = 0;

    /** The newest save asked of each setting, by a key naming it, so an older answer moves nothing. */
    const newest = new Map<string, number>();

    function announce(text: string): void {
        state.announcement = { text, isAssertive: false, count: ++announcementCount };
    }

    function room(channelId: string): RoomPrefs {
        return state.rooms[channelId] ??= { save: null, notes: {} };
    }

    function rowOf(channelId: string): RowPrefs | undefined {
        return dependencies.channels.rows.find(row => row.channel_id === channelId);
    }

    /** Numbers a save of a setting and marks it the newest of that setting. */
    function begin(key: string): number {
        const count = ++saveCount;
        newest.set(key, count);
        return count;
    }

    /** Whether an answer may still move what it saved: the shell runs and no newer save was asked. */
    function isCurrent(key: string, count: number): boolean {
        return !isStopped && newest.get(key) === count;
    }

    /** A refusal in the server's own words; a lost connection says so. */
    function refusalText(error: ChatError): string {
        return error.code === "rpc.transport" ? offlineSentence : messageForError(error);
    }

    /**
     * Writes a room's patch, already shown, and settles it: the newest save marks the room and
     * takes back what it showed on a refusal.
     */
    async function saveRoom(channelId: string, key: string, patch: Record<string, boolean | string | null>, was: RowPrefs, settled: {
        saved: (data: unknown) => void,
        refused: (text: string) => void
    }): Promise<void> {
        const count = begin(`${channelId}:${key}`);
        room(channelId).save = { state: "saving", count };
        const result = await dependencies.setPrefs(channelId, patch);
        if (!isCurrent(`${channelId}:${key}`, count)) {
            return;
        }

        if (result.ok) {
            room(channelId).save = { state: "saved", count };
            settled.saved(result.data);
            return;
        }

        dependencies.channels.applyPrefs(channelId, was);
        room(channelId).save = { state: "refused", count };
        settled.refused(refusalText(result.error));
    }

    /** Settles a save of one of the person's own switches, already shown. */
    function settleMe(key: MeKey, count: number, was: boolean, outcome: { pending: boolean } | { refused: string }): void {
        if (!isCurrent(`me:${key}`, count)) {
            return;
        }

        // Another switch's save still out keeps the head at Saving; its own answer settles it.
        delete state.meSaving[key];
        const isOtherOut = Object.keys(state.meSaving).length > 0;
        if ("refused" in outcome) {
            state.me[meFields[key]] = was;
            state.meSave = isOtherOut ? state.meSave : { state: "refused", count };
            state.meNotes[key] = { kind: "error", text: outcome.refused };
            announce(`Not saved. ${outcome.refused}`);
            return;
        }

        const now = state.me[meFields[key]] ? "on" : "off";
        if (outcome.pending) {
            state.meSave = isOtherOut ? state.meSave : { state: "pending", count };
            state.meNotes[key] = { kind: "pending", text: pendingSentence };
            announce(pendingSentence);
        }
        else {
            state.meSave = isOtherOut ? state.meSave : { state: "saved", count };
            announce(`${meTitles[key]} ${now}. Saved.`);
        }
    }

    /** Shows one of the person's own switches at once and numbers its save. */
    function beginMe(key: MeKey, value: boolean): { count: number, was: boolean } {
        const field = meFields[key];
        const was = state.me[field];
        state.me[field] = value;
        const count = begin(`me:${key}`);
        state.meSave = { state: "saving", count };
        state.meSaving[key] = true;
        delete state.meNotes[key];
        return { count, was };
    }

    const prefs: Prefs = {
        state,

        setMe: (me: Partial<MeSettings> | null | undefined): void => {
            if (!me) {
                return;
            }

            const read: MeSettings = {
                show_profile_details: me.show_profile_details === true,
                is_open_dm_allowed: me.is_open_dm_allowed === true,
                reaction_push: me.reaction_push !== false
            };

            // A switch whose save is out keeps what the person chose; its own answer settles it.
            for (const key of Object.keys(meFields) as MeKey[]) {
                if (!state.meSaving[key]) {
                    state.me[meFields[key]] = read[meFields[key]];
                }
            }
        },

        roomOf: (channelId: string): RoomPrefs => state.rooms[channelId] ?? { save: null, notes: {} },

        setNotifyMode: async (channelId: string, mode: NotifyMode, isFromMenu: boolean): Promise<void> => {
            const row = rowOf(channelId);
            if (isStopped || !row || row.notify_mode === mode) {
                return;
            }

            const was: RowPrefs = { notify_mode: row.notify_mode };
            dependencies.channels.applyPrefs(channelId, { notify_mode: mode });
            delete room(channelId).notes.notify;

            await saveRoom(channelId, "notify", { notify_mode: mode }, was, {
                saved: data => {
                    // the mode the room now has for the person, its default included where it is cleared
                    const effective = (data as { channel?: { notify_mode_effective?: unknown } } | null)?.channel?.notify_mode_effective;
                    if (typeof effective === "string") {
                        dependencies.channels.applyPrefs(channelId, { notify_mode: effective });
                    }

                    if (isFromMenu) {
                        dependencies.notify({ code: "notify-mode", text: notifyWords[mode].toast, icon: notifyWords[mode].icon });
                        announce(notifyWords[mode].toast);
                    }
                    else {
                        announce(`${notifyWords[mode].title}. Saved.`);
                    }
                },
                refused: text => {
                    if (isFromMenu) {
                        dependencies.notify({ code: "notify-mode", text, icon: "alert" });
                    }
                    else {
                        room(channelId).notes.notify = { kind: "error", text };
                    }
                    announce(`Not saved. ${text}`);
                }
            });
        },

        setStarred: async (channelId: string, isStarred: boolean, isFromMenu: boolean): Promise<void> => {
            const row = rowOf(channelId);
            if (isStopped || !row) {
                return;
            }

            // starring a hidden room brings it back, in the same one save
            const isUnhiding = isStarred && row.hidden === true;
            const was: RowPrefs = { is_favorite: row.is_favorite, hidden: row.hidden ?? false };
            const patch: Record<string, boolean> = isUnhiding ? { is_favorite: true, hidden: false } : { is_favorite: isStarred };
            dependencies.channels.applyPrefs(channelId, isUnhiding ? { is_favorite: true, hidden: false } : { is_favorite: isStarred });
            delete room(channelId).notes.star;
            delete room(channelId).notes.hide;

            // star and hide are one save's two halves, so they share one newest
            await saveRoom(channelId, "sidebar", patch, was, {
                saved: () => {
                    if (isFromMenu) {
                        dependencies.notify({ code: "starred", text: isStarred ? "Starred" : "Removed from Starred", icon: "star" });
                        announce(isStarred ? "Starred" : "Removed from Starred");
                        return;
                    }

                    if (isUnhiding) {
                        room(channelId).notes.star = { kind: "info", text: "Starred, so it's back in your list." };
                    }
                    announce(`${isStarred ? "Starred" : "Unstarred"}${isUnhiding ? ", and shown again" : ""}. Saved.`);
                },
                refused: text => {
                    if (isFromMenu) {
                        dependencies.notify({ code: "starred", text, icon: "alert" });
                    }
                    else {
                        room(channelId).notes.star = { kind: "error", text };
                    }
                    announce(`Not saved. ${text}`);
                }
            });
        },

        setHidden: async (channelId: string, isHidden: boolean): Promise<void> => {
            const row = rowOf(channelId);
            if (isStopped || !row) {
                return;
            }

            // hiding a starred room takes it out of Starred, in the same one save
            const isUnstarring = isHidden && row.is_favorite === true;
            const was: RowPrefs = { is_favorite: row.is_favorite, hidden: row.hidden ?? false };
            const patch: Record<string, boolean> = isUnstarring ? { hidden: true, is_favorite: false } : { hidden: isHidden };
            dependencies.channels.applyPrefs(channelId, isUnstarring ? { hidden: true, is_favorite: false } : { hidden: isHidden });
            delete room(channelId).notes.star;
            delete room(channelId).notes.hide;

            await saveRoom(channelId, "sidebar", patch, was, {
                saved: () => {
                    if (isHidden) {
                        room(channelId).notes.hide = { kind: "info", text: `${isUnstarring ? "Hidden, and taken out of Starred." : "Hidden."} ${leavesListSentence}` };
                    }
                    announce(`${isHidden ? "Hidden from your sidebar" : "Shown in your sidebar"}${isUnstarring ? ", and unstarred" : ""}. Saved.`);
                },
                refused: text => {
                    room(channelId).notes.hide = { kind: "error", text };
                    announce(`Not saved. ${text}`);
                }
            });
        },

        setPersonSetting: async (setting: PersonSettingKey, value: boolean): Promise<void> => {
            if (isStopped) {
                return;
            }

            const { count, was } = beginMe(setting, value);
            if (!dependencies.savePersonSetting) {
                settleMe(setting, count, was, { refused: noDoorSentence });
                return;
            }

            const answer = await dependencies.savePersonSetting(setting, value);
            if (answer.code === "ok") {
                settleMe(setting, count, was, { pending: answer.isPending });
            }
            // Rock answering without its door decided nothing; the person may ask again.
            else if (answer.code === "door.unavailable") {
                settleMe(setting, count, was, { refused: dependencies.isOffline() ? offlineSentence : failedSentence });
            }
            else {
                settleMe(setting, count, was, { refused: answer.message?.trim() || noDoorSentence });
            }
        },

        setReactionPush: async (value: boolean): Promise<void> => {
            if (isStopped) {
                return;
            }

            const { count, was } = beginMe("reaction_push", value);
            const result = await dependencies.setPrefs(null, { reaction_push: value });
            settleMe("reaction_push", count, was, result.ok ? { pending: false } : { refused: refusalText(result.error) });
        },

        choose: async (channelId: string | null, choice: RoomChoice): Promise<void> => {
            if (!channelId) {
                return;
            }

            const isFromMenu = choice.kind !== "hide" && choice.isFromMenu !== false;
            if (choice.kind === "notify") {
                await prefs.setNotifyMode(channelId, choice.mode, isFromMenu);
            }
            else if (choice.kind === "star") {
                const isStarred = choice.isStarred ?? !dependencies.channels.rows.find(r => r.channel_id === channelId)?.is_favorite;
                await prefs.setStarred(channelId, isStarred, isFromMenu);
            }
            else {
                await prefs.setHidden(channelId, choice.isHidden);
            }
        },

        setSwitch: (key: MeKey, value: boolean): Promise<void> => key === "reaction_push" ? prefs.setReactionPush(value) : prefs.setPersonSetting(key, value),

        stop: (): void => {
            isStopped = true;
        }
    };

    return prefs;
}
