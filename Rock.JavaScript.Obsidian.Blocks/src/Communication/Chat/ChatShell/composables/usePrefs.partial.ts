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
// The control is the feedback: a save that lands shows nothing more, so a change is seamless. A
// save that fails puts the control back to the last value known saved and puts the server's
// sentence under it, with Try again where another try can pass, said aloud at once. Each save has a
// number; only the newest save of a setting says how it went, so quick changes never flicker
// through older answers.
// A sidebar load while a room's save is out leaves that setting as the person chose it, as the
// person's own switches are left; its own answer settles it. Another device sees a change at its
// next load. Once stopped, nothing more is asked and no answer
// reaches the screen.
/* eslint-disable @typescript-eslint/naming-convention */
import { reactive } from "vue";
import { failureWords, finalWords, isOfflineFailure, offlineWords } from "../errors.partial";
import { ChannelStore, RowPrefs } from "../stores/channelStore.partial";
import { ChatError, FailureWords, SidebarRow } from "../types.partial";
import { Note } from "../ui/settingNote.partial.obs";
import { ActionNotice } from "./useMessageActions.partial";
import { createSaveRule, failureNote, SaveChange, saveFailed, SaveOutcome } from "./saveRule.partial";

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

/** A room's place in its details: the note under each of its controls. */
export type RoomPrefs = {
    notes: Partial<Record<"notify" | "star" | "hide", Note>>;
};

/** One of the person's own switches, by the key the dialog names it. */
export type MeKey = PersonSettingKey | "reaction_push";

/** What the settings hold for the screen. */
export type PrefsState = {
    me: MeSettings;
    meNotes: Partial<Record<MeKey, Note>>;

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
    channels: Pick<ChannelStore, "rows" | "applyPrefs" | "setSidebar">;

    /** Whether the connection is lost, when the door would not be reached. */
    isOffline: () => boolean;

    /** Shows a toast for a change made from the header: what a notify mode means, or why a change failed. */
    notify: (notice: ActionNotice) => void;
};

/** The settings a shell holds. */
export type Prefs = {
    state: PrefsState;

    /** The bootstrap's me, read at every sidebar load. */
    setMe: (me: Partial<MeSettings> | null | undefined) => void;

    /** The bootstrap's rooms, read at every sidebar load, into the sidebar. */
    setRooms: (rows: SidebarRow[]) => void;

    /** A room's place, idle when it has saved nothing. */
    roomOf: (channelId: string) => RoomPrefs;

    /** Writes a room's notify mode; from the header's menu it is told in a toast. */
    setNotifyMode: (channelId: string, mode: NotifyMode, isFromMenu: boolean) => Promise<void>;

    /** Stars or unstars a room, unhiding it as it is starred; the star itself shows it, and only a refusal is toasted from the header. */
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
    mentions: { title: "Mentions only", toast: "You'll be notified only when mentioned", icon: "bell-at" },
    silent: { title: "Off", toast: "Muted. A mention by name still reaches you.", icon: "bell-off" }
};

/**
 * The glyph that draws a notify mode wherever the mode is shown: the header's bell, the details'
 * choices, a sidebar row's mark and the toast a change shows.
 *
 * @param mode The notify mode.
 *
 * @returns The name of the chat icon.
 */
export function notifyIconFor(mode: NotifyMode): string {
    return notifyWords[mode].icon;
}

/**
 * One notify mode as the header's menu draws it, by its title and description, and the details'
 * segmented control, by its short label and the one line said under it while it is chosen.
 */
export type NotifyOption = { value: NotifyMode, title: string, description: string, label: string, caption: string, icon: string };

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
        { value: "all", title: notifyWords.all.title, description: `Every new message in this ${room}.`, label: "All", caption: "Every new message.", icon: notifyWords.all.icon },
        { value: "mentions", title: notifyWords.mentions.title, description: "When someone mentions you by name or uses @all.", label: "Mentions", caption: "Your name or @all.", icon: notifyWords.mentions.icon },
        { value: "silent", title: notifyWords.silent.title, description: "Nothing, unless someone mentions you by name.", label: "Off", caption: "Only mentions of your name.", icon: notifyWords.silent.icon }
    ];
}

/** Each switch's label as Chat settings shows it. */
export const meTitles: Record<MeKey, string> = {
    profile_details: "Show public profile details",
    open_dm: "Anyone can message me",
    reaction_push: "Reactions to my messages"
};

/** The field of me each switch writes. */
const meFields: Record<MeKey, keyof MeSettings> = {
    profile_details: "show_profile_details",
    open_dm: "is_open_dm_allowed",
    reaction_push: "reaction_push"
};

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
        meNotes: {},
        rooms: {},
        announcement: null
    }) as PrefsState;

    let isStopped = false;
    let announcementCount = 0;
    const rule = createSaveRule(() => isStopped);

    function announce(text: string, isAssertive = false): void {
        state.announcement = { text, isAssertive, count: ++announcementCount };
    }

    function room(channelId: string): RoomPrefs {
        return state.rooms[channelId] ??= { notes: {} };
    }

    function rowOf(channelId: string): RowPrefs | undefined {
        return dependencies.channels.rows.find(row => row.channel_id === channelId);
    }

    /** A refusal in the server's own words; a lost connection says so. */
    function refusalWords(error: ChatError): FailureWords {
        return failureWords(error, dependencies.isOffline());
    }

    /** A refusal's note under its control, with Try again asking the same change once more where that can pass. */
    function refusalNote(failure: FailureWords, retry: () => Promise<void>): Note {
        return failureNote(failure, () => void retry());
    }

    /** Saves a setting by the one save rule; a refusal is said at once, wherever it is shown. */
    async function save<T>(key: string, change: SaveChange<T>): Promise<void> {
        await rule.save(key, {
            ...change,
            refused: failure => {
                change.refused(failure);
                announce(`Not saved. ${failure.alone}`, true);
            }
        });
    }

    /** A room's patch through chat_set_prefs, as the save rule asks it. */
    async function askRoom(channelId: string, patch: Record<string, boolean | string | null>): Promise<SaveOutcome> {
        const result = await dependencies.setPrefs(channelId, patch);
        return result.ok ? { isSaved: true, data: result.data } : { isSaved: false, failure: refusalWords(result.error) };
    }

    /** One of the person's own switches through the save rule. */
    function saveMe(key: MeKey, value: boolean, ask: () => Promise<SaveOutcome>): Promise<void> {
        const field = meFields[key];
        delete state.meNotes[key];
        return save(`me:${key}`, {
            was: state.me[field],
            value,
            show: shown => {
                state.me[field] = shown;
            },
            ask,
            refused: failure => {
                state.meNotes[key] = refusalNote(failure, () => prefs.setSwitch(key, value));
            }
        });
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
                if (!rule.isOut(`me:${key}`)) {
                    state.me[meFields[key]] = read[meFields[key]];
                }
            }
        },

        setRooms: (rows: SidebarRow[]): void => {
            // What the person chose in each room whose save is out, as the rows show it now.
            const chosen = new Map<string, RowPrefs>();
            for (const row of dependencies.channels.rows) {
                const kept: RowPrefs = {};
                if (rule.isOut(`${row.channel_id}:notify`)) {
                    kept.notify_mode = row.notify_mode;
                }
                if (rule.isOut(`${row.channel_id}:sidebar`)) {
                    kept.is_favorite = row.is_favorite;
                    kept.hidden = row.hidden ?? false;
                }
                if (Object.keys(kept).length > 0) {
                    chosen.set(row.channel_id, kept);
                }
            }

            dependencies.channels.setSidebar(rows);
            chosen.forEach((kept, channelId) => dependencies.channels.applyPrefs(channelId, kept));
        },

        roomOf: (channelId: string): RoomPrefs => state.rooms[channelId] ?? { notes: {} },

        setNotifyMode: async (channelId: string, mode: NotifyMode, isFromMenu: boolean): Promise<void> => {
            const row = rowOf(channelId);
            if (isStopped || !row || row.notify_mode === mode) {
                return;
            }

            delete room(channelId).notes.notify;
            await save<string | null | undefined>(`${channelId}:notify`, {
                was: row.notify_mode,
                value: mode,
                show: shown => dependencies.channels.applyPrefs(channelId, { notify_mode: shown }),
                ask: () => askRoom(channelId, { notify_mode: mode }),
                landed: data => {
                    // the mode the room now has for the person, its default included where it is cleared
                    const effective = (data as { channel?: { notify_mode_effective?: unknown } } | null)?.channel?.notify_mode_effective;
                    if (typeof effective === "string") {
                        dependencies.channels.applyPrefs(channelId, { notify_mode: effective });
                    }

                    // A menu closes as it chooses, so the toast says what the choice means.
                    if (isFromMenu) {
                        dependencies.notify({ code: "notify-mode", text: notifyWords[mode].toast, icon: notifyWords[mode].icon });
                        announce(notifyWords[mode].toast);
                    }
                },
                refused: failure => {
                    if (isFromMenu) {
                        dependencies.notify({ code: "notify-mode", text: failure.alone, icon: "alert" });
                    }
                    else {
                        room(channelId).notes.notify = refusalNote(failure, () => prefs.setNotifyMode(channelId, mode, false));
                    }
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
            const patch: Record<string, boolean> = isUnhiding ? { is_favorite: true, hidden: false } : { is_favorite: isStarred };
            const notes = room(channelId).notes;
            delete notes.star;
            delete notes.hide;
            if (isUnhiding && !isFromMenu) {
                notes.star = { kind: "info", text: "Starred, so it's back in your list." };
            }

            // Star and hide are one save's two halves, so they share one newest, and each save asks
            // for both: the newest then lands what is shown whatever an older one still out does.
            const was = { is_favorite: row.is_favorite, hidden: row.hidden ?? false };
            const value = { ...was, ...patch };
            await save<RowPrefs>(`${channelId}:sidebar`, {
                was,
                value,
                show: shown => dependencies.channels.applyPrefs(channelId, shown),
                ask: () => askRoom(channelId, value),
                refused: failure => {
                    if (isFromMenu) {
                        dependencies.notify({ code: "starred", text: failure.alone, icon: "alert" });
                    }
                    else {
                        delete notes.hide;
                        notes.star = refusalNote(failure, () => prefs.setStarred(channelId, isStarred, false));
                    }
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
            const patch: Record<string, boolean> = isUnstarring ? { hidden: true, is_favorite: false } : { hidden: isHidden };
            const notes = room(channelId).notes;
            delete notes.star;
            delete notes.hide;
            if (isHidden) {
                // what hiding means is said with the change, since the room stays until another opens
                notes.hide = { kind: "info", text: `${isUnstarring ? "Hidden, and taken out of Starred." : "Hidden."} ${leavesListSentence}` };
            }

            const was = { is_favorite: row.is_favorite, hidden: row.hidden ?? false };
            const value = { ...was, ...patch };
            await save<RowPrefs>(`${channelId}:sidebar`, {
                was,
                value,
                show: shown => dependencies.channels.applyPrefs(channelId, shown),
                ask: () => askRoom(channelId, value),
                refused: failure => {
                    delete notes.star;
                    notes.hide = refusalNote(failure, () => prefs.setHidden(channelId, isHidden));
                }
            });
        },

        setPersonSetting: async (setting: PersonSettingKey, value: boolean): Promise<void> => {
            if (isStopped) {
                return;
            }

            const door = dependencies.savePersonSetting;
            await saveMe(setting, value, async (): Promise<SaveOutcome> => {
                if (!door) {
                    return { isSaved: false, failure: finalWords(noDoorSentence) };
                }

                // Pending has saved too: Rock holds it and the platform hears of it shortly.
                const answer = await door(setting, value);
                if (answer.code === "ok") {
                    return { isSaved: true };
                }

                // Rock answering without its door decided nothing; the person may ask again.
                if (answer.code === "door.unavailable") {
                    return { isSaved: false, failure: isOfflineFailure(null, dependencies.isOffline()) ? offlineWords : saveFailed };
                }

                // Rock's door decided: the same value would be refused again.
                return { isSaved: false, failure: finalWords(answer.message?.trim() || noDoorSentence) };
            });
        },

        setReactionPush: async (value: boolean): Promise<void> => {
            if (isStopped) {
                return;
            }

            await saveMe("reaction_push", value, async (): Promise<SaveOutcome> => {
                const result = await dependencies.setPrefs(null, { reaction_push: value });
                return result.ok ? { isSaved: true } : { isSaved: false, failure: refusalWords(result.error) };
            });
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
